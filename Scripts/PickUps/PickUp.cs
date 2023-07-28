using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어와 닿았을 때 OnPickup()호출
public class PickUp : MonoBehaviour
{
    [SerializeField] AudioClip pickup;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            GetComponent<IPickUpObject>().OnPickUp(character);

            
            SoundManager.instance.Play(pickup);
            gameObject.SetActive(false);
        }
    }
}
