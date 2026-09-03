using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonKeeper
{
    public class DynamicSpriteSorter : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private int _baseOrder = 5000; // Valor base para evitar ordens negativas

        private void Awake()
        {
            if (_sortingGroup == null)
                _sortingGroup = GetComponent<SortingGroup>();
        }

        private void LateUpdate()
        {
            if (_sortingGroup != null)
            {
                // Posição Y invertida: quanto menor o Y (mais para baixo na tela), maior o Order (desenha na frente)
                _sortingGroup.sortingOrder = _baseOrder - Mathf.RoundToInt(transform.position.y * 100f);
            }
        }
    }
}