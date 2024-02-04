using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 실제 버튼의 행동과는 상관 없는 기능들. 실제 행동들은 모두 유니티 이벤트로 등록
/// </summary>
public class ButtonEffect : MonoBehaviour
{
    [SerializeField] AudioClip buttonSound;
    [SerializeField] GameObject buttonEffect;
    Button myButton;
    void Start()
    {
        if (myButton == null)
            myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(LockButton);
            myButton.onClick.AddListener(OnpressedAnimation);
        }
    }
    public void LockButton()
    {
        GetComponent<Button>().interactable = false;
    }
    public void OnpressedAnimation()
    {
        myButton.GetComponent<Animator>().SetTrigger("Pressed");
    }

    public void ButtonParticleEffect()
    {
        // 눌렀을 때 이펙트
    }
    public void ButtonSound()
    {
        // 버튼을 눌렀을 때 사운드
    }
}