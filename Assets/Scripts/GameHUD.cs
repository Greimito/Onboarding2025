using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// HUD simple y automático para mostrar la vida del jugador, el tiempo de supervivencia y menú de reinicio.
    /// Funciona de inmediato sin necesidad de configurar un Canvas complejo.
    /// </summary>
    [AddComponentMenu("Platformer 2.5D/Game HUD")]
    public class GameHUD : MonoBehaviour
    {
        [Header("--- Estilo Visual ---")]
        [SerializeField] private bool showOnGUI = true;

        private GUIStyle _hudStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;

        private void OnGUI()
        {
            if (!showOnGUI || GameManager.Instance == null) return;

            InitStyles();

            // 1. Mostrar Tiempo y Salud en la esquina superior izquierda
            GUILayout.BeginArea(new Rect(20, 20, 300, 150));
            GUILayout.Label($"<b>TIEMPO:</b> {GameManager.Instance.GetFormattedTime()}", _hudStyle);

            string healthHearts = "";
            for (int i = 0; i < GameManager.Instance.MaxPlayerHealth; i++)
            {
                healthHearts += (i < GameManager.Instance.CurrentPlayerHealth) ? " ❤️" : " 🖤";
            }
            GUILayout.Label($"<b>VIDA:</b> {healthHearts}", _hudStyle);
            GUILayout.EndArea();

            // 2. Pantalla de Game Over
            if (GameManager.Instance.IsGameOver)
            {
                float panelWidth = 360;
                float panelHeight = 220;
                float posX = (Screen.width - panelWidth) * 0.5f;
                float posY = (Screen.height - panelHeight) * 0.5f;

                GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), "");
                GUILayout.BeginArea(new Rect(posX + 20, posY + 20, panelWidth - 40, panelHeight - 40));

                GUILayout.Label("<color=#FF4444><b>¡GAME OVER!</b></color>", _titleStyle);
                GUILayout.Space(10);
                GUILayout.Label($"Tiempo sobrevivido: {GameManager.Instance.GetFormattedTime()}", _hudStyle);
                GUILayout.Space(20);

                if (GUILayout.Button("REINTENTAR", _buttonStyle, GUILayout.Height(45)))
                {
                    GameManager.Instance.RestartGame();
                }

                GUILayout.EndArea();
            }
        }

        private void InitStyles()
        {
            if (_hudStyle == null)
            {
                _hudStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    richText = true
                };
                _hudStyle.normal.textColor = Color.white;
            }

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 26,
                    alignment = TextAnchor.MiddleCenter,
                    richText = true
                };
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold
                };
            }
        }
    }
}
