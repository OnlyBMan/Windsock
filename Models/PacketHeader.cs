public class PacketHeader
{
    public required byte Length { get; set; }
    public required ushort ProtocolVersion { get; set; }
    public required ushort CompletionAbortFlag { get; set; }
    public required ushort PayloadLength { get; set; }
    public required uint MessageID { get; set; }
    public required uint PacketNumber {get; set; }
    public required byte SegmentNumber {get; set; }
    public required byte Checksum {get; set; }
    public required byte Reserved {get; set; }
}