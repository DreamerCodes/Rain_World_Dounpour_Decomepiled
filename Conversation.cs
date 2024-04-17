using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HUD;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public abstract class Conversation
{
	public class ID : ExtEnum<ID>
	{
		public static readonly ID None = new ID("None", register: true);

		public static readonly ID MoonFirstPostMarkConversation = new ID("MoonFirstPostMarkConversation", register: true);

		public static readonly ID MoonSecondPostMarkConversation = new ID("MoonSecondPostMarkConversation", register: true);

		public static readonly ID MoonRecieveSwarmer = new ID("MoonRecieveSwarmer", register: true);

		public static readonly ID Moon_Pearl_Misc = new ID("Moon_Pearl_Misc", register: true);

		public static readonly ID Moon_Pearl_Misc2 = new ID("Moon_Pearl_Misc2", register: true);

		public static readonly ID Moon_Pebbles_Pearl = new ID("Moon_Pebbles_Pearl", register: true);

		public static readonly ID Moon_Pearl_CC = new ID("Moon_Pearl_CC", register: true);

		public static readonly ID Moon_Pearl_SI_west = new ID("Moon_Pearl_SI_west", register: true);

		public static readonly ID Moon_Pearl_SI_top = new ID("Moon_Pearl_SI_top", register: true);

		public static readonly ID Moon_Pearl_LF_west = new ID("Moon_Pearl_LF_west", register: true);

		public static readonly ID Moon_Pearl_LF_bottom = new ID("Moon_Pearl_LF_bottom", register: true);

		public static readonly ID Moon_Pearl_HI = new ID("Moon_Pearl_HI", register: true);

		public static readonly ID Moon_Pearl_SH = new ID("Moon_Pearl_SH", register: true);

		public static readonly ID Moon_Pearl_DS = new ID("Moon_Pearl_DS", register: true);

		public static readonly ID Moon_Pearl_SB_filtration = new ID("Moon_Pearl_SB_filtration", register: true);

		public static readonly ID Moon_Pearl_GW = new ID("Moon_Pearl_GW", register: true);

		public static readonly ID Moon_Pearl_SL_bridge = new ID("Moon_Pearl_SL_bridge", register: true);

		public static readonly ID Moon_Pearl_SL_moon = new ID("Moon_Pearl_SL_moon", register: true);

		public static readonly ID Moon_Misc_Item = new ID("Moon_Misc_Item", register: true);

		public static readonly ID Moon_Pearl_SU = new ID("Moon_Pearl_SU", register: true);

		public static readonly ID Moon_Pearl_SB_ravine = new ID("Moon_Pearl_SB_ravine", register: true);

		public static readonly ID Moon_Pearl_UW = new ID("Moon_Pearl_UW", register: true);

		public static readonly ID Moon_Pearl_SL_chimney = new ID("Moon_Pearl_SL_chimney", register: true);

		public static readonly ID Moon_Pearl_Red_stomach = new ID("Moon_Pearl_Red_stomach", register: true);

		public static readonly ID Moon_Red_First_Conversation = new ID("Moon_Red_First_Conversation", register: true);

		public static readonly ID Moon_Red_Second_Conversation = new ID("Moon_Red_Second_Conversation", register: true);

		public static readonly ID Moon_Yellow_First_Conversation = new ID("Moon_Yellow_First_Conversation", register: true);

		public static readonly ID Ghost_CC = new ID("Ghost_CC", register: true);

		public static readonly ID Ghost_SI = new ID("Ghost_SI", register: true);

		public static readonly ID Ghost_LF = new ID("Ghost_LF", register: true);

		public static readonly ID Ghost_SH = new ID("Ghost_SH", register: true);

		public static readonly ID Ghost_UW = new ID("Ghost_UW", register: true);

		public static readonly ID Ghost_SB = new ID("Ghost_SB", register: true);

		public static readonly ID Pebbles_White = new ID("Pebbles_White", register: true);

		public static readonly ID Pebbles_Red_Green_Neuron = new ID("Pebbles_Red_Green_Neuron", register: true);

		public static readonly ID Pebbles_Red_No_Neuron = new ID("Pebbles_Red_No_Neuron", register: true);

		public static readonly ID Pebbles_Yellow = new ID("Pebbles_Yellow", register: true);

		public ID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class DialogueEvent
	{
		public int age;

		public Conversation owner;

		public int initialWait;

		protected bool isActivated;

		public virtual bool IsOver => true;

		public DialogueEvent(Conversation owner, int initialWait)
		{
			this.owner = owner;
			this.initialWait = initialWait;
		}

		public virtual void Activate()
		{
			isActivated = true;
		}

		public virtual void Update()
		{
			if (!isActivated && age == initialWait)
			{
				Activate();
			}
			age++;
		}
	}

	public class TextEvent : DialogueEvent
	{
		public string text;

		public int textLinger;

		public override bool IsOver
		{
			get
			{
				if (!ModManager.MMF || (owner != null && owner.dialogBox != null))
				{
					if (age > initialWait)
					{
						return owner.dialogBox.CurrentMessage == null;
					}
					return false;
				}
				return true;
			}
		}

		public TextEvent(Conversation owner, int initialWait, string text, int textLinger)
			: base(owner, initialWait)
		{
			this.text = text;
			this.textLinger = textLinger;
		}

		public override void Activate()
		{
			base.Activate();
			if (ModManager.MSC)
			{
				if (TryGetPrefixColor(text, out var result))
				{
					owner.dialogBox.currentColor = result;
				}
				else
				{
					owner.dialogBox.currentColor = Color.white;
				}
			}
			owner.dialogBox.NewMessage((owner.interfaceOwner != null) ? owner.interfaceOwner.ReplaceParts(text) : text, textLinger);
		}
	}

	public class WaitEvent : DialogueEvent
	{
		public override bool IsOver => age > initialWait;

		public WaitEvent(Conversation owner, int initialWait)
			: base(owner, initialWait)
		{
		}
	}

	public class SpecialEvent : DialogueEvent
	{
		private string eventName;

		public SpecialEvent(Conversation owner, int initialWait, string eventName)
			: base(owner, initialWait)
		{
			this.eventName = eventName;
		}

		public override void Activate()
		{
			base.Activate();
			owner.interfaceOwner.SpecialEvent(eventName);
		}
	}

	public interface IOwnAConversation
	{
		RainWorld rainWorld { get; }

		string ReplaceParts(string s);

		void SpecialEvent(string eventName);
	}

	public bool slatedForDeletion;

	public ID id = ID.None;

	public DialogBox dialogBox;

	public List<DialogueEvent> events;

	public IOwnAConversation interfaceOwner;

	public bool paused;

	private static Dictionary<string, Color> PrefixColors;

	private static InGameTranslator.LanguageID lastLanguage;

	public bool colorMode;

	public SlugcatStats.Name currentSaveFile;

	public Conversation(IOwnAConversation interfaceOwner, ID id, DialogBox dialogBox)
	{
		this.interfaceOwner = interfaceOwner;
		this.id = id;
		this.dialogBox = dialogBox;
		events = new List<DialogueEvent>();
		currentSaveFile = null;
	}

	public virtual void Update()
	{
		if (paused)
		{
			return;
		}
		if (events.Count == 0)
		{
			Destroy();
			return;
		}
		events[0].Update();
		if (events[0].IsOver)
		{
			events.RemoveAt(0);
		}
	}

	public void RestartCurrent()
	{
		while (events.Count > 0 && events[0] is WaitEvent)
		{
			events.RemoveAt(0);
		}
		if (events.Count > 0)
		{
			events[0].age = events[0].initialWait;
			events[0].Activate();
		}
	}

	public void Destroy()
	{
		slatedForDeletion = true;
	}

	public void Interrupt(string text, int extraLinger)
	{
		dialogBox.Interrupt(text, extraLinger);
	}

	public void ForceAddMessage(string text, int extraLinger)
	{
		dialogBox.NewMessage((interfaceOwner != null) ? interfaceOwner.ReplaceParts(text) : text, extraLinger);
	}

	public static bool TryGetPrefixColor(string text, out Color result)
	{
		result = MenuColorEffect.rgbWhite;
		if (!ModManager.MSC)
		{
			return false;
		}
		if (PrefixColors == null || lastLanguage != Custom.rainWorld.inGameTranslator.currentLanguage)
		{
			InitalizePrefixColor();
		}
		foreach (KeyValuePair<string, Color> prefixColor in PrefixColors)
		{
			if (text.StartsWith(prefixColor.Key))
			{
				result = prefixColor.Value;
				return true;
			}
		}
		return false;
	}

	private static void InitalizePrefixColor()
	{
		PrefixColors = new Dictionary<string, Color>();
		lastLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
		string path = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "colors.txt");
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('|');
			string[] array3 = array2[1].Split(',');
			Color value = new Color(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture));
			string key = Custom.rainWorld.inGameTranslator.Translate(array2[0]);
			if (!PrefixColors.ContainsKey(key))
			{
				PrefixColors.Add(key, value);
			}
			dictionary.Add(array2[0], value);
		}
		if (Custom.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.English)
		{
			return;
		}
		foreach (KeyValuePair<string, Color> item in dictionary)
		{
			if (!PrefixColors.ContainsKey(item.Key))
			{
				PrefixColors.Add(item.Key, item.Value);
			}
		}
	}

	protected virtual void AddEvents()
	{
	}

	protected void LoadEventsFromFile(int fileName)
	{
		LoadEventsFromFile(fileName, oneRandomLine: false, 0);
	}

	protected void LoadEventsFromFile(int fileName, bool oneRandomLine, int randomSeed)
	{
		LoadEventsFromFile(fileName, currentSaveFile, oneRandomLine, randomSeed);
	}

	protected void LoadEventsFromFile(int fileName, SlugcatStats.Name saveFile, bool oneRandomLine, int randomSeed)
	{
		Custom.Log("~~~LOAD CONVO", fileName.ToString());
		InGameTranslator.LanguageID languageID = interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
		string text;
		while (true)
		{
			text = AssetManager.ResolveFilePath(interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar + fileName + ".txt");
			if (saveFile != null)
			{
				string text2 = text;
				text = AssetManager.ResolveFilePath(interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar + fileName + "-" + saveFile.value + ".txt");
				if (!File.Exists(text))
				{
					text = text2;
				}
			}
			if (File.Exists(text))
			{
				break;
			}
			Custom.LogWarning("NOT FOUND " + text);
			if (languageID != InGameTranslator.LanguageID.English)
			{
				Custom.LogImportant("RETRY WITH ENGLISH");
				languageID = InGameTranslator.LanguageID.English;
				continue;
			}
			return;
		}
		string text3 = File.ReadAllText(text, Encoding.UTF8);
		if (text3[0] != '0')
		{
			text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
		}
		string[] array = Regex.Split(text3, "\r\n");
		try
		{
			if (!(Regex.Split(array[0], "-")[1] == fileName.ToString()))
			{
				return;
			}
			if (oneRandomLine)
			{
				List<TextEvent> list = new List<TextEvent>();
				for (int i = 1; i < array.Length; i++)
				{
					string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
					if (array2.Length == 3)
					{
						list.Add(new TextEvent(this, int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture), array2[2], int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
					}
					else if (array2.Length == 1 && array2[0].Length > 0)
					{
						list.Add(new TextEvent(this, 0, array2[0], 0));
					}
				}
				if (list.Count > 0)
				{
					Random.State state = Random.state;
					Random.InitState(randomSeed);
					TextEvent item = list[Random.Range(0, list.Count)];
					Random.state = state;
					events.Add(item);
				}
				return;
			}
			for (int j = 1; j < array.Length; j++)
			{
				string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
				if (array3.Length == 3)
				{
					if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var _) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
					{
						events.Add(new TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
					}
					else
					{
						events.Add(new TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
					}
				}
				else if (array3.Length == 2)
				{
					if (array3[0] == "SPECEVENT")
					{
						events.Add(new SpecialEvent(this, 0, array3[1]));
					}
					else if (array3[0] == "PEBBLESWAIT")
					{
						events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(this, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
					}
				}
				else if (array3.Length == 1 && array3[0].Length > 0)
				{
					events.Add(new TextEvent(this, 0, array3[0], 0));
				}
			}
		}
		catch
		{
			Custom.LogWarning("TEXT ERROR");
			events.Add(new TextEvent(this, 0, "TEXT ERROR", 100));
		}
	}

	public static void EncryptAllDialogue()
	{
		Custom.Log("Encrypt all dialogue");
		for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
		{
			InGameTranslator.LanguageID iD = InGameTranslator.LanguageID.Parse(i);
			string path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(iD) + Path.DirectorySeparatorChar).ToLowerInvariant();
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
				for (int j = 0; j < files.Length; j++)
				{
					InGameTranslator.EncryptDecryptFile(files[j], encryptMode: true);
				}
			}
		}
	}

	public static void DecryptAllDialogue()
	{
		Custom.Log("Decrypt all dialogue");
		for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
		{
			InGameTranslator.LanguageID iD = InGameTranslator.LanguageID.Parse(i);
			string path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(iD) + Path.DirectorySeparatorChar).ToLowerInvariant();
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
				for (int j = 0; j < files.Length; j++)
				{
					InGameTranslator.EncryptDecryptFile(files[j], encryptMode: false);
				}
			}
		}
	}

	public static bool EventsFileExists(RainWorld rw, int fileName)
	{
		if (!File.Exists(AssetManager.ResolveFilePath(rw.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + fileName + ".txt")))
		{
			return File.Exists(AssetManager.ResolveFilePath(rw.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + fileName + ".txt"));
		}
		return true;
	}

	public static bool EventsFileExists(RainWorld rw, int fileName, SlugcatStats.Name saveFile)
	{
		if (!File.Exists(AssetManager.ResolveFilePath(rw.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + fileName + "-" + saveFile.value + ".txt")))
		{
			return File.Exists(AssetManager.ResolveFilePath(rw.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + fileName + "-" + saveFile.value + ".txt"));
		}
		return true;
	}

	public static ID DataPearlToConversation(DataPearl.AbstractDataPearl.DataPearlType type)
	{
		ID result = ID.None;
		if (type == DataPearl.AbstractDataPearl.DataPearlType.CC)
		{
			result = ID.Moon_Pearl_CC;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SI_west)
		{
			result = ID.Moon_Pearl_SI_west;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SI_top)
		{
			result = ID.Moon_Pearl_SI_top;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.LF_west)
		{
			result = ID.Moon_Pearl_LF_west;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.LF_bottom)
		{
			result = ID.Moon_Pearl_LF_bottom;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.HI)
		{
			result = ID.Moon_Pearl_HI;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SH)
		{
			result = ID.Moon_Pearl_SH;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.DS)
		{
			result = ID.Moon_Pearl_DS;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SB_filtration)
		{
			result = ID.Moon_Pearl_SB_filtration;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SB_ravine)
		{
			result = ID.Moon_Pearl_SB_ravine;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.GW)
		{
			result = ID.Moon_Pearl_GW;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SL_bridge)
		{
			result = ID.Moon_Pearl_SL_bridge;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SL_moon)
		{
			result = ID.Moon_Pearl_SL_moon;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SU)
		{
			result = ID.Moon_Pearl_SU;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.UW)
		{
			result = ID.Moon_Pearl_UW;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.SL_chimney)
		{
			result = ID.Moon_Pearl_SL_chimney;
		}
		else if (type == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
		{
			result = ID.Moon_Pearl_Red_stomach;
		}
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.DataPearlType.SU_filt)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.SI_chat3)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.SI_chat4)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.SI_chat5)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.DM)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.LC)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.OE)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.MS)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.RM)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.Rivulet_stomach)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.LC_second)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second;
			}
			else if (type == MoreSlugcatsEnums.DataPearlType.VS)
			{
				result = MoreSlugcatsEnums.ConversationID.Moon_Pearl_VS;
			}
		}
		return result;
	}
}
