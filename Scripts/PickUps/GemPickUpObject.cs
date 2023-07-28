using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어와 닿았을 때 PickUP 클래스에서 OnPickUp() 실행
public class GemPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int ExpAmount { get; set; }
    public void OnPickUp(Character character)
    {
        if(ExpAmount == 0) ExpAmount = 400;
        character.level.AddExperience(ExpAmount);
    }
}
