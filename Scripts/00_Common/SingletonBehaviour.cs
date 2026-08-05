using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    // 씬 전환 시 삭제할지 여부 (자식 클래스가 Init() 안에서 자유롭게 설정 가능)
    protected bool m_IsDestroyOnLoad = false;

    // 이 클래스의 스태틱 인스턴스 변수
    protected static T m_Instance;

    public static T Instance
    {
        get { return m_Instance; }
    }

    void Awake()
    {
        if (m_Instance == null)
        {
            // ⭐ 내가 최초의 인스턴스로 확정된 경우에만 m_Instance를 세팅
            m_Instance = (T)this;

            // ⭐ 변경: Init()을 먼저 실행 (여기서 m_IsDestroyOnLoad 설정 가능)
            Init();

            // ⭐ 변경: Init() 실행 후에 DontDestroyOnLoad 여부 결정
            if (m_IsDestroyOnLoad == false)
            {
                DontDestroyOnLoad(this);
            }
        }
        else if (m_Instance != this)
        {
            // ⭐ 변경: 중복 인스턴스는 Init()을 아예 호출하지 않고 즉시 파괴 예약
            //         → 부수효과(네트워크 요청, 이벤트 구독, 코루틴 시작 등)가 원천적으로 발생하지 않음
            Debug.LogWarning($"[SingletonBehaviour] 중복 인스턴스 감지: {typeof(T).Name} (InstanceID={GetInstanceID()}) - Init() 실행 없이 즉시 파괴합니다.");
            Destroy(gameObject);
        }
    }

    // ⭐ 변경: 더 이상 중복 판정 로직을 갖지 않음. 순수하게 "진짜 싱글톤이 됐을 때 할 일"만 담당.
    //         이제 Instance == this는 이 메서드 안에서 항상 true이므로,
    //         자식 클래스에서 "if (Instance != this) return;" 가드는 더 이상 필요 없음 (있어도 무해함).
    protected virtual void Init()
    {
    }

    // 삭제 시 실행되는 함수
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    // 삭제 시 추가로 처리해 주어야할 작업을 여기서 처리
    protected virtual void Dispose()
    {
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }
}