using UnityEngine;
using System;

namespace DungeonKeeper
{
    public class Treasure : MonoBehaviour
    {
        [Header("Gold")]
        [SerializeField] private int   _startingGold    = 100;
        [SerializeField] private int   _minGold         = 10;  // nunca zera
        [SerializeField] private float _regenRate       = 1f;  // gold por segundo
        [SerializeField] private int   _regenAmount     = 1;

        public int  Gold        { get; private set; }
        public bool IsEmpty     => Gold <= _minGold;

        public event Action<int> OnGoldChanged;
        public event Action      OnTreasureSacked; // herói saqueou

        private float _regenTimer;

        void Start()
        {
            Gold = _startingGold;
            ResourceManager.Instance.SetGold(Gold);
        }

        void Update()
        {
            // regenera passivamente
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= _regenRate)
            {
                _regenTimer = 0f;
                AddGold(_regenAmount);
            }
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            ResourceManager.Instance.SetGold(Gold);
            OnGoldChanged?.Invoke(Gold);
            if (Gold % 10 == 0)
            {
                Debug.Log($"Tesouro: +{amount} Gold (total: {Gold})");
            }
        }

        // herói chegou e saqueou
        public void Sack(int amount)
        {
            Debug.Log($"Sack chamado — disparando OnTreasureSacked");
            Gold = Mathf.Max(_minGold, Gold - amount);
            ResourceManager.Instance.SetGold(Gold);
            OnGoldChanged?.Invoke(Gold);
            OnTreasureSacked?.Invoke();
            Debug.Log($"Tesouro saqueado: -{amount} Gold (total: {Gold})");
        }
    }
}