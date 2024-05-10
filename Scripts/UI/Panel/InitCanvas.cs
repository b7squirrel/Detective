using UnityEngine;

/// <summary>
/// 시작할 때 활성화 시킬 오브젝트와 비활성화 시킬 오브젝트 구분해서 활성/비활성
/// 작업할 때 어떻게 해놓든지 플레이 하면 제대로 작동하도록 하기 위한 클래스
/// </summary>
public class InitCanvas : MonoBehaviour
{
    [SerializeField] private GameObject[] ActiveOnStart;
    [SerializeField] private GameObject[] inActiveOnStart;

    void Awake()
    {
        for (int i = 0; i < ActiveOnStart.Length; i++)
        {
            if (ActiveOnStart[i] == null) continue;
            ActiveOnStart[i].SetActive(true);
        }
        for (int i = 0; i < inActiveOnStart.Length; i++)
        {
            if (inActiveOnStart[i] == null) continue;
            inActiveOnStart[i].SetActive(false);
        }
    }
}