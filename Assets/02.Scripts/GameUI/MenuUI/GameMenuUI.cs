using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ParkSeyang
{
    public class GameMenuUI : BaseUI
    {
        [Header("UI Components")] 
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button exitButton;

        private bool isMenuOpen = false;
        private PlayerController playerController;

        // 매직 넘버 제거를 위한 상수 선언
        private const float TimeScalePaused = 0f;
        private const float TimeScaleNormal = 1f;

        public override UIType UIType => UIType.Menu;

        protected override void Awake()
        {
            base.Awake(); // UIManager 등록

            // 버튼 리스너 등록 - 본문 표현식 활용
            resumeButton?.onClick.AddListener(OnResumeButtonClicked);
            saveButton?.onClick.AddListener(() => StartCoroutine(SaveProcessRoutine()));
            loadButton?.onClick.AddListener(() => StartCoroutine(LoadProcessRoutine()));
            exitButton?.onClick.AddListener(OnExitButtonClicked);

            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
        }

        public override void Open()
        {
            base.Open();
            if (menuPanel != null) menuPanel.SetActive(true);
        }

        public override void Close()
        {
            base.Close();
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        private void OnResumeButtonClicked()
        {
            // UIManager를 통해 모든 팝업을 닫고 게임으로 복귀
            UIManager.Instance.CloseAllPopup();
        }

        private void ToggleMenu()
        {
            isMenuOpen = isMenuOpen == false; // ! 연산자 지양 규칙 준수

            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuOpen);
            }

            // 메뉴 상태에 따른 게임 환경 제어
            if (isMenuOpen)
            {
                Time.timeScale = TimeScalePaused;
                SetPlayerMoveState(false);
                SetCursorState(true);
            }
            else
            {
                Time.timeScale = TimeScaleNormal;
                SetPlayerMoveState(true);
                SetCursorState(false);
            }
        }

        private void SetPlayerMoveState(bool canMove)
        {
            if (playerController != null)
            {
                //playerController.CanMove = canMove;
            }
        }

        private void SetCursorState(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // --- 코루틴을 활용한 기능 로직 ---

        private IEnumerator SaveProcessRoutine()
        {
            if (DataManager.IsInitialized == false) yield break;

            Debug.Log("[GameMenu] 저장을 시작합니다...");
            
            // 1. 데이터 추출
            Vector3 currentPos = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
            PlayerStat currentStat = Player.Instance != null ? Player.Instance.GetCurrentStatData() : null;

            // 2. 저장 실행
            DataManager.Instance.SaveUserData(currentPos, currentStat);

            yield return null; // 한 프레임 대기
            
            Debug.Log("[GameMenu] 저장 완료");
        }

        private IEnumerator LoadProcessRoutine()
        {
            if (DataManager.IsInitialized == false) yield break;

            Debug.Log("[GameMenu] 로드를 시작합니다...");
            
            // 1. 데이터 로드
            var data = DataManager.Instance.LoadUserData();
            
            // 2. 데이터 적용
            if (data != null && PlayerStatusController.IsInitialized)
            {
                PlayerStatusController.Instance.ApplySaveData(data);
            }

            Debug.Log("[GameMenu] 로드 완료");
            ToggleMenu(); // 로드 후 메뉴 닫기

            yield return null;
        }

        private void OnExitButtonClicked()
        {
            Debug.Log("[GameMenu] 게임을 종료합니다.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}