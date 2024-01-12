using UnityEngine;
public interface Idamageable
{
    public void TakeDamage(int damage, float knockBackChance, float knockBackSpeed, Vector2 target, GameObject hitEffect);
}
