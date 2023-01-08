using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public PoolManager poolManager;

    public bool IsPlayerDead { get; set; }

    public float gameTime;
    public float maxGameTime = 20f;

    #region Unity CallBack Functions
    private void Awake()
    {
        instance = this;
        SubscribeOnDie();
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

    void SubscribeOnDie()
    {
        Character character = player.GetComponent<Character>();
        character.OnDie += SetPlayerDead;
    }
    void SetPlayerDead()
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
}
