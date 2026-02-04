using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : SingletonBase<InventorySystem>
{
   private List<BaseInventory> inventories = new List<BaseInventory>();
   
   public void RegisterInventory(BaseInventory inventory)
   {
      if (inventories.Contains(inventory) == false)
      {
         inventories.Add(inventory);
      }
   }

   public void RemoveInventory(BaseInventory inventory)
   {
      inventories.Remove(inventory);
   }

   protected override void OnInitialize()
   {
      base.OnInitialize();
      ForceFindInventories();
   }

   public void ForceFindInventories()
   {
      // 게임 시작 시 비활성화된 인벤토리들도 미리 찾아서 등록
      var allInventories = FindObjectsOfType<BaseInventory>(true);
      foreach (var inven in allInventories)
      {
         // Awake가 실행되지 않아 이름이 없는 경우 강제로 할당
         if (string.IsNullOrEmpty(inven.InventoryName))
         {
             if (inven is UserInventory) inven.InventoryName = "User";
             else if (inven is EquipInventory) inven.InventoryName = "Equip";
             else if (inven is QuickSlotInventory) inven.InventoryName = "Quick";
             else if (inven is TraderInventory) inven.InventoryName = "Trader";
         }
         
         RegisterInventory(inven);
      }
      Debug.Log($"[InventorySystem] 인벤토리 강제 탐색 완료. 등록된 인벤토리 수: {inventories.Count}");
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