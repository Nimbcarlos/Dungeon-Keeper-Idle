using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class UI_WindowCoordinator : MonoBehaviour
    {
        public static UI_WindowCoordinator Instance { get; private set; }

        [Header("Grupo dos botões principais do HUD")]
        [SerializeField] private CanvasGroup _mainButtons;

        [Tooltip("Oculta os botões enquanto alguma janela está aberta.")]
        [SerializeField] private bool _hideButtons = true;

        private readonly HashSet<MonoBehaviour> _openWindows = new();

        private bool _locked;
        private float _previousTimeScale;

        private float _previousAlpha;
        private bool _previousInteractable;
        private bool _previousBlocksRaycasts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterWindow(MonoBehaviour window)
        {
            if (window == null || !_openWindows.Add(window))
                return;

            if (_locked) return;

            _locked = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (_mainButtons != null)
            {
                _previousAlpha = _mainButtons.alpha;
                _previousInteractable = _mainButtons.interactable;
                _previousBlocksRaycasts = _mainButtons.blocksRaycasts;

                _mainButtons.interactable = false;
                _mainButtons.blocksRaycasts = false;

                if (_hideButtons)
                    _mainButtons.alpha = 0f;
            }
        }

        public void UnregisterWindow(MonoBehaviour window)
        {
            if (window == null) return;

            _openWindows.Remove(window);

            if (_openWindows.Count == 0)
                ReleaseLock();
        }

        private void LateUpdate()
        {
            // Evita manter a pausa por um controller destruído
            // ou desativado sem executar seu fechamento normal.
            _openWindows.RemoveWhere(
                window => window == null || !window.isActiveAndEnabled);

            if (_openWindows.Count == 0)
                ReleaseLock();
        }

        private void ReleaseLock()
        {
            if (!_locked) return;

            _locked = false;
            Time.timeScale = _previousTimeScale;

            if (_mainButtons != null)
            {
                _mainButtons.alpha = _previousAlpha;
                _mainButtons.interactable = _previousInteractable;
                _mainButtons.blocksRaycasts = _previousBlocksRaycasts;
            }
        }

        private void OnDisable()
        {
            _openWindows.Clear();
            ReleaseLock();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}