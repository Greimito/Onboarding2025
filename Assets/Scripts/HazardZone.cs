using System.Collections;
using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Zona de peligro ("Zonal") generada cuando una bola enemiga impacta contra el suelo.
    /// Permanece activa durante X segundos y daña al jugador si entra o permanece en contacto con ella.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Platformer 2.5D/Hazard Zone")]
    public class HazardZone : MonoBehaviour
    {
        [Header("--- Parámetros del Zonal ---")]
        [Tooltip("Tiempo de duración de la zona en segundos antes de autodestruirse.")]
        [SerializeField] private float lifetime = 5.0f;

        [Tooltip("Daño que inflige al jugador al entrar en contacto.")]
        [SerializeField] private int damage = 1;

        [Tooltip("Intervalo en segundos entre cada tick de daño si el jugador se queda dentro.")]
        [SerializeField] private float damageTickRate = 0.5f;

        public float Lifetime
        {
            get => lifetime;
            set => lifetime = value;
        }

        [Header("--- Efectos Visuales ---")]
        [Tooltip("Si está activo, la zona parpadeará o se encogerá antes de desaparecer.")]
        [SerializeField] private bool animateExpiration = true;

        [Tooltip("Tiempo previo al final donde empieza la animación de aviso.")]
        [SerializeField] private float warningTime = 1.0f;

        private float _timeRemaining;
        private float _nextDamageTime;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Start()
        {
            _timeRemaining = lifetime;
            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            while (_timeRemaining > 0f)
            {
                _timeRemaining -= Time.deltaTime;

                if (animateExpiration && _timeRemaining <= warningTime)
                {
                    // Efecto de pulso/encogimiento para avisar que está por desaparecer
                    float t = _timeRemaining / warningTime;
                    transform.localScale = _originalScale * Mathf.Lerp(0.1f, 1f, t);
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time >= _nextDamageTime)
            {
                if (TryDamage(other))
                {
                    _nextDamageTime = Time.time + damageTickRate;
                }
            }
        }

        private bool TryDamage(Collider other)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                return true;
            }

            return false;
        }
    }
}
