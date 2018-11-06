using log4net.Appender.ElasticSearch.Configuration;
using System;
using System.Collections.Generic;
using System.Net;

namespace log4net.Appender.ElasticSearch.ElasticClient
{
    public abstract class AbstractWebElasticClient : IElasticsearchClient
    {
        public ServerDataCollection Servers { get; private set; }
        public int Timeout { get; private set; }
        public bool Ssl { get; private set; }
        public bool AllowSelfSignedServerCert { get; private set; }
        public string Url { get { return GetServerUrl(); } }

        protected AbstractWebElasticClient(ServerDataCollection servers,
            int timeout,
            bool ssl,
            bool allowSelfSignedServerCert)
        {
            Servers = servers;
            Timeout = timeout;
            ServicePointManager.Expect100Continue = false;

            // SSL related properties
            Ssl = ssl;
            AllowSelfSignedServerCert = allowSelfSignedServerCert;
        }

        public abstract void PutTemplateRaw(string templateName, string rawBody);
        public abstract void IndexBulk(IEnumerable<InnerBulkOperation> bulk);
        public abstract void IndexBulkAsync(IEnumerable<InnerBulkOperation> bulk);
        public abstract void Dispose();

        protected string GetServerUrl()
        {
            var serverData = Servers.GetRandomServerData();
            var url = string.Format("{0}://{1}:{2}{3}/", Ssl ? "https" : "http", serverData.Address, serverData.Port, String.IsNullOrEmpty(serverData.Path) ? "" : serverData.Path);
            return url;
        }

        protected string GetServerUrl(IServerData serverData)
        {
            var url = string.Format("{0}://{1}:{2}{3}/", Ssl ? "https" : "http", serverData.Address, serverData.Port, String.IsNullOrEmpty(serverData.Path) ? "" : serverData.Path);
            return url;
        }
    }
}