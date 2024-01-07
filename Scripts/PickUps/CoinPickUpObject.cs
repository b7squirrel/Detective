using UnityEngine;

public class CoinPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] int coinAmount;
    public void OnPickUp(Character character)
    {
        character.coin.Add(coinAmount);
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
