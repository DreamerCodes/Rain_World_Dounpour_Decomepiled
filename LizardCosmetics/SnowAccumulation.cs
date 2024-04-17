using System;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class SnowAccumulation : Template
{
	public string[] debrisName;

	public float[] debrisRotate;

	public float[] debrisSpinePos;

	public float[] debrisAlphas;

	public float[] debrisX;

	public float[] debrisY;

	public int debrisRevealed;

	public int debrisRevealCounter;

	public float DebrisSaturation => (float)debrisRevealed / (float)numberOfSprites;

	public SnowAccumulation(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		numberOfSprites = 16;
		debrisName = new string[numberOfSprites];
		debrisRotate = new float[numberOfSprites];
		debrisSpinePos = new float[numberOfSprites];
		debrisAlphas = new float[numberOfSprites];
		debrisX = new float[numberOfSprites];
		debrisY = new float[numberOfSprites];
		for (int i = 0; i < numberOfSprites; i++)
		{
			float value = UnityEngine.Random.value;
			if (value < 0.2f)
			{
				debrisName[i] = "Cicada6head";
			}
			else if (value < 0.4f)
			{
				debrisName[i] = "Cicada8head";
			}
			else if (value < 0.6f)
			{
				debrisName[i] = "JellyFish0A";
			}
			else if (value < 0.8f)
			{
				debrisName[i] = "mouseHeadDown";
			}
			else
			{
				debrisName[i] = "Cicada3head";
			}
			debrisRotate[i] = UnityEngine.Random.Range(-40f, 40f);
			debrisSpinePos[i] = UnityEngine.Random.Range(0.06f, 0.7f);
			debrisAlphas[i] = 0f;
			debrisX[i] = 0f;
			debrisY[i] = 0f;
		}
	}

	public override void Update()
	{
		for (int i = 0; i < numberOfSprites; i++)
		{
			if (i + 1 <= debrisRevealed)
			{
				debrisAlphas[i] = Mathf.Lerp(debrisAlphas[i], 1f, 0.1f);
			}
			else if (debrisAlphas[i] > 0f)
			{
				debrisAlphas[i] = 0f;
				if (lGraphics.lizard.room != null)
				{
					Vector2 vector = Custom.RNV() * 10f;
					CentipedeShell obj = new CentipedeShell(vel: new Vector2(vector.x, Mathf.Abs(vector.y)), pos: new Vector2(debrisX[i], debrisY[i] + 20f), overrideColor: Color.Lerp(Color.white, Color.Lerp(Color.white, lGraphics.whiteCamoColor, 0.3f), Mathf.Min(1f, DebrisSaturation * 1.5f)), scaleX: 0.75f, scaleY: 0.75f, overrideSprite: debrisName[i]);
					lGraphics.lizard.room.AddObject(obj);
				}
			}
		}
		if (debrisRevealed < numberOfSprites && lGraphics.lizard.HypothermiaExposure > 0f)
		{
			debrisRevealCounter++;
			if (debrisRevealCounter > 60)
			{
				debrisRevealed++;
				debrisRevealCounter = 0;
			}
		}
	}

	public void ShatterDebris()
	{
		if (debrisRevealed > 0)
		{
			debrisRevealed = Math.Max(debrisRevealed - UnityEngine.Random.Range(4, 8), 0);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < numberOfSprites; i++)
		{
			sLeaser.sprites[startSprite + i] = new FSprite(debrisName[i]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float b = Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
		float a = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		for (int i = 0; i < numberOfSprites; i++)
		{
			float num = debrisSpinePos[i];
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(num, timeStacker);
			Vector2 vector = lizardSpineData.dir;
			Vector2 pos = lizardSpineData.pos;
			if (num < 0.34f)
			{
				vector = (vector - Custom.DirVec(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker))).normalized;
			}
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			Vector2 vector3 = Custom.DegToVec(50f * Mathf.Lerp(a, b, 0.2f + debrisSpinePos[i]) + debrisRotate[i]);
			float num2 = Mathf.Lerp(0.9f, 0.3f, num);
			float num3 = Mathf.Lerp(1.8f, 0.7f, num);
			Vector2 p = pos + vector2 * lizardSpineData.rad * num3 * vector3.x;
			Vector2 vector4 = vector;
			vector4 = ((!(num < 0.34f)) ? (vector4 + 2f * Custom.DirVec(p, Vector2.Lerp(lGraphics.tail[0].lastPos, lGraphics.tail[0].pos, timeStacker)) * Mathf.Abs(vector3.y)).normalized : (vector4 - 2f * Custom.DirVec(p, Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)) * Mathf.Abs(vector3.y)).normalized);
			sLeaser.sprites[startSprite + i].x = p.x - camPos.x;
			sLeaser.sprites[startSprite + i].y = p.y - camPos.y;
			sLeaser.sprites[startSprite + i].rotation = Custom.VecToDeg(vector4);
			float t = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(vector3.x)), 2f);
			sLeaser.sprites[startSprite + i].scaleX = ((vector3.y > 0f) ? Mathf.Lerp(num2 * 0.9f, 0f, t) : 0f);
			sLeaser.sprites[startSprite + i].scaleY = num2 * 1.1f;
			sLeaser.sprites[startSprite + i].alpha = debrisAlphas[i];
			sLeaser.sprites[startSprite + i].color = Color.Lerp(Color.white, Color.Lerp(Color.white, lGraphics.whiteCamoColor, 0.3f), Mathf.Min(1f, DebrisSaturation * 1.5f));
			debrisX[i] = sLeaser.sprites[startSprite + i].x;
			debrisY[i] = sLeaser.sprites[startSprite + i].y;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < numberOfSprites; i++)
		{
			sLeaser.sprites[startSprite + i].color = Color.white;
		}
	}
}
