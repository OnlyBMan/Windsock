namespace Windsock
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            Config config = Config.Load(configPath);

            string lfRecordPath = Path.Combine(AppContext.BaseDirectory, "LFRecord");
            string cachedLocationsPath = Path.Combine(AppContext.BaseDirectory, "CachedLocations.json");
            string i1ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.py");

            if (!File.Exists(lfRecordPath) && !File.Exists(cachedLocationsPath))
            {
                Console.WriteLine("In order to use Windsock, you need to supply your own LFRecord!");
                return;
            }

            Dictionary<string, List<string>> interests = I1Config.Parse(i1ConfigPath);
            List<LFRecordLocation> locations = LFRecord.ParseLFRecord(lfRecordPath, interests);
            List<string> stationIds = interests.GetValueOrDefault("obsStation") ?? new List<string>();

            Dictionary<string, TWC_CurrentObservation> observations = await DataCollector.CollectCurrentConditions(locations, stationIds, config.API);
            foreach (KeyValuePair<string, TWC_CurrentObservation> observation in observations)
            {
                List<string> segments = CurrentConditions.BuildSegments(observation.Key, observation.Value);
                List<byte[]> packets = PacketEncoding.BuildMessage(segments);
                PacketSending.SendMulticast(packets, config.Network, priority: true);
            }
        }
    }
}
