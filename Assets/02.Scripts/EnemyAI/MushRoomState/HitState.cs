using UnityEngine;

public class HitState : BaseState
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
        
        MushRoomAnimator.SetTrigger(Hit);
        Debug.Log($"한대 맞앗다 {Hit}");
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
            if (MushRoom.Target != null)
            {
                MushRoom.ChangeState<ChaseState>();
            }
            else
            {
                // 타겟이 없으면 대기
                MushRoom.ChangeState<IdleState>();
            }
        }
    }


}
