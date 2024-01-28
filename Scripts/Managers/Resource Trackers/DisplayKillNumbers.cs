using UnityEngine;

public class DisplayKillNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killNumbers;
    [SerializeField] Animator killIconAnim;
    KillManager killManager;

    void Awake()
    {
        killManager = GetComponent<KillManager>();
        killManager.OnKill += UpdateKillNumberDisp;
        killIconAnim = killNumbers.GetComponentInChildren<Animator>();
    }

    void UpdateKillNumberDisp()
    {
        killNumbers.text = killManager.GetCurrentKills().ToString();
        killIconAnim.SetTrigger("Pop");
    }
}