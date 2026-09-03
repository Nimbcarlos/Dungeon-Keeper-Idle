using UnityEngine;
using System.Linq; // Necessário para ordenar os slots com .OrderBy()

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

            if (ResourceManager.Instance != null)
            {
                data.gold = ResourceManager.Instance.Gold;
                data.essence = ResourceManager.Instance.Essence;
            }

            // 🎯 Ordena os slots pelo NOME do GameObject ou Posição Y para garantir ordem fixa
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude)
                                  .OrderBy(s => s.gameObject.name)
                                  .ToArray();

            for (int i = 0; i < slots.Length; i++)
            {
                MonsterSlot slot = slots[i];

                if (slot.HasMonsterEquipped)
                {
                    Monster monster = slot.GetSpawnedMonster();
                    if (monster != null)
                    {
                        MonsterSaveState mState = monster.GetSaveData();
                        mState.laneIndex = i; // Grava o índice rigorosamente ordenado
                        data.activeMonsters.Add(mState);
                    }
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

            ResourceManager.Instance?.SetGold(CurrentData.gold);
            ResourceManager.Instance?.SetEssence(CurrentData.essence);

            // 🎯 Mesma ordenação exata usada no SaveGame
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude)
                                .OrderBy(s => s.gameObject.name)
                                .ToArray();

            // 1. Limpa TODOS os slots existentes antes de restaurar o save
            foreach (var slot in slots)
            {
                slot.ClearSlot();
            }

            // 2. Restaura apenas o que está no Save
            foreach (MonsterSaveState state in CurrentData.activeMonsters)
            {
                if (state.laneIndex < 0 || state.laneIndex >= slots.Length) continue;

                MonsterData data = InventoryManager.Instance?.FindMonsterDataByID(state.monsterID);
                if (data == null) continue;

                MonsterSlot targetSlot = slots[state.laneIndex];
                targetSlot.EquipMonster(data);

                Monster monster = targetSlot.GetSpawnedMonster();
                if (monster != null)
                {
                    monster.quality = state.quality;

                    // 🎯 1. Monta o objeto de progressão individual vindo dos dados do save
                    MonsterProgression progression = new MonsterProgression
                    {
                        currentLevel = state.currentLevel,
                        currentXP = state.currentXP,
                        unlockedSkillIDs = state.unlockedSkillIDs != null ? new System.Collections.Generic.List<string>(state.unlockedSkillIDs) : new System.Collections.Generic.List<string>()
                    };

                    // 🎯 2. Inicializa o monstro com seu Data e sua Progression de runtime
                    monster.InitializeMonster(data, progression);

                    // 🎯 3. Restaura as habilidades passando a instância de MonsterProgression criada
                    MonsterSkillTree tree = monster.GetComponent<MonsterSkillTree>();
                    tree?.RestoreSkills(progression);
                }
            }
        }
    }
}