using UnityEngine;

public class WildBoarIdleState : WildBoarBaseState
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private const float WAIT_TIME = 5.0f;
    private float timer = 0.0f;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        timer = 0.0f;
        WildBoarAnimator.SetTrigger(Idle);
    }

    public override void UpdateState()
    {
        // 플레이어는 대기중에도 시야 감지 범위를 통해서 언제든지 추격 상태로 전환
        // 할 수 있어야 되고 만약 대기시간이 길어지면 정찰상태로 전환되게끔 해줘야됨 
        
        // 플레이어가 탐지 범위에 들어왔는지 확인하는 조건문이다
        if (IsPlayerInSight())
        {
            WildBoar.ChangeState<WildBoarChaseState>();
            return; // 발견했으면 굳이 밑의 로직을 실행할 이유가 없으므로 대기타이머를 무시하고 추적 상태로 전환
            
        }
        
        timer += Time.deltaTime;
        if (timer > WAIT_TIME)
        {
            WildBoar.ChangeState<WildBoarPatrolState>();
        }
        
    }

    public override void ExitState()
    {
        
    }
}
