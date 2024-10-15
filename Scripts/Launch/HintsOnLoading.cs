using System;
using UnityEngine;

[Serializable]
public class Hint
{
    [TextArea]
    public string hint;
}

public class HintsOnLoading : MonoBehaviour
{
    public Hint[] hints;
    public int hintIndex;
    bool initDone; 

    public void Init()
    {
        if(initDone == false)
        {
            hintIndex = UnityEngine.Random.Range(0, hints.Length);
            initDone = true;
        }
    }
    public string GetHint()
    {
        return hints[hintIndex].hint;
    }

    public void ResetHint()
    {
        initDone = false;
    }


}