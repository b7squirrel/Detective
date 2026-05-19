using UnityEngine;

public class SnowManager : MonoBehaviour
{
    [SerializeField] ParticleSystem snowParticle;

    void Awake()
    {
        if (snowParticle != null) snowParticle.Stop();
    }

    void Update()
    {
        if (snowParticle == null) return;

        float cameraTop = Camera.main.transform.position.y
            + Camera.main.orthographicSize + 1f;

        snowParticle.transform.position = new Vector3(
            Camera.main.transform.position.x,
            cameraTop,
            0f
        );
    }

    public void StartSnow()
    {
        if (snowParticle != null) snowParticle.Play();
    }

    public void StopSnow()
    {
        if (snowParticle != null) snowParticle.Stop();
    }
}