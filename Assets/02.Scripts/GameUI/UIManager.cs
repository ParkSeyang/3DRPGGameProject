using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    private Dictionary<UIType, BaseUI> uiDic = new Dictionary<UIType, BaseUI>();

    public bool IsPopupOpen { get; private set; }

    public void RegisterUI(BaseUI ui)
    {
        if (uiDic.ContainsKey(ui.UIType) == false)
        {
            uiDic.Add(ui.UIType, ui);
            Debug.Log($"[UIManager] {ui.UIType} 등록됨");
        }
    }

    protected override void OnInitialize()
    {
        // 씬 내의 모든 BaseUI(비활성 포함) 검색 및 등록
        BaseUI[] allUIs = FindObjectsOfType<BaseUI>(true);
        foreach (var ui in allUIs)
        {
            RegisterUI(ui);
        }

        // 초기 상태: HUD와 QuickSlot만 켜고 나머지는 끈다
        foreach (var pair in uiDic)
        {
            if (pair.Key == UIType.HUD || pair.Key == UIType.QuickSlot)
            {
                pair.Value.Open();
            }
            else
            {
                pair.Value.Close();
            }
        }
        
        RefreshUIState();
    }

    private void Start()
    {
        if (uiDic.Count == 0)
        {
            OnInitialize();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPopupOpen) CloseAllPopup();
            else ToggleUI(UIType.Menu);
        }

        if (Input.GetKeyDown(KeyCode.K)) ToggleUI(UIType.Skill);
        if (Input.GetKeyDown(KeyCode.I)) ToggleUI(UIType.Inventory);
    }

    public void ToggleUI(UIType uiType)
    {
        if (uiDic.TryGetValue(uiType, out BaseUI targetUI) == false)
        {
            Debug.LogWarning($"[UIManager] {uiType} UI를 찾을 수 없습니다. 등록 여부를 확인하세요.");
            return;
        }

        // 현재 켜져 있으면 끄고, 꺼져 있으면 연다
        if (targetUI.gameObject.activeSelf)
        {
            targetUI.Close();
            // 인벤토리를 닫을 때 장비창도 같이 닫음
            if (uiType == UIType.Inventory) SetUIActive(UIType.Equip, false);
        }
        else
        {
            // 인벤토리나 스킬창을 열 때 기존의 다른 팝업들은 닫는다 (선택 사항)
            CloseAllPopup(); 
            
            targetUI.Open();
            // 인벤토리를 열 때 장비창도 같이 엶
            if (uiType == UIType.Inventory) SetUIActive(UIType.Equip, true);
        }

        RefreshUIState();
    }

    public void CloseAllPopup()
    {
        // 리스트를 복사해서 순회 (Dictionary 수정 중 오류 방지)
        foreach (var ui in uiDic.Values)
        {
            if (ui.UIType == UIType.HUD || ui.UIType == UIType.QuickSlot)
            {
                continue; // HUD와 퀵슬롯은 팝업이 아님
            }
            ui.Close();
        }

        RefreshUIState();
    }

    private void RefreshUIState()
    {
        bool isInventoryOpen = IsUIOpen(UIType.Inventory);
        bool isEquipOpen = IsUIOpen(UIType.Equip);
        bool isTradeOpen = IsUIOpen(UIType.Trade);
        bool isSkillOpen = IsUIOpen(UIType.Skill);
        bool isMenuOpen = IsUIOpen(UIType.Menu);

        // 팝업 중 하나라도 열려있는지 체크
        IsPopupOpen = isInventoryOpen || isEquipOpen || isTradeOpen || isSkillOpen || isMenuOpen;

        // 1. HUD: 팝업이 하나라도 열리면 끈다
        SetUIActive(UIType.HUD, IsPopupOpen == false);

        // 2. QuickSlot: 인벤토리나 상점이 열렸을 때만 HUD와 상관없이 보여준다
        // 메뉴나 스킬창에서는 꺼지도록 설정
        bool showQuickSlot = (IsPopupOpen == false) || isInventoryOpen || isEquipOpen || isTradeOpen;
        if (isMenuOpen || isSkillOpen) 
        {
            showQuickSlot = false;
        }
        SetUIActive(UIType.QuickSlot, showQuickSlot);

        // 3. 시간 및 커서 제어
        if (IsPopupOpen)
        {
            Time.timeScale = 0f;
            SetControlState(false); // 커서 보임
        }
        else
        {
            Time.timeScale = 1f;
            SetControlState(true); // 커서 숨김
        }
    }

    private bool IsUIOpen(UIType type)
    {
        return uiDic.ContainsKey(type) && uiDic[type].gameObject.activeSelf;
    }

    private void SetUIActive(UIType type, bool isActive)
    {
        if (uiDic.TryGetValue(type, out var ui))
        {
            if (isActive && ui.gameObject.activeSelf == false) ui.Open();
            else if (isActive == false && ui.gameObject.activeSelf) ui.Close();
        }
    }

    private void SetControlState(bool canControl)
    {
        if (canControl)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}