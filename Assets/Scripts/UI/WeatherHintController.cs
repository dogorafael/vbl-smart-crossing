//Alterna os gameObjects de dica do clima

using UnityEngine;
using VBLSmartCrossing.Core;
using VBLSmartCrossing.Data;
using VBLSmartCrossing.Weather;

namespace VBLSmartCrossing.UI
{
    public class WeatherHintController : MonoBehaviour
    {
        [Header("Weather Hint Icons")]
        [SerializeField] private GameObject _sunny;
        [SerializeField] private GameObject _clouded;
        [SerializeField] private GameObject _foggy;
        [SerializeField] private GameObject _lightRain;
        [SerializeField] private GameObject _heavyRain;

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

        private void HandleApiDataLoaded(TrafficResponse response) =>
            ApplyWeather(response.current_status.weather);

        private void HandleStatusChanged(Status status) =>
            ApplyWeather(status.weather);

        private void ApplyWeather(string weather)
        {
            var condition = WeatherConditionParser.Parse(weather);

            _sunny?.SetActive(condition == WeatherCondition.Sunny);
            _clouded?.SetActive(condition == WeatherCondition.Clouded);
            _foggy?.SetActive(condition == WeatherCondition.Foggy);
            _lightRain?.SetActive(condition == WeatherCondition.LightRain);
            _heavyRain?.SetActive(condition == WeatherCondition.HeavyRain);
        }
    }
}