using System.Buffers.Binary;

namespace Windsock
{
    public class MessageHeader
    {
        public required byte Length { get; set; }
        public required byte SegmentCount { get; set; }
        public required byte MessageType { get; set; }
        public required byte Reserved { get; set; }
        public required uint Timeout { get; set; }

        public byte[] Serialize()
        {
            byte[] outputBytes = new byte[8];
            outputBytes[0] = this.Length;
            outputBytes[1] = this.SegmentCount;
            outputBytes[2] = this.MessageType;
            outputBytes[3] = this.Reserved;
            BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(4, 4), this.Timeout);
            return outputBytes;
        }
    }
}