using UnityEngine;

public class MagnetPickUPObject : Collectable, IPickUpObject
{
    [SerializeField] GameObject magnetEffect;

    public void OnPickUp(Character character)
    {
        GameObject effect = GameManager.instance.poolManager.GetMisc(magnetEffect);
        if (effect != null) effect.transform.position = transform.position;

        character.GetComponentInChildren<Magnetic>().MagneticField(60f);
        // ⭐ 추가: 자석 메시지
        MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.magnet, MessageSystem.instance.GetBuffColor(FieldMessageType.Magnet));
    }

    public override void OnHitMagnetField(Vector2 direction)
    {
        // 자력에 영향을 받지 않는다
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