using System;
using UnityEngine;

public class PlayerDiveRoll : MonoBehaviour
{
    private static readonly int DiveRoll = Animator.StringToHash("DiveRoll");
    [SerializeField] private Animator animator;
    [SerializeField] private AnimEventReceiver animEventReceiver;
    [SerializeField] private Collider hurtCollider;
    
    [Header("구르기 설정")]
    [SerializeField] private float DiveRollSpeed = 10.0f;
    [SerializeField] private bool isDiveRoll = false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
        hurtCollider = GetComponent<Collider>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&& isDiveRoll == false)
        {
            animator.SetBool(DiveRoll, true);
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
            isDiveRoll = true;
        }
        else if(parameter.Equals("Input_End"))
        {
            isDiveRoll = false;
            animator.SetBool(DiveRoll, false);
        }
        
        if (parameter.Equals("Invincibility_Start"))
        {
            hurtCollider.enabled = false;
        }
        else if (parameter.Equals("Invincibility_End"))
        {
            hurtCollider.enabled = true;
        }

       

        Debug.Log(parameter);
    }

    
    
}
