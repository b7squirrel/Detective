using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    List<string> up = new List<string>();
    Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();
    void Start()
    {
        up.Add("fire1");
        up.Add("fire2");
        up.Add("fire3");
        dic.Add(dic.Count, up);

        up.Add("armor1");
        up.Add("armor2");
        up.Add("armor3");
        dic.Add(dic.Count, up);

        up.Add("tennis1");
        up.Add("tennis2");
        up.Add("tennis3");
        dic.Add(dic.Count, up);

        int r = Random.Range(0, dic.Count);
        

    }
}
