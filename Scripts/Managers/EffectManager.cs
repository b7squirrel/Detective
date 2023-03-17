using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;
    public PoolManager EffectPoolManager { get; private set; }

    private void Awake()
    {
        instance = this;
        EffectPoolManager = GetComponent<PoolManager>();
    }

    public void GenerateEffect(int effectIndex, Transform transform)
    {
        GameObject go = EffectPoolManager.Get(effectIndex);
        go.transform.position = transform.position;
        float angle = Random.Range(0, 359f);
        go.transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
