using System.Text.Json;

namespace Windsock
{
    public class PythonStringHelper
    {
        public static string PyValue<T>(T? value) => value is null ? "None" : JsonSerializer.Serialize(value);
    }
}