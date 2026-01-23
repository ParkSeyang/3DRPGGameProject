using UnityEngine;

public class AttackState : BaseState
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
    private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
    private const string ATP_ANIM_END = "Attack_End";
    
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

        if (MushRoom.Target != null)
        {
            // 방향 구하는법 
            // (플레이어의 위치 - 몬스터의 위치) 해주고 정규화 해준다.
            Vector3 direction = (MushRoom.Target.position - MushRoom.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                MushRoom.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        AttackCollider.enabled = false;
        
        MushRoomAnimator.SetTrigger(Attack);
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;

    }

    public override void UpdateState()
    {
        return;
    }

    public override void ExitState()
    { 
        AttackCollider.enabled = false;
        
        AnimEventReceiver.OnAnimationTriggerReceived -= OnTriggeredEvent;
    }

    public void OnTriggeredEvent(string animEvent)
    {
        switch (animEvent)
        {
            case ATP_COLLIDER_ON:
                AttackCollider.enabled = true;
                break;
            case ATP_COLLIDER_OFF:
                AttackCollider.enabled = false;
                break;
            case ATP_ANIM_END:
                MushRoom.ChangeState<IdleState>();
                break;
            default:
                break;
        }
    }

}
