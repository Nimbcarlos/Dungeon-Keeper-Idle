using UnityEngine;
using System;


namespace DungeonKeeper
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public int Gold    { get; private set; }
        public int Essence { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action<int> OnEssenceChanged;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanAffordGold(int amount)    => Gold >= amount;
        public bool CanAffordEssence(int amount) => Essence >= amount;

        public void SetGold(int amount)
        {
            Gold = amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (!CanAffordGold(amount)) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void AddEssence(int amount)
        {
            Essence += amount;
            OnEssenceChanged?.Invoke(Essence);
            Debug.Log($"Essência: +{amount} (total: {Essence})");
        }

        public bool SpendEssence(int amount)
        {
            if (!CanAffordEssence(amount)) return false;
            Essence -= amount;
            OnEssenceChanged?.Invoke(Essence);
            return true;
        }
    }
}