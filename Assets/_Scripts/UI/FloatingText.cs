using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh; // Versao 3D (World Space nativo)
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float fadeDuration = 0.8f;

    private Color textColor;
    private float alphaTimer;

    public void Setup(int damageAmount, bool isCritical = false)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = damageAmount.ToString();
        
        // Cores e tamanho 3D
        textColor = isCritical ? Color.red : Color.yellow;
        textMesh.color = textColor;
        textMesh.fontSize = isCritical ? 8 : 5;
        
        alphaTimer = fadeDuration;
    }

    void Update()
    {
        // Desloca o texto diretamente para cima no mundo 3D/2D
        transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

        // Fade Out
        alphaTimer -= Time.deltaTime;
        if (alphaTimer <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            textColor.a = alphaTimer / fadeDuration;
            textMesh.color = textColor;
        }
    }
}