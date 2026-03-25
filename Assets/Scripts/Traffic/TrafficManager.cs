// Gerencia o spawn e a velocidade dos veículos a partir dos dados da API usando as fórmulas da especificação:
//
//   IntervaloSpawn (s) = 1 / vehicleDensity
//   VelocidadeUnity    = (averageSpeed / 100) * VelocidadeReferencia
//
// Cada pista alterna a direção: pistas com índice par se movem para a direita (+X),
// pistas com índice ímpar se movem para a esquerda (-X), criando a sensação clássica de Frogger.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VBLSmartCrossing.Core;
using VBLSmartCrossing.Data;

namespace VBLSmartCrossing.Traffic
{
    public class TrafficManager : MonoBehaviour
    {
        // - Inspector -

        [Header("Prefab")]
        [SerializeField] private GameObject _vehiclePrefab;

        [Header("Lanes - assign one spawn Transform per lane")]
        [Tooltip("Place spawn Transforms off-screen. Even indices spawn on the left, odd on the right.")]
        [SerializeField] private Transform[] _laneSpawnPoints;

        [Header("Speed Reference")]
        [Tooltip("Unity units/s when averageSpeed == 100 km/h. Tune this for your scene scale.")]
        [SerializeField] private float _referenceSpeed = 12f;

        [Header("Boundary")]
        [Tooltip("Absolute X value beyond which vehicles self-destroy.")]
        [SerializeField] private float _despawnAbsX = 20f;

        // - Runtime -

        private float _currentDensity = 0.2f;
        private float _currentSpeed   = 30f;
        private bool  _isActive;

        private Coroutine _spawnCoroutine;
        private readonly List<VehicleController> _activeVehicles = new();

        // - Derived values (spec formulas) -
     
        /// <summary>Segundos entre spaws consecutivos de veículos por pista.</summary>
        private float SpawnInterval => 1f / Mathf.Max(_currentDensity, 0.01f);

        /// <summary>Vehicle speed in Unity world units per second.</summary>
        /// <summary>Velocidade do veículo em unidades(Unity) por segundo.</summary>
        private float UnitySpeed => (_currentSpeed / 100f) * _referenceSpeed;

        // - Unity lifecycle -

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnStatusChanged += HandleStatusChanged;
            GameEvents.OnLevelCompleted += StopTraffic;
            GameEvents.OnGameOver += StopTraffic;
            GameEvents.OnGameReset += ClearAll;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnStatusChanged -= HandleStatusChanged;
            GameEvents.OnLevelCompleted -= StopTraffic;
            GameEvents.OnGameOver -= StopTraffic;
            GameEvents.OnGameReset -= ClearAll;
        }

        // - Event handlers -

        private void HandleApiDataLoaded(TrafficResponse response)
        {
            ApplyStatus(response.current_status);
            StartTraffic();
        }

        private void HandleStatusChanged(Status status) => ApplyStatus(status);

        // - Traffic control -

        private void ApplyStatus(Status status)
        {
            print("APPLY STATUS");
            _currentDensity = status.vehicleDensity;
            _currentSpeed   = status.averageSpeed;

            Debug.Log($"[TrafficManager] density={_currentDensity:F2} -> interval={SpawnInterval:F2}s | " +
                      $"speed={_currentSpeed:F0}km/h -> {UnitySpeed:F2}u/s");

            // Atualiza em tempo real os veículos já presentes na cena
            foreach (var v in _activeVehicles)
                v?.SetSpeed(UnitySpeed);

            // Reinicia o loop de spawn para que o novo intervalo tenha efeito imediato
            if (_isActive) RestartSpawnLoop();
        }

        private void StartTraffic()
        {
            _isActive = true;
            RestartSpawnLoop();
        }

        private void StopTraffic()
        {
            _isActive = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private void RestartSpawnLoop()
        {
            if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        // - Spawn loop -

        private IEnumerator SpawnLoop()
        {
            while (_isActive)
            {
                SpawnVehicleInRandomLane();
                yield return new WaitForSeconds(SpawnInterval);
            }
        }

        private void SpawnVehicleInRandomLane()
        {
            if (_laneSpawnPoints == null || _laneSpawnPoints.Length == 0)
            {
                Debug.LogWarning("[TrafficManager] Nenhum spown point foi associado!");
                return;
            }

            int laneIndex  = Random.Range(0, _laneSpawnPoints.Length);
            Transform lane = _laneSpawnPoints[laneIndex];

            // Pistas pares vão para direita, ímpares para a esquerda
            float direction = (laneIndex % 2 == 0) ? 1f : -1f;

            var go = Instantiate(_vehiclePrefab, lane.position, Quaternion.identity);
            var vc = go.GetComponent<VehicleController>();

            if (vc == null)
            {
                Debug.LogError("[TrafficManager] VehiclePrefab não possui um VehicleController!");
                Destroy(go);
                return;
            }

            vc.Initialize(UnitySpeed, direction, _despawnAbsX, this);
            _activeVehicles.Add(vc);
        }
        
        public void NotifyVehicleDespawned(VehicleController vehicle) => _activeVehicles.Remove(vehicle);

        // - Reset -

        private void ClearAll()
        {
            StopTraffic();
            foreach (var v in _activeVehicles)
                if (v != null) Destroy(v.gameObject);
            _activeVehicles.Clear();
        }
    }
}
