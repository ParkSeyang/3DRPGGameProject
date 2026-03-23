using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : BaseUI
{
    public override UIType UIType => UIType.HUD;
    public override bool IsPopup => false; 

    [Header("Bars")]
    [SerializeField] private Image hpBar;
    [SerializeField] private Image mpBar;
    [SerializeField] private Image expBar;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;

    private void OnEnable()
    {
        // UI가 활성화되는 즉시 최신 데이터를 끌어옵니다.
        if (Player.Instance != null)
        {
            RegisterEvents();
            // Refresh는 BaseUI.Open()에서 호출됨
        }
    }

    private void OnDisable()
    {
        // 파괴되는 중이거나 인스턴스가 없을 때의 예외 처리 강화
        if (Player.IsInitialized == true && Player.Instance != null)
        {
            UnregisterEvents();
        }
    }

    private void RegisterEvents()
    {
        if (Player.Instance == null) return;
        
        Player.Instance.OnNameChanged += UpdateNameUI;
        Player.Instance.OnHpChanged += UpdateHpUI;
        Player.Instance.OnMpChanged += UpdateMpUI;
        Player.Instance.OnExpChanged += UpdateExpUI;
        Player.Instance.OnLevelChanged += UpdateLevelUI;
        Player.Instance.OnGoldChanged += UpdateGoldUI;
        Player.Instance.OnStatChanged += RefreshAll;
    }

    private void UnregisterEvents()
    {
        Player.Instance.OnNameChanged -= UpdateNameUI;
        Player.Instance.OnHpChanged -= UpdateHpUI;
        Player.Instance.OnMpChanged -= UpdateMpUI;
        Player.Instance.OnExpChanged -= UpdateExpUI;
        Player.Instance.OnLevelChanged -= UpdateLevelUI;
        Player.Instance.OnGoldChanged -= UpdateGoldUI;
        Player.Instance.OnStatChanged -= RefreshAll;
    }

    public override void Refresh()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        var p = Player.Instance;
        if (p == null) return;

        UpdateNameUI(p.Name);
        UpdateHpUI(p.HP, p.MaxHP);
        UpdateMpUI(p.MP, p.MaxMP);
        UpdateExpUI(p.Exp, p.MaxExp);
        UpdateLevelUI(p.Level);
        UpdateGoldUI(p.Gold);
    }

    private void UpdateNameUI(string newName)
    {
        if (nameText != null) nameText.text = newName;
    }

    private void UpdateHpUI(float current, float max)
    {
        if (hpBar != null) hpBar.fillAmount = current / max;
    }

    private void UpdateMpUI(float current, float max)
    {
        if (mpBar != null) mpBar.fillAmount = current / max;
    }

    private void UpdateExpUI(int current, int max)
    {
        if (expBar != null) expBar.fillAmount = (float)current / max;
    }

    private void UpdateLevelUI(int level)
    {
        if (levelText != null) levelText.text = $"Lv.{level}";
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null) goldText.text = $"{gold:#,0} G";
    }
}