using TMPro;
using UnityEngine;

public class TestFinishLevel : MonoBehaviour
{
    [SerializeField] bool isGameOver;
    int index = 0; // 한 번만 충돌 처리하도록
    TextMeshPro endTypeText; // win인지 game over인지

    void OnEnable()
    {
        endTypeText = GetComponentInChildren<TextMeshPro>();
        if (isGameOver)
        {
            endTypeText.text = "GAME OVER";
        }
        else
        {
            endTypeText.text = "WIN";
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (index != 0)
            return;
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            index++;
            if (isGameOver)
            {
                GameManager.instance.characterGameOver.GameOver();
            }
            else
            {
                GameManager.instance.GetComponent<WinStage>().OpenPanel();
            }
        }
    }
}