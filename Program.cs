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

            string lfRecordPath = Path.Combine(AppContext.BaseDirectory, "LFRecord");
            string cachedLocationsPath = Path.Combine(AppContext.BaseDirectory, "CachedLocations.json");
            string i1ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.py");
            
            if (!File.Exists(lfRecordPath) && !File.Exists(cachedLocationsPath))
            {
                Console.WriteLine("In order to use Windsock, you need to supply your own LFRecord!");
                return;
            }

            List<LFRecordLocation> locations = LFRecord.ParseLFRecord(lfRecordPath, i1ConfigPath);

            // For now, send a simple message to our i1
            SampleMessage sampleMessage = new SampleMessage();
            List<string> segments = sampleMessage.Serialize();
            var packets = PacketEncoding.BuildMessage(segments);
            PacketSending.SendMulticast(packets, config.Network, priority: true);
        }
    }
}
