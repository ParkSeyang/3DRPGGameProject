using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    private void Update()
    {
        // F1: 체력 포션(소) 5개 추가
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddItemToUserInventory("I001", 5);
        }

        // F2: 마나 포션(소) 3개 추가
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AddItemToUserInventory("I002", 3);
        }

        // F3: 강철 대검 1개 추가
        if (Input.GetKeyDown(KeyCode.F3))
        {
            AddItemToUserInventory("I005", 1);
        }

        // F5: 강철 갑옷 1개 추가
        if (Input.GetKeyDown(KeyCode.F5))
        {
            AddItemToUserInventory("I006", 1);
        }

        // F6: 초심자의 반지 1개 추가
        if (Input.GetKeyDown(KeyCode.F6))
        {
            AddItemToUserInventory("I008", 1);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (Player.Instance != null)
            {
                Player.Instance.SetHP(Player.Instance.HP - 20f);
                Debug.Log($"[Tester] 플레이어 체력 감소됨. 현재 HP: {Player.Instance.HP}");
            }
        }
        if (Input.GetKeyDown(KeyCode.F11))
        { 
            Player.Instance.AddSP(10); // Player.cs에 AddSP 메서드가 있어야 함
        }
        
    }

    private void AddItemToUserInventory(string itemID, int count)
    {
        if (ItemDataManager.Instance == null) return;

        Item itemRes = ItemDataManager.Instance.GetItem(itemID);
        if (itemRes == null) return;

        var userInven = InventorySystem.Instance.GetInventoryOrNull("User");
        if (userInven != null)
        {
            // 수정됨: 개수(count)를 함께 전달
            if (userInven.AddItem(itemRes, count))
            {
                Debug.Log($"[Tester] {itemRes.ItemName} {count}개 추가 완료.");
            }
        }
    }
}