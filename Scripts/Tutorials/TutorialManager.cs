using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialData
{
	public string Name;
    public GameObject tutorialObject;
    public bool hasShown;
}

public class TutorialManager : MonoBehaviour
{
	public static TutorialManager instance;
	[SerializeField] Transform tutorialParent;
	[SerializeField] List<TutorialData> tutorials = new List<TutorialData>();

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);

		
	}
	void Start()
	{
		// PlayerPrefs.DeleteAll(); // 테스트 용
		LoadTutorialState();
		StartCoroutine(PauseeWithDelay());
    }
	IEnumerator PauseeWithDelay()
	{
		yield return null;
		ActivateTutorial("Move");
	}

    /// <summary>
	/// tutorial type을 인자로 넘겨줌
	/// </summary>
	/// <param name="type"></param>
	public void ActivateTutorial(string _name)
	{
		var tutorial = tutorials.Find(t => t.Name == _name);
		if (tutorial.hasShown) return;
		ShowTutorial(tutorial);
	}

	void ShowTutorial(TutorialData t)
	{
		var obj = Instantiate(t.tutorialObject, tutorialParent);
		obj.SetActive(true);
		t.hasShown = true;
		SaveTutorialState();
		Debug.Log("Show Tuto");
	}

	// 저장,불러오기
	void SaveTutorialState()
	{
		foreach (var t in tutorials)
		{
			PlayerPrefs.SetInt(t.Name, t.hasShown ? 1 : 0);
		}
		PlayerPrefs.Save();
	}

	void LoadTutorialState()
	{
		foreach (var t in tutorials)
		{
			t.hasShown = PlayerPrefs.GetInt(t.Name, 0) == 1;
		}
	}

	#region 디버깅
	public void ResetTutorialState()
	{
		foreach (var item in tutorials)
		{
			item.hasShown = false;
		}
	}
	#endregion
}