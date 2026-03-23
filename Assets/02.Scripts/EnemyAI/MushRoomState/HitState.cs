using UnityEngine;

public class HitState : BaseState
{ 
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private const string HIT_ANIM_END = "Hit_End";
    
    public override void Initialize(StateControllerParameter parameter) => base.Initialize(parameter);
    
    public override void EnterState()
    { 
        MushRoomAnimator.SetFloat(MoveSpeed, 0f);
        
        if (Agent.isOnNavMesh == true)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        // [추가] 피격 시 즉시 플레이어를 바라봄
        if (MushRoom.Target != null)
        {
            MushRoom.transform.rotation = MushRoom.transform.FlatRotationTo(MushRoom.Target);
        }
        
        AttackCollider.enabled = false;
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
        MushRoomAnimator.SetTrigger(Hit);
    }

    public override void UpdateState() { } 

    public override void ExitState() => AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;

    private void OnTriggeredEvent(string animEvent)
    {
        if (animEvent == HIT_ANIM_END)
        {
            if (MushRoom.Target != null)
            {
                float distance = MushRoom.transform.FlatDistanceTo(MushRoom.Target);

                // [수정] 이전 ChaseState와 동일하게 여유분 0.01f 적용하여 정밀하게 판정
                if (distance <= Agent.stoppingDistance + 0.01f) 
                {
                    MushRoom.ChangeState<AttackState>();
                }
                else
                {
                    MushRoom.ChangeState<ChaseState>();
                }
            }
            else 
            {
                MushRoom.ChangeState<IdleState>();
            }
        }
    }

}



    