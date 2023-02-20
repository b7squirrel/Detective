using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EggPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] List<UpgradeData> upgradeToPick;
    int index;

    protected override void MoveToPlayer()
    {
        // 알은 자력으로 끌려오지 않는다. 끌려와 버리면 알을 얻었다는 것이 읽히지가 않음
    }

    public void OnPickUp(Character character)
    {
        // 플레이어에게 무기를 설치한 후 Egg 이벤트 화면으로 들어간다
        index = Random.Range(0, upgradeToPick.Count);
        character.GetComponent<Level>().GetWeapon(upgradeToPick[index]);
        
        GameManager.instance.eggPanelManager.EggPanelUP(upgradeToPick[index].newKidAnim);
    }
}
