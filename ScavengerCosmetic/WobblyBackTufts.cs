using System;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace ScavengerCosmetic;

public class WobblyBackTufts : BackTuftsAndRidges
{
	public Scale[] scales;

	public Vector2[] randomDirs;

	private float outToSides;

	private float downAlongSpine;

	public WobblyBackTufts(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		pattern = Pattern.RandomBackBlotch;
		if (UnityEngine.Random.value < 0.25f && (base.scavGrphs.iVars.scruffy == 0f || UnityEngine.Random.value < 0.05f))
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				pattern = Pattern.DoubleSpineRidge;
			}
			else
			{
				pattern = Pattern.SpineRidge;
			}
		}
		if (UnityEngine.Random.value < 0.2f)
		{
			scaleGraf = 0;
		}
		else if (UnityEngine.Random.value < 0.5f)
		{
			if (UnityEngine.Random.value < 1.1764705f)
			{
				scaleGraf = UnityEngine.Random.Range(3, 6);
			}
			else
			{
				scaleGraf = 0;
			}
		}
		else
		{
			xFlip *= 0.5f + UnityEngine.Random.value * 0.5f;
		}
		GeneratePattern(pattern);
		if (pattern == Pattern.RandomBackBlotch)
		{
			outToSides = UnityEngine.Random.value;
		}
		else
		{
			outToSides = 0f;
		}
		downAlongSpine = UnityEngine.Random.value;
		totalSprites = positions.Length * ((!base.Colored) ? 1 : 2);
		scales = new Scale[positions.Length];
		generalSize = Mathf.Lerp(UnityEngine.Random.value, base.scavGrphs.scavenger.abstractCreature.personality.dominance, UnityEngine.Random.value);
		generalSize = Mathf.Lerp(generalSize, UnityEngine.Random.value, UnityEngine.Random.value);
		generalSize = Mathf.Pow(generalSize, Mathf.Lerp(2f, 0.65f, base.scavGrphs.scavenger.abstractCreature.personality.dominance));
		float grav = Mathf.Lerp(0f, 0.9f, UnityEngine.Random.value);
		float airFric = Mathf.Lerp(0.2f, 0.95f, UnityEngine.Random.value);
		float num = Mathf.Lerp(0.1f, 9f, Mathf.Pow(UnityEngine.Random.value, 0.2f));
		float rigidGradRad = Mathf.Lerp(Mathf.Max(4f, num * 1.5f), 37f, Mathf.Pow(UnityEngine.Random.value, 2f));
		float rigidExp = Mathf.Lerp(1f, 6f, Mathf.Pow(UnityEngine.Random.value, 5f));
		Scale.Stats stats = new Scale.Stats(grav, airFric, num, rigidGradRad, rigidExp, 0.5f + UnityEngine.Random.value * 0.5f);
		float num2 = Mathf.Lerp(0.1f, 0.6f, UnityEngine.Random.value);
		float p = Mathf.Lerp(1.2f, 0.3f, UnityEngine.Random.value);
		float a = num2 * UnityEngine.Random.value;
		randomDirs = new Vector2[scales.Length];
		for (int i = 0; i < scales.Length; i++)
		{
			randomDirs[i] = Custom.RNV() * UnityEngine.Random.value * base.scavGrphs.iVars.scruffy;
			float num3 = num2;
			if (pattern == Pattern.SpineRidge || pattern == Pattern.DoubleSpineRidge)
			{
				num3 = Mathf.Lerp(num2, 1f, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(top, bottom, positions[i].y), p) * (float)Math.PI));
			}
			else if (pattern == Pattern.RandomBackBlotch)
			{
				num3 = Mathf.Lerp(num2, 1f, UnityEngine.Random.value);
				num3 = Mathf.Min(num3, Mathf.Lerp(a, 1f, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(top, bottom, positions[i].y), p) * (float)Math.PI)));
				randomDirs[i] = Custom.RNV() * (1f - 2f * Mathf.Abs(0.5f - Mathf.InverseLerp(top, bottom, positions[i].y)));
			}
			else
			{
				num3 = Mathf.Lerp(1f, num2, Mathf.InverseLerp(top, bottom, positions[i].y));
			}
			if (UnityEngine.Random.value < base.scavGrphs.iVars.scruffy)
			{
				num3 = Mathf.Lerp(num3, UnityEngine.Random.value, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(4f, 0.5f, base.scavGrphs.iVars.scruffy)));
			}
			scales[i] = new Scale(i, stats, 40f * num3 * Mathf.Lerp(0.1f, 1f, generalSize));
		}
		if (!base.Colored)
		{
			return;
		}
		colorAlphas = new float[positions.Length];
		if (UnityEngine.Random.value < 0.25f + 0.5f * colored)
		{
			float a2 = float.MaxValue;
			float num4 = float.MinValue;
			for (int j = 0; j < positions.Length; j++)
			{
				a2 = Mathf.Min(a2, positions[j].y);
				num4 = Mathf.Max(num4, positions[j].y);
			}
			float p2 = Mathf.Lerp(0.2f, 1.2f, UnityEngine.Random.value);
			for (int k = 0; k < colorAlphas.Length; k++)
			{
				colorAlphas[k] = Mathf.Lerp(colored, 0f, Mathf.Pow(Mathf.InverseLerp(a2, num4, positions[k].y), p2));
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

	public override void Reset()
	{
		for (int i = 0; i < scales.Length; i++)
		{
			scales[i].pos = base.scavGrphs.OnBackSurfacePos(new float2(positions[i].x, positions[i].y), 1f);
			scales[i].lastPos = scales[i].pos;
			scales[i].vel *= 0f;
		}
	}

	public override void Update()
	{
		base.Update();
		float degAng = Custom.AimFromOneVectorToAnother(base.scavGrphs.drawPositions[base.scavGrphs.hipsDrawPos, 1], base.scavGrphs.drawPositions[base.scavGrphs.chestDrawPos, 1]);
		for (int i = 0; i < scales.Length; i++)
		{
			float2 @float = new float2(positions[i].x, positions[i].y);
			float2 float2 = base.scavGrphs.OnBackSurfacePos(@float, 1f);
			scales[i].vel += Custom.RNV() * (UnityEngine.Random.value * (base.scavGrphs.shake + base.scavGrphs.bristle * 1.5f));
			scales[i].vel.y -= (scales[i].stats.grav * 0.5f + 0.2f * UnityEngine.Random.value) * base.scavGrphs.bristle;
			float2 float3 = float2 + (base.scavGrphs.OnSpineUpDir(positions[i].y, 1f) + base.scavGrphs.OnSpineDir(positions[i].y, 1f) * (0.5f * positions[i].y * downAlongSpine * (1f - base.scavGrphs.bristle)) + base.scavGrphs.OnSpineOutwardsDir(@float, 1f) * (math.pow(math.abs(positions[i].x), 0.4f) * outToSides) + Custom.RotateAroundOrigo(new float2(randomDirs[i].x, randomDirs[i].y), degAng)).normalized() * (scales[i].length * (1f + 0.5f * base.scavGrphs.bristle) + 5f * base.scavGrphs.bristle);
			scales[i].Update(new Vector2(float2.x, float2.y), float3);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < totalSprites / ((!base.Colored) ? 1 : 2); i++)
		{
			float2 @float = base.scavGrphs.OnBackSurfacePos(new float2(positions[i].x, positions[i].y), timeStacker);
			Vector2 vector = new Vector2(@float.x, @float.y);
			Vector2 vector2 = Vector2.Lerp(scales[i].lastPos, scales[i].pos, timeStacker);
			for (int j = 0; j < ((!base.Colored) ? 1 : 2); j++)
			{
				int num = firstSprite + i * ((!base.Colored) ? 1 : 2) + j;
				sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				sLeaser.sprites[num].x = vector.x - camPos.x;
				sLeaser.sprites[num].y = vector.y - camPos.y;
				sLeaser.sprites[num].scaleX = Mathf.Sign(Mathf.Lerp(base.scavGrphs.lastFlip, base.scavGrphs.flip, timeStacker) + positions[i].x * 0.5f) * Mathf.Lerp(0.5f, 1f, generalSize) * xFlip;
				sLeaser.sprites[num].scaleY = Vector2.Distance(vector, vector2) / scaleGrafHeight;
			}
		}
	}
}
