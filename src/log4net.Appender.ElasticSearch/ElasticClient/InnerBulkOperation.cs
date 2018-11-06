using System.Collections.Generic;

namespace log4net.Appender.ElasticSearch
{
    public class InnerBulkOperation 
    {
        public string IndexName { get; set; }
        public string IndexType { get; set; }
        public object Document { get; set; }
        public Dictionary<string, string> IndexOperationParams { get; set; }

        public InnerBulkOperation()
        {
        }
    }
}