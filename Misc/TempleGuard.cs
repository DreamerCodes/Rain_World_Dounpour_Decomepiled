using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class TempleGuard : Creature
{
	public TempleGuardAI AI;

	public bool moving;

	public WorldCoordinate spawnPosition;

	public Vector2 telekineticPoint;

	public Vector2 telekineticDir;

	public float telekinesis;

	public Vector2 StoneDir => Custom.DirVec(Vector2.Lerp(base.bodyChunks[1].pos, base.bodyChunks[2].pos, 0.5f), Vector2.Lerp(base.bodyChunks[3].pos, base.bodyChunks[4].pos, 0.5f));

	public TempleGuard(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		List<Vector2> list = new List<Vector2>
		{
			new Vector2(0f, 0f),
			new Vector2(-15f, 10f),
			new Vector2(15f, 10f),
			new Vector2(15f, -10f),
			new Vector2(-15f, -10f)
		};
		base.bodyChunks = new BodyChunk[list.Count];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 17.5f, 4f);
		for (int i = 1; i < list.Count; i++)
		{
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), 9.5f, 10f / (float)list.Count);
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * (base.bodyChunks.Length - 1) / 2];
		int num = 0;
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			for (int k = j + 1; k < base.bodyChunks.Length; k++)
			{
				bodyChunkConnections[num] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[k], Vector2.Distance(list[j], list[k]), BodyChunkConnection.Type.Normal, 1f, -1f);
				num++;
			}
		}
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.35f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.92f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new TempleGuardGraphics(this);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((i == 1) ? 10f : 5f) + Custom.DegToVec(Random.value * 360f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 5f;
		}
		shortcutDelay = 80;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is TempleGuard && base.Consious)
		{
			Vector2 vector = Custom.RNV() * 6f * Random.value;
			base.mainBodyChunk.vel += vector;
			otherObject.firstChunk.vel -= vector;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		int num = 0;
		for (int i = 0; i < placeRoom.abstractRoom.creatures.Count; i++)
		{
			if (placeRoom.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.TempleGuard)
			{
				if (placeRoom.abstractRoom.creatures[i] == base.abstractCreature)
				{
					break;
				}
				num++;
			}
		}
		int num2 = -1;
		for (int j = 0; j < placeRoom.roomSettings.placedObjects.Count; j++)
		{
			if (placeRoom.roomSettings.placedObjects[j].type == PlacedObject.Type.TempleGuard)
			{
				num2++;
				if (num2 == num)
				{
					base.abstractCreature.pos.Tile = placeRoom.GetTilePosition(placeRoom.roomSettings.placedObjects[j].pos);
				}
			}
		}
		base.PlaceInRoom(placeRoom);
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		spawnPosition = new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, base.abstractCreature.pos.y, base.abstractCreature.pos.abstractNode);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public override void Update(bool eu)
	{
		CheckFlip();
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel *= 0.9f;
			}
			Stun(12);
		}
		moving = false;
		if (base.Consious)
		{
			Act(eu);
		}
		if (AI.telekinesis && base.Consious)
		{
			telekinesis = Mathf.Min(1f, telekinesis + 0.05f);
		}
		else
		{
			telekinesis = Mathf.Max(0f, telekinesis - 0.05f);
		}
	}

	public void Act(bool eu)
	{
		AI.Update();
		Vector2 vector = default(Vector2);
		if (base.safariControlled)
		{
			int num = 25;
			IntVector2 tile = base.abstractCreature.pos.Tile;
			for (int i = 0; i < 25; i++)
			{
				if (room.GetTile(new IntVector2(tile.x, tile.y - i)).Solid)
				{
					num = i;
					break;
				}
			}
			if (inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0) && !inputWithoutDiagonals.Value.pckp)
			{
				MovementConnection movementConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * 40f), 2);
				vector = Vector2.ClampMagnitude(room.MiddleOfTile(movementConnection.destinationCoord) - base.mainBodyChunk.pos, 30f) / 30f;
				if (vector.magnitude > 1f)
				{
					vector.Normalize();
				}
			}
			if (num <= 9)
			{
				for (int j = 0; j < base.bodyChunks.Length; j++)
				{
					BodyChunk bodyChunk = base.bodyChunks[j];
					bodyChunk.vel.y = bodyChunk.vel.y + (float)(9 - num) * 0.04f;
				}
			}
			if (num > 15)
			{
				for (int k = 0; k < base.bodyChunks.Length; k++)
				{
					BodyChunk bodyChunk2 = base.bodyChunks[k];
					bodyChunk2.vel.y = bodyChunk2.vel.y - (float)(num - 15) * 0.025f;
				}
			}
		}
		else if (AI.bowDown)
		{
			vector.y = -1f;
		}
		else if (Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetDestination) < 3)
		{
			vector = Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.mainBodyChunk.pos, 30f) / 30f;
		}
		else
		{
			moving = true;
			MovementConnection movementConnection2 = default(MovementConnection);
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				if (!(movementConnection2 == default(MovementConnection)))
				{
					break;
				}
				movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[l].pos), actuallyFollowingThisPath: true);
			}
			if (movementConnection2 != default(MovementConnection))
			{
				vector = Vector2.ClampMagnitude(room.MiddleOfTile(movementConnection2.destinationCoord) - base.mainBodyChunk.pos, 30f) / 30f;
			}
			for (int m = 0; m < 15; m++)
			{
				movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: false);
				if (!(movementConnection2 != default(MovementConnection)) || !AI.pathFinder.RayTraceInAccessibleTiles(base.abstractCreature.pos.Tile, movementConnection2.DestTile))
				{
					break;
				}
				vector += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection2.destinationCoord));
			}
			if (vector.magnitude > 1f)
			{
				vector.Normalize();
			}
			if (Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) < 3)
			{
				vector = Vector2.Lerp(vector, Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetEffectualDestination) - base.mainBodyChunk.pos, 30f) / 30f, 0.5f);
			}
		}
		float num2 = 1f;
		if (!base.safariControlled && ModManager.MMF && !MMF.cfgVanillaExploits.Value)
		{
			Vector2 rhs = Custom.DirVec(base.abstractCreature.pos.Tile.ToVector2(), spawnPosition.Tile.ToVector2());
			if (base.abstractCreature.pos.room == spawnPosition.room && base.abstractCreature.pos.x - spawnPosition.x >= 10 && Vector2.Dot(vector, rhs) < 0f)
			{
				num2 = 0f;
			}
		}
		for (int n = 0; n < base.bodyChunks.Length; n++)
		{
			base.bodyChunks[n].vel *= 0.96f;
			base.bodyChunks[n].vel.y += base.gravity * Mathf.Lerp(1f, 0.5f, room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt));
			base.bodyChunks[n].vel += vector * 0.2f;
			base.bodyChunks[n].vel.x *= num2;
			if (n > 0)
			{
				base.bodyChunks[n].vel.y += 0.9f * ((n < 3) ? 1f : (-1f));
			}
		}
		telekineticPoint += Vector2.ClampMagnitude(AI.telekinGetToPoint - telekineticPoint, 30f);
		telekineticPoint = Vector2.Lerp(telekineticPoint, AI.telekinGetToPoint, AI.floorSlam ? 0.9f : 0.1f);
		if (!AI.telekinesis)
		{
			return;
		}
		telekineticDir = Vector2.Lerp(telekineticDir, AI.telekinGetToDir, 0.5f);
		for (int num3 = 0; num3 < room.physicalObjects.Length; num3++)
		{
			for (int num4 = 0; num4 < room.physicalObjects[num3].Count; num4++)
			{
				for (int num5 = 0; num5 < room.physicalObjects[num3][num4].bodyChunks.Length; num5++)
				{
					if (!Custom.DistLess(telekineticPoint, room.physicalObjects[num3][num4].bodyChunks[num5].pos, AI.floorSlam ? 300f : 200f))
					{
						continue;
					}
					float num6 = Mathf.InverseLerp(200f, AI.floorSlam ? 150f : 100f, Vector2.Distance(telekineticPoint, room.physicalObjects[num3][num4].bodyChunks[num5].pos));
					num6 /= Mathf.Lerp(room.physicalObjects[num3][num4].bodyChunks[num5].mass, 0.7f, 0.5f);
					float num7 = 0f;
					if (base.safariControlled)
					{
						num7 = 1f;
						if (room.physicalObjects[num3][num4] is Creature)
						{
							(room.physicalObjects[num3][num4] as Creature).Stun(20);
						}
					}
					room.physicalObjects[num3][num4].bodyChunks[num5].vel += (telekineticDir * 0.8f + Custom.RNV() * 1.3f) * telekinesis * num6 * 0.7f + num7 * telekineticDir;
					if (AI.floorSlam && room.physicalObjects[num3][num4] == AI.pickUpObject)
					{
						room.physicalObjects[num3][num4].bodyChunks[num5].vel.y += (AI.floorSlamDir ? 1f : (-1f)) * num6 * telekinesis * (AI.floorSlamDir ? 0.2f : 2.2f);
					}
				}
			}
		}
	}

	public override void Blind(int blnd)
	{
		if (!ModManager.MMF || MMF.cfgVanillaExploits.Value)
		{
			base.Blind(blnd);
		}
		else
		{
			base.Blind(0);
		}
	}

	private void CheckFlip()
	{
		if (Custom.DistanceToLine(base.bodyChunks[1].pos, base.bodyChunks[0].pos, base.bodyChunks[0].pos + StoneDir) < 0f)
		{
			Vector2 pos = base.bodyChunks[1].pos;
			Vector2 vel = base.bodyChunks[1].vel;
			Vector2 lastPos = base.bodyChunks[1].lastPos;
			base.bodyChunks[1].pos = base.bodyChunks[2].pos;
			base.bodyChunks[1].vel = base.bodyChunks[2].vel;
			base.bodyChunks[1].lastPos = base.bodyChunks[2].lastPos;
			base.bodyChunks[1].pos = pos;
			base.bodyChunks[1].vel = vel;
			base.bodyChunks[1].lastPos = lastPos;
			pos = base.bodyChunks[3].pos;
			vel = base.bodyChunks[3].vel;
			lastPos = base.bodyChunks[3].lastPos;
			base.bodyChunks[3].pos = base.bodyChunks[4].pos;
			base.bodyChunks[3].vel = base.bodyChunks[4].vel;
			base.bodyChunks[3].lastPos = base.bodyChunks[4].lastPos;
			base.bodyChunks[4].pos = pos;
			base.bodyChunks[4].vel = vel;
			base.bodyChunks[4].lastPos = lastPos;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 1.5f && firstContact)
		{
			float num = Mathf.InverseLerp(6f, 14f, speed);
			if (num < 1f)
			{
				room.PlaySound(SoundID.Vulture_Light_Terrain_Impact, base.mainBodyChunk, loop: false, 1f - num, 1f);
			}
			if (num > 0f)
			{
				room.PlaySound(SoundID.Vulture_Heavy_Terrain_Impact, base.mainBodyChunk, loop: false, num, 1f);
			}
		}
	}

	public override void Stun(int st)
	{
		base.Stun(0);
	}

	public override void Die()
	{
		base.Die();
	}
}
