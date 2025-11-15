using UnityEngine;

public class LineAnimController : MonoBehaviour
{
    LineRenderer lineRenderer;
    [SerializeField] Texture[] textures;
    int animationStep;
    [SerializeField] float fps = 30f;
    float fpsCounter;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        fpsCounter += Time.deltaTime;
        if (fpsCounter >= 1f / fps)
        {
            animationStep++;
            if (animationStep == textures.Length) animationStep = 0;
            lineRenderer.material.SetTexture("_MainTex", textures[animationStep]);
            fpsCounter = 0f;
        }
    }
}
