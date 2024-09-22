using UnityEngine;

public class EnemyDieEffects : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] decalColors;
    [SerializeField] float duration;
    public void SetColor(Color _decal, Color _highLight)
    {
        for (int i = 0; i < decalColors.Length; i++)
        {
            decalColors[i].color = _decal;
        }
    }
}
