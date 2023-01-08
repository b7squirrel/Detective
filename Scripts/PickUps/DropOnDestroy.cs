using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField] GameObject dropItemPrefab;
    [SerializeField][Range(0f, 1f)] float chance = 1f;

    bool isQuiting;

    void OnApplicationQuit()
    {
        isQuiting = true;
    }

    public void CheckDrop()
    {
        if (isQuiting) return;

        if (Random.value < chance)
        {
            Transform pickUP = Instantiate(dropItemPrefab).transform;
            pickUP.position = transform.position;
        }
    }
}
