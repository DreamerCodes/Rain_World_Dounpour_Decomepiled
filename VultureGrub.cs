using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class VultureGrub : Creature, IPlayerEdible
{
	public class VultureGrubState : CreatureState
	{
		public int origRoom;

		public int placedObjectIndex;

		public VultureGrubState(AbstractCreature creature)
			: base(creature)
		{
			origRoom = -1;
			placedObjectIndex = -1;
		}
	}

	public float lungs;

	public Vector2 lookDir;

	public Vector2 bodDir;

	public float wiggle;

	public int signalWaitCounter;

	public int singalCounter;

	public Vector2 headDir;

	public Vector2 lastHeadDir;

	public Vector2 foundSkyDir;

	public IntVector2? skyPosition;

	public bool vultureCalled;

	public int callingMode;

	public float swallowed;

	public PlacedObject placedObj;

	public IntVector2 lastAirTile;

	public bool sandboxVulture;

	private IntVector2[] _cachedTls = new IntVector2[100];

	private int bites = 3;

	public bool Singalling
	{
		get
		{
			if (!base.dead)
			{
				return singalCounter > 0;
			}
			return false;
		}
	}

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => base.dead;

	public bool AutomaticPickUp => true;

	public BodyChunk ChunkInOrder(int i)
	{
		return base.bodyChunks[i switch
		{
			1 => 0, 
			0 => 1, 
			_ => 2, 
		}];
	}

	public VultureGrub(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.25f;
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 3f, num * 0.25f);
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 3.5f, num * 0.5f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 3f, num * 0.25f);
		bodyChunkConnections = new BodyChunkConnection[3];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[0], 7f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 7f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 3.5f, BodyChunkConnection.Type.Push, 1f, -1f);
		base.airFriction = 0.995f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.1f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		lookDir = Custom.RNV() * UnityEngine.Random.value;
		bodDir = Custom.RNV() * UnityEngine.Random.value;
		foundSkyDir = new Vector2(0f, 1f);
		sandboxVulture = world.game.IsArenaSession && (world.game.GetArenaGameSession.GameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox || (ModManager.MSC && world.game.GetArenaGameSession.GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge));
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if ((base.State as VultureGrubState).origRoom > -1 && (base.State as VultureGrubState).origRoom == placeRoom.abstractRoom.index && (base.State as VultureGrubState).placedObjectIndex >= 0 && (base.State as VultureGrubState).placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			placedObj = placeRoom.roomSettings.placedObjects[(base.State as VultureGrubState).placedObjectIndex];
		}
	}

	public void InitiateSignalCountDown()
	{
		if (signalWaitCounter <= 0)
		{
			signalWaitCounter = 40;
		}
	}

	private void InitiateSignal()
	{
		foundSkyDir = new Vector2(0f, 1f);
		skyPosition = null;
		singalCounter = 200;
		vultureCalled = false;
		callingMode = 0;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new VultureGrubGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.CollideWithTerrain = grabbedBy.Count == 0;
		base.GoThroughFloors = grabbedBy.Count > 0;
		base.CollideWithObjects = grabbedBy.Count == 0;
		WeightedPush(1, 2, Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[1].pos), Custom.LerpMap(Vector2.Distance(base.bodyChunks[2].pos, base.bodyChunks[1].pos), 3.5f, 8f, 1f, 0f));
		if (!room.GetTile(base.mainBodyChunk.pos).Solid)
		{
			lastAirTile = room.GetTilePosition(base.mainBodyChunk.pos);
		}
		else
		{
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].HardSetPosition(room.MiddleOfTile(lastAirTile) + Custom.RNV());
			}
		}
		if (placedObj != null)
		{
			if (grabbedBy.Count == 0 && Mathf.Abs(base.mainBodyChunk.pos.x - placedObj.pos.x) > 10f)
			{
				base.mainBodyChunk.vel.x += (Mathf.Abs(base.mainBodyChunk.pos.x - placedObj.pos.x) - 10f) / (4f * ((base.mainBodyChunk.pos.x < placedObj.pos.x) ? 1f : (-1f)));
			}
			if (!Custom.DistLess(base.mainBodyChunk.pos, placedObj.pos, 50f) || grabbedBy.Count > 0)
			{
				if (room.game.session is StoryGameSession)
				{
					(room.game.session as StoryGameSession).saveState.ReportConsumedItem(room.world, karmaFlower: false, (base.State as VultureGrubState).origRoom, (base.State as VultureGrubState).placedObjectIndex, UnityEngine.Random.Range((placedObj.data as PlacedObject.ConsumableObjectData).minRegen, (placedObj.data as PlacedObject.ConsumableObjectData).maxRegen));
				}
				placedObj = null;
			}
		}
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		lastHeadDir = headDir;
		headDir *= 0.9f;
		if (grabbedBy.Count > 0)
		{
			Vector2 dir = Custom.PerpendicularVector(Custom.DirVec(base.bodyChunks[0].pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			dir.y = Mathf.Abs(dir.y);
			WeightedPush(1, 2, dir, 4f);
		}
		if (!base.dead)
		{
			Act();
		}
		if (!base.dead && base.mainBodyChunk.submersion > 0.5f)
		{
			lungs = Mathf.Max(lungs - 1f / 180f, 0f);
			if (lungs == 0f)
			{
				Die();
			}
		}
		else
		{
			lungs = Mathf.Min(lungs + 0.02f, 1f);
		}
		if (singalCounter > 0)
		{
			singalCounter--;
		}
		if (signalWaitCounter > 0)
		{
			signalWaitCounter--;
			if (signalWaitCounter == 0)
			{
				InitiateSignal();
			}
		}
		bool flag = false;
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
		{
			int num = -1;
			for (int j = 0; j < 2; j++)
			{
				if ((grabbedBy[0].grabber as Player).grasps[j] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[j].grabbed))
				{
					num = j;
					break;
				}
			}
			if (num > -1 && (grabbedBy[0].grabber as Player).grasps[num] != null && (grabbedBy[0].grabber as Player).grasps[num].grabbed == this)
			{
				flag = true;
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag ? 1f : 0f, 0.05f, 0.05f);
	}

	private void Act()
	{
		if (Singalling)
		{
			lookDir += Custom.RNV() * UnityEngine.Random.value * 0.15f + new Vector2(0f, 0.25f * UnityEngine.Random.value);
			lookDir = Vector2.ClampMagnitude(lookDir, 1f);
			if (base.bodyChunks[2].pos.y > base.bodyChunks[1].pos.y)
			{
				base.bodyChunks[2].vel.y -= 0.5f;
				base.bodyChunks[1].vel.y += 0.5f;
				base.bodyChunks[2].vel += Custom.RNV() * UnityEngine.Random.value;
				base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value;
			}
			WeightedPush(1, 0, lookDir.normalized, lookDir.magnitude);
			WeightedPush(1, 2, lookDir.normalized, lookDir.magnitude);
			if (RayTraceSky(foundSkyDir))
			{
				headDir = Vector2.ClampMagnitude(headDir + foundSkyDir * UnityEngine.Random.value, 1f);
				Vector2 testDir = Vector3.Slerp(foundSkyDir, new Vector2(0f, 1f), UnityEngine.Random.value);
				if (RayTraceSky(testDir))
				{
					foundSkyDir = testDir;
				}
			}
			else
			{
				Vector2 testDir2 = Custom.DegToVec(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value));
				if (RayTraceSky(testDir2))
				{
					foundSkyDir = testDir2;
				}
				headDir = Vector2.ClampMagnitude(headDir + Custom.DegToVec(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value)) * UnityEngine.Random.value, 1f);
			}
			if (!vultureCalled && callingMode == 0 && skyPosition.HasValue && singalCounter < 100)
			{
				AttemptCallVulture();
			}
			if (singalCounter < 5 && !vultureCalled)
			{
				callingMode = -1;
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as VultureGrubGraphics).blinking = 220;
				}
			}
			return;
		}
		if (!base.safariControlled || (base.safariControlled && inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0)))
		{
			lookDir += Custom.RNV() * UnityEngine.Random.value * 0.25f + new Vector2(0f, 0.025f * room.gravity * UnityEngine.Random.value * UnityEngine.Random.value);
			bodDir += Custom.RNV() * UnityEngine.Random.value * 0.25f;
		}
		else
		{
			bodDir = Vector2.zero;
		}
		lookDir = Vector2.ClampMagnitude(lookDir, 1f);
		bodDir = Vector2.ClampMagnitude(bodDir, 1f);
		WeightedPush(1, 0, lookDir.normalized, lookDir.magnitude);
		WeightedPush(1, 2, lookDir.normalized, lookDir.magnitude);
		base.bodyChunks[0].vel += bodDir * 0.2f;
		base.bodyChunks[1].vel -= bodDir * 0.2f;
		base.bodyChunks[2].vel -= bodDir * 0.2f;
		WeightedPush(0, 1, bodDir.normalized, bodDir.magnitude);
		WeightedPush(0, 2, bodDir.normalized, bodDir.magnitude);
		if (base.safariControlled)
		{
			if (!inputWithoutDiagonals.HasValue)
			{
				return;
			}
			if (inputWithoutDiagonals.Value.y != 0 || inputWithoutDiagonals.Value.x != 0)
			{
				wiggle = Mathf.Max(UnityEngine.Random.Range(0f, 0.001f), Custom.LerpAndTick(wiggle, 0f, 0.05f, 0.025f));
				if (UnityEngine.Random.value < 1f / 60f)
				{
					wiggle = Mathf.Max(wiggle, UnityEngine.Random.value);
				}
				Vector2 vector = Custom.RNV();
				Vector2 vector2 = Custom.RNV();
				base.bodyChunks[1].vel += new Vector2(Mathf.Abs(vector.x) * Mathf.Sign(inputWithoutDiagonals.Value.x) * (float)Mathf.Abs(inputWithoutDiagonals.Value.x), vector.y) * UnityEngine.Random.value * Mathf.Pow(wiggle, 0.1f) * 5f;
				base.bodyChunks[2].vel += new Vector2(Mathf.Abs(vector2.x) * Mathf.Sign(inputWithoutDiagonals.Value.x) * (float)Mathf.Abs(inputWithoutDiagonals.Value.x), vector2.y) * UnityEngine.Random.value * Mathf.Pow(wiggle, 0.1f) * 5f;
				if (inputWithoutDiagonals.Value.x != 0)
				{
					lookDir.x = Mathf.Abs(lookDir.x) * Mathf.Sign(inputWithoutDiagonals.Value.x);
					bodDir.x = Mathf.Abs(bodDir.x) * Mathf.Sign(inputWithoutDiagonals.Value.x);
				}
			}
			else
			{
				wiggle = 0f;
			}
			if (inputWithoutDiagonals.Value.jmp && !lastInputWithoutDiagonals.Value.jmp)
			{
				InitiateSignal();
			}
		}
		else
		{
			wiggle = Custom.LerpAndTick(wiggle, 0f, 0.05f, 0.025f);
			if (UnityEngine.Random.value < 1f / 60f)
			{
				wiggle = Mathf.Max(wiggle, UnityEngine.Random.value);
			}
			if (wiggle > 0f)
			{
				base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(wiggle, 0.1f) * 5f;
				base.bodyChunks[2].vel += Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(wiggle, 0.1f) * 5f;
			}
		}
	}

	public bool RayTraceSky(Vector2 testDir)
	{
		if (room.abstractRoom.skyExits < 1)
		{
			return false;
		}
		if (room.abstractRoom.AttractionForCreature(CreatureTemplate.Type.Vulture) == AbstractRoom.CreatureRoomAttraction.Forbidden || room.abstractRoom.AttractionForCreature(CreatureTemplate.Type.KingVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden || (ModManager.MSC && room.abstractRoom.AttractionForCreature(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden))
		{
			return false;
		}
		Vector2 corner = Custom.RectCollision(base.bodyChunks[1].pos, base.bodyChunks[1].pos + testDir * 100000f, room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
		if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.bodyChunks[1].pos, corner).HasValue)
		{
			return false;
		}
		if (corner.y >= room.PixelHeight - 5f)
		{
			skyPosition = room.GetTilePosition(corner);
			return true;
		}
		return false;
	}

	public void AttemptCallVulture()
	{
		AbstractCreature abstractCreature = null;
		AbstractRoom abstractRoom = room.world.GetAbstractRoom(UnityEngine.Random.Range(room.world.firstRoomIndex, room.world.firstRoomIndex + room.world.NumberOfRooms));
		if (!ModManager.MMF || !room.game.IsArenaSession)
		{
			if (abstractRoom == room.abstractRoom || (abstractRoom.AttractionForCreature(CreatureTemplate.Type.Vulture) == AbstractRoom.CreatureRoomAttraction.Forbidden && abstractRoom.AttractionForCreature(CreatureTemplate.Type.KingVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden && (!ModManager.MSC || abstractRoom.AttractionForCreature(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden)))
			{
				return;
			}
		}
		else
		{
			abstractRoom = room.world.offScreenDen;
		}
		CreatureTemplate.Type[] array = ((!ModManager.MSC) ? new CreatureTemplate.Type[2]
		{
			CreatureTemplate.Type.Vulture,
			CreatureTemplate.Type.KingVulture
		} : new CreatureTemplate.Type[3]
		{
			CreatureTemplate.Type.Vulture,
			CreatureTemplate.Type.KingVulture,
			MoreSlugcatsEnums.CreatureTemplateType.MirosVulture
		});
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (abstractRoom.creatures[i].state.alive && abstractRoom.creatures[i].creatureTemplate.type == array[j] && abstractRoom.AttractionForCreature(array[j]) != AbstractRoom.CreatureRoomAttraction.Forbidden)
				{
					abstractCreature = abstractRoom.creatures[i];
				}
			}
			if (abstractCreature != null)
			{
				break;
			}
		}
		if (abstractCreature == null && sandboxVulture && (UnityEngine.Random.value < 0.025f || (ModManager.MSC && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)))
		{
			float value = UnityEngine.Random.value;
			CreatureTemplate.Type type = CreatureTemplate.Type.Vulture;
			if (ModManager.MMF)
			{
				if (ModManager.MSC && value <= 0.15f && (global::MoreSlugcats.MoreSlugcats.chtUnlockCreatures.Value || room.game.rainWorld.progression.miscProgressionData.GetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.MirosVulture)))
				{
					type = MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
				}
				else if (value > 0.15f && value <= 0.3f && (MultiplayerUnlocks.CheckUnlockAll() || room.game.rainWorld.progression.miscProgressionData.GetTokenCollected(MultiplayerUnlocks.SandboxUnlockID.KingVulture)))
				{
					type = CreatureTemplate.Type.KingVulture;
				}
				if (ModManager.MSC && room.game.GetArenaGameSession.chMeta != null)
				{
					type = CreatureTemplate.Type.Vulture;
				}
			}
			abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
			room.world.offScreenDen.AddEntity(abstractCreature);
			sandboxVulture = false;
		}
		if (abstractCreature == null || (abstractCreature.realizedCreature != null && singalCounter > 20))
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = -1;
		for (int k = 0; k < room.borderExits.Length; k++)
		{
			if (!(room.borderExits[k].type == AbstractRoomNode.Type.SkyExit))
			{
				continue;
			}
			for (int l = 0; l < room.borderExits[k].borderTiles.Length; l++)
			{
				if (Custom.ManhattanDistance(room.borderExits[k].borderTiles[l], skyPosition.Value) < num)
				{
					num = Custom.ManhattanDistance(room.borderExits[k].borderTiles[l], skyPosition.Value);
					num2 = k + room.exitAndDenIndex.Length;
				}
			}
		}
		if (num2 < 0)
		{
			return;
		}
		int num3;
		for (num3 = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, room.MiddleOfTile(skyPosition.Value), _cachedTls); num3 >= _cachedTls.Length; num3 = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, room.MiddleOfTile(skyPosition.Value), _cachedTls))
		{
			Custom.LogWarning($"AttemptCallVulture ray tracing limit exceeded, extending cache to {_cachedTls.Length + 100} and trying again!");
			Array.Resize(ref _cachedTls, _cachedTls.Length + 100);
		}
		IntVector2? intVector = null;
		for (int m = 0; m < num3; m++)
		{
			if (room.aimap.TileAccessibleToCreature(_cachedTls[m], StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture)))
			{
				intVector = _cachedTls[m];
				break;
			}
		}
		if (intVector.HasValue)
		{
			abstractCreature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, intVector.Value.x, intVector.Value.y, num2));
			if (abstractCreature.realizedCreature == null)
			{
				abstractCreature.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, num2));
			}
			callingMode = 1;
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as VultureGrubGraphics).blinking = 220;
			}
			vultureCalled = true;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 8f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound(SoundID.Slugcat_Eat_Centipede, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
