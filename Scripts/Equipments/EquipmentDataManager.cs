using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 내가 구글시트로 작성할 필요 없는 데이터
/// </summary>
[System.Serializable]
public class CardEquipmentData
{
    public CardEquipmentData(string _ori, string _head, string _chest,
        string _legs, string _weapon)
    {
        // 안전한 파싱으로 변경
        charID = SafeParseInt(_ori, "charID");
        IDs[0] = SafeParseInt(_head, "head");
        IDs[1] = SafeParseInt(_chest, "chest");
        IDs[2] = SafeParseInt(_legs, "legs");
        IDs[3] = SafeParseInt(_weapon, "weapon");
    }
    
    private int SafeParseInt(string value, string fieldName)
    {
        string cleanValue = value?.Trim();
        if (int.TryParse(cleanValue, out int result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"{fieldName} 파싱 실패: '{value}' - 기본값 0 사용");
            return 0;
        }
    }
    
    public int charID;
    public int[] IDs = new int[4];
}

[System.Serializable]
public class ReadEquipmentsData
{
    public List<CardEquipmentData> cardStatsList;
    
    public List<CardEquipmentData> GetCardEquipmentsList(TextAsset statsDataText)
    {
        cardStatsList = new List<CardEquipmentData>();
        
        try
        {
            if (statsDataText == null || string.IsNullOrEmpty(statsDataText.text))
            {
                Debug.LogError("장비 데이터 파일이 비어있습니다.");
                return cardStatsList;
            }
            
            // 크로스 플랫폼 텍스트 정규화
            string normalizedText = statsDataText.text
                .Replace("\r\n", "\n")  // Windows → Unix
                .Replace("\r", "\n");   // Old Mac → Unix
            
            // 마지막 줄바꿈 제거
            if (normalizedText.EndsWith("\n"))
            {
                normalizedText = normalizedText.Substring(0, normalizedText.Length - 1);
            }
            
            string[] lines = normalizedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    string[] row = lines[i].Split('\t');
                    
                    // 최소 필요한 열 개수 확인 (5개)
                    if (row.Length < 5)
                    {
                        Debug.LogWarning($"라인 {i}: 열 개수 부족 ({row.Length}/5) - 건너뜀");
                        continue;
                    }
                    
                    // 각 셀의 데이터 정리
                    for (int j = 0; j < row.Length && j < 5; j++)
                    {
                        row[j] = row[j].Trim();
                    }
                    
                    CardEquipmentData newEquipment = new CardEquipmentData(
                        row[0], row[1], row[2], row[3], row[4]
                    );
                    
                    cardStatsList.Add(newEquipment);
                }
                catch (Exception e)
                {
                    Debug.LogError($"라인 {i} 파싱 오류: {e.Message}");
                }
            }
            
            Debug.Log($"장비 데이터 로드 완료: {cardStatsList.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"장비 데이터 읽기 오류: {e.Message}");
        }
        
        return cardStatsList;
    }
}

public class EquipmentDataManager : MonoBehaviour
{
    public List<CardEquipmentData> MyEquipmentsList;
    
    string filePath;
    string myEquips = "MyEquipments.txt";
    bool isSaving = false;
    
    void Start()
    {
        // InitializeDataDirectory(); // Start보다 Save가 먼저 호출되면 filepath가 만들어지지 않아서 오류가 생김. 그냥 save에서 경로 생성했음
        Load();
        InitializeCardList();
    }
    
    void InitializeDataDirectory()
    {
        try
        {
            string dataDir = Application.persistentDataPath + "/MyEquipmentsData";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
                Debug.Log($"장비 데이터 디렉토리 생성: {dataDir}");
            }
            
            filePath = Path.Combine(dataDir, myEquips);
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 디렉토리 초기화 오류: {e.Message}");
        }
    }
    
    void InitializeCardList()
    {
        try
        {
            var cardListComponent = GetComponent<CardList>();
            if (cardListComponent != null)
            {
                cardListComponent.InitCardList();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"카드 리스트 초기화 오류: {e.Message}");
        }
    }
    
    void Save()
    {
        if (isSaving) return;
        InitializeDataDirectory();
        
        try
        {
            isSaving = true;

            if (MyEquipmentsList == null)
            {
                MyEquipmentsList = new List<CardEquipmentData>();
            }

            string jsonData = JsonUtility.ToJson(new Serialization<CardEquipmentData>(MyEquipmentsList), true);
            File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);

            Debug.Log($"장비 데이터 저장 완료: {MyEquipmentsList.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"장비 데이터 저장 오류: {e.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }
    
    void Load()
    {
        try
        {
            // 로드할 파일이 없으면 빈 리스트로 초기화
            if (!File.Exists(filePath))
            {
                Debug.Log("저장된 장비 데이터가 없습니다. 빈 리스트로 시작합니다.");
                MyEquipmentsList = new List<CardEquipmentData>();
                return;
            }
            
            string jsonData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning("저장 파일이 비어있습니다. 빈 리스트로 초기화합니다.");
                MyEquipmentsList = new List<CardEquipmentData>();
                return;
            }
            
            var serializedData = JsonUtility.FromJson<Serialization<CardEquipmentData>>(jsonData);
            MyEquipmentsList = serializedData?.Data ?? new List<CardEquipmentData>();
            
            Debug.Log($"장비 데이터 로드 완료: {MyEquipmentsList.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"장비 데이터 로드 오류: {e.Message}. 빈 리스트로 초기화합니다.");
            MyEquipmentsList = new List<CardEquipmentData>();
        }
    }
    
    public List<CardEquipmentData> GetMyEquipmentsList()
    {
        if (MyEquipmentsList == null)
        {
            Debug.LogWarning("장비 리스트가 null입니다. 빈 리스트를 반환합니다.");
            MyEquipmentsList = new List<CardEquipmentData>();
        }
        return MyEquipmentsList;
    }
    
    // 아이디로 charCard를 찾아서 장비 데이터(My Equipment List) 업데이트
    // Card List의 Equip과 UnEquip이 호출.
    public void UpdateEquipment(CharCard charCard, int equipmentIndex)
    {
        try
        {
            if (charCard?.CardData == null)
            {
                Debug.LogError("UpdateEquipment: charCard 또는 CardData가 null입니다.");
                return;
            }
            
            if (equipmentIndex < 0 || equipmentIndex >= 4)
            {
                Debug.LogError($"UpdateEquipment: 잘못된 장비 인덱스 {equipmentIndex}");
                return;
            }
            
            if (MyEquipmentsList == null)
            {
                MyEquipmentsList = new List<CardEquipmentData>();
            }
            
            // 오리ID 비교
            CardEquipmentData charEquipData = MyEquipmentsList.Find(x => x.charID == charCard.CardData.ID);
            CardData charCardData = charCard.CardData;
            int[] equipmentCardID = new int[4];
            
            // 쓰기 편하게 equipmentCardID 에 아이디를 저장
            for (int i = 0; i < 4; i++)
            {
                if (charCard.equipmentCards != null && 
                    i < charCard.equipmentCards.Length && 
                    charCard.equipmentCards[i]?.CardData != null)
                {
                    equipmentCardID[i] = charCard.equipmentCards[i].CardData.ID;
                }
                else
                {
                    equipmentCardID[i] = 0; // 장비가 없는 경우
                }
            }
            
            // 해당 charCard의 장비 데이터가 존재하지 않으면 새로 생성해서 저장
            if (charEquipData == null)
            {
                CardEquipmentData newEquipData = new CardEquipmentData(
                    charCardData.ID.ToString(),
                    equipmentCardID[0].ToString(),
                    equipmentCardID[1].ToString(),
                    equipmentCardID[2].ToString(),
                    equipmentCardID[3].ToString()
                );
                
                MyEquipmentsList.Add(newEquipData);
                Debug.Log($"새 장비 데이터 생성: charID {charCardData.ID}");
            }
            // 해당 charCard의 장비 데이터가 존재한다면 바뀌는 장비만 교체하고 저장
            else
            {
                charEquipData.IDs[equipmentIndex] = equipmentCardID[equipmentIndex];
                Debug.Log($"장비 데이터 업데이트: charID {charCardData.ID}, 슬롯 {equipmentIndex}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateEquipment 오류: {e.Message}");
        }
    }

    public void ImmediateSave()
    {
        Save();
    }

    public void DelayedSave()
    {
        if (!isSaving)
        {
            StartCoroutine(WaitToStartMethodRunning());
            Debug.Log("장비 데이터 지연 저장 시작");
        }
    }
    
    IEnumerator WaitToStartMethodRunning()
    {
        yield return new WaitForSeconds(0.04f);
        Debug.Log("장비 데이터 지연 저장 실행");
        Save();
    }
    
    // 특정 캐릭터의 장비 데이터 제거
    public void RemoveEquipmentData(int charID)
    {
        try
        {
            if (MyEquipmentsList == null) return;
            
            CardEquipmentData equipData = MyEquipmentsList.Find(x => x.charID == charID);
            if (equipData != null)
            {
                MyEquipmentsList.Remove(equipData);
                Save();
                Debug.Log($"장비 데이터 제거: charID {charID}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RemoveEquipmentData 오류: {e.Message}");
        }
    }
    
    // 모든 장비 데이터 초기화
    public void ClearAllEquipmentData()
    {
        try
        {
            if (MyEquipmentsList == null)
                MyEquipmentsList = new List<CardEquipmentData>();
            else
                MyEquipmentsList.Clear();
            
            Save();
            Debug.Log("모든 장비 데이터 초기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"ClearAllEquipmentData 오류: {e.Message}");
        }
    }
}