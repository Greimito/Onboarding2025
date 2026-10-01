using System.Collections;
using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Gestiona la vida del jugador, la inmunidad temporal tras recibir daño (i-frames) y la interacción con IDamageable.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Platformer 2.5D/Player Health")]
    public class PlayerHealth : MonoBehaviour, IDamageable, IDamage
    {
        [Header("--- Parámetros de Salud ---")]
        [Tooltip("Vida máxima del jugador.")]
        [SerializeField] private int maxHealth = 3;

        [Tooltip("Segundos de inmunidad tras recibir daño.")]
        [SerializeField] private float invulnerabilityDuration = 1.2f;

        [Header("--- Feedback Visual ---")]
        [Tooltip("Renderer que parpadeará durante la inmunidad.")]
        [SerializeField] private Renderer playerRenderer;

        [Tooltip("Velocidad del parpadeo visual al ser dañado.")]
        [SerializeField] private float flashInterval = 0.1f;

        private int _currentHealth;
        private bool _isInvulnerable;
        private Coroutine _invulnerabilityCoroutine;
        private SphereController25D _controller;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsInvulnerable => _isInvulnerable;

        private void Awake()
        {
            _controller = GetComponent<SphereController25D>();
            if (playerRenderer == null)
            {
                playerRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                maxHealth = GameManager.Instance.MaxPlayerHealth;
            }

            _currentHealth = maxHealth;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePlayerHealth(_currentHealth);
            }
        }

        /// <summary>
        /// Implementación de IDamageable / IDamage.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (_isInvulnerable || _currentHealth <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            Debug.Log($"[PlayerHealth] Jugador recibió {damage} de daño. Vida restante: {_currentHealth}/{maxHealth}");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePlayerHealth(_currentHealth);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (_invulnerabilityCoroutine != null) StopCoroutine(_invulnerabilityCoroutine);
                _invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
            }
        }

        private void Die()
        {
            Debug.Log("[PlayerHealth] El jugador ha muerto.");
            if (_controller != null)
            {
                _controller.enabled = false;
            }
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            _isInvulnerable = true;
            float elapsed = 0f;

            while (elapsed < invulnerabilityDuration)
            {
                if (playerRenderer != null)
                {
                    playerRenderer.enabled = !playerRenderer.enabled;
                }
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            if (playerRenderer != null)
            {
                playerRenderer.enabled = true;
            }

            _isInvulnerable = false;
            _invulnerabilityCoroutine = null;
        }
    }
}
