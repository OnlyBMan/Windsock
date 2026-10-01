namespace Windsock
{
    public static class WindDirectionMapping
    {
        private static readonly Dictionary<string, int> CARDINAL_TO_I1 = new Dictionary<string, int>
        {
            { "NNE", 1 },
            { "NE", 2 },
            { "ENE", 3 },
            { "E", 4 },
            { "ESE", 5 },
            { "SE", 6 },
            { "SSE", 7 },
            { "S", 8 },
            { "SSW", 9 },
            { "SW", 10 },
            { "WSW", 11 },
            { "W", 12 },
            { "WNW", 13 },
            { "NW", 14 },
            { "NNW", 15 },
            { "N", 16 }
        };

        public static string ToOrdinal(string cardinal)
        {
            if (CARDINAL_TO_I1.TryGetValue(cardinal, out int value))
            {
                return value.ToString();
            }
            return "0"; // Default value if the cardinal direction is not found
        }
    }
}