using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public PoolManager poolManager;
    public EggPanelManager eggPanelManager;

    public GameObject joystick;

    public bool IsPlayerDead { get; set; }

    public float gameTime;
    public float maxGameTime = 20f;

    [SerializeField] Camera currentCamera;
    public Collider2D cameraBoundary;

    #region Unity CallBack Functions
    private void Awake()
    {
        instance = this;
        currentCamera = Camera.main;
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
        }
    }
    #endregion

    public void SetPlayerDead()
    {
        IsPlayerDead = true;
    }

    #region Option Input
    public void OnQuitGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Application.Quit();
        }
    }
    public void OnResetGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    #endregion

    public void ShakeCam(float _duration, float _magnitude)
    {
        StartCoroutine(ShakeCamCo(_duration, _magnitude));
    }
    IEnumerator ShakeCamCo(float duration, float magnitude)
    {
        Vector3 originalPos = currentCamera.transform.position;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            currentCamera.transform.position += new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentCamera.transform.position = originalPos;
    }
}
