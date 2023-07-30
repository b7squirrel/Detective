using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetPickUPObject : Collectable, IPickUpObject
{
    public void OnPickUp(Character character)
    {
        character.GetComponentInChildren<Magnetic>().MagneticField(60f);
    }
    
    public override void OnHitMagnetField(Vector2 direction)
    {
        // 자력에 영향을 받지 않는다
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
