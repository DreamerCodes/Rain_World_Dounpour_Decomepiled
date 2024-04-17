using System;
using System.Collections.Generic;
using System.Linq;
using DevInterface;
using Expedition;
using JollyCoop;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RainWorldGame : MainLoopProcess
{
	public struct SetupValues
	{
		public int pink;

		public int green;

		public int blue;

		public int white;

		public int spears;

		public int flies;

		public int leeches;

		public int snails;

		public int vultures;

		public int lanternMice;

		public int cicadas;

		public int palette;

		public int cycleTimeMin;

		public int cycleTimeMax;

		public int fliesToWin;

		public int yellows;

		public int reds;

		public int spiders;

		public int garbageWorms;

		public int jetFish;

		public int black;

		public int seaLeeches;

		public int salamanders;

		public int bigEels;

		public int defaultSettingsScreen;

		public int deers;

		public int daddyLongLegs;

		public int tubeWorms;

		public int broLongLegs;

		public int tentaclePlants;

		public int poleMimics;

		public int mirosBirds;

		public int templeGuards;

		public int centipedes;

		public int gravityFlickerCycleMin;

		public int gravityFlickerCycleMax;

		public int scavengers;

		public int scavengersShy;

		public int scavengersLikePlayer;

		public int centiWings;

		public int smallCentipedes;

		public int lungs;

		public int cheatKarma;

		public int overseers;

		public int ghosts;

		public int fireSpears;

		public int scavLanterns;

		public int scavBombs;

		public int custom;

		public int bigSpiders;

		public int eggBugs;

		public int singlePlayerChar;

		public int needleWorms;

		public int smallNeedleWorms;

		public int spitterSpiders;

		public int dropbugs;

		public int cyanLizards;

		public int kingVultures;

		public int redCentis;

		public int proceedLineages;

		public bool world;

		public bool player1;

		public bool player2;

		public bool lizardLaserEyes;

		public bool invincibility;

		public bool worldCreaturesSpawn;

		public bool bake;

		public bool OBSwidescreen;

		public bool startScreen;

		public bool cycleStartUp;

		public bool OBSfullscreen;

		public bool playerGlowing;

		public bool devToolsActive;

		public bool loadGame;

		public bool multiUseGates;

		public bool revealMap;

		public bool playMusic;

		public bool loadProg;

		public bool loadAllAmbientSounds;

		public bool alwaysTravel;

		public bool theMark;

		public bool logSpawned;

		public SlugcatStats.Name betaTestSlugcat;

		public string startMap;

		public bool disableRain;

		public bool forcePrecycles;

		public bool testMoonFixed;

		public bool testMoonCloak;

		public bool unlockMSCCharacters;

		public bool lockTravel;

		public bool forcePup;

		public bool cleanSpawns;

		public bool cleanMaps;

		public bool player3;

		public bool player4;

		public bool randomStart;

		public int artificerDreamTest;

		public bool saintInfinitePower;

		public int slugPupsMax;

		public bool arenaDefaultColors;

		public SetupValues(bool player2, int pink, int green, int blue, int white, int spears, int flies, int leeches, int snails, int vultures, int lanternMice, int cicadas, int palette, bool lizardLaserEyes, bool invincibility, int fliesToWin, bool worldCreaturesSpawn, bool bake, bool OBSwidescreen, bool startScreen, bool cycleStartUp, bool OBSfullscreen, int yellows, int reds, int spiders, bool playerGlowing, int garbageWorms, int jetFish, int black, int seaLeeches, int salamanders, int bigEels, bool player1, int defaultSettingsScreen, int deers, bool devToolsActive, int daddyLongLegs, int tubeWorms, int broLongLegs, int tentaclePlants, int poleMimics, int mirosBirds, bool loadGame, bool multiUseGates, int templeGuards, int centipedes, bool world, int gravityFlickerCycleMin, int gravityFlickerCycleMax, bool revealMap, int scavengers, int scavengersShy, int scavengersLikePlayer, int centiWings, int smallCentipedes, bool loadProg, int lungs, bool playMusic, int cycleTimeMin, int cycleTimeMax, int cheatKarma, bool loadAllAmbientSounds, int overseers, int ghosts, int fireSpears, int scavLanterns, bool alwaysTravel, int scavBombs, bool theMark, int custom, int bigSpiders, int eggBugs, int singlePlayerChar, int needleWorms, int smallNeedleWorms, int spitterSpiders, int dropbugs, int cyanLizards, int kingVultures, bool logSpawned, int redCentis, int proceedLineages)
		{
			this.player2 = player2;
			this.pink = pink;
			this.green = green;
			this.blue = blue;
			this.white = white;
			this.spears = spears;
			this.flies = flies;
			this.leeches = leeches;
			this.palette = palette;
			this.fliesToWin = 4;
			this.lizardLaserEyes = lizardLaserEyes;
			this.snails = snails;
			this.vultures = vultures;
			this.lanternMice = lanternMice;
			this.cicadas = cicadas;
			this.invincibility = invincibility;
			this.worldCreaturesSpawn = worldCreaturesSpawn;
			this.bake = bake;
			this.OBSwidescreen = OBSwidescreen;
			this.startScreen = startScreen;
			this.cycleStartUp = cycleStartUp;
			this.OBSfullscreen = OBSfullscreen;
			this.yellows = yellows;
			this.reds = reds;
			this.spiders = spiders;
			this.playerGlowing = playerGlowing;
			this.garbageWorms = garbageWorms;
			this.jetFish = jetFish;
			this.black = black;
			this.seaLeeches = seaLeeches;
			this.salamanders = salamanders;
			this.bigEels = bigEels;
			this.player1 = player1;
			this.defaultSettingsScreen = defaultSettingsScreen;
			this.deers = deers;
			this.devToolsActive = devToolsActive;
			this.daddyLongLegs = daddyLongLegs;
			this.tubeWorms = tubeWorms;
			this.broLongLegs = broLongLegs;
			this.tentaclePlants = tentaclePlants;
			this.poleMimics = poleMimics;
			this.mirosBirds = mirosBirds;
			this.loadGame = loadGame;
			this.multiUseGates = multiUseGates;
			this.templeGuards = templeGuards;
			this.centipedes = centipedes;
			this.world = world;
			this.gravityFlickerCycleMin = gravityFlickerCycleMin;
			this.gravityFlickerCycleMax = gravityFlickerCycleMax;
			this.revealMap = revealMap;
			this.scavengers = scavengers;
			this.scavengersShy = scavengersShy;
			this.scavengersLikePlayer = scavengersLikePlayer;
			this.centiWings = centiWings;
			this.smallCentipedes = smallCentipedes;
			this.loadProg = loadProg;
			this.lungs = lungs;
			this.playMusic = playMusic;
			this.cycleTimeMin = cycleTimeMin;
			this.cycleTimeMax = cycleTimeMax;
			this.cheatKarma = cheatKarma;
			this.loadAllAmbientSounds = loadAllAmbientSounds;
			this.overseers = overseers;
			this.ghosts = ghosts;
			this.fireSpears = fireSpears;
			this.scavLanterns = scavLanterns;
			this.alwaysTravel = alwaysTravel;
			this.scavBombs = scavBombs;
			this.theMark = theMark;
			this.custom = custom;
			this.bigSpiders = bigSpiders;
			this.eggBugs = eggBugs;
			this.singlePlayerChar = singlePlayerChar;
			this.needleWorms = needleWorms;
			this.smallNeedleWorms = smallNeedleWorms;
			this.spitterSpiders = spitterSpiders;
			this.dropbugs = dropbugs;
			this.cyanLizards = cyanLizards;
			this.kingVultures = kingVultures;
			this.logSpawned = logSpawned;
			this.redCentis = redCentis;
			this.proceedLineages = proceedLineages;
			betaTestSlugcat = null;
			startMap = "";
			disableRain = false;
			forcePrecycles = false;
			testMoonFixed = false;
			testMoonCloak = false;
			unlockMSCCharacters = false;
			lockTravel = false;
			forcePup = false;
			cleanSpawns = false;
			cleanMaps = false;
			player3 = false;
			player4 = false;
			randomStart = false;
			artificerDreamTest = -1;
			saintInfinitePower = false;
			slugPupsMax = -1;
			arenaDefaultColors = false;
		}
	}

	private static readonly AGLog<RainWorldGame> Log = new AGLog<RainWorldGame>();

	private int updateAbstractRoom;

	private int updateShortCut;

	public bool DEBUGMODE;

	public bool showAImap;

	private DebugGraphDrawer debugGraphDrawer;

	public int nextIssuedId;

	public RoomRealizer roomRealizer;

	public int numberOfUtilityVisualizers;

	public AbstractSpaceVisualizer abstractSpaceVisualizer;

	public OverWorld overWorld;

	public bool evenUpdate;

	public PauseMenu pauseMenu;

	public List<global::Menu.Menu> grafUpdateMenus;

	public bool mapVisible;

	private bool mDown;

	public AccessibilityVisualizer AV;

	private bool pDown;

	public bool devToolsActive;

	public FLabel devToolsLabel;

	private bool oDown;

	private bool hDown;

	private bool lastRestartButton;

	private bool lastPauseButton;

	public DevUI devUI;

	public RainWorld rainWorld;

	public GameSession session;

	public PathfinderResourceDivider pathfinderResourceDivider;

	public int clock;

	public GlobalRain globalRain;

	public bool playedGameOverSound;

	public string startingRoom;

	public ArenaOverlay arenaOverlay;

	public GhostWorldPresence.GhostID sawAGhost;

	public bool spawnedPendingObjects;

	public bool pauseUpdate;

	public ConsoleVisualizer console;

	public bool consoleVisible;

	public bool kDown;

	public bool autoPupStoryCompanionPlayers;

	public bool rivuletEpilogueRainPause;

	public bool wasAnArtificerDream;

	public int timeWithoutCorpse;

	public int timeSinceScavsSentToPlayer;

	public int timeInRegionThisCycle;

	public bool paused;

	private const int MaxConcurrentHeavyAi = 3;

	private static int _concurrentHeavyAi = 0;

	private static int _concurrentHeavyAiDelayedExceptLastRunAis = 0;

	private static readonly List<EntityID> _lastRunAiIds = new List<EntityID>(3);

	public SetupValues setupValues => rainWorld.setup;

	public RoomCamera[] cameras { get; private set; }

	public ShortcutHandler shortcuts { get; private set; }

	public World world => overWorld.activeWorld;

	public override float TimeSpeedFac
	{
		get
		{
			if (GamePaused)
			{
				return 0f;
			}
			return base.TimeSpeedFac;
		}
	}

	public override bool AllowDialogs => true;

	public bool GameOverModeActive
	{
		get
		{
			if (cameras[0].hud != null && cameras[0].hud.textPrompt != null)
			{
				return cameras[0].hud.textPrompt.gameOverMode;
			}
			return false;
		}
	}

	public SoundLoader soundLoader => manager.soundLoader;

	public List<AbstractCreature> Players => session.Players;

	public List<AbstractCreature> NonPermaDeadPlayers => session.Players.Where((AbstractCreature x) => !(x.state as PlayerState).permaDead).ToList();

	public List<AbstractCreature> PlayersToProgressOrWin
	{
		get
		{
			if (Custom.rainWorld.options.jollyDifficulty == Options.JollyDifficulty.EASY)
			{
				return AlivePlayers;
			}
			return Players;
		}
	}

	public List<AbstractCreature> AlivePlayers => session.Players.Where((AbstractCreature x) => !(x.state as PlayerState).dead && !(x.state as PlayerState).permaDead).ToList();

	public AbstractCreature FirstAlivePlayer
	{
		get
		{
			if (!ModManager.CoopAvailable)
			{
				if (Players.Count != 0)
				{
					return Players[0];
				}
				return null;
			}
			AbstractCreature abstractCreature = null;
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i] != null)
				{
					abstractCreature = Players[i];
					if (!abstractCreature.state.dead && !(abstractCreature.state as PlayerState).permaDead)
					{
						return abstractCreature;
					}
				}
			}
			return abstractCreature;
		}
	}

	public AbstractCreature FirstAnyPlayer
	{
		get
		{
			if (!ModManager.CoopAvailable)
			{
				if (Players.Count != 0)
				{
					return Players[0];
				}
				return null;
			}
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i] != null)
				{
					return Players[i];
				}
			}
			return null;
		}
	}

	public Player RealizedPlayerFollowedByCamera
	{
		get
		{
			if (Players.Count == 0)
			{
				return null;
			}
			return (Player)(cameras[0].followAbstractCreature?.realizedCreature);
		}
	}

	public Player FirstRealizedPlayer
	{
		get
		{
			if (Players.Count == 0)
			{
				return null;
			}
			return (Player)(Players[0]?.realizedCreature);
		}
	}

	public bool AllPlayersRealized
	{
		get
		{
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i].realizedCreature == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsArenaSession => session is ArenaGameSession;

	public bool IsStorySession => session is StoryGameSession;

	public ArenaGameSession GetArenaGameSession => session as ArenaGameSession;

	public StoryGameSession GetStorySession => session as StoryGameSession;

	public SlugcatStats.Name StoryCharacter
	{
		get
		{
			if (!IsStorySession)
			{
				return null;
			}
			return GetStorySession.saveStateNumber;
		}
	}

	public override float FadeInTime
	{
		get
		{
			if (manager.menuSetup.FastTravelInitCondition)
			{
				return 0f;
			}
			if (manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.New)
			{
				return 2f;
			}
			if (manager.menuSetup.startGameCondition != ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
			{
				if (rainWorld.progression.currentSaveState != null)
				{
					return rainWorld.progression.currentSaveState.SlowFadeIn;
				}
				return 0.8f;
			}
			return base.FadeInTime;
		}
	}

	public override float InitialBlackSeconds
	{
		get
		{
			if (manager.menuSetup.FastTravelInitCondition)
			{
				return 10f;
			}
			if (manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.New)
			{
				if (!(rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == SlugcatStats.Name.Red))
				{
					return 3f;
				}
				return 5.5f;
			}
			if (manager.menuSetup.startGameCondition != ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
			{
				return 0.75f;
			}
			return base.InitialBlackSeconds;
		}
	}

	public int StoryPlayerCount
	{
		get
		{
			int num = 0;
			if (setupValues.player1)
			{
				num++;
			}
			if (setupValues.player2)
			{
				num++;
			}
			if (setupValues.player3)
			{
				num++;
			}
			if (setupValues.player4)
			{
				num++;
			}
			if (ModManager.CoopAvailable)
			{
				return Math.Max(num, rainWorld.options.JollyPlayerCount);
			}
			return num;
		}
	}

	public static float DefaultHeatSourceWarmth => 0.0005f;

	public static float BlizzardMaxColdness => DefaultHeatSourceWarmth * 3.2f;

	public bool GamePaused
	{
		get
		{
			if (pauseMenu == null)
			{
				return paused;
			}
			return true;
		}
	}

	public Player RealizedPlayerOfPlayerNumber(int i)
	{
		Player result = null;
		foreach (AbstractCreature player2 in Players)
		{
			Player player = null;
			if (player2.realizedCreature != null)
			{
				player = player2.realizedCreature as Player;
			}
			if (player != null && player.playerState.playerNumber == i)
			{
				result = player;
				break;
			}
		}
		return result;
	}

	public RainWorldGame(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Game)
	{
		rainWorld = manager.rainWorld;
		startingRoom = "";
		Shader.SetGlobalFloat(RainWorld.ShadPropWindDir, ModManager.MSC ? (-1f) : 1f);
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.NewCycleEvent();
		}
		nextIssuedId = UnityEngine.Random.Range(1000, 10000);
		shortcuts = new ShortcutHandler(this);
		globalRain = new GlobalRain(this);
		cameras = new RoomCamera[1];
		cameras[0] = new RoomCamera(this, 0);
		grafUpdateMenus = new List<global::Menu.Menu>();
		wasAnArtificerDream = false;
		if (ModManager.MSC && manager.artificerDreamNumber != -1)
		{
			wasAnArtificerDream = true;
			RainWorld.lockGameTimer = true;
		}
		if (manager.arenaSitting != null)
		{
			if (manager.arenaSitting.gameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive)
			{
				session = new CompetitiveGameSession(this);
			}
			else if (manager.arenaSitting.gameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox || (ModManager.MSC && manager.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge))
			{
				session = new SandboxGameSession(this);
			}
		}
		else if (ModManager.MSC && rainWorld.safariMode)
		{
			session = new StoryGameSession(manager.rainWorld.safariSlugcat, this);
		}
		else if (ModManager.MSC && wasAnArtificerDream)
		{
			session = new StoryGameSession(MoreSlugcatsEnums.SlugcatStatsName.Slugpup, this);
		}
		else
		{
			session = new StoryGameSession(manager.rainWorld.progression.PlayingAsSlugcat, this);
		}
		overWorld = new OverWorld(this);
		if (world.singleRoomWorld)
		{
			DefaultRoomSettings.ancestor.pal = setupValues.palette;
		}
		if (IsArenaSession)
		{
			GetArenaGameSession.SpawnCreatures();
		}
		if (IsStorySession)
		{
			startingRoom = GetStorySession.saveState.GetSaveStateDenToUse();
			if (rainWorld.setup.cheatKarma > 0)
			{
				GetStorySession.saveState.deathPersistentSaveData.karma = rainWorld.setup.cheatKarma - 1;
				GetStorySession.saveState.deathPersistentSaveData.karmaCap = Math.Max(GetStorySession.saveState.deathPersistentSaveData.karmaCap, GetStorySession.saveState.deathPersistentSaveData.karma);
			}
			if (rainWorld.safariMode)
			{
				ModManager.CoopAvailable = false;
			}
			playedGameOverSound = false;
			if (ModManager.JollyCoop && ModManager.CoopAvailable && rainWorld.options.JollyPlayerCount > 1)
			{
				UserInput.SetUserCount(rainWorld.options.JollyPlayerCount);
			}
			else
			{
				UserInput.SetUserCount(1);
			}
		}
		else
		{
			JollyCustom.Log("Game mode is arena, setting coop to false!" + ModManager.CoopAvailable);
			ModManager.CoopAvailable = false;
		}
		pathfinderResourceDivider = new PathfinderResourceDivider(this);
		devToolsActive = setupValues.devToolsActive;
		if (ModManager.MSC && IsArenaSession && GetArenaGameSession.chMeta != null && GetArenaGameSession.chMeta.challengeNumber <= MultiplayerUnlocks.TOTAL_CHALLENGES)
		{
			devToolsActive = false;
		}
		devToolsLabel = new FLabel(Custom.GetFont(), "Dev tools active");
		Futile.stage.AddChild(devToolsLabel);
		devToolsLabel.x = rainWorld.options.ScreenSize.x / 2f + 0.01f;
		devToolsLabel.y = 755.01f;
		devToolsLabel.color = new Color(1f, 1f, 0.5f);
		devToolsLabel.isVisible = devToolsActive;
		int num = 0;
		if (world.GetAbstractRoom(overWorld.FIRSTROOM) != null)
		{
			num = world.GetAbstractRoom(overWorld.FIRSTROOM).index;
		}
		if (!world.IsRoomInRegion(num))
		{
			num = world.region.firstRoomIndex;
		}
		for (int i = 0; i < setupValues.pink; i++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Pink Lizard"), null, new WorldCoordinate(num, 25 + i * 2, 25, -1), GetNewID()));
		}
		for (int j = 0; j < setupValues.green; j++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Green Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int k = 0; k < setupValues.blue; k++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Blue Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int l = 0; l < setupValues.yellows; l++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Yellow Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int m = 0; m < setupValues.white; m++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("White Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int n = 0; n < setupValues.reds; n++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Red Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num2 = 0; num2 < setupValues.black; num2++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Black Lizard"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num3 = 0; num3 < setupValues.salamanders; num3++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Salamander"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num4 = 0; num4 < setupValues.spears; num4++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractSpear(world, null, new WorldCoordinate(num, 20 + num4, 15, -1), GetNewID(), explosive: false));
		}
		for (int num5 = 0; num5 < setupValues.fireSpears; num5++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractSpear(world, null, new WorldCoordinate(num, 20 + num5, 15, -1), GetNewID(), explosive: true));
		}
		for (int num6 = 0; num6 < setupValues.scavLanterns; num6++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, new WorldCoordinate(num, 30 + num6, 15, -1), GetNewID()));
		}
		for (int num7 = 0; num7 < setupValues.scavBombs; num7++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, new WorldCoordinate(num, 30 + num7, 15, -1), GetNewID()));
		}
		for (int num8 = 0; num8 < setupValues.flies; num8++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, new WorldCoordinate(num, 26, 15, -1), GetNewID()));
		}
		for (int num9 = 0; num9 < setupValues.leeches; num9++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num10 = 0; num10 < setupValues.seaLeeches; num10++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SeaLeech), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num11 = 0; num11 < setupValues.spiders; num11++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num12 = 0; num12 < setupValues.snails; num12++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Snail), null, new WorldCoordinate(num, 26, 25, -1), GetNewID()));
		}
		for (int num13 = 0; num13 < setupValues.garbageWorms; num13++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GarbageWorm), null, new WorldCoordinate(num, -1, -1, -1), GetNewID()));
		}
		AbstractCreature abstractCreature = null;
		for (int num14 = 0; num14 < setupValues.vultures; num14++)
		{
			abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Vulture"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature);
		}
		for (int num15 = 0; num15 < setupValues.kingVultures; num15++)
		{
			abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.KingVulture), null, new WorldCoordinate(num, 26, 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature);
		}
		AbstractCreature abstractCreature2 = null;
		for (int num16 = 0; num16 < setupValues.cicadas; num16++)
		{
			abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Cicada " + ((UnityEngine.Random.value < 0.5f) ? "A" : "B")), null, new WorldCoordinate(num, (num16 == 0) ? 40 : (15 + num16), 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature2);
		}
		AbstractCreature abstractCreature3 = null;
		for (int num17 = 0; num17 < setupValues.lanternMice; num17++)
		{
			abstractCreature3 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Lantern Mouse"), null, new WorldCoordinate(num, 26, 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature3);
		}
		AbstractCreature abstractCreature4 = null;
		for (int num18 = 0; num18 < setupValues.jetFish; num18++)
		{
			abstractCreature4 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Jet Fish"), null, new WorldCoordinate(num, 25 + num18, 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature4);
		}
		for (int num19 = 0; num19 < setupValues.bigEels; num19++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Big Eel"), null, new WorldCoordinate(num, 25 + num19, 25, -1), GetNewID()));
		}
		for (int num20 = 0; num20 < setupValues.deers; num20++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Deer"), null, new WorldCoordinate(num, 25 + num20, 25, -1), GetNewID()));
		}
		for (int num21 = 0; num21 < setupValues.tubeWorms; num21++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TubeWorm), null, new WorldCoordinate(num, 25 + num21, 25, -1), GetNewID()));
		}
		for (int num22 = 0; num22 < setupValues.daddyLongLegs; num22++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs), null, new WorldCoordinate(num, 25 + num22, 25, -1), GetNewID()));
		}
		for (int num23 = 0; num23 < setupValues.broLongLegs; num23++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BrotherLongLegs), null, new WorldCoordinate(num, 25 + num23, 25, -1), GetNewID()));
		}
		for (int num24 = 0; num24 < setupValues.mirosBirds; num24++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.MirosBird), null, new WorldCoordinate(num, 25 + num24, 25, -1), GetNewID()));
		}
		for (int num25 = 0; num25 < setupValues.templeGuards; num25++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, new WorldCoordinate(num, 25 + num25, 25, -1), GetNewID()));
		}
		for (int num26 = 0; num26 < setupValues.centipedes; num26++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centipede), null, new WorldCoordinate(num, 25 + num26, 25, -1), GetNewID()));
		}
		for (int num27 = 0; num27 < setupValues.redCentis; num27++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.RedCentipede), null, new WorldCoordinate(num, 25 + num27, 25, -1), GetNewID()));
		}
		for (int num28 = 0; num28 < setupValues.centiWings; num28++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centiwing), null, new WorldCoordinate(num, 25 + num28, 25, -1), GetNewID()));
		}
		for (int num29 = 0; num29 < setupValues.smallCentipedes; num29++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallCentipede), null, new WorldCoordinate(num, 25 + num29, 25, -1), GetNewID()));
		}
		for (int num30 = 0; num30 < setupValues.bigSpiders; num30++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigSpider), null, new WorldCoordinate(num, 25 + num30, 25, -1), GetNewID()));
		}
		for (int num31 = 0; num31 < setupValues.spitterSpiders; num31++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SpitterSpider), null, new WorldCoordinate(num, 25 + num31, 25, -1), GetNewID()));
		}
		for (int num32 = 0; num32 < setupValues.eggBugs; num32++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.EggBug), null, new WorldCoordinate(num, 25 + num32, 25, -1), GetNewID()));
		}
		for (int num33 = 0; num33 < setupValues.needleWorms; num33++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigNeedleWorm), null, new WorldCoordinate(num, 25 + num33, 25, -1), GetNewID()));
		}
		for (int num34 = 0; num34 < setupValues.smallNeedleWorms; num34++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, new WorldCoordinate(num, 25 + num34, 25, -1), GetNewID()));
		}
		for (int num35 = 0; num35 < setupValues.dropbugs; num35++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DropBug), null, new WorldCoordinate(num, 25 + num35, 25, -1), GetNewID()));
		}
		for (int num36 = 0; num36 < setupValues.cyanLizards; num36++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CyanLizard), null, new WorldCoordinate(num, 25 + num36, 25, -1), GetNewID()));
		}
		AbstractCreature abstractCreature5 = null;
		for (int num37 = 0; num37 < setupValues.scavengers; num37++)
		{
			abstractCreature5 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, new WorldCoordinate(num, 25 + num37, 25, -1), GetNewID());
			world.GetAbstractRoom(num).AddEntity(abstractCreature5);
		}
		for (int num38 = 0; num38 < setupValues.overseers; num38++)
		{
			world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(num, 14 + num38, 18, -1), GetNewID()));
		}
		if (world.GetAbstractRoom(num).dens > 0)
		{
			int num39 = 4;
			for (int num40 = 0; num40 < setupValues.tentaclePlants; num40++)
			{
				if (num39 >= world.GetAbstractRoom(num).nodes.Length)
				{
					num39 = 0;
				}
				int num41 = 0;
				while (world.GetAbstractRoom(num).nodes[num39].type != AbstractRoomNode.Type.Den && num41 < 1000)
				{
					num39++;
					if (num39 >= world.GetAbstractRoom(num).nodes.Length)
					{
						num39 = 0;
					}
					num41++;
				}
				world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TentaclePlant), null, new WorldCoordinate(num, -1, -1, num39), GetNewID()));
				num39++;
			}
			for (int num42 = 0; num42 < setupValues.poleMimics; num42++)
			{
				if (num39 >= world.GetAbstractRoom(num).nodes.Length)
				{
					num39 = 0;
				}
				int num43 = 0;
				while (world.GetAbstractRoom(num).nodes[num39].type != AbstractRoomNode.Type.Den && num43 < 1000)
				{
					num39++;
					if (num39 >= world.GetAbstractRoom(num).nodes.Length)
					{
						num39 = 0;
					}
					num43++;
				}
				world.GetAbstractRoom(num).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PoleMimic), null, new WorldCoordinate(num, -1, -1, num39), GetNewID()));
				num39++;
			}
		}
		AbstractCreature abstractCreature6 = null;
		if (IsStorySession && (!ModManager.MSC || !rainWorld.safariMode))
		{
			abstractCreature6 = SpawnPlayers(player1: true, setupValues.player2, setupValues.player3, setupValues.player4, new WorldCoordinate(num, 15, 25, -1));
		}
		if (IsStorySession && ModManager.MSC && GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear && GetStorySession.saveState.deathPersistentSaveData.altEnding)
		{
			rainWorld.progression.miscProgressionData.beaten_SpearMaster_AltEnd = true;
		}
		if (IsStorySession && ModManager.MSC && GetStorySession.saveState.miscWorldSaveData.SLOracleState != null)
		{
			bool flag = false;
			foreach (DataPearl.AbstractDataPearl.DataPearlType significantPearl in GetStorySession.saveState.miscWorldSaveData.SLOracleState.significantPearls)
			{
				if (GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					if (!rainWorld.progression.miscProgressionData.GetPebblesPearlDeciphered(significantPearl))
					{
						rainWorld.progression.miscProgressionData.SetPebblesPearlDeciphered(significantPearl);
						flag = true;
					}
				}
				else if (GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					if (!rainWorld.progression.miscProgressionData.GetDMPearlDeciphered(significantPearl))
					{
						rainWorld.progression.miscProgressionData.SetDMPearlDeciphered(significantPearl);
						flag = true;
					}
				}
				else if (GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					if (!rainWorld.progression.miscProgressionData.GetFuturePearlDeciphered(significantPearl))
					{
						rainWorld.progression.miscProgressionData.SetFuturePearlDeciphered(significantPearl);
						flag = true;
					}
				}
				else if (!rainWorld.progression.miscProgressionData.GetPearlDeciphered(significantPearl))
				{
					rainWorld.progression.miscProgressionData.SetPearlDeciphered(significantPearl);
					flag = true;
				}
			}
			if (flag)
			{
				rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
			}
		}
		for (int num44 = 0; num44 < Players.Count; num44++)
		{
			if (world.GetAbstractRoom(Players[num44].pos) != null)
			{
				IntVector2 pos;
				if (world.GetAbstractRoom(Players[num44].pos).shelter)
				{
					Players[num44].pos.WashTileData();
				}
				else if (TryGetPlayerStartPos(world.GetAbstractRoom(Players[num44].pos).name, out pos))
				{
					Players[num44].pos.Tile = pos;
				}
			}
		}
		if (IsStorySession && manager.menuSetup.LoadInitCondition && (!ModManager.MSC || !rainWorld.safariMode))
		{
			int num45 = GetStorySession.saveState.food;
			if (!ModManager.CoopAvailable)
			{
				while (num45 > 0)
				{
					for (int num46 = 0; num46 < Players.Count; num46++)
					{
						(Players[num46].state as PlayerState).foodInStomach++;
						num45--;
					}
				}
			}
			else if (num45 > 0)
			{
				for (int num47 = 0; num47 < Players.Count; num47++)
				{
					(Players[num47].state as PlayerState).foodInStomach = num45;
				}
			}
		}
		if (abstractCreature == null)
		{
			for (int num48 = 0; num48 < world.NumberOfRooms; num48++)
			{
				for (int num49 = 0; num49 < world.GetAbstractRoom(num48 + world.firstRoomIndex).creatures.Count; num49++)
				{
					if (world.GetAbstractRoom(num48 + world.firstRoomIndex).creatures[num49].creatureTemplate.type == CreatureTemplate.Type.Vulture)
					{
						abstractCreature = world.GetAbstractRoom(num48 + world.firstRoomIndex).creatures[num49];
						break;
					}
				}
			}
		}
		AbstractCreature abstractCreature7 = null;
		if (abstractCreature7 == null)
		{
			for (int num50 = 0; num50 < world.NumberOfRooms; num50++)
			{
				for (int num51 = 0; num51 < world.GetAbstractRoom(num50 + world.firstRoomIndex).creatures.Count; num51++)
				{
					if (world.GetAbstractRoom(num50 + world.firstRoomIndex).creatures[num51].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
					{
						abstractCreature7 = world.GetAbstractRoom(num50 + world.firstRoomIndex).creatures[num51];
						break;
					}
				}
			}
		}
		if (!IsArenaSession)
		{
			if (abstractCreature6 != null)
			{
				cameras[0].followAbstractCreature = abstractCreature6;
			}
			else if (abstractCreature5 != null && abstractCreature5.Room.index == num)
			{
				cameras[0].followAbstractCreature = abstractCreature5;
			}
			else if (abstractCreature7 != null && abstractCreature7.Room.index == num)
			{
				cameras[0].followAbstractCreature = abstractCreature7;
			}
			else if (abstractCreature != null && abstractCreature.Room.index == num)
			{
				cameras[0].followAbstractCreature = abstractCreature;
			}
			else if (abstractCreature2 != null && abstractCreature2.Room.index == num)
			{
				cameras[0].followAbstractCreature = abstractCreature2;
			}
			else if (abstractCreature3 != null && abstractCreature3.Room.index == num)
			{
				cameras[0].followAbstractCreature = abstractCreature3;
			}
			if (cameras[0].followAbstractCreature == null)
			{
				for (int num52 = 0; num52 < world.GetAbstractRoom(num).creatures.Count; num52++)
				{
					if (!world.GetAbstractRoom(num).creatures[num52].creatureTemplate.smallCreature && !world.GetAbstractRoom(num).creatures[num52].pos.NodeDefined)
					{
						cameras[0].followAbstractCreature = world.GetAbstractRoom(num).creatures[num52];
						break;
					}
				}
			}
			if (cameras[0].followAbstractCreature == null)
			{
				for (int num53 = 0; num53 < world.GetAbstractRoom(num).creatures.Count; num53++)
				{
					if (!world.GetAbstractRoom(num).creatures[num53].creatureTemplate.smallCreature)
					{
						cameras[0].followAbstractCreature = world.GetAbstractRoom(num).creatures[num53];
						break;
					}
				}
			}
			if (ModManager.MSC && rainWorld.safariMode)
			{
				AbstractCreature abstractCreature8 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Overseer"), null, new WorldCoordinate(num, 15, 25, -1), new EntityID(-1, 0));
				world.GetAbstractRoom(num).AddEntity(abstractCreature8);
				cameras[0].followAbstractCreature = abstractCreature8;
				(abstractCreature8.abstractAI as OverseerAbstractAI).safariOwner = true;
				abstractCreature8.ignoreCycle = true;
				GetStorySession.saveState.deathPersistentSaveData.karma = 0;
			}
			if (!ModManager.MSC && cameras[0].followAbstractCreature == null)
			{
				cameras[0].followAbstractCreature = world.GetAbstractRoom(num).creatures[0];
			}
		}
		world.ActivateRoom(num);
		abstractSpaceVisualizer = new AbstractSpaceVisualizer(world, world.activeRooms[0]);
		abstractSpaceVisualizer.Visibility(mapVisible);
		console = new ConsoleVisualizer();
		console.Visibility(consoleVisible);
		for (int num54 = 0; num54 < cameras.Length; num54++)
		{
			cameras[num54].MoveCamera(world.activeRooms[0], 0);
		}
		if (!world.singleRoomWorld)
		{
			roomRealizer = new RoomRealizer(cameras[0].followAbstractCreature, world);
		}
		for (int num55 = 0; num55 < 4; num55++)
		{
			PlayerHandler playerHandler = rainWorld.GetPlayerHandler(num55);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.ResetHandler(isPlayerDead: false);
			}
		}
		if (ModManager.Expedition && rainWorld.ExpeditionMode)
		{
			ExpeditionGame.SetUpUnlockTrackers(this);
			ExpeditionGame.SetUpBurdenTrackers(this);
		}
	}

	public static bool TryGetPlayerStartPos(string room, out IntVector2 pos)
	{
		switch (room)
		{
		case "DS_B02":
			pos = new IntVector2(12, 25);
			break;
		case "SH_D01":
			pos = new IntVector2(15, 10);
			break;
		case "GW_B01":
			pos = new IntVector2(10, 19);
			break;
		case "SB_H03":
			pos = new IntVector2(13, 70);
			break;
		case "SS_A08":
			pos = new IntVector2(8, 13);
			break;
		case "SU_A22":
			pos = new IntVector2(22, 12);
			break;
		case "SI_B13":
			pos = new IntVector2(36, 49);
			break;
		case "SL_A08":
			pos = new IntVector2(8, 13);
			break;
		case "SB_F03":
			pos = new IntVector2(43, 208);
			break;
		case "SU_C04":
			pos = new IntVector2(7, 28);
			break;
		case "LH_J01":
			pos = new IntVector2(350, 13);
			break;
		default:
			pos = new IntVector2(0, 0);
			return false;
		}
		return true;
	}

	public bool AllowRainCounterToTick()
	{
		if ((setupValues.disableRain || (ModManager.MSC && world.game.rainWorld.safariMode && world.game.rainWorld.safariRainDisable)) && world.rainCycle.TimeUntilRain / 40 < 400 && world.rainCycle.timer > 500)
		{
			return false;
		}
		for (int i = 0; i < cameras.Length; i++)
		{
			if (cameras[i].ghostMode > 0.5f)
			{
				return false;
			}
			bool flag = ModManager.MSC && world.rainCycle.preTimer > 0;
			if (ModManager.MMF && !flag && cameras[i].hud != null && cameras[i].hud.dialogBox != null && cameras[i].hud.dialogBox.ShowingAMessage)
			{
				return false;
			}
		}
		if (!InActiveGate())
		{
			if (ModManager.MSC)
			{
				return !rivuletEpilogueRainPause;
			}
			return true;
		}
		return false;
	}

	public bool InActiveGate()
	{
		if (!ModManager.CoopAvailable)
		{
			if (Players.Count < 1 || Players[0].Room == null || !Players[0].Room.gate || Players[0].Room.realizedRoom == null || Players[0].Room.realizedRoom.regionGate == null)
			{
				return false;
			}
			return Players[0].Room.realizedRoom.regionGate.mode != RegionGate.Mode.MiddleClosed;
		}
		foreach (AbstractCreature alivePlayer in AlivePlayers)
		{
			if (alivePlayer.Room?.realizedRoom?.regionGate != null && alivePlayer.Room.realizedRoom.regionGate.mode != RegionGate.Mode.MiddleClosed)
			{
				return true;
			}
		}
		return false;
	}

	public bool InClosingShelter()
	{
		if (!ModManager.CoopAvailable)
		{
			if (Players.Count < 1 || Players[0].Room == null || !Players[0].Room.shelter || Players[0].Room.realizedRoom == null || Players[0].Room.realizedRoom.shelterDoor == null)
			{
				return false;
			}
			return Players[0].Room.realizedRoom.shelterDoor.IsClosing;
		}
		foreach (AbstractCreature alivePlayer in AlivePlayers)
		{
			if (alivePlayer.Room?.realizedRoom?.shelterDoor != null && alivePlayer.Room.realizedRoom.shelterDoor.IsClosing)
			{
				return true;
			}
		}
		return false;
	}

	public EntityID GetNewID()
	{
		nextIssuedId++;
		return new EntityID(-1, nextIssuedId);
	}

	public EntityID GetNewID(int spawner)
	{
		nextIssuedId++;
		return new EntityID(spawner, nextIssuedId);
	}

	public float SeededRandom(int seed)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		float value = UnityEngine.Random.value;
		UnityEngine.Random.state = state;
		return value;
	}

	public int SeededRandomRange(int seed, int min, int exclMax)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		int result = UnityEngine.Random.Range(min, exclMax);
		UnityEngine.Random.state = state;
		return result;
	}

	public void GameOver(Creature.Grasp dependentOnGrasp)
	{
		Custom.Log("Trying to game over...");
		if (ModManager.MSC && manager.artificerDreamNumber > -1)
		{
			ArtificerDreamEnd();
		}
		else
		{
			if (!IsStorySession)
			{
				return;
			}
			if (ModManager.CoopAvailable && rainWorld.options.JollyPlayerCount > 1)
			{
				if (!JollyGameOverEvaluator(dependentOnGrasp))
				{
					return;
				}
				if (!playedGameOverSound && dependentOnGrasp == null)
				{
					cameras[0].hud.PlaySound(SoundID.HUD_Game_Over_Prompt);
					playedGameOverSound = true;
				}
			}
			if (GetStorySession.RedIsOutOfCycles)
			{
				GoToRedsGameOver();
				return;
			}
			GetStorySession.PlaceKarmaFlowerOnDeathSpot();
			if (cameras[0].hud != null)
			{
				if (Players[0].realizedCreature != null)
				{
					if (Players[0].realizedCreature.room != null)
					{
						cameras[0].hud.InitGameOverMode(dependentOnGrasp, (Players[0].realizedCreature as Player).FoodInStomach, Players[0].pos.room, Custom.RestrictInRect((Players[0].realizedCreature as Player).mainBodyChunk.pos, Players[0].realizedCreature.room.RoomRect.Grow(50f)));
					}
					else
					{
						cameras[0].hud.InitGameOverMode(dependentOnGrasp, (Players[0].realizedCreature as Player).FoodInStomach, Players[0].pos.room, (Players[0].realizedCreature as Player).mainBodyChunk.pos);
					}
				}
				else
				{
					cameras[0].hud.InitGameOverMode(dependentOnGrasp, 0, Players[0].pos.room, new Vector2(0f, 0f));
				}
			}
			if (manager.musicPlayer != null)
			{
				manager.musicPlayer.DeathEvent();
			}
		}
	}

	private bool JollyGameOverEvaluator(Creature.Grasp dependentOnGrasp)
	{
		JollyCustom.Log($"Evaluating game over [{rainWorld.options.jollyDifficulty}]");
		if (cameras[0].InCutscene && cameras[0].cutsceneType == RoomCamera.CameraCutsceneType.VoidSea)
		{
			JollyCustom.Log("Avoding gameover on voidsea scene!");
			return false;
		}
		if (dependentOnGrasp != null)
		{
			AbstractCreature firstAlivePlayer = FirstAlivePlayer;
			if (AlivePlayers.Count == 0 || (AlivePlayers.Count == 1 && firstAlivePlayer != null && (firstAlivePlayer.realizedCreature as Player)?.dangerGrasp != null))
			{
				JollyCustom.Log("[Jolly] Grasp: Triggering GameOver! No players can help! " + AlivePlayers.Count);
				return true;
			}
			JollyCustom.Log("[Jolly] Grasp: Player grabbed by lizard, but some players are still alive! " + AlivePlayers.Count);
			return false;
		}
		Custom.Log("Game over triggered, not grasp dependent...");
		if (NonPermaDeadPlayers.Count != 0 && AlivePlayers.Count != 0)
		{
			if (rainWorld.options.jollyDifficulty == Options.JollyDifficulty.EASY)
			{
				JollyCustom.Log("[Jolly] Avoiding gameOver! Some players are still are alive");
				return false;
			}
			if (rainWorld.options.jollyDifficulty == Options.JollyDifficulty.NORMAL && NonPermaDeadPlayers.Count < rainWorld.options.JollyPlayerCount)
			{
				Custom.Log("[Jolly" + rainWorld.options.jollyDifficulty?.ToString() + "] Triggering GameOver! Nonpermadead players different as current players!! " + NonPermaDeadPlayers.Count + "/" + rainWorld.options.JollyPlayerCount);
				return true;
			}
			if (rainWorld.options.jollyDifficulty == Options.JollyDifficulty.HARD)
			{
				JollyCustom.Log("[Jolly" + rainWorld.options.jollyDifficulty?.ToString() + "] Triggering GameOver! Some players are dead! " + AlivePlayers.Count);
				return true;
			}
			JollyCustom.Log($"[Jolly] Avoiding gameOver! Some players are still are alive {NonPermaDeadPlayers.Count}/{rainWorld.options.JollyPlayerCount}");
			return false;
		}
		JollyCustom.Log("[Jolly] Triggering gameOver! This is the last player alive!");
		return true;
	}

	public void Win(bool malnourished)
	{
		manager.artificerDreamNumber = -1;
		if (manager.upcomingProcess != null)
		{
			return;
		}
		Custom.Log("MALNOURISHED:", malnourished.ToString());
		if (!malnourished && !rainWorld.saveBackedUp)
		{
			rainWorld.saveBackedUp = true;
			rainWorld.progression.BackUpSave("_Backup");
		}
		DreamsState dreamsState = GetStorySession.saveState.dreamsState;
		if (manager.rainWorld.progression.miscProgressionData.starvationTutorialCounter > -1)
		{
			manager.rainWorld.progression.miscProgressionData.starvationTutorialCounter++;
		}
		if (!ModManager.MSC || GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.White || GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Yellow || GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Red)
		{
			if (GetStorySession.saveState.miscWorldSaveData.EverMetMoon)
			{
				if (!GetStorySession.lastEverMetMoon)
				{
					manager.CueAchievement((GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 5) ? RainWorld.AchievementID.MoonEncounterGood : RainWorld.AchievementID.MoonEncounterBad, 5f);
					dreamsState?.InitiateEventDream((GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 5) ? DreamsState.DreamID.MoonFriend : DreamsState.DreamID.MoonThief);
				}
				else if (dreamsState != null && !dreamsState.everAteMoonNeuron && GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft < 5)
				{
					dreamsState.InitiateEventDream(DreamsState.DreamID.MoonThief);
				}
			}
			if (!GetStorySession.lastEverMetPebbles && GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
			{
				manager.CueAchievement(RainWorld.AchievementID.PebblesEncounter, 5f);
				if (StoryCharacter == SlugcatStats.Name.Red)
				{
					manager.rainWorld.progression.miscProgressionData.redHasVisitedPebbles = true;
				}
				dreamsState?.InitiateEventDream(DreamsState.DreamID.Pebbles);
			}
		}
		if (dreamsState != null)
		{
			bool malnourished2 = GetStorySession.saveState.malnourished;
			GetStorySession.saveState.malnourished = malnourished;
			AbstractCreature firstAlivePlayer = FirstAlivePlayer;
			if (firstAlivePlayer != null)
			{
				dreamsState.EndOfCycleProgress(GetStorySession.saveState, world.region.name, world.GetAbstractRoom(firstAlivePlayer.pos).name);
			}
			else
			{
				AbstractCreature firstAnyPlayer = FirstAnyPlayer;
				if (firstAnyPlayer != null)
				{
					dreamsState.EndOfCycleProgress(GetStorySession.saveState, world.region.name, world.GetAbstractRoom(firstAnyPlayer.pos).name);
				}
			}
			GetStorySession.saveState.malnourished = malnourished2;
		}
		GetStorySession.saveState.SessionEnded(this, survived: true, malnourished);
		if (ModManager.MSC && GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && dreamsState != null && dreamsState.AnyDreamComingUp && dreamsState.UpcomingDreamID != MoreSlugcatsEnums.DreamID.SaintKarma)
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
		}
		else
		{
			manager.RequestMainProcessSwitch((dreamsState != null && dreamsState.AnyDreamComingUp && !malnourished) ? ProcessManager.ProcessID.Dream : ProcessManager.ProcessID.SleepScreen);
		}
	}

	public void ExitToMenu()
	{
		if (!(manager.upcomingProcess != null))
		{
			if (manager.musicPlayer != null)
			{
				manager.musicPlayer.DeathEvent();
			}
			ExitGame(asDeath: true, asQuit: true);
			manager.RequestMainProcessSwitch((!IsArenaSession && (!ModManager.MSC || !manager.rainWorld.safariMode)) ? ProcessManager.ProcessID.MainMenu : ProcessManager.ProcessID.MultiplayerMenu);
		}
	}

	public void ExitToVoidSeaSlideShow()
	{
		if (!(manager.upcomingProcess != null))
		{
			BeatGameMode(this, standardVoidSea: true);
			if (!ModManager.MMF)
			{
				ExitGame(asDeath: false, asQuit: false);
			}
			if (StoryCharacter == SlugcatStats.Name.Yellow)
			{
				manager.nextSlideshow = SlideShow.SlideShowID.YellowOutro;
			}
			else if (StoryCharacter == SlugcatStats.Name.Red)
			{
				manager.nextSlideshow = SlideShow.SlideShowID.RedOutro;
			}
			else if (ModManager.MSC && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.InvOutro;
			}
			else if (ModManager.MSC && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.RivuletOutro;
			}
			else if (ModManager.MSC && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.ArtificerOutro;
			}
			else if (ModManager.MSC && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.SpearmasterOutro;
			}
			else if (ModManager.MSC && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.GourmandOutro;
			}
			else
			{
				manager.nextSlideshow = SlideShow.SlideShowID.WhiteOutro;
			}
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
		}
	}

	public void RestartGame()
	{
		if (!(manager.upcomingProcess != null))
		{
			manager.rainWorld.setup = RainWorld.LoadSetupValues(manager.rainWorld.buildType == RainWorld.BuildType.Distribution && !ModManager.DevTools);
			ExitGame(asDeath: true, asQuit: true);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		}
	}

	private void ExitGame(bool asDeath, bool asQuit)
	{
		Custom.Log("Exit game");
		if (IsStorySession && (!ModManager.MSC || (!rainWorld.safariMode && !wasAnArtificerDream)))
		{
			if (asQuit)
			{
				GetStorySession.AppendTimeOnCycleEnd(deathOrGhost: true);
			}
			manager.rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(asDeath && clock > 1200, asQuit && clock > 200);
		}
		if (ModManager.MSC && wasAnArtificerDream)
		{
			manager.artificerDreamNumber = -1;
		}
	}

	public void ContinuePaused()
	{
		if (pauseMenu != null)
		{
			pauseMenu.ShutDownProcess();
		}
		pauseMenu = null;
	}

	public void GoToRedsGameOver()
	{
		if (manager.upcomingProcess != null)
		{
			return;
		}
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(20f);
		}
		if (Players[0].realizedCreature != null && (Players[0].realizedCreature as Player).redsIllness != null)
		{
			(Players[0].realizedCreature as Player).redsIllness.fadeOutSlow = true;
		}
		if (GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Red)
		{
			GetStorySession.saveState.deathPersistentSaveData.redsDeath = true;
			if (ModManager.CoopAvailable)
			{
				int num = 0;
				foreach (Player item in Players.Select((AbstractCreature x) => x.realizedCreature as Player))
				{
					GetStorySession.saveState.AppendCycleToStatistics(item, GetStorySession, death: true, num);
					num++;
				}
			}
			else
			{
				GetStorySession.saveState.AppendCycleToStatistics(Players[0].realizedCreature as Player, GetStorySession, death: true, 0);
			}
		}
		manager.rainWorld.progression.SaveWorldStateAndProgression(malnourished: false);
		if (ModManager.MSC)
		{
			if (GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				manager.RequestMainProcessSwitch(MoreSlugcatsEnums.ProcessID.KarmaToMinScreen, 10f);
				return;
			}
			if (GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.White)
			{
				manager.statsAfterCredits = true;
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.SurvivorAltEnd;
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
				return;
			}
			if (GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Yellow)
			{
				manager.statsAfterCredits = true;
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.MonkAltEnd;
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
				return;
			}
			if (GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				manager.statsAfterCredits = true;
				manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.GourmandAltEnd;
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
				return;
			}
		}
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics, 10f);
	}

	public void GoToDeathScreen()
	{
		if (ModManager.Expedition && rainWorld.ExpeditionMode)
		{
			if ((ExpeditionGame.activeUnlocks.Contains("bur-doomed") || GetStorySession.saveState.deathPersistentSaveData.karma == 0) && !GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma)
			{
				if (ExpeditionGame.runData == null)
				{
					GetStorySession.saveState.SessionEnded(this, survived: false, newMalnourished: false);
					ExpeditionGame.runData = SlugcatSelectMenu.MineForSaveData(manager, ExpeditionData.slugcatPlayer);
				}
				GetStorySession.saveState.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
				manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionGameOver);
				return;
			}
			if (!GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma)
			{
				ExpeditionGame.tempKarma--;
			}
			else
			{
				ExpeditionGame.tempReinforce = false;
			}
		}
		if (!(manager.upcomingProcess != null))
		{
			if (IsStorySession && GetStorySession.RedIsOutOfCycles && !rainWorld.ExpeditionMode)
			{
				GoToRedsGameOver();
				return;
			}
			GetStorySession.saveState.SessionEnded(this, survived: false, newMalnourished: false);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.DeathScreen);
		}
	}

	public void GoToStarveScreen()
	{
		if (manager.upcomingProcess != null)
		{
			return;
		}
		Custom.Log("GO TO STARVE");
		if (IsStorySession && GetStorySession.RedIsOutOfCycles)
		{
			GoToRedsGameOver();
			return;
		}
		GetStorySession.PlaceKarmaFlowerOnDeathSpot();
		GetStorySession.saveState.SessionEnded(this, survived: false, newMalnourished: false);
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.DeathEvent();
		}
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.StarveScreen);
	}

	public void GhostShutDown(GhostWorldPresence.GhostID ghostID)
	{
		if (!(manager.upcomingProcess != null))
		{
			sawAGhost = ghostID;
			GetStorySession.AppendTimeOnCycleEnd(deathOrGhost: true);
			if (ModManager.Expedition && manager.rainWorld.ExpeditionMode)
			{
				global::Expedition.Expedition.coreFile.Save(runEnded: false);
			}
			if (ModManager.MSC && GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				manager.RequestMainProcessSwitch(MoreSlugcatsEnums.ProcessID.VengeanceGhostScreen);
			}
			else if (GetStorySession.saveState.deathPersistentSaveData.karmaCap < 9)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.GhostScreen);
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.KarmaToMaxScreen);
			}
		}
	}

	public void CustomEndGameSaveAndRestart(bool addFiveCycles)
	{
		if (!(manager.upcomingProcess != null))
		{
			GetStorySession.saveState.ApplyCustomEndGame(this, addFiveCycles);
			manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		}
	}

	public override void RawUpdate(float dt)
	{
		framesPerSecond = 40;
		float num = 0f;
		bool flag = false;
		if (!GamePaused)
		{
			for (int i = 0; i < session.Players.Count; i++)
			{
				if (session.Players[i].realizedCreature != null && (session.Players[i].realizedCreature as Player).Adrenaline > num)
				{
					num = (session.Players[i].realizedCreature as Player).Adrenaline;
					if (ModManager.MSC && (session.Players[i].realizedCreature as Player).chatlog)
					{
						flag = true;
					}
				}
			}
		}
		float num2 = (flag ? Mathf.Lerp(framesPerSecond, 8f, num) : Mathf.Lerp(framesPerSecond, 15f, num));
		for (int j = 0; j < session.Players.Count; j++)
		{
			if (session.Players[j].realizedCreature != null && session.Players[j].realizedCreature.room != null && session.Players[j].realizedCreature.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) > 0f)
			{
				num2 = Math.Min(num2, Mathf.Lerp(num2, 15f, session.Players[j].realizedCreature.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) * Mathf.InverseLerp(-7000f, -2000f, session.Players[j].realizedCreature.mainBodyChunk.pos.y)));
			}
			if (session.Players[j].realizedCreature != null && (session.Players[j].realizedCreature as Player).redsIllness != null)
			{
				num2 *= (session.Players[j].realizedCreature as Player).redsIllness.TimeFactor;
			}
		}
		if (ModManager.MMF)
		{
			num2 *= 1f / MMF.cfgSlowTimeFactor.Value;
		}
		num2 = Math.Min(num2, (float)framesPerSecond - cameras[0].ghostMode * 10f);
		framesPerSecond = (GamePaused ? framesPerSecond : Math.Max(1, (int)num2));
		if (devToolsActive)
		{
			if (devToolsActive && Input.GetKey("a"))
			{
				framesPerSecond = 10;
			}
			else if (Input.GetKey("s") && devToolsActive)
			{
				framesPerSecond = 400;
			}
			if (debugGraphDrawer != null)
			{
				debugGraphDrawer.Update();
			}
			if (Input.GetKey("q"))
			{
				for (int num3 = world.activeRooms.Count - 1; num3 >= 0; num3--)
				{
					if (world.activeRooms[num3] != cameras[0].room)
					{
						if (roomRealizer != null)
						{
							roomRealizer.KillRoom(world.activeRooms[num3].abstractRoom);
						}
						else
						{
							world.activeRooms[num3].abstractRoom.Abstractize();
						}
					}
				}
			}
			if (Input.GetKey("e"))
			{
				IntVector2 tilePosition = world.activeRooms[0].GetTilePosition((Vector2)Futile.mousePosition + cameras[0].pos);
				for (int num4 = world.NumberOfRooms - 1; num4 >= 0; num4--)
				{
					foreach (AbstractCreature creature in world.GetAbstractRoom(num4 + world.firstRoomIndex).creatures)
					{
						if (creature.abstractAI != null)
						{
							creature.abstractAI.SetDestination(new WorldCoordinate(cameras[0].room.abstractRoom.index, tilePosition.x, tilePosition.y, -1));
						}
					}
				}
			}
			if (Input.GetKey("m") && !mDown)
			{
				mapVisible = !mapVisible;
				abstractSpaceVisualizer.Visibility(mapVisible);
				if (Input.GetMouseButton(0))
				{
					cameras[0].room.AddObject(new TileAccessibilityVisualizer(cameras[0].room));
				}
			}
			mDown = Input.GetKey("m");
			if (Input.GetKey("h") && !hDown)
			{
				if (devUI == null)
				{
					devUI = new DevUI(this);
					Cursor.visible = true;
				}
				else
				{
					Cursor.visible = !rainWorld.options.fullScreen;
					devUI.ClearSprites();
					devUI = null;
				}
			}
			if (devUI != null)
			{
				devUI.Update();
			}
			hDown = Input.GetKey("h");
			if (Input.GetKey("p") && !pDown)
			{
				if (AV != null)
				{
					AV.Destroy();
					AV = null;
				}
				else
				{
					AV = new AccessibilityVisualizer(this);
				}
			}
			pDown = Input.GetKey("p");
			if (Input.GetKey("k") && !kDown)
			{
				consoleVisible = !consoleVisible;
				console.Visibility(consoleVisible);
			}
			kDown = Input.GetKey("k");
			if (ModManager.MSC && Input.GetKey("l") && world.rainCycle.preTimer > world.rainCycle.maxPreTimer / 4)
			{
				world.rainCycle.preTimer = world.rainCycle.maxPreTimer / 4;
			}
		}
		if (Input.GetKey("o") && !oDown)
		{
			devToolsActive = !devToolsActive && (setupValues.devToolsActive || ModManager.DevTools);
			if (ModManager.MSC && IsArenaSession && GetArenaGameSession.chMeta != null && GetArenaGameSession.chMeta.challengeNumber <= MultiplayerUnlocks.TOTAL_CHALLENGES)
			{
				devToolsActive = false;
			}
			devToolsLabel.isVisible = devToolsActive;
		}
		oDown = Input.GetKey("o");
		if (!GamePaused && IsStorySession && (!ModManager.MSC || (!rainWorld.safariMode && manager.artificerDreamNumber == -1)) && !RainWorld.lockGameTimer)
		{
			(session as StoryGameSession).TimeTick(dt);
			SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(GetStorySession.saveStateNumber);
			if (campaignTimeTracker != null && cameras[0].hud != null)
			{
				double dt2 = 1.0 / (double)framesPerSecond;
				campaignTimeTracker.UndeterminedFixedTime += SpeedRunTimer.GetTimerTickIncrement(this, dt2);
			}
		}
		base.RawUpdate(dt);
	}

	public override void Update()
	{
		QuickConnectivity.ResetFrameIterationQuota();
		base.Update();
		if (setupValues.logSpawned && world.logCreatures && !world.singleRoomWorld)
		{
			world.LogCreatures();
		}
		if (IsArenaSession)
		{
			GetArenaGameSession.Update();
		}
		if (pauseMenu != null)
		{
			pauseMenu.Update();
		}
		if (GamePaused)
		{
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i].hud != null)
				{
					cameras[i].hud.Update();
				}
			}
			for (int num = world.activeRooms.Count - 1; num >= 0; num--)
			{
				world.activeRooms[num].PausedUpdate();
			}
		}
		else
		{
			if (!processActive)
			{
				return;
			}
			for (int j = 0; j < cameras.Length; j++)
			{
				cameras[j].Update();
			}
			clock++;
			if (cameras[0].room != null)
			{
				devToolsLabel.text = cameras[0].room.abstractRoom.name + " : Dev tools active";
			}
			evenUpdate = !evenUpdate;
			if (!pauseUpdate)
			{
				globalRain.Update();
			}
			bool flag = RWInput.CheckPauseButton(0, inMenu: false);
			if (((flag && !lastPauseButton) || Platform.systemMenuShowing) && (cameras[0].hud == null || IsArenaSession || cameras[0].hud.map == null || cameras[0].hud.map.fade < 0.1f) && (cameras[0].hud == null || IsArenaSession || !cameras[0].hud.textPrompt.gameOverMode) && manager.fadeToBlack == 0f && cameras[0].roomSafeForPause)
			{
				pauseMenu = new PauseMenu(manager, this);
			}
			if (consoleVisible)
			{
				console.Update();
			}
			lastPauseButton = flag;
			if (devToolsActive)
			{
				bool key = Input.GetKey("r");
				if (key && !lastRestartButton)
				{
					RestartGame();
				}
				lastRestartButton = key;
			}
			if (roomRealizer != null)
			{
				roomRealizer.Update();
			}
			if (AV != null)
			{
				AV.Update();
			}
			if (mapVisible)
			{
				abstractSpaceVisualizer.Update();
			}
			if (abstractSpaceVisualizer.room != cameras[0].room && cameras[0].room != null)
			{
				abstractSpaceVisualizer.ChangeRoom(cameras[0].room);
			}
			if (IsStorySession)
			{
				updateAbstractRoom++;
				if (updateAbstractRoom >= world.NumberOfRooms)
				{
					updateAbstractRoom = 0;
				}
				world.GetAbstractRoom(updateAbstractRoom + world.firstRoomIndex).Update(world.NumberOfRooms);
			}
			else
			{
				world.GetAbstractRoom(0).Update(1);
				if (world.rainCycle.timer > 100)
				{
					world.offScreenDen.Update(1);
				}
			}
			for (int k = 0; k < world.worldProcesses.Count; k++)
			{
				world.worldProcesses[k].Update();
			}
			world.rainCycle.Update();
			overWorld.Update();
			pathfinderResourceDivider.Update();
			updateShortCut++;
			if (updateShortCut > 2)
			{
				updateShortCut = 0;
				shortcuts.Update();
			}
			for (int num2 = world.activeRooms.Count - 1; num2 >= 0; num2--)
			{
				world.activeRooms[num2].Update();
				world.activeRooms[num2].PausedUpdate();
			}
			if (world.loadingRooms.Count > 0)
			{
				for (int l = 0; l < 1; l++)
				{
					for (int num3 = world.loadingRooms.Count - 1; num3 >= 0; num3--)
					{
						if (world.loadingRooms[num3].done)
						{
							world.loadingRooms.RemoveAt(num3);
						}
						else
						{
							world.loadingRooms[num3].Update();
						}
					}
				}
			}
			if (manager.menuSetup.FastTravelInitCondition && Players[0].realizedCreature != null)
			{
				CustomEndGameSaveAndRestart(manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.FastTravel);
			}
			if (cameras[0] != null)
			{
				for (int m = 0; m < 4; m++)
				{
					PlayerHandler playerHandler = rainWorld.GetPlayerHandler(m);
					if (playerHandler != null && RealizedPlayerOfPlayerNumber(m) != null)
					{
						playerHandler.ControllerHandler.AttemptScreenShakeRumble(cameras[0].controllerShake);
					}
				}
			}
			AbstractCreature firstAlivePlayer = FirstAlivePlayer;
			if (ModManager.MSC && Players.Count > 0 && firstAlivePlayer != null && IsStorySession && StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !world.GetAbstractRoom(firstAlivePlayer.pos.room).shelter && world.GetAbstractRoom(firstAlivePlayer.pos.room).AttractionForCreature(CreatureTemplate.Type.Scavenger) != AbstractRoom.CreatureRoomAttraction.Forbidden && timeInRegionThisCycle > 4800)
			{
				timeWithoutCorpse++;
				Player player = firstAlivePlayer.realizedCreature as Player;
				for (int n = 0; n < player.grasps.Length; n++)
				{
					if (player.grasps[n] != null && player.grasps[n].grabbedChunk != null && player.grasps[n].grabbedChunk.owner is Scavenger && (player.grasps[n].grabbedChunk.owner as Scavenger).dead)
					{
						timeWithoutCorpse = 0;
						timeSinceScavsSentToPlayer = 0;
					}
				}
				if (timeWithoutCorpse >= 1200)
				{
					if (timeSinceScavsSentToPlayer % 2400 == 0)
					{
						SendScavsToPlayer();
					}
					timeSinceScavsSentToPlayer++;
				}
			}
			timeInRegionThisCycle++;
			if (session != null && session is StoryGameSession && !rainWorld.safariMode && rainWorld.ExpeditionMode)
			{
				if ((ExpeditionGame.egg == null || (ExpeditionGame.egg != null && ExpeditionGame.egg.rwGame != this)) && ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer) > -1 && ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] == 2)
				{
					ExpeditionGame.egg = new Eggspedition(this);
				}
				if (ExpeditionGame.egg != null)
				{
					ExpeditionGame.egg.Update();
				}
				if (ExpeditionData.devMode && ExpeditionData.challengeList != null)
				{
					if (Input.GetKey(KeyCode.Alpha1) && ExpeditionData.challengeList.Count > 0 && ExpeditionData.challengeList[0] != null)
					{
						ExpeditionData.challengeList[0].CompleteChallenge();
					}
					if (Input.GetKey(KeyCode.Alpha2) && ExpeditionData.challengeList.Count > 1 && ExpeditionData.challengeList[1] != null)
					{
						ExpeditionData.challengeList[1].CompleteChallenge();
					}
					if (Input.GetKey(KeyCode.Alpha3) && ExpeditionData.challengeList.Count > 2 && ExpeditionData.challengeList[2] != null)
					{
						ExpeditionData.challengeList[2].CompleteChallenge();
					}
					if (Input.GetKey(KeyCode.Alpha4) && ExpeditionData.challengeList.Count > 3 && ExpeditionData.challengeList[3] != null)
					{
						ExpeditionData.challengeList[3].CompleteChallenge();
					}
					if (Input.GetKey(KeyCode.Alpha5) && ExpeditionData.challengeList.Count > 4 && ExpeditionData.challengeList[4] != null)
					{
						ExpeditionData.challengeList[4].CompleteChallenge();
					}
				}
				for (int num4 = 0; num4 < ExpeditionGame.unlockTrackers.Count; num4++)
				{
					ExpeditionGame.unlockTrackers[num4].Update();
				}
				for (int num5 = 0; num5 < ExpeditionGame.burdenTrackers.Count; num5++)
				{
					ExpeditionGame.burdenTrackers[num5].Update();
				}
				if (global::Expedition.Expedition.coreFile.coreLoaded)
				{
					int num6 = 0;
					for (int num7 = 0; num7 < ExpeditionData.challengeList.Count; num7++)
					{
						Challenge challenge = ExpeditionData.challengeList[num7];
						challenge.game = this;
						challenge.Update();
						if (challenge.completed)
						{
							num6++;
						}
						if (num6 >= ExpeditionData.challengeList.Count && !ExpeditionGame.expeditionComplete)
						{
							ExpeditionGame.expeditionComplete = true;
						}
					}
				}
				if (ExpeditionGame.expeditionComplete)
				{
					if (ExpeditionGame.voidSeaFinish)
					{
						ExpeditionGame.voidSeaFinish = false;
						ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, ended: true);
						global::Expedition.Expedition.coreFile.Save(runEnded: false);
						ExpeditionGame.runData = SlugcatSelectMenu.MineForSaveData(manager, ExpeditionData.slugcatPlayer);
						manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionWinScreen);
					}
					return;
				}
			}
			if (_concurrentHeavyAiDelayedExceptLastRunAis <= 0)
			{
				_lastRunAiIds.Clear();
			}
			_concurrentHeavyAi = 0;
			_concurrentHeavyAiDelayedExceptLastRunAis = 0;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		for (int i = 0; i < grafUpdateMenus.Count; i++)
		{
			grafUpdateMenus[i].GrafUpdate(timeStacker);
		}
		float timeStacker2 = timeStacker;
		if (pauseUpdate)
		{
			timeStacker2 = 1f;
		}
		base.GrafUpdate(timeStacker2);
		if (pauseMenu != null)
		{
			pauseMenu.GrafUpdate(timeStacker2);
		}
		if (GamePaused)
		{
			for (int j = 0; j < cameras.Length; j++)
			{
				if (cameras[j].hud != null)
				{
					cameras[j].hud.Draw(timeStacker2);
				}
			}
			for (int k = 0; k < cameras.Length; k++)
			{
				cameras[k].PausedDrawUpdate(timeStacker2, TimeSpeedFac);
			}
		}
		else if (processActive)
		{
			for (int l = 0; l < cameras.Length; l++)
			{
				cameras[l].DrawUpdate(timeStacker2, TimeSpeedFac);
				cameras[l].PausedDrawUpdate(timeStacker2, TimeSpeedFac);
			}
		}
	}

	public override void ShutDownProcess()
	{
		devToolsLabel.RemoveFromContainer();
		base.ShutDownProcess();
		for (int i = 0; i < cameras.Length; i++)
		{
			cameras[i].ClearAllSprites();
		}
		for (int j = 0; j < cameras.Length; j++)
		{
			cameras[j].virtualMicrophone.AllQuiet();
		}
		if (pauseMenu != null)
		{
			pauseMenu.ShutDownProcess();
		}
		if (arenaOverlay != null)
		{
			arenaOverlay.ShutDownProcess();
			manager.sideProcesses.Remove(arenaOverlay);
			arenaOverlay = null;
		}
		if (IsArenaSession)
		{
			GetArenaGameSession.ProcessShutDown();
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		Custom.Log("NEXT PROCESS COMMUNICATION");
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is StoryGameStatisticsScreen)
		{
			(nextProcess as StoryGameStatisticsScreen).forceWatch = true;
			if ((nextProcess as StoryGameStatisticsScreen).scene != null && (nextProcess as StoryGameStatisticsScreen).scene.sceneID == MenuScene.SceneID.RedsDeathStatisticsBkg)
			{
				((nextProcess as StoryGameStatisticsScreen).scene as InteractiveMenuScene).timer = 0;
			}
		}
		if (!(nextProcess is KarmaLadderScreen) && !(nextProcess is DreamScreen) && (!(StoryCharacter == SlugcatStats.Name.Red) || !(nextProcess is SlideShow)) && (!ModManager.MSC || (!(nextProcess is ScribbleDreamScreen) && !(nextProcess is ScribbleDreamScreenOld) && !(nextProcess is SlideShow) && !(nextProcess is EndCredits))))
		{
			return;
		}
		int karma = GetStorySession.saveState.deathPersistentSaveData.karma;
		Custom.Log("savKarma:", karma.ToString());
		if (sawAGhost != null)
		{
			Custom.Log("Ghost end of process stuff");
			manager.CueAchievement(GhostWorldPresence.PassageAchievementID(sawAGhost), 2f);
			if (GetStorySession.saveState.deathPersistentSaveData.karmaCap == 8)
			{
				manager.CueAchievement(RainWorld.AchievementID.AllGhostsEncountered, 10f);
			}
			GetStorySession.saveState.GhostEncounter(sawAGhost, rainWorld);
		}
		int num = karma;
		if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && !GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma)
		{
			num = Custom.IntClamp(num - 1, 0, GetStorySession.saveState.deathPersistentSaveData.karmaCap);
		}
		Custom.Log("next screen MAP KARMA:", num.ToString());
		if (cameras[0].hud != null)
		{
			cameras[0].hud.map.mapData.UpdateData(world, 1 + GetStorySession.saveState.deathPersistentSaveData.foodReplenishBonus, num, GetStorySession.saveState.deathPersistentSaveData.karmaFlowerPosition, putItemsInShelters: true);
		}
		AbstractCreature abstractCreature = FirstAlivePlayer;
		if (abstractCreature == null)
		{
			abstractCreature = FirstAnyPlayer;
		}
		int num2 = -1;
		Vector2 vector = Vector2.zero;
		if (abstractCreature != null)
		{
			num2 = abstractCreature.pos.room;
			vector = abstractCreature.pos.Tile.ToVector2() * 20f;
			if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && cameras[0].hud != null && cameras[0].hud.textPrompt != null)
			{
				num2 = cameras[0].hud.textPrompt.deathRoom;
				vector = cameras[0].hud.textPrompt.deathPos;
			}
			else if (abstractCreature.realizedCreature != null)
			{
				vector = abstractCreature.realizedCreature.mainBodyChunk.pos;
			}
			if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room != null && num2 == abstractCreature.realizedCreature.room.abstractRoom.index)
			{
				vector = Custom.RestrictInRect(vector, abstractCreature.realizedCreature.room.RoomRect.Grow(50f));
			}
		}
		KarmaLadderScreen.SleepDeathScreenDataPackage sleepDeathScreenDataPackage;
		if (ModManager.MSC && wasAnArtificerDream)
		{
			sleepDeathScreenDataPackage = manager.dataBeforeArtificerDream;
			manager.dataBeforeArtificerDream = null;
		}
		else
		{
			sleepDeathScreenDataPackage = new KarmaLadderScreen.SleepDeathScreenDataPackage((nextProcess.ID == ProcessManager.ProcessID.SleepScreen || nextProcess.ID == ProcessManager.ProcessID.Dream) ? GetStorySession.saveState.food : cameras[0].hud.textPrompt.foodInStomach, new IntVector2(karma, GetStorySession.saveState.deathPersistentSaveData.karmaCap), GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma, num2, vector, cameras[0].hud.map.mapData, GetStorySession.saveState, GetStorySession.characterStats, GetStorySession.playerSessionRecords[0], GetStorySession.saveState.lastMalnourished, GetStorySession.saveState.malnourished);
			if (ModManager.MSC && GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				manager.dataBeforeArtificerDream = sleepDeathScreenDataPackage;
			}
			if (ModManager.CoopAvailable)
			{
				for (int i = 1; i < GetStorySession.playerSessionRecords.Length; i++)
				{
					if (GetStorySession.playerSessionRecords[i].kills != null && GetStorySession.playerSessionRecords[i].kills.Count > 0)
					{
						sleepDeathScreenDataPackage.sessionRecord.kills.AddRange(GetStorySession.playerSessionRecords[i].kills);
					}
				}
			}
		}
		if (nextProcess is KarmaLadderScreen)
		{
			(nextProcess as KarmaLadderScreen).GetDataFromGame(sleepDeathScreenDataPackage);
		}
		else if (nextProcess is DreamScreen)
		{
			(nextProcess as DreamScreen).GetDataFromGame(GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
		}
		else if (StoryCharacter == SlugcatStats.Name.Red && nextProcess is SlideShow)
		{
			(nextProcess as SlideShow).endGameStatsPackage = sleepDeathScreenDataPackage;
			(nextProcess as SlideShow).processAfterSlideShow = ProcessManager.ProcessID.Statistics;
		}
		if (nextProcess is ScribbleDreamScreen)
		{
			(nextProcess as ScribbleDreamScreen).GetDataFromGame(GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
		}
		if (nextProcess is ScribbleDreamScreenOld)
		{
			(nextProcess as ScribbleDreamScreenOld).GetDataFromGame(GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
		}
		if (nextProcess is EndCredits)
		{
			(nextProcess as EndCredits).passthroughPackage = sleepDeathScreenDataPackage;
		}
		if (nextProcess is SlideShow)
		{
			(nextProcess as SlideShow).passthroughPackage = sleepDeathScreenDataPackage;
		}
	}

	public void ShowPauseMenu()
	{
		if (pauseMenu == null && (cameras[0].hud == null || IsArenaSession || cameras[0].hud.map == null || cameras[0].hud.map.fade < 0.1f) && (cameras[0].hud == null || IsArenaSession || !cameras[0].hud.textPrompt.gameOverMode))
		{
			pauseMenu = new PauseMenu(manager, this);
		}
	}

	public AbstractCreature SpawnPlayers(int count, WorldCoordinate location)
	{
		AbstractCreature result = null;
		if (count > session.Players.Count)
		{
			Custom.LogWarning("More players then slots spawned, expand session.Players! limiting to size of list...");
			count = session.Players.Count;
		}
		for (int i = 0; i < count; i++)
		{
			AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 0));
			abstractCreature.state = new PlayerState(abstractCreature, i, GetStorySession.saveState.saveStateNumber, isGhost: false);
			world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
			session.AddPlayer(abstractCreature);
			if (i == 0)
			{
				result = abstractCreature;
			}
		}
		return result;
	}

	public AbstractCreature SpawnPlayers(bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
	{
		AbstractCreature abstractCreature = null;
		if (ModManager.MSC && manager.artificerDreamNumber > -1)
		{
			if (manager.artificerDreamNumber == 1 || manager.artificerDreamNumber == 5 || manager.artificerDreamNumber == 6)
			{
				location = new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
				abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 0));
				abstractCreature.ID.setAltSeed(1002);
				abstractCreature.state = new PlayerState(abstractCreature, 0, MoreSlugcatsEnums.SlugcatStatsName.Artificer, isGhost: false);
			}
			else
			{
				if (manager.artificerDreamNumber == 2)
				{
					location = new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
				}
				abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 0));
				abstractCreature.state = new PlayerState(abstractCreature, 0, MoreSlugcatsEnums.SlugcatStatsName.Slugpup, isGhost: false);
				abstractCreature.ID.setAltSeed((manager.artificerDreamNumber < 4 && manager.artificerDreamNumber != 2) ? 1000 : 1001);
			}
			world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
			session.AddPlayer(abstractCreature);
			return abstractCreature;
		}
		if (player1)
		{
			abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 0));
			abstractCreature.state = new PlayerState(abstractCreature, 0, GetStorySession.saveState.saveStateNumber, isGhost: false);
			if (ModManager.CoopAvailable)
			{
				(abstractCreature.state as PlayerState).isPup = rainWorld.options.jollyPlayerOptionsArray[0].isPup;
			}
			world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
			session.AddPlayer(abstractCreature);
		}
		if (!ModManager.CoopAvailable)
		{
			if (player2)
			{
				AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 1));
				abstractCreature2.state = new PlayerState(abstractCreature2, 1, GetStorySession.saveState.saveStateNumber, isGhost: false);
				world.GetAbstractRoom(abstractCreature2.pos.room).AddEntity(abstractCreature2);
				session.AddPlayer(abstractCreature2);
			}
			if (player3)
			{
				AbstractCreature abstractCreature3 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 1));
				abstractCreature3.state = new PlayerState(abstractCreature3, 2, GetStorySession.saveState.saveStateNumber, isGhost: false);
				world.GetAbstractRoom(abstractCreature3.pos.room).AddEntity(abstractCreature3);
				session.AddPlayer(abstractCreature3);
			}
			if (player4)
			{
				AbstractCreature abstractCreature4 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, 1));
				abstractCreature4.state = new PlayerState(abstractCreature4, 3, GetStorySession.saveState.saveStateNumber, isGhost: false);
				world.GetAbstractRoom(abstractCreature4.pos.room).AddEntity(abstractCreature4);
				session.AddPlayer(abstractCreature4);
			}
		}
		else
		{
			JollySpawnPlayers(location);
			StoryGameSession obj = session as StoryGameSession;
			obj.CreateJollySlugStats((obj?.saveState?.malnourished).GetValueOrDefault());
		}
		return abstractCreature;
	}

	public void SpawnObjs(List<AbstractPhysicalObject> objs)
	{
		foreach (AbstractPhysicalObject obj in objs)
		{
			world.GetAbstractRoom(obj.pos.room).AddEntity(obj);
			obj.RealizeInRoom();
		}
	}

	public void SpawnCritters(List<string> objs, AbstractCreature player)
	{
		Custom.Log("Spawncritters running!");
		foreach (string obj in objs)
		{
			AbstractCreature abstractCreature = SaveState.AbstractCreatureFromString(world, obj, onlyInCurrentRegion: false);
			Custom.Log("Spawncritters! creating obj of:" + obj);
			abstractCreature.state.CycleTick();
			abstractCreature.pos = player.pos;
			world.GetAbstractRoom(player.pos.room).AddEntity(abstractCreature);
			abstractCreature.RealizeInRoom();
			Custom.Log("Creature count is:", world.GetAbstractRoom(player.pos.room).creatures.Count.ToString());
		}
	}

	public void JollySpawnPlayers(WorldCoordinate location)
	{
		JollyCustom.Log("Number of jolly players: " + rainWorld.options.JollyPlayerCount + " accesing directly: " + rainWorld.options.jollyPlayerOptionsArray.Count((JollyPlayerOptions x) => x.joined));
		JollyPlayerOptions[] jollyPlayerOptionsArray = rainWorld.options.jollyPlayerOptionsArray;
		for (int i = 0; i < jollyPlayerOptionsArray.Length; i++)
		{
			JollyCustom.Log(jollyPlayerOptionsArray[i].ToString());
		}
		for (int j = 1; j < rainWorld.options.jollyPlayerOptionsArray.Length; j++)
		{
			if (rainWorld.options.jollyPlayerOptionsArray[j].joined || (bool)rainWorld.setup.GetType().GetField("player" + (j + 1)).GetValue(rainWorld.setup))
			{
				JollyCustom.Log("[JOLLY] Spawning player: " + j);
				AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate("Slugcat"), null, location, new EntityID(-1, j));
				PlayerState state = new PlayerState(abstractCreature, j, new SlugcatStats.Name("JollyPlayer" + (j + 1)), isGhost: false)
				{
					isPup = rainWorld.options.jollyPlayerOptionsArray[j].isPup,
					swallowedItem = null
				};
				abstractCreature.state = state;
				world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
				JollyCustom.Log("Adding player: " + (abstractCreature.state as PlayerState).playerNumber);
				JollyCustom.Log("Player session records: " + (session as StoryGameSession).playerSessionRecords.Length);
				session.AddPlayer(abstractCreature);
			}
		}
	}

	public bool IsMoonHeartActive()
	{
		if (ModManager.MSC && IsStorySession)
		{
			if (!setupValues.testMoonFixed && (!(StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint) || GetStorySession.saveState.deathPersistentSaveData.ripMoon))
			{
				if (IsStorySession)
				{
					return GetStorySession.saveState.miscWorldSaveData.moonHeartRestored;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsMoonActive()
	{
		if (ModManager.MSC && IsStorySession)
		{
			if (!setupValues.testMoonFixed && (!(StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint) || GetStorySession.saveState.deathPersistentSaveData.ripMoon || GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0))
			{
				if (IsStorySession && GetStorySession.saveState.miscWorldSaveData.moonHeartRestored)
				{
					return GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool MoonHasRobe()
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (setupValues.testMoonCloak)
		{
			return true;
		}
		if (!IsStorySession)
		{
			return false;
		}
		if (GetStorySession.saveState.miscWorldSaveData.moonGivenRobe)
		{
			return true;
		}
		SlugcatStats.Name[] array = SlugcatStats.SlugcatTimelineOrder().ToArray();
		int num = -1;
		int num2 = -1;
		SlugcatStats.Name cloakTimelinePosition = rainWorld.progression.miscProgressionData.CloakTimelinePosition;
		for (int i = 0; i < array.Length; i++)
		{
			if (GetStorySession.saveStateNumber == array[i])
			{
				num = i;
			}
			if (cloakTimelinePosition == array[i])
			{
				num2 = i;
			}
		}
		if (cloakTimelinePosition != null)
		{
			return num >= num2;
		}
		return false;
	}

	public static int BlizzardHardEndTimer(bool storyMode)
	{
		if (!storyMode)
		{
			return 500;
		}
		return 32000;
	}

	public static void ForceSaveNewDenLocation(RainWorldGame game, string roomName, bool saveWorldStates)
	{
		Custom.LogImportant("Forced den to room", roomName);
		if (saveWorldStates)
		{
			Custom.LogImportant("Updated world state with den too!");
			game.GetStorySession.saveState.BringUpToDate(game);
		}
		game.GetStorySession.saveState.denPosition = roomName;
		game.GetStorySession.saveState.progression.SaveWorldStateAndProgression(malnourished: false);
	}

	public static void BeatGameMode(RainWorldGame game, bool standardVoidSea)
	{
		if (standardVoidSea)
		{
			Custom.Log($"Beat Game Mode(void sea ending) : {game.GetStorySession.saveState}");
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = true;
			if (ModManager.MSC || game.GetStorySession.saveStateNumber == SlugcatStats.Name.Red)
			{
				if (ModManager.CoopAvailable)
				{
					int num = 0;
					foreach (Player item in game.Players.Select((AbstractCreature x) => x.realizedCreature as Player))
					{
						game.GetStorySession.saveState.AppendCycleToStatistics(item, game.GetStorySession, death: true, num);
						num++;
					}
				}
				else
				{
					game.GetStorySession.saveState.AppendCycleToStatistics(game.Players[0].realizedCreature as Player, game.GetStorySession, death: true, 0);
				}
			}
			if (ModManager.MSC)
			{
				game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			}
			if (game.GetStorySession.saveStateNumber == SlugcatStats.Name.White || game.GetStorySession.saveStateNumber == SlugcatStats.Name.Yellow)
			{
				game.rainWorld.progression.miscProgressionData.redUnlocked = true;
			}
			if (game.GetStorySession.saveStateNumber == SlugcatStats.Name.Red)
			{
				game.rainWorld.progression.miscProgressionData.beaten_Hunter = true;
				game.GetStorySession.saveState.deathPersistentSaveData.redsDeath = true;
			}
			if (ModManager.MSC && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
				game.rainWorld.progression.miscProgressionData.beaten_Artificer = true;
				game.rainWorld.progression.miscProgressionData.artificerEndingID = 1;
			}
			if (ModManager.MSC && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				game.rainWorld.progression.miscProgressionData.beaten_Rivulet = true;
			}
			if (ModManager.MSC && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				game.rainWorld.progression.miscProgressionData.beaten_Gourmand = true;
			}
			if (ModManager.MSC && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				game.rainWorld.progression.miscProgressionData.beaten_SpearMaster = true;
			}
			if (game.GetStorySession.saveStateNumber == SlugcatStats.Name.White)
			{
				game.rainWorld.progression.miscProgressionData.survivorEndingID = 1;
			}
			if (game.GetStorySession.saveStateNumber == SlugcatStats.Name.Yellow)
			{
				game.rainWorld.progression.miscProgressionData.monkEndingID = 1;
			}
			game.GetStorySession.saveState.justBeatGame = true;
			game.GetStorySession.saveState.progression.SaveWorldStateAndProgression(malnourished: false);
			return;
		}
		Custom.Log($"Beat Game Mode(alt ending) : {game.GetStorySession.saveState}");
		string roomName = "";
		if (game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.White)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.rainWorld.progression.miscProgressionData.beaten_Survivor = true;
			game.rainWorld.progression.miscProgressionData.survivorEndingID = 2;
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
			game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			roomName = "OE_SEXTRA";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Yellow)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.rainWorld.progression.miscProgressionData.beaten_Survivor = true;
			game.rainWorld.progression.miscProgressionData.monkEndingID = 2;
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
			game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			roomName = "OE_SEXTRA";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
			game.rainWorld.progression.miscProgressionData.beaten_Rivulet = true;
			if (game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.EnergyCell))
			{
				game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("The Rarefaction Cell item is now available in Sandbox Mode.");
			}
			game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.likesPlayer = 1f;
			roomName = "SL_AI";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.rainWorld.progression.miscProgressionData.beaten_Artificer = true;
			game.rainWorld.progression.miscProgressionData.artificerEndingID = 2;
			roomName = "LC_FINAL";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = true;
			game.rainWorld.progression.miscProgressionData.beaten_Saint = true;
			game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.FireBug);
			game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.FireEgg);
			game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.HellSpear);
			if (game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.LevelUnlockID.HR))
			{
				game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("Rubicon content is now available in Arena Mode.");
			}
			roomName = "SI_SAINTINTRO";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
			game.rainWorld.progression.miscProgressionData.beaten_SpearMaster_AltEnd = true;
			game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			roomName = "SI_A07";
		}
		else if (game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
			game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
			game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
			bool beaten_Gourmand = game.rainWorld.progression.miscProgressionData.beaten_Gourmand;
			bool flag = false;
			bool flag2 = false;
			game.rainWorld.progression.miscProgressionData.beaten_Gourmand = true;
			game.rainWorld.processManager.foodTrackerCompletedThisSession = false;
			if (game.GetStorySession.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, addIfMissing: true) is WinState.GourFeastTracker { GoalFullfilled: var flag3 } gourFeastTracker)
			{
				if (!flag3)
				{
					flag3 = true;
					for (int i = 0; i < gourFeastTracker.currentCycleProgress.Length; i++)
					{
						if (gourFeastTracker.currentCycleProgress[i] <= 0)
						{
							flag3 = false;
							break;
						}
					}
				}
				if (flag3)
				{
					if (!game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full)
					{
						flag = true;
					}
					game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full = true;
					if (game.GetStorySession.saveState.forcePupsNextCycle == 0)
					{
						game.GetStorySession.saveState.forcePupsNextCycle = 1;
					}
					game.rainWorld.processManager.foodTrackerCompletedThisSession = true;
					flag2 = game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC);
				}
			}
			if (!beaten_Gourmand)
			{
				game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("The Outer Expanse gate remains unlocked beyond this point in the timeline.");
			}
			string text = "";
			if (flag2 && flag)
			{
				text = "Sandbox Mode, and some campaigns.";
			}
			else if (flag2)
			{
				text = "Sandbox Mode.";
			}
			else if (flag)
			{
				text = "some campaigns.";
			}
			if (text != "")
			{
				if (game.manager.specialUnlockText != "")
				{
					game.manager.specialUnlockText += "\n";
				}
				game.manager.specialUnlockText += game.rainWorld.inGameTranslator.Translate("Pups can now be found in " + text);
			}
			roomName = "OE_SEXTRA";
		}
		game.GetStorySession.saveState.justBeatGame = true;
		AbstractCreature abstractCreature = game.FirstAlivePlayer;
		if (abstractCreature == null)
		{
			abstractCreature = game.FirstAnyPlayer;
		}
		SaveState.forcedEndRoomToAllowwSave = abstractCreature.Room.name;
		game.GetStorySession.saveState.BringUpToDate(game);
		SaveState.forcedEndRoomToAllowwSave = "";
		game.GetStorySession.saveState.AppendCycleToStatistics(abstractCreature.realizedCreature as Player, game.GetStorySession, death: false, 0);
		ForceSaveNewDenLocation(game, roomName, saveWorldStates: false);
	}

	public void ArtificerDreamEnd()
	{
		if (GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
		{
			Custom.Log("Artificer dream ending...");
			manager.artificerDreamNumber = -1;
			manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
			List<AbstractCreature> collection = new List<AbstractCreature>(session.Players);
			session = new StoryGameSession(MoreSlugcatsEnums.SlugcatStatsName.Artificer, this);
			session.Players = new List<AbstractCreature>(collection);
			if (manager.musicPlayer != null)
			{
				manager.musicPlayer.FadeOutAllSongs(20f);
			}
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen, 10f);
		}
	}

	public void SendScavsToPlayer()
	{
		if (Players.Count == 0 || IsArenaSession || rainWorld.safariMode)
		{
			return;
		}
		List<AbstractCreature> list = new List<AbstractCreature>();
		for (int num = world.NumberOfRooms - 1; num >= 0; num--)
		{
			List<AbstractCreature> creatures = world.GetAbstractRoom(num + world.firstRoomIndex).creatures;
			for (int i = 0; i < creatures.Count; i++)
			{
				AbstractCreature abstractCreature = creatures[i];
				if ((abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger || abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) && abstractCreature.abstractAI != null)
				{
					list.Add(abstractCreature);
				}
			}
		}
		list.Shuffle();
		AbstractCreature firstAlivePlayer = FirstAlivePlayer;
		if (firstAlivePlayer != null)
		{
			for (int j = 0; j < Math.Min(list.Count, 3); j++)
			{
				list[j].abstractAI.SetDestination(firstAlivePlayer.pos);
			}
		}
	}

	public static bool RequestHeavyAi(Creature creature)
	{
		EntityID iD = creature.abstractCreature.ID;
		if (_lastRunAiIds.Contains(iD))
		{
			return false;
		}
		if (_concurrentHeavyAi < 3)
		{
			_lastRunAiIds.Add(iD);
			_concurrentHeavyAi++;
			return true;
		}
		_concurrentHeavyAiDelayedExceptLastRunAis++;
		return false;
	}
}
