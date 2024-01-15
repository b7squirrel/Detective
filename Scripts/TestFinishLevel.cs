using UnityEngine;

public class TestFinishLevel : MonoBehaviour
{
    int index = 0; // 여러번 충돌을 방지

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (index != 0)
            return;
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            index++;
            GameManager.instance.GetComponent<WinStage>().OpenPanel();
        }
    }
}