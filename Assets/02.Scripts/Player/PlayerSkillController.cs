using UnityEngine;
using System;

public class PlayerSkillController : MonoBehaviour, ICombatAgent
{
    private Animator animator;
    private AnimEventReceiver animEventReceiver;
    private Skill currentSkill; // 현재 시전 중인 스킬

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
        currentSkill = skill;
        animator.SetTrigger(triggerName);
        Debug.Log($"[SkillController] {skill.SkillName} 애니메이션 트리거: {triggerName}");
    }

    // 기존 PlayerAttack의 방식을 응용한 이벤트 수신처
    private void OnTriggerAnim(string parameter)
    {
        // 애니메이션 이벤트(AnimTriggerEventSender)에서 "Skill_Cast" 파라미터를 보낼 때 발동
        if (parameter.Equals("Skill_Cast"))
        {
            SpawnSkillEffect();
        }
        
        // 필요하다면 Attack_Start / Attack_End 처럼 스킬 히트박스를 정교하게 제어할 수도 있습니다.
        Debug.Log($"[Skill Event] Received: {parameter}");
    }

    private void SpawnSkillEffect()
    {
        if (currentSkill == null || currentSkill.EffectPrefab == null) return;

        // 이펙트 생성 위치 (기본 높이는 1.0f)
        float yOffset = 1.0f;
        
        // 4번 스킬(대지분쇄)은 바닥에서 터져야 하므로 높이를 낮춤
        if (currentSkill.SkillID == 4) yOffset = 0.1f;

        Vector3 spawnPos = transform.position + transform.forward * 3.0f + Vector3.up * yOffset;
        
        // 회전값 계산
        Quaternion spawnRot = transform.rotation;

        // 스킬별 회전 보정 (검의 궤적에 맞추기)
        if (currentSkill.SkillID == 2) // SwordSlash
        {
            spawnRot *= Quaternion.Euler(0, 0, 45); // 사선 베기 (각도는 프리팹에 맞게 조절)
        }
        
        GameObject effectObj = Instantiate(currentSkill.EffectPrefab, spawnPos, spawnRot);

        // 스킬 이펙트 사이즈 대폭 확대 (균등하게 5배)
        // ※ 프리팹 내부 자식들의 스케일이 (1,1,1)이 아닌 경우 비정상적으로 보일 수 있음
        effectObj.transform.localScale = Vector3.one * 5.0f; 

        // HitBox 초기화
        var hitBox = effectObj.GetComponent<HitBox>();
        if (hitBox != null)
        {
            hitBox.Initialize(this); // 나(PlayerSkillController)를 주인으로 설정
            hitBox.EnableDetection(); // 즉시 판정 시작
        }
        else
        {
            Debug.LogWarning($"[Skill] {currentSkill.SkillName} 프리팹에 HitBox가 없습니다. 데미지가 들어가지 않습니다.");
        }

        // 2초 뒤 삭제
        Destroy(effectObj, 2.0f);
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
        
        Debug.Log($"[Skill Hit] {currentSkill.SkillName} (Lv.{currentSkill.Level}) 적중! 대상: {hitInfo.receiver}, 데미지: {damage}");
    }
}