using System.Collections;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;

    [Header("일반 모드 요소")]
    [SerializeField] TMPro.TextMeshProUGUI stageNumber;

    [Header("무한 모드 요소")]
    [SerializeField] TMPro.TextMeshProUGUI survivalTimeText;
    [SerializeField] TMPro.TextMeshProUGUI bestRecordText;

    [SerializeField] bool isDarkBG;
    [SerializeField] AudioClip resultSound;
    [SerializeField] CardDisp cardDisp; // 플레이어를 보여줄 cardDisp
    [SerializeField] Animator charAnim; // 오리의 애니메이터

    [Header("오리폭죽")]
    [SerializeField] ImageBouncerManager bouncerManager; // 오리 폭죽
    [SerializeField] int confettiNums; // 오리 수

    public void InitAwards(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        StartCoroutine(InitAwardsCo(killNum, coinNum, stageNum, isWinningStage));
    }

    IEnumerator InitAwardsCo(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        if(isWinningStage) bouncerManager.Jump(confettiNums); // 150마리 폭죽
        

        yield return new WaitForSecondsRealtime(.5f);

        SetEquipSpriteRow();
        if (isWinningStage == false) charAnim.SetTrigger("Hit"); // 패배 화면이라면 오리도 패배 모션으로

        SoundManager.instance.Play(resultSound);
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        stageNumber.text = stageNum.ToString();

        if (isDarkBG)
        {
            GameManager.instance.darkBG.SetActive(true);
            GameManager.instance.lightBG.SetActive(false);
        }
        else
        {
            GameManager.instance.lightBG.SetActive(true);
            GameManager.instance.darkBG.SetActive(false);
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