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

            I1Config i1Config = I1ConfigParser.Parse(i1ConfigPath);
            Dictionary<string, List<string>> interests = i1Config.Interests;
            List<LFRecordLocation> locations = LFRecord.ParseLFRecord(lfRecordPath, interests);
            List<string> stationIds = interests.GetValueOrDefault("obsStation") ?? new List<string>();
            Dictionary<string, TWC_CurrentObservation> observations = await DataCollector.CollectCurrentConditions(locations, stationIds, config.API);

            foreach (KeyValuePair<string, TWC_CurrentObservation> observation in observations)
            {
                List<string> segments = CurrentConditions.BuildSegments(observation.Key, observation.Value);
                if (segments.Count == 0)
                {
                    continue;
                }
                List<byte[]> packets = PacketEncoding.BuildMessage(segments);
                PacketSending.SendMulticast(packets, config.Network, priority: true);
            }

            List<string> coopIds = interests.GetValueOrDefault("coopId") ?? new List<string>();
            Dictionary<string, TWC_DailyForecast> dailyForecasts = await DataCollector.CollectDailyForecasts(locations, coopIds, config.API);
            foreach (KeyValuePair<string, TWC_DailyForecast> forecast in dailyForecasts)
            {
                List<string> segments = TWC_DailyForecasts.BuildSegments(forecast.Key, forecast.Value);
                if (segments.Count == 0)
                {
                    continue;
                }
                List<byte[]> packets = PacketEncoding.BuildMessage(segments);
                PacketSending.SendMulticast(packets, config.Network, priority: false);
            }

            List<string> radarFrames = await DataCollector.DownloadMapCutRange(i1Config, Path.Combine(AppContext.BaseDirectory, "MapTiles"), config.API, frames: 30);

            foreach (string framePath in radarFrames)
            {
                List<byte[]> packets = RadarImages.BuildPackets(framePath, i1Config.InstallName);
                PacketSending.SendMulticast(packets, config.Network, priority: false);
            }

            // Now delete the downloaded radar frames
            foreach (string framePath in radarFrames)
            {
                if (File.Exists(framePath))
                {
                    File.Delete(framePath);
                }
            }
        }
    }
}
