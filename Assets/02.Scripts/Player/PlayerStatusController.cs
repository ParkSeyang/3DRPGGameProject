using UnityEngine;

public class PlayerStatusController : SingletonBase<PlayerStatusController>, ICombatAgent
{
    private Player player;

    protected override void OnInitialize()
    {
        // Player 싱글톤 참조
        player = Player.Instance;
        if (player == null)
        {
            Debug.LogError("[PlayerStatusController] Player 인스턴스를 찾을 수 없습니다.");
        }
        
        InitializeCombat();
    }

    private void InitializeCombat()
    {
        if (player == null) return;

        // Player 오브젝트 하위의 모든 HitBox와 HurtBox를 찾아 초기화
        // PlayerStatusController가 전투의 주체가 됨
        var hitBoxes = player.GetComponentsInChildren<HitBox>(true);
        foreach (var hb in hitBoxes)
        {
            hb.Initialize(this);
        }

        var hurtBoxes = player.GetComponentsInChildren<HurtBox>(true);
        foreach (var hb in hurtBoxes)
        {
            hb.Initialize(this);
        }
        
        Debug.Log("[PlayerStatusController] 전투 컴포넌트 초기화 완료");
    }

    // --- ICombatAgent Implementation ---

    public void TakeDamage(float damage, HitInfo hitInfo)
    {
        if (player == null) return;

        float finalDamage = damage;

        // 가드 판정 확인 (Player 오브젝트에 있는 PlayerGuard 컴포넌트 참조)
        var playerGuard = player.GetComponent<PlayerGuard>();
        
        if (playerGuard != null && hitInfo.hitTarget != null)
        {
            // 맞은 콜라이더가 가드 콜라이더인지 확인
            if (playerGuard.IsGuardSuccess(hitInfo.hitTarget.Collider))
            {
                finalDamage *= 0.5f; // 50% 데미지 감소
                Debug.Log("<color=blue>[Player] 가드 성공! 데미지 50% 경감</color>");
                
                // 가드 성공 시 이펙트/애니메이션 등은 PlayerGuard 혹은 Animator에서 처리 권장
            }
        }

        // 방어력 계산
        float defense = player.DEF + player.BonusDEF;
        finalDamage = Mathf.Max(1f, finalDamage - defense);
        
        // 체력 적용
        player.SetHP(player.HP - finalDamage);
        
        Debug.Log($"[PlayerStatusController] 피격! 데미지: {finalDamage}, 남은 HP: {player.HP}");

        if (player.HP <= 0)
        {
            // 사망 처리
            player.GetComponent<Animator>()?.SetTrigger("Dead");
            HandleDeathPenalty();
        }
        else
        {
            // 가드가 아닐 때만 피격 모션 재생 등
            player.GetComponent<Animator>()?.SetTrigger("Hit");
        }
    }

    public void OnHitDetected(HitInfo hitInfo)
    {
        if (player == null) return;

        CombatEvent combatEvent = new CombatEvent();
        combatEvent.Sender = this;
        combatEvent.Receiver = hitInfo.receiver;
        // 데미지 계산
        combatEvent.Damage = player.ATK + player.BonusATK;
        combatEvent.HitInfo = hitInfo;

        CombatSystem.Instance.AddCombatEvent(combatEvent);
        
        Debug.Log($"[PlayerStatusController] 공격 적중! 대상: {hitInfo.receiver}, 데미지: {combatEvent.Damage}");
    }

    // 경험치 획득 및 레벨업 체크
    public void AddExp(int amount)
    {
        if (player == null) return;

        player.AddExp(amount);
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        // 경험치가 최대치를 초과했을 경우 레벨업 처리 (다중 레벨업 지원)
        while (player.Exp >= player.MaxExp)
        {
            player.SetExp(player.Exp - player.MaxExp); // 남은 경험치 이월
            player.SetLevel(player.Level + 1);

            // 레벨업 효과: HP/MP 완전 회복
            player.SetHP(player.MaxHP);
            player.SetMP(player.MaxMP);

            // 다음 레벨 필요 경험치 증가 (예: 1.2배 증가)
            int nextMaxExp = (int)(player.MaxExp * 1.2f);
            player.SetMaxExp(nextMaxExp);

            Debug.Log($"[Level Up!] 레벨 {player.Level} 달성! (HP/MP 회복, 다음 필요 경험치: {nextMaxExp})");
            
            // TODO: 레벨업 UI 표시나 이펙트 재생 이벤트 호출 가능
        }
    }

    // 골드 획득
    public void AddGold(int amount)
    {
        if (player == null) return;
        player.AddGold(amount);
        Debug.Log($"[Gold] {amount}G 획득 (현재: {player.Gold}G)");
    }

    // 사망 시 페널티 처리 (전투 시스템이나 다른 곳에서 호출)
    public void HandleDeathPenalty()
    {
        if (player == null) return;

        // 예시: 골드 20% 손실
        int penalty = (int)(player.Gold * 0.2f);
        player.AddGold(-penalty); // 음수 값을 더해서 차감

        Debug.Log($"[Death] 사망 페널티: {penalty}G 소실");
    }

    public void ApplySaveData(UserSaveData data)
    {
        if (player == null || data == null) return;

        // 1. 위치 적용
        player.transform.position = data.GetPosition();

        // 2. 스탯 적용
        player.ApplyStatData(data.playerStat);

        Debug.Log("[PlayerStatusController] 세이브 데이터가 월드에 적용되었습니다.");
    }
}