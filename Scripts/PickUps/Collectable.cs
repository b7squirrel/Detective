using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float knockBackForce = 12f;
    public bool IsFlying { get; private set; }

    public GameObject pickupEffect;

    protected Vector2 target;
    protected bool isKnockBack;
    public bool IsHit { get; private set; }
    Rigidbody2D rb;

    public Vector2 dir;
    public float delay = 0.05f;

    protected float timeToDisapear;

    [Header("Effect")]
    [SerializeField] Material whiteMaterial;
    [SerializeField] float whiteFlashDuration;
    Material initialMat;
    SpriteRenderer sr;

    [SerializeField] float acc;

    protected virtual void OnEnable()
    {
        IsFlying = false;
        IsHit = false;
        timeToDisapear = 30f;
    }
    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        initialMat = sr.material;
    }
    protected void Update()
    {
        MoveToPlayer();
        TimeUP();
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
        StartCoroutine(Reset());
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
            StartCoroutine(Blink());
        }
    }
    IEnumerator Blink()
    {
        ShadowSprite shadow = GetComponent<ShadowSprite>();

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
}
