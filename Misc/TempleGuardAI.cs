using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class TempleGuardAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class CreatureSpottedClosestToExit
	{
		public int exit;

		public EntityID ID;

		public CreatureSpottedClosestToExit(int exit, EntityID ID)
		{
			this.exit = exit;
			this.ID = ID;
			Custom.Log($"THROWBACKEXIT {ID} E:{exit}");
		}
	}

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public Tracker.CreatureRepresentation focusCreature;

	public WorldCoordinate tryHoverPos;

	public DebugSprite dbSpr;

	public bool bowDown;

	public int protectExit;

	public List<int> protectExitDistances;

	public PhysicalObject pickUpObject;

	public bool telekinesis;

	public bool floorSlam;

	public Vector2 telekinGetToPoint;

	public Vector2 telekinGetToDir;

	public IntVector2 telekinArm;

	public int patience;

	public int throwBackCounter;

	public float stress;

	public bool floorSlamDir;

	public WorldCoordinate idlePos;

	public List<CreatureSpottedClosestToExit> throwOutExits;

	public TempleGuard guard => creature.realizedCreature as TempleGuard;

	public bool FocusCreatureMovingTowardsProtectExit
	{
		get
		{
			if (focusCreature == null || protectExitDistances.Count < 1)
			{
				return false;
			}
			return protectExitDistances[0] < protectExitDistances[protectExitDistances.Count - 1];
		}
	}

	public TempleGuardAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		guard.AI = this;
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		tryHoverPos = creature.pos;
		protectExitDistances = new List<int>();
		throwOutExits = new List<CreatureSpottedClosestToExit>();
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		protectExit = -1;
		int num = int.MaxValue;
		for (int i = 0; i < room.abstractRoom.nodes.Length; i++)
		{
			if (room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Exit || (room.game.world.singleRoomWorld && room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den))
			{
				int x = room.LocalCoordinateOfNode(i).x;
				if (x < num)
				{
					num = x;
					protectExit = i;
				}
			}
		}
		idlePos = creature.pos;
		throwOutExits.Clear();
		Custom.Log($"PROTECT EXIT: {protectExit}");
	}

	public override void Update()
	{
		if (guard.room.game.StoryCharacter == SlugcatStats.Name.Red && guard.room.game.Players.Count > 0)
		{
			AbstractCreature firstAlivePlayer = guard.room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null)
			{
				base.tracker.SeeCreature(firstAlivePlayer);
			}
		}
		if (focusCreature != null && focusCreature.VisualContact && FocusCreatureMovingTowardsProtectExit)
		{
			throwBackCounter = Math.Min(throwBackCounter + 1, 500);
		}
		else
		{
			throwBackCounter = Math.Max(throwBackCounter - 1, 0);
		}
		float num = 0f;
		Tracker.CreatureRepresentation creatureRepresentation = null;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
			{
				float num2 = ThrowOutScore(base.tracker.GetRep(i));
				if (base.tracker.GetRep(i) == focusCreature)
				{
					num2 *= 2f;
				}
				if (base.tracker.GetRep(i).representedCreature.realizedCreature == pickUpObject)
				{
					num2 *= 2f;
				}
				if (num2 > 5f && num2 > num)
				{
					num = num2;
					creatureRepresentation = base.tracker.GetRep(i);
				}
			}
		}
		if (creatureRepresentation != focusCreature && !guard.safariControlled)
		{
			focusCreature = creatureRepresentation;
			protectExitDistances.Clear();
		}
		if (ModManager.MSC && guard.room != null && guard.room.world.region != null && guard.room.world.region.name == "HR")
		{
			focusCreature = null;
		}
		if (guard.safariControlled)
		{
			if (guard.inputWithDiagonals.HasValue && guard.inputWithDiagonals.Value.pckp)
			{
				if (focusCreature != null && focusCreature.representedCreature.pos.Tile.FloatDist(guard.abstractCreature.pos.Tile) > 30f)
				{
					focusCreature = null;
					pickUpObject = null;
				}
				if ((guard.inputWithDiagonals.Value.x != 0 || guard.inputWithDiagonals.Value.y != 0) && focusCreature == null)
				{
					Creature creature = null;
					float num3 = float.MaxValue;
					for (int j = 0; j < guard.room.physicalObjects.Length; j++)
					{
						for (int k = 0; k < guard.room.physicalObjects[j].Count; k++)
						{
							if (guard.room.physicalObjects[j][k] is Creature && (guard.room.physicalObjects[j][k] as Creature).abstractCreature.creatureTemplate.type != CreatureTemplate.Type.TempleGuard)
							{
								Vector2 pos = guard.room.physicalObjects[j][k].firstChunk.pos;
								bool flag = true;
								if (guard.inputWithDiagonals.Value.x != 0 && Mathf.Sign(guard.inputWithDiagonals.Value.x) != Mathf.Sign(pos.x - guard.mainBodyChunk.pos.x))
								{
									flag = false;
								}
								if (guard.inputWithDiagonals.Value.y != 0 && Mathf.Sign(guard.inputWithDiagonals.Value.y) != Mathf.Sign(pos.y - guard.mainBodyChunk.pos.y))
								{
									flag = false;
								}
								float num4 = Custom.Dist(guard.mainBodyChunk.pos, pos);
								if (flag && num4 < num3)
								{
									num3 = num4;
									creature = guard.room.physicalObjects[j][k] as Creature;
								}
							}
						}
					}
					if (creature != null)
					{
						focusCreature = base.tracker.RepresentationForCreature(creature.abstractCreature, addIfMissing: true);
						pickUpObject = creature;
						patience++;
					}
				}
			}
			else
			{
				focusCreature = null;
				pickUpObject = null;
			}
		}
		if (!floorSlam && (focusCreature == null || focusCreature.representedCreature.realizedCreature != pickUpObject))
		{
			pickUpObject = null;
		}
		if (focusCreature != null && !guard.safariControlled)
		{
			float num5 = 0.2f;
			if (FocusCreatureMovingTowardsProtectExit)
			{
				num5 += 0.6f;
			}
			if (pickUpObject == null && focusCreature.representedCreature.realizedCreature != null && focusCreature.VisualContact)
			{
				int num6 = ProtectExitDistance(guard.room.GetTilePosition(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos));
				if (DistToClosestNonProtectExit(guard.room.GetTilePosition(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos)) > num6)
				{
					num5 += 0.4f;
				}
				if (num6 > 0)
				{
					protectExitDistances.Insert(0, num6);
					if (protectExitDistances.Count > 10)
					{
						protectExitDistances.RemoveAt(10);
					}
					if (Custom.DistLess(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, guard.mainBodyChunk.pos, 1000f) && FocusCreatureMovingTowardsProtectExit && DistToClosestNonProtectExit(guard.room.GetTilePosition(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos)) - 10 + patience + Math.Max(0, throwBackCounter - 150) > num6 && (!ModManager.MSC || (guard.room != null && guard.room.world.region != null && guard.room.world.region.name != "HR")))
					{
						pickUpObject = focusCreature.representedCreature.realizedCreature;
						telekinArm.x = ((!(pickUpObject.firstChunk.pos.x < guard.mainBodyChunk.pos.x)) ? 1 : 0);
						telekinArm.y = UnityEngine.Random.Range(0, 2);
						patience++;
					}
				}
			}
			num5 = Mathf.Clamp(num5, 0f, 1f);
			if (Custom.ManhattanDistance(base.creature.pos, base.pathFinder.GetDestination) > 3)
			{
				num5 = Mathf.Max(num5, 0.5f);
			}
			stress = Mathf.Lerp(stress, Mathf.Max(num5, Mathf.InverseLerp(1f, 18f, ThrowOutScore(focusCreature))), 0.1f);
			IntVector2 tilePosition = guard.room.GetTilePosition(guard.mainBodyChunk.pos + Custom.RNV() * 200f * UnityEngine.Random.value);
			if (HangAroundScore(focusCreature.BestGuessForPosition(), guard.room.GetWorldCoordinate(tilePosition)) > HangAroundScore(focusCreature.BestGuessForPosition(), tryHoverPos))
			{
				tryHoverPos = guard.room.GetWorldCoordinate(tilePosition);
			}
			tilePosition = guard.room.GetTilePosition(guard.room.MiddleOfTile(focusCreature.BestGuessForPosition()) + Custom.RNV() * 18f * 20f);
			if (HangAroundScore(focusCreature.BestGuessForPosition(), guard.room.GetWorldCoordinate(tilePosition)) > HangAroundScore(focusCreature.BestGuessForPosition(), tryHoverPos))
			{
				tryHoverPos = guard.room.GetWorldCoordinate(tilePosition);
			}
			if (HangAroundScore(focusCreature.BestGuessForPosition(), tryHoverPos) > HangAroundScore(focusCreature.BestGuessForPosition(), base.pathFinder.GetDestination) + 100f || (UnityEngine.Random.value < 0.001f && HangAroundScore(focusCreature.BestGuessForPosition(), tryHoverPos) > HangAroundScore(focusCreature.BestGuessForPosition(), base.pathFinder.GetDestination)))
			{
				base.creature.abstractAI.SetDestination(tryHoverPos);
			}
		}
		else
		{
			stress = Mathf.Lerp(stress, (Custom.ManhattanDistance(base.creature.pos, base.pathFinder.GetDestination) < 5) ? 0f : 0.2f, 0.01f);
			base.creature.abstractAI.SetDestination(idlePos);
		}
		telekinGetToPoint = guard.mainBodyChunk.pos;
		telekinesis = false;
		if (pickUpObject != null)
		{
			stress = Mathf.Min(1f, stress + 1f / 30f);
			telekinGetToPoint = pickUpObject.firstChunk.pos;
			IntVector2 tilePosition2 = guard.room.GetTilePosition(pickUpObject.firstChunk.pos);
			if (guard.safariControlled)
			{
				telekinArm.x = ((pickUpObject.firstChunk.pos.x >= guard.mainBodyChunk.pos.x) ? 1 : 0);
				telekinArm.y = UnityEngine.Random.Range(0, 2);
			}
			if (patience < 4 || (ModManager.MMF && !MMF.cfgVanillaExploits.Value && !(pickUpObject is Player)) || (guard.safariControlled && guard.inputWithDiagonals.HasValue))
			{
				int ext = ThrowOutExit(tilePosition2, pickUpObject.abstractPhysicalObject.ID);
				float num7 = float.MinValue;
				int num8 = -1;
				for (int l = 0; l < 8; l++)
				{
					IntVector2 intVector = tilePosition2 + Custom.eightDirections[l];
					if (!guard.room.GetTile(intVector).Solid)
					{
						float num9 = Mathf.Lerp(ProtectExitDistance(intVector), -DistToThrowOutExit(intVector, ext), Mathf.InverseLerp(10f, 30f, tilePosition2.FloatDist(guard.room.LocalCoordinateOfNode(protectExit).Tile)));
						if (num9 > num7)
						{
							num8 = l;
							num7 = num9;
						}
					}
				}
				if (num8 > -1 && !guard.safariControlled)
				{
					telekinGetToDir = Custom.eightDirections[num8].ToVector2().normalized;
				}
				if (guard.safariControlled && guard.inputWithDiagonals.HasValue && (guard.inputWithDiagonals.Value.x != 0 || guard.inputWithDiagonals.Value.y != 0))
				{
					telekinGetToDir = new Vector2(guard.inputWithDiagonals.Value.x, guard.inputWithDiagonals.Value.y);
				}
				telekinesis = true;
				floorSlam = false;
				if (!guard.safariControlled)
				{
					int num10 = DistToThrowOutExit(tilePosition2, ext);
					if (num10 + 10 + 5 * patience + Math.Max(0, throwBackCounter - 150) < ProtectExitDistance(tilePosition2) || num10 < 3 + 30 / patience)
					{
						pickUpObject = null;
					}
					else if (!Custom.DistLess(guard.mainBodyChunk.pos, pickUpObject.firstChunk.pos, 1000f))
					{
						pickUpObject = null;
					}
				}
			}
			else
			{
				if (floorSlamDir && pickUpObject.firstChunk.pos.y > guard.firstChunk.pos.y + 20f)
				{
					floorSlamDir = false;
				}
				else if (!floorSlamDir && pickUpObject.firstChunk.ContactPoint.y < 0)
				{
					floorSlamDir = true;
				}
				telekinGetToDir = new Vector2(Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value), floorSlamDir ? 1f : (-1f)).normalized;
				telekinesis = true;
				floorSlam = true;
				if (UnityEngine.Random.value < 0.005f && (!(pickUpObject is Creature) || (pickUpObject as Creature).dead))
				{
					floorSlam = false;
					pickUpObject = null;
				}
			}
		}
		bowDown = false;
		for (int m = 0; m < base.tracker.CreaturesCount; m++)
		{
			if (bowDown)
			{
				break;
			}
			if (base.tracker.GetRep(m).representedCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard && base.tracker.GetRep(m).BestGuessForPosition().room == base.creature.pos.room && base.tracker.GetRep(m).representedCreature.realizedCreature != null && base.tracker.GetRep(m).representedCreature.realizedCreature.mainBodyChunk.pos.y > guard.mainBodyChunk.pos.y && Custom.DistLess(guard.mainBodyChunk.pos, base.tracker.GetRep(m).representedCreature.realizedCreature.mainBodyChunk.pos, 100f) && (base.tracker.GetRep(m).representedCreature.realizedCreature as TempleGuard).moving && !guard.moving && VisualContact(base.tracker.GetRep(m).representedCreature.realizedCreature.mainBodyChunk))
			{
				bowDown = true;
			}
		}
		base.Update();
	}

	private float ThrowOutScore(Tracker.CreatureRepresentation crit)
	{
		if (crit.BestGuessForPosition().room != creature.pos.room)
		{
			return 0f;
		}
		if (crit.representedCreature.realizedCreature != null && crit.representedCreature.realizedCreature is Player && (crit.representedCreature.realizedCreature as Player).KarmaCap == 9)
		{
			return 0f;
		}
		if (ModManager.MSC && crit.representedCreature.realizedCreature != null && crit.representedCreature.realizedCreature is Player && (crit.representedCreature.realizedCreature as Player).KarmaCap < 9)
		{
			Player player = crit.representedCreature.realizedCreature as Player;
			_ = player.Karma;
			if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && player.grasps.Length != 0)
			{
				for (int i = 0; i < player.grasps.Length; i++)
				{
					if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Scavenger && (guard.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma + (player.grasps[i].grabbedChunk.owner as Scavenger).abstractCreature.karmicPotential >= 9)
					{
						return 0f;
					}
				}
			}
		}
		return 500f / ((float)ProtectExitDistance(crit.BestGuessForPosition().Tile) + (float)crit.TicksSinceSeen / 2f);
	}

	private int ClosestNonProtectExit(IntVector2 testPos)
	{
		int result = -1;
		int num = int.MaxValue;
		for (int i = 0; i < guard.room.abstractRoom.nodes.Length; i++)
		{
			if (i != protectExit && (guard.room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Exit || (guard.room.game.world.singleRoomWorld && guard.room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)))
			{
				int num2 = guard.room.aimap.ExitDistanceForCreature(testPos, guard.room.abstractRoom.CommonToCreatureSpecificNodeIndex(i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
				if ((num2 > 0 || Custom.ManhattanDistance(testPos, guard.room.LocalCoordinateOfNode(i).Tile) < 3) && num2 < num)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	private int ThrowOutExit(IntVector2 pos, EntityID ID)
	{
		for (int i = 0; i < throwOutExits.Count; i++)
		{
			if (throwOutExits[i].ID == ID)
			{
				return throwOutExits[i].exit;
			}
		}
		return ClosestNonProtectExit(pos);
	}

	private int DistToThrowOutExit(IntVector2 testPos, int ext)
	{
		int num = guard.room.aimap.ExitDistanceForCreature(testPos, guard.room.abstractRoom.CommonToCreatureSpecificNodeIndex(ext, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
		if (num > 0)
		{
			return num;
		}
		return int.MaxValue;
	}

	private int DistToClosestNonProtectExit(IntVector2 testPos)
	{
		int num = int.MaxValue;
		for (int i = 0; i < guard.room.abstractRoom.nodes.Length; i++)
		{
			if (i != protectExit && (guard.room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Exit || (guard.room.game.world.singleRoomWorld && guard.room.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)))
			{
				int num2 = guard.room.aimap.ExitDistanceForCreature(testPos, guard.room.abstractRoom.CommonToCreatureSpecificNodeIndex(i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
				if (num2 > 0 && num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private int ProtectExitDistance(IntVector2 testPos)
	{
		int num = -1;
		for (int i = 0; i < 9; i++)
		{
			if (num >= 1)
			{
				break;
			}
			num = guard.room.aimap.ExitDistanceForCreature(testPos + Custom.eightDirectionsAndZero[i], guard.room.abstractRoom.CommonToCreatureSpecificNodeIndex(protectExit, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
		}
		if (num < 1)
		{
			num = int.MaxValue;
		}
		return num;
	}

	private float HangAroundScore(WorldCoordinate creaturePos, WorldCoordinate testPos)
	{
		if (creaturePos.room != testPos.room)
		{
			return float.MinValue;
		}
		if (!testPos.TileDefined)
		{
			return float.MinValue;
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(testPos))
		{
			return float.MinValue;
		}
		bool flag = true;
		for (int i = 0; i < base.tracker.CreaturesCount && flag; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard && base.tracker.GetRep(i).representedCreature.realizedCreature != null && base.tracker.GetRep(i).BestGuessForPosition().room == creature.pos.room && base.tracker.GetRep(i).representedCreature.personality.dominance > creature.personality.dominance)
			{
				flag = false;
			}
		}
		float num = (18f - (float)Mathf.Abs(18 - guard.room.aimap.getAItile(testPos).smoothedFloorAltitude)) * 20f;
		num += Mathf.Pow(Mathf.Clamp(guard.room.aimap.getTerrainProximity(testPos), 1, 10), 1.2f) * 5f;
		if (flag)
		{
			num /= Mathf.Max(1f, Mathf.Abs(18f - guard.room.LocalCoordinateOfNode(protectExit).Tile.FloatDist(testPos.Tile)));
		}
		else
		{
			num /= Mathf.Max(1f, Mathf.Abs(18f - creaturePos.Tile.FloatDist(testPos.Tile)));
			if (guard.room.VisualContact(creaturePos, testPos))
			{
				num += 100f;
			}
			for (int j = 0; j < base.tracker.CreaturesCount; j++)
			{
				if (base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard && base.tracker.GetRep(j).representedCreature.realizedCreature != null && base.tracker.GetRep(j).BestGuessForPosition().room == creature.pos.room && base.tracker.GetRep(j).representedCreature.personality.dominance >= creature.personality.dominance)
				{
					num -= Custom.LerpMap(testPos.Tile.FloatDist(base.tracker.GetRep(j).representedCreature.abstractAI.RealAI.pathFinder.GetDestination.Tile), 0f, 100f, 800f, 0f);
				}
			}
		}
		return num;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		if (creatureRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.TempleGuard && !creatureRep.representedCreature.creatureTemplate.smallCreature)
		{
			if (firstSpot)
			{
				stress = Mathf.Min(stress + 0.2f, 1f);
			}
			if (guard.graphicsModule != null)
			{
				(guard.graphicsModule as TempleGuardGraphics).ReactToCreature(firstSpot, creatureRep);
			}
		}
		bool flag = true;
		for (int i = 0; i < throwOutExits.Count && flag; i++)
		{
			if (throwOutExits[i].ID == creatureRep.representedCreature.ID)
			{
				flag = false;
			}
		}
		if (flag)
		{
			for (int j = 0; j < base.tracker.CreaturesCount && flag; j++)
			{
				if (!(base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard) || base.tracker.GetRep(j).representedCreature.realizedCreature == null || base.tracker.GetRep(j).representedCreature.realizedCreature.room != guard.room)
				{
					continue;
				}
				TempleGuardAI aI = (base.tracker.GetRep(j).representedCreature.realizedCreature as TempleGuard).AI;
				for (int k = 0; k < aI.throwOutExits.Count && flag; k++)
				{
					if (aI.throwOutExits[k].ID == creatureRep.representedCreature.ID)
					{
						throwOutExits.Add(new CreatureSpottedClosestToExit(aI.throwOutExits[k].exit, creatureRep.representedCreature.ID));
						flag = false;
					}
				}
			}
		}
		if (flag)
		{
			throwOutExits.Add(new CreatureSpottedClosestToExit(ClosestNonProtectExit(creatureRep.BestGuessForPosition().Tile), creatureRep.representedCreature.ID));
		}
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return null;
		}
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		cost = new PathCost(cost.resistance + Mathf.Clamp(Mathf.Pow(Mathf.Abs(18f - (float)guard.room.aimap.getTerrainProximity(connection.destinationCoord)), 1.5f), 0f, 18f) * 15f + Mathf.Clamp(Mathf.Pow(Mathf.Abs(18f - (float)guard.room.aimap.getAItile(connection.destinationCoord).smoothedFloorAltitude), 1.5f), 0f, 18f) * 15f, cost.legality);
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard && base.tracker.GetRep(i).BestGuessForPosition().room == connection.destinationCoord.room)
			{
				cost.resistance += Custom.LerpMap(connection.DestTile.FloatDist(base.tracker.GetRep(i).BestGuessForPosition().Tile), 0f, 10f, 300f, 0f);
			}
		}
		return cost;
	}
}
