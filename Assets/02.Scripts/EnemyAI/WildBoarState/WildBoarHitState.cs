using UnityEngine;

public class WildBoarHitState : WildBoarBaseState
{
    private static readonly int Hit = Animator.StringToHash("Hit");
    private const string HIT_ANIM_END = "Hit_End";
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

        AttackCollider.enabled = false;

        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
        
        WildBoarAnimator.SetTrigger(Hit);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;
    }

    private void OnTriggeredEvent(string animEvent)
    {
        if (animEvent == HIT_ANIM_END)
        {
            if (WildBoar.Target != null)
            {
                WildBoar.ChangeState<WildBoarChaseState>();
            }
            else
            {
                // 타겟이 없으면 대기
                WildBoar.ChangeState<WildBoarIdleState>();
            }
        }
    }

}
