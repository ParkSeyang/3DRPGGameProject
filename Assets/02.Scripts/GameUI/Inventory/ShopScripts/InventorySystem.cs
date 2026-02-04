using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : SingletonBase<InventorySystem>
{
   private List<BaseInventory> inventories = new List<BaseInventory>();
   
   public void RegisterInventory(BaseInventory inventory)
   {
      if (!inventories.Contains(inventory))
      {
         inventories.Add(inventory);
      }
   }

   public void RemoveInventory(BaseInventory inventory)
   {
      inventories.Remove(inventory);
   }

   public BaseInventory GetInventoryOrNull(string targetInventoryName)
   {
      foreach(var inven in inventories)
      {
         if (inven.InventoryName.Equals(targetInventoryName))
         {
            return inven;
         }
      }
      return null;
   }

   public BaseInventory GetInventoryorNullBySlot(SlotSystem slot)
   {
      foreach(var inven in inventories)
      {
         if (inven.IsInInventory(slot))
         {
            return inven;
         }
      }
      return null;
   }
}