namespace Windsock
{
    class Program
    {
        private static long? lastRadarFrameTimestampSent;

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

            using CancellationTokenSource shutdown = new CancellationTokenSource();
            ConsoleCancelEventHandler cancelHandler = (_, e) =>
            {
                e.Cancel = true;
                shutdown.Cancel();
            };
            Console.CancelKeyPress += cancelHandler;

            try
            {
                await Task.WhenAll(
                    RunScheduled("Current conditions", TimeSpan.FromMinutes(config.Timing.CurrentConditions), () => CollectCurrentConditions(config, interests, locations), shutdown.Token),
                    RunScheduled("Daily forecast", TimeSpan.FromMinutes(config.Timing.DailyForecast), () => CollectDailyForecast(config, interests, locations), shutdown.Token),
                    RunScheduled("Radar", TimeSpan.FromMinutes(config.Timing.Radar), () => CollectRadar(config, i1Config), shutdown.Token));
            }
            finally
            {
                Console.CancelKeyPress -= cancelHandler;
            }
        }

        private static async Task RunScheduled(string name, TimeSpan interval, Func<Task> collect, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{name}: running now and every {interval.TotalMinutes} minutes. Press Ctrl+C to stop.");
            using PeriodicTimer timer = new PeriodicTimer(interval);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await collect();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"{name} failed: {ex.Message}. Will retry on the next interval.");
                    }

                    if (!await timer.WaitForNextTickAsync(cancellationToken))
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Allow an active collection to finish, then stop scheduling.
            }
        }

        private static async Task CollectCurrentConditions(Config config, Dictionary<string, List<string>> interests, List<LFRecordLocation> locations)
        {
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
        }

        private static async Task CollectDailyForecast(Config config, Dictionary<string, List<string>> interests, List<LFRecordLocation> locations)
        {
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
        }

        private static async Task CollectRadar(Config config, I1Config i1Config)
        {
            List<string> radarFrames = await DataCollector.DownloadMapCutRange(i1Config, Path.Combine(AppContext.BaseDirectory, "MapTiles"), config.API, frames: 30, afterTimestamp: lastRadarFrameTimestampSent);

            try
            {
                foreach (string framePath in radarFrames)
                {
                    long timestamp = long.Parse(Path.GetFileName(framePath).Split('.')[0], System.Globalization.CultureInfo.InvariantCulture);
                    if (lastRadarFrameTimestampSent.HasValue && timestamp <= lastRadarFrameTimestampSent.Value)
                    {
                        continue;
                    }

                    List<byte[]> packets = RadarImages.BuildPackets(framePath, i1Config.InstallName);
                    if (!PacketSending.SendMulticast(packets, config.Network, priority: false))
                    {
                        break;
                    }
                    lastRadarFrameTimestampSent = timestamp;
                }
            }
            finally
            {
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
}
