using UnityEngine;

public class SkillTreeSystem : SingletonBase<SkillTreeSystem>
{
    // 스킬 해금 여부 확인
    public bool IsSkillUnlocked(int skillId)
    {
        if (SkillDataManager.Instance == null) return false;

        // 기본 스킬 (선행 조건 없음)
        if (skillId == 1 || skillId == 3) return true;

        // 파생 스킬 해금 조건 체크
        switch (skillId)
        {
            case 2: // SwordSlash -> StrongBody(1) Lv.4 이상
                var skill1 = SkillDataManager.Instance.GetSkill(1);
                return skill1 != null && skill1.Level >= 4;

            case 4: // EarthShatter -> GreatSwordTraining(3) Lv.4 이상
                var skill3 = SkillDataManager.Instance.GetSkill(3);
                return skill3 != null && skill3.Level >= 4;
        }

        return false; // 그 외 정의되지 않은 스킬은 잠금 처리 (혹은 기본 true)
    }

    // 스킬 포인트(SP) 확인 및 레벨업 처리
    public bool TryLevelUp(int skillId)
    {
        if (SkillDataManager.Instance == null || Player.Instance == null) return false;

        // 0. 해금 조건 체크
        if (IsSkillUnlocked(skillId) == false)
        {
            Debug.Log("[SkillTree] 선행 스킬 조건을 만족하지 못해 잠겨있습니다.");
            return false;
        }

        var skill = SkillDataManager.Instance.GetSkill(skillId);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillTree] 존재하지 않는 스킬 ID: {skillId}");
            return false;
        }

        // 1. 만렙 체크
        if (skill.IsMaxLevel)
        {
            Debug.Log("[SkillTree] 이미 최대 레벨입니다.");
            return false;
        }

        // 2. SP 체크 (기본 비용 1)
        int cost = 1; 
        if (Player.Instance.SP < cost)
        {
            Debug.Log("[SkillTree] SP가 부족합니다.");
            return false;
        }

        // 3. 레벨업 실행
        if (Player.Instance.UseSP(cost))
        {
            skill.Level++;
            Debug.Log($"[SkillTree] {skill.SkillName} 레벨 업! (Lv.{skill.Level})");

            // 4. 패시브라면 스탯 즉시 갱신
            if (skill.Type == SkillType.Passive)
            {
                PlayerStatusController.Instance.UpdatePassiveStats();
            }
            return true;
        }

        return false;
    }

    // 스킬 레벨 다운 (SP 반환)
    public bool TryLevelDown(int skillId)
    {
        if (SkillDataManager.Instance == null || Player.Instance == null) return false;

        var skill = SkillDataManager.Instance.GetSkill(skillId);
        if (skill == null || skill.Level <= 0) return false;

        // 레벨 다운 실행
        skill.Level--;
        
        // SP 반환 (비용 1)
        int refund = 1;
        Player.Instance.AddSP(refund);
        
        Debug.Log($"[SkillTree] {skill.SkillName} 레벨 다운 (Lv.{skill.Level}) - SP {refund} 반환");

        // 패시브라면 스탯 갱신
        if (skill.Type == SkillType.Passive)
        {
            PlayerStatusController.Instance.UpdatePassiveStats();
        }

        return true;
    }
}