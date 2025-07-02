using UnityEngine;
using System.Collections;

public class DialogBubble : MonoBehaviour
{
    [SerializeField] GameObject dialogBubblePrefab;
    GameObject bubble;
    TMPro.TextMeshPro dialogText;

    Coroutine co;

    public void InitDialogBubble(Transform targetTransform)
    {
        // 말풍선 생성
        if (bubble == null)
        {
            bubble = Instantiate(dialogBubblePrefab);
            dialogText = bubble.GetComponentInChildren<TMPro.TextMeshPro>();
        }

        // if (co != null) StopCoroutine(co);
        // co = StartCoroutine(DeactivateCo());
    }

    public void SetDialogText(string text)
    {
        if (dialogText != null)
            dialogText.text = text;
    }

    public void SetBubbleActive(bool active)
    {
        bubble.SetActive(active);
    }
    IEnumerator DeactivateCo()
    {
        yield return new WaitForSeconds(3f);
        SetBubbleActive(false);
    }

    public void DestroyBubble()
    {
        if (bubble != null)
            Destroy(bubble);
    }

    void Update()
    {
        // 말풍선 위치 업데이트 (LateUpdate: 카메라 이동 후 실행)
        if (bubble != null)
        {
            bubble.transform.position = this.transform.position;
        }
    }
}