using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    int level = 1;
    int experience = 0;
    [SerializeField] ExperienceBar experienceBar;
    [SerializeField] UpgradePanelManager upgradeManager;

    [SerializeField] List<UpgradeData> upgrades;
    [SerializeField] List<UpgradeData> synergyUpgrades;
    List<UpgradeData> randomPool = new List<UpgradeData>();
    List<UpgradeData> selectedUpgrads;

    [SerializeField] List<UpgradeData> acquiredUpgrades;
    [SerializeField] int itemsAquired;
    [SerializeField] int maxItemLimit = 5;

    WeaponManager weaponManager;
    PassiveItems passiveItems;
    SynergyManager synergyManager;
    bool NoMoreUpgrade;

    [SerializeField] List<UpgradeData> upgradesAvailableOnStart;

    [SerializeField] List<UpgradeData> instantUpgrade = new List<UpgradeData>();

    CoinManager coinManager;

    [Header("연속 레벨업 제한")]
    [SerializeField] int maxConsecutivePanels = 3; // 최대 몇 번까지 패널을 띄울지. 지금은 사용안함. Popup manager에서 담당
    int consecutiveLevelUpCount = 0; // 현재 연속 레벨업 횟수

    [Header("리드 오리 가중치")]
    [SerializeField] float leadWeaponWeightMultiplier = 5f;
    [SerializeField] float leadSynergyItemWeightMultiplier = 2f; // ⭐ 추가: 리드 오리 시너지 짝 아이템 가중치
    WeaponData leadWeaponData;

    [Header("Debug")]
    [SerializeField] float Exp;
    [SerializeField] float ExpToLevelUp;

    int To_Level_Up
    {
        get
        {
            return (int)(50 * Mathf.Pow(level, 2.5f)) + 150;
        }
    }

    public void ActivateNoMoreUpgrade()
    {
        NoMoreUpgrade = true;
        upgradeManager.ClosePanel();
    }


    void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
        passiveItems = GetComponent<PassiveItems>();
        synergyManager = GetComponent<SynergyManager>();

        NoMoreUpgrade = false;

        // 디버그
        ExpToLevelUp = To_Level_Up;
    }

    void Start()
    {
        coinManager = GameManager.instance.GetComponent<CoinManager>();

        // 리드 오리 정보 캐싱 (스테이지 중에는 바뀌지 않음)
        leadWeaponData = GameManager.instance.startingDataContainer.GetLeadWeaponData();

        experienceBar.UpdateExperienceSlider(experience, To_Level_Up);
        experienceBar.SetLevelText(level);
        AddUpgradesIntoTheListOfAvailableUpgrades(upgradesAvailableOnStart);
    }

    public void AddExperience(int expAmount)
    {
        bool bossDead = BossDieManager.instance.IsBossDead;
        GameMode gameMode = PlayerDataManager.Instance.GetGameMode();
        if (bossDead && gameMode == GameMode.Regular)
        {
            Logger.Log($"[Level] 보스가 죽어서 경험치 증가를 막습니다.");
            return;
        }
        if (GameManager.instance.IsPlayerDead)
        {
            Logger.LogWarning($"[Level] 플레이어가 죽어서 경험치 증가를 막습니다.");
        }
        experience += expAmount;
        Exp = experience;
        ExpToLevelUp = To_Level_Up;
        CheckLevelUp();
    }

    public void CheckLevelUp()
    {
        // 레벨업을 했는데도 경험치가 레벨업 경험치보다 높으면 계속 레벨업
        while (experience >= To_Level_Up)
        {
            if (NoMoreUpgrade)
            {
                // 업그레이드 없이 자동 레벨업
                level++;
                experience -= To_Level_Up;
                experienceBar.SetLevelText(level);
            }
            else
            {
                UIEvent upgradeEvent = new UIEvent(() => LevelUp(), "Upgrade", upgradeManager.ForceClose); // ⭐ ForceClose 연결
                GameManager.instance.popupManager.EnqueueUIEvent(upgradeEvent);
                break; // 패널을 열 때는 한 번만
            }
        }

        experienceBar.UpdateExperienceSlider(experience, To_Level_Up);
    }

    void LevelUp()
    {
        if (GameManager.instance.IsPlayerDead)
        {
            Logger.LogWarning($"[Level] 플레이어가 죽어서 레벨업을 막습니다.");
        }

        if (selectedUpgrads == null)
        {
            selectedUpgrads = new List<UpgradeData>();
        }

        selectedUpgrads.Clear();
        selectedUpgrads.AddRange(GetRandomUpgrades());

        if (NoMoreUpgrade)
            return;

        upgradeManager.OpenPanel(selectedUpgrads);
    }
    public int GetExpToLevelUp()
    {
        return To_Level_Up;
    }
    public void ApplyUpdatedLevel()
    {
        if (experience > To_Level_Up) experience -= To_Level_Up; // 경험치가 0보다 작아지는 경우가 생긴다. 이유는 잘 모르겠다.
        level++;
        experienceBar.SetLevelText(level);
        experienceBar.UpdateExperienceSlider(experience, To_Level_Up);

        //Debug.Log("다음 업그레이드를 위해 필요한 경험치 = " + To_Level_Up);
    }

    // 알을 통해 무기를 얻을 경우
    public void GetWeapon(UpgradeData data)
    {
        if (selectedUpgrads == null)
        {
            selectedUpgrads = new List<UpgradeData>();
        }
        selectedUpgrads.Clear();
        selectedUpgrads.Add(data);
        Upgrade(0);
    }

    // LevelUp()을 통해 무기를 얻거나 업그레이드 하는 경우
    public void Upgrade(int selectedUpgradeID)
    {
        UpgradeData upgradeData = selectedUpgrads[selectedUpgradeID];

        if (acquiredUpgrades == null) { acquiredUpgrades = new List<UpgradeData>(); }

        switch (upgradeData.upgradeType)
        {
            case UpgradeType.WeaponUpgrade:
                weaponManager.UpgradeWeapon(upgradeData);
                break;
            case UpgradeType.ItemUpgrade:
                passiveItems.UpgradeItem(upgradeData);
                break;
            case UpgradeType.WeaponGet:
                weaponManager.AddWeapon(upgradeData.weaponData, false);
                // Logger.LogError($"[Level] {upgradeData.weaponData.DisplayName} 을 Weapon Manager에 추가합니다.");
                break;
            case UpgradeType.ItemGet:
                itemsAquired++;
                passiveItems.Equip(upgradeData.item);
                AddUpgradesIntoTheListOfAvailableUpgrades(upgradeData.item.upgrades);
                break;
            case UpgradeType.Heal:
                GetComponent<Character>().Heal(upgradeData.itemStats.hp, true);
                break;
            case UpgradeType.Coin:
                coinManager.updateCurrentCoinNumbers(upgradeData.itemStats.coins);
                break;
            case UpgradeType.SynergyUpgrade:
                weaponManager.UpgradeWeapon(upgradeData);
                synergyManager.ActivateSynergyWeapon(upgradeData);
                break;
        }

        // 업그레이드를 할 목록에서 뺴고, 업그레이드를 한 목록에 추가
        acquiredUpgrades.Add(upgradeData);
        upgrades.Remove(upgradeData);
    }

    void ShuffleRandomPool(List<UpgradeData> randomPool)
    {
        // 가중치 기반 정렬 (Efraimidis-Spirakis weighted sampling)
        // 가중치가 높을수록 key 값이 커질 확률이 높아져 앞쪽에 위치하기 쉬워짐
        List<KeyValuePair<float, UpgradeData>> keyed = new List<KeyValuePair<float, UpgradeData>>(randomPool.Count);

        for (int i = 0; i < randomPool.Count; i++)
        {
            float weight = GetWeightForUpgrade(randomPool[i]);
            float key = Mathf.Pow(Random.value, 1f / weight);
            keyed.Add(new KeyValuePair<float, UpgradeData>(key, randomPool[i]));
        }

        keyed.Sort((a, b) => b.Key.CompareTo(a.Key)); // 내림차순 정렬

        randomPool.Clear();
        for (int i = 0; i < keyed.Count; i++)
        {
            randomPool.Add(keyed[i].Value);
        }
    }

    List<UpgradeData> GetRandomUpgrades()
    {
        randomPool.Clear();
        List<UpgradeData> upgradeList = new List<UpgradeData>();

        // 먼저 아이템 제한을 적용하여 randomPool에 업그레이드 추가
        for (int i = 0; i < upgrades.Count; i++)
        {
            // 아이템 획득 제한 검사 - ItemGet 타입이고 이미 최대 아이템 개수에 도달했으면 추가하지 않음
            if (upgrades[i].upgradeType == UpgradeType.ItemGet && itemsAquired >= maxItemLimit)
                continue;

            randomPool.Add(upgrades[i]);
        }

        ShuffleRandomPool(randomPool);

        for (int index = 0; index < randomPool.Count; index++)
        {
            upgradeList.Add(randomPool[index]);

            // 동일한 무기/아이템 중복 제거
            for (int i = randomPool.Count - 1; i > index; i--)
            {
                // 무기 업그레이드라면 무기 업그레이드끼리만 비교
                if (randomPool[i].weaponData != null && randomPool[index].weaponData != null)
                {
                    if (randomPool[i].weaponData.Name == randomPool[index].weaponData.Name)
                    {
                        randomPool.Remove(randomPool[i]);
                        continue;
                    }
                }

                // 아이템 업그레이드라면 아이템 업그레이드끼리만 비교
                if (randomPool[i].item != null && randomPool[index].item != null)
                {
                    if (randomPool[i].item.Name == randomPool[index].item.Name)
                    {
                        randomPool.Remove(randomPool[i]);
                    }
                }
            }

            if (upgradeList.Count == 3)
            {
                // 가능한 시너지 업그레이드가 있다면 추가
                UpgradeData synergyUpgrade = synergyManager.GetSynergyUpgrade(); // ⭐ 한 번만 호출
                if (synergyUpgrade != null)
                {
                    upgradeList.Remove(upgradeList[0]); // 그냥 첫번째 슬롯의 업그레이드를 빼고
                    upgradeList.Add(synergyUpgrade); // 시너지 업그레이드 추가
                }
                return upgradeList;
            }
        }

        UpgradeData fallbackSynergyUpgrade = synergyManager.GetSynergyUpgrade(); // ⭐ 한 번만 호출
        if (fallbackSynergyUpgrade != null)
        {
            upgradeList.Add(fallbackSynergyUpgrade); // 업그레이드 리스트가 3보다 부족하다면 그냥 추가
        }

        // 랜덤풀을 비교? upgradeList를 비교해야 하지 않나?

        // 부족한 슬롯만큼 달콤우유나 동전을 추가
        List<UpgradeData> lacks = new List<UpgradeData>();

        int numberOfInstantUp = 3 - upgradeList.Count;
        if (numberOfInstantUp > 2) numberOfInstantUp = 2; // 중복으로 하트나 동전이 나오지 않도록

        for (int i = 0; i < numberOfInstantUp; i++)
        {
            // lacks.Add(instantUpgrade[Random.Range(0, instantUpgrade.Count)]);
            lacks.Add(instantUpgrade[i]); // 일단 순서대로 나오도록 했다. 나중에 랜덤으로 겹치지 않게 구현하기
        }
        upgradeList.AddRange(lacks);

        return upgradeList;
    }

    // 알에서 중복되는 무기가 나오지 않도록 하기위한 플래그
    public bool HavingWeapon(WeaponData weaponData)
    {
        WeaponData leadWd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        if (weaponData.Name == leadWd.Name) return true;

        // ⭐ 스쿼드 동료도 검사
        List<CompanionData> companions = GameManager.instance.startingDataContainer.GetCompanions();
        if (companions != null)
        {
            for (int i = 0; i < companions.Count; i++)
            {
                if (companions[i]?.weaponData != null && companions[i].weaponData.Name == weaponData.Name)
                    return true;
            }
        }

        List<UpgradeData> weaponUpgrades = new();
        for (int i = 0; i < acquiredUpgrades.Count; i++)
        {
            if (acquiredUpgrades[i].weaponData != null)
                weaponUpgrades.Add(acquiredUpgrades[i]);
        }
        return weaponUpgrades.Find(x => x.weaponData.Name == weaponData.Name) != null;
    }

    internal void AddUpgradesIntoTheListOfAvailableUpgrades(List<UpgradeData> upgradesToAdd)
    {
        if (upgradesToAdd == null)
            return;

        this.upgrades.AddRange(upgradesToAdd);
    }

    float GetWeightForUpgrade(UpgradeData data)
    {
        float weight = 1f;

        // 리드 오리 자신의 무기 업그레이드
        if (data.upgradeType == UpgradeType.WeaponUpgrade
            && data.weaponData != null
            && leadWeaponData != null
            && data.weaponData.Name == leadWeaponData.Name)
        {
            weight = leadWeaponWeightMultiplier;
        }
        // ⭐ 리드 오리와 시너지를 이루는 아이템 (획득/업그레이드 둘 다)
        else if ((data.upgradeType == UpgradeType.ItemGet || data.upgradeType == UpgradeType.ItemUpgrade)
            && data.item != null
            && leadWeaponData != null
            && leadWeaponData.SynergyWeapon != null
            && data.item.SynergyWeapons != null
            && data.item.SynergyWeapons.Contains(leadWeaponData.SynergyWeapon))
        {
            weight = leadSynergyItemWeightMultiplier;
        }

        return weight;
    }

    // =====================================================
    // Level.cs 의 #region 디버그 안에 아래 코드를 추가하세요
    // =====================================================

    #region 디버그

    public void LevelupDebug()
    {
        LevelUp();
    }

    /// <summary>
    /// 디버그용: 모든 업그레이드를 한 번에 적용합니다.
    ///
    /// 시너지 조건:
    ///   - 무기: 최대 레벨 도달
    ///   - 아이템: 획득만 하면 됨 (currentLevel >= 1, 업그레이드 불필요)
    ///
    /// 순서: WeaponGet → ItemGet → WeaponUpgrade(→시너지 풀 등록) → SynergyUpgrade → ItemUpgrade
    /// </summary>
    public void FullAutoUpgrade()
    {
        Logger.Log("[FullAutoUpgrade] 전체 자동 업그레이드 시작");

        // 1단계: 새 무기 획득
        ApplyAllUpgradesByType(UpgradeType.WeaponGet);

        // 2단계: 새 아이템 획득
        // 아이템은 획득(currentLevel >= 1)만 해도 시너지 조건 충족
        ApplyAllUpgradesByType(UpgradeType.ItemGet);

        // 3단계: 무기 최대 레벨까지 업그레이드
        // WeaponBase.CheckIfMaxLevel()이 내부 호출되어 조건 충족 시 시너지 풀에 자동 등록됨
        ApplyAllUpgradesByType(UpgradeType.WeaponUpgrade);

        // 4단계: 시너지 업그레이드 적용 (3단계에서 풀에 등록된 것 모두)
        ApplyAllSynergyUpgrades();

        // 5단계: 아이템 업그레이드 (시너지 조건과 무관, 스탯 강화 목적)
        ApplyAllUpgradesByType(UpgradeType.ItemUpgrade);

        Logger.Log("[FullAutoUpgrade] 전체 자동 업그레이드 완료");
    }

    /// <summary>
    /// 특정 타입의 업그레이드를 upgrades 리스트에서 전부 꺼내 적용합니다.
    /// </summary>
    void ApplyAllUpgradesByType(UpgradeType type)
    {
        // FindAll로 별도 리스트 생성 → 원본을 순회 중에 수정해도 안전
        List<UpgradeData> targets = upgrades.FindAll(u => u.upgradeType == type);

        foreach (UpgradeData upgradeData in targets)
        {
            if (selectedUpgrads == null)
                selectedUpgrads = new List<UpgradeData>();

            selectedUpgrads.Clear();
            selectedUpgrads.Add(upgradeData);
            Upgrade(0);

            ApplyUpdatedLevel();

            Logger.Log($"[FullAutoUpgrade] 적용됨: [{type}] {upgradeData.name}");
        }
    }

    /// <summary>
    /// SynergyManager 풀에 있는 시너지 업그레이드를 모두 적용합니다.
    /// </summary>
    void ApplyAllSynergyUpgrades()
    {
        int safetyLimit = 20; // 무한루프 방지
        int count = 0;

        while (count < safetyLimit)
        {
            UpgradeData synergy = synergyManager.GetSynergyUpgrade();
            if (synergy == null) break;

            if (selectedUpgrads == null)
                selectedUpgrads = new List<UpgradeData>();

            selectedUpgrads.Clear();
            selectedUpgrads.Add(synergy);
            Upgrade(0);

            ApplyUpdatedLevel();

            Logger.Log($"[FullAutoUpgrade] 시너지 적용됨: {synergy.name}");
            count++;
        }
    }

    #endregion
}
