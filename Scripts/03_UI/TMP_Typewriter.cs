using System.Collections;
using TMPro;
using UnityEngine;

public class TMP_Typewriter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] TextMeshProUGUI textUI;

    [Header("Settings")]
    [SerializeField] float charInterval = 0.04f;
    [SerializeField] AudioClip typeSound;
    [SerializeField] bool ignoreTimeScale = true;

    bool isPlaying;
    Coroutine typingCo;
    AudioSource audioSource;

    void Awake()
    {
        if (textUI == null)
            textUI = GetComponent<TextMeshProUGUI>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && typeSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Play()
    {
        if (textUI == null) return;

        Stop();

        typingCo = StartCoroutine(TypeRoutine());
    }

    public void Skip()
    {
        if (!isPlaying) return;

        StopCoroutine(typingCo);
        typingCo = null;

        textUI.maxVisibleCharacters = textUI.text.Length;
        isPlaying = false;
    }

    public void Stop()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }

        isPlaying = false;
    }

    IEnumerator TypeRoutine()
    {
        isPlaying = true;
        textUI.ForceMeshUpdate();
        int totalChars = textUI.textInfo.characterCount;
        textUI.maxVisibleCharacters = 0;

        for (int i = 1; i <= totalChars; i++)
        {
            textUI.maxVisibleCharacters = i;

            // if (typeSound != null && audioSource != null)
            //     audioSource.PlayOneShot(typeSound);

            if (ignoreTimeScale)
                yield return new WaitForSecondsRealtime(charInterval);
            else
                yield return new WaitForSeconds(charInterval);
        }

        isPlaying = false;
        typingCo = null;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
    public void ResetMaxVisibleChar()
    {
        textUI.maxVisibleCharacters = 0;
    }
}