using RWCustom;
using UnityEngine;

public class MouseAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public struct Dangle
	{
		public WorldCoordinate attachedPos;

		public WorldCoordinate bodyPos;

		public Dangle(WorldCoordinate bodyPos, WorldCoordinate attachedPos)
		{
			this.attachedPos = attachedPos;
			this.bodyPos = bodyPos;
		}
	}

	public LanternMouse mouse;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public float fear;

	public bool wantToSleep;

	public float pullUp;

	public Behavior behavior;

	public WorldCoordinate? walkWithMouse;

	public int dangleChecksCounter;

	private int idlePosCounter;

	public Dangle? dangle;

	public MouseAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		mouse = creature.realizedCreature as LanternMouse;
		mouse.AI = this;
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		dangleChecksCounter = 0;
	}

	public override void Update()
	{
		base.Update();
		if (mouse.room == null)
		{
			return;
		}
		if (ModManager.MSC && mouse.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(mouse.LickedByPlayer.abstractCreature);
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value) || base.threatTracker.Utility() > 0.95f;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
		}
		if (currentUtility < 0.2f)
		{
			behavior = Behavior.Idle;
		}
		if (!mouse.safariControlled && mouse.ropeAttatchedPos.HasValue && (!dangle.HasValue || dangle.Value.attachedPos.Tile != mouse.room.GetTilePosition(mouse.ropeAttatchedPos.Value) || (behavior != Behavior.Idle && Random.value < base.threatTracker.Panic * 0.5f)))
		{
			mouse.DetatchRope();
		}
		if (behavior == Behavior.Idle)
		{
			mouse.runSpeed = Mathf.Lerp(mouse.runSpeed, 0.5f, 0.05f);
			ReconsiderDanglePos();
			if (!base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				creature.abstractAI.SetDestination(creature.pos);
			}
			if (walkWithMouse.HasValue)
			{
				creature.abstractAI.SetDestination(walkWithMouse.Value);
				if (Random.value < 0.02f || Custom.ManhattanDistance(creature.pos, walkWithMouse.Value) < 4)
				{
					walkWithMouse = null;
				}
			}
			else if (dangle.HasValue && dangle.Value.attachedPos.room == mouse.room.abstractRoom.index)
			{
				for (int i = 0; i < Custom.eightDirectionsAndZero.Length; i++)
				{
					if (base.pathFinder.CoordinateReachableAndGetbackable(mouse.room.GetWorldCoordinate(dangle.Value.attachedPos.Tile + Custom.eightDirectionsAndZero[i])))
					{
						IntVector2 intVector = dangle.Value.attachedPos.Tile + Custom.eightDirectionsAndZero[i];
						creature.abstractAI.SetDestination(mouse.room.GetWorldCoordinate(intVector));
						if (mouse.room.GetTilePosition(mouse.bodyChunks[0].pos) == intVector || mouse.room.GetTilePosition(mouse.bodyChunks[1].pos) == intVector)
						{
							mouse.AttatchRope(dangle.Value.attachedPos.Tile);
						}
						break;
					}
					if (!mouse.room.GetTile(dangle.Value.attachedPos.Tile + Custom.eightDirectionsAndZero[i]).Solid && base.pathFinder.CoordinateReachableAndGetbackable(mouse.room.GetWorldCoordinate(dangle.Value.attachedPos.Tile + Custom.eightDirectionsAndZero[i] + new IntVector2(0, 1))))
					{
						IntVector2 intVector2 = dangle.Value.attachedPos.Tile + Custom.eightDirectionsAndZero[i] + new IntVector2(0, 1);
						creature.abstractAI.SetDestination(mouse.room.GetWorldCoordinate(intVector2));
						if (mouse.room.GetTilePosition(mouse.bodyChunks[0].pos) == intVector2 || mouse.room.GetTilePosition(mouse.bodyChunks[1].pos) == intVector2)
						{
							mouse.AttatchRope(dangle.Value.attachedPos.Tile);
						}
						break;
					}
				}
			}
			else
			{
				dangle = null;
				bool flag = base.pathFinder.GetDestination.room != mouse.room.abstractRoom.index;
				if (!flag && dangleChecksCounter > 200)
				{
					int abstractNode = mouse.room.abstractRoom.RandomNodeInRoom().abstractNode;
					if (mouse.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
					{
						int num = mouse.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, mouse.Template);
						if (num > -1 && mouse.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(mouse.abstractCreature.pos.Tile, num, mouse.Template) > -1)
						{
							AbstractRoom abstractRoom = mouse.room.game.world.GetAbstractRoom(mouse.room.abstractRoom.connections[abstractNode]);
							if (abstractRoom != null)
							{
								WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
								if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
								{
									creature.abstractAI.SetDestination(worldCoordinate);
									idlePosCounter = Random.Range(200, 500);
									flag = true;
								}
							}
						}
					}
				}
				if (!flag && (Random.value < 0.0045454544f || idlePosCounter <= 0))
				{
					IntVector2 pos = new IntVector2(Random.Range(0, mouse.room.TileWidth), Random.Range(0, mouse.room.TileHeight));
					if (base.pathFinder.CoordinateReachableAndGetbackable(mouse.room.GetWorldCoordinate(pos)))
					{
						creature.abstractAI.SetDestination(mouse.room.GetWorldCoordinate(pos));
						idlePosCounter = Random.Range(200, 1900);
					}
				}
				if (!mouse.sitting)
				{
					idlePosCounter--;
				}
				idlePosCounter--;
			}
		}
		else if (behavior == Behavior.Flee)
		{
			mouse.runSpeed = Mathf.Lerp(mouse.runSpeed, 1f, 0.08f);
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 6, 20, considerLeavingRoom: true));
		}
		else if (behavior == Behavior.EscapeRain)
		{
			mouse.runSpeed = Mathf.Lerp(mouse.runSpeed, 1f, 0.08f);
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		if (behavior == Behavior.Flee)
		{
			fear = Mathf.Lerp(fear, Mathf.Pow(base.threatTracker.Panic, 0.7f), 0.5f);
		}
		else
		{
			fear = Mathf.Max(fear - 0.0125f, 0f);
		}
		wantToSleep = true;
		float num2 = 0f;
		for (int j = 0; j < base.tracker.CreaturesCount; j++)
		{
			if (StaticRelationship(base.tracker.GetRep(j).representedCreature).type == CreatureTemplate.Relationship.Type.Afraid)
			{
				wantToSleep = false;
				if (!base.tracker.GetRep(j).representedCreature.creatureTemplate.canFly && base.tracker.GetRep(j).VisualContact && base.tracker.GetRep(j).BestGuessForPosition().y < creature.pos.y + 1)
				{
					num2 = Mathf.Max(num2, Mathf.Pow(Mathf.InverseLerp(400f, 30f, Vector2.Distance(mouse.mainBodyChunk.pos, base.tracker.GetRep(j).representedCreature.realizedCreature.DangerPos)), 2.7f));
				}
			}
			else if (StaticRelationship(base.tracker.GetRep(j).representedCreature).type == CreatureTemplate.Relationship.Type.Eats && Custom.ManhattanDistance(base.tracker.GetRep(j).BestGuessForPosition(), creature.pos) < 10)
			{
				wantToSleep = false;
			}
		}
		pullUp = Mathf.Lerp(pullUp, num2, 0.05f);
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		if (mouse.Sleeping && !Custom.DistLess(mouse.mainBodyChunk.pos, lookAtPoint, 200f))
		{
			return 0f;
		}
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public void CollideWithMouse(LanternMouse otherMouse)
	{
		if (Custom.ManhattanDistance(creature.pos, otherMouse.AI.pathFinder.GetDestination) >= 4 && ((otherMouse.iVars.dominance > mouse.iVars.dominance && !otherMouse.sitting) || mouse.sitting || otherMouse.AI.pathFinder.GetDestination.room != otherMouse.room.abstractRoom.index))
		{
			walkWithMouse = otherMouse.AI.pathFinder.GetDestination;
		}
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		if (mouse.graphicsModule != null)
		{
			(mouse.graphicsModule as MouseGraphics).creatureLooker.ReevaluateLookObject(creatureRep, 2f);
		}
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			if (!dRelation.state.alive)
			{
				result.intensity = 0f;
			}
			else if (dRelation.trackerRep.BestGuessForPosition().room == mouse.room.abstractRoom.index && !dRelation.trackerRep.representedCreature.creatureTemplate.canFly)
			{
				float a = Mathf.Lerp(0.1f, 1.6f, Mathf.InverseLerp(-100f, 200f, mouse.room.MiddleOfTile(dRelation.trackerRep.BestGuessForPosition().Tile).y - mouse.mainBodyChunk.pos.y));
				float value = float.MaxValue;
				if (dangle.HasValue)
				{
					value = Mathf.Min(Vector2.Distance(mouse.mainBodyChunk.pos, mouse.room.MiddleOfTile(dangle.Value.attachedPos)), Vector2.Distance(mouse.mainBodyChunk.pos, mouse.room.MiddleOfTile(dangle.Value.bodyPos)));
				}
				a = Mathf.Lerp(a, 1f, Mathf.InverseLerp(50f, 500f, value));
				result.intensity *= a;
			}
		}
		return result;
	}

	private void ReconsiderDanglePos()
	{
		Dangle? dangle = DangleTile(new IntVector2(Random.Range(0, mouse.room.TileWidth), Random.Range(0, mouse.room.TileHeight)), noAccessMap: false);
		if (dangle.HasValue)
		{
			dangleChecksCounter = 0;
		}
		else
		{
			dangleChecksCounter++;
		}
		if ((!this.dangle.HasValue && dangle.HasValue) || (this.dangle.HasValue && this.dangle.Value.attachedPos.room != mouse.room.abstractRoom.index && dangle.HasValue && dangle.Value.attachedPos.room == mouse.room.abstractRoom.index))
		{
			this.dangle = dangle;
		}
		else if (this.dangle.HasValue && dangle.HasValue && this.dangle.Value.attachedPos.room == mouse.room.abstractRoom.index && dangle.Value.attachedPos.room == mouse.room.abstractRoom.index && DanglePosScore(dangle.Value) > DanglePosScore(this.dangle.Value))
		{
			this.dangle = dangle;
		}
	}

	private float DanglePosScore(Dangle d)
	{
		float num = (float)mouse.room.aimap.getTerrainProximity(d.bodyPos) * 2f;
		for (int i = 0; i < mouse.room.abstractRoom.creatures.Count; i++)
		{
			if (mouse.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.LanternMouse && mouse.room.abstractRoom.creatures[i] != creature && mouse.room.abstractRoom.creatures[i].realizedCreature != null && (mouse.room.abstractRoom.creatures[i].realizedCreature as LanternMouse).iVars.dominance > mouse.iVars.dominance && (mouse.room.abstractRoom.creatures[i].realizedCreature as LanternMouse).AI.dangle.HasValue)
			{
				float num2 = d.bodyPos.Tile.FloatDist((mouse.room.abstractRoom.creatures[i].realizedCreature as LanternMouse).AI.dangle.Value.bodyPos.Tile);
				if (num2 < 3f)
				{
					return float.MinValue;
				}
				num += num2;
			}
		}
		if (dangle.HasValue && d.attachedPos == dangle.Value.attachedPos)
		{
			num += 1000f;
		}
		return num;
	}

	public Dangle? DangleTile(IntVector2 tile, bool noAccessMap)
	{
		if (mouse.safariControlled)
		{
			return null;
		}
		if (mouse.room.aimap.getTerrainProximity(tile) < 5)
		{
			return null;
		}
		if (mouse.room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb)
		{
			return null;
		}
		bool flag = false;
		for (int num = tile.y - 1; num >= 0; num--)
		{
			if (TileAccessible(new IntVector2(tile.x, num), noAccessMap))
			{
				flag = true;
				break;
			}
			if (mouse.room.GetTile(tile.x, num).Solid)
			{
				break;
			}
		}
		if (!flag)
		{
			return null;
		}
		for (int i = tile.y; i < mouse.room.TileHeight; i++)
		{
			if (mouse.room.GetTile(tile.x, i).Solid)
			{
				return null;
			}
			if (i <= tile.y + 4 || (!mouse.room.GetTile(tile.x, i + 1).Solid && !mouse.room.GetTile(tile.x, i).horizontalBeam))
			{
				continue;
			}
			for (int j = -1; j < 2; j++)
			{
				if (TileAccessible(new IntVector2(tile.x + j, i), noAccessMap) || TileAccessible(new IntVector2(tile.x + j, i + 1), noAccessMap) || (!mouse.room.GetTile(tile.x + j, i + 1).Solid && TileAccessible(new IntVector2(tile.x + j, i + 2), noAccessMap)))
				{
					return new Dangle(mouse.room.GetWorldCoordinate(tile), mouse.room.GetWorldCoordinate(new IntVector2(tile.x, i)));
				}
			}
		}
		return null;
	}

	private bool TileAccessible(IntVector2 tl, bool noAccessMap)
	{
		if (noAccessMap)
		{
			return mouse.room.aimap.AnyExitReachableFromTile(tl, mouse.Template);
		}
		return base.pathFinder.CoordinateReachableAndGetbackable(mouse.room.GetWorldCoordinate(tl));
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (behavior != Behavior.Flee)
		{
			return cost;
		}
		return new PathCost(cost.resistance + base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: true) * 100f, cost.legality);
	}
}
