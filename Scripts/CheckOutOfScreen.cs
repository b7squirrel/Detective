using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOutOfScreen : MonoBehaviour
{
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("VisibleArea"))
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("VisibleArea"))
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
