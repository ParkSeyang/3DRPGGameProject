using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI levelText;
    public Button levelUpButton;
    public Button levelDownButton; // 레벨 다운 버튼 추가
    public Image lockPanel; // 해금 안 되었을 때 가리는 패널

    public int SkillID { get; private set; }
    private Skill skillData;

    public void Init(int skillId)
    {
        this.SkillID = skillId;
        levelUpButton.onClick.AddListener(OnLevelUpClick);
        if (levelDownButton != null) levelDownButton.onClick.AddListener(OnLevelDownClick);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (SkillDataManager.Instance == null) return;
        skillData = SkillDataManager.Instance.GetSkill(SkillID);
        if (skillData == null) return;

        // 아이콘 설정
        if (skillData.Icon != null)
        {
            iconImage.sprite = skillData.Icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // 레벨 표시
        levelText.text = $"Lv.{skillData.Level} / {skillData.MaxLevel}";

        // 해금 여부 체크
        bool isUnlocked = SkillTreeSystem.Instance != null && SkillTreeSystem.Instance.IsSkillUnlocked(SkillID);
        
        if (lockPanel != null)
        {
            lockPanel.gameObject.SetActive(!isUnlocked);
        }
        
        // 버튼 활성화 상태
        levelUpButton.interactable = isUnlocked && !skillData.IsMaxLevel;
        if (levelDownButton != null)
        {
            levelDownButton.interactable = skillData.Level > 0;
        }
    }

    private void OnLevelUpClick()
    {
        if (SkillTreeSystem.Instance != null && SkillTreeSystem.Instance.TryLevelUp(SkillID))
        {
            RefreshUI();
            SkillTreeUI.Instance?.UpdateSPText();
        }
    }

    private void OnLevelDownClick()
    {
        if (SkillTreeSystem.Instance != null && SkillTreeSystem.Instance.TryLevelDown(SkillID))
        {
            RefreshUI();
            SkillTreeUI.Instance?.UpdateSPText();
        }
    }

    // --- 드래그 구현 (액티브 스킬만 가능) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillData == null || skillData.Type != SkillType.Active || skillData.Level <= 0) return;
        
        // 드래그 시작 로직 (SkillDragHandler 호출)
        if (SkillDragHandler.Instance != null)
        {
            SkillDragHandler.Instance.ProcessBeginDrag(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (skillData == null || skillData.Type != SkillType.Active || skillData.Level <= 0) return;
        
        if (SkillDragHandler.Instance != null)
        {
            SkillDragHandler.Instance.ProcessDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (skillData == null || skillData.Type != SkillType.Active || skillData.Level <= 0) return;
        
        if (SkillDragHandler.Instance != null)
        {
            SkillDragHandler.Instance.ProcessEndDrag(eventData);
        }
    }
}