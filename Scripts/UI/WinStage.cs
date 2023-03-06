using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    public void OpenPanel()
    {
        winStage.gameObject.SetActive(true);
    }
}
