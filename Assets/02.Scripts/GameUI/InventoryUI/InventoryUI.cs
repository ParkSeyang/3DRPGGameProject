using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : BaseUI
{
    public override UIType UIType => UIType.Inventory;

    [Header("UI References")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventoryCursorUI cursorUI;

    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();
    private GraphicRaycaster graphicRaycaster;
    private int draggingIndex = -1;

    protected override void Awake()
    {
        base.Awake();
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
    }

    private void Start()
    {
        InitializeSlots();
        InventorySystem.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void InitializeSlots()
    {
        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        slotUIList.Clear();

        int capacity = InventorySystem.Instance.InventorySize;
        for (int i = 0; i < capacity; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
            slotUI.Initialize(this, i);
            slotUIList.Add(slotUI);
        }
    }

    public void RefreshUI()
    {
        var slots = InventorySystem.Instance.Slots;
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < slots.Count) slotUIList[i].SetItem(slots[i]);
            else slotUIList[i].SetItem(null);
        }
    }

    // --- Drag & Drop Core Logic ---

    public void OnBeginDrag(ItemSlotUI slotUI)
    {
        int index = slotUI.Index;
        if (InventorySystem.Instance.Slots[index].IsEmpty) return;

        draggingIndex = index;
        cursorUI.SetCursorItem(InventorySystem.Instance.Slots[index]);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingIndex != -1) cursorUI.UpdatePosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingIndex == -1) return;

        ItemSlotUI targetSlot = GetSlotUnderPointer(eventData);
        if (targetSlot != null && targetSlot.Index != draggingIndex)
        {
            InventorySystem.Instance.SwapItems(draggingIndex, targetSlot.Index);
        }

        cursorUI.Hide();
        draggingIndex = -1;
    }

    private ItemSlotUI GetSlotUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);

        foreach (var result in results)
        {
            // ItemSlotUI 컴포넌트를 찾습니다.
            ItemSlotUI slot = result.gameObject.GetComponent<ItemSlotUI>();
            if (slot == null) slot = result.gameObject.GetComponentInParent<ItemSlotUI>();
            
            if (slot != null) return slot;
        }
        return null;
    }
}