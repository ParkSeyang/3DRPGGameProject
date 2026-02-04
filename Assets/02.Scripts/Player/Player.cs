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

    // UI 업데이트를 위한 이벤트 정의
    public event Action<string> OnNameChanged;
    public event Action<float, float> OnHpChanged; // Current, Max
    public event Action<float, float> OnMpChanged; // Current, Max
    public event Action<int, int> OnExpChanged;    // Current, Max
    public event Action<int> OnLevelChanged;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnSpChanged; // SP 변경 알림
    public event Action OnStatChanged; // 기타 스탯(ATK, DEF 등) 변경 알림

    protected override void OnInitialize()
    {
        LoadPlayerData();
    }
    
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
            
            // 초기화 후 UI 갱신을 위해 이벤트 호출
            RefreshAllStats();
        }
        
    }

    public void RefreshAllStats()
    {
        OnNameChanged?.Invoke(Name);
        OnHpChanged?.Invoke(HP, MaxHP);
        OnMpChanged?.Invoke(MP, MaxMP);
        OnExpChanged?.Invoke(Exp, MaxExp);
        OnLevelChanged?.Invoke(Level);
        OnGoldChanged?.Invoke(Gold);
        OnStatChanged?.Invoke();
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
    public void SetHP(float value)
    {
        HP = Mathf.Clamp(value, 0, MaxHP);
        OnHpChanged?.Invoke(HP, MaxHP);
    }

    public void SetMP(float value)
    {
        MP = Mathf.Clamp(value, 0, MaxMP);
        OnMpChanged?.Invoke(MP, MaxMP);
    }

    public void SetLevel(int value)
    {
        Level = value;
        OnLevelChanged?.Invoke(Level);
    }

    public void SetExp(int value)
    {
        Exp = value;
        OnExpChanged?.Invoke(Exp, MaxExp);
    }

    public void SetMaxExp(int value)
    {
        MaxExp = value;
        OnExpChanged?.Invoke(Exp, MaxExp); // 최대 경험치가 바뀌면 비율도 바뀌므로 갱신
    }

    public void AddGold(int amount)
    {
        Gold = Mathf.Max(0, Gold + amount); // 골드는 음수가 될 수 없음
        OnGoldChanged?.Invoke(Gold);
    }
    
    public void AddExp(int amount) => Exp += amount; // 레벨업 체크는 Controller에서 수행. 여기서 이벤트 호출 안 함 (Controller가 SetExp 호출할 것임)

    // --- 스탯 변경 메서드 (PlayerStatusController 전용) ---
    public void AddBonusATK(float amount)
    {
        BonusATK += amount;
        OnStatChanged?.Invoke();
    }

    public void AddBonusDEF(float amount)
    {
        BonusDEF += amount;
        OnStatChanged?.Invoke();
    }

    public void AddMaxHP(float amount)
    {
        MaxHP += amount;
        HP = Mathf.Clamp(HP, 0, MaxHP); 
        OnHpChanged?.Invoke(HP, MaxHP);
    }

    public void AddMaxMP(float amount)
    {
        MaxMP += amount;
        MP = Mathf.Clamp(MP, 0, MaxMP);
        OnMpChanged?.Invoke(MP, MaxMP);
    }

    public bool UseSP(int amount)
    {
        if (SP >= amount)
        {
            SP -= amount;
            OnSpChanged?.Invoke(SP);
            return true;
        }
        return false;
    }

    public void AddSP(int amount)
    {
        SP += amount;
        Debug.Log($"[Cheat] SP {amount} 추가됨. 현재 SP: {SP}");
        OnSpChanged?.Invoke(SP);
    }

    public void ApplyStatData(PlayerStat stat)
    {
        if (stat == null) return;

        Name = stat.Name;
        HP = stat.HP;
        MP = stat.MP;
        ATK = stat.ATK;
        DEF = stat.DEF;
        SP = stat.SP;
        Level = stat.Level;
        Exp = stat.Exp;
        Gold = stat.Gold;

        // 로드된 데이터에 맞춰 최대 경험치 등 계산 로직 필요 시 추가
        MaxExp = Level * 100; 

        RefreshAllStats();
        Debug.Log($"[Player] 로드된 스탯 데이터 적용 완료 (Lv.{Level})");
    }
}

