using System.Collections.Generic;
using System.Xml;
using log4net.Appender.ElasticSearch.Extensions;
using log4net.Appender.ElasticSearch.SmartFormatters;
using Newtonsoft.Json;

namespace log4net.Appender.ElasticSearch.Filters
{
    public class XmlFilter : IElasticAppenderFilter
    {
        private LogEventSmartFormatter _sourceKey;
        private JsonFilter _jsonFilter;

        [PropertyNotEmpty]
        public string SourceKey
        {
            get { return _sourceKey; }
            set { _sourceKey = value; }
        }

        public bool FlattenXml { get; set; }

        public string Separator { get; set; }

        public XmlFilter()
        {
            SourceKey = "XmlRaw";
            FlattenXml = false;
            Separator = JsonFilter.DefaultSeparator;
        }

        public void PrepareConfiguration(IElasticsearchClient client)
        {
            _jsonFilter = new JsonFilter {FlattenJson = FlattenXml, SourceKey = SourceKey, Separator = Separator};
            _jsonFilter.PrepareConfiguration(client);
        }

        public void PrepareEvent(Dictionary<string, object> logEvent)
        {
            var key = _sourceKey.Format(logEvent);
            if (!logEvent.TryGetStringValue(key, out string input))
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(input);
            var jsonDoc = JsonConvert.SerializeXmlNode(xmlDoc);
            logEvent[key] = jsonDoc;
            _jsonFilter.PrepareEvent(logEvent);
        }
    }
}
