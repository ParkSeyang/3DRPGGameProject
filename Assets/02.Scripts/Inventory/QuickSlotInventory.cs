using UnityEngine;

public class QuickSlotInventory : BaseInventory
{
    public override UIType UIType => UIType.QuickSlot; // 퀵슬롯 별도 타입 지정
    public override bool IsPopup => false; // 팝업이 아닌 HUD 성격

    protected override void Awake()
    {
        InventoryName = "Quick";
        isManagedByUIManager = true; // UIManager 관리 대상임을 명시
        base.Awake(); // BaseUI의 RegisterUI 호출 보장
    }

    private void Update()
    {
        // 팝업이 열려있거나 타이틀 씬인 경우 입력 차단
        if (UIManager.IsInitialized == true && UIManager.Instance.IsPopupOpen == true)
        {
            return;
        }

        CheckQuickSlotInput();
    }

    private void CheckQuickSlotInput()
    {
        // Q, E는 스킬용으로 변경되었으므로 숫자키 1~5로 매핑 변경
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemInSlot(0, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemInSlot(0, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemInSlot(0, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseItemInSlot(0, 3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseItemInSlot(0, 4);
    }

    private void UseItemInSlot(int row, int col)
    {
        var slot = SlotsGrid[row, col];
        if (slot != null && slot.IsEmptySlot == false)
        {
            if (PlayerStatusController.Instance == null) return;

            // 아이템 효과 적용 및 소모 여부 확인
            bool isConsumed = PlayerStatusController.Instance.ApplyItemEffect(slot.Item);

            if (isConsumed == true)
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
}