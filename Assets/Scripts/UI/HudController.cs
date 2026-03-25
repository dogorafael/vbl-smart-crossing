// Exibe o painel da simulação conforme exigido pela especificação:
// Também gerencia a visibilidade dos painéis de Carregando / Nível Completo / Fim de Jogo.

using TMPro;
using UnityEngine;
using VBLSmartCrossing.Core;
using VBLSmartCrossing.Data;
using VBLSmartCrossing.Weather;

namespace VBLSmartCrossing.UI
{
    public class HudController : MonoBehaviour
    {
        // - Inspector -

        [Header("Level")]
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("API Data Panel")]
        [SerializeField] private TextMeshProUGUI _densityText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _weatherText;
        [SerializeField] private TextMeshProUGUI _spawnIntervalText;
        [SerializeField] private TextMeshProUGUI _weatherMultiplierText;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private float _urgencyThreshold = 8f; // abaixo desse valor o timer fica vermelho

        [Header("State Panels")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GameObject _levelCompletePanel;
        [SerializeField] private GameObject _gameOverPanel;

        // - Unity lifecycle -

        private void Start()
        {
            HideAllPanels();
            ShowPanel(_loadingPanel);
            SetLevel(1);
        }

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnStatusChanged += HandleStatusChanged;
            GameEvents.OnLevelChanged += SetLevel;
            GameEvents.OnTimerTick += UpdateTimer;
            GameEvents.OnLevelCompleted += ShowLevelComplete;
            GameEvents.OnGameOver += ShowGameOver;
            GameEvents.OnBeforeLevelLoad += HandleReset;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnStatusChanged -= HandleStatusChanged;
            GameEvents.OnLevelChanged -= SetLevel;
            GameEvents.OnTimerTick -= UpdateTimer;
            GameEvents.OnLevelCompleted -= ShowLevelComplete;
            GameEvents.OnGameOver -= ShowGameOver;
            GameEvents.OnBeforeLevelLoad -= HandleReset;
        }

        // - Event handlers -

        private void HandleApiDataLoaded(TrafficResponse data)
        {
            HideAllPanels();
            UpdateStatusPanel(data.current_status);
        }

        private void HandleStatusChanged(Status status) => UpdateStatusPanel(status);

        private void HandleReset()
        {
            HideAllPanels();
            ShowPanel(_loadingPanel);
        }

        // - Metodos para atualizar HUD  -

        private void SetLevel(int level)
        {
            if (_levelText != null)
                _levelText.text = $"Level {level}";
        }

        private void UpdateStatusPanel(Status status)
        {
            // formula: SpawnInterval = 1 / vehicleDensity
            float spawnInterval = 1f / Mathf.Max(status.vehicleDensity, 0.01f);
            float multiplier = WeatherManager.GetMultiplier(WeatherConditionParser.Parse(status.weather));

            SetText(_densityText, $"Density: {status.vehicleDensity:F2}");
            SetText(_speedText, $"Speed: {status.averageSpeed:F0} km/h");
            SetText(_weatherText, $"Weather: {status.weather}");
            SetText(_spawnIntervalText, $"Spawn interval: {spawnInterval:F1}s");
            SetText(_weatherMultiplierText, $"Move penalty: {multiplier:F1}x");
        }

        private void UpdateTimer(float remaining)
        {
            if (_timerText == null) return;

            int secs = Mathf.CeilToInt(remaining);
            _timerText.text  = $"Tempo: {secs}s";
            _timerText.color = remaining <= _urgencyThreshold ? Color.red : Color.orange;
        }

        // - Panel helpers -

        private void ShowLevelComplete()
        {
            HideAllPanels();
            ShowPanel(_levelCompletePanel);
        }

        private void ShowGameOver()
        {
            HideAllPanels();
            ShowPanel(_gameOverPanel);
        }

        private void HideAllPanels()
        {
            _loadingPanel?.SetActive(false);
            _levelCompletePanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
        }

        private static void ShowPanel(GameObject panel) => panel?.SetActive(true);

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
