using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemBehaviour : MonoBehaviour
{
    [SerializeField] float searchCycle;
    [SerializeField] float searchRadius;
    [SerializeField] LayerMask propLayer;
    [SerializeField] GameObject gem;
    int id;
    public bool IsTarget{get; set;}
    public bool IsFollower{get; private set;}
    List<GameObject> gems;
    GameObject targetGem;
    int numberOfMerging; // 머지를 한 횟수

    void OnEnable()
    {
        if (id == 0) Init();
        IsTarget = false;
        IsFollower = false;
    }
    void Init()
    {
        id = GetInstanceID();
        gems = new List<GameObject>();
    }
    
    void Update()
    {
        gems.Clear();

        SetMergeTarget();
    }

    void SetMergeTarget()
    {
        if (!IsFollower && IsTarget)
            return;

        Collider2D[] props = Physics2D.OverlapCircleAll(transform.position, searchRadius, propLayer);
        if (props == null)
            return;

        for (int i = 0; i < props.Length; i++)
        {
            if(props[i].GetComponent<GemPickUpObject>() != null)
            {
                GemBehaviour g = props[i].GetComponent<GemBehaviour>();
                if (g.IsFollower == false)
                    g.IsTarget = true;
                gems.Add(props[i].gameObject);
            }
        }

        if(gems.Count == 0) 
        {
            targetGem = null;
            return;
        }

        int index = UnityEngine.Random.Range(0, gems.Count);
        targetGem = gems[index];

        SetExp();
    }

    void SetExp()
    {
        if (targetGem.GetComponent<GemPickUpObject>().ExpAmount > GetComponent<GemPickUpObject>().ExpAmount)
        {
            targetGem.GetComponent<GemPickUpObject>().ExpAmount += GetComponent<GemPickUpObject>().ExpAmount;
            
        }
        else
        {
            GetComponent<GemPickUpObject>().ExpAmount += targetGem.GetComponent<GemPickUpObject>().ExpAmount;
        }
        GameManager.instance.poolManager.GetMisc(gem);
        targetGem.SetActive(false);
        gameObject.SetActive(false);
    }
}
