using UnityEngine;

public class TriggerHeightShadow : MonoBehaviour
{
    ShadowHeight shadowHeight;
    [SerializeField] float verticalVel;
    [SerializeField] Vector2 groundVel;

    [SerializeField] bool randomGroundVel;
    void OnEnable()
    {
        if (randomGroundVel)
        {
            groundVel = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
            float speedfactoer = Random.Range(3, 5);

            groundVel *= speedfactoer;
        }
        shadowHeight = GetComponent<ShadowHeight>();
        shadowHeight.Initialize(groundVel, verticalVel);
    }
}