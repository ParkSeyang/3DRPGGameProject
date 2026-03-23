using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpawnManager : SingletonBase<PlayerSpawnManager>
{
    private Dictionary<int, PlayerSpawnPoint> spawnPointMap = new Dictionary<int, PlayerSpawnPoint>();

    /// <summary>
    /// 새로운 씬이 로드되었을 때 호출되어 스폰 포인트들을 캐싱합니다.
    /// </summary>
    public void RefreshSpawnPoints()
    {
        spawnPointMap.Clear();
        
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var allPoints = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (var point in allPoints)
        {
            // [가독성 개선] 현재 활성화된 씬에 속한 스폰 포인트만 필터링
            if (point.gameObject.scene != activeScene)
            {
                continue;
            }

            // 딕셔너리에 추가 (중복 인덱스 체크 포함)
            spawnPointMap.TryAdd(point.spawnIndex, point);
        }
    }

    /// <summary>
    /// 포탈 이동 등 특정 인덱스로 스폰이 필요할 때 사용합니다.
    /// </summary>
    public void SpawnAtPoint(int spawnIndex)
    {
        if (spawnPointMap.TryGetValue(spawnIndex, out var spawnPoint))
        {
            MovePlayer(spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
    }

    /// <summary>
    /// 게임 로드 시 정밀 좌표로 플레이어를 복구할 때 사용합니다.
    /// </summary>
    public void SpawnAtSavedPosition(Vector3 savedPosition, float rotationY)
    {
        MovePlayer(savedPosition, Quaternion.Euler(0, rotationY, 0));
    }

    private void MovePlayer(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (Player.Instance == null) return;

        // [수정] Unity 6 대응: kinematic 상태에서는 속도를 설정할 수 없습니다.
        var playerRigidbody = Player.Instance.GetComponent<Rigidbody>();
        if (playerRigidbody != null && playerRigidbody.isKinematic == false)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        // 위치 및 회전 주입 (스폰 포인트의 값을 1:1로 전달)
        Player.Instance.transform.position = targetPosition;
        Player.Instance.transform.rotation = targetRotation;
    }
}
