namespace Windsock
{
    public class Packet
    {
        public required PacketHeader Header { get; set; }
        public required byte[] Payload { get; set; }
    }
}