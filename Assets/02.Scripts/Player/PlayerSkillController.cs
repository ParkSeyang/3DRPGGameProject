using UnityEngine;
using System;

public class PlayerSkillController : MonoBehaviour, ICombatAgent
{
    private Animator animator;
    private AnimEventReceiver animEventReceiver;
    private Skill currentSkill; // 현재 시전 중인 스킬

    public bool IsCasting { get; private set; } // [추가] 스킬 시전 상태 플래그

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animEventReceiver = GetComponent<AnimEventReceiver>();
    }

    private void OnEnable()
    {
        if (animEventReceiver != null)
        {
            animEventReceiver.OnAnimationTriggerReceived += OnTriggerAnim;
        }
    }

    private void OnDisable()
    {
        if (animEventReceiver != null)
        {
            animEventReceiver.OnAnimationTriggerReceived -= OnTriggerAnim;
        }
    }

    // PlayerSkillSystem에서 호출
    public void ExecuteSkill(Skill skill, string triggerName)
    {
        IsCasting = true; // [추가] 시전 시작
        currentSkill = skill;
        animator.SetTrigger(triggerName);
    }

    // 외부에서 스킬을 강제로 중단시킬 때 사용 (피격 등)
    public void CancelSkill()
    {
        IsCasting = false;
    }

    // 기존 PlayerAttack의 방식을 응용한 이벤트 수신처
    private void OnTriggerAnim(string parameter)
    {
        // 애니메이션 이벤트(AnimTriggerEventSender)에서 "Skill_Cast" 파라미터를 보낼 때 발동
        if (parameter.Equals("Skill_Cast"))
        {
            InvokeSkillEvent();
        }
        else if (parameter.Equals("Skill_End")) // [추가] 애니메이터에서 시전 종료 이벤트 수신
        {
            IsCasting = false;
        }
        
        // 필요하다면 Attack_Start / Attack_End 처럼 스킬 히트박스를 정교하게 제어할 수도 있습니다.
    }

    private void InvokeSkillEvent()
    {
        if (currentSkill == null) return;

        // [옵저버 패턴] 스킬 이벤트 발생
        // HitInfo에 시전자 정보(gameObject, position)를 실어서 Binder에 전달
        HitInfo castInfo = new HitInfo
        {
            gameObject = this.gameObject,
            position = transform.position
        };

        CombatEvent skillEvent = new CombatEvent
        {
            Sender = this,
            HitInfo = castInfo
        };

        CombatSystem.Instance.Subscribe.OnSomeoneCastSkill?.Invoke(skillEvent, currentSkill);
    }

    // --- ICombatAgent Implementation ---

    public void TakeDamage(float damage, HitInfo hitInfo) 
    { 
        // 본체 피격은 PlayerStatusController에서 별도로 처리함
    }

    public void OnHitDetected(HitInfo hitInfo)
    {
        if (currentSkill == null) return;

        // 데미지 계산 로직
        float damage = (Player.Instance.ATK + Player.Instance.BonusATK) + (currentSkill.Value * currentSkill.Level);
        
        CombatEvent combatEvent = new CombatEvent();
        combatEvent.Sender = this;
        combatEvent.Receiver = hitInfo.receiver;
        combatEvent.Damage = damage;
        combatEvent.HitInfo = hitInfo;

        CombatSystem.Instance.AddCombatEvent(combatEvent);
    }
}

    