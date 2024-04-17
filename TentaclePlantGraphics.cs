using System;
using RWCustom;
using UnityEngine;

public class TentaclePlantGraphics : GraphicsModule, HasDanglers
{
	public class TentaclePlantRopeGraphics : RopeGraphic
	{
		private TentaclePlantGraphics owner;

		public TentaclePlantRopeGraphics(TentaclePlantGraphics owner)
			: base(40)
		{
			this.owner = owner;
		}

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, owner.plant.tentacle.FloatBase);
			for (int i = 0; i < owner.plant.tentacle.tChunks.Length; i++)
			{
				for (int j = 1; j < owner.plant.tentacle.tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, owner.plant.tentacle.tChunks[i].rope.GetPosition(j));
				}
			}
			AlignAndConnect(listCount);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["JaggedSquare"];
			sLeaser.sprites[0].alpha = 0.7f + 0.1f * UnityEngine.Random.value;
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = owner.plant.rootPos - owner.plant.stickOutDir * 30f;
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float a = owner.plant.Rad(0f) * 1.7f + 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				float f = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				float num = owner.plant.Rad(f) * 1.7f + 2f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * Mathf.Lerp(a, num, 0.5f) - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
				a = num;
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.plant.room.GetTile(smoothedGoalPos).Solid && !owner.plant.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.plant.room.TileRect(owner.plant.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
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

	private TentaclePlantRopeGraphics ropeGraphic;

	public Dangler[] danglers;

	public float[,] danglerProps;

	public Dangler.DanglerProps danglerVals;

	public TentaclePlant plant => base.owner as TentaclePlant;

	public TentaclePlantGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		ropeGraphic = new TentaclePlantRopeGraphics(this);
		danglers = new Dangler[50];
		danglerProps = new float[danglers.Length, 3];
		danglerVals = new Dangler.DanglerProps();
		cullRange = 1400f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(plant.abstractCreature.ID.RandomSeed);
		for (int i = 0; i < danglers.Length; i++)
		{
			if (i < 15)
			{
				danglers[i] = new Dangler(this, i, UnityEngine.Random.Range(4, 12), 5f, 5f);
			}
			else
			{
				danglers[i] = new Dangler(this, i, UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, UnityEngine.Random.Range(4, UnityEngine.Random.Range(4, 12)))), 5f, 5f);
			}
			danglerProps[i, 0] = Mathf.Pow(UnityEngine.Random.value, 0.6f);
			danglerProps[i, 1] = Mathf.Lerp(0f - plant.Rad(danglerProps[i, 0]), plant.Rad(danglerProps[i, 0]), UnityEngine.Random.value);
			danglerProps[i, 2] = UnityEngine.Random.value;
			float num = Mathf.Lerp(plant.Rad(danglerProps[i, 0]), 4f, 0.5f) * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
			float num2 = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
			for (int j = 0; j < danglers[i].segments.Length; j++)
			{
				float num3 = (float)j / (float)(danglers[i].segments.Length - 1);
				danglers[i].segments[j].rad = Mathf.Lerp(Mathf.Lerp(1f, 0.5f, Mathf.Pow(num3, 0.7f)), 0.5f + Mathf.Sin(Mathf.Pow(num3, 2.5f) * (float)Math.PI) * 0.5f, num3) * num;
				danglers[i].segments[j].conRad = Mathf.Lerp(30f, 5f, num3) * num2;
			}
		}
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		ropeGraphic.AddToPositionsList(0, plant.rootPos);
		ropeGraphic.AddToPositionsList(1, plant.tentacle.Tip.pos);
		ropeGraphic.AlignAndConnect(2);
		base.Reset();
		for (int i = 0; i < danglers.Length; i++)
		{
			danglers[i].Reset();
		}
	}

	public override void Update()
	{
		base.Update();
		ropeGraphic.Update();
		if (culled)
		{
			return;
		}
		for (int i = 0; i < danglers.Length; i++)
		{
			danglers[i].Update();
			Vector2 vector = ropeGraphic.OnTubeDir(danglerProps[i, 0], 1f);
			danglers[i].segments[0].vel += vector * Mathf.Lerp(-1f, 1f, danglerProps[i, 0]) + Custom.DegToVec(Custom.VecToDeg(vector) + danglerProps[i, 2] * 360f) / danglers[i].segments[0].rad;
			if (plant.attack > 0.5f && plant.attack < 1f)
			{
				for (int j = 0; j < Math.Min(4, danglers[i].segments.Length); j++)
				{
					danglers[i].segments[j].vel += Custom.RNV() * Mathf.InverseLerp(0.5f, 1f, plant.attack) * 15f * danglerProps[i, 0];
				}
			}
			if (plant.extended < 0.25f)
			{
				for (int k = 0; k < danglers[i].segments.Length; k++)
				{
					danglers[i].segments[k].pos = Vector2.Lerp(danglers[i].segments[k].pos, DanglerConnection(i, 1f), Mathf.InverseLerp(0.25f, 0f, plant.extended));
				}
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + danglers.Length];
		for (int i = 0; i < danglers.Length; i++)
		{
			danglers[i].InitSprite(sLeaser, i + 1);
			sLeaser.sprites[i + 1].shader = rCam.room.game.rainWorld.Shaders["TentaclePlant"];
			sLeaser.sprites[i + 1].alpha = Mathf.InverseLerp(2f, 12f, danglers[i].segments.Length) * 0.9f + 0.1f * danglerProps[i, 2];
		}
		ropeGraphic.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		ropeGraphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!culled)
		{
			for (int i = 0; i < danglers.Length; i++)
			{
				danglers[i].DrawSprite(i + 1, sLeaser, rCam, timeStacker, camPos);
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
		for (int i = 0; i < danglers.Length; i++)
		{
			Color a = ((rCam.room != null && rCam.room.world.region != null && rCam.room.world.region.regionParams.kelpColor.HasValue) ? rCam.room.world.region.regionParams.kelpColor.Value : ((rCam.paletteA != 13 && rCam.paletteB != 13) ? Custom.HSL2RGB(Custom.Decimal(Mathf.Lerp(0.9f, 1.02f, Mathf.Pow(danglerProps[i, 0], 1.6f))), 1f, Mathf.Lerp(0f, 0.5f, danglerProps[i, 0])) : Custom.HSL2RGB(Mathf.Lerp(0.6f, 0.3f, Mathf.Pow(danglerProps[i, 0], 1.6f)), Mathf.Lerp(1f, 0.35f, danglerProps[i, 0]), Mathf.Lerp(0.05f, 0.35f, danglerProps[i, 0]))));
			sLeaser.sprites[i + 1].color = Color.Lerp(a, palette.blackColor, rCam.room.Darkness(plant.rootPos));
		}
	}

	public Vector2 DanglerConnection(int index, float timeStacker)
	{
		return ropeGraphic.OnTubePos(new Vector2(danglerProps[index, 1], danglerProps[index, 0]), timeStacker);
	}

	public Dangler.DanglerProps Props(int index)
	{
		return danglerVals;
	}
}
