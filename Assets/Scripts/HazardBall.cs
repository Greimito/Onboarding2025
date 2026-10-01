using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Bola enemiga que cae y rueda por física y gravedad pura (sin restricciones artificiales en el eje Z).
    /// Si impacta contra el jugador le inflige daño.
    /// Si impacta contra el suelo se destruye y genera un HazardZone ("Zonal") adaptado a la inclinación de la superficie.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Platformer 2.5D/Hazard Ball")]
    public class HazardBall : MonoBehaviour, IInteractable, IInteract
    {
        [Header("--- Daño y Colisiones ---")]
        [Tooltip("Daño infligido al jugador al impactar.")]
        [SerializeField] private int directHitDamage = 1;

        [Tooltip("Prefab del Zonal que se instanciará al chocar con el suelo.")]
        [SerializeField] private GameObject hazardZonePrefab;

        [Tooltip("Capas consideradas como suelo para crear el zonal.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("--- Física y Gravedad ---")]
        [Tooltip("Tiempo de vida máximo antes de autodestruirse si cae fuera del escenario.")]
        [SerializeField] private float maxLifetime = 15f;

        [Tooltip("Impulso o empuje inicial opcional al spawnear (dejar en 0 si solo cae por gravedad).")]
        [SerializeField] private Vector3 initialPush = Vector3.zero;

        private Rigidbody _rb;
        private bool _hasImpacted = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                // Aseguramos que la física y gravedad de Unity actúen libremente sin locks
                _rb.useGravity = true;
                _rb.isKinematic = false;
                _rb.constraints = RigidbodyConstraints.None; // Libre para rodar por planos inclinados
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        private void Start()
        {
            Destroy(gameObject, maxLifetime);

            if (_rb != null && initialPush != Vector3.zero)
            {
                _rb.AddForce(initialPush, ForceMode.Impulse);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Vector3 contactPoint = (collision.contacts.Length > 0) ? collision.contacts[0].point : transform.position;
            Vector3 contactNormal = (collision.contacts.Length > 0) ? collision.contacts[0].normal : Vector3.up;

            HandleHit(collision.gameObject, contactPoint, contactNormal);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject, transform.position, Vector3.up);
        }

        private void HandleHit(GameObject hitObject, Vector3 contactPoint, Vector3 surfaceNormal)
        {
            if (_hasImpacted) return;

            // 1. Comprobar si golpea al jugador (IDamageable)
            IDamageable damageable = hitObject.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hitObject.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                _hasImpacted = true;
                damageable.TakeDamage(directHitDamage);
                Destroy(gameObject);
                return;
            }

            // 2. Comprobar si golpea el suelo
            int objectLayerMask = 1 << hitObject.layer;
            if ((groundLayers.value & objectLayerMask) != 0)
            {
                _hasImpacted = true;
                SpawnHazardZone(contactPoint, surfaceNormal);
                Destroy(gameObject);
            }
        }

        private void SpawnHazardZone(Vector3 spawnPosition, Vector3 surfaceNormal)
        {
            if (hazardZonePrefab != null)
            {
                // Alineamos la rotación del Zonal con la inclinación del suelo (normal de la superficie)
                Quaternion alignmentRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
                Instantiate(hazardZonePrefab, spawnPosition + surfaceNormal * 0.05f, alignmentRotation);
            }
        }

        /// <summary>
        /// Implementación de IInteractable / IInteract
        /// </summary>
        public void Interact(GameObject source)
        {
            Debug.Log($"[HazardBall] Interacción con {source.name}");
        }
    }
}
