using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace DungeonKeeper
{
    public class UI_MonsterStatsPanel : MonoBehaviour
    {
        public enum Attribute
        {
            MaxHP, AttackPower, MoveSpeed, AttackSpeed, AttackRange, DetectionRange
        }

        [Serializable]
        private class Row
        {
            public Attribute attribute;
            public TextMeshProUGUI valueText;
            public string numberFormat = "0.##";
        }

        [SerializeField] private List<Row> _rows = new();
        private bool _readableLayout;

        public void UseReadableLayout()
        {
            if (_readableLayout || _rows.Count == 0) return;
            _readableLayout = true;
            foreach (var layout in GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true)) layout.enabled = false;
            int index = 0;
            foreach (var row in _rows)
            {
                if (row == null || row.valueText == null) continue;
                Transform rowRoot = row.valueText.transform;
                while (rowRoot.parent != null && rowRoot.parent != transform) rowRoot = rowRoot.parent;
                var labels = rowRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var label in labels)
                {
                    bool value = label == row.valueText;
                    var rect = label.rectTransform;
                    rect.SetParent(transform, false);
                    rect.anchorMin = new Vector2(value ? .66f : 0, 1f - (index + 1f) / _rows.Count);
                    rect.anchorMax = new Vector2(value ? 1 : .64f, 1f - index / (float)_rows.Count);
                    rect.offsetMin = new Vector2(8, 4);
                    rect.offsetMax = new Vector2(-8, -4);
                    rect.localScale = Vector3.one;
                    label.enableAutoSizing = false;
                    label.fontSize = 36;
                    label.alignment = value ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
                    label.overflowMode = TextOverflowModes.Ellipsis;
                    label.raycastTarget = false;
                }
                if (rowRoot != transform && rowRoot != row.valueText.transform) rowRoot.gameObject.SetActive(false);
                index++;
            }
        }

        public void Display(Stats stats)
        {
            foreach (var row in _rows)
            {
                if (row == null || row.valueText == null) continue;
                if (stats == null)
                {
                    row.valueText.text = string.Empty;
                    continue;
                }

                float value = ReadValue(stats, row.attribute);
                try
                {
                    row.valueText.text = value.ToString(
                        row.numberFormat, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    row.valueText.text = value.ToString(CultureInfo.CurrentCulture);
                    Debug.LogWarning("Formato numerico invalido na ficha de atributos.", this);
                }
            }
        }

        private static float ReadValue(Stats stats, Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.MaxHP: return stats.maxHP;
                case Attribute.AttackPower: return stats.attackPower;
                case Attribute.MoveSpeed: return stats.moveSpeed;
                case Attribute.AttackSpeed: return stats.attackSpeed;
                case Attribute.AttackRange: return stats.attackRange;
                case Attribute.DetectionRange: return stats.detectionRange;
                default: throw new ArgumentOutOfRangeException(nameof(attribute));
            }
        }
    }
}
