using log4net.Appender.ElasticSearch.Extensions;
using log4net.Appender.ElasticSearch.SmartFormatters;
using System.Collections.Generic;

namespace log4net.Appender.ElasticSearch.Filters
{
    public class RenameKeyFilter : IElasticAppenderFilter
    {
        private LogEventSmartFormatter _key;
        private LogEventSmartFormatter _renameTo;
        private const string FailedToRename = "RenameFailed";

        public bool Overwrite { get; set; }

        [PropertyNotEmpty]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        [PropertyNotEmpty]
        public string RenameTo
        {
            get { return _renameTo; }
            set { _renameTo = value; }
        }

        public RenameKeyFilter()
        {
            Overwrite = true;
        }

        public void PrepareConfiguration(IElasticsearchClient client)
        {
        }

        public void PrepareEvent(Dictionary<string, object> logEvent)
        {
            object token;
            string key = _key.Format(logEvent);
            if (logEvent.TryGetValue(key, out token))
            {
                logEvent.Remove(key);

                var newName = _renameTo.Format(logEvent);

                if (!Overwrite && logEvent.ContainsKey(newName))
                {
                    logEvent.AddTag(FailedToRename);
                    return;
                }

                logEvent[newName] = token;
            }
        }
    }
}