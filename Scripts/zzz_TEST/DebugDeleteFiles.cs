using System.IO;
using UnityEngine;

public class DebugDeleteFiles : MonoBehaviour
{
    public void ClearAllSaveData()
    {
        string path = Application.persistentDataPath;
        
        if (Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            
            // 모든 파일 삭제
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                    Debug.Log($"파일 삭제됨: {file.Name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"파일 삭제 실패: {file.Name}, 오류: {e.Message}");
                }
            }
            
            // 모든 하위 폴더 삭제
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                try
                {
                    dir.Delete(true); // true = 하위 폴더와 파일도 모두 삭제
                    Debug.Log($"폴더 삭제됨: {dir.Name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"폴더 삭제 실패: {dir.Name}, 오류: {e.Message}");
                }
            }
            
            Debug.Log("모든 저장 데이터가 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning("persistentDataPath 폴더가 존재하지 않습니다.");
        }
    }
}
