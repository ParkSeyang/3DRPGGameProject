using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

public abstract class BaseState
{
    public struct StateControllerParameter
    {
        public Mushroom mushroom;
        public Animator mushroomAnimator;
        public NavMeshAgent agent;
        public AnimEventReceiver animEventReceiver;
        public Collider attackCollider;
    }
    
    private readonly Collider[] targetBuffer = new Collider[1];
    
    protected Mushroom MushRoom { get; private set; }
    protected Animator MushRoomAnimator { get; private set; }
    protected NavMeshAgent Agent { get; private set; }
    protected AnimEventReceiver AnimEventReceiver { get; private set; }
    protected Collider AttackCollider { get; private set; }

    public virtual void Initialize(StateControllerParameter parameter)
    {
        MushRoom = parameter.mushroom;
        MushRoomAnimator = parameter.mushroomAnimator;
        Agent = parameter.agent;
        AnimEventReceiver = parameter.animEventReceiver;
        AttackCollider = parameter.attackCollider;
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public bool IsPlayerInSight()
    {
        int detectionCount = Physics.OverlapSphereNonAlloc(MushRoom.EyeTransform.position, MushRoom.DetectionRadius,
            targetBuffer, MushRoom.PlayerLayer);
        
        if (detectionCount == 0) return false;

        Player targetPlayer = targetBuffer[0].GetComponentInParent<Player>();
        Transform potentialTarget = (targetPlayer != null) ? targetPlayer.transform : targetBuffer[0].transform;
        
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1.0f;
        Vector3 directionToTarget = (targetPosition - MushRoom.EyeTransform.position).normalized;
        float angleToTarget = Vector3.Angle(MushRoom.EyeTransform.forward, directionToTarget);

        if (angleToTarget > MushRoom.DetectionAngle / 2.0f) return false;

        float distanceToTarget = Vector3.Distance(MushRoom.EyeTransform.position, targetPosition);
        
        if (Physics.Raycast(MushRoom.EyeTransform.position, directionToTarget, distanceToTarget, MushRoom.ObstacleLayer))
        {
            return false;
        }

        MushRoom.SetTarget(potentialTarget);
        return true;
    }
        
    public bool IsMonsterInFront()
    {
        Vector3 originPosition = MushRoom.EyeTransform.position;
        Vector3 checkDirection = MushRoom.transform.forward;
        float checkDistance = 2.0f; 
        
        if (Physics.SphereCast(originPosition, 0.5f, checkDirection, out RaycastHit enemyHit, checkDistance, MushRoom.EnemyLayer))
        {
            if (enemyHit.collider.gameObject != MushRoom.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}