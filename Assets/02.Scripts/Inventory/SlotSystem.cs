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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        // 이미 할당되어 있다면 스킵
        if (Image != null && CountText != null) return;

        if (Image == null) Image = GetComponentInChildren<Image>(true);
        
        if (CountText == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in allTexts)
            {
                if (txt.gameObject.name.Contains("Count") || txt.gameObject.name.Contains("Text"))
                {
                    CountText = txt;
                    break;
                }
            }
            if (CountText == null && allTexts.Length > 0) CountText = allTexts[0];
        }
    }

    public void SetPosition(Vector2 inputPosition)
    {
        transform.position = inputPosition;
    }
    
    public void SetItem(Item item, int count = 1)
    {
        // 데이터 주입 전 참조 보장 (방어적 코드)
        EnsureReferences();
        
        this.Item = item;
        this.ItemCount = (item == null) ? 0 : count;

        if (item == null)
        {
            if (Image != null)
            {
                Image.sprite = null;
                Image.enabled = false;
            }
            if (CountText != null) CountText.text = "";
        }
        else
        {
            if (Image != null)
            {
                Image.sprite = this.Item.Icon;
                Image.enabled = true;
            }
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