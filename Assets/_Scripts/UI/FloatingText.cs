using UnityEngine;
using TMPro;

namespace DungeonKeeper
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh; // Versao 3D (World Space nativo)
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float fadeDuration = 0.8f;

        private Color textColor;
        private float alphaTimer;

        /// <summary>
        /// Setup para valores numéricos de dano
        /// </summary>
        public void Setup(int damageAmount, bool isCritical = false)
        {
            Color color = isCritical ? Color.red : Color.yellow;
            float size = isCritical ? 8f : 5f;

            Setup(damageAmount.ToString(), color, size);
        }

        /// <summary>
        /// Setup genérico para qualquer texto (XP, "LEVEL UP!", mensagens, etc.)
        /// </summary>
        public void Setup(string message, Color color, float fontSize = 5f)
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

            textMesh.text = message;
            textColor = color;
            textMesh.color = textColor;
            textMesh.fontSize = fontSize;

            // FORÇA O TEXTO A RENDERIZAR NO TOPO DOS SPRITES
            MeshRenderer meshRenderer = textMesh.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = "UI"; // Ou "Default", "Foreground", etc.
                meshRenderer.sortingOrder = 5000;    // Número alto para ficar acima de tudo
            }

            alphaTimer = fadeDuration;
        }

        void Update()
        {
            // Desloca o texto diretamente para cima no mundo 3D/2D
            transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

            // Fade Out
            alphaTimer -= Time.deltaTime;
            if (alphaTimer <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                textColor.a = alphaTimer / fadeDuration;
                textMesh.color = textColor;
            }
        }
    }
}