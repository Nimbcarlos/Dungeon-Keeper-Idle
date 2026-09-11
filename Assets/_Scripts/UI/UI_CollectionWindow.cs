using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_CollectionWindow : MonoBehaviour
    {
        [Header("Janela")]
        [SerializeField] private GameObject _windowPanel;
        [SerializeField] private Button _closeButton;

        [Header("Páginas")]
        [SerializeField] private GameObject _monstersPage;
        [SerializeField] private GameObject _itemsPage;
        [SerializeField] private GameObject _summoningPage;

        [Header("Botões das Abas")]
        [SerializeField] private Button _monstersTabButton;
        [SerializeField] private Button _itemsTabButton;
        [SerializeField] private Button _summoningTabButton;

        [Header("Capacidade — provisória")]
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private int _monsterCapacity = 10;

        private InventoryManager _inventory;

        public bool IsOpen =>
            _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseWindow);

            if (_monstersTabButton != null)
                _monstersTabButton.onClick.AddListener(ShowMonsters);

            if (_itemsTabButton != null)
                _itemsTabButton.onClick.AddListener(ShowItems);

            if (_summoningTabButton != null)
                _summoningTabButton.onClick.AddListener(ShowSummoning);

            if (_windowPanel != null)
                _windowPanel.SetActive(false);
        }

        public void ToggleWindow()
        {
            if (IsOpen)
                CloseWindow();
            else
                OpenWindow();
        }

        public void OpenWindow()
        {
            if (_windowPanel == null)
            {
                Debug.LogError(
                    "[Collection] Atribua Window Panel no Inspector.",
                    this);
                return;
            }

            if (IsOpen) return;

            UI_WindowCoordinator.Instance.RegisterWindow(this);

            // Define a página antes de ativar a janela.
            ShowMonsters();
            _windowPanel.SetActive(true);

            SubscribeInventory();
            RefreshCapacity();
        }

        public void CloseWindow()
        {
            UnsubscribeInventory();

            if (_windowPanel != null)
                _windowPanel.SetActive(false);

            UI_WindowCoordinator.Instance.UnregisterWindow(this);
        }

        public void ShowMonsters()
        {
            ShowPage(_monstersPage);
        }

        public void ShowItems()
        {
            ShowPage(_itemsPage);
        }

        public void ShowSummoning()
        {
            ShowPage(_summoningPage);
        }

        private void ShowPage(GameObject selectedPage)
        {
            if (selectedPage == null) return;

            SetPage(_monstersPage, selectedPage);
            SetPage(_itemsPage, selectedPage);
            SetPage(_summoningPage, selectedPage);

            SetTab(_monstersTabButton, _monstersPage, selectedPage);
            SetTab(_itemsTabButton, _itemsPage, selectedPage);
            SetTab(_summoningTabButton, _summoningPage, selectedPage);
        }

        private static void SetPage(
            GameObject page,
            GameObject selectedPage)
        {
            if (page != null)
                page.SetActive(page == selectedPage);
        }

        private static void SetTab(
            Button button,
            GameObject page,
            GameObject selectedPage)
        {
            if (button != null)
                button.interactable =
                    page != null && page != selectedPage;
        }

        private void SubscribeInventory()
        {
            UnsubscribeInventory();

            _inventory = InventoryManager.Instance;

            if (_inventory != null)
                _inventory.OnInventoryChanged += RefreshCapacity;
        }

        private void UnsubscribeInventory()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshCapacity;

            _inventory = null;
        }

        private void RefreshCapacity()
        {
            if (_capacityText == null) return;

            int count = _inventory != null
                ? _inventory.OwnedInstances.Count
                : 0;

            _capacityText.text =
                $"Monsters: {count} / {_monsterCapacity}";
        }

        private void OnDisable()
        {
            CloseWindow();
        }

        private void OnDestroy()
        {
            UnsubscribeInventory();
            UI_WindowCoordinator.Instance.UnregisterWindow(this);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(CloseWindow);

            if (_monstersTabButton != null)
                _monstersTabButton.onClick.RemoveListener(ShowMonsters);

            if (_itemsTabButton != null)
                _itemsTabButton.onClick.RemoveListener(ShowItems);

            if (_summoningTabButton != null)
                _summoningTabButton.onClick.RemoveListener(ShowSummoning);
        }
    }
}