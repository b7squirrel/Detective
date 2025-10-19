using UnityEngine;
using System.Collections;
using UnityEditor;

public class DialogBubble : MonoBehaviour
{
    [SerializeField] bool showDialog; // 말풍선을 보여줄 것인지 여부
    [SerializeField] GameObject dialogBubblePrefab;
    [SerializeField] Transform dialogBubbleTrans;
    GameObject bubble;
    TMPro.TextMeshPro dialogText;

    Coroutine co;

    public void InitDialogBubble(Transform targetTransform)
    {
        if (showDialog == false) return;
        // 말풍선 생성
        if (bubble == null)
        {
            bubble = Instantiate(dialogBubblePrefab, dialogBubbleTrans);
            bubble.transform.position = Vector2.zero;
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
        if (showDialog == false) return;

        bubble.SetActive(active);
    }
    // IEnumerator DeactivateCo()
    // {
    //     yield return new WaitForSeconds(3f);
    //     SetBubbleActive(false);
    // }

    public void DestroyBubble()
    {
        if (bubble != null)
            Destroy(bubble);
    }

    // void Update()
    // {
    //     // 말풍선 위치 업데이트 (LateUpdate: 카메라 이동 후 실행)
    //     if (bubble != null)
    //     {
    //         // if (dialogBubbleTrans == null) dialogBubbleTrans = this.transform;
    //         bubble.transform.position = dialogBubbleTrans.position;
    //     }
    // }
}