using UnityEngine;

public class PlayerRegion : MonoBehaviour
{
    Rect mainArea;
    Transform player;

    public Region GetPlayerRegion()
    {
        if (player == null) player = GameManager.instance.player.transform;
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);

        // �־��� ���� ���� �÷��̾ ���ٸ� 
        if (!mainArea.Contains(playerPos))
        {
            Debug.Log("�÷��̾ �־��� ���� �ۿ� �ֽ��ϴ�.");
            return Region.None;
        }

        // �־��� ������ �߽���
        Vector2 center = new Vector2(mainArea.xMin + mainArea.width /2, mainArea.yMin + mainArea.height /2);

        // �÷��̾��� ��ġ�� ��� �������� �Ǻ�
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