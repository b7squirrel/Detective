using UnityEngine;
// 플레이어와 오브젝트 사이에 벽이 있는지 탐색
// 벽이 있으면
public class Indicator : MonoBehaviour
{
    RaycastHit2D hit;
    [SerializeField] LayerMask ScreenCollision;
    [SerializeField] GameObject indicatorPrefab;
    [SerializeField] Sprite iconSprite;
    [SerializeField] AudioClip onSpotSFX;
    Animator anim;
    SpriteRenderer sr;
    GameObject indicator;
    float angle;
    bool isVisible; // 도착한 상태. 코루틴이 한 번만 실행되도록 하기 위한 플래그
    bool isInitiated;
    void Init()
    {
        if (isInitiated)
            return;
        sr = GetComponent<SpriteRenderer>();
        if (indicator == null)
        {
            indicator = Instantiate(indicatorPrefab, transform);
            anim = indicator.GetComponentInChildren<Animator>();
            indicator.GetComponent<BubbleIndicator>().SetIconImage(iconSprite);
            indicator.SetActive(false);
        }
        isInitiated = true;
    }
    void OnEnable()
    {
        Init();
        isVisible = false;
        anim.SetTrigger("Init");
    }
    void Update()
    {
        hit = Physics2D.Linecast(transform.position, Player.instance.transform.position, ScreenCollision);
        if (hit.collider != null) // 화면 바깥이라면
        {
            if (indicator.activeSelf == false)
            {
                indicator.SetActive(true);
                anim.SetTrigger("onSpot");
                SoundManager.instance.PlaySoundWith(onSpotSFX, 1f, false, .2f);
            }
            indicator.transform.position = hit.point;
            Vector2 dir = -((Vector2)Player.instance.transform.position - hit.point);
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicator.transform.eulerAngles = new Vector3(0, 0, angle);
            float distance = Vector2.Distance(transform.position, indicator.transform.position);
            isVisible = false;
        }
        else // 화면 안쪽이라면
        {
            if (isVisible == false)
            {
                indicator.SetActive(false);
                SoundManager.instance.PlaySoundWith(onSpotSFX, 1f, false, .2f);
                isVisible = true;
            }
        }
    }
}