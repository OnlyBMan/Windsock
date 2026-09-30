using System.Linq.Expressions;

namespace Windsock
{
    public class PacketEncoding
    {
        public static byte[] BuildBodyPacket(byte[] payload, uint messageID, uint packetNum, byte segmentNum)
        {
            PacketHeader packetHeader = new PacketHeader
            {
                Length = 18,
                ProtocolVersion = 1,
                CompletionAbortFlag = 0,
                PayloadLength = checked((ushort)payload.Length),
                MessageID = messageID,
                PacketNumber = packetNum,
                SegmentNumber = segmentNum,
                Checksum = 0,
                Reserved = 0
            };

            byte[] bodyPacket = new byte[Constants.PACKET_HEADER_LENGTH + payload.Length];

            // Copy our initial packet header into our blank body packet
            packetHeader.Serialize().CopyTo(bodyPacket, 0);

            // Then copy our payload!
            payload.CopyTo(bodyPacket, Constants.PACKET_HEADER_LENGTH);

            return bodyPacket;
        }
    }
}