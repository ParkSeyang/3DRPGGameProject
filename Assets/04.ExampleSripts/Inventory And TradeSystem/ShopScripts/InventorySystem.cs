using System;
using System.Collections.Generic;
using UnityEngine;
using Jay;

namespace Examples.Inventory
{
    public class InventorySystem : SingletonBase<InventorySystem>
    {
    private List<Inventory> inventories = new List<Inventory>();
    
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
                return  inventories[i];
            }
        }
            
        return null;
    }

    public Inventory GetInventoryorNullBySlot(SlotSystem slot)
    {
        //모든 Inventory에서 slot이 어떠한 인벤토리에 있는지 검색을한다.
            
        for (int i = 0; i < inventories.Count; i++)
        {
            if (inventories[i].IsInInventory(slot))
            {
                return inventories[i];
            }
        }

        return null;
    }

    }
}