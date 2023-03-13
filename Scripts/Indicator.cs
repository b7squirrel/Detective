using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    RaycastHit2D hit;
    [SerializeField] LayerMask ScreenCollision;
    [SerializeField] GameObject indicatorPrefab;
    [SerializeField] AudioClip onSpotSFX;
    Animator anim;
    GameObject indicator;
    float angle;
    bool isOnSpot; // 도착한 상태. 코루틴이 한 번만 실행되도록 하기 위한 플래그
    Coroutine disableIndicator;

    void OnEnable()
    {
        isOnSpot = false;
        if(disableIndicator != null)
        {
            StopCoroutine(disableIndicator);
        }
    }
    void Update()
    {
        if (indicator == null)
        {
            indicator = Instantiate(indicatorPrefab, transform);
            Transform indicatorObject = transform.GetChild(1);
            anim = indicatorObject.GetComponentInChildren<Animator>();
            indicator.SetActive(false);
        }

        hit = Physics2D.Linecast(transform.position, Player.instance.transform.position, ScreenCollision);
        if (hit)
        {
            // Debug.DrawLine(Player.instance.transform.position, hit.point, Color.blue);
            if (indicator.activeSelf == false)
            {
                indicator.SetActive(true);
                anim.SetTrigger("onSpot");
            }

            indicator.transform.position = hit.point;
            Vector2 dir = -((Vector2)Player.instance.transform.position - hit.point);
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicator.transform.eulerAngles = new Vector3(0, 0, angle);

            float distance = Vector2.Distance(transform.position, indicator.transform.position);
            if(distance < .1f)
            {
                isOnSpot = true;
            }
        }
        else
        {
            if (isOnSpot == true)
                disableIndicator = StartCoroutine(DisableIndicator());
                isOnSpot = false;
        }
    }

    IEnumerator DisableIndicator()
    {
        anim.SetTrigger("onSpot");
        yield return new WaitForSeconds(.25f); // indicator on spot 애니메이션의 길이만큼
        Debug.Log("Test");
        indicator.SetActive(false);
        SoundManager.instance.Play(onSpotSFX);
    }
}
