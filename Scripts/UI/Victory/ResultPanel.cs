using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI stageNumber;
    [SerializeField] GameObject rayBGEffect;
    [SerializeField] bool isDarkBG;
    [SerializeField] AudioClip resultSound;
    [SerializeField] GameObject[] confetties;

    public void InitAwards(int _killNum, int _coinNum, int _stageNum)
    {
        if (confetties != null)
        {
            foreach (var item in confetties)
            {
                item.SetActive(true);
                ParticleSystem ps = item.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play(); // 명시적으로 파티클 재생
                }
            }
        }

        SoundManager.instance.Play(resultSound);
        killText.text = _killNum.ToString();
        coinText.text = _coinNum.ToString();
        stageNumber.text = _stageNum.ToString();

        if (isDarkBG)
        {
            GameManager.instance.darkBG.SetActive(true);
            GameManager.instance.lightBG.SetActive(false);
            // rayBGEffect.SetActive(false);
        }
        else
        {
            GameManager.instance.lightBG.SetActive(true);
            GameManager.instance.darkBG.SetActive(false);
            // rayBGEffect.SetActive(true);
        }
        GameManager.instance.ActivateConfirmationButton(2.7f);
    }
}