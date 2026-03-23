using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("이 포탈의 고유 ID (GameManager의 연결 테이블과 연동)")]
    public int portalID;
    
    [SerializeField] private LayerMask playerLayer; 

    private bool isTransitioning = false;

    private void Awake()
    {
        // [방어 코드] 포탈은 절대 밀리면 안 됨. 트리거 강제 설정
        var portalCollider = GetComponent<Collider>();
        if (portalCollider != null) portalCollider.isTrigger = true;

        // Rigidbody 제거 (물리적 간섭 완전 차단)
        var portalRigidbody = GetComponent<Rigidbody>();
        if (portalRigidbody != null)
        {
            Destroy(portalRigidbody);
        }
    }

    private void OnTriggerEnter(Collider other) => HandlePortalCollision(other);
    private void OnTriggerStay(Collider other) => HandlePortalCollision(other);

    private void HandlePortalCollision(Collider other)
    {
        if (isTransitioning == true) return; 
        if (UIManager.IsInitialized == true && UIManager.Instance.IsPopupOpen == true) return;
        if (other.isTrigger == true) return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null) return;

        isTransitioning = true;
        GameSceneManager.Instance.OnPortalTriggered(portalID);

        // 안전장치: 1.5초 후 상태 리셋
        Invoke(nameof(ResetPortal), 1.5f);
    }

    public void ResetPortal()
    {
        isTransitioning = false;
    }
}
