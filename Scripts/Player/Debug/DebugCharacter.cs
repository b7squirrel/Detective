using System.Collections;
using UnityEngine;

public class DebugCharacter : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI playerCurrentHP;
    [SerializeField] TMPro.TextMeshProUGUI hit;
    Character character;

    [Header("TIme Scale")]
    [SerializeField] TMPro.TextMeshProUGUI timeScale;

    private void Start()
    {
        if (character == null) character = FindObjectOfType<Character>();
        hit.text = "";
    }
    private void Update()
    {
        playerCurrentHP.text = character.GetCurrentHP().ToString();
        timeScale.text = Time.timeScale.ToString();
    }
    public void HitMessage(int _damage)
    {
        StartCoroutine(HitMessageCo(_damage));
    }
    IEnumerator HitMessageCo(int _damage)
    {
        hit.text = "HIT " + _damage;
        yield return new WaitForSeconds(1f);
        hit.text = "";
    }
}