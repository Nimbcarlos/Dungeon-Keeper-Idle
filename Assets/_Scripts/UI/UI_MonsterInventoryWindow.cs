using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_MonsterInventoryWindow : MonoBehaviour
    {
        public static UI_MonsterInventoryWindow Instance { get; private set; }

        [Header("Painel Principal")]
        [SerializeField] private GameObject _windowPanel;

        [Header("Contêiner da Lista de Monstros")]
        [SerializeField] private Transform _monsterListContainer;
        [SerializeField] private GameObject _monsterCardPrefab;

        [Header("Painel de Detalhes")]
        [SerializeField] private TextMeshProUGUI _monsterNameText;
        [SerializeField] private TextMeshProUGUI _levelXPText;
        [SerializeField] private TextMeshProUGUI _statsSummaryText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _openSkillTreeButton;

        private bool _ownsPause;
        private float _previousTimeScale;

        public void OpenWindow() => OpenWindowForLane(null);
        public void ToggleWindow()
        {
            if (IsOpen) CloseWindow();
            else OpenWindow();
        }

        private void RestorePause()
        {
            if (!_ownsPause) return;
            Time.timeScale = _previousTimeScale;
            _ownsPause = false;
        }

        private void OnDisable()
        {
            RestorePause();
            HighlightLanes(false);
        }

        private MonsterInstance _selectedInstance;
        private MonsterSlot _targetLaneSlot;
        private List<GameObject> _instantiatedCards = new List<GameObject>();

        public bool IsOpen => _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_windowPanel != null) _windowPanel.SetActive(false);
        }

        public void OpenWindowForLane(MonsterSlot slot)
        {
            if (_windowPanel == null)
            {
                Debug.LogError("[Equip] Atribua Window Panel no Inspector.", this);
                return;
            }
            _targetLaneSlot = slot;
            _windowPanel.SetActive(true);
            if (!_ownsPause)
            {
                _previousTimeScale = Time.timeScale;
                _ownsPause = true;
            }
            Time.timeScale = 0f;

            HighlightLanes(true);
            RefreshInventoryList();
        }

        public void CloseWindow()
        {
            RestorePause();
            HighlightLanes(false);
            _targetLaneSlot = null;
            _selectedInstance = null;

            if (_windowPanel != null) _windowPanel.SetActive(false);
        }

        public void RefreshInventoryList()
        {
            // Limpa cards antigos da UI
            foreach (var card in _instantiatedCards)
            {
                if (card != null) Destroy(card);
            }
            _instantiatedCards.Clear();

            if (InventoryManager.Instance == null) return;

            if (_monsterListContainer == null || _monsterCardPrefab == null)
            {
                Debug.LogError("[Equip] Atribua o container da lista e o prefab no Inspector.", this);
                return;
            }
            InventoryManager.Instance.EnsureDemoMonsters();

            // 🎯 Lê a lista de instâncias vivas possuídas pelo jogador
            IReadOnlyList<MonsterInstance> ownedInstances = InventoryManager.Instance.OwnedInstances;
            MonsterDatabase database = InventoryManager.Instance.GetDatabase();

            foreach (MonsterInstance instance in ownedInstances)
            {
                if (instance == null) continue;

                GameObject cardObj = Instantiate(_monsterCardPrefab, _monsterListContainer);
                _instantiatedCards.Add(cardObj);

                MonsterData data = instance.GetData(database);
                if (data == null) { Destroy(cardObj); continue; }

                // Configura o card (se você tiver um script de card UI dedicado)
                UI_MonsterCard cardScript = cardObj.GetComponent<UI_MonsterCard>();
                if (cardScript != null)
                {
                    cardScript.Setup(instance, database, () => OnSelectMonsterInstance(instance));
                }
                else
                {
                    var listItem = cardObj.GetComponent<UI_MonsterListItem>();
                    if (listItem != null)
                        listItem.Setup(
                            instance,
                            database,
                            OnSelectMonsterInstance
                        );
                    else
                        Debug.LogError("[Equip] O prefab precisa de UI_MonsterCard ou UI_MonsterListItem.", cardObj);
                }
            }

            // Seleciona o primeiro da lista por padrão se nenhum estiver selecionado
            if (_selectedInstance == null && ownedInstances.Count > 0)
            {
                OnSelectMonsterInstance(ownedInstances[0]);
            }
            else
            {
                UpdateDetailsPanel();
            }
        }

        private void OnSelectMonsterInstance(MonsterInstance instance)
        {
            _selectedInstance = instance;
            UpdateDetailsPanel();
        }

        private void UpdateDetailsPanel()
        {
            if (_selectedInstance == null || InventoryManager.Instance == null)
            {
                ClearDetailsPanel();
                return;
            }

            MonsterDatabase database = InventoryManager.Instance.GetDatabase();
            MonsterData data = _selectedInstance.GetData(database);

            if (data != null && _monsterNameText != null)
            {
                _monsterNameText.text = $"{data.displayName} ({_selectedInstance.quality})";
            }

            if (_selectedInstance.progression != null)
            {
                if (_levelXPText != null)
                {
                    _levelXPText.text = $"Lv. {_selectedInstance.progression.currentLevel} | XP: {_selectedInstance.progression.currentXP}";
                }
            }

            // Ação do Botão Equipar
            if (_equipButton != null)
            {
                _equipButton.onClick.RemoveAllListeners();
                _equipButton.onClick.AddListener(() =>
                {
                    if (_targetLaneSlot != null && _selectedInstance != null)
                    {
                        AssignSelectedMonsterToLane(_targetLaneSlot);
                    }
                });
            }

            // Ação do Botão Árvore de Habilidades
            if (_openSkillTreeButton != null)
            {
                _openSkillTreeButton.onClick.RemoveAllListeners();
                _openSkillTreeButton.onClick.AddListener(() =>
                {
                    if (_targetLaneSlot != null)
                    {
                        Monster spawnedMonster = _targetLaneSlot.GetSpawnedMonster();
                        if (spawnedMonster != null && UI_SkillTreeWindow.Instance != null)
                        {
                            UI_SkillTreeWindow.Instance.OpenWindowForMonster(spawnedMonster);
                        }
                    }
                });
            }
        }

        private void ClearDetailsPanel()
        {
            if (_monsterNameText != null) _monsterNameText.text = "No Monster Selected";
            if (_levelXPText != null) _levelXPText.text = "-";
            if (_statsSummaryText != null) _statsSummaryText.text = "";
        }

        public void AssignSelectedMonsterToLane(MonsterSlot slot)
        {
            if (slot == null || _selectedInstance == null || InventoryManager.Instance == null) return;

            _targetLaneSlot = slot;
            InventoryManager.Instance.RequestEquipMonster(slot, _selectedInstance);
            // Mantem a selecao e a pausa para experimentar outras lanes e monstros.
            UpdateDetailsPanel();
        }

        private void HighlightLanes(bool highlight)
        {
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    // Usa a verificação de se o objeto possui destaque visual
                    slot.SetHighlightVisible(highlight);
                }
            }
        }
    }
}
