// Máquina de estados central. Gerencia o cronômetro regressivo cuja duração é definida pela ÚLTIMA entrada em predicted_status
// O tempo total que o jogador tem para atravessar é definido pelo estimated_time da última predição do array").
// Estados:  Idle -> Loading -> Playing -> LevelComplete -> GameOver

using System.Collections;
using UnityEngine;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Core
{
    public class GameManager : MonoBehaviour
    {
        // - Properties -
        public enum GameState { Idle, Loading, Playing, LevelComplete, GameOver }
        public GameState CurrentState { get; private set; } = GameState.Idle;        

        private float _timerDuration;
        private float _timeRemaining;
        private Coroutine _timerCoroutine;        

        // - Unity lifecycle -        

        private void Start()
        {
            TransitionTo(GameState.Loading);            
            GameEvents.RaiseRequestStartGame();
        }

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
            GameEvents.OnTimeOut += HandleTimeOut;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnBeforeLevelLoad += HandleGameReset;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            GameEvents.OnTimeOut -= HandleTimeOut;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnBeforeLevelLoad -= HandleGameReset;
        }

        // - Event handlers -

        private void HandleApiDataLoaded(TrafficResponse data)
        {
            // duração do timer = estimated_time da ÚLTIMA predição (ms -> s)
            if (data.predicted_status != null && data.predicted_status.Count > 0)
            {
                int lastMs = data.predicted_status[^1].estimated_time; 
                _timerDuration = lastMs / 1000f;
                Debug.Log($"[GameManager] Timer definido para {_timerDuration:F1}s (última predição).");
            }
            else
            {
                _timerDuration = 30f;
                Debug.LogWarning("[GameManager] Nenhuma predição na resposta, usando timer alternativo de 30s.");
            }

            TransitionTo(GameState.Playing);
            StartTimer();
        }

        private void HandleLevelCompleted()
        {
            StopTimer();
            TransitionTo(GameState.LevelComplete);
        }

        private void HandleTimeOut()
        {
            if (CurrentState != GameState.Playing) return;            
            Debug.Log("[GameManager] Cronômetro zerou -> Game Over.");
            GameEvents.RaiseGameOver();
        }

        private void HandleGameOver()
        {
            StopTimer();
            TransitionTo(GameState.GameOver);
        }

        private void HandleGameReset()
        {
            StopTimer();
            TransitionTo(GameState.Loading);
        }

        // - Countdown Timer -

        private void StartTimer()
        {
            StopTimer();
            _timeRemaining = _timerDuration;
            _timerCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private void StopTimer()
        {
            if (_timerCoroutine == null) return;
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        private IEnumerator CountdownCoroutine()
        {
            while (_timeRemaining > 0f)
            {
                _timeRemaining -= Time.deltaTime;
                GameEvents.RaiseTimerTick(Mathf.Max(_timeRemaining, 0f));
                yield return null; // Atualiza todo frame
            }

            GameEvents.RaiseTimeOut();
        }

        // - Helpers -

        private void TransitionTo(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"[GameManager] Estado -> {newState}");
        }
    }
}
