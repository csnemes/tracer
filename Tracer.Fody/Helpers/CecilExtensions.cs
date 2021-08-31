using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Tracer.Fody.Helpers
{
    /// <summary>
    /// Helpers for Cecil
    /// </summary>
    public static class CecilExtensions
    {
        #region Functions for cloning body instructions

        public static ICollection<ExceptionHandler> CopyExceptions(this IEnumerable<ExceptionHandler> exceptionHandlersToCopy, IEnumerable<Instruction> newInstructions)
        {
            ICollection<ExceptionHandler> newHandlers = new Mono.Collections.Generic.Collection<ExceptionHandler>();

            foreach (ExceptionHandler origHandler in exceptionHandlersToCopy)
            {
                ExceptionHandler clonedHandler = new ExceptionHandler(origHandler.HandlerType);
                if (origHandler.CatchType != null)
                {
                    clonedHandler.CatchType = origHandler.CatchType;
                }
                if (origHandler.HandlerStart != null)
                {
                    clonedHandler.HandlerStart = newInstructions.FindInstructionByOffset(origHandler.HandlerStart.Offset);
                }

                if (origHandler.HandlerEnd != null)
                {
                    clonedHandler.HandlerEnd = newInstructions.FindInstructionByOffset(origHandler.HandlerEnd.Offset);
                }

                if (origHandler.FilterStart != null)
                {
                    clonedHandler.FilterStart = newInstructions.FindInstructionByOffset(origHandler.FilterStart.Offset);
                }

                if (origHandler.TryEnd != null)
                {
                    clonedHandler.TryEnd = newInstructions.FindInstructionByOffset(origHandler.TryEnd.Offset);
                }

                if (origHandler.TryStart != null)
                {
                    clonedHandler.TryStart = newInstructions.FindInstructionByOffset(origHandler.TryStart.Offset);
                }

                newHandlers.Add(clonedHandler);
            }
            return newHandlers;
        }

        private static Instruction FindInstructionByOffset(this IEnumerable<Instruction> instructions, int offsetToFind)
        {
            foreach (Instruction instruction in instructions)
            {
                if (instruction.Offset == offsetToFind)
                {
                    return instruction;
                }
            }
            throw new ArgumentException("Given offset instruction was not found", nameof(offsetToFind));
        }

        /// <summary>
        /// Clones instruction & fixes branching.
        /// </summary>
        /// <returns> Cloned instruction. </returns>
        public static IEnumerable<Instruction> CloneInstructions(this IEnumerable<Instruction> instructions)
        {
            List<Instruction> clonedInstructions = new List<Instruction>();
            int instructionOffset = 0;

            foreach (Instruction instr in instructions)
            {
                instr.Offset = instructionOffset;
                Instruction clonedInstruction = instr.CloneInstruction();
                clonedInstructions.Add(clonedInstruction);
                ++instructionOffset;
            };

            var mapInstructions = clonedInstructions.GetReferenceMap();
            foreach (Instruction instruction in clonedInstructions)
            {
                instruction.FixBranching(mapInstructions);
            }
            return clonedInstructions;
        }

        /// <summary>
        /// Clones instruction as new one.
        /// </summary>
        /// <returns> Cloned instruction. </returns>
        private static Instruction CloneInstruction(this Instruction instr)
        {
            var newInstr = Instruction.Create(OpCodes.Nop);
            newInstr.OpCode = instr.OpCode;
            newInstr.Offset = instr.Offset;
            newInstr.Operand = instr.Operand;
            return newInstr;
        }

        /// <summary>
        /// Returns reference map created by Offset. Instructions must have options
        /// </summary>
        private static IDictionary<int, Instruction> GetReferenceMap(this IEnumerable<Instruction> instructionsWithOffsets)
        {
            return instructionsWithOffsets.ToDictionary(x => x.Offset, y => y);
        }

        /// <summary>
        /// After cloning if branches shows jump to original instuction instead new one. Calling this function on inctructions will fix it.
        /// </summary>
        public static void FixBranching(this Instruction instr, IDictionary<int,Instruction> referenceMap)
        {
            if (instr.Operand is Instruction)
            {
                Instruction operandInstruction = (Instruction)instr.Operand;
                instr.Operand = referenceMap[operandInstruction.Offset];
            }
            else if (instr.Operand is Instruction[])
            {
                Instruction[] operandInstructions = (Instruction[])instr.Operand;
                foreach(Instruction  operandInstruction in operandInstructions)
                {
                    operandInstruction.FixBranching(referenceMap);
                }
            }
        }

        #endregion

        /// <summary>
        /// Inserts the given instructions before the current (this) instruction using the given processor
        /// </summary>
        public static void InsertBefore(this Instruction instruction, ILProcessor processor, IEnumerable<Instruction> instructions)
        {
            foreach (var newInstruction in instructions)
            {
                processor.InsertBefore(instruction, newInstruction);
            }
        }

        /// <summary>
        /// Inserts the given instructions after the current (this) instruction using the given processor
        /// </summary>
        public static void InsertAfter(this Instruction instruction, ILProcessor processor, IEnumerable<Instruction> instructions)
        {
            foreach (var newInstruction in instructions.Reverse())
            {
                processor.InsertAfter(instruction, newInstruction);
            }
        }

        /// <summary>
        /// Inserts the given instructions at the beginning of the method body keeping the debug sequence point intact
        /// </summary>
        public static void InsertAtTheBeginning(this MethodBody body, IEnumerable<Instruction> instructions)
        {
            var debugInfo = body.Method?.DebugInformation;
            var processor = body.GetILProcessor();
            if (body.Instructions.Count > 0)
            {
                var seqPointToReplace = debugInfo?.GetSequencePoint(body.Instructions[0]);
                body.Instructions[0].InsertBefore(processor, instructions);

                if (seqPointToReplace != null)
                {
                    var newSeqPoint = new SequencePoint(body.Instructions[0], seqPointToReplace.Document)
                    {
                        StartColumn = seqPointToReplace.StartColumn,
                        EndColumn = seqPointToReplace.EndColumn,
                        StartLine = seqPointToReplace.StartLine,
                        EndLine = seqPointToReplace.EndLine
                    };
                    var idx = debugInfo.SequencePoints.IndexOf(seqPointToReplace);
                    debugInfo.SequencePoints[idx] = newSeqPoint;
                }

            }
            else
            {
                foreach (var instruction in instructions)
                {
                    processor.Append(instruction);
                }
            }
        }

        /// <summary>
        /// Inserts the given instructions at the end of the method body keeping the debug sequence point intact, returning the first 
        /// </summary>
        /// <returns>
        /// The first isntruction added
        /// </returns>
        public static Instruction AddAtTheEnd(this MethodBody body, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                body.Instructions.Add(instruction);
            }
            return instructions.First();
        }

        public static void Replace(this MethodBody body, Instruction instructionToReplace, ICollection<Instruction> newInstructions)
        {
            Replace(body.Instructions, instructionToReplace, newInstructions);
        }

        /// <summary>
        /// Replaces the given instruction in the collection of instructions with the new instructions
        /// </summary>
        public static void Replace(this Collection<Instruction> collection, Instruction instructionToReplace, ICollection<Instruction> newInstructions)
        {
            var newInstruction = newInstructions.First();
            instructionToReplace.Operand = newInstruction.Operand;
            instructionToReplace.OpCode = newInstruction.OpCode;

            var indexOf = collection.IndexOf(instructionToReplace);
            foreach (var instruction1 in newInstructions.Skip(1))
            {
                collection.Insert(indexOf + 1, instruction1);
                indexOf++;
            }
        }


        public static TypeReference GetGenericInstantiationIfGeneric(this TypeReference definition)
        {
            if (!definition.HasGenericParameters) return definition;
            var instType = new GenericInstanceType(definition);
            foreach (var parameter in definition.GenericParameters)
            {
                instType.GenericArguments.Add(parameter);
            }
            return instType;
        }

        public static FieldReference FixFieldReferenceToUseSameGenericArgumentsAsVariable(this FieldReference fieldReference, VariableDefinition variableDefinition)
        {
            if (fieldReference.DeclaringType.HasGenericParameters)
            {
                var instType = new GenericInstanceType(fieldReference.DeclaringType);
                foreach (var arg in ((GenericInstanceType)variableDefinition.VariableType).GenericArguments)
                {
                    instType.GenericArguments.Add(arg);
                }
                return new FieldReference(fieldReference.Name, fieldReference.FieldType, instType);
            }
            return fieldReference;
        }

        public static FieldReference FixFieldReferenceIfDeclaringTypeIsGeneric(this FieldReference fieldReference)
        {
            if (fieldReference.DeclaringType.HasGenericParameters)
            {
                return new FieldReference(fieldReference.Name, fieldReference.FieldType, fieldReference.DeclaringType.GetGenericInstantiationIfGeneric());
            }
            return fieldReference;
        }

        public static VariableDefinition DeclareVariable(this MethodBody body, string name, TypeReference type)
        {
            var variable = new VariableDefinition(type);
            body.Variables.Add(variable);
            var variableDebug = new VariableDebugInformation(variable, name);
            body.Method?.DebugInformation?.Scope?.Variables?.Add(variableDebug);
            return variable;
        }

        public static bool IsPropertyAccessor(this MethodDefinition methodDefinition)
        {
            return (methodDefinition.IsGetter || methodDefinition.IsSetter);
        }
    }
}
