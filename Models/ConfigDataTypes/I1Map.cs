namespace Windsock
{
    public class I1Map
    {
        public string Name { get; set; } = "";
        public string ConfigKey { get; set; } = "";

        public string? MapName { get; set; }

        public int[]? MapcutCoordinate { get; set; }
        public int[]? MapcutSize { get; set; }
        public int[]? MapFinalSize { get; set; }
        public int[]? MapMilesSize { get; set; }

        public string? DatacutType { get; set; }
        public int[]? DatacutCoordinate { get; set; }
        public int[]? DatacutSize { get; set; }
        public int[]? DataFinalSize { get; set; }
        public int[]? DataOffset { get; set; }

        public List<string> Vectors { get; set; } = new();
    }
}