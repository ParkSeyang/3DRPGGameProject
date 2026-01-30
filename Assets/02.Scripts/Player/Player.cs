using UnityEngine;
using System;
using System.Collections.Generic;
public class Player : SingletonBase<Player>
{
    [Header("Data Settings")] 
    [SerializeField] private string className = "ZeroDarkMos";
    // 스탯등 로직 정리
    public string Name { get; private set; }
    public float HP { get; private set; }
    public float MaxHP { get; private set; }
    
    public float MP { get; private set; }
    public float MaxMP { get; private set; }
    
    public float ATK { get; private set; }
    public float BonusATK { get; private set; }
    
    public float DEF { get; private set; }
    public float BonusDEF { get; private set; }
    
    public int SP { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int MaxExp { get; private set; }
    public int Gold { get; private set; }

    protected override void OnInitialize() => LoadPlayerData();
    
    private void LoadPlayerData()
    {
        if (DataManager.Instance == null)
        {
            Debug.Log("로드할 데이터가 없음.");
            return;
        }

        var stat = DataManager.Instance.GetPlayerStat(className);
        if (stat != null)
        {
            Name = stat.Name;
            HP = stat.HP;
            MaxHP = stat.HP;
            MP = stat.MP;
            MaxMP = stat.MP;
            ATK = stat.ATK;
            DEF = stat.DEF;

            BonusATK = 0f;
            BonusDEF = 0f;
            
            SP = stat.SP;
            Level = stat.Level;
            Exp = stat.Exp;
            MaxExp = Level * 100;
            Gold = stat.Gold;

            Debug.Log($"[Player] 데이터 로드 성공\n" +
                               $"이름: {Name}, 레벨: {Level}, 골드: {Gold}\n" +
                                 $"HP: {HP}/{MaxHP}, MP: {MP}/{MaxMP}\n" +
                                         $"공격력: {ATK}, 방어력: {DEF}");
        }
        
    }

    public PlayerStat GetCurrentStatData() => new PlayerStat()
    {
        Name = this.Name,
        HP = this.HP,
        MP = this.MP,
        ATK = this.ATK,
        DEF = this.DEF,
        SP = this.SP,
        Level = this.Level,
        Exp = this.Exp,
        Gold = this.Gold
    };

    // --- 값 변경 메서드 (PlayerStatusController 등에서 사용) ---
    public void SetHP(float value) => HP = Mathf.Clamp(value, 0, MaxHP);
    public void SetMP(float value) => MP = Mathf.Clamp(value, 0, MaxMP);
    
    public void SetLevel(int value) => Level = value;
    public void SetExp(int value) => Exp = value;
    public void SetMaxExp(int value) => MaxExp = value;
    
    public void AddGold(int amount) => Gold = Mathf.Max(0, Gold + amount); // 골드는 음수가 될 수 없음
    public void AddExp(int amount) => Exp += amount; // 레벨업 체크는 Controller에서 수행

}
