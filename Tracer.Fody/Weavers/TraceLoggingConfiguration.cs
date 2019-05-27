using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    public class TraceLoggingConfiguration
    {
        private readonly ITraceLoggingFilter _filter;
        private readonly AssemblyName _adapterAssemblyName;
        private readonly string _loggerAdapterTypeFullName;
        private readonly string _logManagerAdapterTypeFullName;
        private readonly string _staticLoggerTypeFullName;
        private readonly bool _traceConstructors;
        private readonly bool _traceProperties;

        private TraceLoggingConfiguration(ITraceLoggingFilter filter, string adapterAssemblyDisplayName, 
            string loggerAdapterTypeName, string logManagerAdapterTypeName, string staticLoggerTypeFullName, bool traceConstructors
            ,bool traceProperties)
        {
            _filter = filter ?? NullFilter.Instance;
            _adapterAssemblyName = new AssemblyName(adapterAssemblyDisplayName ?? "Tracer.LogAdapter, Version=1.0.0.0");
            //set version to avoid cecil nullRef exception issue
            if (_adapterAssemblyName.Version == null) _adapterAssemblyName.Version = new Version(0,0,0,0);
            _loggerAdapterTypeFullName = loggerAdapterTypeName ?? "Tracer.LogAdapter.ILog";
            _logManagerAdapterTypeFullName = logManagerAdapterTypeName ?? "Tracer.LogAdapter.LogManager";
            _staticLoggerTypeFullName = staticLoggerTypeFullName ?? "Tracer.LogAdapter.Log";
            _traceConstructors = traceConstructors;
            _traceProperties = traceProperties;
        }

        public static TraceLoggingConfigurationBuilder New
        {
            get {  return new TraceLoggingConfigurationBuilder(); }
        }

        public ITraceLoggingFilter Filter
        {
            get { return _filter; }
        }

        public AssemblyNameReference AssemblyNameReference
        {
            get
            {
                return new AssemblyNameReference(_adapterAssemblyName.Name, _adapterAssemblyName.Version)
                {
                    PublicKeyToken = _adapterAssemblyName.GetPublicKeyToken()
                };
            }
        }

        public TypeName LogMannager
        {
            get
            {
                return new TypeName(_logManagerAdapterTypeFullName);
            }
        }

        public TypeName Logger
        {
            get
            {
                return new TypeName(_loggerAdapterTypeFullName);
            }
        }

        public TypeName StaticLogger
        {
            get
            {
                return new TypeName(_staticLoggerTypeFullName);
            }
        }

        public bool ShouldTraceConstructors
        {
            get { return _traceConstructors; }
        }

        public bool ShouldTraceProperties
        {
            get {  return _traceProperties; }
        }

        public class TypeName
        {
            private readonly string _namespace;
            private readonly string _name;

            public TypeName(string fullName)
            {
                //TODO checks, validation
                var nameSplit = fullName.Split('.');
                _name = nameSplit[nameSplit.Length - 1];
                _namespace = String.Join(".", nameSplit.Take(nameSplit.Length - 1));
            }

            public TypeName(string ns, string name)
            {
                _namespace = ns;
                _name = name;
            }

            public string Namespace
            {
                get { return _namespace; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        private class NullFilter : ITraceLoggingFilter
        {
            public static readonly NullFilter Instance = new NullFilter();

            private NullFilter()
            {}

            public FilterResult ShouldAddTrace(MethodDefinition definition)
            {
                return new FilterResult(true);
            }

            public void LogFilterInfo(IWeavingLogger weavingLogger)
            {
            }
        }

        public class TraceLoggingConfigurationBuilder
        {
            private ITraceLoggingFilter _filter;
            private string _loggerAdapterAssemblyDisplayName;
            private string _loggerAdapterTypeName;
            private string _logManagerAdapterTypeName;
            private string _staticLoggerTypeName;
            private bool _traceConstructors = false;
            private bool _traceProperties = true;

            public static implicit operator TraceLoggingConfiguration(TraceLoggingConfigurationBuilder builder)
            {
                return new TraceLoggingConfiguration(
                    builder._filter,
                    builder._loggerAdapterAssemblyDisplayName,
                    builder._loggerAdapterTypeName,
                    builder._logManagerAdapterTypeName,
                    builder._staticLoggerTypeName,
                    builder._traceConstructors,
                    builder._traceProperties);
            }

            public TraceLoggingConfigurationBuilder WithFilter(ITraceLoggingFilter filter)
            {
                _filter = filter;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithAdapterAssembly(string displayName)
            {
                _loggerAdapterAssemblyDisplayName = displayName;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithLogManager(string name)
            {
                _logManagerAdapterTypeName = name;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithLogger(string name)
            {
                _loggerAdapterTypeName = name;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithStaticLogger(string name)
            {
                _staticLoggerTypeName = name;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithConstructorTraceOn()
            {
                _traceConstructors = true;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithConstructorTraceOff()
            {
                _traceConstructors = false;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithPropertiesTraceOn()
            {
                _traceProperties = true;
                return this;
            }

            public TraceLoggingConfigurationBuilder WithPropertiesTraceOff()
            {
                _traceProperties = false;
                return this;
            }
        }

    }


}
