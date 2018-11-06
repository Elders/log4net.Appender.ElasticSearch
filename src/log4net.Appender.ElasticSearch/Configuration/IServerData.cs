namespace log4net.Appender.ElasticSearch.Configuration
{
    public interface IServerData
    {
        string Address { get; set; }
        int Port { get; set; }
        string Path { get; set; }
    }
}