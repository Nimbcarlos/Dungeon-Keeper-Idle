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
