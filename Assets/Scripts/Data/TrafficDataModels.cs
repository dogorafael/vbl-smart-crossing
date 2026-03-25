// Modelo que espelha exatamente o contrato OpenAPI. 

using System.Collections.Generic;

namespace VBLSmartCrossing.Data
{
    /// <summary>
    /// Objeto raiz de resposta do GET /v1/traffic/status
    /// </summary>
    [System.Serializable]
    public class TrafficResponse
    {
        public Status current_status;
        public List<PredictedStatus> predicted_status;
    }

    /// <summary>
    /// Uma mudança de estado futuro agendada para ocorrer após <see cref="estimated_time"/> ms.
    /// </summary>
    [System.Serializable]
    public class PredictedStatus
    {
        /// <summary>Milissegundos a partir do momento em que os dados foram recebidos até que este estado seja ativado.</summary>
        public int estimated_time;

        public Status predictions;
    }

    /// <summary>
    /// Um instantâneo das condições de tráfego e clima na travessia.
    /// </summary>
    [System.Serializable]
    public class Status
    {
        /// <summary>
        /// Fração da via ocupada por veículos. Intervalo: 0.1 – 1.0.
        /// Intervalo de spawn (s) = 1 / vehicleDensity
        /// </summary>
        public float vehicleDensity;

        /// <summary>
        /// Velocidade média dos veículos em km/h. Intervalo: 0 – 100.
        /// Velocidade no Unity = (averageSpeed / 100) * VelocidadeReferencia
        /// </summary>
        public float averageSpeed;

        /// <summary>
        /// Um dos valores: sunny | clouded | foggy | light rain | heavy rain
        /// Afeta o multiplicador de velocidade do movimento do jogador.
        /// </summary>
        public string weather;
    }
}