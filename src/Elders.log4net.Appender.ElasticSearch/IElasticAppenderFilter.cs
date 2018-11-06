using System.Collections.Generic;

namespace log4net.Appender.ElasticSearch
{
    public interface IElasticAppenderFilter 
    {
        void PrepareConfiguration(IElasticsearchClient client);
        void PrepareEvent(Dictionary<string, object> logEvent);
    }
}