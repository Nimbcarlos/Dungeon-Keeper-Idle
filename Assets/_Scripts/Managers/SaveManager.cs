using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonKeeper
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public SaveData CurrentData { get; private set; }

        private bool _loaded;
        private InventoryManager _inventory;
        private SummoningItemInventory _items;
        private bool _pendingSave;
        private void MarkSavePending() => _pendingSave = true;
        private void LateUpdate() { if (_pendingSave) { _pendingSave = false; SaveGame(); } }

        private void OnApplicationPause(bool paused)
        {
            if (paused && _loaded) SaveGame();
        }

        private void OnApplicationQuit()
        {
            if (_loaded) SaveGame();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= SaveGame;
            if (_items != null) _items.OnInventoryChanged -= MarkSavePending;
            if (Instance == this) Instance = null;
        }

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
            if (!_loaded || InventoryManager.Instance == null) return;
            if (SummoningManager.Instance != null && SummoningManager.Instance.IsBusy) { _pendingSave = true; return; }
            _pendingSave = false;
            // Preserva os campos que este manager ainda nao gerencia (ovos, skins).
            SaveData data = CurrentData ?? new SaveData();
            data.laneDeployments = new List<LaneSaveState>();

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
                    data.laneDeployments.Add(new LaneSaveState
                    {
                        laneIndex = i,
                        instanceID = slot.EquippedInstance.instanceID
                    });
                }
            }

            if (SummoningItemInventory.Instance != null) data.summoningItems = SummoningItemInventory.Instance.CaptureState();
            if (SummoningManager.Instance != null) data.summoningSlots = SummoningManager.Instance.CaptureState();
            SaveSystem.Save(data);
            CurrentData = data;
        }

        public void LoadGame()
        {
            _loaded = false;
            _pendingSave = false;
            CurrentData = SaveSystem.Load();
            ApplyLoadedData();
            _loaded = InventoryManager.Instance != null;
            if (_inventory != null) _inventory.OnInventoryChanged -= SaveGame;
            _inventory = InventoryManager.Instance;
            if (_inventory != null) _inventory.OnInventoryChanged += SaveGame;
            if (_items != null) _items.OnInventoryChanged -= MarkSavePending;
            _items = SummoningItemInventory.Instance;
            if (_items != null) _items.OnInventoryChanged += MarkSavePending;
        }

        private void ApplyLoadedData()
        {
            if (CurrentData == null) return;
            SummoningItemInventory.Instance?.RestoreState(CurrentData.summoningItems);
            SummoningManager.Instance?.RestoreState(CurrentData.summoningSlots);

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
                    if (entry == null) continue;
                    int laneIndex = entry.laneIndex;
                    string instanceID = entry.instanceID;

                    if (laneIndex >= 0 && laneIndex < slots.Length)
                    {
                        MonsterInstance instance = InventoryManager.Instance.OwnedInstances.FirstOrDefault(m => m != null && m.instanceID == instanceID);
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
