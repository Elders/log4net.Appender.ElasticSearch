using log4net.Appender.ElasticSearch.Configuration;
using System;
using System.Collections.Generic;

namespace log4net.Appender.ElasticSearch
{
    public interface IElasticsearchClient : IDisposable
    {
        ServerDataCollection Servers { get; }
        bool Ssl { get; }
        bool AllowSelfSignedServerCert { get; }
        void PutTemplateRaw(string templateName, string rawBody);
        void IndexBulk(IEnumerable<InnerBulkOperation> bulk);
        void IndexBulkAsync(IEnumerable<InnerBulkOperation> bulk);
    }
}