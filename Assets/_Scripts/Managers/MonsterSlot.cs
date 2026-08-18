using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSlot : MonoBehaviour
    {
        [Header("Configurações do Slot")]
        [SerializeField] private GameObject _highlightObject;
        [SerializeField] private Transform _spawnPoint;
        
        [Header("Configurações de Respawn")]
        [SerializeField] private float _respawnDelay = 5f; // Tempo em segundos para o monstro renascer

        public MonsterData EquippedMonsterData { get; private set; }
        private GameObject _spawnedMonsterInstance;
        private Coroutine _respawnCoroutine;

        public bool HasMonsterEquipped => EquippedMonsterData != null;

        private void OnMouseDown()
        {
            OnClickSlot();
        }

        public void OnClickSlot()
        {
            if (UI_MonsterInventoryWindow.Instance != null && UI_MonsterInventoryWindow.Instance.IsOpen)
            {
                UI_MonsterInventoryWindow.Instance.AssignSelectedMonsterToLane(this);
            }
        }

        public void SetHighlightVisible(bool visible)
        {
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Equipa um novo monstro e o instancia imediatamente
        /// </summary>
        public void EquipMonster(MonsterData data)
        {
            ClearSlot();

            EquippedMonsterData = data;

            // Instancia o monstro sem delay na primeira vez que equipa
            SpawnEquippedMonster();
        }

        /// <summary>
        /// Inicia o timer para renascer o monstro após X segundos (chame quando o monstro morrer)
        /// </summary>
        public void ScheduleRespawn()
        {
            if (EquippedMonsterData == null) return;

            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
            }

            _respawnCoroutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);
            
            SpawnEquippedMonster();
            _respawnCoroutine = null;
        }

        /// <summary>
        /// Método de Spawn: Instancia a entidade no jogo
        /// </summary>
        public void SpawnEquippedMonster()
        {
            if (_spawnedMonsterInstance != null)
            {
                Destroy(_spawnedMonsterInstance);
                _spawnedMonsterInstance = null;
            }

            if (EquippedMonsterData == null || EquippedMonsterData.prefab == null) return;

            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            _spawnedMonsterInstance = Instantiate(EquippedMonsterData.prefab, spawnPos, Quaternion.identity);

            Monster monsterComp = _spawnedMonsterInstance.GetComponent<Monster>();
            if (monsterComp != null)
            {
                monsterComp.Initialize(EquippedMonsterData);

                // ← registra o respawn quando o monstro morrer
                monsterComp.Health.OnDeath += () => ScheduleRespawn();
            }
        }


        /// <summary>
        /// Limpa o monstro do slot e cancela qualquer respawn pendente
        /// </summary>
        public void ClearSlot()
        {
            if (_respawnCoroutine != null)
            {
                Debug.Log($"🗑️ Cancelando respawn do monstro {EquippedMonsterData.displayName} na lane.");
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }

            if (_spawnedMonsterInstance != null)
            {
                Debug.Log($"🗑️ Monstro {EquippedMonsterData.displayName} removido da lane.");
                Destroy(_spawnedMonsterInstance);
                _spawnedMonsterInstance = null;
            }

            EquippedMonsterData = null;
        }
    }
}