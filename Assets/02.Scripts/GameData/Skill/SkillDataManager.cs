using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class SkillDataManager : SingletonBase<SkillDataManager>
{
    // 인스펙터에서 아이콘 리스트를 할당 (ID 순서대로 1번 스킬 -> 0번 인덱스, 2번 -> 1번...)
    [Header("Skill Icons (Index 0 = ID 1)")]
    public List<Sprite> skillIcons = new List<Sprite>();

    [Header("Skill Effects (Index 0 = ID 1)")]
    public List<GameObject> skillEffects = new List<GameObject>();

    public Dictionary<int, Skill> SkillTable { get; private set; } = new Dictionary<int, Skill>();

    protected override void OnInitialize()
    {
        LoadSkillData();
    }

    private void LoadSkillData()
    {
        SkillTable.Clear();
        string basePath = Path.Combine(Application.streamingAssetsPath, "TSVData");
        string path = Path.Combine(basePath, "SkillData.tsv");

        var infos = TSVReader.ReadTable<SkillInfo>(path);
        if (infos == null) return;

        foreach (var info in infos)
        {
            // 런타임 SO 생성
            Skill skill = ScriptableObject.CreateInstance<Skill>();
            
            skill.SkillID = info.SkillID;
            skill.SkillName = info.SkillName;
            
            // Enum 파싱 (대소문자 무시)
            if (Enum.TryParse(info.SkillType, true, out SkillType type))
            {
                skill.Type = type;
            }
            else
            {
                skill.Type = SkillType.Passive; // 기본값
            }

            skill.Level = info.SkillLevel; // 초기 레벨 (TSV에는 보통 0)
            skill.MaxLevel = info.MaxSkillLevel;
            skill.CoolTime = info.Skill_CoolTime;
            skill.MpCost = info.MP_Consumption;
            skill.Value = info.Value;
            skill.Description = info.Description;

            // 아이콘 연동 (ID 1 -> Index 0)
            int index = skill.SkillID - 1;
            if (index >= 0 && index < skillIcons.Count)
            {
                skill.Icon = skillIcons[index];
            }

            // 이펙트 프리팹 연동 (Active 스킬인 경우에만 할당)
            if (skill.Type == SkillType.Active && index >= 0 && index < skillEffects.Count)
            {
                skill.EffectPrefab = skillEffects[index];
            }
            else
            {
                skill.EffectPrefab = null; // Passive 스킬이거나 인덱스 범위를 벗어나면 null
            }

            if (SkillTable.ContainsKey(skill.SkillID) == false)
            {
                SkillTable.Add(skill.SkillID, skill);
            }
        }
    }

    public Skill GetSkill(int id)
    {
        if (SkillTable.TryGetValue(id, out var skill))
        {
            return skill;
        }
        return null;
    }

    public List<Skill> GetAllSkills()
    {
        return new List<Skill>(SkillTable.Values);
    }

    // --- Save & Load ---

    public List<SkillSaveData> GetSaveData()
    {
        var list = new List<SkillSaveData>();
        foreach (var skill in SkillTable.Values)
        {
            // 레벨이 0보다 큰(배운) 스킬만 저장
            if (skill.Level > 0)
            {
                list.Add(new SkillSaveData { SkillID = skill.SkillID, Level = skill.Level });
            }
        }
        return list;
    }

    public void LoadFromSaveData(List<SkillSaveData> data)
    {
        if (data == null) return;

        // 먼저 모든 스킬 레벨을 0으로 초기화 (데이터 꼬임 방지)
        foreach (var skill in SkillTable.Values)
        {
            skill.Level = 0;
        }

        foreach (var saveItem in data)
        {
            if (SkillTable.TryGetValue(saveItem.SkillID, out var skill))
            {
                skill.Level = saveItem.Level;
            }
        }
    }
}

    