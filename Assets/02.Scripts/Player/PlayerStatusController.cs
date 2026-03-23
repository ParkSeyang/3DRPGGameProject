using UnityEngine;

public class PlayerStatusController : SingletonBase<PlayerStatusController>, ICombatAgent
{
    private Player player;

    [Header("Effects")]
    [SerializeField] private GameObject levelUpEffectPrefab;

    // --- 전투 관련 상수 (Magic Number 제거) ---
    private const float GuardAngleThreshold = 0.2f; // 전방 약 160도 범위 방어 (0.4 -> 0.2로 완화)
    private const float GuardDamageReduction = 0.5f; // 가드 성공 시 데미지 50% 경감

    private Player EnsurePlayer()
    {
        if (player == null)
        {
            player = Player.Instance;
        }
        return player;
    }

    protected override void OnInitialize()
    {
        EnsurePlayer();
        InitializeCombat();
    }

    private void InitializeCombat()
    {
        var targetPlayer = EnsurePlayer();
        if (targetPlayer == null) return;

        var hitBoxes = targetPlayer.GetComponentsInChildren<HitBox>(true);
        foreach (var hitBox in hitBoxes)
        {
            hitBox.Initialize(this);
        }

        var hurtBoxes = targetPlayer.GetComponentsInChildren<HurtBox>(true);
        foreach (var hurtBox in hurtBoxes)
        {
            hurtBox.Initialize(this);
        }
    }

    // --- ICombatAgent Implementation ---

    public void TakeDamage(float damage, HitInfo hitInfo)
    {
        if (player == null) return;

        float finalDamage = damage;

        // --- 가드 판정 로직 강화 ---
        var playerGuard = player.GetComponent<PlayerGuard>();
        if (playerGuard != null && playerGuard.IsGuarding)
        {
            bool isGuardSuccess = false;

            // [수정] 단순히 버튼을 누른 상태가 아니라, 애니메이션 이벤트(Guard_On)로 인해 콜라이더가 켜졌을 때만 성공으로 간주
            if (hitInfo.hitTarget != null && playerGuard.IsGuardCollider(hitInfo.hitTarget.Collider))
            {
                // 방패 콜라이더에 직접 맞은 경우
                isGuardSuccess = true;
            }
            else
            {
                // 가드 상태(IsGuarding)인데 몸에 맞았더라도, 전방 판정인지 체크
                // ※ 여기서 Guard_On 이벤트가 발생하여 실제로 본체 콜라이더가 꺼졌는지 여부가 중요
                Vector3 directionToHit = (hitInfo.position - player.transform.position).normalized;
                directionToHit.y = 0; 
                float dotProduct = Vector3.Dot(player.transform.forward, directionToHit);

                // 전방에서 왔고, 현재 애니메이션상 가드 판정이 활성화된 상태여야 함
                if (dotProduct > GuardAngleThreshold && hitInfo.hitTarget.Collider.enabled == false) 
                {
                    // 본체 콜라이더가 꺼져있다는 건 가드가 유효하게 작동 중이라는 증거
                    isGuardSuccess = true;
                }
            }

            if (isGuardSuccess)
            {
                finalDamage *= GuardDamageReduction;
                
                // [옵저버 패턴] 가드 이벤트 발생 (이때 가드 이펙트가 터짐)
                CombatEvent guardEvent = new CombatEvent { Receiver = this, HitInfo = hitInfo };
                CombatSystem.Instance.Subscribe.OnSomeoneGuard?.Invoke(guardEvent);
            }
        }

        // --- 방어력 계산 및 최소 데미지(1) 보정 ---
        float totalDefense = player.DEF + player.BonusDEF;
        finalDamage = Mathf.Max(1f, finalDamage - totalDefense);
        
        // 체력 적용
        player.SetHP(player.HP - finalDamage);

        if (player.HP <= 0)
        {
            // 사망 처리
            player.GetComponent<Animator>()?.SetTrigger("Dead");
            HandleDeathPenalty();

            // 1.5초 후 게임 오버 UI 출력 (사망 애니메이션 시청 시간 확보)
            Invoke(nameof(RequestGameOverUI), 1.5f);
        }
        else
        {
            // [수정] 상단에서 이미 선언된 playerGuard를 재사용하여 중복 선언 해결
            bool isGuarding = playerGuard != null && playerGuard.IsGuarding;

            if (isGuarding == false)
            {
                player.GetComponent<Animator>()?.SetTrigger("Hit");
                player.GetComponent<PlayerSkillController>()?.CancelSkill(); // [추가] 피격 시 스킬 상태 해제
                player.GetComponent<PlayerGuard>()?.CancelGuardAction(); // [추가] 피격 시 가드 동작 해제
            }
        }
    }

    /// <summary>
    /// 플레이어 사망 애니메이션 이후 게임 오버 메뉴를 호출합니다.
    /// </summary>
    private void RequestGameOverUI()
    {
        if (UIManager.IsInitialized)
        {
            UIManager.Instance.ShowGameOver();
        }
    }

    public void OnHitDetected(HitInfo hitInfo)
    {
        if (player == null) return;

        CombatEvent combatEvent = new CombatEvent();
        combatEvent.Sender = this;
        combatEvent.Receiver = hitInfo.receiver;
        combatEvent.Damage = player.ATK + player.BonusATK;
        combatEvent.HitInfo = hitInfo;

        CombatSystem.Instance.AddCombatEvent(combatEvent);
    }

    // 경험치 획득 및 레벨업 체크
    public void AddExp(int amount)
    {
        if (player == null) return;

        player.AddExp(amount);
        
        // [수정] 값 반영 후 SetExp를 호출하여 UI 이벤트를 강제로 발생시킴
        player.SetExp(player.Exp);
        
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (player.Exp >= player.MaxExp)
        {
            player.SetExp(player.Exp - player.MaxExp); 
            player.SetLevel(player.Level + 1);

            player.AddMaxHP(10f);
            player.AddMaxMP(10f);
            player.AddBaseATK(5f);
            player.AddBaseDEF(1f);

            player.AddSP(4);

            player.SetHP(player.MaxHP);
            player.SetMP(player.MaxMP);

            int nextMaxExp = (int)(player.MaxExp * 1.2f);
            player.SetMaxExp(nextMaxExp);

            if (levelUpEffectPrefab != null)
            {
                GameObject effect = Instantiate(levelUpEffectPrefab, player.transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * 8.0f; 
                Destroy(effect, 2.0f);
            }
        }
    }

    // 골드 획득
    public void AddGold(int amount)
    {
        if (player == null) return;
        player.AddGold(amount);
    }

    // 사망 시 페널티 처리
    public void HandleDeathPenalty()
    {
        if (player == null || DataManager.Instance == null) return;

        int goldPenalty = (int)(player.Gold * 0.2f);
        player.AddGold(-goldPenalty); 

        UserSaveData saveContainer = DataManager.Instance.LoadUserData();
        if (saveContainer != null && saveContainer.playerStat != null)
        {
            saveContainer.playerStat.Gold = player.Gold; 
            DataManager.Instance.SaveUserData(saveContainer);
        }
    }

    public UserSaveData GetSaveData()
    {
        var targetPlayer = EnsurePlayer();
        if (targetPlayer == null) return null;

        var saveData = new UserSaveData();
        saveData.userName = targetPlayer.Name;
        saveData.playerStat = targetPlayer.GetCurrentStatData();
        saveData.SetPosition(targetPlayer.transform.position);
        saveData.rotY = targetPlayer.transform.eulerAngles.y; 
        saveData.lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; 

        if (InventoryDataManager.Instance != null)
        {
            saveData.userInventoryData = InventoryDataManager.Instance.GetInventoryData("User");
            saveData.equipInventoryData = InventoryDataManager.Instance.GetInventoryData("Equip");
            saveData.quickSlotData = InventoryDataManager.Instance.GetInventoryData("Quick");
        }

        if (SkillDataManager.Instance != null)
        {
            saveData.skillData = SkillDataManager.Instance.GetSaveData();
        }

        if (QuestManager.IsInitialized)
        {
            saveData.questSaveData = QuestManager.Instance.GetSaveData();
        }
        
        var skillSystem = targetPlayer.GetComponent<PlayerSkillSystem>();
        if (skillSystem != null)
        {
            saveData.skillSlotQ = skillSystem.SkillSlot_Q;
            saveData.skillSlotE = skillSystem.SkillSlot_E;
        }

        return saveData;
    }

    public void ApplySaveData(UserSaveData saveData)
    {
        if (player == null || saveData == null) return;

        InitializeCombat();

        float restoredHP = saveData.playerStat.HP;
        float restoredMP = saveData.playerStat.MP;

        player.transform.position = saveData.GetPosition();
        player.transform.rotation = Quaternion.Euler(0, saveData.rotY, 0);
        
        player.ApplyStatData(saveData.playerStat);

        if (SkillDataManager.Instance != null && saveData.skillData != null)
        {
            SkillDataManager.Instance.LoadFromSaveData(saveData.skillData);
            UpdatePassiveStats();
        }
        
        var skillSystem = player.GetComponent<PlayerSkillSystem>();
        if (skillSystem != null)
        {
            skillSystem.SkillSlot_Q = saveData.skillSlotQ;
            skillSystem.SkillSlot_E = saveData.skillSlotE;
        }

        if (InventoryDataManager.Instance != null)
        {
            player.AddBonusATK(-player.BonusATK); 
            player.AddBonusDEF(-player.BonusDEF);

            InventoryDataManager.Instance.SetInventoryData("User", saveData.userInventoryData);
            InventoryDataManager.Instance.SetInventoryData("Equip", saveData.equipInventoryData);
            InventoryDataManager.Instance.SetInventoryData("Quick", saveData.quickSlotData);
        }

        if (QuestManager.IsInitialized && saveData.questSaveData != null)
        {
            QuestManager.Instance.LoadSaveData(saveData.questSaveData);
        }
        
        player.SetHP(restoredHP);
        player.SetMP(restoredMP);
        player.RefreshAllStats();

        var animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    /// <summary>
    /// 아이템 효과를 적용합니다. (소모품 전용)
    /// </summary>
    /// <returns>아이템 소모 여부 (true: 소모, false: 유지)</returns>
    public bool ApplyItemEffect(Item item)
    {
        if (item == null || player == null) return false;

        if (item.ItemCategory == "Potion")
        {
            return HandlePotion(item);
        }
        
        // 장비 아이템 등은 소모품이 아니므로 여기서 처리하지 않고 false 반환
        // 장착 로직은 EquipItem 메서드에서 처리
        return false;
    }

    public void EquipItem(Item item)
    {
        if (item == null || player == null) return;

        switch (item.ItemCategory)
        {
            case "Weapon":
                player.AddBonusATK(item.Value);
                break;
            case "Armor":
                player.AddBonusDEF(item.Value);
                break;
            case "Artifact":
                player.AddMaxHP(25f);
                player.AddMaxMP(25f);
                break;
        }
    }

    public void UnequipItem(Item item)
    {
        if (item == null || player == null) return;

        switch (item.ItemCategory)
        {
            case "Weapon":
                player.AddBonusATK(-item.Value); // 차감
                break;
            case "Armor":
                player.AddBonusDEF(-item.Value);
                break;
            case "Artifact":
                player.AddMaxHP(-25f);
                player.AddMaxMP(-25f);
                break;
        }
    }

    private bool HandlePotion(Item item)
    {
        if (item == null) return false;
        
        // ID별 상세 로직 (소/중 구분은 item.Value에 이미 반영되어 있음)
        if (item.ItemID == "I001" || item.ItemID == "I003") // 체력 포션
        {
            player.SetHP(player.HP + item.Value);
            
            // [옵저버 패턴] 힐 이벤트 발생 (parameter 0: HP)
            CombatEvent healEvent = new CombatEvent { Receiver = this, HitInfo = new HitInfo { parameter = 0 } };
            CombatSystem.Instance.InvokeHealEvent(healEvent);
            
            return true;
        }
        else if (item.ItemID == "I002" || item.ItemID == "I004") // 마나 포션
        {
            player.SetMP(player.MP + item.Value);

            // [옵저버 패턴] 힐 이벤트 발생 (parameter 1: MP)
            CombatEvent healEvent = new CombatEvent { Receiver = this, HitInfo = new HitInfo { parameter = 1 } };
            CombatSystem.Instance.InvokeHealEvent(healEvent);

            return true;
        }

        return false;
    }

    // --- 패시브 스킬 시스템 연동 ---
    private float currentPassiveHp;
    private float currentPassiveMp;
    private float currentPassiveAtk;
    private float currentPassiveDef;

    public void UpdatePassiveStats()
    {
        if (SkillDataManager.Instance == null || player == null) return;

        // 1. 기존 패시브 효과 제거 (이전 계산값 차감)
        player.AddMaxHP(-currentPassiveHp);
        player.AddMaxMP(-currentPassiveMp);
        player.AddBonusATK(-currentPassiveAtk);
        player.AddBonusDEF(-currentPassiveDef);

        // 2. 새 패시브 효과 계산
        float newHp = 0, newMp = 0, newAtk = 0, newDef = 0;

        foreach (var skill in SkillDataManager.Instance.GetAllSkills())
        {
            if (skill.Type != SkillType.Passive || skill.Level == 0) continue;

            // 스킬 ID에 따른 효과 분기
            // ID 1: StrongBody (HP/MP)
            // ID 3: GreatSwordTraining (ATK)
            float effectValue = skill.GetCurrentValue();

            switch (skill.SkillID)
            {
                case 1: // StrongBody
                    newHp += effectValue;
                    newMp += effectValue;
                    break;
                case 3: // GreatSwordTraining
                    newAtk += effectValue;
                    break;
            }
        }

        // 3. 새 효과 적용
        player.AddMaxHP(newHp);
        player.AddMaxMP(newMp);
        player.AddBonusATK(newAtk);
        player.AddBonusDEF(newDef);

        // 4. 누적치 갱신
        currentPassiveHp = newHp;
        currentPassiveMp = newMp;
        currentPassiveAtk = newAtk;
        currentPassiveDef = newDef;
                
        player.RefreshAllStats();
    }
}
        