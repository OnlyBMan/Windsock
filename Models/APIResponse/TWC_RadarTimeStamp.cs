using System.Text.Json.Serialization;

namespace Windsock
{
    public class TWC_RadarTimeStamp
    {
        [JsonPropertyName("nativeZoom")]
        public int NativeZoom { get; set; }

        [JsonPropertyName("maxZoom")]
        public int MaxZoom { get; set; }

        [JsonPropertyName("bb")]
        public TWC_RadarBoundingBox? BoundingBox { get; set; }

        [JsonPropertyName("series")]
        public List<TWC_RadarTimeStampEntry> Series { get; set; } = new List<TWC_RadarTimeStampEntry>();
    }

    public class TWC_RadarBoundingBox
    {
        [JsonPropertyName("tl")]
        public TWC_RadarCoordinate? TopLeft { get; set; }

        [JsonPropertyName("br")]
        public TWC_RadarCoordinate? BottomRight { get; set; }
    }

    public class TWC_RadarCoordinate
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; set; } = string.Empty;

        [JsonPropertyName("lng")]
        public string Longitude { get; set; } = string.Empty;
    }

    public class TWC_RadarTimeStampEntry
    {
        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }
    }
}
