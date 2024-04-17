using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ChatLogDisplay : HudPart
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
			this.text = Custom.ReplaceLineDelimeters(text);
			this.xOrientation = xOrientation;
			linger = Mathf.Max(150, (int)((float)text.Length * 3f) + extraLinger);
			string[] array = Regex.Split(text.Replace("<WWLINE>", ""), "<LINE>");
			for (int i = 0; i < array.Length; i++)
			{
				longestLine = Math.Max(longestLine, array[i].Length);
			}
			lines = array.Length;
			this.yPos = yPos + (20f + 15f * (float)lines);
		}
	}

	private int showDelay;

	public float defaultXOrientation;

	public float defaultYPos;

	public FLabel[] label;

	public List<Message> messages;

	private int showCharacter;

	private int showLine;

	private string showText;

	private float width;

	private float totalHeight;

	private int lingerCounter;

	public static float meanCharWidth = 6f;

	public static float lineHeight = 15f;

	public static float widthMargin = 30f;

	public static float messageSep = 15f;

	public FSprite[] sprites;

	public float mainAlpha;

	public float[] messageWidths;

	public bool disable_fastDisplay;

	private float fixedTime;

	public bool permanentDisplay;

	public Message CurrentMessage => messages[showLine];

	public ChatLogDisplay(global::HUD.HUD hud, string[] chatLog)
		: base(hud)
	{
		mainAlpha = 1f;
		defaultXOrientation = 0.5f;
		messages = new List<Message>();
		for (int i = 0; i < chatLog.Length; i++)
		{
			if (!(chatLog[i] == ""))
			{
				if (i == chatLog.Length - 1)
				{
					NewMessage(chatLog[i], 250);
				}
				else
				{
					NewMessage(chatLog[i], 0);
				}
			}
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
		if (InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage))
		{
			lineHeight = 30f;
		}
		else
		{
			lineHeight = 15f;
		}
		InitiateSprites();
		if (hud.owner is Player)
		{
			(base.hud.owner as Player).abstractCreature.world.game.pauseUpdate = true;
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return new Vector2(CurrentMessage.xOrientation * hud.rainWorld.screenSize.x, CurrentMessage.yPos);
	}

	public override void Update()
	{
		if (permanentDisplay && hud.owner is Player)
		{
			(hud.owner as Player).abstractCreature.world.game.pauseUpdate = false;
		}
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

	public void InitiateSprites()
	{
		messageWidths = new float[messages.Count];
		sprites = new FSprite[messages.Count];
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i] = new FSprite("pixel");
			sprites[i].color = new Color(0f, 0f, 0f);
			sprites[i].alpha = 0.75f;
			hud.fContainers[1].AddChild(sprites[i]);
		}
		label = new FLabel[messages.Count];
		for (int j = 0; j < label.Length; j++)
		{
			label[j] = new FLabel(Custom.GetFont(), string.Empty);
			label[j].alignment = FLabelAlignment.Center;
			label[j].anchorX = 0f;
			label[j].anchorY = 1f;
			label[j].text = messages[j].text;
			if (Conversation.TryGetPrefixColor(messages[j].text, out var result))
			{
				label[j].color = result;
			}
			messageWidths[j] = label[j].textRect.width;
			label[j].text = "";
			hud.fContainers[1].AddChild(label[j]);
		}
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		fixedTime += Time.deltaTime;
		if (fixedTime > 0.015f && (!(hud.owner is Player) || (hud.owner as Player).abstractCreature.world.game.pauseMenu == null))
		{
			fixedTime = 0f;
			bool flag = RWInput.CheckSpecificButton(0, 0) && !disable_fastDisplay;
			if (showCharacter < CurrentMessage.text.Length)
			{
				if (permanentDisplay)
				{
					showDelay = 0;
					showCharacter = CurrentMessage.text.Length;
				}
				else if (flag)
				{
					showDelay = 0;
					showCharacter = Mathf.Clamp(showCharacter + 3, 0, CurrentMessage.text.Length);
				}
				else
				{
					showDelay++;
					if (showDelay > DialogBox.GetDelay())
					{
						showDelay = 0;
						showCharacter++;
					}
				}
				showText = CurrentMessage.text.Substring(0, showCharacter);
			}
			if (showLine >= messages.Count - 1 && lingerCounter > CurrentMessage.linger && !permanentDisplay)
			{
				mainAlpha = Mathf.Lerp(mainAlpha, 0f, 0.05f);
			}
			if (showCharacter >= CurrentMessage.text.Length)
			{
				if (flag)
				{
					lingerCounter += 10;
				}
				else
				{
					lingerCounter++;
				}
				if (lingerCounter > CurrentMessage.linger || permanentDisplay)
				{
					if (showLine < messages.Count - 1)
					{
						InitNextMessage();
						return;
					}
					if (mainAlpha < 0.01f)
					{
						if (hud.owner is Player)
						{
							(hud.owner as Player).abstractCreature.world.game.pauseUpdate = false;
						}
						slatedForDeletion = true;
					}
				}
			}
		}
		float num = hud.rainWorld.screenSize.y / 2f + totalHeight / 2f;
		float num2 = 3f;
		for (int i = 0; i < label.Length; i++)
		{
			Vector2 vector = DrawPos(timeStacker);
			Vector2 vector2 = new Vector2(0f, num);
			vector2.x = widthMargin + messageWidths[i];
			label[i].x = (float)Mathf.FloorToInt((vector.x - messageWidths[i] * 0.5f) / 2f) * 2f;
			label[i].y = (float)Mathf.FloorToInt((num - lineHeight * 0.6666f) / 2f) * 2f;
			label[i].x -= 1f / 3f;
			label[i].y -= 1f / 3f;
			if (i <= showLine)
			{
				sprites[i].scaleX = label[i].textRect.width + num2 * 2f;
				sprites[i].isVisible = true;
				sprites[i].x = label[i].x + sprites[i].scaleX / 2f - num2;
				sprites[i].y = label[i].y - label[i].textRect.height * 0.5f;
				sprites[i].scaleY = label[i].textRect.height + num2 * 2f;
			}
			else
			{
				sprites[i].isVisible = false;
			}
			sprites[i].alpha = 0.75f * mainAlpha;
			label[i].alpha = mainAlpha;
			if (i == showLine)
			{
				label[i].text = showText;
			}
			else if (i < showLine)
			{
				label[i].text = messages[i].text;
			}
			else
			{
				label[i].text = "";
			}
			num -= label[i].textRect.height + messageSep;
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < label.Length; i++)
		{
			label[i].RemoveFromContainer();
			sprites[i].RemoveFromContainer();
		}
	}
}
