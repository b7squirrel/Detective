using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossElectricLaser : MonoBehaviour
{
    [SerializeField] Transform targetObject; // 회전시킬 오브젝트
    [SerializeField] float rotationSpeed; // 회전 속도
    Animator beamAnim;
    SpriteRenderer sr;
    float duration;

    void Start()
    {
        StartCoroutine(TempCo());
    }
    void Update()
    {
        targetObject.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
    public void Init()
    {

    }
    IEnumerator TempCo()
    {
        ActivateSpin(false);

        yield return new WaitForSeconds(4f);
        ActivateSpin(true);
        if (beamAnim == null) beamAnim = GetComponentInChildren<Animator>();
        beamAnim.SetTrigger("In");

        yield return new WaitForSeconds(2f);
        beamAnim.SetTrigger("Out");

        yield return new WaitForSeconds(.48f);
        ActivateSpin(false);
    }
    public void ActivateSpin(bool activateSpin)
    {
        targetObject.gameObject.SetActive(activateSpin);
    }
    public void ActivateSpinObj(int activateSpin)
    {
        bool active = activateSpin == 1 ? true : false;
        targetObject.gameObject.SetActive(active);
    }
}
