using System.Text.Json;

namespace Windsock
{
    public static class I1Config
    {
        public static Dictionary<string, List<string>> Parse(string configPath, string configId = "1")
        {
            Dictionary<string, List<string>> interestLists = new Dictionary<string, List<string>>();

            foreach (string configLine in File.ReadAllLines(configPath))
            {
                string line = configLine.Trim();
                if (!line.StartsWith("wxdata.setInterestList("))
                {
                    continue;
                }
                
                // Separate the interest name and config ID from the list of values.
                int argumentsStart = line.IndexOf('(') + 1;
                int listStart = line.IndexOf('[');
                int listEnd = line.LastIndexOf(']');
                if (listStart < argumentsStart || listEnd < listStart)
                {
                    throw new FormatException("Invalid interest list: " + line);
                }

                string arguments = line.Substring(argumentsStart, listStart - argumentsStart);
                string[] argumentParts = arguments.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (argumentParts.Length != 2)
                {
                    throw new FormatException("Invalid interest list arguments: " + line);
                }

                string interestName = argumentParts[0].Trim().Trim('\'', '"');
                string interestConfigId = argumentParts[1].Trim().Trim('\'', '"');
                if (interestConfigId != configId)
                {
                    continue;
                }

                string listContents = line.Substring(listStart + 1, listEnd - listStart - 1);
                List<string> values = new List<string>();
                foreach (string listValue in listContents.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string value = listValue.Trim();
                    if (value.Length == 0)
                    {
                        continue;
                    }

                    value = value.Trim('\'', '"');
                    if (!values.Contains(value))
                    {
                        values.Add(value);
                    }
                }

                interestLists[interestName] = values;
            }

            List<Interest> interests = new List<Interest>();
            foreach (KeyValuePair<string, List<string>> interestList in interestLists)
            {
                interests.Add(new Interest
                {
                    Category = interestList.Key,
                    LocationIDs = interestList.Value
                });
            }

            string cachedInterestPath = Path.Combine(AppContext.BaseDirectory, "CachedInterest.json");
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(cachedInterestPath, JsonSerializer.Serialize(interests, jsonOptions));
            return interestLists;
        }
    }
}
