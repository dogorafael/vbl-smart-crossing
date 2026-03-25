// Move um veículo em linha reta até sair do limite de despawn,
// então notifica o TrafficManager e se destrói.
// A velocidade pode ser atualizada em tempo real quando o status da API muda durante o nível.

using UnityEngine;
using VBLSmartCrossing.Core;

namespace VBLSmartCrossing.Traffic
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        // - Properties -
                
        private float _speed;
        private float _direction;   // +1 = direita, -1 = esquerda
        private float _despawnAbsX;
        private TrafficManager _manager;
        private Rigidbody _rb;

        // - Unity lifecycle -

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Veiculos translocam apenas pelo eixo X , podemos travar todo o resto
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation |
                              RigidbodyConstraints.FreezePositionY |
                              RigidbodyConstraints.FreezePositionZ;            
        }

        private void OnEnable()
        {
            GameEvents.OnLevelCompleted += Stop;
            GameEvents.OnGameOver += Stop;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCompleted -= Stop;
            GameEvents.OnGameOver -= Stop;
        }

        // - Public API -

        /// <summary>        
        /// Chamadso uma vez pelo TrafficManager imediatamente após instanciar.
        /// </summary>
        public void Initialize(float speed, float direction, float despawnAbsX, TrafficManager manager)
        {
            _speed = speed;
            _direction = direction;
            _despawnAbsX = despawnAbsX;
            _manager = manager;            

            // Orienta o veiculo para apontar para a direção que vai avançar
            transform.rotation = Quaternion.Euler(0f, direction > 0f ? 0f : 180f, 0f);
        }

        /// <summary>Atualiza a velocidade em tempo real quando um novo Status é recebido da API.</summary>
        public void SetSpeed(float newSpeed) => _speed = newSpeed;

        // - Physics update -

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + Vector3.right * (_direction * _speed * Time.fixedDeltaTime));

            if (Mathf.Abs(transform.position.x) > _despawnAbsX)
                Despawn();
        }

        // - Helpers -

        private void Stop()
        {
            SetSpeed(0);
        }

        private void Despawn()
        {
            _manager?.NotifyVehicleDespawned(this);
            Destroy(gameObject);
        }
    }
}
