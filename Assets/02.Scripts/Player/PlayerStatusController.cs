using UnityEngine;

public class PlayerStatusController : SingletonBase<PlayerStatusController>
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
}