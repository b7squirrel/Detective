using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �ð� �Ŀ� ��Ȱ��ȭ
/// </summary>
public class DamageMessage : MonoBehaviour
{
    [SerializeField] float lifeOfMessage;
    float lifeTimer;

    void OnEnable()
    {
        lifeTimer = lifeOfMessage;
    }
    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer < 0)
        {
            gameObject.SetActive(false);
        }
    }
}
