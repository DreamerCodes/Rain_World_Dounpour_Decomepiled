using System;
using RWCustom;
using UnityEngine;

public class VultureFeather : BodyPart
{
	private enum ContractMode
	{
		Even,
		Jerky,
		Jammed
	}

	public VultureGraphics kGraphics;

	public VultureTentacle wing;

	public float wingPosition;

	private float ef;

	public float width;

	public float contractedLength;

	public float extendedLength;

	private ContractMode contractMode;

	public float contractSpeed;

	public float lose;

	public float brokenColor;

	public float forcedAlpha;

	public float lightnessBonus;

	public float saturationBonus;

	private int terrainContactTimer;

	public float extendedFac
	{
		get
		{
			return ef;
		}
		set
		{
			ef = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float CurrentLength => Mathf.Lerp(contractedLength, extendedLength, extendedFac);

	public Tentacle.TentacleChunk PreviousPreviousChunk => wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * (float)wing.tChunks.Length) - 1, 0, wing.tChunks.Length - 1)];

	public Tentacle.TentacleChunk PreviousChunk => wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * (float)wing.tChunks.Length), 0, wing.tChunks.Length - 1)];

	public Tentacle.TentacleChunk NextChunk => wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * (float)wing.tChunks.Length) + 1, 0, wing.tChunks.Length - 1)];

	public float BetweenChunksLerp => wingPosition * (float)wing.tChunks.Length - Mathf.Floor(wingPosition * (float)wing.tChunks.Length);

	public Vector2 ConnectedPos => Vector2.Lerp(PreviousChunk.pos, NextChunk.pos, BetweenChunksLerp);

	public Vector2 ConnectedLastPos => Vector2.Lerp(PreviousChunk.lastPos, NextChunk.lastPos, BetweenChunksLerp);

	public VultureFeather(VultureGraphics kGraphics, VultureTentacle wing, float wingPosition, float contractedLength, float extendedLength, float width)
		: base(kGraphics)
	{
		this.kGraphics = kGraphics;
		this.wing = wing;
		this.wingPosition = wingPosition;
		this.contractedLength = contractedLength;
		this.extendedLength = extendedLength;
		this.width = width;
		lose = 0f;
	}

	public override void Update()
	{
		base.Update();
		if (kGraphics.owner.room.PointSubmerged(pos))
		{
			vel *= 0.1f;
		}
		lastPos = pos;
		pos += vel;
		vel *= 0.7f;
		Vector2 normalized = Vector2.Lerp(PreviousChunk.pos - PreviousPreviousChunk.pos, NextChunk.pos - PreviousChunk.pos, (PreviousPreviousChunk == PreviousChunk) ? 1f : BetweenChunksLerp).normalized;
		Vector2 vector = Custom.PerpendicularVector(normalized) * (kGraphics.vulture.IsMiros ? GetTentacleAngle(wing.tentacleNumber) : ((wing.tentacleNumber == 1) ? (-1f) : 1f));
		float num = Mathf.Lerp(Mathf.Lerp(1f, Mathf.Lerp(-0.9f, 1.5f, Mathf.InverseLerp(wing.idealLength * 0.5f, wing.idealLength, Vector2.Distance(wing.FloatBase, wing.Tip.pos))), wingPosition), Mathf.Lerp(-0.5f, 4f, wingPosition), extendedFac);
		Vector2 vector2 = ConnectedPos + (vector + normalized * num).normalized * CurrentLength;
		vel += (vector2 - pos) * Mathf.Lerp(0.3f, 0.8f, wing.flyingMode) * (1f - lose);
		if (wing.flyingMode > extendedFac)
		{
			extendedFac += 1f / Mathf.Lerp(10f, 40f, UnityEngine.Random.value);
		}
		else if (wing.flyingMode < extendedFac)
		{
			if (extendedFac == 1f)
			{
				contractMode = (kGraphics.vulture.IsMiros ? ContractMode.Jerky : ContractMode.Even);
				contractSpeed = 1f / Mathf.Lerp(20f, 800f, UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
				if (UnityEngine.Random.value < 0.3f)
				{
					if (UnityEngine.Random.value < 0.7f)
					{
						contractMode = ContractMode.Jerky;
					}
					else
					{
						contractMode = ContractMode.Jammed;
					}
				}
			}
			if (contractMode != 0 && extendedFac > 0.5f)
			{
				extendedFac -= 1f / 120f;
			}
			switch (contractMode)
			{
			case ContractMode.Even:
				extendedFac -= contractSpeed;
				break;
			case ContractMode.Jerky:
				if (UnityEngine.Random.value < 0.0016666667f)
				{
					extendedFac -= 1f / Mathf.Lerp(4f, 30f, UnityEngine.Random.value);
				}
				break;
			case ContractMode.Jammed:
				if (UnityEngine.Random.value < 0.0007142857f)
				{
					contractMode = ContractMode.Jerky;
				}
				break;
			}
		}
		lightnessBonus = Mathf.Max(lightnessBonus - 0.1f, 0f);
		if (lightnessBonus == 0f)
		{
			saturationBonus = Mathf.Max(saturationBonus - 0.02f, 0f);
		}
		forcedAlpha = Mathf.Lerp(forcedAlpha, 0f, 0.05f);
		ConnectToPoint(ConnectedPos, CurrentLength, push: true, 0f, PreviousChunk.vel, 0.3f, 0f);
		if (terrainContact)
		{
			terrainContactTimer++;
		}
		else
		{
			terrainContactTimer = 0;
		}
		Vector2 vector3 = vel;
		PushOutOfTerrain(kGraphics.vulture.room, ConnectedPos);
		if (terrainContact && terrainContactTimer > 4)
		{
			if (kGraphics.vulture.IsMiros)
			{
				kGraphics.vulture.room.PlaySound((UnityEngine.Random.value < 0.5f) ? SoundID.Spear_Fragment_Bounce : SoundID.Spear_Bounce_Off_Wall, pos, Mathf.InverseLerp(10f, 60f, vector3.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, CurrentLength)));
			}
			else
			{
				kGraphics.vulture.room.PlaySound(SoundID.Vulture_Feather_Hit_Terrain, pos, Mathf.InverseLerp(0.2f, 20f, vector3.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, CurrentLength)));
			}
			terrainContactTimer = 0;
		}
	}

	public Color CurrentColor()
	{
		if (kGraphics.vulture.IsMiros)
		{
			Color rgb = HSLColor.Lerp(new HSLColor(kGraphics.ColorB.hue, Mathf.Lerp(kGraphics.ColorB.saturation, 1f, saturationBonus), Mathf.Lerp(kGraphics.ColorB.lightness, 1f, lightnessBonus)), kGraphics.ColorA, Mathf.Cos(Mathf.Pow(wingPosition, 0.75f) * (float)Math.PI)).rgb;
			rgb.a = Mathf.Max(0.4f, forcedAlpha, Mathf.Lerp(0.4f, 0.8f, Mathf.Cos(Mathf.Pow(wingPosition, 1.7f) * (float)Math.PI))) * (extendedFac + wing.flyingMode) * 0.5f * (1f - brokenColor);
			if (kGraphics.vulture.isLaserActive())
			{
				rgb.a = UnityEngine.Random.value;
			}
			return rgb;
		}
		HSLColor colorB = kGraphics.ColorB;
		HSLColor colorA = kGraphics.ColorA;
		if (kGraphics.albino)
		{
			colorB.saturation = Mathf.Lerp(colorB.saturation, 1f, 0.2f);
			colorB.hue = 0f;
			colorB.lightness = Mathf.Lerp(colorB.saturation, 0.2f, 0.8f);
			colorA.saturation = 0.8f;
			colorA.lightness = 0.6f;
		}
		Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, saturationBonus), Mathf.Lerp(colorB.lightness, 1f, lightnessBonus)), colorA, Mathf.Cos(Mathf.Pow(wingPosition, 0.75f) * (float)Math.PI)).rgb;
		rgb2.a = Mathf.Max(forcedAlpha, Mathf.Lerp(0.2f, 0.6f, Mathf.Cos(Mathf.Pow(wingPosition, 1.7f) * (float)Math.PI))) * (extendedFac + wing.flyingMode) * 0.5f * (1f - brokenColor);
		return rgb2;
	}

	public float GetTentacleAngle(int id)
	{
		return id switch
		{
			0 => 1f, 
			1 => -1f, 
			2 => 4f, 
			_ => -4f, 
		};
	}
}
