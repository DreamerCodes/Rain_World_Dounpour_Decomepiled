using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class VoidSpawnGraphics : ComplexGraphicsModule
{
	public class Antenna : GraphicsSubModule
	{
		public Vector2[,] segments;

		public float conRad;

		public float thickness;

		public float rigid;

		public float forceDirection;

		public float ang;

		public int rigidSegments;

		public VoidSpawnGraphics vsGraphics => owner as VoidSpawnGraphics;

		public virtual Vector2 ResetPos => vsGraphics.spawn.mainBody[0].pos;

		public virtual Vector2 ResetDir => Custom.RNV();

		public Antenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite)
		{
			this.conRad = conRad;
			this.thickness = thickness;
			this.rigid = rigid;
			this.rigidSegments = rigidSegments;
			this.forceDirection = forceDirection;
			this.ang = ang;
			totalSprites = 1;
			segments = new Vector2[segs, 3];
		}

		public override void Update()
		{
			base.Update();
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
				segments[i, 0] += segments[i, 2];
				segments[i, 2] *= Custom.LerpMap(segments[i, 2].magnitude, 0.2f * vsGraphics.spawn.sizeFac, 6f * vsGraphics.spawn.sizeFac, 1f, 0.7f);
			}
			for (int j = 1; j < segments.GetLength(0); j++)
			{
				Vector2 vector = Custom.DirVec(segments[j, 0], segments[j - 1, 0]);
				float num = Vector2.Distance(segments[j, 0], segments[j - 1, 0]);
				segments[j, 0] -= (conRad - num) * vector * 0.5f;
				segments[j, 2] -= (conRad - num) * vector * 0.5f;
				segments[j - 1, 0] += (conRad - num) * vector * 0.5f;
				segments[j - 1, 2] += (conRad - num) * vector * 0.5f;
			}
			for (int k = 2; k < segments.GetLength(0); k++)
			{
				Vector2 vector2 = Custom.DirVec(segments[k, 0], segments[k - 2, 0]);
				segments[k, 2] -= vector2 * rigid;
				segments[k - 2, 2] += vector2 * rigid;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Vector2 resetPos = ResetPos;
			Vector2 resetDir = ResetDir;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 0] = resetPos + resetDir * conRad;
				segments[i, 1] = segments[i, 0];
				segments[i, 2] *= 0f;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
			sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders["VoidSpawnBody"];
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[1, 1], segments[1, 0], timeStacker), vector) * conRad * 0.3f;
			float num = 1f;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float f = (float)i / (float)(segments.GetLength(0) - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector2, vector) / 5f;
				float num3 = Mathf.Lerp(thickness, 0.5f, Mathf.Pow(f, 0.2f));
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (num + num3) * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (num + num3) * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - normalized * num2 - camPos);
				vector = vector2;
				num = num3;
			}
			for (int j = 0; j < (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[j] = new Color(vsGraphics.meshColor.r, vsGraphics.meshColor.g, vsGraphics.meshColor.b, vsGraphics.AlphaFromGlowDist((sLeaser.sprites[firstSprite] as TriangleMesh).vertices[j], vsGraphics.glowPos - camPos));
			}
		}
	}

	public class FrontAntenna : Antenna
	{
		public override Vector2 ResetPos => base.vsGraphics.spawn.mainBody[0].pos;

		public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[1].pos, base.vsGraphics.spawn.mainBody[0].pos) + ang);

		public FrontAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
		{
		}

		public override void Update()
		{
			base.Update();
			segments[0, 0] = base.vsGraphics.spawn.mainBody[0].pos;
			segments[0, 2] *= 0f;
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[1].pos, base.vsGraphics.spawn.mainBody[0].pos) + ang);
			for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
			{
				segments[i, 2] += vector * forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i);
			}
		}
	}

	public class TailAntenna : Antenna
	{
		public override Vector2 ResetPos => base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos;

		public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 2].pos, base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos) + ang);

		public TailAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
		{
		}

		public override void Update()
		{
			base.Update();
			segments[0, 0] = base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos;
			segments[0, 2] *= 0f;
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 2].pos, base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos) + ang);
			for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
			{
				segments[i, 2] += vector * forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i);
			}
		}
	}

	public Vector2 glowPos;

	public float playerGlowVision;

	public float darkness;

	public List<Antenna> antennae;

	public float[,] playersGlowVision;

	public Color meshColor;

	public VoidSpawn spawn => base.owner as VoidSpawn;

	public bool dayLightMode => spawn.dayLightMode;

	public int BodyMeshSprite => 0;

	public int GlowSprite => 1;

	public int EffectSprite => 2;

	public bool hasOwnGoldEffect => spawn.voidMeltInRoom == 0f;

	public VoidSpawnGraphics(PhysicalObject owner)
		: base(owner, internalContainers: false)
	{
		playersGlowVision = new float[owner.abstractPhysicalObject.world.game.Players.Count, 2];
		totalSprites = (hasOwnGoldEffect ? 3 : 2);
		antennae = new List<Antenna>();
		float num = Mathf.Lerp(spawn.sizeFac, 0.5f + 0.5f * UnityEngine.Random.value, UnityEngine.Random.value);
		int num2;
		int num5;
		float num7;
		float num4;
		float forceDirection;
		float num3;
		switch (UnityEngine.Random.Range(0, 3))
		{
		case 0:
			antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(3, 18), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
			AddSubModule(antennae[antennae.Count - 1]);
			break;
		case 1:
		{
			num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 5));
			num4 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
			num5 = UnityEngine.Random.Range(3, 18);
			float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
			for (int j = 0; j < num2; j++)
			{
				float num9 = (float)j / (float)(num2 - 1);
				antennae.Add(new TailAntenna(this, totalSprites, num5, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num9 * (float)Math.PI)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(0f - num4, num4, num9), 0.1f * num, 2, 2.2f));
				AddSubModule(antennae[antennae.Count - 1]);
			}
			break;
		}
		case 2:
		{
			num2 = UnityEngine.Random.Range(2, 6);
			num3 = Mathf.Lerp(0.1f, 1.8f, UnityEngine.Random.value);
			num4 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
			num5 = UnityEngine.Random.Range(3, UnityEngine.Random.Range(5, 8));
			int num6 = UnityEngine.Random.Range(1, num5 + 1);
			forceDirection = Mathf.Lerp(1.5f, 7f, UnityEngine.Random.value) * num3 / Mathf.Lerp(1f, num6, 0.5f);
			num7 = Mathf.Lerp(4f, 12f, UnityEngine.Random.value) * num;
			float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
			for (int i = 0; i < num2; i++)
			{
				float num8 = (float)i / (float)(num2 - 1);
				antennae.Add(new TailAntenna(this, totalSprites, num5, num7 * Mathf.Lerp(a, 1f, Mathf.Sin(num8 * (float)Math.PI)), 2f, Mathf.Lerp(0f - num4, num4, num8), num3, num6, forceDirection));
				AddSubModule(antennae[antennae.Count - 1]);
			}
			break;
		}
		}
		num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 7));
		num5 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, (int)Custom.LerpMap(spawn.mainBody.Length, 3f, 16f, 6f, 12f, 0.5f)));
		int num10 = num5;
		if (UnityEngine.Random.value < 0.5f)
		{
			num10 = UnityEngine.Random.Range(1, num5 + 1);
		}
		num7 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
		num4 = Mathf.Lerp(12f, 50f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
		forceDirection = Mathf.Lerp(2f, 7f, UnityEngine.Random.value) / Mathf.Lerp(1f, num10, 0.5f);
		num3 = Mathf.Lerp(0.4f, 2.2f, UnityEngine.Random.value);
		for (int k = 0; k < num2; k++)
		{
			float t = (float)k / (float)(num2 - 1);
			antennae.Add(new FrontAntenna(this, totalSprites, num5, num7, 2f * num, Mathf.Lerp(0f - num4, num4, t), num3, num10, forceDirection));
			AddSubModule(antennae[antennae.Count - 1]);
		}
		Reset();
	}

	public override void Update()
	{
		if (!spawn.culled)
		{
			base.Update();
		}
		for (int i = 0; i < base.owner.room.game.Players.Count; i++)
		{
			playersGlowVision[i, 1] = playersGlowVision[i, 0];
			float num = 0f;
			if (base.owner.room.game.Players[i].realizedCreature != null && (base.owner.room.game.setupValues.playerGlowing || (base.owner.room.game.session is StoryGameSession && (base.owner.room.game.session as StoryGameSession).saveState.CanSeeVoidSpawn)) && !base.owner.room.game.Players[i].realizedCreature.inShortcut)
			{
				num = 1f;
			}
			if (playersGlowVision[i, 0] < num)
			{
				playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.025f, 1f / 60f);
			}
			else
			{
				playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.1f, 1f / 3f);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spawn.mainBody.Length, pointyTip: false, customColor: true);
		sLeaser.sprites[BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["VoidSpawnBody"];
		sLeaser.sprites[GlowSprite] = new FSprite("Futile_White");
		sLeaser.sprites[GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
		if (hasOwnGoldEffect)
		{
			sLeaser.sprites[EffectSprite] = new FSprite("Futile_White");
			sLeaser.sprites[EffectSprite].shader = rCam.game.rainWorld.Shaders["GoldenGlow"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature is Player)
		{
			glowPos = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker);
			playerGlowVision = Mathf.Lerp(playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 1], playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 0], timeStacker) * Mathf.Lerp(spawn.lastFade, spawn.fade, timeStacker);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(spawn.mainBody[0].lastPos, spawn.mainBody[0].pos, timeStacker);
		if (!spawn.culled)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = true;
			}
			Vector2 a = vector;
			if (spawn.inEggMode > 0f && spawn.egg != null)
			{
				a = Vector2.Lerp(a, Vector2.Lerp(spawn.egg.lastPos, spawn.egg.pos, timeStacker), spawn.inEggMode);
			}
			if (dayLightMode)
			{
				meshColor = new Color(1f - AlphaFromGlowDist(a, glowPos) * 0.5f, 0.5f, 0f);
			}
			else
			{
				meshColor = new Color(0.7f + 0.3f * Mathf.InverseLerp(0.1f, 0.9f, darkness), Mathf.Lerp(Mathf.Lerp(Custom.LerpMap(darkness, 0.1f, 0.9f, 0.1f, 0.075f, 0.5f), 0.24f, spawn.inEggMode), 0.8f, spawn.voidMeltInRoom), 0f);
			}
			sLeaser.sprites[GlowSprite].x = a.x - camPos.x;
			sLeaser.sprites[GlowSprite].y = a.y - camPos.y;
			if (spawn.inEggMode > 0.9f && spawn.egg != null)
			{
				sLeaser.sprites[GlowSprite].scale = Mathf.Lerp(2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness), spawn.egg.rad / 4f, Mathf.Lerp(0.9f, 1f, spawn.inEggMode));
			}
			else
			{
				sLeaser.sprites[GlowSprite].scale = 2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness);
			}
			if (dayLightMode)
			{
				sLeaser.sprites[GlowSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.5f) * Mathf.Lerp(0.5f, 1f, spawn.inEggMode);
			}
			else
			{
				sLeaser.sprites[GlowSprite].alpha = AlphaFromGlowDist(a, glowPos) * Mathf.Lerp(Mathf.Lerp(0.1f, 0.4f, Mathf.Pow(darkness, 2f)), 1f, spawn.voidMeltInRoom * 0.5f);
			}
			if (hasOwnGoldEffect)
			{
				sLeaser.sprites[EffectSprite].x = a.x - camPos.x;
				sLeaser.sprites[EffectSprite].y = a.y - camPos.y;
				sLeaser.sprites[EffectSprite].scale = 6f * spawn.TotalMass;
				if (dayLightMode)
				{
					sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.5f) * 0.5f;
				}
				else
				{
					sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.6f) * Custom.LerpMap(darkness, 0.1f, 0.9f, 0.3f, 1f, 1.5f);
				}
			}
			vector += Custom.DirVec(Vector2.Lerp(spawn.mainBody[1].lastPos, spawn.mainBody[1].pos, timeStacker), vector) * spawn.mainBody[0].rad;
			float num = spawn.mainBody[0].rad / 2f;
			for (int j = 0; j < spawn.mainBody.Length; j++)
			{
				Vector2 vector2 = Vector2.Lerp(spawn.mainBody[j].lastPos, spawn.mainBody[j].pos, timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector2, vector) / 5f;
				float rad = spawn.mainBody[j].rad;
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector3 * (num + rad) * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector3 * (num + rad) * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * rad - normalized * num2 - camPos);
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * rad - normalized * num2 - camPos);
				vector = vector2;
				num = rad;
			}
			for (int k = 0; k < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; k++)
			{
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[k] = new Color(meshColor.r, meshColor.g, meshColor.b, AlphaFromGlowDist((sLeaser.sprites[BodyMeshSprite] as TriangleMesh).vertices[k], glowPos - camPos));
			}
		}
		else
		{
			for (int l = 0; l < sLeaser.sprites.Length; l++)
			{
				sLeaser.sprites[l].isVisible = false;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		darkness = palette.darkness;
		if (dayLightMode)
		{
			sLeaser.sprites[GlowSprite].color = RainWorld.SaturatedGold;
		}
		else
		{
			sLeaser.sprites[GlowSprite].color = Color.Lerp(RainWorld.SaturatedGold, RainWorld.GoldRGB, Mathf.InverseLerp(0.3f, 0.9f, darkness));
		}
	}

	public bool VisibleAtGlowDist(Vector2 A, Vector2 B, float margin)
	{
		if (playerGlowVision == 0f)
		{
			return false;
		}
		return Custom.DistLess(A, B, Mathf.Lerp(100f, 400f, playerGlowVision) + margin);
	}

	public float AlphaFromGlowDist(Vector2 A, Vector2 B)
	{
		if (spawn.inEggMode > 0f)
		{
			return Mathf.Lerp(Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f, playerGlowVision), 50f, Vector2.Distance(A, B)) * (float)Math.PI), Mathf.InverseLerp(Mathf.Lerp(100f, 400f, playerGlowVision), 50f, Vector2.Distance(A, B)), spawn.inEggMode) * playerGlowVision;
		}
		return Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f, playerGlowVision), 50f, Vector2.Distance(A, B)) * (float)Math.PI) * playerGlowVision;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == GlowSprite)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
			}
			else if (i == EffectSprite && hasOwnGoldEffect)
			{
				rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}
}
