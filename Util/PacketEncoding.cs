using System.Linq.Expressions;

namespace Windsock
{
    public class PacketEncoding
    {
        private static long unixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static byte[] BuildIntroductionPacket(uint messageID, byte segmentNum, byte segmentCount, uint packetCount)
        {
            SegmentHeader segmentHeader = new SegmentHeader
            {
                Length = Constants.SEGMENT_HEADER_LENGTH,
                PacketCount = packetCount,
                Encoding = 0,
                Compression = 0,
                Reserved = 0
            };

            MessageHeader? messageHeader = null;

            if (segmentNum == 0)
            {
                messageHeader = new MessageHeader
                {
                    Length = Constants.MESSAGE_HEADER_LENGTH,
                    SegmentCount = segmentCount,
                    MessageType = 1,
                    Reserved = 0,
                    Timeout = 300
                };
            }

            int payloadLength = Constants.SEGMENT_HEADER_LENGTH + (messageHeader == null ? 0 : Constants.MESSAGE_HEADER_LENGTH);

            PacketHeader packetHeader = new PacketHeader
            {
                PacketNumber = 0,
                Length = Constants.PACKET_HEADER_LENGTH,
                ProtocolVersion = 1,
                CompletionAbortFlag = 0,
                PayloadLength = (ushort)payloadLength,
                MessageID = messageID,
                SegmentNumber = segmentNum,
                Checksum = 0,
                Reserved = 0
            };

            byte[] payloadBytes = new byte[payloadLength];

            if (messageHeader != null)
            {
                messageHeader.Serialize().CopyTo(payloadBytes, 0);
                segmentHeader.Serialize().CopyTo(payloadBytes, Constants.MESSAGE_HEADER_LENGTH);
            }
            else
            {
                segmentHeader.Serialize().CopyTo(payloadBytes, 0);
            }

            Packet returnPacket = new Packet
            {
                Header = packetHeader,
                Payload = payloadBytes
            };

            return returnPacket.Serialize();
        }

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

        public static List<byte[]> BuildMessage(List<string> segments)
        {
            uint messageID = NextMessageId();
            List<byte[]> returnPackets = new List<byte[]>();

            foreach (var segment in segments)
            {
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(segment);

                List<byte[]> chunks = new List<byte[]>();
                if (payload.Length == 0)
                {
                    chunks.Add(Array.Empty<byte>());
                }

                for (int i = 0; i < payload.Length / Constants.MAX_PACKET_SIZE; i++)
                {
                    chunks.Add(payload.AsSpan(i * Constants.MAX_PACKET_SIZE, Constants.MAX_PACKET_SIZE).ToArray());
                }

                if (payload.Length % Constants.MAX_PACKET_SIZE > 0)
                {
                    int remainder = payload.Length % Constants.MAX_PACKET_SIZE;
                    chunks.Add(payload.AsSpan(payload.Length - remainder, remainder).ToArray());
                }

                returnPackets.Add(
                    PacketEncoding.BuildIntroductionPacket(
                        messageID,
                        segmentNum: (byte)segments.IndexOf(segment),
                        segmentCount: (byte)segments.Count,
                        packetCount: checked((uint)chunks.Count + 1)));

                for (int i = 0; i < chunks.Count; i++)
                {
                    returnPackets.Add(PacketEncoding.BuildBodyPacket(
                        chunks[i],
                        messageID,
                        packetNum: checked((uint)(i + 1)),
                        segmentNum: (byte)segments.IndexOf(segment)));
                }
            }
            return returnPackets;
        }

        static uint NextMessageId()
        {
            return unchecked((uint)System.Threading.Interlocked.Increment(ref unixMilliseconds));
        }
    }
}
