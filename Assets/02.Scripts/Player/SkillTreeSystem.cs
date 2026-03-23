using UnityEngine;

public class SkillTreeSystem : SingletonBase<SkillTreeSystem>
{
    public bool IsSkillUnlocked(int skillId)
    {
        if (SkillDataManager.Instance == null) return false;

        if (skillId == 1 || skillId == 3) return true;

        switch (skillId)
        {
            case 2:
                var skill1 = SkillDataManager.Instance.GetSkill(1);
                return skill1 != null && skill1.Level >= 4;
            case 4:
                var skill3 = SkillDataManager.Instance.GetSkill(3);
                return skill3 != null && skill3.Level >= 4;
        }

        return false;
    }

    public bool TryLevelUp(int skillId)
    {
        if (SkillDataManager.Instance == null || Player.Instance == null) return false;

        if (IsSkillUnlocked(skillId) == false)
        {
            UIManager.Instance.ShowWarning("선행 스킬 조건을 만족하지 못해 잠겨있습니다.");
            return false;
        }

        var skill = SkillDataManager.Instance.GetSkill(skillId);
        if (skill == null) return false;

        if (skill.IsMaxLevel)
        {
            UIManager.Instance.ShowWarning("이미 최대 레벨입니다.");
            return false;
        }

        int cost = 1;
        if (Player.Instance.SP < cost)
        {
            UIManager.Instance.ShowWarning("기술 포인트(SP)가 부족합니다.");
            return false;
        }

        if (Player.Instance.UseSP(cost))
        {
            skill.Level++;
            if (skill.Type == SkillType.Passive)
            {
                PlayerStatusController.Instance.UpdatePassiveStats();
            }
            return true;
        }

        return false;
    }

    public bool TryLevelDown(int skillId)
    {
        if (SkillDataManager.Instance == null || Player.Instance == null) return false;

        var skill = SkillDataManager.Instance.GetSkill(skillId);
        if (skill == null || skill.Level <= 0) return false;

        skill.Level--;
        Player.Instance.AddSP(1);

        if (skill.Type == SkillType.Passive)
        {
            PlayerStatusController.Instance.UpdatePassiveStats();
        }

        return true;
    }
}