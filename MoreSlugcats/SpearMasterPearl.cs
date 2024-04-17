using System.Globalization;
using CoralBrain;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SpearMasterPearl : DataPearl, IOwnProjectedCircles, IDrawable
{
	public class AbstractSpearMasterPearl : AbstractDataPearl
	{
		public bool broadcastTagged;

		public AbstractSpearMasterPearl(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
			: base(world, MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
		{
			Custom.Log("Abstract SM pearl made?");
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}", BaseToString(), broadcastTagged ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public bool storyFly;

	public Vector2 storyFlyTarget;

	private QuickPathFinder quickPather;

	private QuickPath path;

	public Vector2 direction;

	public Vector2 lastDirection;

	public Oracle SeekOracle;

	public Vector2[,,] cords;

	private Color blackCol;

	private float rotation;

	private float lastRotation;

	public float sin;

	public float lastSin;

	public ProjectedCircle myCircle;

	public bool holoVisible;

	public float holoFade;

	public float lastHoloFade;

	public NSHSwarmer.Shape holoShape;

	public float[,] directionsPower;

	public int PearlSprite => 0;

	public int SpecularSprite => 1;

	public int ShineSprite => 2;

	public int TailSprite => 3;

	public int TotalSprites => CordsSprites + holoShape.LinesCount;

	public int CordsSprites => 3 + cords.GetLength(0);

	public SpearMasterPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		Custom.Log("SM pearl made?");
		rotation = Random.value * 360f;
		lastRotation = rotation;
		cords = new Vector2[2, 5, 4];
		for (int i = 0; i < cords.GetLength(0); i++)
		{
			for (int j = 0; j < cords.GetLength(1); j++)
			{
				cords[i, j, 3] = Custom.RNV() * Random.value;
			}
		}
		holoShape = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.SmallDiamondHolder, new Vector3(0f, 0f, 0f), 0f, 0f);
		directionsPower = new float[12, 3];
		if (world.game.IsStorySession && world.game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged)
		{
			(abstractPhysicalObject as AbstractSpearMasterPearl).broadcastTagged = true;
		}
		if ((abstractPhysicalObject as AbstractSpearMasterPearl).broadcastTagged)
		{
			holoVisible = true;
		}
	}

	public new void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		blackCol = palette.blackColor;
		for (int i = 0; i < cords.GetLength(0); i++)
		{
			sLeaser.sprites[TailSprite + i].color = blackCol;
		}
	}

	private void ConnectSegment(int c, int i)
	{
		if (i == 0)
		{
			Vector2 vector = base.firstChunk.pos - Custom.DegToVec(rotation) * 8f;
			Vector2 vector2 = Custom.DirVec(cords[c, i, 0], vector);
			float num = Vector2.Distance(cords[c, i, 0], vector);
			cords[c, i, 0] -= vector2 * (1.5f - num);
			cords[c, i, 2] -= vector2 * (1.5f - num);
		}
		else
		{
			Vector2 vector3 = Custom.DirVec(cords[c, i, 0], cords[c, i - 1, 0]);
			float num2 = Vector2.Distance(cords[c, i, 0], cords[c, i - 1, 0]);
			cords[c, i, 0] -= vector3 * (1.5f - num2) * 0.5f;
			cords[c, i, 2] -= vector3 * (1.5f - num2) * 0.5f;
			cords[c, i - 1, 0] += vector3 * (1.5f - num2) * 0.5f;
			cords[c, i - 1, 2] += vector3 * (1.5f - num2) * 0.5f;
		}
	}

	public new void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker));
		for (int i = 0; i < cords.GetLength(0); i++)
		{
			Vector2 vector3 = vector - vector2 * 4f;
			for (int j = 0; j < cords.GetLength(1); j++)
			{
				Vector2 vector4 = Vector2.Lerp(cords[i, j, 1], cords[i, j, 0], timeStacker);
				Vector2 normalized = (vector4 - vector3).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized);
				float num = Vector2.Distance(vector4, vector3) / 5f;
				if (j == 0)
				{
					(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * 0.5f - camPos);
					(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * 0.5f - camPos);
				}
				else
				{
					(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * 0.5f + normalized * num - camPos);
					(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * 0.5f + normalized * num - camPos);
				}
				(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * 0.5f - normalized * num - camPos);
				(sLeaser.sprites[TailSprite + i] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * 0.5f - normalized * num - camPos);
				vector3 = vector4;
			}
		}
		Vector2 pointsVec = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		float num2 = Custom.SCurve(Mathf.Lerp(lastHoloFade, holoFade, timeStacker), 0.65f);
		float pointsWeight = 1f;
		float maxDist = 1f;
		if (num2 > 0f)
		{
			int sprite = CordsSprites;
			holoShape.Draw(sLeaser, rCam, timeStacker, pointsVec, camPos, ref sprite, 0f, 0f, num2, shakeErr: false, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
			pointsVec /= pointsWeight;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void EndStoryMovement()
	{
		SeekOracle = null;
		storyFly = false;
		base.gravity = 0.9f;
	}

	public new void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[PearlSprite] = new FSprite("JetFishEyeA");
		sLeaser.sprites[SpecularSprite] = new FSprite("tinyStar");
		sLeaser.sprites[ShineSprite] = new FSprite("Futile_White");
		sLeaser.sprites[ShineSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		for (int i = 0; i < cords.GetLength(0); i++)
		{
			sLeaser.sprites[TailSprite + i] = TriangleMesh.MakeLongMesh(cords.GetLength(1), pointyTip: false, customColor: false);
		}
		for (int num = holoShape.LinesCount - 1; num >= 0; num--)
		{
			sLeaser.sprites[CordsSprites + num] = new FSprite("pixel");
			sLeaser.sprites[CordsSprites + num].anchorY = 0f;
			sLeaser.sprites[CordsSprites + num].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
			sLeaser.sprites[CordsSprites + num].color = Color.yellow;
		}
		base.AddToContainer(sLeaser, rCam, (FContainer)null);
	}

	public void Reset()
	{
		for (int i = 0; i < cords.GetLength(0); i++)
		{
			for (int j = 0; j < cords.GetLength(1); j++)
			{
				cords[i, j, 0] = base.firstChunk.pos + new Vector2(0f, 5f * (float)i);
				cords[i, j, 1] = cords[i, j, 0];
				cords[i, j, 2] *= 0f;
			}
		}
	}

	public void SeekToOracle(Oracle myOracle)
	{
		SeekOracle = myOracle;
		storyFly = true;
		base.gravity = 0f;
	}

	public void StartStoryMovement(Vector2 goloc)
	{
		storyFlyTarget = goloc;
		storyFly = true;
		base.gravity = 0f;
	}

	public void StoryMovement()
	{
		Vector2 vector = Custom.DirVec(base.firstChunk.pos, storyFlyTarget);
		if (!room.readyForAI || !Custom.DistLess(base.firstChunk.pos, storyFlyTarget, 2000f) || room.VisualContact(base.firstChunk.pos, storyFlyTarget))
		{
			path = null;
			quickPather = null;
		}
		else if (path == null)
		{
			if (quickPather == null)
			{
				quickPather = new QuickPathFinder(room.GetTilePosition(base.firstChunk.pos), room.GetTilePosition(storyFlyTarget), room.aimap, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
			}
			for (int i = 0; i < 100; i++)
			{
				quickPather.Update();
				if (quickPather.status != 0)
				{
					path = quickPather.ReturnPath();
					quickPather = null;
					break;
				}
			}
		}
		else
		{
			bool flag = false;
			IntVector2 pos = new IntVector2(-1, -1);
			for (int num = path.tiles.Length - 1; num >= 0; num--)
			{
				if (pos.x == -1 && pos.y == -1 && room.VisualContact(base.firstChunk.pos, room.MiddleOfTile(path.tiles[num])))
				{
					pos = path.tiles[num];
				}
				if (!flag && room.VisualContact(storyFlyTarget, room.MiddleOfTile(path.tiles[num])))
				{
					flag = true;
				}
				if ((pos.x != -1 || pos.y != -1) && flag)
				{
					break;
				}
			}
			if (!flag || (pos.x == -1 && pos.y == -1))
			{
				path = null;
			}
			else
			{
				vector = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(pos));
			}
		}
		bool solid = room.GetTile(base.firstChunk.pos + (base.firstChunk.pos - base.firstChunk.lastPos) * 7f + direction * 30f).Solid;
		if (solid)
		{
			base.firstChunk.vel *= 0.7f;
			if (room.readyForAI)
			{
				IntVector2 tilePosition = room.GetTilePosition(base.firstChunk.pos);
				for (int j = 0; j < 8; j++)
				{
					if (room.aimap.getTerrainProximity(tilePosition + Custom.eightDirections[j]) > room.aimap.getTerrainProximity(tilePosition))
					{
						vector += 0.2f * Custom.eightDirections[j].ToVector2();
					}
				}
				vector.Normalize();
			}
		}
		else if (base.firstChunk.lastPos != base.firstChunk.pos)
		{
			BodyChunk bodyChunk = base.firstChunk;
			Vector2 vel = bodyChunk.vel;
			bodyChunk.vel = vel * Custom.LerpMap(Vector2.Dot((base.firstChunk.pos - base.firstChunk.lastPos).normalized, vector), -1f, 1f, 0.85f, 0.97f);
		}
		direction = Vector3.Slerp(direction, vector, (!solid) ? Custom.LerpMap(Vector3.Distance(base.firstChunk.pos, storyFlyTarget), 20f, 200f, 1f, 0.3f) : 1f);
		if (Vector3.Distance(base.firstChunk.pos, storyFlyTarget) < 2f)
		{
			direction.Scale(new Vector2(0.2f, 0.2f));
		}
		base.firstChunk.vel += direction;
	}

	public override void Update(bool eu)
	{
		lastSin = sin;
		sin += (1f + glimmer) / 75f;
		lastRotation = rotation;
		base.Update(eu);
		for (int i = 0; i < directionsPower.GetLength(0); i++)
		{
			directionsPower[i, 1] = directionsPower[i, 0];
			directionsPower[i, 0] = Custom.LerpAndTick(directionsPower[i, 0], directionsPower[i, 2], 0.03f, 1f / 15f);
			directionsPower[i, 2] = 0f;
		}
		lastHoloFade = holoFade;
		if (holoVisible)
		{
			holoFade = Mathf.Min(1f, holoFade + 1f / 12f);
		}
		else
		{
			holoFade = Mathf.Max(0f, holoFade - 1f / 12f);
		}
		if (holoFade > 0f || lastHoloFade > 0f)
		{
			holoShape.Update(changeLikely: false, 0f, holoFade, base.firstChunk.pos - base.firstChunk.lastPos, 0f, ref directionsPower);
		}
		else
		{
			holoShape.ResetUpdate(base.firstChunk.pos);
		}
		if (storyFly)
		{
			if (SeekOracle != null && SeekOracle.graphicsModule != null)
			{
				storyFlyTarget = (SeekOracle.graphicsModule as OracleGraphics).hands[1].pos;
			}
			StoryMovement();
		}
		Vector2 vector = -Custom.DegToVec(0f);
		for (int j = 0; j < cords.GetLength(0); j++)
		{
			for (int k = 0; k < cords.GetLength(1); k++)
			{
				float num = (float)k / (float)(cords.GetLength(1) - 1);
				cords[j, k, 1] = cords[j, k, 0];
				cords[j, k, 0] += cords[j, k, 2];
				cords[j, k, 2] *= Mathf.Lerp(1f, 0.85f, num);
				cords[j, k, 2] += vector * (3f + (float)Mathf.Abs(0) / 5f) * Mathf.Pow(1f - num, 3f);
				if (k > 1 && room.GetTile(cords[j, k, 0]).Solid)
				{
					SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(cords[j, k, 0], cords[j, k, 1], cords[j, k, 2], 1f, new IntVector2(0, 0), goThroughFloors: false), room: room);
					cd2 = SharedPhysics.HorizontalCollision(room, cd2);
					cords[j, k, 0] = cd2.pos;
					cords[j, k, 2] = cd2.vel;
				}
				if (!room.PointSubmerged(cords[j, k, 0]))
				{
					cords[j, k, 2].y -= 0.9f * room.gravity * num;
				}
				else
				{
					cords[j, k, 2] *= 0.8f;
					cords[j, k, 2].y += 0.3f * num;
				}
				cords[j, k, 2] += Custom.RotateAroundOrigo(cords[j, k, 3], 0f);
				ConnectSegment(j, k);
			}
			for (int num2 = cords.GetLength(0) - 1; num2 >= 0; num2--)
			{
				ConnectSegment(j, num2);
			}
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		myCircle = null;
		if (newRoom.abstractRoom.name == "SS_AI" || newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
		{
			myCircle = new ProjectedCircle(newRoom, this, 0, 0f);
			newRoom.AddObject(myCircle);
		}
		Reset();
	}

	public void DisableGravity()
	{
		base.gravity = 0f;
	}

	public Vector2 CircleCenter(int index, float timeStacker)
	{
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
	}

	public Room HostingCircleFromRoom()
	{
		return room;
	}

	public bool CanHostCircle()
	{
		return !base.slatedForDeletetion;
	}

	public new void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, (FContainer)null);
	}
}
