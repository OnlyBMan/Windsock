using System.Text.Json;

namespace Windsock
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load our config
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath), jsonSerializerOptions) ?? throw new InvalidOperationException("Configuration is empty.");

            // For now, send a simple message to our i1
            SampleMessage sampleMessage = new SampleMessage();
            List<string> segments = sampleMessage.Serialize();
            var packets = PacketEncoding.BuildMessage(segments);
            PacketSending.SendMulticast(packets, config.Network, priority: true);
        }
    }
}