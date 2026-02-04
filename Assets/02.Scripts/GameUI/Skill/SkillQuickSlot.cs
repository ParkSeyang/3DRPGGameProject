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

    public void SetSkill(int skillId)
    {
        this.currentSkillId = skillId;
        
        if (SkillDataManager.Instance == null) return;
        var skill = SkillDataManager.Instance.GetSkill(skillId);
        
        if (skill != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = skill.Icon;
                iconImage.enabled = true;
            }
            
            // 실제 PlayerSystem에 장착 요청
            if (Player.Instance != null)
            {
                Player.Instance.GetComponent<PlayerSkillSystem>()?.EquipSkillToSlot(KeyName, skillId);
            }
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }
        }
    }
}