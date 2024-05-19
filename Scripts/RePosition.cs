using UnityEngine;

public class RePosition : MonoBehaviour
{
    //Collider2D col;
    
    //private void Awake()
    //{
    //    col = GetComponent<Collider2D>();
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (!collision.CompareTag("Area"))
    //        return;

    //    Vector3 playerPos = Player.instance.transform.position;
    //    Vector3 myPos = transform.position;

    //    // Vector3 playerDir = Player.instance.InputVec;

    //    switch (transform.tag)
    //    {
    //        case "Ground":
    //            float diffx = playerPos.x - myPos.x;
    //            float diffy = playerPos.y - myPos.y;

    //            float dirX = diffx < 0 ? -1 : 1;
    //            float dirY = diffy < 0 ? -1 : 1;
    //            diffx = Mathf.Abs(diffx);
    //            diffy = Mathf.Abs(diffy);

    //            if (diffx > diffy)
    //            {
    //                transform.Translate(Vector3.right * dirX * 120);
    //            }
    //            else if (diffx < diffy)
    //            {
    //                transform.Translate(Vector3.up * dirY * 120);
    //            }
    //            break;

    //        case "Enemy":
    //            if (col.enabled)
    //            {
    //                Enemy enemy = GetComponent<Enemy>();
    //                // Player의 Area 밖으로 보내버렸는데 플레이어가 방향을 바꿔서 가버리면 oncollisionExit이 생길 기회가 없어진다. 
    //                // 적이 계속 뒤쳐져서 방황하게 되므로 그냥 spawn 포인트로 이동시켜 버리자
    //                // sub Boss는 indicator가 있으니까 reposition 되지 않도록 하자. 그냥 reposition 스크립트를 붙이지 않으면 됨
    //                // transform.position += new Vector3(playerDir.x * 40f, playerDir.y * 50f, transform.position.z);
    //                transform.position = Spawner.instance.GetRandomSpawnPoint();
    //                if(enemy.IsGrouping == true)
    //                {
    //                    enemy.Deactivate();
    //                }
    //            }
    //            break;
    //    }
    //}
}
