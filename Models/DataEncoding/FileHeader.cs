using System.Buffers.Binary;

namespace Windsock
{
    public class FileHeader
    {
        public required uint FileLength { get; set; }
        public required ushort DestinationLength { get; set; }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[Constants.FILE_HEADER_LENGTH];
            bytes[0] = Constants.FILE_HEADER_LENGTH;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(1, 4), FileLength);
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(5, 2), DestinationLength);
            return bytes;
        }
    }
}
