using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Tracer.Fody.Helpers;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Tracer.Fody.Weavers
{
    internal class TypeWeaver
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly TraceLoggingConfiguration _configuration;
        private readonly ILoggerScopeProvider _loggerScopeProvider;
        private FieldDefinition _staticLoggerField;

        private TypeWeaver(TraceLoggingConfiguration configuration, ILoggerScopeProvider loggerScopeProvider, TypeDefinition typeDefinition)
        {
            _configuration = configuration;
            _loggerScopeProvider = loggerScopeProvider;
            _typeDefinition = typeDefinition;
        }

        public static void Execute(TraceLoggingConfiguration configuration, ILoggerScopeProvider loggerScopeProvider, TypeDefinition typeDefinition)
        {
            var weaver = new TypeWeaver(configuration, loggerScopeProvider, typeDefinition);
            weaver.InternalExecute();
        }


        private void InternalExecute()
        {
            foreach (var method in _typeDefinition.GetMethods().ToList())
            {
                //weaving trace
                if (_configuration.Filter.ShouldAddTrace(method) && method.HasBody && !method.IsAbstract)
                {
                    AddTraceEnterAndReturn(method);
                }

                //weaving other log entries
                //TODO
            }
        }

        private void AddTraceEnterAndReturn(MethodDefinition methodDefinition)
        {
            var body = methodDefinition.Body;
            body.SimplifyMacros();

            /* TRACE ENTRY: 
             * What we'd like to achieve is:
             * var paramNames = new string[] { "param1", "param2" }
             * var paramValues = new object[] { param1, param2 }
             * _log.TraceCallEnter("MethodFullName", paramNames, paramValues)
             * var startTick = Stopwatch.GetTimestamp();
             * ...(existing code)...
             */

            //Declare local variables for the two arrays
            var paramNamesDef = new VariableDefinition("paramNames", methodDefinition.Module.Import(typeof(string[])));
            body.Variables.Add(paramNamesDef);
            var paramValuesDef = new VariableDefinition("paramValues", methodDefinition.Module.Import(typeof(object[])));
            body.Variables.Add(paramValuesDef);

            int arraySize = methodDefinition.Parameters.Count(param => !param.IsOut); 
            
            var loggerField = GetStaticLogger();
            var logTraceEnterMethod = new MethodReference("TraceEnter", methodDefinition.Module.Import(typeof(void)), loggerField.FieldType);
            logTraceEnterMethod.HasThis = true; //instance method
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(string))));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(string[]))));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(object[]))));

            var instructions = new List<Instruction>();
            instructions.AddRange(new []
            {
                Instruction.Create(OpCodes.Ldc_I4, arraySize),  //setArraySize
                Instruction.Create(OpCodes.Newarr, methodDefinition.Module.Import(typeof(string))), //create name array
                Instruction.Create(OpCodes.Stloc, paramNamesDef), //store it in local variable
                Instruction.Create(OpCodes.Ldc_I4, arraySize),  //setArraySize
                Instruction.Create(OpCodes.Newarr, methodDefinition.Module.Import(typeof(object))), //create value array
                Instruction.Create(OpCodes.Stloc, paramValuesDef), //store it in local variable
            });

            int thisArgumentOffset = methodDefinition.IsStatic ? 0 : 1;

            //fill name array and param array
            for (int idx = 0; idx < arraySize; idx++)
            {
                var parameter = methodDefinition.Parameters[idx];

                if (parameter.IsOut) continue;

                //set name at index
                instructions.AddRange(new []
                {
                    Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                    Instruction.Create(OpCodes.Ldc_I4, idx),
                    Instruction.Create(OpCodes.Ldstr, parameter.Name),
                    Instruction.Create(OpCodes.Stelem_Ref)
                });

                //TODO boxing, OUT param
                //set value at index
                instructions.Add(Instruction.Create(OpCodes.Ldloc, paramValuesDef));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, idx));
                instructions.Add(Instruction.Create(OpCodes.Ldarg, parameter));
                
                //box if necessary
                if (parameter.ParameterType.IsPrimitive)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                }

                instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
            }


            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldsfld, loggerField),
                Instruction.Create(OpCodes.Ldstr, methodDefinition.FullName),
                Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                Instruction.Create(OpCodes.Ldloc, paramValuesDef),
                Instruction.Create(OpCodes.Callvirt, logTraceEnterMethod)
            });

            body.InsertAtTheBeginning(instructions);
            //----------------------

            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodFullName", methodTimeInTicks, returnValue)
               * 
            */
            //var ldstr = Instruction.Create(OpCodes.Ldstr, "Goodbye");
            //var call = Instruction.Create(OpCodes.Call,
            //    methodDefinition.Module.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })));
            
            AddTraceLeave(methodDefinition);
            
            body.OptimizeMacros();
        }

        private void AddTraceLeave(MethodDefinition methodDefinition)
        {
            VariableDefinition returnValueDef = null;
            bool hasReturnValue = (methodDefinition.ReturnType.MetadataType != MetadataType.Void);

            if (hasReturnValue)
            {
                //Declare local variable for the return value
                returnValueDef = new VariableDefinition("returnValue", methodDefinition.ReturnType);
                methodDefinition.Body.Variables.Add(returnValueDef);
            }

            var allReturns = methodDefinition.Body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                if (hasReturnValue)
                {
                    instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable
                }

                var loggerField = GetStaticLogger();
                var logTraceLeaveMethod = new MethodReference("TraceLeave", methodDefinition.Module.Import(typeof(void)), loggerField.FieldType);
                logTraceLeaveMethod.HasThis = true; //instance method
                logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(string))));
                logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(long))));
                if (hasReturnValue)
                {
                    logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(methodDefinition.Module.Import(typeof(object))));
                }
                
                if (hasReturnValue)
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldsfld, loggerField));
                    instructions.Add(Instruction.Create(OpCodes.Ldstr, methodDefinition.FullName));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L));
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef));
                    
                    //boxing if needed
                    if (methodDefinition.ReturnType.IsPrimitive)
                    {
                        instructions.Add(Instruction.Create(OpCodes.Box, methodDefinition.ReturnType));
                    }

                    instructions.Add(Instruction.Create(OpCodes.Callvirt, logTraceLeaveMethod));
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable
                }
                else
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldsfld, loggerField));
                    instructions.Add(Instruction.Create(OpCodes.Ldstr, methodDefinition.FullName));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L)); 
                    instructions.Add(Instruction.Create(OpCodes.Callvirt, logTraceLeaveMethod));
                }

                instructions.Add(Instruction.Create(OpCodes.Ret));

                methodDefinition.Body.Replace(@return, instructions);
            }
        }


        private FieldDefinition GetStaticLogger()
        {
            if (_staticLoggerField == null)
            {
                _staticLoggerField = CreateLoggerStaticField();
            }

            return _staticLoggerField;
        }

        private FieldDefinition CreateLoggerStaticField()
        {
            //TODO check for existing logger
            var moduleDefinition = _typeDefinition.Module;
            var loggerScope = _loggerScopeProvider.GetLoggerScope();

            var logManager = _configuration.LogMannager;
            var logger = _configuration.Logger;

            var logTypeRef = new TypeReference(logger.Namespace, logger.Name, moduleDefinition, loggerScope);
            var logManagerTypeRef = new TypeReference(logManager.Namespace, logManager.Name, moduleDefinition, loggerScope);

            //look for existing one
            var loggerField = _typeDefinition.Fields.FirstOrDefault(fld => fld.IsStatic && fld.FieldType == logTypeRef);

            if (loggerField == null)
            {
                //TODO check if the _log name is used for something else and use a unqiue name
                loggerField = new FieldDefinition("_log", FieldAttributes.Public | FieldAttributes.Static, logTypeRef);
                _typeDefinition.Fields.Add(loggerField);

                //create field init
                var staticConstructor = _typeDefinition.GetStaticConstructor();
                if (staticConstructor == null)
                {
                    const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                    staticConstructor = new MethodDefinition(".cctor", methodAttributes, moduleDefinition.TypeSystem.Void);
                    _typeDefinition.Methods.Add(staticConstructor);
                }

                var getLoggerMethod = new MethodReference("GetLogger", logTypeRef, logManagerTypeRef);
                getLoggerMethod.Parameters.Add(new ParameterDefinition(moduleDefinition.Import(typeof(Type))));

                //build up typeInfo

                var getTypeFromHandleMethod =
                moduleDefinition.Import(typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));


                staticConstructor.Body.InsertAtTheBeginning(new[]
                    {
                        Instruction.Create(OpCodes.Ldtoken, _typeDefinition),
                        Instruction.Create(OpCodes.Call, getTypeFromHandleMethod),
                        Instruction.Create(OpCodes.Call, getLoggerMethod),
                        Instruction.Create(OpCodes.Stsfld, loggerField),
                        Instruction.Create(OpCodes.Ret)
                    });
            }

            return loggerField;
        }
    }
}
