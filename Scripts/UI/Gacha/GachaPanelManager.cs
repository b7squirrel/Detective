using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaPanelManager : MonoBehaviour
{
    [SerializeField] GachaField gachaField;
    [SerializeField] GameObject FG;

    // ⭐ 골드 FX 관련
    [Header("골드 FX")]
    [SerializeField] float goldFXDelay = 0.3f;

    MainMenuManager mainMenuManager;

    private RectTransform pendingGemPoint;
    private int pendingGoldAmount;

    // ⭐ 알 깨짐 연출까지 대기할 카드 데이터
    private List<CardData> pendingCards;

    [Header("타이틀")]
    [SerializeField] TMPro.TextMeshProUGUI titleText;

    public void InitGachaPanel(List<CardData> cards)
    {
        if (mainMenuManager == null) mainMenuManager = FindObjectOfType<MainMenuManager>();

        // 타이틀 세팅
        var g = LocalizationManager.Game;
        bool isItem = cards.Count > 0 && cards[0].Type == "Item";
        if (isItem)
        {
            titleText.text = cards.Count == 1 ? g.newItem : g.newItems;
        }
        else
        {
            titleText.text = cards.Count == 1 ? g.newFriend : g.newFriends;
        }

        // ⭐ 카드 데이터는 저장만 해두고, 생성은 알 깨짐 애니메이션 이벤트로 미룸
        pendingCards = cards;

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
    /// ⭐ Animation Event 전용: 알이 깨지고 폭발 이펙트가 나오는 타이밍에 호출됨
    /// GachaEgg Init 클립에 이 메서드를 이벤트로 등록해주세요.
    /// </summary>
    public void OnEggBrokenAnimEvent()
    {
        if (pendingCards == null)
        {
            Logger.LogWarning("[GachaPanelManager] OnEggBrokenAnimEvent 호출되었지만 pendingCards가 없습니다.");
            return;
        }

        gachaField.GenerateAllCardsOfType(pendingCards);
        pendingCards = null;
    }

    public void RegisterGoldFX(RectTransform gemPoint, int goldAmount)
    {
        pendingGemPoint = gemPoint;
        pendingGoldAmount = goldAmount;
    }

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

            pendingGemPoint = null;
            pendingGoldAmount = 0;
            pendingCards = null; // ⭐ 패널 닫힐 때 대기 중이던 카드 데이터도 초기화
        }
    }
}