using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private InventoryUI inventoryUI;
    public int Index { get; private set; }

    public void Initialize(InventoryUI ui, int index)
    {
        inventoryUI = ui;
        Index = index;
    }

    public void SetItem(ItemSlot slotData)
    {
        if (slotData == null || slotData.IsEmpty)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            amountText.gameObject.SetActive(false);
        }
        else
        {
            iconImage.sprite = slotData.ItemData.Icon;
            iconImage.enabled = true;
            if (slotData.Amount > 1)
            {
                amountText.text = slotData.Amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    // 슬롯이 직접 드래그 이벤트를 받아 InventoryUI로 토스합니다.
    public void OnBeginDrag(PointerEventData eventData) => inventoryUI.OnBeginDrag(this);
    public void OnDrag(PointerEventData eventData) => inventoryUI.OnDrag(eventData);
    public void OnEndDrag(PointerEventData eventData) => inventoryUI.OnEndDrag(eventData);
}