using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 습득 가능한 오브젝트의 행동들 정의
public class Collectable : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float knockBackForce = 20f;
    public bool IsFlyingToPlayer { get; private set; }
    [field : SerializeField] public bool IsGem{get; private set;}

    public GameObject pickupEffect;

    protected Vector2 target;
    protected bool isKnockBack;
    public bool IsHit { get; private set; }

    public Vector2 dir;
    public float delay = 0.05f;

    protected float timeToDisapear;

    [Header("Effect")]
    [SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    [SerializeField] public AudioClip pickup;
    Material initialMat;
    SpriteRenderer sr;
    GemManager gemManager;

    float accel = 50f;

    public void TempWhite()
    {
        sr.material = whiteMaterial;
    }
    #region 유니티 콜백함수
    protected virtual void OnEnable()
    {
        IsFlyingToPlayer = false;
        IsHit = false;
        timeToDisapear = 60f;
        if (gemManager == null)
        {
            gemManager = FindAnyObjectByType<GemManager>();
        }
    }
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        sr.material = initialMat;

        if (IsGem) gemManager.RemoveVisibleGemFromList(transform);
    }
    protected void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        initialMat = sr.material;
    }
    protected void Update()
    {
        if (sr.isVisible && IsGem)
        {
            gemManager.AddVisibleGemToList(transform);
        }
        else if (sr.isVisible == false && IsGem)
        {
            gemManager.RemoveVisibleGemFromList(transform);
        }

        TimeUP();
    }
    #endregion

    #region 자력에 닿았을 때
    public virtual void OnHitMagnetField(Vector2 direction)
    {
        StartCoroutine(Antic(direction));
        StartCoroutine(Blink()); // 반짝거리면서 antic 모션
    }

    IEnumerator Antic(Vector2 dir)
    {
        IsHit = true;
        GameObject effect = GameManager.instance.poolManager.GetMisc(pickupEffect);
        effect.transform.position = transform.position;

        float anticTime = .3f;
        float currentSpeed = 15;
        while (true)
        {
            currentSpeed -= accel * Time.deltaTime;
            transform.position += new Vector3(currentSpeed * dir.x, currentSpeed * dir.y, 0) * Time.deltaTime;
            anticTime -= Time.deltaTime;
            if (anticTime < 0) break;
            yield return null;
        }

        yield return StartCoroutine(FlyToPlayer()); // antic이 끝나면 플레이어에게 날아가기 시작
    }
    IEnumerator Blink()
    {
        yield return new WaitForSeconds(.08f);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.08f);
        sr.material = initialMat;
    }

    IEnumerator FlyToPlayer()
    {
        IsFlyingToPlayer = true;

        while (true)
        {
            transform.position =
            Vector2.MoveTowards(transform.position,
            GameManager.instance.player.transform.position,
            moveSpeed * Time.deltaTime);

            if ((transform.position - Player.instance.transform.position).sqrMagnitude <= .04f)
            {
                PickedUp();
                break;
            }
            yield return null;
        }
    }
    #endregion

    #region 목표물에 도달했을 때
    public void PickedUp()
    {
        StartCoroutine(PickUpCo());
    }

    IEnumerator PickUpCo()
    {
        while (true)
        {
            if ((transform.position - Player.instance.transform.position).sqrMagnitude < .04f)
            {
                HitPlayerFeedback();
                break;
            }
            yield return null;
        }
    }

    void HitPlayerFeedback()
    {
        Character character = Player.instance.GetComponent<Character>();

        GetComponent<IPickUpObject>().OnPickUp(character);

        SoundManager.instance.Play(pickup);
        gameObject.SetActive(false);
    }
    #endregion

    #region 일정 시간 이후 사라짐
    protected virtual void TimeUP()
    {
        if (IsGem == false) // 보석만 사라지도록
            return;
        if (IsHit) // 자력에 닿아서 움직이는 도중이라면 사라짐 취소
            return;
        if (timeToDisapear > 0)
        {
            timeToDisapear -= Time.deltaTime;
        }
        else
        {
            StartCoroutine(TimesUpBlinking());
        }
    }

    IEnumerator TimesUpBlinking()
    {
        ShadowSprite shadow = GetComponentInChildren<ShadowSprite>();

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        shadow.Show();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        shadow.Show();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        shadow.Hide();
        yield return new WaitForSeconds(.1f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        shadow.Show();
        if (!IsHit)
            gameObject.SetActive(false);
    }
    #endregion
}
