using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.Rendering;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;
    public PoolManager EffectPoolManager { get; private set; }

    private void Awake()
    {
        instance= this;
        EffectPoolManager = GetComponent<PoolManager>();
    }

    public void GenerateEffect(int effectIndex, Transform transform)
    {
        GameObject go = EffectPoolManager.Get(effectIndex);
        go.transform.position= transform.position;
    }
}
