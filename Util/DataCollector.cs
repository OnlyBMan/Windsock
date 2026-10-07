using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using ImageMagick;

namespace Windsock
{
    public static class DataCollector
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        public static async Task<Dictionary<string, TWC_DailyForecast>> CollectDailyForecasts(List<LFRecordLocation> locations, List<string> coopIds, ApiConfig api)
        {
            string? apiKey = api.TwcForecastsKey;
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your_api_key_here")
            {
                throw new InvalidOperationException("You didn't set the TWC Forecasts API key.");
            }

            Dictionary<string, TWC_DailyForecast> forecasts = new Dictionary<string, TWC_DailyForecast>();
            foreach (string coopId in coopIds.Distinct())
            {
                LFRecordLocation? location = locations.Find(item => item.CoopID == coopId);
                if (location == null || !double.TryParse(location.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) || !double.TryParse(location.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) || latitude is < -90 or > 90 || longitude is < -180 or > 180)
                {
                    Console.WriteLine("No valid coordinates found for forecast location: " + coopId);
                    continue;
                }

                string url = "https://api.weather.com/v1/geocode/" + latitude.ToString(CultureInfo.InvariantCulture) + "/" + longitude.ToString(CultureInfo.InvariantCulture) + "/forecast/daily/7day.json?units=e&language=en-US&apiKey=" + Uri.EscapeDataString(apiKey);

                try
                {
                    using HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Daily forecast failed for " + coopId + ": HTTP " + (int)response.StatusCode);
                        continue;
                    }

                    using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (!json.RootElement.TryGetProperty("forecasts", out JsonElement entries) || entries.ValueKind != JsonValueKind.Array)
                    {
                        throw new JsonException("The response does not contain daily forecasts.");
                    }

                    TWC_DailyForecast forecast = json.RootElement.Deserialize<TWC_DailyForecast>() ?? throw new JsonException("The daily forecast response is empty.");
                    if (forecast.Forecasts?.Count > 0)
                    {
                        // Remove the "F" after degrees in the temperature narratives
                        string tempNarrativePattern = @"(?<=\d)F\b";
                        foreach (var dailyForecast in forecast.Forecasts)
                        {
                            if (!string.IsNullOrWhiteSpace(dailyForecast.Day?.Narrative))
                            {
                                dailyForecast.Day.Narrative = Regex.Replace(dailyForecast.Day.Narrative, tempNarrativePattern, "");
                            }
                            if (!string.IsNullOrWhiteSpace(dailyForecast.Night?.Narrative))
                            {
                                dailyForecast.Night.Narrative = Regex.Replace(dailyForecast.Night.Narrative, tempNarrativePattern, "");
                            }
                        }
                        forecasts[coopId] = forecast;
                    }
                }
                catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException || exception is JsonException)
                {
                    Console.WriteLine("Could not collect daily forecast for " + coopId + " (" + exception.GetType().Name + ").");
                }
            }

            return forecasts;
        }

        public static async Task<Dictionary<string, TWC_CurrentObservation>> CollectCurrentConditions(List<LFRecordLocation> locations, List<string> stationIds, ApiConfig api)
        {
            string? apiKey = api.TwcForecastsKey;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your_api_key_here")
            {
                throw new InvalidOperationException("You didn't set the TWC Forecasts API key.");
            }

            Dictionary<string, TWC_CurrentObservation> observations = new Dictionary<string, TWC_CurrentObservation>();

            foreach (string stationId in stationIds.Distinct())
            {
                LFRecordLocation? location = locations.Find(location => location.ObservationStation == stationId || (location.CoopID != null && "T" + location.CoopID == stationId));
                if (location == null || string.IsNullOrWhiteSpace(location.Latitude) || string.IsNullOrWhiteSpace(location.Longitude))
                {
                    Console.WriteLine("No coordinates found for observation station: " + stationId);
                    continue;
                }

                string geocode = Uri.EscapeDataString(location.Latitude + "," + location.Longitude);
                string url = "https://api.weather.com/v3/wx/observations/current?geocode=" + geocode + "&units=e&language=en-US&format=json&apiKey=" + Uri.EscapeDataString(apiKey);

                try
                {
                    using HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Current conditions failed for " + stationId + ": HTTP " + (int)response.StatusCode);
                        continue;
                    }

                    using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (!json.RootElement.TryGetProperty("temperature", out _) || !json.RootElement.TryGetProperty("validTimeUtc", out _))
                    {
                        throw new JsonException("The response does not contain current conditions? How did that happen?");
                    }

                    TWC_CurrentObservation observation = json.RootElement.Deserialize<TWC_CurrentObservation>() ?? throw new JsonException("The current conditions response is empty.");
                    observations[stationId] = observation;
                }
                catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException || exception is JsonException)
                {
                    Console.WriteLine("Could not collect current conditions for " + stationId + " (" + exception.GetType().Name + ").");
                }
            }

            return observations;
        }

        public static async Task<TWC_RadarTimeStamp> CollectRadarTimeStamps(ApiConfig api)
        {

            string? apiKey = api.TwcRadarKey;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your_api_key_here")
            {
                throw new InvalidOperationException("You didn't set the TWC Radar API key.");
            }

            TWC_RadarTimeStamp radarTimeStamp = new TWC_RadarTimeStamp();

            try
            {
                string url = $"https://api.weather.com/v3/TileServer/series/product/twcRadarMosaic?apiKey={apiKey}";
                using HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Radar time stamps collection failed: HTTP " + (int)response.StatusCode);
                    throw new HttpRequestException("Radar time stamps collection failed: HTTP " + (int)response.StatusCode);
                }

                using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (!json.RootElement.TryGetProperty("series", out _))
                {
                    throw new JsonException("The response does not contain radar time stamps? Check key?");
                }

                radarTimeStamp = json.RootElement.Deserialize<TWC_RadarTimeStamp>() ?? throw new JsonException("The radar time stamps response is empty.");
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException || exception is JsonException)
            {
                Console.WriteLine("Could not collect radar time stamps (" + exception.GetType().Name + ").");
            }


            return radarTimeStamp;
        }

        public static async Task<List<string>> DownloadMapCutRange(I1Config config, string destinationPath, ApiConfig api, int frames = 0)
        {
            List<I1Map> cuts = new List<I1Map>();
            foreach (I1Map map in config.Maps.Values)
            {
                if (map.DatacutType != "radar.us")
                {
                    continue;
                }
                if (map.DatacutCoordinate is not { Length: >= 2 } || map.DatacutSize is not { Length: >= 2 }
                    || map.MapName != "mercator.us.bfg"
                    || map.MapcutCoordinate is not { Length: >= 2 } || map.MapcutSize is not { Length: >= 2 }
                    || map.DatacutCoordinate[0] < 0 || map.DatacutCoordinate[1] < 0
                    || map.DatacutSize[0] <= 0 || map.DatacutSize[1] <= 0
                    || map.MapcutSize[0] <= 0 || map.MapcutSize[1] <= 0)
                {
                    throw new InvalidDataException("Invalid radar cut: " + map.Name);
                }
                cuts.Add(map);
            }
            if (cuts.Count == 0)
            {
                throw new InvalidDataException("The i1 config does not contain any radar.us data cuts.");
            }

            int left = cuts.Min(cut => cut.DatacutCoordinate![0]);
            int lower = cuts.Min(cut => cut.DatacutCoordinate![1]);
            int right = cuts.Max(cut => cut.DatacutCoordinate![0] + cut.DatacutSize![0]);
            int upper = cuts.Max(cut => cut.DatacutCoordinate![1] + cut.DatacutSize![1]);
            int canvasWidth = Math.Max(3400, right + 64);
            int canvasHeight = Math.Max(1600, upper + 64);

            TWC_RadarTimeStamp radar = await CollectRadarTimeStamps(api);
            if (radar.Series.Count == 0)
            {
                throw new InvalidOperationException("No radar frames were returned.");
            }

            const int zoom = 7;

            MapCutRange range = MapUtils.ConvertMercatorRange(cuts);
            int minX = range.minX;
            int maxX = range.maxX;
            int minY = range.minY;
            int maxY = range.maxY;
            int mosaicWidth = (maxX - minX + 1) * 256;
            int mosaicHeight = (maxY - minY + 1) * 256;

            double scaleX = (right - left) / (range.Right - range.Left);
            double scaleY = (upper - lower) / (range.Bottom - range.Top);
            int imageWidth = (int)Math.Ceiling(mosaicWidth * scaleX);
            int imageHeight = (int)Math.Ceiling(mosaicHeight * scaleY);
            int imageX = (int)Math.Round(left + (minX * 256 - range.Left) * scaleX);
            // i1 cuts use a bottom-left origin; TIFF images use top-left.
            int imageY = (int)Math.Round(canvasHeight - upper + (minY * 256 - range.Top) * scaleY);

            int frameCount = frames > 0 ? Math.Min(frames, radar.Series.Count) : radar.Series.Count;
            List<long> timestamps = radar.Series.Take(frameCount).Select(frame => frame.Timestamp).Reverse().ToList();
            long expiration = timestamps.Max() + 43200;
            string apiKey = Uri.EscapeDataString(api.TwcRadarKey);
            Directory.CreateDirectory(destinationPath);
            List<string> framePaths = new List<string>();

            MagickColor[] palette =
            {
                new MagickColor("#000000"), new MagickColor("#40CC55"), new MagickColor("#009900"),
                new MagickColor("#006600"), new MagickColor("#BFCC55"), new MagickColor("#BF9900"),
                new MagickColor("#FF3300"), new MagickColor("#BF3300"), new MagickColor("#800000"),
                new MagickColor("#400000")
            };

            foreach (long timestamp in timestamps)
            {
                using MagickImage mosaic = new MagickImage(MagickColors.Transparent, (uint)mosaicWidth, (uint)mosaicHeight);
                bool complete = true;
                for (int y = minY; y <= maxY && complete; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        string tileUrl = $"https://api.weather.com/v3/TileServer/tile/twcRadarMosaic?ts={timestamp}&xyz={x}:{y}:{zoom}&apiKey={apiKey}";
                        try
                        {
                            byte[] tileData = await client.GetByteArrayAsync(tileUrl);
                            using MagickImage tile = new MagickImage(tileData, MagickFormat.Png);
                            // TWC returns a 1x1 transparent PNG for an empty radar tile.
                            if (tile.Width == 1 && tile.Height == 1)
                            {
                                tile.Resize(256, 256, FilterType.Point);
                            }
                            else if (tile.Width != 256 || tile.Height != 256)
                            {
                                throw new InvalidDataException("Radar tiles must be 256 by 256 pixels.");
                            }
                            mosaic.Composite(tile, (x - minX) * 256, (y - minY) * 256, CompositeOperator.Copy);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Skipping radar frame {timestamp}: tile ({x}, {y}) failed ({ex.GetType().Name}).");
                            complete = false;
                            break;
                        }
                    }
                }
                if (!complete)
                {
                    continue;
                }

                mosaic.Resize(new MagickGeometry((uint)imageWidth, (uint)imageHeight) { IgnoreAspectRatio = true }, FilterType.Triangle);
                using IMagickImage<byte> mask = mosaic.Clone();
                mask.Alpha(AlphaOption.Extract);
                mask.Threshold(new Percentage(63 * 100.0 / 255));
                mosaic.Alpha(AlphaOption.Off);
                mosaic.Remap(palette, new QuantizeSettings { DitherMethod = DitherMethod.No });
                mosaic.Composite(mask, CompositeOperator.Multiply);

                using MagickImage canvas = new MagickImage(MagickColors.Black, (uint)canvasWidth, (uint)canvasHeight);
                canvas.Composite(mosaic, imageX, imageY, CompositeOperator.Copy);
                canvas.Alpha(AlphaOption.Off);
                canvas.ColorType = ColorType.TrueColor;
                canvas.Depth = 8;
                canvas.Settings.ColorType = ColorType.TrueColor;
                canvas.Settings.Compression = CompressionMethod.RLE;
                string framePath = Path.Combine(destinationPath, $"{timestamp}.{expiration}.tif");
                await canvas.WriteAsync(framePath, MagickFormat.Tiff);
                framePaths.Add(framePath);
                Console.WriteLine("Saved radar frame: " + Path.GetFileName(framePath));
            }

            return framePaths;
        }
    }
}
