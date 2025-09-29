using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class Serialization<T>
{
    public Serialization(List<T> _target) => Data = _target;
    public List<T> Data;
}

[System.Serializable]
public class CardData
{
    public CardData(string _id, string _Type, string _Grade, string _EvoStage, 
        string _Name, string _level, string _hp, string _atk, string _equipmentType, 
        string _essectialEquip, string _bindingTo, string _startingMember, 
        string _defaultItem, string _passiveSkill)
    {
        // ID 파싱 (안전하게)
        if (int.TryParse(_id?.Trim(), out int intID))
        {
            ID = intID;
        }
        else
        {
            // pool의 아이디 셀이 비어있음. 나중에 랜덤한 아이디를 부여받게 되므로 에러는 아님.
            // 빈 칸을 파싱 실패로 인식하지 않도록 따로 조건문을 추가했음
            if (string.IsNullOrWhiteSpace(_id))
            {
                ID = -1; // 임시 값, 나중에 교체됨
            }
            else
            {
                Debug.LogWarning($"ID 파싱 실패: '{_id}' - 기본값 -1 사용");
                ID = -1;
            }
        }

        Type = _Type?.Trim() ?? "";
        Name = _Name?.Trim() ?? "";
        EquipmentType = _equipmentType?.Trim() ?? "";
        EssentialEquip = _essectialEquip?.Trim() ?? "";
        BindingTo = _bindingTo?.Trim() ?? "";
        StartingMember = _startingMember?.Trim() ?? "";
        DefaultItem = _defaultItem?.Trim() ?? "";
        
        // 숫자 필드들 안전하게 파싱
        Grade = SafeParseInt(_Grade, "Grade");
        EvoStage = SafeParseInt(_EvoStage, "EvoStage");
        Level = SafeParseInt(_level, "Level");
        Hp = SafeParseInt(_hp, "HP");
        Atk = SafeParseInt(_atk, "ATK");
        PassiveSkill = SafeParseInt(_passiveSkill, "PassiveSkill");
    }
    
    // filedName은 디버깅을 위한 문자열
    private int SafeParseInt(string value, string fieldName)
    {
        string cleanValue = value?.Trim(); // 입력 문자열 앞뒤 공백 제거 (null도 안전하게 처리)
        if (int.TryParse(cleanValue, out int result)) // 정수 변환 시도
        {
            return result; // 변환 성공 → 정수 반환
        }
        else
        {
            Debug.LogWarning($"{fieldName} 파싱 실패: '{value}' - 기본값 0 사용");
            return 0; // 변환 실패 → 0 반환
        }
    }

    public string Type, Name, 
            EquipmentType, EssentialEquip, BindingTo,
            StartingMember, DefaultItem;
    public int ID, Grade, EvoStage, Level, Hp, Atk, PassiveSkill;
}

public class ReadCardData
{
    public List<CardData> cardList;
    
    public List<CardData> GetCardsList(TextAsset cardDataText)
    {
        cardList = new List<CardData>();
        
        try
        {
            if (cardDataText == null || string.IsNullOrEmpty(cardDataText.text))
            {
                Debug.LogError("카드 데이터 파일이 비어있습니다.");
                return cardList;
            }
            
            // 크로스 플랫폼 텍스트 정규화
            string normalizedText = cardDataText.text
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
                    
                    // 최소 필요한 열 개수 확인 (14개)
                    if (row.Length < 14)
                    {
                        Debug.LogWarning($"라인 {i}: 열 개수 부족 ({row.Length}/14) - 건너뜀");
                        continue;
                    }
                    
                    // 각 셀의 데이터 정리
                    for (int j = 0; j < row.Length; j++)
                    {
                        row[j] = row[j].Trim();
                    }
                    
                    CardData newCard = new CardData(
                        row[0], row[1], row[2], row[3], row[4], 
                        row[5], row[6], row[7], row[8], row[9], 
                        row[10], row[11], row[12], row[13]
                    );
                    
                    cardList.Add(newCard);
                }
                catch (Exception e)
                {
                    Debug.LogError($"라인 {i} 파싱 오류: {e.Message}");
                }
            }
            
            Debug.Log($"카드 데이터 로드 완료: {cardList.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"카드 데이터 읽기 오류: {e.Message}");
        }
        
        return cardList;
    }
}

public class CardDataManager : MonoBehaviour
{
    public TextAsset CardDatabase;
    public TextAsset WeaponCardDatabase;
    public TextAsset ItemCardDatabase;
    public TextAsset startingCardData;
    public List<CardData> AllCardsList, MyCardsList;
    
    string filePath;
    string myCards = "MyCards.txt";
    bool isInitializing = false;

    void Start()
    {
        InitializeDataDirectory();
        Debug.Log(Application.persistentDataPath);
        Load();
    }
    
    void InitializeDataDirectory()
    {
        try
        {
            string dataDir = Application.persistentDataPath + "/PlayerData";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
                Debug.Log($"플레이어 데이터 디렉토리 생성: {dataDir}");
            }
            
            filePath = Path.Combine(dataDir, myCards);
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 디렉토리 초기화 오류: {e.Message}");
        }
    }

    void Save()
    {
        if (isInitializing) return;
        
        try
        {
            isInitializing = true;
            
            if (MyCardsList == null)
            {
                MyCardsList = new List<CardData>();
            }
            
            string jsonData = JsonUtility.ToJson(new Serialization<CardData>(MyCardsList), true);
            File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);
            
            Debug.Log($"카드 데이터 저장 완료: {MyCardsList.Count}개");
            
            var cardListComponent = GetComponent<CardList>();
            if (cardListComponent != null)
            {
                cardListComponent.InitCardList();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"카드 데이터 저장 오류: {e.Message}");
        }
        finally
        {
            isInitializing = false;
        }
    }

    void Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.Log("저장된 카드 데이터가 없습니다. 초기 데이터로 시작합니다.");
                ResetCards();
                return;
            }
            
            string jsonData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning("저장 파일이 비어있습니다. 초기화합니다.");
                ResetCards();
                return;
            }
            
            var serializedData = JsonUtility.FromJson<Serialization<CardData>>(jsonData);
            MyCardsList = serializedData?.Data ?? new List<CardData>();
            
            Debug.Log($"카드 데이터 로드 완료: {MyCardsList.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"카드 데이터 로드 오류: {e.Message}. 초기화합니다.");
            ResetCards();
        }
    }

    void ResetCards()
    {
        try
        {
            Debug.Log("카드 데이터 초기화");
            
            if (MyCardsList == null)
                MyCardsList = new List<CardData>();
            else
                MyCardsList.Clear();
            
            if (startingCardData != null)
            {
                Debug.Log("시작 카드를 로드합니다");
                List<CardData> startingCards = new ReadCardData().GetCardsList(startingCardData);
                
                if (startingCards.Count > 0)
                {
                    AddNewCardToMyCardsList(startingCards[0]);
                    startingCards[0].StartingMember = StartingMember.Zero.ToString();
                    
                    var gachaSys = FindObjectOfType<GachaSystem>();
                    if (gachaSys != null)
                    {
                        gachaSys.AddEssentialEquip(startingCards[0]);
                        // gachaSys.DelayedSaveEquipmentData();
                        gachaSys.ImmediateSaveEquipmentData();
                    }
                }
            }
            
            Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"카드 데이터 초기화 오류: {e.Message}");
        }
    }

    public List<CardData> GetMyCardList()
    {
        if (MyCardsList == null)
        {
            Debug.LogWarning("내 카드 리스트가 null입니다. 빈 리스트를 반환합니다.");
            MyCardsList = new List<CardData>();
        }
        return MyCardsList;
    }
    
    public List<CardData> GetAllCardList()
    {
        if (AllCardsList == null) 
            AllCardsList = new List<CardData>();
            
        if (AllCardsList.Count == 0 && CardDatabase != null)
        {
            AllCardsList = new ReadCardData().GetCardsList(CardDatabase);
        }
        
        Debug.Log($"전체 카드 리스트 개수: {AllCardsList.Count}");
        return AllCardsList;
    }

    public void RemoveCardFromMyCardList(CardData cardToRemove)
    {
        if (cardToRemove == null || MyCardsList == null) return;
        
        int mID = cardToRemove.ID;
        int indexToRemove = -1;

        for (int i = 0; i < MyCardsList.Count; i++)
        {
            if (MyCardsList[i].ID == mID)
            {
                indexToRemove = i;
                break;
            }
        }

        if(indexToRemove != -1)
        {
            MyCardsList.RemoveAt(indexToRemove);
            Save();
            Debug.Log($"카드 제거 완료: ID {mID}");
        }
    }

    public void AddNewCardToMyCardsList(CardData _cardData)
    {
        if (_cardData == null) return;
        
        if (MyCardsList == null)
            MyCardsList = new List<CardData>();
        
        _cardData.ID = GenerateRandomId();
        AddRandomSkill(_cardData);
        
        MyCardsList.Add(_cardData);
        Save();
    }

    // AddNewCardToMyCardsList와 동일. 디버그 용으로 특정 스킬을 지정해서 카드를 뽑을 때 사용하는 메서드
    // 이미 카드 데이터에 Skill을 가지고 이 메서드로 오게 되므로 Add Random Skill이 없음
    public void AddNewCardToMyCardsListWithSkill(CardData _cardData)
    {
        if (_cardData == null) return;

        if (MyCardsList == null)
            MyCardsList = new List<CardData>();

        _cardData.ID = GenerateRandomId();
        Debug.LogError($"카드 데이터 메니져. Skill = {_cardData.PassiveSkill}");
        MyCardsList.Add(_cardData);
        Save();
    }
    
    public void AddUpgradedCardToMyCardList(CardData _cardData)
    {
        if (_cardData == null) return;

        if (MyCardsList == null)
            MyCardsList = new List<CardData>();

        MyCardsList.Add(_cardData);
        Save();
    }
    
    void AddRandomSkill(CardData _cardData)
    {
        int skill = UnityEngine.Random.Range(1, StaticValues.MaxSkillNumbers + 1);
        _cardData.PassiveSkill = skill;
    }
    
    public void UpgradeCardData(CardData _cardData, int _level, int _hp, int _atk)
    {
        if (_cardData == null) return;
        
        _cardData.Level = _level;
        _cardData.Hp = _hp;
        _cardData.Atk = _atk;
        Save();
    }
    
    public void UpdateStartingmemberOfCard(CardData cardToUpdate, string orderIndex)
    {
        if (cardToUpdate == null) return;
        
        cardToUpdate.StartingMember = orderIndex ?? "";
        Save();
    }
    
    public void DeleteData()
    {
        ResetCards();
    }

    int GenerateRandomId()
    {
        int randomId;
        int attempts = 0;
        const int maxAttempts = 1000;
        
        do
        {
            randomId = UnityEngine.Random.Range(1, 10000);
            attempts++;
            
            if (attempts > maxAttempts)
            {
                Debug.LogError("고유 ID 생성 실패. 시스템 시간 기반 ID 사용.");
                return (int)(System.DateTime.Now.Ticks % 10000);
            }
        } while (ItemIdExists(randomId));

        return randomId;
    }

    bool ItemIdExists(int id)
    {
        if (MyCardsList == null) return false;
        
        for (int i = 0; i < MyCardsList.Count; i++)
        {
            if (MyCardsList[i]?.ID == id)
            {
                return true;
            }
        }
        return false;
    }

    #region Debug
    public void SetAllToMaxLevel()
    {
        List<CardData> cards = GetMyCardList();
        if (cards == null) return;
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].Level = StaticValues.MaxLevel;
            }
        }
        Save();
        Debug.Log("모든 카드 최대 레벨로 설정 완료");
    }
    #endregion
}