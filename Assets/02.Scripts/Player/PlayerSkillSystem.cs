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
        // 타이틀 씬 혹은 팝업 중 조작 차단
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isTitle = sceneName.Contains("StartGame") || sceneName.Contains("GameStart");

        if (isTitle || (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen))
        {
            return;
        }

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
            return;
        }

        var skill = SkillDataManager.Instance.GetSkill(skillID);
        if (skill == null || skill.Type != SkillType.Active)
        {
            return;
        }
        
        if (skill.Level <= 0)
        {
            return;
        }

        if (key == "Q")
        {
            SkillSlot_Q = skillID;
        }
        else if (key == "E")
        {
            SkillSlot_E = skillID;
        }
        
    }

    private void UseSkill(int skillID)
    {
        if (skillID == 0)
        {
            return;
        }

        var skill = SkillDataManager.Instance.GetSkill(skillID);
        if (skill == null || skill.Level == 0)
        {
            return;
        }

        // 쿨타임 체크
        if (skill.IsAvailable == false)
        {
            return;
        }

        // 마나 체크
        if (Player.Instance.MP < skill.MpCost)
        {
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

    