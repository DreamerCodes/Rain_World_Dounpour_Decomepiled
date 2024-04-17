using System;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace ScavengerCosmetic;

public class HardBackSpikes : BackTuftsAndRidges
{
	public float[] sizes;

	public HardBackSpikes(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		pattern = ((UnityEngine.Random.value < 0.6f) ? Pattern.SpineRidge : Pattern.DoubleSpineRidge);
		if (UnityEngine.Random.value < 0.1f)
		{
			pattern = Pattern.RandomBackBlotch;
		}
		GeneratePattern(pattern);
		totalSprites = positions.Length * ((!base.Colored) ? 1 : 2);
		if (UnityEngine.Random.value < 0.5f)
		{
			if (UnityEngine.Random.value < 0.85f)
			{
				scaleGraf = UnityEngine.Random.Range(0, 4);
			}
			else
			{
				scaleGraf = 6;
			}
		}
		sizes = new float[positions.Length];
		float a = Mathf.Lerp(0.1f, 0.6f, UnityEngine.Random.value);
		float p = Mathf.Lerp(0.3f, 1f, UnityEngine.Random.value);
		generalSize = Custom.LerpMap(positions.Length, 5f, 35f, 1f, 0.2f);
		generalSize = Mathf.Lerp(generalSize, base.scavGrphs.scavenger.abstractCreature.personality.dominance, UnityEngine.Random.value);
		generalSize = Mathf.Lerp(generalSize, Mathf.Pow(UnityEngine.Random.value, 0.75f), UnityEngine.Random.value);
		for (int i = 0; i < sizes.Length; i++)
		{
			sizes[i] = Mathf.Lerp(a, 1f, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(top, bottom, positions[i].y), p) * (float)Math.PI));
		}
		if (!base.Colored)
		{
			return;
		}
		colorAlphas = new float[positions.Length];
		if (UnityEngine.Random.value < 0.25f + 0.5f * colored)
		{
			float a2 = float.MaxValue;
			float num = float.MinValue;
			for (int j = 0; j < positions.Length; j++)
			{
				a2 = Mathf.Min(a2, positions[j].y);
				num = Mathf.Max(num, positions[j].y);
			}
			float p2 = Mathf.Lerp(0.2f, 1.2f, UnityEngine.Random.value);
			for (int k = 0; k < colorAlphas.Length; k++)
			{
				colorAlphas[k] = Mathf.Lerp(colored, 0f, Mathf.Pow(Mathf.InverseLerp(a2, num, positions[k].y), p2));
			}
		}
		else
		{
			for (int l = 0; l < colorAlphas.Length; l++)
			{
				colorAlphas[l] = colored;
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		bool flag = false;
		if (owner is ScavengerGraphics)
		{
			flag = (owner as ScavengerGraphics).scavenger.Elite;
		}
		float num = (flag ? 1.5f : 1f);
		float num2 = (flag ? 1.5f : 1f);
		for (int i = 0; i < totalSprites / ((!base.Colored) ? 1 : 2); i++)
		{
			float2 relPos = new float2(positions[i].x, positions[i].y);
			float2 @float = base.scavGrphs.OnBackSurfacePos(relPos, timeStacker);
			float2 float2 = base.scavGrphs.OnSpineUpDir(positions[i].y, timeStacker) * Mathf.Lerp(4f, 14f * num2, generalSize) * (1f - 0.5f * Mathf.InverseLerp(0.5f, 1f, positions[i].y)) + base.scavGrphs.OnSpineDir(positions[i].y, timeStacker) * Mathf.Lerp(4f * num2, 18f * num2, generalSize) * 0.5f * Mathf.InverseLerp(0.5f, 1f, positions[i].y);
			float num3 = 1f + Mathf.Lerp(base.scavGrphs.lastBristle, base.scavGrphs.bristle, timeStacker) * 0.5f;
			Vector2 vector = Custom.RNV() * UnityEngine.Random.value * 0.2f * Mathf.Lerp(base.scavGrphs.lastBristle, base.scavGrphs.bristle, timeStacker);
			Vector2 vector2 = new Vector2(float2.x * num3 + vector.x, float2.y * num3 + vector.y);
			for (int j = 0; j < ((!base.Colored) ? 1 : 2); j++)
			{
				int num4 = firstSprite + i * ((!base.Colored) ? 1 : 2) + j;
				sLeaser.sprites[num4].rotation = Custom.VecToDeg(vector2.normalized);
				sLeaser.sprites[num4].x = @float.x - camPos.x;
				sLeaser.sprites[num4].y = @float.y - camPos.y;
				sLeaser.sprites[num4].scaleX = Mathf.Sign(Mathf.Lerp(base.scavGrphs.lastFlip, base.scavGrphs.flip, timeStacker) + positions[i].x * 0.5f) * Mathf.Lerp(0.5f * num, 1f * num, generalSize) * xFlip;
				sLeaser.sprites[num4].scaleY = vector2.magnitude / scaleGrafHeight;
			}
		}
	}
}
