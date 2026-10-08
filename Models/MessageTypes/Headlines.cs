using System.Text.RegularExpressions;

namespace Windsock
{
    public static class Headlines
    {
        public static List<string> BuildSegments(string areaId, TWC_Alert alert)
        {
            long expiration = alert.ExpireTimeUTC is > 0 ? alert.ExpireTimeUTC.Value : alert.EndTimeUTC ?? 0;
            if (expiration > 100000000000)
            {
                expiration /= 1000;
            }
            if (expiration <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return new List<string>();
            }

            string areaLiteral = PythonStringHelper.PyValue(areaId);
            string interest = Regex.IsMatch(areaId, @"^[A-Z]{2}C\d{3}$") ? "county" : "zone";
            string lookup = $"areaList = wxdata.getUGCInterestList({areaLiteral}, '{interest}')\n";
            if (interest == "zone")
            {
                lookup += $"if not areaList:\n    areaList = wxdata.getUGCInterestList({areaLiteral}, 'zone.cwf')\n";
            }
            string headline = Regex.Replace(string.IsNullOrWhiteSpace(alert.HeadlineText) ? alert.EventDescription ?? "" : alert.HeadlineText, @"\s+", " ").Trim();
            if (headline.Length == 0)
            {
                return new List<string>();
            }

            return new List<string>
            {
                "import twccommon\n" + lookup + "if not areaList:\n    abortMsg()\n",
                lookup + $"""
                twccommon.Log.info("THIS IS WINDSOCK HEADLINE")
                headline = {PythonStringHelper.PyBytes(headline)}
                phenSig = {PythonStringHelper.PyValue((alert.Phenomena ?? "") + "_" + (alert.Significance ?? ""))}
                hdlnExp = {PythonStringHelper.PyValue(expiration)}
                for area in areaList:
                    d = twc.Data()
                    d.headline = headline
                    d.phenSig = phenSig
                    d.expiration = hdlnExp
                    wxdata.setHeadline(area, d, hdlnExp)

                """
            };
        }
    }
}
