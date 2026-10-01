using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer25D
{
    /// <summary>
    /// GameManager centralizado: controla el tiempo de juego, la dificultad progresiva, la vida del jugador y el estado de la partida.
    /// </summary>
    [AddComponentMenu("Platformer 2.5D/Game Manager")]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("--- Estado del Juego ---")]
        [Tooltip("Vida máxima inicial del jugador.")]
        [SerializeField] private int maxPlayerHealth = 3;

        [Tooltip("Indica si el juego comienza automáticamente en Start.")]
        [SerializeField] private bool autoStartGame = true;

        // Estado interno
        private int _currentPlayerHealth;
        private float _gameTimer;
        private bool _isGameOver;
        private bool _isGameRunning;

        // Eventos para UI y otros sistemas
        public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
        public event Action<float> OnTimeUpdated;       // (elapsedTimeInSeconds)
        public event Action OnGameOver;
        public event Action OnGameRestart;

        // Getters públicos
        public int CurrentPlayerHealth => _currentPlayerHealth;
        public int MaxPlayerHealth => maxPlayerHealth;
        public float GameTime => _gameTimer;
        public bool IsGameOver => _isGameOver;
        public bool IsGameRunning => _isGameRunning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (autoStartGame)
            {
                StartGame();
            }
        }

        private void Update()
        {
            if (!_isGameRunning || _isGameOver) return;

            _gameTimer += Time.deltaTime;
            OnTimeUpdated?.Invoke(_gameTimer);
        }

        public void StartGame()
        {
            _gameTimer = 0f;
            _isGameOver = false;
            _isGameRunning = true;
            _currentPlayerHealth = maxPlayerHealth;

            OnHealthChanged?.Invoke(_currentPlayerHealth, maxPlayerHealth);
            OnTimeUpdated?.Invoke(_gameTimer);
        }

        /// <summary>
        /// Registra el cambio de salud del jugador e informa a la UI y componentes interesados.
        /// </summary>
        public void UpdatePlayerHealth(int newHealth)
        {
            if (_isGameOver) return;

            _currentPlayerHealth = Mathf.Clamp(newHealth, 0, maxPlayerHealth);
            OnHealthChanged?.Invoke(_currentPlayerHealth, maxPlayerHealth);

            if (_currentPlayerHealth <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Aplica daño directo a través del GameManager.
        /// </summary>
        public void DamagePlayer(int amount)
        {
            UpdatePlayerHealth(_currentPlayerHealth - amount);
        }

        public void TriggerGameOver()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            _isGameRunning = false;
            Debug.Log($"[GameManager] ¡Game Over! Tiempo sobrevivido: {GetFormattedTime()}");
            OnGameOver?.Invoke();
        }

        /// <summary>
        /// Reinicia la escena actual.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            OnGameRestart?.Invoke();
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        /// <summary>
        /// Devuelve el tiempo de juego formateado en MM:SS.
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(_gameTimer / 60F);
            int seconds = Mathf.FloorToInt(_gameTimer % 60F);
            int milliseconds = Mathf.FloorToInt((_gameTimer * 100f) % 100F);
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }
    }
}
