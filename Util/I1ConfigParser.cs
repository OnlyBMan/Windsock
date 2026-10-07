using System.Text.Json;
using System.Text.RegularExpressions;
using TreeSitter;

namespace Windsock
{
    public static class I1ConfigParser
    {
        public static I1Config Parse(string configPath, string? configId = null)
        {
            string source = File.ReadAllText(configPath);
            using var language = new Language("Python");
            using var parser = new Parser(language);
            using var tree = parser.Parse(source);

            if (tree == null)
            {
                throw new FormatException("Unable to parse I1 config.");
            }

            configId ??= FindConfigId(tree.RootNode);
            I1Config config = new()
            {
                Interests = ParseInterests(tree.RootNode, language, configId),
                Maps = ParseMaps(tree.RootNode, configId)
            };

            foreach (Node statement in tree.RootNode.NamedChildren)
            {
                Node? function = statement.GetChildForField("function");
                if (statement.Type != "call" || function?.GetChildForField("object")?.Text != "dsm" || function.GetChildForField("attribute")?.Text != "set")
                {
                    continue;
                }

                IReadOnlyList<Node>? arguments = statement.GetChildForField("arguments")?.NamedChildren;
                if (arguments == null || arguments.Count < 2)
                {
                    continue;
                }

                string name = ReadPythonString(arguments[0].Text).ToLowerInvariant();
                if (name == "scmt_configtype")
                {
                    config.InstallName = ReadPythonString(arguments[1].Text);
                }
            }

            List<Interest> interests = new();
            foreach (var interest in config.Interests)
            {
                interests.Add(new Interest
                {
                    Category = interest.Key,
                    LocationIDs = interest.Value
                });
            }

            JsonSerializerOptions options = new() { WriteIndented = true };
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "CachedInterest.json"), JsonSerializer.Serialize(interests, options));
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "CachedMaps.json"), JsonSerializer.Serialize(config.Maps.Values, options));

            return config;
        }

        public static Dictionary<string, I1Map> ParseMaps(string configPath, string? configId = null)
        {
            string source = File.ReadAllText(configPath);
            using var language = new Language("Python");
            using var parser = new Parser(language);
            using var tree = parser.Parse(source);

            if (tree == null)
            {
                throw new FormatException("Unable to parse I1 config.");
            }

            return ParseMaps(tree.RootNode, configId ?? FindConfigId(tree.RootNode));
        }

        private static string FindConfigId(Node root)
        {
            foreach (Node statement in root.NamedChildren)
            {
                Node? function = statement.GetChildForField("function");
                if (statement.Type != "call" || function?.GetChildForField("object")?.Text != "wxdata")
                {
                    continue;
                }

                IReadOnlyList<Node>? arguments = statement.GetChildForField("arguments")?.NamedChildren;
                string? method = function.GetChildForField("attribute")?.Text;
                if (method == "setMapData" && arguments?.Count >= 2)
                {
                    string[] key = ReadPythonString(arguments[0].Text).Split('.', 3);
                    if (key.Length == 3 && key[0] == "Config")
                    {
                        return key[1];
                    }
                }
                else if (method == "setInterestList" && arguments?.Count == 3)
                {
                    return ReadPythonString(arguments[1].Text);
                }
            }

            return "1";
        }

        private static Dictionary<string, List<string>> ParseInterests(Node root, Language language, string configId)
        {
            Dictionary<string, List<string>> interests = new(StringComparer.OrdinalIgnoreCase);
            using var query = new Query(language,
                """
                (
                    (call
                        function: (attribute
                            object: (identifier) @object
                            attribute: (identifier) @method
                        )
                        arguments: (argument_list) @arguments
                    )
                    (#eq? @object "wxdata")
                    (#eq? @method "setInterestList")
                )
                """);

            foreach (QueryMatch match in query.Execute(root).Matches)
            {
                Node argumentList = match.Captures.First(capture => capture.Name == "arguments").Node;
                IReadOnlyList<Node> arguments = argumentList.NamedChildren;
                if (arguments.Count != 2 && arguments.Count != 3)
                {
                    continue;
                }

                // The middle argument is an optional config ID.
                if (arguments.Count == 3)
                {
                    string interestConfigId = ReadPythonString(arguments[1].Text);
                    if (!string.Equals(interestConfigId, configId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                string name = ReadPythonString(arguments[0].Text);
                interests[name] = ReadListValues(arguments[^1]).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            return interests;
        }

        private static Dictionary<string, I1Map> ParseMaps(Node root, string configId)
        {
            Dictionary<string, I1Map> maps = new(StringComparer.OrdinalIgnoreCase);
            string configPrefix = $"Config.{configId}.";
            I1Map? pendingMap = null;

            foreach (Node statement in root.NamedChildren)
            {
                if (statement.Type == "assignment" && statement.GetChildForField("left")?.Text == "d")
                {
                    pendingMap = null;
                    Node? value = statement.GetChildForField("right");
                    Node? function = value?.GetChildForField("function");
                    if (value?.Type != "call" || function?.Type != "attribute" || function.GetChildForField("object")?.Text != "twc" || function.GetChildForField("attribute")?.Text != "Data")
                    {
                        continue;
                    }

                    Node? argumentList = value.GetChildForField("arguments");
                    if (argumentList == null)
                    {
                        continue;
                    }

                    I1Map map = new();
                    foreach (Node argument in argumentList.NamedChildren)
                    {
                        if (argument.Type != "keyword_argument")
                        {
                            continue;
                        }

                        Node? name = argument.GetChildForField("name");
                        Node? fieldValue = argument.GetChildForField("value");
                        if (name == null || fieldValue == null)
                        {
                            continue;
                        }

                        switch (name.Text.ToLowerInvariant())
                        {
                            case "mapname":
                                map.MapName = ReadPythonString(fieldValue.Text);
                                break;
                            case "mapcutcoordinate":
                                map.MapcutCoordinate = ReadIntTuple(fieldValue);
                                break;
                            case "mapcutsize":
                                map.MapcutSize = ReadIntTuple(fieldValue);
                                break;
                            case "mapfinalsize":
                                map.MapFinalSize = ReadIntTuple(fieldValue);
                                break;
                            case "mapmilessize":
                                map.MapMilesSize = ReadIntTuple(fieldValue);
                                break;
                            case "datacuttype":
                                map.DatacutType = ReadPythonString(fieldValue.Text);
                                break;
                            case "datacutcoordinate":
                                map.DatacutCoordinate = ReadIntTuple(fieldValue);
                                break;
                            case "datacutsize":
                                map.DatacutSize = ReadIntTuple(fieldValue);
                                break;
                            case "datafinalsize":
                                map.DataFinalSize = ReadIntTuple(fieldValue);
                                break;
                            case "dataoffset":
                                map.DataOffset = ReadIntTuple(fieldValue);
                                break;
                            case "vectors":
                                map.Vectors = ReadListValues(fieldValue);
                                break;
                        }
                    }

                    if (map.MapName != null)
                    {
                        pendingMap = map;
                    }
                    continue;
                }

                if (pendingMap == null || statement.Type != "call")
                {
                    continue;
                }

                Node? mapFunction = statement.GetChildForField("function");
                if (mapFunction?.Type != "attribute" || mapFunction.GetChildForField("object")?.Text != "wxdata" || mapFunction.GetChildForField("attribute")?.Text != "setMapData")
                {
                    continue;
                }

                Node? mapArguments = statement.GetChildForField("arguments");
                IReadOnlyList<Node>? arguments = mapArguments?.NamedChildren;
                if (arguments == null || arguments.Count < 2 || arguments[1].Text.Trim() != "d")
                {
                    continue;
                }

                string configKey = ReadPythonString(arguments[0].Text);
                if (!configKey.StartsWith(configPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                pendingMap.Name = configKey[configPrefix.Length..];
                pendingMap.ConfigKey = configKey;
                maps[pendingMap.Name] = pendingMap;
                pendingMap = null;
            }

            return maps;
        }

        private static int[]? ReadIntTuple(Node node)
        {
            MatchCollection matches = Regex.Matches(node.Text, @"-?\d+");
            if (matches.Count == 0)
            {
                return null;
            }

            return matches.Select(match => int.Parse(match.Value)).ToArray();
        }

        private static List<string> ReadListValues(Node node)
        {
            List<string> values = new();
            foreach (Node item in node.NamedChildren)
            {
                string value;
                if (item.Type == "string")
                {
                    value = ReadPythonString(item.Text);
                }
                else
                {
                    value = item.Text.Trim();
                }

                if (value.Length > 0)
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private static string ReadPythonString(string value)
        {
            value = value.Trim();
            int quoteIndex = value.IndexOfAny(new[] { '\'', '"' });
            if (quoteIndex < 0 || value.Length <= quoteIndex + 1 || value[^1] != value[quoteIndex])
            {
                return value;
            }

            string contents = value[(quoteIndex + 1)..^1];

            return contents
                .Replace("\\\\", "\u0001")
                .Replace("\\'", "'")
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\u0001", "\\");
        }
    }
}
