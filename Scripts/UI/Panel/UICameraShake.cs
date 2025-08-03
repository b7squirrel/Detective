using UnityEngine;

public class UICameraShake : MonoBehaviour
{
    public static UICameraShake instance;

    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeMagnitude = 0.1f;
    [SerializeField] float dampingSpeed = 1.0f;

    Vector3 initialPosition;
    float currentDuration = 0f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (currentDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            currentDuration -= Time.unscaledDeltaTime * dampingSpeed;
        }
        else
        {
            currentDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public void TestShake()
    {
        Shake(.5f, 10f);
    }
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        currentDuration = duration > 0 ? duration : shakeDuration;
        shakeMagnitude = magnitude > 0 ? magnitude : shakeMagnitude;
        Debug.Log("Shake");
    }
}
