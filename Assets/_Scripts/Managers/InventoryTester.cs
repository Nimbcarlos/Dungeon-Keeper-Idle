using UnityEngine;
using DungeonKeeper;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private MonsterData _slimeDataTest;

    // Método chamado pelo Botão na UI
    public void TestEquipSlimeToSlot0()
    {
        if (InventoryManager.Instance == null) return;

        // 1. Simula adicionar um Slime na reserva
        InventoryManager.Instance.AddToReserve(_slimeDataTest);

        // 2. Pega o índice do monstro recém-adicionado (último da lista)
        int lastIndex = InventoryManager.Instance.ReserveCount - 1;

        // 3. Equipa no Slot 0 (MonsterSlot_1 da sala)
        InventoryManager.Instance.EquipToActiveSlot(lastIndex, 2);

        Debug.Log("Monstro equipado no Slot 0 via Teste!");
    }
}