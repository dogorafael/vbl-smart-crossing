// Controla o movimento do jogador (WASD / Setas) e eventos de colisão.
//
// A velocidade de movimento é multiplicada pelo modificador de clima do WeatherManager:
//   Velocidade_Base * multiplicador_clima
//
// Condição de vitória  -> OnTriggerEnter com tag "SafeZone"
// Condição de derrota  -> OnTriggerEnter com tag "Vehicle" OU tempo esgotado (gerenciado pelo GameManager)

using UnityEngine;
using VBLSmartCrossing.Core;
using VBLSmartCrossing.Data;
using VBLSmartCrossing.Weather;

namespace VBLSmartCrossing.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        // - Inspector -

        [Header("Movement")]
        [Tooltip("Base movement speed in Unity units/s at 1.0x weather multiplier.")]
        [SerializeField] private float _baseSpeed = 5f;

        [Header("References")]
        [SerializeField] private WeatherManager _weatherManager;

        // - Properties  -

        private Rigidbody _rb;
        private Vector3   _spawnPosition;
        private bool      _movementEnabled;

        // - Unity lifecycle -

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity  = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation |
                              RigidbodyConstraints.FreezePositionY;

            _spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            GameEvents.OnApiDataLoaded += HandleApiDataLoaded;
            GameEvents.OnLevelCompleted += DisableMovement;
            GameEvents.OnGameOver += DisableMovement;
            GameEvents.OnBeforeLevelLoad += ResetToSpawn;
        }

        private void OnDisable()
        {
            GameEvents.OnApiDataLoaded -= HandleApiDataLoaded;
            GameEvents.OnLevelCompleted -= DisableMovement;
            GameEvents.OnGameOver -= DisableMovement;
            GameEvents.OnBeforeLevelLoad -= ResetToSpawn;
        }

        // - Movement -

        private void FixedUpdate()
        {
            if (!_movementEnabled) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h == 0f && v == 0f) return;

            float multiplier = _weatherManager != null
                ? _weatherManager.CurrentSpeedMultiplier
                : 1f;

            Vector3 direction  = new Vector3(h, 0f, v).normalized;
            float   speed      = _baseSpeed * multiplier;

            _rb.MovePosition(_rb.position + direction * speed * Time.fixedDeltaTime);
        }

        // - Collision -

        private void OnTriggerEnter(Collider other)
        {
            if (!_movementEnabled) return;

            if (other.CompareTag("SafeZone"))
            {                
                Debug.Log("[Player] Atingiu a 'safe zone' -> Completou o nível!");
                DisableMovement();
                GameEvents.RaiseLevelCompleted();
            }
            else if (other.CompareTag("Vehicle"))
            {
                Debug.Log("[Player] Atingido por veículo -> Game Over!");
                DisableMovement();
                GameEvents.RaiseGameOver();
            }
        }

        // - State helpers -

        /// <summary>Movimento habilitado apenas quando os dados da API foram completamente carregados.</summary>
        private void HandleApiDataLoaded(TrafficResponse _) => _movementEnabled = true;

        private void DisableMovement() => _movementEnabled = false;

        private void ResetToSpawn()
        {
            _movementEnabled = false;
            _rb.linearVelocity = Vector3.zero;
            transform.position = _spawnPosition;
        }
    }
}
