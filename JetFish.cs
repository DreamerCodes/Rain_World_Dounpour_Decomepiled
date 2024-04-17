using System;
using System.Collections.Generic;
using RWCustom;
using SplashWater;
using UnityEngine;

public class JetFish : Creature
{
	public struct IndividualVariations
	{
		public float fatness;

		public float tentacleLength;

		public float tentacleFatness;

		public float flipperSize;

		public float flipperOrientation;

		public int tentacleContour;

		public int whiskers;

		public int flipper;

		public int whiskerSeed;

		public Color eyeColor;

		public IndividualVariations(float fatness, float tentacleLength, int tentacleContour, int whiskers, int whiskerSeed, int flipper, float flipperSize, float flipperOrientation)
		{
			this.fatness = fatness;
			this.tentacleLength = tentacleLength;
			tentacleFatness = 1f - tentacleLength;
			this.tentacleContour = tentacleContour;
			eyeColor = Custom.HSL2RGB(5f / 6f, 1f, 0.5f);
			this.whiskers = whiskers;
			this.whiskerSeed = whiskerSeed;
			this.flipper = flipper;
			this.flipperSize = flipperSize;
			this.flipperOrientation = flipperOrientation;
		}
	}

	public JetFishAI AI;

	public IndividualVariations iVars;

	public Vector2 swimDir;

	public float swimSpeed;

	public float jetActive;

	public float jetWater;

	public WaterJet waterJet;

	public float slowDownForPrecision;

	public List<Vector2> trail;

	public float turnSpeed;

	public float diveSpeed;

	public float surfSpeed;

	public bool allDry;

	private StaticSoundLoop jetSound;

	public int grabable;

	public bool albino;

	public float availableWater;

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		iVars = new IndividualVariations(Custom.ClampedRandomVariation(0.5f, 0.1f, 0.5f) * 2f, whiskers: UnityEngine.Random.Range(0, 4), tentacleLength: UnityEngine.Random.value, tentacleContour: UnityEngine.Random.Range(0, 3), whiskerSeed: UnityEngine.Random.Range(0, int.MaxValue), flipper: UnityEngine.Random.Range(0, 5), flipperSize: Mathf.Lerp(0.7f, 1.1f, UnityEngine.Random.value), flipperOrientation: UnityEngine.Random.value);
		UnityEngine.Random.state = state;
	}

	public JetFish(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		albino = false;
		if (world.game.overWorld != null)
		{
			int num = world.RegionNumberOfSpawner(abstractCreature.ID);
			if (num >= 0 && num < world.game.overWorld.regions.Length && (world.game.overWorld.regions[num].name == "SB" || world.game.overWorld.regions[num].regionParams.albinos))
			{
				albino = true;
			}
		}
		albino = albino || (ModManager.MMF && world.game.IsArenaSession && UnityEngine.Random.value <= 0.04f);
		GenerateIVars();
		if (albino)
		{
			iVars.eyeColor = Color.red;
		}
		float num2 = 0.7f;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8.5f, num2 / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, num2 / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.buoyancy = 0.99f;
		base.GoThroughFloors = true;
		trail = new List<Vector2>();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new JetFishGraphics(this);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		jetSound = new StaticSoundLoop(SoundID.Jet_Fish_Water_Jet_LOOP, base.bodyChunks[1].pos, room, 0f, 1f);
		waterJet = null;
	}

	public override void Update(bool eu)
	{
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
		if (!base.Consious || room.GetTilePosition(base.mainBodyChunk.pos).y >= room.defaultWaterLevel - 1 || room.GetTilePosition(base.bodyChunks[1].pos).y >= room.defaultWaterLevel - 1 || base.Submersion < 1f)
		{
			grabable = 40;
		}
		else if (grabable > 0)
		{
			grabable--;
		}
		base.mainBodyChunk.terrainSqueeze = 1f;
		base.bodyChunks[1].terrainSqueeze = 1f;
		trail.Insert(0, (base.bodyChunks[1].pos - base.bodyChunks[0].pos).normalized);
		if (trail.Count > 20)
		{
			trail.RemoveAt(trail.Count - 1);
		}
		if (trail.Count > 1)
		{
			float num = 0f;
			for (int i = 0; i < trail.Count - 1; i++)
			{
				num += Vector2.Dot(trail[i], trail[i + 1]);
			}
			num /= (float)(trail.Count - 1);
			num = Mathf.InverseLerp(0.99f, 0.98f, num);
			if (turnSpeed < num)
			{
				turnSpeed = Mathf.Min(turnSpeed + 0.05f, num);
			}
			else
			{
				turnSpeed = Mathf.Max(turnSpeed - 0.0125f, num);
			}
		}
		if (allDry)
		{
			if (base.Submersion == 1f)
			{
				diveSpeed = Mathf.Pow(Mathf.InverseLerp(1f, -0.5f, Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos).y) * Mathf.InverseLerp(0f, -20f, base.mainBodyChunk.vel.y), 0.65f);
				allDry = false;
			}
		}
		else if (base.Submersion == 0f)
		{
			allDry = true;
		}
		diveSpeed = Mathf.Max(diveSpeed - 1f / 60f, 0f);
		jetSound.Update();
		jetSound.pos = base.bodyChunks[1].pos;
		jetSound.pitch = Custom.LerpMap(jetWater, 0f, 0.2f, 0.5f, 1f);
		jetSound.volume = jetActive;
		float num2 = 0f;
		if (base.Submersion > 0f && base.Submersion < 1f)
		{
			num2 = Mathf.Abs((base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized.y);
		}
		if (surfSpeed < num2)
		{
			surfSpeed = Mathf.Min(surfSpeed + 0.05f, num2);
		}
		else
		{
			surfSpeed = Mathf.Max(surfSpeed - 0.0125f, num2);
		}
		if (base.Consious)
		{
			base.waterFriction = 0.995f;
			waterRetardationImmunity = 0.8f;
			bool flag = false;
			for (int j = 0; j < grabbedBy.Count; j++)
			{
				if (grabbedBy[j].grabber is Player)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GrabbedByPlayer();
			}
			else
			{
				turnSpeed = 0f;
				diveSpeed *= 0.8f;
				surfSpeed *= 0.9f;
				Act();
			}
		}
		else
		{
			base.waterFriction = 0.95f;
			waterRetardationImmunity = 0f;
		}
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
	}

	private void Act()
	{
		AI.Update();
		if (AI.behavior == JetFishAI.Behavior.GetUnstuck && base.Submersion > 0f)
		{
			float num = Mathf.InverseLerp(0.5f, 1f, AI.stuckTracker.Utility());
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value * 10f * num;
				base.bodyChunks[i].terrainSqueeze = Mathf.Lerp(1f, 0.1f, num);
			}
		}
		MovementConnection movementConnection = (AI.pathFinder as FishPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					LoseAllGrasps();
				}
			}
		}
		if (movementConnection != default(MovementConnection) && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation))
		{
			enteringShortCut = movementConnection.StartTile;
			if (ModManager.MMF && AI.denFinder.GetDenPosition().HasValue)
			{
				WorldCoordinate destinationCoord = movementConnection.destinationCoord;
				WorldCoordinate? denPosition = AI.denFinder.GetDenPosition();
				if (denPosition.HasValue && destinationCoord == denPosition.GetValueOrDefault() && AI.behavior == JetFishAI.Behavior.ReturnPrey && base.grasps[0] != null && !(base.grasps[0].grabbed is Creature))
				{
					Custom.Log($"Jetfish returned froot to home den at {AI.denFinder.GetDenPosition().Value}!");
					base.grasps[0].grabbed.abstractPhysicalObject.Abstractize(AI.denFinder.GetDenPosition().Value);
					base.grasps[0].grabbed.abstractPhysicalObject.Destroy();
					base.grasps[0].Release();
				}
			}
			if (base.safariControlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int j = 0; j < shortcuts.Length; j++)
				{
					ShortcutData shortcutData = shortcuts[j];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
					{
						list.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (list.Count > 0)
					{
						list.Shuffle();
						NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
					}
					else
					{
						NPCTransportationDestination = movementConnection.destinationCoord;
					}
				}
			}
			else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = movementConnection.destinationCoord;
			}
			return;
		}
		if (AI.floatGoalPos.HasValue && AI.pathFinder.GetDestination.TileDefined && AI.pathFinder.GetDestination.room == room.abstractRoom.index && room.VisualContact(base.mainBodyChunk.pos, AI.floatGoalPos.Value) && !base.safariControlled)
		{
			swimDir = Custom.DirVec(base.mainBodyChunk.pos, AI.floatGoalPos.Value);
		}
		else if (movementConnection != default(MovementConnection))
		{
			swimDir = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.DestTile));
			WorldCoordinate destinationCoord2 = movementConnection.destinationCoord;
			for (int k = 0; k < 4; k++)
			{
				MovementConnection movementConnection2 = (AI.pathFinder as FishPather).FollowPath(destinationCoord2, actuallyFollowingThisPath: false);
				if (movementConnection2 != default(MovementConnection) && movementConnection2.destinationCoord.TileDefined && movementConnection2.destinationCoord.room == room.abstractRoom.index && room.VisualContact(movementConnection.destinationCoord.Tile, movementConnection2.DestTile))
				{
					swimDir += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection2.DestTile));
					destinationCoord2 = movementConnection2.destinationCoord;
					if (room.aimap.getAItile(movementConnection2.DestTile).narrowSpace)
					{
						slowDownForPrecision += 0.3f;
						break;
					}
				}
			}
			swimDir = swimDir.normalized;
		}
		slowDownForPrecision = Mathf.Clamp(slowDownForPrecision - 0.1f, 0f, 1f);
		if (base.safariControlled)
		{
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.x != 0 && base.Submersion < 0.4f && (base.bodyChunks[0].ContactPoint.y < 0 || base.bodyChunks[1].ContactPoint.y < 0))
			{
				float ang = Mathf.Lerp(-45f, 0f, UnityEngine.Random.value);
				if (inputWithDiagonals.Value.x > 0)
				{
					ang = Mathf.Lerp(0f, 45f, UnityEngine.Random.value);
				}
				base.mainBodyChunk.vel += Custom.DegToVec(ang) * Mathf.Lerp(6f, 26f, UnityEngine.Random.value);
				room.PlaySound(SoundID.Jet_Fish_On_Land_Jump, base.mainBodyChunk);
			}
		}
		else if (base.Submersion < 0.4f && (base.bodyChunks[0].ContactPoint.y < 0 || base.bodyChunks[1].ContactPoint.y < 0) && UnityEngine.Random.value < 0.05f)
		{
			base.mainBodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-45f, 45f, UnityEngine.Random.value)) * Mathf.Lerp(6f, 26f, UnityEngine.Random.value);
			room.PlaySound(SoundID.Jet_Fish_On_Land_Jump, base.mainBodyChunk);
		}
		if (base.safariControlled && inputWithDiagonals.HasValue && !inputWithDiagonals.Value.AnyDirectionalInput)
		{
			Swim(0.1f);
		}
		else
		{
			Swim(ModManager.MMF ? Mathf.Lerp(1f, 1.6f, base.abstractCreature.personality.energy * base.Submersion) : 1f);
		}
	}

	private void GrabbedByPlayer()
	{
		if (!base.Consious)
		{
			return;
		}
		Player player = null;
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Player)
			{
				player = grabbedBy[i].grabber as Player;
				break;
			}
		}
		if (player == null)
		{
			return;
		}
		if (ModManager.MMF)
		{
			if (player.submerged)
			{
				player.GoThroughFloors = true;
			}
			base.GoThroughFloors = true;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].terrainSqueeze = 0.5f;
			}
		}
		AI.getAwayFromCreature = player.abstractCreature;
		AI.getAwayCounter = 150;
		SocialMemory.Relationship orInitiateRelationship = base.abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
		orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, -1f, 0.00025f);
		if (player.input[0].analogueDir.magnitude > 0f)
		{
			swimDir = player.input[0].analogueDir.normalized;
		}
		else if (player.input[0].x != 0 || player.input[0].y != 0)
		{
			swimDir = new Vector2(player.input[0].x, player.input[0].y).normalized;
		}
		for (int k = 0; k < 2; k++)
		{
			float b = 0.97f;
			if (player.slugcatStats.bodyWeightFac <= 1f)
			{
				b = 0.95f;
			}
			if (ModManager.MSC && player.slugcatStats.bodyWeightFac >= 1.3f)
			{
				b = 0.9f;
			}
			player.bodyChunks[k].vel *= Mathf.Lerp(1f, b, player.bodyChunks[k].submersion);
		}
		if (player.input[0].jmp && !player.input[1].jmp && (base.bodyChunks[0].ContactPoint.y < 0 || base.bodyChunks[1].ContactPoint.y < 0))
		{
			base.bodyChunks[1].vel.y += 8f;
			room.PlaySound(SoundID.Jet_Fish_On_Land_Jump, base.mainBodyChunk);
		}
		if (ModManager.MMF)
		{
			float a = 1.8f;
			if (player.slugcatStats.bodyWeightFac > 1f)
			{
				a = 2f;
			}
			if (player.slugcatStats.bodyWeightFac >= 2f)
			{
				a = 3f;
			}
			a = Mathf.Lerp(a, 2.2f, base.abstractCreature.personality.energy);
			a = Mathf.Lerp(1f, a, base.Submersion);
			a = Mathf.Lerp(a, 1.4f, diveSpeed);
			a = Mathf.Lerp(a, 0.9f, slowDownForPrecision);
			Swim(a);
		}
		else
		{
			Swim((player.slugcatStats.bodyWeightFac > 1f) ? 1.2f : 1f);
		}
	}

	private void Swim(float speedFac)
	{
		if ((room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance) && room.GetTile(base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 20f).Terrain != 0)
		{
			MovementConnection movementConnection = (AI.pathFinder as FishPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
			if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
			{
				movementConnection = default(MovementConnection);
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
			bool flag = false;
			if (movementConnection == default(MovementConnection))
			{
				flag = true;
				movementConnection = (AI.pathFinder as FishPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
			}
			if (movementConnection != default(MovementConnection) && movementConnection.destinationCoord.TileDefined)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[flag ? 1 : 0].pos, room.MiddleOfTile(movementConnection.DestTile)) * 1.8f;
				base.bodyChunks[1].vel += Custom.RNV() * 2f * UnityEngine.Random.value + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos) * UnityEngine.Random.value;
			}
			base.mainBodyChunk.terrainSqueeze = 0.1f;
			base.bodyChunks[1].terrainSqueeze = 0.1f;
			return;
		}
		if (base.safariControlled)
		{
			if ((double)base.bodyChunks[1].submersion >= 0.5)
			{
				availableWater = 1f;
			}
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp && availableWater > 0f)
			{
				jetWater = Mathf.Clamp(jetWater + 1f / 30f, 0f, 1f);
				availableWater -= 0.015f;
			}
			else
			{
				jetWater = Mathf.Clamp(jetWater - 0.04f, 0f, 1f);
			}
		}
		else
		{
			jetWater = Mathf.Clamp(jetWater + Mathf.Lerp(-0.04f, 1f / 30f, Mathf.Pow(base.bodyChunks[1].submersion, 0.2f)), 0f, 1f);
		}
		if (waterJet != null)
		{
			waterJet.Update();
		}
		if (jetWater > 0f && base.bodyChunks[1].submersion < 0.5f)
		{
			if (waterJet == null)
			{
				waterJet = new WaterJet(room);
			}
			else if (waterJet.Dead)
			{
				waterJet = null;
			}
			else
			{
				for (int i = 0; i < 1; i++)
				{
					waterJet.NewParticle(base.bodyChunks[1].lastPos + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos) * 8f, base.bodyChunks[1].vel + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos) * Mathf.Lerp(22f, 28f, UnityEngine.Random.value) * Mathf.InverseLerp(0f, 0.2f, jetWater) + Custom.RNV() * UnityEngine.Random.value * 1f, 15f, 0.5f);
				}
			}
		}
		base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 1.4f * (1f - Mathf.Pow(base.bodyChunks[1].submersion, 0.2f)) * Mathf.InverseLerp(0f, 0.2f, jetWater) * speedFac;
		jetActive = Mathf.Lerp(jetActive, (1f - Mathf.Pow(base.bodyChunks[1].submersion, 0.2f)) * Mathf.InverseLerp(0f, 0.2f, jetWater), 0.3f);
		swimSpeed = Mathf.Lerp(Mathf.Lerp(ModManager.MMF ? 2.2f : 1.8f, 3.2f + 6f * diveSpeed, turnSpeed) * (1f + surfSpeed * 0.5f), 1.4f, slowDownForPrecision) * ((grabbedBy.Count == 0) ? 0.85f : 1f);
		base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * swimSpeed * Mathf.Pow(base.bodyChunks[1].submersion, 0.2f) * speedFac;
		base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.8f, Mathf.Pow(base.bodyChunks[1].submersion, 0.2f));
		if (swimDir.magnitude > 0f)
		{
			swimDir = Vector3.Slerp(swimDir.normalized, Custom.RNV(), 0.2f);
		}
		else
		{
			swimDir = Vector3.Slerp(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), Custom.RNV(), 0.3f);
		}
		base.mainBodyChunk.vel += swimDir * ((grabbedBy.Count == 0) ? 0.7f : 1f) * 2.5f * speedFac;
		base.bodyChunks[1].vel -= swimDir * ((grabbedBy.Count == 0) ? 0.7f : 1f) * 2.5f * speedFac;
		if (base.mainBodyChunk.submersion < base.bodyChunks[1].submersion && swimDir.y > 0f)
		{
			base.mainBodyChunk.vel.y += 4f * base.bodyChunks[1].submersion;
		}
		swimDir *= 0f;
	}

	public void CarryObject(bool eu)
	{
		if (((base.Submersion < 0.1f && UnityEngine.Random.value < 0.025f) || !Custom.DistLess(base.mainBodyChunk.pos, base.grasps[0].grabbedChunk.pos, 100f) || (!AI.denFinder.GetDenPosition().HasValue && UnityEngine.Random.value < 0.025f)) && !base.safariControlled)
		{
			LoseAllGrasps();
			return;
		}
		base.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 10f);
		base.grasps[0].grabbedChunk.vel = base.mainBodyChunk.vel;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		float num = Vector2.Distance(base.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel);
		if (otherObject is Player && num < 8f)
		{
			grabable = Math.Max(grabable, 7);
		}
		if (num > 12f && otherObject is Creature)
		{
			(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * base.bodyChunks[myChunk].mass, otherObject.bodyChunks[otherChunk], null, DamageType.Blunt, 0.1f, 10f);
			room.PlaySound(SoundID.Jet_Fish_Ram_Creature, base.mainBodyChunk);
			Vector2 pos = base.bodyChunks[myChunk].pos + Custom.DirVec(base.bodyChunks[myChunk].pos, otherObject.bodyChunks[otherChunk].pos) * base.bodyChunks[myChunk].rad;
			for (int i = 0; i < 5; i++)
			{
				room.AddObject(new Bubble(pos, Custom.RNV() * 18f * UnityEngine.Random.value, bottomBubble: false, fakeWaterBubble: false));
			}
			return;
		}
		if (myChunk == 0 && base.grasps[0] == null && AI.WantToEatObject(otherObject) && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)))
		{
			Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, 1f, overrideEquallyDominant: true, pacifying: true);
			room.PlaySound(SoundID.Jet_Fish_Grab_NPC, base.mainBodyChunk);
		}
		else if (!(otherObject is JetFish) && otherObject.bodyChunks[otherChunk].pos.y < base.bodyChunks[myChunk].pos.y && AI.attackCounter > 0)
		{
			otherObject.bodyChunks[otherChunk].vel.y -= num / otherObject.bodyChunks[otherChunk].mass;
			base.bodyChunks[myChunk].vel.y += num / 2f;
			int num2 = 30;
			if (otherObject is Creature)
			{
				SocialMemory.Relationship relationship = base.abstractCreature.state.socialMemory.GetRelationship((otherObject as Creature).abstractCreature.ID);
				if (relationship != null)
				{
					if (relationship.like > -0.5f)
					{
						relationship.like = Mathf.Lerp(relationship.like, 0f, 0.001f);
					}
					num2 = ((!(relationship.like >= 0f)) ? (30 + (int)(220f * Mathf.InverseLerp(0f, -1f, relationship.like))) : (10 + (int)(20f * Mathf.InverseLerp(1f, 0f, relationship.like))));
				}
			}
			if (AI.attackCounter > num2)
			{
				AI.attackCounter = num2;
			}
		}
		if (myChunk == 0 && base.grasps[0] == null && otherObject is Creature && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)) && ((otherObject as Creature).dead || otherObject.TotalMass < base.TotalMass * 0.7f) && base.Template.CreatureRelationship((otherObject as Creature).Template).type == CreatureTemplate.Relationship.Type.Eats)
		{
			Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, 1f, overrideEquallyDominant: true, pacifying: true);
			room.PlaySound((otherObject is Player) ? SoundID.Jet_Fish_Grab_Player : SoundID.Jet_Fish_Grab_NPC, base.mainBodyChunk);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 1.5f && firstContact)
		{
			room.PlaySound((speed < 8f) ? SoundID.Jet_Fish_Light_Terrain_Impact : SoundID.Jet_Fish_Heavy_Terrain_Impact, base.mainBodyChunk);
		}
	}

	public override void Die()
	{
		base.Die();
		waterRetardationImmunity = 0.2f;
		base.buoyancy = 0.92f;
		jetActive = 0f;
		jetSound.volume = 0f;
	}

	public override Color ShortCutColor()
	{
		return new Color(1f, 1f, 1f);
	}

	public override void Stun(int st)
	{
		if (UnityEngine.Random.value < 0.5f)
		{
			LoseAllGrasps();
		}
		base.Stun(st);
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
}
