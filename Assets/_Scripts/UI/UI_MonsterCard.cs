using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DungeonKeeper
{
    public class UI_MonsterCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _cardButton;

        private MonsterInstance _instance;

        public void Setup(MonsterInstance instance, MonsterDatabase database, Action onClickCallback)
        {
            _instance = instance;

            if (instance == null) return;

            // Busca os dados visuais (ícone e nome) no ScriptableObject através do Database
            MonsterData data = instance.GetData(database);
            if (data != null)
            {
                if (_nameText != null) _nameText.text = data.displayName;
                if (_iconImage != null) _iconImage.sprite = data.icon;
            }

            // Exibe o nível individual da instância
            if (_levelText != null && instance.progression != null)
            {
                _levelText.text = $"Lv. {instance.progression.currentLevel}";
            }

            // Configura a ação de clique do card
            if (_cardButton != null)
            {
                _cardButton.onClick.RemoveAllListeners();
                _cardButton.onClick.AddListener(() => onClickCallback?.Invoke());
            }
        }
    }
}