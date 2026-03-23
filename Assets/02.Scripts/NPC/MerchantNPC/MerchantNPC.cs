using UnityEngine;
using System.Collections.Generic;

public enum MerchantType { Smithy, Artifact, Potion, Wandering }

public class MerchantNPC : NPC
{
    [Header("Merchant Settings")]
    public MerchantType MerchantKind; // Smithy, Artifact, Potion, Wandering
    public string DialogueKey;        // TSV의 Category (예: Smithy) 기반 식별 키
    public int MerchantGold = 1000;   // 상인의 초기 소지금
    
    public string ShopName = "Trader"; // 연동할 TraderInventory 이름

    protected override void OnInteract()
    {
        // 1. 거래 시작 대화 생성 (DialogueKey + _Start)
        string startKey = $"{DialogueKey}_Start";
        var dialogs = DialogDataManager.Instance.GetDialogsByKey(startKey);
        
        // UIManager에서 Dialogue UI 가져오기
        var dialogUI = UIManager.Instance.GetDialogueUI();
        
        if (dialogs != null && dialogUI != null)
        {
            // 대화가 끝나면 상점을 열도록 콜백 등록
            dialogUI.StartDialog(dialogs, NPCName, "", () => 
            {
                PrepareShop();
                OpenShopUI();
            });
        }
        else
        {
            // 대화 데이터가 없으면 즉시 상점 오픈
            PrepareShop();
            OpenShopUI();
        }
    }

    private void PrepareShop()
    {
        var traderInven = InventorySystem.Instance.GetInventoryOrNull(ShopName) as TraderInventory;
        if (traderInven == null) return;

        List<string> itemToSell = new List<string>();
        var allItemInfos = ItemDataManager.Instance.ItemInfoTable;

        switch (MerchantKind)
        {
            case MerchantType.Smithy:
                foreach (var info in allItemInfos.Values)
                    if (info.ItemCategory == "Weapon" || info.ItemCategory == "Armor") itemToSell.Add(info.ItemID);
                break;

            case MerchantType.Artifact:
                foreach (var info in allItemInfos.Values)
                    if (info.ItemCategory == "Artifact") itemToSell.Add(info.ItemID);
                break;

            case MerchantType.Potion:
                foreach (var info in allItemInfos.Values)
                    if (info.ItemCategory == "Potion") itemToSell.Add(info.ItemID);
                break;

            case MerchantType.Wandering:
                var keys = new List<string>(allItemInfos.Keys);
                for (int i = 0; i < 4 && keys.Count > 0; i++)
                {
                    int randIdx = Random.Range(0, keys.Count);
                    itemToSell.Add(keys[randIdx]);
                    keys.RemoveAt(randIdx);
                }
                break;
        }

        traderInven.SetShopItems(itemToSell);
        traderInven.SetGold(MerchantGold);
    }

    private void OpenShopUI()
    {
        UIManager.Instance.CurrentMerchant = this;
        UIManager.Instance.ToggleUI(UIType.Trade);
        
        // 코루틴을 통해 UI 활성화 시간을 확보한 뒤 아이템 주입
        StartCoroutine(DelayedShopSetup());
    }

    private System.Collections.IEnumerator DelayedShopSetup()
    {
        // UI가 완전히 켜질 때까지 한 프레임 대기 (실제 시간 기준)
        yield return new WaitForSecondsRealtime(0.01f);
        PrepareShop();
    }

    // 상점이 닫힐 때 호출될 메서드
    public void OnShopClosed()
    {
        string endKey = $"{DialogueKey}_End";
        var dialogs = DialogDataManager.Instance.GetDialogsByKey(endKey);
        
        if (dialogs != null)
        {
            var dialogUI = UIManager.Instance.GetDialogueUI();
            if (dialogUI != null)
            {
                // 즉시 호출하되, 혹시 모를 팝업 닫기 로직과의 충돌을 방지하기 위해 
                // UIManager의 상태 갱신을 한 번 더 유도
                dialogUI.StartDialog(dialogs, NPCName, "", null);
            }
        }
    }
}

    