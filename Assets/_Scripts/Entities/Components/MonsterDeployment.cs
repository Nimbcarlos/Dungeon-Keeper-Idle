using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterDeployment : MonoBehaviour
    {
        public static MonsterDeployment Instance { get; private set; }

        // Mapeia o índice da Lane para a MonsterInstance atribuída
        private Dictionary<int, string> _laneDeployments = new Dictionary<int, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsMonsterDeployed(string instanceID)
        {
            return _laneDeployments.ContainsValue(instanceID);
        }

        public bool AssignToLane(int laneIndex, MonsterInstance instance, MonsterSlot targetSlot)
        {
            if (instance == null || targetSlot == null) return false;

            // 🛑 Checagem Anti-Duplicação: Se o monstro já estiver em OUTRA lane, remove da lane antiga primeiro!
            foreach (var kvp in new Dictionary<int, string>(_laneDeployments))
            {
                if (kvp.Value == instance.instanceID)
                {
                    _laneDeployments.Remove(kvp.Key);
                    // Avisa a outra lane para limpar se necessário
                }
            }

            _laneDeployments[laneIndex] = instance.instanceID;
            targetSlot.EquipMonster(instance);

            Debug.Log($"⚔️ [Deployment] Monstro {instance.monsterDataID} atribuído à Lane {laneIndex}");
            return true;
        }

        public void UnequipLane(int laneIndex, MonsterSlot slot)
        {
            if (_laneDeployments.ContainsKey(laneIndex))
            {
                _laneDeployments.Remove(laneIndex);
            }
            if (slot != null) slot.ClearSlot();
        }
    }
}