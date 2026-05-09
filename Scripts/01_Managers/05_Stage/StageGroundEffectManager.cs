using UnityEngine;

public class StageGroundEffectManager : MonoBehaviour
{
    [SerializeField] float desertSlowFactor = 0.6f;
    [SerializeField] float iceSlideDecay = 0.92f; // 1에 가까울수록 더 많이 미끄러짐

    Player player;
    Character character;

    public void Init(StageGroundType groundType)
    {
        player = FindObjectOfType<Player>();
        character = FindObjectOfType<Character>();

        switch (groundType)
        {
            case StageGroundType.OrangeDesert:
                player.SetSlowDownFactor(desertSlowFactor);
                break;

            case StageGroundType.BlueIce:
                player.EnableIceMode(true, iceSlideDecay);
                break;

            case StageGroundType.GreyLava:
                character.StartLavaDrain();
                break;

            default:
                // GreenForest, GreyStone: 효과 없음
                break;
        }
    }
}