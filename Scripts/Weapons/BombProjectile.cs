using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    Vector2 targetPos;
    [SerializeField] float moveSpeed;

    void Update()
    {
        transform.position =
            Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        float distance = Vector2.Distance(transform.position, targetPos);
        if (distance < .1f)
        {
            Debug.Log("Booom!!");
        }
    }
    public void SetTargetDirection(Vector2 target)
    {
        targetPos = target;
    }
}
