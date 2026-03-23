using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

public class IdleState : BaseState
{
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private const float WAIT_TIME = 5.0f;
    private const float DAMP_TIME = 0.2f; // 멈출 때의 부드러움 정도
    private float timer = 0.0f;
    
    public override void Initialize(StateControllerParameter parameter)
    {
        base.Initialize(parameter);
    }
    
    public override void EnterState()
    {
        timer = 0.0f;
        // Enter에서 바로 0을 넣지 않고 Update에서 서서히 줄임
    }

    public override void UpdateState()
    {
        // 부드럽게 0(Idle)으로 감속
        MushRoomAnimator.SetFloat(MoveSpeed, 0f, DAMP_TIME, Time.deltaTime);

        if (IsPlayerInSight())
        {
            MushRoom.ChangeState<ChaseState>();
            return;
        }
        
        timer += Time.deltaTime;
        if (timer > WAIT_TIME)
        {
            MushRoom.ChangeState<PatrolState>();
        }
    }

    public override void ExitState() { }
}