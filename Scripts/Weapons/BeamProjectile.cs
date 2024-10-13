using UnityEngine;

// 재활용해서 계속 사용하는 projectile의 경우 active여부를 weapon단계에서 관리하도록 한다
public class BeamProjectile : ProjectileBase
{
    [SerializeField] Transform endPoint;
    [SerializeField] LayerMask destructables;
    [SerializeField] LayerMask walls;
    [SerializeField] LayerMask screenEdges;
    [SerializeField] Transform hitWallEffect;

    Transform startPoint; // 오리의 눈 부위에 있는 로케이터
    Animator anim;
    int frameCount = 7; // 몇 프레임 간격으로 데미지를 입힐지 정하는 변수 
    bool isSynergyActivated;
    bool disableLaser;

    [SerializeField] LineRenderer laserLine;

    void OnEnable()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        
        if (isSynergyActivated)
        {
            anim.SetTrigger("Synergy");
        }
    }
    private void OnDisable()
    {
        disableLaser = true;
        laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 0);
        laserLine.endColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 0);
        hitWallEffect.gameObject.SetActive(false);
    }

    protected override void DieProjectile()
    {
        //TimeToLive = 3f;
        //transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
    protected override void HitObject()
    {
        //TimeToLive = 3f;
        //transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(false);
    }
    protected override void Update()
    {
        // do nothing
        CastRayToDestructables();
    }
    public void SetAnimToSynergy()
    {
        anim.SetTrigger("Synergy");
        frameCount = 4;
        isSynergyActivated = true;
    }
    void DrawLaser(Vector2 _endPos)
    {
        //if (laserLine == null) GetComponent<LineRenderer>();
        Vector2 startPos = new Vector2(transform.position.x, transform.position.y + .5f);
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, _endPos);
        hitWallEffect.position = _endPos;
        laserLine.widthMultiplier = Random.Range(.1f, .4f);
        hitWallEffect.gameObject.SetActive(true);

    }
    void CastRayToDestructables()
    {
        if (Time.timeScale == 0) return;
        Vector2 startPos = new Vector2(transform.position.x, transform.position.y+ .5f);
        RaycastHit2D hit = Physics2D.Linecast(startPos, endPoint.position, destructables);
        if(Time.frameCount % frameCount != 0) // 일정 프레임 간격으로 데미지를 입힘. 반면에 레이져 그림은 계속 업데이트 됨.
        {
            DealDamage(hit);
        }
        //if (disableLaser) // 1프레임 후에 레이져를 그려주자. onEnable 되고 나서 직전의 위치에 레이져를 쏨.
        //{
        //    disableLaser = false;
        //    return;
        //}
        laserLine.startColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 1);
        laserLine.endColor = new Color(laserLine.startColor.r, laserLine.startColor.g, laserLine.startColor.b, 1);
        DrawLaser(hit.point);
        

    }
    void DealDamage(RaycastHit2D _object)
    {
        if (_object == false) return;
        if (_object.transform.GetComponent<Idamageable>() == null) 
            return;

        // 카메라 밖에 있으면 데미지 전달 하지 않음. 빌드에서 잘 작동함
        // 에디터에서는 씬 뷰에서 보이면 보이는 것으로 간주하므로 테스트가 불편함
        if (_object.transform.GetComponentInChildren<SpriteRenderer>().isVisible == false)
        {
            Debug.Log("out of the camera");
            return;
        }

        Transform enmey = _object.transform.GetComponent<Transform>();
        PostMessage(Damage, enmey.position);
        GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
        hitEffect.transform.position = enmey.position;
        enmey.GetComponent<Idamageable>().TakeDamage(Damage,
                                                     KnockBackChance,
                                                     KnockBackSpeedFactor,
                                                     _object.transform.position,
                                                     hitEffect);
    }
}