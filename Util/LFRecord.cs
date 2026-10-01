using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace Windsock
{
    public static class LFRecord
    {
        public static List<LFRecordLocation> ParseLFRecord(string lfRecordPath, Dictionary<string, List<string>> interestLists)
        {
            List<string> coopIds = interestLists.GetValueOrDefault("coopId") ?? new List<string>();
            List<string> observationStations = interestLists.GetValueOrDefault("obsStation") ?? new List<string>();
            string cachedLocationsPath = Path.Combine(AppContext.BaseDirectory, "CachedLocations.json");
            List<LFRecordLocation> locations = new List<LFRecordLocation>();

            if (File.Exists(cachedLocationsPath))
            {
                string json = File.ReadAllText(cachedLocationsPath);
                locations = JsonSerializer.Deserialize<List<LFRecordLocation>>(json) ?? new List<LFRecordLocation>();
                locations = FilterLocations(locations);
            }

            if (locations.Count == 0)
            {
                SqliteConnectionStringBuilder connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = lfRecordPath,
                    Mode = SqliteOpenMode.ReadOnly
                };

                using SqliteConnection connection = new SqliteConnection(connectionString.ToString());
                connection.Open();

                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT key, compressed, xml FROM datarecords WHERE key GLOB '1_US_?*' ORDER BY key";

                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string key = reader.GetString(0);
                    bool compressed = reader.GetBoolean(1);
                    string xml = reader.GetString(2);
                    if (compressed)
                    {
                        xml = DecompressLFRecord(xml);
                    }

                    XElement? data = XDocument.Parse(xml).Root?.Element("LFData");
                    if (data == null)
                    {
                        throw new InvalidDataException("LFData is missing for record: " + key);
                    }

                    string? coopId = (string?)data.Element("coopId");
                    string? observationStation = (string?)data.Element("obsStn");
                    bool matchesCoop = coopId != null && (coopIds.Contains(coopId) || observationStations.Contains("T" + coopId));
                    bool matchesStation = observationStation != null && observationStations.Contains(observationStation);

                    if (!matchesCoop && !matchesStation)
                    {
                        continue;
                    }

                    LFRecordLocation location = new LFRecordLocation
                    {
                        Key = key,
                        CoopID = coopId,
                        ObservationStation = observationStation,
                        Latitude = (string?)data.Element("lat"),
                        Longitude = (string?)data.Element("long"),
                        ZipCode = (string?)data.Element("zip2locId"),
                        NWSC = (string?)data.Element("cntyId"),
                        NWSZ = (string?)data.Element("zoneId"),
                        NWSF = (string?)data.Element("cntyFips"),
                        GMTOffset = (string?)data.Element("gmtDiff")
                    };

                    locations.Add(location);
                }
            }

            locations = FilterLocations(locations);
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(cachedLocationsPath, JsonSerializer.Serialize(locations, jsonOptions));
            return locations;
        }

        private static List<LFRecordLocation> FilterLocations(List<LFRecordLocation> locations)
        {
            List<LFRecordLocation> filteredLocations = new List<LFRecordLocation>();
            HashSet<string> locationIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (LFRecordLocation location in locations.OrderBy(location => location.Key, StringComparer.Ordinal))
            {
                if (location.Key == null || !location.Key.StartsWith("1_US_", StringComparison.Ordinal) || location.Key.Length <= 5)
                {
                    continue;
                }

                string locationId = string.IsNullOrWhiteSpace(location.CoopID) ? location.Key : location.CoopID;
                if (!locationIds.Add(locationId))
                {
                    continue;
                }

                filteredLocations.Add(location);
            }

            return filteredLocations;
        }

        private static string DecompressLFRecord(string encodedXml)
        {
            byte[] compressedBytes = Convert.FromBase64String(encodedXml);
            if (compressedBytes.Length < 5)
            {
                throw new InvalidDataException("Compressed LFRecord data is too short.");
            }

            // The first four bytes are a length prefix; the remaining bytes are raw DEFLATE.
            using MemoryStream input = new MemoryStream(compressedBytes, 4, compressedBytes.Length - 4);
            using DeflateStream decompressor = new DeflateStream(input, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(decompressor, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
