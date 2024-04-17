using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class TailGeckoScales : Template
{
	public int rows;

	public int lines;

	private bool bigScales;

	public TailGeckoScales(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BehindHead;
		rows = Random.Range(7, 14);
		lines = Random.Range(3, Random.Range(3, 4));
		if (lGraphics.iVars.tailColor > 0.1f && Random.value < Mathf.Lerp(0.7f, 0.99f, lGraphics.iVars.tailColor))
		{
			bigScales = true;
			for (int i = 0; i < lGraphics.cosmetics.Count; i++)
			{
				if (lGraphics.cosmetics[i] is WingScales)
				{
					if ((lGraphics.cosmetics[i] as WingScales).scaleLength > 10f)
					{
						bigScales = false;
					}
					break;
				}
			}
		}
		if (Random.value < 0.5f)
		{
			rows += Random.Range(0, Random.Range(0, 7));
			lines += Random.Range(0, Random.Range(0, 3));
		}
		numberOfSprites = rows * lines;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < lines; j++)
			{
				if (bigScales)
				{
					sLeaser.sprites[startSprite + i * lines + j] = new FSprite("Circle20");
					sLeaser.sprites[startSprite + i * lines + j].scaleY = 0.3f;
				}
				else
				{
					sLeaser.sprites[startSprite + i * lines + j] = new FSprite("tinyStar");
				}
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (bigScales)
		{
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(0.4f, timeStacker);
			for (int i = 0; i < rows; i++)
			{
				float num = Mathf.InverseLerp(0f, rows - 1, i);
				float num2 = Mathf.Lerp(0.5f, 0.99f, Mathf.Pow(num, 0.8f));
				LizardGraphics.LizardSpineData lizardSpineData2 = lGraphics.SpinePosition(num2, timeStacker);
				Color a = lGraphics.BodyColor(num2);
				for (int j = 0; j < lines; j++)
				{
					float num3 = ((float)j + ((i % 2 == 0) ? 0.5f : 0f)) / (float)(lines - 1);
					num3 = -1f + 2f * num3;
					num3 += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
					if (num3 < -1f)
					{
						num3 += 2f;
					}
					else if (num3 > 1f)
					{
						num3 -= 2f;
					}
					Vector2 vector = lizardSpineData.pos + lizardSpineData.perp * (lizardSpineData.rad + 0.5f) * num3;
					Vector2 vector2 = lizardSpineData2.pos + lizardSpineData2.perp * (lizardSpineData2.rad + 0.5f) * num3;
					sLeaser.sprites[startSprite + i * lines + j].x = (vector.x + vector2.x) * 0.5f - camPos.x;
					sLeaser.sprites[startSprite + i * lines + j].y = (vector.y + vector2.y) * 0.5f - camPos.y;
					sLeaser.sprites[startSprite + i * lines + j].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
					sLeaser.sprites[startSprite + i * lines + j].scaleX = Custom.LerpMap(Mathf.Abs(num3), 0.4f, 1f, lizardSpineData2.rad * 3.5f / (float)rows, 0f) / 10f;
					sLeaser.sprites[startSprite + i * lines + j].scaleY = Vector2.Distance(vector, vector2) * 1.1f / 20f;
					if (lGraphics.iVars.tailColor > 0f)
					{
						float num4 = Mathf.InverseLerp(0.5f, 1f, Mathf.Abs(Vector2.Dot(Custom.DirVec(vector2, vector), Custom.DegToVec(-45f + 120f * num3))));
						num4 = Custom.LerpMap(Mathf.Abs(num3), 0.5f, 1f, 0.3f, 0f) + 0.7f * Mathf.Pow(num4 * Mathf.Pow(lGraphics.iVars.tailColor, 0.3f), Mathf.Lerp(2f, 0.5f, num));
						if (num < 0.5f)
						{
							num4 *= Custom.LerpMap(num, 0f, 0.5f, 0.2f, 1f);
						}
						num4 = Mathf.Pow(num4, Mathf.Lerp(2f, 0.5f, num));
						if (num4 < 0.5f)
						{
							sLeaser.sprites[startSprite + i * lines + j].color = Color.Lerp(a, lGraphics.effectColor, Mathf.InverseLerp(0f, 0.5f, num4));
						}
						else
						{
							sLeaser.sprites[startSprite + i * lines + j].color = Color.Lerp(lGraphics.effectColor, Color.white, Mathf.InverseLerp(0.5f, 1f, num4));
						}
					}
					else
					{
						sLeaser.sprites[startSprite + i * lines + j].color = Color.Lerp(a, lGraphics.effectColor, Custom.LerpMap(num, 0f, 0.8f, 0.2f, Custom.LerpMap(Mathf.Abs(num3), 0.5f, 1f, 0.8f, 0.4f), 0.8f));
					}
				}
				lizardSpineData = lizardSpineData2;
			}
			return;
		}
		for (int k = 0; k < rows; k++)
		{
			float f = Mathf.InverseLerp(0f, rows - 1, k);
			float num5 = Mathf.Lerp(0.4f, 0.95f, Mathf.Pow(f, 0.8f));
			LizardGraphics.LizardSpineData lizardSpineData3 = lGraphics.SpinePosition(num5, timeStacker);
			Color color = Color.Lerp(lGraphics.BodyColor(num5), lGraphics.effectColor, 0.2f + 0.8f * Mathf.Pow(f, 0.5f));
			for (int l = 0; l < lines; l++)
			{
				float num6 = ((float)l + ((k % 2 == 0) ? 0.5f : 0f)) / (float)(lines - 1);
				num6 = -1f + 2f * num6;
				num6 += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
				if (num6 < -1f)
				{
					num6 += 2f;
				}
				else if (num6 > 1f)
				{
					num6 -= 2f;
				}
				num6 = Mathf.Sign(num6) * Mathf.Pow(Mathf.Abs(num6), 0.6f);
				Vector2 vector3 = lizardSpineData3.pos + lizardSpineData3.perp * (lizardSpineData3.rad + 0.5f) * num6;
				sLeaser.sprites[startSprite + k * lines + l].x = vector3.x - camPos.x;
				sLeaser.sprites[startSprite + k * lines + l].y = vector3.y - camPos.y;
				sLeaser.sprites[startSprite + k * lines + l].color = new Color(1f, 0f, 0f);
				sLeaser.sprites[startSprite + k * lines + l].rotation = Custom.VecToDeg(lizardSpineData3.dir);
				sLeaser.sprites[startSprite + k * lines + l].scaleX = Custom.LerpMap(Mathf.Abs(num6), 0.4f, 1f, 1f, 0f);
				sLeaser.sprites[startSprite + k * lines + l].color = color;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
