public class Packet
{
    public required PacketHeader Header { get; set; }
    public required MessageHeader Message { get; set; }
    public required SegmentHeader Segment { get; set; }
}