using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryCursorUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.blocksRaycasts = false; // 입력을 절대 방해하지 않음
        canvasGroup.interactable = false;
        
        gameObject.SetActive(false);
    }

    public void SetCursorItem(ItemSlot slotData)
    {
        if (slotData == null || slotData.IsEmpty)
        {
            Hide();
            return;
        }

        iconImage.sprite = slotData.ItemData.Icon;
        iconImage.enabled = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void UpdatePosition(Vector2 screenPosition)
    {
        rectTransform.position = screenPosition;
    }

    public void Hide() => gameObject.SetActive(false);
}
