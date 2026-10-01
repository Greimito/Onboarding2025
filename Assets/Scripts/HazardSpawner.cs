using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Sistema de Spawn con puntos configurables.
    /// Spawnea proyectiles / bolas enemigas con un intervalo que disminuye dinámicamente con el tiempo de juego.
    /// </summary>
    [AddComponentMenu("Platformer 2.5D/Hazard Spawner")]
    public class HazardSpawner : MonoBehaviour
    {
        [Header("--- Puntos de Spawn y Prefab ---")]
        [Tooltip("Array de Transforms que definen las posiciones donde pueden aparecer las bolas.")]
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("Prefab del objeto a spawnear (HazardBall).")]
        [SerializeField] private GameObject hazardPrefab;

        [Header("--- Progresión de Dificultad ---")]
        [Tooltip("Tiempo de espera inicial entre spawns (en segundos).")]
        [SerializeField] private float initialSpawnInterval = 3.0f;

        [Tooltip("Tiempo mínimo al que puede reducirse el intervalo (límite de máxima velocidad).")]
        [SerializeField] private float minSpawnInterval = 0.5f;

        [Tooltip("Tiempo total de juego (en segundos) tras el cual se alcanza el spawn más rápido.")]
        [SerializeField] private float timeToReachMaxDifficulty = 90f;

        [Header("--- Comportamiento de Selección ---")]
        [Tooltip("Evita spawnear dos veces consecutivas en el mismo punto exacto.")]
        [SerializeField] private bool avoidSamePointRepeats = true;

        private float _currentSpawnInterval;
        private int _lastSpawnIndex = -1;
        private Coroutine _spawnCoroutine;

        public float CurrentSpawnInterval => _currentSpawnInterval;

        private void Start()
        {
            _currentSpawnInterval = initialSpawnInterval;
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        private void Update()
        {
            UpdateDifficulty();
        }

        private void UpdateDifficulty()
        {
            float elapsedTime = (GameManager.Instance != null) ? GameManager.Instance.GameTime : Time.timeSinceLevelLoad;

            // Reducción lineal suave del intervalo en función del tiempo transcurrido
            float progress = Mathf.Clamp01(elapsedTime / timeToReachMaxDifficulty);
            _currentSpawnInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, progress);
        }

        private IEnumerator SpawnLoop()
        {
            // Pequeño retardo antes del primer spawn
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                if (GameManager.Instance == null || (!GameManager.Instance.IsGameOver && GameManager.Instance.IsGameRunning))
                {
                    SpawnHazard();
                }

                yield return new WaitForSeconds(_currentSpawnInterval);
            }
        }

        private void SpawnHazard()
        {
            if (hazardPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

            Transform selectedPoint = GetNextSpawnPoint();
            if (selectedPoint != null)
            {
                Instantiate(hazardPrefab, selectedPoint.position, selectedPoint.rotation);
            }
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints.Length == 1) return spawnPoints[0];

            int index;
            int attempts = 0;
            do
            {
                index = Random.Range(0, spawnPoints.Length);
                attempts++;
            } while (avoidSamePointRepeats && index == _lastSpawnIndex && attempts < 10);

            _lastSpawnIndex = index;
            return spawnPoints[index];
        }

        #region Gizmos para el Editor

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;

            Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(spawnPoints[i].position, 0.35f);
                    Gizmos.DrawRay(spawnPoints[i].position, Vector3.down * 1.5f);
                }
            }
        }

        #endregion
    }
}
