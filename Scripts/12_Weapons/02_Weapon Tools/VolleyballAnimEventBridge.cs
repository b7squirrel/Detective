using UnityEngine;

public class VolleyballAnimEventBridge : MonoBehaviour
{
    PitcherWeapon pitcherWeapon;

    // ⭐ PitcherWeapon이 직접 자신을 등록
    public void SetPitcherWeapon(PitcherWeapon weapon)
    {
        pitcherWeapon = weapon;
    }

    public void OnThrowBall()
    {
        if (pitcherWeapon == null)
        {
            Debug.LogError("[VolleyballAnimEventBridge] PitcherWeapon이 등록되지 않았습니다!");
            return;
        }
        pitcherWeapon.ThrowBall();
    }
}