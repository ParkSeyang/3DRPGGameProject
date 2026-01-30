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

        private void Awake()
        {
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

        private void Start()
        {
            // 플레이어 컨트롤러 찾기 (씬 내에 존재한다고 가정)
            playerController = FindFirstObjectByType<PlayerController>();
        }

        private void Update()
        {
            // ESC 키 입력 시 메뉴 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }

        private void OnResumeButtonClicked()
        {
            // Resume 버튼은 무조건 메뉴를 닫는 역할
            if (isMenuOpen)
            {
                ToggleMenu();
            }
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
            
            // 1. 플레이어 데이터 저장 (위치 포함)
            // Player가 싱글톤이므로 직접 접근하여 위치를 가져옵니다.
            Vector3 currentPos = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
           // DataManager.Instance.SaveUserData(currentPos);

            // 2. 스킬 데이터 저장
          //  if (SkillDataManager.IsInitialized)
            {
           //     SkillDataManager.Instance.SaveSkillData();
            }

            yield return null; // 한 프레임 대기 (필요 시 연출 추가 가능)
            
            Debug.Log("[GameMenu] 저장 완료");
        }

        private IEnumerator LoadProcessRoutine()
        {
            if (DataManager.IsInitialized == false) yield break;

            Debug.Log("[GameMenu] 로드를 시작합니다...");
            
            // 1. 플레이어 데이터 로드 및 적용
          //  var data = DataManager.Instance.LoadUserData();
         //   if (data != null)
            {
                // StatusController에 데이터 적용 요청 (스탯, 레벨 등)
            //    if (PlayerStatusController.IsInitialized)
                {
              //      PlayerStatusController.Instance.ApplySaveData(data);
                }

                // Player 싱글톤에 위치 적용
                if (Player.Instance != null)
                {
             //       Player.Instance.transform.position = data.GetPosition();
                }
            }

            // 2. 스킬 데이터 로드
        //    if (SkillDataManager.IsInitialized)
            {
          //      SkillDataManager.Instance.LoadSkillData();
                
                // TODO: 스킬 데이터 로드 후 패시브 효과 재적용 등의 후처리 로직이 필요할 수 있음
                // SkillTreeSystem.Instance.RefreshPassiveEffects(); 
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