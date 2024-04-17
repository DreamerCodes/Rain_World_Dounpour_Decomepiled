using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class Tracker : AIModule
{
	public abstract class CreatureRepresentation
	{
		public Tracker parent;

		public AbstractCreature representedCreature;

		public int forgetCounter;

		public int age;

		public RelationshipTracker.DynamicRelationship dynamicRelationship;

		private bool visualContact;

		private bool goneToOtherRoomSense;

		private int ticksSinceSeen;

		public bool deleteMeNextFrame;

		public float priority;

		public WorldCoordinate lastSeenCoord;

		public bool VisualContact
		{
			get
			{
				if (representedCreature.realizedCreature == null || parent.AI.creature.Room == null || representedCreature.realizedCreature.room != parent.AI.creature.Room.realizedRoom)
				{
					visualContact = false;
				}
				return visualContact;
			}
		}

		public int TicksSinceSeen
		{
			get
			{
				if (ticksSinceSeen < parent.seeAroundCorners)
				{
					return 0;
				}
				return ticksSinceSeen - parent.seeAroundCorners;
			}
		}

		public virtual int LowestGenerationAvailable => 1;

		public float EstimatedChanceOfFinding
		{
			get
			{
				float num = ((float)LowestGenerationAvailable * 10f + (float)TicksSinceSeen) / 4f;
				if (visualContact)
				{
					return 1f;
				}
				if (num < 45f)
				{
					return Mathf.Clamp(1f / (0f - (1f + Mathf.Pow((float)Math.E, 0f - (num / 12f - 5f)))) + 1.007f, 0f, 1f);
				}
				return 1f / (num - 7f) * 30f;
			}
		}

		public void HeardThisCreature()
		{
			if (ticksSinceSeen > parent.seeAroundCorners + 5)
			{
				ticksSinceSeen = (ticksSinceSeen - parent.seeAroundCorners) / 2;
			}
		}

		public void MakeVisible()
		{
			ticksSinceSeen = 0;
			visualContact = true;
		}

		public CreatureRepresentation(Tracker parent, AbstractCreature representedCreature, float priority)
		{
			this.parent = parent;
			this.representedCreature = representedCreature;
			this.priority = priority;
			lastSeenCoord = representedCreature.pos.WashNode();
			visualContact = true;
			goneToOtherRoomSense = true;
		}

		public virtual void Update()
		{
			age++;
			bool flag = visualContact;
			ticksSinceSeen++;
			if (ticksSinceSeen > parent.seeAroundCorners)
			{
				visualContact = false;
				if (!representedCreature.InDen && representedCreature.pos.room == parent.AI.creature.pos.room && representedCreature.realizedCreature != null)
				{
					BodyChunk[] bodyChunks = representedCreature.realizedCreature.bodyChunks;
					foreach (BodyChunk chunk in bodyChunks)
					{
						if (parent.AI.VisualContact(chunk))
						{
							ticksSinceSeen = 0;
							visualContact = true;
							break;
						}
					}
				}
			}
			if (visualContact)
			{
				if (representedCreature.realizedCreature == null || representedCreature.pos.room != parent.AI.creature.pos.room)
				{
					visualContact = false;
					return;
				}
				lastSeenCoord = representedCreature.pos.WashNode();
				if (!flag)
				{
					parent.AI.CreatureSpotted(firstSpot: false, this);
				}
				forgetCounter = 0;
				return;
			}
			forgetCounter++;
			if (goneToOtherRoomSense && representedCreature.pos.room != lastSeenCoord.room && representedCreature.realizedCreature != null && representedCreature.realizedCreature.room != null)
			{
				SenseThatCreatureHasLeftRoom();
				goneToOtherRoomSense = false;
			}
			if (parent.framesToRememberCreatures > -1 && forgetCounter > parent.framesToRememberCreatures)
			{
				Destroy();
			}
		}

		public virtual WorldCoordinate BestGuessForPosition()
		{
			return lastSeenCoord;
		}

		protected virtual void SenseThatCreatureHasLeftRoom()
		{
		}

		public void Destroy()
		{
			deleteMeNextFrame = true;
		}
	}

	public class SimpleCreatureRepresentation : CreatureRepresentation
	{
		public bool forgetWhenNotVisible;

		public SimpleCreatureRepresentation(Tracker parent, AbstractCreature representedCreature, float priority, bool forgetWhenNotVisible)
			: base(parent, representedCreature, priority)
		{
			this.forgetWhenNotVisible = forgetWhenNotVisible;
		}

		public override void Update()
		{
			base.Update();
			if (forgetWhenNotVisible && !base.VisualContact)
			{
				Destroy();
			}
		}
	}

	public class ElaborateCreatureRepresentation : CreatureRepresentation
	{
		public List<Ghost> ghosts;

		public Ghost bestGhost;

		public bool bestGhostDirty;

		private int maxGhosts;

		private int lowestGenerationAvailable;

		private bool lastVisualContact;

		public int MaxGhosts => Custom.IntClamp((int)((float)maxGhosts * Mathf.Pow(base.EstimatedChanceOfFinding, 0.25f)), 2, maxGhosts);

		public override int LowestGenerationAvailable => lowestGenerationAvailable;

		public ElaborateCreatureRepresentation(Tracker parent, AbstractCreature representedCreature, float priority, int maxGhosts)
			: base(parent, representedCreature, priority)
		{
			this.maxGhosts = maxGhosts;
			ghosts = new List<Ghost>
			{
				new Ghost(this, representedCreature.realizedCreature.mainBodyChunk.pos, representedCreature.realizedCreature.mainBodyChunk.vel)
			};
			bestGhost = ghosts[0];
			bestGhostDirty = true;
		}

		public override void Update()
		{
			base.Update();
			if (base.VisualContact)
			{
				if (ghosts.Count > 1)
				{
					ghosts.RemoveRange(1, ghosts.Count - 1);
					bestGhost = ghosts[0];
					bestGhostDirty = false;
				}
				ghosts[0].Reset();
				lastSeenCoord = ghosts[0].coord;
			}
			else
			{
				for (int num = ghosts.Count - 1; num >= 0; num--)
				{
					ghosts[num].Update();
					if (ghosts[num].DeleteMeNextFrame && ghosts.Count > 1)
					{
						ghosts.RemoveAt(num);
						bestGhostDirty = true;
					}
				}
				if (lastVisualContact)
				{
					LostVisualContact();
				}
			}
			lastVisualContact = base.VisualContact;
		}

		public override WorldCoordinate BestGuessForPosition()
		{
			if (bestGhostDirty)
			{
				FindBestGhost();
			}
			if (bestGhost != null)
			{
				if (bestGhost.coord.room == parent.AI.creature.pos.room)
				{
					return bestGhost.coord.WashNode();
				}
				return bestGhost.coord;
			}
			return new WorldCoordinate(-1, -1, -1, -1);
		}

		private void FindBestGhost()
		{
			float num = -1f;
			int num2 = 0;
			bestGhostDirty = false;
			lowestGenerationAvailable = -1;
			foreach (Ghost ghost in ghosts)
			{
				if (ghost == null)
				{
					continue;
				}
				_ = ghost.coord;
				if (parent.AI.creature != null && parent.AI.creature.world != null)
				{
					float num3 = (float)(ghost.generation * 20) + Custom.BetweenRoomsDistance(parent.AI.creature.world, parent.AI.creature.pos, ghost.coord) * 0.5f + Custom.BetweenRoomsDistance(parent.AI.creature.world, lastSeenCoord, ghost.coord);
					int num4 = 0;
					if (parent.AI.pathFinder != null)
					{
						num4 = (parent.AI.pathFinder.CoordinateReachable(ghost.coord) ? 1 : 0) + (parent.AI.pathFinder.CoordinatePossibleToGetBackFrom(ghost.coord) ? 1 : 0);
					}
					AbstractRoom abstractRoom = parent.AI.creature.world.GetAbstractRoom(ghost.coord);
					if (abstractRoom != null && abstractRoom.realizedRoom != null && abstractRoom.realizedRoom.GetTile(ghost.coord).Solid)
					{
						num3 -= 1000f;
					}
					if (num4 > num2)
					{
						num2 = num4;
						num = num3;
						bestGhost = ghost;
					}
					else if (num4 == num2 && (num < 0f || num3 <= num))
					{
						num = num3;
						bestGhost = ghost;
					}
					if (ghost.generation < lowestGenerationAvailable || lowestGenerationAvailable < 0)
					{
						lowestGenerationAvailable = ghost.generation;
					}
				}
			}
		}

		private void LostVisualContact()
		{
			if (representedCreature.Room.realizedRoom == null || !representedCreature.Room.realizedRoom.readyForAI)
			{
				bestGhostDirty = true;
				return;
			}
			bool flag = representedCreature.Room.realizedRoom.aimap.TileAccessibleToCreature(bestGhost.coord.Tile, representedCreature.creatureTemplate);
			if (!flag && representedCreature.Room.realizedRoom != null && representedCreature.Room.realizedRoom.readyForAI)
			{
				for (int i = 0; i < 4; i++)
				{
					if (representedCreature.Room.realizedRoom.aimap.TileAccessibleToCreature(bestGhost.coord.Tile + Custom.fourDirections[i], representedCreature.creatureTemplate))
					{
						flag = true;
						break;
					}
				}
			}
			flag = flag && representedCreature.realizedCreature != null && Vector2.Distance(representedCreature.realizedCreature.mainBodyChunk.lastLastPos, representedCreature.realizedCreature.mainBodyChunk.lastPos) < Mathf.Lerp(representedCreature.creatureTemplate.offScreenSpeed, 2f, 0.5f);
			Ghost ghost = ghosts[0];
			Ghost ghost2 = ghost.Clone(ghost.coord, 500);
			if (flag)
			{
				ghost.stopped = true;
				ghost.coord.Tile = representedCreature.Room.realizedRoom.aimap.TryForAccessibleNeighbor(ghost.coord.Tile, parent.AI.creature.creatureTemplate);
				ghost.pos = representedCreature.Room.realizedRoom.MiddleOfTile(ghost.coord);
				ghost2.stopped = false;
			}
			else
			{
				ghost.stopped = false;
				ghost2.stopped = true;
				ghost2.coord.Tile = representedCreature.Room.realizedRoom.aimap.TryForAccessibleNeighbor(ghost2.coord.Tile, parent.AI.creature.creatureTemplate);
				ghost2.pos = representedCreature.Room.realizedRoom.MiddleOfTile(ghost.coord);
			}
			if (parent.viz != null)
			{
				parent.viz.Update();
			}
			bestGhostDirty = true;
		}

		protected override void SenseThatCreatureHasLeftRoom()
		{
			bool flag = false;
			for (int i = 0; i < ghosts.Count; i++)
			{
				if (ghosts[i].coord.room == representedCreature.pos.room)
				{
					flag = true;
					ghosts[i].generation = 0;
				}
				else
				{
					ghosts[i].generation += 100;
				}
			}
			if (!flag && representedCreature.Room.realizedRoom != null)
			{
				Room realizedRoom = representedCreature.Room.realizedRoom;
				WorldCoordinate coord = realizedRoom.LocalCoordinateOfNode(representedCreature.pos.abstractNode);
				Ghost ghost = new Ghost(this, realizedRoom.MiddleOfTile(coord), realizedRoom.ShorcutEntranceHoleDirection(coord.Tile).ToVector2());
				ghost.forbiddenRoomExit = ghost.coord;
				ghosts.Add(ghost);
			}
			bestGhostDirty = true;
		}
	}

	public class Ghost
	{
		public ElaborateCreatureRepresentation parent;

		public Vector2 pos;

		public Vector2 vel;

		public WorldCoordinate coord;

		public WorldCoordinate lastCoord;

		public WorldCoordinate forbiddenRoomExit;

		public float moveBuffer;

		public int pushForward;

		public int generation;

		public int connectionsFollowed;

		public bool DeleteMeNextFrame;

		public bool stopped;

		public bool allowedToMoveInVisibleAreas;

		public Ghost(ElaborateCreatureRepresentation parent, Vector2 pos, Vector2 vel)
		{
			this.parent = parent;
			this.pos = pos;
			this.vel = vel;
			coord = parent.representedCreature.pos.WashNode();
			lastCoord = coord;
			moveBuffer = UnityEngine.Random.value;
			generation = 0;
			stopped = parent.parent.ghostSpeed == 0f;
			pushForward = parent.parent.ghostPush;
			allowedToMoveInVisibleAreas = false;
		}

		public void Move(WorldCoordinate newCoord, Vector2 newPos)
		{
			coord = newCoord;
			lastCoord = newCoord;
			pos = newPos;
		}

		public void Update()
		{
			Room realizedRoom = parent.representedCreature.world.GetAbstractRoom(coord.room).realizedRoom;
			if (stopped || realizedRoom == null || !realizedRoom.readyForAI)
			{
				return;
			}
			if (!realizedRoom.aimap.TileAccessibleToCreature(pos, parent.representedCreature.creatureTemplate))
			{
				pos += vel;
				if (!parent.representedCreature.creatureTemplate.canFly)
				{
					vel.y -= 0.9f;
				}
				coord.Tile = realizedRoom.GetTilePosition(pos);
				lastCoord = coord;
				if (vel.x != 0f && !NeighbourLegal(new IntVector2((!(vel.x < 0f)) ? 1 : (-1), 0)))
				{
					pos.x = realizedRoom.MiddleOfTile(pos).x;
					vel.x = 0f;
				}
				if (vel.y != 0f && !NeighbourLegal(new IntVector2(0, (!(vel.y < 0f)) ? 1 : (-1))))
				{
					pos.y = realizedRoom.MiddleOfTile(pos).y;
					vel.y = 0f;
				}
				return;
			}
			moveBuffer += parent.representedCreature.creatureTemplate.offScreenSpeed * parent.parent.ghostSpeed;
			if (!(moveBuffer > 1f))
			{
				return;
			}
			moveBuffer = 0f;
			AItile aItile = realizedRoom.aimap.getAItile(pos);
			if (realizedRoom.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && !forbiddenRoomExit.Equals(coord) && realizedRoom.shortcutData(pos).shortCutType == ShortcutData.Type.RoomExit)
			{
				AbstractRoom abstractRoom = realizedRoom.game.world.GetAbstractRoom(realizedRoom.abstractRoom.connections[realizedRoom.shortcutData(pos).destNode]);
				if (abstractRoom != null)
				{
					WorldCoordinate dest = new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.ExitIndex(coord.room));
					if (abstractRoom.realizedRoom != null && abstractRoom.realizedRoom.shortCutsReady)
					{
						dest.Tile = abstractRoom.realizedRoom.ShortcutLeadingToNode(dest.abstractNode).StartTile;
					}
					Clone(dest, 0);
					parent.ghosts[parent.ghosts.Count - 1].forbiddenRoomExit = parent.ghosts[parent.ghosts.Count - 1].coord;
				}
				DeleteMeNextFrame = true;
				return;
			}
			float num = -1f;
			MovementConnection movementConnection = default(MovementConnection);
			float num2 = -1f;
			MovementConnection movementConnection2 = default(MovementConnection);
			foreach (MovementConnection outgoingPath in aItile.outgoingPaths)
			{
				if (!ConnectionLegal(outgoingPath))
				{
					continue;
				}
				float num3 = Vector2.Distance(IntVector2.ToVector2(outgoingPath.DestTile - outgoingPath.StartTile), vel * 100f);
				if (!parent.parent.AI.creature.creatureTemplate.ConnectionResistance(outgoingPath.type).Allowed)
				{
					num3 *= 2f;
				}
				if (parent.parent.AI.pathFinder != null)
				{
					if (!parent.parent.AI.pathFinder.CoordinateReachable(outgoingPath.destinationCoord))
					{
						num3 *= 2f;
					}
					if (!parent.parent.AI.pathFinder.CoordinatePossibleToGetBackFrom(outgoingPath.destinationCoord))
					{
						num3 *= 2f;
					}
				}
				if (outgoingPath.type == MovementConnection.MovementType.DropToClimb || outgoingPath.type == MovementConnection.MovementType.DropToFloor)
				{
					num3 *= 2f;
				}
				if (num3 < num || num < 0f)
				{
					num = num3;
					movementConnection = outgoingPath;
				}
				else if (num3 < num2 || num2 < 0f)
				{
					num2 = num3;
					movementConnection2 = outgoingPath;
				}
			}
			if (movementConnection != default(MovementConnection))
			{
				if (movementConnection2 != default(MovementConnection))
				{
					if (parent.ghosts.Count < parent.MaxGhosts)
					{
						Clone(movementConnection2.destinationCoord, 1);
					}
					else
					{
						stopped = true;
					}
				}
				vel = (vel * 4f + Custom.DirVec(IntVector2.ToVector2(movementConnection.StartTile), IntVector2.ToVector2(movementConnection.DestTile))) / 5f;
				lastCoord = coord;
				coord.Tile = movementConnection.DestTile;
				pos = realizedRoom.MiddleOfTile(coord.Tile);
				connectionsFollowed++;
				{
					foreach (Ghost ghost in parent.ghosts)
					{
						if (ghost != this && ghost.coord.room == coord.room && ghost.coord.x == coord.x && ghost.coord.y == coord.y)
						{
							if (ghost.generation <= generation && !ghost.DeleteMeNextFrame)
							{
								DeleteMeNextFrame = true;
							}
							else if (!DeleteMeNextFrame)
							{
								ghost.DeleteMeNextFrame = true;
							}
						}
					}
					return;
				}
			}
			stopped = true;
		}

		public bool ConnectionLegal(MovementConnection connection)
		{
			if (connection.DestTile != lastCoord.Tile && parent.representedCreature.creatureTemplate.ConnectionResistance(connection.type).Allowed && (parent.representedCreature.world.GetAbstractRoom(connection.destinationCoord.room).realizedRoom == null || parent.representedCreature.world.GetAbstractRoom(connection.destinationCoord.room).realizedRoom.aimap.TileAccessibleToCreature(connection.DestTile, parent.representedCreature.creatureTemplate)) && IntVector2.ClampAtOne(coord.Tile - connection.DestTile) != IntVector2.ClampAtOne(coord.Tile - lastCoord.Tile))
			{
				if (!allowedToMoveInVisibleAreas)
				{
					return !parent.parent.AI.VisualContact(connection.destinationCoord, 0f);
				}
				return true;
			}
			return false;
		}

		public void Reset()
		{
			pos = parent.representedCreature.realizedCreature.mainBodyChunk.pos;
			coord = parent.representedCreature.pos.WashNode();
			lastCoord = parent.representedCreature.pos.WashNode();
			generation = 0;
			stopped = false;
			vel = parent.representedCreature.realizedCreature.bodyChunks[0].vel;
			for (int i = 1; i < parent.representedCreature.realizedCreature.bodyChunks.Length; i++)
			{
				vel += parent.representedCreature.realizedCreature.bodyChunks[i].vel;
			}
			vel /= (float)parent.representedCreature.realizedCreature.bodyChunks.Length;
		}

		public void Push()
		{
			if (pushForward < 1 && parent.ghosts.Count > 1)
			{
				DeleteMeNextFrame = true;
			}
			else
			{
				pushForward--;
				generation++;
				lastCoord = coord;
				allowedToMoveInVisibleAreas = true;
				for (int i = 0; i < parent.parent.ghostPushSpeed; i++)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						vel = Custom.DirVec(parent.parent.AI.creature.realizedCreature.mainBodyChunk.pos, pos) * 10f;
					}
					else
					{
						vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * 10f;
					}
					moveBuffer = 1f;
					stopped = false;
					Update();
					if (parent.parent.AI.creature.pos.room != coord.room)
					{
						break;
					}
				}
				allowedToMoveInVisibleAreas = false;
			}
			parent.bestGhostDirty = true;
		}

		public Ghost Clone(WorldCoordinate dest, int generationAdd)
		{
			Ghost ghost = new Ghost(parent, pos, vel);
			ghost.coord = dest;
			if (parent.parent.AI.creature.world.GetAbstractRoom(dest.room).realizedRoom != null)
			{
				ghost.pos = parent.parent.AI.creature.world.GetAbstractRoom(dest.room).realizedRoom.MiddleOfTile(dest.Tile);
			}
			ghost.lastCoord = coord;
			ghost.vel = Custom.DirVec(IntVector2.ToVector2(coord.Tile), IntVector2.ToVector2(dest.Tile));
			ghost.generation = generation + generationAdd;
			ghost.pushForward = parent.parent.ghostPush - generation;
			parent.bestGhostDirty = true;
			parent.ghosts.Add(ghost);
			return ghost;
		}

		public bool NeighbourLegal(IntVector2 nb)
		{
			Room realizedRoom = parent.representedCreature.world.GetAbstractRoom(coord.room).realizedRoom;
			if (realizedRoom == null)
			{
				return false;
			}
			if (realizedRoom.aimap.TileAccessibleToCreature(pos, parent.representedCreature.creatureTemplate))
			{
				return realizedRoom.aimap.TileAccessibleToCreature(realizedRoom.GetTilePosition(pos) + nb, parent.representedCreature.creatureTemplate);
			}
			return realizedRoom.GetTile(realizedRoom.GetTilePosition(pos) + nb).Terrain != Room.Tile.TerrainType.Solid;
		}
	}

	private List<CreatureRepresentation> creatures;

	private int seeAroundCorners;

	public int maxTrackedCreatures;

	private float ghostSpeed;

	protected int ghostPush;

	protected int ghostPushSpeed;

	protected int ghostDismissalRange;

	public bool visualize;

	protected DebugTrackerVisualizer viz;

	public int framesToRememberCreatures;

	public NoiseTracker noiseTracker;

	public int CreaturesCount => creatures.Count;

	public CreatureRepresentation GetRep(int index)
	{
		return creatures[index];
	}

	public Tracker(ArtificialIntelligence AI, int seeAroundCorners, int maxTrackedCreatures, int framesToRememberCreatures, float ghostSpeed, int ghostPush, int ghostPushSpeed, int ghostDismissalRange)
		: base(AI)
	{
		this.seeAroundCorners = seeAroundCorners;
		this.maxTrackedCreatures = maxTrackedCreatures;
		this.framesToRememberCreatures = framesToRememberCreatures;
		this.ghostSpeed = ghostSpeed;
		this.ghostPush = ghostPush;
		this.ghostPushSpeed = ghostPushSpeed;
		this.ghostDismissalRange = ghostDismissalRange;
		creatures = new List<CreatureRepresentation>();
	}

	public override void Update()
	{
		if (visualize)
		{
			if (viz != null)
			{
				viz.Update();
			}
			else
			{
				viz = new DebugTrackerVisualizer(this);
			}
		}
		if (creatures.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, creatures.Count);
			if (creatures[index] is ElaborateCreatureRepresentation && (creatures[index] as ElaborateCreatureRepresentation).ghosts.Count > 0)
			{
				Ghost ghost = (creatures[index] as ElaborateCreatureRepresentation).ghosts[UnityEngine.Random.Range(0, (creatures[index] as ElaborateCreatureRepresentation).ghosts.Count)];
				if (!ghost.parent.VisualContact && !ghost.DeleteMeNextFrame && AI.creature.pos.room == ghost.coord.room && AI.VisualContact(ghost.pos, 0f) && (Custom.ManhattanDistance(AI.creature.pos.Tile, ghost.coord.Tile) < 2 || (Vector2.Distance(IntVector2.ToVector2(AI.creature.pos.Tile), IntVector2.ToVector2(ghost.coord.Tile)) < (float)ghostDismissalRange && AI.VisualContact(ghost.coord, 0f))))
				{
					ghost.Push();
				}
			}
			else if (!creatures[index].VisualContact && AI.creature.Room.realizedRoom != null && AI.VisualContact(AI.creature.Room.realizedRoom.MiddleOfTile(creatures[index].BestGuessForPosition()), 0f) && (Custom.ManhattanDistance(AI.creature.pos.Tile, creatures[index].BestGuessForPosition().Tile) < 2 || (Vector2.Distance(IntVector2.ToVector2(AI.creature.pos.Tile), IntVector2.ToVector2(creatures[index].BestGuessForPosition().Tile)) < (float)ghostDismissalRange && AI.VisualContact(creatures[index].BestGuessForPosition(), 0f))))
			{
				creatures[index].Destroy();
			}
			if (creatures[index].VisualContact && creatures[index].representedCreature.state.dead && AI.TrackerToDiscardDeadCreature(creatures[index].representedCreature))
			{
				creatures[index].Destroy();
			}
		}
		if (AI.creature.Room.creatures.Count <= 0)
		{
			return;
		}
		AbstractCreature abstractCreature = AI.creature.Room.creatures[UnityEngine.Random.Range(0, AI.creature.Room.creatures.Count)];
		bool flag = abstractCreature != AI.creature;
		for (int num = creatures.Count - 1; num >= 0; num--)
		{
			creatures[num].Update();
			if (creatures[num].representedCreature.Equals(abstractCreature))
			{
				flag = false;
			}
			if (creatures[num].deleteMeNextFrame)
			{
				creatures.RemoveAt(num);
			}
		}
		if (!flag || abstractCreature.realizedCreature == null || abstractCreature.realizedCreature.inShortcut)
		{
			return;
		}
		BodyChunk[] bodyChunks = abstractCreature.realizedCreature.bodyChunks;
		foreach (BodyChunk chunk in bodyChunks)
		{
			if (AI.VisualContact(chunk))
			{
				CreatureNoticed(abstractCreature);
				break;
			}
		}
	}

	public CreatureRepresentation RepresentationForObject(PhysicalObject obj, bool AddIfMissing)
	{
		if (obj is Creature)
		{
			CreatureRepresentation creatureRepresentation = null;
			foreach (CreatureRepresentation creature in creatures)
			{
				if (creature.representedCreature.realizedCreature == obj)
				{
					creatureRepresentation = creature;
					break;
				}
			}
			if (creatureRepresentation == null && AddIfMissing)
			{
				creatureRepresentation = CreatureNoticed((obj as Creature).abstractCreature);
			}
			return creatureRepresentation;
		}
		return null;
	}

	public CreatureRepresentation RepresentationForCreature(AbstractCreature checkCrit, bool addIfMissing)
	{
		CreatureRepresentation creatureRepresentation = null;
		foreach (CreatureRepresentation creature in creatures)
		{
			if (creature.representedCreature == checkCrit)
			{
				creatureRepresentation = creature;
				break;
			}
		}
		if (creatureRepresentation == null && addIfMissing)
		{
			creatureRepresentation = CreatureNoticed(checkCrit);
		}
		return creatureRepresentation;
	}

	private CreatureRepresentation CreatureNoticed(AbstractCreature crit)
	{
		if (crit.realizedCreature == null)
		{
			return null;
		}
		if (crit.state.dead && AI.TrackerToDiscardDeadCreature(crit))
		{
			return null;
		}
		if (AI.StaticRelationship(crit).type == CreatureTemplate.Relationship.Type.DoesntTrack)
		{
			return null;
		}
		bool flag = false;
		if (AI.creature.creatureTemplate.grasps > 0)
		{
			foreach (AbstractPhysicalObject.AbstractObjectStick stuckObject in AI.creature.stuckObjects)
			{
				if (stuckObject.A == crit || stuckObject.B == crit)
				{
					flag = true;
					break;
				}
			}
		}
		CreatureRepresentation creatureRepresentation = AI.CreateTrackerRepresentationForCreature(crit);
		if (creatureRepresentation == null)
		{
			return null;
		}
		AI.CreatureSpotted(!flag, creatureRepresentation);
		if (AI.relationshipTracker != null)
		{
			AI.relationshipTracker.EstablishDynamicRelationship(creatureRepresentation);
		}
		if (creatureRepresentation != null)
		{
			creatures.Add(creatureRepresentation);
			if (creatures.Count > maxTrackedCreatures)
			{
				float num = float.MaxValue;
				CreatureRepresentation creatureRepresentation2 = null;
				foreach (CreatureRepresentation creature in creatures)
				{
					float num2 = ((creature.dynamicRelationship != null) ? creature.dynamicRelationship.currentRelationship.intensity : AI.creature.creatureTemplate.CreatureRelationship(creature.representedCreature.creatureTemplate).intensity) * 100000f + (creature.VisualContact ? 2f : 1f) / (1f + Vector2.Distance(IntVector2.ToVector2(creature.BestGuessForPosition().Tile), IntVector2.ToVector2(AI.creature.pos.Tile)));
					num2 /= Mathf.Lerp(creature.forgetCounter, 100f, 0.7f);
					if (num2 < num)
					{
						num = num2;
						creatureRepresentation2 = creature;
					}
				}
				if (creatureRepresentation2 == creatureRepresentation)
				{
					creatureRepresentation = null;
				}
				creatureRepresentation2.Destroy();
			}
		}
		return creatureRepresentation;
	}

	public void SeeCreature(AbstractCreature crit)
	{
		bool flag = false;
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].representedCreature == crit)
			{
				creatures[i].MakeVisible();
				flag = true;
			}
		}
		if (!flag)
		{
			CreatureNoticed(crit);
		}
	}

	public void ForgetCreature(AbstractCreature crit)
	{
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].representedCreature == crit)
			{
				creatures[i].Destroy();
				break;
			}
		}
	}
}
