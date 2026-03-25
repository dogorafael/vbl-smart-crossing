// Gerencia o nível atual e as chamadas à API.
// Ao vencer um nível, avança e carrega o próximo cenário da API.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VBLSmartCrossing.Data;
using VBLSmartCrossing.Services;

namespace VBLSmartCrossing.Core
{
    public class LevelManager : MonoBehaviour
    {
        // - Properties -
        public int CurrentLevel { get; private set; } = 1;       
        private ApiService _apiService;
        //Fallback (caso a chamada a API falhe)
        private List<TrafficResponse> _fallbackScenarios;        

        // - Unity lifecycle -

        private void Awake()
        {
            _apiService = new ApiService();           
        }

        private void OnEnable()
        {
            GameEvents.OnRequestStartGame += HandleStartGame;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnRequestStartGame -= HandleStartGame;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }
                       
        private async Task LoadInitialLevelAsync()
        {            
            Debug.Log("[LevelManager] Carregando dados do level inicial...");
            CurrentLevel = 1;
            GameEvents.RaiseLevelChanged(CurrentLevel);
            await FetchAndPublishAsync();
        }

        // - Event handlers -

        private void HandleStartGame()
        {
            _ = LoadInitialLevelAsync();
        }

        private void HandleLevelCompleted()
        {
            CurrentLevel++;
            Debug.Log($"[LevelManager] Level complete! Advancing to level {CurrentLevel}.");
            GameEvents.RaiseLevelChanged(CurrentLevel);
            _ = LoadNextLevelAsync(); // Descarta a Task, execução continua sem aguardar e evita warning
        }

        // - Private helpers -

        private async Task LoadNextLevelAsync()
        {            
            // Pausa breve para que o texto "Nível Completo" fique visível antes do reset
            await Task.Delay(1800);
            GameEvents.RaiseBeforeLevelLoad();
            
            // Pequeno delay adicional para o sistema terminar de resetar
            await Task.Delay(300);
            await FetchAndPublishAsync();
        }

        private async Task FetchAndPublishAsync()
        {
            var data = await _apiService.FetchTrafficStatusAsync();

            if (data == null)
            {                
                Debug.LogWarning("[LevelManager] API indisponível —> carregando JSON local.");

                // Carrega sob demanda, só uma vez
                if (_fallbackScenarios == null)
                    LoadFallbackScenarios();

                data = GetNextFallbackScenario();
            }

            if (data == null)
            {                
                Debug.LogError("[LevelManager] JSON local também falhou ao carregar, obtendo dados hardcoded.");
                data = BuildFallbackData();
            }

            GameEvents.RaiseApiDataLoaded(data);
        }

        /// <summary>
        /// Carrega os dados do arquivo JSON local, como se fosse os dados offline caso a chamada a API falhe
        /// </summary>
        private void LoadFallbackScenarios()
        {
            var asset = Resources.Load<TextAsset>("MockData/vbl-traffic-fallback");
            if (asset == null)
            {                
                Debug.LogError("[LevelManager] JSON local não encontrado em Resources/MockData/");
                return;
            }

            // Deserializa o array raiz do JSON
            _fallbackScenarios = JsonConvert.DeserializeObject<List<TrafficResponse>>(asset.text);
            Debug.Log($"[LevelManager] JSON local carregou : {_fallbackScenarios.Count} cenário(s).");
        }

        /// <summary>
        /// Pega os dados gerados pelo JSON local de forma ciclica.
        /// </summary>        
        private TrafficResponse GetNextFallbackScenario()
        {
            if (_fallbackScenarios == null || _fallbackScenarios.Count == 0)
                return null;

            // CurrentLevel começa em 1, por isso o -1 para indexar o array
            int index = (CurrentLevel - 1) % _fallbackScenarios.Count; //Ciclico como o modo SEQUENTIAL do mockoon
            return _fallbackScenarios[index];
        }


        /// <summary>
        /// Em caso de tanto a API quanto o JSON local falharem em carregar os dados esse método gera um único TrafficResponse fixo.
        /// Dessa forma a aplicação sempre vai conseguir rodar.
        /// </summary>
        private static TrafficResponse BuildFallbackData()
        {
            return new TrafficResponse
            {
                current_status = new Status
                {
                    vehicleDensity = 0.3f,
                    averageSpeed   = 40f,
                    weather        = "sunny"
                },
                predicted_status = new List<PredictedStatus>
                {
                    new() { estimated_time = 10000, predictions = new Status { vehicleDensity = 0.5f, averageSpeed = 55f, weather = "clouded" } },
                    new() { estimated_time = 20000, predictions = new Status { vehicleDensity = 0.7f, averageSpeed = 70f, weather = "light rain" } },
                    new() { estimated_time = 30000, predictions = new Status { vehicleDensity = 0.9f, averageSpeed = 85f, weather = "heavy rain" } }
                }
            };
        }
    }
}
