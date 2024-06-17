using UnityEngine;

public class Wall : MonoBehaviour
{
    int wallDamage = 2;
    PlayerEffects playerEffects;
    Character character;
    public void Init(int _damage)
    {
        wallDamage = _damage;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {

        if (collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            if(playerEffects == null) playerEffects = collision.gameObject.GetComponent<PlayerEffects>();
            playerEffects.PlayWallDust();

            if(character == null) character = collision.gameObject.GetComponent<Character>();
            character.TakeDamage(wallDamage, EnemyType.Melee);
        }
    }
}