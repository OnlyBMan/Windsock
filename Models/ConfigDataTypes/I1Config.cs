namespace Windsock
{
    public class I1Config
    {
        public Dictionary<string, List<string>> Interests { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, I1Map> Maps { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
