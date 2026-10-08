using System.Text.Json.Serialization;

namespace Windsock
{
    public class TWC_HourlyForecast
    {
        [JsonPropertyName("forecasts")]
        public List<TWC_HourlyForecastEntry>? Forecasts { get; set; }
    }

    public class TWC_HourlyForecastEntry
    {
        [JsonPropertyName("expire_time_gmt")]
        public long? ExpireTimeGmt { get; set; }

        [JsonPropertyName("fcst_valid")]
        public long? FcstValid { get; set; }

        [JsonPropertyName("temp")]
        public int? Temp { get; set; }

        [JsonPropertyName("wspd")]
        public int? Wspd { get; set; }

        [JsonPropertyName("wdir_cardinal")]
        public string? WdirCardinal { get; set; }

        [JsonPropertyName("icon_extd")]
        public int? IconExtd { get; set; }

        [JsonPropertyName("hi")]
        public int? Hi { get; set; }

        [JsonPropertyName("wc")]
        public int? Wc { get; set; }

        [JsonPropertyName("pop")]
        public int? Pop { get; set; }

        [JsonPropertyName("precip_type")]
        public string? PrecipType { get; set; }

        [JsonPropertyName("uv_index_raw")]
        public double? UvIndexRaw { get; set; }
    }
}
