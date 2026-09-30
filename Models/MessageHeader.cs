public class MessageHeader
{
    public required byte Length { get; set; }
    public required byte SegmentCount { get; set; }
    public required byte MessageType { get; set; }
    public required byte Reserved { get; set; }
    public required uint Timeout { get; set; }
}