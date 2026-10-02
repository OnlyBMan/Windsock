namespace Windsock
{
    public static class CurrentConditions
    {
        public static List<string> BuildSegments(string stationId, TWC_CurrentObservation observation)
        {
            List<string> segments = new List<string>();
            long expiration = observation.ValidTimeUtc + 1800;
            string stationLiteral = PythonStringHelper.PyValue(stationId);

            segments.Add($"""
            import twccommon
            twccommon.Log.info("THIS IS WINDSOCK CURRENT OBSERVATIONS")
            if not isInterested(obsStation=[{stationLiteral}]):
                abortMsg()

            """);

            segments.Add($"""
            d = twc.Data()
            d.temp = {PythonStringHelper.PyValue(observation.Temperature)}
            d.altimeter = {PythonStringHelper.PyValue(observation.PressureAltimeter)}
            d.pressure = {PythonStringHelper.PyValue(observation.PressureMeanSeaLevel)}
            d.dewpoint = {PythonStringHelper.PyValue(observation.TemperatureDewPoint)}
            d.heatIndex = {PythonStringHelper.PyValue(observation.TemperatureHeatIndex)}
            d.humidity = {PythonStringHelper.PyValue(observation.RelativeHumidity)}
            d.gusts = {PythonStringHelper.PyValue(observation.WindGust)}
            d.windSpeed = {PythonStringHelper.PyValue(observation.WindSpeed)}
            d.windChill = {PythonStringHelper.PyValue(observation.TemperatureWindChill)}
            d.ceiling = {PythonStringHelper.PyValue(observation.CloudCeiling)}
            d.skyCondition = {PythonStringHelper.PyValue(observation.IconCodeExtend)}
            d.pressureTendency = {PythonStringHelper.PyValue(observation.PressureTendencyCode)}
            d.visibility = {PythonStringHelper.PyValue(observation.Visibility)}
            d.windDirection = {WindDirectionMapping.ToOrdinal(observation.WindDirectionCardinal)}
            d.feelsLikeIndex = {PythonStringHelper.PyValue(observation.TemperatureFeelsLike)}
            d.uvIndex = {PythonStringHelper.PyValue(observation.UvIndex)}
            et = {PythonStringHelper.PyValue(expiration)}
            wxdata.setData({stationLiteral}, 'obs', d, et)
            d = twc.Data()
            d.tempMax = {PythonStringHelper.PyValue(observation.TemperatureMax24Hour)}
            d.tempMin = {PythonStringHelper.PyValue(observation.TemperatureMin24Hour)}
            et = {PythonStringHelper.PyValue(expiration)}
            wxdata.setData({stationLiteral}, 'recObs', d, et)

            """);

            return segments;
        }
    }
}
