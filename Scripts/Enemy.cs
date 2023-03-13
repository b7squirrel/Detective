using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    public int hp = 999;
    public float speed = 5;
    public int damage = 1;
    public int experience_reward = 0;

    public EnemyStats(EnemyStats stats)
    {
        this.hp = stats.hp;
        this.speed = stats.speed;
        this.damage = stats.damage;
        this.speed = stats.speed;
        this.experience_reward = stats.experience_reward;
    }
}

public class Enemy : EnemyBase
{
    public int ExperienceReward { get; private set; }
    bool isLive;

    public bool IsFlying { get; set; }
    public Vector2 LandingTarget { get; set; }
    [SerializeField] float flyingSpeed;

    [SerializeField] public float Timer { get; set; }// 플레이어의 Area바깥에서 머무는 시간을 체크
    [SerializeField] LayerMask playerLayer;
    [SerializeField] bool isDetectingPlayer;
    protected override void OnEnable()
    {
        base.OnEnable();
        isLive = true;
        SetWalking(); // 날으는 상태로 소환되지 않도록
        Timer = 8f;
        isDetectingPlayer = false;

    }
    void FixedUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;
        ApplyMovement();
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;
        if (GameManager.instance.player == null)
            return;
        Flip();

        if (Timer > 0)
        {
            isDetectingPlayer = false;

            Collider2D player = Physics2D.OverlapCircle(transform.position, 20f, playerLayer);
            if (player != null)
            {
                isDetectingPlayer = true;
                Timer = 8f;
                return;
            }

            // 플레이어가 주변에 감지되지 않으면서 아직 타이머가 0이 아닐 떄
            Timer -= Time.deltaTime;
        }
        else
        {
            Die();
        }
    }

    public void Init(EnemyData data)
    {
        anim.runtimeAnimatorController = data.animController;
        this.Stats = new EnemyStats(data.stats);
        ExperienceReward = this.Stats.experience_reward;
    }
    public void SetFlying(Vector2 target)
    {
        IsFlying = true;
        LandingTarget = target;
        gameObject.layer = LayerMask.NameToLayer("InAir");
        sr.sortingLayerName = "InAir";
    }
    void SetWalking()
    {
        IsFlying = false;
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        sr.sortingLayerName = "Enemy";
    }

    public override void ApplyMovement()
    {
        if (IsFlying)
        {
            transform.position = Vector2.MoveTowards(transform.position, LandingTarget, flyingSpeed * Time.deltaTime);
            if (Vector2.Distance((Vector2)transform.position, LandingTarget) < 0.1f)
            {
                SetWalking();
            }
            return;
        }
        base.ApplyMovement();
    }
}
