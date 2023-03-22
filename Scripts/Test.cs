using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Vector3 startDir = Vector3.right;
    Vector3 vec;
    Vector3[] targetDir = new Vector3[2];

    // void Update()
    // {
    //     Vector3 axisVec = Vector3.forward;

    //     Debug.DrawLine(Vector3.zero, startDir, Color.red);

    //     for (int i = 0; i < 2; i++)
    //     {
    //         targetDir[i] = Quaternion.AngleAxis((float)(120 * (i + 1)), axisVec) * vec;
    //         Debug.DrawLine(Vector3.zero, targetDir[i], Color.blue);
    //     }

    //     Debug.DrawLine(Vector3.zero, targetDir[0], Color.blue);
    //     Debug.DrawLine(Vector3.zero, targetDir[1], Color.yellow);
    // }
}
