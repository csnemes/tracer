using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Weaver for a single method
    /// </summary>
    internal class MethodWeaver : MethodWeaverBase
    {
        private Instruction _firstInstructionAfterTraceEnter;


        internal MethodWeaver(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            ILoggerProvider loggerProvider, MethodDefinition methodDefinition) : base(typeReferenceProvider, methodReferenceProvider,
                loggerProvider, methodDefinition)
        {}

        protected override void WeaveTraceEnter()
        {
            _firstInstructionAfterTraceEnter = _body.Instructions.FirstOrDefault();
            var instructions = CreateTraceEnterCallInstructions();
            _body.InsertAtTheBeginning(instructions);
        }

        protected override void WeaveTraceLeave()
        {
            //----------------------

            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodName", methodTimeInTicks, returnValue)
               * 
               * we'd also like to catch any exception at leave without using rethrow as it messes up the callstack
               * we use CLR's fault block capbility to do so
            */
            VariableDefinition returnValueDef = null;
            
            if (HasReturnValue)
            {
                //Declare local variable for the return value
                returnValueDef = _body.GetOrDeclareVariable("$returnValue", ReturnType);
            }

            var allReturns = _body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();
            var handlerStart = CreateExceptionHandlerAtTheEnd();

            var loggingReturnStart = CreateLoggingReturnAtTheEnd(returnValueDef);

            //add exception handler 
            if (!_isEmptyBody)
            {
                _body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = _firstInstructionAfterTraceEnter,
                    TryEnd = handlerStart,
                    HandlerStart = handlerStart,
                    HandlerEnd = loggingReturnStart,
                    CatchType = _typeReferenceProvider.Exception
                });
            }
            
            foreach (var @return in allReturns)
            {
                ChangeReturnToLeaveLoggingReturn(@return, returnValueDef, loggingReturnStart);
            }
        }

        //redirect return to actualReturn
        private void ChangeReturnToLeaveLoggingReturn(Instruction @return, VariableDefinition returnValueDef, Instruction actualReturn)
        {
            var instructions = new List<Instruction>();
            
            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable
            }
            instructions.Add(Instruction.Create(OpCodes.Leave, actualReturn));

            _body.Replace(@return, instructions);
        }


        private Instruction CreateExceptionHandlerAtTheEnd()
        {
            var instructions = new List<Instruction>();

            //store the exception
            var exceptionValue = _body.GetOrDeclareVariable("$exceptionValue", _typeReferenceProvider.Object);
            instructions.Add(Instruction.Create(OpCodes.Stloc, exceptionValue));

            //do the logging 
            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;

            paramNamesDef = _body.GetOrDeclareVariable("$paramNames", _typeReferenceProvider.StringArray);
            paramValuesDef = _body.GetOrDeclareVariable("$paramValues", _typeReferenceProvider.ObjectArray);

            instructions.AddRange(InitArray(paramNamesDef, 1, _typeReferenceProvider.String));
            instructions.AddRange(InitArray(paramValuesDef, 1, _typeReferenceProvider.Object));

            instructions.AddRange(StoreValueReadByInstructionsInArray(paramNamesDef, 0, Instruction.Create(OpCodes.Ldstr, ExceptionMarker)));
            instructions.AddRange(StoreVariableInObjectArray(paramValuesDef, 0, exceptionValue));

            //build up Trace call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());

            //start ticks
            instructions.Add(Instruction.Create(OpCodes.Ldloc, _body.GetVariable(StartTickVarName)));
            //end ticks
            instructions.Add(Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetTimestampReference()));

            instructions.Add(Instruction.Create(OpCodes.Ldloc, paramNamesDef));
            instructions.Add(Instruction.Create(OpCodes.Ldloc, paramValuesDef));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetTraceLeaveReference()));

            //and rethrow
            instructions.Add(Instruction.Create(OpCodes.Rethrow));

            return _body.AddAtTheEnd(instructions);
        }

        private Instruction CreateLoggingReturnAtTheEnd(VariableDefinition returnValueDef)
        {
            var instructions = new List<Instruction>();

            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;
            
            var traceLeaveNeedsParamArray = (HasReturnValue || _body.Method.Parameters.Any(param => param.IsOut || param.ParameterType.IsByReference));
            var traceLeaveParamArraySize = _body.Method.Parameters.Count(param => param.IsOut || param.ParameterType.IsByReference) + (HasReturnValue ? 1 : 0); 

            if (traceLeaveNeedsParamArray)
            {
                //Get local variables for the arrays or declare them if they not exist
                paramNamesDef = _body.GetOrDeclareVariable("$paramNames", _typeReferenceProvider.StringArray);
                paramValuesDef = _body.GetOrDeclareVariable("$paramValues", _typeReferenceProvider.ObjectArray);

                //init arrays
                instructions.AddRange(InitArray(paramNamesDef, traceLeaveParamArraySize, _typeReferenceProvider.String));
                instructions.AddRange(InitArray(paramValuesDef, traceLeaveParamArraySize, _typeReferenceProvider.Object));

                if (HasReturnValue)
                {
                    instructions.AddRange(StoreValueReadByInstructionsInArray(paramNamesDef, 0, Instruction.Create(OpCodes.Ldnull)));
                    instructions.AddRange(StoreVariableInObjectArray(paramValuesDef, 0, returnValueDef));
                }

                instructions.AddRange(
                    BuildInstructionsToCopyParameterNamesAndValues(_body.Method.Parameters.Where(p => p.IsOut || p.ParameterType.IsByReference),
                        paramNamesDef, paramValuesDef, HasReturnValue ? 1 : 0));
            }

            //build up Trace call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());

            //start ticks
            instructions.Add(Instruction.Create(OpCodes.Ldloc, _body.GetVariable(StartTickVarName)));
            //end ticks
            instructions.Add(Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetTimestampReference()));

            instructions.Add(traceLeaveNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramNamesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(traceLeaveNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramValuesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetTraceLeaveReference()));

            //return with original value
            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable
            }
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return _body.AddAtTheEnd(instructions);
        }

        protected override void SearchForAndReplaceStaticLogCalls()
        {
            //look for static log calls
            foreach (var instruction in _body.Instructions.ToList()) //create a copy of the instructions so we can update the original
            {
                var methodReference = instruction.Operand as MethodReference;
                if (instruction.OpCode == OpCodes.Call && methodReference != null && IsStaticLogTypeOrItsInnerType(methodReference.DeclaringType))
                {
                    //change the call
                    if (!methodReference.HasParameters)
                    {
                        ChangeStaticLogCallWithoutParameter(instruction);
                    }
                    else
                    {
                        ChangeStaticLogCallWithParameter(instruction);
                    }
                }
            }
        }

        ///The way how we solve this is a bit lame, but fairly simple. We store all parameters into local variables
        /// then call the instance log method reading the parameters from these variables.
        /// A better solution would be to figure out where the call really begins (where is the bottom of the stack)
        /// and insert the instance ref there plus change the call instraction
        private void ChangeStaticLogCallWithParameter(Instruction oldInstruction)
        {
            var instructions = new List<Instruction>();
            var methodReference = (MethodReference)oldInstruction.Operand;
            var methodReferenceInfo = new MethodReferenceInfo(methodReference);

            if (methodReferenceInfo.IsPropertyAccessor() && methodReferenceInfo.IsSetter)
            {
                throw new ApplicationException("Rewriting static property setters is not supported.");
            }

            var parameters = methodReference.Parameters;

            //create variables to store parameters and push values into them
            var variables = new VariableDefinition[parameters.Count];

            for (int idx = 0; idx < parameters.Count; idx++)
            {
                variables[idx] = GetVariableDefinitionForType(parameters[idx].ParameterType, methodReference, _methodDefinition);
                _body.Variables.Add(variables[idx]);
            }

            //store in reverse order
            for (int idx = parameters.Count - 1; idx >= 0; idx--)
            {
                instructions.Add(Instruction.Create(OpCodes.Stloc, variables[idx]));
            }

            //build-up instance call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());

            for (int idx = 0; idx < parameters.Count; idx++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, variables[idx]));
            }

            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetInstanceLogMethod(methodReferenceInfo, parameters)));

            _body.Replace(oldInstruction, instructions);
        }

        private void ChangeStaticLogCallWithoutParameter(Instruction oldInstruction)
        {
            var instructions = new List<Instruction>();

            var methodReference = (MethodReference)oldInstruction.Operand;
            var methodReferenceInfo = new MethodReferenceInfo(methodReference);

            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));

            if (!methodReferenceInfo.IsPropertyAccessor())
            {
                instructions.AddRange(LoadMethodNameOnStack());
            }

            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetInstanceLogMethod(methodReferenceInfo)));

            _body.Replace(oldInstruction, instructions);
        }
    }
}
