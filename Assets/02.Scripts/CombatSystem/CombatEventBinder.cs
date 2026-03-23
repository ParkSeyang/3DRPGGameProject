using UnityEngine;

[System.Serializable]
public class CombatEffectData
{
    public GameObject hpHealEffect;
    public GameObject mpHealEffect;
    public GameObject guardEffect;
}

public class CombatEventBinder
{
    private CombatEffectData effectData;

    public void Initialize(CombatEffectData data)
    {
        effectData = data;
    }

    public void Enable()
    {
        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.Subscribe.OnSomeoneTakeDamage += OnSomeoneTakeDamage;
            CombatSystem.Instance.Subscribe.OnSomeoneHeal += OnSomeoneHeal;
            CombatSystem.Instance.Subscribe.OnSomeoneGuard += OnSomeoneGuard;
            CombatSystem.Instance.Subscribe.OnSomeoneCastSkill += OnSomeoneCastSkill;
        }
    }

    public void Disable()
    {
        // [수정] 종료 시점에 CombatSystem이 이미 파괴되었을 수 있으므로 안전하게 체크
        if (CombatSystem.IsInitialized == false) return;

        var system = CombatSystem.Instance;
        if (system != null && system.Subscribe != null)
        {
            system.Subscribe.OnSomeoneTakeDamage -= OnSomeoneTakeDamage;
            system.Subscribe.OnSomeoneHeal -= OnSomeoneHeal;
            system.Subscribe.OnSomeoneGuard -= OnSomeoneGuard;
            system.Subscribe.OnSomeoneCastSkill -= OnSomeoneCastSkill;
        }
    }
    
    private void OnSomeoneTakeDamage(CombatEvent combatEvent)
    {
        // 데미지 텍스트나 피격 이펙트 처리 (필요 시)
    }

    private void OnSomeoneHeal(CombatEvent combatEvent)
    {
        if (effectData == null)
        {
            return;
        }

        // parameter를 통해 HP(0) / MP(1) 구분
        GameObject prefab = combatEvent.HitInfo.parameter == 0 ? effectData.hpHealEffect : effectData.mpHealEffect;
        
        if (prefab == null)
        {
            return;
        }

        if (combatEvent.Receiver is PlayerStatusController playerController)
        {
            GameObject healEffectInstance = Object.Instantiate(prefab, playerController.transform.position, Quaternion.identity);
            healEffectInstance.transform.SetParent(playerController.transform);
            Object.Destroy(healEffectInstance, 2.0f);
        }
    }

    private void OnSomeoneGuard(CombatEvent combatEvent)
    {
        if (effectData == null || effectData.guardEffect == null) return;

        Vector3 spawnPos = combatEvent.HitInfo.position;

        // 플레이어의 PlayerGuard 컴포넌트에서 EffectPoint를 찾음
        if (combatEvent.Receiver is PlayerStatusController playerStatus)
        {
            var guard = playerStatus.GetComponent<PlayerGuard>();
            if (guard != null && guard.EffectPoint != null)
            {
                spawnPos = guard.EffectPoint.position;
            }
        }

        GameObject guardEffectInstance = Object.Instantiate(effectData.guardEffect, spawnPos, Quaternion.identity);
        guardEffectInstance.transform.localScale = Vector3.one * 5.0f; // [추가] 가드 이펙트 크기 확대
        Object.Destroy(guardEffectInstance, 1.5f);
    }

    private void OnSomeoneCastSkill(CombatEvent combatEvent, Skill skill)
    {
        if (skill == null || skill.EffectPrefab == null) return;

        // [수정] 이펙트 생성 높이(yOffset) 세분화
        float yOffset = 1.0f; // 기본 높이
        if (skill.SkillID == 4) yOffset = 0.1f; // 대지분쇄: 바닥 안착
        else if (skill.SkillID == 2) yOffset = 1.8f; // 참격(Q): 조금 더 위로

        float forwardDistance = skill.SkillID == 2 ? 5.0f : 3.0f; // 2번 스킬(참격)은 더 앞에서 생성

        Vector3 spawnPos = combatEvent.HitInfo.position + combatEvent.HitInfo.gameObject.transform.forward * forwardDistance + Vector3.up * yOffset;
        Quaternion spawnRot = combatEvent.HitInfo.gameObject.transform.rotation;

        if (skill.SkillID == 2) // SwordSlash 사선 보정
        {
            spawnRot *= Quaternion.Euler(0, 0, 45);
        }

        GameObject skillEffectInstance = Object.Instantiate(skill.EffectPrefab, spawnPos, spawnRot);
        skillEffectInstance.transform.localScale = Vector3.one * 6.5f;

        // HitBox 초기화 (이벤트 발신자(Sender)를 주인으로 설정)
        var hitBox = skillEffectInstance.GetComponent<HitBox>();
        if (hitBox != null)
        {
            hitBox.Initialize(combatEvent.Sender);
            hitBox.EnableDetection();
        }

        Object.Destroy(skillEffectInstance, 2.0f);
    }
}

