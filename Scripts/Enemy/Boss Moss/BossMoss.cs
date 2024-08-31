using UnityEngine;

public class BossMoss : EnemyBase
{
    [Header("Boss Properties")]
    [SerializeField] float timeToChangeState;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject deadBody;
    [SerializeField] int numberOfStates; // ������ �� ���� ��. 
    Spawner spawner;
    float timer;

    protected override void Update()
    {
    }

    public override void InitEnemy(EnemyData _enemyToSpawn)
    {
        this.Stats = new EnemyStats(_enemyToSpawn.stats);
        spawner = FindObjectOfType<Spawner>(); // �Կ��� enemy�� �߻��ϱ� ���ؼ�
        col = GetComponent<CapsuleCollider2D>();

        Name = _enemyToSpawn.name;

        DefaultSpeed = Stats.speed;
        currentSpeed = DefaultSpeed;
        InitHpBar();
    }

    public void ChangeStateTImer()
    {
        if (timer < timeToChangeState)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0f;

        int stateIndex = UnityEngine.Random.Range(0, numberOfStates);
        
        anim.SetTrigger(stateIndex.ToString());
    }

    public float GetDefaultSpeed()
    {
        return DefaultSpeed;
    }
}
