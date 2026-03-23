using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAttack : MonoBehaviour
{
    private static readonly int ComboAttack = Animator.StringToHash("ComboAttack");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");

    [Header("Combo Settings")]
    [Tooltip("7단 콤보 각각의 속도를 설정하세요 (0번~6번 순서)")]
    [SerializeField] private float[] attackSpeeds = { 3.0f, 2.5f, 1.2f, 5.0f, 5.0f, 1.3f, 3.5f };

    [Header("Weapon Settings")]
    [SerializeField] private Collider weaponCollider;

    private int currentComboIndex = 0;
    private Animator animator;
    private AnimEventReceiver animEventReceiver;
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
        string sceneName = SceneManager.GetActiveScene().name;
        bool isTitle = sceneName.Contains("StartGame") || sceneName.Contains("GameStart");
        if (isTitle || (UIManager.IsInitialized && UIManager.Instance.IsPopupOpen))
        {
            return;
        }

        if (isAttackAble && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (attackSpeeds == null || attackSpeeds.Length == 0) return;

        float speed = attackSpeeds[currentComboIndex % attackSpeeds.Length];

        animator.SetFloat(AttackSpeed, speed);
        animator.SetTrigger(ComboAttack);
        isAttackAble = false;

        currentComboIndex = (currentComboIndex + 1) % attackSpeeds.Length;

        bool currentAttackState = animator.GetBool(IsAttack);
        if (currentAttackState == false)
        {
            animator.SetBool(IsAttack, true);
        }
        else
        {
            animator.SetBool(IsAttack, false);
        }
    }

    private void OnEnable() => animEventReceiver.OnAnimationTriggerReceived += OnTriggerAnim;
    private void OnDisable() => animEventReceiver.OnAnimationTriggerReceived -= OnTriggerAnim;

    private void OnTriggerAnim(string parameter)
    {
        if (parameter.Equals("Input_Start"))
        {
            isAttackAble = true;
        }
        else if (parameter.Equals("Input_End"))
        {
            isAttackAble = false;
            currentComboIndex = 0;
        }
        else if (parameter.Equals("Attack_Start"))
        {
            hitBox?.EnableDetection();
        }
        else if (parameter.Equals("Attack_End"))
        {
            hitBox?.DisableDetection();
        }
    }
}
