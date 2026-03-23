using UnityEngine;

public class SlimeHitState : SlimeBaseState
{ 
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private const string HIT_ANIM_END = "Hit_End";
    
    public override void Initialize(StateControllerParameter parameter) => base.Initialize(parameter);
    
    public override void EnterState()
    {
        SlimeAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh == true)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        // [추가] 피격 시 즉시 플레이어를 바라봄
        if (Slime.Target != null)
        {
            Slime.transform.rotation = Slime.transform.FlatRotationTo(Slime.Target);
        }

        AttackCollider.enabled = false;
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
        SlimeAnimator.SetTrigger(Hit);
    }

    public override void UpdateState() { }

    public override void ExitState() => AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;

    private void OnTriggeredEvent(string animEvent)
    {
        if (animEvent == HIT_ANIM_END)
        {
            if (Slime.Target != null)
            {
                float distance = Slime.transform.FlatDistanceTo(Slime.Target);

                if (distance <= Agent.stoppingDistance + 0.01f)
                {
                    Slime.ChangeState<SlimeAttackState>();
                }
                else
                {
                    Slime.ChangeState<SlimeChaseState>();
                }
            }
            else
            {
                Slime.ChangeState<SlimeIdleState>();
            }
        }
    }

}



    
