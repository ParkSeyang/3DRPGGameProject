using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SkillTreeUI : BaseUI
{
    public static SkillTreeUI Instance { get; private set; }

    public override UIType UIType => UIType.Skill;

    [Header("UI Components")]
    public Transform skillListParent; // SkillSlotUI들이 들어갈 부모 (Content 등)
    public TextMeshProUGUI spText;
    
    // 미리 생성해둔 슬롯들 (Inspector 할당 or GetComponentsInChildren)
    private List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();

    // 퀵슬롯 창 (SkillTree가 열릴 때 같이 켜주고 닫을 때 닫기 위함)
    public GameObject skillQuickSlotCanvas; 

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        
        // 슬롯들 수집
        if (skillListParent != null)
        {
            var slots = skillListParent.GetComponentsInChildren<SkillSlotUI>(true);
            skillSlots.AddRange(slots);
        }
    }

    private void Start()
    {
        InitSlots();
        if (Player.Instance != null)
        {
            Player.Instance.OnSpChanged += UpdateSPAndSlots;
        }
    }

    private void OnDestroy()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnSpChanged -= UpdateSPAndSlots;
        }
    }

    private void UpdateSPAndSlots(int sp)
    {
        UpdateSPText();
        RefreshAllSlots();
    }

    private void InitSlots()
    {
        // 슬롯 ID 할당 (ID 1 ~ 4)
        // Hierarchy 순서대로 ID 1, 2, 3, 4 할당
        for (int i = 0; i < skillSlots.Count; i++)
        {
            // 스킬 ID는 1부터 시작하므로 i + 1
            skillSlots[i].Init(i + 1);
        }
    }

    public override void Open()
    {
        base.Open();
        RefreshAllSlots();
        UpdateSPText();
    }

    public override void Close()
    {
        base.Close();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in skillSlots)
        {
            slot.RefreshUI();
        }
    }

    public void UpdateSPText()
    {
        if (spText != null && Player.Instance != null)
        {
            spText.text = $"SP: {Player.Instance.SP}";
        }
    }
}