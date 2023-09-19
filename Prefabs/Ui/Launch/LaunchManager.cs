using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 대장 오리는 playerPref에 저장하자
public class LaunchManager : MonoBehaviour
{
    [SerializeField] CardSlot leadOriSlot;
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] SetCardDataOnSlot setCardDataOnSlot;
    string firstOriID;

    void OnEnable()
    {
        LoadLeadOri();
    }

    public void LoadLeadOri()
    {
        StartCoroutine(loadCo());
    }
    IEnumerator loadCo()
    {
        yield return new WaitForSeconds(.01f);
        CardData lead = cardDataManager.GetMyCardList().Find(x => x.startingMember == "1");
        setCardDataOnSlot.PutCardDataIntoSlot(lead, leadOriSlot);
    }
}
