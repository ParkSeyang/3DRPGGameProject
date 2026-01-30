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
            ui.Close();
            Debug.Log($"[UIManager] {ui.UIType} 등록됨");
        }
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
    }

    public void ToggleUI(UIType uiType)
    {
        if (uiDic.TryGetValue(uiType, out BaseUI targetUI) == false)
        {
            return;
        }

        if (targetUI.gameObject.activeSelf)
        {
            targetUI.Close();
            IsPopupOpen = false;
            SetControlState(true);
        }
        else
        {
            CloseAllPopup();
            targetUI.Open();
            IsPopupOpen = true;

            if (uiType == UIType.Menu)
            {
                Time.timeScale = 0f;
                SetControlState(false);
            }
        }


    }

    public void CloseAllPopup()
    {
        foreach (var ui in uiDic.Values)
        {
            ui.Close();
            IsPopupOpen = false;
            Time.timeScale = 1f;
            SetControlState(true);
        }
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
