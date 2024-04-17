using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class MenuScene : MenuObject
{
	public class SceneID : ExtEnum<SceneID>
	{
		public static readonly SceneID Empty = new SceneID("Empty", register: true);

		public static readonly SceneID MainMenu = new SceneID("MainMenu", register: true);

		public static readonly SceneID MainMenu_Downpour = new SceneID("MainMenu_Downpour", register: true);

		public static readonly SceneID SleepScreen = new SceneID("SleepScreen", register: true);

		public static readonly SceneID RedsDeathStatisticsBkg = new SceneID("RedsDeathStatisticsBkg", register: true);

		public static readonly SceneID NewDeath = new SceneID("NewDeath", register: true);

		public static readonly SceneID StarveScreen = new SceneID("StarveScreen", register: true);

		public static readonly SceneID Intro_1_Tree = new SceneID("Intro_1_Tree", register: true);

		public static readonly SceneID Intro_2_Branch = new SceneID("Intro_2_Branch", register: true);

		public static readonly SceneID Intro_3_In_Tree = new SceneID("Intro_3_In_Tree", register: true);

		public static readonly SceneID Intro_4_Walking = new SceneID("Intro_4_Walking", register: true);

		public static readonly SceneID Intro_5_Hunting = new SceneID("Intro_5_Hunting", register: true);

		public static readonly SceneID Intro_6_7_Rain_Drop = new SceneID("Intro_6_7_Rain_Drop", register: true);

		public static readonly SceneID Intro_8_Climbing = new SceneID("Intro_8_Climbing", register: true);

		public static readonly SceneID Intro_9_Rainy_Climb = new SceneID("Intro_9_Rainy_Climb", register: true);

		public static readonly SceneID Intro_10_Fall = new SceneID("Intro_10_Fall", register: true);

		public static readonly SceneID Intro_10_5_Separation = new SceneID("Intro_10_5_Separation", register: true);

		public static readonly SceneID Intro_11_Drowning = new SceneID("Intro_11_Drowning", register: true);

		public static readonly SceneID Intro_12_Waking = new SceneID("Intro_12_Waking", register: true);

		public static readonly SceneID Intro_13_Alone = new SceneID("Intro_13_Alone", register: true);

		public static readonly SceneID Intro_14_Title = new SceneID("Intro_14_Title", register: true);

		public static readonly SceneID Endgame_Survivor = new SceneID("Endgame_Survivor", register: true);

		public static readonly SceneID Endgame_Hunter = new SceneID("Endgame_Hunter", register: true);

		public static readonly SceneID Endgame_Saint = new SceneID("Endgame_Saint", register: true);

		public static readonly SceneID Endgame_Traveller = new SceneID("Endgame_Traveller", register: true);

		public static readonly SceneID Endgame_Chieftain = new SceneID("Endgame_Chieftain", register: true);

		public static readonly SceneID Endgame_Monk = new SceneID("Endgame_Monk", register: true);

		public static readonly SceneID Endgame_Outlaw = new SceneID("Endgame_Outlaw", register: true);

		public static readonly SceneID Endgame_DragonSlayer = new SceneID("Endgame_DragonSlayer", register: true);

		public static readonly SceneID Endgame_Martyr = new SceneID("Endgame_Martyr", register: true);

		public static readonly SceneID Endgame_Scholar = new SceneID("Endgame_Scholar", register: true);

		public static readonly SceneID Endgame_Mother = new SceneID("Endgame_Mother", register: true);

		public static readonly SceneID Endgame_Friend = new SceneID("Endgame_Friend", register: true);

		public static readonly SceneID Landscape_CC = new SceneID("Landscape_CC", register: true);

		public static readonly SceneID Landscape_DS = new SceneID("Landscape_DS", register: true);

		public static readonly SceneID Landscape_GW = new SceneID("Landscape_GW", register: true);

		public static readonly SceneID Landscape_HI = new SceneID("Landscape_HI", register: true);

		public static readonly SceneID Landscape_LF = new SceneID("Landscape_LF", register: true);

		public static readonly SceneID Landscape_SB = new SceneID("Landscape_SB", register: true);

		public static readonly SceneID Landscape_SH = new SceneID("Landscape_SH", register: true);

		public static readonly SceneID Landscape_SI = new SceneID("Landscape_SI", register: true);

		public static readonly SceneID Landscape_SL = new SceneID("Landscape_SL", register: true);

		public static readonly SceneID Landscape_SS = new SceneID("Landscape_SS", register: true);

		public static readonly SceneID Landscape_SU = new SceneID("Landscape_SU", register: true);

		public static readonly SceneID Landscape_UW = new SceneID("Landscape_UW", register: true);

		public static readonly SceneID Outro_Hunter_1_Swim = new SceneID("Outro_Hunter_1_Swim", register: true);

		public static readonly SceneID Outro_Hunter_2_Sink = new SceneID("Outro_Hunter_2_Sink", register: true);

		public static readonly SceneID Outro_Hunter_3_Embrace = new SceneID("Outro_Hunter_3_Embrace", register: true);

		public static readonly SceneID Outro_1_Left_Swim = new SceneID("Outro_1_Left_Swim", register: true);

		public static readonly SceneID Outro_2_Up_Swim = new SceneID("Outro_2_Up_Swim", register: true);

		public static readonly SceneID Outro_3_Face = new SceneID("Outro_3_Face", register: true);

		public static readonly SceneID Outro_4_Tree = new SceneID("Outro_4_Tree", register: true);

		public static readonly SceneID Outro_Monk_1_Swim = new SceneID("Outro_Monk_1_Swim", register: true);

		public static readonly SceneID Outro_Monk_2_Reach = new SceneID("Outro_Monk_2_Reach", register: true);

		public static readonly SceneID Outro_Monk_3_Stop = new SceneID("Outro_Monk_3_Stop", register: true);

		public static readonly SceneID Options_Bkg = new SceneID("Options_Bkg", register: true);

		public static readonly SceneID Dream_Sleep = new SceneID("Dream_Sleep", register: true);

		public static readonly SceneID Dream_Sleep_Fade = new SceneID("Dream_Sleep_Fade", register: true);

		public static readonly SceneID Dream_Acceptance = new SceneID("Dream_Acceptance", register: true);

		public static readonly SceneID Dream_Iggy = new SceneID("Dream_Iggy", register: true);

		public static readonly SceneID Dream_Iggy_Doubt = new SceneID("Dream_Iggy_Doubt", register: true);

		public static readonly SceneID Dream_Iggy_Image = new SceneID("Dream_Iggy_Image", register: true);

		public static readonly SceneID Dream_Moon_Betrayal = new SceneID("Dream_Moon_Betrayal", register: true);

		public static readonly SceneID Dream_Moon_Friend = new SceneID("Dream_Moon_Friend", register: true);

		public static readonly SceneID Dream_Pebbles = new SceneID("Dream_Pebbles", register: true);

		public static readonly SceneID Void_Slugcat_Upright = new SceneID("Void_Slugcat_Upright", register: true);

		public static readonly SceneID Void_Slugcat_Down = new SceneID("Void_Slugcat_Down", register: true);

		public static readonly SceneID Slugcat_White = new SceneID("Slugcat_White", register: true);

		public static readonly SceneID Slugcat_Yellow = new SceneID("Slugcat_Yellow", register: true);

		public static readonly SceneID Slugcat_Red = new SceneID("Slugcat_Red", register: true);

		public static readonly SceneID Ghost_White = new SceneID("Ghost_White", register: true);

		public static readonly SceneID Ghost_Yellow = new SceneID("Ghost_Yellow", register: true);

		public static readonly SceneID Ghost_Red = new SceneID("Ghost_Red", register: true);

		public static readonly SceneID Yellow_Intro_A = new SceneID("Yellow_Intro_A", register: true);

		public static readonly SceneID Yellow_Intro_B = new SceneID("Yellow_Intro_B", register: true);

		public static readonly SceneID Slugcat_Dead_Red = new SceneID("Slugcat_Dead_Red", register: true);

		public static readonly SceneID Red_Ascend = new SceneID("Red_Ascend", register: true);

		public SceneID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public bool flatMode;

	public float focus;

	public float lastFocus;

	public Vector2 camPos;

	public Vector2 lastCamPos;

	public List<MenuDepthIllustration> depthIllustrations;

	public List<MenuIllustration> flatIllustrations;

	public int slugcatColor;

	public List<string> recolorIllustrations = new List<string>();

	public float cameraRange = 1f;

	public MenuDepthIllustration dragIllustration;

	public Vector2 dragOffset;

	public bool saveButton;

	public string sceneFolder;

	public bool initialized;

	public bool hidden;

	public float blurMin;

	public float blurMax;

	public string positionsFile;

	public string cameraFile;

	public Dictionary<int, List<MenuDepthIllustration>> crossFades;

	public int crossFadeInd;

	public int crossFadeLength;

	public int crossFadeTime;

	public bool useFlatCrossfades;

	public SceneID sceneID;

	public MenuIllustration scribbleA;

	public MenuIllustration scribbleB;

	public Vector2 CamPos(float timeStacker)
	{
		return Vector2.Lerp(lastCamPos, camPos, timeStacker) * cameraRange;
	}

	public void AddIllustration(MenuIllustration newIllu)
	{
		if (newIllu is MenuDepthIllustration)
		{
			depthIllustrations.Add(newIllu as MenuDepthIllustration);
			if (initialized)
			{
				subObjects.Add(depthIllustrations[depthIllustrations.Count - 1]);
			}
			return;
		}
		flatIllustrations.Add(newIllu);
		if (flatIllustrations.Count > 1 && useFlatCrossfades)
		{
			newIllu.setAlpha = 0f;
		}
		if (initialized)
		{
			subObjects.Add(flatIllustrations[flatIllustrations.Count - 1]);
		}
	}

	public void HorizontalDisplace(float dp)
	{
		foreach (KeyValuePair<int, List<MenuDepthIllustration>> crossFade in crossFades)
		{
			for (int i = 0; i < crossFade.Value.Count; i++)
			{
				MenuDepthIllustration menuDepthIllustration = crossFade.Value[i];
				menuDepthIllustration.pos.x = menuDepthIllustration.pos.x + dp;
			}
		}
		for (int j = 0; j < depthIllustrations.Count; j++)
		{
			depthIllustrations[j].pos.x += dp;
		}
		for (int k = 0; k < flatIllustrations.Count; k++)
		{
			flatIllustrations[k].pos.x += dp;
		}
	}

	public MenuScene(Menu menu, MenuObject owner, SceneID sceneID)
		: base(menu, owner)
	{
		this.sceneID = sceneID;
		flatMode = menu.manager.rainWorld.flatIllustrations || (ModManager.MMF && (menu.manager.rainWorld.options.quality == Options.Quality.MEDIUM || menu.manager.rainWorld.options.quality == Options.Quality.LOW));
		depthIllustrations = new List<MenuDepthIllustration>();
		flatIllustrations = new List<MenuIllustration>();
		crossFades = new Dictionary<int, List<MenuDepthIllustration>>();
		hidden = false;
		blurMin = 0.25f;
		blurMax = 0.8f;
		positionsFile = "";
		cameraFile = "";
		crossFadeInd = -1;
		crossFadeTime = 0;
		crossFadeLength = 0;
		BuildScene();
		if (ModManager.MSC)
		{
			BuildMSCScene();
		}
		RefreshPositions();
		for (int i = 0; i < depthIllustrations.Count; i++)
		{
			subObjects.Add(depthIllustrations[i]);
			if (crossFades.ContainsKey(i))
			{
				List<MenuDepthIllustration> list = crossFades[i];
				for (int j = 0; j < list.Count; j++)
				{
					subObjects.Add(list[j]);
				}
			}
		}
		for (int k = 0; k < flatIllustrations.Count; k++)
		{
			subObjects.Add(flatIllustrations[k]);
		}
		initialized = true;
	}

	public void Hide()
	{
		hidden = true;
		foreach (KeyValuePair<int, List<MenuDepthIllustration>> crossFade in crossFades)
		{
			for (int i = 0; i < crossFade.Value.Count; i++)
			{
				crossFade.Value[i].visible = false;
			}
		}
		for (int j = 0; j < depthIllustrations.Count; j++)
		{
			depthIllustrations[j].visible = false;
		}
		for (int k = 0; k < flatIllustrations.Count; k++)
		{
			flatIllustrations[k].visible = false;
		}
	}

	public void UnloadImages()
	{
		for (int i = 0; i < depthIllustrations.Count; i++)
		{
			depthIllustrations[i].UnloadFile();
		}
	}

	public void Show()
	{
		hidden = false;
		for (int i = 0; i < depthIllustrations.Count; i++)
		{
			depthIllustrations[i].visible = true;
			if (crossFades.ContainsKey(i))
			{
				List<MenuDepthIllustration> list = crossFades[i];
				for (int j = 0; j < list.Count; j++)
				{
					list[j].visible = true;
				}
			}
		}
		for (int k = 0; k < flatIllustrations.Count; k++)
		{
			flatIllustrations[k].visible = true;
		}
	}

	public override void Update()
	{
		if (hidden)
		{
			return;
		}
		UpdateCrossfade();
		base.Update();
		if (!menu.manager.rainWorld.setup.devToolsActive && !ModManager.DevTools)
		{
			return;
		}
		if (dragIllustration == null)
		{
			if (menu.pressButton && Input.GetKey("n"))
			{
				dragIllustration = null;
				float num = float.MaxValue;
				for (int num2 = depthIllustrations.Count - 1; num2 >= 0; num2--)
				{
					if (depthIllustrations[num2].sprite.alpha > 0.2f)
					{
						float num3 = depthIllustrations[num2].DepthAtPosition(menu.mousePosition, devtool: true);
						if (num3 > -1f && num3 < num)
						{
							dragIllustration = depthIllustrations[num2];
							dragOffset = dragIllustration.pos - (Vector2)Futile.mousePosition;
							num = num3;
						}
					}
				}
			}
		}
		else if (menu.holdButton)
		{
			dragIllustration.pos = (Vector2)Futile.mousePosition + dragOffset;
		}
		else
		{
			dragIllustration = null;
		}
		if (Input.GetKey("b") && !saveButton)
		{
			SaveToFile();
		}
		saveButton = Input.GetKey("b");
	}

	protected virtual void SaveToFile()
	{
		Custom.Log($"Saving : {sceneID}");
		string text = "";
		for (int i = 0; i < depthIllustrations.Count; i++)
		{
			Custom.Log(depthIllustrations[i].fileName, depthIllustrations[i].pos.ToString());
			text = text + depthIllustrations[i].pos.x + ", " + depthIllustrations[i].pos.y + "\r\n";
		}
		string text2 = AssetManager.ResolveDirectory(sceneFolder);
		if (positionsFile != "")
		{
			using (StreamWriter streamWriter = File.CreateText((text2 + Path.DirectorySeparatorChar + positionsFile).ToLowerInvariant()))
			{
				streamWriter.Write(text);
				return;
			}
		}
		using StreamWriter streamWriter2 = File.CreateText((text2 + Path.DirectorySeparatorChar + "positions.txt").ToLowerInvariant());
		streamWriter2.Write(text);
	}

	public override void GrafUpdate(float timeStacker)
	{
		if (!hidden)
		{
			base.GrafUpdate(timeStacker);
			Shader.SetGlobalFloat(RainWorld.ShadPropBlurDepth, 1f + Mathf.Lerp(lastFocus, focus, timeStacker) * 9f);
			Shader.SetGlobalFloat(RainWorld.ShadPropBlurRange, Mathf.Clamp(Mathf.Lerp(blurMax, blurMin, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFocus, focus, timeStacker)), 0.75f)), 0f, 1f));
			Shader.SetGlobalVector(RainWorld.ShadPropMenuCamPos, new Vector2(Mathf.InverseLerp(-1f, 1f, Mathf.Lerp(lastCamPos.x, camPos.x, timeStacker) * cameraRange), Mathf.InverseLerp(-1f, 1f, Mathf.Lerp(lastCamPos.y, camPos.y, timeStacker) * cameraRange)));
		}
	}

	private void RefreshPositions()
	{
		if (sceneFolder == "")
		{
			return;
		}
		string text = sceneFolder;
		string path = ((!(positionsFile != "")) ? AssetManager.ResolveFilePath(text + Path.DirectorySeparatorChar + "positions_ims.txt") : AssetManager.ResolveFilePath(text + Path.DirectorySeparatorChar + positionsFile));
		if (!File.Exists(path) || !(this is InteractiveMenuScene))
		{
			path = AssetManager.ResolveFilePath(text + Path.DirectorySeparatorChar + "positions.txt");
		}
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length && i < depthIllustrations.Count; i++)
		{
			if (crossFades.ContainsKey(i))
			{
				List<MenuDepthIllustration> list = crossFades[i];
				for (int j = 0; j < list.Count; j++)
				{
					list[j].pos.x = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture) + list[j].offset.x;
					list[j].pos.y = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture) + list[j].offset.y;
					list[j].lastPos = list[j].pos;
				}
			}
			depthIllustrations[i].pos.x = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture) + depthIllustrations[i].offset.x;
			depthIllustrations[i].pos.y = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture) + depthIllustrations[i].offset.y;
			depthIllustrations[i].lastPos = depthIllustrations[i].pos;
		}
	}

	public void AddCrossfade(MenuDepthIllustration newIllu)
	{
		int key = depthIllustrations.Count - 1;
		if (!crossFades.ContainsKey(key))
		{
			crossFades[key] = new List<MenuDepthIllustration>();
		}
		crossFades[key].Add(newIllu);
		newIllu.setAlpha = 0f;
	}

	public void TriggerCrossfade(int duration)
	{
		crossFadeInd++;
		crossFadeTime = 0;
		crossFadeLength = duration;
	}

	public void UpdateCrossfade()
	{
		if (crossFadeTime >= crossFadeLength)
		{
			return;
		}
		crossFadeTime++;
		float num = (float)crossFadeTime / (float)crossFadeLength;
		if (useFlatCrossfades && flatMode)
		{
			float b;
			float value;
			if (num < 0.5f)
			{
				b = 1f;
				value = num * 2f;
			}
			else
			{
				b = 1f - (num - 0.5f) / 0.5f;
				value = 1f;
			}
			for (int i = 0; i < flatIllustrations.Count; i++)
			{
				if (i < crossFadeInd + 1)
				{
					flatIllustrations[i].setAlpha = Mathf.Min(flatIllustrations[i].alpha, b);
				}
				else if (i == crossFadeInd + 1)
				{
					flatIllustrations[i].setAlpha = value;
				}
			}
		}
		for (int j = 0; j < depthIllustrations.Count; j++)
		{
			if (!crossFades.ContainsKey(j))
			{
				continue;
			}
			List<MenuDepthIllustration> list = crossFades[j];
			float b2 = 1f - num;
			float value2 = num;
			if (crossFadeInd < list.Count && list[crossFadeInd].crossfadeMethod == MenuIllustration.CrossfadeType.MaintainBackground)
			{
				if (num < 0.5f)
				{
					b2 = 1f;
					value2 = num * 2f;
				}
				else
				{
					b2 = 1f - (num - 0.5f) / 0.5f;
					value2 = 1f;
				}
			}
			depthIllustrations[j].setAlpha = Mathf.Min(depthIllustrations[j].alpha, b2);
			for (int k = 0; k < list.Count; k++)
			{
				if (k < crossFadeInd)
				{
					list[k].setAlpha = Mathf.Min(list[k].alpha, b2);
				}
				if (k == crossFadeInd)
				{
					list[k].setAlpha = value2;
				}
			}
		}
	}

	private void BuildScene()
	{
		if (this is InteractiveMenuScene)
		{
			(this as InteractiveMenuScene).idleDepths = new List<float>();
		}
		Vector2 vector = new Vector2(0f, 0f);
		if (!(sceneID == SceneID.Empty))
		{
			if (sceneID == SceneID.MainMenu || sceneID == SceneID.MainMenu_Downpour)
			{
				bool flag = false;
				if (sceneID == SceneID.MainMenu_Downpour)
				{
					string path = AssetManager.ResolveFilePath("Scenes" + Path.DirectorySeparatorChar + "main menu - downpour" + Path.DirectorySeparatorChar + "main menu - downpour - flat.png");
					sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Main Menu - Downpour";
					flag = File.Exists(path);
					if (!flag)
					{
						string text = AssetManager.ResolveFilePath("main menu - downpour" + Path.DirectorySeparatorChar + "main menu - downpour - flat.png");
						if (text.Contains("consolefiles"))
						{
							sceneFolder = AssetManager.ResolveDirectory("main menu - downpour");
						}
						else
						{
							sceneFolder = AssetManager.ResolveDirectory("Scenes" + Path.DirectorySeparatorChar + "main menu - downpour");
						}
						sceneFolder = "Main Menu - Downpour";
						flag = File.Exists(text);
					}
				}
				if (flag && menu.manager.rainWorld.dlcVersion == 1)
				{
					if (flatMode)
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Main Menu - Downpour - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 bg", new Vector2(23f, 17f), 14f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 bats haze", new Vector2(23f, 17f), 7.5f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 scavs", new Vector2(23f, 17f), 3.3f, MenuDepthIllustration.MenuShader.Basic));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 liz", new Vector2(23f, 17f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 liz tail", new Vector2(23f, 17f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					}
				}
				else
				{
					sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Main Menu";
					if (flatMode)
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Main Menu - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainFarBackground", new Vector2(23f, 17f), 14f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainArchs", new Vector2(23f, 17f), 8.5f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainWaterfall", new Vector2(23f, 17f), 8f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, "", "MainVines3", new Vector2(-131f, 675f), 7.5f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, "", "MainVines2", new Vector2(-13f, 488f), 4f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainTerrain1", new Vector2(23f, 17f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainMGLizard2", new Vector2(23f, 17f), 3.45f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainMGLizard1", new Vector2(23f, 17f), 3.4f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainDarken", new Vector2(23f, 17f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
						depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.66f;
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainFog", new Vector2(23f, 17f), 4f, MenuDepthIllustration.MenuShader.Normal));
						depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.21f;
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainVignette", new Vector2(23f, 17f), 3f, MenuDepthIllustration.MenuShader.Normal));
						depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.55f;
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainFGLizard3", new Vector2(85f, 91f), 3.3f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainFGLizard2", new Vector2(143f, 81f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainFGLizard1", new Vector2(96f, 39f), 2.35f, MenuDepthIllustration.MenuShader.Normal));
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MainVines1", new Vector2(-66f, -68f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					}
				}
			}
			else if (sceneID == SceneID.SleepScreen)
			{
				SlugcatStats.Name white = SlugcatStats.Name.White;
				white = ((!(menu.manager.currentMainLoop is RainWorldGame)) ? menu.manager.rainWorld.progression.PlayingAsSlugcat : (menu.manager.currentMainLoop as RainWorldGame).StoryCharacter);
				string value = white.value;
				if (ModManager.MSC && white == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
				{
					value = SlugcatStats.Name.White.value;
				}
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Sleep Screen - " + value;
				if (flatMode)
				{
					if (ModManager.MSC && white == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Sleep Screen - White - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Sleep Screen - " + value + " - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sleep - 5", new Vector2(23f, 17f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sleep - 4", new Vector2(23f, 17f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.24f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sleep - 3", new Vector2(23f, 17f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					if (ModManager.MSC && white == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						int num = 1;
						int num2 = 1;
						string path2 = AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + "Sleep - D" + num2 + ".png");
						while (num2 < 999 && File.Exists(path2))
						{
							num = num2;
							num2++;
							path2 = AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + "Sleep - D" + num2 + ".png");
						}
						string fileName = "Sleep - D" + Random.Range(1, num + 1);
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, fileName, new Vector2(23f, 17f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
					}
					if (ModManager.MSC && white == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						AddIllustration(new MenuDepthIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Sleep - 2 - " + value, new Vector2(23f, 17f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sleep - 2 - " + value, new Vector2(23f, 17f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sleep - 1", new Vector2(23f, 17f), 1.2f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3.3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.7f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
					(this as InteractiveMenuScene).idleDepths.Add(1.7f);
					if (ModManager.MSC && white == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						(this as InteractiveMenuScene).idleDepths.Add(1.65f);
					}
					(this as InteractiveMenuScene).idleDepths.Add(1.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.2f);
				}
			}
			else if (sceneID == SceneID.RedsDeathStatisticsBkg)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Death Screen";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Death Screen - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Death - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Death - 4", new Vector2(0f, 0f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.53f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Death - 3", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Death - 2", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Death - 1", new Vector2(0f, 0f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerA", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerA2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerB", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerB2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerC", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerC2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					(this as InteractiveMenuScene).idleDepths.Add(2.6f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(3.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.StarveScreen)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Starve Screen";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Starve Screen - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Starve - 5", new Vector2(0f, 0f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Starve - 4", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Starve - 3", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.59f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Starve - 2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Starve - 1", new Vector2(0f, 0f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3.1f);
					(this as InteractiveMenuScene).idleDepths.Add(2.7f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(1.6f);
				}
			}
			else if (sceneID == SceneID.Intro_1_Tree)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 1 - Tree";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 1 - Tree - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - Bkg", new Vector2(0f, 0f), 16f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - TerrainC", new Vector2(0f, 0f), 11f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - TerrainB", new Vector2(0f, 0f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - TerrainA", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - FoliageB", new Vector2(0f, 0f), 4f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - Trunk", new Vector2(0f, 0f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - LightC", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - FoliageA", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - LightB", new Vector2(0f, 0f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - LightA", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - Pipes", new Vector2(0f, 0f), (this is InteractiveMenuScene) ? 0.8f : 0.4f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_2_Branch)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 2 - Branch";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 2 - Branch - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - Bkg", new Vector2(0f, 0f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Trunk", new Vector2(0f, 0f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - FoliageB", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - Slugcats", new Vector2(0f, 0f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - Light", new Vector2(0f, 0f), 1.4f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - FoliageA", new Vector2(0f, 0f), 0.1f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_3_In_Tree)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 3 - In Tree";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 3 - In Tree - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					float[] array = ((!(this is InteractiveMenuScene)) ? new float[14]
					{
						15f, 8f, 6f, 4.8f, 4f, 5.4f, 5.1f, 2.5f, 2f, 3f,
						1f, 1.5f, 1.3f, 1.1f
					} : new float[14]
					{
						15f, 8f, 6f, 4.8f, 4f, 5.4f, 3.2f, 2.2f, 3.2f, 3.2f,
						1.8f, 1.8f, 1.2f, 1.8f
					});
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "12 - Bkg", new Vector2(71f, 49f), array[0], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - TerrainD", new Vector2(543f, 51f), array[1], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - RainB", new Vector2(475f, 126f), array[2], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - RainA", new Vector2(593f, 19f), array[3], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RainEffect", new Vector2(593f, 19f), array[4], MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.75f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RainMist", new Vector2(726f, 272f), array[5], MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.75f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - TerrainC", new Vector2(726f, 72f), array[6], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - TerrainB", new Vector2(-126f, -100f), array[7], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - FruitsB", new Vector2(480f, 441f), array[8], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - SlugcatB", new Vector2(547f, 4f), array[9], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - TerrainA", new Vector2(-358f, -89f), array[10], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - SlugcatsA", new Vector2(283f, -83f), array[11], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - FruitsA", new Vector2(-103f, 423f), array[12], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - Roots", new Vector2(-213f, -85f), array[13], MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_4_Walking)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 4 - Walking";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 4 - Walking - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - RocksD", new Vector2(71f, 49f), 15f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - RocksC", new Vector2(71f, 49f), 12f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - RocksB", new Vector2(71f, 49f), 9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - RocksA", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - Pipe", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - Slugcats", new Vector2(543f, 51f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - Foreground", new Vector2(475f, 126f), (this is InteractiveMenuScene) ? 1.3f : 0.9f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_5_Hunting)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 5 - Hunting";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 5 - Hunting - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					float[] array2 = ((!(this is InteractiveMenuScene)) ? new float[13]
					{
						7.4f, 7f, 5f, 3f, 1.8f, 2.5f, 2.1f, 2.2f, 2f, 1.9f,
						0.9f, 0.6f, 0.2f
					} : new float[13]
					{
						7.4f, 7f, 5f, 3f, 1.8f, 2.5f, 2.1f, 2.2f, 2f, 1.9f,
						1.5f, 1.2f, 0.9f
					});
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "13 - Bkg", new Vector2(71f, 49f), array2[0], MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "12 - Light", new Vector2(71f, 49f), array2[1], MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.3f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - Waterfall", new Vector2(71f, 49f), array2[2], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - Pillar", new Vector2(71f, 49f), array2[3], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - Terrain", new Vector2(71f, 49f), array2[4], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - SlugcatC", new Vector2(71f, 49f), array2[5], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - SlugcatD", new Vector2(71f, 49f), array2[6], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - SlugcatB", new Vector2(71f, 49f), array2[7], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Specks", new Vector2(71f, 49f), array2[8], MenuDepthIllustration.MenuShader.LightEdges));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.7f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - CeilingB", new Vector2(71f, 49f), array2[9], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - CeilingA", new Vector2(71f, 49f), array2[10], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - SlugcatA", new Vector2(71f, 49f), array2[11], MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - Grass", new Vector2(71f, 49f), array2[12], MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Intro_6_7_Rain_Drop)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 6 - 7 - Rain Drop";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 6 - 7 - Rain Drop - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 6 - 7 - Rain Drop - Flat2", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sharp Ground", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Blurred Ground", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Sharp Slugcats", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Blurred Slugcats", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RainDrops", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_8_Climbing)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 8 - Climbing";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 8 - Climbing - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - PipesB", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - PipesA", new Vector2(71f, 49f), 5.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - WaterB", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - RockFormation", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - RockBloom", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - ClimbingSlugcat", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - SlugcatsAndRock", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - WaterB", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - RainDistortion", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - RainFilter", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.25f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-1 - Dark", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-2 - Bloom", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.25f;
				}
			}
			else if (sceneID == SceneID.Intro_9_Rainy_Climb)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 9 - Rainy Climb";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 9 - Rainy Climb - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "13 - Background", new Vector2(71f, 49f), 100f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "12 - Flash", new Vector2(71f, 49f), 15f, MenuDepthIllustration.MenuShader.Basic));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - BkgAndSlugcat", new Vector2(71f, 49f), 6.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - Terrain", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - SkyOverlay", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Overlay));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.5f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - BkgWater", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - WaterFallB", new Vector2(71f, 49f), 3.6f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - WaterFoamA", new Vector2(71f, 49f), 2.85f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - WaterFallA", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - BlurredSlugcats", new Vector2(71f, 49f), 4.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.5f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - SlugcatsClimbing", new Vector2(71f, 49f), 4.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - ForegroundWavesB", new Vector2(71f, 49f), 0.8f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - RainOverlay", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.SoftLight));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - ForegroundWaves", new Vector2(71f, 49f), 0.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "WhiteFade", new Vector2(0f, 0f), 200f, MenuDepthIllustration.MenuShader.Basic));
				}
			}
			else if (sceneID == SceneID.Intro_10_Fall)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 10 - Fall";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 10 - Fall - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "13 - Background", new Vector2(71f, 49f), 100f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - BkgAndSlugcat", new Vector2(71f, 49f), 6.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - Terrain", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - SidePipes", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - DarkSlugcatsClimbing", new Vector2(71f, 49f), 4.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8.5 - Darken", new Vector2(71f, 49f), 100f, MenuDepthIllustration.MenuShader.Basic));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.89f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - DarkBkgWater", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - DarkWaterFallB", new Vector2(71f, 49f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - DarkWaterFoamA", new Vector2(71f, 49f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - DarkWaterFallA", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - DarkForegroundWavesB", new Vector2(71f, 49f), 0.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - DarkForegroundWaves", new Vector2(71f, 49f), 0.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - Lightning", new Vector2(71f, 49f), 14f, MenuDepthIllustration.MenuShader.Basic));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-1 - SkySoftLight", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.SoftLight));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-2 - SkyOverlay", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-3 - LightOutline", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "-4 - FallingSlugcat", new Vector2(71f, 49f), 4.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - RainOverlay", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.SoftLight));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.75f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "WhiteFade", new Vector2(0f, 0f), 200f, MenuDepthIllustration.MenuShader.Basic));
				}
			}
			else if (sceneID == SceneID.Intro_10_5_Separation)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 10p5 - Separation";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 10p5 - Separation - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - SlugcatPup", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - DripsD", new Vector2(71f, 49f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - DripsC", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - SlugcatMother", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - DripsB", new Vector2(71f, 49f), 1.3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - DripsA", new Vector2(71f, 49f), 1.1f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_11_Drowning)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 11 - Drowning";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 11 - Drowning - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - DrowningBkg", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - FloraC", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - FloraB", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - FloraA", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - BubblesD", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - BubblesC", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Slugcat", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - BubblesB", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - BubblesA", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - LightB", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.LightEdges));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.2f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - LightA", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.2f;
				}
			}
			else if (sceneID == SceneID.Intro_12_Waking)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 12 - Waking";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 12 - Waking - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - Ground", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - SlugcatWaking", new Vector2(71f, 49f), 5.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - Dust", new Vector2(71f, 49f), 4.6f, MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Intro_13_Alone)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro 13 - Alone";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro 13 - Alone - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "12 - AloneBkg", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "11 - BkgRain", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - Archways", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - SlugcatBloom", new Vector2(71f, 49f), 2.25f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - SlugcatOnRock", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - Dust", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - Bloom", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Wires", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - ForegroundRocks", new Vector2(71f, 49f), 0.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - ChainB", new Vector2(71f, 49f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - ChainA", new Vector2(71f, 49f), 0.6f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Intro_14_Title)
			{
				AddIllustration(new MenuIllustration(menu, this, "", "MainTitle", new Vector2(433f, 340f), crispPixels: true, anchorCenter: false));
			}
			else if (sceneID == SceneID.Endgame_Survivor)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Survivor";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Survivor - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 6", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 5", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 4", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 3", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 2", new Vector2(71f, 49f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Survivor - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
					(this as InteractiveMenuScene).idleDepths.Add(3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(1.4f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Survivor - Symbol", new Vector2(683f, 70f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Hunter)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Hunter";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Hunter - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Hunter - 4", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Hunter - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Hunter - 2", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Hunter - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(2f);
					(this as InteractiveMenuScene).idleDepths.Add(2f);
					(this as InteractiveMenuScene).idleDepths.Add(2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Hunter - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Saint)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Saint";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Saint - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint - 5", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint - 4", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint - 3", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint - 2", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint - 1", new Vector2(71f, 49f), 1.2f, MenuDepthIllustration.MenuShader.Lighten));
					(this as InteractiveMenuScene).idleDepths.Add(2.2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.7f);
					(this as InteractiveMenuScene).idleDepths.Add(1.7f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Saint - Symbol", new Vector2(683f, 120f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= 0.01f + flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Monk)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Monk";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Monk - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Monk - 5", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Monk - 4", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.5f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Monk - 3", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Monk - 2", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Monk - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
					(this as InteractiveMenuScene).idleDepths.Add(2.2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.7f);
					(this as InteractiveMenuScene).idleDepths.Add(1.7f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Monk - Symbol", new Vector2(683f, 125f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= 0.01f + flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Chieftain)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Chieftain";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Chieftain - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Chieftain - 3", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Chieftain - 2", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Chieftain - 1", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
					(this as InteractiveMenuScene).idleDepths.Add(1.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.4f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Chieftain - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Friend)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Friend";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Friend - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 4", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 2", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
					(this as InteractiveMenuScene).idleDepths.Add(2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Friend - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Scholar)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Scholar";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Scholar - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 8", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 7", new Vector2(71f, 49f), 5.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 6", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 5", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.25f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 4", new Vector2(71f, 49f), 1.85f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.1f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 3", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 2", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Scholar - 1", new Vector2(71f, 49f), 1.2f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Scholar - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_DragonSlayer)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - DragonSlayer";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - DragonSlayer - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DragonSlayer - 2", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DragonSlayer - 1", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "DragonSlayer - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Outlaw)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Outlaw";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Outlaw - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 8", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 6", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 7", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 5", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 4", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 3", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 2", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outlaw - 1", new Vector2(71f, 49f), 0.7f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
					(this as InteractiveMenuScene).idleDepths.Add(0.8f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outlaw - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Traveller)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Wanderer";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Wanderer - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 6", new Vector2(71f, 49f), 12f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 7", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 5", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 4", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 3", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 2", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Wanderer - 1", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(8f);
					(this as InteractiveMenuScene).idleDepths.Add(6.5f);
					(this as InteractiveMenuScene).idleDepths.Add(5f);
					(this as InteractiveMenuScene).idleDepths.Add(3f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.8f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Wanderer - Symbol", new Vector2(683.01f, 50f), crispPixels: true, anchorCenter: false));
				flatIllustrations[flatIllustrations.Count - 1].pos.x -= flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
			}
			else if (sceneID == SceneID.Endgame_Martyr || sceneID == SceneID.Endgame_Mother)
			{
				(this as InteractiveMenuScene).idleDepths.Add(1f);
			}
			else if (sceneID == SceneID.Outro_Hunter_1_Swim)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Hunter 1 - Swim";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 6", new Vector2(71f, 49f), 100f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 5", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 4", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 3", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 2", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 1", new Vector2(71f, 49f), 1.2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 1 - Swim - 0", new Vector2(71f, 49f), 0.25f, MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Outro_Hunter_2_Sink)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Hunter 2 - Sink";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 5", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 4", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 2", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 1", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 2 - Sink - 0", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_Hunter_3_Embrace)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Hunter 3 - Embrace";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 7", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 6", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 5", new Vector2(71f, 49f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 4", new Vector2(71f, 49f), 3.8f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 3", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 2", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 0", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Hunter 3 - Embrace - 1", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_Monk_1_Swim)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Monk 1 - Swim";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 6", new Vector2(71f, 49f), 9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 5", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 4", new Vector2(71f, 49f), 5.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 3", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 2", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 1", new Vector2(71f, 49f), 0.6f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 1 - Swim - 0", new Vector2(71f, 49f), 0.5f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_Monk_2_Reach)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Monk 2 - Reach";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 2 - Reach - 4", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 2 - Reach - 2", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 2 - Reach - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 2 - Reach - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 2 - Reach - 0", new Vector2(71f, 49f), 1f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_Monk_3_Stop)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Monk 3 - Stop";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 5", new Vector2(71f, 49f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 3", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 2", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 1", new Vector2(71f, 49f), 0.4f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Monk 3 - Stop - 0", new Vector2(71f, 49f), 1f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_1_Left_Swim)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro 1 - Left Swim";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - Left Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "10 - SlugcatsE", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "9 - SlugcatsD", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - SlugcatsC", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - SlugcatsB", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - SlugcatsA", new Vector2(71f, 49f), 1.3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - MainSlugcat", new Vector2(71f, 49f), 0.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - BlueBloom", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - CatsBloom", new Vector2(71f, 49f), 0.9f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - Specks", new Vector2(71f, 49f), 11f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - ForegroundBloom", new Vector2(71f, 49f), 1f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Outro_2_Up_Swim)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro 2 - Up Swim";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 2 - Up Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - SwimBkg", new Vector2(71f, 49f), 18f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - BkgSwimmers", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - MainSwimmer", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - SlugcatBloomLighten", new Vector2(71f, 49f), 2.9f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.5f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - SlugcatBloomOverlay", new Vector2(71f, 49f), 2.95f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - Bubbles", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - ForegroundSlugcats", new Vector2(71f, 49f), 0.6f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Outro_3_Face)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro 3 - Face";
				if (flatMode)
				{
					string fileName2 = "Outro 3 - Face - Flat";
					if (menu is SlideShow && (menu as SlideShow).slideShowID == SlideShow.SlideShowID.YellowOutro)
					{
						fileName2 = "Outro 3 - Face - Yellow - Flat";
					}
					else if (menu is SlideShow && (menu as SlideShow).slideShowID == SlideShow.SlideShowID.RedOutro)
					{
						fileName2 = "Outro 3 - Face - Red - Flat";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro)
					{
						fileName2 = "Outro 3 - Face - Gourmand - Flat";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro)
					{
						fileName2 = "Outro 3 - Face - Artificer - Flat";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro)
					{
						fileName2 = "Outro 3 - Face - Spearmaster - Flat";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro)
					{
						fileName2 = "Outro 3 - Face - Rivulet - Flat";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro)
					{
						fileName2 = "Outro 3 - Face - Gourmand - Flat";
					}
					if (menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)
					{
						AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Outro 3 - Face - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, fileName2, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - CloudsB", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - CloudsA", new Vector2(71f, 49f), 6.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - BloomLights", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
					string fileName3 = "2 - FaceCloseUp";
					if (menu is SlideShow && (menu as SlideShow).slideShowID == SlideShow.SlideShowID.YellowOutro)
					{
						fileName3 = "2 - FaceCloseUpYellow";
					}
					else if (menu is SlideShow && (menu as SlideShow).slideShowID == SlideShow.SlideShowID.RedOutro)
					{
						fileName3 = "2 - FaceCloseUpRed";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro)
					{
						fileName3 = "2 - FaceCloseUpGourmand";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro)
					{
						fileName3 = "2 - FaceCloseUpArtificer";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro)
					{
						fileName3 = "2 - FaceCloseUpSpearmaster";
					}
					else if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro)
					{
						fileName3 = "2 - FaceCloseUpRivulet";
					}
					if (ModManager.MSC && menu is SlideShow && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)
					{
						AddIllustration(new MenuDepthIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "2 - FaceCloseUp", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, fileName3, new Vector2((ModManager.MSC && (menu as SlideShow).slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro) ? 129 : 71, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - FaceBloom", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Overlay));
				}
			}
			else if (sceneID == SceneID.Outro_4_Tree)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro 4 - Tree";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 4 - Tree - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					string text2 = "";
					if (ModManager.MSC && menu.manager.fakeGlitchedEnding)
					{
						text2 = "_";
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "8 - Bkg" + text2, new Vector2(71f, 49f), 18f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "7 - Trunk" + text2, new Vector2(71f, 49f), 7.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "6 - Foliage" + text2, new Vector2(71f, 49f), 5.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Fog", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - TreeLight" + text2, new Vector2(71f, 49f), 5.1f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - TreeOverlay" + text2, new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - MainSlugcat", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - SlugcatBloom", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - ForeGroundSlugcats", new Vector2(71f, 49f), 0.7f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Landscape_CC)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - CC";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - CC - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CC_Landscape - 4", new Vector2(23f, 17f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CC_Landscape - 3", new Vector2(85f, 91f), 4.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CC_Landscape - 2", new Vector2(143f, 81f), 3.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CC_Landscape - 1", new Vector2(96f, 39f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_CC_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_CC", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_GW)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - GW";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - GW - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "GW_Landscape - 4", new Vector2(23f, 17f), 7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "GW_Landscape - 3", new Vector2(85f, 91f), 4.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "GW_Landscape - 2", new Vector2(143f, 81f), 3.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "GW_Landscape - 1", new Vector2(96f, 39f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_GW_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_GW", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SI)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SI";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SI - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SI_Landscape - 5", new Vector2(23f, 17f), 15f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SI_Landscape - 4", new Vector2(85f, 91f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SI_Landscape - 3", new Vector2(85f, 91f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SI_Landscape - 2", new Vector2(143f, 81f), 0.9f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SI_Landscape - 1", new Vector2(96f, 39f), 0.4f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SI_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SI", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SU)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SU";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SU - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SU_Landscape - 3", new Vector2(85f, 91f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SU_Landscape - 2", new Vector2(143f, 81f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SU_Landscape - 1", new Vector2(96f, 39f), 1.25f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SU_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SU", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_HI)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - HI";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - HI - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HI_Landscape - 4", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HI_Landscape - 3", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HI_Landscape - 2", new Vector2(143f, 81f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HI_Landscape - 1", new Vector2(96f, 39f), 0.75f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_HI_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_HI", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_DS)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - DS";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - DS - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 6", new Vector2(85f, 91f), 9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 5", new Vector2(85f, 91f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 4", new Vector2(85f, 91f), 3.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 3", new Vector2(143f, 81f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 2", new Vector2(96f, 39f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DS_Landscape - 1", new Vector2(96f, 39f), 0.65f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_DS_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_DS", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SH)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SH";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SH - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SH_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SH_Landscape - 4", new Vector2(85f, 91f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SH_Landscape - 3", new Vector2(143f, 81f), 2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SH_Landscape - 2", new Vector2(96f, 39f), 0.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SH_Landscape - 1", new Vector2(96f, 39f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SH_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SH", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SL)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SL";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SL - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 6", new Vector2(85f, 91f), 14f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 1", new Vector2(96f, 39f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 4", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 1", new Vector2(96f, 39f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 3", new Vector2(143f, 81f), 2.5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 2", new Vector2(96f, 39f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 1", new Vector2(96f, 39f), 1.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SL_Landscape - 1", new Vector2(96f, 39f), 0.8f, MenuDepthIllustration.MenuShader.Overlay));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SL_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SL", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_LF)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - LF";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - LF - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LF_Landscape - 4", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LF_Landscape - 3", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LF_Landscape - 2", new Vector2(143f, 81f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LF_Landscape - 1", new Vector2(96f, 39f), 0.65f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_LF_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_LF", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_UW)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - UW";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - UW - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UW_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UW_Landscape - 4", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UW_Landscape - 3", new Vector2(143f, 81f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UW_Landscape - 2", new Vector2(96f, 39f), 1.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UW_Landscape - 1", new Vector2(96f, 39f), 0.65f, MenuDepthIllustration.MenuShader.LightEdges));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_UW_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_UW", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SS)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SS";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SS - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 6", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 5", new Vector2(85f, 91f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 4", new Vector2(85f, 91f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 3", new Vector2(85f, 91f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 2", new Vector2(143f, 81f), 1.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SS_Landscape - 1", new Vector2(96f, 39f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SS_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SS", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Landscape_SB)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - SB";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - SB - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SB_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SB_Landscape - 4", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SB_Landscape - 3", new Vector2(85f, 91f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SB_Landscape - 2", new Vector2(143f, 81f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "SB_Landscape - 1", new Vector2(96f, 39f), 0.75f, MenuDepthIllustration.MenuShader.Normal));
				}
				if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
				{
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SB_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					AddIllustration(new MenuIllustration(menu, this, "", "Title_SB", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
					flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
				}
			}
			else if (sceneID == SceneID.Options_Bkg)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Options Menu Bkg";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Options Menu Bkg - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - OptnsPipe", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - OptnsSlugcats", new Vector2(543f, 51f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - OptnsWires", new Vector2(475f, 126f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(1.9f);
					(this as InteractiveMenuScene).idleDepths.Add(1.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.35f);
				}
			}
			else if (sceneID == SceneID.Dream_Sleep || sceneID == SceneID.Dream_Sleep_Fade)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Sleep";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Sleep - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					if (sceneID == SceneID.Dream_Sleep_Fade)
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Sleep - Flat2", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - 5", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - LoneSlugcat", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - 2", new Vector2(543f, 51f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DreamSleep - 1", new Vector2(475f, 126f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Dream_Acceptance)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Acceptance";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Acceptance - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 6", new Vector2(71f, 49f), 12f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 5", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 4", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 3", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 2", new Vector2(543f, 51f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Acceptance - 1", new Vector2(475f, 126f), 0.9f, MenuDepthIllustration.MenuShader.Overlay));
				}
			}
			else if (sceneID == SceneID.Dream_Iggy)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Iggy";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Iggy - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy - 5", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy - 2", new Vector2(543f, 51f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy - 1", new Vector2(475f, 126f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
				}
			}
			else if (sceneID == SceneID.Dream_Iggy_Doubt)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Iggy Doubt";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Iggy Doubt - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 7", new Vector2(71f, 49f), 12f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 6", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 5", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 4", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 3", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 2", new Vector2(543f, 51f), 1.4f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Doubt - 1", new Vector2(475f, 126f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Dream_Iggy_Image)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Iggy Image";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Iggy Image - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 9", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 8", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 7", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 6", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 5", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 4", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 3", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 2", new Vector2(543f, 51f), 1.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Iggy Image - 1", new Vector2(475f, 126f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Dream_Moon_Betrayal)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Moon Betrayal";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Moon Betrayal - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 9", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 8", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 7", new Vector2(71f, 49f), 3.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 6", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 5", new Vector2(71f, 49f), 2.7f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 4", new Vector2(71f, 49f), 2.9f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 3", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 2", new Vector2(71f, 49f), 1.7f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Betrayal - 1", new Vector2(71f, 49f), 1.2f, MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Dream_Moon_Friend)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Moon Friend";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Moon Friend - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 12", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 11", new Vector2(71f, 49f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 10", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 9", new Vector2(71f, 49f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 8", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 7", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 6", new Vector2(71f, 49f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 5", new Vector2(71f, 49f), 4.4f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 4", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 3", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 2", new Vector2(71f, 49f), 2.7f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Friend - 1", new Vector2(71f, 49f), 1.2f, MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Dream_Pebbles)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Pebbles";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Pebbles - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 10", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 9", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 9", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.SoftLight));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 8", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 7", new Vector2(71f, 49f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 6", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 5", new Vector2(71f, 49f), 1.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 4", new Vector2(71f, 49f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 3", new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 2", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles - 1", new Vector2(71f, 49f), 0.8f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Void_Slugcat_Upright || sceneID == SceneID.Void_Slugcat_Down)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Void";
				if (flatMode)
				{
					if (sceneID == SceneID.Void_Slugcat_Upright)
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Void Upwards - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Void Downwards - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 4", new Vector2(71f, 49f), 9f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 3", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 2", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.7f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 1", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Downwards Slugcat", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Upright Slugcat", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.LightEdges));
				}
			}
			else if (sceneID == SceneID.Slugcat_White)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - White";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - White - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Background - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Haze - 4", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.3f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White BkgPlants - 3", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Vines - 1", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
					if (owner is SlugcatSelectMenu.SlugcatPage)
					{
						(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Slugcat - 2", new Vector2(0f, 0f), 2.7f, ModManager.MMF ? MenuDepthIllustration.MenuShader.Basic : MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White FgPlants - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3.6f);
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
					(this as InteractiveMenuScene).idleDepths.Add(2.7f);
					(this as InteractiveMenuScene).idleDepths.Add(2.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.Slugcat_Yellow)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Yellow";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Yellow - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Background - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Specks - 4", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Vines - 3", new Vector2(0f, 0f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow BkgPlants - 2", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
					if (owner is SlugcatSelectMenu.SlugcatPage)
					{
						(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Slugcat - 1", new Vector2(0f, 0f), 2.6f, ModManager.MMF ? MenuDepthIllustration.MenuShader.Basic : MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow FgPlants - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3.6f);
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
					(this as InteractiveMenuScene).idleDepths.Add(2.6f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.Slugcat_Red)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Red";
				if (flatMode)
				{
					if (owner.menu.manager.rainWorld.progression.miscProgressionData.redUnlocked)
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Red - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Red Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Background - 4", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Spears - 3", new Vector2(0f, 0f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red BgPlants - 2", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
					if (owner is SlugcatSelectMenu.SlugcatPage)
					{
						(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
					}
					if (owner.menu.manager.rainWorld.progression.miscProgressionData.redUnlocked)
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Slugcat - 1", new Vector2(0f, 0f), 2.3f, ModManager.MMF ? MenuDepthIllustration.MenuShader.Basic : MenuDepthIllustration.MenuShader.LightEdges));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red FgPlants - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.4f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.NewDeath)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "New Death";
				SlugcatStats.Name name = ((!(menu.manager.currentMainLoop is RainWorldGame)) ? menu.manager.rainWorld.progression.PlayingAsSlugcat : (menu.manager.currentMainLoop as RainWorldGame).StoryCharacter);
				if (flatMode)
				{
					if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "New Death - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
						AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "New Death Flower - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
					else
					{
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "New Death - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
						AddIllustration(new MenuIllustration(menu, this, sceneFolder, "New Death Flower - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					}
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 6", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 55", new Vector2(0f, 0f), 3.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 5", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 4", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Lighten));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 3", new Vector2(0f, 0f), 3f, MenuDepthIllustration.MenuShader.Normal));
					if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						AddIllustration(new MenuDepthIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "New Death - 2", new Vector2(0f, 0f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
					}
					else
					{
						AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 2", new Vector2(0f, 0f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
					}
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "New Death - 1", new Vector2(0f, 0f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerA", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerA2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerB", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerB2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerC", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "FlowerC2", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Lighten));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					(this as InteractiveMenuScene).idleDepths.Add(2.6f);
					(this as InteractiveMenuScene).idleDepths.Add(2.5f);
					(this as InteractiveMenuScene).idleDepths.Add(3.6f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.Yellow_Intro_A)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Yellow Intro A";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Yellow Intro A - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Yellow Intro A - Flat2", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 8", new Vector2(0f, 0f), 12f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 7", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 6", new Vector2(0f, 0f), 3.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 5", new Vector2(0f, 0f), 3.3f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 4", new Vector2(0f, 0f), 1.9f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 3", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 1", new Vector2(0f, 0f), 0.8f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 2", new Vector2(0f, 0f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - Pixel", new Vector2(0f, 0f), 100f, MenuDepthIllustration.MenuShader.Basic));
					depthIllustrations[depthIllustrations.Count - 1].sprite.scaleX = 18f;
					depthIllustrations[depthIllustrations.Count - 1].sprite.scaleY = 9f;
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowA - 0", new Vector2(0f, 0f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0f;
				}
			}
			else if (sceneID == SceneID.Yellow_Intro_B)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Yellow Intro B";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Yellow Intro B - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowB - 5", new Vector2(0f, 0f), 5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowB - 4", new Vector2(0f, 0f), 50f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowB - 3", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowB - 1", new Vector2(0f, 0f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "YellowB - 2", new Vector2(0f, 0f), 0.75f, MenuDepthIllustration.MenuShader.Normal));
				}
			}
			else if (sceneID == SceneID.Ghost_White)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "White Ghost Slugcat";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "White Ghost Slugcat - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Ghost Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Ghost A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Ghost B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
					(this as InteractiveMenuScene).idleDepths.Add(3.1f);
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
				}
			}
			else if (sceneID == SceneID.Ghost_Yellow)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Yellow Ghost Slugcat";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Yellow Ghost Slugcat - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Ghost Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Ghost A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Yellow Ghost B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
					(this as InteractiveMenuScene).idleDepths.Add(3.1f);
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
				}
			}
			else if (sceneID == SceneID.Ghost_Red)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Red Ghost Slugcat";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Red Ghost Slugcat - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ghost Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ghost A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ghost B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
					(this as InteractiveMenuScene).idleDepths.Add(3.1f);
					(this as InteractiveMenuScene).idleDepths.Add(2.8f);
				}
			}
			else if (sceneID == SceneID.Slugcat_Dead_Red)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dead Red";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dead Red - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Death - 5", new Vector2(0f, 0f), 3.4f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Death - 4", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.53f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Death - 3", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Death - 2", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Death - 1", new Vector2(0f, 0f), 1.6f, MenuDepthIllustration.MenuShader.Normal));
					(this as InteractiveMenuScene).idleDepths.Add(3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.4f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.2f);
					(this as InteractiveMenuScene).idleDepths.Add(1.5f);
				}
			}
			else if (sceneID == SceneID.Red_Ascend)
			{
				sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Red Ascended Scene";
				if (flatMode)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Red Ascended Scene - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ascend - 3", new Vector2(0f, 0f), 4f, MenuDepthIllustration.MenuShader.Normal));
					depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.5f;
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ascend - 2", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
					AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Red Ascend - 1", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
					(this as InteractiveMenuScene).idleDepths.Add(2.4f);
					(this as InteractiveMenuScene).idleDepths.Add(2.3f);
					(this as InteractiveMenuScene).idleDepths.Add(2.2f);
				}
			}
		}
		if (sceneFolder == "")
		{
			return;
		}
		string path3 = AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + "positions_ims.txt");
		if (!File.Exists(path3) || !(this is InteractiveMenuScene))
		{
			path3 = AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + "positions.txt");
		}
		if (File.Exists(path3))
		{
			string[] array3 = File.ReadAllLines(path3);
			for (int i = 0; i < array3.Length && i < depthIllustrations.Count; i++)
			{
				depthIllustrations[i].pos.x = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array3[i], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture) + vector.x;
				depthIllustrations[i].pos.y = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array3[i], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture) + vector.y;
				depthIllustrations[i].lastPos = depthIllustrations[i].pos;
			}
		}
	}

	private void BuildMSCScene()
	{
		if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Gourmand)
		{
			BuildGourmandScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo2)
		{
			BuildArtificerScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet_Cell)
		{
			BuildRivuletScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint_Max)
		{
			BuildSaintScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Spear)
		{
			BuildSpearScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Inv)
		{
			BuildInvScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Gourmand)
		{
			BuildGourmandEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Artificer)
		{
			BuildArtificerEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Rivulet)
		{
			BuildRivuletEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Saint)
		{
			BuildSaintEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Spear)
		{
			BuildSpearEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.End_Inv)
		{
			BuildInvEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_LM)
		{
			BuildLMLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_HR)
		{
			BuildHRLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_OE)
		{
			BuildOELandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_MS)
		{
			BuildMSLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_LC)
		{
			BuildLCLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_DM)
		{
			BuildDMLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_CL)
		{
			BuildCLLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_RM)
		{
			BuildRMLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_UG)
		{
			BuildUGLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_VS)
		{
			BuildVSLandscapeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.SaintMaxKarma)
		{
			BuildSaintKarmaDream();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Spearmaster)
		{
			BuildSpearAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Rivulet)
		{
			BuildRivuletAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Rivulet_Robe)
		{
			BuildRivuletAltEndRobeScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet1)
		{
			BuildRivuletOutro(menu.manager.pebblesHasHalcyon ? 1 : 0);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L0)
		{
			BuildRivuletOutroL(menu.manager.pebblesHasHalcyon ? 1 : 0, 0);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L)
		{
			BuildRivuletOutroL(menu.manager.pebblesHasHalcyon ? 1 : 0, 1);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L2)
		{
			BuildRivuletOutroL(menu.manager.pebblesHasHalcyon ? 1 : 0, 2);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L3)
		{
			BuildRivuletOutroL(menu.manager.pebblesHasHalcyon ? 1 : 0, 3);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1)
		{
			BuildVanillaAltEnd(1, menu.manager.sceneSlot, menu.manager.rainWorld.progression.miscProgressionData.survivorPupsAtEnding);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_2)
		{
			BuildVanillaAltEnd(2, menu.manager.sceneSlot, menu.manager.rainWorld.progression.miscProgressionData.survivorPupsAtEnding);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_3)
		{
			BuildVanillaAltEnd(3, menu.manager.sceneSlot, menu.manager.rainWorld.progression.miscProgressionData.survivorPupsAtEnding);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4)
		{
			BuildVanillaAltEnd(4, menu.manager.sceneSlot, menu.manager.rainWorld.progression.miscProgressionData.survivorPupsAtEnding);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Artificer1)
		{
			BuildArtificerOutroLeftSwim();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Artificer2)
		{
			BuildArtificerOutroSwim();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Artificer3)
		{
			BuildArtificerOutroLook();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Artificer4)
		{
			BuildArtificerOutroFamily(0);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Artificer5)
		{
			BuildArtificerOutroFamily(1);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Gourmand)
		{
			BuildGourmandAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Gourmand_Full)
		{
			BuildGourmandAltEndFullScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_Portrait)
		{
			BuildArtificerAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Survivor)
		{
			BuildSurvivorAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Monk)
		{
			BuildMonkAltEndScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Endgame_Pilgrim)
		{
			BuildEndgamePilgrimScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Endgame_Nomad)
		{
			BuildEndgameNomadScene();
		}
		else if (sceneID == SceneID.Endgame_Mother)
		{
			BuildEndgameMotherScene();
		}
		else if (sceneID == SceneID.Endgame_Martyr)
		{
			BuildEndgameMartyrScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_1)
		{
			BuildArtificerOutroB(1);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_2)
		{
			BuildArtificerOutroB(2);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_3)
		{
			BuildArtificerOutroB(3);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand1)
		{
			BuildGourmandOutro(1);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand2)
		{
			BuildGourmandOutro(2);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand3)
		{
			BuildGourmandOutro(3);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S1 || sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S2 || sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S3 || sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S4)
		{
			BuildSaintIntroScene();
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream1)
		{
			BuildGourmandDream(1);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream2)
		{
			BuildGourmandDream(2);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream3)
		{
			BuildGourmandDream(3);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream4)
		{
			BuildGourmandDream(4);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream5)
		{
			BuildGourmandDream(5);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream_Start)
		{
			BuildGourmandDream(0);
		}
	}

	public void FlipScribble()
	{
		if (scribbleA == null || scribbleB == null)
		{
			return;
		}
		if (scribbleA is MenuDepthIllustration)
		{
			for (int i = 0; i < depthIllustrations.Count; i++)
			{
				if (depthIllustrations[i].sprite.element.name == scribbleA.sprite.element.name)
				{
					depthIllustrations[i].sprite.SetElementByName(scribbleB.sprite.element.name);
				}
				else if (depthIllustrations[i].sprite.element.name == scribbleB.sprite.element.name)
				{
					depthIllustrations[i].sprite.SetElementByName(scribbleA.sprite.element.name);
				}
			}
			return;
		}
		for (int j = 0; j < flatIllustrations.Count; j++)
		{
			if (flatIllustrations[j].sprite.element.name == scribbleA.sprite.element.name)
			{
				flatIllustrations[j].sprite.SetElementByName(scribbleB.sprite.element.name);
			}
			else if (flatIllustrations[j].sprite.element.name == scribbleB.sprite.element.name)
			{
				flatIllustrations[j].sprite.SetElementByName(scribbleA.sprite.element.name);
			}
		}
	}

	private void BuildEndgameMartyrScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Martyr";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Martyr - 5", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Martyr - 4", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Martyr - 3", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Martyr - 2", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Martyr - 1", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(2.9f);
			(this as InteractiveMenuScene).idleDepths.Add(2.5f);
			(this as InteractiveMenuScene).idleDepths.Add(1.9f);
			(this as InteractiveMenuScene).idleDepths.Add(2.2f);
		}
		AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Martyr - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
		MenuIllustration menuIllustration = flatIllustrations[flatIllustrations.Count - 1];
		menuIllustration.pos.x = menuIllustration.pos.x - flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Martyr - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildEndgameMotherScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Mother";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 7", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 6", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 5", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 4", new Vector2(71f, 49f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 3", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 2", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Mother - 1", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(2.9f);
			(this as InteractiveMenuScene).idleDepths.Add(2.6f);
			(this as InteractiveMenuScene).idleDepths.Add(1.9f);
			(this as InteractiveMenuScene).idleDepths.Add(2.7f);
		}
		AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Mother - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
		MenuIllustration menuIllustration = flatIllustrations[flatIllustrations.Count - 1];
		menuIllustration.pos.x = menuIllustration.pos.x - flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Mother - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildEndgamePilgrimScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Pilgrim";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 6", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 5", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 4", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 3", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 2", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pilgrim - 1", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.LightEdges));
			(this as InteractiveMenuScene).idleDepths.Add(2.5f);
			(this as InteractiveMenuScene).idleDepths.Add(2.4f);
			(this as InteractiveMenuScene).idleDepths.Add(1.9f);
			(this as InteractiveMenuScene).idleDepths.Add(2.5f);
		}
		AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Pilgrim - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
		MenuIllustration menuIllustration = flatIllustrations[flatIllustrations.Count - 1];
		menuIllustration.pos.x = menuIllustration.pos.x - flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Pilgrim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildEndgameNomadScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Endgame - Traveller";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 11", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 10", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 9", new Vector2(71f, 49f), 5.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 8", new Vector2(71f, 49f), 5.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 7", new Vector2(71f, 49f), 4.7f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 6", new Vector2(71f, 49f), 4.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 5", new Vector2(71f, 49f), 3.9f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 4", new Vector2(71f, 49f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 3", new Vector2(71f, 49f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 2", new Vector2(71f, 49f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Traveller - 1", new Vector2(71f, 49f), 3.1f, MenuDepthIllustration.MenuShader.LightEdges));
			(this as InteractiveMenuScene).idleDepths.Add(3.6f);
			(this as InteractiveMenuScene).idleDepths.Add(5.5f);
			(this as InteractiveMenuScene).idleDepths.Add(4.7f);
			(this as InteractiveMenuScene).idleDepths.Add(3.9f);
			(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		}
		AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Traveller - Symbol", new Vector2(683f, 50f), crispPixels: true, anchorCenter: false));
		MenuIllustration menuIllustration = flatIllustrations[flatIllustrations.Count - 1];
		menuIllustration.pos.x = menuIllustration.pos.x - flatIllustrations[flatIllustrations.Count - 1].size.x / 2f;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Endgame - Traveller - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private bool UseSlugcatUnlocked(SlugcatStats.Name slugcat)
	{
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			return ((owner as SlugcatSelectMenu.SlugcatPage).menu as SlugcatSelectMenu).SlugcatUnlocked(slugcat);
		}
		return SlugcatStats.SlugcatUnlocked(slugcat, owner.menu.manager.rainWorld);
	}

	private void BuildGourmandScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Gourmand";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Background - 4", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Pipes - 3", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Bg - 2", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
			if (owner is SlugcatSelectMenu.SlugcatPage)
			{
				(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
			}
			if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Slugcat - 1", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Fg - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(3.6f);
			(this as InteractiveMenuScene).idleDepths.Add(2.8f);
			(this as InteractiveMenuScene).idleDepths.Add(2.7f);
			(this as InteractiveMenuScene).idleDepths.Add(2.6f);
			(this as InteractiveMenuScene).idleDepths.Add(1.5f);
		}
		else if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Gourmand Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Gourmand - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildArtificerScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Artificer";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Background - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Vines - 4", new Vector2(0f, 0f), 3.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer MidBg - 3", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Smoke - 2", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
			if (owner is SlugcatSelectMenu.SlugcatPage)
			{
				(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
			}
			if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Slugcat - 1", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo)
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Robot - 1", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
			}
			else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo2)
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Robot - 2", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Robot - 0", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Fg - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(3.6f);
			(this as InteractiveMenuScene).idleDepths.Add(2.8f);
			(this as InteractiveMenuScene).idleDepths.Add(2.7f);
			(this as InteractiveMenuScene).idleDepths.Add(2.6f);
			(this as InteractiveMenuScene).idleDepths.Add(1.5f);
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Artificer Robot 1 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo2)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Artificer Robot 2 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Artificer Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Artificer - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildRivuletScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Rivulet";
		if (flatMode)
		{
			if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Rivulet Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet_Cell)
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Rivulet - Flat 2", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			else
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Rivulet - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet Background - 4", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet WaterPipes - 3", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet BkgPlants - 2", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
		}
		if (!UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
		}
		else if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet_Cell)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet Slugcat - 1b", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet Slugcat - 1", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet FgPlants - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.6f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
		(this as InteractiveMenuScene).idleDepths.Add(2.6f);
		(this as InteractiveMenuScene).idleDepths.Add(1.5f);
	}

	private void BuildSaintScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Saint";
		if (flatMode)
		{
			if (UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
			{
				if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint_Max)
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Saint - 1b - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
				else
				{
					AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Saint - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				}
			}
			else
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Saint Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint Background - 4", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint Chains - 3", new Vector2(0f, 0f), 2.9f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint BgPillars - 2", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
		}
		if (UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			if (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint_Max)
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint Slugcat - 1b", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint Slugcat - 1", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
			}
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint FgFence - 0", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3f);
		(this as InteractiveMenuScene).idleDepths.Add(2.4f);
		(this as InteractiveMenuScene).idleDepths.Add(2.3f);
		(this as InteractiveMenuScene).idleDepths.Add(2.2f);
		(this as InteractiveMenuScene).idleDepths.Add(1.5f);
	}

	private void BuildSpearScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - Spearmaster";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Background - 4", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Pipes - 3", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Vines - 2", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
			if (owner is SlugcatSelectMenu.SlugcatPage)
			{
				(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
			}
			if (UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Slugcat - 1", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Slugcat - 1 - Dark", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
			}
			(this as InteractiveMenuScene).idleDepths.Add(3.6f);
			(this as InteractiveMenuScene).idleDepths.Add(2.8f);
			(this as InteractiveMenuScene).idleDepths.Add(2.7f);
			(this as InteractiveMenuScene).idleDepths.Add(2.6f);
			(this as InteractiveMenuScene).idleDepths.Add(1.5f);
		}
		else if (UseSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Spearmaster - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat - Spearmaster Dark - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	private void BuildInvScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat - White";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "Slugcat - White - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Background - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Haze - 4", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Lighten));
		depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.3f;
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White BkgPlants - 3", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Vines - 1", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
		}
		AddIllustration(new MenuDepthIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "White Slugcat - 2b", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White FgPlants - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.6f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
		(this as InteractiveMenuScene).idleDepths.Add(2.7f);
		(this as InteractiveMenuScene).idleDepths.Add(2.6f);
		(this as InteractiveMenuScene).idleDepths.Add(1.5f);
	}

	private void BuildInvEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "White Ghost Slugcat";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "White Ghost Slugcat - FlatB", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Ghost Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, "Scenes" + Path.DirectorySeparatorChar + "Inv Screen", "White Ghost Ab", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "White Ghost B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildGourmandEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End - Gourmand";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Gourmand - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Gourmand B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	public void BuildRivuletOutro(int index)
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Rivulet 1";
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Background - 7", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			if (index == 1)
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Halcyon - 6G", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Pebbles - 5G", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Halcyon - 6", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Pebbles - 5", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Wires - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Pearls - 3", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Cable - 2", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
		}
		else if (index == 1)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Pebbles Sitting - Flat - G", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Pebbles Sitting - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	public void BuildRivuletOutroL(int index, int lighting)
	{
		useFlatCrossfades = true;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Rivulet 2";
		bool flag = index == 1;
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Background - 7L0", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			for (int i = 1; i < 4; i++)
			{
				AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Background - 7L" + i, new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal)
				{
					crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
				});
			}
			if (flag)
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Halcyon - 6G", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			}
			else
			{
				AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Halcyon - 6", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Pebbles - 5L1", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			for (int j = 1; j < 4; j++)
			{
				AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, (j == 3) ? "Pebbles Sitting - Pebbles - 5L3" : "Pebbles Sitting - Pebbles - 5L1", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges)
				{
					crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
				});
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Wires - 4L", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Pearls - 3L0", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal));
			for (int k = 1; k < 4; k++)
			{
				AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, (k == 3) ? "Pebbles Sitting - Pearls - 3L3" : "Pebbles Sitting - Pearls - 3L1", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Normal)
				{
					crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
				});
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Cable - 2L0", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges));
			for (int l = 1; l < 4; l++)
			{
				AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Cable - 2L" + l, new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.LightEdges)
				{
					crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
				});
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Pebbles Sitting - Sparks - 1", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
			return;
		}
		for (int m = 0; m < 4; m++)
		{
			if (flag)
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Pebbles Sitting - Flat - GL" + m, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			else
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Pebbles Sitting - Flat - L" + m, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
		}
	}

	private void BuildRivuletEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End - Rivulet";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Rivulet - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildRivuletAltEndRobeScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Rivulet";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Rivulet End B - Flat 2", new Vector2(683f, 404f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Rocks", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Moon 2", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Neurons", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildArtificerEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End - Artificer";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Artificer - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Artificer B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildSaintEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End - Saint";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Saint End - Flat", new Vector2(683f, 414f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint End Distortion", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint End Plants", new Vector2(0f, 0f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint End Shiny", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint End Body", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Saint End Chains", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(4.5f);
		(this as InteractiveMenuScene).idleDepths.Add(2.7f);
	}

	private void BuildSpearEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End - Spearmaster";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Spearmaster - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster A", new Vector2(0f, 0f), 2.85f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster B", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Overlay));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	public void BuildLMLandscapeScene()
	{
		blurMin = -0.2f;
		blurMax = 0.3f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - LM";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - LM - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LM_Landscape - 5", new Vector2(85f, 91f), 10f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LM_Landscape - 4", new Vector2(143f, 81f), 6.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LM_Landscape - 3", new Vector2(96f, 39f), 4.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LM_Landscape - 2", new Vector2(96f, 39f), 2.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LM_Landscape - 1", new Vector2(96f, 39f), 1.2f, MenuDepthIllustration.MenuShader.LightEdges));
			(this as InteractiveMenuScene).idleDepths.Add(1.2f);
			(this as InteractiveMenuScene).idleDepths.Add(2.4f);
			(this as InteractiveMenuScene).idleDepths.Add(1.8f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_LM_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_LM", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildHRLandscapeScene()
	{
		blurMin = -0.1f;
		blurMax = 0.4f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - HR";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - HR - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 6", new Vector2(85f, 91f), 11f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 5", new Vector2(85f, 91f), 6.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 4", new Vector2(85f, 91f), 4.8f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 3", new Vector2(143f, 81f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 2", new Vector2(96f, 39f), 1.1f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "HR_Landscape - 1", new Vector2(96f, 39f), 1f, MenuDepthIllustration.MenuShader.Basic));
			(this as InteractiveMenuScene).idleDepths.Add(11f);
			(this as InteractiveMenuScene).idleDepths.Add(6.5f);
			(this as InteractiveMenuScene).idleDepths.Add(4.8f);
			(this as InteractiveMenuScene).idleDepths.Add(8f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_HR_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_HR", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildMSLandscapeScene()
	{
		blurMin = -0.1f;
		blurMax = 0.5f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - MS";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - MS - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 6", new Vector2(85f, 91f), 10f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 5", new Vector2(85f, 91f), 6.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 4", new Vector2(143f, 81f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 3", new Vector2(96f, 39f), 2.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 2", new Vector2(96f, 39f), 2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "MS_Landscape - 1", new Vector2(96f, 39f), 1.2f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(2.2f);
			(this as InteractiveMenuScene).idleDepths.Add(3.4f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_MS_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_MS", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildLCLandscapeScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - LC";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - LC - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LC_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LC_Landscape - 4", new Vector2(85f, 91f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LC_Landscape - 3", new Vector2(143f, 81f), 2.8f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LC_Landscape - 2", new Vector2(96f, 39f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "LC_Landscape - 1", new Vector2(96f, 39f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
			(this as InteractiveMenuScene).idleDepths.Add(2.8f);
			(this as InteractiveMenuScene).idleDepths.Add(4.5f);
			(this as InteractiveMenuScene).idleDepths.Add(3.4f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_LC_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_LC", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildDMLandscapeScene()
	{
		blurMin = -0.1f;
		blurMax = 0.4f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - DM";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - DM - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DM_Landscape - 5", new Vector2(85f, 91f), 12f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DM_Landscape - 4", new Vector2(143f, 81f), 6.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DM_Landscape - 3", new Vector2(96f, 39f), 4.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DM_Landscape - 2", new Vector2(96f, 39f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "DM_Landscape - 1", new Vector2(96f, 39f), 1.2f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(2.8f);
			(this as InteractiveMenuScene).idleDepths.Add(1.2f);
			(this as InteractiveMenuScene).idleDepths.Add(2.2f);
			(this as InteractiveMenuScene).idleDepths.Add(3.4f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_DM_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_DM", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildOELandscapeScene()
	{
		blurMin = -0.1f;
		blurMax = 0.8f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - OE";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - OE - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 7", new Vector2(85f, 91f), 10f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 6", new Vector2(85f, 91f), 7.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 5", new Vector2(85f, 91f), 5.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 4", new Vector2(85f, 91f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 3", new Vector2(143f, 81f), 2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 2", new Vector2(96f, 39f), 1.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "OE_Landscape - 1", new Vector2(96f, 39f), 1f, MenuDepthIllustration.MenuShader.LightEdges));
			(this as InteractiveMenuScene).idleDepths.Add(1.4f);
			(this as InteractiveMenuScene).idleDepths.Add(2f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_OE_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_OE", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildCLLandscapeScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - CL";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - CL - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 8", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 7", new Vector2(85f, 91f), 6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 6", new Vector2(85f, 91f), 4.8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 5", new Vector2(85f, 91f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 4", new Vector2(85f, 91f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 3", new Vector2(143f, 81f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 2", new Vector2(96f, 39f), 0.8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "CL_Landscape - 1", new Vector2(96f, 39f), 0.5f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(2.9f);
			(this as InteractiveMenuScene).idleDepths.Add(5.2f);
			(this as InteractiveMenuScene).idleDepths.Add(1f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_CL_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_CL", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildRMLandscapeScene()
	{
		blurMin = 0.1f;
		blurMax = 1f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - RM";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - RM - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 8", new Vector2(85f, 91f), 10f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 7", new Vector2(85f, 91f), 9f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 6", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 5", new Vector2(85f, 91f), 7f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 4", new Vector2(85f, 91f), 3.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 3", new Vector2(85f, 91f), 2.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 2", new Vector2(143f, 81f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "RM_Landscape - 1", new Vector2(96f, 39f), 0.8f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(3.8f);
			(this as InteractiveMenuScene).idleDepths.Add(7f);
			(this as InteractiveMenuScene).idleDepths.Add(2f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_RM_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_RM", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildUGLandscapeScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - UG";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - UG - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UG_Landscape - 4", new Vector2(85f, 91f), 9f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UG_Landscape - 3", new Vector2(143f, 81f), 5.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UG_Landscape - 2", new Vector2(96f, 39f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "UG_Landscape - 1", new Vector2(96f, 39f), 1.1f, MenuDepthIllustration.MenuShader.Normal));
			(this as InteractiveMenuScene).idleDepths.Add(3.8f);
			(this as InteractiveMenuScene).idleDepths.Add(6f);
			(this as InteractiveMenuScene).idleDepths.Add(1.5f);
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_UG_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_UG", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	public void BuildVSLandscapeScene()
	{
		blurMin = 0.1f;
		blurMax = 0.5f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Landscape - VS";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Landscape - VS - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 6", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 4", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 3", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 2", new Vector2(143f, 81f), 2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "VS_Landscape - 1", new Vector2(96f, 39f), 0.75f, MenuDepthIllustration.MenuShader.Normal));
		}
		if (menu.ID == ProcessManager.ProcessID.FastTravelScreen || menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
		{
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_VS_Shadow", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			AddIllustration(new MenuIllustration(menu, this, string.Empty, "Title_VS", new Vector2(0.01f, 0.01f), crispPixels: true, anchorCenter: false));
			flatIllustrations[flatIllustrations.Count - 1].sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		}
	}

	private void BuildSaintIntroScene()
	{
		if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S2)
		{
			sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro S2 - Face";
			if (flatMode)
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro S2 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
				return;
			}
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - CloudsB", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - CloudsA", new Vector2(71f, 49f), 6.1f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - BloomLights", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - FaceCloseUp", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - FaceBloom", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.Overlay));
			return;
		}
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Intro S1 - Void";
		if (flatMode)
		{
			if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S1)
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro S1 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			else
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Intro S3 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 4", new Vector2(71f, 49f), 9f, MenuDepthIllustration.MenuShader.Lighten));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 3", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Lighten));
		if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S1 || sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S3)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 2", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Lighten));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Empty", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
		}
		depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.7f;
		if (sceneID != MoreSlugcatsEnums.MenuSceneID.Intro_S1)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Ascension", new Vector2(71f, 49f), 3.9f, MenuDepthIllustration.MenuShader.LightEdges));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Empty", new Vector2(71f, 49f), 3.9f, MenuDepthIllustration.MenuShader.Normal));
		}
		if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S1)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Void - 1", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Lighten));
		}
		else
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Empty", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		}
		if (sceneID == MoreSlugcatsEnums.MenuSceneID.Intro_S1)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Upright Slugcat", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.LightEdges));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Empty", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Upright Slugcat", new Vector2(71f, -200f), 1.4f, MenuDepthIllustration.MenuShader.LightEdges));
	}

	public void BuildSaintKarmaDream()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Karma";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Karma - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Karma - 5", new Vector2(71f, 49f), 4f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Karma - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Karma - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Karma - 2", new Vector2(543f, 51f), 1.7f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Karma - 1", new Vector2(475f, 126f), 1.5f, MenuDepthIllustration.MenuShader.Lighten));
	}

	public void BuildArtificerOutroLeftSwim()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Artificer Left Swim";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro 1 - OALeft Swim - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - OAMainSlugcat", new Vector2(71f, 49f), 0.9f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - OABlueBloom", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Lighten));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - OACatsBloom", new Vector2(71f, 49f), 0.9f, MenuDepthIllustration.MenuShader.Lighten));
	}

	public void BuildArtificerOutroSwim()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Artificer 1";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro Artificer 1 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "5 - Solid", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - OA1Background", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - OA1Swimmers", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - OA1Artificer", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - OA1Voided", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Lighten));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - OA1Foreground Lights", new Vector2(71f, 49f), 1f, MenuDepthIllustration.MenuShader.Lighten));
	}

	public void BuildArtificerOutroLook()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Artificer 2";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro Artificer 2 - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - Solid", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - OA2Background", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - OA2Artificer", new Vector2(71f, 49f), 2.5f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - OA2ForeGroundSlugcats", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
	}

	public void BuildArtificerOutroFamily(int variant)
	{
		int num = 4;
		if (variant == 0)
		{
			num = 3;
		}
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Artificer " + num;
		if (!flatMode)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "4 - Solid", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "3 - OA" + num + "Background", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "2 - OA" + num + "Voided Back", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Lighten));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "1 - OA" + num + "Family", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "0 - OA" + num + "Voided Front", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro Artificer " + num + " - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	public void BuildArtificerOutroB(int index)
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Outro Artificer " + index + "_B";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Outro Artificer " + index + "_B - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		if (index == 1)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 8", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 7", new Vector2(71f, 49f), 7.2f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 6", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 5", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 4", new Vector2(71f, 49f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 3", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 2", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 1_B - 1", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		}
		if (index == 2)
		{
			blurMin = -0.2f;
			blurMax = 0f;
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 7", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 6", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 5", new Vector2(71f, 49f), 3.8f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 4", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 3", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 2", new Vector2(71f, 49f), 1.4f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 2_B - 1", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Basic));
		}
		if (index == 3)
		{
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 3_B - 4", new Vector2(71f, 49f), 6.2f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 3_B - 3", new Vector2(71f, 49f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 3_B - 2", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Artificer 3_B - 1", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.Normal));
		}
	}

	private void BuildSpearAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Spearmaster";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Spearmaster End B - Flat", new Vector2(683f, 404f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster End Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster End Halo", new Vector2(0f, 0f), 3.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Spearmaster End SRS", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.Basic));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildRivuletAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Rivulet";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Rivulet End B - Flat", new Vector2(683f, 404f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Bkg", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Rocks", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Moon", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Rivulet End Neurons", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildGourmandAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Gourmand";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat End_B - Gourmand - Flat", new Vector2(683f, 404f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Gourmand - 3", new Vector2(0f, 0f), 4.5f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Gourmand - 2", new Vector2(0f, 0f), 3.1f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Gourmand - 1", new Vector2(0f, 0f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Gourmand - 0", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	private void BuildGourmandAltEndFullScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_C - Gourmand";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - Flat", new Vector2(683f, 404f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 6", new Vector2(0f, 0f), 6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 5", new Vector2(0f, 0f), 4.6f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 4", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 3", new Vector2(0f, 0f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 2", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 1", new Vector2(0f, 0f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_C - Gourmand - 0", new Vector2(0f, 0f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.1f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
	}

	public void BuildGourmandOutro(int sceneID)
	{
		useFlatCrossfades = true;
		string text = "Outro Gourmand " + sceneID;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + text;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, text + " - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			if (sceneID == 1 || sceneID == 2)
			{
				AddIllustration(new MenuIllustration(menu, this, sceneFolder, text + "B - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			}
			return;
		}
		switch (sceneID)
		{
		case 1:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 6", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1B - 6", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges)
			{
				crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
			});
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 5", new Vector2(71f, 49f), 6f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 4", new Vector2(71f, 49f), 4.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1B - 4", new Vector2(71f, 49f), 4.2f, MenuDepthIllustration.MenuShader.LightEdges)
			{
				crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
			});
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 3", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 2", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1B - 2", new Vector2(71f, 49f), 2.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 1 - 1", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.Normal));
			break;
		case 2:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 11", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 10", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 9", new Vector2(71f, 49f), 4.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2B - 9", new Vector2(71f, 49f), 4.8f, MenuDepthIllustration.MenuShader.LightEdges)
			{
				crossfadeMethod = MenuIllustration.CrossfadeType.MaintainBackground
			});
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 8", new Vector2(71f, 49f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 7", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 6", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Multiply));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2B - 6", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 5", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2B - 5", new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.Normal));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 4", new Vector2(71f, 49f), 2.1f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 3B", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 3", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddCrossfade(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2B - 3", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 2", new Vector2(71f, 49f), 1.3f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 2 - 1", new Vector2(71f, 49f), 1f, MenuDepthIllustration.MenuShader.Normal));
			break;
		case 3:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 3 - 5", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 3 - 4", new Vector2(71f, 49f), 4.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 3 - 3", new Vector2(71f, 49f), 3.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 3 - 2", new Vector2(71f, 49f), 2.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro Gourmand 3 - 1", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
			break;
		}
	}

	private void BuildArtificerAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Artificer";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 5", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 4", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 3", new Vector2(0f, 0f), 2.25f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 2", new Vector2(0f, 0f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 1", new Vector2(0f, 0f), 2.15f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Artificer - 0", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Basic));
		(this as InteractiveMenuScene).idleDepths.Add(3.2f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
		(this as InteractiveMenuScene).idleDepths.Add(2.7f);
		(this as InteractiveMenuScene).idleDepths.Add(2.2f);
	}

	private void BuildSurvivorAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - White";
		int num = Mathf.Clamp(menu.manager.rainWorld.progression.miscProgressionData.survivorPupsAtEnding, 0, 2);
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Flat_" + num, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - BG", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Vines", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Fruit2", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Ground", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Slugcat_" + num, new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Fruit1", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Fruit3", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - White - Grass", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.6f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
		(this as InteractiveMenuScene).idleDepths.Add(2.7f);
		(this as InteractiveMenuScene).idleDepths.Add(2.6f);
		(this as InteractiveMenuScene).idleDepths.Add(1.5f);
	}

	private void BuildMonkAltEndScene()
	{
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Slugcat End_B - Yellow";
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - BG", new Vector2(0f, 0f), 3.6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Vines", new Vector2(0f, 0f), 3.2f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Fruit2", new Vector2(0f, 0f), 2.8f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Ground", new Vector2(0f, 0f), 2.7f, MenuDepthIllustration.MenuShader.LightEdges));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Slugcat B", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Slugcat F", new Vector2(0f, 0f), 2.6f, MenuDepthIllustration.MenuShader.Basic));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Fruit1", new Vector2(0f, 0f), 2.5f, MenuDepthIllustration.MenuShader.Normal));
		if (owner is SlugcatSelectMenu.SlugcatPage)
		{
			(owner as SlugcatSelectMenu.SlugcatPage).AddGlow();
		}
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Fruit3", new Vector2(0f, 0f), 2.3f, MenuDepthIllustration.MenuShader.Normal));
		AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Slugcat End_B - Yellow - Grass", new Vector2(0f, 0f), 2.1f, MenuDepthIllustration.MenuShader.Normal));
		(this as InteractiveMenuScene).idleDepths.Add(3.6f);
		(this as InteractiveMenuScene).idleDepths.Add(2.8f);
		(this as InteractiveMenuScene).idleDepths.Add(2.7f);
		(this as InteractiveMenuScene).idleDepths.Add(2.6f);
		(this as InteractiveMenuScene).idleDepths.Add(1.5f);
	}

	public void BuildGourmandDream(int index)
	{
		blurMin = -0.4f;
		blurMax = 0.1f;
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + "Dream - Gourmand";
		if (!flatMode && index != 0)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - Backlayer", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal));
			MenuDepthIllustration menuDepthIllustration = new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - Background - " + index, new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal);
			menuDepthIllustration.offset = zero;
			AddIllustration(menuDepthIllustration);
			scribbleA = new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "a - ol", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal);
			scribbleB = new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "b - ol", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal);
			MenuDepthIllustration menuDepthIllustration2 = new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "a - ol", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.Normal);
			(scribbleA as MenuDepthIllustration).offset = zero2;
			(scribbleB as MenuDepthIllustration).offset = zero2;
			menuDepthIllustration2.offset = zero2;
			AddIllustration(menuDepthIllustration2);
			MenuDepthIllustration menuDepthIllustration3 = new MenuDepthIllustration(menu, this, sceneFolder, "Dream - Gourmand - Foreground - " + index, new Vector2(71f, 49f), 1.6f, MenuDepthIllustration.MenuShader.LightEdges);
			menuDepthIllustration3.offset = zero;
			AddIllustration(menuDepthIllustration3);
		}
		else
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Gourmand - Background - Flat - " + index, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			scribbleA = new MenuIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "a", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true);
			scribbleB = new MenuIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "b", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true);
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, "Dream - Gourmand - " + index + "a", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
		}
	}

	public void BuildVanillaAltEnd(int sceneID, SlugcatStats.Name character, int slugpups)
	{
		string text = "Outro 1_B - Clearing";
		switch (sceneID)
		{
		case 2:
			text = "Outro 2_B - Peek";
			break;
		case 3:
			text = "Outro 3_B - Return";
			break;
		case 4:
			text = "Outro 4_B - Home";
			break;
		}
		int num = Mathf.Min(slugpups, 2);
		if (character == null)
		{
			character = SlugcatStats.Name.White;
		}
		if (character != SlugcatStats.Name.White)
		{
			num = 0;
		}
		sceneFolder = "Scenes" + Path.DirectorySeparatorChar + text;
		if (flatMode)
		{
			AddIllustration(new MenuIllustration(menu, this, sceneFolder, text + " - Flat_" + character.ToString() + "_" + num, new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
			return;
		}
		switch (sceneID)
		{
		case 1:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 1_B - Clearing - 3", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 1_B - Clearing - 2", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 1_B - Clearing - 1", new Vector2(71f, 49f), 1.9f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 1_B - Clearing - 0_" + character.ToString() + "_" + num, new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
			break;
		case 2:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 2_B - Peek - 3", new Vector2(71f, 49f), 12f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 2_B - Peek - 2_" + character.ToString() + "_" + num, new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 2_B - Peek - 1", new Vector2(71f, 49f), 6.5f, MenuDepthIllustration.MenuShader.Basic));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 2_B - Peek - 0", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
			break;
		case 3:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 3_B - Return - 3", new Vector2(71f, 49f), 8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 3_B - Return - 2", new Vector2(71f, 49f), 4.5f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 3_B - Return - 1", new Vector2(71f, 49f), 3.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 3_B - Return - 0", new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
			break;
		case 4:
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 10", new Vector2(71f, 49f), 10f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 9", new Vector2(71f, 49f), 6.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 8", new Vector2(71f, 49f), 4.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 7", new Vector2(71f, 49f), 3.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 6", new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 5_" + character.ToString() + "_" + ((num > 0) ? "1" : "0"), new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.Multiply));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 4_" + character.ToString() + "_" + ((num > 0) ? "1" : "0"), new Vector2(71f, 49f), 3.2f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 3_" + ((num > 1) ? "1" : "0"), new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 2 - " + character.ToString(), new Vector2(71f, 49f), 2.4f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 1", new Vector2(71f, 49f), 1.8f, MenuDepthIllustration.MenuShader.LightEdges));
			AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "Outro 4_B - Home - 0 - " + character.ToString(), new Vector2(71f, 49f), 1.5f, MenuDepthIllustration.MenuShader.LightEdges));
			break;
		}
	}
}
