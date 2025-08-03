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
    [SerializeField] CardDisp cardDisp; // 플레이어를 보여줄 cardDisp
    [Header("오리폭죽")]
    [SerializeField] ImageBouncerManager bouncerManager; // 오리 폭죽
    [SerializeField] int confettiNums; // 오리 수

    public void InitAwards(int _killNum, int _coinNum, int _stageNum)
    {
        if (confetties != null)
        {
            foreach (var item in confetties)
            {
                item.SetActive(true);
                // ParticleSystem ps = item.GetComponent<ParticleSystem>();
                // if (ps != null)
                // {
                //     ps.Play(); // 명시적으로 파티클 재생
                // }
            }
        }
        bouncerManager.Jump(confettiNums); // 150마리 폭죽

        SetEquipSpriteRow();

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

    void SetEquipSpriteRow()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow(); // card sprite row의 이미지 참조들이 남지 않게 초기화

        for (int i = 0; i < 4; i++)
        {
            Item item = GameManager.instance.startingDataContainer.GetItemDatas()[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                continue;
            }
            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

            cardDisp.SetEquipCardDisplay(i, equipmentSpriteRow, item.needToOffset, offset);
        }
    }
}