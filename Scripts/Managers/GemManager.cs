using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gem의 갯수를 세고 
// 최대치인지 알려주는 역할
public class GemManager : MonoBehaviour
{
    [SerializeField] int MaxGemNumbers;
    public int GemNumbers {get; private set;}

    public bool IsMaxGemNumber()
    {
        if (GemNumbers >= MaxGemNumbers)
            return true;
        return false;
    }
}
