using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_EssenceShopWindow : MonoBehaviour
    {
        [Header("Janela — controller deve ficar fora do painel")]
        [SerializeField] private GameObject _windowPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _content;
        [SerializeField] private UI_SummoningShopCard _cardPrefab;
        [SerializeField] private TextMeshProUGUI _essenceBalanceText;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("Collection — opcional, apenas o painel visual")]
        [SerializeField] private CanvasGroup _collectionGroup;

        [Header("Mensagens")]
        [SerializeField] private string _purchasedMessage = "Purchase completed!";
        [SerializeField] private string _insufficientMessage = "Not enough Essence.";
        [SerializeField] private string _unavailableMessage = "This item is unavailable.";
        [SerializeField] private string _emptyMessage = "No items available.";

        private readonly List<UI_SummoningShopCard> _cards = new();
        private EssenceShopManager _shop;
        private SummoningItemInventory _inventory;
        private ResourceManager _resources;
        private UI_WindowCoordinator _coordinator;
        private bool _open;
        private bool _dirty;
        private bool _collectionHidden;
        private float _oldAlpha;
        private bool _oldInteractable;
        private bool _oldRaycasts;
        public bool IsOpen => _open;

        private void Awake()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseWindow);
            if (_windowPanel != null) _windowPanel.SetActive(false);
        }

        public void ToggleWindow() { if (_open) CloseWindow(); else OpenWindow(); }

        public void OpenWindow()
        {
            if (_open) return;
            _shop = EssenceShopManager.Instance;
            _inventory = SummoningItemInventory.Instance;
            _resources = ResourceManager.Instance;
            _coordinator = UI_WindowCoordinator.Instance;
            if (_windowPanel == null || _content == null || _cardPrefab == null ||
                _shop == null || _inventory == null || _resources == null || _coordinator == null)
            {
                Debug.LogError("[Shop UI] Confira painel, Content, prefab, managers e WindowCoordinator.", this);
                return;
            }

            _open = true;
            _coordinator.RegisterWindow(this);
            // Oculta a Collection sem desativar seus controllers nem liberar sua pausa.
            if (_collectionGroup != null && _collectionGroup.gameObject.activeInHierarchy)
            {
                _oldAlpha = _collectionGroup.alpha;
                _oldInteractable = _collectionGroup.interactable;
                _oldRaycasts = _collectionGroup.blocksRaycasts;
                _collectionHidden = true;
                _collectionGroup.alpha = 0;
                _collectionGroup.interactable = false;
                _collectionGroup.blocksRaycasts = false;
            }
            _windowPanel.SetActive(true);
            _windowPanel.transform.SetAsLastSibling();
            _resources.OnEssenceChanged += OnEssenceChanged;
            _inventory.OnInventoryChanged += MarkDirty;
            _shop.OnPurchaseCompleted += OnPurchased;
            SetFeedback("");
            RebuildCards();
        }

        public void RebuildCards()
        {
            ClearCards();
            if (!_open || _shop == null) return;
            var seen = new HashSet<SummoningItemData>();
            foreach (var item in _shop.ItemsForSale)
            {
                if (item == null || !item.availableInShop || !seen.Add(item)) continue;
                var card = Instantiate(_cardPrefab, _content);
                card.gameObject.SetActive(true);
                card.Setup(item, Buy);
                _cards.Add(card);
            }
            if (_cards.Count == 0) SetFeedback(_emptyMessage);
            RefreshUI();
        }

        private void Buy(SummoningItemData item)
        {
            if (!_open || _shop == null) return;
            var result = _shop.TryBuy(item);
            SetFeedback(result == ShopPurchaseResult.Success ? _purchasedMessage :
                result == ShopPurchaseResult.InsufficientEssence ? _insufficientMessage : _unavailableMessage);
            if (result != ShopPurchaseResult.Success)
                Debug.LogWarning($"[Shop UI] Compra recusada: {result}", this);
            RefreshUI();
        }

        private void OnPurchased(SummoningItemData item) { SetFeedback(_purchasedMessage); MarkDirty(); }
        private void OnEssenceChanged(int value) => MarkDirty();
        private void MarkDirty() => _dirty = true;

        private void LateUpdate()
        {
            if (!_open) return;
            if (_windowPanel == null || !_windowPanel.activeInHierarchy) { CloseWindow(); return; }
            // Aguarda o fim da transacao antes de consultar CanBuy (manager pode estar Busy).
            if (_dirty) RefreshUI();
        }

        private void RefreshUI()
        {
            _dirty = false;
            if (_essenceBalanceText != null)
                _essenceBalanceText.text = _resources != null ? _resources.Essence.ToString() : "0";
            foreach (var card in _cards)
                if (card != null) card.RefreshState(_shop, _inventory);
        }

        public void CloseWindow()
        {
            if (!_open) return;
            _open = false;
            if (_resources != null) _resources.OnEssenceChanged -= OnEssenceChanged;
            if (_inventory != null) _inventory.OnInventoryChanged -= MarkDirty;
            if (_shop != null) _shop.OnPurchaseCompleted -= OnPurchased;
            if (_windowPanel != null) _windowPanel.SetActive(false);
            if (_collectionHidden && _collectionGroup != null)
            {
                _collectionGroup.alpha = _oldAlpha;
                _collectionGroup.interactable = _oldInteractable;
                _collectionGroup.blocksRaycasts = _oldRaycasts;
            }
            _collectionHidden = false;
            if (_coordinator != null) _coordinator.UnregisterWindow(this);
            ClearCards();
        }

        private void ClearCards()
        {
            foreach (var card in _cards)
                if (card != null) { card.gameObject.SetActive(false); Destroy(card.gameObject); }
            _cards.Clear();
        }

        private void SetFeedback(string message) { if (_feedbackText != null) _feedbackText.text = message; }
        private void OnDisable() => CloseWindow();
        private void OnDestroy()
        {
            CloseWindow();
            if (_closeButton != null) _closeButton.onClick.RemoveListener(CloseWindow);
        }
    }
}
