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

        // UI 상태 백업용 딕셔너리
        private System.Collections.Generic.Dictionary<string, bool> uiStateBackup = new System.Collections.Generic.Dictionary<string, bool>();

        private void BackupAndActivateInventories()
        {
            uiStateBackup.Clear();
            
            if (InventorySystem.Instance == null) return;
            
            // 혹시 모르니 강제 탐색 한 번 수행
            InventorySystem.Instance.ForceFindInventories();

            string[] targets = { "User", "Equip", "Quick" };
            foreach (var name in targets)
            {
                var inven = InventorySystem.Instance.GetInventoryOrNull(name);
                if (inven != null)
                {
                    bool wasActive = inven.gameObject.activeSelf;
                    uiStateBackup[name] = wasActive;
                    
                    if (wasActive == false)
                    {
                        inven.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void RestoreInventories()
        {
            if (InventorySystem.Instance == null) return;

            foreach (var pair in uiStateBackup)
            {
                var inven = InventorySystem.Instance.GetInventoryOrNull(pair.Key);
                if (inven != null)
                {
                    // 원래 꺼져 있었던 녀석들만 다시 꺼준다
                    if (pair.Value == false)
                    {
                        inven.gameObject.SetActive(false);
                    }
                }
            }
            uiStateBackup.Clear();
        }

        private IEnumerator SaveProcessRoutine()
        {
            if (DataManager.IsInitialized == false || PlayerStatusController.IsInitialized == false) yield break;

            Debug.Log("[GameMenu] 저장을 시작합니다...");

            // 1. UI 강제 활성화 (데이터 확보를 위해)
            BackupAndActivateInventories();

            // 안정화를 위해 1프레임 대기
            yield return null;
            
            // 2. 전체 데이터 추출 (인벤토리 포함)
            UserSaveData saveData = PlayerStatusController.Instance.GetSaveData();

            // 3. 저장 실행
            DataManager.Instance.SaveUserData(saveData);

            yield return null; 
            
            // 4. UI 상태 복구
            RestoreInventories();
            
            Debug.Log("[GameMenu] 저장 완료");
        }

        private IEnumerator LoadProcessRoutine()
        {
            if (DataManager.IsInitialized == false) yield break;

            Debug.Log("[GameMenu] 로드를 시작합니다...");
            
            // 1. 데이터 로드
            var data = DataManager.Instance.LoadUserData();
            
            // 2. UI 강제 활성화 (데이터 적용 및 Awake 보장을 위해)
            BackupAndActivateInventories();
            
            // 시스템 안정화 대기
            yield return null;
            
            // 3. 데이터 적용
            if (data != null && PlayerStatusController.IsInitialized)
            {
                PlayerStatusController.Instance.ApplySaveData(data);
                
                // 데이터 적용 후 한 프레임 대기
                yield return null;

                // 스탯 UI 갱신
                if (Player.Instance != null) Player.Instance.RefreshAllStats();
            }

            // 4. UI 상태 복구 (원래대로 되돌리기)
            RestoreInventories();

            Debug.Log("[GameMenu] 로드 완료");
            
            // 메뉴를 닫고 HUD를 갱신하기 위해 UIManager 사용
            UIManager.Instance.ToggleUI(UIType.Menu); 

            yield return null;
        }

        private void ForceRefreshUI(string inventoryName)
        {
             // 더 이상 사용하지 않음 (BackupAndActivateInventories로 대체됨)
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