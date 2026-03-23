using UnityEngine;

public class WildBoarIdleState : WildBoarBaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float WAIT_TIME = 5.0f;
    private const float DAMP_TIME = 0.2f;
    private float timer = 0.0f;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        timer = 0.0f;
    }

    public override void UpdateState()
    {
        // 부드럽게 0(Idle)으로 감속
        WildBoarAnimator.SetFloat(MoveSpeed, 0f, DAMP_TIME, Time.deltaTime);

        if (IsPlayerInSight())
        {
            WildBoar.ChangeState<WildBoarChaseState>();
            return;
        }
        
        timer += Time.deltaTime;
        if (timer > WAIT_TIME)
        {
            WildBoar.ChangeState<WildBoarPatrolState>();
        }
    }

    public override void ExitState() { }
}