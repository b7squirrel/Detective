using UnityEngine;

public class TutorialFadeEffect : TutorialBase
{
	[SerializeField]
	FadeEffect fadeEffect;
	[SerializeField]
	bool isFadeIn = false;
	bool isCompleted = false;

	public override void Enter()
	{
		if (isFadeIn == true)
		{
			fadeEffect.FadeIn(OnAfterFadeEffect);
		}
		else
		{
			fadeEffect.FadeOut(OnAfterFadeEffect);
		}
	}

	void OnAfterFadeEffect()
	{
		isCompleted = true;
	}

	public override void Execute(TutorialController controller)
	{
		if (isCompleted == true)
		{
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}

