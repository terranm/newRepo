using System.Collections.Generic;

namespace StompRelayClient
{
    public class ControllerConfig
    {
        public string Name { get; set; } = "Unknown";
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9999;
    }

    public class StompConfig
    {
        public string WsUrl { get; set; } = "";
        public string SubscribeChannel { get; set; } = "";
        public string PublishChannel { get; set; } = "";
        public int ReconnectIntervalSeconds { get; set; } = 5;
        public List<ControllerConfig> Controllers { get; set; } = new List<ControllerConfig>();
    }
}