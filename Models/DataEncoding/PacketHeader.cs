using System.Buffers.Binary;

namespace Windsock
{
    public class PacketHeader
    {
        public required byte Length { get; set; }
        public required ushort ProtocolVersion { get; set; }
        public required ushort CompletionAbortFlag { get; set; }
        public required ushort PayloadLength { get; set; }
        public required uint MessageID { get; set; }
        public required uint PacketNumber { get; set; }
        public required byte SegmentNumber { get; set; }
        public required byte Checksum { get; set; }
        public required byte Reserved { get; set; }

        public byte[] Serialize()
        {
            byte[] outputBytes = new byte[18];
            outputBytes[0] = this.Length;

            BinaryPrimitives.WriteUInt16BigEndian(outputBytes.AsSpan(1, 2), this.ProtocolVersion);
            BinaryPrimitives.WriteUInt16BigEndian(outputBytes.AsSpan(3, 2), this.CompletionAbortFlag);
            BinaryPrimitives.WriteUInt16BigEndian(outputBytes.AsSpan(5, 2), this.PayloadLength);
            BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(7, 4), this.MessageID);
            BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(11, 4), this.PacketNumber);
            outputBytes[15] = this.SegmentNumber;
            outputBytes[16] = this.Checksum;
            outputBytes[17] = this.Reserved;
            return outputBytes;
        }
    }
}