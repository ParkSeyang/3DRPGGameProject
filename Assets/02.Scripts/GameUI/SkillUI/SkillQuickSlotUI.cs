using UnityEngine;

public class SkillQuickSlotUI : BaseUI
{
    public override UIType UIType => UIType.SkillQuickSlot;
    public override bool IsPopup => false; 

    protected override void Awake()
    {
        isManagedByUIManager = true;
        base.Awake();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }
}