using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CollectionsMenu : global::Menu.Menu, Conversation.IOwnAConversation
{
	public class PearlReadContext : ExtEnum<PearlReadContext>
	{
		public static readonly PearlReadContext StandardMoon = new PearlReadContext("StandardMoon", register: true);

		public static readonly PearlReadContext PastMoon = new PearlReadContext("PastMoon", register: true);

		public static readonly PearlReadContext FutureMoon = new PearlReadContext("FutureMoon", register: true);

		public static readonly PearlReadContext Pebbles = new PearlReadContext("Pebbles", register: true);

		public static readonly PearlReadContext UnreadPebbles = new PearlReadContext("UnreadPebbles", register: true);

		public static readonly PearlReadContext UnreadMoon = new PearlReadContext("UnreadMoon", register: true);

		public PearlReadContext(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ConversationLoader : Conversation
	{
		public ConversationLoader(IOwnAConversation interfaceOwner)
			: base(interfaceOwner, ID.None, null)
		{
		}

		public void LoadEvents(int id, SlugcatStats.Name saveFile)
		{
			LoadEventsFromFile(id, saveFile, oneRandomLine: false, 0);
		}
	}

	public SimpleButton backButton;

	private FSprite darkSprite;

	private FSprite[] pearlSprites;

	private SimpleButton[] pearlButtons;

	public DataPearl.AbstractDataPearl.DataPearlType[] usedPearlTypes;

	public List<ChatlogData.ChatlogID> usedChatlogs;

	private SimpleButton[] chatlogButtons;

	public RoundedRect textBoxBorder;

	private FSprite textBoxBack;

	public MenuLabel[] labels;

	public FSprite[] chatlogSprites;

	public List<ChatlogData.ChatlogID> prePebsBroadcastChatlogs;

	public List<ChatlogData.ChatlogID> postPebsBroadcastChatlogs;

	public bool debug_enableAllButtons;

	private FSprite[] iteratorSprites;

	private SimpleButton[] iteratorButtons;

	public int selectedPearlInd;

	public static int ITEMS_PER_COLUMN = 13;

	public static float BUTTON_SIZE = 50f;

	private bool lastPauseButton;

	private bool exiting;

	public RainWorld rainWorld => manager.rainWorld;

	public CollectionsMenu(ProcessManager manager)
		: base(manager, MoreSlugcatsEnums.ProcessID.Collections)
	{
		debug_enableAllButtons = MoreSlugcats.chtUnlockCollections.Value;
		pages.Add(new Page(this, null, "main", 0));
		scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
		pages[0].subObjects.Add(scene);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.85f;
		pages[0].Container.AddChild(darkSprite);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(195f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		backButton.nextSelectable[0] = backButton;
		backButton.nextSelectable[2] = backButton;
		usedPearlTypes = GetCollectionReadablePearls();
		pearlButtons = new SimpleButton[usedPearlTypes.Length];
		pearlSprites = new FSprite[pearlButtons.Length];
		usedChatlogs = new List<ChatlogData.ChatlogID>();
		prePebsBroadcastChatlogs = new List<ChatlogData.ChatlogID>();
		postPebsBroadcastChatlogs = new List<ChatlogData.ChatlogID>();
		List<string> fullRegionOrder = Region.GetFullRegionOrder();
		for (int i = 0; i < fullRegionOrder.Count; i++)
		{
			if (!manager.rainWorld.regionGreyTokens.ContainsKey(fullRegionOrder[i].ToLowerInvariant()))
			{
				continue;
			}
			foreach (ChatlogData.ChatlogID item in manager.rainWorld.regionGreyTokens[fullRegionOrder[i].ToLowerInvariant()])
			{
				if (item.Index >= ChatlogData.ChatlogID.Chatlog_Broadcast0.Index && item.Index <= ChatlogData.ChatlogID.Chatlog_Broadcast19.Index)
				{
					if (File.Exists(ChatlogData.linearChatlogPath(item.Index - ChatlogData.ChatlogID.Chatlog_Broadcast0.Index, postPebbles: false)) && !prePebsBroadcastChatlogs.Contains(item))
					{
						prePebsBroadcastChatlogs.Add(item);
					}
					if (File.Exists(ChatlogData.linearChatlogPath(item.Index - ChatlogData.ChatlogID.Chatlog_Broadcast0.Index, postPebbles: true)) && !postPebsBroadcastChatlogs.Contains(item))
					{
						postPebsBroadcastChatlogs.Add(item);
					}
				}
				else
				{
					usedChatlogs.Add(item);
				}
			}
		}
		chatlogButtons = new SimpleButton[usedChatlogs.Count + prePebsBroadcastChatlogs.Count + postPebsBroadcastChatlogs.Count];
		chatlogSprites = new FSprite[chatlogButtons.Length];
		float num = 650f;
		float num2 = 250f;
		int num3 = pearlButtons.Length + chatlogButtons.Length;
		ITEMS_PER_COLUMN = (int)(num / BUTTON_SIZE);
		int num4 = (int)(num2 / BUTTON_SIZE);
		while (ITEMS_PER_COLUMN * num4 < num3)
		{
			BUTTON_SIZE = (int)(BUTTON_SIZE * 0.9f);
			ITEMS_PER_COLUMN = (int)(num / BUTTON_SIZE);
			num4 = (int)(num2 / BUTTON_SIZE);
		}
		for (int j = 0; j < pearlButtons.Length; j++)
		{
			int num5 = j % ITEMS_PER_COLUMN;
			int num6 = j / ITEMS_PER_COLUMN;
			pearlButtons[j] = new SimpleButton(this, pages[0], "", "PEARL" + j, new Vector2(90f + (float)num6 * BUTTON_SIZE, 765f - (float)num5 * BUTTON_SIZE) + new Vector2(106f, -60f), new Vector2(BUTTON_SIZE - 8f, BUTTON_SIZE - 8f));
			if (usedPearlTypes[j] == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
			{
				if (DMStoryFinished())
				{
					pearlSprites[j] = new FSprite("Symbol_Pearl");
				}
				else
				{
					pearlSprites[j] = new FSprite("Symbol_Unknown");
				}
			}
			else if (!manager.rainWorld.progression.miscProgressionData.GetPearlDeciphered(usedPearlTypes[j]) && !manager.rainWorld.progression.miscProgressionData.GetPebblesPearlDeciphered(usedPearlTypes[j]) && !manager.rainWorld.progression.miscProgressionData.GetDMPearlDeciphered(usedPearlTypes[j]) && (!manager.rainWorld.progression.miscProgressionData.GetFuturePearlDeciphered(usedPearlTypes[j]) || usedPearlTypes[j] == DataPearl.AbstractDataPearl.DataPearlType.CC) && !debug_enableAllButtons)
			{
				pearlSprites[j] = new FSprite("Symbol_Unknown");
			}
			else
			{
				pearlSprites[j] = new FSprite("Symbol_Pearl");
			}
			pearlSprites[j].x = pearlButtons[j].pos.x + pearlButtons[j].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			pearlSprites[j].y = pearlButtons[j].pos.y + pearlButtons[j].size.y / 2f;
			Color a = DataPearl.UniquePearlMainColor(usedPearlTypes[j]);
			Color? color = DataPearl.UniquePearlHighLightColor(usedPearlTypes[j]);
			a = ((!color.HasValue) ? Color.Lerp(a, Color.white, 0.15f) : Custom.Screen(a, color.Value * Custom.QuickSaturation(color.Value) * 0.5f));
			if (a.r < 0.1f && a.g < 0.1f && a.b < 0.1f)
			{
				a = Color.Lerp(a, global::Menu.Menu.MenuRGB(MenuColors.MediumGrey), 0.3f);
			}
			pearlSprites[j].color = a;
			if (pearlSprites[j].element.name == "Symbol_Unknown")
			{
				pearlButtons[j].buttonBehav.greyedOut = true;
				pearlSprites[j].color = global::Menu.Menu.MenuRGB(MenuColors.DarkGrey);
			}
			pages[0].subObjects.Add(pearlButtons[j]);
			pages[0].Container.AddChild(pearlSprites[j]);
		}
		for (int k = 0; k < chatlogButtons.Length; k++)
		{
			int num7 = (k + pearlButtons.Length) % ITEMS_PER_COLUMN;
			int num8 = (k + pearlButtons.Length) / ITEMS_PER_COLUMN;
			Color color2 = Color.white;
			bool flag = true;
			string singalText;
			if (k < prePebsBroadcastChatlogs.Count)
			{
				singalText = "CHATLOG_PREPEB_" + k;
				color2 = Color.white;
				if (manager.rainWorld.progression.miscProgressionData.prePebblesBroadcasts <= k && manager.rainWorld.progression.miscProgressionData.postPebblesBroadcasts <= k && !debug_enableAllButtons)
				{
					flag = false;
				}
			}
			else if (k >= prePebsBroadcastChatlogs.Count && k < prePebsBroadcastChatlogs.Count + postPebsBroadcastChatlogs.Count)
			{
				singalText = "CHATLOG_POSTPEB_" + (k - prePebsBroadcastChatlogs.Count);
				color2 = Color.gray;
				if (manager.rainWorld.progression.miscProgressionData.postPebblesBroadcasts <= k - prePebsBroadcastChatlogs.Count && !debug_enableAllButtons)
				{
					flag = false;
				}
			}
			else
			{
				int index = k - (prePebsBroadcastChatlogs.Count + postPebsBroadcastChatlogs.Count);
				if (!manager.rainWorld.progression.miscProgressionData.GetBroadcastListened(usedChatlogs[index]) && !debug_enableAllButtons)
				{
					flag = false;
				}
				singalText = "CHATLOG_NORMAL_" + index;
				for (int l = 0; l < fullRegionOrder.Count; l++)
				{
					string text = usedChatlogs[index].ToString();
					if (Region.EquivalentRegion(text.Substring(text.Length - 3, 2), fullRegionOrder[l]))
					{
						color2 = Region.RegionColor(fullRegionOrder[l]);
						break;
					}
				}
			}
			chatlogButtons[k] = new SimpleButton(this, pages[0], "", singalText, new Vector2(90f + (float)num8 * BUTTON_SIZE, 765f - (float)num7 * BUTTON_SIZE) + new Vector2(106f, -60f), new Vector2(BUTTON_SIZE - 8f, BUTTON_SIZE - 8f));
			if (!flag)
			{
				chatlogSprites[k] = new FSprite("Symbol_Unknown");
			}
			else
			{
				chatlogSprites[k] = new FSprite("Symbol_Satellite");
			}
			chatlogSprites[k].x = chatlogButtons[k].pos.x + chatlogButtons[k].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			chatlogSprites[k].y = chatlogButtons[k].pos.y + chatlogButtons[k].size.y / 2f;
			chatlogSprites[k].color = color2;
			if (!flag)
			{
				chatlogButtons[k].buttonBehav.greyedOut = true;
				chatlogSprites[k].color = global::Menu.Menu.MenuRGB(MenuColors.DarkGrey);
			}
			pages[0].Container.AddChild(chatlogSprites[k]);
			pages[0].subObjects.Add(chatlogButtons[k]);
		}
		textBoxBorder = new RoundedRect(this, pages[0], new Vector2(455f, 50f), new Vector2(720f, 700f), filled: true);
		textBoxBack = new FSprite("pixel");
		textBoxBack.color = new Color(0f, 0f, 0f);
		textBoxBack.anchorX = 0f;
		textBoxBack.anchorY = 0f;
		textBoxBack.scaleX = textBoxBorder.size.x - 12f;
		textBoxBack.scaleY = textBoxBorder.size.y - 12f;
		textBoxBack.x = textBoxBorder.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
		textBoxBack.y = textBoxBorder.pos.y + 6f;
		textBoxBack.alpha = 0.65f;
		infoLabel.x = Mathf.Ceil(textBoxBack.x + textBoxBack.scaleX / 2f);
		pages[0].Container.AddChild(textBoxBack);
		pages[0].subObjects.Add(textBoxBorder);
		labels = new MenuLabel[20];
		for (int m = 0; m < labels.Length; m++)
		{
			labels[m] = new MenuLabel(this, pages[0], "", Vector2.zero, Vector2.zero, bigText: false);
			pages[0].subObjects.Add(labels[m]);
		}
		labels[0].text = Translate("[ Collection Empty ]");
		RefreshLabelPositions();
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	public override void Update()
	{
		int num = (int)Math.Max(0f, Mathf.Floor((float)(pearlButtons.Length + chatlogButtons.Length - 1) / (float)ITEMS_PER_COLUMN) * (float)ITEMS_PER_COLUMN);
		if (num >= pearlButtons.Length)
		{
			int num2 = pearlButtons.Length + chatlogButtons.Length - num;
			_ = chatlogButtons.Length;
		}
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null)
		{
			OnExit();
		}
		lastPauseButton = flag;
		for (int i = 0; i < Math.Min(ITEMS_PER_COLUMN, pearlButtons.Length); i++)
		{
			pearlButtons[i].nextSelectable[0] = pearlButtons[i];
		}
		for (int j = pearlButtons.Length; j < ITEMS_PER_COLUMN; j++)
		{
			chatlogButtons[pearlButtons.Length - j].nextSelectable[0] = chatlogButtons[pearlButtons.Length - j];
		}
		if (iteratorButtons != null)
		{
			for (int k = 0; k < pearlButtons.Length; k++)
			{
				if (pearlButtons[k].toggled)
				{
					iteratorButtons[iteratorButtons.Length - 1].nextSelectable[0] = pearlButtons[k];
					for (int l = 0; l < iteratorButtons.Length; l++)
					{
						iteratorButtons[l].nextSelectable[1] = pearlButtons[k];
						iteratorButtons[l].nextSelectable[3] = pearlButtons[k];
					}
				}
			}
			for (int m = 0; m < chatlogButtons.Length; m++)
			{
				if (chatlogButtons[m].toggled)
				{
					iteratorButtons[iteratorButtons.Length - 1].nextSelectable[0] = chatlogButtons[m];
					for (int n = 0; n < iteratorButtons.Length; n++)
					{
						iteratorButtons[n].nextSelectable[1] = chatlogButtons[m];
						iteratorButtons[n].nextSelectable[3] = chatlogButtons[m];
					}
				}
			}
			iteratorButtons[0].nextSelectable[2] = iteratorButtons[0];
			for (int num3 = 0; num3 < pearlButtons.Length; num3++)
			{
				if (num3 + ITEMS_PER_COLUMN >= pearlButtons.Length + chatlogButtons.Length)
				{
					pearlButtons[num3].nextSelectable[2] = iteratorButtons[iteratorButtons.Length - 1];
				}
			}
			for (int num4 = 0; num4 < chatlogButtons.Length; num4++)
			{
				if (num4 + pearlButtons.Length + ITEMS_PER_COLUMN >= pearlButtons.Length + chatlogButtons.Length)
				{
					chatlogButtons[num4].nextSelectable[2] = iteratorButtons[iteratorButtons.Length - 1];
				}
			}
		}
		else
		{
			for (int num5 = 0; num5 < pearlButtons.Length; num5++)
			{
				if (num5 + ITEMS_PER_COLUMN >= pearlButtons.Length + chatlogButtons.Length)
				{
					pearlButtons[num5].nextSelectable[2] = pearlButtons[num5];
				}
			}
			for (int num6 = 0; num6 < chatlogButtons.Length; num6++)
			{
				if (num6 + pearlButtons.Length + ITEMS_PER_COLUMN >= pearlButtons.Length + chatlogButtons.Length)
				{
					chatlogButtons[num6].nextSelectable[2] = chatlogButtons[num6];
				}
			}
		}
		base.Update();
	}

	public void OnExit()
	{
		if (!exiting)
		{
			exiting = true;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "BACK")
		{
			OnExit();
		}
		if (message.Contains("CHATLOG"))
		{
			ClearIteratorButtons();
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			int num = 0;
			int num2 = int.Parse(message.Substring(message.LastIndexOf("_") + 1), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (message.Contains("NORMAL"))
			{
				num = num2 + prePebsBroadcastChatlogs.Count + postPebsBroadcastChatlogs.Count;
				InitLabelsFromChatlog(ChatlogData.getChatlog(usedChatlogs[num2]));
			}
			else if (message.Contains("PREPEB"))
			{
				num = num2;
				InitLabelsFromChatlog(ChatlogData.getLinearBroadcast(num2, postPebbles: false));
			}
			else if (message.Contains("POSTPEB"))
			{
				num = num2 + prePebsBroadcastChatlogs.Count;
				InitLabelsFromChatlog(ChatlogData.getLinearBroadcast(num2, postPebbles: true));
			}
			for (int i = 0; i < pearlButtons.Length; i++)
			{
				pearlButtons[i].toggled = false;
			}
			for (int j = 0; j < chatlogButtons.Length; j++)
			{
				chatlogButtons[j].toggled = false;
			}
			chatlogButtons[num].toggled = true;
		}
		if (!message.Contains("PEARL") && !message.Contains("TYPE"))
		{
			return;
		}
		PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		if (message.Contains("PEARL"))
		{
			selectedPearlInd = int.Parse(message.Substring(5), NumberStyles.Any, CultureInfo.InvariantCulture);
			for (int k = 0; k < pearlButtons.Length; k++)
			{
				pearlButtons[k].toggled = false;
			}
			for (int l = 0; l < chatlogButtons.Length; l++)
			{
				chatlogButtons[l].toggled = false;
			}
			pearlButtons[selectedPearlInd].toggled = true;
		}
		DataPearl.AbstractDataPearl.DataPearlType dataPearlType = usedPearlTypes[selectedPearlInd];
		int num3 = DataPearlToFileID(dataPearlType);
		int num4 = -1;
		if (dataPearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
		{
			num4 = 100;
		}
		if (dataPearlType == MoreSlugcatsEnums.DataPearlType.RM)
		{
			num3 = 106;
			num4 = 120;
		}
		if (num3 == -1 && num4 == -1)
		{
			InitLabelsFromChatlog(new string[1] { Translate("Unknown Pearl Contents...") });
			return;
		}
		PearlReadContext pearlReadContext = PearlReadContext.UnreadMoon;
		if (message.Contains("PEARL"))
		{
			pearlReadContext = AddIteratorButtons(num3, num4);
			if (pearlReadContext == PearlReadContext.UnreadPebbles)
			{
				pearlReadContext = PearlReadContext.UnreadMoon;
				num3 = num4;
			}
		}
		else
		{
			for (int m = 0; m < iteratorButtons.Length; m++)
			{
				if (iteratorButtons[m].signalText == message)
				{
					iteratorButtons[m].toggled = true;
				}
				else
				{
					iteratorButtons[m].toggled = false;
				}
				if (message.Contains("PEBBLES"))
				{
					if (num4 != -1)
					{
						pearlReadContext = PearlReadContext.UnreadMoon;
						num3 = num4;
					}
					else
					{
						pearlReadContext = PearlReadContext.Pebbles;
					}
				}
				if (message.Contains("DM"))
				{
					pearlReadContext = PearlReadContext.PastMoon;
				}
				if (message.Contains("FUTURE"))
				{
					pearlReadContext = PearlReadContext.FutureMoon;
				}
			}
		}
		if (dataPearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl && (message.Contains("DM") || message.Contains("PEARL")))
		{
			InitLabelsFromChatlog(ChatlogData.getChatlog(ChatlogData.ChatlogID.Chatlog_SI9));
			return;
		}
		SlugcatStats.Name saveFile = null;
		if (pearlReadContext == PearlReadContext.Pebbles)
		{
			saveFile = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
		}
		if (pearlReadContext == PearlReadContext.PastMoon)
		{
			saveFile = MoreSlugcatsEnums.SlugcatStatsName.Spear;
		}
		if (pearlReadContext == PearlReadContext.FutureMoon)
		{
			saveFile = MoreSlugcatsEnums.SlugcatStatsName.Saint;
		}
		InitLabelsFromPearlFile(num3, saveFile);
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is SimpleButton)
		{
			if ((selectedObject as SimpleButton).buttonBehav.greyedOut)
			{
				return Translate("Undiscovered Transcription");
			}
			if ((selectedObject as SimpleButton).signalText == "TYPE_MOON")
			{
				return Translate("Looks to the Moon's Transcription");
			}
			if ((selectedObject as SimpleButton).signalText == "TYPE_PEBBLES")
			{
				return Translate("Five Pebble's Transcription");
			}
			if ((selectedObject as SimpleButton).signalText == "TYPE_DM")
			{
				return Translate("Looks to the Moon's Transcription (Pre-Collapse)");
			}
			if ((selectedObject as SimpleButton).signalText == "TYPE_FUTURE")
			{
				return Translate("Looks to the Moon's Transcription (Future)");
			}
		}
		return base.UpdateInfoText();
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		darkSprite.RemoveFromContainer();
		textBoxBack.RemoveFromContainer();
		for (int i = 0; i < pearlSprites.Length; i++)
		{
			pearlSprites[i].RemoveFromContainer();
		}
		for (int j = 0; j < chatlogSprites.Length; j++)
		{
			chatlogSprites[j].RemoveFromContainer();
		}
		ClearIteratorButtons();
		if (manager.rainWorld.options.musicVolume == 0f && manager.musicPlayer != null)
		{
			manager.StopSideProcess(manager.musicPlayer);
		}
	}

	public void RefreshLabelPositions()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 10f;
		if (base.CurrLang == InGameTranslator.LanguageID.Japanese)
		{
			int num4 = 0;
			for (int i = 0; i < labels.Length; i++)
			{
				if (labels[i].text != "")
				{
					num4 += 1 + labels[i].label.text.Count((char f) => f == '\n');
				}
			}
			if (num4 >= 20)
			{
				num3 = 0f;
			}
		}
		for (int j = 0; j < labels.Length; j++)
		{
			if (labels[j].text != "")
			{
				num += labels[j].label.textRect.height + num3;
			}
		}
		float num5 = textBoxBorder.pos.x + textBoxBorder.size.x / 2f;
		float num6 = textBoxBorder.pos.y + textBoxBorder.size.y / 2f;
		for (int k = 0; k < labels.Length; k++)
		{
			if (labels[k].text != "")
			{
				labels[k].pos.y = (int)(num6 + num / 2f - num2 - labels[k].label.textRect.height / 2f);
				labels[k].pos.x = (int)num5;
				num2 += labels[k].label.textRect.height + num3;
			}
		}
	}

	public void ResetLabels()
	{
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].text = "";
			labels[i].label.color = Color.white;
		}
	}

	public string[] RemoveBlankMessages(string[] messages)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < messages.Length; i++)
		{
			if (messages[i].Trim() != string.Empty)
			{
				list.Add(messages[i]);
			}
		}
		return list.ToArray();
	}

	public void InitLabelsFromChatlog(string[] messages)
	{
		ResetLabels();
		messages = RemoveBlankMessages(messages);
		string path = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "colors.txt");
		if (File.Exists(path))
		{
			File.ReadAllLines(path);
		}
		for (int i = 0; i < Mathf.Min(labels.Length, messages.Length); i++)
		{
			labels[i].text = ReplaceParts(messages[i]);
			WordWrapLabel(labels[i].label, 650f);
			if (Conversation.TryGetPrefixColor(messages[i], out var result))
			{
				labels[i].label.color = result;
			}
		}
		RefreshLabelPositions();
	}

	public string ReplaceParts(string s)
	{
		s = Regex.Replace(s, "<PLAYERNAME>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CAPPLAYERNAME>", NameForPlayer(capitalized: true));
		s = Regex.Replace(s, "<PlayerName>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CapPlayerName>", NameForPlayer(capitalized: true));
		return s;
	}

	protected string NameForPlayer(bool capitalized)
	{
		string text = Translate("creature");
		string text2 = Translate("little");
		if (capitalized && InGameTranslator.LanguageID.UsesCapitals(manager.rainWorld.inGameTranslator.currentLanguage))
		{
			text2 = char.ToUpper(text2[0]) + text2.Substring(1);
		}
		return text2 + " " + text;
	}

	public void SpecialEvent(string eventName)
	{
	}

	public void WordWrapLabel(FLabel label, float maxWidth)
	{
		StringBuilder stringBuilder = new StringBuilder(label.text);
		if (!InGameTranslator.LanguageID.WordWrappingAllowed(base.CurrLang))
		{
			label.text = Custom.ReplaceWordWrapLineDelimeters(stringBuilder.ToString());
			return;
		}
		if (InGameTranslator.LanguageID.UsesLargeFont(base.CurrLang))
		{
			stringBuilder = stringBuilder.Replace("<LINE>", " ");
			label.text = stringBuilder.ToString().WrapText(bigText: false, maxWidth, forceWrapping: true);
			return;
		}
		stringBuilder = stringBuilder.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("  ", " ");
		string[] array = Regex.Split(stringBuilder.ToString(), "<LINE>");
		stringBuilder.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length < 1)
			{
				continue;
			}
			string text = array[i];
			int num = 1;
			while (DoesExceedWidth(text) && num++ < 4)
			{
				text = array[i];
				for (int j = 1; j < num; j++)
				{
					int num2 = text.Substring(text.Length * j / num).IndexOf(" ");
					text = text.Insert(text.Length * j / num + num2, Environment.NewLine);
				}
			}
			stringBuilder.Append(text);
			if (i < array.Length - 1)
			{
				stringBuilder.Append(Environment.NewLine);
			}
		}
		label.text = stringBuilder.ToString();
		bool DoesExceedWidth(string T)
		{
			string[] array2 = T.Split(Environment.NewLine.ToCharArray());
			for (int k = 0; k < array2.Length; k++)
			{
				if (LabelTest.GetWidth(array2[k]) > maxWidth)
				{
					return true;
				}
			}
			return false;
		}
	}

	public PearlReadContext AddIteratorButtons(int pearlIndex, int pebAltPearlIndex)
	{
		bool flag = DMStoryFinished();
		bool flag2 = Conversation.EventsFileExists(rainWorld, pearlIndex);
		bool flag3 = Conversation.EventsFileExists(rainWorld, pearlIndex, MoreSlugcatsEnums.SlugcatStatsName.Spear);
		bool flag4 = Conversation.EventsFileExists(rainWorld, pearlIndex, MoreSlugcatsEnums.SlugcatStatsName.Saint);
		bool flag5 = Conversation.EventsFileExists(rainWorld, pearlIndex, MoreSlugcatsEnums.SlugcatStatsName.Artificer);
		bool flag6 = pebAltPearlIndex != -1 && Conversation.EventsFileExists(rainWorld, pebAltPearlIndex);
		bool flag7 = flag2 && (debug_enableAllButtons || manager.rainWorld.progression.miscProgressionData.GetPearlDeciphered(usedPearlTypes[selectedPearlInd]));
		bool flag8 = flag4 && (debug_enableAllButtons || manager.rainWorld.progression.miscProgressionData.GetFuturePearlDeciphered(usedPearlTypes[selectedPearlInd]));
		bool flag9 = flag3 && (debug_enableAllButtons || manager.rainWorld.progression.miscProgressionData.GetDMPearlDeciphered(usedPearlTypes[selectedPearlInd]) || (flag && manager.rainWorld.progression.miscProgressionData.GetPearlDeciphered(usedPearlTypes[selectedPearlInd])));
		bool flag10 = (flag5 || flag6) && (debug_enableAllButtons || manager.rainWorld.progression.miscProgressionData.GetPebblesPearlDeciphered(usedPearlTypes[selectedPearlInd]));
		if (flag2 && !flag4 && manager.rainWorld.progression.miscProgressionData.GetFuturePearlDeciphered(usedPearlTypes[selectedPearlInd]))
		{
			flag7 = true;
		}
		if (flag2 && !flag5 && !flag6 && manager.rainWorld.progression.miscProgressionData.GetPebblesPearlDeciphered(usedPearlTypes[selectedPearlInd]))
		{
			flag7 = true;
		}
		if (flag2 && !flag3 && manager.rainWorld.progression.miscProgressionData.GetDMPearlDeciphered(usedPearlTypes[selectedPearlInd]))
		{
			flag7 = true;
		}
		if (pebAltPearlIndex == 100)
		{
			flag7 = false;
			flag9 = flag;
			flag3 = true;
			flag2 = false;
			flag6 = false;
			flag10 = false;
		}
		ClearIteratorButtons();
		int num = 0;
		if (flag2)
		{
			num++;
		}
		if (flag3)
		{
			num++;
		}
		if (flag5 || flag6)
		{
			num++;
		}
		if (flag4)
		{
			num++;
		}
		if (num == 0)
		{
			return PearlReadContext.UnreadMoon;
		}
		iteratorSprites = new FSprite[num];
		iteratorButtons = new SimpleButton[num];
		int num2 = 0;
		float num3 = 10f;
		float num4 = 42f;
		Vector2 vector = new Vector2((int)(textBoxBorder.pos.x + textBoxBorder.size.x - num4 - 20f), (int)(textBoxBorder.pos.y + textBoxBorder.size.y - num4 - 20f));
		PearlReadContext pearlReadContext = PearlReadContext.UnreadMoon;
		if (flag2)
		{
			iteratorButtons[num2] = new SimpleButton(this, pages[0], "", "TYPE_MOON", new Vector2((int)(vector.x - (num3 + num4) * (float)num2), vector.y), new Vector2(num4, num4));
			iteratorButtons[num2].buttonBehav.greyedOut = !flag7;
			iteratorSprites[num2] = new FSprite(flag7 ? "GuidanceMoon" : "Sandbox_SmallQuestionmark");
			iteratorSprites[num2].x = (int)(iteratorButtons[num2].pos.x + iteratorButtons[num2].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f);
			iteratorSprites[num2].y = (int)(iteratorButtons[num2].pos.y + iteratorButtons[num2].size.y / 2f);
			pages[0].subObjects.Add(iteratorButtons[num2]);
			pages[0].Container.AddChild(iteratorSprites[num2]);
			if (pearlReadContext == PearlReadContext.UnreadMoon && flag7)
			{
				iteratorButtons[num2].toggled = true;
				pearlReadContext = PearlReadContext.StandardMoon;
			}
			num2++;
		}
		if (flag3)
		{
			iteratorButtons[num2] = new SimpleButton(this, pages[0], "", "TYPE_DM", new Vector2((int)(vector.x - (num3 + num4) * (float)num2), vector.y), new Vector2(num4, num4));
			iteratorButtons[num2].buttonBehav.greyedOut = !flag9;
			iteratorSprites[num2] = new FSprite(flag9 ? "GuidanceMoon" : "Sandbox_SmallQuestionmark");
			iteratorSprites[num2].color = Color.yellow;
			iteratorSprites[num2].x = (int)(iteratorButtons[num2].pos.x + iteratorButtons[num2].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f);
			iteratorSprites[num2].y = (int)(iteratorButtons[num2].pos.y + iteratorButtons[num2].size.y / 2f);
			pages[0].subObjects.Add(iteratorButtons[num2]);
			pages[0].Container.AddChild(iteratorSprites[num2]);
			if (pearlReadContext == PearlReadContext.UnreadMoon && flag9)
			{
				iteratorButtons[num2].toggled = true;
				pearlReadContext = PearlReadContext.PastMoon;
			}
			num2++;
		}
		if (flag5 || flag6)
		{
			iteratorButtons[num2] = new SimpleButton(this, pages[0], "", "TYPE_PEBBLES", new Vector2((int)(vector.x - (num3 + num4) * (float)num2), vector.y), new Vector2(num4, num4));
			iteratorButtons[num2].buttonBehav.greyedOut = !flag10;
			iteratorSprites[num2] = new FSprite(flag10 ? "GuidancePebbles" : "Sandbox_SmallQuestionmark");
			iteratorSprites[num2].color = new Color(38f / 85f, 46f / 51f, 0.76862746f);
			iteratorSprites[num2].x = (int)(iteratorButtons[num2].pos.x + iteratorButtons[num2].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f);
			iteratorSprites[num2].y = (int)(iteratorButtons[num2].pos.y + iteratorButtons[num2].size.y / 2f);
			pages[0].subObjects.Add(iteratorButtons[num2]);
			pages[0].Container.AddChild(iteratorSprites[num2]);
			if (pearlReadContext == PearlReadContext.UnreadMoon && flag10)
			{
				iteratorButtons[num2].toggled = true;
				pearlReadContext = ((!flag5) ? PearlReadContext.UnreadPebbles : PearlReadContext.Pebbles);
			}
			num2++;
		}
		if (flag4)
		{
			iteratorButtons[num2] = new SimpleButton(this, pages[0], "", "TYPE_FUTURE", new Vector2((int)(vector.x - (num3 + num4) * (float)num2), vector.y), new Vector2(num4, num4));
			iteratorButtons[num2].buttonBehav.greyedOut = !flag8;
			iteratorSprites[num2] = new FSprite(flag8 ? "GuidanceMoon" : "Sandbox_SmallQuestionmark");
			iteratorSprites[num2].color = new Color(0.29411766f, 0.45490196f, 0.5254902f);
			iteratorSprites[num2].x = (int)(iteratorButtons[num2].pos.x + iteratorButtons[num2].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f);
			iteratorSprites[num2].y = (int)(iteratorButtons[num2].pos.y + iteratorButtons[num2].size.y / 2f);
			pages[0].subObjects.Add(iteratorButtons[num2]);
			pages[0].Container.AddChild(iteratorSprites[num2]);
			if (pearlReadContext == PearlReadContext.UnreadMoon && flag8)
			{
				iteratorButtons[num2].toggled = true;
				pearlReadContext = PearlReadContext.FutureMoon;
			}
			num2++;
		}
		if (pebAltPearlIndex == 100)
		{
			return PearlReadContext.UnreadPebbles;
		}
		if (pearlReadContext == PearlReadContext.StandardMoon)
		{
			return PearlReadContext.UnreadMoon;
		}
		return pearlReadContext;
	}

	public void ClearIteratorButtons()
	{
		if (iteratorSprites != null)
		{
			for (int i = 0; i < iteratorSprites.Length; i++)
			{
				iteratorSprites[i].RemoveFromContainer();
			}
			iteratorSprites = null;
		}
		if (iteratorButtons != null)
		{
			for (int j = 0; j < iteratorButtons.Length; j++)
			{
				iteratorButtons[j].RemoveSprites();
				pages[0].RemoveSubObject(iteratorButtons[j]);
			}
			iteratorButtons = null;
		}
	}

	public void InitLabelsFromPearlFile(int id, SlugcatStats.Name saveFile)
	{
		ConversationLoader conversationLoader = new ConversationLoader(this);
		conversationLoader.LoadEvents(id, saveFile);
		List<string> list = new List<string>();
		for (int i = 0; i < conversationLoader.events.Count; i++)
		{
			if (conversationLoader.events[i] is Conversation.TextEvent)
			{
				list.Add((conversationLoader.events[i] as Conversation.TextEvent).text);
			}
		}
		InitLabelsFromChatlog(list.ToArray());
	}

	public bool DMStoryFinished()
	{
		if (debug_enableAllButtons)
		{
			return true;
		}
		if (manager.rainWorld.progression.miscProgressionData.beaten_SpearMaster_AltEnd)
		{
			return true;
		}
		if (!manager.rainWorld.progression.IsThereASavedGame(MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			return false;
		}
		if (manager.rainWorld.progression.currentSaveState != null && manager.rainWorld.progression.currentSaveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding;
		}
		string[] progLinesFromMemory = manager.rainWorld.progression.GetProgLinesFromMemory();
		if (progLinesFromMemory.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length != 2 || !(array[0] == "SAVE STATE") || !(BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == MoreSlugcatsEnums.SlugcatStatsName.Spear))
			{
				continue;
			}
			List<SaveStateMiner.Target> list = new List<SaveStateMiner.Target>();
			list.Add(new SaveStateMiner.Target(">ALTENDING", null, "<dpA>", 20));
			List<SaveStateMiner.Result> list2 = SaveStateMiner.Mine(manager.rainWorld, array[1], list);
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].name == ">ALTENDING")
				{
					return true;
				}
			}
		}
		return false;
	}

	public DataPearl.AbstractDataPearl.DataPearlType[] GetCollectionReadablePearls()
	{
		return new DataPearl.AbstractDataPearl.DataPearlType[30]
		{
			DataPearl.AbstractDataPearl.DataPearlType.SL_moon,
			DataPearl.AbstractDataPearl.DataPearlType.SL_bridge,
			DataPearl.AbstractDataPearl.DataPearlType.SL_chimney,
			DataPearl.AbstractDataPearl.DataPearlType.SI_west,
			DataPearl.AbstractDataPearl.DataPearlType.SI_top,
			MoreSlugcatsEnums.DataPearlType.SI_chat3,
			MoreSlugcatsEnums.DataPearlType.SI_chat4,
			MoreSlugcatsEnums.DataPearlType.SI_chat5,
			DataPearl.AbstractDataPearl.DataPearlType.SB_ravine,
			DataPearl.AbstractDataPearl.DataPearlType.SU,
			DataPearl.AbstractDataPearl.DataPearlType.HI,
			DataPearl.AbstractDataPearl.DataPearlType.GW,
			MoreSlugcatsEnums.DataPearlType.MS,
			DataPearl.AbstractDataPearl.DataPearlType.DS,
			DataPearl.AbstractDataPearl.DataPearlType.SH,
			DataPearl.AbstractDataPearl.DataPearlType.CC,
			MoreSlugcatsEnums.DataPearlType.VS,
			DataPearl.AbstractDataPearl.DataPearlType.UW,
			DataPearl.AbstractDataPearl.DataPearlType.LF_bottom,
			DataPearl.AbstractDataPearl.DataPearlType.LF_west,
			DataPearl.AbstractDataPearl.DataPearlType.SB_filtration,
			MoreSlugcatsEnums.DataPearlType.SU_filt,
			MoreSlugcatsEnums.DataPearlType.OE,
			MoreSlugcatsEnums.DataPearlType.LC,
			MoreSlugcatsEnums.DataPearlType.LC_second,
			MoreSlugcatsEnums.DataPearlType.RM,
			DataPearl.AbstractDataPearl.DataPearlType.Red_stomach,
			MoreSlugcatsEnums.DataPearlType.DM,
			MoreSlugcatsEnums.DataPearlType.Spearmasterpearl,
			MoreSlugcatsEnums.DataPearlType.Rivulet_stomach
		};
	}

	public static int DataPearlToFileID(DataPearl.AbstractDataPearl.DataPearlType type)
	{
		Conversation.ID iD = Conversation.DataPearlToConversation(type);
		if (iD == Conversation.ID.Moon_Pearl_CC)
		{
			return 7;
		}
		if (iD == Conversation.ID.Moon_Pearl_SI_west)
		{
			return 20;
		}
		if (iD == Conversation.ID.Moon_Pearl_SI_top)
		{
			return 21;
		}
		if (iD == Conversation.ID.Moon_Pearl_LF_west)
		{
			return 10;
		}
		if (iD == Conversation.ID.Moon_Pearl_LF_bottom)
		{
			return 11;
		}
		if (iD == Conversation.ID.Moon_Pearl_HI)
		{
			return 12;
		}
		if (iD == Conversation.ID.Moon_Pearl_SH)
		{
			return 13;
		}
		if (iD == Conversation.ID.Moon_Pearl_DS)
		{
			return 14;
		}
		if (iD == Conversation.ID.Moon_Pearl_SB_filtration)
		{
			return 15;
		}
		if (iD == Conversation.ID.Moon_Pearl_GW)
		{
			return 16;
		}
		if (iD == Conversation.ID.Moon_Pearl_SL_bridge)
		{
			return 17;
		}
		if (iD == Conversation.ID.Moon_Pearl_SL_moon)
		{
			return 18;
		}
		if (iD == Conversation.ID.Moon_Pearl_SU)
		{
			return 41;
		}
		if (iD == Conversation.ID.Moon_Pearl_SB_ravine)
		{
			return 43;
		}
		if (iD == Conversation.ID.Moon_Pearl_UW)
		{
			return 42;
		}
		if (iD == Conversation.ID.Moon_Pearl_SL_chimney)
		{
			return 54;
		}
		if (iD == Conversation.ID.Moon_Pearl_Red_stomach)
		{
			return 51;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3)
		{
			return 22;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4)
		{
			return 23;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5)
		{
			return 24;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt)
		{
			return 101;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM)
		{
			return 102;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC)
		{
			return 103;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE)
		{
			return 104;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS)
		{
			return 105;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
		{
			return 106;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach)
		{
			return 119;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second)
		{
			return 121;
		}
		if (ModManager.MSC && iD == MoreSlugcatsEnums.ConversationID.Moon_Pearl_VS)
		{
			return 128;
		}
		return -1;
	}
}
