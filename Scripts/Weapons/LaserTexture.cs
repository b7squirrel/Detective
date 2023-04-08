using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTexture : MonoBehaviour
{
    [SerializeField] Texture[] idleTextures;
    [SerializeField] Material mat;
    public int animationStep {get; set;}
    float fps = 30f;
    float fpsCounter;
    LineRenderer lr;

    void Update()
    {
        if (lr == null)
        {
            lr = GetComponentInChildren<LineRenderer>();
            lr.material = mat;
            lr.textureMode = LineTextureMode.Tile;
        }
            
        fpsCounter += Time.deltaTime;
        if (fpsCounter >= 1f / 30f)
        {
            if (animationStep == idleTextures.Length)
                animationStep = idleTextures.Length;
            lr.material.SetTexture("_MainTex", idleTextures[animationStep]);
            fpsCounter = 0f;
            animationStep++;
        }
    }
}
