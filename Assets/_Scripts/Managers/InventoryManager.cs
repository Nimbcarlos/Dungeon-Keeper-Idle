using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonKeeper
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Slots Ativos na Sala (Máximo 3)")]
        [SerializeField] private List<MonsterSlot> _activeSlots = new List<MonsterSlot>();

        [Header("Configuração da Reserva")]
        [SerializeField] private int _initialReserveSlots = 5;
        [SerializeField] private int _maxReserveSlotsAllowed = 10;

        // Lista de dados dos monstros na reserva
        private List<MonsterData> _reserveMonsters = new List<MonsterData>();

        public int CurrentReserveCapacity { get; private set; }
        public int ReserveCount => _reserveMonsters.Count;

        public event Action OnInventoryUpdated;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            CurrentReserveCapacity = _initialReserveSlots;
        }

        /// <summary>
        /// Adiciona um monstro recém-chocado do Casulo para a Reserva
        /// </summary>
        public bool AddToReserve(MonsterData newMonster)
        {
            if (_reserveMonsters.Count >= CurrentReserveCapacity)
            {
                Debug.LogWarning("Inventário de Reserva Cheio!");
                return false;
            }

            _reserveMonsters.Add(newMonster);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// Equipar/Trocar um monstro da Reserva para um Slot Ativo da Sala
        /// </summary>
        public void EquipToActiveSlot(int reserveIndex, int activeSlotIndex)
        {
            if (reserveIndex < 0 || reserveIndex >= _reserveMonsters.Count) return;
            if (activeSlotIndex < 0 || activeSlotIndex >= _activeSlots.Count) return;

            MonsterData dataToEquip = _reserveMonsters[reserveIndex];

            // Se o slot ativo já tinha um monstro, o antigo volta para a reserva
            _activeSlots[activeSlotIndex].AssignMonster(dataToEquip);
            _reserveMonsters.RemoveAt(reserveIndex);

            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Expande a capacidade de slots da reserva (Comprado via Gold/Monetização)
        /// </summary>
        public bool ExpandReserveSlots(int amount = 1)
        {
            if (CurrentReserveCapacity >= _maxReserveSlotsAllowed) return false;

            CurrentReserveCapacity = Mathf.Min(CurrentReserveCapacity + amount, _maxReserveSlotsAllowed);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public List<MonsterData> GetReserveMonsters() => _reserveMonsters;
    }
}