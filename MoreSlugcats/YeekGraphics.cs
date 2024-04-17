using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class YeekGraphics : GraphicsModule, IDrawable, ILookingAtCreatures
{
	public class YeekFeather
	{
		public Vector2 rootPos;

		public Vector2 lastRootPos;

		public Vector2 featherDirection;

		public Vector2 lastFeatherDirection;

		public Color rootColor;

		public Color tipColor;

		public int startSpriteIndex;

		public YeekGraphics owner;

		public float featherScaler;

		public int MaxFeathersInGroup;

		public int featherIndex;

		public static int spriteCount = 2;

		public YeekFeather(Vector2 RootPos, YeekGraphics owner, int featherNumber, int groupCount)
		{
			this.owner = owner;
			rootPos = RootPos;
			lastRootPos = rootPos;
			MaxFeathersInGroup = groupCount;
			featherIndex = featherNumber;
		}

		public void Update(Vector2 newRootPos, Vector2? leanFeather)
		{
			lastFeatherDirection = featherDirection;
			lastRootPos = rootPos;
			rootPos = newRootPos;
			Vector2 vector = Custom.DirVec(owner.myYeek.bodyChunks[1].pos, owner.myYeek.bodyChunks[0].pos);
			Vector2 a = owner.headDrawDirection * -1f + vector * -0.4f;
			a.Normalize();
			lastFeatherDirection = featherDirection;
			Vector2 a2 = Vector2.Lerp(a, vector, (float)(featherIndex / MaxFeathersInGroup) * 0.3f);
			featherDirection = Vector2.Lerp(a2, featherDirection, 0.9f);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int startSprite)
		{
			startSpriteIndex = startSprite;
			sLeaser.sprites[startSpriteIndex] = new FSprite("LizardScaleA" + owner.plumageGraphic);
			sLeaser.sprites[startSpriteIndex].anchorY = 0f;
			sLeaser.sprites[startSpriteIndex + 1] = new FSprite("LizardScaleB" + owner.plumageGraphic);
			sLeaser.sprites[startSpriteIndex + 1].anchorY = 0f;
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			rootColor = owner.furColor;
			tipColor = owner.featherColor;
			sLeaser.sprites[startSpriteIndex].color = Color.Lerp(rootColor, owner.blackColor, 0.3f + owner.darkness * 0.7f);
			sLeaser.sprites[startSpriteIndex + 1].color = Color.Lerp(tipColor, owner.blackColor, 0.3f + owner.darkness * 0.7f);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 2f;
			if (owner.plumageGraphic == 4 || owner.plumageGraphic == 5)
			{
				num = 1.5f;
			}
			float perc = (float)featherIndex / (float)MaxFeathersInGroup;
			num *= Math.Max(Custom.LerpQuadEaseOut(1f, 0.3f, perc), 0.3f);
			sLeaser.sprites[startSpriteIndex].x = Mathf.Lerp(lastRootPos.x, rootPos.x, timeStacker) - camPos.x;
			sLeaser.sprites[startSpriteIndex].y = Mathf.Lerp(lastRootPos.y, rootPos.y, timeStacker) - camPos.y;
			sLeaser.sprites[startSpriteIndex].rotation = Custom.VecToDeg(Vector2.Lerp(lastFeatherDirection, featherDirection, timeStacker));
			sLeaser.sprites[startSpriteIndex].scaleX = 1f;
			sLeaser.sprites[startSpriteIndex].scaleY = num;
			sLeaser.sprites[startSpriteIndex + 1].x = sLeaser.sprites[startSpriteIndex].x;
			sLeaser.sprites[startSpriteIndex + 1].y = sLeaser.sprites[startSpriteIndex].y;
			sLeaser.sprites[startSpriteIndex + 1].rotation = sLeaser.sprites[startSpriteIndex].rotation;
			sLeaser.sprites[startSpriteIndex + 1].scaleX = sLeaser.sprites[startSpriteIndex].scaleX;
			sLeaser.sprites[startSpriteIndex + 1].scaleY = sLeaser.sprites[startSpriteIndex].scaleY;
		}
	}

	public class YeekLeg
	{
		public class AnimState : ExtEnum<AnimState>
		{
			public static readonly AnimState Sit = new AnimState("Sit", register: true);

			public static readonly AnimState Climb = new AnimState("Climb", register: true);

			public static readonly AnimState Tunnel = new AnimState("Tunnel", register: true);

			public static readonly AnimState Jump = new AnimState("Jump", register: true);

			public static readonly AnimState Dangle = new AnimState("Dangle", register: true);

			public AnimState(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public float maxLength;

		public Vector2 footPos;

		public Vector2 lastFootPos;

		public Vector2 rootPos;

		public Vector2 lastRootPos;

		public Vector2 goalPos;

		public YeekGraphics owner;

		public static int spriteCount = 1;

		public bool debug;

		public bool debugOrientation;

		public AnimState animationState;

		public bool isRightLeg;

		public AnimState currentState;

		public float jumpTimer;

		public float maxLegDist
		{
			get
			{
				float result = maxLength;
				if (owner.myYeek.GetTunnelMode)
				{
					result = 8f;
				}
				if (owner.myYeek.GetClimbingMode)
				{
					result = 11f;
				}
				return result;
			}
		}

		public YeekLeg(YeekGraphics owner, bool isRightLeg)
		{
			maxLength = 16f;
			this.owner = owner;
			Reset();
			lastRootPos = rootPos;
			debug = false;
			debugOrientation = false;
			this.isRightLeg = isRightLeg;
		}

		public bool AtGoal()
		{
			return Vector2.Distance(footPos, goalPos) < 1f;
		}

		public YeekLeg GetOtherLeg()
		{
			if (!isRightLeg)
			{
				return owner.legs[0];
			}
			return owner.legs[1];
		}

		public void Update(AnimState animState)
		{
			currentState = animState;
			if (animState != AnimState.Jump)
			{
				jumpTimer = 0f;
			}
			lastRootPos = rootPos;
			lastFootPos = footPos;
			Vector2 vector = (rootPos = ((!isRightLeg) ? (owner.myYeek.bodyChunks[0].pos + new Vector2(Mathf.Lerp(1.4f, 0f, owner.RightSideShown(1f)) + Mathf.Lerp(0f, -2f, owner.LeftSideShown(1f)), 2f)) : (owner.myYeek.bodyChunks[0].pos + new Vector2(Mathf.Lerp(-1.4f, 0f, owner.LeftSideShown(1f)) + Mathf.Lerp(0f, 2f, owner.RightSideShown(1f)), 2f))));
			if (currentState == AnimState.Sit)
			{
				float num = maxLegDist / 2f;
				if (isRightLeg)
				{
					vector = owner.myYeek.bodyChunks[0].pos + new Vector2(2f, -1f);
					goalPos = vector + new Vector2(-1f, -1f) * num;
				}
				else
				{
					vector = owner.myYeek.bodyChunks[0].pos + new Vector2(-2f, -1f);
					goalPos = vector + new Vector2(1f, -1f) * num;
				}
			}
			else if (currentState == AnimState.Climb)
			{
				float num2 = -2f;
				if (GetOtherLeg().AtGoal() && Vector2.Distance(goalPos, owner.myYeek.firstChunk.pos) > Vector2.Distance(GetOtherLeg().goalPos, owner.myYeek.firstChunk.pos))
				{
					Vector2 vector2 = Custom.PerpendicularVector(owner.myYeek.climbingOrientation) * (0f - num2);
					if (isRightLeg)
					{
						vector2 = Custom.PerpendicularVector(owner.myYeek.climbingOrientation) * num2;
					}
					goalPos = owner.myYeek.room.MiddleOfTile(owner.myYeek.firstChunk.pos) + owner.myYeek.climbingOrientation.normalized * 2f + vector2;
				}
			}
			else if (currentState == AnimState.Tunnel)
			{
				float num3 = 4f;
				if (GetOtherLeg().AtGoal() && Vector2.Distance(goalPos, owner.myYeek.firstChunk.pos) > Vector2.Distance(GetOtherLeg().goalPos, owner.myYeek.firstChunk.pos))
				{
					Vector2 vector3 = Custom.PerpendicularVector(owner.myYeek.climbingOrientation) * (0f - num3);
					if (isRightLeg)
					{
						vector3 = Custom.PerpendicularVector(owner.myYeek.climbingOrientation) * num3;
					}
					goalPos = owner.myYeek.room.MiddleOfTile(owner.myYeek.firstChunk.pos) + owner.myYeek.climbingOrientation.normalized * 2f + vector3;
				}
			}
			else if (currentState == AnimState.Jump)
			{
				jumpTimer += 1f;
				float t = Mathf.Clamp01(jumpTimer / 8f);
				goalPos = vector + new Vector2(owner.myYeek.bodyChunks[1].vel.x / 2f * Mathf.Lerp(2f, 0f, t), Mathf.Lerp(-5f, -3f, t)) * maxLegDist;
			}
			if (Vector2.Distance(rootPos, goalPos) > maxLegDist)
			{
				goalPos = rootPos + Custom.DirVec(rootPos, goalPos) * maxLegDist;
			}
			footPos = Custom.MoveTowards(footPos, goalPos, 8f);
			if (Vector2.Distance(rootPos, footPos) > maxLegDist)
			{
				footPos = rootPos + Custom.DirVec(rootPos, footPos) * maxLegDist;
			}
		}

		public void DrawLeg(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int startIndex)
		{
			if (Mathf.Abs(owner.myYeek.mainBodyChunk.vel.x) > 1f)
			{
				Mathf.Sign(owner.myYeek.mainBodyChunk.vel.x);
			}
			float num = Vector2.Distance(rootPos, footPos);
			if (debugOrientation)
			{
				sLeaser.sprites[startIndex].color = Color.red;
				if (!isRightLeg)
				{
					sLeaser.sprites[startIndex].color = Color.blue;
				}
			}
			if (!debug)
			{
				if (num <= maxLength * 0.3f || currentState == AnimState.Tunnel)
				{
					sLeaser.sprites[startIndex].element = Futile.atlasManager.GetElementWithName("LizardArm_10");
				}
				else if (num <= maxLength * 0.55f)
				{
					sLeaser.sprites[startIndex].element = Futile.atlasManager.GetElementWithName("LizardArm_20");
				}
				else if (num <= maxLength * 0.8f)
				{
					sLeaser.sprites[startIndex].element = Futile.atlasManager.GetElementWithName("LizardArm_30");
				}
				else
				{
					sLeaser.sprites[startIndex].element = Futile.atlasManager.GetElementWithName("LizardArm_40");
				}
				if (!isRightLeg)
				{
					sLeaser.sprites[startIndex].rotation = Custom.VecToDeg(Custom.DirVec(Vector2.Lerp(lastFootPos, footPos, timeStacker), Vector2.Lerp(lastRootPos, rootPos, timeStacker))) + 90f;
					sLeaser.sprites[startIndex].scaleX = 1f;
					if (owner.LeftSideShown(timeStacker) > 0f && currentState != AnimState.Climb && currentState != AnimState.Sit)
					{
						sLeaser.sprites[startIndex].scaleY = -1f;
					}
					else
					{
						sLeaser.sprites[startIndex].scaleY = 1f;
					}
				}
				else
				{
					sLeaser.sprites[startIndex].rotation = Custom.VecToDeg(Custom.DirVec(Vector2.Lerp(lastFootPos, footPos, timeStacker), Vector2.Lerp(lastRootPos, rootPos, timeStacker))) + 90f;
					sLeaser.sprites[startIndex].scaleX = 1f;
					if (owner.RightSideShown(timeStacker) > 0f && currentState != AnimState.Climb && currentState != AnimState.Sit)
					{
						sLeaser.sprites[startIndex].scaleY = 1f;
					}
					else
					{
						sLeaser.sprites[startIndex].scaleY = -1f;
					}
				}
				sLeaser.sprites[startIndex].x = Vector2.Lerp(lastFootPos, footPos, timeStacker).x - camPos.x;
				sLeaser.sprites[startIndex].y = Vector2.Lerp(lastFootPos, footPos, timeStacker).y - camPos.y;
			}
			else
			{
				sLeaser.sprites[startIndex].element = Futile.atlasManager.GetElementWithName("pixel");
				sLeaser.sprites[startIndex].scale = 8f;
				sLeaser.sprites[startIndex].x = goalPos.x - camPos.x;
				sLeaser.sprites[startIndex].y = goalPos.y - camPos.y;
			}
		}

		public void Reset()
		{
			rootPos = owner.myYeek.mainBodyChunk.pos;
			goalPos = owner.myYeek.mainBodyChunk.pos;
		}
	}

	public Color blackColor;

	public Color tailHighlightColor;

	public CreatureLooker creatureLooker;

	public Vector2 debugPos;

	public Color eyeColor;

	public float blinkPercent;

	public int blinkStartCounter;

	public int moreBlinks;

	public Color furColor;

	public Color beakColor;

	public float flip;

	public float lastFlip;

	public Color HeadfurColor;

	public float watchIntensity;

	public Vector2 fearLookLoc;

	public List<YeekFeather> bodyFeathers;

	public int plumageGraphic;

	public Color featherColor;

	public Color trueEyeColor;

	public float darkness;

	public float lastDarkness;

	public float swimCounter;

	public float eyeShudder;

	public Vector2 headDrawDirection;

	public Vector2 lastHeadDrawDirection;

	public YeekLeg[] legs;

	public bool climbingModeGraphicsClean;

	public bool previousClimbingMode;

	public Yeek myYeek => base.owner as Yeek;

	public int BodySpritesStart => FeatherSpritesStart + FeatherSpritesLength;

	public int BodySpritesLength => 2;

	public int SecondTailStart => BodySpritesStart + BodySpritesLength;

	public int SecondTailLength => 1;

	public int FeatherSpritesStart => 2;

	public int FeatherSpritesLength => YeekFeather.spriteCount * BodyFeatherCount;

	public int LegSpritesStart => SecondTailStart + SecondTailLength;

	public int LegSpritesLength => YeekLeg.spriteCount;

	public int HeadSpritesStart => LegSpritesStart + LegSpritesLength + LegSpritesLength;

	public int HeadSpritesLength => 3;

	public int SpritesTotalLength => HeadSpritesStart + HeadSpritesLength;

	public int BodyFeatherCount => bodyFeathers.Count;

	public int TotGraphSegments => BodyMeshLength + TailGraphSegments;

	public int TailGraphSegments => myYeek.TailSegments;

	public int BodyMeshLength => 5;

	public bool drawHeadInClimbingMode => myYeek.GetClimbingMode;

	public bool CanAdvanceClimb
	{
		get
		{
			if (legs[0].AtGoal())
			{
				return legs[1].AtGoal();
			}
			return false;
		}
	}

	public YeekGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		creatureLooker = new CreatureLooker(this, myYeek.AI.tracker, myYeek, 0f, 30);
		bodyFeathers = new List<YeekFeather>();
		CreateCosmeticAppearance();
		lastDarkness = -1f;
		UpdateHeadAngle();
		legs = new YeekLeg[2];
		legs[0] = new YeekLeg(this, isRightLeg: true);
		legs[1] = new YeekLeg(this, isRightLeg: false);
	}

	public override void Update()
	{
		base.Update();
		eyeShudder *= 0.98f;
		swimCounter += 0.1f;
		creatureLooker.Update();
		blinkStartCounter--;
		lastFlip = flip;
		Vector2 v = Custom.DirVec(myYeek.bodyChunks[myYeek.bodyChunks.Length - 2].pos, myYeek.bodyChunks[1].pos);
		_ = v.normalized;
		Custom.VecToDeg(Custom.PerpendicularVector(v));
		for (int i = 0; i < BodyFeatherCount; i++)
		{
			float t = (float)i / (float)BodyFeatherCount;
			Vector2 vector = Custom.DirVec(base.owner.bodyChunks[0].pos, base.owner.bodyChunks[1].pos);
			Vector2 newRootPos = Vector2.Lerp(myYeek.bodyChunks[1].pos + vector * 4f, myYeek.bodyChunks[0].pos + vector * -10f, t);
			bodyFeathers[i].Update(newRootPos, null);
		}
		if (myYeek.bodyDirection.x != 0f)
		{
			flip = Mathf.Sign(myYeek.bodyDirection.x);
		}
		UpdateHeadAngle();
		UpdateLegs();
		if (!myYeek.Consious)
		{
			blinkStartCounter = 0;
			blinkPercent += 0.5f;
			if (blinkPercent > 0.8f)
			{
				blinkPercent = 0.8f;
			}
		}
		else if (blinkStartCounter < 5)
		{
			blinkPercent += 0.3f;
			if (blinkPercent > 1f)
			{
				blinkPercent = 1f;
			}
			if (blinkStartCounter < 0)
			{
				blinkStartCounter = UnityEngine.Random.Range(40, 300) + (int)(100f * myYeek.abstractCreature.personality.nervous);
				if (moreBlinks > 0)
				{
					moreBlinks--;
					blinkStartCounter = UnityEngine.Random.Range(8, 10) + (int)(10f * myYeek.abstractCreature.personality.bravery);
				}
				else if (UnityEngine.Random.value < 0.5f)
				{
					moreBlinks = 1;
				}
			}
		}
		else
		{
			blinkPercent -= 0.18f;
			if (blinkPercent < 0f)
			{
				blinkPercent = 0f;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		Color color = Color.Lerp(furColor, blackColor, 0.2f + darkness * 0.8f);
		sLeaser.sprites[BodySpritesStart].color = color;
		sLeaser.sprites[SecondTailStart].color = color;
		int num = (sLeaser.sprites[BodySpritesStart] as TriangleMesh).verticeColors.Length;
		for (int i = 0; i < num; i++)
		{
			if (i > num - 16 || (i > num - 24 && i <= num - 20))
			{
				(sLeaser.sprites[BodySpritesStart] as TriangleMesh).verticeColors[i] = tailHighlightColor;
			}
			else
			{
				(sLeaser.sprites[BodySpritesStart] as TriangleMesh).verticeColors[i] = color;
			}
		}
		num = (sLeaser.sprites[SecondTailStart] as TriangleMesh).verticeColors.Length;
		for (int j = 0; j < num; j++)
		{
			if (j > num - 16 || (j > num - 24 && j <= num - 20))
			{
				(sLeaser.sprites[SecondTailStart] as TriangleMesh).verticeColors[j] = tailHighlightColor;
			}
			else
			{
				(sLeaser.sprites[SecondTailStart] as TriangleMesh).verticeColors[j] = color;
			}
		}
		num = (sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).verticeColors.Length;
		for (int k = 0; k < num; k++)
		{
			(sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).verticeColors[k] = color;
		}
		sLeaser.sprites[HeadSpritesStart].color = color;
		eyeColor = Color.Lerp(trueEyeColor, palette.blackColor, 0.7f);
		ApplyPaletteLeg(sLeaser, rCam, palette, LegSpritesStart);
		ApplyPaletteLeg(sLeaser, rCam, palette, LegSpritesStart + LegSpritesLength);
		for (int l = 0; l < BodyFeatherCount; l++)
		{
			bodyFeathers[l].ApplyPalette(sLeaser, rCam, palette);
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[SpritesTotalLength];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].scale = 0f;
		sLeaser.sprites[0].isVisible = false;
		sLeaser.sprites[1] = new FSprite("pixel");
		sLeaser.sprites[1].scale = 0f;
		sLeaser.sprites[1].isVisible = false;
		for (int i = 0; i < BodyFeatherCount; i++)
		{
			bodyFeathers[i].InitiateSprites(sLeaser, rCam, FeatherSpritesStart + YeekFeather.spriteCount * i);
		}
		sLeaser.sprites[BodySpritesStart] = TriangleMesh.MakeLongMesh(TotGraphSegments, pointyTip: true, customColor: true);
		sLeaser.sprites[BodySpritesStart + 1] = TriangleMesh.MakeLongMesh(1, pointyTip: false, customColor: true);
		sLeaser.sprites[SecondTailStart] = TriangleMesh.MakeLongMesh(TailGraphSegments, pointyTip: true, customColor: true);
		InititateLeg(sLeaser, rCam, LegSpritesStart);
		InititateLeg(sLeaser, rCam, LegSpritesStart + LegSpritesLength);
		sLeaser.sprites[HeadSpritesStart] = new FSprite("SnailShellA");
		sLeaser.sprites[HeadSpritesStart].anchorY = 0.1f;
		sLeaser.sprites[HeadSpritesStart].scaleX = 1.2f;
		sLeaser.sprites[HeadSpritesStart].scaleY = 0.9f;
		sLeaser.sprites[HeadSpritesStart + 1] = new FSprite("Circle20");
		sLeaser.sprites[HeadSpritesStart + 1].scaleX = 0.3f;
		sLeaser.sprites[HeadSpritesStart + 1].scaleY = 0.6f;
		sLeaser.sprites[HeadSpritesStart + 2] = new FSprite("Circle20");
		sLeaser.sprites[HeadSpritesStart + 2].scaleX = 0.3f;
		sLeaser.sprites[HeadSpritesStart + 2].scaleY = 0.6f;
		AddToContainer(sLeaser, rCam, null);
	}

	public Vector2 GraphSegmentPos(int i, float timeStacker, bool secondary)
	{
		float t = (float)i / (float)BodyMeshLength;
		if (i < BodyMeshLength)
		{
			Vector2 vector = Custom.DirVec(Vector2.Lerp(myYeek.bodyChunks[0].lastPos + new Vector2(0f, -4f), myYeek.bodyChunks[0].pos + new Vector2(0f, -4f), timeStacker), Vector2.Lerp(myYeek.bodyChunks[1].lastPos, myYeek.bodyChunks[1].pos, timeStacker));
			return Vector2.Lerp(Vector2.Lerp(myYeek.bodyChunks[1].lastPos, myYeek.bodyChunks[1].pos, timeStacker) + vector * 2f, Vector2.Lerp(myYeek.bodyChunks[0].lastPos + new Vector2(0f, -4f), myYeek.bodyChunks[0].pos + new Vector2(0f, -4f), timeStacker) - vector * 10f, t);
		}
		int index = i - BodyMeshLength;
		if (secondary)
		{
			return myYeek.tails[1].GetPos(index, timeStacker);
		}
		return myYeek.tails[0].GetPos(index, timeStacker);
	}

	public float GraphSegmentRad(int i)
	{
		float t = 1f;
		float num = 1f;
		if (i >= TotGraphSegments - 3)
		{
			num = 2f;
		}
		float num2 = 2.1f;
		float num3 = (float)i / (float)BodyMeshLength;
		if (i < BodyMeshLength)
		{
			float num4 = Mathf.Lerp(myYeek.bodyChunks[1].rad + 1f, myYeek.bodyChunks[0].rad + 1f, num3);
			float num5 = 0.477f;
			float num6 = Mathf.Lerp(num4 * 0.6f, num4 * num2, Mathf.InverseLerp(0f, num5, Mathf.Clamp(num3, 0f, num5)));
			if (num3 > num5)
			{
				float b = 1.2f * Mathf.Lerp(0.75f, 1.35f, t) * num;
				num6 = Mathf.Lerp(num4 * num2, b, Mathf.InverseLerp(num5, 1f, Mathf.Clamp(num3, num5, 1f)));
			}
			return num6 * num;
		}
		if (i < BodyMeshLength + 2)
		{
			return 1.6f * Mathf.Lerp(0.75f, 1.35f, t) * num;
		}
		return 1.2f * Mathf.Lerp(0.75f, 1.35f, t) * num;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled || myYeek.room == null)
		{
			return;
		}
		if (previousClimbingMode != drawHeadInClimbingMode)
		{
			climbingModeGraphicsClean = false;
		}
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(myYeek.firstChunk.pos) * (1f - rCam.room.LightSourceExposure(myYeek.firstChunk.pos));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (!climbingModeGraphicsClean)
		{
			AddToContainer(sLeaser, rCam, null);
		}
		sLeaser.sprites[0].x = myYeek.bodyChunks[1].pos.x + headDrawDirection.x * 40f - camPos.x;
		sLeaser.sprites[0].y = myYeek.bodyChunks[1].pos.y + headDrawDirection.y * 40f - camPos.y;
		sLeaser.sprites[1].x = myYeek.room.MiddleOfTile(myYeek.AI.pathFinder.GetDestination).x - camPos.x;
		sLeaser.sprites[1].y = myYeek.room.MiddleOfTile(myYeek.AI.pathFinder.GetDestination).y - camPos.y;
		Vector2.Lerp(myYeek.firstChunk.lastPos, myYeek.firstChunk.pos, timeStacker);
		_ = myYeek.bodyDirection;
		Vector2 vector = GraphSegmentPos(0, timeStacker, secondary: false);
		Vector2 vector2 = GraphSegmentPos(0, timeStacker, secondary: true);
		float num = GraphSegmentRad(0);
		for (int i = 0; i < TotGraphSegments; i++)
		{
			Vector2 vector3 = GraphSegmentPos(i, timeStacker, secondary: false);
			Vector2 vector4 = GraphSegmentPos(i, timeStacker, secondary: true);
			float num2 = GraphSegmentRad(i);
			Vector2 normalized = (vector3 - vector).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num3 = Vector2.Distance(vector3, vector) / 5f;
			(sLeaser.sprites[BodySpritesStart] as TriangleMesh).MoveVertice(i * 4, vector - vector5 * ((num + num2) * 0.5f) + normalized * num3 - camPos);
			(sLeaser.sprites[BodySpritesStart] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector5 * ((num + num2) * 0.5f) + normalized * num3 - camPos);
			if (i < TotGraphSegments - 1)
			{
				(sLeaser.sprites[BodySpritesStart] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - vector5 * num2 - normalized * num3 - camPos);
				(sLeaser.sprites[BodySpritesStart] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + vector5 * num2 - normalized * num3 - camPos);
			}
			else
			{
				(sLeaser.sprites[BodySpritesStart] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - camPos);
			}
			normalized = (vector4 - vector2).normalized;
			vector5 = Custom.PerpendicularVector(normalized);
			num3 = Vector2.Distance(vector4, vector2) / 5f;
			int num4 = TotGraphSegments - TailGraphSegments;
			if (i >= num4)
			{
				int num5 = i - num4;
				(sLeaser.sprites[SecondTailStart] as TriangleMesh).MoveVertice(num5 * 4, vector2 - vector5 * ((num + num2) * 0.5f) + normalized * num3 - camPos);
				(sLeaser.sprites[SecondTailStart] as TriangleMesh).MoveVertice(num5 * 4 + 1, vector2 + vector5 * ((num + num2) * 0.5f) + normalized * num3 - camPos);
				if (i < TotGraphSegments - 1)
				{
					(sLeaser.sprites[SecondTailStart] as TriangleMesh).MoveVertice(num5 * 4 + 2, vector4 - vector5 * num2 - normalized * num3 - camPos);
					(sLeaser.sprites[SecondTailStart] as TriangleMesh).MoveVertice(num5 * 4 + 3, vector4 + vector5 * num2 - normalized * num3 - camPos);
				}
				else
				{
					(sLeaser.sprites[SecondTailStart] as TriangleMesh).MoveVertice(num5 * 4 + 2, vector4 - camPos);
				}
			}
			num = num2;
			vector = vector3;
			vector2 = vector4;
		}
		vector = GraphSegmentPos(1, timeStacker, secondary: false);
		vector2 = GraphSegmentPos(1, timeStacker, secondary: true);
		DrawHead(sLeaser, rCam, timeStacker, camPos);
		for (int j = 0; j < legs.Length; j++)
		{
			legs[j].DrawLeg(sLeaser, rCam, timeStacker, camPos, LegSpritesStart + LegSpritesLength * j);
		}
		for (int k = 0; k < BodyFeatherCount; k++)
		{
			bodyFeathers[k].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return myYeek.AI.focusCreature;
	}

	public void LookAtNothing()
	{
	}

	public void InititateLeg(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int startIndex)
	{
		sLeaser.sprites[startIndex] = new FSprite("pixel");
		sLeaser.sprites[startIndex].scale = 1f;
	}

	public void ApplyPaletteLeg(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int startIndex)
	{
		sLeaser.sprites[startIndex].color = Color.Lerp(furColor, blackColor, 0.2f + darkness * 0.8f);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FContainer fContainer = rCam.ReturnFContainer("Foreground");
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (((i >= HeadSpritesStart && i < HeadSpritesStart + HeadSpritesLength) || (i >= LegSpritesStart && i < LegSpritesStart + LegSpritesLength * 2)) && drawHeadInClimbingMode)
			{
				fContainer.AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
		climbingModeGraphicsClean = true;
		previousClimbingMode = drawHeadInClimbingMode;
	}

	public void StartClimbing(Vector2 Pos)
	{
		legs[0].Reset();
		legs[1].Reset();
		AdvanceClimb();
	}

	public void AdvanceClimb()
	{
		Vector2 p = myYeek.room.MiddleOfTile(myYeek.firstChunk.pos + myYeek.climbingOrientation.normalized * 20f);
		myYeek.bodyChunks[1].vel += Custom.DirVec(myYeek.bodyChunks[1].pos, p) * UnityEngine.Random.value;
	}

	public void CreateCosmeticAppearance()
	{
		float groupLeaderPotential = myYeek.GroupLeaderPotential;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(myYeek.abstractCreature.ID.RandomSeed);
		featherColor = new Color((1f - groupLeaderPotential) * UnityEngine.Random.Range(0.4f, 0.9f), Mathf.Clamp(UnityEngine.Random.value * groupLeaderPotential, 0f, 1f), Mathf.Clamp(groupLeaderPotential, 0f, 1f));
		tailHighlightColor = Color.Lerp(new Color(0.85490197f, 0.23137255f, 1f / 85f), featherColor, 0.4f + myYeek.abstractCreature.personality.bravery * 0.3f);
		furColor = Color.Lerp(featherColor, new Color(0.55f, 0.3f, 0.12f), 0.33f + myYeek.abstractCreature.personality.energy * 0.25f);
		Color a = Color.Lerp(featherColor, new Color(0.38f, 0.37f, 0.35f), 0.33f + myYeek.abstractCreature.personality.aggression * 0.25f);
		a = ((!(myYeek.abstractCreature.personality.nervous > myYeek.abstractCreature.personality.bravery)) ? Color.Lerp(a, Color.black, myYeek.abstractCreature.personality.bravery * 0.5f) : Color.Lerp(a, Color.white, myYeek.abstractCreature.personality.nervous * 0.5f));
		furColor = Color.Lerp(a, furColor, myYeek.abstractCreature.personality.sympathy);
		furColor = Color.Lerp(furColor, Color.white, UnityEngine.Random.Range(0.3f, 0.75f));
		HeadfurColor = Color.Lerp(furColor + new Color(0.1f, 0.1f, 0.1f), furColor + new Color(0.3f, 0.15f, 0.15f), myYeek.abstractCreature.personality.bravery);
		HeadfurColor = Color.Lerp(furColor, HeadfurColor, myYeek.abstractCreature.personality.dominance);
		beakColor = Color.Lerp(furColor, new Color(0.81f, 0.53f, 0.34f), 0.6f + myYeek.abstractCreature.personality.dominance / 3f);
		featherColor = tailHighlightColor;
		trueEyeColor = featherColor;
		if (myYeek.abstractCreature.Winterized)
		{
			Vector3 vector = Custom.RGB2HSL(furColor);
			Vector3 vector2 = Custom.RGB2HSL(HeadfurColor);
			Vector3 vector3 = Custom.RGB2HSL(beakColor);
			vector.y *= 0.1f;
			vector.z = Mathf.Lerp(vector.z, 1f, 0.6f);
			vector2.y *= 0.1f;
			vector2.z = Mathf.Lerp(vector2.z, 1f, 0.7f);
			vector3.y *= 0.1f;
			vector3.z = Mathf.Lerp(vector3.z, 1f, 0.5f);
			furColor = Custom.HSL2RGB(vector.x, vector.y, vector.z);
			HeadfurColor = Custom.HSL2RGB(vector2.x, vector2.y, vector2.z);
			beakColor = Custom.HSL2RGB(vector3.x, vector3.y, vector3.z);
			tailHighlightColor = new Color(23f / 51f, 32f / 51f, 0.8156863f);
			featherColor = Color.Lerp(furColor, tailHighlightColor, 0.2f + groupLeaderPotential / 2f);
			trueEyeColor = new Color(0f, 0.8f, 0.8f);
		}
		plumageGraphic = 2;
		while (plumageGraphic == 2 || plumageGraphic == 1)
		{
			plumageGraphic = UnityEngine.Random.Range(0, 7);
		}
		float num = 0.6f;
		UnityEngine.Random.Range(0.5f, Mathf.Clamp(groupLeaderPotential * 1.5f, 0.6f, 1.2f));
		int num2 = UnityEngine.Random.Range(3, 5);
		for (int num3 = num2; num3 > 0; num3--)
		{
			YeekFeather yeekFeather = new YeekFeather(myYeek.bodyChunks[0].pos, this, num3, num2);
			yeekFeather.featherScaler = num * 2f;
			bodyFeathers.Add(yeekFeather);
		}
		UnityEngine.Random.state = state;
	}

	public float EyeShaker()
	{
		return Mathf.Lerp(0f, UnityEngine.Random.Range(-2f, 2f), eyeShudder);
	}

	public float EyeShakerScale()
	{
		return Mathf.Lerp(1f, UnityEngine.Random.Range(0.8f, 1.1f), eyeShudder);
	}

	public void UpdateHeadAngle()
	{
		if (creatureLooker.lookCreature != null)
		{
			if (creatureLooker.lookCreature.VisualContact)
			{
				fearLookLoc = Custom.DirVec(myYeek.mainBodyChunk.pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
			}
			else if (creatureLooker.lookCreature.EstimatedChanceOfFinding * myYeek.abstractCreature.personality.nervous > 0.2f)
			{
				fearLookLoc = Custom.DirVec(myYeek.mainBodyChunk.pos, myYeek.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()));
			}
		}
		if (creatureLooker != null && creatureLooker.lookCreature != null)
		{
			watchIntensity = Mathf.Lerp(watchIntensity, creatureLooker.lookCreature.EstimatedChanceOfFinding, 0.05f + myYeek.abstractCreature.personality.nervous / 10f);
		}
		else
		{
			watchIntensity = Mathf.Lerp(watchIntensity, 0f, 0.15f - myYeek.abstractCreature.personality.nervous / 10f);
		}
		if (Mathf.Abs(myYeek.mainBodyChunk.vel.x) > 1f)
		{
			watchIntensity = Mathf.Lerp(watchIntensity, 0f, 0.45f);
		}
		Vector2 vector = new Vector2(0f, 0f);
		float num = Mathf.InverseLerp(-90f, 90f, myYeek.headLeadingCounter);
		if (myYeek.GetTunnelMode)
		{
			vector = myYeek.climbingOrientation;
			num = 0f;
		}
		else
		{
			vector = new Vector2(myYeek.bodyDirection.x, -3f * Mathf.InverseLerp(2f, 0f, Mathf.Abs(myYeek.bodyDirection.x)));
		}
		vector = ((!(num > 0.75f) && myYeek.AI.goalFruit != null && myYeek.grasps[0] == null) ? Custom.DirVec(myYeek.mainBodyChunk.pos, myYeek.room.MiddleOfTile(myYeek.AI.goalFruit.pos)) : Vector2.Lerp(vector, new Vector2(myYeek.bodyDirection.x * (0.5f + num / 2f) * 5f, 0f), num));
		if (drawHeadInClimbingMode)
		{
			vector = Vector2.Lerp(vector, new Vector2(Mathf.Sign(myYeek.bodyChunks[1].pos.x - myYeek.bodyChunks[0].pos.x), -1f), 0.5f);
		}
		else if (num <= 0.75f && watchIntensity > 0.5f)
		{
			vector = Vector2.Lerp(vector, fearLookLoc, watchIntensity);
		}
		vector += Custom.RNV() * UnityEngine.Random.value * myYeek.abstractCreature.personality.nervous * myYeek.AI.threatTracker.Panic * 0.1f;
		if (vector.magnitude < 1f)
		{
			vector = Vector2.Lerp(vector, new Vector2(0f, -0.6f), 1f - vector.magnitude);
		}
		else
		{
			vector.Normalize();
		}
		lastHeadDrawDirection = headDrawDirection;
		headDrawDirection = Vector2.Lerp(headDrawDirection, vector, 0.3f);
	}

	public void DrawHead(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(Vector2.Lerp(myYeek.bodyChunks[0].lastPos, myYeek.bodyChunks[0].pos, timeStacker), Vector2.Lerp(myYeek.bodyChunks[1].lastPos + new Vector2(0f, -2f), myYeek.bodyChunks[1].pos + new Vector2(0f, -2f), timeStacker), 0.9f);
		Vector2 vector2 = Vector2.Lerp(lastHeadDrawDirection, headDrawDirection, timeStacker);
		float num = Mathf.InverseLerp(0f, 1f, vector2.y);
		vector.y -= num * 6f;
		sLeaser.sprites[HeadSpritesStart].x = vector.x - camPos.x;
		sLeaser.sprites[HeadSpritesStart].y = vector.y - camPos.y;
		sLeaser.sprites[HeadSpritesStart].rotation = Custom.VecToDeg(vector2);
		Vector2 vector3 = Custom.DegToVec(sLeaser.sprites[HeadSpritesStart].rotation);
		float b = 0.3f;
		float a = 0.4f;
		float b2 = 0.3f;
		float num2 = 3f;
		float num3 = 5f;
		float num4 = -1f;
		float num5 = RightSideShown(timeStacker);
		float num6 = LeftSideShown(timeStacker);
		float num7 = Mathf.InverseLerp(-0.3f, -1f, vector3.y);
		float num8 = Mathf.InverseLerp(0.3f, 1f, vector3.y);
		Vector2 vector4 = Custom.DirVec(Vector2.Lerp(myYeek.bodyChunks[0].lastPos, myYeek.bodyChunks[0].pos, timeStacker), Vector2.Lerp(myYeek.bodyChunks[1].lastPos, myYeek.bodyChunks[1].pos, timeStacker));
		if (drawHeadInClimbingMode)
		{
			vector4 *= 0.13f;
		}
		Vector2 vector5 = Custom.PerpendicularVector(Custom.DirVec(new Vector2(0f, 0f), new Vector2((0f - num6) * 3f, 1f) + new Vector2(num5 * 3f, 1f)));
		(sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).MoveVertice(0, Vector2.Lerp(myYeek.bodyChunks[0].lastPos, myYeek.bodyChunks[0].pos, timeStacker) + vector4 * 3f + vector5 * -10f - camPos);
		(sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).MoveVertice(1, Vector2.Lerp(myYeek.bodyChunks[0].lastPos, myYeek.bodyChunks[0].pos, timeStacker) + vector5 * 10f - camPos);
		(sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).MoveVertice(2, vector + vector4 * 3f + vector5 * -8f - camPos);
		(sLeaser.sprites[BodySpritesStart + 1] as TriangleMesh).MoveVertice(3, vector + vector4 * 3f + vector5 * 8f - camPos);
		vector += vector2 * 4f;
		sLeaser.sprites[HeadSpritesStart].scaleX = Mathf.Lerp(0.65f, 0.7f, num7);
		sLeaser.sprites[HeadSpritesStart].scaleY = Mathf.Lerp(0.7f, 0.8f, num7);
		sLeaser.sprites[HeadSpritesStart].y += Mathf.Lerp(6f, 0f, num7);
		sLeaser.sprites[HeadSpritesStart + 1].isVisible = num5 > 0.14f || num7 > 0.1f;
		sLeaser.sprites[HeadSpritesStart + 1].rotation = sLeaser.sprites[HeadSpritesStart].rotation + Mathf.Lerp(0f, -10f, num7);
		sLeaser.sprites[HeadSpritesStart + 1].rotation = sLeaser.sprites[HeadSpritesStart].rotation + Mathf.Lerp(0f, 20f, num8);
		sLeaser.sprites[HeadSpritesStart + 1].x = vector.x + Mathf.Lerp(0f, num2, num5) - camPos.x;
		sLeaser.sprites[HeadSpritesStart + 1].x = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 1].x, vector.x - num3 - camPos.x, num7 + num8) + EyeShaker();
		sLeaser.sprites[HeadSpritesStart + 1].y = vector.y + Mathf.Lerp(10f, 5f, num5) - camPos.y;
		sLeaser.sprites[HeadSpritesStart + 1].y = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 1].y, vector.y + num4 - camPos.y, num7) + EyeShaker();
		sLeaser.sprites[HeadSpritesStart + 1].scaleX = Mathf.Lerp(0f, b, num5);
		sLeaser.sprites[HeadSpritesStart + 1].scaleX = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 1].scaleX, 0.3f, num7);
		sLeaser.sprites[HeadSpritesStart + 1].scaleX *= 1f - blinkPercent;
		sLeaser.sprites[HeadSpritesStart + 1].scaleY = Mathf.Lerp(a, b2, num5);
		sLeaser.sprites[HeadSpritesStart + 1].scaleX *= EyeShakerScale();
		sLeaser.sprites[HeadSpritesStart + 1].scaleY *= EyeShakerScale();
		sLeaser.sprites[HeadSpritesStart + 1].color = Color.Lerp(eyeColor, new Color(0.95f, 0.1f, 0f), darkness);
		sLeaser.sprites[HeadSpritesStart + 2].isVisible = num6 > 0.14f || num7 > 0.1f;
		sLeaser.sprites[HeadSpritesStart + 2].rotation = sLeaser.sprites[HeadSpritesStart].rotation + Mathf.Lerp(0f, 10f, num7);
		sLeaser.sprites[HeadSpritesStart + 2].rotation = sLeaser.sprites[HeadSpritesStart].rotation + Mathf.Lerp(0f, -20f, num8);
		sLeaser.sprites[HeadSpritesStart + 2].x = vector.x + Mathf.Lerp(0f, 0f - num2, num6) - camPos.x;
		sLeaser.sprites[HeadSpritesStart + 2].x = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 2].x, vector.x + num3 - camPos.x, num7 + num8) + EyeShaker();
		sLeaser.sprites[HeadSpritesStart + 2].y = vector.y + Mathf.Lerp(10f, 5f, num6) - camPos.y;
		sLeaser.sprites[HeadSpritesStart + 2].y = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 2].y, vector.y + num4 - camPos.y, num7) + EyeShaker();
		sLeaser.sprites[HeadSpritesStart + 2].scaleX = Mathf.Lerp(0f, b, num6);
		sLeaser.sprites[HeadSpritesStart + 2].scaleX = Mathf.Lerp(sLeaser.sprites[HeadSpritesStart + 2].scaleX, 0.3f, num7);
		sLeaser.sprites[HeadSpritesStart + 2].scaleX *= 1f - blinkPercent;
		sLeaser.sprites[HeadSpritesStart + 2].scaleY = Mathf.Lerp(a, b2, num6);
		sLeaser.sprites[HeadSpritesStart + 2].scaleX *= EyeShakerScale();
		sLeaser.sprites[HeadSpritesStart + 2].scaleY *= EyeShakerScale();
		sLeaser.sprites[HeadSpritesStart + 2].color = Color.Lerp(eyeColor, new Color(0.95f, 0.1f, 0f), darkness);
	}

	public void UpdateLegs()
	{
		YeekLeg.AnimState animState = YeekLeg.AnimState.Sit;
		if (!myYeek.OnGround || myYeek.firstChunk.vel.y > 1f)
		{
			animState = YeekLeg.AnimState.Jump;
		}
		if (myYeek.GetClimbingMode)
		{
			animState = YeekLeg.AnimState.Climb;
		}
		if (myYeek.GetTunnelMode)
		{
			animState = YeekLeg.AnimState.Tunnel;
		}
		if (!myYeek.Consious)
		{
			animState = YeekLeg.AnimState.Dangle;
		}
		legs[0].Update(animState);
		legs[1].Update(animState);
	}

	public float SideShown(float timeStacker)
	{
		float num = Mathf.InverseLerp(0.2f, 0.9f, Vector2.Lerp(lastHeadDrawDirection, headDrawDirection, timeStacker).x);
		float num2 = Mathf.InverseLerp(-0.2f, -0.9f, Vector2.Lerp(lastHeadDrawDirection, headDrawDirection, timeStacker).x);
		if (myYeek.dead || myYeek.grabbedBy.Count > 0)
		{
			num *= 0.15f;
			num2 *= 0.15f;
		}
		return num + (0f - num2);
	}

	public float RightSideShown(float timeStacker)
	{
		return Mathf.Clamp01(SideShown(timeStacker));
	}

	public float LeftSideShown(float timeStacker)
	{
		return Mathf.Clamp01(SideShown(timeStacker) * -1f);
	}
}
