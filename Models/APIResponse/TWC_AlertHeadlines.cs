using System.Text.Json.Serialization;

namespace Windsock
{
    public class TWC_AlertHeadlines
    {
        [JsonPropertyName("metadata")]
        public TWC_AlertMetadata? Metadata { get; set; }

        [JsonPropertyName("alerts")]
        public List<TWC_Alert>? Alerts { get; set; }
    }

    public class TWC_AlertMetadata
    {
        [JsonPropertyName("next")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long? Next { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class TWC_Alert
    {
        [JsonPropertyName("areaId")]
        public string? AreaId { get; set; }

        [JsonPropertyName("detailKey")]
        public string? DetailKey { get; set; }

        [JsonPropertyName("headlineText")]
        public string? HeadlineText { get; set; }

        [JsonPropertyName("summaryHeadline")]
        public string? SummaryHeadline { get; set; }

        [JsonPropertyName("eventDescription")]
        public string? EventDescription { get; set; }

        [JsonPropertyName("messageType")]
        public string? MessageType { get; set; }

        [JsonPropertyName("productIdentifier")]
        public string? ProductIdentifier { get; set; }

        [JsonPropertyName("officeCode")]
        public string? OfficeCode { get; set; }

        [JsonPropertyName("phenomena")]
        public string? Phenomena { get; set; }

        [JsonPropertyName("significance")]
        public string? Significance { get; set; }

        [JsonPropertyName("severityCode")]
        public int? SeverityCode { get; set; }

        [JsonPropertyName("expireTimeUTC")]
        public long? ExpireTimeUTC { get; set; }

        [JsonPropertyName("endTimeUTC")]
        public long? EndTimeUTC { get; set; }

        [JsonPropertyName("processTimeUTC")]
        public long? ProcessTimeUTC { get; set; }

        [JsonPropertyName("texts")]
        public List<TWC_AlertText>? Texts { get; set; }

        [JsonIgnore]
        public string Product { get; set; } = "alerts";

        [JsonIgnore]
        public TWC_Alert? Detail { get; set; }
    }

    public class TWC_AlertText
    {
        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("instruction")]
        public string? Instruction { get; set; }
    }
}
