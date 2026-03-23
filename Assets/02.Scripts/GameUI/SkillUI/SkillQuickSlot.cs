using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillQuickSlot : MonoBehaviour
{
    public string KeyName; // "Q" or "E" (인스펙터에서 설정)
    public Image iconImage;
    public Image coolTimeImage; // 쿨타임 표시용 (fillAmount), 없으면 null 처리
    public TextMeshProUGUI coolTimeText; // 쿨타임 텍스트

    private int currentSkillId;

    private void Update()
    {
        // 쿨타임 표시 로직
        if (currentSkillId != 0 && SkillDataManager.Instance != null)
        {
            var skill = SkillDataManager.Instance.GetSkill(currentSkillId);
            if (skill != null)
            {
                // 이미지 갱신
                if (coolTimeImage != null)
                {
                    if (skill.CoolTime > 0)
                        coolTimeImage.fillAmount = skill.CurrentCoolTime / skill.CoolTime;
                    else
                        coolTimeImage.fillAmount = 0;
                }

                // 텍스트 갱신
                if (coolTimeText != null)
                {
                    if (skill.CurrentCoolTime > 0)
                    {
                        if (!coolTimeText.gameObject.activeSelf) coolTimeText.gameObject.SetActive(true);
                        
                        // 1초 미만은 소수점, 그 이상은 정수로 표시하는 등 가독성 향상 가능
                        // 여기선 10초 미만일 때만 소수점 표시 예시
                        if (skill.CurrentCoolTime < 10f)
                            coolTimeText.text = skill.CurrentCoolTime.ToString("F1");
                        else
                            coolTimeText.text = skill.CurrentCoolTime.ToString("F0");
                    }
                    else
                    {
                        if (coolTimeText.gameObject.activeSelf) coolTimeText.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        // 씬이 로드되거나 UI가 켜질 때, PlayerSkillSystem에 저장된 ID를 기반으로 UI 복원
        if (Player.Instance != null)
        {
            var skillSystem = Player.Instance.GetComponent<PlayerSkillSystem>();
            if (skillSystem != null)
            {
                int savedId = (KeyName == "Q") ? skillSystem.SkillSlot_Q : skillSystem.SkillSlot_E;
                
                // [보강] 복원 시에도 규칙 검증 (Q: 2, E: 4)
                if (IsValidSkillForSlot(savedId) == true)
                {
                    RefreshSlotUI(savedId);
                }
                else if (savedId != 0)
                {
                    // 규칙에 어긋나는 저장 데이터가 있다면 초기화
                    skillSystem.EquipSkillToSlot(KeyName, 0);
                    RefreshSlotUI(0);
                }
            }
        }
    }

    private void RefreshSlotUI(int skillId)
    {
        this.currentSkillId = skillId;

        // ID가 0인 경우(해제) 처리
        if (skillId == 0)
        {
            if (iconImage != null) iconImage.enabled = false;
            return;
        }

        if (SkillDataManager.Instance == null) return;
        var skill = SkillDataManager.Instance.GetSkill(skillId);
        
        if (skill != null && iconImage != null)
        {
            iconImage.sprite = skill.Icon;
            iconImage.enabled = true;
        }
    }

    public void SetSkill(int skillId)
    {
        // [핵심] 특정 슬롯에 특정 스킬만 허용 (Q: 2, E: 4)
        if (IsValidSkillForSlot(skillId) == false)
        {
            string slotName = (KeyName == "Q") ? "Q" : "E";
            int requiredId = (KeyName == "Q") ? 2 : 4;
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWarning($"{slotName} 슬롯에는 {requiredId}번 스킬만 등록할 수 있습니다.");
            }
            return;
        }

        RefreshSlotUI(skillId);
        
        // 실제 PlayerSystem에 장착 요청
        if (Player.Instance != null)
        {
            Player.Instance.GetComponent<PlayerSkillSystem>()?.EquipSkillToSlot(KeyName, skillId);
        }
    }

    private bool IsValidSkillForSlot(int skillId)
    {
        // 0은 해제 상태이므로 항상 허용
        if (skillId == 0) return true;

        if (KeyName == "Q") return skillId == 2;
        if (KeyName == "E") return skillId == 4;

        return true; // 그 외 슬롯(정의되지 않은 경우)은 기본 허용
    }
}