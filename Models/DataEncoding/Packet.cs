namespace Windsock
{
    public class Packet
    {
        public required PacketHeader Header { get; set; }
        public required byte[] Payload { get; set; }

        public byte[] Serialize()
        {
            byte[] serializedPacket = new byte[Header.Length + Payload.Length];
            Header.Serialize().CopyTo(serializedPacket, 0);
            Payload.CopyTo(serializedPacket, Header.Length);
            return serializedPacket;
        }
    }
}