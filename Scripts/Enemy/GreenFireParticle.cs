using UnityEngine;

/// <summary>
/// 초록빛 불꽃 파티클 이펙트
/// 
/// [사용 방법 A] 투사체 꼬리불꽃: fireProjectile 프리팹에 추가
///   - mode = ProjectileTrail
///   - 투사체가 날아가는 동안 꼬리처럼 불꽃이 따라옴
///
/// [사용 방법 B] 발사구 불꽃: shootPoint 오브젝트에 추가하고 BossFireFan에서 제어
///   - mode = MuzzleFlame
///   - 발사할 때 Play(), 종료 시 Stop() 호출
///
/// [텍스처 설정]
///   Inspector의 flameTexture 슬롯에 첨부된 초록 glow 이미지를 할당하세요.
///   없으면 Unity 기본 파티클 텍스처를 사용합니다.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class GreenFireParticle : MonoBehaviour
{
    public enum FireMode
    {
        ProjectileTrail,  // 투사체에 붙어서 꼬리처럼 따라다님
        MuzzleFlame,      // 발사구에서 불꽃이 뿜어나옴
    }

    [Header("모드")]
    [SerializeField] FireMode mode = FireMode.ProjectileTrail;

    [Header("텍스처")]
    [SerializeField] Texture2D flameTexture; // 첨부된 초록 glow 이미지를 여기에 할당

    [Header("색상 — 기본값: 초록 불꽃")]
    [SerializeField] Color colorCore  = new Color(1f,   1f,   1f,   1f);   // 중심: 흰색
    [SerializeField] Color colorMid   = new Color(0.2f, 1f,   0.3f, 0.9f); // 중간: 밝은 초록
    [SerializeField] Color colorEdge  = new Color(0f,   0.6f, 0.1f, 0.5f); // 가장자리: 짙은 초록
    [SerializeField] Color colorFade  = new Color(0f,   0.3f, 0.05f, 0f);  // 소멸: 완전 투명

    [Header("파티클 크기")]
    [SerializeField] float sizeMin = 0.15f;
    [SerializeField] float sizeMax = 0.45f;

    [Header("방출량")]
    [SerializeField] float emissionRate = 40f; // ProjectileTrail 모드
    [SerializeField] float muzzleEmissionRate = 80f; // MuzzleFlame 모드

    [Header("생존 시간 (초)")]
    [SerializeField] float lifetimeMin = 0.2f;
    [SerializeField] float lifetimeMax = 0.5f;

    ParticleSystem ps;
    ParticleSystemRenderer psRenderer;
    Material flameMat;
    bool isSetup = false;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        Setup();
    }

    // ─────────────────────────────────────────────
    // 외부 제어 (MuzzleFlame 모드에서 BossFireFan이 호출)
    // ─────────────────────────────────────────────
    public void Play()
    {
        if (!isSetup) Setup();
        ps.Play();
    }

    public void Stop()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // ─────────────────────────────────────────────
    // 파티클 시스템 전체 세팅
    // ─────────────────────────────────────────────
    void Setup()
    {
        SetupRenderer();
        SetupMain();
        SetupEmission();
        SetupShape();
        SetupColorOverLifetime();
        SetupSizeOverLifetime();
        SetupVelocityOverLifetime();
        SetupNoise();

        // ProjectileTrail은 Awake에서 자동 재생
        if (mode == FireMode.ProjectileTrail)
            ps.Play();

        isSetup = true;
    }

    // ── 1. 렌더러: Additive 블렌딩으로 빛나는 느낌 ──
    void SetupRenderer()
    {
        flameMat = new Material(Shader.Find("Particles/Standard Unlit"));
        if (flameMat == null)
            flameMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        // Additive 블렌딩 설정
        flameMat.SetFloat("_Mode", 4); // Additive

        if (flameTexture != null)
            flameMat.mainTexture = flameTexture;

        psRenderer.material = flameMat;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.sortingOrder = 5; // 캐릭터 위에 그려지도록
    }

    // ── 2. 메인 모듈 ──
    void SetupMain()
    {
        var main = ps.main;
        main.loop = true;
        main.prewarm = (mode == FireMode.ProjectileTrail); // Trail은 즉시 꽉 차 있도록

        // 생존 시간
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);

        // 초기 크기
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);

        // 초기 속도: Trail은 느리게, Muzzle은 빠르게
        main.startSpeed = mode == FireMode.ProjectileTrail
            ? new ParticleSystem.MinMaxCurve(0.1f, 0.4f)
            : new ParticleSystem.MinMaxCurve(0.5f, 1.5f);

        // 초기 색상은 ColorOverLifetime에서 관리
        main.startColor = Color.white;

        // 중력: 약간 위로 떠오르는 느낌
        main.gravityModifier = -0.05f;

        // 파티클 공간: Trail은 World(꼬리 흔적), Muzzle은 Local
        main.simulationSpace = mode == FireMode.ProjectileTrail
            ? ParticleSystemSimulationSpace.World
            : ParticleSystemSimulationSpace.Local;

        // 최대 파티클 수
        main.maxParticles = 200;
    }

    // ── 3. 방출 모듈 ──
    void SetupEmission()
    {
        var emission = ps.emission;
        emission.enabled = true;

        float rate = mode == FireMode.ProjectileTrail ? emissionRate : muzzleEmissionRate;
        emission.rateOverTime = rate;
    }

    // ── 4. 형태 모듈 ──
    void SetupShape()
    {
        var shape = ps.shape;
        shape.enabled = true;

        if (mode == FireMode.ProjectileTrail)
        {
            // 투사체 꼬리: 좁은 구체에서 방출
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;
        }
        else
        {
            // 발사구 불꽃: 앞쪽으로 뻗어나가는 콘 형태
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.1f;
        }
    }

    // ── 5. 수명에 따른 색상: 흰색 → 밝은초록 → 짙은초록 → 투명 ──
    void SetupColorOverLifetime()
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorCore,  0.00f),
                new GradientColorKey(colorMid,   0.25f),
                new GradientColorKey(colorEdge,  0.65f),
                new GradientColorKey(colorFade,  1.00f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.00f),
                new GradientAlphaKey(0.9f, 0.25f),
                new GradientAlphaKey(0.4f, 0.70f),
                new GradientAlphaKey(0.0f, 1.00f),
            }
        );

        col.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    // ── 6. 수명에 따른 크기: 작게 시작 → 커졌다가 → 작아지며 소멸 ──
    void SetupSizeOverLifetime()
    {
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.3f);  // 시작: 작음
        curve.AddKey(0.3f, 1.0f);  // 중간: 최대
        curve.AddKey(1.0f, 0.0f);  // 끝: 소멸

        sol.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    // ── 7. 수명에 따른 속도: 위로 약하게 떠오름 ──
    void SetupVelocityOverLifetime()
    {
        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.space = ParticleSystemSimulationSpace.Local;

        // Y축 위로 약하게
        vol.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        vol.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f); // 좌우 약간 흔들
    }

    // ── 8. 노이즈: 불꽃이 자연스럽게 흔들리도록 ──
    void SetupNoise()
    {
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.5f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
    }

    void OnDestroy()
    {
        if (flameMat != null)
            Destroy(flameMat);
    }
}