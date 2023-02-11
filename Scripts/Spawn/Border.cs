using UnityEngine;

public class Border : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            other.GetComponent<SpawnPoint>().IsAvailable = false;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            other.GetComponent<SpawnPoint>().IsAvailable = false;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            other.GetComponent<SpawnPoint>().IsAvailable = true;
        }
    }
}
