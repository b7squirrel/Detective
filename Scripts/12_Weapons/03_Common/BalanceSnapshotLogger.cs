using System.Text;
using UnityEngine;

public class BalanceSnapshotLogger : MonoBehaviour
{
    [SerializeField] float autoLogInterval = 15f; // ⭐ 원하는 주기로 조절 가능
    float autoLogTimer;
    int lastLoggedStage = -1; // ⭐ 스테이지 전환 감지용

    Character playerCharacter;

    void Update()
    {
        autoLogTimer += Time.deltaTime;
        if (autoLogTimer >= autoLogInterval)
        {
            autoLogTimer = 0f;
            LogSnapshot();
        }

        // ⭐ 스테이지가 바뀌는 순간에도 한 번 찍기
        int currentStage = PlayerDataManager.Instance.GetCurrentStageNumber();
        if (currentStage != lastLoggedStage)
        {
            lastLoggedStage = currentStage;
            LogSnapshot();
        }
    }

    public void LogSnapshot() // 버튼에서도 그대로 호출 가능
    {
        if (DamageTracker.instance == null) return;

        if (playerCharacter == null && GameManager.instance != null && GameManager.instance.player != null)
        {
            playerCharacter = GameManager.instance.player.GetComponent<Character>();
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== BALANCE SNAPSHOT =====");
        sb.AppendLine($"Stage: {PlayerDataManager.Instance.GetCurrentStageNumber()}  Time: {Time.time:F1}s");

        if (playerCharacter != null)
        {
            sb.AppendLine($"Player HP: {playerCharacter.GetCurrentHP()}/{playerCharacter.MaxHealth}  Armor: {playerCharacter.Armor}");
        }

        sb.AppendLine("--- Outgoing (무기별 딜량) ---");
        foreach (var name in DamageTracker.instance.GetAllWeaponNames())
        {
            sb.AppendLine($"{name}: total={DamageTracker.instance.GetTotalDamage(name)}, dps5={DamageTracker.instance.GetDPS_5Second(name):F1}");
        }

        sb.AppendLine("--- Incoming (받은 데미지, 타입별) ---");
        foreach (var name in DamageTracker.instance.GetAllIncomingSourceNames())
        {
            sb.AppendLine($"{name}: total={DamageTracker.instance.GetIncomingDamage(name)}, dps5={DamageTracker.instance.GetIncomingDPS_5Second(name):F1}");
        }
        sb.AppendLine($"Dodged hits: {DamageTracker.instance.GetDodgedCount()}");

        sb.AppendLine("--- Enemy Kills (TTK) ---");
        foreach (var name in DamageTracker.instance.GetAllKilledEnemyNames())
        {
            var s = DamageTracker.instance.GetKillStats(name);
            sb.AppendLine($"{name}: kills={s.count}, avgTTK={s.avgTTK:F2}s, avgHP={s.avgHP:F0}");
        }

        sb.AppendLine("============================");
        Debug.Log(sb.ToString());
    }
}