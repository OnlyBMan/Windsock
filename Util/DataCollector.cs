using System.Text.Json;

namespace Windsock
{
    public static class DataCollector
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

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