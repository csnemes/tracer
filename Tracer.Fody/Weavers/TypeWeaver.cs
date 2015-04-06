using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
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

        internal TypeWeaver(ITraceLoggingFilter filter, TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            TypeDefinition typeDefinition)
        {
            _filter = filter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _typeDefinition = typeDefinition;
            _staticLoggerField = new Lazy<FieldReference>(CreateLoggerStaticField);
            _methodWeaverFactory = new MethodWeaverFactory(typeReferenceProvider, methodReferenceProvider, this);
        }

        public void Execute()
        {
            foreach (var method in _typeDefinition.GetMethods().Where(method => method.HasBody && !method.IsAbstract).ToList())
            {
                if (AlreadyWeaved(method)) continue;
                
                bool shouldAddTrace = _filter.ShouldAddTrace(method);
               
                _methodWeaverFactory.Create(method).Execute(shouldAddTrace);
            }
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

        private FieldReference CreateLoggerStaticField()
        {
            var logTypeRef = _typeReferenceProvider.LogAdapterReference;
            var logManagerTypeRef = _typeReferenceProvider.LogManagerReference;

            //look for existing one
            var loggerField = _typeDefinition.Fields.FirstOrDefault(fld => fld.IsStatic && fld.FieldType == logTypeRef);

            if (loggerField != null) return loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            //$log should be unique
            loggerField = new FieldDefinition("$log", FieldAttributes.Private | FieldAttributes.Static, logTypeRef); 
            _typeDefinition.Fields.Add(loggerField);

            //create field init
            var staticConstructor = _typeDefinition.GetStaticConstructor();
            if (staticConstructor == null)
            {
                const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                staticConstructor = new MethodDefinition(".cctor", methodAttributes, _typeReferenceProvider.Void);
                _typeDefinition.Methods.Add(staticConstructor);
                staticConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            var getLoggerMethod = new MethodReference("GetLogger", logTypeRef, logManagerTypeRef);
            getLoggerMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.Type));

            //build up typeInfo
            var getTypeFromHandleMethod = _methodReferenceProvider.GetGetTypeFromHandleReference();

            //spec treatment for generic types 
            var loggerFieldRef = loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            staticConstructor.Body.InsertAtTheBeginning(new[]
            {
                Instruction.Create(OpCodes.Ldtoken, _typeDefinition.GetGenericInstantiationIfGeneric()),
                Instruction.Create(OpCodes.Call, getTypeFromHandleMethod),
                Instruction.Create(OpCodes.Call, getLoggerMethod),
                Instruction.Create(OpCodes.Stsfld, loggerFieldRef)
            });

            return loggerFieldRef;
        }
    }
}
