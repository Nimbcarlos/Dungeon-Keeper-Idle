using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _essenceText;
    [SerializeField] private UnityEngine.UI.Button _pipButton;

    void Start()
    {
        // inscreve nos eventos
        ResourceManager.Instance.OnGoldChanged    += UpdateGold;
        ResourceManager.Instance.OnEssenceChanged += UpdateEssence;

        // inicializa com valores atuais
        UpdateGold(ResourceManager.Instance.Gold);
        UpdateEssence(ResourceManager.Instance.Essence);

        // configura botão PiP
        if (_pipButton != null)
            _pipButton.onClick.AddListener(OnPiPButtonClicked);

        // esconde o botão no editor — só faz sentido no Android
        #if UNITY_EDITOR
        if (_pipButton != null)
            _pipButton.gameObject.SetActive(false);
        #endif
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance == null) return;
        ResourceManager.Instance.OnGoldChanged    -= UpdateGold;
        ResourceManager.Instance.OnEssenceChanged -= UpdateEssence;
    }

    void UpdateGold(int total)
    {
        _goldText.text = $"{total}";
    }

    void UpdateEssence(int total)
    {
        _essenceText.text = $"{total}";
    }

    void OnPiPButtonClicked()
    {
        AndroidPiP pip = FindAnyObjectByType<AndroidPiP>();
        if (pip != null) pip.EnterPiP();
    }
}