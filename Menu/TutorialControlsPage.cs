using RWCustom;
using UnityEngine;

namespace Menu;

public class TutorialControlsPage : Menu
{
	public RainWorldGame game;

	public FSprite blackSprite;

	public float blackFade;

	public float lastBlackFade;

	public ControlMap controlMap;

	public int counter;

	public bool wantToContinue;

	public TutorialControlsPageOwner inGameObject;

	public TutorialControlsPage(ProcessManager manager, RainWorldGame game, TutorialControlsPageOwner inGameObject, FContainer hudContainer)
		: base(manager, ProcessManager.ProcessID.TutorialControlsPage)
	{
		this.game = game;
		this.inGameObject = inGameObject;
		pages.Add(new Page(this, null, "main", 0));
		pages[0].Container = new FContainer();
		hudContainer.AddChild(pages[0].Container);
		blackSprite = new FSprite("pixel");
		blackSprite.color = Menu.MenuRGB(MenuColors.Black);
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = manager.rainWorld.options.ScreenSize.x / 2f;
		blackSprite.y = manager.rainWorld.options.ScreenSize.y / 2f;
		blackSprite.alpha = 0.5f;
		pages[0].Container.AddChild(blackSprite);
		Options.ControlSetup.Preset activePreset = manager.rainWorld.options.controls[0].GetActivePreset();
		float num = 0f;
		num = 450f;
		if (activePreset != Options.ControlSetup.Preset.None)
		{
			controlMap = new ControlMap(this, pages[0], new Vector2(manager.rainWorld.screenSize.x / 2f + (1366f - manager.rainWorld.screenSize.x) / 2f, num), activePreset, showPickupInstructions: false);
			pages[0].subObjects.Add(controlMap);
		}
	}

	public override void Update()
	{
		counter++;
		lastBlackFade = blackFade;
		if (wantToContinue)
		{
			blackFade = Mathf.Max(0f, blackFade - 0.125f);
			if (blackFade <= 0f)
			{
				ShutDownProcess();
				inGameObject.Destroy();
			}
		}
		else
		{
			blackFade = Mathf.Min(1f, blackFade + 0.0625f);
		}
		base.Update();
	}

	public override void GrafUpdate(float timeStacker)
	{
		blackSprite.alpha = Custom.SCurve(Mathf.Lerp(lastBlackFade, blackFade, timeStacker), 0.6f) * 0.25f;
		base.GrafUpdate(timeStacker);
	}

	public override void ShutDownProcess()
	{
		blackSprite.RemoveFromContainer();
		base.ShutDownProcess();
	}
}
