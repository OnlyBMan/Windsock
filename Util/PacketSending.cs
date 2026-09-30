using System.Net;
using System.Net.Sockets;

namespace Windsock
{
    public class PacketSending
    {
        public static void SendMulticast(IEnumerable<byte[]> packets, NetworkConfig network, bool priority)
        {
            using var sender = new UdpClient(AddressFamily.InterNetwork);

            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.Parse(network.InterfaceAddress).GetAddressBytes());

            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, network.Ttl);

            var destination = new IPEndPoint(IPAddress.Parse(network.MulticastAddress), priority ? network.PriorityPort : network.RoutinePort);

            foreach (byte[] packet in packets)
            {
                sender.Send(packet, packet.Length, destination);
                System.Threading.Thread.Sleep(network.PacketDelayMs);
            }
        }
    }
}