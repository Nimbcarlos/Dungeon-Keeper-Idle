using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class UI_InventoryPanel : MonoBehaviour
    {
        public static UI_InventoryPanel Instance { get; private set; }

        [Header("Referências de UI")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Transform _gridContentParent;
        [SerializeField] private GameObject _monsterCardPrefab;

        private MonsterSlot _currentSelectedSlot;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;

            ClosePanel();
        }

        /// <summary>
        /// Abre a janela do inventário focada em equipar um Slot específico
        /// </summary>
        public void OpenForSlot(MonsterSlot targetSlot)
        {
            _currentSelectedSlot = targetSlot;
            _panelContainer.SetActive(true);
            RefreshGrid();
        }

        public void ClosePanel()
        {
            _panelContainer.SetActive(false);
            _currentSelectedSlot = null;
        }

        private void RefreshGrid()
        {
            foreach (Transform child in _gridContentParent)
            {
                Destroy(child.gameObject);
            }

            foreach (MonsterData monster in InventoryManager.Instance.AllDiscoveredMonsters)
            {
                int availableCount = InventoryManager.Instance.GetAvailableCount(monster);

                // Se o jogador não tem nenhum disponível no estoque, pula ou desabilita o card!
                if (availableCount <= 0) continue;

                GameObject cardObj = Instantiate(_monsterCardPrefab, _gridContentParent);
                UI_MonsterCard card = cardObj.GetComponent<UI_MonsterCard>();

                // Exibe a quantidade disponível no card (Ex: "Green Slime x2")
                card.Setup(monster, availableCount, OnMonsterCardClicked);
            }
        }

        private void OnMonsterCardClicked(MonsterData selectedMonster)
        {
            if (_currentSelectedSlot != null)
            {
                _currentSelectedSlot.EquipMonster(selectedMonster);
            }

            ClosePanel();
        }
    }
}