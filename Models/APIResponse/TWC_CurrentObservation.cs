using System.Text.Json.Serialization;

namespace Windsock
{
    public class TWC_CurrentObservation
    {
        [JsonPropertyName("cloudCeiling")]
        public double? CloudCeiling { get; set; }

        [JsonPropertyName("cloudCover")]
        public double CloudCover { get; set; }

        [JsonPropertyName("cloudCoverPhrase")]
        public string CloudCoverPhrase { get; set; } = string.Empty;

        [JsonPropertyName("dayOfWeek")]
        public string DayOfWeek { get; set; } = string.Empty;

        [JsonPropertyName("dayOrNight")]
        public string DayOrNight { get; set; } = string.Empty;

        [JsonPropertyName("expirationTimeUtc")]
        public long ExpirationTimeUtc { get; set; }

        [JsonPropertyName("iconCode")]
        public int IconCode { get; set; }

        [JsonPropertyName("iconCodeExtend")]
        public int IconCodeExtend { get; set; }

        [JsonPropertyName("obsQualifierCode")]
        public string? ObsQualifierCode { get; set; }

        [JsonPropertyName("obsQualifierSeverity")]
        public int? ObsQualifierSeverity { get; set; }

        [JsonPropertyName("precip1Hour")]
        public double? Precip1Hour { get; set; }

        [JsonPropertyName("precip6Hour")]
        public double? Precip6Hour { get; set; }

        [JsonPropertyName("precip24Hour")]
        public double? Precip24Hour { get; set; }

        [JsonPropertyName("pressureAltimeter")]
        public double PressureAltimeter { get; set; }

        [JsonPropertyName("pressureChange")]
        public double PressureChange { get; set; }

        [JsonPropertyName("pressureMeanSeaLevel")]
        public double PressureMeanSeaLevel { get; set; }

        [JsonPropertyName("pressureTendencyCode")]
        public int PressureTendencyCode { get; set; }

        [JsonPropertyName("pressureTendencyTrend")]
        public string PressureTendencyTrend { get; set; } = string.Empty;

        [JsonPropertyName("relativeHumidity")]
        public double RelativeHumidity { get; set; }

        [JsonPropertyName("snow1Hour")]
        public double? Snow1Hour { get; set; }

        [JsonPropertyName("snow6Hour")]
        public double? Snow6Hour { get; set; }

        [JsonPropertyName("snow24Hour")]
        public double? Snow24Hour { get; set; }

        [JsonPropertyName("sunriseTimeLocal")]
        public string? SunriseTimeLocal { get; set; }

        [JsonPropertyName("sunriseTimeUtc")]
        public long? SunriseTimeUtc { get; set; }

        [JsonPropertyName("sunsetTimeLocal")]
        public string? SunsetTimeLocal { get; set; }

        [JsonPropertyName("sunsetTimeUtc")]
        public long? SunsetTimeUtc { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("temperatureChange24Hour")]
        public double TemperatureChange24Hour { get; set; }

        [JsonPropertyName("temperatureDewPoint")]
        public double TemperatureDewPoint { get; set; }

        [JsonPropertyName("temperatureFeelsLike")]
        public double TemperatureFeelsLike { get; set; }

        [JsonPropertyName("temperatureHeatIndex")]
        public double TemperatureHeatIndex { get; set; }

        [JsonPropertyName("temperatureMax24Hour")]
        public double TemperatureMax24Hour { get; set; }

        [JsonPropertyName("temperatureMaxSince7Am")]
        public double TemperatureMaxSince7Am { get; set; }

        [JsonPropertyName("temperatureMin24Hour")]
        public double TemperatureMin24Hour { get; set; }

        [JsonPropertyName("temperatureWetBulbGlobe")]
        public double TemperatureWetBulbGlobe { get; set; }

        [JsonPropertyName("temperatureWindChill")]
        public double TemperatureWindChill { get; set; }

        [JsonPropertyName("uvDescription")]
        public string UvDescription { get; set; } = string.Empty;

        [JsonPropertyName("uvIndex")]
        public int UvIndex { get; set; }

        [JsonPropertyName("validTimeLocal")]
        public string ValidTimeLocal { get; set; } = string.Empty;

        [JsonPropertyName("validTimeUtc")]
        public long ValidTimeUtc { get; set; }

        [JsonPropertyName("visibility")]
        public double Visibility { get; set; }

        [JsonPropertyName("windDirection")]
        public int WindDirection { get; set; }

        [JsonPropertyName("windDirectionCardinal")]
        public string WindDirectionCardinal { get; set; } = string.Empty;

        [JsonPropertyName("windGust")]
        public double? WindGust { get; set; }

        [JsonPropertyName("windSpeed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wxPhraseLong")]
        public string WxPhraseLong { get; set; } = string.Empty;

        [JsonPropertyName("wxPhraseMedium")]
        public string? WxPhraseMedium { get; set; }

        [JsonPropertyName("wxPhraseShort")]
        public string? WxPhraseShort { get; set; }
    }
}
