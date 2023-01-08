using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일정 시간 후에 비활성화
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
