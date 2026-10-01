using System.Text.Json;

namespace Windsock
{
    public class Config
    {
        public NetworkConfig Network { get; set; } = new();
        public ApiConfig API { get; set; } = new();

        public static Config Load(string configPath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using JsonDocument json = JsonDocument.Parse(File.ReadAllText(configPath));

            return new Config
            {
                Network = json.RootElement.GetProperty("network").Deserialize<NetworkConfig>(options)
                    ?? throw new InvalidOperationException("Network configuration is empty."),
                API = json.RootElement.GetProperty("api").Deserialize<ApiConfig>(options)
                    ?? throw new InvalidOperationException("API configuration is empty.")
            };
        }
    }

    public class NetworkConfig
    {
        public string InterfaceAddress { get; set; } = "10.100.102.1";
        public string MulticastAddress { get; set; } = "224.1.1.77";
        public int RoutinePort { get; set; } = 7777;
        public int PriorityPort { get; set; } = 7778;
        public int Ttl { get; set; } = 2;
        public int PacketDelayMs { get; set; } = 10;
    }

    public class ApiConfig
    {
        public string TwcForecastsKey { get; set; } = "your_api_key_here";
        public string TwcRadarKey { get; set; } = "your_api_key_here";
    }
}
