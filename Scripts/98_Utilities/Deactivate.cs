using System.Collections;
using UnityEngine;

public class Deactivate : MonoBehaviour
{
    [SerializeField] float lifeTime;

    void OnEnable()
    {
        StartCoroutine(AutoDisable());
    }

    IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}
