namespace Windsock
{
    public class Config
    {
        public NetworkConfig Network { get; set; } = new();
    }

    public class NetworkConfig
    {
        public string InterfaceAddress { get; set; } = "";
        public string MulticastAddress { get; set; } = "224.1.1.77";
        public int RoutinePort { get; set; } = 7777;
        public int PriorityPort { get; set; } = 7778;
        public int Ttl { get; set; } = 2;
        public int PacketDelayMs { get; set; } = 10;
    }
}