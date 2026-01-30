using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Jay;

public class TradeSystem : SingletonBase<TradeSystem>
{
    public enum TradeType
    {
        Buy,
        Sell,
    }
    
    // 원래는 묶어서 써야합니다.
    public GameObject RootObject;
    public TMP_Text ContentText;
    public Button YesButton;
    public Button NoButton;

    public bool isClick = false;
    public bool isPositiveClick = false;
    
    // 이 함수의 역할 은 유저가 선택하기까지 잠깐 대기했다가 TiggerTradeEvent를 처리하는함수
    // 요 함수는 요청을 받아서 코루틴을 실행하는 함수
    public void RequestTradeEvent(TradeType type, Slot start, Slot end)
    {
        StartCoroutine(TiggerTradeEvent(type, start, end));
    }

    //요 함수의 역할은 유저가 선택하기까지 잠깐 대기했다가 TriggerTradeEvent를 처리하는 함수
    private IEnumerator TiggerTradeEvent(TradeType type, Slot start, Slot end)
    {
        Debug.Log($"TradeEvent : {type}, start = {start.gameObject.name} , end = {end.gameObject.name}");
       
        // 1. 팝업을 띄우고 
        RootObject.SetActive(true);
        ContentText.SetText($"Would you like to Purchase? \n {end.item.Name}  Price : {end.item.Price}$ ");
        // 2. isClick이 true될때까지 대기한다
        while (isClick == false)
        {
            yield return null;
        }

        // 위의 반복문은 이거랑 같다.. 람다식은 C#중급에서 알려드립니다.
        // yield return new WaitUntil(() => IsClick);
        
        // 3. 선택 완료시 팝업을 종료
        RootObject.SetActive(false);
        isClick = false;
        
        //4. 거래 처리
        if (isPositiveClick == true)
        {
            // 처리로직
            User currentUser = User.Instance;
            Item item = end.item;
            switch (type)
            {
                // 구매로직
                case TradeType.Buy:
                    currentUser.DecreaseMoney(item.Price);
                    Debug.Log($"[TradeEvent] {item.name}을 구매({item.Price})하였습니다. 잔액 : {currentUser.Money}");
                    break;
                case TradeType.Sell:
                    currentUser.IncreaseMoney(item.Price / 2);
                    Debug.Log($"[TradeEvent] {item.name}을 판매({item.Price / 2})하였습니다. 잔액 : {currentUser.Money}");
                    break;
                default:
                    break;
            }
        }
        else // No를 클릭한 경우
        {
            // 거래가 취소될 경우 end를 start로 다시 보내준다.
            start.SetItem(end.item);
            end.SetItem(null);
        }
       
        
    }

    public void OnClickButton(bool isPositive)
    {
        isClick = true;
        isPositiveClick = isPositive;
    }
}
