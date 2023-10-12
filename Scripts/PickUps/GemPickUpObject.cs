using UnityEngine;

// 플레이어와 닿았을 때 PickUP 클래스에서 OnPickUp() 실행
public class GemPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int ExpAmount { get; set; }
    public void OnPickUp(Character character)
    {
        // 보석의 경험치를 플레이어에게 넘겨줄 때, 임시 경험치가 저장되어 있다면 같이 넘겨줌
        if (ExpAmount == 0) ExpAmount = 400;

        character.level.AddExperience(ExpAmount);
        GemManager.instance.DecreaseGemCount();


        if (GemManager.instance.HasPotentialExp())
        {
            character.level.AddExperience(GemManager.instance.GetPotentialExp());
            GemManager.instance.ResetPotentialExp(); // 임시 경험치 비우기
        }
    }
}
