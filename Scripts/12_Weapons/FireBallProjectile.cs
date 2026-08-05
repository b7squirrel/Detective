using UnityEngine;

public class FireBallProjectile : ProjectileBase
{
    TrailRenderer trailRenderer;

    protected override void Awake()
    {
        base.Awake();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MainCamera") || other.gameObject.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
    }
}
