using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Mushroom : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Dead = Animator.StringToHash("Dead");


    [SerializeField] private Collider AttackCollider;
    [SerializeField] private AnimEventReceiver AnimEventReceiver;

    private Animator Animator { get; set; }

    private Dictionary<Type, BaseState> States { get; set; }
    private BaseState CurrentState { get; set; }
    private BaseState DefaultState { get; set; }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        AttackCollider.enabled = false;

        States = new Dictionary<Type, BaseState>();
        States.Add(typeof(IdleState), new IdleState());
        States.Add(typeof(PatrolState), new PatrolState());
        States.Add(typeof(ChaseState), new ChaseState());
        States.Add(typeof(AttackState), new AttackState());
        States.Add(typeof(HitState), new HitState());
        States.Add(typeof(DeadState), new DeadState());
        
        DefaultState = States[typeof(IdleState)];

        BaseState.StateControllerParameter param = 
            new BaseState.StateControllerParameter();

        param.mushroom = this;
        param.attackCollider = AttackCollider;
        param.mushroomAnimator = Animator;
        param.AnimEventReceiver = AnimEventReceiver;

        foreach (var state in States.Values)
        {
            state.Initialize(param);
        }
        
        
    }

    private void Update()
    {
        CurrentState.UpdateState();
    }

    public void ChangeState<T>() where T : BaseState
    {
        var prevState = CurrentState;
        prevState?.ExitState();

        CurrentState = DefaultState;
        if (States.ContainsKey(typeof(T)))
        {
            CurrentState = States[typeof(T)];
        }
        
        CurrentState.EnterState();
        Debug.Log($"{prevState?.GetType().Name} changed to {CurrentState.GetType().Name}");
    }



}
