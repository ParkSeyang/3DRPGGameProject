using UnityEngine;

public class AttackState : BaseState
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private Collider AttackCollider;
    private AnimEventReceiver receiver;

    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
    private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
    private const string ATP_ANIM_END = "Attack_End";
    
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
        AttackCollider = parameter.attackCollider;
        receiver = parameter.AnimEventReceiver;
    }


    public override void EnterState()
    {
        AttackCollider.enabled = true;
        MushRoomAnimator.SetTrigger(Attack);
        
    }

    public override void UpdateState()
    {
        return;
    }

    public override void ExitState()
    {
        AttackCollider.enabled = false;
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
                Mushroom.ChangeState<IdleState>();
                break;
            default:
                break;
        }
    }

}
