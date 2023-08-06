using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMessage : MonoBehaviour
{
    [SerializeField] float lifeOfMessage;
    float lifeTimer;
    Animator anim;

    void OnEnable()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
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

    public void PlayCriticalDamage()
    {
        anim.SetTrigger("IsCritical");
    }
}
