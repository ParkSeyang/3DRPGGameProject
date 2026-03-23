using UnityEngine;

public enum NPCType { Common, Merchant, Quest }

public abstract class NPC : MonoBehaviour
{
    [Header("NPC Settings")]
    public string NPCName;
    public NPCType Type;
    
    [Tooltip("상호작용 범위를 결정할 콜라이더 (없으면 InteractionRadius 수치 사용)")]
    [SerializeField] protected SphereCollider interactionCollider;
    public float InteractionRadius = 3.0f;

    [Header("UI Reference")]
    [SerializeField] protected InteractionUI interactionUI;

    protected bool isPlayerInRange = false;

    protected virtual void Awake()
    {
        // 인스펙터에서 할당 안 했을 경우 자식까지 뒤져서 자동으로 찾아봄
        if (interactionCollider == null)
        {
            interactionCollider = GetComponentInChildren<SphereCollider>();
        }
    }

    protected virtual void Update()
    {
        CheckPlayerDistance();

        // 팝업 UI가 열려있을 때는 상호작용 방지 (대화 중복 시작 방지)
        if (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen)
        {
            return;
        }

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            OnInteract();
        }
    }

    /// <summary>
    /// 콜라이더 스케일을 고려한 실제 월드 판정 반지름 반환
    /// </summary>
    public float GetActualInteractionRadius()
    {
        if (interactionCollider == null) return InteractionRadius;

        // 구체 콜라이더의 반지름에 오브젝트의 가장 큰 스케일 값을 곱함
        float maxScale = Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        return interactionCollider.radius * maxScale;
    }

    private void CheckPlayerDistance()
    {
        if (Player.Instance == null) return;

        float actualRadius = GetActualInteractionRadius();
        float distance = transform.FlatDistanceTo(Player.Instance.transform);
        
        if (distance <= actualRadius)
        {
            if (isPlayerInRange == false)
            {
                isPlayerInRange = true;
                interactionUI?.Show();
            }
        }
        else
        {
            if (isPlayerInRange == true)
            {
                isPlayerInRange = false;
                interactionUI?.Hide();
            }
        }
    }

    // 각 NPC 타입에서 구현할 상호작용 로직
    protected abstract void OnInteract();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        float actualRadius = GetActualInteractionRadius();

        // 코드상에서 판정하는 실제 범위를 노란색 원으로 표시
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, actualRadius);
        
        // 라벨 표시
        UnityEditor.Handles.Label(transform.position + Vector3.up * actualRadius, $"Actual Range: {actualRadius:F1}m (Scale Applied)");
    }
#endif
}
