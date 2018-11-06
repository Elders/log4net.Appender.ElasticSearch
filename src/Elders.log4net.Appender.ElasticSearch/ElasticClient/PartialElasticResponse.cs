using Newtonsoft.Json;

namespace log4net.Appender.ElasticSearch
{
    internal sealed class PartialElasticResponse
    {
        [JsonProperty("errors")]
        public bool Errors { get; set; }
    }
}