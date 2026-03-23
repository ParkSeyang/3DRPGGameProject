 using UnityEngine;

public class AttackState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
    private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
    private const string ATP_ANIM_END = "Attack_End";

    private const float ATTACK_RANGE_TOLERANCE = 1.0f;
    private const float FAIL_SAFE_TIME = 2.5f; 
    
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
        MushRoomAnimator.SetFloat(MoveSpeed, 0f);

        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        if (MushRoom.Target != null)
        {
           MushRoom.transform.rotation = MushRoom.transform.FlatRotationTo(MushRoom.Target);
        }
        
        AttackCollider.enabled = false;
        MushRoomAnimator.SetTrigger(Attack);
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
    }

    public override void UpdateState()
    {
        stateTimer += Time.deltaTime;

        if (MushRoom.Target != null)
        {
            MushRoom.transform.SmoothLookAtFlat(MushRoom.Target, 5.0f);
        }

        if (stateTimer >= FAIL_SAFE_TIME)
        {
            DetermineNextState();
        }
    }

    public override void ExitState()
    {
        AttackCollider.enabled = false;
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
        stateTimer = -100f;

        if (MushRoom.Target == null || IsPlayerInSight() == false)
        {
            MushRoom.ChangeState<IdleState>();
            return;
        }
        float distance = MushRoom.transform.FlatDistanceTo(MushRoom.Target);
        
        if (distance > Agent.stoppingDistance + ATTACK_RANGE_TOLERANCE)
        {
            MushRoom.ChangeState<ChaseState>();
        }
        else
        {
            // Idle로 보내어 리셋 유도
            MushRoom.ChangeState<IdleState>();
        }
    }
}