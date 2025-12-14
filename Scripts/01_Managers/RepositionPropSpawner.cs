using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionPropSpawner : MonoBehaviour
{
    Collider2D col;
    PropManager propManager;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        propManager = GetComponentInParent<PropManager>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("AreaProp"))
            return;

        Vector3 playerPos = Player.instance.transform.position;
        Vector3 myPos = transform.position;
        float diffx = Mathf.Abs(playerPos.x - myPos.x);
        float diffy = Mathf.Abs(playerPos.y - myPos.y);

        Vector3 playerDir = Player.instance.InputVec;
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        if (diffx > diffy)
        {
            transform.Translate(Vector3.right * dirX * 160);
        }
        else if (diffx < diffy)
        {
            transform.Translate(Vector3.up * dirY * 160);
        }

        propManager.SpawnProps(transform.position);
    }
}
