using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBouncable
{
    void GetBounced(float bouncingForce, Vector2 direction, float bouncingTime);
}
