using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace HUD;

public class DialogBox : HudPart
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
			linger = (int)Mathf.Lerp((float)text.Length * 2f, 80f, 0.5f) + extraLinger;
			string[] array = Regex.Split(text.Replace("<WWLINE>", ""), "<LINE>");
			for (int i = 0; i < array.Length; i++)
			{
				longestLine = Math.Max(longestLine, array[i].Length);
			}
			lines = array.Length;
			this.yPos = yPos + (20f + 15f * (float)lines);
		}
	}

	public float defaultXOrientation = 0.5f;

	public float defaultYPos;

	public FLabel label;

	public FSprite[] sprites;

	public List<Message> messages;

	private int showCharacter;

	private string showText;

	private float sizeFac;

	private float lastSizeFac;

	private float width;

	private int lingerCounter;

	public bool permanentDisplay;

	public float actualWidth;

	public Color currentColor;

	private int showDelay;

	public static float meanCharWidth = 6f;

	public static float lineHeight = 15f;

	public static float heightMargin = 20f;

	public static float widthMargin = 30f;

	private int MainFillSprite => 8;

	public Message CurrentMessage
	{
		get
		{
			if (messages.Count < 1)
			{
				return null;
			}
			return messages[0];
		}
	}

	public bool ShowingAMessage => CurrentMessage != null;

	private int SideSprite(int side)
	{
		return 9 + side;
	}

	private int CornerSprite(int corner)
	{
		return 13 + corner;
	}

	private int FillSideSprite(int side)
	{
		return side;
	}

	private int FillCornerSprite(int corner)
	{
		return 4 + corner;
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return new Vector2(CurrentMessage.xOrientation * hud.rainWorld.screenSize.x, CurrentMessage.yPos + hud.rainWorld.options.SafeScreenOffset.y);
	}

	public static int GetDelay()
	{
		if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Japanese)
		{
			return 2;
		}
		if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Korean)
		{
			return 2;
		}
		if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Chinese)
		{
			return 3;
		}
		return 1;
	}

	public DialogBox(HUD hud)
		: base(hud)
	{
		messages = new List<Message>();
		currentColor = Color.white;
		InitiateSprites();
	}

	public override void Update()
	{
		if (CurrentMessage == null)
		{
			return;
		}
		lastSizeFac = sizeFac;
		if (sizeFac < 1f && lingerCounter < 1)
		{
			sizeFac = Mathf.Min(sizeFac + 1f / 6f, 1f);
			return;
		}
		if (permanentDisplay)
		{
			showDelay = 0;
			showCharacter = CurrentMessage.text.Length;
			showText = CurrentMessage.text;
		}
		else if (showCharacter < CurrentMessage.text.Length)
		{
			showDelay++;
			if (showDelay >= GetDelay())
			{
				showDelay = 0;
				showCharacter++;
				showText = CurrentMessage.text.Substring(0, showCharacter);
			}
		}
		else
		{
			if (hud.owner.GetOwnerType() != HUD.OwnerType.Player || (hud.owner as Player).abstractCreature.world.game.pauseMenu == null)
			{
				lingerCounter++;
			}
			if (lingerCounter > CurrentMessage.linger)
			{
				showText = "";
				if (sizeFac > 0f)
				{
					sizeFac = Mathf.Max(0f, sizeFac - 1f / 6f);
				}
				else
				{
					messages.RemoveAt(0);
					if (messages.Count > 0)
					{
						InitNextMessage();
					}
				}
			}
		}
		if (ShowingAMessage && hud.owner.GetOwnerType() == HUD.OwnerType.Player && (hud.owner as Player).graphicsModule != null && (hud.owner as Player).abstractCreature.world.game.IsStorySession && (hud.owner as Player).abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
		{
			((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha = Mathf.Min(1f, ((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha + 0.005f);
			((hud.owner as Player).graphicsModule as PlayerGraphics).markAlpha = Mathf.Min(0.5f + UnityEngine.Random.value, 1f);
		}
	}

	public void Interrupt(string text, int extraLinger)
	{
		if (messages.Count > 0)
		{
			messages = new List<Message> { messages[0] };
			lingerCounter = messages[0].linger + 1;
			showCharacter = messages[0].text.Length + 2;
		}
		NewMessage(text, defaultXOrientation, defaultYPos, extraLinger);
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
			InitNextMessage();
		}
	}

	private void InitNextMessage()
	{
		showCharacter = 0;
		showText = "";
		lastSizeFac = 0f;
		sizeFac = 0f;
		lingerCounter = 0;
		label.text = CurrentMessage.text;
		actualWidth = label.textRect.width;
		label.text = "";
	}

	public void InitiateSprites()
	{
		sprites = new FSprite[17];
		for (int i = 0; i < 4; i++)
		{
			sprites[SideSprite(i)] = new FSprite("pixel");
			sprites[SideSprite(i)].scaleY = 2f;
			sprites[SideSprite(i)].scaleX = 2f;
			sprites[CornerSprite(i)] = new FSprite("UIroundedCorner");
			sprites[FillSideSprite(i)] = new FSprite("pixel");
			sprites[FillSideSprite(i)].scaleY = 6f;
			sprites[FillSideSprite(i)].scaleX = 6f;
			sprites[FillCornerSprite(i)] = new FSprite("UIroundedCornerInside");
		}
		sprites[SideSprite(0)].anchorY = 0f;
		sprites[SideSprite(2)].anchorY = 0f;
		sprites[SideSprite(1)].anchorX = 0f;
		sprites[SideSprite(3)].anchorX = 0f;
		sprites[CornerSprite(0)].scaleY = -1f;
		sprites[CornerSprite(2)].scaleX = -1f;
		sprites[CornerSprite(3)].scaleY = -1f;
		sprites[CornerSprite(3)].scaleX = -1f;
		sprites[MainFillSprite] = new FSprite("pixel");
		sprites[MainFillSprite].anchorY = 0f;
		sprites[MainFillSprite].anchorX = 0f;
		sprites[FillSideSprite(0)].anchorY = 0f;
		sprites[FillSideSprite(2)].anchorY = 0f;
		sprites[FillSideSprite(1)].anchorX = 0f;
		sprites[FillSideSprite(3)].anchorX = 0f;
		sprites[FillCornerSprite(0)].scaleY = -1f;
		sprites[FillCornerSprite(2)].scaleX = -1f;
		sprites[FillCornerSprite(3)].scaleY = -1f;
		sprites[FillCornerSprite(3)].scaleX = -1f;
		for (int j = 0; j < 9; j++)
		{
			sprites[j].color = new Color(0f, 0f, 0f);
			sprites[j].alpha = 0.75f;
		}
		label = new FLabel(Custom.GetFont(), "");
		label.alignment = FLabelAlignment.Left;
		label.anchorX = 0f;
		label.anchorY = 1f;
		for (int k = 0; k < sprites.Length; k++)
		{
			hud.fContainers[1].AddChild(sprites[k]);
		}
		hud.fContainers[1].AddChild(label);
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].isVisible = CurrentMessage != null;
		}
		label.isVisible = CurrentMessage != null;
		if (CurrentMessage != null)
		{
			if (InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage))
			{
				_ = label.FontMaxCharWidth;
			}
			else
			{
				_ = meanCharWidth;
			}
			float num = ((!InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage)) ? lineHeight : label.FontLineHeight);
			Vector2 vector = DrawPos(timeStacker);
			Vector2 vector2 = new Vector2(0f, heightMargin + num * (float)CurrentMessage.lines);
			if (Custom.GetFont().Contains("Full"))
			{
				vector2.y += LabelTest.LineHalfHeight(bigText: false);
			}
			vector2.x = widthMargin + actualWidth;
			vector2.x = Mathf.Lerp(40f, vector2.x, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastSizeFac, sizeFac, timeStacker)), 0.5f));
			vector2.y *= 0.5f + 0.5f * Mathf.Lerp(lastSizeFac, sizeFac, timeStacker);
			vector.x -= 1f / 3f;
			vector.y -= 1f / 3f;
			label.color = currentColor;
			label.x = vector.x - actualWidth * 0.5f;
			label.y = vector.y + vector2.y / 2f - num * ((!InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage)) ? 0.6666f : 0.3333f);
			label.text = showText;
			vector.x -= vector2.x / 2f;
			vector.y -= vector2.y / 2f;
			sprites[SideSprite(0)].x = vector.x + 1f;
			sprites[SideSprite(0)].y = vector.y + 6f;
			sprites[SideSprite(0)].scaleY = vector2.y - 12f;
			sprites[SideSprite(1)].x = vector.x + 6f;
			sprites[SideSprite(1)].y = vector.y + vector2.y - 1f;
			sprites[SideSprite(1)].scaleX = vector2.x - 12f;
			sprites[SideSprite(2)].x = vector.x + vector2.x - 1f;
			sprites[SideSprite(2)].y = vector.y + 6f;
			sprites[SideSprite(2)].scaleY = vector2.y - 12f;
			sprites[SideSprite(3)].x = vector.x + 6f;
			sprites[SideSprite(3)].y = vector.y + 1f;
			sprites[SideSprite(3)].scaleX = vector2.x - 12f;
			sprites[CornerSprite(0)].x = vector.x + 3.5f;
			sprites[CornerSprite(0)].y = vector.y + 3.5f;
			sprites[CornerSprite(1)].x = vector.x + 3.5f;
			sprites[CornerSprite(1)].y = vector.y + vector2.y - 3.5f;
			sprites[CornerSprite(2)].x = vector.x + vector2.x - 3.5f;
			sprites[CornerSprite(2)].y = vector.y + vector2.y - 3.5f;
			sprites[CornerSprite(3)].x = vector.x + vector2.x - 3.5f;
			sprites[CornerSprite(3)].y = vector.y + 3.5f;
			Color color = new Color(1f, 1f, 1f);
			for (int j = 0; j < 4; j++)
			{
				sprites[SideSprite(j)].color = color;
				sprites[CornerSprite(j)].color = color;
			}
			sprites[FillSideSprite(0)].x = vector.x + 4f;
			sprites[FillSideSprite(0)].y = vector.y + 7f;
			sprites[FillSideSprite(0)].scaleY = vector2.y - 14f;
			sprites[FillSideSprite(1)].x = vector.x + 7f;
			sprites[FillSideSprite(1)].y = vector.y + vector2.y - 4f;
			sprites[FillSideSprite(1)].scaleX = vector2.x - 14f;
			sprites[FillSideSprite(2)].x = vector.x + vector2.x - 4f;
			sprites[FillSideSprite(2)].y = vector.y + 7f;
			sprites[FillSideSprite(2)].scaleY = vector2.y - 14f;
			sprites[FillSideSprite(3)].x = vector.x + 7f;
			sprites[FillSideSprite(3)].y = vector.y + 4f;
			sprites[FillSideSprite(3)].scaleX = vector2.x - 14f;
			sprites[FillCornerSprite(0)].x = vector.x + 3.5f;
			sprites[FillCornerSprite(0)].y = vector.y + 3.5f;
			sprites[FillCornerSprite(1)].x = vector.x + 3.5f;
			sprites[FillCornerSprite(1)].y = vector.y + vector2.y - 3.5f;
			sprites[FillCornerSprite(2)].x = vector.x + vector2.x - 3.5f;
			sprites[FillCornerSprite(2)].y = vector.y + vector2.y - 3.5f;
			sprites[FillCornerSprite(3)].x = vector.x + vector2.x - 3.5f;
			sprites[FillCornerSprite(3)].y = vector.y + 3.5f;
			sprites[MainFillSprite].x = vector.x + 7f;
			sprites[MainFillSprite].y = vector.y + 7f;
			sprites[MainFillSprite].scaleX = vector2.x - 14f;
			sprites[MainFillSprite].scaleY = vector2.y - 14f;
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
	}
}
