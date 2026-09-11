using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_ExpandableMenu : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _optionsPanel;

        public bool IsOpen =>
            _optionsPanel != null && _optionsPanel.activeSelf;

        private void Awake()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.AddListener(Toggle);

            Close();
        }

        public void Toggle()
        {
            if (_optionsPanel != null)
                _optionsPanel.SetActive(!IsOpen);
        }

        public void Open()
        {
            if (_optionsPanel != null)
                _optionsPanel.SetActive(true);
        }

        public void Close()
        {
            if (_optionsPanel != null)
                _optionsPanel.SetActive(false);
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnDestroy()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.RemoveListener(Toggle);
        }
    }
}