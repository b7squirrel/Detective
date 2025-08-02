using UnityEngine;

public class TestFinishLevel : MonoBehaviour
{
    int index = 0; // 한 번만 충돌 처리하도록

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