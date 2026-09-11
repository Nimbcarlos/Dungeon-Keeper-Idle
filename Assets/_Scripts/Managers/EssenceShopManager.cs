using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public enum ShopPurchaseResult
    {
        Success, Busy, MissingDependencies, ItemNotForSale,
        InvalidItem, DuplicateItemID, InsufficientEssence, QuantityLimitReached
    }

    public class EssenceShopManager : MonoBehaviour
    {
        public static EssenceShopManager Instance { get; private set; }

        [Header("Referencias — usa os singletons se estiverem vazias")]
        [SerializeField] private ResourceManager _resources;
        [SerializeField] private SummoningItemInventory _inventory;

        [Header("Itens da Loja")]
        [SerializeField] private List<SummoningItemData> _itemsForSale = new();

        public IReadOnlyList<SummoningItemData> ItemsForSale => _itemsForSale.AsReadOnly();
        public event Action<SummoningItemData> OnPurchaseCompleted;
        private bool _isPurchasing;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public ShopPurchaseResult CheckPurchase(SummoningItemData item)
        {
            if (_isPurchasing) return ShopPurchaseResult.Busy;
            if (_resources == null) _resources = ResourceManager.Instance;
            if (_inventory == null) _inventory = SummoningItemInventory.Instance;
            if (_resources == null || _inventory == null) return ShopPurchaseResult.MissingDependencies;
            if (item == null || !_itemsForSale.Contains(item) || !item.availableInShop)
                return ShopPurchaseResult.ItemNotForSale;
            if (!item.TryValidate(out _)) return ShopPurchaseResult.InvalidItem;

            foreach (var other in _itemsForSale)
                if (other != null && other != item && other.itemID == item.itemID)
                    return ShopPurchaseResult.DuplicateItemID;

            if (!_inventory.CanAdd(item, item.quantityPerPurchase)) return ShopPurchaseResult.QuantityLimitReached;
            if (!_resources.CanAffordEssence(item.essencePrice)) return ShopPurchaseResult.InsufficientEssence;
            return ShopPurchaseResult.Success;
        }

        public bool CanBuy(SummoningItemData item) => CheckPurchase(item) == ShopPurchaseResult.Success;

        // Compra um pacote; quantidade e preco sao os configurados no asset.
        public ShopPurchaseResult TryBuy(SummoningItemData item)
        {
            ShopPurchaseResult result = CheckPurchase(item);
            if (result != ShopPurchaseResult.Success) return result;
            _isPurchasing = true;
            int price = item.essencePrice;
            int quantity = item.quantityPerPurchase;

            try
            {
                // Debita primeiro. ResourceManager emite OnEssenceChanged durante a chamada.
                // Eventos de UI nao devem comprar/consumir itens ou salvar a transacao parcial.
                if (!_resources.SpendEssence(price)) return ShopPurchaseResult.InsufficientEssence;

                if (!_inventory.TryAdd(item, quantity))
                {
                    _resources.AddEssence(price);
                    return ShopPurchaseResult.QuantityLimitReached;
                }
            }
            finally
            {
                _isPurchasing = false;
            }

            // Notificacao somente depois de saldo e inventario estarem atualizados.
            if (OnPurchaseCompleted != null)
                foreach (Action<SummoningItemData> listener in OnPurchaseCompleted.GetInvocationList())
                    try { listener(item); }
                    catch (Exception exception) { Debug.LogException(exception, this); }

            return ShopPurchaseResult.Success;
        }

        // Metodo void selecionavel no OnClick do Inspector para um teste manual.
        public void BuyItem(SummoningItemData item)
        {
            var result = TryBuy(item);
            Debug.Log($"[Essence Shop] {(item != null ? item.displayName : "null")}: {result}", this);
        }
    }
}