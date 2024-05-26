using UnityEngine;

public class Wall : MonoBehaviour
{
    int wallDamage = 2;
    public void Init(int _damage)
    {
        wallDamage = _damage;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("IN");

        if (collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ININ");
            collision.gameObject.GetComponent<Character>().TakeDamage(wallDamage);
        }
    }
}