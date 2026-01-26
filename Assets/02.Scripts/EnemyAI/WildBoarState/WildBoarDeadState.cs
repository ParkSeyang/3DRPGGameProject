using UnityEngine;

public class WildBoarDeadState : WildBoarBaseState
{
    // 피격상태에서 체력이 0이라서 사망상태에 돌입했을시 
    // 몬스터 객체는 보상을 떨군후 소멸되야됨.
    private static readonly int Dead = Animator.StringToHash("Dead");
    private float destroyTimer = 0.0f;
    private const float DESTROY_DELAY = 8.0f; // 시체 소멸 시간
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        var mainCollider = WildBoar.GetComponent<Collider>();

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        AttackCollider.enabled = false;
        
        WildBoarAnimator.SetTrigger(Dead);

        Debug.Log($"{WildBoar.name}이 {WildBoar.CurrentHitCount}번 맞고 쓰러졌음");
    }

    public override void UpdateState()
    {
        destroyTimer += Time.deltaTime;
        if (destroyTimer >= DESTROY_DELAY)
        {
            GameObject.Destroy(WildBoar.gameObject);
        }
    }

    public override void ExitState()
    {
        
    }
}
