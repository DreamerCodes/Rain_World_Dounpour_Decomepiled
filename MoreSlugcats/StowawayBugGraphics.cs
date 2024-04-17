using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class StowawayBugGraphics : GraphicsModule, ILookingAtCreatures
{
	public class StowawayRopeGraphics : RopeGraphic
	{
		private int headNumber;

		private StowawayBugGraphics owner;

		private int spriteOffset;

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, owner.myBug.heads[headNumber].FloatBase);
			for (int i = 0; i < owner.myBug.heads[headNumber].tChunks.Length; i++)
			{
				for (int j = 1; j < owner.myBug.heads[headNumber].tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, owner.myBug.heads[headNumber].tChunks[i].rope.GetPosition(j));
				}
			}
			AlignAndConnect(listCount);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = owner.myBug.mainBodyChunk.pos;
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float a = owner.RadOfSegment(0f, timeStacker) * 1.7f + 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				float f = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				if (i >= segments.Length - 1)
				{
					vector2 += Custom.DirVec(vector, vector2) * 1f;
				}
				else
				{
					vector2 = Vector2.Lerp(segments[i + 1].lastPos, segments[i + 1].pos, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				float num = owner.RadOfSegment(f, timeStacker) * 1.7f + 2f;
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[spriteOffset] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
				a = num;
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.myBug.room.GetTile(smoothedGoalPos).Solid && !owner.myBug.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.myBug.room.TileRect(owner.myBug.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)segments.Length, timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OnTubeDir(float floatPos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(floatPos - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(floatPos + 1f / (float)segments.Length, timeStacker);
			return Custom.DirVec(p, p2);
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int sproffset)
		{
			spriteOffset = sproffset;
			sLeaser.sprites[spriteOffset] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: true);
		}

		public StowawayRopeGraphics(StowawayBugGraphics owner, int head)
			: base(40)
		{
			this.owner = owner;
			headNumber = head;
		}
	}

	private Vector2 lookDir;

	public CreatureLooker creatureLooker;

	public Color bodyColor;

	private StowawayRopeGraphics[] ropeGraphics;

	public float mouthOpen;

	private float mouthExtension;

	public float biting;

	private bool biteIsKiller;

	public float digestPrey;

	private float overShoot;

	private float overShootCounter;

	private float overShootScale;

	private Vector2[] bodyOrbs;

	private float[] orbRadius;

	public StowawayBug myBug => base.owner as StowawayBug;

	public int TotalSprites => 2 + myBug.tentacles.Length + SpritesTotal_heads + SpritesTotal_Mouth + SpritesTotal_Hood + SpritesTotal_Feelers + SpritesTotal_Mass;

	public int TentaclesStart => 2;

	public int BodyStart => 0;

	public int SpritesBegin_heads => 2 + myBug.tentacles.Length;

	public int SpritesTotal_heads
	{
		get
		{
			int i = 0;
			int num = 0;
			for (; i < myBug.heads.Length; i++)
			{
				num += SpritesTotal_singlehead();
			}
			return num;
		}
	}

	public int SpritesTotal_Mouth => myBug.teethCount * 2;

	public int SpritesTotal_Hood => 4;

	public int SpritesBegin_Mouth => 2 + myBug.tentacles.Length + SpritesTotal_heads;

	public int SpritesBegin_Hood => 2 + myBug.tentacles.Length + SpritesTotal_heads + SpritesTotal_Mouth;

	public int SpritesTotal_Mass => bodyOrbs.Length;

	public int SpritesBegin_Mass => 2 + myBug.tentacles.Length + SpritesTotal_heads + SpritesTotal_Mouth + SpritesTotal_Hood + SpritesTotal_Feelers;

	public int SpritesTotal_Feelers => bodyOrbs.Length;

	public int SpritesBegin_Feelers => 2 + myBug.tentacles.Length + SpritesTotal_heads + SpritesTotal_Mouth + SpritesTotal_Hood;

	public StowawayBugGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		creatureLooker = new CreatureLooker(this, (ow as StowawayBug).AI.tracker, ow as StowawayBug, 0.4f, 20);
		ropeGraphics = new StowawayRopeGraphics[(ow as StowawayBug).heads.Length];
		for (int i = 0; i < (ow as StowawayBug).heads.Length; i++)
		{
			ropeGraphics[i] = new StowawayRopeGraphics(this, i);
		}
		mouthOpen = 0f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((ow as StowawayBug).abstractCreature.ID.RandomSeed);
		int num = UnityEngine.Random.Range(5, 9);
		bodyOrbs = new Vector2[num];
		orbRadius = new float[num];
		bodyParts = new BodyPart[num];
		for (int j = 0; j < num; j++)
		{
			Vector2 vector = (ow as StowawayBug).placedDirection + Custom.PerpendicularVector((ow as StowawayBug).placedDirection) * (UnityEngine.Random.value * 10f + Mathf.Lerp(0f, 25f, (float)j / (float)num)) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			bodyOrbs[j] = vector + (ow as StowawayBug).placedDirection * Mathf.Lerp(-5f, -19f, (float)j / (float)num);
			orbRadius[j] = UnityEngine.Random.Range(8, 12);
			bodyParts[j] = new BodyPart(this);
		}
		UnityEngine.Random.state = state;
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		for (int i = 0; i < myBug.heads.Length; i++)
		{
			ropeGraphics[i].Update();
		}
		if (digestPrey > 0f)
		{
			digestPrey += 0.001f;
		}
		if (digestPrey > 1f)
		{
			digestPrey = 0f;
		}
		if (!myBug.Consious)
		{
			mouthOpen = Mathf.Lerp(mouthOpen, 0.45f, 0.03f);
		}
		else if (myBug.mawOpen || digestPrey > 0f)
		{
			mouthOpen += 0.05f;
		}
		else
		{
			mouthOpen -= 0.01f;
		}
		mouthOpen = Mathf.Clamp(mouthOpen, 0f, 1f);
		if (biting > 0f)
		{
			biting += 0.15f;
		}
		if (biting > 2f && digestPrey == 0f)
		{
			biting = 0f;
			biteIsKiller = false;
		}
		if (digestPrey > 0f)
		{
			biting = 1f;
		}
		if (biting > 1f && biting < 1.3f && overShootCounter == 0f)
		{
			overShootCounter = 0.01f;
			overShootScale = UnityEngine.Random.value * 12f;
		}
		if (overShootCounter > 0f)
		{
			overShootCounter += 0.09f;
			overShoot = Mathf.Sin(overShootCounter * (float)Math.PI) * overShootScale;
		}
		if (overShootCounter >= 1f)
		{
			overShootCounter = 0f;
		}
		mouthExtension = Mathf.Sin(biting * ((float)Math.PI / 2f)) * mouthOpen;
		for (int j = 0; j < bodyParts.Length; j++)
		{
			BodyPart bodyPart = bodyParts[j];
			bodyPart.vel.y = bodyPart.vel.y - myBug.gravity;
			bodyParts[j].lastPos = bodyParts[j].pos;
			bodyParts[j].pos += bodyParts[j].vel;
			Vector2 vector = bodyOrbs[j].normalized * 5f * (mouthOpen - mouthExtension);
			bodyParts[j].ConnectToPoint(myBug.originalPos + vector + bodyOrbs[j], 20f, push: false, 0.1f, default(Vector2), 1f, 0f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[0] = new FSprite("mouseBodyA");
		sLeaser.sprites[0].isVisible = false;
		sLeaser.sprites[1] = new FSprite("mouseBodyB");
		sLeaser.sprites[1].anchorY = 0.1f;
		for (int i = 0; i < myBug.tentacles.Length; i++)
		{
			sLeaser.sprites[TentaclesStart + i] = TriangleMesh.MakeLongMesh(myBug.tentacles[i].GetLength(0), pointyTip: false, customColor: true);
		}
		int num = 0;
		for (int j = 0; j < myBug.heads.Length; j++)
		{
			ropeGraphics[j].InitiateSprites(sLeaser, rCam, SpritesBegin_heads + num);
			sLeaser.sprites[SpritesBegin_heads + num + 1] = new FSprite("TrapHook");
			sLeaser.sprites[SpritesBegin_heads + num + 1].anchorY = 0.1f;
			sLeaser.sprites[SpritesBegin_heads + num + 1].scale = 0.6f;
			sLeaser.sprites[SpritesBegin_heads + num + 1].scaleY = 1.2f;
			sLeaser.sprites[SpritesBegin_heads + num + 2] = new FSprite("mouseBodyA");
			sLeaser.sprites[SpritesBegin_heads + num + 2].scale = 0.6f;
			num += SpritesTotal_singlehead();
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(myBug.teethSeed);
		for (int k = 0; k < myBug.teethCount * 2; k += 2)
		{
			float num2 = ((!((float)k < (float)myBug.teethCount / 2f)) ? Mathf.InverseLerp(myBug.teethCount, (float)myBug.teethCount / 2f, k) : Mathf.InverseLerp(0f, (float)myBug.teethCount / 2f, k));
			sLeaser.sprites[SpritesBegin_Mouth + k] = new FSprite("SpiderLeg0A");
			sLeaser.sprites[SpritesBegin_Mouth + k].anchorY = 0.1f;
			sLeaser.sprites[SpritesBegin_Mouth + k].scaleX = 0.4f;
			sLeaser.sprites[SpritesBegin_Mouth + k].scaleY = UnityEngine.Random.Range(0.8f, 1.2f) + (1f - num2) / 20f;
			sLeaser.sprites[SpritesBegin_Mouth + k + 1] = new FSprite("SpiderLeg0B");
			sLeaser.sprites[SpritesBegin_Mouth + k + 1].anchorY = 0.1f;
			sLeaser.sprites[SpritesBegin_Mouth + k + 1].scaleX = 1f;
			sLeaser.sprites[SpritesBegin_Mouth + k + 1].scaleY = sLeaser.sprites[SpritesBegin_Mouth + k].scaleY;
		}
		sLeaser.sprites[SpritesBegin_Hood] = TriangleMesh.MakeLongMesh(10, pointyTip: false, customColor: true);
		sLeaser.sprites[SpritesBegin_Hood + 1] = TriangleMesh.MakeLongMesh(10, pointyTip: false, customColor: true);
		sLeaser.sprites[SpritesBegin_Hood + 1].shader = rCam.room.game.rainWorld.Shaders["TubeWorm"];
		sLeaser.sprites[SpritesBegin_Hood + 2] = new FSprite("DangleFruit0A");
		sLeaser.sprites[SpritesBegin_Hood + 3] = new FSprite("DangleFruit0A");
		for (int l = 0; l < bodyOrbs.Length; l++)
		{
			sLeaser.sprites[SpritesBegin_Feelers + l] = new FSprite("LizardScaleA6");
			sLeaser.sprites[SpritesBegin_Feelers + l].rotation = 180f;
			sLeaser.sprites[SpritesBegin_Feelers + l].scale = 1.1f;
			sLeaser.sprites[SpritesBegin_Feelers + l].scaleY = 1.5f;
			sLeaser.sprites[SpritesBegin_Feelers + l].anchorY = 0f;
			sLeaser.sprites[SpritesBegin_Mass + l] = new FSprite("KrakenShield0");
			sLeaser.sprites[SpritesBegin_Mass + l].rotation = Custom.VecToDeg(Custom.RNV());
			sLeaser.sprites[SpritesBegin_Mass + l].scale = orbRadius[l] / 7f;
		}
		UnityEngine.Random.state = state;
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(myBug.firstChunk.lastPos, myBug.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(myBug.bodyChunks[1].lastPos, myBug.bodyChunks[1].pos, timeStacker);
		Vector2 vector3 = Vector2.Lerp(Custom.DirVec(myBug.bodyChunks[0].lastPos, myBug.bodyChunks[1].lastPos), Custom.DirVec(myBug.bodyChunks[0].pos, myBug.bodyChunks[1].pos), timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[1].x = vector2.x - camPos.x;
		sLeaser.sprites[1].y = vector2.y - camPos.y;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(vector3);
		for (int i = 0; i < myBug.tentacles.Length; i++)
		{
			float num = 0f;
			Vector2 vector4 = myBug.AttachPos(i, timeStacker);
			for (int j = 0; j < myBug.tentacles[i].GetLength(0); j++)
			{
				Vector2 vector5 = Vector2.Lerp(myBug.tentacles[i][j, 1], myBug.tentacles[i][j, 0], timeStacker);
				float num2 = 0.5f;
				Vector2 normalized = (vector4 - vector5).normalized;
				Vector2 vector6 = Custom.PerpendicularVector(normalized);
				float num3 = Vector2.Distance(vector4, vector5) / 5f;
				(sLeaser.sprites[TentaclesStart + i] as TriangleMesh).MoveVertice(j * 4, vector4 - normalized * num3 - vector6 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[TentaclesStart + i] as TriangleMesh).MoveVertice(j * 4 + 1, vector4 - normalized * num3 + vector6 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[TentaclesStart + i] as TriangleMesh).MoveVertice(j * 4 + 2, vector5 + normalized * num3 - vector6 * num2 - camPos);
				(sLeaser.sprites[TentaclesStart + i] as TriangleMesh).MoveVertice(j * 4 + 3, vector5 + normalized * num3 + vector6 * num2 - camPos);
				vector4 = vector5;
				num = num2;
			}
		}
		for (int k = 0; k < myBug.heads.Length; k++)
		{
			Vector2 lastPos = myBug.heads[k].Tip.lastPos;
			Vector2 a = Custom.DirVec(myBug.heads[k].tChunks[myBug.heads[k].tChunks.Length - 2].lastPos, myBug.heads[k].Tip.lastPos);
			a = Vector2.Lerp(a, myBug.placedDirection, Mathf.Pow(myBug.heads[k].retractFac, 6f));
			Vector2 vector7 = myBug.heads[k].Tip.lastPos - a * 5f;
			Vector2 v = Custom.DirVec(myBug.heads[k].tChunks[myBug.heads[k].tChunks.Length - 3].lastPos, myBug.heads[k].tChunks[myBug.heads[k].tChunks.Length - 1].lastPos);
			ropeGraphics[k].DrawSprite(sLeaser, rCam, 0f, camPos);
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 1].x = lastPos.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 1].y = lastPos.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 1].rotation = Custom.VecToDeg(a);
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 2].x = vector7.x - camPos.x;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 2].y = vector7.y - camPos.y;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 2].rotation = Custom.VecToDeg(v);
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k].isVisible = myBug.heads[k].retractFac < 0.95f;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 1].isVisible = myBug.heads[k].retractFac < 0.95f;
			sLeaser.sprites[SpritesBegin_heads + SpritesTotal_singlehead() * k + 2].isVisible = myBug.heads[k].retractFac < 0.95f;
		}
		float b = (biteIsKiller ? 47f : 35f) * (1f - myBug.sleepScale) + overShoot;
		Vector2 vector8 = Custom.PerpendicularVector(vector3);
		float num4 = Mathf.Lerp(6f, biteIsKiller ? 25f : 15f, mouthOpen - mouthExtension);
		float num5 = num4 * 0.95f;
		Vector2 vector9 = Vector2.Lerp(vector, vector2 + vector3 * Mathf.Lerp(10f + overShoot, b, mouthExtension), mouthOpen * (1f - myBug.sleepScale));
		for (int l = 0; l < myBug.teethCount; l++)
		{
			float num6 = Mathf.Lerp(24f, 0f, mouthOpen - mouthExtension);
			float num7 = ((!((float)l < (float)myBug.teethCount / 2f)) ? Mathf.InverseLerp(myBug.teethCount, (float)myBug.teethCount / 2f, l) : Mathf.InverseLerp(0f, (float)myBug.teethCount / 2f, l));
			Vector2 vector10 = vector8 * num5 - vector8 * Mathf.Lerp(0f, num5 * 2f, (float)l / ((float)myBug.teethCount - 1f));
			sLeaser.sprites[SpritesBegin_Mouth + l * 2].x = vector9.x + vector10.x - camPos.x;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2].y = vector9.y + vector10.y - camPos.y;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2].scaleX = (((float)l < (float)myBug.teethCount / 2f) ? 0.4f : (-0.4f));
			sLeaser.sprites[SpritesBegin_Mouth + l * 2 + 1].x = vector9.x + vector10.x - camPos.x;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2 + 1].y = vector9.y + vector10.y - camPos.y;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2 + 1].scaleX = (((float)l < (float)myBug.teethCount / 2f) ? 1f : (-1f));
			float num8 = (1f - num7) * Mathf.Lerp(-10f + num6, -51f, mouthOpen - mouthExtension) * Mathf.Sign(sLeaser.sprites[SpritesBegin_Mouth + l * 2].scaleX);
			sLeaser.sprites[SpritesBegin_Mouth + l * 2].rotation = Custom.VecToDeg(vector3) + num8 + 2.5f;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2 + 1].rotation = sLeaser.sprites[SpritesBegin_Mouth + l * 2].rotation;
			bool isVisible = mouthOpen * (1f - myBug.sleepScale) > 0f;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2].isVisible = isVisible;
			sLeaser.sprites[SpritesBegin_Mouth + l * 2 + 1].isVisible = isVisible;
		}
		int num9 = 10;
		Vector2 vector11 = vector - myBug.placedDirection * 20f;
		Vector2 vector12 = vector11;
		Vector2 vector13 = default(Vector2);
		Vector2 vector14 = Custom.PerpendicularVector(myBug.placedDirection);
		for (int m = 0; m < num9; m++)
		{
			float num10 = (float)m / ((float)num9 - 1f);
			Vector2 vector15 = Vector2.Lerp(vector11, vector2 + vector3 * Mathf.Lerp(10f, b, mouthExtension) + vector3 * Mathf.Lerp(-5f, 4f, mouthOpen), num10);
			float num11 = num4 * (Mathf.Sin(num10 * 5f) / 1.4f + Mathf.Pow(num10, 2f) * 1.4f + mouthOpen * Mathf.Pow(num10, 2f)) + 0.3f;
			num11 *= 1f - num10 / 2f;
			num11 += Mathf.Lerp(8f, 18f, mouthOpen);
			Vector2 b2 = vector8 * num11;
			num11 = Mathf.Lerp(42f, num11, num10 * 2f);
			b2 = Vector2.Lerp(vector14 * num11, b2, num10);
			(sLeaser.sprites[SpritesBegin_Hood] as TriangleMesh).MoveVertice(m * 4, vector12 - vector13 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood] as TriangleMesh).MoveVertice(m * 4 + 1, vector12 + vector13 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood] as TriangleMesh).MoveVertice(m * 4 + 2, vector15 - b2 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood] as TriangleMesh).MoveVertice(m * 4 + 3, vector15 + b2 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood + 1] as TriangleMesh).MoveVertice(m * 4, vector12 - vector13 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood + 1] as TriangleMesh).MoveVertice(m * 4 + 1, vector12 + vector13 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood + 1] as TriangleMesh).MoveVertice(m * 4 + 2, vector15 - b2 - camPos);
			(sLeaser.sprites[SpritesBegin_Hood + 1] as TriangleMesh).MoveVertice(m * 4 + 3, vector15 + b2 - camPos);
			vector12 = vector15;
			vector13 = b2;
		}
		vector9 = vector2 + vector3 * Mathf.Lerp(10f + overShoot, b, mouthExtension) + vector3 * Mathf.Lerp(-5f, 4f, mouthOpen);
		float num12 = Mathf.Lerp(2f, 16f, mouthOpen - mouthExtension);
		sLeaser.sprites[SpritesBegin_Hood + 2].x = vector9.x + vector8.x * (0f - num12) - camPos.x;
		sLeaser.sprites[SpritesBegin_Hood + 2].y = vector9.y + vector8.y * (0f - num12) - camPos.y;
		sLeaser.sprites[SpritesBegin_Hood + 2].scaleX = Mathf.Lerp(0.6f, 0.5f, mouthOpen - mouthExtension);
		sLeaser.sprites[SpritesBegin_Hood + 2].scaleY = Mathf.Lerp(1.3f, 1.9f, mouthOpen - mouthExtension);
		sLeaser.sprites[SpritesBegin_Hood + 2].rotation = Custom.VecToDeg(vector3) - 100f;
		sLeaser.sprites[SpritesBegin_Hood + 3].x = vector9.x + vector8.x * num12 - camPos.x;
		sLeaser.sprites[SpritesBegin_Hood + 3].y = vector9.y + vector8.y * num12 - camPos.y;
		sLeaser.sprites[SpritesBegin_Hood + 3].scaleX = Mathf.Lerp(0.6f, 0.5f, mouthOpen - mouthExtension);
		sLeaser.sprites[SpritesBegin_Hood + 3].scaleY = Mathf.Lerp(1.3f, 1.9f, mouthOpen - mouthExtension);
		sLeaser.sprites[SpritesBegin_Hood + 3].rotation = Custom.VecToDeg(vector3) + 180f - 80f;
		for (int n = 0; n < bodyOrbs.Length; n++)
		{
			Vector2 vector16 = bodyOrbs[n].normalized * 5f * (mouthOpen - mouthExtension);
			Vector2 placedDirection = myBug.placedDirection;
			sLeaser.sprites[SpritesBegin_Feelers + n].x = myBug.originalPos.x + bodyOrbs[n].x + placedDirection.x * orbRadius[n] - camPos.x;
			sLeaser.sprites[SpritesBegin_Feelers + n].y = myBug.originalPos.y + bodyOrbs[n].y + placedDirection.y * orbRadius[n] - camPos.y;
			sLeaser.sprites[SpritesBegin_Feelers + n].rotation = Custom.VecToDeg(placedDirection);
			sLeaser.sprites[SpritesBegin_Mass + n].x = myBug.originalPos.x + vector16.x + bodyOrbs[n].x - camPos.x;
			sLeaser.sprites[SpritesBegin_Mass + n].y = myBug.originalPos.y + vector16.y + bodyOrbs[n].y - camPos.y;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color blackColor = palette.blackColor;
		Color pixel = palette.texture.GetPixel(12, 9);
		Color color = Color.Lerp(palette.texture.GetPixel(12, 2), blackColor, palette.darkness);
		Color color2 = Color.Lerp(palette.texture.GetPixel(5, 0), blackColor, palette.darkness);
		Color color3 = rCam.PixelColorAtCoordinate(myBug.colorPickPos);
		bodyColor = palette.blackColor;
		sLeaser.sprites[0].color = Color.white;
		sLeaser.sprites[1].color = color3;
		for (int i = 0; i < myBug.tentacles.Length; i++)
		{
			for (int j = 0; j < (sLeaser.sprites[TentaclesStart + i] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[TentaclesStart + i] as TriangleMesh).verticeColors[j] = Color.Lerp(color3, color, 1f - Mathf.Pow((float)j / (float)((sLeaser.sprites[TentaclesStart + i] as TriangleMesh).verticeColors.Length - 1), 0.06f));
			}
		}
		for (int k = 0; k < myBug.heads.Length; k++)
		{
		}
		int num = 0;
		for (int l = 0; l < myBug.heads.Length; l++)
		{
			for (int m = 0; m < (sLeaser.sprites[SpritesBegin_heads + num] as TriangleMesh).verticeColors.Length; m++)
			{
				(sLeaser.sprites[SpritesBegin_heads + num] as TriangleMesh).verticeColors[m] = Color.Lerp(color3, pixel, (float)m / (float)((sLeaser.sprites[SpritesBegin_heads + num] as TriangleMesh).verticeColors.Length - 1));
			}
			sLeaser.sprites[SpritesBegin_heads + num + 1].color = pixel;
			sLeaser.sprites[SpritesBegin_heads + num + 2].color = pixel;
			num += SpritesTotal_singlehead();
		}
		for (int n = 0; n < myBug.teethCount * 2; n += 2)
		{
			sLeaser.sprites[SpritesBegin_Mouth + n].color = color;
			sLeaser.sprites[SpritesBegin_Mouth + n + 1].color = pixel;
		}
		sLeaser.sprites[SpritesBegin_Hood].color = color;
		sLeaser.sprites[SpritesBegin_Hood + 1].color = color2;
		sLeaser.sprites[SpritesBegin_Hood + 2].color = color;
		sLeaser.sprites[SpritesBegin_Hood + 3].color = color;
		for (int num2 = 0; num2 < bodyOrbs.Length; num2++)
		{
			sLeaser.sprites[SpritesBegin_Feelers + num2].color = Color.Lerp(color3, color2, (float)num2 / (float)bodyOrbs.Length);
			sLeaser.sprites[SpritesBegin_Mass + num2].color = Color.Lerp(color, color2, (float)num2 / (float)bodyOrbs.Length);
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		for (int i = 0; i < TotalSprites; i++)
		{
			if (i >= SpritesBegin_Hood)
			{
				newContatiner = rCam.ReturnFContainer("Items");
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else if (i >= SpritesBegin_heads && i < SpritesBegin_heads + SpritesTotal_singlehead() * myBug.heads.Length)
			{
				newContatiner = rCam.ReturnFContainer("Background");
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner = rCam.ReturnFContainer("Midground");
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return myBug.AI.focusCreature;
	}

	public void LookAtNothing()
	{
	}

	private float RadOfSegment(float f, float timeStacker)
	{
		return myBug.Rad(f);
	}

	public int SpritesTotal_singlehead()
	{
		return 3;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < myBug.heads.Length; i++)
		{
			ropeGraphics[i].AddToPositionsList(0, myBug.mainBodyChunk.pos);
			ropeGraphics[i].AddToPositionsList(1, myBug.heads[i].Tip.pos);
			ropeGraphics[i].AlignAndConnect(2);
		}
		for (int j = 0; j < bodyParts.Length; j++)
		{
			bodyParts[j].Reset(bodyOrbs[j] + new Vector2(0f, -40f));
		}
	}

	public bool Bite()
	{
		if (biting == 0f && myBug.mawOpen)
		{
			for (int i = 0; i < bodyParts.Length; i++)
			{
				bodyParts[i].vel += Custom.RNV() * 8f;
			}
			biting = 0.1f;
			return true;
		}
		return false;
	}

	public bool KillerBite()
	{
		if (biting == 0f && myBug.mawOpen)
		{
			for (int i = 0; i < bodyParts.Length; i++)
			{
				bodyParts[i].vel += Custom.RNV() * 11f;
			}
			biting = 0.1f;
			biteIsKiller = true;
			return true;
		}
		return false;
	}
}
