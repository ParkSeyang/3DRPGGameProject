using UnityEngine;
using TMPro;

public class StatUI : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI goldText;

    private void OnEnable()
    {
        // UI가 켜질 때 이벤트를 연결하고 데이터를 갱신합니다.
        if (Player.Instance != null)
        {
            RegisterEvents();
            RefreshAll();
        }
    }

    private void OnDisable()
    {
        // UI가 꺼질 때 이벤트 연결을 해제합니다.
        if (Player.IsInitialized && Player.Instance != null)
        {
            UnregisterEvents();
        }
    }

    private void RegisterEvents()
    {
        Player.Instance.OnHpChanged += UpdateHp;
        Player.Instance.OnMpChanged += UpdateMp;
        Player.Instance.OnLevelChanged += UpdateLevel;
        Player.Instance.OnExpChanged += UpdateExp;
        Player.Instance.OnGoldChanged += UpdateGold;
        Player.Instance.OnNameChanged += UpdateName;
        Player.Instance.OnStatChanged += UpdateStat;
    }

    private void UnregisterEvents()
    {
        Player.Instance.OnHpChanged -= UpdateHp;
        Player.Instance.OnMpChanged -= UpdateMp;
        Player.Instance.OnLevelChanged -= UpdateLevel;
        Player.Instance.OnExpChanged -= UpdateExp;
        Player.Instance.OnGoldChanged -= UpdateGold;
        Player.Instance.OnNameChanged -= UpdateName;
        Player.Instance.OnStatChanged -= UpdateStat;
    }

    public void RefreshAll()
    {
        var player = Player.Instance;
        
        UpdateName(player.Name);
        UpdateLevel(player.Level);
        UpdateHp(player.HP, player.MaxHP);
        UpdateMp(player.MP, player.MaxMP);
        UpdateExp(player.Exp, player.MaxExp);
        UpdateGold(player.Gold);
        
        // 공격력과 방어력은 이벤트가 없으므로 직접 값을 가져와 갱신
        UpdateAtkDef(player.ATK, player.BonusATK, player.DEF, player.BonusDEF);
    }
    
    private void UpdateStat()
    {
        // 스탯 변경 이벤트가 발생하면 전체 갱신 (또는 ATK/DEF만 갱신)
        RefreshAll();
    }

    // --- Update Methods ---

    private void UpdateName(string newName)
    {
        if (nameText != null) nameText.text = newName;
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"Lv.{level}";
    }

    private void UpdateHp(float current, float max)
    {
        if (hpText != null) hpText.text = $"{current:F0} / {max:F0}";
    }

    private void UpdateMp(float current, float max)
    {
        if (mpText != null) mpText.text = $"{current:F0} / {max:F0}";
    }

    private void UpdateExp(int current, int max)
    {
        if (expText != null)
        {
            float percentage = max > 0 ? (float)current / max * 100f : 0f;
            expText.text = $"{percentage:F2}%";
        }
    }

    private void UpdateGold(int gold)
    {
        if (goldText != null) goldText.text = $"{gold:#,0} G";
    }

    private void UpdateAtkDef(float atk, float bonusAtk, float def, float bonusDef)
    {
        if (atkText != null)
        {
            // (기본 + 보너스) 형태로 표시하거나 합산해서 표시
            float totalAtk = atk + bonusAtk;
            atkText.text = $"{totalAtk:F0}"; 
        }

        if (defText != null)
        {
            float totalDef = def + bonusDef;
            defText.text = $"{totalDef:F0}";
        }
    }
}