using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Porvides types used during weaving bound to the given module definition.
    /// </summary>
    internal class TypeReferenceProvider
    {
        private readonly Lazy<TypeReference> _stringArray;
        private readonly Lazy<TypeReference> _objectArray;
        private readonly Lazy<TypeReference> _type; 
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TraceLoggingConfiguration _configuration;
        private readonly ILoggerAdapterMetadataScopeProvider _loggerAdapterMetadataScopeProvider;

        public TypeReferenceProvider(TraceLoggingConfiguration configuration, ILoggerAdapterMetadataScopeProvider loggerAdapterMetadataScopeProvider, ModuleDefinition moduleDefinition)
        {
            _configuration = configuration;
            _moduleDefinition = moduleDefinition;
            _loggerAdapterMetadataScopeProvider = loggerAdapterMetadataScopeProvider;
            _stringArray = new Lazy<TypeReference>(() => moduleDefinition.Import(typeof(string[])));
            _objectArray = new Lazy<TypeReference>(() => moduleDefinition.Import(typeof(object[])));
            _type = new Lazy<TypeReference>(() => moduleDefinition.Import(typeof(Type)));
        }

        public TypeReference StringArray
        {
            get { return _stringArray.Value; }
        }

        public TypeReference ObjectArray
        {
            get { return _objectArray.Value; }
        }

        public TypeReference Type
        {
            get { return _type.Value; }
        }

        public TypeReference String
        {
            get { return _moduleDefinition.TypeSystem.String; }
        }
        public TypeReference Object
        {
            get { return _moduleDefinition.TypeSystem.Object; }
        }

        public TypeReference Void
        {
            get { return _moduleDefinition.TypeSystem.Void; }
        }

        public TypeReference LogAdapterReference
        {
            get
            {
                var loggerScope = _loggerAdapterMetadataScopeProvider.GetLoggerAdapterMetadataScope();
                var logger = _configuration.Logger; return new TypeReference(logger.Namespace, logger.Name, _moduleDefinition, loggerScope); 
            }
        }

        public TypeReference LogManagerReference
        {
            get
            {
                var loggerScope = _loggerAdapterMetadataScopeProvider.GetLoggerAdapterMetadataScope();
                var logManager = _configuration.LogMannager;
                return new TypeReference(logManager.Namespace, logManager.Name, _moduleDefinition, loggerScope);
            }
        }
    }
}
