using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ScavengerAbstractAI : AbstractCreatureAI, IOwnAnAbstractSpacePathFinder
{
	private struct CoordinateAndFloat
	{
		public WorldCoordinate coord;

		public float flt;

		public CoordinateAndFloat(WorldCoordinate coord, float flt)
		{
			this.coord = coord;
			this.flt = flt;
		}
	}

	public class ScavengerSquad
	{
		public class MissionID : ExtEnum<MissionID>
		{
			public static readonly MissionID None = new MissionID("None", register: true);

			public static readonly MissionID GuardOutpost = new MissionID("GuardOutpost", register: true);

			public static readonly MissionID HuntCreature = new MissionID("HuntCreature", register: true);

			public static readonly MissionID ProtectCreature = new MissionID("ProtectCreature", register: true);

			public static readonly MissionID Trade = new MissionID("Trade", register: true);

			public MissionID(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public List<AbstractCreature> members;

		public AbstractCreature leader;

		public Color color;

		public ScavengersWorldAI.Outpost guardOutpost;

		public ScavengersWorldAI.Trader tradeSpot;

		public AbstractCreature targetCreature;

		private MissionID mType;

		public bool Active
		{
			get
			{
				if (members.Count > 0)
				{
					return (leader.abstractAI as ScavengerAbstractAI).squad == this;
				}
				return false;
			}
		}

		public bool StayIn
		{
			get
			{
				if (members.Count >= 2 || missionType != MissionID.None)
				{
					if (leader != null)
					{
						return leader.state.alive;
					}
					return false;
				}
				return false;
			}
		}

		public bool HasAMission
		{
			get
			{
				if (!MissionRoom.HasValue)
				{
					return false;
				}
				if (guardOutpost == null && targetCreature == null && tradeSpot == null)
				{
					return false;
				}
				if (leader != null && (leader.abstractAI as ScavengerAbstractAI).RoomGhostScary(MissionRoom.Value) > 0f)
				{
					return false;
				}
				if (ModManager.MSC && targetCreature != null && (targetCreature.world.GetAbstractRoom(targetCreature.pos.room).shelter || targetCreature.world.GetAbstractRoom(targetCreature.pos.room).gate))
				{
					return false;
				}
				return true;
			}
		}

		public int? MissionRoom
		{
			get
			{
				if (guardOutpost != null)
				{
					return guardOutpost.room;
				}
				if (tradeSpot != null)
				{
					return tradeSpot.room;
				}
				if (targetCreature != null)
				{
					return targetCreature.pos.room;
				}
				return null;
			}
		}

		public MissionID missionType
		{
			get
			{
				if (mType == MissionID.None)
				{
					return MissionID.None;
				}
				if ((mType == MissionID.GuardOutpost && guardOutpost == null) || (mType == MissionID.HuntCreature && targetCreature == null) || (mType == MissionID.ProtectCreature && targetCreature == null) || (mType == MissionID.Trade && (tradeSpot == null || tradeSpot.transgressedByPlayer)))
				{
					mType = MissionID.None;
				}
				return mType;
			}
			set
			{
				mType = value;
			}
		}

		public bool StationaryMission
		{
			get
			{
				if (!(mType == MissionID.GuardOutpost))
				{
					return mType == MissionID.Trade;
				}
				return true;
			}
		}

		public ScavengerSquad(AbstractCreature leader)
		{
			this.leader = leader;
			members = new List<AbstractCreature> { leader };
			color = Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f);
		}

		public void AddMember(AbstractCreature newMember)
		{
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] == newMember)
				{
					Custom.Log("scav already member of this squad.", ((members[i].abstractAI as ScavengerAbstractAI).squad == this).ToString(), ((members[i].abstractAI as ScavengerAbstractAI).squad == null).ToString());
					return;
				}
			}
			(newMember.abstractAI as ScavengerAbstractAI).squad = this;
			members.Add(newMember);
			UpdateLeader();
		}

		public void RemoveMember(AbstractCreature noLongerMember)
		{
			for (int num = members.Count - 1; num >= 0; num--)
			{
				if (members[num] == noLongerMember)
				{
					(members[num].abstractAI as ScavengerAbstractAI).squad = null;
					members.RemoveAt(num);
				}
			}
			UpdateLeader();
		}

		public void Dissolve()
		{
			for (int num = members.Count - 1; num >= 0; num--)
			{
				(members[num].abstractAI as ScavengerAbstractAI).squad = null;
			}
			members.Clear();
			leader = null;
		}

		private void UpdateLeader()
		{
			float num = 0f;
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i].personality.dominance * (members[i].state as HealthState).health > num)
				{
					num = members[i].personality.dominance * (members[i].state as HealthState).health;
					leader = members[i];
				}
			}
		}

		public void CommonMovement(int dstRoom, AbstractCreature notThisOne, bool onlyInRoom)
		{
			Custom.Log($"Squad {leader.ID} is moving to {leader.world.GetAbstractRoom(dstRoom).name}");
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] != notThisOne && (!onlyInRoom || members[i].pos.room == leader.pos.room))
				{
					(members[i].abstractAI as ScavengerAbstractAI).GoToRoom(dstRoom);
				}
			}
		}

		public bool DoesScavengerWantToBeInSquad(ScavengerAbstractAI testScav)
		{
			if (targetCreature != null)
			{
				if (missionType == MissionID.HuntCreature && testScav.parent.state.socialMemory.GetLike(targetCreature.ID) > 0.35f)
				{
					return false;
				}
				if (missionType == MissionID.ProtectCreature && testScav.parent.state.socialMemory.GetLike(targetCreature.ID) < -0.35f)
				{
					return false;
				}
			}
			return true;
		}
	}

	public int timeInRoom;

	public int freeze;

	public int dontMigrate;

	public bool lastInOffscreenDen;

	public ScavengerSquad squad;

	public int carryRocks;

	public WorldCoordinate unreachableSquadLeaderPos;

	public WorldCoordinate longTermMigration;

	public ScavengersWorldAI worldAI;

	public int controlledMigrateTime;

	public bool bringPearlHome;

	public bool missionAppropriateGear;

	public override float offscreenSpeedFac => Mathf.Max(0.5f, base.offscreenSpeedFac);

	public bool UnderSquadLeaderControl
	{
		get
		{
			if (squad != null)
			{
				return squad.leader != parent;
			}
			return false;
		}
	}

	public bool GhostOutOfCurrentRoom => RoomGhostScary(parent.pos.room) > 0f;

	public float Shyness => world.game.session.creatureCommunities.scavengerShyness;

	public ScavengerAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		if (ModManager.MSC)
		{
			if (world.game.IsArenaSession && world.game.GetArenaGameSession.chMeta != null && world.game.GetArenaGameSession.chMeta.seed >= 0)
			{
				if (world.game.GetArenaGameSession.chMeta.seed == 123)
				{
					int[] array = new int[12]
					{
						5845, 7576, 6566, 7033, 2710, 5930, 8274, 3007, 9509, 4917,
						1255, 7801
					};
					if (Scavenger.ArenaScavID < array.Length)
					{
						parent.ID.setAltSeed(array[Scavenger.ArenaScavID]);
					}
					else
					{
						parent.ID.setAltSeed(world.game.GetArenaGameSession.chMeta.seed + Scavenger.ArenaScavID);
					}
				}
				else
				{
					parent.ID.setAltSeed(world.game.GetArenaGameSession.chMeta.seed + Scavenger.ArenaScavID);
				}
			}
			Scavenger.ArenaScavID++;
		}
		carryRocks = Custom.IntClamp((int)(Mathf.Pow(world.game.SeededRandom(parent.ID.RandomSeed), Mathf.Lerp(2.5f, 0.5f, parent.personality.aggression * (1f - parent.personality.dominance))) * 4.5f), 0, 4);
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
		{
			carryRocks = Mathf.Min(carryRocks, 1);
		}
		lastInOffscreenDen = true;
		if (world.singleRoomWorld || world.offScreenDen.index == parent.pos.room)
		{
			InitGearUp();
		}
		missionAppropriateGear = true;
		dontMigrate = UnityEngine.Random.Range(400, 4800);
		longTermMigration = parent.pos;
		if (world.scavengersWorldAI == null)
		{
			world.AddWorldProcess(new ScavengersWorldAI(world));
		}
		world.scavengersWorldAI.AddScavenger(this);
		worldAI = world.scavengersWorldAI;
		if (world.game.IsArenaSession && parent.pos.room == world.offScreenDen.index)
		{
			freeze = UnityEngine.Random.Range(40, 400);
		}
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		if (newWorld.scavengersWorldAI == null)
		{
			newWorld.AddWorldProcess(new ScavengersWorldAI(newWorld));
		}
		newWorld.scavengersWorldAI.AddScavenger(this);
		worldAI = newWorld.scavengersWorldAI;
		longTermMigration = parent.pos;
	}

	public override void AbstractBehavior(int time)
	{
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
		{
			freeze = 40;
		}
		if (freeze > 0)
		{
			freeze -= time;
			return;
		}
		controlledMigrateTime--;
		if (parent.pos.room == base.MigrationDestination.room && dontMigrate > 0)
		{
			dontMigrate -= time;
		}
		if (squad != null && !squad.DoesScavengerWantToBeInSquad(this))
		{
			if (parent.world.game.devToolsActive)
			{
				Custom.Log("scav leave squad");
			}
			squad.RemoveMember(parent);
		}
		bool flag = ModManager.MSC && parent.controlled;
		if (parent.realizedCreature == null)
		{
			if (path.Count > 0)
			{
				FollowPath(time);
				return;
			}
			if (base.MigrationDestination != longTermMigration && (!flag || controlledMigrateTime > 0) && (squad == null || !squad.StationaryMission))
			{
				SetDestination(longTermMigration);
				if (!DoIHaveAPathToCoordinate(longTermMigration))
				{
					longTermMigration = base.destination;
				}
			}
			if (!flag && ((world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle) || (parent.nightCreature && world.rainCycle.dayNightCounter < 600) || GoHome()))
			{
				if (!base.denPosition.HasValue || !parent.pos.CompareDisregardingTile(base.denPosition.Value))
				{
					GoToDen();
				}
				return;
			}
		}
		else if (!flag && base.denPosition.HasValue && !base.destination.CompareDisregardingTile(base.denPosition.Value) && GoHome())
		{
			SetDestination(base.denPosition.Value);
			return;
		}
		if (squad != null && !squad.StayIn)
		{
			squad.RemoveMember(parent);
		}
		if (parent.pos.room == world.offScreenDen.index)
		{
			InOffscreenDen();
		}
		else
		{
			if (flag)
			{
				return;
			}
			lastInOffscreenDen = false;
			if (GhostOutOfCurrentRoom && RoomGhostScary(base.MigrationDestination.room) > 0f)
			{
				Migrate(1f);
			}
			else if (squad != null && squad.HasAMission && squad.MissionRoom.HasValue && base.destination.room != squad.MissionRoom && worldAI.floodFiller.IsRoomAccessible(squad.MissionRoom.Value))
			{
				GoToRoom(squad.MissionRoom.Value);
			}
			else if (UnderSquadLeaderControl && (squad.leader.pos.room != unreachableSquadLeaderPos.room || squad.leader.pos.abstractNode != unreachableSquadLeaderPos.abstractNode) && squad.leader.pos.room != world.offScreenDen.index && squad.leader.pos.room != base.MigrationDestination.room && squad.leader.abstractAI.MigrationDestination.room != base.MigrationDestination.room && TimeInfluencedRandomRoll(1f - parent.personality.bravery, time))
			{
				unreachableSquadLeaderPos = squad.leader.pos.WashTileData();
				GoToRoom(squad.leader.pos.room);
			}
			else if ((squad == null || !squad.HasAMission) && (path.Count == 0 || parent.realizedCreature != null) && base.MigrationDestination.room == parent.pos.room)
			{
				float num = parent.personality.bravery * parent.personality.energy;
				if (UnderSquadLeaderControl)
				{
					num /= 3f;
				}
				if (squad != null && squad.leader == parent)
				{
					num = 0.8f;
				}
				if (dontMigrate < 1 && (squad == null || squad.leader.abstractAI.MigrationDestination.room == parent.pos.room) && TimeInfluencedRandomRoll(0.1f / Mathf.Lerp(100f, 2f, num), time))
				{
					Migrate(num);
				}
				else if (TimeInfluencedRandomRoll(parent.personality.nervous * 0.1f, time) && parent.realizedCreature == null)
				{
					RandomMoveWithinRoom();
				}
			}
		}
	}

	private void Migrate(float roaming)
	{
		if (squad != null && squad.HasAMission)
		{
			return;
		}
		bool flag = (UnityEngine.Random.value < 0.2f || squad == null) && (squad == null || squad.leader == parent);
		if (flag)
		{
			SetDestination(base.denPosition.Value);
		}
		else
		{
			RandomMoveToOtherRoom((int)Mathf.Lerp(30f, 600f, roaming));
		}
		if (base.MigrationDestination.room != parent.pos.room && squad != null && squad.leader == parent)
		{
			squad.CommonMovement(base.MigrationDestination.room, parent, !flag);
		}
		if (flag && squad != null)
		{
			if (squad.leader == parent)
			{
				squad.Dissolve();
			}
			else
			{
				squad.RemoveMember(parent);
			}
		}
		dontMigrate = UnityEngine.Random.Range(400, 4800);
	}

	private void InOffscreenDen()
	{
		if (!lastInOffscreenDen)
		{
			ReGearInDen();
			freeze = UnityEngine.Random.Range(300, 500);
			lastInOffscreenDen = true;
			return;
		}
		if (!missionAppropriateGear)
		{
			ReGearInDen();
		}
		if ((parent.state as HealthState).health < 0.5f || (world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle) || (parent.nightCreature && world.rainCycle.dayNightCounter < 600))
		{
			freeze = 100;
		}
		else if (world.singleRoomWorld)
		{
			GoToRoom(0);
		}
		else if (UnderSquadLeaderControl)
		{
			if (squad.leader.abstractAI.MigrationDestination.room != world.offScreenDen.index)
			{
				GoToRoom(squad.leader.abstractAI.MigrationDestination.room);
			}
			else if (squad.leader.abstractAI.destination.room != world.offScreenDen.index)
			{
				GoToRoom(squad.leader.abstractAI.destination.room);
			}
		}
		else if (squad != null && squad.MissionRoom.HasValue && squad.missionType != ScavengerSquad.MissionID.None)
		{
			GoToRoom(squad.MissionRoom.Value);
		}
		else
		{
			TryAssembleSquad();
		}
	}

	public bool ReadyToJoinSquad()
	{
		if (freeze < 1)
		{
			return squad == null;
		}
		return false;
	}

	private void TryAssembleSquad()
	{
		if ((parent.nightCreature && world.rainCycle.dayNightCounter < 600) || (world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < parent.Room.creatures.Count; i++)
		{
			if (parent.Room.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Scavenger && parent.Room.creatures[i] != parent && (parent.Room.creatures[i].abstractAI as ScavengerAbstractAI).ReadyToJoinSquad())
			{
				if (parent.Room.creatures[i].personality.dominance > parent.personality.dominance)
				{
					return;
				}
				num++;
			}
		}
		if (num < 2)
		{
			return;
		}
		WorldCoordinate coord = RandomDestinationRoom();
		if (!CanRoamThroughRoom(coord.room) || !worldAI.floodFiller.IsRoomAccessible(coord.room))
		{
			return;
		}
		for (int j = 0; j < world.GetAbstractRoom(coord).creatures.Count; j++)
		{
			if (world.GetAbstractRoom(coord).creatures[j].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && (world.GetAbstractRoom(coord).creatures[j].abstractAI as ScavengerAbstractAI).squad != null)
			{
				return;
			}
		}
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		squad = new ScavengerSquad(parent);
		int maxExclusive = 7;
		if (ModManager.MMF && world.region != null)
		{
			if (world.region.name == "SH" || world.region.name == "CC" || (ModManager.MSC && world.region.name == "LC"))
			{
				maxExclusive = 5;
			}
			else if (world.region.name == "SU" || world.region.name == "UW" || world.region.name == "SI")
			{
				maxExclusive = 3;
			}
		}
		int num2 = Math.Min(num, UnityEngine.Random.Range(2, maxExclusive));
		for (int k = 0; k < parent.Room.creatures.Count; k++)
		{
			if (num2 <= 0)
			{
				break;
			}
			if (parent.Room.creatures[k].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && parent.Room.creatures[k] != parent && (parent.Room.creatures[k].abstractAI as ScavengerAbstractAI).ReadyToJoinSquad())
			{
				squad.AddMember(parent.Room.creatures[k]);
				num2--;
			}
		}
		if (!squad.StayIn)
		{
			squad = null;
		}
		if (squad != null)
		{
			ScavengersWorldAI.Outpost outpost = worldAI.OutPostInNeedOfGuard();
			if (outpost != null && RoomGhostScary(outpost.room) == 0f)
			{
				Custom.Log("Squad going to guard outpost!");
				outpost.guardSquad = squad;
				squad.guardOutpost = outpost;
				squad.missionType = ScavengerSquad.MissionID.GuardOutpost;
				for (int l = 0; l < world.GetAbstractRoom(outpost.room).nodes.Length; l++)
				{
					if (world.GetAbstractRoom(outpost.room).nodes[l].type == AbstractRoomNode.Type.RegionTransportation)
					{
						coord = new WorldCoordinate(outpost.room, -1, -1, l);
						break;
					}
				}
			}
		}
		SetDestination(coord);
		longTermMigration = coord;
		dontMigrate = UnityEngine.Random.Range(400, 4800);
		if (squad == null || squad.leader != parent)
		{
			return;
		}
		squad.CommonMovement(coord.room, parent, onlyInRoom: false);
		for (int m = 0; m < squad.members.Count; m++)
		{
			if (squad.members[m] != parent)
			{
				(squad.members[m].abstractAI as ScavengerAbstractAI).freeze = m * 10 + UnityEngine.Random.Range(0, 10);
				(squad.members[m].abstractAI as ScavengerAbstractAI).dontMigrate = UnityEngine.Random.Range(400, 4800);
			}
		}
	}

	public void GoToRoom(int dstRoom)
	{
		if (base.MigrationDestination.room == dstRoom || parent.pos.room == dstRoom || RoomGhostScary(dstRoom) > 0f || (ModManager.MSC && (parent.world.GetAbstractRoom(dstRoom).shelter || parent.world.GetAbstractRoom(dstRoom).gate)))
		{
			return;
		}
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		for (int i = 0; i < parent.world.GetAbstractRoom(dstRoom).nodes.Length; i++)
		{
			if (parent.world.GetAbstractRoom(dstRoom).nodes[i].type.Index == -1 || !parent.creatureTemplate.mappedNodeTypes[parent.world.GetAbstractRoom(dstRoom).nodes[i].type.Index])
			{
				continue;
			}
			bool flag = true;
			if (parent.world.GetAbstractRoom(dstRoom).nodes[i].type != AbstractRoomNode.Type.RegionTransportation)
			{
				flag = false;
			}
			for (int j = 0; j < parent.world.GetAbstractRoom(dstRoom).nodes.Length; j++)
			{
				if (flag)
				{
					break;
				}
				if (parent.world.GetAbstractRoom(dstRoom).nodes[j].type == AbstractRoomNode.Type.RegionTransportation && parent.world.GetAbstractRoom(dstRoom).ConnectionAndBackPossible(j, i, parent.creatureTemplate))
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.Add(new WorldCoordinate(dstRoom, -1, -1, i));
			}
			if (worldAI.floodFiller.IsNodeAccessible(dstRoom, i))
			{
				list2.Add(new WorldCoordinate(dstRoom, -1, -1, i));
			}
		}
		if (list.Count > 0)
		{
			SetDestination(list[UnityEngine.Random.Range(0, list.Count)]);
			longTermMigration = base.destination;
		}
		else if (list2.Count > 0)
		{
			SetDestination(list2[UnityEngine.Random.Range(0, list2.Count)]);
			longTermMigration = base.destination;
		}
		else
		{
			Custom.Log("scav can't reach room", world.IsRoomInRegion(dstRoom) ? world.GetAbstractRoom(dstRoom).name : dstRoom.ToString());
		}
		if (dstRoom == world.offScreenDen.index && base.MigrationDestination.room == world.offScreenDen.index && longTermMigration.room == world.offScreenDen.index)
		{
			Custom.Log("Go home freeze");
			freeze = UnityEngine.Random.Range(300, 500);
		}
	}

	public override bool CanRoamThroughRoom(int room)
	{
		return RoomGhostScary(room) <= RoomGhostScary(parent.pos.room);
	}

	public float RoomGhostScary(int testRoom)
	{
		if (ModManager.MSC && world.game.IsStorySession && world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			if (world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				for (int i = 0; i < world.game.Players.Count; i++)
				{
					if (world.game.Players[i].pos.room == testRoom)
					{
						return 1f;
					}
				}
			}
			return 0f;
		}
		if (Shyness == 0f || testRoom == world.offScreenDen.index)
		{
			return 0f;
		}
		float num = 0f;
		for (int j = 0; j < world.game.Players.Count; j++)
		{
			if (world.game.Players[j].pos.room == testRoom)
			{
				num += 0.5f * Shyness;
			}
		}
		if (Shyness > 0.33f)
		{
			for (int k = 0; k < world.GetAbstractRoom(testRoom).connections.Length; k++)
			{
				if (world.GetAbstractRoom(testRoom).connections[k] <= -1)
				{
					continue;
				}
				for (int l = 0; l < world.game.Players.Count; l++)
				{
					if (world.game.Players[l].pos.room == world.GetAbstractRoom(testRoom).connections[k])
					{
						num += 0.25f * Mathf.InverseLerp(0.33f, 1f, Shyness);
					}
				}
				if (!(Shyness > 0.66f))
				{
					continue;
				}
				AbstractRoom abstractRoom = world.GetAbstractRoom(world.GetAbstractRoom(testRoom).connections[k]);
				for (int m = 0; m < abstractRoom.connections.Length; m++)
				{
					if (abstractRoom.connections[m] <= -1)
					{
						continue;
					}
					for (int n = 0; n < world.game.Players.Count; n++)
					{
						if (world.game.Players[n].pos.room == abstractRoom.connections[m])
						{
							num += 0.125f * Mathf.InverseLerp(0.66f, 1f, Shyness);
						}
					}
				}
			}
		}
		return num;
	}

	public float CostAddOfNode(WorldCoordinate coordinate)
	{
		return RoomGhostScary(coordinate.room) * 10000f;
	}

	public override void Die()
	{
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		base.Die();
	}

	public override bool DoIwantToDropThisItemInDen(AbstractPhysicalObject item)
	{
		return false;
	}

	private bool GoHome()
	{
		if (parent.pos.room == world.offScreenDen.index)
		{
			return false;
		}
		if (bringPearlHome)
		{
			return true;
		}
		if (!missionAppropriateGear)
		{
			return true;
		}
		return false;
	}

	private WorldCoordinate RandomDestinationRoom()
	{
		List<CoordinateAndFloat> list = new List<CoordinateAndFloat>();
		float num = 0f;
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			if ((ModManager.MSC && (world.GetAbstractRoom(i + world.firstRoomIndex).shelter || world.GetAbstractRoom(i + world.firstRoomIndex).gate)) || !(world.GetAbstractRoom(i + world.firstRoomIndex).AttractionForCreature(parent.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden) || !worldAI.floodFiller.IsRoomAccessible(i + world.firstRoomIndex) || (ModManager.MSC && world.game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(i + world.firstRoomIndex, 0, 11, -1))))
			{
				continue;
			}
			float num2 = world.GetAbstractRoom(i + world.firstRoomIndex).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type);
			int num3 = 0;
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; j++)
			{
				if (world.GetAbstractRoom(i + world.firstRoomIndex).nodes[j].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[world.GetAbstractRoom(i + world.firstRoomIndex).nodes[j].type.Index])
				{
					num3++;
				}
			}
			for (int k = 0; k < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; k++)
			{
				if (world.GetAbstractRoom(i + world.firstRoomIndex).nodes[k].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[world.GetAbstractRoom(i + world.firstRoomIndex).nodes[k].type.Index])
				{
					list.Add(new CoordinateAndFloat(new WorldCoordinate(i + world.firstRoomIndex, -1, -1, k), num2 / (float)num3));
					num += num2 / (float)num3;
				}
			}
		}
		float num4 = UnityEngine.Random.value * num;
		for (int l = 0; l < list.Count; l++)
		{
			if (num4 < list[l].flt)
			{
				return list[l].coord;
			}
			num4 -= list[l].flt;
		}
		Custom.Log("scav weighted random failure");
		return new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
	}

	public void ControlledLongTermDestination()
	{
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		WorldCoordinate[] regionAccessNodes = world.regionAccessNodes;
		for (int i = 0; i < regionAccessNodes.Length; i++)
		{
			WorldCoordinate item = regionAccessNodes[i];
			if (item.room != parent.Room.index)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			WorldCoordinate worldCoordinate = list[Mathf.Min(list.Count - 1, UnityEngine.Random.Range(0, list.Count))];
			controlledMigrateTime = 80;
			SetDestination(worldCoordinate);
			longTermMigration = worldCoordinate;
			dontMigrate = 400;
		}
	}

	public void InitGearUp()
	{
		int num = 3;
		int num2 = 40;
		float num3 = 1f;
		if (world.game.IsStorySession)
		{
			num2 = (world.game.session as StoryGameSession).saveState.cycleNumber;
			if (world.game.StoryCharacter == SlugcatStats.Name.Yellow)
			{
				num2 = Mathf.FloorToInt((float)num2 * 0.75f);
				num3 = 0.5f;
			}
			else if (world.game.StoryCharacter == SlugcatStats.Name.Red)
			{
				num2 += 60;
				num3 = 1.5f;
			}
		}
		if (ModManager.MSC && world.game.IsArenaSession && world.game.GetArenaGameSession.chMeta != null && world.game.GetArenaGameSession.chMeta.seed >= 0)
		{
			UnityEngine.Random.InitState(parent.ID.RandomSeed);
		}
		bool flag = false;
		if (!ModManager.MSC || (world.game.IsStorySession && ((world.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.Yellow || (world.game.session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || (world.game.session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)))
		{
			flag = true;
		}
		int num4 = Custom.IntClamp((int)(Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(parent.personality.dominance, 3f - num3))) * (3.5f + num3)), 0, 4);
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite && num4 < 1)
		{
			num4 = 1;
		}
		if (num4 > 0)
		{
			for (int i = 0; i < num4; i++)
			{
				AbstractPhysicalObject abstractPhysicalObject = ((!ModManager.MSC || !(parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)) ? new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(num2)) : ((!(UnityEngine.Random.value < 0.5f || flag)) ? new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: false, electric: true) : new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: true)));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, num, carry: true);
				num--;
			}
		}
		if (num >= 0 && UnityEngine.Random.value < 0.6f && ((!world.singleRoomWorld && ((world.game.IsStorySession && ModManager.MSC && world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint) || world.region.name == "SH" || (world.region.name == "SB" && UnityEngine.Random.value < 0.7f))) || (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)))
		{
			AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, parent.pos, world.game.GetNewID());
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, num, carry: true);
			num--;
		}
		if (ModManager.MSC)
		{
			if (num >= 0 && !world.singleRoomWorld && (world.region.name == "SB" || world.region.name == "SL" || world.region.name == "MS") && UnityEngine.Random.value < 0.27f)
			{
				AbstractConsumable abstractConsumable = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null, parent.pos, world.game.GetNewID(), -1, -1, null);
				abstractConsumable.isConsumed = true;
				world.GetAbstractRoom(parent.pos).AddEntity(abstractConsumable);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractConsumable, num, carry: true);
				num--;
			}
			if (num >= 0 && !world.singleRoomWorld && (world.region.name == "LF" || world.region.name == "OE") && UnityEngine.Random.value < 0.27f)
			{
				AbstractConsumable abstractConsumable2 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null, parent.pos, world.game.GetNewID(), -1, -1, null);
				abstractConsumable2.isConsumed = true;
				world.GetAbstractRoom(parent.pos).AddEntity(abstractConsumable2);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractConsumable2, num, carry: true);
				num--;
			}
		}
		if (num >= 0 && Mathf.Pow(UnityEngine.Random.value, num3) < Mathf.InverseLerp(10f, 60f, num2))
		{
			int num5 = Custom.IntClamp((int)(Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(parent.personality.dominance, 2f) * Mathf.InverseLerp(10f, 60f, num2))) * 2.5f), 0, 2);
			for (int j = 0; j < num5; j++)
			{
				AbstractPhysicalObject abstractPhysicalObject3 = ((!ModManager.MSC || !(parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) || flag || ((!world.game.IsArenaSession || world.game.GetArenaGameSession.chMeta != null) && (!(UnityEngine.Random.value <= parent.personality.aggression / 5f) || (!(parent.personality.dominance > 0.6f) && !parent.nightCreature)))) ? new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, parent.pos, world.game.GetNewID()) : new AbstractPhysicalObject(world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, parent.pos, world.game.GetNewID()));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject3);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject3, num, carry: true);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		if (num >= 0 && UnityEngine.Random.value < 0.08f)
		{
			AbstractPhysicalObject abstractPhysicalObject4 = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject4);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject4, num, carry: true);
			num--;
		}
		if (num >= 0 && Mathf.Pow(UnityEngine.Random.value, num3) < (world.game.IsStorySession ? Mathf.InverseLerp(40f, 110f, num2) : 0.8f) && UnityEngine.Random.value < 1f / Mathf.Lerp(12f, 3f, parent.personality.dominance))
		{
			SporePlant.AbstractSporePlant abstractSporePlant = new SporePlant.AbstractSporePlant(world, null, parent.pos, world.game.GetNewID(), -1, -1, null, used: false, pacified: true);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractSporePlant, num, carry: true);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractSporePlant);
			num--;
		}
		if (num >= 0)
		{
			int num6 = UnityEngine.Random.Range(0, carryRocks + 1);
			for (int k = 0; k < num6; k++)
			{
				AbstractPhysicalObject abstractPhysicalObject5 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, parent.pos, world.game.GetNewID());
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject5);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject5, num, carry: true);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
		{
			while (num >= 0)
			{
				AbstractPhysicalObject abstractPhysicalObject6 = new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(num2));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject6);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject6, num, carry: true);
				num--;
			}
		}
	}

	private bool IsSpearExplosive(int cycleNum)
	{
		float num = parent.personality.dominance * ((parent.personality.aggression + parent.personality.bravery) / 2f) * 0.75f;
		if (world.game.IsStorySession && world.region != null)
		{
			if (world.region.name == "SU")
			{
				num = Mathf.Pow(num, 2f) * 0.5f;
			}
			else if (world.region.name == "HI" || world.region.name == "DS")
			{
				num *= 0.5f;
			}
			num = Mathf.Lerp(num, 1f, Mathf.InverseLerp(-0.5f, -1f, world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, world.RegionNumber, 0)) * 0.15f);
			num *= Mathf.InverseLerp(10f, 30f, cycleNum);
			if ((world.game.session as StoryGameSession).saveState.saveStateNumber == SlugcatStats.Name.Red)
			{
				num = Mathf.Lerp(num, 1f, 0.25f * UnityEngine.Random.value);
			}
		}
		else
		{
			num = Mathf.Lerp(num, 1f, 0.15f);
		}
		return UnityEngine.Random.value < num;
	}

	private void ReGearInDen()
	{
		Custom.Log("regear in den");
		bringPearlHome = false;
		int num = 0;
		bool[] array = new bool[4];
		for (int num2 = parent.stuckObjects.Count - 1; num2 >= 0; num2--)
		{
			if (parent.stuckObjects[num2] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num2].A == parent)
			{
				if (parent.stuckObjects[num2].B.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
				{
					Custom.Log($"scav dropping {parent.stuckObjects[num2].B.type} in den");
					DropAndDestroy(parent.stuckObjects[num2]);
				}
				else
				{
					int grasp = (parent.stuckObjects[num2] as AbstractPhysicalObject.CreatureGripStick).grasp;
					if (grasp >= 0 && grasp < 4)
					{
						array[grasp] = true;
						num++;
					}
					else
					{
						DropAndDestroy(parent.stuckObjects[num2]);
					}
				}
			}
		}
		if (squad == null)
		{
			return;
		}
		if (squad.missionType == ScavengerSquad.MissionID.Trade)
		{
			for (int num3 = parent.stuckObjects.Count - 1; num3 >= 0; num3--)
			{
				if (parent.stuckObjects[num3] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num3].A == parent && parent.stuckObjects[num3].B.type == AbstractPhysicalObject.AbstractObjectType.Rock)
				{
					DropAndDestroy(parent.stuckObjects[num3]);
				}
			}
		}
		if (squad.missionType != ScavengerSquad.MissionID.Trade)
		{
			UpdateMissionAppropriateGear();
			if (missionAppropriateGear)
			{
				return;
			}
		}
		if (num == 4)
		{
			for (int num4 = parent.stuckObjects.Count - 1; num4 >= 0; num4--)
			{
				if (parent.stuckObjects[num4] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num4].A == parent)
				{
					int grasp2 = (parent.stuckObjects[num4] as AbstractPhysicalObject.CreatureGripStick).grasp;
					DropAndDestroy(parent.stuckObjects[num4]);
					num--;
					array[grasp2] = false;
					Custom.Log("make grasp free:", grasp2.ToString());
					break;
				}
			}
		}
		int num5 = -1;
		for (int i = 0; i < 4; i++)
		{
			if (!array[i])
			{
				num5 = i;
				break;
			}
		}
		if (num5 == -1)
		{
			return;
		}
		if (squad.missionType == ScavengerSquad.MissionID.GuardOutpost || squad.missionType == ScavengerSquad.MissionID.HuntCreature || squad.missionType == ScavengerSquad.MissionID.ProtectCreature)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(world.game.IsStorySession ? (world.game.GetStorySession.saveState.cycleNumber + ((world.game.StoryCharacter == SlugcatStats.Name.Red) ? 60 : 0)) : 0));
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, num5, carry: true);
			array[num5] = true;
		}
		else if (squad.missionType == ScavengerSquad.MissionID.Trade)
		{
			AbstractPhysicalObject abstractPhysicalObject2 = TradeItem(main: true);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, num5, carry: true);
			array[num5] = true;
			for (int j = 0; j < array.Length; j++)
			{
				if (!array[j])
				{
					abstractPhysicalObject2 = TradeItem(main: false);
					world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
					new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, j, carry: true);
					array[j] = true;
				}
			}
			for (int k = 0; k < parent.stuckObjects.Count; k++)
			{
				if (parent.stuckObjects[k] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[k].A == parent && parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
				{
					(parent.stuckObjects[k].B as AbstractSpear).explosive = true;
				}
			}
		}
		missionAppropriateGear = true;
	}

	private void DropAndDestroy(AbstractPhysicalObject.AbstractObjectStick stick)
	{
		stick.Deactivate();
		stick.B.Destroy();
	}

	private AbstractPhysicalObject TradeItem(bool main)
	{
		bool flag = world.region.name == "SH" || world.region.name == "SB";
		if (main)
		{
			if (flag)
			{
				return new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, parent.pos, world.game.GetNewID());
			}
		}
		else
		{
			if (UnityEngine.Random.value < (flag ? 0.5f : 0.05f))
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < ((world.region.name == "LF") ? 0.5f : 0.05f))
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.2f)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.1f)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.02f && world.game.StoryCharacter != SlugcatStats.Name.Red)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
		}
		return new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: true);
	}

	public void UpdateMissionAppropriateGear()
	{
		missionAppropriateGear = false;
		if (squad == null || squad.missionType == ScavengerSquad.MissionID.None)
		{
			missionAppropriateGear = true;
		}
		else if (squad.missionType == ScavengerSquad.MissionID.GuardOutpost || squad.missionType == ScavengerSquad.MissionID.HuntCreature || squad.missionType == ScavengerSquad.MissionID.ProtectCreature)
		{
			for (int i = 0; i < parent.stuckObjects.Count; i++)
			{
				if (parent.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[i].A == parent && parent.stuckObjects[i].B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
				{
					missionAppropriateGear = true;
					break;
				}
			}
		}
		else
		{
			if (!(squad.missionType == ScavengerSquad.MissionID.Trade))
			{
				return;
			}
			switch (world.region.name)
			{
			case "SH":
			case "SB":
			{
				for (int j = 0; j < parent.stuckObjects.Count; j++)
				{
					if (parent.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[j].A == parent && parent.stuckObjects[j].B.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
					{
						missionAppropriateGear = true;
						break;
					}
				}
				return;
			}
			}
			for (int k = 0; k < parent.stuckObjects.Count; k++)
			{
				if (parent.stuckObjects[k] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[k].A == parent && parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Spear && (parent.stuckObjects[k].B as AbstractSpear).explosive)
				{
					missionAppropriateGear = true;
					break;
				}
			}
		}
	}
}
