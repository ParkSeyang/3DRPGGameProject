 using UnityEngine;

 public class CheatManager : MonoBehaviour
 {
     private const int CHEAT_EXP_AMOUNT = 1000;

     private void Update()
     {
             // T 키를 누르면 경험치 획득
             if (Input.GetKeyDown(KeyCode.T))
             {
                   ApplyExpCheat();
             }
     }

     private void ApplyExpCheat()
     {
             // 1. 시스템 초기화 여부 확인 (안정성)
             if (PlayerStatusController.IsInitialized == false) return;
    
             // 2. 컨트롤러를 통해 경험치 주입 (레벨업 로직 자동 포함)
             PlayerStatusController.Instance.AddExp(CHEAT_EXP_AMOUNT);
    
             // 3. UI로 피드백 출력 (선택 사항)
             if (UIManager.IsInitialized)
             {
                     UIManager.Instance.ShowWarning($"[Cheat] 경험치 {CHEAT_EXP_AMOUNT}를 획득했습니다.");
             }
     }
}