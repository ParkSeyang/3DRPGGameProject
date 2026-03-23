using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : BaseUI
{
    public override UIType UIType => UIType.Title;
    public override bool IsPopup => false;

    [Header("Title Resources")]
    [SerializeField] private GameObject titleCamera; // 타이틀 전용 카메라
    [SerializeField] private GameObject gameTitleCanvas;
    [SerializeField] private GameObject manualUICanvas;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button manualButton;
    [SerializeField] private Button exitButton;

    private bool isManualOpen = false;
    private int openFrame = 0;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 리스너 연결
        startButton?.onClick.AddListener(() => 
        {
            HideTitleImmediate();
            GameSceneManager.Instance.StartNewGame();
        });

        loadButton?.onClick.AddListener(() => 
        {
            HideTitleImmediate();
            GameSceneManager.Instance.LoadGame();
        });

        manualButton?.onClick.AddListener(OpenManual);
        exitButton?.onClick.AddListener(() => GameSceneManager.Instance.QuitGame());
    }

    public override void Open()
    {
        // [보강] DontDestroyOnLoad로 인해 기존 참조가 유실되었을 경우(다른 씬 갔다 왔을 때) 재연결
        if (titleCamera == null || titleCamera.gameObject == null)
        {
            // 이름이나 태그로 타이틀 카메라를 다시 찾음
            var cam = GameObject.Find("TitleCamera"); 
            if (cam != null) titleCamera = cam;
        }

        base.Open();
        if (titleCamera != null) titleCamera.SetActive(true);
        if (gameTitleCanvas != null) gameTitleCanvas.SetActive(true);
        if (manualUICanvas != null) manualUICanvas.SetActive(false);
    }

    public override void Close()
    {
        base.Close();
        HideTitleImmediate();
    }

    private void HideTitleImmediate()
    {
        if (titleCamera != null) titleCamera.SetActive(false);
        if (gameTitleCanvas != null) gameTitleCanvas.SetActive(false);
        if (manualUICanvas != null) manualUICanvas.SetActive(false);
    }

    private void Update()
    {
        if (isManualOpen == true && Time.frameCount > openFrame)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                CloseManual();
            }
        }
    }

    public void OpenManual()
    {
        isManualOpen = true;
        openFrame = Time.frameCount;
        if (gameTitleCanvas != null) gameTitleCanvas.SetActive(false);
        if (manualUICanvas != null) manualUICanvas.SetActive(true);
    }

    public void CloseManual()
    {
        isManualOpen = false;
        if (gameTitleCanvas != null) gameTitleCanvas.SetActive(true);
        if (manualUICanvas != null) manualUICanvas.SetActive(false);
    }
}
