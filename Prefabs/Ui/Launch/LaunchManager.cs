using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

// 대장 오리는 playerPref에 저장하자
public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    [SerializeField] CardDataManager cardDataManager;
    string firstOriID;

    void Start()
    {
        LoadLeadOri();
    }
    public void SaveLeadOri()
    {
        PlayerPrefs.SetString("1st", firstOriID);
    }
    public void LoadLeadOri()
    {
        if (PlayerPrefs.HasKey("1st"))
        {
            firstOriID = PlayerPrefs.GetString("1st");
        }
        else
        {
            firstOriID = cardDataManager.GetStartingCardData().ID;
        }

        SaveLeadOri();
    }
}
