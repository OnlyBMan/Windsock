using System.Text.RegularExpressions;

namespace Windsock
{
    public static class Bulletins
    {
        public static List<string> BuildSegments(string areaId, TWC_Alert alert)
        {
            TWC_Alert? detail = alert.Detail;
            long expiration = alert.ExpireTimeUTC is > 0 ? alert.ExpireTimeUTC.Value : alert.EndTimeUTC ?? 0;
            if (expiration > 100000000000)
            {
                expiration /= 1000;
            }
            if (detail == null || expiration <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return new List<string>();
            }

            string pil = (string.IsNullOrWhiteSpace(detail.ProductIdentifier) ? alert.ProductIdentifier ?? "" : detail.ProductIdentifier).ToUpperInvariant();
            string title = Regex.Replace(string.IsNullOrWhiteSpace(alert.EventDescription) ? detail.EventDescription ?? "" : alert.EventDescription, @"\s+", " ").Trim().ToLowerInvariant();
            if (!Regex.IsMatch(pil, "^[A-Z]{3}$") || title.Length == 0)
            {
                return new List<string>();
            }

            List<string> pils = new List<string> { pil };
            if ((pil == "TCV" || pil == "MWW") && title is "hurricane warning" or "hurricane watch" or "tropical storm warning" or "tropical storm watch")
            {
                pils.Add("HLS");
                pils.Add("NPW");
            }
            bool update = (string.IsNullOrWhiteSpace(alert.MessageType) ? detail.MessageType ?? "" : alert.MessageType).ToLowerInvariant() is "update" or "continue" or "correction";
            List<string> titles = update ? new List<string> { title + " update", title } : new List<string> { title, title + " update" };
            TWC_AlertText? textRecord = detail.Texts?.FirstOrDefault();
            string text = Regex.Replace(string.Join(" ", new[] { textRecord?.Overview, textRecord?.Description, textRecord?.Instruction }.Where(value => !string.IsNullOrWhiteSpace(value))), @"\s+", " ").Trim();
            if (text.Length == 0)
            {
                text = detail.SummaryHeadline ?? "";
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = string.IsNullOrWhiteSpace(alert.HeadlineText) ? alert.EventDescription ?? "" : alert.HeadlineText;
                }
                text = Regex.Replace(text, @"\s+", " ").Trim();
            }
            long issueTime = detail.ProcessTimeUTC is > 0 ? detail.ProcessTimeUTC.Value : alert.ProcessTimeUTC ?? 0;
            if (issueTime > 100000000000)
            {
                issueTime /= 1000;
            }
            string areaLiteral = PythonStringHelper.PyValue(areaId);

            return new List<string>
            {
                $"""
                import twccommon
                import twc.dsmarshal as dsm
                areaList = wxdata.getBulletinInterestList({areaLiteral})
                if not areaList:
                    abortMsg()

                """,
                $"""
                twccommon.Log.info("THIS IS WINDSOCK BULLETIN")
                areaList = wxdata.getBulletinInterestList({areaLiteral})
                keys = []
                for key in dsm.defaultedConfigGet('interestlist.pil') or []:
                    if isinstance(key, (tuple, list)) and len(key) == 2:
                        if not isinstance(key[0], str) or not isinstance(key[1], str):
                            continue
                        key = key[0] + key[1]
                    if isinstance(key, str) and len(key) == 6 and key[:3].isalpha() and key[3:].isdigit():
                        keys.append(key)
                keys.sort()
                bulletinKey = None
                for candidatePil in {PythonStringHelper.PyValue(pils)}:
                    for title in {PythonStringHelper.PyValue(titles)}:
                        for key in keys:
                            if key[:3] != candidatePil:
                                continue
                            info = dsm.defaultedConfigGet('pil.' + key)
                            headline = getattr(info, 'headline', '')
                            if isinstance(headline, str) and getattr(info, 'ldl', None) in (0, 1) and ' '.join(headline.split()).lower() == title:
                                bulletinKey = key
                                break
                        if bulletinKey:
                            break
                    if bulletinKey:
                        break
                if not bulletinKey:
                    twccommon.Log.info('No matching IntelliStar bulletin title')
                    abortMsg()
                txt = {PythonStringHelper.PyBytes(text)}
                for area in areaList:
                    b = twc.Data()
                    b.pil = bulletinKey[:3]
                    b.pilExt = bulletinKey[3:]
                    b.issueTime = {PythonStringHelper.PyValue(issueTime)}
                    b.dispExpiration = {PythonStringHelper.PyValue(expiration)}
                    b.group = ''
                    b.text = txt
                    exp = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setBulletin(area, b, exp)

                """
            };
        }
    }
}
