using System.Text.Json.Serialization;

namespace Windsock
{
    public class TWC_DailyForecast
    {
        [JsonPropertyName("metadata")]
        public TWC_DailyForecastMetadata? Metadata { get; set; }

        [JsonPropertyName("forecasts")]
        public List<TWC_DailyForecastEntry>? Forecasts { get; set; }

    }

    public class TWC_DailyForecastMetadata
    {
        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("units")]
        public string? Units { get; set; }

        [JsonPropertyName("expire_time_gmt")]
        public long? ExpireTimeGmt { get; set; }

        [JsonPropertyName("status_code")]
        public int? StatusCode { get; set; }

    }

    public class TWC_DailyForecastEntry
    {
        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("expire_time_gmt")]
        public long? ExpireTimeGmt { get; set; }

        [JsonPropertyName("fcst_valid")]
        public long? FcstValid { get; set; }

        [JsonPropertyName("fcst_valid_local")]
        public string? FcstValidLocal { get; set; }

        [JsonPropertyName("num")]
        public int? Num { get; set; }

        [JsonPropertyName("max_temp")]
        public int? MaxTemp { get; set; }

        [JsonPropertyName("min_temp")]
        public int? MinTemp { get; set; }

        [JsonPropertyName("torcon")]
        public int? Torcon { get; set; }

        [JsonPropertyName("stormcon")]
        public int? Stormcon { get; set; }

        [JsonPropertyName("blurb")]
        public string? Blurb { get; set; }

        [JsonPropertyName("blurb_author")]
        public string? BlurbAuthor { get; set; }

        [JsonPropertyName("lunar_phase_day")]
        public int? LunarPhaseDay { get; set; }

        [JsonPropertyName("dow")]
        public string? Dow { get; set; }

        [JsonPropertyName("lunar_phase")]
        public string? LunarPhase { get; set; }

        [JsonPropertyName("lunar_phase_code")]
        public string? LunarPhaseCode { get; set; }

        [JsonPropertyName("sunrise")]
        public string? Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public string? Sunset { get; set; }

        [JsonPropertyName("moonrise")]
        public string? Moonrise { get; set; }

        [JsonPropertyName("moonset")]
        public string? Moonset { get; set; }

        [JsonPropertyName("qualifier_code")]
        public string? QualifierCode { get; set; }

        [JsonPropertyName("qualifier")]
        public string? Qualifier { get; set; }

        [JsonPropertyName("narrative")]
        public string? Narrative { get; set; }

        [JsonPropertyName("qpf")]
        public double? Qpf { get; set; }

        [JsonPropertyName("snow_qpf")]
        public double? SnowQpf { get; set; }

        [JsonPropertyName("snow_range")]
        public string? SnowRange { get; set; }

        [JsonPropertyName("snow_phrase")]
        public string? SnowPhrase { get; set; }

        [JsonPropertyName("snow_code")]
        public string? SnowCode { get; set; }

        [JsonPropertyName("night")]
        public TWC_DailyForecastDaypart? Night { get; set; }

        [JsonPropertyName("day")]
        public TWC_DailyForecastDaypart? Day { get; set; }

    }

    public class TWC_DailyForecastDaypart
    {
        [JsonPropertyName("fcst_valid")]
        public long? FcstValid { get; set; }

        [JsonPropertyName("fcst_valid_local")]
        public string? FcstValidLocal { get; set; }

        [JsonPropertyName("day_ind")]
        public string? DayInd { get; set; }

        [JsonPropertyName("thunder_enum")]
        public int? ThunderEnum { get; set; }

        [JsonPropertyName("daypart_name")]
        public string? DaypartName { get; set; }

        [JsonPropertyName("long_daypart_name")]
        public string? LongDaypartName { get; set; }

        [JsonPropertyName("alt_daypart_name")]
        public string? AltDaypartName { get; set; }

        [JsonPropertyName("thunder_enum_phrase")]
        public string? ThunderEnumPhrase { get; set; }

        [JsonPropertyName("num")]
        public int? Num { get; set; }

        [JsonPropertyName("temp")]
        public int? Temp { get; set; }

        [JsonPropertyName("hi")]
        public int? Hi { get; set; }

        [JsonPropertyName("wc")]
        public int? Wc { get; set; }

        [JsonPropertyName("pop")]
        public int? Pop { get; set; }

        [JsonPropertyName("icon_extd")]
        public int? IconExtd { get; set; }

        [JsonPropertyName("icon_code")]
        public int? IconCode { get; set; }

        [JsonPropertyName("wxman")]
        public string? Wxman { get; set; }

        [JsonPropertyName("phrase_12char")]
        public string? Phrase12char { get; set; }

        [JsonPropertyName("phrase_22char")]
        public string? Phrase22char { get; set; }

        [JsonPropertyName("phrase_32char")]
        public string? Phrase32char { get; set; }

        [JsonPropertyName("subphrase_pt1")]
        public string? SubphrasePt1 { get; set; }

        [JsonPropertyName("subphrase_pt2")]
        public string? SubphrasePt2 { get; set; }

        [JsonPropertyName("subphrase_pt3")]
        public string? SubphrasePt3 { get; set; }

        [JsonPropertyName("precip_type")]
        public string? PrecipType { get; set; }

        [JsonPropertyName("rh")]
        public int? Rh { get; set; }

        [JsonPropertyName("wspd")]
        public int? Wspd { get; set; }

        [JsonPropertyName("wdir")]
        public int? Wdir { get; set; }

        [JsonPropertyName("wdir_cardinal")]
        public string? WdirCardinal { get; set; }

        [JsonPropertyName("clds")]
        public int? Clds { get; set; }

        [JsonPropertyName("pop_phrase")]
        public string? PopPhrase { get; set; }

        [JsonPropertyName("temp_phrase")]
        public string? TempPhrase { get; set; }

        [JsonPropertyName("accumulation_phrase")]
        public string? AccumulationPhrase { get; set; }

        [JsonPropertyName("wind_phrase")]
        public string? WindPhrase { get; set; }

        [JsonPropertyName("shortcast")]
        public string? Shortcast { get; set; }

        [JsonPropertyName("narrative")]
        public string? Narrative { get; set; }

        [JsonPropertyName("qpf")]
        public double? Qpf { get; set; }

        [JsonPropertyName("snow_qpf")]
        public double? SnowQpf { get; set; }

        [JsonPropertyName("snow_range")]
        public string? SnowRange { get; set; }

        [JsonPropertyName("snow_phrase")]
        public string? SnowPhrase { get; set; }

        [JsonPropertyName("snow_code")]
        public string? SnowCode { get; set; }

        [JsonPropertyName("vocal_key")]
        public string? VocalKey { get; set; }

        [JsonPropertyName("qualifier_code")]
        public string? QualifierCode { get; set; }

        [JsonPropertyName("qualifier")]
        public string? Qualifier { get; set; }

        [JsonPropertyName("uv_index_raw")]
        public double? UvIndexRaw { get; set; }

        [JsonPropertyName("uv_index")]
        public int? UvIndex { get; set; }

        [JsonPropertyName("uv_warning")]
        public int? UvWarning { get; set; }

        [JsonPropertyName("uv_desc")]
        public string? UvDesc { get; set; }

        [JsonPropertyName("golf_index")]
        public int? GolfIndex { get; set; }

        [JsonPropertyName("golf_category")]
        public string? GolfCategory { get; set; }

    }

}
