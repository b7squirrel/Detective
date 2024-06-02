using UnityEngine;

public class PlayerRegion : MonoBehaviour
{
    Rect mainArea;
    Transform player;

    public Region GetPlayerRegion()
    {
        if (player == null) player = GameManager.instance.player.transform;
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);

        // 주어진 구역 내에 플레이어가 없다면 
        if (!mainArea.Contains(playerPos))
        {
            Debug.Log("플레이어가 주어진 구역 밖에 있습니다.");
            return Region.None;
        }

        // 주어진 구간의 중심점
        Vector2 center = new Vector2(mainArea.xMin + mainArea.width /2, mainArea.yMin + mainArea.height /2);

        // 플레이어의 위치가 어느 구간인지 판별
        if (playerPos.x < center.x && playerPos.y < center.y)
        {
            return Region.BottomLeft;
        }
        else if (playerPos.x >= center.x && playerPos.y < center.y)
        {
            return Region.BottomRight;
        }
        else if (playerPos.x < center.x && playerPos.y >= center.y)
        {
            return Region.TopLeft;
        }
        else
        {
            return Region.TopRight;
        }
    }
}