using System.Globalization;
using System.Text.Json;

namespace Windsock
{
    public static class CurrentConditions
    {
        public static List<string> BuildSegments(string stationId, TWC_CurrentObservation observation)
        {
            List<string> segments = new List<string>();
            long expiration = observation.ValidTimeUtc + 1800;
            string stationLiteral = JsonSerializer.Serialize(stationId);

            segments.Add($"""
            import twccommon
            twccommon.Log.info("THIS IS WINDSOCK CURRENT OBSERVATIONS")
            if not isInterested(obsStation=[{stationLiteral}]):
                abortMsg()

            """);

            segments.Add(FormattableString.Invariant($"""
            d = twc.Data()
            d.temp = {observation.Temperature}
            d.altimeter = {observation.PressureAltimeter}
            d.pressure = {observation.PressureMeanSeaLevel}
            d.dewpoint = {observation.TemperatureDewPoint}
            d.heatIndex = {observation.TemperatureHeatIndex}
            d.humidity = {observation.RelativeHumidity}
            d.gusts = {observation.WindGust?.ToString(CultureInfo.InvariantCulture) ?? "None"}
            d.windSpeed = {observation.WindSpeed}
            d.windChill = {observation.TemperatureWindChill}
            d.ceiling = {observation.CloudCeiling?.ToString(CultureInfo.InvariantCulture) ?? "None"}
            d.skyCondition = {observation.IconCodeExtend}
            d.pressureTendency = {observation.PressureTendencyCode}
            d.visibility = {observation.Visibility}
            d.windDirection = {WindDirectionMapping.ToOrdinal(observation.WindDirectionCardinal)}
            d.feelsLikeIndex = {observation.TemperatureFeelsLike}
            d.uvIndex = {observation.UvIndex}
            et = {expiration}
            wxdata.setData({stationLiteral}, 'obs', d, et)
            d = twc.Data()
            d.tempMax = {observation.TemperatureMax24Hour}
            d.tempMin = {observation.TemperatureMin24Hour}
            et = {expiration}
            wxdata.setData({stationLiteral}, 'recObs', d, et)

            """));

            return segments;
        }
    }
}
