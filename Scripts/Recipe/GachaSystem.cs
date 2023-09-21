using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager;

    [SerializeField] TextAsset gachaPoolDataBase;
    List<CardData> gachaPools;

    void Start()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
    }

    // shop 패널의 뽑기 버튼에서 호출
    public void Draw()
    {
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }

        // 임시로 3개씩 카드가 뽑힘
        for (int i = 0; i < 1; i++)
        {
            int pickIndex = UnityEngine.Random.Range(0, gachaPools.Count);
            string mType = gachaPools[pickIndex].Type;
            string mGrade = gachaPools[pickIndex].Grade;
            string mName = gachaPools[pickIndex].Name;

            Debug.Log(gachaPools[pickIndex].Name + gachaPools[pickIndex].ID + " 을 뽑았습니다.");
            cardDataManager.AddNewCardToMyCardsList(gachaPools[pickIndex]);

            gachaPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        }
    }
}
