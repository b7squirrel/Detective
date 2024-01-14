using UnityEngine;

public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;
    Vector2 groundVel;

    [SerializeField] bool randomGroundVel;
    void OnEnable()
    {
        if (randomGroundVel)
        {
            float x = Random.Range(-1.1f, 1.1f);
            if (x > 0)
            {
                x += 1f;
            }
            else
            {
                x -= 1f;
            }
            float y = Random.Range(-1.1f, 1.1f);
            if (y > 0)
            {
                y += 1f;
            }
            else
            {
                y -= 1f;
            }
            groundVel = new Vector2(x, y).normalized;

            groundVel *= 3f;
            //groundVel = groundVel.normalized;
        }
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(groundVel, verticalVel);
        Debug.Log("Ground Velocity = " + groundVel);
    }
}