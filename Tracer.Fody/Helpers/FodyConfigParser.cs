using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tracer.Fody.Filters;
using Tracer.Fody.Filters.DefaultFilter;
using Tracer.Fody.Filters.PatternFilter;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Helpers
{
    /// <summary>
    /// Class that parses the fody configuration belonging to tracer (in FodyWeavers.xml file).
    /// </summary>
    internal class FodyConfigParser
    {
        private FodyConfigParser()
        { }

        private string _error;
        private string _adapterAssembly;
        private string _logManager;
        private string _logger;
        private string _staticLogger;
        private string _filter;
        private bool _traceConstructorsFlag;
        private bool _tracePropertiesFlag = true;
        private IEnumerable<XElement> _filterConfigElements;

        public static FodyConfigParser Parse(XElement element)
        {
            var result = new FodyConfigParser();
            result.DoParse(element);
            return result;
        }

        public TraceLoggingConfiguration Result
        {
            get
            {
                var result = TraceLoggingConfiguration.New
                    .WithAdapterAssembly(_adapterAssembly)
                    .WithLogger(_logger)
                    .WithLogManager(_logManager)
                    .WithStaticLogger(_staticLogger);

                if (String.Equals(_filter, "pattern", StringComparison.OrdinalIgnoreCase))
                {
                    result.WithFilter(new PatternFilter(_filterConfigElements));
                }
                else
                {
                    result.WithFilter(new DefaultFilter(_filterConfigElements));
                }

                if (_traceConstructorsFlag) { result.WithConstructorTraceOn(); }
                    else { result.WithConstructorTraceOff(); }

                if (_tracePropertiesFlag) { result.WithPropertiesTraceOn(); }
                else { result.WithPropertiesTraceOff(); }

                return result;
            }
        }

        public bool IsErroneous
        {
            get { return !String.IsNullOrEmpty(_error); }
        }

        public string Error
        {
            get { return _error; }
        }

        private void DoParse(XElement element)
        {

            try
            {
                _adapterAssembly = GetAttributeValue(element, "adapterAssembly", true);
                _logManager = GetAttributeValue(element, "logManager", true);
                _logger = GetAttributeValue(element, "logger", true);
                _staticLogger = GetAttributeValue(element, "staticLogger", false);
                _traceConstructorsFlag = Boolean.Parse(GetAttributeValueOrDefault(element, "traceConstructors", Boolean.FalseString));
                _tracePropertiesFlag = Boolean.Parse(GetAttributeValueOrDefault(element, "traceProperties", Boolean.TrueString));
                _filter = GetAttributeValue(element, "filter", false);
                _filterConfigElements = element.Descendants();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }

        private string GetAttributeValue(XElement element, string attributeName, bool isMandatory)
        {
            var attribute = element.Attribute(attributeName);
            if (isMandatory && (attribute == null || String.IsNullOrWhiteSpace(attribute.Value)))
            {
                throw new Exception(String.Format("Tracer: attribute {0} is missing or empty.", attributeName));
            }

            return attribute != null ? attribute.Value : null;
        }

        private string GetAttributeValueOrDefault(XElement element, string attributeName, string defaultValue)
        {
            var attribute = element.Attribute(attributeName);
            return attribute != null ? attribute.Value : defaultValue;
        }
    }
}
