using System;
using RWCustom;
using Smoke;
using UnityEngine;

public class Hazer : Creature, IPlayerEdible
{
	public float swallowed;

	public PlacedObject placedObj;

	public IntVector2 lastAirTile;

	public int hopDir;

	public int moveCounter;

	public BlackHaze smoke;

	public Vector2 sprayDir;

	public Vector2 getToSprayDir;

	public Vector2? sprayStuckPos;

	public float inkLeft;

	public bool spraying;

	public bool tossed;

	public Vector2 swimDir;

	public float swimCycle;

	public float swim;

	public float floatHeight;

	public bool hasSprayed;

	private int clds;

	private int bites = 3;

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

	public Hazer(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		inkLeft = (abstractCreature.state.alive ? 1f : 0f);
		hasSprayed = !abstractCreature.state.alive;
		float num = 0.27f;
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), base.State.alive ? 4f : 3f, num * 0.5f);
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), base.State.alive ? 3.5f : 3f, num * 0.3f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 3f, num * 0.2f);
		bodyChunkConnections = new BodyChunkConnection[2];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[0], 5f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 5f, BodyChunkConnection.Type.Normal, 1f, -1f);
		base.airFriction = 0.995f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.1f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		hopDir = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		moveCounter = -UnityEngine.Random.Range(120, 2500);
		getToSprayDir = Custom.RNV();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if ((base.State as VultureGrub.VultureGrubState).origRoom > -1 && (base.State as VultureGrub.VultureGrubState).origRoom == placeRoom.abstractRoom.index && (base.State as VultureGrub.VultureGrubState).placedObjectIndex >= 0 && (base.State as VultureGrub.VultureGrubState).placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			placedObj = placeRoom.roomSettings.placedObjects[(base.State as VultureGrub.VultureGrubState).placedObjectIndex];
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new HazerGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
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
		if (spraying)
		{
			inkLeft = Mathf.Max(0f, inkLeft - 0.0045454544f);
			if (!base.dead)
			{
				sprayDir = Vector3.Slerp(sprayDir, getToSprayDir, 0.2f);
				if (UnityEngine.Random.value < 0.1f)
				{
					getToSprayDir = Custom.DegToVec(-90f + 180f * UnityEngine.Random.value);
				}
				WeightedPush(1, 2, sprayDir, 5f);
			}
			if (inkLeft <= 0f)
			{
				for (int j = 0; j < base.bodyChunks.Length; j++)
				{
					base.bodyChunks[j].rad = 3f;
				}
				spraying = false;
				Die();
			}
			if (sprayStuckPos.HasValue)
			{
				if (Custom.DistLess(sprayStuckPos.Value, base.bodyChunks[2].pos, 15f) && grabbedBy.Count == 0)
				{
					base.bodyChunks[2].pos = sprayStuckPos.Value;
				}
				else
				{
					sprayStuckPos = null;
				}
			}
			else if (base.bodyChunks[2].ContactPoint.y < 0 && grabbedBy.Count == 0)
			{
				sprayStuckPos = base.bodyChunks[2].pos;
			}
		}
		else
		{
			sprayStuckPos = null;
		}
		if (smoke != null)
		{
			if (smoke.room != room || smoke.slatedForDeletetion)
			{
				smoke = null;
			}
			else
			{
				smoke.MoveTo(ChunkInOrder(0).pos, eu);
				if (spraying)
				{
					smoke.EmitSmoke(Vector3.Slerp(Custom.DirVec(ChunkInOrder(1).pos, ChunkInOrder(0).pos), sprayDir, 0.4f) * 20f, 1f);
					if ((1f - inkLeft) * 3f > (float)clds)
					{
						smoke.EmitBigSmoke(Mathf.InverseLerp(2f, 0f, clds));
						clds++;
					}
				}
			}
		}
		else if (spraying)
		{
			smoke = new BlackHaze(room, ChunkInOrder(0).pos);
			room.AddObject(smoke);
		}
		if (placedObj != null && grabbedBy.Count > 0)
		{
			if (room.game.session is StoryGameSession)
			{
				(room.game.session as StoryGameSession).saveState.ReportConsumedItem(room.world, karmaFlower: false, (base.State as VultureGrub.VultureGrubState).origRoom, (base.State as VultureGrub.VultureGrubState).placedObjectIndex, UnityEngine.Random.Range((placedObj.data as PlacedObject.ConsumableObjectData).minRegen, (placedObj.data as PlacedObject.ConsumableObjectData).maxRegen));
			}
			placedObj = null;
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
		if (grabbedBy.Count > 0)
		{
			Vector2 dir = Custom.PerpendicularVector(Custom.DirVec(base.bodyChunks[0].pos, grabbedBy[0].grabber.mainBodyChunk.pos)) * ((grabbedBy[0].graspUsed == 0) ? 1f : (-1f));
			WeightedPush(1, 2, dir, 4f);
			moveCounter = -Math.Abs(moveCounter);
			tossed = false;
			base.CollideWithSlopes = false;
			base.CollideWithTerrain = false;
			base.GoThroughFloors = true;
			base.CollideWithObjects = false;
		}
		else
		{
			base.CollideWithSlopes = true;
			base.CollideWithTerrain = true;
			base.GoThroughFloors = false;
			base.CollideWithObjects = true;
			if (!base.dead && !spraying)
			{
				Act();
			}
			else if (base.safariControlled && !base.dead && spraying && (!inputWithoutDiagonals.HasValue || !inputWithoutDiagonals.Value.thrw))
			{
				spraying = false;
			}
			else
			{
				swim = Mathf.Max(0f, swim - 1f / 30f);
			}
		}
		if (tossed)
		{
			moveCounter = 0;
			if (room.GetTile(base.mainBodyChunk.pos).DeepWater || base.dead || inkLeft < 0.5f)
			{
				tossed = false;
			}
			else if (base.bodyChunks[0].ContactPoint.y < 0)
			{
				spraying = true;
				tossed = false;
				hasSprayed = true;
				moveCounter = -1000;
			}
		}
		bool flag = false;
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
		{
			int num = -1;
			for (int k = 0; k < 2; k++)
			{
				if ((grabbedBy[0].grabber as Player).grasps[k] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[k].grabbed))
				{
					num = k;
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
		if (grabbedBy.Count > 0)
		{
			return;
		}
		if (base.safariControlled)
		{
			if (inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0))
			{
				if (moveCounter < 0)
				{
					moveCounter = 0;
				}
				moveCounter++;
				swimDir = new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y);
				hopDir = (int)Mathf.Sign(inputWithoutDiagonals.Value.x);
			}
			else
			{
				moveCounter = -10;
			}
			if (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.thrw && inkLeft > 0f)
			{
				spraying = true;
			}
			if (base.graphicsModule != null)
			{
				if (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.jmp)
				{
					(base.graphicsModule as HazerGraphics).camoGetTo = 1f;
				}
				else
				{
					(base.graphicsModule as HazerGraphics).camoGetTo = 0f;
				}
			}
		}
		else
		{
			moveCounter++;
		}
		if (base.Submersion < 0.8f || (room.GetTile(base.mainBodyChunk.pos).WaterSurface && room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, -20f)).Solid))
		{
			swim = Mathf.Max(0f, swim - 1f / 30f);
			if (room.GetTile(base.mainBodyChunk.pos).AnyWater)
			{
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					base.bodyChunks[i].vel.y -= 0.3f;
				}
			}
			if (base.bodyChunks[1].ContactPoint.y > -1 && base.bodyChunks[0].ContactPoint.y > -1)
			{
				return;
			}
			if (moveCounter > 0 && moveCounter % 6 == 0)
			{
				if (room.readyForAI && (room.aimap.getAItile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(hopDir, 0)).floorAltitude > 2 || room.aimap.getAItile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(hopDir * 2, 0)).floorAltitude > 2))
				{
					hopDir = -hopDir;
					moveCounter = -UnityEngine.Random.Range(60, 120);
				}
				else
				{
					WeightedPush(2, 1, new Vector2(hopDir, 0f), 4f);
					base.bodyChunks[1].vel += new Vector2(-hopDir, 0f);
					base.bodyChunks[0].vel += new Vector2((float)hopDir * 3f, 4f);
					base.bodyChunks[2].vel += new Vector2((float)hopDir * 3f, 4f);
					room.PlaySound(SoundID.Hazer_Shuffle, base.mainBodyChunk);
					if (moveCounter > UnityEngine.Random.Range(30, 400))
					{
						if (UnityEngine.Random.value < 1f / 3f)
						{
							hopDir = -hopDir;
						}
						moveCounter = -UnityEngine.Random.Range(120, 2500);
					}
				}
			}
			if (UnityEngine.Random.value < 0.1f && base.bodyChunks[1].ContactPoint.x != 0)
			{
				hopDir = -base.bodyChunks[1].ContactPoint.x;
			}
			if (UnityEngine.Random.value < 0.1f && base.bodyChunks[2].ContactPoint.x != 0)
			{
				hopDir = -base.bodyChunks[1].ContactPoint.x;
			}
		}
		else if (moveCounter > 0)
		{
			swim = Mathf.Min(1f, swim + 1f / 30f);
			swimCycle += swim / 18f;
			swimDir = (swimDir + Custom.RNV() * UnityEngine.Random.value * 0.1f).normalized;
			if (room.readyForAI)
			{
				Vector2 vector = new Vector2(0f, 0f);
				IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[2].pos);
				int terrainProximity = room.aimap.getTerrainProximity(base.bodyChunks[2].pos);
				for (int j = 1; j < 3; j++)
				{
					for (int k = 0; k < 8; k++)
					{
						if (terrainProximity < 3 && room.aimap.getTerrainProximity(tilePosition + Custom.eightDirections[k] * j) > terrainProximity)
						{
							vector += Custom.eightDirections[k].ToVector2() * UnityEngine.Random.value / j;
						}
						else if (!room.GetTile(tilePosition + Custom.eightDirections[k] * j).AnyWater)
						{
							vector -= Custom.eightDirections[k].ToVector2() * 0.1f * UnityEngine.Random.value / j;
						}
					}
				}
				swimDir = (swimDir + Vector2.ClampMagnitude(vector, 1f) * UnityEngine.Random.value).normalized;
			}
			base.bodyChunks[2].vel += swimDir;
			if (moveCounter > UnityEngine.Random.Range(120, 8000))
			{
				moveCounter = -UnityEngine.Random.Range(120, 800);
			}
			floatHeight = Mathf.Max(30f, Mathf.Abs((float)room.defaultWaterLevel * 20f - base.bodyChunks[2].pos.y));
		}
		else
		{
			swim = Mathf.Max(0f, swim - 1f / 130f);
			swimCycle += 1f / 160f;
			float num = (float)room.defaultWaterLevel * 20f - floatHeight + Mathf.Sin(swimCycle * (float)Math.PI * 2f) * 10f;
			ChunkInOrder(0).vel.y += 0.25f * (1f - swim);
			ChunkInOrder(1).vel.y -= Mathf.Clamp(base.bodyChunks[0].pos.y - num, -0.6f, 0.6f) * (1f - swim);
			ChunkInOrder(2).vel.y -= 0.25f * (1f - swim);
			if (swimDir.y < 0.5f)
			{
				swimDir = (swimDir + new Vector2(0f, 0.1f)).normalized;
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (moveCounter < 0 && otherObject is Creature && !(otherObject as Creature).dead && !(otherObject is Hazer))
		{
			moveCounter /= 2;
			if (UnityEngine.Random.value < 0.2f)
			{
				hopDir = (int)Mathf.Sign(base.mainBodyChunk.pos.x - otherObject.bodyChunks[otherChunk].pos.x);
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override void Die()
	{
		base.Die();
		if (!hasSprayed && inkLeft > 0.5f)
		{
			spraying = true;
			hasSprayed = true;
		}
	}

	public void SpitOutByPlayer(Color playerCol)
	{
		if (base.graphicsModule != null && !(UnityEngine.Random.value < 0.6f))
		{
			(base.graphicsModule as HazerGraphics).camo = 1f;
			(base.graphicsModule as HazerGraphics).lastCamo = 0.9f;
			(base.graphicsModule as HazerGraphics).camoGetTo = 1f;
			(base.graphicsModule as HazerGraphics).camoPickupColor = playerCol;
			(base.graphicsModule as HazerGraphics).camoColor = playerCol;
		}
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
