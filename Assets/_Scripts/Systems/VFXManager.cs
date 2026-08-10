using UnityEngine;
using System.Collections.Generic;


namespace DungeonKeeper
{
    public enum VFXType
    {
        Explosion,
        Poison,
        Fire,
        Ice,
        Heal,
        Spark,
        Smoke,
        Lightning
    }

    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [System.Serializable]
        public class VFXEntry
        {
            public VFXType    type;
            public GameObject prefab;
        }

        [SerializeField] private List<VFXEntry> _effects;

        private Dictionary<VFXType, GameObject> _map;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _map = new Dictionary<VFXType, GameObject>();
            foreach (var entry in _effects)
                _map[entry.type] = entry.prefab;
        }

        public void Play(VFXType type, Vector3 position)
        {
            if (!_map.ContainsKey(type)) return;
            Instantiate(_map[type], position, Quaternion.identity);
        }
    }
}