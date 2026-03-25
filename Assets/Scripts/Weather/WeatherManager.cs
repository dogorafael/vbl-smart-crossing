// Controla a condição climática ativa e expõe o multiplicador de velocidade
// do jogador correspondente definido na especificação:
//   sunny      -> 1.0x
//   clouded    -> 0.8x
//   foggy      -> 0.8x
//   light rain -> 0.6x
//   heavy rain -> 0.4x

using UnityEngine;
using VBLSmartCrossing.Core;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Weather
{
    public class WeatherManager : MonoBehaviour
    {
        // - Properties -

        public WeatherCondition CurrentWeather { get; private set; }
        public float  CurrentSpeedMultiplier => GetMultiplier(CurrentWeather);

        // - Unity lifecycle -

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnStatusChanged += HandleStatusChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnStatusChanged -= HandleStatusChanged;
        }

        // - Event handlers -

        private void HandleApiDataLoaded(TrafficResponse response) =>
            ApplyWeather(response.current_status.weather);

        private void HandleStatusChanged(Status status) =>
            ApplyWeather(status.weather);

        // - Private helpers -

        private void ApplyWeather(string weather)
        {
            CurrentWeather = WeatherConditionParser.Parse(weather);
            Debug.Log($"[WeatherManager] Clima -> {weather}  (multiplicador: {CurrentSpeedMultiplier:F1}x)");
        }

        // - Static utility -

        /// <summary>
        /// Retorna o multiplicador de velocidade do jogador para uma determinada string de clima.
        /// Estático para que outros sistemas possam chamá-lo sem uma referência.
        /// </summary>
        public static float GetMultiplier(WeatherCondition weather) => weather switch
        {
            WeatherCondition.Sunny => 1.0f,
            WeatherCondition.Clouded => 0.8f,
            WeatherCondition.Foggy => 0.8f,
            WeatherCondition.LightRain => 0.6f,
            WeatherCondition.HeavyRain => 0.4f,
            _ => 1.0f
        };
    }
}
