using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;

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
    }
}
