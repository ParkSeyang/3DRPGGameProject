using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private static readonly int ComboAttack = Animator.StringToHash("ComboAttack");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    private Animator animator;
    AnimEventReceiver animEventReceiver;
    
    [SerializeField] private Collider weaponCollider;
    private HitBox hitBox;
    
    private bool isAttackAble = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        
        if (weaponCollider != null)
        {
            hitBox = weaponCollider.GetComponent<HitBox>();
        }
        
        isAttackAble = true;
    }
    
    private void Update()
    {
        if (isAttackAble && Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger(ComboAttack);
            isAttackAble = false;
            if (animator.GetBool(IsAttack) == false)
            {
                animator.SetBool(IsAttack, true);
            }
            else
            {
                animator.SetBool(IsAttack, false);
            }
        }
    }
    private void OnEnable()
    {
        
        animEventReceiver.OnAnimationTriggerReceived += OnTriggerAnim;
    }

    private void OnDisable()
    {
        
        animEventReceiver.OnAnimationTriggerReceived -= OnTriggerAnim;
    }
    
    private void OnTriggerAnim(string parameter)
    {
        if (parameter.Equals("Input_Start"))
        {
            isAttackAble = true;
        }
        else if (parameter.Equals("Input_End"))
        {
            isAttackAble = false;
        }
        
        if (parameter.Equals("Attack_Start"))
        {
            hitBox?.EnableDetection();
        }
        else if (parameter.Equals("Attack_End"))
        {
            hitBox?.DisableDetection();
        }
        Debug.Log(parameter);
    }

}
