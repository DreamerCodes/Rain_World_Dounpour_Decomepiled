using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HUD;
using UnityEngine;

namespace DevInterface;

public class DialogPage : Page, Conversation.IOwnAConversation
{
	private SelectDialogPanel dialogPanel;

	private ConversationLoader convoLoader;

	private List<DialogBox> dialogBoxes;

	private FSprite leftBoundary;

	private FSprite rightBoundary;

	private float shiftY;

	public RainWorld rainWorld => owner.game.rainWorld;

	public DialogPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		string[] array = AssetManager.ListDirectory("text" + Path.DirectorySeparatorChar + "text_" + LocalizationTranslator.LangShort(owner.game.rainWorld.inGameTranslator.currentLanguage), directories: false, includeAll: false);
		convoLoader = new ConversationLoader(this);
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].ToLowerInvariant().EndsWith(".txt"))
			{
				list.Add(array[i]);
			}
		}
		dialogPanel = new SelectDialogPanel(owner, this, new Vector2(1050f, 20f), list.ToArray());
		dialogBoxes = new List<DialogBox>();
		subNodes.Add(dialogPanel);
		leftBoundary = new FSprite("pixel");
		leftBoundary.anchorY = 0f;
		leftBoundary.scaleY = 768f;
		leftBoundary.x = owner.game.rainWorld.options.ScreenSize.x / 2f - 512f;
		leftBoundary.y = 0f;
		leftBoundary.color = Color.red;
		Futile.stage.AddChild(leftBoundary);
		rightBoundary = new FSprite("pixel");
		rightBoundary.anchorY = 0f;
		rightBoundary.scaleY = 768f;
		rightBoundary.x = owner.game.rainWorld.options.ScreenSize.x / 2f + 512f;
		rightBoundary.y = 0f;
		rightBoundary.color = Color.red;
		Futile.stage.AddChild(rightBoundary);
	}

	public override void Update()
	{
		base.Update();
		float num = 0f;
		for (int num2 = dialogBoxes.Count - 1; num2 >= 0; num2--)
		{
			if (num == 0f)
			{
				num = dialogBoxes[num2].defaultYPos + (20f + 15f * (float)dialogBoxes[num2].CurrentMessage.lines) + shiftY;
			}
			dialogBoxes[num2].CurrentMessage.yPos = num;
			if (num2 > 0)
			{
				num += dialogBoxes[num2].label.textRect.height / 2f + dialogBoxes[num2 - 1].label.textRect.height / 2f + 25f;
			}
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			shiftY -= 5f;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			shiftY += 5f;
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		ClearDialogs();
		leftBoundary.RemoveFromContainer();
		rightBoundary.RemoveFromContainer();
	}

	public void ClearDialogs()
	{
		for (int i = 0; i < dialogBoxes.Count; i++)
		{
			dialogBoxes[i].label.RemoveFromContainer();
			dialogBoxes[i].ClearSprites();
			dialogBoxes[i].slatedForDeletion = true;
			owner.game.cameras[0].hud.parts.Remove(dialogBoxes[i]);
		}
		dialogBoxes.Clear();
		if (ModManager.MSC)
		{
			owner.game.cameras[0].hud.DisposeChatLog();
		}
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender.IDstring == "BackPage99288..?/~")
		{
			dialogPanel.PrevPage();
		}
		else if (sender.IDstring == "NextPage99288..?/~")
		{
			dialogPanel.NextPage();
		}
		else
		{
			if (!File.Exists(sender.IDstring))
			{
				return;
			}
			shiftY = 0f;
			ClearDialogs();
			convoLoader.LoadEvents(sender.IDstring);
			if (convoLoader.chatlogMessages == null && convoLoader.events.Count > 0)
			{
				for (int i = 0; i < convoLoader.events.Count; i++)
				{
					if (convoLoader.events[i] is Conversation.TextEvent)
					{
						DialogBox dialogBox = new DialogBox(owner.game.cameras[0].hud);
						dialogBox.permanentDisplay = true;
						owner.game.cameras[0].hud.AddPart(dialogBox);
						dialogBoxes.Add(dialogBox);
						dialogBox.Interrupt(ReplaceParts((convoLoader.events[i] as Conversation.TextEvent).text), 0);
					}
				}
			}
			else if (convoLoader.chatlogMessages != null)
			{
				owner.game.cameras[0].hud.InitChatLog(convoLoader.chatlogMessages).permanentDisplay = true;
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	protected string NameForPlayer(bool capitalized)
	{
		string text = owner.game.rainWorld.inGameTranslator.Translate("creature");
		string text2 = owner.game.rainWorld.inGameTranslator.Translate("little");
		if (capitalized && InGameTranslator.LanguageID.UsesCapitals(owner.game.rainWorld.inGameTranslator.currentLanguage))
		{
			text2 = char.ToUpper(text2[0]) + text2.Substring(1);
		}
		return text2 + " " + text;
	}

	public string ReplaceParts(string s)
	{
		s = Regex.Replace(s, "<PLAYERNAME>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CAPPLAYERNAME>", NameForPlayer(capitalized: true));
		s = Regex.Replace(s, "<PlayerName>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CapPlayerName>", NameForPlayer(capitalized: true));
		return s;
	}

	public void SpecialEvent(string eventName)
	{
	}
}
