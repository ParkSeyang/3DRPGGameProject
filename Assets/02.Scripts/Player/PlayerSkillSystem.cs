using UnityEngine;

public class PlayerSkillSystem : MonoBehaviour
{
    // Q, E 키에 매핑된 스킬 ID (0이면 없음)
    public int SkillSlot_Q = 0; 
    public int SkillSlot_E = 0;

    private PlayerSkillController skillController;

    private void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
    }

    private void Update()
    {
        // 쿨타임 감소 로직
        if (SkillDataManager.Instance != null)
        {
            foreach (var skill in SkillDataManager.Instance.GetAllSkills())
            {
                if (skill.Type == SkillType.Active)
                {
                    skill.UpdateCoolTime(Time.deltaTime);
                }
            }
        }

        // 입력 처리
        if (Input.GetKeyDown(KeyCode.Q)) UseSkill(SkillSlot_Q);
        if (Input.GetKeyDown(KeyCode.E)) UseSkill(SkillSlot_E);
    }

    // 스킬 장착 메서드
    public void EquipSkillToSlot(string key, int skillID)
    {
        if (SkillDataManager.Instance == null) return;

        if (SkillTreeSystem.Instance.IsSkillUnlocked(skillID) == false)
        {
            Debug.LogWarning("해금되지 않은 스킬입니다.");
            return;
        }

        var skill = SkillDataManager.Instance.GetSkill(skillID);
        if (skill == null || skill.Type != SkillType.Active)
        {
            Debug.LogWarning("액티브 스킬만 장착할 수 있습니다.");
            return;
        }
        
        if (skill.Level <= 0)
        {
            Debug.LogWarning("아직 배우지 않은 스킬입니다.");
            return;
        }

        if (key == "Q") SkillSlot_Q = skillID;
        else if (key == "E") SkillSlot_E = skillID;
        
        Debug.Log($"[{key}] 슬롯에 {skill.SkillName} 장착 완료");
    }

    private void UseSkill(int skillID)
    {
        if (skillID == 0) return;

        var skill = SkillDataManager.Instance.GetSkill(skillID);
        if (skill == null || skill.Level == 0) return; 

        // 쿨타임 체크
        if (skill.IsAvailable == false)
        {
            Debug.Log($"[Skill] 쿨타임 중... ({skill.CurrentCoolTime:F1}s)");
            return;
        }

        // 마나 체크
        if (Player.Instance.MP < skill.MpCost)
        {
            Debug.Log("[Skill] MP 부족");
            return;
        }

        // 실행 위임
        if (skillController != null)
        {
            // 자원 소모 및 쿨타임 시작
            Player.Instance.SetMP(Player.Instance.MP - skill.MpCost);
            skill.CurrentCoolTime = skill.CoolTime;

            string triggerName = (skillID == SkillSlot_Q) ? "SkillA" : "SkillB";
            skillController.ExecuteSkill(skill, triggerName);
        }
    }
}