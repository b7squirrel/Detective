using UnityEngine;

/// <summary>
/// ������ ���߰� �ϰ�, ���� �̺�Ʈ�� �Ͻ� �����ؾ� ��. 
/// �÷��̾ ������ ������� �ϹǷ� Field Item Effect�� �ൿ�� �ñ�
/// </summary>
public class ClockPickUpObject : Collectable, IPickUpObject
{
    FieldItemEffect fieldItemEffect;
    public void OnPickUp(Character character)
    {
        if(fieldItemEffect == null) fieldItemEffect = FindObjectOfType<FieldItemEffect>();
        fieldItemEffect.StopEnemies();
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