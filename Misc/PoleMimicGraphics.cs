using System;
using RWCustom;
using UnityEngine;

public class PoleMimicGraphics : GraphicsModule
{
	public class PoleMimicRopeGraphics : RopeGraphic
	{
		private PoleMimicGraphics owner;

		public PoleMimicRopeGraphics(PoleMimicGraphics owner)
			: base(owner.leafPairs * 2)
		{
			this.owner = owner;
		}

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, owner.pole.tentacle.FloatBase);
			for (int i = 0; i < owner.pole.tentacle.tChunks.Length; i++)
			{
				for (int j = 1; j < owner.pole.tentacle.tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, owner.pole.tentacle.tChunks[i].rope.GetPosition(j));
				}
			}
			AlignAndConnect(listCount);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			sLeaser.sprites[1].color = new Color(0.003921569f, 0f, 0f);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if ((sLeaser.sprites[0] as TriangleMesh).vertices.Length != segments.Length * 4)
			{
				InitiateSprites(sLeaser, rCam);
			}
			Vector2 vector = owner.pole.rootPos - owner.pole.stickOutDir * 30f;
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float a = 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				float num = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector, vector2) / 3f;
				float f = owner.StemLookLikePole(num, timeStacker);
				float num3 = Mathf.Lerp((i % 2 == 0) ? Mathf.Lerp(4f, 1.5f, num) : Mathf.Lerp(1.4f, 0.75f, num), 2f, Mathf.Pow(f, 0.75f));
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num2 - vector3 * Mathf.Lerp(a, num3, 0.5f) - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * Mathf.Lerp(a, num3, 0.5f) - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * num3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * num3 - camPos);
				float num4 = (1f + rCam.room.lightAngle.magnitude / 10f) * Mathf.Pow(f, 1.8f);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num2 - vector3 * (Mathf.Lerp(a, num3, 0.5f) + num4) - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * (Mathf.Lerp(a, num3, 0.5f) + num4) - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * (num3 + num4) - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * (num3 + num4) - camPos);
				vector = vector2;
				a = num3;
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.pole.room.GetTile(smoothedGoalPos).Solid && !owner.pole.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.pole.room.TileRect(owner.pole.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
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
	}

	private PoleMimicRopeGraphics ropeGraphic;

	public int leafPairs;

	public int decoratedLeafPairs;

	public Vector2[,,] leaves;

	public float[,,] leavesMimic;

	public float lookLikeAPole;

	public float lastLookLikeAPole;

	public Color mimicColor;

	public Color blackColor;

	private float leafSizeFac;

	private float flipPoint = 1f;

	private bool leavesFlip;

	public float reveal;

	public float lastReveal;

	public float revealFac;

	private int firstReveals;

	public StaticSoundLoop soundLoop;

	public PoleMimic pole => base.owner as PoleMimic;

	public float LeafLength(int pair)
	{
		float num = (float)pair / (float)(leafPairs - 1);
		if (num < 0.75f)
		{
			num = Mathf.InverseLerp(0f, 0.6f, num);
			return 4f + Mathf.Lerp(1f - Mathf.Pow(num, 1.2f), Mathf.Sin(Mathf.InverseLerp(0f, 0.75f, Mathf.Pow(num, 0.6f)) * (float)Math.PI), Mathf.Lerp(0.8f, 0.3f, num)) * 42f * leafSizeFac;
		}
		num = Mathf.InverseLerp(0.75f, 1f, num);
		return 4f + Mathf.Lerp(Mathf.Pow(num, 1.5f), 0f, 0f) * 20f * leafSizeFac;
	}

	public float LeafWidth(int pair)
	{
		return Mathf.Lerp(1f, 0.5f, Mathf.Pow((float)pair / (float)(leafPairs - 1), 0.5f)) * leafSizeFac;
	}

	public float LeafPerpFac(int pair)
	{
		float num = (float)pair / (float)(leafPairs - 1);
		return Mathf.Lerp(0.15f + 0.85f * Mathf.Sin(Mathf.InverseLerp(0f, 0.75f, Mathf.Pow(num, 0.2f)) * (float)Math.PI), Mathf.InverseLerp(0.6f, 1f, num), num);
	}

	public float LeafForwardFac(int pair)
	{
		float f = (float)pair / (float)(leafPairs - 1);
		return Mathf.Lerp(-0.2f, 1f - Mathf.Sin(Mathf.Pow(f, 1.8f) * (float)Math.PI), Mathf.Pow(f, 0.3f));
	}

	public float StemLookLikePole(float stemPos, float timeStacker)
	{
		int pair = Custom.IntClamp((int)(stemPos * (float)(leafPairs - 1)), 0, leafPairs - 1);
		return 1f - (1f - LeafLookLikePole(pair, 0, timeStacker)) * (1f - LeafLookLikePole(pair, 1, timeStacker));
	}

	public float LeafLookLikePole(int pair, int side, float timeStacker)
	{
		return Mathf.Lerp(leavesMimic[pair, side, 1], leavesMimic[pair, side, 0], timeStacker);
	}

	public int LeafSprite(int pair, int side)
	{
		return 2 + pair * 2 + side;
	}

	public int LeafDecorationSprite(int pair, int side)
	{
		return 2 + leafPairs * 2 + pair * 2 + side;
	}

	public PoleMimicGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		lookLikeAPole = (pole.DontSpawnInPoleMode ? 0f : 1f);
		lastLookLikeAPole = lookLikeAPole;
		soundLoop = new StaticSoundLoop(SoundID.Pole_Mimic_Movement_LOOP, pole.mainBodyChunk.pos, pole.room, 1f, 1f);
		cullRange = -1f;
	}

	public override void Update()
	{
		base.Update();
		if (ropeGraphic == null)
		{
			return;
		}
		if (lookLikeAPole > 0.9f && reveal == 0f && lastReveal == 0f)
		{
			if (UnityEngine.Random.value < 0.025f && pole.room.game.session is StoryGameSession && !(pole.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.PoleMimicEverSeen && pole.room.ViewedByAnyCamera(pole.firstChunk.pos, 20f))
			{
				firstReveals++;
				if (firstReveals > 5)
				{
					(pole.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.PoleMimicEverSeen = true;
				}
				reveal = 0.01f;
				revealFac = 1f + UnityEngine.Random.value;
			}
			else if (UnityEngine.Random.value < 0.0014285714f)
			{
				reveal = 0.01f;
				revealFac = UnityEngine.Random.value;
			}
		}
		ropeGraphic.Update();
		soundLoop.Update();
		if (lookLikeAPole == 1f)
		{
			soundLoop.volume = 0f;
		}
		else
		{
			soundLoop.pos = pole.firstChunk.pos;
			soundLoop.volume = Mathf.Pow(Mathf.InverseLerp(0f, 11f, Vector2.Distance(pole.tentacle.tChunks[Math.Min(2, pole.tentacle.tChunks.Length - 1)].lastPos, pole.tentacle.tChunks[Math.Min(2, pole.tentacle.tChunks.Length - 1)].pos)), Mathf.Lerp(1f, 1.5f, lookLikeAPole));
			soundLoop.pitch = Custom.LerpMap(Vector2.Distance(pole.tentacle.tChunks[Math.Min(5, pole.tentacle.tChunks.Length - 1)].lastPos, pole.tentacle.tChunks[Math.Min(5, pole.tentacle.tChunks.Length - 1)].pos), 0f, 8f, 0.5f, 1.5f);
		}
		bool flag = true;
		for (int i = 0; i < leafPairs; i++)
		{
			float num = (float)i / (float)(leafPairs - 1);
			int num2 = Custom.IntClamp((int)(num * (float)pole.tentacle.tChunks.Length), 0, pole.tentacle.tChunks.Length - 1);
			Vector2 vector = ropeGraphic.OnTubePos(new Vector2(0f, num), 1f);
			Vector2 vector2 = ropeGraphic.OnTubeDir(num, 1f);
			if (num > 0.42857143f && pole.tentacle.floatGrabDest.HasValue)
			{
				vector2 = Vector3.Slerp(vector2, Custom.DirVec(vector, pole.tentacle.floatGrabDest.Value), Mathf.InverseLerp(0.42857143f, 1f, num) * 0.5f * (1f - pole.mimic));
			}
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			float num3 = LeafLength(i);
			Vector2 b = ropeGraphic.OneDimensionalTubePos(num + num3 / pole.tentacle.CurrentLength(), 1f);
			for (int j = 0; j < 2; j++)
			{
				leavesMimic[i, j, 1] = leavesMimic[i, j, 0];
				leavesMimic[i, j, 0] = Mathf.Lerp(leavesMimic[i, j, 0], leavesMimic[i, j, 2], 0.1f);
				if (UnityEngine.Random.value < 0.1f)
				{
					leavesMimic[i, j, 2] = Custom.LerpAndTick(leavesMimic[i, j, 2], lookLikeAPole, UnityEngine.Random.value * 0.1f, UnityEngine.Random.value / 40f);
				}
				if (flag && leavesMimic[i, j, 2] > 0.5f)
				{
					flag = false;
				}
				Vector2 a = vector + (vector2 * LeafForwardFac(i) + vector3 * ((j == 0) ? (-1f) : 1f) * LeafPerpFac(i)).normalized * num3;
				if (reveal > 0f && UnityEngine.Random.value > num)
				{
					float num4 = Mathf.InverseLerp(0.35f, 0.1f, Mathf.Abs(num - Mathf.Clamp(num, lastReveal, reveal))) * revealFac;
					if (num4 > 0f)
					{
						leavesMimic[i, j, 2] = Mathf.Lerp(leavesMimic[i, j, 2], 0f, num4 * 0.4f * UnityEngine.Random.value);
						leavesMimic[i, j, 0] = Mathf.Lerp(leavesMimic[i, j, 0], 0f, num4 * 0.04f * UnityEngine.Random.value);
					}
					else
					{
						leavesMimic[i, j, 2] = Mathf.Lerp(leavesMimic[i, j, 2], 1f, UnityEngine.Random.value * 0.4f * revealFac);
						leavesMimic[i, j, 0] = Mathf.Lerp(leavesMimic[i, j, 0], leavesMimic[i, j, 2], UnityEngine.Random.value * revealFac);
					}
				}
				a = Vector2.Lerp(a, b, leavesMimic[i, j, 0]);
				leaves[i, j, 1] = leaves[i, j, 0];
				leaves[i, j, 0] += leaves[i, j, 2];
				leaves[i, j, 2] *= 0.75f * (1f - Mathf.Pow(leavesMimic[i, j, 2], 3f));
				leaves[i, j, 2].y -= 0.3f;
				leaves[i, j, 0] = Vector2.Lerp(leaves[i, j, 0], b, Mathf.Pow(leavesMimic[i, j, 2], 3f));
				if (!Custom.DistLess(a, leaves[i, j, 0], num3 * 0.5f))
				{
					a = vector + Custom.DirVec(vector, a) * num3 * 0.5f;
				}
				if (pole.stickChunks[num2] != null)
				{
					leaves[i, j, 0] = pole.stickChunks[num2].pos + Custom.DirVec(pole.stickChunks[num2].pos, leaves[i, j, 0]) * pole.stickChunks[num2].rad;
					leaves[i, j, 2] *= 0f;
				}
				leaves[i, j, 2] += (a - leaves[i, j, 0]) / 4f;
				leavesMimic[i, j, 4] = leavesMimic[i, j, 3];
				if (UnityEngine.Random.value < 1f / 3f)
				{
					leavesMimic[i, j, 3] = Custom.LerpAndTick(leavesMimic[i, j, 3], (num < flipPoint == leavesFlip) ? 1f : (-1f), 0.1f * (1f - leavesMimic[i, j, 2]), 0.25f);
				}
			}
		}
		if ((flipPoint > 0f || leavesFlip) && lookLikeAPole < 0.6f && flag)
		{
			flipPoint += 1f / (3.5f * Mathf.Lerp(leafPairs, 10f, 0.6f));
		}
		else if ((flipPoint > 0f || !leavesFlip) && lookLikeAPole > 0.4f)
		{
			flipPoint += 1f / (10f * Mathf.Lerp(leafPairs, 10f, 0.3f));
		}
		if (flipPoint >= 1f)
		{
			flipPoint = 0f;
			leavesFlip = !leavesFlip;
		}
		lastLookLikeAPole = lookLikeAPole;
		lastReveal = reveal;
		if (reveal > 0f)
		{
			reveal += 1f / (Mathf.Lerp(15f, pole.tentacle.segments.Count, 0.5f) * Mathf.Lerp(2.5f, 1.5f, revealFac));
			lookLikeAPole = Custom.LerpAndTick(lookLikeAPole, 0.9f, 0.04f, 1f / 60f);
			if (reveal >= 1f)
			{
				reveal = 0f;
				lastReveal = 0f;
			}
		}
		else
		{
			lookLikeAPole = Custom.LerpAndTick(lookLikeAPole, pole.mimic, 0.03f, 1f / 30f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(pole.abstractCreature.ID.RandomSeed);
		leafPairs = (int)(pole.tentacle.idealLength / 8f);
		decoratedLeafPairs = Custom.IntClamp((int)((float)leafPairs * 0.6f), 1, 80);
		leafSizeFac = Custom.LerpMap(Mathf.Lerp(leafPairs, 65f, 0.1f), 5f, 185f, 0.5f, 2f);
		UnityEngine.Random.state = state;
		ropeGraphic = new PoleMimicRopeGraphics(this);
		sLeaser.sprites = new FSprite[2 + leafPairs * 2 + decoratedLeafPairs * 2];
		leaves = new Vector2[leafPairs, 2, 3];
		leavesMimic = new float[leafPairs, 2, 5];
		for (int i = 0; i < leafPairs; i++)
		{
			float num = (float)i / (float)(leafPairs - 1);
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[LeafSprite(i, j)] = new FSprite((num < 0.75f) ? "LizardScaleA3" : "LizardScaleA0");
				sLeaser.sprites[LeafSprite(i, j)].anchorY = 0f;
				leaves[i, j, 0] = pole.bodyChunks[1].pos;
				leaves[i, j, 2] = leaves[i, j, 0];
				for (int k = 0; k < 5; k++)
				{
					leavesMimic[i, j, k] = (pole.DontSpawnInPoleMode ? ((k > 2) ? (-1f) : 0f) : 1f);
				}
				if (i < decoratedLeafPairs)
				{
					sLeaser.sprites[LeafDecorationSprite(i, j)] = new FSprite((num < 0.75f) ? "LizardScaleB3" : "LizardScaleB0");
					sLeaser.sprites[LeafDecorationSprite(i, j)].anchorY = 0f;
				}
			}
		}
		ropeGraphic.InitiateSprites(sLeaser, rCam);
		ropeGraphic.Update();
		if (pole.DontSpawnInPoleMode)
		{
			lookLikeAPole = 0f;
			lastLookLikeAPole = 0f;
			for (int l = 0; l < leafPairs; l++)
			{
				for (int m = 0; m < 2; m++)
				{
					leaves[l, m, 0] = pole.rootPos;
					leaves[l, m, 1] = pole.rootPos;
					leaves[l, m, 2] *= 0f;
				}
			}
		}
		else
		{
			for (int n = 0; n < ropeGraphic.segments.Length; n++)
			{
				ropeGraphic.segments[n].pos = pole.tentacle.tChunks[Custom.IntClamp(n, 0, pole.tentacle.tChunks.Length - 1)].pos;
				ropeGraphic.segments[n].lastPos = ropeGraphic.segments[Custom.IntClamp(n, 0, pole.tentacle.tChunks.Length - 1)].pos;
			}
			ropeGraphic.segments[ropeGraphic.segments.Length - 1].pos = pole.tipPos;
			ropeGraphic.segments[ropeGraphic.segments.Length - 1].lastPos = pole.tipPos;
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[1]);
		for (int i = 2; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		ropeGraphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		Color color = Color.Lerp(blackColor, mimicColor, Mathf.Lerp(lastLookLikeAPole, lookLikeAPole, timeStacker));
		sLeaser.sprites[0].color = color;
		for (int i = 0; i < leafPairs; i++)
		{
			float num = (float)i / (float)(leafPairs - 1);
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector = ropeGraphic.OnTubePos(new Vector2((j == 0) ? (-1f) : 1f, num), timeStacker);
				Vector2 vector2 = Vector2.Lerp(leaves[i, j, 1], leaves[i, j, 0], timeStacker);
				sLeaser.sprites[LeafSprite(i, j)].x = vector.x - camPos.x;
				sLeaser.sprites[LeafSprite(i, j)].y = vector.y - camPos.y;
				sLeaser.sprites[LeafSprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				sLeaser.sprites[LeafSprite(i, j)].scaleY = Vector2.Distance(vector, vector2) / sLeaser.sprites[LeafSprite(i, j)].element.sourceSize.y;
				float num2 = 0f - Mathf.Lerp(leavesMimic[i, j, 4], leavesMimic[i, j, 3], timeStacker);
				if (num >= 0.75f)
				{
					num2 = Mathf.Abs(num2);
				}
				float scaleX = ((j == 0) ? (-1f) : 1f) * LeafWidth(i) * Mathf.Pow(Mathf.InverseLerp(1f, 0.6f, LeafLookLikePole(i, j, timeStacker)), 0.5f) * num2;
				sLeaser.sprites[LeafSprite(i, j)].scaleX = scaleX;
				sLeaser.sprites[LeafSprite(i, j)].color = color;
				if (i < decoratedLeafPairs)
				{
					sLeaser.sprites[LeafDecorationSprite(i, j)].x = vector.x - camPos.x;
					sLeaser.sprites[LeafDecorationSprite(i, j)].y = vector.y - camPos.y;
					sLeaser.sprites[LeafDecorationSprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
					sLeaser.sprites[LeafDecorationSprite(i, j)].scaleY = Vector2.Distance(vector, vector2) / sLeaser.sprites[LeafDecorationSprite(i, j)].element.sourceSize.y;
					sLeaser.sprites[LeafDecorationSprite(i, j)].scaleX = scaleX;
					sLeaser.sprites[LeafDecorationSprite(i, j)].color = Color.Lerp(new Color(1f, 0f, 0f), color, Mathf.Pow(Mathf.InverseLerp(decoratedLeafPairs / 2, decoratedLeafPairs, i), 0.6f));
					sLeaser.sprites[LeafDecorationSprite(i, j)].alpha = Mathf.Pow(1f - StemLookLikePole(num, timeStacker), 0.2f) * ((num2 > 0f) ? (1f / 3f) : 1f);
				}
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		mimicColor = Color.Lerp(palette.texture.GetPixel(4, 3), palette.fogColor, palette.fogAmount * (2f / 15f));
		blackColor = palette.blackColor;
	}
}
