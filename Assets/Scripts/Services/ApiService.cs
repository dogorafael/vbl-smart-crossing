// Única responsável pela comunicação HTTP com a API de Tráfego e Clima.
// Classe C# pura (sem MonoBehaviour), fácil de testar unitariamente e trocar
// por uma implementação diferente (ex: fallback com JSON local) sem afetar nenhuma lógica de jogo.

using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Services
{
    public class ApiService
    {
        // - Configuration -
        // Change BaseUrl to match your Mockoon port (default: 3000).
        private const string BaseUrl = "http://localhost:3000";
        private const string TrafficEndpoint = "/v1/traffic/status";
        private const int TimeoutSeconds = 10;

        // - Public API -

        /// <summary>
        /// Obtém de forma assíncrona o status atual de tráfego e clima.
        /// Retorna <c>null</c> e registra um erro em caso de falha de rede ou de parsing.
        /// </summary>
        public async Task<TrafficResponse> FetchTrafficStatusAsync()
        {
            string url = BaseUrl + TrafficEndpoint;
            Debug.Log($"[ApiService] GET {url}");

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = TimeoutSeconds;

            // Aguarda sem bloquear a thread principal do Unity (não é necessário corrotina).
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ApiService] Request falhou: {request.error}");
                return null;
            }

            return Deserialize(request.downloadHandler.text);
        }

        // - Private helpers -

        private static TrafficResponse Deserialize(string json)
        {
            Debug.Log($"[ApiService] Raw response:\n{json}");
            try
            {
                return JsonConvert.DeserializeObject<TrafficResponse>(json);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[ApiService] JSON parse error: {ex.Message}");
                return null;
            }
        }
    }
}
