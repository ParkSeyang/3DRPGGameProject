using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotSystem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item Item;
    public Image Image;
    public TextMeshProUGUI CountText;
    
    private RectTransform rectTransform;
    public int ItemCount { get; private set; }

    public bool IsEmptySlot => Item == null;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // GetComponentsInChildren으로 자식들을 모두 검색 (비활성화된 것 포함)
        if (CountText == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in allTexts)
            {
                // 보통 개수 표시용은 이름에 'Count'나 'Text'가 들어가는 경우가 많음
                // 또는 단순히 첫 번째로 발견된 것을 할당
                if (txt.gameObject.name.Contains("Count") || txt.gameObject.name.Contains("Text"))
                {
                    CountText = txt;
                    break;
                }
            }
            
            // 만약 위에서 못 찾았다면 첫 번째 요소라도 할당
            if (CountText == null && allTexts.Length > 0)
            {
                CountText = allTexts[0];
            }
        }
        
        SetItem(null);
    }

    public void SetPosition(Vector2 inputPosition)
    {
        transform.position = inputPosition;
    }
    
    public void SetItem(Item item, int count = 1)
    {
        this.Item = item;
        this.ItemCount = (item == null) ? 0 : count;

        if (item == null)
        {
            Image.sprite = null;
            Image.enabled = false;
            if (CountText != null) CountText.text = "";
        }
        else
        {
            Image.sprite = this.Item.Icon;
            Image.enabled = true;
            UpdateCountUI();
        }
    }

    private void UpdateCountUI()
    {
        if (CountText == null) return;

        if (ItemCount > 1)
        {
            CountText.text = ItemCount.ToString();
        }
        else
        {
            CountText.text = ""; 
        }
    }

    // --- Drag Interface Implementations ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UserInputSystem.Instance != null)
        {
            UserInputSystem.Instance.ProcessBeginDrag(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (UserInputSystem.Instance != null)
        {
            UserInputSystem.Instance.ProcessDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (UserInputSystem.Instance != null)
        {
            UserInputSystem.Instance.ProcessEndDrag(eventData);
        }
    }
}