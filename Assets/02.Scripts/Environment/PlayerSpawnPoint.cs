using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Tooltip("해당 씬 내에서의 스폰 위치 인덱스")]
    public int spawnIndex;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        // 구체는 위치 표시
        Gizmos.DrawSphere(transform.position, 0.3f);
        
        // 화살표 형태의 레이로 방향 표시
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        Gizmos.DrawRay(transform.position, forward * 2.0f);
        Gizmos.DrawRay(transform.position + forward * 2.0f, (right - forward).normalized * 0.5f);
        Gizmos.DrawRay(transform.position + forward * 2.0f, (-right - forward).normalized * 0.5f);
        
        // 상단에 텍스트나 레이블이 필요하다면 에디터 클래스에서 처리 가능
    }
#endif
}
