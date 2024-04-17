using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class TipScreen : global::Menu.Menu
{
	public SimpleButton continueButton;

	public MenuLabel test;

	public int counter;

	public int tipID;

	public SlugcatStats.Name slugcat;

	public int tipSeed;

	public FSprite tipImage;

	public string loadedAtlas;

	public int frameCount;

	public int cycleJump;

	public float ContinueAndExitButtonsXPos => manager.rainWorld.options.ScreenSize.x + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;

	public TipScreen(ProcessManager manager)
		: base(manager, MMFEnums.ProcessID.Tips)
	{
		pages.Add(new Page(this, null, "main", 0));
		test = new MenuLabel(this, pages[0], Translate(""), new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, manager.rainWorld.options.ScreenSize.y * 0.5f - 50f), new Vector2(0f, 0f), bigText: true);
		test.label.alignment = FLabelAlignment.Center;
		pages[0].subObjects.Add(test);
		continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(ContinueAndExitButtonsXPos - 180f, 15f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(continueButton);
		pages[0].lastSelectedObject = continueButton;
		backObject = continueButton;
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if (counter > 120)
		{
			continueButton.buttonBehav.greyedOut = false;
		}
		if (tipImage != null && loadedAtlas != "tipImage" && counter % 3 == 0)
		{
			frameCount++;
			string elementName = "tipframe" + frameCount;
			if (frameCount < 10)
			{
				elementName = "tipframe00" + frameCount;
			}
			else if (frameCount < 100)
			{
				elementName = "tipframe0" + frameCount;
			}
			if (!Futile.atlasManager.DoesContainElementWithName(elementName))
			{
				frameCount = 1;
			}
		}
		manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Mathf.InverseLerp(15f, 54f, counter);
		if (num > 0f)
		{
			num = UnityEngine.Random.Range(num, Mathf.Min(1f, num + 0.45f));
		}
		test.label.alpha = num;
		if (tipImage != null)
		{
			tipImage.alpha = Mathf.Pow(num, 6f);
			if (loadedAtlas != "tipImage")
			{
				string elementName = "tipframe" + frameCount;
				if (frameCount < 10)
				{
					elementName = "tipframe00" + frameCount;
				}
				else if (frameCount < 100)
				{
					elementName = "tipframe0" + frameCount;
				}
				if (Futile.atlasManager.DoesContainElementWithName(elementName))
				{
					tipImage.element = Futile.atlasManager.GetElementWithName(elementName);
				}
			}
			float num2 = test.label.textRect.height + 32f + tipImage.element.sourceSize.y;
			test.pos.y = manager.rainWorld.options.ScreenSize.y * 0.5f - num2 * 0.5f + test.label.textRect.height * 0.5f;
			tipImage.x = test.pos.x - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			tipImage.y = manager.rainWorld.options.ScreenSize.y * 0.5f + num2 * 0.5f - tipImage.element.sourceSize.y * 0.5f;
		}
		else
		{
			test.pos.y = manager.rainWorld.options.ScreenSize.y * 0.5f;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			PlaySound(SoundID.MENU_Continue_From_Sleep_Death_Screen);
		}
	}

	public void InitializeTip()
	{
		counter = 0;
		continueButton.buttonBehav.greyedOut = true;
		if (tipImage != null)
		{
			tipImage.RemoveFromContainer();
			tipImage = null;
			Futile.atlasManager.UnloadAtlas(loadedAtlas);
		}
		frameCount = 1;
		string characterTipMeta = GetCharacterTipMeta(slugcat);
		if (characterTipMeta == "")
		{
			return;
		}
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("Tips" + Path.DirectorySeparatorChar + "tips.txt"));
		_ = UnityEngine.Random.state;
		UnityEngine.Random.InitState(tipSeed);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] array2 = Regex.Split(characterTipMeta, "-");
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = Regex.Split(array2[i], ",");
			List<string>[] array4 = new List<string>[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = new List<string>();
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].Length <= 0 || array[k][0] == '/' || !array[k].Contains("~"))
					{
						continue;
					}
					string[] array5 = Regex.Split(array[k], "~");
					if (array5[0] == array3[j])
					{
						if (array5.Length > 2)
						{
							array4[j].Add(array5[1] + "~" + array5[array5.Length - 1]);
						}
						else
						{
							array4[j].Add(array5[array5.Length - 1]);
						}
					}
				}
				array4[j].Sort((string a, string b) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
			}
			bool flag = false;
			while (!flag)
			{
				flag = true;
				for (int l = 0; l < array3.Length; l++)
				{
					if (array4[l].Count > 0)
					{
						flag = false;
						string text = array4[l].Pop();
						if (text.Contains("~"))
						{
							string[] array6 = Regex.Split(text, "~");
							list2.Add(array6[0]);
							list.Add(array6[1]);
						}
						else
						{
							list2.Add("");
							list.Add(text);
						}
					}
				}
			}
		}
		if (tipID >= list.Count)
		{
			return;
		}
		test.label.text = Translate(list[tipID]).Replace("<LINE>", Environment.NewLine);
		if (list2[tipID] != "")
		{
			if (list2[tipID].Contains(".png"))
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				string text2 = AssetManager.ResolveFilePath("Tips" + Path.DirectorySeparatorChar + list2[tipID]);
				AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text2, clampWrapMode: false, crispPixels: false);
				loadedAtlas = "tipImage";
				Futile.atlasManager.LoadAtlasFromTexture(loadedAtlas, texture2D, textureFromAsset: false);
				tipImage = new FSprite(loadedAtlas);
				pages[0].Container.AddChild(tipImage);
			}
			else
			{
				loadedAtlas = "Tips/" + list2[tipID];
				Futile.atlasManager.LoadAtlas(loadedAtlas);
				tipImage = new FSprite("tipframe001");
				pages[0].Container.AddChild(tipImage);
			}
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		if (nextProcess is RainWorldGame)
		{
			RainWorldGame rainWorldGame = nextProcess as RainWorldGame;
			if (rainWorldGame.world != null && rainWorldGame.world.rainCycle != null && rainWorldGame.world.rainCycle.preTimer == 0)
			{
				(nextProcess as RainWorldGame).world.rainCycle.timer = cycleJump;
			}
			if (rainWorldGame.IsStorySession && rainWorldGame.GetStorySession.saveState != null)
			{
				rainWorldGame.GetStorySession.saveState.deathPersistentSaveData.tipCounter++;
				manager.rainWorld.progression.SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: false, saveAsIfPlayerQuit: false);
			}
		}
	}

	public static bool AnyTipsAvailable(SlugcatStats.Name slugcat, int tipID)
	{
		string characterTipMeta = GetCharacterTipMeta(slugcat);
		if (characterTipMeta == "")
		{
			return false;
		}
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("Tips" + Path.DirectorySeparatorChar + "tips.txt"));
		int num = 0;
		string[] array2 = Regex.Split(characterTipMeta, "-");
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = Regex.Split(array2[i], ",");
			for (int j = 0; j < array3.Length; j++)
			{
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].Length > 0 && array[k][0] != '/' && array[k].Contains("~") && Regex.Split(array[k], "~")[0] == array3[j])
					{
						num++;
					}
				}
			}
		}
		return tipID < num;
	}

	public static string GetCharacterTipMeta(SlugcatStats.Name slugcat)
	{
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("Tips" + Path.DirectorySeparatorChar + "character_usage.txt"));
		string result = "";
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length > 0 && array[i].Contains("~"))
			{
				string[] array2 = Regex.Split(array[i], "~");
				if (array2[0] == slugcat.ToString())
				{
					result = array2[array2.Length - 1];
					break;
				}
			}
		}
		return result;
	}

	public static int GetCharacterTipFrequency(SlugcatStats.Name slugcat)
	{
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("Tips" + Path.DirectorySeparatorChar + "character_usage.txt"));
		int result = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length > 0 && array[i].Contains("~"))
			{
				string[] array2 = Regex.Split(array[i], "~");
				if (array2[0] == slugcat.ToString())
				{
					result = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				}
			}
		}
		return result;
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (tipImage != null)
		{
			tipImage.RemoveFromContainer();
			Futile.atlasManager.UnloadAtlas(loadedAtlas);
		}
	}

	public void GetDataFromGame(SlugcatStats.Name slugcatID, int tipID, int tipSeed, int cycleJump)
	{
		slugcat = slugcatID;
		this.tipSeed = tipSeed;
		this.tipID = tipID;
		this.cycleJump = cycleJump;
		InitializeTip();
	}
}
