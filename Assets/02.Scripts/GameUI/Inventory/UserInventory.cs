using UnityEngine;

public class UserInventory : BaseInventory
{
    protected override void Awake()
    {
        InventoryName = "User";
        base.Awake();
    }

    // 유저 인벤토리 특화 기능 (예: 전체 정렬 등)이 필요하면 여기에 작성합니다.
}