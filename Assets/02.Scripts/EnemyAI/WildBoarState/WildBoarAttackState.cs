using UnityEngine;

public class WildBoarAttackState : WildBoarBaseState
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
        private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
        private const string ATP_ANIM_END = "Attack_End";
        
        // 공격 상태를 유지하는 범위를 넉넉하게 주어 빈번한 상태 전환 방지 (0.2 -> 1.5)
        private const float ATTACK_RANGE_TOLERANCE = 1.5f;
        private HitBox hitBox;


    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
        if(AttackCollider != null)
        { 
            hitBox = AttackCollider.GetComponent<HitBox>();
        }
    }
    
    public override void EnterState()
    {
        if(Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }
            
        if(WildBoar.Target != null)
        {
            WildBoar.transform.rotation = 
                WildBoar.transform.FlatRotationTo(WildBoar.Target);

        }
            
        // AttackCollider.enabled = false;
            
        WildBoarAnimator.SetTrigger(Attack);

        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;

    }

    
    public override void UpdateState()
    {
        if(WildBoar.Target != null) 
        {
            WildBoar.transform.SmoothLookAtFlat(WildBoar.Target, 5.0f);
        }
    }

    

    public override void ExitState()
    {
        // AttackCollider.enabled = false;
        hitBox?.DisableDetection();
        AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;
    }

        
    public void OnTriggeredEvent(string animEvent)
    {
        switch(animEvent)
        {
            case ATP_COLLIDER_ON: 
                Debug.Log($"공격 시작");
                hitBox?.EnableDetection();
                break;
                
            case ATP_COLLIDER_OFF:
                Debug.Log($"공격 끝");
                hitBox?.DisableDetection();
                break;

            case ATP_ANIM_END:
                Debug.Log($"애니메이션 종료 상태 다음상태를 진행합니다.");
                DetermineNextState();
                break;

                default: 
                    break;
        }

    }

    

    private void DetermineNextState()
    {
            // 타겟이 없거나 시야에서 사라졌다면 Idle 상태로 전환
        if(WildBoar.Target == null || IsPlayerInSight() == false)
        { 
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        // 거리 계산 (TransformExtensions 활용 - Y축 무시)

        float distance = WildBoar.transform.FlatDistanceTo(WildBoar.Target);
            
        // 사거리(StoppingDistance) + 오차 범위보다 멀어져야 추격 시작

        if(distance > Agent.stoppingDistance + ATTACK_RANGE_TOLERANCE)
        { 
            WildBoar.ChangeState<WildBoarChaseState>();
        }

        else
        {
            // 여전히 사거리 내라면 다시 공격 (연속 공격)
            WildBoar.ChangeState<WildBoarAttackState>();
        }

    }

}
