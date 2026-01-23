using UnityEngine;

public class DeadState : BaseState
{
    // 피격상태에서 체력이 0이라서 사망상태에 돌입했을시 
    // 몬스터 객체는 보상을 떨군후 소멸되야됨.
    private static readonly int Dead = Animator.StringToHash("Dead");
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }

    public override void EnterState()
    {
        
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }
}
