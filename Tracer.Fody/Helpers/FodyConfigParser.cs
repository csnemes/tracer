using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tracer.Fody.Filters;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Helpers
{
    internal class FodyConfigParser
    {
        private FodyConfigParser()
        { }

        private string _error;
        private string _adapterAssembly;
        private string _logManager;
        private string _logger;
        private string _staticLogger;
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
                return TraceLoggingConfiguration.New
                    .WithAdapterAssembly(_adapterAssembly)
                    .WithFilter(new DefaultFilter(_filterConfigElements))
                    .WithLogger(_logger)
                    .WithLogManager(_logManager)
                    .WithStaticLogger(_staticLogger);
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
                throw new ApplicationException(String.Format("Tracer: attribute {0} is missing or empty.", attributeName));
            }

            return attribute != null ? attribute.Value : null;
        }
    }
}
