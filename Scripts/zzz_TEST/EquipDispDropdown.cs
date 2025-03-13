using System.Collections.Generic;
using UnityEngine;

public class EquipDispDropdown : MonoBehaviour
{
    //Dropdown 제어를 위한 TMP_Dropdown타입 변수와 
	//Dropdown의 옵션 데이터로 활용되는 문자열 배열 변수 arrayClass를 선언합니다.
    
    [SerializeField] TMPro.TMP_Dropdown dropdownWeapon;
    [SerializeField] TMPro.TMP_Dropdown[] dropdownItems;
    [SerializeField] TMPro.TMP_Dropdown dropdownAnims;

    EquipDisplayTest equipDisplayTest;

    void Start()
    {
        dropdownWeapon.ClearOptions();
        foreach (var item in dropdownItems)
        {
            item.ClearOptions();
        }
        dropdownAnims.ClearOptions();

        if(equipDisplayTest == null) equipDisplayTest = GetComponent<EquipDisplayTest>();
        List<string> weaponNames = equipDisplayTest.GetWeapon();
        List<string>[] itemNames = new List<string>[4];
        for (int i = 0; i < 4; i++)
        {
            itemNames[i] = equipDisplayTest.GetItems(i); // 인덱스 부위의 리스트에 항목 추가
        }
        List<string> anims = new();
        anims.Add("Idle");
        anims.Add("Walk");

        dropdownWeapon.AddOptions(weaponNames);
        for (int i = 0; i < 4; i++)
        {
            dropdownItems[i].AddOptions(itemNames[i]);
        }
        
        dropdownAnims.AddOptions(anims);
    }

    public void OnWeaponDropdownEvent()
    {
        string weapon = dropdownWeapon.options[dropdownWeapon.value].text;
        equipDisplayTest.SetWeapon(weapon);
    }
    public void OnItemDropdownEvent(int index)
    {
        string item = dropdownWeapon.options[dropdownItems[index].value].text;
        equipDisplayTest.SetItem(index, item);
    }
    public void OnAnimDropdownEvent()
    {
        string anim = dropdownWeapon.options[dropdownAnims.value].text;
        equipDisplayTest.SetAnim(anim);
    }
}
