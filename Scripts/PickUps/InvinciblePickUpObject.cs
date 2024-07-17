using UnityEngine;

public class InvinciblePickUpObject : Collectable, IPickUpObject
{
    FieldItemEffect fieldItemEffect;
    public void OnPickUp(Character character)
    {
        if (fieldItemEffect == null) fieldItemEffect = FindObjectOfType<FieldItemEffect>();
        fieldItemEffect.SetPlayerInvincible();
    }
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