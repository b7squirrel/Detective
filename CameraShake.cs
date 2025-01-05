using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    [SerializeField] Camera mainCamera;
    [SerializeField][Range(.01f, 2f)] float shakeRange = .05f;
    [SerializeField][Range(.1f, 1f)] float duration = .5f;

    Vector3 originalCameraPos;
    bool isShaking; // 쉐이킹하는 도중에 또 쉐이킹 하지 않도록

    void Awake()
    {
        instance = this;
    }
    public void Shake()
    {
        if (isShaking) return;
        originalCameraPos = mainCamera.transform.position;
        InvokeRepeating("StartShake", 0f, .005f);
        Invoke("StopShake", duration);
    }

    void StartShake()
    {
        isShaking = true;
        float cameraPosX = Random.value * shakeRange * 2 - shakeRange;
        float cameraPosY = Random.value * shakeRange * 2 - shakeRange;
        Vector3 cameraPos = mainCamera.transform.position;
        cameraPos.x += cameraPosX;
        cameraPos.y += cameraPosY;
        mainCamera.transform.position = cameraPos;
    }
    void StopShake()
    {
        CancelInvoke("StartShake");
        mainCamera.transform.position = originalCameraPos;
        isShaking=false;
    }
}