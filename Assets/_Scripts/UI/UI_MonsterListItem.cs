using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_MonsterListItem : MonoBehaviour
    {
        [Header("Informações")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _qualityText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _locationText;

        [Header("Experiência")]
        [SerializeField] private Slider _xpSlider;
        [SerializeField] private TextMeshProUGUI _xpText;

        [Header("Interação")]
        [SerializeField] private Button _itemButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private GameObject _equippedTag;
        [SerializeField] private GameObject _selectionHighlight;

        private MonsterInstance _instance;
        private MonsterData _data;

        private Action<MonsterInstance> _onSelected;
        private Action<MonsterInstance> _onInfo;

        public void Setup(
            MonsterInstance instance,
            MonsterDatabase database,
            Action<MonsterInstance> onSelected,
            Action<MonsterInstance> onInfo = null)
        {
            RemoveListeners();

            _instance = instance;
            _data = instance != null ? instance.GetData(database) : null;

            _onSelected = onSelected;
            _onInfo = onInfo;

            bool valid = _instance != null && _data != null;

            if (_itemButton != null)
            {
                _itemButton.interactable = valid;
                _itemButton.onClick.AddListener(HandleSelected);
            }

            if (_infoButton != null)
            {
                _infoButton.interactable = valid && _onInfo != null;
                _infoButton.onClick.AddListener(HandleInfo);
            }

            if (_xpSlider != null)
            {
                _xpSlider.minValue = 0f;
                _xpSlider.maxValue = 1f;
                _xpSlider.wholeNumbers = false;
                _xpSlider.interactable = false;
            }

            SetSelected(false);
            Refresh();
        }

        // Chame novamente quando a progressão ou a lane mudar.
        public void Refresh()
        {
            bool valid = _instance != null && _data != null;

            if (_iconImage != null)
            {
                _iconImage.sprite = valid ? _data.icon : null;
                _iconImage.enabled = _iconImage.sprite != null;
            }

            if (_nameText != null)
                _nameText.text = valid ? _data.displayName : "Unavailable";

            if (_qualityText != null)
                _qualityText.text = valid ? _instance.quality.ToString() : "";

            var progression = _instance != null ? _instance.progression : null;
            int level = progression != null ? progression.currentLevel : 1;
            int xp = progression != null ? progression.currentXP : 0;

            if (_levelText != null)
                _levelText.text = valid ? $"Lv. {level}" : "";

            bool isMaxLevel = valid && level >= GetMaxLevel(_instance.quality);
            int requiredXP = valid
                ? Mathf.Max(1, _data.GetXPRequired(level + 1))
                : 1;

            if (_xpSlider != null)
            {
                float progress = !valid ? 0f
                    : isMaxLevel ? 1f
                    : Mathf.Clamp01((float)xp / requiredXP);

                _xpSlider.SetValueWithoutNotify(progress);
            }

            if (_xpText != null)
            {
                _xpText.text = !valid ? ""
                    : isMaxLevel ? "MAX LEVEL"
                    : $"{xp} / {requiredXP} XP";
            }

            RefreshLocation();
        }

        public void RefreshLocation()
        {
            MonsterSlot equippedSlot = null;

            if (_instance != null)
            {
                var slots = FindObjectsByType<MonsterSlot>(
                    FindObjectsInactive.Exclude);

                foreach (var slot in slots)
                {
                    if (slot.EquippedInstance != null &&
                        slot.EquippedInstance.instanceID == _instance.instanceID)
                    {
                        equippedSlot = slot;
                        break;
                    }
                }
            }

            bool isEquipped = equippedSlot != null;

            if (_equippedTag != null)
                _equippedTag.SetActive(isEquipped);

            if (_locationText != null)
            {
                _locationText.text = _instance == null ? ""
                    : isEquipped ? equippedSlot.gameObject.name
                    : "Available";
            }
        }

        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);
        }

        private void HandleSelected()
        {
            if (_instance != null && _data != null)
                _onSelected?.Invoke(_instance);
        }

        private void HandleInfo()
        {
            if (_instance != null && _data != null)
                _onInfo?.Invoke(_instance);
        }

        private void RemoveListeners()
        {
            if (_itemButton != null)
                _itemButton.onClick.RemoveListener(HandleSelected);

            if (_infoButton != null)
                _infoButton.onClick.RemoveListener(HandleInfo);
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private static int GetMaxLevel(MonsterQuality quality)
        {
            switch (quality)
            {
                case MonsterQuality.Common: return 10;
                case MonsterQuality.Uncommon: return 20;
                case MonsterQuality.Rare: return 30;
                case MonsterQuality.Epic: return 40;
                case MonsterQuality.Legendary: return 50;
                default: return 10;
            }
        }
    }
}