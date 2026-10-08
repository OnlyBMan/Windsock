namespace Windsock
{
    public static class TWC_HourlyForecasts
    {
        public static List<string> BuildSegments(string coopId, TWC_HourlyForecast hourlyForecast)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(coopId);
            ArgumentNullException.ThrowIfNull(hourlyForecast);

            List<TWC_HourlyForecastEntry> forecasts = hourlyForecast.Forecasts ?? throw new InvalidDataException("The hourly forecast response has no forecasts array.");
            string coopLiteral = PythonStringHelper.PyValue(coopId);
            long minimumExpiration = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 86400;
            List<string> segments = new List<string>
            {
                $"""
                import twccommon
                twccommon.Log.info("THIS IS WINDSOCK HOURLY FORECAST")
                if not isInterested(coopId=[{coopLiteral}]):
                    abortMsg()

                """
            };

            foreach (TWC_HourlyForecastEntry forecast in forecasts)
            {
                if (forecast.FcstValid is not long valid)
                {
                    continue;
                }

                long expiration = Math.Max(forecast.ExpireTimeGmt is > 0 ? forecast.ExpireTimeGmt.Value : valid + 5400, minimumExpiration);
                segments.Add($"""
                d = twc.Data()
                d.temp = {PythonStringHelper.PyValue(forecast.Temp)}
                d.windSpeed = {PythonStringHelper.PyValue(forecast.Wspd)}
                d.windDir = {(forecast.WdirCardinal is null ? "None" : WindDirectionMapping.ToOrdinal(forecast.WdirCardinal))}
                d.skyCondition = {PythonStringHelper.PyValue(forecast.IconExtd)}
                d.heatIndex = {PythonStringHelper.PyValue(forecast.Hi)}
                d.windChill = {PythonStringHelper.PyValue(forecast.Wc)}
                d.chanceOfPrecip = {PythonStringHelper.PyValue(forecast.Pop)}
                d.precipType = {PythonStringHelper.PyValue(forecast.PrecipType)}
                et = {PythonStringHelper.PyValue(expiration)}
                vt = {PythonStringHelper.PyValue(valid)}
                wxdata.setDaypartData({coopLiteral}, 'hourlyFcst', d, vt, 24, et)
                uv = twc.Data()
                uv.index = {PythonStringHelper.PyValue(forecast.UvIndexRaw)}
                wxdata.setDaypartData({coopLiteral}, 'uvHourlyFcst', uv, vt, 24, et)

                """);
            }

            return segments.Count > 1 ? segments : new List<string>();
        }
    }
}
