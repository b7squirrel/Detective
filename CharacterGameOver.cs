using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    [SerializeField] GameObject weaponsGroup;

    public void GameOver()
    {
        Debug.Log("Game Over");
        gameOverPanel.SetActive(true);
        weaponsGroup.SetActive(false);
    }
}
