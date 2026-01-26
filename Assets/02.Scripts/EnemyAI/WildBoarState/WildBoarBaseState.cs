using UnityEngine;
using UnityEngine.AI;

public abstract class WildBoarBaseState
{
      public struct StateControllerParameter 
      { 
        public WildBoar wildboar;
        public Animator wildboarAnimator;
        public NavMeshAgent agent;
        public AnimEventReceiver animEventReceiver;
        public Collider attackCollider; 
      }
    
    // 플레이어의 콜라이더를 감지할 배열을 미리 할당해놓는다.
    private readonly Collider[] targetBuffer = new Collider[1];

    protected WildBoar WildBoar { get; private set; }
    protected Animator WildBoarAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected AnimEventReceiver AnimEventReceiver { get; private set; }
    protected Collider AttackCollider { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        WildBoar = parameter.wildboar;
        WildBoarAnimator = parameter.wildboarAnimator;
        Agent = parameter.agent;
        AnimEventReceiver = parameter.animEventReceiver;
        AttackCollider = parameter.attackCollider;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        int count = Physics.OverlapSphereNonAlloc(WildBoar.EyeTransform.position, WildBoar.DetectionRadius,
            targetBuffer, WildBoar.PlayerLayer);
        if (count == 0)
        {
            return false;
        }

        Transform potentialTarget = targetBuffer[0].transform;
        
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.0f;

        Vector3 directionToTarget = (targetPosition - WildBoar.EyeTransform.position).normalized;

        float angleToTarget = Vector3.Angle(WildBoar.EyeTransform.forward, directionToTarget);

        if (angleToTarget > WildBoar.DetectionAngle / 2.0f)
        {
            Debug.Log($"감지 실패 : 시야각에서 벗어남 {angleToTarget:F1}");
            return false;
        }

        float distanceToTarget = Vector3.Distance(WildBoar.EyeTransform.position, targetPosition);
        
        
        if (Physics.Raycast(WildBoar.EyeTransform.position, directionToTarget, 
                out RaycastHit hit, distanceToTarget, WildBoar.ObstacleLayer))
        {
            Debug.Log($"감지 실패: 장애물에 막힘 ({hit.collider.name})");
            return false;
        }
        Debug.Log("감지 성공! 추격을 시작합니다");
        WildBoar.SetTarget(potentialTarget);
        return true;
    }

}
