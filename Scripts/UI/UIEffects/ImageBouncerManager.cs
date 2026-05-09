using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageBouncerManager : MonoBehaviour
{
    [SerializeField] GameObject[] objectToJump;
    [SerializeField] Transform[] rootObjs;
    [SerializeField] float verticalVelocity;
    [SerializeField] float horizontalVelocity;
    [SerializeField] float gravity;
    [SerializeField] float xOffsetRange;
    [SerializeField] float yOffsetRange;

    [Header("Block FG")]
    [SerializeField] GameObject blockFG; // ← 추가: 인스펙터에서 FG 오브젝트 연결

    [Header("Sound")]
    [SerializeField] AudioClip[] oriSound;

    public void Jump(int nums, float timeOffset)      { StartCoroutine(JumpCo(nums, timeOffset, 0)); }
    public void JumpWithDelay(int nums)               { StartCoroutine(JumpCo(nums, .4f, 0)); }
    public void JumpHappy(int nums)                   { StartCoroutine(JumpCo(nums, 0f, 0)); }
    public void JumpSad(int nums)                     { StartCoroutine(JumpCo(nums, 0f, 1)); }

    IEnumerator JumpCo(int nums, float timeOffset, int spriteIndex)
    {
        yield return new WaitForSecondsRealtime(timeOffset);

        // ① FG 활성화 → 터치 블락
        if (blockFG != null) blockFG.SetActive(true);

        // 생성된 오브젝트 추적용 리스트
        List<GameObject> spawnedObjects = new List<GameObject>(); // ← 추가

        foreach (var item in rootObjs)
        {
            RectTransform root = item.GetComponent<RectTransform>();
            for (int i = 0; i < nums; i++)
            {
                float verticalVel   = verticalVelocity + UnityEngine.Random.Range(-400f, 400f);
                float randomValue   = UnityEngine.Random.Range(0f, 1f);
                float dir           = randomValue > .5f ? 1f : -1f;
                float horizontalVel = (dir * horizontalVelocity) + UnityEngine.Random.Range(-300f, 300f);

                GameObject go = Instantiate(objectToJump[spriteIndex], item);
                spawnedObjects.Add(go); // ← 추가

                float xOffset = dir * UnityEngine.Random.Range(-xOffsetRange, xOffsetRange);
                float yOffset = UnityEngine.Random.Range(-yOffsetRange, yOffsetRange);

                RectTransform goRec = go.GetComponent<RectTransform>();
                goRec.position   = new Vector2(root.position.x + xOffset, root.position.y + yOffset);

                float scaleOffset  = UnityEngine.Random.Range(.5f, 4.5f);
                goRec.localScale   = scaleOffset * Vector2.one;

                float rotateOffset = UnityEngine.Random.Range(0, 360f);
                Transform sprite   = goRec.GetChild(0);
                sprite.eulerAngles = new Vector3(0f, 0f, rotateOffset);

                go.GetComponent<ImageBouncer>().InitBouncer(verticalVel, horizontalVel, gravity, root.position.y);
            }
        }

        SoundManager.instance.Play(oriSound[spriteIndex]);

        // ② 모든 오브젝트가 파괴될 때까지 대기 후 FG 비활성화
        yield return new WaitUntil(() => spawnedObjects.TrueForAll(go => go == null)); // ← 추가
        if (blockFG != null) blockFG.SetActive(false);
    }
}