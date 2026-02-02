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
            
            // HUD는 닫지 않고 유지, 나머지 팝업들은 닫기
            if (ui.UIType != UIType.HUD)
            {
                ui.Close();
            }
            
            Debug.Log($"[UIManager] {ui.UIType} 등록됨");
        }
    }

    private void Start()
    {
        // 게임 시작 시 초기화: HUD만 켜고 나머지는 모두 닫기
        CloseAllPopup();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPopupOpen)
            {
                CloseAllPopup();
            }
            else
            {
                ToggleUI(UIType.Menu);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleUI(UIType.Skill);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleUI(UIType.Inventory);
        }
    }

    public void ToggleUI(UIType uiType)
    {
        if (uiDic.TryGetValue(uiType, out BaseUI targetUI) == false)
        {
            return;
        }

        // HUD는 토글 대상이 아님 (항상 켜져있거나 다른 UI에 의해 제어됨)
        if (uiType == UIType.HUD) return;

        if (targetUI.gameObject.activeSelf)
        {
            targetUI.Close();
            IsPopupOpen = false;
            SetControlState(true);
            
            // 팝업이 닫히면 HUD 다시 켜기
            if (uiDic.TryGetValue(UIType.HUD, out BaseUI hud))
            {
                hud.Open();
            }
        }
        else
        {
            CloseAllPopup();
            targetUI.Open();
            IsPopupOpen = true;

            // 팝업이 열리면 HUD 끄기
            if (uiDic.TryGetValue(UIType.HUD, out BaseUI hud))
            {
                hud.Close();
            }

            if (uiType == UIType.Menu || uiType == UIType.Inventory)
            {
                // 인벤토리 열 때 시간은 멈추지 않음 (기획에 따라 다름, 여기선 메뉴만 멈춤)
                if(uiType == UIType.Menu) Time.timeScale = 0f;
                
                SetControlState(false); // 커서 보이기
            }
        }
    }

    public void CloseAllPopup()
    {
        // 모든 UI를 순회하며 처리
        foreach (var ui in uiDic.Values)
        {
            if (ui.UIType == UIType.HUD)
            {
                ui.Open(); // HUD는 켠다
            }
            else
            {
                ui.Close(); // 나머지는 다 끈다
            }
        }
        
        IsPopupOpen = false;
        Time.timeScale = 1f;
        SetControlState(true);
    }

    private void SetControlState(bool canControl)
    {
        if (canControl == true)
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
