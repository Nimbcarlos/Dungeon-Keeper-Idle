using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DungeonKeeper
{
    public class UI_SummonResultWindow : MonoBehaviour
    {
        public GameObject panel;
        public Image portrait;
        public TMP_Text nameText, qualityText, affixesText;
        public Button closeButton, viewButton;
        public UI_CollectionWindow collection;
        public UI_MonsterCollectionPage monstersPage;
        private MonsterInstance _monster;
        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            viewButton.onClick.AddListener(ViewMonster);
            panel.SetActive(false);
        }
        public void Show(MonsterInstance monster)
        {
            _monster = monster;
            var data = monster.GetData(InventoryManager.Instance.GetDatabase());
            portrait.sprite = data.icon; portrait.preserveAspect = true;
            nameText.text = data.displayName;
            qualityText.text = monster.quality + "  •  Lv. " + monster.progression.currentLevel;
            affixesText.text = "Added to your collection\n\n" + string.Join("\n", monster.affixes.Select(a => a.type + "  +" + a.value.ToString("0.##"))) +
                (monster.behaviors.Count == 0 ? "" : "\n" + string.Join(", ", monster.behaviors));
            UI_WindowCoordinator.Instance.RegisterWindow(this);
            panel.SetActive(true); panel.transform.SetAsLastSibling();
        }
        public void Close()
        {
            if (panel != null) panel.SetActive(false);
            if (UI_WindowCoordinator.Instance != null) UI_WindowCoordinator.Instance.UnregisterWindow(this);
        }
        private void ViewMonster()
        {
            collection.OpenWindow();
            collection.ShowMonsters();
            monstersPage.SelectMonster(_monster);
            Close();
        }
        private void OnDisable() { Close(); }
        private void OnDestroy()
        {
            Close();
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (viewButton != null) viewButton.onClick.RemoveListener(ViewMonster);
        }
    }
}