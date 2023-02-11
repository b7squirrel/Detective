using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallProjectile : MonoBehaviour
{
    public Vector3 Direction { get; set; }
    [SerializeField] float speed;
    bool hitDetected = false;
    public int Damage { get; set; } = 5;

    float timeToLive = 6f;

    void Update()
    {
        transform.position += speed * Time.deltaTime * Direction.normalized;

        if (Time.frameCount % 3 == 0)
        {
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
            foreach (var item in hit)
            {
                Enemy enmey = item.GetComponent<Enemy>();
                if (enmey != null)
                {
                    PostMessage(Damage, enmey.transform.position);
                    enmey.TakeDamage(Damage);
                    hitDetected = true;
                    break;
                }
            }
            if (hitDetected == true)
            {
                Destroy(gameObject);
            }
        }

        timeToLive -= Time.deltaTime;
        if (timeToLive < 0f)
        {
            Destroy(gameObject);
        }
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition);
    }
}
