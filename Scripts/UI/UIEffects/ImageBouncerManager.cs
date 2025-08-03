using System.Collections;
using UnityEngine;

public class ImageBouncerManager : MonoBehaviour
{
    [SerializeField] GameObject objectToJump; // 튀어 오를 이미지 프리펩
    [SerializeField] Transform[] rootObjs; // 모든 점프 이미지들의 부모가 될 오브젝트
    [SerializeField] float verticalVelocity;
    [SerializeField] float horizontalVelocity;
    [SerializeField] float gravity;
    [SerializeField] float xOffsetRange;
    [SerializeField] float yOffsetRange;
    [Header("Sound")]
    [SerializeField] AudioClip oriSound;

    public void Jump(int nums, float timeOffset)
    {
        StartCoroutine(JumpCo(nums, timeOffset));
    }

    // Launch Button을 누르면 호출
    public void JumpWithDelay(int nums)
    {
        StartCoroutine(JumpCo(nums, .4f));
    }
    public void Jump(int nums)
    {
        StartCoroutine(JumpCo(nums, 0f));
    }

    IEnumerator JumpCo(int nums, float timeOffset)
    {
        yield return new WaitForSeconds(timeOffset);
        foreach (var item in rootObjs)
        {
            RectTransform root = item.GetComponent<RectTransform>();
            for (int i = 0; i < nums; i++)
            {
                float verticalVel = verticalVelocity + UnityEngine.Random.Range(-400f, 400f);

                float randomValue = UnityEngine.Random.Range(0f, 1f);
                float dir = randomValue > .5f ? 1f : -1f;

                float horizontalVel = (dir * horizontalVelocity) + UnityEngine.Random.Range(-300f, 300f);

                GameObject go = Instantiate(objectToJump, item);
                float xOffset = dir * UnityEngine.Random.Range(-xOffsetRange, xOffsetRange);
                float yOffset = UnityEngine.Random.Range(-yOffsetRange, yOffsetRange);

                // 시작 위치
                RectTransform goRec = go.GetComponent<RectTransform>();
                goRec.position = new Vector2(root.position.x + xOffset, root.position.y + yOffset);

                // 크기
                float scaleOffset = UnityEngine.Random.Range(.5f, 4.5f);
                goRec.localScale = scaleOffset * Vector2.one;

                // 회전
                float rotateOffset = UnityEngine.Random.Range(0, 360f);
                Transform sprite = goRec.GetChild(0);
                sprite.eulerAngles = new Vector3(0f, 0f, rotateOffset);

                // 점프 초기화
                go.GetComponent<ImageBouncer>().InitBouncer(verticalVel, horizontalVel, gravity, root.position.y);
            }
        }
        SoundManager.instance.Play(oriSound);
    }
}
