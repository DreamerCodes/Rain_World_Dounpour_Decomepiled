using System;
using System.Collections.Generic;
using System.Linq;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ShortcutHandler
{
	public class Vessel
	{
		public Creature creature;

		public AbstractRoom room;

		public int entranceNode;

		public Vessel(Creature creature, AbstractRoom room)
		{
			this.creature = creature;
			this.room = room;
			entranceNode = -1;
		}
	}

	public class BatHiveVessel : Vessel
	{
		public BatHiveVessel(Creature creature, AbstractRoom room)
			: base(creature, room)
		{
		}
	}

	public class ShortCutVessel : Vessel
	{
		public IntVector2 pos;

		public IntVector2[] lastPositions;

		public int wait;

		public IntVector2 lastPos => lastPositions[0];

		public ShortCutVessel(IntVector2 pos, Creature creature, AbstractRoom room, int wait)
			: base(creature, room)
		{
			this.pos = pos;
			this.wait = wait;
			lastPositions = new IntVector2[Custom.IntClamp(creature.Template.shortcutSegments - 1, 1, 100)];
			for (int i = 0; i < lastPositions.Length; i++)
			{
				lastPositions[i] = pos;
			}
		}

		public void PushNewLastPos(IntVector2 pos)
		{
			if (lastPositions.Length > 1)
			{
				for (int num = lastPositions.Length - 1; num > 0; num--)
				{
					lastPositions[num] = lastPositions[num - 1];
				}
			}
			lastPositions[0] = pos;
		}

		public void SetAllPositions(IntVector2 pos)
		{
			this.pos = pos;
			for (int i = 0; i < lastPositions.Length; i++)
			{
				lastPositions[i] = pos;
			}
		}
	}

	public class BorderVessel : Vessel
	{
		public WorldCoordinate destination;

		public float distance;

		public float distanceTravelled;

		public AbstractRoomNode.Type type;

		public bool Arrived => distanceTravelled >= distance;

		public BorderVessel(Creature creature, AbstractRoomNode.Type type, WorldCoordinate destination, float distance, AbstractRoom room)
			: base(creature, room)
		{
			this.type = type;
			this.destination = destination;
			this.distance = distance;
			distanceTravelled = 0f;
		}

		public void Update()
		{
			distanceTravelled += 1f;
		}
	}

	public class TeleportationVessel : Vessel
	{
		public WorldCoordinate destination;

		public TeleportationVessel(Creature creature, WorldCoordinate destination, AbstractRoom room)
			: base(creature, room)
		{
			this.destination = destination;
		}
	}

	private RainWorldGame game;

	public List<ShortCutVessel> transportVessels;

	public List<Vessel> betweenRoomsWaitingLobby;

	public List<BorderVessel> borderTravelVessels;

	public ShortcutHandler(RainWorldGame gm)
	{
		game = gm;
		transportVessels = new List<ShortCutVessel>();
		betweenRoomsWaitingLobby = new List<Vessel>();
		borderTravelVessels = new List<BorderVessel>();
	}

	public void Update()
	{
		for (int num = transportVessels.Count - 1; num >= 0; num--)
		{
			if (transportVessels[num].wait > 0)
			{
				transportVessels[num].wait--;
				if (ModManager.CoopAvailable && transportVessels[num].creature is Player player && RWInput.CheckSpecificButton((player.State as PlayerState).playerNumber, 0))
				{
					transportVessels[num].wait = 0;
				}
			}
			else if (transportVessels[num].room.realizedRoom != null)
			{
				Room realizedRoom = transportVessels[num].room.realizedRoom;
				IntVector2 pos = transportVessels[num].pos;
				transportVessels[num].pos = NextShortcutPosition(transportVessels[num].pos, transportVessels[num].lastPos, realizedRoom);
				if (transportVessels[num].creature is Player || (ModManager.MSC && game.rainWorld.safariMode && transportVessels[num].creature.abstractCreature.FollowedByCamera(0)))
				{
					realizedRoom.PlaySound(SoundID.Player_Tick_Along_In_Shortcut, 0f, 1f, 1f);
				}
				else
				{
					realizedRoom.PlaySound(NPCShortcutSound(transportVessels[num].creature, 1), realizedRoom.MiddleOfTile(transportVessels[num].pos));
				}
				transportVessels[num].PushNewLastPos(pos);
				if (realizedRoom.GetTile(transportVessels[num].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					SpitOutCreature(transportVessels[num]);
					transportVessels.RemoveAt(num);
				}
				else if (transportVessels[num].pos != transportVessels[num].lastPos && realizedRoom.GetTile(transportVessels[num].pos).shortCut > 1)
				{
					int num2 = realizedRoom.exitAndDenIndex.IndexfOf(transportVessels[num].pos);
					transportVessels[num].creature.abstractCreature.pos.abstractNode = num2;
					switch (realizedRoom.GetTile(transportVessels[num].pos).shortCut)
					{
					case 2:
						if (game.IsArenaSession)
						{
							if (transportVessels[num].creature is Player && game.GetArenaGameSession.PlayerTryingToEnterDen(transportVessels[num]))
							{
								transportVessels[num].wait = game.world.rainCycle.TimeUntilRain + 10000;
								transportVessels[num].pos = pos;
								break;
							}
							return;
						}
						if (transportVessels[num].room.connections.Length != 0)
						{
							if (num2 >= transportVessels[num].room.connections.Length)
							{
								transportVessels[num].PushNewLastPos(transportVessels[num].pos);
								transportVessels[num].pos = pos;
								Custom.LogWarning("faulty room exit");
							}
							else
							{
								int num3 = transportVessels[num].room.connections[num2];
								if (num3 <= -1)
								{
									return;
								}
								transportVessels[num].entranceNode = game.world.GetAbstractRoom(num3).ExitIndex(transportVessels[num].room.index);
								transportVessels[num].room = game.world.GetAbstractRoom(num3);
								betweenRoomsWaitingLobby.Add(transportVessels[num]);
							}
						}
						transportVessels.RemoveAt(num);
						break;
					case 3:
						transportVessels[num].creature.abstractCreature.OpportunityToEnterDen(new WorldCoordinate(transportVessels[num].room.index, -1, -1, num2));
						if (transportVessels[num].creature.abstractCreature.InDen)
						{
							transportVessels.RemoveAt(num);
						}
						break;
					case 5:
						if (transportVessels[num].creature.NPCTransportationDestination.room > -1 && transportVessels[num].creature.NPCTransportationDestination.NodeDefined)
						{
							transportVessels[num].entranceNode = transportVessels[num].creature.NPCTransportationDestination.abstractNode;
							transportVessels[num].room = game.world.GetAbstractRoom(transportVessels[num].creature.NPCTransportationDestination.room);
							betweenRoomsWaitingLobby.Add(transportVessels[num]);
							transportVessels.RemoveAt(num);
						}
						break;
					}
				}
			}
			else
			{
				transportVessels.RemoveAt(num);
			}
		}
		for (int num4 = borderTravelVessels.Count - 1; num4 >= 0; num4--)
		{
			if (borderTravelVessels[num4].Arrived)
			{
				betweenRoomsWaitingLobby.Add(borderTravelVessels[num4]);
				borderTravelVessels.RemoveAt(num4);
			}
			else
			{
				borderTravelVessels[num4].Update();
			}
		}
		for (int num5 = betweenRoomsWaitingLobby.Count - 1; num5 >= 0; num5--)
		{
			if (betweenRoomsWaitingLobby[num5].room.realizedRoom == null)
			{
				if (betweenRoomsWaitingLobby[num5].creature.abstractCreature.FollowedByCamera(0) && !betweenRoomsWaitingLobby[num5].room.offScreenDen)
				{
					game.world.ActivateRoom(betweenRoomsWaitingLobby[num5].room);
				}
				else
				{
					WorldCoordinate coord = new WorldCoordinate(betweenRoomsWaitingLobby[num5].room.index, -1, -1, betweenRoomsWaitingLobby[num5].entranceNode);
					betweenRoomsWaitingLobby[num5].creature.abstractCreature.Abstractize(coord);
					betweenRoomsWaitingLobby.RemoveAt(num5);
				}
			}
			else if (VesselAllowedInRoom(betweenRoomsWaitingLobby[num5]))
			{
				WorldCoordinate newCoord = new WorldCoordinate(betweenRoomsWaitingLobby[num5].room.index, -1, -1, betweenRoomsWaitingLobby[num5].entranceNode);
				betweenRoomsWaitingLobby[num5].creature.abstractCreature.Move(newCoord);
				if (betweenRoomsWaitingLobby[num5] is TeleportationVessel)
				{
					TeleportingCreatureArrivedInRealizedRoom(betweenRoomsWaitingLobby[num5] as TeleportationVessel);
				}
				else if (betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.Den || betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.Exit || betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.RegionTransportation)
				{
					(betweenRoomsWaitingLobby[num5] as ShortCutVessel).SetAllPositions(betweenRoomsWaitingLobby[num5].room.realizedRoom.exitAndDenIndex[betweenRoomsWaitingLobby[num5].entranceNode]);
					transportVessels.Add(betweenRoomsWaitingLobby[num5] as ShortCutVessel);
					if (betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.Exit)
					{
						betweenRoomsWaitingLobby[num5].room.realizedRoom.BlinkShortCut(betweenRoomsWaitingLobby[num5].room.realizedRoom.shortcutsIndex.IndexfOf(betweenRoomsWaitingLobby[num5].room.realizedRoom.ShortcutLeadingToNode(betweenRoomsWaitingLobby[num5].entranceNode).StartTile), -1, (betweenRoomsWaitingLobby[num5].creature.Template.type == CreatureTemplate.Type.Slugcat) ? 2.5f : 1.5f);
						for (int i = 0; i < betweenRoomsWaitingLobby[num5].room.realizedRoom.game.cameras.Length; i++)
						{
							if (betweenRoomsWaitingLobby[num5].room.realizedRoom.game.cameras[i].room == betweenRoomsWaitingLobby[num5].room.realizedRoom)
							{
								betweenRoomsWaitingLobby[num5].room.realizedRoom.game.cameras[i].shortcutGraphics.ColorEntrance(betweenRoomsWaitingLobby[num5].room.realizedRoom.shortcutsIndex.IndexfOf(betweenRoomsWaitingLobby[num5].room.realizedRoom.ShortcutLeadingToNode(betweenRoomsWaitingLobby[num5].entranceNode).StartTile), betweenRoomsWaitingLobby[num5].creature.ShortCutColor());
							}
						}
					}
				}
				else if (betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.SideExit || betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.SkyExit || betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.SeaExit)
				{
					FlyingCreatureArrivedInRealizedRoom(betweenRoomsWaitingLobby[num5] as BorderVessel);
				}
				else if (betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].type == AbstractRoomNode.Type.BatHive)
				{
					PopOutOfBatHive(betweenRoomsWaitingLobby[num5] as BatHiveVessel);
				}
				if (betweenRoomsWaitingLobby[num5].creature.abstractCreature.FollowedByCamera(0))
				{
					game.cameras[0].MoveCamera(betweenRoomsWaitingLobby[num5].room.realizedRoom, betweenRoomsWaitingLobby[num5].room.nodes[betweenRoomsWaitingLobby[num5].entranceNode].viewedByCamera);
				}
				betweenRoomsWaitingLobby[num5].entranceNode = -1;
				betweenRoomsWaitingLobby.RemoveAt(num5);
			}
		}
	}

	private bool VesselAllowedInRoom(Vessel vessel)
	{
		if (vessel.room.realizedRoom == null || !vessel.room.realizedRoom.ReadyForPlayer)
		{
			return false;
		}
		if (vessel.creature.abstractCreature.stuckObjects.Count < 1)
		{
			return CreatureAllowedInRoom(vessel.creature.abstractCreature, vessel.room.realizedRoom);
		}
		List<AbstractPhysicalObject> allConnectedObjects = vessel.creature.abstractCreature.GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			if (allConnectedObjects[i] is AbstractCreature && !CreatureAllowedInRoom(allConnectedObjects[i] as AbstractCreature, vessel.room.realizedRoom))
			{
				Custom.Log("denied room entrance because of carried creature");
				return false;
			}
		}
		return true;
	}

	private bool CreatureAllowedInRoom(AbstractCreature crit, Room realizedRoom)
	{
		if (crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			return realizedRoom.ReadyForPlayer;
		}
		if (!realizedRoom.readyForAI)
		{
			if (realizedRoom.readyForNonAICreaturesToEnter)
			{
				return !crit.RequiresAIMapToEnterRoom();
			}
			return false;
		}
		return true;
	}

	public static IntVector2 NextShortcutPosition(IntVector2 pos, IntVector2 lastPos, Room room)
	{
		IntVector2 intVector = pos - lastPos;
		if ((intVector.x != 0 || intVector.y != 0) && room.GetTile(pos + intVector).shortCut != 0 && room.IsPositionInsideBoundries(pos + intVector))
		{
			pos += intVector;
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				if ((Custom.fourDirections[i].x != -intVector.x || Custom.fourDirections[i].y != -intVector.y) && room.GetTile(pos + Custom.fourDirections[i]).shortCut != 0 && room.IsPositionInsideBoundries(pos + Custom.fourDirections[i]))
				{
					pos += Custom.fourDirections[i];
					break;
				}
			}
			if (pos.x == lastPos.x && pos.y == lastPos.y)
			{
				pos -= intVector;
			}
		}
		return pos;
	}

	public void SuckInCreature(Creature creature, Room room, ShortcutData shortCut)
	{
		room.PlaySound((creature is Player) ? SoundID.Player_Enter_Shortcut : NPCShortcutSound(creature, 0), creature.mainBodyChunk.pos);
		if (creature is Player && shortCut.shortCutType == ShortcutData.Type.RoomExit)
		{
			int num = room.abstractRoom.connections[shortCut.destNode];
			if (num > -1)
			{
				room.world.ActivateRoom(room.world.GetAbstractRoom(num));
			}
		}
		if (shortCut.shortCutType == ShortcutData.Type.NPCTransportation && Array.IndexOf(room.shortcutsIndex, creature.NPCTransportationDestination.Tile) > -1)
		{
			transportVessels.Add(new ShortCutVessel(creature.NPCTransportationDestination.Tile, creature, room.abstractRoom, (int)Vector2.Distance(IntVector2.ToVector2(shortCut.DestTile), IntVector2.ToVector2(creature.NPCTransportationDestination.Tile))));
			return;
		}
		int wait = 0;
		if (creature is Player player && ModManager.CoopAvailable && Custom.rainWorld.options.JollyPlayerCount > 1 && room == room.game.cameras[0].room && shortCut.shortCutType == ShortcutData.Type.RoomExit)
		{
			JollyCustom.Log($"Sucking shortcut: {shortCut.shortCutType}, players in current room: {room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Player>().ToList().Count()}");
			wait = Player.InitialShortcutWaitTime;
			List<ShortCutVessel> list = (from x in transportVessels
				where x.creature is Player
				orderby x.wait
				select x).ToList();
			int num2 = room.world.game.Players.Count((AbstractCreature absC) => absC?.realizedCreature is Player player2 && player2.slugOnBack?.slugcat != null);
			bool flag = player.slugOnBack != null && player.slugOnBack.slugcat != null;
			int num3 = room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Player>().ToList()
				.Count();
			if (list.Count == 0)
			{
				if (num3 - num2 <= 1)
				{
					wait = 0;
				}
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					int num4 = list[i].wait;
					if (num2 + list.Count() + 1 < Custom.rainWorld.options.JollyPlayerCount)
					{
						num4 = 0;
					}
					if (i == 0)
					{
						list[i].wait = (int)Math.Max(0f, (float)list[i].wait * (flag ? 0.3f : 0.5f));
					}
					else
					{
						list[i].wait = list[i - 1].wait + 5;
					}
					wait = list[i].wait + 5;
					JollyCustom.Log($"Reduced vessel [{list[i].pos}] wait time from [{num4}] to [{list[i].wait}]");
				}
			}
		}
		transportVessels.Add(new ShortCutVessel(shortCut.StartTile, creature, room.abstractRoom, wait));
	}

	public Vector2? OnScreenPositionOfInShortCutCreature(Room room, Creature crit)
	{
		for (int i = 0; i < transportVessels.Count; i++)
		{
			if (transportVessels[i].creature == crit)
			{
				if (room.abstractRoom == room.game.shortcuts.transportVessels[i].room)
				{
					return room.MiddleOfTile(room.game.shortcuts.transportVessels[i].pos);
				}
				return null;
			}
		}
		for (int j = 0; j < transportVessels.Count; j++)
		{
			if (room.abstractRoom != room.game.shortcuts.transportVessels[j].room || transportVessels[j].creature.abstractCreature.stuckObjects.Count <= 0)
			{
				continue;
			}
			for (int k = 0; k < transportVessels[j].creature.abstractCreature.stuckObjects.Count; k++)
			{
				if (transportVessels[j].creature.abstractCreature.stuckObjects[k].A == crit.abstractCreature || transportVessels[j].creature.abstractCreature.stuckObjects[k].B == crit.abstractCreature)
				{
					return room.MiddleOfTile(room.game.shortcuts.transportVessels[j].pos);
				}
			}
		}
		return null;
	}

	public void CreatureEnterFromAbstractRoom(Creature creature, AbstractRoom enterRoom, int enterNode)
	{
		Vessel vessel = null;
		if (creature is ITeleportingCreature)
		{
			vessel = new TeleportationVessel(creature, new WorldCoordinate(enterRoom.index, -1, -1, enterNode), enterRoom);
		}
		else if (enterRoom.nodes[enterNode].type == AbstractRoomNode.Type.Den || enterRoom.nodes[enterNode].type == AbstractRoomNode.Type.Exit || enterRoom.nodes[enterNode].type == AbstractRoomNode.Type.RegionTransportation)
		{
			vessel = new ShortCutVessel(enterRoom.realizedRoom.ShortcutLeadingToNode(enterNode).DestTile, creature, enterRoom, 0);
			creature.inShortcut = true;
		}
		else if (enterRoom.nodes[enterNode].borderExit)
		{
			vessel = new BorderVessel(creature, enterRoom.nodes[enterNode].type, new WorldCoordinate(enterRoom.index, -1, -1, enterNode), 0f, enterRoom);
		}
		else
		{
			if (!(enterRoom.nodes[enterNode].type == AbstractRoomNode.Type.BatHive))
			{
				return;
			}
			vessel = new BatHiveVessel(creature, enterRoom);
		}
		vessel.entranceNode = enterNode;
		betweenRoomsWaitingLobby.Add(vessel);
	}

	public void CreatureTakeFlight(Creature creature, AbstractRoomNode.Type type, WorldCoordinate start, WorldCoordinate dest)
	{
		creature.FlyAwayFromRoom(carriedByOther: false);
		BorderVessel borderVessel = new BorderVessel(creature, type, dest, game.world.SkyHighwayDistanceBetweenNodes(start, dest), game.world.GetAbstractRoom(dest));
		borderVessel.entranceNode = dest.abstractNode;
		borderTravelVessels.Add(borderVessel);
		creature.RemoveFromRoom();
	}

	public void CreatureTeleportOutOfRoom(Creature creature, WorldCoordinate start, WorldCoordinate dest)
	{
		TeleportationVessel teleportationVessel = new TeleportationVessel(creature, dest, game.world.GetAbstractRoom(dest));
		teleportationVessel.entranceNode = dest.abstractNode;
		betweenRoomsWaitingLobby.Add(teleportationVessel);
		creature.RemoveFromRoom();
	}

	public void FlyingCreatureArrivedInRealizedRoom(BorderVessel fCrit)
	{
		Room realizedRoom = fCrit.room.realizedRoom;
		WorldCoordinate worldCoordinate = realizedRoom.LocalCoordinateOfNode(fCrit.entranceNode);
		worldCoordinate.abstractNode = fCrit.entranceNode;
		if (fCrit.creature.abstractCreature.abstractAI.RealAI != null && fCrit.creature.abstractCreature.abstractAI.RealAI.pathFinder != null)
		{
			fCrit.creature.abstractCreature.abstractAI.RealAI.pathFinder.nonShortcutRoomEntrancePos = worldCoordinate;
		}
		fCrit.creature.shortcutDelay = 100;
		IntVector2 intVector = new IntVector2(0, 1);
		if (worldCoordinate.x == 0)
		{
			intVector = new IntVector2(-1, 0);
		}
		else if (worldCoordinate.x == realizedRoom.TileWidth - 1)
		{
			intVector = new IntVector2(1, 0);
		}
		else if (worldCoordinate.y == 0)
		{
			intVector = new IntVector2(0, -1);
		}
		if (fCrit.creature.safariControlled && (fCrit.creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Vulture || fCrit.creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.KingVulture || fCrit.creature.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture))
		{
			worldCoordinate.Tile += intVector;
		}
		else
		{
			worldCoordinate.Tile += intVector * ((intVector.y == 1) ? 99 : 59);
		}
		fCrit.creature.FlyIntoRoom(worldCoordinate, realizedRoom);
	}

	public void TeleportingCreatureArrivedInRealizedRoom(TeleportationVessel tVessel)
	{
		(tVessel.creature as ITeleportingCreature).TeleportingIntoRoom(tVessel.room.realizedRoom);
	}

	public void PopOutOfBatHive(BatHiveVessel vessel)
	{
	}

	private SoundID NPCShortcutSound(Creature creature, int situation)
	{
		int num = 1;
		if (creature.Template.smallCreature || creature.Template.bodySize < 0.4f)
		{
			num = 0;
		}
		else if (creature.Template.bodySize > 1.5f)
		{
			num = 2;
		}
		return situation switch
		{
			0 => num switch
			{
				0 => SoundID.Small_NPC_Enter_Shortcut, 
				2 => SoundID.Large_NPC_Enter_Shortcut, 
				_ => SoundID.Medium_NPC_Enter_Shortcut, 
			}, 
			1 => num switch
			{
				0 => SoundID.Small_NPC_Tick_Along_In_Shortcut, 
				2 => SoundID.Large_NPC_Tick_Along_In_Shortcut, 
				_ => SoundID.Medium_NPC_Tick_Along_In_Shortcut, 
			}, 
			_ => num switch
			{
				0 => SoundID.Small_NPC_Exit_Shortcut, 
				2 => SoundID.Large_NPC_Exit_Shortcut, 
				_ => SoundID.Medium_NPC_Exit_Shortcut, 
			}, 
		};
	}

	public void SpitOutCreature(ShortCutVessel vessel)
	{
		if (!ModManager.MMF || ((!(vessel.creature is PoleMimic) || !(vessel.creature as PoleMimic).dead) && (!(vessel.creature is TentaclePlant) || !(vessel.creature as TentaclePlant).dead)))
		{
			vessel.room.realizedRoom.PlaySound((vessel.creature is Player) ? SoundID.Player_Exit_Shortcut : NPCShortcutSound(vessel.creature, 2), vessel.room.realizedRoom.MiddleOfTile(vessel.pos));
			vessel.creature.SpitOutOfShortCut(vessel.pos, vessel.room.realizedRoom, spitOutAllSticks: true);
		}
	}
}
