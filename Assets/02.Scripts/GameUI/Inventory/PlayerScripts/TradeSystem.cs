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
    public Button YesButton;
    public Button NoButton;
    
    private bool isClick = false;
    private bool isPositiveClick = false;
    
    public void RequestTradeEvent(InventoryType inven, TradeType type, SlotSystem start, SlotSystem end)
    {
        StartCoroutine(TriggerTradeEvent(inven, type, start, end));
    }
    
    private IEnumerator TriggerTradeEvent(InventoryType inven, TradeType type, SlotSystem start, SlotSystem end)
    {
        Debug.Log($"TradeEvent :: {type}, start ={start.gameObject.name}, end ={end.gameObject.name}");

        if (inven == InventoryType.User || TradeType.Buy == type)
        {
            if(BuyObject != null) BuyObject.SetActive(true);
           // if(BuyText != null) BuyText.text = $"구매하시겠습니까? {end.Item.Name} : {end.Item.Price} G";
        }

        if (inven == InventoryType.Trader || TradeType.Sell == type)
        {
            if(SellObject != null) SellObject.SetActive(true);
           // if(SellText != null) SellText.text = $"판매하시겠습니까? {end.Item.Name} : {end.Item.Price / 2} G";
        }
       
        // 버튼 클릭 대기
        isClick = false;
        while (isClick == false)
        {
            yield return null;
        }
        
        if(BuyObject != null) BuyObject.SetActive(false);
        if(SellObject != null) SellObject.SetActive(false);
        
        if (isPositiveClick == true) 
        {
            var player = Player.Instance;
            Item item = end.Item;
        
            switch (type)
            {
                case TradeType.Buy:
                  //  if (player.Gold >= item.Price)
                  //  {
                  //      player.AddGold(-item.Price);
                  //      // 아이템 복사 (구매)
                  //      start.SetItem(end.Item); // 딥카피 필요시 수정
                  //      Debug.Log($"[Trade] 구매 성공: {item.Name}");
                  //  }
                  //  else
                  //  {
                  //      Debug.LogWarning("[Trade] 골드가 부족합니다.");
                  //  }
                    break;

                case TradeType.Sell:
                  //  player.AddGold(item.Price / 2);
                    start.SetItem(null); // 판매했으므로 슬롯 비움
                   // Debug.Log($"[Trade] 판매 성공: {item.Name}");
                    break;

                case TradeType.None:
                    Debug.Log("거래 타입 없음");
                    break;
            }
        }
        else 
        {
            // 거래 취소 시 (단순 이동인 경우 원래대로 돌려놓기 등의 로직 필요 시 추가)
            // 여기서는 드래그 앤 드롭이 이미 끝난 상태에서의 확정 로직이므로,
            // 구매가 아니면 아이템 이동을 취소(원복)해야 할 수도 있음.
            // 현재 구조상 UserInput에서 MoveItem 호출 후 TradeSystem을 부르므로, 
            // 취소 시 아이템을 다시 돌려놓는 로직이 필요할 수 있음.
            
            // 단순 이동(같은 인벤토리)은 TradeSystem을 안 거치므로 상관없음.
        }
    }

    public void OnClickButton(bool isPositive)
    {
        isClick = true;
        isPositiveClick = isPositive;
    }
}