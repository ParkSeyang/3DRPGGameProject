using UnityEngine;

public class QuickSlotInventory : BaseInventory
{
    public override UIType UIType => UIType.QuickSlot; // 퀵슬롯 별도 타입 지정

    protected override void Awake()
    {
        InventoryName = "Quick";
        base.Awake();
    }

    private void Update()
    {
        CheckQuickSlotInput();
    }

    private void CheckQuickSlotInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) UseItemInSlot(0, 0);
        if (Input.GetKeyDown(KeyCode.E)) UseItemInSlot(0, 1);
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemInSlot(0, 2);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemInSlot(0, 3);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemInSlot(0, 4);
    }

    private void UseItemInSlot(int row, int col)
    {
        var slot = SlotsGrid[row, col];
        if (slot != null && !slot.IsEmptySlot)
        {
            if (PlayerStatusController.Instance == null) return;

            // 아이템 효과 적용 및 소모 여부 확인
            bool isConsumed = PlayerStatusController.Instance.ApplyItemEffect(slot.Item);

            if (isConsumed)
            {
                // 아이템 개수 차감 로직
                if (slot.ItemCount > 1)
                {
                    slot.SetItem(slot.Item, slot.ItemCount - 1);
                }
                else
                {
                    slot.SetItem(null); // 0개가 되면 슬롯 비움
                }
            }
        }
    }
    
    // UseItemLogic 메서드 삭제됨 (PlayerStatusController로 이동)
}