using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Todos os Monstros Existentes no Jogo")]
        [SerializeField] private List<MonsterData> _allGameMonsters = new List<MonsterData>();

        // Mapeia: MonsterData -> Quantidade Desbloqueada/Possuída pelo jogador
        private Dictionary<MonsterData, int> _unlockedMonsters = new Dictionary<MonsterData, int>();

        public IReadOnlyList<MonsterData> AllGameMonsters => _allGameMonsters;
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // INICIALIZAÇÃO DE TESTE: Libera 1 cópia de cada monstro por padrão
            foreach (var monster in _allGameMonsters)
            {
                _unlockedMonsters[monster] = 1;
            }
        }

        /// <summary>
        /// MÉTODO DO SUMMON: Chama quando o jogador ganha/invoca um novo monstro!
        /// </summary>
        public void UnlockOrAddMonster(MonsterData monster, int amount = 1)
        {
            if (monster == null) return;

            if (_unlockedMonsters.ContainsKey(monster))
                _unlockedMonsters[monster] += amount;
            else
                _unlockedMonsters[monster] = amount;

            OnInventoryChanged?.Invoke();
            Debug.Log($"🎉 Monstro {monster.displayName} liberado! Quantidade total: {_unlockedMonsters[monster]}");
        }

        /// <summary>
        /// Retorna a quantidade TOTAL que o jogador possui desse monstro
        /// </summary>
        public int GetOwnedCount(MonsterData monster)
        {
            if (monster == null || !_unlockedMonsters.ContainsKey(monster)) return 0;
            return _unlockedMonsters[monster];
        }

        /// <summary>
        /// Retorna quantas cópias desse monstro estão equipadas atualmente nas lanes
        /// </summary>
        public int GetEquippedCount(MonsterData monster)
        {
            int count = 0;
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            foreach (var slot in slots)
            {
                if (slot.EquippedMonsterData == monster)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Retorna apenas a lista de ScriptableObjects que o jogador realmente possui no Save (Quantidade > 0)
        /// </summary>
        public List<MonsterData> GetUnlockedMonstersList()
        {
            List<MonsterData> unlocked = new List<MonsterData>();

            foreach (var entry in _unlockedMonsters)
            {
                if (entry.Value > 0 && entry.Key != null)
                {
                    unlocked.Add(entry.Key);
                }
            }

            return unlocked;
        }

        /// <summary>
        /// REGRA DE EQUIPAR COM AUTO-SWAP
        /// </summary>
        public void RequestEquipMonster(MonsterSlot targetSlot, MonsterData monster)
        {
            if (targetSlot == null || monster == null) return;

            int owned = GetOwnedCount(monster);
            if (owned <= 0) return; // Jogador ainda não desbloqueou este monstro

            int currentlyEquipped = GetEquippedCount(monster);

            // Se o monstro já está na mesma lane que tentamos equipar, não faz nada
            if (targetSlot.EquippedMonsterData == monster) return;

            // Se já atingimos o limite de cópias que o jogador possui,
            // precisamos remover a cópia de outra lane antes de colocar na nova!
            if (currentlyEquipped >= owned)
            {
                MonsterSlot[] allSlots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
                foreach (var slot in allSlots)
                {
                    if (slot != targetSlot && slot.EquippedMonsterData == monster)
                    {
                        slot.ClearSlot(); // Apaga a lane anterior!
                        break;
                    }
                }
            }

            // Equipar na nova lane
            targetSlot.EquipMonster(monster);
            OnInventoryChanged?.Invoke();
        }
    }
}