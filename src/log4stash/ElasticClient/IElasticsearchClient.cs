using System;
using System.Collections.Generic;
using log4stash.Configuration;

namespace log4stash
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