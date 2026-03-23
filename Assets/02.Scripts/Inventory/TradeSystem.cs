using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSystem : SingletonBase<TradeSystem>
{
    public enum InventoryType
    {
        User,
        Trader,
        Quick,
    }

    public enum TradeType 
    {
        None,
        Buy,
        Sell,
    }
    
    [Header("Trade UI")]
    public GameObject BuyObject;
    public GameObject SellObject;
    public TMP_Text BuyText;
    public TMP_Text SellText;
    public TMP_Text PlayerGoldText; // 플레이어 소지금 표시용 추가

    [Header("Buy Buttons")]
    public Button BuyYesButton;
    public Button BuyNoButton;

    [Header("Sell Buttons")]
    public Button SellYesButton;
    public Button SellNoButton;
    
    private bool isClick = false;
    private bool isPositiveClick = false;
    private bool isProcessing = false; // [추가] 중복 거래 방지 플래그

    protected override void OnInitialize()
    {
        // 리스너 일괄 등록
        BuyYesButton?.onClick.AddListener(() => OnClickButton(true));
        BuyNoButton?.onClick.AddListener(() => OnClickButton(false));
        SellYesButton?.onClick.AddListener(() => OnClickButton(true));
        SellNoButton?.onClick.AddListener(() => OnClickButton(false));

        // 플레이어 골드 변경 이벤트 구독
        if (Player.Instance != null)
        {
            Player.Instance.OnGoldChanged += UpdatePlayerGoldUI;
            UpdatePlayerGoldUI(Player.Instance.Gold);
        }
    }

    private void UpdatePlayerGoldUI(int currentGold)
    {
        if (PlayerGoldText != null)
        {
            PlayerGoldText.text = $"{currentGold:#,0} G";
        }
    }
    
    public void RequestTradeEvent(InventoryType inventoryType, TradeType tradeType, SlotSystem startSlot, SlotSystem endSlot)
    {
        if (isProcessing == true) return; // 이미 거래 처리 중이면 중복 요청 무시
        StartCoroutine(TriggerTradeEvent(inventoryType, tradeType, startSlot, endSlot));
    }
    
    private IEnumerator TriggerTradeEvent(InventoryType inventoryType, TradeType tradeType, SlotSystem startSlot, SlotSystem endSlot)
    {
        isProcessing = true; // 처리 시작

        // [수정] 드래그 중인 아이템 정보를 CursorSlot에서 최우선으로 확보
        // UserInputSystem에서 드래그 시작 시 원본 슬롯을 비우기 때문에 CursorSlot을 먼저 봐야 함
        Item tradingItem = null;
        int buyCount = 0;

        if (UserInputSystem.Instance != null && UserInputSystem.Instance.CursorSlot.IsEmptySlot == false)
        {
            tradingItem = UserInputSystem.Instance.CursorSlot.Item;
            buyCount = UserInputSystem.Instance.CursorSlot.ItemCount;
        }
        else
        {
            // 드래그 방식이 아닌 직접 클릭 거래 등의 경우를 대비한 백업
            tradingItem = (startSlot.Item != null) ? startSlot.Item : (endSlot.Item != null ? endSlot.Item : null);
            buyCount = (startSlot.Item != null) ? startSlot.ItemCount : (endSlot.Item != null ? endSlot.ItemCount : 1);
        }

        if (tradingItem == null || buyCount <= 0)
        {
            isProcessing = true == false;
            yield break;
        }

        // 상인 인벤토리 참조 확보
        var traderInventory = (inventoryType == InventoryType.Trader) 
            ? InventorySystem.Instance.GetInventoryorNullBySlot(endSlot) as TraderInventory
            : InventorySystem.Instance.GetInventoryorNullBySlot(startSlot) as TraderInventory;

        if (tradeType == TradeType.Buy)
        {
            int totalPrice = tradingItem.BuyPrice * buyCount;
            if (BuyObject != null) BuyObject.SetActive(true);
            if (BuyText != null) BuyText.text = $"{tradingItem.ItemName} x{buyCount}을(를) {totalPrice} G에 구매하시겠습니까?";
        }
        else if (tradeType == TradeType.Sell)
        {
            int totalSellPrice = tradingItem.SellPrice * buyCount;
            if (SellObject != null) SellObject.SetActive(true);
            if (SellText != null) SellText.text = $"{tradingItem.ItemName} x{buyCount}을(를) {totalSellPrice} G에 판매하시겠습니까?";
        }
       
        // 버튼 클릭 대기
        isClick = false;
        while (isClick == false)
        {
            yield return null;
        }
        
        if (BuyObject != null) BuyObject.SetActive(false);
        if (SellObject != null) SellObject.SetActive(false);
        
        if (isPositiveClick == true) 
        {
            var player = Player.Instance;
            
            switch (tradeType)
            {
                case TradeType.Buy:
                    // 기존 슬롯에 합칠 수 있는지 최종 확인
                    bool isSameItem = (endSlot.IsEmptySlot == false && endSlot.Item.ItemID == tradingItem.ItemID);
                    bool canStack = (isSameItem == true && (endSlot.ItemCount + buyCount <= tradingItem.MaxStack));
                    
                    // 빈 슬롯도 아니고, 합치기도 불가능하다면 거절
                    if (endSlot.IsEmptySlot == false && canStack == false)
                    {
                        UIManager.Instance.ShowWarning("아이템을 놓을 수 있는 공간이 부족합니다.");
                        break;
                    }

                    int finalBuyPrice = tradingItem.BuyPrice * buyCount;
                    if (player.Gold >= finalBuyPrice)
                    {
                        // 1. 재화 이동
                        player.AddGold(-finalBuyPrice);
                        traderInventory?.AddGold(finalBuyPrice);

                        // 2. 아이템 이동 (중첩 또는 신규 생성)
                        if (canStack == true)
                        {
                            // 이미 있는 뭉치에 수량만 더함
                            endSlot.SetItem(endSlot.Item, endSlot.ItemCount + buyCount);
                        }
                        else
                        {
                            // 빈 슬롯에 새 아이템 생성
                            endSlot.SetItem(ItemDataManager.Instance.GetItem(tradingItem.ItemID), buyCount);
                        }
                        
                        // 퀘스트 업데이트
                        if (QuestManager.IsInitialized == true)
                        {
                            QuestManager.Instance.UpdateBuyQuest(tradingItem.ItemID);
                        }
                    }
                    else
                    {
                        UIManager.Instance.ShowWarning("아이템을 구매하기 위한 골드가 부족합니다.");
                        // 구매 실패 시 (중첩이 아니었을 경우에만) 슬롯 비우기
                        if (canStack == false) endSlot.SetItem(null);
                    }
                    break;

                case TradeType.Sell:
                    int finalSellPrice = tradingItem.SellPrice * buyCount;

                    if (traderInventory != null && traderInventory.MerchantGold < finalSellPrice)
                    {
                        UIManager.Instance.ShowWarning("상인의 보유 골드가 부족하여 판매할 수 없습니다.");
                        startSlot.SetItem(tradingItem, buyCount);
                        endSlot.SetItem(null);
                        break;
                    }

                    // 1. 재화 이동
                    player.AddGold(finalSellPrice);
                    traderInventory?.AddGold(-finalSellPrice);
                    
                    // 2. 아이템 제거 (판매 성공했으므로 유저 인벤토리에서 제거)
                    startSlot.SetItem(null);
                    break;
            }
        }
        else
        {
            // 거래 취소(No 버튼) 시 복구 로직
            if (tradeType == TradeType.Sell)
            {
                // 이미 UserInputSystem에서 startSlot으로 아이템을 복구했으므로 추가 작업 불필요
            }
            else if (tradeType == TradeType.Buy)
            {
                // 구매 대기 중이던 슬롯 비우기
                endSlot.SetItem(null);
            }
        }

        isProcessing = false; // 처리 완료
    }

    public void OnClickButton(bool isPositive)
    {
        isClick = true;
        isPositiveClick = isPositive;
    }
}
