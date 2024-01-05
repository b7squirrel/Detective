using UnityEngine;

public class LightRay : MonoBehaviour
{
    [SerializeField] Transform bodyTrans;
    [SerializeField] float rotateSpeed;

    void Update()
    {
        transform.position = bodyTrans.position;
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}