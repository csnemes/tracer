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


        public bool? ShouldTraceBasedOnMethodLevelInfo(MethodDefinition definition)
        {
            if (!definition.IsPropertyAccessor())
            {
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                    return true;
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                    return false;
            }
            else
            { //its a property accessor check the prop for the attribute
                var correspondingProp =
                    definition.DeclaringType.Properties.FirstOrDefault(prop => prop.GetMethod == definition || prop.SetMethod == definition);
                if (correspondingProp != null)
                {
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                        return true;
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                        return false;
                }
            }

            return null;
        }

        public bool? ShouldTraceBasedOnClassLevelInfo(MethodDefinition definition)
        {
            var attributeInfo = GetTraceAttributeInfoForType(definition.DeclaringType);

            if (attributeInfo != null)
            {
                if (attributeInfo.IsNoTrace) { return false; }

                var targetVisibility = attributeInfo.TargetVisibility;
                var methodVisibility = VisibilityHelper.GetMethodVisibilityLevel(definition);
                return ((int)targetVisibility >= (int)methodVisibility);

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
                return TraceAttributeInfo.TraceOn(GetTargetVisibilityFromAttribute(traceOnAttribute));
            }

            //no attrib present on type see if we have an outer class
            if (type.DeclaringType != null) return GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type.DeclaringType);

            return null;
        }

        private TraceTargetVisibility GetTargetVisibilityFromAttribute(CustomAttribute attribute)
        {
            var intVal = 0; //defaults to public
            if (attribute.HasProperties)
            {
                intVal = (int)attribute.Properties[0].Argument.Value;
            }
            else if (attribute.HasConstructorArguments)
            {
                intVal = (int)attribute.ConstructorArguments[0].Value;
            }

            return (TraceTargetVisibility)intVal;
        }


        private class TraceAttributeInfo
        {
            private readonly TraceTargetVisibility _targetVisibility;
            private readonly bool _noTrace;

            private TraceAttributeInfo(TraceTargetVisibility targetVisibility, bool noTrace)
            {
                _targetVisibility = targetVisibility;
                _noTrace = noTrace;
            }

            public static TraceAttributeInfo NoTrace()
            {
                return new TraceAttributeInfo(TraceTargetVisibility.All, true);
            }

            public static TraceAttributeInfo TraceOn(TraceTargetVisibility visibility)
            {
                return new TraceAttributeInfo(visibility, false);
            }

            public bool IsNoTrace
            {
                get { return _noTrace; }
            }

            public bool IsTraceOn
            {
                get { return !_noTrace; }
            }

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
