using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform shootPoint;
    public Transform effectPoint;
    [SerializeField] SpriteRenderer weaponSprite;
    [field : SerializeField] public bool IsDirectional{get; private set;}

    public SpriteRenderer GetWeaponSprite()
    {
        if (weaponSprite == null) return null;

        return weaponSprite;
    }
}
