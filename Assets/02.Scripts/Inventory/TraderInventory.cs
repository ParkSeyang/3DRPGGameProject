using UnityEngine;
using UnityEngine.UI;

public class TraderInventory : BaseInventory
{
    public TMPro.TextMeshProUGUI MerchantGoldText; // 상인 소지금 표시용 (UI 필요)

    public int MerchantGold { get; private set; }

    // 상점 전용 UIType 반환
    public override UIType UIType => UIType.Trade;

    protected override void Awake()
    {
        InventoryName = "Trader";
        base.Awake();
    }

    public void SetGold(int amount)
    {
        MerchantGold = amount;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        MerchantGold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (MerchantGoldText != null)
        {
            MerchantGoldText.text = $"{MerchantGold:#,0} G";
        }
    }

    public void SetShopItems(System.Collections.Generic.List<string> itemIdList)
    { 
        // 데이터 주입 전 초기화 보장
        if (isInitialized == false)
        {
            InitializeInventory();
        }

        // 슬롯 비우기
        foreach (var slot in slotList) 
        { 
            slot.SetItem(null);
        } 

        // 지정된 아이템 채우기
        for (int i = 0; i < Mathf.Min(itemIdList.Count, slotList.Count); i++)
        {
            Item item = ItemDataManager.Instance.GetItem(itemIdList[i]);
            if (item != null) 
            {
                slotList[i].SetItem(item, 1);
            }
        }
    }
}
            