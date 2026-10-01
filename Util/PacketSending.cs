using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Windsock
{
    public static class PacketSending
    {
        public static void SendMulticast(IEnumerable<byte[]> packets, NetworkConfig network, bool priority)
        {
            IPAddress interfaceAddress = IPAddress.Parse(network.InterfaceAddress);
            bool interfaceExists = NetworkInterface.GetAllNetworkInterfaces().Any(networkInterface =>
                networkInterface.GetIPProperties().UnicastAddresses.Any(address => address.Address.Equals(interfaceAddress)));
            if (!interfaceExists)
            {
                Console.WriteLine("Cannot send packets: " + network.InterfaceAddress + " is not assigned to this computer. Update network.interfaceAddress in config.json or connect the VM network adapter.");
                return;
            }

            using UdpClient sender = new UdpClient(AddressFamily.InterNetwork);

            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, interfaceAddress.GetAddressBytes());

            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, network.Ttl);

            IPEndPoint destination = new IPEndPoint(IPAddress.Parse(network.MulticastAddress), priority ? network.PriorityPort : network.RoutinePort);

            foreach (byte[] packet in packets)
            {
                sender.Send(packet, packet.Length, destination);
                System.Threading.Thread.Sleep(network.PacketDelayMs);
            }
        }
    }
}
