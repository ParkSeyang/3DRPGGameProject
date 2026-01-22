using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
public class IdleState : BaseState
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    
    private const float WAIT_TIME = 3.0f;
    private float timer = 0.0f;
    
    [SerializeField] private Transform target;
    
    
    public override void EnterState()
    {
        timer = 0.0f;
    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;
        if (timer > WAIT_TIME)
        {
            switch (mushroomState)
            {
                case MushroomState.Idle:
                    Mushroom.ChangeState<IdleState>();
                    break;
                case MushroomState.Patrol:
                    Mushroom.ChangeState<PatrolState>();
                    break;
                case MushroomState.Chase:
                    Mushroom.ChangeState<ChaseState>();
                    break;
                case MushroomState.Attack:
                    Mushroom.ChangeState<AttackState>();
                    break;
                case MushroomState.Hit:
                    Mushroom.ChangeState<HitState>();
                    break;
                case MushroomState.Dead:
                    Mushroom.ChangeState<DeadState>();
                    break;
                default:
                    Mushroom.ChangeState<IdleState>();
                    break;
            }
        }
    }

    public override void ExitState()
    {
        // if문으로 플레이어를 발견하면 공격상태로 전환
        // 그게 아니라면 정찰 모드로 전환 
    }
}
