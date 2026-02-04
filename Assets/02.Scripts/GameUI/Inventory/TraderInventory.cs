using UnityEngine;
using UnityEngine.UI;

public class TraderInventory : BaseInventory
{
    public Button ShopRerollButton;

    // 상점 전용 UIType 반환
    public override UIType UIType => UIType.Trade;

    protected override void Awake()
    {
        InventoryName = "Trader";
        base.Awake();

        if (ShopRerollButton != null)
        {
            ShopRerollButton.onClick.AddListener(OnShopRerollButton);
        }
    }

    public void OnShopRerollButton()
    {
        // 기존 로직: 슬롯을 순회하며 랜덤 아이템으로 채움
        foreach (var slot in slotList)
        {
            // ItemDataManager에서 로드된 아이템 중 랜덤하게 선택하는 로직                                                                        │
            // 현재는 테스트를 위해 전체 아이템 중 하나를 가져오도록 구성
            var allInfos = ItemDataManager.Instance.ItemInfoTable;
            if (allInfos.Count > 0)
            {
                var keys = new System.Collections.Generic.List<string>(allInfos.Keys);
                string randomKey = keys[Random.Range(0, keys.Count)];
                Item itemRes = ItemDataManager.Instance.GetItem(randomKey);
                slot.SetItem(itemRes);
            }
        }
        Debug.Log("[Trader] 상점 물품 리롤 완료");
    }
}