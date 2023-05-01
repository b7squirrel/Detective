using UnityEngine;
public interface Idamageable
{
    public void TakeDamage(int damage, float knockBackChance, Vector2 target, GameObject hitEffect);
}
