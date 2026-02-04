using UnityEngine;

public enum SkillType
{
    Passive,
    Active
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
public class Skill : ScriptableObject
{
    public int SkillID;
    public string SkillName;
    public SkillType Type;
    public int Level;
    public int MaxLevel;
    public float CoolTime;
    public float MpCost;
    public float Value; // 공격 계수(Active) 또는 스탯 증가량(Passive)
    public Sprite Icon;
    public GameObject EffectPrefab; // 스킬 사용 시 소환할 이펙트 프리팹
    [TextArea] public string Description;
    
    // 런타임 변수
    public float CurrentCoolTime; 
    
    // 런타임 편의 기능
    public bool IsMaxLevel => Level >= MaxLevel;
    public bool IsAvailable => CurrentCoolTime <= 0;
    
    public void UpdateCoolTime(float deltaTime)
    {
        if (CurrentCoolTime > 0)
        { 
            CurrentCoolTime -= deltaTime; 
            if (CurrentCoolTime < 0) CurrentCoolTime = 0;
        }
    }
        
        // 현재 레벨에 따른 총 효과 값 반환
    public float GetCurrentValue()
    {
        return Value * Level;
    }
}

// TSV 매핑용 클래스 (CsvHelper가 프로퍼티에 값을 넣어줌)
public class SkillInfo
{
    public int SkillID { get; set; }
    public string SkillName { get; set; }
    public string SkillType { get; set; } 
    public int SkillLevel { get; set; }
    public int MaxSkillLevel { get; set; }
    public float Skill_CoolTime { get; set; }
    public float MP_Consumption { get; set; }
    public float Value { get; set; }
    public string Description { get; set; }
    // Icon은 TSV에 경로가 있어도 되지만, 여기서는 인스펙터 연동을 위해 제외
}