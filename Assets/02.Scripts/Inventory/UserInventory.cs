using UnityEngine;

public class UserInventory : BaseInventory
{
    protected override void Awake()
    {
        InventoryName = "User";
        base.Awake();
    }

    private void OnEnable()
    {
        // 직접 Instance를 호출하여 동기화 보장
        if (InventoryDataManager.Instance != null)
        {
            var data = InventoryDataManager.Instance.GetCachedDataOrNull(InventoryName);
            if (data != null)
            {
                LoadFromSaveData(data);
                RefreshInventory();
            }
        }
    }
}