using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;
    public PoolManager EffectPoolManager { get; private set; }
    [SerializeField] GameObject tempEffect;

    private void Awake()
    {
        instance = this;
    }

    public void GenerateEffect(int effectIndex, Transform transform)
    {
        GameObject go = Instantiate(tempEffect, transform);
        go.transform.position = transform.position;
        float angle = Random.Range(0, 359f);
        go.transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
