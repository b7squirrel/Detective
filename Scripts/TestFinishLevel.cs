using UnityEngine;

public class TestFinishLevel : MonoBehaviour
{
    [SerializeField] WinStage winStage;
    int index = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (index != 0)
            return;
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            index++;
            winStage.OpenPanel();
        }
    }
}