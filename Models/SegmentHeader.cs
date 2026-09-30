public class SegmentHeader
{
    public required byte Length { get; set; }
    public required uint PacketCount { get; set; }
    public required byte Encoding { get; set; }
    public required byte Compression { get; set; }
    public required byte Reserved { get; set; }
}