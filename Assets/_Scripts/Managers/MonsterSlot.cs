using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSlot : MonoBehaviour
    {
        [Header("Configurações da Lane")]
        [SerializeField] private GameObject _highlightObject;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _respawnDelay = 5f;
        [SerializeField] private MonsterDatabase _database;

        public MonsterInstance EquippedInstance { get; private set; }
        private GameObject _spawnedMonsterInstance;
        private Coroutine _respawnCoroutine;

        public bool HasMonsterEquipped => EquippedInstance != null;

        public void SetHighlightVisible(bool visible)
        {
            if (_highlightObject != null)
                _highlightObject.SetActive(visible);
        }

        public void EquipMonster(MonsterInstance instance)
        {
            ClearSlot();
            EquippedInstance = instance;
            Spawn();
        }

        private void Spawn()
        {
            if (EquippedInstance == null) return;

            MonsterData data = EquippedInstance.GetData(_database);
            if (data == null || data.prefab == null) return;

            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            _spawnedMonsterInstance = Instantiate(data.prefab, spawnPos, Quaternion.identity);

            Monster monster = _spawnedMonsterInstance.GetComponent<Monster>();
            if (monster != null)
            {
                // Injeta a instância viva e o database no monstro
                monster.InitializeMonster(EquippedInstance, _database);
                monster.Health.OnDeath += () => ScheduleRespawn();
            }
        }

        public void ScheduleRespawn()
        {
            if (EquippedInstance == null) return;

            if (_respawnCoroutine != null) StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);
            Spawn();
            _respawnCoroutine = null;
        }

        public void ClearSlot()
        {
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }

            if (_spawnedMonsterInstance != null)
            {
                Destroy(_spawnedMonsterInstance);
                _spawnedMonsterInstance = null;
            }

            EquippedInstance = null;
        }

        /// <summary>
        /// Devolve o componente Monster ativo na lane
        /// </summary>
        public Monster GetSpawnedMonster()
        {
            if (_spawnedMonsterInstance == null) return null;
            return _spawnedMonsterInstance.GetComponent<Monster>();
        }
    }
}