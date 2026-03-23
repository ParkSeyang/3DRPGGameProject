using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : BaseUI
{
    [Header("Panels")]
    [SerializeField] private GameObject normalMenuPanel;   // 기존 MenuUI
    [SerializeField] private GameObject gameOverMenuPanel; // 신규 GameOverMenuUI

    [Header("Normal Menu Buttons")] 
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button menuButton;   // [추가] 타이틀로 이동 버튼
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    [Header("GameOver Menu Buttons")]
    [SerializeField] private Button goMenuButton; // 타이틀로 이동
    [SerializeField] private Button goLoadButton; // 세이브 로드
    [SerializeField] private Button goExitButton; // 게임 종료

    private bool isGameOver = false;

    // 시간 제어 상수
    private const float TimeScalePaused = 0f;
    private const float TimeScaleNormal = 1f;

    public override UIType UIType => UIType.Menu;

    protected override void Awake()
    {
        base.Awake(); 
        // 시작 시 두 패널 모두 비활성화
        if (normalMenuPanel != null) normalMenuPanel.SetActive(false);
        if (gameOverMenuPanel != null) gameOverMenuPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // 일반 메뉴 버튼 리스너
        resumeButton?.onClick.AddListener(OnResumeButtonClicked);
        saveButton?.onClick.AddListener(OnSaveButtonClicked);
        loadButton?.onClick.AddListener(OnLoadButtonClicked);
        menuButton?.onClick.AddListener(OnMenuButtonClicked); // [추가]
        exitButton?.onClick.AddListener(OnExitButtonClicked);

        // 게임오버 메뉴 버튼 리스너
        goMenuButton?.onClick.AddListener(OnMenuButtonClicked);
        goLoadButton?.onClick.AddListener(OnLoadButtonClicked);
        goExitButton?.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnDisable()
    {
        resumeButton?.onClick.RemoveListener(OnResumeButtonClicked);
        saveButton?.onClick.RemoveListener(OnSaveButtonClicked);
        loadButton?.onClick.RemoveListener(OnLoadButtonClicked);
        menuButton?.onClick.RemoveListener(OnMenuButtonClicked); // [추가]
        exitButton?.onClick.RemoveListener(OnExitButtonClicked);

        goMenuButton?.onClick.RemoveListener(OnMenuButtonClicked);
        goLoadButton?.onClick.RemoveListener(OnLoadButtonClicked);
        goExitButton?.onClick.RemoveListener(OnExitButtonClicked);
    }

    /// <summary>
    /// 플레이어 사망 시 외부에서 호출되어 게임 오버 전용 UI를 엽니다.
    /// </summary>
    public void SetGameOverMode()
    {
        isGameOver = true;
        Open();
    }

    public override void Open()
    {
        base.Open();
        
        // 상태에 따라 표시할 패널 결정
        if (isGameOver == true)
        {
            if (normalMenuPanel != null) normalMenuPanel.SetActive(false);
            if (gameOverMenuPanel != null) gameOverMenuPanel.SetActive(true);
        }
        else
        {
            if (normalMenuPanel != null) normalMenuPanel.SetActive(true);
            if (gameOverMenuPanel != null) gameOverMenuPanel.SetActive(false);
        }

        // [최적화] 시간 정지 및 커서 활성화는 UIManager가 전역적으로 처리하므로 여기서 중복 호출하지 않습니다.
    }

    public override void Close()
    {
        base.Close();
        if (normalMenuPanel != null) normalMenuPanel.SetActive(false);
        if (gameOverMenuPanel != null) gameOverMenuPanel.SetActive(false);
        
        // 닫힐 때 게임 오버 플래그 리셋
        isGameOver = false; 
        // [최적화] 시간 복구 로직 역시 UIManager에게 위임합니다.
    }

    private void OnResumeButtonClicked()
    {
        UIManager.Instance.CloseAllPopup();
    }

    private void OnSaveButtonClicked()
    {
        if (DataManager.Instance != null && DataManager.Instance.CanSave == false)
        {
            UIManager.Instance.ShowWarning("현재 위치에서는 저장이 불가능합니다.");
            return;
        }
        
        // 실시간 데이터 추출 및 저장
        UserSaveData saveData = PlayerStatusController.Instance.GetSaveData();
        DataManager.Instance.SaveUserData(saveData);
        UIManager.Instance.ShowWarning("게임이 저장되었습니다.");
    }

    private void OnLoadButtonClicked()
    {
        if (GameSceneManager.Instance != null)
        {
            // [수정] 정지된 시간을 먼저 풀고 로직 진행
            Time.timeScale = TimeScaleNormal;
            UIManager.Instance.CloseAllPopup();
            GameSceneManager.Instance.LoadGame();
        }
    }

    private void OnMenuButtonClicked()
    {
        if (GameSceneManager.Instance != null)
        {
            // [수정] 타이틀 전용 로딩 루틴 호출
            GameSceneManager.Instance.ReturnToTitle();
        }
    }

    private void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
