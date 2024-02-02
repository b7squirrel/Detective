using UnityEngine;

/// <summary>
/// ������ �� Ȱ��ȭ ��ų ������Ʈ�� ��Ȱ��ȭ ��ų ������Ʈ �����ؼ� Ȱ��/��Ȱ��
/// �۾��� �� ��� �س����� �÷��� �ϸ� ����� �۵��ϵ��� �ϱ� ���� Ŭ����
/// </summary>
public class InitCanvas : MonoBehaviour
{
    [SerializeField] private GameObject[] ActiveOnStart;
    [SerializeField] private GameObject[] inActiveOnStart;

    void Awake()
    {
        for (int i = 0; i < ActiveOnStart.Length; i++)
        {
            ActiveOnStart[i].SetActive(true);
        }
        for (int i = 0;i < inActiveOnStart.Length; i++)
        {
            inActiveOnStart[i].SetActive(false);
        }
    }
}