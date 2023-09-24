using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    OriAttribute leadAttr = new OriAttribute(0, 0);
    public void StartGamePlay(string stageToPlay)
    {
        SceneManager.LoadScene("Essential", LoadSceneMode.Single);
        SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
    }
    public void SetLeadAttr(OriAttribute _leadAttr)
    {
        this.leadAttr = _leadAttr;
    }
    public void Debugging()
    {
        Debug.Log("HP = " + leadAttr.Hp + " Atk = " + leadAttr.Atk);
    }
}
