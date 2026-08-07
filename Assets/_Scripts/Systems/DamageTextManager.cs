using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [SerializeField] private GameObject floatingTextPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnDamageText(Vector3 spawnPosition, int damage, bool isCritical = false)
    {
        // Se estiver em PiP, nem gasta processamento gerando o texto
        if (PiPUIVisibility.IsPiPActive) return;

        // Pequena variação horizontal no X para os números não sobreporem exatamente no mesmo pixel
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);

        // Instancia o texto 3D direto na posição do HeadPoint
        GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition + randomOffset, Quaternion.identity);
        
        FloatingText ft = textObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(damage, isCritical);
        }
    }
}