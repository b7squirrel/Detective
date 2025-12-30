using UnityEngine;
using TMPro;

public class EnemyCountUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI waveProgressText;

    [Header("디버그")]
    [SerializeField] TextMeshProUGUI currentEnemiesText;

    /// <summary>
    /// wave가 변할 때마다, 적이 죽을 때마다 업데이트
    /// </summary>
    public void UpdateWaveProgress(string currentWaveEnemiesKilled, string currentWavePlannedEnemies)
    {
        waveProgressText.text = $"({currentWaveEnemiesKilled} / {currentWavePlannedEnemies})";
    }

    #region 디버그
    public void InitDebugCurrentEnemies(string currentEnemies)
    {
        currentEnemiesText.text = currentEnemies;
    }
    #endregion
}
