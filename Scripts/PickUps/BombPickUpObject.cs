using UnityEngine;

public class BombPickUpObject : Collectable, IPickUpObject
{
    FieldItemEffect fieldItemEffect;
    public void OnPickUp(Character character)
    {
        if (fieldItemEffect == null) fieldItemEffect = FindObjectOfType<FieldItemEffect>();
        fieldItemEffect.Explode(transform.position);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            OnPickUp(character);
            //SoundManager.instance.Play(pickup);
            SoundManager.instance.PlayAtPosition(pickup, transform.position); // 수정된 부분
            CameraShake.instance.Shake();
            
            gameObject.SetActive(false);
        }
    }
}