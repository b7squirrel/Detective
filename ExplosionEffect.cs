using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    ParticleSystem ps;
    public void Init(float _radius, Vector2 _pos)
    {
        if (ps == null) ps = GetComponentInChildren<ParticleSystem>();

        var shape = ps.shape;
        shape.radius = _radius;
        transform.position = _pos;
        transform.localScale = _radius * Vector2.one;
        ps.Play();
    }
}
