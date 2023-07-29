using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 현재 화면 안에 있는 Gem의 갯수를 세고 
// 최대치라면 더 이상 젬을 pool에서 빼오지 않고 경험치만 기존 보석에 합쳐주기

// 전체 보석은 pooling에 접근해서 얻어내고 그 중 화면에 보이는 것만 따로 관리
// gem이 pool에서 새로 만들어질때마다 gems 리스트 업데이트
public class GemManager : MonoBehaviour
{
    [SerializeField] int MaxGemNumbers;
    public int GemNumbers {get; private set; }
    [SerializeField] List<Transform> gems;
    [SerializeField] List<Transform> gemsVisible;
    string gemPoolingKey;

    void Awake()
    {
        gemsVisible = new List<Transform>();
    }

    public bool IsMaxGemNumber()
    {
        if (GemNumbers >= MaxGemNumbers)
            return true;
        return false;
    }

    public void OnPoolingGem(Transform gemCreated)
    {
        gems.Add(gemCreated);
        // GetVisibleGems();
    }

    public void AddVisibleGemToList(Transform gemVisible)
    {
        bool result = gemsVisible.Exists(x => x == gemVisible);
        if (result)
            return;
        gemsVisible.Add(gemVisible);
    }

    public void RemoveVisibleGemFromList(Transform gemVisible)
    {
        gemsVisible.Remove(gemVisible);
    }

    void GetVisibleGems()
    {
        if (gems == null)
            gems = new List<Transform>();

        gemsVisible.Clear();

        for (int i = 0; i < gems.Count; i++)
        {
            if (gems[i].gameObject.activeSelf == false)
                continue;

            if (gems[i].GetComponentInChildren<SpriteRenderer>().isVisible)
                gemsVisible.Add(gems[i]);

                Debug.Log("GetVisibleGems" + gems[i].GetComponentInChildren<SpriteRenderer>().isVisible);
        }
    }
    public bool IsVisibleGemMax()
    {
        if (gemsVisible.Count >= MaxGemNumbers)
            return true;
        return false;
    }

    public void MergeExp(int exp)
    {
        int index = UnityEngine.Random.Range(0, gemsVisible.Count);
        gemsVisible[index].GetComponent<GemPickUpObject>().ExpAmount += exp;
    }
}
