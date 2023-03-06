using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    [SerializeField] GameObject weaponsGroup;

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        weaponsGroup.SetActive(false);

        // GameManager.instance.joystick.SetActive(false);
    }
}
