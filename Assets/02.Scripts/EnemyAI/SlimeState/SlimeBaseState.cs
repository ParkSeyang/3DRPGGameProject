using UnityEngine;
using UnityEngine.AI;
public abstract class SlimeBaseState
{
     public struct StateControllerParameter
    {
        public Slime slime;
        public Animator slimeAnimator;
        public NavMeshAgent agent;
        public AnimEventReceiver animEventReceiver;
        public Collider attackCollider;
    }
    
    // 플레이어의 콜라이더를 감지할 배열을 미리 할당해놓는다.
    private readonly Collider[] targetBuffer = new Collider[1];
    
    protected Slime Slime { get; private set; }
    protected Animator SlimeAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected AnimEventReceiver AnimEventReceiver { get; private set; }
    protected Collider AttackCollider { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        Slime = parameter.slime;
        SlimeAnimator = parameter.slimeAnimator;
        Agent = parameter.agent;
        AnimEventReceiver = parameter.animEventReceiver;
        AttackCollider = parameter.attackCollider;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        int count = Physics.OverlapSphereNonAlloc(Slime.EyeTransform.position, Slime.DetectionRadius,
            targetBuffer, Slime.PlayerLayer);
        if (count == 0)
        {
            return false;
        }

        Transform potentialTarget = targetBuffer[0].transform;
        
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.0f;

        Vector3 directionToTarget = (targetPosition - Slime.EyeTransform.position).normalized;

        float angleToTarget = Vector3.Angle(Slime.EyeTransform.forward, directionToTarget);

        if (angleToTarget > Slime.DetectionAngle / 2.0f)
        {
            Debug.Log($"감지 실패 : 시야각에서 벗어남 {angleToTarget:F1}");
            return false;
        }

        float distanceToTarget = Vector3.Distance(Slime.EyeTransform.position, targetPosition);
        
        
        if (Physics.Raycast(Slime.EyeTransform.position, directionToTarget, 
                out RaycastHit hit, distanceToTarget, Slime.ObstacleLayer))
        {
            Debug.Log($"감지 실패: 장애물에 막힘 ({hit.collider.name})");
            return false;
        }
        Debug.Log("감지 성공! 추격을 시작합니다");
        Slime.SetTarget(potentialTarget);
        return true;
    }

}
