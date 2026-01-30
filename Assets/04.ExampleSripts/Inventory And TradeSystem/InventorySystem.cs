using UnityEngine;
using System.Collections.Generic;
using System;
using Jay;
// 모든 인벤토리를 관리하고, 서칭할 수 있는 역할의 객체
public class InventorySystem : SingletonBase<InventorySystem>
{
    private List<Inventory> inventories = new List<Inventory>();

    // start와 유사하지만 먼저 호출된ㄴ 이벤트 함수
    // 객체가 메모리에 할당(게임오브젝트의 경우 인스턴시에이트) 되면서 동시에 호출이됨
    
    public void RegisterInventory(Inventory inventory)
    {
        inventories.Add(inventory);
    }

    public void RemoveInventory(Inventory inventory)
    {
        inventories.Remove(inventory);
    }


    public Inventory GetInventoryorNull(string targetInventoryName)
    {
        for (int i = 0; i < inventories.Count; i++)
        {
            if (inventories[i].Name.Equals(targetInventoryName))
            {
                return inventories[i];
            }
        }
        return null;
    }

    public Inventory GetInventoryOrNullBySlot(Slot slot)
    {
        // 모든 인벤토리에 slot이 어떠한 인벤토리에 있는지 검색을한다.
        for (int i =0; i < inventories.Count; i++)
        {
            if (inventories[i].IsInInventory(slot))
            {
                return inventories[i];
            }
        }

        return null;
    }

}
