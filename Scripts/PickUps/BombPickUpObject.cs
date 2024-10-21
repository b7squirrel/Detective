using UnityEngine;

public class BombPickUpObject : Collectable, IPickUpObject
{
    public void OnPickUp(Character character)
    {
        GameManager.instance.fieldItemEffect.Explode(transform.position);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            OnPickUp(character);
            //SoundManager.instance.Play(pickup);
            SoundManager.instance.Play(pickup); 
            CameraShake.instance.Shake();
            
            gameObject.SetActive(false);
        }
    }
}