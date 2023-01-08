using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallProjectile : MonoBehaviour
{
    Vector3 direction;
    [SerializeField] float speed;
    [SerializeField] int damage = 5;
    bool hitDetected = false;

    public void SetDireciotn(float dir_x, float dir_y)
    {
        direction = new Vector2(dir_x, dir_y);

        if (dir_x < 0)
        {
            Vector2 scale = transform.localScale;
            scale.x = scale.x * -1;
            transform.localScale = scale;
        }
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Time.frameCount % 6 == 0)
        {
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
            foreach (var item in hit)
            {
                Enemy enmey = item.GetComponent<Enemy>();
                if (enmey != null)
                {
                    enmey.TakeDamage(damage);
                    hitDetected = true;
                    break;
                }
            }
            if (hitDetected == true)
            {
                Destroy(gameObject);
            }
        }
    }
}
