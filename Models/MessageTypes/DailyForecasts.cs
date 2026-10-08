using System.Text.Json;

namespace Windsock
{
    public static class TWC_DailyForecasts
    {
        public static List<string> BuildSegments(string coopId, TWC_DailyForecast dailyForecast)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(coopId);
            ArgumentNullException.ThrowIfNull(dailyForecast);

            List<TWC_DailyForecastEntry> forecasts = dailyForecast.Forecasts ?? throw new InvalidDataException("The daily forecast response has no forecasts array.");
            string coopLiteral = PythonStringHelper.PyValue(coopId);
            long minimumExpiration = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 86400;
            List<string> segments = new List<string>
            {
                $"""
                import twccommon
                twccommon.Log.info("THIS IS WINDSOCK DAILY FORECAST")
                if not isInterested(coopId=[{coopLiteral}]):
                    abortMsg()

                """
            };

            foreach (TWC_DailyForecastEntry forecast in forecasts)
            {
                if (forecast.Day?.FcstValid is null && forecast.Night?.FcstValid is null)
                {
                    continue;
                }

                long expiration = Math.Max(forecast.ExpireTimeGmt ?? 0, minimumExpiration);
                string segment = "import twccommon\n";

                if (forecast.Day is { FcstValid: long dayValid } day)
                {
                    segment += $"""
                    twccommon.Log.info("THIS IS SUN UV FORECAST")
                    uv = twc.Data()
                    uv.index = {PythonStringHelper.PyValue(day.UvIndexRaw)}
                    vt = {PythonStringHelper.PyValue(dayValid)}
                    et = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setDaypartData({coopLiteral}, 'uvDailyFcst', uv, vt, 1, et)
                    d = twc.Data()
                    d.highTemp = {PythonStringHelper.PyValue(day.Temp)}
                    d.dayWindSpeed = {PythonStringHelper.PyValue(day.Wspd)}
                    d.dayWindDir = {(day.WdirCardinal is null ? "None" : WindDirectionMapping.ToOrdinal(day.WdirCardinal))}
                    d.dayChanceOfPrecip = {PythonStringHelper.PyValue(day.Pop)}
                    d.dayPrecipType = {PythonStringHelper.PyValue(day.PrecipType)}
                    d.dayRelHumidity = {PythonStringHelper.PyValue(day.Rh)}
                    d.dayCloudCover = {PythonStringHelper.PyValue(day.Clds)}
                    d.daySkyCondition = {PythonStringHelper.PyValue(day.IconExtd)}
                    d.heatIndex = {PythonStringHelper.PyValue(day.Hi)}
                    d.golfIndex = {PythonStringHelper.PyValue(day.GolfIndex)}
                    d.snowAccum = {PythonStringHelper.PyValue(forecast.SnowQpf)}
                    d.daySnowAccum = {PythonStringHelper.PyValue(day.SnowQpf)}
                    d.daySevWxQualifier = {PythonStringHelper.PyValue(day.QualifierCode)}
                    vt = {PythonStringHelper.PyValue(dayValid)}
                    et = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setDaypartData({coopLiteral}, 'dailyFcst', d, vt, 1, et, 1)
                    textFcst = twc.Data()
                    textFcst.phrase = {PythonStringHelper.PyValue(day.Narrative)}
                    textFcst.audioCode = {PythonStringHelper.PyValue(day.VocalKey)}
                    textFcst.shortCast = {PythonStringHelper.PyValue(day.Shortcast)}
                    textFcst.daypartName = {PythonStringHelper.PyValue(day.DaypartName)}
                    textFcst.tersePhrase = {PythonStringHelper.PyValue(day.Phrase22char)}
                    vt = {PythonStringHelper.PyValue(dayValid)}
                    et = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setDaypartData({coopLiteral}, 'textFcst', textFcst, vt, 2, et)
                    """ + "\n";
                }

                if (forecast.Night is { FcstValid: long nightValid } night)
                {
                    segment += $"""
                    d = twc.Data()
                    d.lowTemp = {PythonStringHelper.PyValue(night.Temp)}
                    d.eveningWindSpeed = {PythonStringHelper.PyValue(night.Wspd)}
                    d.eveningChanceOfPrecip = {PythonStringHelper.PyValue(night.Pop)}
                    d.eveningPrecipType = {PythonStringHelper.PyValue(night.PrecipType)}
                    d.eveningRelHumidity = {PythonStringHelper.PyValue(night.Rh)}
                    d.eveningCloudCover = {PythonStringHelper.PyValue(night.Clds)}
                    d.eveningWindDir = {(night.WdirCardinal is null ? "None" : WindDirectionMapping.ToOrdinal(night.WdirCardinal))}
                    d.eveningSkyCondition = {PythonStringHelper.PyValue(night.IconExtd)}
                    d.windChill = {PythonStringHelper.PyValue(night.Wc)}
                    d.eveningSevWxQualifier = {PythonStringHelper.PyValue(night.QualifierCode)}
                    vt = {PythonStringHelper.PyValue(nightValid)}
                    et = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setDaypartData({coopLiteral}, 'dailyFcst', d, vt, 1, et, 1)
                    textFcst = twc.Data()
                    textFcst.phrase = {PythonStringHelper.PyValue(night.Narrative)}
                    textFcst.audioCode = {PythonStringHelper.PyValue(night.VocalKey)}
                    textFcst.shortCast = {PythonStringHelper.PyValue(night.Shortcast)}
                    textFcst.daypartName = {PythonStringHelper.PyValue(night.DaypartName)}
                    textFcst.tersePhrase = {PythonStringHelper.PyValue(night.Phrase22char)}
                    vt = {PythonStringHelper.PyValue(nightValid)}
                    et = {PythonStringHelper.PyValue(expiration)}
                    wxdata.setDaypartData({coopLiteral}, 'textFcst', textFcst, vt, 2, et)
                    """ + "\n";
                }

                segments.Add(segment);
            }

            return segments.Count > 1 ? segments : new List<string>();
        }
    }
}
