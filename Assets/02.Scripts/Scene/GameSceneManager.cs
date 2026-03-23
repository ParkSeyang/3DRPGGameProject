using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameSceneManager : SingletonBase<GameSceneManager>
{
    [System.Serializable]
    public struct PortalConnection
    {
        public int portalID;
        public string targetScene;
        public int targetSpawnIndex;
    }

    [Header("World Flow Settings")]
    [SerializeField] private List<PortalConnection> portalConnections = new List<PortalConnection>();

    [Header("Scene Names")]
    [SerializeField] private string firstSceneName = "01_StartVillage";

    public string currentSceneName;
    public bool IsLevelLoading { get; private set; } // 외부 공개용 속성
    private int pendingSpawnIndex = -1; 

    private UserSaveData loadedData = null;

    protected override void OnInitialize()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        if (portalConnections == null || portalConnections.Count == 0) SetupDefaultConnections();
    }

    private void SetupDefaultConnections()
    {
        portalConnections = new List<PortalConnection>();
        portalConnections.Add(new PortalConnection { portalID = 0, targetScene = "01_StartVillage", targetSpawnIndex = 0 });
        portalConnections.Add(new PortalConnection { portalID = 1, targetScene = "02_BeginnersForest", targetSpawnIndex = 0 });
        portalConnections.Add(new PortalConnection { portalID = 2, targetScene = "01_StartVillage", targetSpawnIndex = 1 });
    }

    public void StartNewGame()
    {
        if (DataManager.Instance != null) DataManager.Instance.DeleteSaveData();
        pendingSpawnIndex = 0; 
        LoadScene(firstSceneName, InitializeNewGamePlayer);
    }

    private void InitializeNewGamePlayer()
    {
        PlayerSpawnManager.Instance.RefreshSpawnPoints();
        PlayerSpawnManager.Instance.SpawnAtPoint(pendingSpawnIndex);

        if (PlayerStatusController.IsInitialized)
        {
            Player.Instance.InitializeDefaultStat(); 
            if (InventoryDataManager.IsInitialized) InventoryDataManager.Instance.ClearAllData();
        }
        FinalizeSceneTransition();
    }

    public void LoadGame()
    {
        if (DataManager.Instance == null) return;
        loadedData = DataManager.Instance.LoadUserData();
        
        if (loadedData == null)
        {
            StartNewGame();
            return;
        }

        string targetScene = string.IsNullOrEmpty(loadedData.lastSceneName) ? firstSceneName : loadedData.lastSceneName;
        LoadScene(targetScene, ApplyLoadedData);
    }

    private void ApplyLoadedData()
    {
        if (loadedData == null || Player.Instance == null) return;

        PlayerSpawnManager.Instance.RefreshSpawnPoints();
        Vector3 savedPos = new Vector3(loadedData.posX, loadedData.posY, loadedData.posZ);
        PlayerSpawnManager.Instance.SpawnAtSavedPosition(savedPos, loadedData.rotY);

        if (PlayerStatusController.IsInitialized)
        {
            PlayerStatusController.Instance.ApplySaveData(loadedData);
        }

        loadedData = null;
        FinalizeSceneTransition();
    }

    private void FinalizeSceneTransition()
    {
        var cameraView = Object.FindAnyObjectByType<ThirdPersonView>();
        cameraView?.ResetCameraPosition();
    }

    public void OnPortalTriggered(int portalID)
    {
        if (IsLevelLoading) return;
        var connection = portalConnections.Find(c => c.portalID == portalID);
        if (string.IsNullOrEmpty(connection.targetScene) == false)
        {
            pendingSpawnIndex = connection.targetSpawnIndex;
            LoadScene(connection.targetScene, OnSceneMoveComplete);
        }
    }

    private void OnSceneMoveComplete()
    {
        PlayerSpawnManager.Instance.RefreshSpawnPoints();
        PlayerSpawnManager.Instance.SpawnAtPoint(pendingSpawnIndex);
        
        var portals = Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var p in portals) p.ResetPortal();
    }

    public void ReturnToTitle()
    {
        if (IsLevelLoading) return;
        StartCoroutine(LoadTitleSceneRoutine());
    }

    private IEnumerator LoadTitleSceneRoutine()
    {
        IsLevelLoading = true;
        Time.timeScale = 1.0f; 

        // 1. 모든 UI 끄고 로딩창 활성화
        if (UIManager.IsInitialized)
        {
            UIManager.Instance.CloseAllPopup();
            UIManager.Instance.SetAllInGameUIActive(false);
            UIManager.Instance.SetUIActive(UIType.Loading, true);
        }

        // 2. 물리 봉인 (타이틀에서도 안전하게 이동하기 위함)
        Rigidbody playerRigidbody = Player.Instance?.GetComponent<Rigidbody>();
        if (playerRigidbody != null) playerRigidbody.isKinematic = true;

        // 3. 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("00_GameStart");
        while (asyncLoad.isDone == false)
        {
            float sceneProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(sceneProgress * 0.5f);
            yield return null;
        }

        currentSceneName = "00_GameStart";

        // [추가] 타이틀 씬 스폰 처리 (0번 포인트)
        if (PlayerSpawnManager.IsInitialized)
        {
            PlayerSpawnManager.Instance.RefreshSpawnPoints();
            PlayerSpawnManager.Instance.SpawnAtPoint(0);
        }

        // 4. 타이틀 씬 전용 예열
        float timer = 0f;
        while (timer < 1.0f)
        {
            timer += Time.unscaledDeltaTime;
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(0.5f + (timer / 1.0f) * 0.5f);
            yield return null;
        }

        // 5. 물리 복구
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;
        }

        // 6. 로딩 완료 및 UI 주도권 정리
        IsLevelLoading = false;

        if (UIManager.IsInitialized)
        {
            UIManager.Instance.SetUIActive(UIType.Loading, false);
            UIManager.Instance.SetAllInGameUIActive(false);
            
            var titleUI = Object.FindAnyObjectByType<TitleUI>(FindObjectsInactive.Include);
            if (titleUI != null) titleUI.Open();
        }
    }

    public void LoadScene(string sceneName, System.Action onComplete = null)
    {
        if (IsLevelLoading) return;
        Time.timeScale = 1.0f; // [추가] 일반 로딩 시에도 시간 리셋
        StartCoroutine(LoadSceneRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, System.Action onComplete)
    {
        IsLevelLoading = true;
        
        // 1. 모든 UI 끄고 로딩창만 활성화
        if (UIManager.IsInitialized)
        {
            UIManager.Instance.SetAllInGameUIActive(false); 
            UIManager.Instance.SetUIActive(UIType.Loading, true);
        }

        // 2. 물리 봉인
        Rigidbody playerRigidbody = Player.Instance?.GetComponent<Rigidbody>();
        if (playerRigidbody != null) playerRigidbody.isKinematic = true;

        // 3. 씬 비동기 로드 (0% ~ 20%)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (asyncLoad.isDone == false)
        {
            float sceneProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(sceneProgress * 0.2f);
            yield return null;
        }

        currentSceneName = sceneName;
        if (DataManager.IsInitialized) DataManager.Instance.UpdateSavePolicy(currentSceneName);

        // 4. 데이터 복구 (스폰 등)
        onComplete?.Invoke();
        
        // 5. 초기 안정화 (20% ~ 30%)
        float recoveryTimer = 0f;
        while (recoveryTimer < 0.5f)
        {
            recoveryTimer += Time.unscaledDeltaTime;
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(0.2f + (recoveryTimer / 0.5f) * 0.1f);
            yield return null;
        }

        // 6. UI 데이터 예열 (인벤토리 수집 등)
        if (InventorySystem.IsInitialized) InventorySystem.Instance.ForceFindInventories();
        if (UIManager.IsInitialized) UIManager.Instance.ForceRefreshAll();
        
        yield return new WaitForEndOfFrame(); 
        
        // 7. 시스템 안착 대기 (30% ~ 40%)
        float stabilizationTimer = 0f;
        while (stabilizationTimer < 1.0f)
        {
            stabilizationTimer += Time.unscaledDeltaTime;
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(0.3f + (stabilizationTimer / 1.0f) * 0.1f);
            yield return null;
        }

        // 8. [핵심] 고정 8초 대기 구간 (40% ~ 100%)
        float bufferTimer = 0f;
        while (bufferTimer < 8.0f)
        {
            bufferTimer += Time.unscaledDeltaTime;
            UIManager.Instance.GetLoadingUI()?.UpdateProgress(0.4f + (bufferTimer / 8.0f) * 0.6f);
            yield return null;
        }

        // 9. 로딩 완료 시퀀스 (100%)
        UIManager.Instance.GetLoadingUI()?.UpdateProgress(1.0f);
        yield return new WaitForSecondsRealtime(0.2f);

        // 10. 물리 및 조작 해제
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;
        }

        // 11. 로딩창 걷어내고 HUD 일괄 활성화 (타이틀 씬이 아닐 때만)
        IsLevelLoading = false;

        if (UIManager.IsInitialized)
        {
            UIManager.Instance.SetUIActive(UIType.Loading, false);
            
            if (currentSceneName == "00_GameStart")
            {
                UIManager.Instance.SetAllInGameUIActive(false);
            }
            else
            {
                UIManager.Instance.SetAllInGameUIActive(true);
            }
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}