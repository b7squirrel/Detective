using System.Collections.Generic;
using UnityEngine;

public class GachaPanelManager : MonoBehaviour
{
    [SerializeField] GachaField gachaField;
    MainMenuManager mainMenuManager;

    public void InitGachaPanel(List<CardData> cards)
    {
        if(mainMenuManager == null) mainMenuManager = FindObjectOfType<MainMenuManager>();

        gachaField.GenerateAllCardsOfType(cards);
        // 바로 숨기고 회전하면서 탕탕탕탕 나오는 연출
        // 연출이 끝나면 탭해서 계속하기 활성화
        ActivateButtonTapToCon(true);
    }

    public void ActivateButtonTapToCon(bool activate)
    {
        gameObject.SetActive(activate);
    }
}
