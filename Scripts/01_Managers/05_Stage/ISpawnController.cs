/// <summary>
/// 적 스폰을 제어할 수 있는 매니저 인터페이스
/// StageEventManager와 InfiniteStageManager가 구현
/// </summary>
public interface ISpawnController
{
    /// <summary>
    /// 적 스폰을 일시정지하거나 재개합니다
    /// </summary>
    /// <param name="pause">true면 일시정지, false면 재개</param>
    void PauseSpawn(bool pause);
}
