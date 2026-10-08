using System.Text.Json;

namespace Windsock
{
    public class PythonStringHelper
    {
        public static string PyValue<T>(T? value) => value is null ? "None" : JsonSerializer.Serialize(value);

        public static string PyBytes(string value) => "'" + string.Concat(System.Text.Encoding.UTF8.GetBytes(value).Select(item => "\\x" + item.ToString("x2"))) + "'";
    }
}
