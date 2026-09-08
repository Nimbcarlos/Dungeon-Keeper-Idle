using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Banco de Dados")]
        [SerializeField] private MonsterDatabase _database;

        [Header("Prototipo / Demonstracao")]
        [Tooltip("Cria uma copia de cada especie somente quando a colecao esta vazia. Desative para a economia final.")]
        [SerializeField] private bool _seedDemoMonstersWhenEmpty = true;

        public void EnsureDemoMonsters()
        {
            if (!_seedDemoMonstersWhenEmpty || _ownedInstances.Count > 0 || _database == null) return;
            var ids = new HashSet<string>();
            foreach (var data in _database.AllMonsters)
            {
                if (data == null || string.IsNullOrEmpty(data.id) || data.prefab == null || !ids.Add(data.id)) continue;
                _ownedInstances.Add(new MonsterInstance(data.id, MonsterQuality.Common));
            }
            if (_ownedInstances.Count > 0) OnInventoryChanged?.Invoke();
        }

        // Lista de instâncias vivas que o jogador possui
        private List<MonsterInstance> _ownedInstances = new List<MonsterInstance>();

        public IReadOnlyList<MonsterInstance> OwnedInstances => _ownedInstances;
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Adiciona uma nova instância de monstro vinda do Hatchery (Chocadeira/Invocação)
        /// </summary>
        public void AddMonsterInstance(MonsterInstance newInstance)
        {
            if (newInstance == null) return;

            _ownedInstances.Add(newInstance);
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"🎉 Nova instância do monstro '{newInstance.monsterDataID}' ({newInstance.quality}) adicionada ao inventário!");
        }

        /// <summary>
        /// Retorna todas as instâncias de uma determinada espécie (ex: todos os Green Slimes do jogador)
        /// </summary>
        public List<MonsterInstance> GetInstancesByDataID(string monsterDataID)
        {
            return _ownedInstances.FindAll(m => m != null && m.monsterDataID == monsterDataID);
        }

        /// <summary>
        /// REGRA DE EQUIPAR COM AUTO-SWAP usando a nova arquitetura de MonsterInstance
        /// </summary>
        public void RequestEquipMonster(MonsterSlot targetSlot, MonsterInstance instanceToEquip)
        {
            if (targetSlot == null || instanceToEquip == null) return;

            // 1. Se este MESMO slot já tem esta instância, desequipa (Auto-Toggle)
            if (targetSlot.EquippedInstance == instanceToEquip)
            {
                RequestUnequipMonster(targetSlot);
                return;
            }

            // 2. Se a mesma instância já estiver equipada em OUTRO slot, limpa a lane antiga primeiro (Anti-Duplicação)
            MonsterSlot[] allSlots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            foreach (var slot in allSlots)
            {
                if (slot != targetSlot && slot.EquippedInstance == instanceToEquip)
                {
                    slot.ClearSlot();
                    break;
                }
            }

            // 3. Equipa a instância no slot solicitado
            targetSlot.EquipMonster(instanceToEquip);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Substitui a lista de instâncias do inventário pelas instâncias vindas do Save Data.
        /// </summary>
        public void LoadSavedInstances(List<MonsterInstance> loadedInstances)
        {
            _ownedInstances = loadedInstances ?? new List<MonsterInstance>();
            EnsureDemoMonsters();
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"💾 [Inventory] Carregadas {_ownedInstances.Count} instâncias de monstros do save!");
        }

        public void RequestUnequipMonster(MonsterSlot targetSlot)
        {
            if (targetSlot == null || !targetSlot.HasMonsterEquipped) return;

            targetSlot.ClearSlot();
            OnInventoryChanged?.Invoke();
        }

        public MonsterDatabase GetDatabase() => _database;
    }
}