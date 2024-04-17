using System.Globalization;
using System.IO;
using MoreSlugcats;

namespace DevInterface;

public class ConversationLoader : Conversation
{
	public string[] chatlogMessages;

	public ConversationLoader(IOwnAConversation interfaceOwner)
		: base(interfaceOwner, ID.None, null)
	{
	}

	public void LoadEvents(string filePath)
	{
		chatlogMessages = null;
		events.Clear();
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
		string text = Path.GetFileNameWithoutExtension(filePath);
		SlugcatStats.Name name = null;
		if (fileNameWithoutExtension.Contains("-"))
		{
			string[] array = fileNameWithoutExtension.Split('-');
			for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
			{
				if (ExtEnum<SlugcatStats.Name>.values.GetEntry(i).ToLowerInvariant() == array[1])
				{
					name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
					break;
				}
			}
			text = array[0];
		}
		int result = -1;
		if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
		{
			LoadEventsFromFile(result, name, oneRandomLine: false, 0);
		}
		else
		{
			if (!ModManager.MSC)
			{
				return;
			}
			if (text.ToLowerInvariant().StartsWith("chatlog"))
			{
				ChatlogData.ChatlogID chatlogID = null;
				for (int j = 0; j < ExtEnum<ChatlogData.ChatlogID>.values.Count; j++)
				{
					if (ExtEnum<ChatlogData.ChatlogID>.values.GetEntry(j).ToLowerInvariant() == text.ToLowerInvariant())
					{
						chatlogID = new ChatlogData.ChatlogID(ExtEnum<ChatlogData.ChatlogID>.values.GetEntry(j));
						break;
					}
				}
				if (chatlogID != null)
				{
					chatlogMessages = ChatlogData.getChatlog(chatlogID);
				}
			}
			else if (text.ToLowerInvariant().StartsWith("lp"))
			{
				string[] array2 = text.Split('_');
				int result2 = -1;
				if (int.TryParse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
				{
					chatlogMessages = ChatlogData.getLinearBroadcast(result2, text.ToLowerInvariant().EndsWith("peb"));
				}
			}
			else
			{
				if (name == null)
				{
					name = SlugcatStats.Name.White;
				}
				chatlogMessages = ChatlogData.getDevComm(text, name);
			}
		}
	}
}
