using UnityEngine;

namespace DungeonKeeper
{
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    public abstract class ItemData : ScriptableObject
    {
        [Header("Identificacao")]
        [Tooltip("ID unico e permanente usado pelo inventario/save. Nao altere depois de distribuir saves.")]
        public string itemID;
        public string displayName;
        [TextArea(2, 5)] public string description;
        public Sprite icon;
        public GameObject previewPrefab;
        public ItemRarity rarity;

        [Header("Loja")]
        public bool availableInShop = true;
        [Min(0)] public int essencePrice = 50;
        [Min(1)] public int quantityPerPurchase = 1;
    }
}
