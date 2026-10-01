using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Cámara de seguimiento suave para juegos 2.5D.
    /// Sigue al jugador en los ejes X e Y manteniendo fija la distancia en Z.
    /// </summary>
    [AddComponentMenu("Platformer 2.5D/Camera Follow 2.5D")]
    public class CameraFollow25D : MonoBehaviour
    {
        [Header("--- Objetivo ---")]
        [Tooltip("El Transform del jugador (la esfera).")]
        [SerializeField] private Transform target;

        [Header("--- Ajustes de Seguimiento ---")]
        [Tooltip("Desplazamiento relativo de la cámara respecto al objetivo.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

        [Tooltip("Suavizado del movimiento (valores más bajos = seguimiento más rápido).")]
        [Range(0.01f, 1f)]
        [SerializeField] private float smoothTime = 0.2f;

        [Header("--- Límites (Opcional) ---")]
        [Tooltip("Activar límites mínimos/máximos en X e Y.")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

        private Vector3 _velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            // Mantenemos la posición Z del offset fija
            desiredPosition.z = target.position.z + offset.z;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
