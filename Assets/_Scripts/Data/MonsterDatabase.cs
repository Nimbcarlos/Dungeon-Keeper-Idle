using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Dungeon/Monster Database")]
    public class MonsterDatabase : ScriptableObject
    {
        [Header("Lista Global de Monstros do Jogo")]
        [SerializeField] private List<MonsterData> _allMonsters = new List<MonsterData>();

        private Dictionary<string, MonsterData> _databaseLookup;

        /// <summary>
        /// Mapeia a lista para um Dicionário para buscas rápidas por ID O(1)
        /// </summary>
        public void InitializeDatabase()
        {
            _databaseLookup = new Dictionary<string, MonsterData>();

            foreach (MonsterData monster in _allMonsters)
            {
                if (monster != null && !string.IsNullOrEmpty(monster.id))
                {
                    if (!_databaseLookup.ContainsKey(monster.id))
                    {
                        _databaseLookup.Add(monster.id, monster);
                    }
                    else
                    {
                        Debug.LogError($"[MonsterDatabase] ID duplicado encontrado: '{monster.id}' no asset {monster.name}!");
                    }
                }
            }
        }

        /// <summary>
        /// Recebe o ID gravado no Save ("green_slime") e devolve o Asset ScriptableObject
        /// </summary>
        public MonsterData GetMonsterDataByID(string id)
        {
            if (_databaseLookup == null)
            {
                InitializeDatabase();
            }

            if (_databaseLookup.TryGetValue(id, out MonsterData data))
            {
                return data;
            }

            Debug.LogError($"[MonsterDatabase] Monstro com ID '{id}' não foi encontrado no Database!");
            return null;
        }
    }
}