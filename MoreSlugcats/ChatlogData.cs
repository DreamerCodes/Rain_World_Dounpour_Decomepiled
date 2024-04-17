using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RWCustom;

namespace MoreSlugcats;

public class ChatlogData
{
	public class ChatlogID : ExtEnum<ChatlogID>
	{
		public static readonly ChatlogID Chatlog_SU0 = new ChatlogID("Chatlog_SU0", register: true);

		public static readonly ChatlogID Chatlog_SU1 = new ChatlogID("Chatlog_SU1", register: true);

		public static readonly ChatlogID Chatlog_SU2 = new ChatlogID("Chatlog_SU2", register: true);

		public static readonly ChatlogID Chatlog_SU3 = new ChatlogID("Chatlog_SU3", register: true);

		public static readonly ChatlogID Chatlog_SU4 = new ChatlogID("Chatlog_SU4", register: true);

		public static readonly ChatlogID Chatlog_SU5 = new ChatlogID("Chatlog_SU5", register: true);

		public static readonly ChatlogID Chatlog_SU6 = new ChatlogID("Chatlog_SU6", register: true);

		public static readonly ChatlogID Chatlog_SU7 = new ChatlogID("Chatlog_SU7", register: true);

		public static readonly ChatlogID Chatlog_SU8 = new ChatlogID("Chatlog_SU8", register: true);

		public static readonly ChatlogID Chatlog_SU9 = new ChatlogID("Chatlog_SU9", register: true);

		public static readonly ChatlogID Chatlog_HI0 = new ChatlogID("Chatlog_HI0", register: true);

		public static readonly ChatlogID Chatlog_HI1 = new ChatlogID("Chatlog_HI1", register: true);

		public static readonly ChatlogID Chatlog_HI2 = new ChatlogID("Chatlog_HI2", register: true);

		public static readonly ChatlogID Chatlog_HI3 = new ChatlogID("Chatlog_HI3", register: true);

		public static readonly ChatlogID Chatlog_HI4 = new ChatlogID("Chatlog_HI4", register: true);

		public static readonly ChatlogID Chatlog_HI5 = new ChatlogID("Chatlog_HI5", register: true);

		public static readonly ChatlogID Chatlog_HI6 = new ChatlogID("Chatlog_HI6", register: true);

		public static readonly ChatlogID Chatlog_HI7 = new ChatlogID("Chatlog_HI7", register: true);

		public static readonly ChatlogID Chatlog_HI8 = new ChatlogID("Chatlog_HI8", register: true);

		public static readonly ChatlogID Chatlog_HI9 = new ChatlogID("Chatlog_HI9", register: true);

		public static readonly ChatlogID Chatlog_DS0 = new ChatlogID("Chatlog_DS0", register: true);

		public static readonly ChatlogID Chatlog_DS1 = new ChatlogID("Chatlog_DS1", register: true);

		public static readonly ChatlogID Chatlog_DS2 = new ChatlogID("Chatlog_DS2", register: true);

		public static readonly ChatlogID Chatlog_DS3 = new ChatlogID("Chatlog_DS3", register: true);

		public static readonly ChatlogID Chatlog_DS4 = new ChatlogID("Chatlog_DS4", register: true);

		public static readonly ChatlogID Chatlog_DS5 = new ChatlogID("Chatlog_DS5", register: true);

		public static readonly ChatlogID Chatlog_DS6 = new ChatlogID("Chatlog_DS6", register: true);

		public static readonly ChatlogID Chatlog_DS7 = new ChatlogID("Chatlog_DS7", register: true);

		public static readonly ChatlogID Chatlog_DS8 = new ChatlogID("Chatlog_DS8", register: true);

		public static readonly ChatlogID Chatlog_DS9 = new ChatlogID("Chatlog_DS9", register: true);

		public static readonly ChatlogID Chatlog_SH0 = new ChatlogID("Chatlog_SH0", register: true);

		public static readonly ChatlogID Chatlog_SH1 = new ChatlogID("Chatlog_SH1", register: true);

		public static readonly ChatlogID Chatlog_SH2 = new ChatlogID("Chatlog_SH2", register: true);

		public static readonly ChatlogID Chatlog_SH3 = new ChatlogID("Chatlog_SH3", register: true);

		public static readonly ChatlogID Chatlog_SH4 = new ChatlogID("Chatlog_SH4", register: true);

		public static readonly ChatlogID Chatlog_SH5 = new ChatlogID("Chatlog_SH5", register: true);

		public static readonly ChatlogID Chatlog_SH6 = new ChatlogID("Chatlog_SH6", register: true);

		public static readonly ChatlogID Chatlog_SH7 = new ChatlogID("Chatlog_SH7", register: true);

		public static readonly ChatlogID Chatlog_SH8 = new ChatlogID("Chatlog_SH8", register: true);

		public static readonly ChatlogID Chatlog_SH9 = new ChatlogID("Chatlog_SH9", register: true);

		public static readonly ChatlogID Chatlog_GW0 = new ChatlogID("Chatlog_GW0", register: true);

		public static readonly ChatlogID Chatlog_GW1 = new ChatlogID("Chatlog_GW1", register: true);

		public static readonly ChatlogID Chatlog_GW2 = new ChatlogID("Chatlog_GW2", register: true);

		public static readonly ChatlogID Chatlog_GW3 = new ChatlogID("Chatlog_GW3", register: true);

		public static readonly ChatlogID Chatlog_GW4 = new ChatlogID("Chatlog_GW4", register: true);

		public static readonly ChatlogID Chatlog_GW5 = new ChatlogID("Chatlog_GW5", register: true);

		public static readonly ChatlogID Chatlog_GW6 = new ChatlogID("Chatlog_GW6", register: true);

		public static readonly ChatlogID Chatlog_GW7 = new ChatlogID("Chatlog_GW7", register: true);

		public static readonly ChatlogID Chatlog_GW8 = new ChatlogID("Chatlog_GW8", register: true);

		public static readonly ChatlogID Chatlog_GW9 = new ChatlogID("Chatlog_GW9", register: true);

		public static readonly ChatlogID Chatlog_CC0 = new ChatlogID("Chatlog_CC0", register: true);

		public static readonly ChatlogID Chatlog_CC1 = new ChatlogID("Chatlog_CC1", register: true);

		public static readonly ChatlogID Chatlog_CC2 = new ChatlogID("Chatlog_CC2", register: true);

		public static readonly ChatlogID Chatlog_CC3 = new ChatlogID("Chatlog_CC3", register: true);

		public static readonly ChatlogID Chatlog_CC4 = new ChatlogID("Chatlog_CC4", register: true);

		public static readonly ChatlogID Chatlog_CC5 = new ChatlogID("Chatlog_CC5", register: true);

		public static readonly ChatlogID Chatlog_CC6 = new ChatlogID("Chatlog_CC6", register: true);

		public static readonly ChatlogID Chatlog_CC7 = new ChatlogID("Chatlog_CC7", register: true);

		public static readonly ChatlogID Chatlog_CC8 = new ChatlogID("Chatlog_CC8", register: true);

		public static readonly ChatlogID Chatlog_CC9 = new ChatlogID("Chatlog_CC9", register: true);

		public static readonly ChatlogID Chatlog_LM0 = new ChatlogID("Chatlog_LM0", register: true);

		public static readonly ChatlogID Chatlog_LM1 = new ChatlogID("Chatlog_LM1", register: true);

		public static readonly ChatlogID Chatlog_LM2 = new ChatlogID("Chatlog_LM2", register: true);

		public static readonly ChatlogID Chatlog_LM3 = new ChatlogID("Chatlog_LM3", register: true);

		public static readonly ChatlogID Chatlog_LM4 = new ChatlogID("Chatlog_LM4", register: true);

		public static readonly ChatlogID Chatlog_LM5 = new ChatlogID("Chatlog_LM5", register: true);

		public static readonly ChatlogID Chatlog_LM6 = new ChatlogID("Chatlog_LM6", register: true);

		public static readonly ChatlogID Chatlog_LM7 = new ChatlogID("Chatlog_LM7", register: true);

		public static readonly ChatlogID Chatlog_LM8 = new ChatlogID("Chatlog_LM8", register: true);

		public static readonly ChatlogID Chatlog_LM9 = new ChatlogID("Chatlog_LM9", register: true);

		public static readonly ChatlogID Chatlog_DM0 = new ChatlogID("Chatlog_DM0", register: true);

		public static readonly ChatlogID Chatlog_DM1 = new ChatlogID("Chatlog_DM1", register: true);

		public static readonly ChatlogID Chatlog_DM2 = new ChatlogID("Chatlog_DM2", register: true);

		public static readonly ChatlogID Chatlog_DM3 = new ChatlogID("Chatlog_DM3", register: true);

		public static readonly ChatlogID Chatlog_DM4 = new ChatlogID("Chatlog_DM4", register: true);

		public static readonly ChatlogID Chatlog_DM5 = new ChatlogID("Chatlog_DM5", register: true);

		public static readonly ChatlogID Chatlog_DM6 = new ChatlogID("Chatlog_DM6", register: true);

		public static readonly ChatlogID Chatlog_DM7 = new ChatlogID("Chatlog_DM7", register: true);

		public static readonly ChatlogID Chatlog_DM8 = new ChatlogID("Chatlog_DM8", register: true);

		public static readonly ChatlogID Chatlog_DM9 = new ChatlogID("Chatlog_DM9", register: true);

		public static readonly ChatlogID Chatlog_UW0 = new ChatlogID("Chatlog_UW0", register: true);

		public static readonly ChatlogID Chatlog_UW1 = new ChatlogID("Chatlog_UW1", register: true);

		public static readonly ChatlogID Chatlog_UW2 = new ChatlogID("Chatlog_UW2", register: true);

		public static readonly ChatlogID Chatlog_UW3 = new ChatlogID("Chatlog_UW3", register: true);

		public static readonly ChatlogID Chatlog_UW4 = new ChatlogID("Chatlog_UW4", register: true);

		public static readonly ChatlogID Chatlog_UW5 = new ChatlogID("Chatlog_UW5", register: true);

		public static readonly ChatlogID Chatlog_UW6 = new ChatlogID("Chatlog_UW6", register: true);

		public static readonly ChatlogID Chatlog_UW7 = new ChatlogID("Chatlog_UW7", register: true);

		public static readonly ChatlogID Chatlog_UW8 = new ChatlogID("Chatlog_UW8", register: true);

		public static readonly ChatlogID Chatlog_UW9 = new ChatlogID("Chatlog_UW9", register: true);

		public static readonly ChatlogID Chatlog_SS0 = new ChatlogID("Chatlog_SS0", register: true);

		public static readonly ChatlogID Chatlog_SS1 = new ChatlogID("Chatlog_SS1", register: true);

		public static readonly ChatlogID Chatlog_SS2 = new ChatlogID("Chatlog_SS2", register: true);

		public static readonly ChatlogID Chatlog_SS3 = new ChatlogID("Chatlog_SS3", register: true);

		public static readonly ChatlogID Chatlog_SS4 = new ChatlogID("Chatlog_SS4", register: true);

		public static readonly ChatlogID Chatlog_SS5 = new ChatlogID("Chatlog_SS5", register: true);

		public static readonly ChatlogID Chatlog_SS6 = new ChatlogID("Chatlog_SS6", register: true);

		public static readonly ChatlogID Chatlog_SS7 = new ChatlogID("Chatlog_SS7", register: true);

		public static readonly ChatlogID Chatlog_SS8 = new ChatlogID("Chatlog_SS8", register: true);

		public static readonly ChatlogID Chatlog_SS9 = new ChatlogID("Chatlog_SS9", register: true);

		public static readonly ChatlogID Chatlog_LF0 = new ChatlogID("Chatlog_LF0", register: true);

		public static readonly ChatlogID Chatlog_LF1 = new ChatlogID("Chatlog_LF1", register: true);

		public static readonly ChatlogID Chatlog_LF2 = new ChatlogID("Chatlog_LF2", register: true);

		public static readonly ChatlogID Chatlog_LF3 = new ChatlogID("Chatlog_LF3", register: true);

		public static readonly ChatlogID Chatlog_LF4 = new ChatlogID("Chatlog_LF4", register: true);

		public static readonly ChatlogID Chatlog_LF5 = new ChatlogID("Chatlog_LF5", register: true);

		public static readonly ChatlogID Chatlog_LF6 = new ChatlogID("Chatlog_LF6", register: true);

		public static readonly ChatlogID Chatlog_LF7 = new ChatlogID("Chatlog_LF7", register: true);

		public static readonly ChatlogID Chatlog_LF8 = new ChatlogID("Chatlog_LF8", register: true);

		public static readonly ChatlogID Chatlog_LF9 = new ChatlogID("Chatlog_LF9", register: true);

		public static readonly ChatlogID Chatlog_SB0 = new ChatlogID("Chatlog_SB0", register: true);

		public static readonly ChatlogID Chatlog_SB1 = new ChatlogID("Chatlog_SB1", register: true);

		public static readonly ChatlogID Chatlog_SB2 = new ChatlogID("Chatlog_SB2", register: true);

		public static readonly ChatlogID Chatlog_SB3 = new ChatlogID("Chatlog_SB3", register: true);

		public static readonly ChatlogID Chatlog_SB4 = new ChatlogID("Chatlog_SB4", register: true);

		public static readonly ChatlogID Chatlog_SB5 = new ChatlogID("Chatlog_SB5", register: true);

		public static readonly ChatlogID Chatlog_SB6 = new ChatlogID("Chatlog_SB6", register: true);

		public static readonly ChatlogID Chatlog_SB7 = new ChatlogID("Chatlog_SB7", register: true);

		public static readonly ChatlogID Chatlog_SB8 = new ChatlogID("Chatlog_SB8", register: true);

		public static readonly ChatlogID Chatlog_SB9 = new ChatlogID("Chatlog_SB9", register: true);

		public static readonly ChatlogID Chatlog_SI0 = new ChatlogID("Chatlog_SI0", register: true);

		public static readonly ChatlogID Chatlog_SI1 = new ChatlogID("Chatlog_SI1", register: true);

		public static readonly ChatlogID Chatlog_SI2 = new ChatlogID("Chatlog_SI2", register: true);

		public static readonly ChatlogID Chatlog_SI3 = new ChatlogID("Chatlog_SI3", register: true);

		public static readonly ChatlogID Chatlog_SI4 = new ChatlogID("Chatlog_SI4", register: true);

		public static readonly ChatlogID Chatlog_SI5 = new ChatlogID("Chatlog_SI5", register: true);

		public static readonly ChatlogID Chatlog_SI6 = new ChatlogID("Chatlog_SI6", register: true);

		public static readonly ChatlogID Chatlog_SI7 = new ChatlogID("Chatlog_SI7", register: true);

		public static readonly ChatlogID Chatlog_SI8 = new ChatlogID("Chatlog_SI8", register: true);

		public static readonly ChatlogID Chatlog_SI9 = new ChatlogID("Chatlog_SI9", register: true);

		public static readonly ChatlogID Chatlog_Broadcast0 = new ChatlogID("Chatlog_Broadcast0", register: true);

		public static readonly ChatlogID Chatlog_Broadcast1 = new ChatlogID("Chatlog_Broadcast1", register: true);

		public static readonly ChatlogID Chatlog_Broadcast2 = new ChatlogID("Chatlog_Broadcast2", register: true);

		public static readonly ChatlogID Chatlog_Broadcast3 = new ChatlogID("Chatlog_Broadcast3", register: true);

		public static readonly ChatlogID Chatlog_Broadcast4 = new ChatlogID("Chatlog_Broadcast4", register: true);

		public static readonly ChatlogID Chatlog_Broadcast5 = new ChatlogID("Chatlog_Broadcast5", register: true);

		public static readonly ChatlogID Chatlog_Broadcast6 = new ChatlogID("Chatlog_Broadcast6", register: true);

		public static readonly ChatlogID Chatlog_Broadcast7 = new ChatlogID("Chatlog_Broadcast7", register: true);

		public static readonly ChatlogID Chatlog_Broadcast8 = new ChatlogID("Chatlog_Broadcast8", register: true);

		public static readonly ChatlogID Chatlog_Broadcast9 = new ChatlogID("Chatlog_Broadcast9", register: true);

		public static readonly ChatlogID Chatlog_Broadcast10 = new ChatlogID("Chatlog_Broadcast10", register: true);

		public static readonly ChatlogID Chatlog_Broadcast11 = new ChatlogID("Chatlog_Broadcast11", register: true);

		public static readonly ChatlogID Chatlog_Broadcast12 = new ChatlogID("Chatlog_Broadcast12", register: true);

		public static readonly ChatlogID Chatlog_Broadcast13 = new ChatlogID("Chatlog_Broadcast13", register: true);

		public static readonly ChatlogID Chatlog_Broadcast14 = new ChatlogID("Chatlog_Broadcast14", register: true);

		public static readonly ChatlogID Chatlog_Broadcast15 = new ChatlogID("Chatlog_Broadcast15", register: true);

		public static readonly ChatlogID Chatlog_Broadcast16 = new ChatlogID("Chatlog_Broadcast16", register: true);

		public static readonly ChatlogID Chatlog_Broadcast17 = new ChatlogID("Chatlog_Broadcast17", register: true);

		public static readonly ChatlogID Chatlog_Broadcast18 = new ChatlogID("Chatlog_Broadcast18", register: true);

		public static readonly ChatlogID Chatlog_Broadcast19 = new ChatlogID("Chatlog_Broadcast19", register: true);

		public static readonly ChatlogID DevCommentaryNode = new ChatlogID("DevCommentaryNode", register: true);

		public ChatlogID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private static Player myPlayer;

	public static RainWorld rainWorld;

	public static void setHostPlayer(Player Inputplayer)
	{
		myPlayer = Inputplayer;
	}

	public static void setRainworldReference(RainWorld rw)
	{
		rainWorld = rw;
	}

	public static string[] getLinearBroadcast(int id, bool postPebbles)
	{
		string text = id.ToString();
		if (postPebbles)
		{
			text += "_PEB";
		}
		InGameTranslator.LanguageID languageID = rainWorld.inGameTranslator.currentLanguage;
		string[] array2;
		while (true)
		{
			string path = AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar + "LP_" + text + ".txt");
			if (File.Exists(path))
			{
				string[] array = DecryptResult(File.ReadAllText(path, Encoding.Default), path).Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				array2 = new string[array.Length - 1];
				for (int i = 1; i < array.Length; i++)
				{
					array2[i - 1] = array[i];
				}
				break;
			}
			if (languageID != InGameTranslator.LanguageID.English)
			{
				languageID = InGameTranslator.LanguageID.English;
				continue;
			}
			array2 = new string[1] { "UNABLE TO ESTABLISH COMMUNICATION" };
			break;
		}
		return array2;
	}

	public static string[] getChatlog(int id)
	{
		if (myPlayer.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
		{
			return getLinearBroadcast(id, postPebbles: true);
		}
		return getLinearBroadcast(id, postPebbles: false);
	}

	public static bool getChatlogExists(int id)
	{
		if (myPlayer.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
		{
			return File.Exists(linearChatlogPath(id, postPebbles: true));
		}
		return File.Exists(linearChatlogPath(id, postPebbles: false));
	}

	public static string DecryptResult(string result, string path)
	{
		int num = 0;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		for (int i = 0; i < fileNameWithoutExtension.Length; i++)
		{
			num += fileNameWithoutExtension[i] - 48;
		}
		return Custom.xorEncrypt(result, 54 + num + rainWorld.inGameTranslator.currentLanguage.Index * 7);
	}

	public static string linearChatlogPath(int id, bool postPebbles)
	{
		string text = id.ToString();
		if (postPebbles)
		{
			text += "_PEB";
		}
		string text2 = AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + "LP_" + text + ".txt");
		if (File.Exists(text2))
		{
			return text2;
		}
		return AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + "LP_" + text + ".txt");
	}

	public static string DevCommPath(string roomName, SlugcatStats.Name slugcatIndex)
	{
		string text = AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + roomName + "-" + slugcatIndex.value + ".txt");
		if (File.Exists(text))
		{
			return text;
		}
		return AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + roomName + "-" + slugcatIndex.value + ".txt");
	}

	public static bool HasDevComm(string roomName, SlugcatStats.Name slugcatIndex)
	{
		return File.Exists(DevCommPath(roomName, slugcatIndex));
	}

	public static string UniquePath(ChatlogID id)
	{
		string text = AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + id.value + ".txt");
		if (File.Exists(text))
		{
			return text;
		}
		return AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English) + Path.DirectorySeparatorChar + id.value + ".txt");
	}

	public static bool HasUnique(ChatlogID id)
	{
		return File.Exists(UniquePath(id));
	}

	public static string[] getChatlog(ChatlogID id)
	{
		string path = UniquePath(id);
		string[] array2;
		if (File.Exists(path))
		{
			string[] array = DecryptResult(File.ReadAllText(path, Encoding.Default), path).Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			array2 = new string[array.Length - 1];
			for (int i = 1; i < array.Length; i++)
			{
				array2[i - 1] = array[i];
			}
		}
		else
		{
			array2 = new string[1] { "UNABLE TO ESTABLISH COMMUNICATION" };
		}
		return array2;
	}

	public static string[] getDevComm(string roomName, SlugcatStats.Name slugcatIndex)
	{
		string path = DevCommPath(roomName, slugcatIndex);
		string[] array;
		if (File.Exists(path))
		{
			array = DecryptResult(File.ReadAllText(path, Encoding.Default), path).Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}
		else
		{
			path = DevCommPath(roomName, SlugcatStats.Name.White);
			if (!File.Exists(path))
			{
				return new string[1] { "?!?NO COMMENTARY?!?" };
			}
			array = DecryptResult(File.ReadAllText(path, Encoding.Default), path).Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}
		string[] array2 = new string[array.Length - 1];
		for (int i = 1; i < array.Length; i++)
		{
			array2[i - 1] = array[i];
		}
		return array2;
	}

	public static void resetBroadcasts()
	{
		myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Clear();
	}

	public static void markAllBroadcastsRead()
	{
		ChatlogID[] array = new ChatlogID[20]
		{
			ChatlogID.Chatlog_Broadcast0,
			ChatlogID.Chatlog_Broadcast1,
			ChatlogID.Chatlog_Broadcast2,
			ChatlogID.Chatlog_Broadcast3,
			ChatlogID.Chatlog_Broadcast4,
			ChatlogID.Chatlog_Broadcast5,
			ChatlogID.Chatlog_Broadcast6,
			ChatlogID.Chatlog_Broadcast7,
			ChatlogID.Chatlog_Broadcast8,
			ChatlogID.Chatlog_Broadcast9,
			ChatlogID.Chatlog_Broadcast10,
			ChatlogID.Chatlog_Broadcast11,
			ChatlogID.Chatlog_Broadcast12,
			ChatlogID.Chatlog_Broadcast13,
			ChatlogID.Chatlog_Broadcast14,
			ChatlogID.Chatlog_Broadcast15,
			ChatlogID.Chatlog_Broadcast16,
			ChatlogID.Chatlog_Broadcast17,
			ChatlogID.Chatlog_Broadcast18,
			ChatlogID.Chatlog_Broadcast19
		};
		foreach (ChatlogID item in array)
		{
			if (!myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Contains(item))
			{
				myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Add(item);
			}
		}
		myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.prePebChatlogsRead = new List<ChatlogID>(myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead);
	}
}
