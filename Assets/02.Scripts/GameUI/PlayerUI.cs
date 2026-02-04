using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : BaseUI
{
    public override UIType UIType => UIType.HUD;

    [Header("Bars")]
    [SerializeField] private Image hpBar;
    [SerializeField] private Image mpBar;
    [SerializeField] private Image expBar;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        // Player의 이벤트 구독
        if (Player.Instance != null)
        {
            Player.Instance.OnNameChanged += UpdateNameUI;
            Player.Instance.OnHpChanged += UpdateHpUI;
            Player.Instance.OnMpChanged += UpdateMpUI;
            Player.Instance.OnExpChanged += UpdateExpUI;
            Player.Instance.OnLevelChanged += UpdateLevelUI;
            Player.Instance.OnGoldChanged += UpdateGoldUI;
            Player.Instance.OnStatChanged += RefreshAll;

            // 초기값 갱신
            RefreshAll();
        }
        
        // HUD는 기본적으로 열려있어야 함
        Open();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (Player.Instance != null)
        {
            Player.Instance.OnNameChanged -= UpdateNameUI;
            Player.Instance.OnHpChanged -= UpdateHpUI;
            Player.Instance.OnMpChanged -= UpdateMpUI;
            Player.Instance.OnExpChanged -= UpdateExpUI;
            Player.Instance.OnLevelChanged -= UpdateLevelUI;
            Player.Instance.OnGoldChanged -= UpdateGoldUI;
            Player.Instance.OnStatChanged -= RefreshAll;
        }
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