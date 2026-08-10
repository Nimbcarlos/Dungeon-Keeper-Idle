using UnityEngine;
using UnityEngine.UI;


namespace DungeonKeeper
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Vector3 offset = new Vector3(0, 2.2f, 0);

        private Transform targetTransform;
        private Camera mainCamera;

        public void Initialize(Transform target)
        {
            targetTransform = target;
            mainCamera = Camera.main;
        }

        void OnEnable()
        {
            PiPUIVisibility.OnPiPStateChanged += HandlePiPChanged;
            // Aplica o estado atual imediatamente
            HandlePiPChanged(PiPUIVisibility.IsPiPActive);
        }

        void OnDisable()
        {
            PiPUIVisibility.OnPiPStateChanged -= HandlePiPChanged;
        }

        void LateUpdate()
        {
            if (targetTransform == null || PiPUIVisibility.IsPiPActive) return;

            // Acompanha a posição do personagem
            transform.position = targetTransform.position + offset;

            // Garante que a barra sempre olhe para a câmera (Billboard)
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }

        public void UpdateHealth(float percent)
        {
            if (healthSlider != null)
            {
                healthSlider.value = Mathf.Clamp01(percent);
            }
        }

        private void HandlePiPChanged(bool inPiP)
        {
            if (canvasGroup != null)
            {
                // Oculta completamente a barra durante o PiP sem destruir o objeto
                canvasGroup.alpha = inPiP ? 0f : 1f;
                canvasGroup.blocksRaycasts = !inPiP;
            }
        }
    }
}