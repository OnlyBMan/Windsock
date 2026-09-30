using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace Windsock
{
    public static class LFRecord
    {
        public static bool LFRecordExists()
        {
            string lfRecordPath = Path.Combine(AppContext.BaseDirectory, "LFRecord");
            return File.Exists(lfRecordPath);
        }
    }
}
