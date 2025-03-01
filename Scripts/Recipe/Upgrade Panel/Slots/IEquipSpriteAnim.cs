using UnityEngine;
public interface IEquipSpriteAnim
{
    void InitSpriteRow();
    void SetEquipCardDisplay(int index, SpriteRow spriteRow, bool needToOffset, Vector2 offset);
}
