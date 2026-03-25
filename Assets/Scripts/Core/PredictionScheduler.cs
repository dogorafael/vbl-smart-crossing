// Lê o array predicted_status da resposta da API e agenda cada mudança de Status
// para ser disparada exatamente no seu estimated_time (ms) usando corrotinas.
//
// O jogo deve 'agendar' essas mudanças. Se uma predição diz que em
// 10.000ms o tempo mudará para 'heavy rain', o jogo deve atualizar
// automaticamente o clima e o tráfego exatamente nesse momento.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Core
{
    public class PredictionScheduler : MonoBehaviour
    {
        private readonly List<Coroutine> _scheduled = new();

        // - Unity lifecycle -

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnGameReset += CancelAll;
            GameEvents.OnGameOver += CancelAll;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnGameReset -= CancelAll;
            GameEvents.OnGameOver -= CancelAll;
        }

        // - Event handlers -

        private void HandleApiDataLoaded(TrafficResponse response)
        {
            CancelAll(); // Descarta qualquer predição dos níveis anteriores

            if (response.predicted_status == null || response.predicted_status.Count == 0)
            {
                Debug.Log("[PredictionScheduler] Sem predições para agendar.");
                return;
            }

            foreach (var prediction in response.predicted_status)
            {
                var coroutine = StartCoroutine(SchedulePrediction(prediction));
                _scheduled.Add(coroutine);
            }
            
            Debug.Log($"[PredictionScheduler] {_scheduled.Count} predições agendadas.");
        }

        // - Coroutines -

        /// <summary>
        /// Aguarda o tempo da predição e então dispara o evento de mudança de status.
        /// Utiliza WaitForSeconds (tempo de jogo) para que pausar o jogo também pause as predições.
        /// </summary>
        private IEnumerator SchedulePrediction(PredictedStatus prediction)
        {
            float delaySeconds = prediction.estimated_time / 1000f;

            Debug.Log($"[PredictionScheduler] Agendado em {delaySeconds:F1}s -> " +
                      $"density={prediction.predictions.vehicleDensity:F2}, " +
                      $"speed={prediction.predictions.averageSpeed:F0}km/h, " +
                      $"weather={prediction.predictions.weather}");

            yield return new WaitForSeconds(delaySeconds);

            Debug.Log($"[PredictionScheduler] Predição disparada!");
            GameEvents.RaiseStatusChanged(prediction.predictions);
        }

        // - Helpers -

        private void CancelAll()
        {
            foreach (var c in _scheduled)
                if (c != null) StopCoroutine(c);

            if (_scheduled.Count > 0)
                Debug.Log($"[PredictionScheduler] Cancelou {_scheduled.Count} predições pendentes.");

            _scheduled.Clear();
        }
    }
}
