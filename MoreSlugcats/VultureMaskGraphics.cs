using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class VultureMaskGraphics
{
	public class CosmeticPearlString
	{
		public Vector2 pos;

		public List<Vector2> pearlPositions;

		public List<Vector2> pearlVelocities;

		public List<float> connectionRads;

		public List<float> pearlGlimmers;

		public List<float> glimmerProg;

		public List<float> glimmerWait;

		public List<float> glimmerSpeed;

		public int startSprite;

		public float darkness;

		public float submersion;

		public Color color;

		public float gravity;

		public int layer;

		public Vector2 lastPos;

		public List<Color> pearlColors;

		public int TotalSprites => pearlPositions.Count * 4;

		public CosmeticPearlString(Vector2 pos, float length, int startSprite)
		{
			this.pos = pos;
			lastPos = pos;
			this.startSprite = startSprite;
			Vector2 vector = new Vector2(this.pos.x, this.pos.y);
			pearlPositions = new List<Vector2>();
			pearlVelocities = new List<Vector2>();
			connectionRads = new List<float>();
			pearlGlimmers = new List<float>();
			glimmerProg = new List<float>();
			glimmerWait = new List<float>();
			glimmerSpeed = new List<float>();
			pearlColors = new List<Color>();
			float num = 0f;
			int i = 0;
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(startSprite + 3);
			for (; (float)i < length; i++)
			{
				float num2 = UnityEngine.Random.Range(5f, 15f);
				if (UnityEngine.Random.value < 0.33f)
				{
					num2 = 5f;
				}
				num += num2;
				vector.y -= num2;
				pearlPositions.Add(new Vector2(vector.x, vector.y));
				pearlVelocities.Add(Vector2.zero);
				float value = UnityEngine.Random.value;
				glimmerProg.Add(value);
				pearlGlimmers.Add(Mathf.Sin(value * (float)Math.PI) * UnityEngine.Random.value);
				glimmerWait.Add(UnityEngine.Random.Range(20, 40));
				glimmerSpeed.Add(1f / Mathf.Lerp(5f, 15f, UnityEngine.Random.value));
				float value2 = UnityEngine.Random.value;
				if (value2 < 0.33f || num >= length * 0.67f)
				{
					pearlColors.Add(new Color(1f, 0.6f, 0.9f));
				}
				else if (value2 < 0.67f)
				{
					pearlColors.Add(new Color(0.9f, 0.9f, 0.9f));
				}
				else
				{
					pearlColors.Add(new Color(0.9f, 0.9f, 0.6f));
				}
				connectionRads.Add(num2);
				if (num > length)
				{
					break;
				}
			}
			UnityEngine.Random.state = state;
		}

		public void Update()
		{
			for (int i = 1; i < pearlPositions.Count; i++)
			{
				float num = Vector2.Distance(pearlPositions[i], pearlPositions[i - 1]);
				if (num > connectionRads[i])
				{
					Vector2 normalized = (pearlPositions[i] - pearlPositions[i - 1]).normalized;
					pearlPositions[i] += normalized * (connectionRads[i] - num) * 0.98f;
					pearlPositions[i - 1] -= normalized * (connectionRads[i] - num) * 0.98f;
					pearlVelocities[i] += normalized * (connectionRads[i] - num) * 0.98f;
					pearlVelocities[i - 1] -= normalized * (connectionRads[i] - num) * 0.98f;
				}
			}
			Attach();
			for (int num2 = pearlPositions.Count - 2; num2 >= 0; num2--)
			{
				float num3 = Vector2.Distance(pearlPositions[num2], pearlPositions[num2 + 1]);
				if (num3 > connectionRads[num2])
				{
					Vector2 normalized2 = (pearlPositions[num2] - pearlPositions[num2 + 1]).normalized;
					pearlPositions[num2] += normalized2 * (connectionRads[num2] - num3) * 0.98f;
					pearlPositions[num2 + 1] -= normalized2 * (connectionRads[num2] - num3) * 0.98f;
					pearlVelocities[num2] += normalized2 * (connectionRads[num2] - num3) * 0.98f;
					pearlVelocities[num2 + 1] -= normalized2 * (connectionRads[num2] - num3) * 0.98f;
				}
			}
			Attach();
			for (int j = 0; j < pearlVelocities.Count; j++)
			{
				pearlVelocities[j] = new Vector2(pearlVelocities[j].x, pearlVelocities[j].y - gravity);
				pearlPositions[j] += pearlVelocities[j];
			}
			for (int k = 0; k < pearlGlimmers.Count; k++)
			{
				pearlGlimmers[k] = Mathf.Sin(glimmerProg[k] * (float)Math.PI) * UnityEngine.Random.value;
				if (glimmerProg[k] < 1f)
				{
					glimmerProg[k] = Mathf.Min(1f, glimmerProg[k] + glimmerSpeed[k]);
					continue;
				}
				if (glimmerWait[k] > 0f)
				{
					glimmerWait[k] -= 1f;
					continue;
				}
				glimmerWait[k] = UnityEngine.Random.Range(20, 40);
				glimmerProg[k] = 0f;
				glimmerSpeed[k] = 1f / Mathf.Lerp(5f, 15f, UnityEngine.Random.value);
			}
		}

		private void Attach()
		{
			Vector2 normalized = (pearlPositions[0] - pos).normalized;
			float num = Vector2.Distance(pearlPositions[0], pos);
			pearlPositions[0] += normalized * (connectionRads[0] - num);
			pearlVelocities[0] += normalized * (connectionRads[0] - num);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			for (int i = startSprite; i < startSprite + TotalSprites; i++)
			{
				if ((i - startSprite) % 4 == 0)
				{
					sLeaser.sprites[i] = new FSprite("pixel");
					sLeaser.sprites[i].anchorY = 0f;
				}
				else if ((i - startSprite) % 4 == 1)
				{
					sLeaser.sprites[i] = new FSprite("JetFishEyeA");
				}
				else if ((i - startSprite) % 4 == 2)
				{
					sLeaser.sprites[i] = new FSprite("tinyStar");
				}
				else if ((i - startSprite) % 4 == 3)
				{
					sLeaser.sprites[i] = new FSprite("Futile_White");
					sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
				}
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = pos;
			int num = 0;
			for (int i = startSprite; i < startSprite + TotalSprites; i++)
			{
				Vector2 vector2 = pearlPositions[num];
				float num2 = pearlGlimmers[num];
				if ((i - startSprite) % 4 == 0)
				{
					sLeaser.sprites[i].x = vector2.x - camPos.x;
					sLeaser.sprites[i].y = vector2.y - camPos.y;
					sLeaser.sprites[i].scaleY = Vector2.Distance(vector2, vector);
					sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
					sLeaser.sprites[i].isVisible = layer != 0;
					vector = vector2;
				}
				else if ((i - startSprite) % 4 == 1)
				{
					sLeaser.sprites[i].x = vector.x - camPos.x;
					sLeaser.sprites[i].y = vector.y - camPos.y;
					sLeaser.sprites[i].color = Color.Lerp(Custom.RGB2RGBA(pearlColors[num] * Mathf.Lerp(1f, 0.2f, darkness), 1f), new Color(1f, 1f, 1f), num2);
					if (num2 > 0.9f && submersion == 1f)
					{
						sLeaser.sprites[i].color = new Color(0f, 0.003921569f, 0f);
					}
					sLeaser.sprites[i].scale = 0.5f;
					sLeaser.sprites[i].isVisible = layer != 0;
				}
				else if ((i - startSprite) % 4 == 2)
				{
					sLeaser.sprites[i].x = vector.x - camPos.x - 0.25f;
					sLeaser.sprites[i].y = vector.y - camPos.y + 0.75f;
					sLeaser.sprites[i].color = Color.Lerp(Custom.RGB2RGBA(pearlColors[num] * Mathf.Lerp(1.3f, 0.5f, darkness), 1f), new Color(1f, 1f, 1f), Mathf.Lerp(0.5f + 0.5f * num2, 0.2f + 0.8f * num2, darkness));
					if (num2 > 0.9f && submersion == 1f)
					{
						sLeaser.sprites[i].color = new Color(0f, 0.003921569f, 0f);
					}
					sLeaser.sprites[i].scale = 0.5f;
					sLeaser.sprites[i].isVisible = layer != 0;
				}
				else if ((i - startSprite) % 4 == 3)
				{
					sLeaser.sprites[i].x = vector.x - camPos.x;
					sLeaser.sprites[i].y = vector.y - camPos.y;
					sLeaser.sprites[i].alpha = num2 * 0.5f;
					sLeaser.sprites[i].scale = 20f * num2 * 1f / 32f;
					sLeaser.sprites[i].isVisible = layer != 0;
					num++;
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = startSprite; i < startSprite + TotalSprites; i++)
			{
				if ((i - startSprite) % 4 == 0)
				{
					sLeaser.sprites[i].color = palette.blackColor;
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = startSprite; i < startSprite + TotalSprites; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				if ((i - startSprite) % 4 == 3)
				{
					rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
				}
				else
				{
					newContatiner.AddChild(sLeaser.sprites[i]);
				}
			}
		}
	}

	public Vector2? overrideDrawVector;

	public PhysicalObject attachedTo;

	public VultureMask.MaskType maskType;

	public Vector2 rotationA;

	public Vector2 lastRotationA;

	public Vector2 rotationB;

	public Vector2 lastRotationB;

	public float fallOffVultureMode;

	public int firstSprite;

	public Color color;

	private HSLColor ColorA;

	private HSLColor ColorB;

	private Color blackColor;

	public Vector2? overrideRotationVector;

	public Vector2? overrideAnchorVector;

	public string overrideSprite;

	public List<CosmeticPearlString> pearlStrings;

	public bool King => maskType == VultureMask.MaskType.KING;

	public bool ScavKing
	{
		get
		{
			if (ModManager.MSC)
			{
				return maskType == VultureMask.MaskType.SCAVKING;
			}
			return false;
		}
	}

	public int BaseTotalSprites
	{
		get
		{
			if (!King)
			{
				return 3;
			}
			return 4;
		}
	}

	public int TotalSprites
	{
		get
		{
			int num = BaseTotalSprites;
			for (int i = 0; i < pearlStrings.Count; i++)
			{
				num += pearlStrings[i].TotalSprites;
			}
			return num;
		}
	}

	public int SpriteIndex
	{
		get
		{
			Vector2 value = rotationB;
			if (overrideAnchorVector.HasValue)
			{
				value = overrideAnchorVector.Value;
			}
			return Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(Custom.VecToDeg(value) / 180f) * 8f), 0, 8);
		}
	}

	public VultureMaskGraphics(PhysicalObject attached, VultureMask.AbstractVultureMask abstractMask, int firstSprite)
	{
		attachedTo = attached;
		if (abstractMask.king)
		{
			maskType = VultureMask.MaskType.KING;
		}
		else if (ModManager.MSC && abstractMask.scavKing)
		{
			maskType = VultureMask.MaskType.SCAVKING;
		}
		else
		{
			maskType = VultureMask.MaskType.NORMAL;
		}
		this.firstSprite = firstSprite;
		overrideSprite = abstractMask.spriteOverride;
		pearlStrings = new List<CosmeticPearlString>();
		if (!ScavKing)
		{
			return;
		}
		overrideSprite = "";
		int num = this.firstSprite + BaseTotalSprites;
		for (int i = 0; i < 4; i++)
		{
			float length = 17f;
			if (i >= 2)
			{
				length = 25f;
			}
			CosmeticPearlString cosmeticPearlString = new CosmeticPearlString(PearlAttachPos(i), length, num);
			pearlStrings.Add(cosmeticPearlString);
			num += cosmeticPearlString.TotalSprites;
		}
	}

	public VultureMaskGraphics(PhysicalObject attached, VultureMask.MaskType type, int firstSprite, string overrideSprite)
	{
		attachedTo = attached;
		maskType = type;
		this.firstSprite = firstSprite;
		this.overrideSprite = overrideSprite;
		pearlStrings = new List<CosmeticPearlString>();
		if (!ScavKing)
		{
			return;
		}
		this.overrideSprite = "";
		int num = firstSprite + BaseTotalSprites;
		for (int i = 0; i < 4; i++)
		{
			float length = 17f;
			if (i >= 2)
			{
				length = 25f;
			}
			CosmeticPearlString cosmeticPearlString = new CosmeticPearlString(PearlAttachPos(i), length, num);
			pearlStrings.Add(cosmeticPearlString);
			num += cosmeticPearlString.TotalSprites;
		}
	}

	public void GenerateColor(int colorSeed)
	{
		if (ModManager.MSC && maskType == VultureMask.MaskType.SCAVKING)
		{
			ColorA = new HSLColor(1f, 1f, 1f);
			ColorB = new HSLColor(1f, 1f, 1f);
			return;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(colorSeed);
		if (King)
		{
			ColorB = new HSLColor(Mathf.Lerp(0.93f, 1.07f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
			ColorA = new HSLColor(ColorB.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
		}
		else
		{
			ColorA = new HSLColor(Mathf.Lerp(0.9f, 1.6f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
			ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
		}
		UnityEngine.Random.state = state;
	}

	public void Update()
	{
		lastRotationA = rotationA;
		lastRotationB = rotationB;
		for (int i = 0; i < pearlStrings.Count; i++)
		{
			pearlStrings[i].submersion = attachedTo.firstChunk.submersion;
			pearlStrings[i].lastPos = pearlStrings[i].pos;
			pearlStrings[i].pos = PearlAttachPos(i);
			for (int j = 0; j < pearlStrings[i].pearlPositions.Count; j++)
			{
				Vector2 vector = pearlStrings[i].pos - pearlStrings[i].lastPos;
				pearlStrings[i].pearlPositions[j] += vector * Custom.LerpQuadEaseIn(0.5f, 1f, Mathf.Lerp(0f, 1f, vector.magnitude / 30f));
			}
			pearlStrings[i].gravity = attachedTo.gravity;
			pearlStrings[i].Update();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[firstSprite] = new FSprite("pixel");
		sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
		sLeaser.sprites[firstSprite + 2] = new FSprite("pixel");
		if (King)
		{
			sLeaser.sprites[firstSprite + 3] = new FSprite("pixel");
			for (int i = 0; i < TotalSprites; i++)
			{
				sLeaser.sprites[firstSprite + i].scale = 1.15f;
			}
		}
		for (int j = 0; j < pearlStrings.Count; j++)
		{
			pearlStrings[j].InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.zero;
		Vector2 v = Vector3.Slerp(lastRotationA, rotationA, timeStacker);
		Vector2 v2 = Vector3.Slerp(lastRotationB, rotationB, timeStacker);
		if (overrideRotationVector.HasValue)
		{
			v = overrideRotationVector.Value;
		}
		if (overrideAnchorVector.HasValue)
		{
			v2 = overrideAnchorVector.Value;
		}
		if (overrideDrawVector.HasValue)
		{
			pos = overrideDrawVector.Value;
		}
		else if (attachedTo != null)
		{
			pos = Vector2.Lerp(attachedTo.firstChunk.lastPos, attachedTo.firstChunk.pos, timeStacker);
		}
		float num = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)) * 0.8f * (1f - fallOffVultureMode);
		float num2 = Custom.VecToDeg(v2);
		int num3 = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num2 / 180f) * 8f), 0, 8);
		float num4 = (King ? 1.15f : 1f);
		for (int i = 0; i < (King ? 4 : 3); i++)
		{
			if (overrideSprite != null && overrideSprite != "")
			{
				sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName(overrideSprite + num3);
			}
			else if (ScavKing)
			{
				sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName("KingMask" + num3);
			}
			else
			{
				sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName(((i != 3) ? "KrakenMask" : "KrakenArrow") + num3);
			}
			sLeaser.sprites[firstSprite + i].scaleX = Mathf.Sign(num2) * num4;
			sLeaser.sprites[firstSprite + i].anchorY = Custom.LerpMap(Mathf.Abs(num2), 0f, 100f, 0.5f, 0.675f, 2.1f);
			sLeaser.sprites[firstSprite + i].anchorX = 0.5f - v2.x * 0.1f * Mathf.Sign(num2);
			sLeaser.sprites[firstSprite + i].rotation = Custom.VecToDeg(v);
			sLeaser.sprites[firstSprite + i].x = pos.x - camPos.x;
			sLeaser.sprites[firstSprite + i].y = pos.y - camPos.y;
		}
		sLeaser.sprites[firstSprite + 1].scaleX *= 0.85f * num4;
		sLeaser.sprites[firstSprite + 1].scaleY = 0.9f * num4;
		sLeaser.sprites[firstSprite + 2].scaleY = 1.1f * num4;
		sLeaser.sprites[firstSprite + 2].anchorY += 0.015f;
		if (attachedTo is PlayerCarryableItem && (attachedTo as PlayerCarryableItem).blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			for (int j = 0; j < ((!King) ? 3 : 4); j++)
			{
				sLeaser.sprites[firstSprite + j].color = new Color(1f, 1f, 1f);
			}
			return;
		}
		color = Color.Lerp(Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f * fallOffVultureMode), blackColor, Mathf.Lerp(0.2f, 1f, Mathf.Pow(num, 2f)));
		sLeaser.sprites[firstSprite].color = color;
		sLeaser.sprites[firstSprite + 1].color = Color.Lerp(color, blackColor, Mathf.Lerp(0.75f, 1f, num));
		sLeaser.sprites[firstSprite + 2].color = Color.Lerp(color, blackColor, Mathf.Lerp(0.75f, 1f, num));
		if (King)
		{
			sLeaser.sprites[firstSprite + 3].color = Color.Lerp(Color.Lerp(Color.Lerp(HSLColor.Lerp(ColorA, ColorB, 0.8f - 0.3f * fallOffVultureMode).rgb, blackColor, 0.53f), Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f), 0.1f), blackColor, 0.6f * num);
		}
		for (int k = 0; k < pearlStrings.Count; k++)
		{
			int num5 = k;
			if (Mathf.Sign(v2.x) < 0f)
			{
				switch (num5)
				{
				case 0:
					num5 = 1;
					break;
				case 1:
					num5 = 0;
					break;
				case 2:
					num5 = 3;
					break;
				case 3:
					num5 = 2;
					break;
				}
			}
			pearlStrings[k].layer = stringLayers(SpriteIndex)[num5];
			pearlStrings[k].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		for (int i = 0; i < pearlStrings.Count; i++)
		{
			pearlStrings[i].ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < TotalSprites; i++)
		{
			sLeaser.sprites[firstSprite + i].RemoveFromContainer();
		}
		newContatiner.AddChild(sLeaser.sprites[firstSprite + 2]);
		newContatiner.AddChild(sLeaser.sprites[firstSprite + 1]);
		newContatiner.AddChild(sLeaser.sprites[firstSprite]);
		if (King)
		{
			newContatiner.AddChild(sLeaser.sprites[firstSprite + 3]);
		}
		for (int j = 0; j < pearlStrings.Count; j++)
		{
			pearlStrings[j].AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public void SetVisible(RoomCamera.SpriteLeaser sLeaser, bool visible)
	{
		for (int i = 0; i < TotalSprites; i++)
		{
			sLeaser.sprites[firstSprite + i].isVisible = visible;
		}
	}

	public Vector2 PearlAttachPos(int i)
	{
		Vector2 vector = Vector2.zero;
		if (overrideDrawVector.HasValue)
		{
			vector = overrideDrawVector.Value;
		}
		else if (attachedTo != null)
		{
			vector = attachedTo.firstChunk.pos;
		}
		Vector2 value = rotationB;
		if (overrideAnchorVector.HasValue)
		{
			value = overrideAnchorVector.Value;
		}
		Vector2 value2 = rotationA;
		if (overrideRotationVector.HasValue)
		{
			value2 = overrideRotationVector.Value;
		}
		int num = i;
		if (Mathf.Sign(value.x) < 0f)
		{
			switch (num)
			{
			case 0:
				num = 1;
				break;
			case 1:
				num = 0;
				break;
			case 2:
				num = 3;
				break;
			case 3:
				num = 2;
				break;
			}
		}
		Vector2 vector2 = stringOffsets(SpriteIndex)[num];
		float f = (float)Math.PI / 180f * (0f - Custom.VecToDeg(value2)) * Mathf.Sign(value.x);
		Vector2 vector3 = new Vector2(vector2.x * Mathf.Cos(f) - vector2.y * Mathf.Sin(f), vector2.x * Mathf.Sin(f) + vector2.y * Mathf.Cos(f));
		return vector + new Vector2(vector3.x * Mathf.Sign(value.x), vector3.y);
	}

	public Vector2[] stringOffsets(int ind)
	{
		return ind switch
		{
			0 => new Vector2[4]
			{
				new Vector2(-27f, 13f),
				new Vector2(22f, 13f),
				new Vector2(-15f, 25f),
				new Vector2(12f, 25f)
			}, 
			1 => new Vector2[4]
			{
				new Vector2(-20f, 9f),
				new Vector2(19f, 12f),
				new Vector2(-13f, 25f),
				new Vector2(9f, 23f)
			}, 
			2 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(24f, 6f),
				new Vector2(-5f, 20f),
				new Vector2(14f, 20f)
			}, 
			3 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(20f, 0f),
				new Vector2(-6f, 20f),
				new Vector2(15f, 19f)
			}, 
			4 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(21f, -2f),
				new Vector2(11f, 12f),
				new Vector2(16f, 10f)
			}, 
			5 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(19f, -19f),
				new Vector2(22f, -3f),
				new Vector2(22f, -5f)
			}, 
			6 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(17f, -20f),
				new Vector2(21f, -7f),
				new Vector2(0f, 0f)
			}, 
			7 => new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(26f, 4f),
				new Vector2(10f, 15f),
				new Vector2(19f, 15f)
			}, 
			_ => new Vector2[4]
			{
				new Vector2(-21f, 2f),
				new Vector2(19f, 2f),
				new Vector2(-14f, 8f),
				new Vector2(12f, 8f)
			}, 
		};
	}

	public int[] stringLayers(int ind)
	{
		return ind switch
		{
			0 => new int[4] { 1, 1, 1, 1 }, 
			1 => new int[4] { 1, 1, 1, 1 }, 
			2 => new int[4] { 0, 1, -1, 1 }, 
			3 => new int[4] { 0, 1, -1, -1 }, 
			4 => new int[4] { 0, 1, -1, -1 }, 
			5 => new int[4] { 0, 1, -1, -1 }, 
			6 => new int[4] { 0, 1, -1, 0 }, 
			7 => new int[4] { 0, 1, -1, -1 }, 
			_ => new int[4] { 1, 1, -1, -1 }, 
		};
	}
}
