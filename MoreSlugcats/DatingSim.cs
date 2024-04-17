using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class DatingSim : global::Menu.Menu
{
	public class Message
	{
		public float xOrientation;

		public float yPos;

		public string text;

		public int linger;

		public int lines;

		public int longestLine;

		public Message(string text, float xOrientation, float yPos, int extraLinger)
		{
			this.text = Regex.Replace(text, "<LINE>", Environment.NewLine);
			this.xOrientation = xOrientation;
			linger = 0;
			string[] array = Regex.Split(text, "<LINE>");
			for (int i = 0; i < array.Length; i++)
			{
				longestLine = Math.Max(longestLine, array[i].Length);
			}
			lines = array.Length;
			this.yPos = yPos + (20f + 15f * (float)lines);
		}
	}

	private string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

	private MenuLabel creditsLabelLeft;

	private MenuLabel creditsLabelRight;

	public SimpleButton exitButton;

	public MenuLabel[] labels;

	public int counter;

	public MenuIllustration slugcat;

	public MenuIllustration other;

	public MenuIllustration bg;

	public List<Message> messages;

	public List<MenuLabel> messageLabels;

	public List<SimpleButton> messageButtons;

	public List<string> messageButtonTexts;

	private float totalHeight;

	public static float lineHeight = 15f;

	public static float messageSep = 15f;

	private int showCharacter;

	private int showLine;

	private string showText;

	private int lingerCounter;

	public float[] messageWidths;

	public List<string> optionFiles;

	public float defaultXOrientation;

	public float defaultYPos;

	private Dictionary<string, float> musicTimes;

	public bool readySetMusicTime;

	public string slugcatBasename;

	public string otherBasename;

	public float slugcatFrame;

	public float otherFrame;

	public float slugcatSpeed;

	public float otherSpeed;

	public KarmaLadderScreen.SleepDeathScreenDataPackage passthroughPackage;

	public override bool ForceNoMouseMode => false;

	protected override bool FreezeMenuFunctions => false;

	public Message CurrentMessage => messages[showLine];

	public DatingSim(ProcessManager manager)
		: base(manager, MoreSlugcatsEnums.ProcessID.DatingSim)
	{
		pages.Add(new Page(this, null, "main", 0));
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(30f);
		}
		musicTimes = new Dictionary<string, float>();
		bg = new MenuIllustration(this, scene, "Content", "bg", Vector2.zero, crispPixels: true, anchorCenter: true);
		bg.sprite.scaleX = base.manager.rainWorld.options.ScreenSize.x;
		bg.sprite.scaleY = base.manager.rainWorld.options.ScreenSize.y;
		bg.sprite.anchorX = 0f;
		bg.sprite.anchorY = 0f;
		slugcat = new MenuIllustration(this, scene, "Content", "sm1", new Vector2(base.manager.rainWorld.options.ScreenSize.x / 7f * 2f - 50f, base.manager.rainWorld.options.ScreenSize.y * 0.7f), crispPixels: true, anchorCenter: true);
		other = new MenuIllustration(this, scene, "Content", "BL1", new Vector2(base.manager.rainWorld.options.ScreenSize.x / 7f * 5f - 50f, base.manager.rainWorld.options.ScreenSize.y * 0.7f), crispPixels: true, anchorCenter: true);
		pages[0].subObjects.Add(bg);
		pages[0].subObjects.Add(slugcat);
		pages[0].subObjects.Add(other);
		defaultXOrientation = 0.5f;
		messages = new List<Message>();
		messageLabels = new List<MenuLabel>();
		messageButtons = new List<SimpleButton>();
		messageButtonTexts = new List<string>();
		creditsLabelLeft = new MenuLabel(this, pages[0], Translate("This easter egg was inspired by the original Moar Slugcats Dating Sim mod, but has no affiliation with the original creators.") + Environment.NewLine + Translate("Assets and content by:") + " Dakras, AndrewFM, Cappin, Norgad", new Vector2(10f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y - 10f), new Vector2(1f, 1f), bigText: false);
		creditsLabelLeft.label.alignment = FLabelAlignment.Left;
		creditsLabelLeft.label.color = new Color(0.01f, 0.01f, 0.01f);
		creditsLabelRight = new MenuLabel(this, pages[0], Translate("Spanish translation by Garrakx") + Environment.NewLine + Translate("Korean translation by topicular"), new Vector2(base.manager.rainWorld.options.ScreenSize.x - 10f + (1366f - base.manager.rainWorld.options.ScreenSize.x) / 2f, base.manager.rainWorld.options.ScreenSize.y - 10f), new Vector2(1f, 1f), bigText: false);
		creditsLabelRight.label.color = new Color(0.01f, 0.01f, 0.01f);
		pages[0].subObjects.Add(creditsLabelLeft);
		pages[0].subObjects.Add(creditsLabelRight);
		creditsLabelRight.pos.x -= creditsLabelRight.label.textRect.width / 2f;
		creditsLabelRight.pos.y -= creditsLabelRight.label.textRect.height / 2f;
		creditsLabelLeft.pos.y -= creditsLabelLeft.label.textRect.height / 2f;
		InitNextFile("start.txt");
	}

	public string SubstituteWildcards(string original)
	{
		string text = "";
		for (int i = 0; i < original.Length; i++)
		{
			text = ((original[i] != '$' || chars.Length <= 0) ? (text + original[i]) : (text + chars[UnityEngine.Random.Range(0, chars.Length)]));
		}
		return text;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = manager.rainWorld.screenSize.y * 0.25f + totalHeight * 0.5f;
		Vector2 vector = DrawPos(timeStacker);
		for (int i = 0; i < messageLabels.Count; i++)
		{
			messageLabels[i].pos = new Vector2(vector.x - messageWidths[i] * 0.5f, num - lineHeight * 0.6666f);
			if (i == showLine)
			{
				if (showText.Contains("$"))
				{
					messageLabels[i].label.text = SubstituteWildcards(showText);
				}
				else
				{
					messageLabels[i].label.text = showText;
				}
			}
			else if (i < showLine)
			{
				if (messages[i].text.Contains("$"))
				{
					messageLabels[i].label.text = SubstituteWildcards(messages[i].text);
				}
				else
				{
					messageLabels[i].label.text = messages[i].text;
				}
			}
			else
			{
				messageLabels[i].label.text = "";
			}
			num -= lineHeight * (float)messages[i].lines + messageSep;
		}
		for (int j = 0; j < messageButtons.Count; j++)
		{
			if (messageButtonTexts[j].Contains("$"))
			{
				messageButtons[j].menuLabel.text = SubstituteWildcards(messageButtonTexts[j]);
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		for (int i = 0; i < messageButtons.Count; i++)
		{
			if (message == "OPTION" + i)
			{
				if (optionFiles[i].ToLower() == "end")
				{
					manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
				}
				else
				{
					InitNextFile(optionFiles[i]);
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (manager.musicPlayer != null && manager.musicPlayer.song != null && musicTimes.ContainsKey(manager.musicPlayer.song.name) && manager.musicPlayer.song.subTracks[0] != null && manager.musicPlayer.song.subTracks[0].source != null && manager.musicPlayer.song.startedPlaying)
		{
			if (readySetMusicTime)
			{
				Custom.Log("SETTING TIME");
				manager.musicPlayer.song.subTracks[0].source.time = musicTimes[manager.musicPlayer.song.name];
				manager.musicPlayer.song.subTracks[0].piece.Loop = true;
				readySetMusicTime = false;
			}
			if (manager.musicPlayer.song.subTracks[0].source.clip != null && manager.musicPlayer.song.subTracks[0].source.time >= manager.musicPlayer.song.subTracks[0].source.clip.length)
			{
				manager.musicPlayer.song.subTracks[0].source.time = 0f;
			}
		}
		counter++;
		if (showCharacter < CurrentMessage.text.Length)
		{
			showCharacter = Mathf.Min(showCharacter + 8, CurrentMessage.text.Length);
			showText = CurrentMessage.text.Substring(0, showCharacter);
		}
		if (showCharacter >= CurrentMessage.text.Length)
		{
			lingerCounter++;
			if (lingerCounter > CurrentMessage.linger && showLine < messages.Count - 1)
			{
				InitNextMessage();
			}
		}
		if (slugcatFrame >= 0f)
		{
			UpdateSlugcatAnim();
		}
		if (otherFrame >= 0f)
		{
			UpdateOtherAnim();
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return new Vector2(CurrentMessage.xOrientation * manager.rainWorld.screenSize.x + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f - 50f, CurrentMessage.yPos);
	}

	public void InitNextFile(string filename)
	{
		if (filename == "start.txt")
		{
			creditsLabelLeft.label.alpha = 1f;
			creditsLabelRight.label.alpha = 1f;
			slugcat.pos.y = manager.rainWorld.options.ScreenSize.y * 0.7f - 20f;
		}
		else
		{
			creditsLabelLeft.label.alpha = 0f;
			creditsLabelRight.label.alpha = 0f;
			slugcat.pos.y = manager.rainWorld.options.ScreenSize.y * 0.7f;
		}
		string path = AssetManager.ResolveFilePath("Content" + Path.DirectorySeparatorChar + "text_" + LocalizationTranslator.LangShort(manager.rainWorld.inGameTranslator.currentLanguage) + Path.DirectorySeparatorChar + filename);
		string[] array;
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		else
		{
			path = AssetManager.ResolveFilePath("Content" + Path.DirectorySeparatorChar + "text_" + LocalizationTranslator.LangShort(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + filename);
			array = File.ReadAllLines(path);
		}
		showLine = 0;
		showCharacter = 0;
		showText = string.Empty;
		lingerCounter = 0;
		if (array[0].Contains("_anim_"))
		{
			string[] array2 = array[0].Split('_');
			slugcatSpeed = float.Parse(array2[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			slugcatFrame = 0f;
			slugcatBasename = array2[0];
			UpdateSlugcatAnim();
		}
		else
		{
			slugcatFrame = -1f;
			slugcatSpeed = 0f;
			slugcatBasename = "";
			slugcat.fileName = array[0];
			slugcat.LoadFile("Content");
			slugcat.sprite.SetElementByName(array[0]);
		}
		if (array[1].Contains("_anim_"))
		{
			string[] array3 = array[1].Split('_');
			otherSpeed = float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			otherFrame = 0f;
			otherBasename = array3[0];
			UpdateOtherAnim();
		}
		else
		{
			otherFrame = -1f;
			otherSpeed = 0f;
			otherBasename = "";
			other.fileName = array[1];
			other.LoadFile("Content");
			other.sprite.SetElementByName(array[1]);
		}
		int num = 0;
		messages = new List<Message>();
		for (int i = 3; i < array.Length; i++)
		{
			num = i;
			if (array[i] == "")
			{
				break;
			}
			NewMessage(array[i], 0);
		}
		totalHeight = 0f;
		for (int j = 0; j < messages.Count; j++)
		{
			totalHeight += lineHeight * (float)messages[j].lines;
			if (j != 0)
			{
				totalHeight += messageSep;
			}
		}
		List<string> list = new List<string>();
		optionFiles = new List<string>();
		int num2 = -1;
		for (int k = num + 1; k < array.Length; k += 2)
		{
			if (array[k] == "")
			{
				num2 = k;
				break;
			}
			list.Add(array[k]);
			optionFiles.Add(array[k + 1]);
		}
		if (num2 != -1 && manager.musicPlayer != null)
		{
			if (manager.musicPlayer.song != null)
			{
				musicTimes[manager.musicPlayer.song.name] = manager.musicPlayer.song.subTracks[0].source.time;
			}
			if (array[num2 + 1] == "MUTE" || array[num2 + 1] == "FADE")
			{
				manager.musicPlayer.FadeOutAllSongs(30f);
			}
			else
			{
				if (manager.musicPlayer.song != null)
				{
					manager.musicPlayer.song.StopAndDestroy();
					manager.musicPlayer.song = null;
				}
				manager.musicPlayer.MenuRequestsSong(array[num2 + 1], 1f, 0.7f);
				readySetMusicTime = true;
			}
		}
		for (int l = 0; l < messageLabels.Count; l++)
		{
			messageLabels[l].RemoveSprites();
			pages[0].subObjects.Remove(messageLabels[l]);
		}
		for (int m = 0; m < messageButtons.Count; m++)
		{
			messageButtons[m].RemoveSprites();
			pages[0].selectables.Remove(messageButtons[m]);
			pages[0].subObjects.Remove(messageButtons[m]);
		}
		messageLabels = new List<MenuLabel>();
		messageButtons = new List<SimpleButton>();
		messageButtonTexts = new List<string>();
		messageWidths = new float[messages.Count];
		for (int n = 0; n < messages.Count; n++)
		{
			MenuLabel menuLabel = new MenuLabel(this, pages[0], messages[n].text, new Vector2(0f, -1000f), new Vector2(100f, 30f), bigText: true);
			menuLabel.label.alignment = FLabelAlignment.Center;
			menuLabel.label.anchorX = 0f;
			menuLabel.label.anchorY = 1f;
			menuLabel.label.color = new Color(0.01f, 0.01f, 0.01f);
			messageWidths[n] = menuLabel.label.textRect.width;
			menuLabel.label.text = "";
			messageLabels.Add(menuLabel);
			pages[0].subObjects.Add(menuLabel);
		}
		float num3 = 270f * (float)list.Count;
		SimpleButton[] array4 = new SimpleButton[list.Count];
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			SimpleButton simpleButton = new SimpleButton(this, pages[0], list[num4], "OPTION" + num4, new Vector2(manager.rainWorld.options.ScreenSize.x * 0.5f - num3 * 0.5f + 270f * (float)num4 + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 15f), new Vector2(250f, 30f));
			simpleButton.buttonBehav.greyedOut = false;
			messageButtons.Add(simpleButton);
			messageButtonTexts.Add(list[num4]);
			pages[0].subObjects.Add(simpleButton);
			if (num4 == 0)
			{
				pages[0].lastSelectedObject = simpleButton;
			}
			array4[num4] = simpleButton;
		}
		float num5 = 0f;
		for (int num6 = 0; num6 < array4.Length; num6++)
		{
			float num7 = array4[num6].menuLabel.label.textRect.width + 20f;
			num5 += num7 + 10f;
			array4[num6].SetSize(new Vector2(num7, 30f));
		}
		float num8 = 0f;
		for (int num9 = 0; num9 < array4.Length; num9++)
		{
			array4[num9].pos.x = manager.rainWorld.options.ScreenSize.x * 0.5f - num5 * 0.5f + num8 + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			num8 += array4[num9].size.x + 10f;
		}
		GrafUpdate(0f);
	}

	public void NewMessage(string text, int extraLinger)
	{
		NewMessage(text, defaultXOrientation, defaultYPos, extraLinger);
	}

	public void NewMessage(string text, float xOrientation, float yPos, int extraLinger)
	{
		messages.Add(new Message(text, xOrientation, yPos, extraLinger));
		if (messages.Count == 1)
		{
			showLine = -1;
			InitNextMessage();
		}
	}

	private void InitNextMessage()
	{
		showCharacter = 0;
		showText = string.Empty;
		showLine++;
		lingerCounter = 0;
	}

	public void UpdateSlugcatAnim()
	{
		slugcatFrame += slugcatSpeed;
		string text = ((int)slugcatFrame).ToString("000");
		if (!File.Exists(AssetManager.ResolveFilePath("Content" + Path.DirectorySeparatorChar + slugcatBasename + "_" + text + ".png")))
		{
			slugcatFrame = 1f;
			text = ((int)slugcatFrame).ToString("000");
		}
		slugcat.fileName = slugcatBasename + "_" + text;
		slugcat.LoadFile("Content");
		slugcat.sprite.SetElementByName(slugcat.fileName);
	}

	public void UpdateOtherAnim()
	{
		otherFrame += otherSpeed;
		string text = ((int)otherFrame).ToString("000");
		if (!File.Exists(AssetManager.ResolveFilePath("Content" + Path.DirectorySeparatorChar + otherBasename + "_" + text + ".png")))
		{
			otherFrame = 1f;
			text = ((int)otherFrame).ToString("000");
		}
		other.fileName = otherBasename + "_" + text;
		if (Futile.atlasManager.GetAtlasWithName(other.fileName) == null)
		{
			other.LoadFile("Content");
		}
		other.sprite.SetElementByName(other.fileName);
	}

	public void ExitToCredits()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (passthroughPackage != null && nextProcess is EndCredits)
		{
			(nextProcess as EndCredits).passthroughPackage = passthroughPackage;
		}
	}
}
