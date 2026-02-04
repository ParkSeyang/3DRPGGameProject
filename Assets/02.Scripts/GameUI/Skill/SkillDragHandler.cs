using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SkillDragHandler : SingletonBase<SkillDragHandler>
{
    public Image cursorImage; // 드래그 중 따라다닐 아이콘
    private SkillSlotUI startSlot;

    protected override void OnInitialize()
    {
        if (cursorImage != null)
        {
            cursorImage.raycastTarget = false;
            cursorImage.gameObject.SetActive(false);
        }
    }

    public void ProcessBeginDrag(SkillSlotUI slot, PointerEventData eventData)
    {
        startSlot = slot;
        if (cursorImage != null)
        {
            cursorImage.gameObject.SetActive(true);
            cursorImage.sprite = slot.iconImage.sprite;
            cursorImage.transform.position = eventData.position;
        }
    }

    public void ProcessDrag(PointerEventData eventData)
    {
        if (startSlot == null || cursorImage == null) return;
        cursorImage.transform.position = eventData.position;
    }

    public void ProcessEndDrag(PointerEventData eventData)
    {
        if (startSlot == null) return;

        // 놓은 곳이 퀵슬롯인지 확인
        SkillQuickSlot slot = GetQuickSlot(eventData);
        if (slot != null)
        {
            slot.SetSkill(startSlot.SkillID);
        }

        if (cursorImage != null)
        {
            cursorImage.gameObject.SetActive(false);
        }
        startSlot = null;
    }

    private SkillQuickSlot GetQuickSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<SkillQuickSlot>();
            if (slot != null) return slot;
        }
        return null;
    }
}