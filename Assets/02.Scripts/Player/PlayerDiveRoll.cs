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
            hurtCollider.enabled = false;
            animator.SetBool(DiveRoll, true);
            isDiveRoll = true;
        }
        else
        {
            hurtCollider.enabled = true;
            animator.SetBool(DiveRoll, false);
            isDiveRoll = false;
        }
     
    }
  
    
}
