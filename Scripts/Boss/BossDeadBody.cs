using System.Collections;
using UnityEngine;

public class BossDeadBody : MonoBehaviour, Idamageable
{
    [Header("이펙트")]
    [SerializeField] GameObject teleportEffectPrefab;

    [Header("사운드")]
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;

    [Header("드롭")]
    [SerializeField] GameObject dropPrefab;
    [SerializeField] int dropNums; // 드롭할 기본 개수
    bool isDamageable; // 아이들 상태로 들어가면 그제서야 데미지를 받고 반응할 수 있다
    public bool FinishBossCam { get; private set; }
    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    public void TeleportOutEffect()
    {
        StartCoroutine(TeleportOutEffectCo());
    }
    IEnumerator TeleportOutEffectCo()
    {
        GameManager.instance.GetComponent<TeleportEffect>().GenTeleportOutEffect(transform.position);
        yield return new WaitForSeconds(.45f);
        isDamageable = false;
        gameObject.SetActive(false);
    }

    //animation events
    public void PlayCrownDropSFX()
    {
        SoundManager.instance.Play(crownDropSFX);
    }
    public void PlayerSquelchSFX()
    {
        SoundManager.instance.Play(squelchSFX);
        SoundManager.instance.Play(squeackSFX);
    }
    public void TriggerPlayerCamera()
    {
        BossDieManager.instance.BossCameraOff();
    }
    public void SetDamageable()
    {
        isDamageable = true;
    }

    public void TakeDamage(int damage, float knockBackChance, float knockBackSpeed, Vector2 target, GameObject hitEffect)
    {
        // 아이들 상태이고 Hit 애니메이션이 끝난 상태라면 반응하기
        if (isDamageable)
        {
            // AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0); // 0은 Base Layer
            // if (stateInfo.IsName("Hit")) return;

            anim.SetTrigger("Hit");

            int num = dropNums + UnityEngine.Random.Range(0, 5);
            for (int i = 0; i < num; i++)
            {
                GameObject go = GameManager.instance.poolManager.GetMisc(dropPrefab);
                go.transform.position = transform.position;
            }
        }
    }
}
