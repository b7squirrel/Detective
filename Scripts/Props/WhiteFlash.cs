using System.Collections;
using UnityEngine;

public class WhiteFlash : MonoBehaviour
{
    [SerializeField] protected Material whiteMaterial;
    Coroutine whiteFlashCoroutine;
    Material initialMat;
    SpriteRenderer sr;

    public void StartWhiteFlash()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        initialMat = sr.material;

        if (gameObject.activeSelf)
        {
            whiteFlashCoroutine = StartCoroutine(WhiteFlashCo());
        }
    }

    IEnumerator WhiteFlashCo()
    {
        yield return new WaitForSeconds(.1f);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.1f);
        sr.material = initialMat;
        yield return new WaitForSeconds(.1f);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.1f);
        sr.material = initialMat;
        yield return new WaitForSeconds(.1f);
        sr.material = whiteMaterial;
        yield return new WaitForSeconds(.1f);
        sr.material = initialMat;
    }

    public void StopWhiteFlash()
    {
        sr.material = initialMat;
        StopCoroutine(whiteFlashCoroutine);
    }
}