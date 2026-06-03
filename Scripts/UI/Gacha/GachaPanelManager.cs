using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaPanelManager : MonoBehaviour
{
    [SerializeField] GachaField gachaField;
    [SerializeField] GameObject FG;

    // ⭐ 골드 FX 관련
    [Header("골드 FX")]
    [SerializeField] float goldFXDelay = 0.3f; // 카드 등장 후 FX 재생 딜레이

    MainMenuManager mainMenuManager;

    // ⭐ 골드 FX 파라미터 (PackBuyButton에서 설정)
    private RectTransform pendingGemPoint;
    private int pendingGoldAmount;

    public void InitGachaPanel(List<CardData> cards)
    {
        if (mainMenuManager == null) mainMenuManager = FindObjectOfType<MainMenuManager>();
        gachaField.GenerateAllCardsOfType(cards);
        ActivateButtonTapToCon(true);

        // ⭐ 골드 FX 예약 실행
        if (pendingGemPoint != null && pendingGoldAmount > 0)
        {
            StartCoroutine(PlayGoldFXDelayed(pendingGemPoint, pendingGoldAmount));
            pendingGemPoint = null;
            pendingGoldAmount = 0;
        }
    }

    /// <summary>
    /// ⭐ PackBuyButton에서 골드 FX 파라미터를 미리 등록합니다.
    /// GachaSystem.OpenBox() 호출 전에 호출해야 합니다.
    /// </summary>
    public void RegisterGoldFX(RectTransform gemPoint, int goldAmount)
    {
        pendingGemPoint = gemPoint;
        pendingGoldAmount = goldAmount;
    }

    /// <summary>
    /// ⭐ 딜레이 후 골드 FX 재생
    /// </summary>
    IEnumerator PlayGoldFXDelayed(RectTransform gemPoint, int goldAmount)
    {
        yield return new WaitForSeconds(goldFXDelay);

        ShopManager shopManager = ShopManager.Instance;
        if (shopManager != null)
            shopManager.PlayGoldFX(gemPoint, goldAmount);
        else
            Logger.LogWarning("[GachaPanelManager] ShopManager를 찾을 수 없습니다.");
    }

    public void ActivateButtonTapToCon(bool activate)
    {
        gameObject.SetActive(activate);
        FG.SetActive(false);

        if (!activate)
        {
            foreach (var btn in FindObjectsOfType<ChestBuyButton>())
                btn.ResetState();
            foreach (var btn in FindObjectsOfType<PackBuyButton>())
                btn.ResetState();

            // ⭐ 패널 닫힐 때 FX 파라미터 초기화
            pendingGemPoint = null;
            pendingGoldAmount = 0;
        }
    }
}