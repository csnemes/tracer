using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Executes weaving on the given type.
    /// </summary>
    internal class TypeWeaver : MethodWeaver.ILoggerProvider
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly ITraceLoggingFilter _filter;
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly Lazy<FieldReference> _staticLoggerField;
        private readonly MethodWeaverFactory _methodWeaverFactory;
        private readonly Lazy<bool> _hasCompilerGeneratedAttribute;
        private readonly bool _shouldTraceConstructors;
        private readonly bool _shouldTraceProperties;

        internal TypeWeaver(ITraceLoggingFilter filter, bool shouldTraceConstructors, bool shouldTraceProperties, TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            TypeDefinition typeDefinition)
        {
            _filter = filter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _typeDefinition = typeDefinition;
            _staticLoggerField = new Lazy<FieldReference>(() => CreateLoggerStaticField(_typeReferenceProvider, _methodReferenceProvider, _typeDefinition));
            _methodWeaverFactory = new MethodWeaverFactory(typeReferenceProvider, methodReferenceProvider, this);
            _hasCompilerGeneratedAttribute =
                new Lazy<bool>(() => CalculateTypeHasCompilerGeneratedAttribute(typeDefinition));
            _shouldTraceConstructors = shouldTraceConstructors;
            _shouldTraceProperties = shouldTraceProperties;
        }

        /// <summary>
        /// Runs the waving on the type linked to this instance (via <see cref="TypeDefinition"/>).
        /// </summary>
        public void Execute()
        {
            var methodsToVisit = _typeDefinition.GetMethods().Concat(_typeDefinition.GetConstructors())
                .Where(method => method.HasBody && !method.IsAbstract);

            foreach (var method in methodsToVisit.ToList())
            {
                if (AlreadyWeaved(method)) continue;

                bool shouldAddTrace = !TypeHasCompilerGeneratedAttribute && !MethodHasCompilerGeneratedAttribute(method)
                    && ((method.IsConstructor && _shouldTraceConstructors && !method.IsStatic) || !method.IsConstructor)
                    && ((method.IsPropertyAccessor() && _shouldTraceProperties) || !method.IsPropertyAccessor());

                FilterResult filterResult = new FilterResult(false);
                if (shouldAddTrace)
                    filterResult = _filter.ShouldAddTrace(method);

                _methodWeaverFactory.Create(method).Execute(filterResult.ShouldTrace, filterResult.Parameters);
            }
        }

        private bool MethodHasCompilerGeneratedAttribute(MethodDefinition methodDefinition)
        {
            return methodDefinition.HasCustomAttributes && methodDefinition.CustomAttributes
                                              .Any(attr => attr.AttributeType.FullName.Equals(typeof(CompilerGeneratedAttribute).FullName,
                                                  StringComparison.Ordinal));
        }

        private bool CalculateTypeHasCompilerGeneratedAttribute(TypeDefinition typeDefinition)
        {
            var typeHasCompGenAttribute = typeDefinition.HasCustomAttributes && typeDefinition.CustomAttributes
                       .Any(attr => attr.AttributeType.FullName.Equals(typeof(CompilerGeneratedAttribute).FullName,
                           StringComparison.Ordinal));

            if (!typeHasCompGenAttribute && typeDefinition.IsNested)
                return CalculateTypeHasCompilerGeneratedAttribute(typeDefinition.DeclaringType); 

            return typeHasCompGenAttribute;
        }

        private bool TypeHasCompilerGeneratedAttribute
        {
            get { return _hasCompilerGeneratedAttribute.Value; }
        }

        private bool AlreadyWeaved(MethodDefinition method)
        {
            //if we have an instruction loading the static logger we've already been here
            var logTypeRef = _typeReferenceProvider.LogAdapterReference;

            var referencedStaticFields = method.Body.Instructions.Where(instr => instr.OpCode == OpCodes.Ldsfld)
                .Select(instr => instr.Operand)
                .OfType<FieldReference>();

            return referencedStaticFields.Any(refFld => refFld.FieldType.FullName == logTypeRef.FullName);
        }

        public FieldReference StaticLogger
        {
            get { return _staticLoggerField.Value; }
        }

        internal static FieldReference CreateLoggerStaticField(TypeReferenceProvider typeReferenceProvider,
            MethodReferenceProvider methodReferenceProvider,
            TypeDefinition typeDefinition)
        {
            var logTypeRef = typeReferenceProvider.LogAdapterReference;
            var logManagerTypeRef = typeReferenceProvider.LogManagerReference;

            //look for existing one
            var loggerField = typeDefinition.Fields.FirstOrDefault(fld => fld.IsStatic &&
            fld.FieldType.FullName.Equals(logTypeRef.FullName));

            if (loggerField != null) return loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            //$log should be unique
            loggerField = new FieldDefinition("$log", FieldAttributes.Private | FieldAttributes.Static, logTypeRef); 
            typeDefinition.Fields.Add(loggerField);

            //create field init
            var staticConstructor = typeDefinition.GetStaticConstructor();
            if (staticConstructor == null)
            {
                const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                staticConstructor = new MethodDefinition(".cctor", methodAttributes, typeReferenceProvider.Void);
                typeDefinition.Methods.Add(staticConstructor);
                staticConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            var getLoggerMethod = new MethodReference("GetLogger", logTypeRef, logManagerTypeRef);
            getLoggerMethod.Parameters.Add(new ParameterDefinition(typeReferenceProvider.Type));

            //build up typeInfo
            var getTypeFromHandleMethod = methodReferenceProvider.GetGetTypeFromHandleReference();

            //spec treatment for generic types 
            var loggerFieldRef = loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            //if generated nested type use the declaring type as logger type as it is more natural from 
            //end users perspective
            var hasCompilerGeneratedAttribute = typeDefinition.HasCustomAttributes && typeDefinition.CustomAttributes
                .Any(attr => attr.AttributeType.FullName.Equals(typeof(CompilerGeneratedAttribute).FullName, StringComparison.Ordinal));

            var loggerTypeDefinition = hasCompilerGeneratedAttribute && typeDefinition.IsNested
                                                ? typeDefinition.DeclaringType :  typeDefinition;

            staticConstructor.Body.InsertAtTheBeginning(new[]
            {
                Instruction.Create(OpCodes.Ldtoken, loggerTypeDefinition.GetGenericInstantiationIfGeneric()),
                Instruction.Create(OpCodes.Call, getTypeFromHandleMethod),
                Instruction.Create(OpCodes.Call, getLoggerMethod),
                Instruction.Create(OpCodes.Stsfld, loggerFieldRef)
            });

            return loggerFieldRef;
        }
    }
}
