using UnityEngine;

public class WildBoarAttackState : WildBoarBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    
    private const string ATP_COLLIDER_ON = "Attack_Collider_On";
    private const string ATP_COLLIDER_OFF = "Attack_Collider_Off";
    private const string ATP_ANIM_END = "Attack_End";
    
    private const float ATTACK_RANGE_TOLERANCE = 1.5f;
    private const float FAIL_SAFE_TIME = 2.0f;

    private HitBox hitBox;
    private float stateTimer = 0f;

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
        stateTimer = 0f;
        WildBoarAnimator.SetFloat(MoveSpeed, 0f);

        if(Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }
            
        if(WildBoar.Target != null)
        {
            WildBoar.transform.rotation = WildBoar.transform.FlatRotationTo(WildBoar.Target);
        }
            
        WildBoarAnimator.SetTrigger(Attack);
        AnimEventReceiver.OnAnimationTriggerReceived += OnTriggeredEvent;
    }

    public override void UpdateState()
    {
        stateTimer += Time.deltaTime;

        if(WildBoar.Target != null) 
        {
            WildBoar.transform.SmoothLookAtFlat(WildBoar.Target, 5.0f);
        }

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
        switch(animEvent)
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

        if(WildBoar.Target == null || IsPlayerInSight() == false)
        { 
            WildBoar.ChangeState<WildBoarIdleState>();
            return;
        }

        float distance = WildBoar.transform.FlatDistanceTo(WildBoar.Target);
            
        if(distance > Agent.stoppingDistance + ATTACK_RANGE_TOLERANCE)
        { 
            WildBoar.ChangeState<WildBoarChaseState>();
        }
        else
        {
            // Idle로 보내어 리셋 유도
            WildBoar.ChangeState<WildBoarIdleState>();
        }
    }
}