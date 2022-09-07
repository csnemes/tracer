using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Filters
{
    public class TraceAttributeHelper
    {
        private readonly Dictionary<string, TraceAttributeInfo> _traceAttributeCache = new Dictionary<string, TraceAttributeInfo>();


        public FilterResult? ShouldTraceBasedOnMethodLevelInfo(MethodDefinition definition)
        {
            while (true)
            {
                if (!definition.IsPropertyAccessor())
                {
                    if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                        return new FilterResult(true, GetTraceOnAttributeParameters(definition));
                    if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                        return new FilterResult(false);
                }
                else
                { //its a property accessor check the prop for the attribute
                    var correspondingProp =
                        definition.DeclaringType.Properties.FirstOrDefault(prop => prop.GetMethod == definition || prop.SetMethod == definition);
                    if (correspondingProp != null)
                    {
                        if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                            return new FilterResult(true, GetTraceOnAttributeParameters(definition));
                        if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                            return new FilterResult(false);
                    }
                }

                MethodDefinition baseDefinition = Mono.Cecil.Rocks.MethodDefinitionRocks.GetBaseMethod(definition);
                if (baseDefinition == definition)
                    break;

                definition = baseDefinition;
            }

            return null;
        }

        private Dictionary<string, string> GetTraceOnAttributeParameters(MethodDefinition definition)
        {
            var attribute = definition.CustomAttributes.FirstOrDefault(attr =>
                attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal));
            return GetTraceOnAttributeParameters(attribute);
        }

        private Dictionary<string, string> GetTraceOnAttributeParameters(CustomAttribute attribute)
        {
            var result = new Dictionary<string, string>();
            if (attribute != null && attribute.HasProperties)
            {
                foreach (var property in attribute.Properties)
                {
                    result.Add(property.Name, property.Argument.Value?.ToString());
                }
            }

            return result;
        }

        public FilterResult? ShouldTraceBasedOnClassLevelInfo(MethodDefinition definition)
        {
            var attributeInfo = GetTraceAttributeInfoForType(definition.DeclaringType);

            if (attributeInfo != null)
            {
                if (attributeInfo.IsNoTrace) { return new FilterResult(false); }

                var targetVisibility = attributeInfo.TargetVisibility;
                var methodVisibility = VisibilityHelper.GetMethodVisibilityLevel(definition);
                var shouldTrace = (int)targetVisibility >= (int)methodVisibility;

                return new FilterResult(shouldTrace, attributeInfo.Parameters);
            }

            return null;
        }

        private TraceAttributeInfo GetTraceAttributeInfoForType(TypeDefinition type)
        {
            TraceAttributeInfo result = null;
            if (!_traceAttributeCache.TryGetValue(type.FullName, out result))
            {
                result = GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type);
                _traceAttributeCache.Add(type.FullName, result);
            }

            return result;
        }

        private TraceAttributeInfo GetNearestTraceAttributeWalkingUpTheTypeNestingChain(TypeDefinition type)
        {
            //with NoTrace present skip tracing
            if (type.CustomAttributes.Any(
                attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
            {
                return TraceAttributeInfo.NoTrace();
            }

            var traceOnAttribute = type.CustomAttributes.FirstOrDefault(
                attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal));

            if (traceOnAttribute != null)
            {
                return TraceAttributeInfo.TraceOn(GetTargetVisibilityFromAttribute(traceOnAttribute), GetTraceOnAttributeParameters(traceOnAttribute));
            }

            //no attrib present on type see if we have an outer class
            if (type.DeclaringType != null) return GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type.DeclaringType);

            return null;
        }

        internal static TraceTargetVisibility GetTargetVisibilityFromAttribute(CustomAttribute attribute)
        {
            var intVal = 0; //defaults to public
            if (attribute.HasProperties)
            {
                var targetProp = attribute.Properties
                    .Where(it => it.Name.Equals("Target", StringComparison.OrdinalIgnoreCase)).ToList();
                if (targetProp.Any())
                {
                    return (TraceTargetVisibility)(int)targetProp[0].Argument.Value;
                }
            }

            if (attribute.HasConstructorArguments)
            {
                intVal = (int)attribute.ConstructorArguments[0].Value;
            }

            return (TraceTargetVisibility)intVal;
        }


        private class TraceAttributeInfo
        {
            private readonly TraceTargetVisibility _targetVisibility;
            private readonly bool _noTrace;
            private readonly Dictionary<string, string> _parameters;

            private TraceAttributeInfo(TraceTargetVisibility targetVisibility, bool noTrace, Dictionary<string, string> parameters = null)
            {
                _targetVisibility = targetVisibility;
                _noTrace = noTrace;
                _parameters = parameters;
            }

            public static TraceAttributeInfo NoTrace()
            {
                return new TraceAttributeInfo(TraceTargetVisibility.All, true);
            }

            public static TraceAttributeInfo TraceOn(TraceTargetVisibility visibility, Dictionary<string, string> parameters)
            {
                return new TraceAttributeInfo(visibility, false, parameters);
            }

            public bool IsNoTrace => _noTrace;

            public bool IsTraceOn => !_noTrace;

            public Dictionary<string, string> Parameters => _parameters ?? new Dictionary<string, string>();

            public TraceTargetVisibility TargetVisibility
            {
                get
                {
                    if (!IsTraceOn) throw new Exception("Not a traceOn result.");
                    return _targetVisibility;
                }
            }
        }
    }
}
