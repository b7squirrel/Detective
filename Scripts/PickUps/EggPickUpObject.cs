using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] List<UpgradeData> upgradeToPick;

    [Header("디버그")]
    // 알에서 나오게 할 무기 선택. 0을 선택하면 매번 upgradeToPick의 첫 번쨰 항목을 가져오게 됨.
    [SerializeField] bool isDebugging; // 알에서 나올 무기를 선택하게 할 것인지
    [SerializeField] int indexWeapon; 

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
            return;
        }

        // 디버그 모드 체크
        int index;
        if (isDebugging)
        {
            // 디버그 모드: indexWeapon 사용 (범위 체크)
            index = Mathf.Clamp(indexWeapon, 0, upgradeToPick.Count - 1);
            Debug.Log($"[디버그 모드] 선택된 무기 인덱스: {index}");
        }
        else
        {
            // 일반 모드: 랜덤 선택
            index = Random.Range(0, upgradeToPick.Count);
        }

        string weaponName = upgradeToPick[index].weaponData.Name;
        // Logger.LogError($"[EggPickupObject] {weaponName}을 얻었습니다.");

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
            // // Collider도 즉시 비활성화
            // GetComponent<Collider2D>().enabled = false;

            UIEvent eggEvent = new UIEvent(() => OnPickUp(character), "Egg");
            GameManager.instance.popupManager.EnqueueUIEvent(eggEvent);
            SoundManager.instance.Play(pickup);

            // 다음 프레임에서 비활성화 (더 안전)
            StartCoroutine(DeactivateNextFrame());
        }
    }

    private IEnumerator DeactivateNextFrame()
    {
        yield return null; // 한 프레임 대기
        gameObject.SetActive(false);
    }
}
