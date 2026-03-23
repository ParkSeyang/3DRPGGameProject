using UnityEngine;

public class PlayerTradeInventory : BaseInventory
{
    // 상점 내 가방은 전용 타입 부여
    public override UIType UIType => UIType.PlayerTrade;

    protected override void Awake()
    {
        InventoryName = "User"; // 데이터는 'User' 가방과 공유
        isManagedByUIManager = true; // UIManager가 관리하도록 변경
        base.Awake();
    }

    private void OnEnable()
    { 
        // 상점 창이 열릴 때(활성화될 때) 캐시로부터 최신 데이터 로드
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

    

    protected override void OnDisable()
    {
            // 상점 창이 닫힐 때 현재 상태를 캐시에 백업
        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.SaveToCache(InventoryName, GetSaveData());
        }

    }

}

    