using UnityEngine;

public abstract class TutorialBase : MonoBehaviour
{

	protected TutorialManager tutorialManager;
	protected Transform tutorialPanel;

	public virtual void ActivateTutorial()
	{
		if (tutorialManager == null) tutorialManager = FindObjectOfType<TutorialManager>();
		if (tutorialPanel == null) tutorialPanel = tutorialManager.transform;
	}
}