using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public abstract class RegionGate : UpdatableAndDeletable, IDrawable
{
	public class Mode : ExtEnum<Mode>
	{
		public static readonly Mode MiddleClosed = new Mode("MiddleClosed", register: true);

		public static readonly Mode ClosingAirLock = new Mode("ClosingAirLock", register: true);

		public static readonly Mode Waiting = new Mode("Waiting", register: true);

		public static readonly Mode OpeningMiddle = new Mode("OpeningMiddle", register: true);

		public static readonly Mode MiddleOpen = new Mode("MiddleOpen", register: true);

		public static readonly Mode ClosingMiddle = new Mode("ClosingMiddle", register: true);

		public static readonly Mode OpeningSide = new Mode("OpeningSide", register: true);

		public static readonly Mode Closed = new Mode("Closed", register: true);

		public static readonly Mode Broken = new Mode("Broken", register: true);

		public Mode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class GateRequirement : ExtEnum<GateRequirement>
	{
		public static readonly GateRequirement OneKarma = new GateRequirement("1", register: true);

		public static readonly GateRequirement TwoKarma = new GateRequirement("2", register: true);

		public static readonly GateRequirement ThreeKarma = new GateRequirement("3", register: true);

		public static readonly GateRequirement FourKarma = new GateRequirement("4", register: true);

		public static readonly GateRequirement FiveKarma = new GateRequirement("5", register: true);

		public static readonly GateRequirement DemoLock = new GateRequirement("D", register: true);

		public GateRequirement(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Door
	{
		private RegionGate gate;

		public int number;

		public float closedFac;

		public float closeSpeed;

		public float openSpeed;

		public bool lastClosed;

		public bool movementStalledByGraphicsModule;

		public Door(RegionGate gate, int number)
		{
			this.gate = gate;
			this.number = number;
			closedFac = (((float)number == 1f) ? 1f : 0f);
			closeSpeed = 1f / 180f;
			openSpeed = 0.0045454544f;
		}

		public void Update()
		{
			if (movementStalledByGraphicsModule)
			{
				return;
			}
			if (closedFac > gate.goalDoorPositions[number])
			{
				closedFac = Mathf.Max(0f, closedFac - openSpeed);
			}
			else if (closedFac < gate.goalDoorPositions[number])
			{
				closedFac = Mathf.Min(1f, closedFac + closeSpeed);
			}
			if (gate.room.readyForAI)
			{
				bool flag = closedFac > 0f;
				if (flag != lastClosed)
				{
					gate.ChangeDoorStatus(number, !flag);
				}
				lastClosed = flag;
			}
		}
	}

	public RainCycle rainCycle;

	public Door[] doors;

	public bool letThroughDir;

	public bool dontOpen;

	public bool waitingForWorldLoader;

	public float[] goalDoorPositions;

	public RegionGateGraphics graphics;

	public int washingCounter;

	public int startCounter;

	public bool unlocked;

	public Mode mode;

	public GateKarmaGlyph[] karmaGlyphs;

	public GateRequirement[] karmaRequirements;

	public virtual bool EnergyEnoughToOpen => true;

	public virtual bool MeetRequirement
	{
		get
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count == 0 || firstAlivePlayer == null || (firstAlivePlayer.realizedCreature == null && ModManager.CoopAvailable))
			{
				return false;
			}
			Player player = null;
			player = ((!ModManager.CoopAvailable || room.game.AlivePlayers.Count <= 0) ? (room.game.Players[0].realizedCreature as Player) : (firstAlivePlayer.realizedCreature as Player));
			int num = player.Karma;
			if (ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && player.grasps.Length != 0)
			{
				for (int i = 0; i < player.grasps.Length; i++)
				{
					if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Scavenger)
					{
						num = (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma + (player.grasps[i].grabbedChunk.owner as Scavenger).abstractCreature.karmicPotential;
						break;
					}
				}
			}
			bool flag = ModManager.MSC && karmaRequirements[(!letThroughDir) ? 1u : 0u] == MoreSlugcatsEnums.GateRequirement.RoboLock && room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.hasRobo && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark && room.world.region.name != "SL" && room.world.region.name != "MS" && room.world.region.name != "DM";
			int result = -1;
			bool flag2 = false;
			if (int.TryParse(karmaRequirements[(!letThroughDir) ? 1u : 0u].value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				flag2 = result - 1 <= num;
			}
			if (ModManager.MSC && ModManager.Expedition && room.game.rainWorld.ExpeditionMode && karmaRequirements[(!letThroughDir) ? 1u : 0u] == MoreSlugcatsEnums.GateRequirement.RoboLock && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer && room.world.region.name == "UW" && room.abstractRoom.name.Contains("LC"))
			{
				return true;
			}
			if (!(flag || flag2))
			{
				return unlocked;
			}
			return true;
		}
	}

	public RegionGate(Room room)
	{
		base.room = room;
		rainCycle = room.world.rainCycle;
		karmaRequirements = new GateRequirement[2];
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "locks.txt"));
		if (base.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f)
		{
			karmaRequirements[0] = GateRequirement.OneKarma;
			karmaRequirements[1] = GateRequirement.OneKarma;
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (Regex.Split(array[i], " : ")[0] == room.abstractRoom.name)
				{
					karmaRequirements[0] = new GateRequirement(Regex.Split(array[i], " : ")[1].Trim());
					karmaRequirements[1] = new GateRequirement(Regex.Split(array[i], " : ")[2].Trim());
					break;
				}
			}
		}
		customKarmaGateRequirements();
		karmaGlyphs = new GateKarmaGlyph[2];
		for (int j = 0; j < 2; j++)
		{
			karmaGlyphs[j] = new GateKarmaGlyph(j == 1, this, karmaRequirements[j]);
			room.AddObject(karmaGlyphs[j]);
		}
		doors = new Door[3];
		for (int k = 0; k < 3; k++)
		{
			doors[k] = new Door(this, k);
		}
		Reset();
		if (this is WaterGate)
		{
			if (base.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f)
			{
				(this as WaterGate).waterLeft = 0f;
			}
			else
			{
				(this as WaterGate).waterLeft = (room.world.regionState.gatesPassedThrough[room.abstractRoom.gateIndex] ? 0f : 1f);
			}
		}
		else if (this is ElectricGate)
		{
			if (base.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f)
			{
				(this as ElectricGate).batteryLeft = 0f;
			}
			else
			{
				(this as ElectricGate).batteryLeft = (room.world.regionState.gatesPassedThrough[room.abstractRoom.gateIndex] ? 0f : 1f);
			}
		}
		graphics = new RegionGateGraphics(this);
		if (room.game != null && room.game.IsStorySession && room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates != null)
		{
			for (int l = 0; l < room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates.Count; l++)
			{
				if (room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates[l] == room.abstractRoom.name)
				{
					unlocked = true;
					break;
				}
			}
		}
		if (room.game != null && room.game.IsStorySession && !unlocked && room.game.StoryCharacter != SlugcatStats.Name.Red && File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "nifflasmode.txt").ToLowerInvariant()))
		{
			int result = -1;
			if (int.TryParse(karmaRequirements[0].value, NumberStyles.Any, CultureInfo.InvariantCulture, out result) && int.TryParse(karmaRequirements[1].value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				unlocked = true;
			}
		}
		if (base.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f)
		{
			unlocked = true;
			mode = Mode.Broken;
			for (int m = 0; m < 3; m++)
			{
				goalDoorPositions[m] = 0f;
				ChangeDoorStatus(m, open: true);
				doors[m].closedFac = 0f;
			}
		}
	}

	public void OPENCLOSE()
	{
		for (int i = 0; i < 3; i++)
		{
			goalDoorPositions[i] = 1f - goalDoorPositions[i];
		}
	}

	private void Reset()
	{
		mode = Mode.MiddleClosed;
		doors[1].closedFac = 1f;
		goalDoorPositions = new float[3] { 0f, 1f, 0f };
	}

	public bool KarmaBlinkRed()
	{
		if (mode != Mode.MiddleClosed || !EnergyEnoughToOpen || unlocked || karmaRequirements[(!letThroughDir) ? 1u : 0u] == GateRequirement.DemoLock || (ModManager.MSC && karmaRequirements[(!letThroughDir) ? 1u : 0u] == MoreSlugcatsEnums.GateRequirement.RoboLock) || (ModManager.MSC && karmaRequirements[(!letThroughDir) ? 1u : 0u] == MoreSlugcatsEnums.GateRequirement.OELock))
		{
			return false;
		}
		int num = PlayersInZone();
		if (num > 0 && num < 3)
		{
			letThroughDir = num == 1;
			if (!dontOpen && !MeetRequirement)
			{
				return true;
			}
		}
		return false;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		graphics.Update();
		if (mode == Mode.MiddleClosed)
		{
			int num = PlayersInZone();
			if (num > 0 && num < 3)
			{
				letThroughDir = num == 1;
				GateKarmaGlyph gateKarmaGlyph = karmaGlyphs[(!letThroughDir) ? 1u : 0u];
				if (!dontOpen && PlayersStandingStill() && EnergyEnoughToOpen && MeetRequirement && (gateKarmaGlyph.ShouldAnimate() == 0 || gateKarmaGlyph.animationFinished))
				{
					startCounter++;
					if (startCounter > 60)
					{
						mode = Mode.ClosingAirLock;
						goalDoorPositions[(!letThroughDir) ? 2 : 0] = 1f;
						room.game.overWorld.GateRequestsSwitchInitiation(this);
						waitingForWorldLoader = true;
						startCounter = 0;
						if (room.game.manager.musicPlayer != null)
						{
							room.game.manager.musicPlayer.GateEvent();
						}
						if (room.game != null && room.game.IsStorySession && room.game.GetStorySession.saveState.deathPersistentSaveData.CanUseUnlockedGates(room.game.GetStorySession.saveStateNumber))
						{
							Unlock();
						}
					}
				}
				else
				{
					startCounter = 0;
				}
			}
			else
			{
				dontOpen = false;
				startCounter = 0;
			}
		}
		else if (mode == Mode.ClosingAirLock)
		{
			if (AllDoorsInPosition())
			{
				washingCounter = 0;
				mode = Mode.Waiting;
			}
		}
		else if (mode == Mode.Waiting)
		{
			washingCounter++;
			if (ModManager.MSC)
			{
				foreach (AbstractCreature creature in room.abstractRoom.creatures)
				{
					creature.Hypothermia = Mathf.Lerp(creature.Hypothermia, 0f, 0.004f);
				}
			}
			if (!waitingForWorldLoader && washingCounter > 400)
			{
				mode = Mode.OpeningMiddle;
				goalDoorPositions[1] = 0f;
			}
		}
		else if (mode == Mode.OpeningMiddle)
		{
			if (AllDoorsInPosition())
			{
				mode = Mode.MiddleOpen;
				for (int i = 0; i < 2; i++)
				{
					karmaGlyphs[i].symbolDirty = true;
					karmaGlyphs[i].UpdateDefaultColor();
				}
			}
		}
		else if (mode == Mode.MiddleOpen)
		{
			if (AllPlayersThroughToOtherSide())
			{
				goalDoorPositions[1] = 1f;
				mode = Mode.ClosingMiddle;
			}
		}
		else if (mode == Mode.ClosingMiddle)
		{
			if (AllDoorsInPosition())
			{
				mode = Mode.OpeningSide;
				goalDoorPositions[0] = 0f;
				goalDoorPositions[2] = 0f;
			}
		}
		else if (mode == Mode.OpeningSide)
		{
			if (AllDoorsInPosition())
			{
				mode = Mode.Closed;
				dontOpen = true;
			}
		}
		else
		{
			_ = mode == Mode.Closed;
		}
		for (int j = 0; j < 3; j++)
		{
			doors[j].Update();
		}
	}

	public void Unlock()
	{
		if (unlocked)
		{
			return;
		}
		unlocked = true;
		bool flag = false;
		if (room.game != null && room.game.IsStorySession)
		{
			flag = room.game.GetStorySession.saveState.deathPersistentSaveData.CanUseUnlockedGates(room.game.GetStorySession.saveState.saveStateNumber);
		}
		if (!flag)
		{
			return;
		}
		if (room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates == null)
		{
			room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates = new List<string>();
		}
		if (room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates == null)
		{
			return;
		}
		for (int i = 0; i < room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates.Count; i++)
		{
			if (room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates[i] == room.abstractRoom.name)
			{
				return;
			}
		}
		room.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates.Add(room.abstractRoom.name);
	}

	public void NewWorldLoaded()
	{
		waitingForWorldLoader = false;
		if (ModManager.MMF && MMF.cfgExtraTutorials.Value && room.game.IsStorySession)
		{
			room.game.GetStorySession.saveState.deathPersistentSaveData.GateStandTutorial = true;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		graphics.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		graphics.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		graphics.ApplyPalette(sLeaser, rCam, palette);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		graphics.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public int PlayersInZone()
	{
		if (room == null)
		{
			return -1;
		}
		int num = -1;
		if (ModManager.CoopAvailable)
		{
			foreach (AbstractCreature item in room.game.PlayersToProgressOrWin)
			{
				int num2 = DetectZone(item);
				if (num2 == num || num == -1)
				{
					num = num2;
					continue;
				}
				num = -1;
				break;
			}
			if (num < 0 && room.BeingViewed)
			{
				foreach (AbstractCreature item2 in room.game.PlayersToProgressOrWin)
				{
					if (DetectZone(item2) == -1)
					{
						try
						{
							room.game.cameras[0].hud.jollyMeter.customFade = 20f;
							room.game.cameras[0].hud.jollyMeter.playerIcons[(item2.state as PlayerState).playerNumber].blinkRed = 20;
						}
						catch
						{
						}
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				int num3 = DetectZone(room.game.Players[i]);
				if (num3 == num || num == -1)
				{
					num = num3;
					continue;
				}
				num = -1;
				break;
			}
		}
		return num;
	}

	private bool PlayersStandingStill()
	{
		if (ModManager.CoopAvailable)
		{
			List<AbstractCreature> playersToProgressOrWin = room.game.PlayersToProgressOrWin;
			List<AbstractCreature> list = (from x in room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Player>()
				select x.abstractCreature).ToList();
			bool flag = false;
			foreach (AbstractCreature item in playersToProgressOrWin)
			{
				if (item.realizedCreature == null)
				{
					continue;
				}
				if (!list.Contains(item))
				{
					int playerNumber = (item.state as PlayerState).playerNumber;
					JollyCustom.Log("Player " + playerNumber + " not in gate " + list.Count);
					try
					{
						room.game.cameras[0].hud.jollyMeter.customFade = 20f;
						room.game.cameras[0].hud.jollyMeter.playerIcons[playerNumber].blinkRed = 20;
					}
					catch
					{
					}
					flag = true;
				}
				if ((item.realizedCreature as Player).touchedNoInputCounter < 20 && (item.realizedCreature as Player).onBack == null)
				{
					flag = true;
				}
			}
			if (flag)
			{
				return false;
			}
		}
		else
		{
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature == null || (room.game.Players[i].realizedCreature as Player).touchedNoInputCounter < 20)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int DetectZone(AbstractCreature crit)
	{
		if (crit.pos.room != room.abstractRoom.index)
		{
			return -1;
		}
		if (crit.pos.x < room.TileWidth / 2 - 8)
		{
			return 0;
		}
		if (crit.pos.x < room.TileWidth / 2)
		{
			return 1;
		}
		if (crit.pos.x < room.TileWidth / 2 + 8)
		{
			return 2;
		}
		return 3;
	}

	private bool AllPlayersThroughToOtherSide()
	{
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].pos.room == room.abstractRoom.index && (!letThroughDir || room.game.Players[i].pos.x < room.TileWidth / 2 + 3) && (letThroughDir || room.game.Players[i].pos.x > room.TileWidth / 2 - 4))
			{
				return false;
			}
		}
		return true;
	}

	public void ChangeDoorStatus(int door, bool open)
	{
		int num = 14 + door * 9;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 8; j <= 16; j++)
			{
				room.GetTile(num + i, j).Terrain = ((!open) ? Room.Tile.TerrainType.Solid : Room.Tile.TerrainType.Air);
			}
		}
	}

	protected bool AllDoorsInPosition()
	{
		for (int i = 0; i < 3; i++)
		{
			if (doors[i].closedFac != goalDoorPositions[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool customOEGateRequirements()
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		bool flag = room.game.rainWorld.progression.miscProgressionData.beaten_Gourmand || room.game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full || global::MoreSlugcats.MoreSlugcats.chtUnlockOuterExpanse.Value;
		if (!(room.game.session is StoryGameSession))
		{
			return false;
		}
		if ((room.game.session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			return (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark || flag;
		}
		return ((room.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.White || (room.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.Yellow) && flag;
	}

	public void customKarmaGateRequirements()
	{
		if (ModManager.MSC && room.abstractRoom.name == "GATE_SB_OE" && !customOEGateRequirements())
		{
			karmaRequirements[0] = MoreSlugcatsEnums.GateRequirement.OELock;
			karmaRequirements[1] = MoreSlugcatsEnums.GateRequirement.OELock;
		}
		if (ModManager.MMF && MMF.cfgDisableGateKarma.Value)
		{
			if (int.TryParse(karmaRequirements[0].value, out var _))
			{
				karmaRequirements[0] = GateRequirement.OneKarma;
			}
			if (int.TryParse(karmaRequirements[1].value, out var _))
			{
				karmaRequirements[1] = GateRequirement.OneKarma;
			}
		}
	}
}
