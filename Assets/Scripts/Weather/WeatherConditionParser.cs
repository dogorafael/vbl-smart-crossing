// Centraliza a conversão das strings de clima da API para o enum WeatherCondition
namespace VBLSmartCrossing.Weather
{    public static class WeatherConditionParser
    {
        public static WeatherCondition Parse(string value) => value switch
        {
            "sunny" => WeatherCondition.Sunny,
            "clouded" => WeatherCondition.Clouded,
            "foggy" => WeatherCondition.Foggy,
            "light rain" => WeatherCondition.LightRain,
            "heavy rain" => WeatherCondition.HeavyRain,
            _ => WeatherCondition.Unknown
        };
    }
}
