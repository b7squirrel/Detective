using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] List<WeaponData> weaponsToPick;

    [Header("디버그")]
    [SerializeField] bool isDebugging;
    [SerializeField] int indexWeapon;

    public override void OnHitMagnetField(Vector2 direction)
    {
        // 알은 자력에 영향을 받지 않는다
    }

    public void OnPickUp(Character character)
    {
        // ⭐ 추가: 동료가 이미 최대 인원(로비 스쿼드 + 필드 획득 합산)이면 알을 써도 아무것도 주지 않음
        WeaponManager weaponManager = character.GetComponent<WeaponManager>();
        if (weaponManager != null && weaponManager.GetCompanionCount() >= weaponManager.MaxCompanions)
        {
            Logger.LogWarning($"[EggPickUpObject] 동료가 이미 최대 인원({weaponManager.MaxCompanions})입니다. 알 보상을 지급하지 않습니다.");
            return;
        }

        // 플레이어가 이미 가진 무기 제거
        List<WeaponData> checks = new List<WeaponData>(weaponsToPick);
        for (int i = 0; i < checks.Count; i++)
        {
            if (character.GetComponent<Level>().HavingWeapon(checks[i]))
            {
                weaponsToPick.Remove(checks[i]);
            }
        }

        if (weaponsToPick.Count == 0)
        {
            Logger.LogWarning("[EggPickUpObject] 줄 수 있는 무기가 없습니다.");
            return;
        }

        int index;
        if (isDebugging)
        {
            index = Mathf.Clamp(indexWeapon, 0, weaponsToPick.Count - 1);
            Debug.Log($"[디버그 모드] 선택된 무기 인덱스: {index}");
        }
        else
        {
            index = Random.Range(0, weaponsToPick.Count);
        }

        string weaponName = weaponsToPick[index].Name;
        Debug.Log($"[EggPickUpObject] 선택된 무기: '{weaponName}', index: {index}");
        GameManager.instance.eggPanelManager.EggPanelUP();
        GameManager.instance.eggPanelManager.SetWeaponName(weaponName);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            UIEvent eggEvent = new UIEvent(() => OnPickUp(character), "Egg", GameManager.instance.eggPanelManager.ForceClose); // ⭐ ForceClose 연결
            GameManager.instance.popupManager.EnqueueUIEvent(eggEvent);
            SoundManager.instance.Play(pickup);
            StartCoroutine(DeactivateNextFrame());
        }
    }

    private IEnumerator DeactivateNextFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}