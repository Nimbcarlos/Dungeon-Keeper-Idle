using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSlot : MonoBehaviour
    {
        [SerializeField] private int _slotIndex; // 0, 1 ou 2
        [SerializeField] private float _respawnDelay = 3f;

        private MonsterData _assignedData;
        private Monster _currentMonster;
        private Coroutine _respawnRoutine;

        public int SlotIndex => _slotIndex;
        public bool HasMonsterAssigned => _assignedData != null;

        /// <summary>
        /// Aloca um novo monstro a este slot (vindo do inventário/casulo)
        /// </summary>
        public void AssignMonster(MonsterData data)
        {
            _assignedData = data;

            // Se trocou de monstro enquanto havia um vivo, limpa o antigo
            if (_currentMonster != null)
            {
                Destroy(_currentMonster.gameObject);
            }

            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
            }

            SpawnMonster();
        }

        /// <summary>
        /// Remove o monstro alocado (manda de volta para a reserva ou limpa)
        /// </summary>
        public void ClearSlot()
        {
            _assignedData = null;
            if (_currentMonster != null)
            {
                Destroy(_currentMonster.gameObject);
            }
        }

        private void SpawnMonster()
        {
            if (_assignedData == null || _assignedData.prefab == null) return;

            GameObject obj = Instantiate(_assignedData.prefab, transform.position, Quaternion.identity);
            _currentMonster = obj.GetComponent<Monster>();

            if (_currentMonster != null)
            {
                _currentMonster.Initialize(_assignedData);

                if (_currentMonster.Health != null)
                {
                    _currentMonster.Health.OnDeath += OnMonsterDied;
                }

                AssignBrain(obj, _assignedData.defaultBehavior);
            }
        }

        private void OnMonsterDied()
        {
            _respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(_respawnDelay);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                yield break;

            // O monstro renasce no mesmo slot sem custo adicional!
            if (_assignedData != null)
            {
                SpawnMonster();
            }
        }

        private void AssignBrain(GameObject obj, MonsterBehavior behavior)
        {
            switch (behavior)
            {
                case MonsterBehavior.Defensive:
                    if (obj.GetComponent<MonsterBrain>() == null)
                        obj.AddComponent<MonsterBrain>();
                    break;
            }
        }
    }
}