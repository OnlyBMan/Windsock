using System.Buffers.Binary;

namespace Windsock
{
    public class SegmentHeader
    {
        public required byte Length { get; set; }
        public required uint PacketCount { get; set; }
        public required byte Encoding { get; set; }
        public required byte Compression { get; set; }
        public required byte Reserved { get; set; }

        public byte[] Serialize()
        {
            byte[] outputBytes = new byte[8];
            outputBytes[0] = this.Length;
            BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(1, 4), this.PacketCount);
            outputBytes[5] = this.Encoding;
            outputBytes[6] = this.Compression;
            outputBytes[7] = this.Reserved;
            return outputBytes;
        }
    }
}