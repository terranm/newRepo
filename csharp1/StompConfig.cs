namespace StompRelayClient
{
    public class StompConfig
    {
        public string WsUrl { get; set; } = "";
        public string SubscribeChannel { get; set; } = "";
        public string PublishChannel { get; set; } = "";
        public int ReconnectIntervalSeconds { get; set; } = 5;
    }
}