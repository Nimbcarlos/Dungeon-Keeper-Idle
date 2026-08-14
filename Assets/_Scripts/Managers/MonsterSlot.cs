using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class MonsterSlot : MonoBehaviour
    {
        [Header("Configurações do Slot")]
        [SerializeField] private Button _slotButton;
        [SerializeField] private SpriteRenderer _slotWorldPreview;
        [SerializeField] private GameObject _highlightObject; // Objeto/Glow visual de highlight do slot
        [SerializeField] private Transform _spawnPoint;

        public MonsterData EquippedMonsterData { get; private set; }
        private GameObject _spawnedMonsterInstance;

        // Propriedade booleana para checar se o slot tem um monstro
        public bool HasMonsterEquipped => EquippedMonsterData != null;

        private void Awake()
        {
            if (_slotButton != null)
                _slotButton.onClick.AddListener(OnClickSlot);

            // Garante que o highlight comece desligado
            SetHighlightVisible(false);
        }

        private void OnClickSlot()
        {
            UI_InventoryPanel.Instance?.OpenForSlot(this);
        }

        /// <summary>
        /// Ativa ou desativa o destaque visual do slot
        /// </summary>
        public void SetHighlightVisible(bool visible)
        {
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(visible);
            }
        }

        public void EquipMonster(MonsterData data)
        {
            UnequipCurrentMonster();

            EquippedMonsterData = data;

            if (_slotWorldPreview != null && data != null && data.icon != null)
            {
                _slotWorldPreview.sprite = data.icon;
            }

            SpawnEquippedMonster();
        }

        public void ApplyEquippedMonster(MonsterData data) => EquipMonster(data);

        public void ClearSlot() => UnequipCurrentMonster();

        public void UnequipCurrentMonster()
        {
            if (_spawnedMonsterInstance != null)
            {
                Destroy(_spawnedMonsterInstance);
                _spawnedMonsterInstance = null;
            }

            EquippedMonsterData = null;

            if (_slotWorldPreview != null)
                _slotWorldPreview.sprite = null;
        }

        private void SpawnEquippedMonster()
        {
            if (EquippedMonsterData == null || EquippedMonsterData.prefab == null) return;

            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            _spawnedMonsterInstance = Instantiate(EquippedMonsterData.prefab, spawnPos, Quaternion.identity);

            Monster monsterComp = _spawnedMonsterInstance.GetComponent<Monster>();
            if (monsterComp != null)
            {
                monsterComp.Initialize(EquippedMonsterData);
            }
        }
    }
}