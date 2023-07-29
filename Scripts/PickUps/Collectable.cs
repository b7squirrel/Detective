using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 습득 가능한 오브젝트의 행동들 정의
public class Collectable : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float knockBackForce = 12f;
    public bool IsFlying { get; private set; }
    [field : SerializeField] public bool IsGem{get; private set;}

    public GameObject pickupEffect;

    protected Vector2 target;
    protected bool isKnockBack;
    public bool IsHit { get; private set; }
    Rigidbody2D rb;

    public Vector2 dir;
    public float delay = 0.05f;

    protected float timeToDisapear;
    Coroutine resetCoroutine;

    [Header("Effect")]
    [SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    Material initialMat;
    SpriteRenderer sr;
    GemManager gemManager;

    [SerializeField] float acc;

    protected virtual void OnEnable()
    {
        IsFlying = false;
        IsHit = false;
        timeToDisapear = 30f;
        if (gemManager == null)
        {
            gemManager = FindAnyObjectByType<GemManager>();
        }
    }
    protected virtual void OnDisable()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        sr.material = initialMat;

        if (IsGem) gemManager.RemoveVisibleGemFromList(transform);
    }
    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        initialMat = sr.material;
    }
    protected void Update()
    {
        rb.simulated = sr.isVisible;
        if (rb.simulated && IsGem)
        {
            gemManager.AddVisibleGemToList(transform);
        }
        MoveToPlayer();
        // TimeUP();
    }

    protected virtual void MoveToPlayer()
    {
        if (!IsFlying)
            return;

        transform.position =
            Vector2.MoveTowards(transform.position,
            GameManager.instance.player.transform.position,
            moveSpeed * Time.deltaTime);
        acc += acc * Time.deltaTime;
    }
    public virtual void OnHitMagnetField(Vector2 direction)
    {
        IsHit = true;
        GameObject effect = GameManager.instance.poolManager.GetMisc(pickupEffect);
        effect.transform.position = transform.position;


        rb.AddForce(direction * knockBackForce, ForceMode2D.Impulse);
        resetCoroutine = StartCoroutine(Reset());
    }

    public IEnumerator Reset()
    {
        yield return new WaitForSeconds(.08f);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.08f);

        sr.material = initialMat;

        yield return new WaitForSeconds(.02f);
        rb.velocity = Vector2.zero;
        IsFlying = true;
    }

    protected virtual void TimeUP()
    {
        if (IsHit)
            return;

        if(timeToDisapear > 0)
        {
            timeToDisapear -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
