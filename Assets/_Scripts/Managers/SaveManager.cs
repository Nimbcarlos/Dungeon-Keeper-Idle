using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonKeeper
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public SaveData CurrentData { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            LoadGame();
        }

        public void SaveGame()
        {
            SaveData data = new SaveData();

            // 1. Recursos Globais
            if (ResourceManager.Instance != null)
            {
                data.gold = ResourceManager.Instance.Gold;
                data.essence = ResourceManager.Instance.Essence;
            }

            // 2. Salva a Coleção Viva do Inventário (MonsterInstance)
            if (InventoryManager.Instance != null)
            {
                data.ownedInstances = new List<MonsterInstance>(InventoryManager.Instance.OwnedInstances);
            }

            // 3. Salva o Estado de Alocação das Lanes ordenadas por nome
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude)
                                  .OrderBy(s => s.gameObject.name)
                                  .ToArray();

            for (int i = 0; i < slots.Length; i++)
            {
                MonsterSlot slot = slots[i];
                if (slot.HasMonsterEquipped && slot.EquippedInstance != null)
                {
                    // Grava qual instanceID está ocupando esta Lane Index
                    data.laneDeployments[i] = slot.EquippedInstance.instanceID;
                }
            }

            SaveSystem.Save(data);
            CurrentData = data;
        }

        public void LoadGame()
        {
            CurrentData = SaveSystem.Load();
            ApplyLoadedData();
        }

        private void ApplyLoadedData()
        {
            if (CurrentData == null) return;

            // 1. Restaura Recursos
            ResourceManager.Instance?.SetGold(CurrentData.gold);
            ResourceManager.Instance?.SetEssence(CurrentData.essence);

            // 2. Restaura o Inventário com as instâncias salvas
            if (InventoryManager.Instance != null && CurrentData.ownedInstances != null)
            {
                InventoryManager.Instance.LoadSavedInstances(CurrentData.ownedInstances);
            }

            // 3. Ordena os slots das Lanes
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude)
                                  .OrderBy(s => s.gameObject.name)
                                  .ToArray();

            // Clear inicial dos slots
            foreach (var slot in slots)
            {
                slot.ClearSlot();
            }

            // 4. Restaura a alocação de MonsterInstance em cada Lane
            if (CurrentData.laneDeployments != null && InventoryManager.Instance != null)
            {
                foreach (var entry in CurrentData.laneDeployments)
                {
                    int laneIndex = entry.Key;
                    string instanceID = entry.Value;

                    if (laneIndex >= 0 && laneIndex < slots.Length)
                    {
                        MonsterInstance instance = InventoryManager.Instance.OwnedInstances.FirstOrDefault(m => m.instanceID == instanceID);
                        if (instance != null)
                        {
                            slots[laneIndex].EquipMonster(instance);
                        }
                    }
                }
            }
        }
    }
}