namespace Windsock
{
    public static class PacketEncoding
    {
        private static long unixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static byte[] BuildIntroductionPacket(uint messageID, byte segmentNum, byte segmentCount, uint packetCount, byte messageType = 1)
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
                    MessageType = messageType,
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
                Length = Constants.PACKET_HEADER_LENGTH,
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
            List<byte[]> packets = new List<byte[]>();
            for (int index = 0; index < segments.Count; index++)
            {
                byte[] payload = System.Text.Encoding.UTF8.GetBytes(segments[index]);
                AddSegment(packets, payload, messageID, (byte)index, (byte)segments.Count, 1);
            }
            return packets;
        }

        public static List<byte[]> BuildFileMessage(List<string> commands, byte[] fileData, string destination, string storeCommand)
        {
            if (commands.Count != 2 || fileData.Length == 0 || !destination.StartsWith("/"))
            {
                throw new ArgumentException("A file message needs two commands, file data, and an absolute i1 destination.");
            }

            byte[] destinationBytes = System.Text.Encoding.UTF8.GetBytes(destination);
            byte[] storeBytes = System.Text.Encoding.UTF8.GetBytes(storeCommand);
            if (destinationBytes.Length > Constants.MAX_PACKET_SIZE - 16 || storeBytes.Length > Constants.MAX_PACKET_SIZE)
            {
                throw new ArgumentException("The destination or store command exceeds one i1 packet.");
            }

            uint messageID = NextMessageId();
            List<byte[]> packets = new List<byte[]>();
            for (int index = 0; index < commands.Count; index++)
            {
                AddSegment(packets, System.Text.Encoding.UTF8.GetBytes(commands[index]), messageID, (byte)index, 4, 2);
            }

            int chunkCount = (fileData.Length - 1) / Constants.MAX_PACKET_SIZE + 1;
            SegmentHeader segmentHeader = new SegmentHeader
            {
                Length = Constants.SEGMENT_HEADER_LENGTH,
                PacketCount = (uint)chunkCount + 1,
                Encoding = 0,
                Compression = 0,
                Reserved = 0
            };
            FileHeader fileHeader = new FileHeader
            {
                FileLength = (uint)fileData.Length,
                DestinationLength = (ushort)destinationBytes.Length
            };
            byte[] introduction = new byte[Constants.SEGMENT_HEADER_LENGTH + Constants.FILE_HEADER_LENGTH + destinationBytes.Length];
            segmentHeader.Serialize().CopyTo(introduction, 0);
            fileHeader.Serialize().CopyTo(introduction, Constants.SEGMENT_HEADER_LENGTH);
            destinationBytes.CopyTo(introduction, Constants.SEGMENT_HEADER_LENGTH + Constants.FILE_HEADER_LENGTH);
            packets.Add(BuildBodyPacket(introduction, messageID, 0, 2));
            for (int index = 0; index < chunkCount; index++)
            {
                int offset = index * Constants.MAX_PACKET_SIZE;
                int length = Math.Min(Constants.MAX_PACKET_SIZE, fileData.Length - offset);
                packets.Add(BuildBodyPacket(fileData.AsSpan(offset, length).ToArray(), messageID, (uint)index + 1, 2));
            }

            AddSegment(packets, storeBytes, messageID, 3, 4, 2);
            return packets;
        }

        private static void AddSegment(List<byte[]> packets, byte[] payload, uint messageID, byte segmentNumber, byte segmentCount, byte messageType)
        {
            int chunkCount = payload.Length == 0 ? 1 : (payload.Length - 1) / Constants.MAX_PACKET_SIZE + 1;
            packets.Add(BuildIntroductionPacket(messageID, segmentNumber, segmentCount, (uint)chunkCount + 1, messageType));
            for (int index = 0; index < chunkCount; index++)
            {
                int offset = index * Constants.MAX_PACKET_SIZE;
                int length = Math.Min(Constants.MAX_PACKET_SIZE, payload.Length - offset);
                packets.Add(BuildBodyPacket(payload.AsSpan(offset, length).ToArray(), messageID, (uint)index + 1, segmentNumber));
            }
        }

        private static uint NextMessageId()
        {
            return unchecked((uint)System.Threading.Interlocked.Increment(ref unixMilliseconds));
        }
    }
}
