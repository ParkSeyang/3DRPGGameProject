using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Slot : MonoBehaviour
{
    public Item item;
    public Image image;
    private RectTransform rectTransform;
    // Slot에 뭔가가 SetItem이 호출 될 경우
    // 내가 포함된 Inventory에 나의 상태 변화를 알리는 함수를 호출하는 로직을 만들어 봅시다.
    // 1. Slot이 상위 개체인 Inventory에 대한 참조 정보가 필요하고 : Slot이 엄청 많기 때문.
    // 그리고 인벤토리도 여러개라서 
    // 2. Slot의 SetItem이 실행된 후 Inventory의 특정함수(OnUpdatedSlot)를 호출하여 특정 로직을 실행합시다.
    
    // Slot에 Item이 Set됨 -> 
    
    public bool IsEmptySlot { get { return item == null; } }

    
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        SetItem(null);
    }

    public void SetPosition(Vector2 inputPosition)
    {
        rectTransform.anchoredPosition = inputPosition - new Vector2(rectTransform.sizeDelta.x / 2, rectTransform.sizeDelta.y / 2);
    }
    
    public void SetItem(Item item)
    {
        this.item = item;
        if (item == null)  // 아이템이 null이면 Slot이 비어있어야함
        {
            image.sprite = null;
            image.enabled = false;
        }
        else // 아이템이 null이 아니라 어떤 인스턴스가 할당되어있다면
        {
            image.sprite = this.item.Icon;
            image.enabled = true;
        }
        
    }
    
}
