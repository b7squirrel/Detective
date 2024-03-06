using System.Collections.Generic;
using UnityEngine;

public class EggPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] List<UpgradeData> upgradeToPick;
    int index;

    public override void OnHitMagnetField(Vector2 direction)
    {
        // 알은 자력에 영향을 받지 않는다
    }

    // 플레이어에게 무기를 설치한 후 Egg 이벤트 화면으로 들어간다
    public void OnPickUp(Character character)
    {
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
        character.GetComponent<Level>().GetWeapon(upgradeToPick[index]);

        GameManager.instance.eggPanelManager.EggPanelUP(upgradeToPick[index].newKidAnim, upgradeToPick[index].Name);
        GameManager.instance.eggPanelManager.SetEquipmentSprites(upgradeToPick[index].weaponData);
    }

    // 알이나 우유 등은 일단 물리를 이용해서 충돌체크
    // 추후에 화면에 보이는 프랍들만 따로 관리해서 물리 없이 하자
    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            OnPickUp(character);

            SoundManager.instance.Play(pickup);
            gameObject.SetActive(false);
        }
    }
}
