// GameEvents (Event Bus)
// Declarações de eventos estáticos que permitem que todos os sistemas se comuniquem
// sem manter referências diretas entre si. O padrão publish/subscribe mantém o código
// modular: adicionar ou remover um sistema nunca quebra os outros.


using System;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Core
{
    public static class GameEvents
    {
        // - Data / API -

        /// <summary>Disparado quando novos dados da API foram desserializados e estão prontos.</summary>
        public static event Action<TrafficResponse> OnApiDataLoaded;
      
        /// <summary>Disparado quando uma predição agendada é recuperada (troca de status).</summary>
        public static event Action<Status> OnStatusChanged;

        // - Game Flow -
        
        /// <summary>Disparado quando o jogador alcança o outro lado da rua.</summary>
        public static event Action OnLevelCompleted;
        
        /// <summary>Disparado quando o cronômetro zera.</summary>
        public static event Action OnTimeOut;
        
        /// <summary>Dispara para transicionar para o estado de Game Over.</summary>
        public static event Action OnGameOver;

        /// <summary>Disparado quando o contador de níveis é incrementado, carregando o novo número do nível.</summary>
        public static event Action<int> OnLevelChanged;

        /// <summary>Disparado a cada frame enquanto o timer está ativo; carrega os segundos restantes.</summary>
        public static event Action<float> OnTimerTick;

        /// <summary>Disparado entre os níveis para resetar todos os sistemas antes de carregar a próxima chamada da API.</summary>
        public static event Action OnGameReset;

        // - Event Dispatchers - 

        public static void RaiseApiDataLoaded(TrafficResponse data) => OnApiDataLoaded?.Invoke(data);
        public static void RaiseStatusChanged(Status status) => OnStatusChanged?.Invoke(status);
        public static void RaiseLevelCompleted() => OnLevelCompleted?.Invoke();
        public static void RaiseTimeOut() => OnTimeOut?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseLevelChanged(int level) => OnLevelChanged?.Invoke(level);
        public static void RaiseTimerTick(float remaining) => OnTimerTick?.Invoke(remaining);
        public static void RaiseGameReset() => OnGameReset?.Invoke();
    }
}
