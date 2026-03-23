using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventorySystem : SingletonBase<InventorySystem>
{
   private List<BaseInventory> inventories = new List<BaseInventory>();

   public void RegisterInventory(BaseInventory inventory)
   {
      // 중복 참조 방지
      inventories.RemoveAll(inventoryComponent => inventoryComponent == null || inventoryComponent == inventory);
      inventories.Add(inventory);
      
      // 데이터 복원 (초기화 여부와 상관없이 매니저 인스턴스를 호출하여 로드 보장)
      if (InventoryDataManager.Instance != null)
      {
          var cachedData = InventoryDataManager.Instance.GetCachedDataOrNull(inventory.InventoryName);
          if (cachedData != null)
          {
              inventory.LoadFromSaveData(cachedData);
              inventory.RefreshInventory();
          }
      }
   }

   private void OnEnable()
   {
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDisable()
   {
      SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      ForceFindInventories();
   }

   protected override void OnInitialize()
   {
      // 1. 씬의 모든 인벤토리 수집
      var allInventories = UnityEngine.Object.FindObjectsByType<BaseInventory>(
          UnityEngine.FindObjectsInactive.Include, 
          UnityEngine.FindObjectsSortMode.None);

      foreach (var inventory in allInventories)
      {
         // 2. 이름 자동 할당
         if (string.IsNullOrEmpty(inventory.InventoryName) == true)
         {
             if (inventory is UserInventory) inventory.InventoryName = "User";
             else if (inventory is EquipInventory) inventory.InventoryName = "Equip";
             else if (inventory is QuickSlotInventory) inventory.InventoryName = "Quick";
             else if (inventory is TraderInventory) inventory.InventoryName = "Trader";
         }

         // 3. '초기화 패스': 명시적 초기화 호출 (이 안에서 Register 및 데이터 복원이 일어남)
         inventory.InitializeInventory();
      }
      
   }

   public void ForceFindInventories()
   {
       // OnInitialize와 동일한 로직을 수행하거나 OnInitialize를 재호출
       OnInitialize();
   }

   public BaseInventory GetInventoryOrNull(string targetInventoryName)
   {
      return inventories.Find(inventory => inventory != null && inventory.InventoryName == targetInventoryName);
   }

   public List<BaseInventory> GetAllInventoriesByName(string targetName)
   {
       return inventories.FindAll(inventory => inventory != null && inventory.InventoryName == targetName);
   }

   public BaseInventory GetInventoryorNullBySlot(SlotSystem slot)
   {
      return inventories.Find(inventory => inventory != null && inventory.IsInInventory(slot));
   }
}