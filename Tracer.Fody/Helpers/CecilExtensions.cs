using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Inserts the given instructions at the beginning of the method body keeping the debug sequence point intact
        /// </summary>
        public static void InsertAtTheBeginning(this MethodBody body, IEnumerable<Instruction> instructions)
        {
            var processor = body.GetILProcessor();
            if (body.Instructions.Count > 0)
            {
                var enteringSeqPoint = body.Instructions[0].SequencePoint;
                body.Instructions[0].InsertBefore(processor, instructions);
                body.Instructions[0].SequencePoint = enteringSeqPoint;
            }
            else
            {
                foreach (var instruction in instructions)
                {
                    processor.Append(instruction);
                }
            }
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

        public static FieldReference FixFieldReferenceIfDeclaringTypeIsGeneric(this FieldReference fieldReference)
        {
            if (fieldReference.DeclaringType.HasGenericParameters)
            {
                return new FieldReference(fieldReference.Name, fieldReference.FieldType, fieldReference.DeclaringType.GetGenericInstantiationIfGeneric());
            }
            return fieldReference;
        }

        public static VariableDefinition GetOrDeclareVariable(this MethodBody body, string name, TypeReference type)
        {
            var variable = body.Variables.FirstOrDefault(p => p.Name.Equals(name));
            if (variable == null)
            {
                variable = new VariableDefinition(name, type);
                body.Variables.Add(variable);
            }

            return variable;
        }

        public static VariableDefinition GetVariable(this MethodBody body, string name)
        {
            var variable = body.Variables.FirstOrDefault(p => p.Name.Equals(name));
            if (variable == null)
            {
                throw new ArgumentException(String.Format("Variable {0} not found in method {1}", name, body.Method.Name));
            }

            return variable;
        }
    }
}
