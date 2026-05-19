using System.Collections;
using UnityEngine;

public class DebrisManager : MonoBehaviour
{
    [Header("파티클")]
    [SerializeField] ParticleSystem dustParticle;
    [SerializeField] ParticleSystem debrisParticle;

    WallManager wallManager;

    void Awake()
    {
        if (dustParticle != null) dustParticle.Stop();
        if (debrisParticle != null) debrisParticle.Stop();
    }

    void Update()
    {
        if (dustParticle == null || debrisParticle == null) return;

        float cameraTop = Camera.main.transform.position.y
            + Camera.main.orthographicSize + 1f;

        Vector3 spawnPos = new Vector3(
            Camera.main.transform.position.x,
            cameraTop,
            0f
        );

        dustParticle.transform.position = spawnPos;
        debrisParticle.transform.position = spawnPos;
    }

    public void StartDebris()
    {
        if (dustParticle != null) dustParticle.Play();
        if (debrisParticle != null) debrisParticle.Play();
    }

    public void StopDebris()
    {
        if (dustParticle != null) dustParticle.Stop();
        if (debrisParticle != null) debrisParticle.Stop();
    }
}