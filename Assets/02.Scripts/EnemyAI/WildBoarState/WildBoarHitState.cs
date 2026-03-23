using UnityEngine;

public class WildBoarHitState : WildBoarBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private const string HIT_ANIM_END = "Hit_End";
    
    public override void Initialize(StateControllerParameter parameter) => base.Initialize(parameter);
    
    public override void EnterState()
    {
        WildBoarAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh == true)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        // [추가] 피격 시 즉시 플레이어를 바라봄
        if (WildBoar.Target != null)
        {
            WildBoar.transform.rotation = WildBoar.transform.FlatRotationTo(WildBoar.Target);
        }

        AttackCollider.enabled = false;
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
        WildBoarAnimator.SetTrigger(Hit);
    }

    public override void UpdateState() { }

    public override void ExitState() => AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;

    private void OnTriggeredEvent(string animEvent)
    {
        if (animEvent == HIT_ANIM_END)
        {
            if (WildBoar.Target != null)
            {
                float distance = WildBoar.transform.FlatDistanceTo(WildBoar.Target);

                if (distance <= Agent.stoppingDistance + 0.01f)
                {
                    WildBoar.ChangeState<WildBoarAttackState>();
                }
                else
                {
                    WildBoar.ChangeState<WildBoarChaseState>();
                }
            }
            else
            {
                WildBoar.ChangeState<WildBoarIdleState>();
            }
        }
    }

}



    
