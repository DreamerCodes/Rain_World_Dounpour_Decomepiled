using System;
using RWCustom;
using UnityEngine;

public class OracleChatLabel : GlyphLabel
{
	public OracleBehavior oracleBehav;

	public int[] glyphsPerLine;

	public Vector2 offsetPos;

	public Vector2 lastOffsetPos;

	public int revealCounter;

	public int totalGlyphsToShow;

	public int lingerCounter;

	public int timeToLinger;

	public bool finishedShowingMessage;

	public int inPlaceCounter;

	public int blinkCounter;

	private IntVector2 blinkWhenFinished;

	private bool visible;

	private bool lastVisible;

	public OracleChatLabel(OracleBehavior oracleBehav)
		: base(default(Vector2), new int[40])
	{
		this.oracleBehav = oracleBehav;
		glyphsPerLine = new int[4];
		for (int i = 0; i < glyphsPerLine.Length; i++)
		{
			glyphsPerLine[i] = 10;
		}
		visibleGlyphs = 0;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastOffsetPos = offsetPos;
		int num = 0;
		for (int i = 0; i < glyphsPerLine.Length; i++)
		{
			num = Math.Max(num, glyphsPerLine[i]);
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < visibleGlyphs; j++)
		{
			num4++;
			if (num4 >= glyphsPerLine[num3])
			{
				num4 = 0;
				num3++;
				if (num3 < 4 && glyphsPerLine[num3] > 0)
				{
					num2++;
				}
			}
		}
		offsetPos.x = (float)(-num) * 15f * scale * 0.5f;
		offsetPos.y = (float)num2 * scale * 10f;
		if (Vector2.Distance(lastPos, pos) < 10f)
		{
			inPlaceCounter++;
		}
		else
		{
			inPlaceCounter = 0;
		}
		if (inPlaceCounter > 20)
		{
			if (visibleGlyphs < totalGlyphsToShow)
			{
				if (revealCounter > 0)
				{
					revealCounter--;
				}
				else
				{
					room.PlaySound(SoundID.SS_AI_Text, 0f, 1f, 1f);
					visibleGlyphs++;
					revealCounter = 4;
				}
			}
			else
			{
				lingerCounter++;
				if (lingerCounter > timeToLinger)
				{
					finishedShowingMessage = true;
				}
			}
		}
		blinkCounter++;
		lastVisible = visible;
		visible = lingerCounter <= 0 || blinkWhenFinished.x <= 0 || blinkCounter % blinkWhenFinished.x <= blinkWhenFinished.y || totalGlyphsToShow <= 0;
		if (!lastVisible && visible)
		{
			room.PlaySound(SoundID.SS_AI_Text_Blink, 0f, 1f, 1f);
		}
	}

	public void NewPhrase(int message)
	{
		Custom.Log("New message", message.ToString());
		for (int i = 0; i < glyphs.Length; i++)
		{
			glyphs[i] = -1;
		}
		visibleGlyphs = 0;
		timeToLinger = 0;
		lingerCounter = 0;
		blinkWhenFinished.x = 0;
		finishedShowingMessage = false;
		switch (message)
		{
		case 0:
			CreateMessage(7, 0, 0, 0, cyrillic: false, 2312);
			timeToLinger = 30;
			break;
		case 1:
			CreateMessage(6, 8, 8, 0, cyrillic: false, 342432);
			glyphs[8] = -1;
			timeToLinger = 60;
			break;
		case 2:
			CreateMessage(2, 6, 0, 0, cyrillic: false, 4432);
			timeToLinger = 80;
			setScale = 1.5f;
			blinkWhenFinished.x = 30;
			blinkWhenFinished.y = 25;
			break;
		case 3:
			CreateMessage(9, 9, 0, 0, cyrillic: true, 11332);
			timeToLinger = 70;
			setScale = 1.75f;
			break;
		case 4:
			CreateMessage(9, 5, 10, 9, cyrillic: false, 124432);
			timeToLinger = 110;
			setScale = 1f;
			break;
		case 5:
			CreateMessage(3, 4, 0, 0, cyrillic: false, 85432);
			timeToLinger = 200;
			setScale = 4f;
			blinkWhenFinished.x = 8;
			blinkWhenFinished.y = 4;
			break;
		case 10:
			CreateMessage(9, 0, 0, 0, cyrillic: false, 3654);
			timeToLinger = 45;
			break;
		case 11:
			CreateMessage(2, 2, 0, 0, cyrillic: false, 565);
			setScale = 3f;
			timeToLinger = 45;
			blinkWhenFinished.x = 10;
			blinkWhenFinished.y = 5;
			break;
		case 12:
			CreateMessage(4, 10, 8, 9, cyrillic: false, 4554);
			setScale = 1f;
			timeToLinger = 110;
			break;
		case 13:
			CreateMessage(5, 7, 3, 0, cyrillic: false, 12869);
			timeToLinger = 60;
			break;
		case 50:
			CreateMessage(5, 0, 0, 0, cyrillic: false, 20958);
			setScale = 2f;
			timeToLinger = 100;
			blinkWhenFinished.x = 30;
			blinkWhenFinished.y = 25;
			break;
		case 51:
			CreateMessage(4, 5, 1, 0, cyrillic: false, 1150289240);
			setScale = 2f;
			timeToLinger = 100;
			blinkWhenFinished.x = 30;
			blinkWhenFinished.y = 25;
			break;
		case 99:
			CreateMessage(5, 6, 4, 0, cyrillic: false, 21958);
			setScale = 3f;
			timeToLinger = 200;
			blinkWhenFinished.x = 30;
			blinkWhenFinished.y = 25;
			break;
		}
	}

	public void Hide()
	{
		visibleGlyphs = 0;
		totalGlyphsToShow = 0;
	}

	private void CreateMessage(int firstLine, int secondLine, int thirdLine, int fourthLine, bool cyrillic, int seed)
	{
		glyphsPerLine[0] = firstLine;
		int[] array = GlyphLabel.RandomString(firstLine, seed, cyrillic);
		for (int i = 0; i < array.Length; i++)
		{
			glyphs[i] = array[i];
		}
		glyphsPerLine[1] = secondLine;
		array = GlyphLabel.RandomString(secondLine, seed + 1, cyrillic);
		for (int j = 0; j < array.Length; j++)
		{
			glyphs[firstLine + j] = array[j];
		}
		glyphsPerLine[2] = thirdLine;
		array = GlyphLabel.RandomString(thirdLine, seed + 2, cyrillic);
		for (int k = 0; k < array.Length; k++)
		{
			glyphs[firstLine + secondLine + k] = array[k];
		}
		glyphsPerLine[3] = fourthLine;
		array = GlyphLabel.RandomString(fourthLine, seed + 3, cyrillic);
		for (int l = 0; l < array.Length; l++)
		{
			glyphs[firstLine + secondLine + thirdLine + l] = array[l];
		}
		totalGlyphsToShow = firstLine + secondLine + thirdLine + fourthLine;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!visible)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = false;
			}
			return;
		}
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) + Vector2.Lerp(lastOffsetPos, offsetPos, timeStacker);
		float num = Mathf.Lerp(lastScale, scale, timeStacker);
		IntVector2 intVector = new IntVector2(0, 0);
		for (int j = 0; j < sLeaser.sprites.Length; j++)
		{
			sLeaser.sprites[j].x = vector.x + (float)intVector.x * 15f * num - camPos.x;
			sLeaser.sprites[j].y = vector.y - (float)intVector.y * 20f * num - camPos.y;
			sLeaser.sprites[j].isVisible = j < visibleGlyphs && glyphs[j] >= 0;
			sLeaser.sprites[j].alpha = (float)glyphs[j] / 50f;
			if (ModManager.MSC)
			{
				sLeaser.sprites[j].color = color;
			}
			intVector.x++;
			if (intVector.y < glyphsPerLine.Length && intVector.x >= glyphsPerLine[intVector.y])
			{
				intVector.y++;
				intVector.x = 0;
			}
			sLeaser.sprites[j].scaleX = 15f / sLeaser.sprites[j].element.sourcePixelSize.x * num;
			sLeaser.sprites[j].scaleY = num;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = ((!ModManager.MSC || oracleBehav != null) ? rCam.ReturnFContainer("BackgroundShortcuts") : rCam.ReturnFContainer("GrabShaders"));
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
