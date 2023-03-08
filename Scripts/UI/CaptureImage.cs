using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureImage : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] RawImage rawImage;
    [SerializeField] RenderTexture renderTexture;

    private void Update()
    {
        capture();
    }

    private void capture()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;
        rawImage.texture = image;
    }
}
