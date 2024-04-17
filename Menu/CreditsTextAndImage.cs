using System.Collections.Generic;
using System.IO;
using System.Text;
using RWCustom;
using UnityEngine;

namespace Menu;

public class CreditsTextAndImage : CreditsObject
{
	public List<FSprite> specials;

	public List<string> specialNames;

	public List<float> specialPos;

	public float slowDownPos = 500f;

	public override bool OutOfScreen
	{
		get
		{
			for (int i = 0; i < subObjects.Count; i++)
			{
				if (subObjects[i] is RectangularMenuObject && LowestPoint(subObjects[i] as RectangularMenuObject) < 800f)
				{
					return false;
				}
			}
			return true;
		}
	}

	public override bool BeforeScreen => scroll <= -100f;

	public override float CurrentDefaultScrollSpeed => Custom.LerpMap(Mathf.Abs(slowDownPos - scroll), slowDownZone, slowDownZone + 100f, slowDownScrollSpeed, defaulScrollSpeed, 0.6f);

	private float LowestPoint(RectangularMenuObject testObj)
	{
		if (testObj is MenuIllustration && (testObj as MenuIllustration).anchorCenter)
		{
			return testObj.DrawY(1f) - testObj.size.y / 2f;
		}
		return testObj.DrawY(1f);
	}

	public CreditsTextAndImage(Menu menu, MenuObject owner, EndCredits.Stage stage, bool startFromBottom)
		: base(menu, owner, stage, startFromBottom)
	{
		string text = "";
		specials = new List<FSprite>();
		specialNames = new List<string>();
		specialPos = new List<float>();
		if (stage == EndCredits.Stage.RainWorldLogo)
		{
			if (menu.manager.rainWorld.dlcVersion == 0)
			{
				subObjects.Add(new MenuIllustration(menu, this, "", "MainTitle", new Vector2(683f, 384f), crispPixels: true, anchorCenter: true));
			}
			else
			{
				subObjects.Add(new MenuIllustration(menu, this, "", "MainTitleDownpour", new Vector2(683f, 384f), crispPixels: true, anchorCenter: true));
			}
			(subObjects[0] as MenuIllustration).alpha = 0f;
			scroll = 0f;
			lastScroll = 0f;
			slowDownPos = 0f;
			defaulScrollSpeed = 4f;
			slowDownScrollSpeed = 4f;
		}
		else if (stage == EndCredits.Stage.VideoCult)
		{
			text = "01 - VIDEOCULT";
			defaulScrollSpeed = (ModManager.MMF ? 1.75f : 2f);
			slowDownScrollSpeed = (ModManager.MMF ? 1f : 2f);
		}
		else if (stage == EndCredits.Stage.AdultSwimGames)
		{
			text = "02 - ADULT SWIM GAMES";
			defaulScrollSpeed = (ModManager.MMF ? 2.5f : 3f);
			slowDownScrollSpeed = (ModManager.MMF ? 1.5f : 2f);
		}
		else if (stage == EndCredits.Stage.Akupara)
		{
			text = "03 - AKUPARA";
			defaulScrollSpeed = (ModManager.MMF ? 2.5f : 3f);
			slowDownScrollSpeed = (ModManager.MMF ? 1.5f : 2f);
		}
		else if (stage == EndCredits.Stage.CarbonGames)
		{
			text = "03.5 - CARBON GAMES";
			defaulScrollSpeed = (ModManager.MMF ? 2.5f : 3f);
			slowDownScrollSpeed = (ModManager.MMF ? 1.5f : 2f);
		}
		else if (stage == EndCredits.Stage.Downpour)
		{
			text = "DOWNPOUR";
			defaulScrollSpeed = (ModManager.MMF ? 1.75f : 2f);
			slowDownScrollSpeed = (ModManager.MMF ? 1f : 2f);
		}
		else if (stage == EndCredits.Stage.SpecialThanks)
		{
			text = "04 - SPECIAL THANKS";
			defaulScrollSpeed = (ModManager.MMF ? 1.5f : 3f);
			slowDownScrollSpeed = (ModManager.MMF ? 1f : 2f);
		}
		else if (stage == EndCredits.Stage.BetaTesters)
		{
			text = "07 - BETA TESTERS";
			defaulScrollSpeed = (ModManager.MMF ? 2.5f : 3f);
			slowDownScrollSpeed = (ModManager.MMF ? 1.5f : 2f);
		}
		else if (stage == EndCredits.Stage.MoreSlugcats)
		{
			text = "MORE SLUGCATS";
			defaulScrollSpeed = (ModManager.MMF ? 1.75f : 2f);
			slowDownScrollSpeed = (ModManager.MMF ? 1f : 2f);
		}
		else if (stage == EndCredits.Stage.MoreSlugcatsThanks)
		{
			text = "MSC SPECIAL THANKS";
			defaulScrollSpeed = (ModManager.MMF ? 1.25f : 2f);
			slowDownScrollSpeed = (ModManager.MMF ? 0.75f : 2f);
		}
		float num = 0f;
		if (text != null)
		{
			string path = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "Credits" + Path.DirectorySeparatorChar + text + ".txt");
			string[] array = ((!File.Exists(path)) ? new string[0] : File.ReadAllLines(path, Encoding.UTF8));
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Replace("<LINE>", "\r\n");
				if (array[i].StartsWith("<@>"))
				{
					ImportSpecialSprite(array[i].Substring(3));
					FSprite fSprite = specials[specials.Count - 1];
					fSprite.x = 683.2f - (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f;
					fSprite.y = (float)(-i) * 40f + 10.2f;
					specialPos.Add(fSprite.y);
				}
				else
				{
					subObjects.Add(new MenuLabel(menu, this, array[i], new Vector2(433f, (float)(-i) * 40f), new Vector2(500f, 30f), bigText: false));
					(subObjects[subObjects.Count - 1] as MenuLabel).label.alignment = FLabelAlignment.Center;
					(subObjects[subObjects.Count - 1] as MenuLabel).label.x = -1000f;
				}
			}
			num = (float)array.Length * 40f + 800f;
		}
		scroll = (startFromBottom ? (num - 50f) : (-100f));
		lastScroll = scroll;
		pos.y = scroll;
		lastPos.y = pos.y;
	}

	public override void Update()
	{
		base.Update();
		pos.y = scroll;
		if (stage == EndCredits.Stage.RainWorldLogo)
		{
			if (age < 80)
			{
				pos.y = 0f;
				scroll = 0f;
				(menu as EndCredits).scrollSpeed = 0f;
			}
			(subObjects[0] as MenuIllustration).alpha = Custom.SCurve(Mathf.InverseLerp(0f, 60f, age), 0.65f);
		}
		for (int i = 0; i < specials.Count; i++)
		{
			specials[i].y = specialPos[i] + scroll;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < specials.Count; i++)
		{
			specials[i].RemoveFromContainer();
			Futile.atlasManager.UnloadAtlas("spec" + specialNames[i]);
		}
	}

	public void ImportSpecialSprite(string name)
	{
		if (Futile.atlasManager.GetAtlasWithName("spec" + name) == null)
		{
			string text = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "Credits" + Path.DirectorySeparatorChar + name + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
			Futile.atlasManager.LoadAtlasFromTexture("spec" + name, texture2D, textureFromAsset: false);
			specials.Add(new FSprite("spec" + name));
			specialNames.Add(name);
			Futile.stage.AddChild(specials[specials.Count - 1]);
		}
	}
}
