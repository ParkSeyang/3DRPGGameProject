using UnityEngine;

public class SlimeAttackState : SlimeBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
    private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
    private const string ATP_ANIM_END = "Attack_End";
    
    private const float ATTACK_RANGE_TOLERANCE = 1.0f;
    private const float FAIL_SAFE_TIME = 2.0f; // 애니메이션 이벤트 누락 대비용
    
    private HitBox hitBox;
    private float stateTimer = 0f;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
        if (AttackCollider != null)
        {
            hitBox = AttackCollider.GetComponent<HitBox>();
        }
    }

    public override void EnterState()
    {
        stateTimer = 0f;
        SlimeAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        if (Slime.Target != null)
        {
           Slime.transform.rotation = Slime.transform.FlatRotationTo(Slime.Target);
        }
        
        SlimeAnimator.SetTrigger(Attack);
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
    }

    public override void UpdateState()
    {
        stateTimer += Time.deltaTime;

        if (Slime.Target != null)
        {
            Slime.transform.SmoothLookAtFlat(Slime.Target, 5.0f);
        }

        // 안전 장치: 어떤 이유로든 애니메이션 이벤트가 안 들어오면 2초 뒤 강제 복귀
        if (stateTimer >= FAIL_SAFE_TIME)
        {
            DetermineNextState();
        }
    }

    public override void ExitState()
    {
        hitBox?.DisableDetection();
        AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;
    }

    public void OnTriggeredEvent(string animEvent)
    {
        switch (animEvent)
        {
            case ATP_COLLIDER_ON:
                hitBox?.EnableDetection();
                break;
            case ATP_COLLIDER_OFF:
                hitBox?.DisableDetection();
                break;
            case ATP_ANIM_END:
                DetermineNextState();
                break;
        }
    }

    private void DetermineNextState()
    {
        // 중복 호출 방지를 위해 타이머 초기화
        stateTimer = -100f; 

        if (Slime.Target == null || IsPlayerInSight() == false)
        {
            Slime.ChangeState<SlimeIdleState>();
            return;
        }

        float distance = Slime.transform.FlatDistanceTo(Slime.Target);

        // 사거리 밖이면 추격
        if (distance > Agent.stoppingDistance + ATTACK_RANGE_TOLERANCE)
        {
            Slime.ChangeState<SlimeChaseState>();
        }
        else
        {
            // [중요] 사거리 안이라도 Idle로 한 번 보내서 애니메이터를 리셋시킴
            // Idle 상태는 다음 프레임에 즉시 다시 Chase나 Attack을 판단할 것임
            Slime.ChangeState<SlimeIdleState>();
        }
    }
}