using System.Collections.Generic;
using UnityEngine;

public class EggPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] List<UpgradeData> upgradeToPick;
    [SerializeField] List<StageItemData> stageUpgradeDatas;
    int index;

    public override void OnHitMagnetField(Vector2 direction)
    {
        // 알은 자력에 영향을 받지 않는다
    }

    void InitUpgradePool()
    {
        if(upgradeToPick == null) upgradeToPick = new List<UpgradeData>();

    }
    List<UpgradeData> GetUpgradePool(int maxUpgradeNum)
    {
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        List<StageItemData> upgradePool = new List<StageItemData>(); // 하나씩 빼면서 랜덤하게 뽑을 풀
        List<UpgradeData> pickedUpgrades = new List<UpgradeData>(); // 뽑은 아이템을 저장할 리스트
        upgradePool.AddRange(stageUpgradeDatas);

        int itemNumbers = 0;
        while (itemNumbers < maxUpgradeNum && upgradePool.Count > 0)
        {
            int index = Random.Range(0, upgradePool.Count);
            if (upgradePool[index].stage >= stageNum) // 현재 스테이지보다 크거나 같은 스테이지의 아이템만 선택
            {
                pickedUpgrades.Add(upgradePool[index].upgrade); // 나중에 Level에 넘겨줄 아이템 리스트
                itemNumbers++;
            }
            upgradePool.RemoveAt(index); // 뽑혔거나 조건이 충족하지 않는 아이템은 삭제
        }

        return pickedUpgrades;
    }

    // 플레이어에게 무기를 설치한 후 Egg 이벤트 화면으로 들어간다
    public void OnPickUp(Character character)
    {
        if(upgradeToPick == null) upgradeToPick = new List<UpgradeData>();

        // 1스테이지 업그레이드 2개, 2스테이지 3개, 5 이상이 되면 5로 고정
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        int itemNums = stageNum + 1;
        if (itemNums > 5) itemNums = 5;
        upgradeToPick = GetUpgradePool(itemNums);
        
        // 항목을 플레이어가 이미 가지고 있는지 체크
        // upgradeToPick을 remove하면서 탐색하면 에러가 생기므로 똑같은 리스트를 만들어서 반복탐색
        List<UpgradeData> checks = new List<UpgradeData>();
        checks.AddRange(upgradeToPick);

        for (int i = 0; i < checks.Count; i++)
        {
            if (character.GetComponent<Level>().HavingWeapon(checks[i]))
            {
                upgradeToPick.Remove(checks[i]);
                // Debug.Log("겹치는 무기 = " + item.Name);
                continue;
            }
            // 리드 오리 무기는 weaponData의 start에서 추가되어 버리므로(Level의 Get weapon을 거치지 않고) acquired 항목으로 acquire upgrade data를 넣지 않는다
            // 그래서 Having Weapon 함수로는 검색이 안되니까 따로 리드오리의 weapon data를 가져와서 이름을 비교한다 
            if(GameManager.instance.startingDataContainer.GetLeadWeaponData().Name == checks[i].weaponData.Name)
            {
                upgradeToPick.Remove(checks[i]);
            }
        }

        if (upgradeToPick.Count == 0)
        {
            // 알에서 무기 말고 아이템을 나오게 할 생각임
            // 해당 스테이지에서 얻을 수 있는 무기를 모두 얻은 상태라면,
            // 아이템을 드롭하도록 구현하기
            // 지금은 일단 아무것도 하지 않게 했음
            return;
        }
        index = Random.Range(0, upgradeToPick.Count);

        string weaponName = upgradeToPick[index].weaponData.Name;

        GameManager.instance.eggPanelManager.EggPanelUP();
        GameManager.instance.eggPanelManager.SetWeaponName(weaponName);
        //GameManager.instance.eggPanelManager.SetEquipmentSprites(upgradeToPick[index].weaponData);
    }

    // 알이나 우유 등은 일단 물리를 이용해서 충돌체크
    // 추후에 화면에 보이는 프랍들만 따로 관리해서 물리 없이 하자
    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            UIEvent eggEvent = new UIEvent(() => OnPickUp(character), "Egg"); 

            GameManager.instance.popupManager.EnqueueUIEvent(eggEvent);

            SoundManager.instance.Play(pickup);
            gameObject.SetActive(false);
        }
    }
}
