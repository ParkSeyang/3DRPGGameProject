using UnityEngine;

public class CombatEventBinder
{
    public void Enable()
    {
        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.Subscribe.OnSomeoneTakeDamage += OnSomeoneTakeDamage;
        }
    }

    public void Disable()
    {
        if (CombatSystem.IsInitialized)
        {
            CombatSystem.Instance.Subscribe.OnSomeoneTakeDamage -= OnSomeoneTakeDamage;
        }
    }
    
    private void OnSomeoneTakeDamage(CombatEvent combatEvent)
    {
        // 예시: 파라미터에 따라 다른 이펙트 소환 (Red / Yellow)
        // 실제 프로젝트에서는 ObjectPoolManager나 EffectManager를 통해 처리
        string key = combatEvent.HitInfo.parameter == 1 ? "Red" : "Yellow";
        
        Debug.Log($"[CombatEventBinder] Effect Spawn Request: {key} at {combatEvent.HitInfo.position}");
        
        // TODO: ObjectPoolManager 연동 필요
        // ObjectPoolManager.Instance.SpawnFxObject(key, combatEvent.HitInfo.position);
    }
}

