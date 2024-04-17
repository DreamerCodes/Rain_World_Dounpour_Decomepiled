using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CoralBrain;
using Expedition;
using MoreSlugcats;
using Noise;
using RWCustom;
using ScavTradeInstruction;
using Unity.Mathematics;
using UnityEngine;
using VoidSea;

public class Room
{
	public class SlopeDirection : ExtEnum<SlopeDirection>
	{
		public static readonly SlopeDirection UpLeft = new SlopeDirection("UpLeft", register: true);

		public static readonly SlopeDirection UpRight = new SlopeDirection("UpRight", register: true);

		public static readonly SlopeDirection DownLeft = new SlopeDirection("DownLeft", register: true);

		public static readonly SlopeDirection DownRight = new SlopeDirection("DownRight", register: true);

		public static readonly SlopeDirection Broken = new SlopeDirection("Broken", register: true);

		public SlopeDirection(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Tile
	{
		public enum TerrainType
		{
			Air,
			Solid,
			Slope,
			Floor,
			ShortcutEntrance
		}

		public TerrainType Terrain;

		public bool verticalBeam;

		public bool horizontalBeam;

		public bool wallbehind;

		public int shortCut;

		public bool wormGrass;

		public int waterInt;

		public int X;

		public int Y;

		public bool hive;

		public bool AnyBeam
		{
			get
			{
				if (!verticalBeam)
				{
					return horizontalBeam;
				}
				return true;
			}
		}

		public bool DeepWater => waterInt == 1;

		public bool WaterSurface => waterInt == 2;

		public bool AnyWater => waterInt != 0;

		public bool Solid => IsSolid();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSolid()
		{
			return Terrain == TerrainType.Solid;
		}

		public Tile(int x, int y, TerrainType tType, bool vBeam, bool hBeam, bool wbhnd, int sc, int wtr)
		{
			X = x;
			Y = y;
			verticalBeam = vBeam;
			horizontalBeam = hBeam;
			Terrain = tType;
			shortCut = sc;
			wallbehind = wbhnd;
			waterInt = wtr;
		}
	}

	public class WaterFluxController
	{
		public float roomShake;

		private Room owner;

		private GlobalRain globalRain;

		private float internalFluxWaterState;

		private float overShooter;

		public float fluxWaterLevel
		{
			get
			{
				float num = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel);
				float num2 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMinLevel);
				if (num2 > num)
				{
					float num3 = num2;
					num2 = num;
					num = num3;
				}
				num2 += Mathf.Min(0f, overShooter);
				num += Mathf.Max(0f, overShooter);
				float num4 = num2 * (float)owner.TileHeight * 22f;
				float num5 = num * (float)owner.TileHeight * 22f;
				return Mathf.Lerp(num4 - 50f, num5 - 50f, waterFluxState());
			}
		}

		public WaterFluxController(Room room, GlobalRain globalRain)
		{
			owner = room;
			this.globalRain = globalRain;
			internalFluxWaterState = float.MinValue;
		}

		private float waterFluxState()
		{
			owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxFrequency);
			float effectAmount = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxDelay);
			float effectAmount2 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMinDelay);
			float t = Mathf.Lerp(effectAmount2, effectAmount, 0.5f);
			float num = 100f + Mathf.Lerp(280f, 490f, t);
			float num2 = Mathf.Max(0f, effectAmount * 1200f);
			float num3 = Mathf.Max(0f, effectAmount2 * 1200f);
			float num4 = num + num + num2 + num3;
			float num5 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxOffset);
			if (num5 != 0f)
			{
				num5 = ((!(num5 < 0.5f)) ? (5000f * Mathf.InverseLerp(0.5f, 1f, num5)) : (-5000f * Mathf.InverseLerp(0.5f, 0f, num5)));
			}
			float num6 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxSpeed);
			if (num6 <= 0f)
			{
				num6 = 1f;
			}
			float num7 = ((float)globalRain.waterFluxTicker + num5) * num6 % num4;
			float b;
			if (num7 < num)
			{
				float t2 = Mathf.InverseLerp(0f, num, num7);
				b = Mathf.Lerp(0f, 1f, t2);
				overShooter += num / 100000000f;
			}
			else if (num7 < num + num2)
			{
				b = 1f;
				overShooter *= 0.995f;
			}
			else if (num7 < num + num2 + num)
			{
				float t3 = Mathf.InverseLerp(num + num2, num + num2 + num, num7);
				b = Mathf.Lerp(1f, 0f, t3);
				overShooter -= num / 100000000f;
			}
			else
			{
				b = 0f;
				overShooter *= 0.995f;
			}
			if (internalFluxWaterState == float.MinValue)
			{
				internalFluxWaterState = b;
				roomShake = 0f;
			}
			else
			{
				float num8 = internalFluxWaterState;
				internalFluxWaterState = Mathf.Lerp(internalFluxWaterState, b, 0.06f);
				roomShake = Mathf.Abs(internalFluxWaterState - num8) * 100f * owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxRumble);
			}
			return internalFluxWaterState;
		}
	}

	private static readonly AGLog<Room> Log = new AGLog<Room>();

	public List<IAccessibilityModifier> accessModifiers;

	private int Width;

	private int Height;

	public float[,] shortcutsBlinking;

	public IntVector2[][] hives;

	public IntVector2[] garbageHoles;

	public IntVector2[] spawnInRoomItems;

	public WaterFall[] waterFalls;

	public RoomBorderExit[] borderExits;

	public List<LightSource> lightSources;

	public List<VisionObscurer> visionObscurers;

	public bool gameOverRoom;

	public DeathFallGraphic deathFallGraphic;

	public Lightning lightning;

	private float lastBackgroundNoise;

	private float backgroundNoise;

	public List<ChunkGlue> chunkGlue;

	public RoomSettings roomSettings;

	private int updateIndex = int.MaxValue;

	private Tile[,] Tiles;

	public Tile DefaultTile;

	public List<PhysicalObject>[] physicalObjects;

	public RainWorldGame game;

	public World world;

	public AImap aimap;

	public SocialEventRecognizer socialEventRecognizer;

	public QuickPathFinder quickPather;

	public IntVector2[] exitAndDenIndex;

	public IntVector2[] shortcutsIndex;

	public ShortcutData[] shortcuts;

	public List<IntVector2> lockedShortcuts;

	public Vector2[] cameraPositions;

	public int camerasChangedTick;

	public Vector2 lightAngle;

	public int loadingProgress;

	public bool readyForNonAICreaturesToEnter;

	public int waitToEnterAfterFullyLoaded = -1;

	public ClimbableVinesSystem climbableVines;

	public InsectCoordinator insectCoordinator;

	public FliesRoomAI fliesRoomAi;

	public IntVector2[] ceilingTiles;

	public float gravity = 1f;

	public DebugPerTileVisualizer PERTILEVISALIZER;

	public float waterGlitterCycle;

	public float lastWaterGlitterCycle;

	public Water waterObject;

	public bool water;

	public int defaultWaterLevel;

	private float floatWaterLevel;

	public bool waterInFrontOfTerrain;

	private AIdataPreprocessor aidataprepro;

	public RegionGate regionGate;

	public ShelterDoor shelterDoor;

	public RoomRain roomRain;

	public long timeAdder;

	public int timeDivider;

	public int cntr;

	public List<LightningMachine> lightningMachines;

	public List<EnergySwirl> energySwirls;

	public List<SnowSource> snowSources;

	public List<LocalBlizzard> localBlizzards;

	public List<Vector2> deathFallFocalPoints;

	public List<CellDistortion> cellDistortions;

	public Snow snowObject;

	public bool snow;

	public BlizzardGraphics blizzardGraphics;

	public bool blizzard;

	public WaterFluxController waterFlux;

	public List<IProvideWarmth> blizzardHeatSources;

	public List<LightSource> cosmeticLightSources;

	public List<ZapCoil> zapCoils;

	public List<OEsphere> oeSpheres;

	public float darkenLightsFactor;

	public int syncTicker;

	public bool dustStorm;

	public bool waterInverted;

	public bool deferred;

	public int SwarmerCount;

	private Vector2 cloudsNdarken;

	public Oracle.OracleID oracleWantToSpawn;

	private List<int> list { get; set; }

	public AbstractRoom abstractRoom { get; private set; }

	public int TileWidth => Width;

	public int TileHeight => Height;

	public float PixelWidth => (float)Width * 20f;

	public float PixelHeight => (float)Height * 20f;

	public List<Player> PlayersInRoom => (from x in game.NonPermaDeadPlayers
		where x.Room == abstractRoom
		select x.realizedCreature as Player).ToList();

	public bool BeingViewed
	{
		get
		{
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float ElectricPower
	{
		get
		{
			if (roomSettings == null || world == null || world.rainCycle == null)
			{
				return 1f;
			}
			if (roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f && (!ModManager.MMF || world.rainCycle.brokenAntiGrav != null))
			{
				return world.rainCycle.brokenAntiGrav.CurrentLightsOn;
			}
			if (ModManager.MSC && world.rainCycle.filtrationPowerBehavior != null)
			{
				return world.rainCycle.filtrationPowerBehavior.ElectricPower;
			}
			return 1f;
		}
	}

	public float BackgroundNoise => lastBackgroundNoise;

	public List<UpdatableAndDeletable> updateList { get; set; }

	public List<IDrawable> drawableObjects { get; set; }

	public bool shortCutsReady => loadingProgress >= 1;

	public bool readyForAI => loadingProgress >= 2;

	public bool quantifiedCreaturesPlaced => loadingProgress >= 3;

	public bool fullyLoaded => loadingProgress >= 3;

	public bool ReadyForPlayer
	{
		get
		{
			if (readyForNonAICreaturesToEnter)
			{
				return waitToEnterAfterFullyLoaded < 1;
			}
			return false;
		}
	}

	public FloatRect RoomRect => new FloatRect(0f, 0f, PixelWidth, PixelHeight);

	public float DustStormIntensity
	{
		get
		{
			if (!ModManager.MSC || !dustStorm)
			{
				return 0f;
			}
			return world.rainCycle.DustStormProgress * roomSettings.RainIntensity;
		}
	}

	public void SetAbstractRoom(AbstractRoom newAbstractRoom)
	{
		abstractRoom = newAbstractRoom;
		abstractRoom.realizedRoom = this;
	}

	public IntVector2 RandomTile()
	{
		return new IntVector2(UnityEngine.Random.Range(0, TileWidth), UnityEngine.Random.Range(0, TileHeight));
	}

	public Vector2 RandomPos()
	{
		return new Vector2(UnityEngine.Random.value * PixelWidth, UnityEngine.Random.value * PixelHeight);
	}

	public void MakeBackgroundNoise(float noise)
	{
		backgroundNoise = Mathf.Clamp(noise, backgroundNoise, 1f);
	}

	public float FloatWaterLevel(float horizontalPos)
	{
		if (waterObject != null)
		{
			return waterObject.DetailedWaterLevel(horizontalPos);
		}
		return floatWaterLevel;
	}

	public bool PointSubmerged(Vector2 pos)
	{
		if (ModManager.MSC && waterInverted)
		{
			if (waterObject != null)
			{
				return pos.y > waterObject.DetailedWaterLevel(pos.x);
			}
			return pos.y > floatWaterLevel;
		}
		if (waterObject != null)
		{
			return pos.y < waterObject.DetailedWaterLevel(pos.x);
		}
		return pos.y < floatWaterLevel;
	}

	public Room(RainWorldGame game, World world, AbstractRoom abstractRoom)
	{
		this.game = game;
		this.world = world;
		this.abstractRoom = abstractRoom;
		if (ModManager.MSC && game != null && game.session != null && game.IsArenaSession && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && File.Exists(AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + "Challenges" + Path.DirectorySeparatorChar + "Challenge" + game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID + "_Settings.txt")))
		{
			roomSettings = new RoomSettings("Challenge" + game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID, world.region, template: false, firstTemplate: false, game?.StoryCharacter);
		}
		else if (ModManager.MSC && game != null && game.IsStorySession && game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken && world.region != null && world.region.name == "RM")
		{
			roomSettings = new RoomSettings(abstractRoom.name + "-2", world.region, template: false, firstTemplate: false, game?.StoryCharacter);
		}
		else
		{
			roomSettings = new RoomSettings(abstractRoom.name, world.region, template: false, firstTemplate: false, game?.StoryCharacter);
		}
		if (game != null)
		{
			SlugcatGamemodeUniqueRoomSettings(game);
			if (roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel) > 0f)
			{
				waterFlux = new WaterFluxController(this, game.globalRain);
			}
			if (ModManager.MSC && roomSettings.GetEffect(MoreSlugcatsEnums.RoomEffectType.InvertedWater) != null)
			{
				waterInverted = true;
			}
		}
		if ((world.region != null && world.region.name == "HR") || roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LavaSurface) != null)
		{
			Shader.EnableKeyword("HR");
		}
		else
		{
			Shader.DisableKeyword("HR");
		}
		physicalObjects = new List<PhysicalObject>[3];
		for (int i = 0; i < physicalObjects.Length; i++)
		{
			physicalObjects[i] = new List<PhysicalObject>();
		}
		drawableObjects = new List<IDrawable>();
		accessModifiers = new List<IAccessibilityModifier>();
		updateList = new List<UpdatableAndDeletable>();
		lightSources = new List<LightSource>();
		waterFalls = new WaterFall[0];
		visionObscurers = new List<VisionObscurer>();
		socialEventRecognizer = new SocialEventRecognizer(this);
		cellDistortions = new List<CellDistortion>();
		cosmeticLightSources = new List<LightSource>();
		zapCoils = new List<ZapCoil>();
		lightningMachines = new List<LightningMachine>();
		energySwirls = new List<EnergySwirl>();
		snowSources = new List<SnowSource>();
		localBlizzards = new List<LocalBlizzard>();
		oeSpheres = new List<OEsphere>();
		deathFallFocalPoints = new List<Vector2>();
		blizzard = false;
		dustStorm = false;
		blizzardHeatSources = new List<IProvideWarmth>();
		lockedShortcuts = new List<IntVector2>();
	}

	public void AddWater()
	{
		if (waterObject == null)
		{
			if (defaultWaterLevel < 0)
			{
				defaultWaterLevel = -10;
				floatWaterLevel = -200f;
			}
			if (ModManager.MSC && waterInverted && defaultWaterLevel > 0)
			{
				waterInverted = false;
			}
			waterObject = new Water(this, defaultWaterLevel);
			accessModifiers.Add(waterObject);
			drawableObjects.Add(waterObject);
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					game.cameras[i].NewObjectInRoom(waterObject);
				}
			}
		}
		for (int j = 0; j < waterFalls.Length; j++)
		{
			waterFalls[j].ConnectToWaterObject(waterObject);
		}
		water = true;
	}

	public void Loaded()
	{
		if (game == null)
		{
			return;
		}
		if (ModManager.MSC)
		{
			List<IntVector2> list = new List<IntVector2>();
			if (game.IsStorySession && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && !abstractRoom.gate && !abstractRoom.shelter && world.region != null && world.region.name == "LF")
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				UnityEngine.Random.InitState(abstractRoom.index);
				int num = 0;
				for (int i = 0; i < Tiles.GetLength(0); i++)
				{
					for (int j = 0; j < Tiles.GetLength(1) - 2; j++)
					{
						if (!Tiles[i, j].Solid || Tiles[i, j + 1].Solid || Tiles[i, j + 2].Solid)
						{
							continue;
						}
						if (UnityEngine.Random.value < 0.25f || num > 0)
						{
							if (num <= -2 && UnityEngine.Random.value < 0.25f)
							{
								num = UnityEngine.Random.Range(2, 6);
							}
							for (int k = 0; k < UnityEngine.Random.Range(1, 4) && j + k + 1 < Tiles.GetLength(1) && !Tiles[i, j + k + 1].Solid; k++)
							{
								list.Add(new IntVector2(i, j + k + 1));
							}
						}
						num--;
					}
				}
				UnityEngine.Random.state = state;
			}
			if (list.Count > 0)
			{
				AddObject(new WormGrass(this, list));
			}
			if (abstractRoom.firstTimeRealized && abstractRoom.shelter)
			{
				for (int num2 = abstractRoom.entities.Count - 1; num2 >= 0; num2--)
				{
					if (abstractRoom.entities[num2] is AbstractSpear)
					{
						AbstractSpear abstractSpear = abstractRoom.entities[num2] as AbstractSpear;
						if (abstractSpear.needle && !abstractSpear.stuckInWall)
						{
							abstractRoom.entities.RemoveAt(num2);
						}
					}
				}
			}
		}
		if (water)
		{
			AddWater();
		}
		if (ModManager.MSC && game.globalRain.drainWorldFlood > 0f && (roomSettings.DangerType == RoomRain.DangerType.Flood || roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && waterObject == null)
		{
			defaultWaterLevel = -2000;
			AddWater();
		}
		if (abstractRoom.shelter)
		{
			shelterDoor = new ShelterDoor(this);
			AddObject(shelterDoor);
		}
		else if (IsGateRoom())
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Egates.txt"));
			for (int l = 0; l < array.Length + 1; l++)
			{
				if (l >= array.Length)
				{
					Custom.Log(abstractRoom.name, "is a WATER GATE!");
					regionGate = new WaterGate(this);
					AddObject(regionGate);
					break;
				}
				if (array[l] == abstractRoom.name)
				{
					Custom.Log(abstractRoom.name, "is an ELECTRIC GATE!");
					regionGate = new ElectricGate(this);
					AddObject(regionGate);
					break;
				}
			}
		}
		List<IntVector2> list2 = new List<IntVector2>();
		for (int m = 0; m < TileWidth; m++)
		{
			for (int n = 0; n < TileHeight - 1; n++)
			{
				if (GetTile(m, n).Terrain != Tile.TerrainType.Solid && GetTile(m, n + 1).Terrain == Tile.TerrainType.Solid && GetTile(m, n - 1).Terrain != Tile.TerrainType.Solid && n > defaultWaterLevel)
				{
					list2.Add(new IntVector2(m, n));
				}
			}
		}
		ceilingTiles = list2.ToArray();
		if ((!abstractRoom.shelter || world.brokenShelters[abstractRoom.shelterIndex]) && (roomSettings.DangerType == RoomRain.DangerType.Rain || roomSettings.DangerType == RoomRain.DangerType.FloodAndRain || roomSettings.DangerType == RoomRain.DangerType.Flood || roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard))
		{
			roomRain = new RoomRain(game.globalRain, this);
			AddObject(roomRain);
		}
		if ((!abstractRoom.shelter || world.brokenShelters[abstractRoom.shelterIndex]) && (roomSettings.DangerType == RoomRain.DangerType.Rain || roomSettings.DangerType == RoomRain.DangerType.FloodAndRain || roomSettings.DangerType == RoomRain.DangerType.Flood || roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard || roomSettings.DangerType == RoomRain.DangerType.None || (ModManager.MSC && roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)) && !water)
		{
			bool flag = false;
			for (int num3 = 0; num3 < TileWidth; num3++)
			{
				if (!GetTile(num3, 0).Solid)
				{
					flag = true;
					break;
				}
			}
			if (ModManager.MSC && roomSettings.GetEffect(MoreSlugcatsEnums.RoomEffectType.RoomWrap) != null)
			{
				flag = false;
			}
			if (flag)
			{
				deathFallGraphic = new DeathFallGraphic();
				AddObject(deathFallGraphic);
			}
		}
		for (int num4 = 0; num4 < roomSettings.effects.Count; num4++)
		{
			if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Dustpuffs)
			{
				AddObject(new RoofTopView.DustpuffSpawner());
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Coldness)
			{
				AddObject(new ColdRoom(this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.SkyDandelions)
			{
				AddObject(new SkyDandelions(roomSettings.effects[num4], this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Lightning || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.BkgOnlyLightning)
			{
				if (lightning == null)
				{
					lightning = new Lightning(this, roomSettings.effects[num4].amount, roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.BkgOnlyLightning);
					AddObject(lightning);
				}
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.GreenSparks)
			{
				AddObject(new GreenSparks(this, roomSettings.effects[num4].amount));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.VoidMelt)
			{
				AddObject(new MeltLights(roomSettings.effects[num4], this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.SunBlock)
			{
				AddObject(new SunBlocker());
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.ZeroG || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.BrokenZeroG)
			{
				bool flag2 = false;
				for (int num5 = 0; num5 < updateList.Count; num5++)
				{
					if (flag2)
					{
						break;
					}
					if (updateList[num5] is AntiGravity)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					AddObject(new AntiGravity(this));
				}
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.SSSwarmers)
			{
				bool flag3 = true;
				int num6 = updateList.Count - 1;
				while (num6 >= 0 && flag3)
				{
					flag3 = !(updateList[num6] is CoralNeuronSystem);
					num6--;
				}
				if (flag3)
				{
					AddObject(new CoralNeuronSystem());
				}
				waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 40);
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
			{
				AddObject(new AboveCloudsView(this, roomSettings.effects[num4]));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.RoofTopView)
			{
				AddObject(new RoofTopView(this, roomSettings.effects[num4]));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.VoidSea)
			{
				AddObject(new VoidSeaScene(this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.ElectricDeath)
			{
				AddObject(new ElectricDeath(roomSettings.effects[num4], this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.VoidSpawn)
			{
				if ((game.StoryCharacter != SlugcatStats.Name.Red || (world.region != null && world.region.name == "SB")) && ((game.session is StoryGameSession && (game.session as StoryGameSession).saveState.CanSeeVoidSpawn) || game.setupValues.playerGlowing))
				{
					AddObject(new VoidSpawnKeeper(this, roomSettings.effects[num4]));
				}
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.SSMusic)
			{
				AddObject(new SSMusicTrigger(roomSettings.effects[num4]));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.BorderPushBack)
			{
				AddObject(new RoomBorderPushBack(this));
			}
			else if (roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Flies || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.FireFlies || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.TinyDragonFly || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.RockFlea || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.RedSwarmer || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Ant || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Beetle || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.WaterGlowworm || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Wasp || roomSettings.effects[num4].type == RoomSettings.RoomEffect.Type.Moth)
			{
				if (insectCoordinator == null)
				{
					insectCoordinator = new InsectCoordinator(this);
					AddObject(insectCoordinator);
				}
				insectCoordinator.AddEffect(roomSettings.effects[num4]);
			}
			else if (ModManager.MSC && roomSettings.effects[num4].type == MoreSlugcatsEnums.RoomEffectType.FastFloodDrain && abstractRoom.firstTimeRealized)
			{
				world.game.globalRain.drainWorldFastDrainCounter += (int)(roomSettings.effects[num4].amount * 4800f);
			}
			else if (ModManager.MSC && roomSettings.effects[num4].type == MoreSlugcatsEnums.RoomEffectType.FastFloodPullDown)
			{
				float num7 = Mathf.InverseLerp(0.5f, 1f, roomSettings.effects[num4].amount);
				float num8 = Mathf.InverseLerp(0.49999f, 0f, roomSettings.effects[num4].amount);
				int num9 = (int)(num7 * 1000f) + (int)(num8 * -1000f);
				if (game.globalRain.drainWorldFlood > 0f && game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(abstractRoom.index, 0, num9 + 1, -1)))
				{
					Custom.Log("Pushed drainworld flood down by", num9.ToString());
					game.globalRain.DrainWorldFloodInit(new WorldCoordinate(abstractRoom.index, 0, num9, -1));
				}
			}
			else if (ModManager.MSC && roomSettings.effects[num4].type == MoreSlugcatsEnums.RoomEffectType.DustWave)
			{
				AddObject(new DustWave(game.cameras[0], roomSettings.effects[num4].amount));
				dustStorm = true;
			}
		}
		for (int num10 = 0; num10 < roomSettings.placedObjects.Count; num10++)
		{
			if (!roomSettings.placedObjects[num10].active)
			{
				continue;
			}
			if (ModManager.Expedition && game.rainWorld.ExpeditionMode && ((ModManager.MSC && roomSettings.placedObjects[num10].type == PlacedObject.Type.BlueToken) || roomSettings.placedObjects[num10].type == PlacedObject.Type.GoldToken || roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.GreenToken || roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.RedToken || roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken || roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.DevToken))
			{
				ExpLog.Log("Preventing Token spawn in Expedition mode");
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LightSource)
			{
				LightSource lightSource = new LightSource(roomSettings.placedObjects[num10].pos, environmentalLight: true, new Color(1f, 1f, 1f), null);
				AddObject(lightSource);
				lightSource.setRad = (roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).Rad;
				lightSource.setAlpha = (roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).strength;
				lightSource.fadeWithSun = (roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).fadeWithSun;
				lightSource.colorFromEnvironment = (roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).colorType == PlacedObject.LightSourceData.ColorType.Environment;
				lightSource.flat = (roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).flat;
				lightSource.effectColor = Math.Max(-1, (int)(roomSettings.placedObjects[num10].data as PlacedObject.LightSourceData).colorType - 2);
				SetLightSourceBlink(lightSource, num10);
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LightFixture)
			{
				if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.RedLight)
				{
					AddObject(new Redlight(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData));
				}
				else if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.HolyFire)
				{
					AddObject(new HolyFire(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData));
				}
				else if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.ZapCoilLight)
				{
					AddObject(new ZapCoilLight(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData));
				}
				else if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.DeepProcessing)
				{
					AddObject(new DeepProcessingLight(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData));
				}
				else if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.SlimeMoldLight)
				{
					AddObject(new SlimeMoldLight(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData));
				}
				else if ((roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData).type == PlacedObject.LightFixtureData.Type.RedSubmersible)
				{
					AddObject(new Redlight(this, roomSettings.placedObjects[num10], roomSettings.placedObjects[num10].data as PlacedObject.LightFixtureData, submersible: true));
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CoralNeuron || roomSettings.placedObjects[num10].type == PlacedObject.Type.CoralCircuit || roomSettings.placedObjects[num10].type == PlacedObject.Type.CoralStem || roomSettings.placedObjects[num10].type == PlacedObject.Type.CoralStemWithNeurons || roomSettings.placedObjects[num10].type == PlacedObject.Type.WallMycelia)
			{
				bool flag4 = true;
				int num11 = updateList.Count - 1;
				while (num11 >= 0 && flag4)
				{
					flag4 = !(updateList[num11] is CoralNeuronSystem);
					num11--;
				}
				if (flag4)
				{
					AddObject(new CoralNeuronSystem());
				}
				waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ProjectedStars)
			{
				AddObject(new StarMatrix(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ZapCoil)
			{
				AddObject(new ZapCoil((roomSettings.placedObjects[num10].data as PlacedObject.GridRectObjectData).Rect, this));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SuperStructureFuses)
			{
				AddObject(new SuperStructureFuses(roomSettings.placedObjects[num10], (roomSettings.placedObjects[num10].data as PlacedObject.GridRectObjectData).Rect, this));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.GravityDisruptor)
			{
				AddObject(new GravityDisruptor(roomSettings.placedObjects[num10], this));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SpotLight)
			{
				AddObject(new SpotLight(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.DeepProcessing)
			{
				AddObject(new DeepProcessing(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.Corruption)
			{
				DaddyCorruption daddyCorruption = null;
				int num12 = updateList.Count - 1;
				while (num12 >= 0 && daddyCorruption == null)
				{
					if (updateList[num12] is DaddyCorruption)
					{
						daddyCorruption = updateList[num12] as DaddyCorruption;
					}
					num12--;
				}
				if (daddyCorruption == null)
				{
					daddyCorruption = new DaddyCorruption(this);
					AddObject(daddyCorruption);
				}
				daddyCorruption.places.Add(roomSettings.placedObjects[num10]);
				waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CorruptionDarkness)
			{
				AddObject(new DaddyCorruption.CorruptionDarkness(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SSLightRod)
			{
				AddObject(new SSLightRod(roomSettings.placedObjects[num10], this));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.GhostSpot)
			{
				if (game.world.worldGhost != null && game.world.worldGhost.ghostRoom == abstractRoom)
				{
					AddObject(new Ghost(this, roomSettings.placedObjects[num10], game.world.worldGhost));
				}
				else if (world.region != null)
				{
					GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(world.region.name);
					if (game.session is StoryGameSession && (!(game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 0))
					{
						AddObject(new GhostHunch(this, ghostID));
					}
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ScavengerOutpost)
			{
				AddObject(new ScavengerOutpost(roomSettings.placedObjects[num10], this));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SuperJumpInstruction)
			{
				AddObject(new SuperJumpInstruction(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LanternOnStick)
			{
				AddObject(new LanternStick(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.TradeOutpost)
			{
				AddObject(new ScavengerTradeSpot(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ScavengerTreasury)
			{
				AddObject(new ScavengerTreasury(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ScavTradeInstruction)
			{
				AddObject(new ScavengerTradeInstructionTrigger(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CosmeticSlimeMold)
			{
				AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num10].pos, (roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData).Rad, throughWalls: false));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CosmeticSlimeMold2)
			{
				AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num10].pos, (roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData).Rad, throughWalls: true));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SlimeMold)
			{
				float num13 = game.SeededRandom((int)(roomSettings.placedObjects[num10].pos.x + roomSettings.placedObjects[num10].pos.y));
				if (num13 > 0.3f)
				{
					AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num10].pos, Custom.LerpMap(num13, 0.3f, 1f, 30f, 70f), throughWalls: false));
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CustomDecal)
			{
				AddObject(new CustomDecal(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.InsectGroup)
			{
				if (insectCoordinator == null)
				{
					insectCoordinator = new InsectCoordinator(this);
					AddObject(insectCoordinator);
				}
				insectCoordinator.AddGroup(roomSettings.placedObjects[num10]);
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.PlayerPushback)
			{
				AddObject(new PlayerPushback(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.MultiplayerItem)
			{
				if (game.IsArenaSession)
				{
					game.GetArenaGameSession.SpawnItem(this, roomSettings.placedObjects[num10]);
				}
				if (game.IsStorySession)
				{
					SpawnMultiplayerItem(roomSettings.placedObjects[num10]);
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.GoldToken || roomSettings.placedObjects[num10].type == PlacedObject.Type.BlueToken)
			{
				if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected((roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).tokenString, (roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).isBlue))
				{
					AddObject(new CollectToken(this, roomSettings.placedObjects[num10]));
				}
				else
				{
					AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.DeadTokenStalk)
			{
				AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData).handlePos, null, blue: false));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.ReliableIggyDirection)
			{
				AddObject(new ReliableIggyDirection(roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.Rainbow)
			{
				if ((world.rainCycle.CycleStartUp < 1f && (game.cameras[0] == null || game.cameras[0].ghostMode == 0f) && (game.SeededRandom(world.rainCycle.rainbowSeed + abstractRoom.index) < (roomSettings.placedObjects[num10].data as Rainbow.RainbowData).Chance || (ModManager.MSC && world.rainCycle.preTimer > 0))) || game.IsArenaSession)
				{
					AddObject(new Rainbow(this, roomSettings.placedObjects[num10]));
				}
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LightBeam)
			{
				LightBeam lightBeam = new LightBeam(roomSettings.placedObjects[num10]);
				SetLightBeamBlink(lightBeam, num10);
				AddObject(lightBeam);
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.DeathFallFocus)
			{
				deathFallFocalPoints.Add(roomSettings.placedObjects[num10].pos);
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.GreenToken)
			{
				if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SlugcatUnlockID((roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).tokenString)))
				{
					AddObject(new CollectToken(this, roomSettings.placedObjects[num10]));
				}
				else
				{
					AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
				}
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken)
			{
				CollectToken.CollectTokenData collectTokenData = roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData;
				if (!collectTokenData.availableToPlayers.Contains(MoreSlugcatsEnums.SlugcatStatsName.Spear))
				{
					collectTokenData.availableToPlayers.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
				}
				if (game.IsStorySession && !collectTokenData.availableToPlayers.Contains(game.StoryCharacter))
				{
					continue;
				}
				if (game.IsStorySession && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData).handlePos, null, blue: false));
					continue;
				}
				ChatlogData.ChatlogID chatlogCollect = collectTokenData.ChatlogCollect;
				if (chatlogCollect == null || !game.IsStorySession || game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Contains(chatlogCollect))
				{
					if (!collectTokenData.availableToPlayers.Contains(MoreSlugcatsEnums.SlugcatStatsName.Spear))
					{
						collectTokenData.availableToPlayers.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
					}
					CollectToken.TokenStalk obj = new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + collectTokenData.handlePos, null, blue: false)
					{
						forceSatellite = true
					};
					AddObject(obj);
				}
				else
				{
					AddObject(new CollectToken(this, roomSettings.placedObjects[num10]));
				}
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.RedToken)
			{
				if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID((roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).tokenString)))
				{
					AddObject(new CollectToken(this, roomSettings.placedObjects[num10]));
				}
				else
				{
					AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
				}
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.DevToken && game.rainWorld.options.commentary && game.rainWorld.options.DeveloperCommentaryLocalized())
			{
				AddObject(new CollectToken(this, roomSettings.placedObjects[num10]));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LightningMachine)
			{
				AddObject(new LightningMachine(roomSettings.placedObjects[num10].pos, new Vector2(0f, 0f), new Vector2(0f, 0f), 1f, permanent: false, radial: false, 1f, 1f, 10f)
				{
					startPoint = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).startPoint,
					endPoint = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).endPoint,
					chance = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).chance,
					permanent = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).permanent,
					radial = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).radial,
					width = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).width,
					intensity = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).intensity,
					lifeTime = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).lifeTime,
					lightningParam = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).lightningParam,
					lightningType = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).lightningType,
					volume = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).volume,
					impactType = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).impact,
					soundType = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).soundType,
					random = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).random,
					light = (roomSettings.placedObjects[num10].data as PlacedObject.LightningMachineData).light
				});
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.EnergySwirl)
			{
				AddObject(new EnergySwirl(roomSettings.placedObjects[num10].pos, new Color(1f, 1f, 1f), null)
				{
					setRad = (roomSettings.placedObjects[num10].data as PlacedObject.EnergySwirlData).Rad,
					setDepth = (roomSettings.placedObjects[num10].data as PlacedObject.EnergySwirlData).depth,
					colorFromEnviroment = ((roomSettings.placedObjects[num10].data as PlacedObject.EnergySwirlData).colorType == PlacedObject.EnergySwirlData.ColorType.Environment),
					effectColor = Math.Max(-1, (roomSettings.placedObjects[num10].data as PlacedObject.EnergySwirlData).colorType.Index - PlacedObject.EnergySwirlData.ColorType.EffectColor1.Index)
				});
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SteamPipe || roomSettings.placedObjects[num10].type == PlacedObject.Type.WallSteamer)
			{
				AddObject(new SteamPipe(roomSettings.placedObjects[num10].pos, Custom.DirVec(roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].pos + (roomSettings.placedObjects[num10].data as PlacedObject.SteamPipeData).handlePos), Mathf.Clamp((roomSettings.placedObjects[num10].data as PlacedObject.SteamPipeData).Rad / 250f, 0f, 1f), roomSettings.placedObjects[num10].type == PlacedObject.Type.WallSteamer));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.SnowSource)
			{
				SnowSource snowSource = new SnowSource(roomSettings.placedObjects[num10].pos);
				AddObject(snowSource);
				snowSource.rad = (roomSettings.placedObjects[num10].data as PlacedObject.SnowSourceData).Rad;
				snowSource.intensity = (roomSettings.placedObjects[num10].data as PlacedObject.SnowSourceData).intensity;
				snowSource.noisiness = (roomSettings.placedObjects[num10].data as PlacedObject.SnowSourceData).noisiness;
				snowSource.shape = (roomSettings.placedObjects[num10].data as PlacedObject.SnowSourceData).shape;
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.LocalBlizzard)
			{
				LocalBlizzard localBlizzard = new LocalBlizzard(roomSettings.placedObjects[num10].pos, 100f, 1f, 0.5f);
				AddObject(localBlizzard);
				localBlizzard.rad = (roomSettings.placedObjects[num10].data as PlacedObject.LocalBlizzardData).Rad;
				localBlizzard.intensity = (roomSettings.placedObjects[num10].data as PlacedObject.LocalBlizzardData).intensity;
				localBlizzard.scale = (roomSettings.placedObjects[num10].data as PlacedObject.LocalBlizzardData).scale;
				localBlizzard.angle = (roomSettings.placedObjects[num10].data as PlacedObject.LocalBlizzardData).angle;
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.Vine)
			{
				ClimbableVineRenderer climbableVineRenderer = null;
				int num14 = updateList.Count - 1;
				while (num14 >= 0 && climbableVineRenderer == null)
				{
					if (updateList[num14] is ClimbableVineRenderer)
					{
						climbableVineRenderer = updateList[num14] as ClimbableVineRenderer;
					}
					num14--;
				}
				if (climbableVineRenderer == null)
				{
					climbableVineRenderer = new ClimbableVineRenderer(this);
					AddObject(climbableVineRenderer);
				}
				waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.OEsphere)
			{
				OEsphere oEsphere = new OEsphere(roomSettings.placedObjects[num10].pos, 100f, 0);
				AddObject(oEsphere);
				oEsphere.rad = (roomSettings.placedObjects[num10].data as PlacedObject.OEsphereData).Rad;
				oEsphere.depth = (roomSettings.placedObjects[num10].data as PlacedObject.OEsphereData).depth;
				oEsphere.lIntensity = (roomSettings.placedObjects[num10].data as PlacedObject.OEsphereData).lIntensity;
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.CellDistortion)
			{
				CellDistortion cellDistortion = new CellDistortion(roomSettings.placedObjects[num10].pos, 100f, 1f, 0.5f, 0f, 0f);
				AddObject(cellDistortion);
				cellDistortion.rad = (roomSettings.placedObjects[num10].data as PlacedObject.CellDistortionData).Rad;
				cellDistortion.intensity = (roomSettings.placedObjects[num10].data as PlacedObject.CellDistortionData).intensity;
				cellDistortion.scale = (roomSettings.placedObjects[num10].data as PlacedObject.CellDistortionData).scale;
				cellDistortion.cromaticIntensity = (roomSettings.placedObjects[num10].data as PlacedObject.CellDistortionData).chromaticIntensity;
				cellDistortion.timeMult = (roomSettings.placedObjects[num10].data as PlacedObject.CellDistortionData).timeMult;
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine)
			{
				AddObject(new HRKarmaShrine(this, roomSettings.placedObjects[num10].pos, roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.NeuronSpawner)
			{
				AddObject(new SSSwarmerSpawner(this, roomSettings.placedObjects[num10].data as PlacedObject.ResizableObjectData));
			}
			else if (roomSettings.placedObjects[num10].type == PlacedObject.Type.HangingPearls)
			{
				float length = Mathf.Lerp(60f, 180f, 0.5f + Mathf.Sin(roomSettings.placedObjects[num10].pos.x * 10f) / 2f);
				AddObject(new HangingPearlString(this, length, roomSettings.placedObjects[num10].pos));
			}
			else if (ModManager.MSC && roomSettings.placedObjects[num10].type == MoreSlugcatsEnums.PlacedObjectType.MSArteryPush)
			{
				AddObject(new MSArteryWaterFlow(this, roomSettings.placedObjects[num10]));
			}
		}
		if (abstractRoom == null)
		{
			Custom.LogWarning("NULL ABSTRACT ROOM");
		}
		if (game.world.worldGhost != null && game.world.worldGhost.CreaturesSleepInRoom(abstractRoom))
		{
			AddObject(new GhostCreatureSedater(this));
		}
		if (roomSettings.roomSpecificScript)
		{
			RoomSpecificScript.AddRoomSpecificScript(this);
		}
		if (abstractRoom.firstTimeRealized)
		{
			for (int num15 = 0; num15 < roomSettings.placedObjects.Count; num15++)
			{
				if (!roomSettings.placedObjects[num15].active)
				{
					continue;
				}
				if (ModManager.Expedition && game.rainWorld.ExpeditionMode && roomSettings.placedObjects[num15].type == PlacedObject.Type.KarmaFlower)
				{
					ExpLog.Log("Preventing natural KarmaFlower spawn");
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.FlareBomb)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.AddEntity(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.PuffBall)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.AddEntity(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.TempleGuard)
				{
					if (game.setupValues.worldCreaturesSpawn)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID());
						abstractRoom.AddEntity(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.DangleFruit)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.DataPearl || roomSettings.placedObjects[num15].type == PlacedObject.Type.UniqueDataPearl)
				{
					if (game.session is StoryGameSession && (game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						continue;
					}
					DataPearl.AbstractDataPearl.DataPearlType pearlType = (roomSettings.placedObjects[num15].data as PlacedObject.DataPearlData).pearlType;
					AbstractPhysicalObject abstractPhysicalObject;
					if (ModManager.MSC && pearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
					{
						abstractPhysicalObject = new SpearMasterPearl.AbstractSpearMasterPearl(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					}
					else
					{
						AbstractPhysicalObject.AbstractObjectType objType = AbstractPhysicalObject.AbstractObjectType.DataPearl;
						if (ModManager.MSC && pearlType == MoreSlugcatsEnums.DataPearlType.RM)
						{
							objType = MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl;
						}
						abstractPhysicalObject = new DataPearl.AbstractDataPearl(world, objType, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData, (game.StoryCharacter == SlugcatStats.Name.Yellow && !ModManager.MSC) ? DataPearl.AbstractDataPearl.DataPearlType.Misc : pearlType);
					}
					(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
					(abstractPhysicalObject as DataPearl.AbstractDataPearl).hidden = (roomSettings.placedObjects[num15].data as PlacedObject.DataPearlData).hidden;
					abstractRoom.entities.Add(abstractPhysicalObject);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.SeedCob)
				{
					AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, dead: false, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject);
					abstractPhysicalObject.Realize();
					abstractPhysicalObject.realizedObject.PlaceInRoom(this);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.DeadSeedCob)
				{
					AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, dead: true, null);
					abstractRoom.entities.Add(abstractPhysicalObject);
					abstractPhysicalObject.Realize();
					abstractPhysicalObject.realizedObject.PlaceInRoom(this);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.WaterNut)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new WaterNut.AbstractWaterNut(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData, swollen: false);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.AddEntity(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.JellyFish)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.JellyFish, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.KarmaFlower)
				{
					if (game.StoryCharacter != SlugcatStats.Name.Red && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: true, abstractRoom.index, num15)))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.Mushroom)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.VoidSpawnEgg)
				{
					if ((game.StoryCharacter != SlugcatStats.Name.Red || UnityEngine.Random.value < 1f / 17f) && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)) && (game.setupValues.playerGlowing || (game.session is StoryGameSession && (game.session as StoryGameSession).saveState.CanSeeVoidSpawn) || world.region.name == "SL"))
					{
						AddObject(new VoidSpawnEgg(this, num15, roomSettings.placedObjects[num15]));
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.FirecrackerPlant)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.AddEntity(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.VultureGrub || roomSettings.placedObjects[num15].type == PlacedObject.Type.DeadVultureGrub)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.VultureGrub), null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID());
						(abstractCreature.state as VultureGrub.VultureGrubState).origRoom = abstractRoom.index;
						(abstractCreature.state as VultureGrub.VultureGrubState).placedObjectIndex = num15;
						abstractRoom.AddEntity(abstractCreature);
						if (roomSettings.placedObjects[num15].type == PlacedObject.Type.DeadVultureGrub)
						{
							(abstractCreature.state as VultureGrub.VultureGrubState).Die();
						}
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.SlimeMold)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.ReliableSpear)
				{
					AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), explosive: false);
					abstractRoom.entities.Add(abstractPhysicalObject);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.FlyLure)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlyLure, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.SporePlant)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new SporePlant.AbstractSporePlant(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData, used: false, pacified: false);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.NeedleEgg)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.BubbleGrass)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractPhysicalObject abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), 1f, abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.Hazer || roomSettings.placedObjects[num15].type == PlacedObject.Type.DeadHazer)
				{
					if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15))
					{
						AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID());
						(abstractCreature2.state as VultureGrub.VultureGrubState).origRoom = abstractRoom.index;
						(abstractCreature2.state as VultureGrub.VultureGrubState).placedObjectIndex = num15;
						abstractRoom.AddEntity(abstractCreature2);
						if (roomSettings.placedObjects[num15].type == PlacedObject.Type.DeadHazer)
						{
							(abstractCreature2.state as VultureGrub.VultureGrubState).Die();
						}
					}
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.VultureMask)
				{
					float num16 = 0.1f;
					EntityID newID = game.GetNewID();
					AbstractPhysicalObject item = new VultureMask.AbstractVultureMask(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), newID, newID.RandomSeed, UnityEngine.Random.value <= num16);
					abstractRoom.entities.Add(item);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.Lantern && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					EntityID newID2 = game.GetNewID();
					AbstractPhysicalObject abstractPhysicalObject2 = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), newID2, abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject2 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject2);
				}
				else if (roomSettings.placedObjects[num15].type == PlacedObject.Type.BlinkingFlower && abstractRoom.firstTimeRealized)
				{
					AbstractPhysicalObject item2 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.BlinkingFlower, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID());
					abstractRoom.entities.Add(item2);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.GooieDuck && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					AbstractPhysicalObject abstractPhysicalObject3 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject3 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject3);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.LillyPuck && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					AbstractPhysicalObject abstractPhysicalObject4 = new LillyPuck.AbstractLillyPuck(world, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), 3, abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject4 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject4);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.GlowWeed && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					AbstractPhysicalObject abstractPhysicalObject5 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject5 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject5);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.BigJellyFish && abstractRoom.firstTimeRealized)
				{
					Vector2 pos = roomSettings.placedObjects[num15].pos;
					AbstractCreature abstractCreature3 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.BigJelly), null, GetWorldCoordinate(pos), new EntityID(-1, abstractRoom.index * 1000 + num15));
					(abstractCreature3.state as BigJellyState).HomePos = pos;
					(abstractCreature3.state as BigJellyState).DriftPos = pos + (roomSettings.placedObjects[num15].data as PlacedObject.ResizableObjectData).handlePos;
					abstractRoom.AddEntity(abstractCreature3);
					abstractCreature3.pos.abstractNode = 0;
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.Stowaway && abstractRoom.firstTimeRealized)
				{
					Vector2 pos2 = roomSettings.placedObjects[num15].pos;
					AbstractCreature abstractCreature4 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug), null, GetWorldCoordinate(pos2), new EntityID(-1, abstractRoom.index * 1000 + num15));
					(abstractCreature4.state as StowawayBugState).HomePos = pos2;
					(abstractCreature4.state as StowawayBugState).aimPos = pos2 + (roomSettings.placedObjects[num15].data as PlacedObject.ResizableObjectData).handlePos;
					abstractRoom.AddEntity(abstractCreature4);
					abstractCreature4.pos.abstractNode = 0;
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.MoonCloak && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					Custom.Log("Spawned moon cloak!");
					AbstractPhysicalObject abstractPhysicalObject6 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject6 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject6);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.DandelionPeach && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					AbstractPhysicalObject abstractPhysicalObject7 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.DandelionPeach, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject7 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject7);
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num15].type == MoreSlugcatsEnums.PlacedObjectType.HRGuard && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num15)))
				{
					AbstractPhysicalObject abstractPhysicalObject8 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.HRGuard, null, GetWorldCoordinate(roomSettings.placedObjects[num15].pos), game.GetNewID(), abstractRoom.index, num15, roomSettings.placedObjects[num15].data as PlacedObject.ConsumableObjectData);
					(abstractPhysicalObject8 as AbstractConsumable).isConsumed = false;
					abstractRoom.entities.Add(abstractPhysicalObject8);
				}
			}
			if (!abstractRoom.shelter && !abstractRoom.gate && game != null && (!game.IsArenaSession || game.GetArenaGameSession.GameTypeSetup.levelItems) && (!ModManager.MMF || roomSettings.RandomItemDensity > 0f))
			{
				for (int num17 = (int)((float)TileWidth * (float)TileHeight * Mathf.Pow(roomSettings.RandomItemDensity, 2f) / 5f); num17 >= 0; num17--)
				{
					IntVector2 intVector = RandomTile();
					if (!GetTile(intVector).Solid)
					{
						bool flag5 = true;
						if (!ModManager.MMF || roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.ZeroG) < 1f || roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
						{
							for (int num18 = -1; num18 < 2; num18++)
							{
								if (!GetTile(intVector + new IntVector2(num18, -1)).Solid)
								{
									flag5 = false;
									break;
								}
								if (ModManager.MMF && GetTile(intVector).Terrain == Tile.TerrainType.Slope && GetTile(intVector + new IntVector2(0, 1)).Solid)
								{
									flag5 = false;
									break;
								}
							}
						}
						else if (ModManager.MMF)
						{
							bool flag6 = false;
							for (int num19 = -1; num19 < 2; num19++)
							{
								if (GetTile(intVector + new IntVector2(num19, 0)).Solid)
								{
									flag6 = true;
									break;
								}
							}
							bool flag7 = false;
							for (int num20 = -1; num20 < 2; num20++)
							{
								if (GetTile(intVector + new IntVector2(0, num20)).Solid)
								{
									flag7 = true;
									break;
								}
							}
							if (flag6 && flag7)
							{
								flag5 = false;
							}
							else if (!(flag6 ^ flag7) && UnityEngine.Random.value > 0.1f)
							{
								flag5 = false;
							}
						}
						if (flag5)
						{
							EntityID newID3 = game.GetNewID(-abstractRoom.index);
							AbstractPhysicalObject abstractPhysicalObject9;
							if (game != null && UnityEngine.Random.value < SlugcatStats.SpearSpawnModifier(game.StoryCharacter, roomSettings.RandomItemSpearChance))
							{
								abstractPhysicalObject9 = new AbstractSpear(world, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID3, UnityEngine.Random.value < SlugcatStats.SpearSpawnExplosiveRandomChance(game.StoryCharacter));
								if (ModManager.MSC && (abstractPhysicalObject9 as AbstractSpear).explosive && UnityEngine.Random.value < SlugcatStats.SpearSpawnElectricRandomChance(game.StoryCharacter))
								{
									(abstractPhysicalObject9 as AbstractSpear).explosive = false;
									(abstractPhysicalObject9 as AbstractSpear).electric = true;
								}
							}
							else
							{
								abstractPhysicalObject9 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID3);
							}
							abstractRoom.AddEntity(abstractPhysicalObject9);
						}
					}
				}
			}
			if (ModManager.Expedition && game.rainWorld.ExpeditionMode && abstractRoom.shelter && abstractRoom.name == ExpeditionData.startingDen && game.rainWorld.progression.currentSaveState.cycleNumber == 0 && game.world.rainCycle.CycleProgression <= 0f)
			{
				WorldCoordinate pos3 = new WorldCoordinate(abstractRoom.index, shelterDoor.playerSpawnPos.x, shelterDoor.playerSpawnPos.y, 0);
				if (ExpeditionData.activeMission == "mis2" && world.region.name == "SS")
				{
					AbstractPhysicalObject abstractPhysicalObject10 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, pos3, game.GetNewID());
					abstractRoom.entities.Add(abstractPhysicalObject10);
					abstractPhysicalObject10.Realize();
				}
				for (int num21 = 0; num21 < ExpeditionGame.activeUnlocks.Count; num21++)
				{
					string text = ExpeditionGame.activeUnlocks[num21];
					if (text == "unl-lantern")
					{
						AbstractPhysicalObject abstractPhysicalObject11 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, pos3, game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject11);
						abstractPhysicalObject11.Realize();
					}
					if (text == "unl-bomb")
					{
						AbstractPhysicalObject abstractPhysicalObject12 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, pos3, game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject12);
						abstractPhysicalObject12.Realize();
					}
					if (text == "unl-vulture")
					{
						bool king = UnityEngine.Random.value > 0.95f;
						AbstractPhysicalObject abstractPhysicalObject13 = new VultureMask.AbstractVultureMask(world, null, pos3, game.GetNewID(), Mathf.RoundToInt(UnityEngine.Random.Range(0f, 100f)), king);
						abstractRoom.entities.Add(abstractPhysicalObject13);
						abstractPhysicalObject13.Realize();
					}
					if (ModManager.MSC)
					{
						if (text == "unl-electric")
						{
							AbstractSpear abstractSpear2 = new AbstractSpear(world, null, pos3, game.GetNewID(), explosive: false);
							abstractSpear2.electric = true;
							abstractSpear2.electricCharge = 10;
							abstractRoom.entities.Add(abstractSpear2);
							abstractSpear2.Realize();
						}
						if (text == "unl-sing")
						{
							AbstractPhysicalObject abstractPhysicalObject14 = new AbstractPhysicalObject(world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, pos3, game.GetNewID());
							abstractRoom.entities.Add(abstractPhysicalObject14);
							abstractPhysicalObject14.Realize();
						}
						if (text == "unl-gun")
						{
							JokeRifle.AbstractRifle abstractRifle = new JokeRifle.AbstractRifle(game.world, null, pos3, game.GetNewID(), JokeRifle.AbstractRifle.AmmoType.Rock);
							abstractRoom.entities.Add(abstractRifle);
							abstractRifle.Realize();
						}
					}
				}
			}
		}
		abstractRoom.firstTimeRealized = false;
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode && updateList != null && updateList.Count > 0)
		{
			if (abstractRoom.name == "SB_A14" || abstractRoom.name == "SB_E05SAINT")
			{
				bool flag8 = false;
				for (int num22 = 0; num22 < updateList.Count; num22++)
				{
					if (updateList[num22] is DepthsFinishScript)
					{
						flag8 = true;
					}
				}
				if (!flag8)
				{
					AddObject(new DepthsFinishScript(this));
				}
			}
			for (int num23 = 0; num23 < updateList.Count; num23++)
			{
				if (ExpeditionGame.IsMSCRoomScript(updateList[num23]) || ExpeditionGame.IsUndesirableRoomScript(updateList[num23]))
				{
					ExpLog.Log("Remove MSCRoomSpecificScript: " + updateList[num23].ToString());
					updateList[num23].Destroy();
				}
			}
		}
		for (int num24 = 0; num24 < roomSettings.triggers.Count; num24++)
		{
			if (!(game.session is StoryGameSession) || ((game.session as StoryGameSession).saveState.cycleNumber >= roomSettings.triggers[num24].activeFromCycle && (game.StoryCharacter == null || roomSettings.triggers[num24].slugcats.Contains(game.StoryCharacter)) && (roomSettings.triggers[num24].activeToCycle < 0 || (game.session as StoryGameSession).saveState.cycleNumber <= roomSettings.triggers[num24].activeToCycle)))
			{
				AddObject(new ActiveTriggerChecker(roomSettings.triggers[num24]));
			}
		}
		if (world.rainCycle.CycleStartUp < 1f && roomSettings.CeilingDrips > 0f && roomSettings.DangerType != RoomRain.DangerType.None && !abstractRoom.shelter)
		{
			AddObject(new DrippingSound());
		}
	}

	public void Unloaded()
	{
		if (roomRain != null)
		{
			roomRain.Unloaded();
		}
	}

	public void NowViewed()
	{
		if (snowObject == null)
		{
			Shader.DisableKeyword("SNOW_ON");
		}
		Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 0f);
		for (int i = 0; i < roomSettings.effects.Count; i++)
		{
			if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
			{
				for (int j = 0; (float)j < (float)(TileWidth * TileHeight) * roomSettings.effects[i].amount / 50f; j++)
				{
					Vector2 pos = new Vector2(UnityEngine.Random.value * PixelWidth, UnityEngine.Random.value * PixelHeight);
					if (!GetTile(pos).Solid && Mathf.Pow(UnityEngine.Random.value, 1f - roomSettings.effects[i].amount) > (float)(readyForAI ? aimap.getTerrainProximity(pos) : 5) * 0.05f && roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
					{
						AddObject(new GreenSparks.GreenSpark(pos));
					}
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ZeroGSpecks)
			{
				for (int k = 0; (float)k < 1000f * roomSettings.effects[i].amount; k++)
				{
					AddObject(new BlinkSpeck());
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.CorruptionSpores)
			{
				for (int l = 0; (float)l < 200f * roomSettings.effects[i].amount; l++)
				{
					AddObject(new CorruptionSpore());
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.SuperStructureProjector)
			{
				AddObject(new SuperStructureProjector(this, roomSettings.effects[i]));
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ProjectedScanLines)
			{
				AddObject(new ProjectedScanLines(this, roomSettings.effects[i]));
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.RoofTopView)
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.FairyParticles)
			{
				for (int m = 0; (float)m < 500f * roomSettings.effects[i].amount; m++)
				{
					int num_keyframes = ((UnityEngine.Random.value < 0.5f) ? 3 : 4);
					FairyParticle obj = new FairyParticle(UnityEngine.Random.Range(0, 360), num_keyframes, 60f, 180f, 40f, 100f, 5f, 30f);
					AddObject(obj);
				}
				for (int n = 0; n < roomSettings.placedObjects.Count; n++)
				{
					if (roomSettings.placedObjects[n].type == PlacedObject.Type.FairyParticleSettings)
					{
						(roomSettings.placedObjects[n].data as PlacedObject.FairyParticleData).Apply(this);
						break;
					}
				}
			}
			else
			{
				if (!(roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.DayNight))
				{
					continue;
				}
				for (int num = 0; num < roomSettings.placedObjects.Count; num++)
				{
					if (roomSettings.placedObjects[num].type == PlacedObject.Type.DayNightSettings)
					{
						(roomSettings.placedObjects[num].data as PlacedObject.DayNightData).Apply(this);
						break;
					}
				}
			}
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-blinded"))
		{
			PlacedObject.DayNightData dayNightData = new PlacedObject.DayNightData(null);
			dayNightData.nightPalette = 10;
			dayNightData.Apply(this);
			if (game.cameras[0].currentPalette.darkness < 0.8f)
			{
				game.cameras[0].effect_dayNight = 1f;
				game.cameras[0].currentPalette.darkness = 0.8f;
			}
			roomSettings.Clouds = 0.875f;
			world.rainCycle.sunDownStartTime = 0;
			world.rainCycle.dayNightCounter = 3750;
		}
		for (int num2 = 0; num2 < physicalObjects.Length; num2++)
		{
			for (int num3 = 0; num3 < physicalObjects[num2].Count; num3++)
			{
				physicalObjects[num2][num3].InitiateGraphicsModule();
				if (physicalObjects[num2][num3].graphicsModule != null && !drawableObjects.Contains(physicalObjects[num2][num3].graphicsModule))
				{
					drawableObjects.Add(physicalObjects[num2][num3].graphicsModule);
				}
			}
		}
		if (world.worldGhost != null)
		{
			for (int num4 = 0; num4 < cameraPositions.Length; num4++)
			{
				if (world.worldGhost.GhostMode(this, num4) > 0f)
				{
					AddObject(new GoldFlakes(this));
					break;
				}
			}
		}
		if (insectCoordinator != null)
		{
			insectCoordinator.NowViewed();
		}
	}

	public void NoLongerViewed()
	{
		for (int num = drawableObjects.Count - 1; num >= 0; num--)
		{
			if (drawableObjects[num] is GraphicsModule)
			{
				(drawableObjects[num] as GraphicsModule).owner.RemoveGraphicsModule();
				drawableObjects.RemoveAt(num);
			}
		}
		if (insectCoordinator != null)
		{
			insectCoordinator.NoLongerViewed();
		}
		blizzardGraphics = null;
	}

	public void BlinkShortCut(int shortcut, int secondary, float blinkFac)
	{
		if (secondary > -1)
		{
			shortcutsBlinking[secondary, 0] = 1f;
			if (shortcutsBlinking[secondary, 1] == 0f)
			{
				shortcutsBlinking[secondary, 1] = 0.01f;
			}
			shortcutsBlinking[secondary, 3] = -5f * blinkFac;
		}
		if (shortcut > -1)
		{
			shortcutsBlinking[shortcut, 3] = -20f * blinkFac;
		}
	}

	public void ShortCutsReady()
	{
		shortcutsBlinking = new float[shortcuts.Length, 4];
		if (game != null)
		{
			foreach (AbstractWorldEntity entity in abstractRoom.entities)
			{
				if (entity is AbstractCreature)
				{
					continue;
				}
				bool flag = true;
				if (entity is AbstractPhysicalObject)
				{
					List<AbstractPhysicalObject> allConnectedObjects = (entity as AbstractPhysicalObject).GetAllConnectedObjects();
					for (int i = 0; i < allConnectedObjects.Count && flag; i++)
					{
						if (allConnectedObjects[i] is AbstractCreature)
						{
							flag = false;
						}
					}
				}
				if (!entity.InDen && flag)
				{
					if (entity.pos.room == abstractRoom.index)
					{
						(entity as AbstractPhysicalObject).RealizeInRoom();
						continue;
					}
					Custom.LogWarning($"Object in wrong room! {entity.ID}");
				}
			}
		}
		bool flag2 = false;
		foreach (AbstractCreature creature in abstractRoom.creatures)
		{
			if (!creature.InDen && creature.creatureTemplate.requireAImap)
			{
				flag2 = true;
				break;
			}
		}
		if (abstractRoom.quantifiedCreatures != null)
		{
			for (int j = 0; j < StaticWorld.quantifiedCreatures.Length; j++)
			{
				if (StaticWorld.quantifiedCreatures[j].requireAImap && abstractRoom.NumberOfQuantifiedCreatureInRoom(StaticWorld.quantifiedCreatures[j].type) > 0)
				{
					flag2 = true;
					break;
				}
			}
		}
		loadingProgress = 1;
		if (!flag2)
		{
			readyForNonAICreaturesToEnter = true;
			if (game != null)
			{
				foreach (AbstractCreature creature2 in abstractRoom.creatures)
				{
					if (creature2.realizedCreature == null && creature2.AllowedToExistInRoom(this))
					{
						creature2.RealizeInRoom();
					}
				}
			}
		}
		for (int k = 0; k < updateList.Count; k++)
		{
			if (updateList[k] is INotifyWhenRoomIsReady)
			{
				(updateList[k] as INotifyWhenRoomIsReady).ShortcutsReady();
			}
		}
		AddObject(new ShortcutHelper(this));
	}

	public void ReadyForAI()
	{
		loadingProgress = 2;
		if (game != null)
		{
			foreach (AbstractCreature creature in abstractRoom.creatures)
			{
				if (creature.realizedCreature == null && creature.AllowedToExistInRoom(this))
				{
					creature.RealizeInRoom();
				}
			}
		}
		if (abstractRoom.quantifiedCreatures != null && game != null)
		{
			for (int i = 0; i < StaticWorld.quantifiedCreatures.Length; i++)
			{
				if (abstractRoom.NumberOfQuantifiedCreatureInRoom(StaticWorld.quantifiedCreatures[i].type) > 0)
				{
					PlaceQuantifiedCreaturesInRoom(StaticWorld.quantifiedCreatures[i].type);
				}
			}
		}
		loadingProgress = 3;
		readyForNonAICreaturesToEnter = true;
		for (int j = 0; j < updateList.Count; j++)
		{
			if (updateList[j] is INotifyWhenRoomIsReady)
			{
				(updateList[j] as INotifyWhenRoomIsReady).AIMapReady();
			}
		}
		bool flag = ModManager.MSC && game != null && ((game.IsStorySession && (abstractRoom.name == "DM_AI" || abstractRoom.name == "CL_AI" || abstractRoom.name == "RM_AI" || abstractRoom.name == "HR_AI")) || abstractRoom.name == "Multi_SL_AI");
		if (!((game != null && game.IsStorySession && (abstractRoom.name == "SS_AI" || abstractRoom.name == "SL_AI")) || flag))
		{
			return;
		}
		if (ModManager.MSC && world.name == "HR")
		{
			if (game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles && !game.GetStorySession.saveState.miscWorldSaveData.hrMelted)
			{
				oracleWantToSpawn = Oracle.OracleID.SS;
				Oracle obj = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
				AddObject(obj);
			}
			if (game.GetStorySession.saveState.deathPersistentSaveData.ripMoon && !game.GetStorySession.saveState.miscWorldSaveData.hrMelted)
			{
				oracleWantToSpawn = MoreSlugcatsEnums.OracleID.DM;
				Oracle obj2 = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
				AddObject(obj2);
			}
		}
		else
		{
			Oracle obj3 = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
			AddObject(obj3);
		}
		waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
	}

	public void AddObject(UpdatableAndDeletable obj)
	{
		if (game == null)
		{
			return;
		}
		if (obj is DeafLoopHolder)
		{
			for (int i = 0; i < updateList.Count; i++)
			{
				if (updateList[i] is DeafLoopHolder)
				{
					(updateList[i] as DeafLoopHolder).muted = true;
				}
			}
		}
		updateList.Add(obj);
		obj.room = this;
		IDrawable drawable = null;
		if (obj is IDrawable)
		{
			drawable = obj as IDrawable;
		}
		if (obj is LightSource)
		{
			LightSource lightSource = obj as LightSource;
			if (ModManager.MMF && lightSource.noGameplayImpact)
			{
				cosmeticLightSources.Add(lightSource);
			}
			else
			{
				lightSources.Add(lightSource);
			}
		}
		if (obj is IAccessibilityModifier)
		{
			accessModifiers.Add(obj as IAccessibilityModifier);
		}
		if (obj is VisionObscurer)
		{
			visionObscurers.Add(obj as VisionObscurer);
		}
		if (obj is ZapCoil)
		{
			zapCoils.Add(obj as ZapCoil);
		}
		if (obj is LightningMachine)
		{
			lightningMachines.Add(obj as LightningMachine);
		}
		if (obj is EnergySwirl)
		{
			energySwirls.Add(obj as EnergySwirl);
		}
		if (obj is SnowSource)
		{
			snowSources.Add(obj as SnowSource);
			AddSnow();
		}
		if (ModManager.MSC && obj is OEsphere)
		{
			oeSpheres.Add(obj as OEsphere);
		}
		if (obj is CellDistortion)
		{
			cellDistortions.Add(obj as CellDistortion);
		}
		if (obj is LocalBlizzard)
		{
			localBlizzards.Add(obj as LocalBlizzard);
		}
		if (ModManager.MSC && obj is IProvideWarmth)
		{
			blizzardHeatSources.Add(obj as IProvideWarmth);
		}
		if (obj is PhysicalObject)
		{
			physicalObjects[(obj as PhysicalObject).collisionLayer].Add(obj as PhysicalObject);
			if (obj is OracleSwarmer)
			{
				SwarmerCount++;
			}
			if ((obj as PhysicalObject).graphicsModule != null)
			{
				drawable = (obj as PhysicalObject).graphicsModule;
			}
			else if (BeingViewed)
			{
				(obj as PhysicalObject).InitiateGraphicsModule();
				if ((obj as PhysicalObject).graphicsModule != null)
				{
					drawable = (obj as PhysicalObject).graphicsModule;
				}
			}
		}
		if (drawable == null)
		{
			return;
		}
		drawableObjects.Add(drawable);
		for (int j = 0; j < game.cameras.Length; j++)
		{
			if (game.cameras[j].room == this)
			{
				game.cameras[j].NewObjectInRoom(drawable);
			}
		}
	}

	public void DestroyObject(EntityID ID)
	{
		for (int i = 0; i < updateList.Count; i++)
		{
			if (updateList[i] is PhysicalObject && (updateList[i] as PhysicalObject).abstractPhysicalObject.ID == ID)
			{
				updateList[i].Destroy();
				break;
			}
		}
	}

	public void RemoveObject(UpdatableAndDeletable obj)
	{
		if (obj.room == this)
		{
			obj.RemoveFromRoom();
		}
		if (updateList.IndexOf(obj) > updateIndex)
		{
			CleanOutObjectNotInThisRoom(obj);
		}
	}

	private void CleanOutObjectNotInThisRoom(UpdatableAndDeletable obj)
	{
		updateList.Remove(obj);
		if (obj is IDrawable)
		{
			drawableObjects.Remove(obj as IDrawable);
		}
		if (obj is LightSource)
		{
			LightSource lightSource = obj as LightSource;
			if (ModManager.MMF && lightSource.noGameplayImpact)
			{
				cosmeticLightSources.Remove(lightSource);
			}
			else
			{
				lightSources.Remove(lightSource);
			}
		}
		if (obj is IAccessibilityModifier)
		{
			accessModifiers.Remove(obj as IAccessibilityModifier);
		}
		if (obj is VisionObscurer)
		{
			visionObscurers.Remove(obj as VisionObscurer);
		}
		if (ModManager.MSC && obj is IProvideWarmth)
		{
			blizzardHeatSources.Remove(obj as IProvideWarmth);
		}
		if (obj is ZapCoil)
		{
			zapCoils.Remove(obj as ZapCoil);
		}
		if (obj is PhysicalObject)
		{
			physicalObjects[(obj as PhysicalObject).collisionLayer].Remove(obj as PhysicalObject);
			if (obj is OracleSwarmer)
			{
				SwarmerCount--;
			}
			if ((obj as PhysicalObject).graphicsModule != null)
			{
				drawableObjects.Remove((obj as PhysicalObject).graphicsModule);
			}
			if (obj.slatedForDeletetion || obj.room != null)
			{
				for (int i = 0; i < abstractRoom.entities.Count; i++)
				{
					if ((obj as PhysicalObject).abstractPhysicalObject == abstractRoom.entities[i])
					{
						abstractRoom.entities[i].Destroy();
						abstractRoom.entities.RemoveAt(i);
						break;
					}
				}
				if (obj is Creature)
				{
					for (int j = 0; j < abstractRoom.creatures.Count; j++)
					{
						if ((obj as Creature).abstractCreature == abstractRoom.creatures[j])
						{
							abstractRoom.creatures.RemoveAt(j);
							break;
						}
					}
				}
			}
		}
		RemoveObject(obj);
	}

	public void ChangeCollisionLayerForObject(PhysicalObject obj, int newLayer)
	{
		int collisionLayer = obj.collisionLayer;
		if (collisionLayer != newLayer)
		{
			physicalObjects[collisionLayer].Remove(obj);
			physicalObjects[newLayer].Add(obj);
			obj.collisionLayer = newLayer;
		}
	}

	public void Update()
	{
		if (game == null)
		{
			return;
		}
		if (snowSources.Count == 0)
		{
			snow = false;
		}
		if (waitToEnterAfterFullyLoaded > 0 && fullyLoaded)
		{
			waitToEnterAfterFullyLoaded--;
		}
		lastBackgroundNoise = backgroundNoise;
		backgroundNoise = Mathf.Max(0f, backgroundNoise - 0.05f);
		if (game.pauseUpdate)
		{
			backgroundNoise = Mathf.Lerp(backgroundNoise, 0f, 0.05f);
		}
		if (aidataprepro != null)
		{
			aidataprepro.Update();
		}
		if (waterObject != null)
		{
			if (defaultWaterLevel == -2000 && waterObject.fWaterLevel < -400f && waterObject.fWaterLevel > -2000f)
			{
				waterObject.fWaterLevel = -2000f;
			}
			waterObject.Update();
		}
		socialEventRecognizer.Update();
		if (ModManager.MSC && game.IsStorySession && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && !abstractRoom.gate && !abstractRoom.shelter && world.region != null && world.region.name == "DS" && syncTicker % 200 == 199)
		{
			int num = 0;
			for (int i = 0; i < physicalObjects.Length; i++)
			{
				for (int j = 0; j < physicalObjects[i].Count; j++)
				{
					if (physicalObjects[i][j] is Snail)
					{
						num++;
					}
				}
			}
			if (num < 10)
			{
				int k = 0;
				IntVector2 intVector = new IntVector2(0, 0);
				for (; k < 100; k++)
				{
					int num2 = UnityEngine.Random.Range(1, Tiles.GetLength(0) - 1);
					int num3 = UnityEngine.Random.Range(1, Tiles.GetLength(1) - 1);
					if (!Tiles[num2, num3].Solid)
					{
						intVector = new IntVector2(num2, num3);
						break;
					}
				}
				if (intVector.x != 0 || intVector.y != 0)
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Snail), null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), game.GetNewID());
					abstractCreature.saveCreature = false;
					abstractRoom.AddEntity(abstractCreature);
					abstractCreature.RealizeInRoom();
					AddObject(new ShockWave(new Vector2((float)intVector.x * 20f, (float)intVector.y * 20f), 300f, 0.2f, 15));
				}
			}
		}
		darkenLightsFactor = roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DarkenLights);
		if (DustStormIntensity > 0f)
		{
			float dustStormIntensity = DustStormIntensity;
			Shader.SetGlobalFloat(RainWorld.ShadPropDustWaveProgress, dustStormIntensity);
			dustStormIntensity = Mathf.InverseLerp(0.4f, 0.2f, dustStormIntensity);
			roomSettings.Clouds = cloudsNdarken.x + dustStormIntensity * (1f - cloudsNdarken.x);
			darkenLightsFactor = cloudsNdarken.y + dustStormIntensity * (1f - cloudsNdarken.y);
		}
		else
		{
			cloudsNdarken.x = roomSettings.Clouds;
			cloudsNdarken.y = darkenLightsFactor;
		}
		syncTicker++;
		if (ModManager.MSC && game.IsStorySession && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && world.region != null && world.region.name == "VS" && !abstractRoom.name.ToLower().Contains("basement"))
		{
			if (roomSettings.Grime > 0.25f)
			{
				roomSettings.Grime = 0.25f;
				Shader.SetGlobalFloat(RainWorld.ShadPropGrime, roomSettings.Grime);
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Hue, 0f, inherited: false));
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Brightness) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Brightness, 1f, inherited: false));
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 1f, inherited: false));
			}
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue).amount = (float)(syncTicker % 400) / 400f;
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = 1f;
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Brightness).amount = 1f;
		}
		if (!game.pauseUpdate && BeingViewed && !abstractRoom.shelter && roomSettings.DangerType != RoomRain.DangerType.None && ceilingTiles.Length != 0 && UnityEngine.Random.value < gravity && (double)UnityEngine.Random.value > Math.Pow(1f - Mathf.Max((1f - world.rainCycle.CycleStartUp) * Mathf.InverseLerp(0f, 0.5f, roomSettings.CeilingDrips), Mathf.Pow(roomSettings.CeilingDrips, 7f)) * 0.05f, ceilingTiles.Length))
		{
			AddObject(new WaterDrip(MiddleOfTile(ceilingTiles[UnityEngine.Random.Range(0, ceilingTiles.Length)]) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 9f), new Vector2(0f, 0f), waterColor: false));
		}
		lastWaterGlitterCycle = waterGlitterCycle;
		waterGlitterCycle -= 1f / 60f;
		if (shortcutsBlinking != null && !game.pauseUpdate)
		{
			for (int l = 0; l < shortcutsBlinking.GetLength(0); l++)
			{
				shortcutsBlinking[l, 0] = Mathf.Clamp(shortcutsBlinking[l, 0] - 1f / ((shortcutsBlinking[l, 1] > 0f) ? Mathf.Lerp(20f, shortcuts[l].length, 0.3f) : 10f), 0f, 1f);
				if (shortcutsBlinking[l, 1] > 0f)
				{
					shortcutsBlinking[l, 1] += 1f / Mathf.Lerp(20f, shortcuts[l].length, 0.3f);
					if (shortcutsBlinking[l, 1] > 1f)
					{
						shortcutsBlinking[l, 1] = 0f;
					}
					shortcutsBlinking[l, 2] = 0f;
				}
				if (shortcutsBlinking[l, 3] == 0f)
				{
					shortcutsBlinking[l, 2] = Mathf.Lerp(shortcutsBlinking[l, 2], 1f, 0.04f);
					if (UnityEngine.Random.value < 0.01f)
					{
						shortcutsBlinking[l, 3] = UnityEngine.Random.value * 20f;
					}
				}
				else if (shortcutsBlinking[l, 3] < 0f)
				{
					shortcutsBlinking[l, 3] = Mathf.Min(shortcutsBlinking[l, 3] + 1f, 0f);
				}
				else
				{
					shortcutsBlinking[l, 3] = Mathf.Max(shortcutsBlinking[l, 3] - 1f, 0f);
					shortcutsBlinking[l, 2] = Mathf.Lerp(shortcutsBlinking[l, 2], UnityEngine.Random.value, Mathf.Lerp(0f, 0.5f, UnityEngine.Random.value));
				}
			}
		}
		if (fliesRoomAi != null)
		{
			fliesRoomAi.Update(game.evenUpdate);
		}
		if (PERTILEVISALIZER != null)
		{
			PERTILEVISALIZER.Update(this);
		}
		if (!game.pauseUpdate)
		{
			abstractRoom.UpdateCreaturesInDens(1);
		}
		for (updateIndex = updateList.Count - 1; updateIndex >= 0; updateIndex--)
		{
			UpdatableAndDeletable updatableAndDeletable = updateList[updateIndex];
			if (updatableAndDeletable.slatedForDeletetion || updatableAndDeletable.room != this)
			{
				CleanOutObjectNotInThisRoom(updatableAndDeletable);
			}
			else
			{
				bool flag = ShouldBeDeferred(updatableAndDeletable);
				if ((!game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag)
				{
					updatableAndDeletable.Update(game.evenUpdate);
				}
				if (updatableAndDeletable.slatedForDeletetion || updatableAndDeletable.room != this)
				{
					CleanOutObjectNotInThisRoom(updatableAndDeletable);
				}
				else if (updatableAndDeletable is PhysicalObject && !flag)
				{
					if ((updatableAndDeletable as PhysicalObject).graphicsModule != null)
					{
						(updatableAndDeletable as PhysicalObject).graphicsModule.Update();
						(updatableAndDeletable as PhysicalObject).GraphicsModuleUpdated(actuallyViewed: true, game.evenUpdate);
					}
					else
					{
						(updatableAndDeletable as PhysicalObject).GraphicsModuleUpdated(actuallyViewed: false, game.evenUpdate);
					}
				}
			}
		}
		if (ModManager.MSC && roomSettings.GetEffect(MoreSlugcatsEnums.RoomEffectType.RoomWrap) != null)
		{
			foreach (AbstractCreature player2 in game.Players)
			{
				if (player2.realizedCreature == null || player2.realizedCreature.room != this)
				{
					continue;
				}
				Player player = player2.realizedCreature as Player;
				if (player.mainBodyChunk.pos.x < -228f)
				{
					player.SuperHardSetPosition(new Vector2(RoomRect.right + 212f, player.mainBodyChunk.pos.y));
				}
				if (player.mainBodyChunk.pos.x > RoomRect.right + 228f)
				{
					player.SuperHardSetPosition(new Vector2(-212f, player.mainBodyChunk.pos.y));
				}
				if (player.mainBodyChunk.pos.y > RoomRect.top + 48f)
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, RoomRect.bottom - 72f));
				}
				if (player.mainBodyChunk.pos.y < RoomRect.bottom - 96f)
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, RoomRect.top + 32f));
					for (int m = 0; m < player.bodyChunks.Length; m++)
					{
						player.bodyChunks[m].vel.y = Mathf.Clamp(player.bodyChunks[m].vel.y, -15f, 15f);
					}
				}
			}
		}
		updateIndex = int.MaxValue;
		if (chunkGlue != null)
		{
			foreach (ChunkGlue item in chunkGlue)
			{
				item.moveChunk.pos = item.otherChunk.pos + item.relativePos;
			}
		}
		chunkGlue = null;
		for (int n = 1; n < physicalObjects.Length; n++)
		{
			for (int num4 = 0; num4 < physicalObjects[n].Count; num4++)
			{
				for (int num5 = num4 + 1; num5 < physicalObjects[n].Count; num5++)
				{
					if (!(Mathf.Abs(physicalObjects[n][num4].bodyChunks[0].pos.x - physicalObjects[n][num5].bodyChunks[0].pos.x) < physicalObjects[n][num4].collisionRange + physicalObjects[n][num5].collisionRange) || !(Mathf.Abs(physicalObjects[n][num4].bodyChunks[0].pos.y - physicalObjects[n][num5].bodyChunks[0].pos.y) < physicalObjects[n][num4].collisionRange + physicalObjects[n][num5].collisionRange))
					{
						continue;
					}
					bool flag2 = false;
					bool flag3 = false;
					if (physicalObjects[n][num4] is Creature && (physicalObjects[n][num4] as Creature).Template.grasps > 0)
					{
						Creature.Grasp[] grasps = (physicalObjects[n][num4] as Creature).grasps;
						foreach (Creature.Grasp grasp in grasps)
						{
							if (grasp != null && grasp.grabbed == physicalObjects[n][num5])
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3 && physicalObjects[n][num5] is Creature && (physicalObjects[n][num5] as Creature).Template.grasps > 0)
					{
						Creature.Grasp[] grasps = (physicalObjects[n][num5] as Creature).grasps;
						foreach (Creature.Grasp grasp2 in grasps)
						{
							if (grasp2 != null && grasp2.grabbed == physicalObjects[n][num4])
							{
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						continue;
					}
					for (int num7 = 0; num7 < physicalObjects[n][num4].bodyChunks.Length; num7++)
					{
						for (int num8 = 0; num8 < physicalObjects[n][num5].bodyChunks.Length; num8++)
						{
							if (physicalObjects[n][num4].bodyChunks[num7].collideWithObjects && physicalObjects[n][num5].bodyChunks[num8].collideWithObjects && Custom.DistLess(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos, physicalObjects[n][num4].bodyChunks[num7].rad + physicalObjects[n][num5].bodyChunks[num8].rad))
							{
								float num9 = physicalObjects[n][num4].bodyChunks[num7].rad + physicalObjects[n][num5].bodyChunks[num8].rad;
								float num10 = Vector2.Distance(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos);
								Vector2 vector = Custom.DirVec(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos);
								float num11 = physicalObjects[n][num5].bodyChunks[num8].mass / (physicalObjects[n][num4].bodyChunks[num7].mass + physicalObjects[n][num5].bodyChunks[num8].mass);
								physicalObjects[n][num4].bodyChunks[num7].pos -= (num9 - num10) * vector * num11;
								physicalObjects[n][num4].bodyChunks[num7].vel -= (num9 - num10) * vector * num11;
								physicalObjects[n][num5].bodyChunks[num8].pos += (num9 - num10) * vector * (1f - num11);
								physicalObjects[n][num5].bodyChunks[num8].vel += (num9 - num10) * vector * (1f - num11);
								if (physicalObjects[n][num4].bodyChunks[num7].pos.x == physicalObjects[n][num5].bodyChunks[num8].pos.x)
								{
									physicalObjects[n][num4].bodyChunks[num7].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.0001f;
									physicalObjects[n][num5].bodyChunks[num8].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.0001f;
								}
								if (!flag2)
								{
									physicalObjects[n][num4].Collide(physicalObjects[n][num5], num7, num8);
									physicalObjects[n][num5].Collide(physicalObjects[n][num4], num8, num7);
								}
								flag2 = true;
							}
						}
					}
				}
			}
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
		{
			ExpeditionGame.ExSpawn(this);
		}
	}

	public bool ReallyTrulyRealizedInRoom(AbstractPhysicalObject obj)
	{
		if (obj.realizedObject == null)
		{
			return false;
		}
		if (obj.realizedObject.room != this)
		{
			return false;
		}
		if (obj.InDen)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntVector2 GetTilePosition(Vector2 pos)
	{
		return StaticGetTilePosition(pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntVector2 StaticGetTilePosition(Vector2 pos)
	{
		return new IntVector2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 StaticGetTilePosition(float2 pos)
	{
		return new int2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
	}

	public WorldCoordinate GetWorldCoordinate(Vector2 pos)
	{
		return GetWorldCoordinate(GetTilePosition(pos));
	}

	public WorldCoordinate GetWorldCoordinate(IntVector2 pos)
	{
		return Custom.MakeWorldCoordinate(pos, abstractRoom.index);
	}

	public Vector2 MiddleOfTile(int x, int y)
	{
		return new Vector2(10f + (float)x * 20f, 10f + (float)y * 20f);
	}

	public Vector2 MiddleOfTile(IntVector2 pos)
	{
		return MiddleOfTile(pos.x, pos.y);
	}

	public Vector2 MiddleOfTile(WorldCoordinate coord)
	{
		return MiddleOfTile(coord.x, coord.y);
	}

	public Vector2 MiddleOfTile(Vector2 pos)
	{
		return MiddleOfTile(GetTilePosition(pos));
	}

	public FloatRect TileRect(IntVector2 pos)
	{
		return FloatRect.MakeFromVector2(MiddleOfTile(pos) - new Vector2(10f, 10f), MiddleOfTile(pos) + new Vector2(10f, 10f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(Vector2 pos)
	{
		return GetTile(GetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(IntVector2 pos)
	{
		return GetTile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(WorldCoordinate pos)
	{
		return GetTile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(int x, int y)
	{
		if (x < Width && y < Height && x > -1 && y > -1)
		{
			return Tiles[x, y];
		}
		if (DefaultTile != null)
		{
			return DefaultTile;
		}
		int num = Custom.IntClamp(x, 0, Width - 1);
		int num2 = Custom.IntClamp(y, 0, Height - 1);
		Tile tile = Tiles[num, num2];
		if (tile.Terrain != Tile.TerrainType.ShortcutEntrance && tile.shortCut == 0)
		{
			return tile;
		}
		return new Tile(tile.X, tile.Y, (tile.Terrain == Tile.TerrainType.ShortcutEntrance) ? Tile.TerrainType.Solid : tile.Terrain, tile.verticalBeam, tile.horizontalBeam, tile.wallbehind, 0, tile.waterInt);
	}

	public bool IsCornerFree(int x, int y, FloatRect.CornerLabel corner)
	{
		switch (corner)
		{
		case FloatRect.CornerLabel.A:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.B:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.C:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.D:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		default:
			return false;
		}
	}

	public bool IsCornerFree(int x, int y, int corner)
	{
		switch (corner)
		{
		case 0:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case 1:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case 2:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		case 3:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		default:
			return false;
		}
	}

	public int RayTraceTilesList(int x0, int y0, int x1, int y1, ref List<IntVector2> path)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		if (path == null)
		{
			path = new List<IntVector2>();
		}
		int num9 = 0;
		while (num5 > 0)
		{
			if (path.Count <= num9)
			{
				path.Add(new IntVector2(num3, num4));
			}
			else
			{
				path[num9] = new IntVector2(num3, num4);
			}
			num9++;
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return num9;
	}

	public bool RayTraceTilesForTerrain(int x0, int y0, int x1, int y1)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		while (num5 > 0)
		{
			if (GetTile(num3, num4).Solid)
			{
				return false;
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public bool IsPositionInsideBoundries(IntVector2 pos)
	{
		if (pos.x >= 0 && pos.x < TileWidth && pos.y >= 0)
		{
			return pos.y < TileHeight;
		}
		return false;
	}

	public IntVector2 ShorcutEntranceHoleDirection(IntVector2 pos)
	{
		IntVector2 result = new IntVector2(0, 0);
		for (int i = 0; i < 4; i++)
		{
			if (GetTile(pos.x + Custom.fourDirections[i].x, pos.y + Custom.fourDirections[i].y).Terrain != Tile.TerrainType.Solid)
			{
				result = new IntVector2(Custom.fourDirections[i].x, Custom.fourDirections[i].y);
				break;
			}
		}
		return result;
	}

	public AbstractRoom WhichRoomDoesThisExitLeadTo(IntVector2 exit)
	{
		int num = exitAndDenIndex.IndexfOf(exit);
		if (num > -1 && num < abstractRoom.connections.Length)
		{
			return world.GetAbstractRoom(abstractRoom.connections[num]);
		}
		return null;
	}

	public ShortcutData ShortcutLeadingToNode(int node)
	{
		for (int i = 0; i < shortcuts.Length; i++)
		{
			if (shortcuts[i].destNode == node)
			{
				return shortcuts[i];
			}
		}
		Custom.LogWarning("Shortcut leading to node error!", node.ToString());
		return shortcuts[0];
	}

	public bool VisualContact(WorldCoordinate a, WorldCoordinate b)
	{
		if (a.room != abstractRoom.index || b.room != abstractRoom.index)
		{
			return false;
		}
		return VisualContact(MiddleOfTile(a), MiddleOfTile(b));
	}

	public bool VisualContact(IntVector2 a, IntVector2 b)
	{
		return VisualContact(MiddleOfTile(a), MiddleOfTile(b));
	}

	public bool VisualContact(Vector2 a, Vector2 b)
	{
		if (GetTile(a).Solid || GetTile(b).Solid)
		{
			return false;
		}
		float num = Vector2.Distance(a, b);
		for (float num2 = 20f; num2 < num; num2 += 20f)
		{
			if (GetTile(Vector2.Lerp(a, b, num2 / num)).Solid)
			{
				return false;
			}
		}
		return true;
	}

	public SlopeDirection IdentifySlope(Vector2 pos)
	{
		return IdentifySlope(GetTilePosition(pos));
	}

	public SlopeDirection IdentifySlope(int X, int Y)
	{
		return IdentifySlope(new IntVector2(X, Y));
	}

	public SlopeDirection IdentifySlope(IntVector2 pos)
	{
		if (GetTile(pos.x, pos.y).Terrain == Tile.TerrainType.Slope)
		{
			if (GetTile(pos.x - 1, pos.y).Terrain == Tile.TerrainType.Solid)
			{
				if (GetTile(pos.x, pos.y - 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.UpRight;
				}
				if (GetTile(pos.x, pos.y + 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.DownRight;
				}
			}
			else if (GetTile(pos.x + 1, pos.y).Terrain == Tile.TerrainType.Solid)
			{
				if (GetTile(pos.x, pos.y - 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.UpLeft;
				}
				if (GetTile(pos.x, pos.y + 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.DownLeft;
				}
			}
		}
		return SlopeDirection.Broken;
	}

	public void LoadFromDataString(string[] lines)
	{
		RoomPreprocessor.VersionFix(ref lines);
		string[] array = lines[1].Split('*');
		if (lines[1].Split('|')[1] == "-1")
		{
			water = false;
			defaultWaterLevel = -1;
			floatWaterLevel = float.MinValue;
		}
		else
		{
			water = true;
			defaultWaterLevel = Convert.ToInt32(lines[1].Split('|')[1], CultureInfo.InvariantCulture);
			floatWaterLevel = MiddleOfTile(new IntVector2(0, defaultWaterLevel)).y;
			waterInFrontOfTerrain = Convert.ToInt32(lines[1].Split('|')[2], CultureInfo.InvariantCulture) == 1;
		}
		array = lines[1].Split('|')[0].Split('*');
		Width = Convert.ToInt32(array[0], CultureInfo.InvariantCulture);
		Height = Convert.ToInt32(array[1], CultureInfo.InvariantCulture);
		string[] array2 = lines[2].Split('|')[0].Split('*');
		lightAngle = new Vector2(Convert.ToSingle(array2[0], CultureInfo.InvariantCulture), Convert.ToSingle(array2[1], CultureInfo.InvariantCulture));
		string[] array3 = lines[3].Split('|');
		cameraPositions = new Vector2[array3.Length];
		for (int i = 0; i < array3.Length; i++)
		{
			cameraPositions[i] = new Vector2(Convert.ToSingle(array3[i].Split(',')[0], CultureInfo.InvariantCulture), 0f - (800f - (float)Height * 20f + Convert.ToSingle(array3[i].Split(',')[1])));
		}
		DefaultTile = null;
		if (world != null && game != null && abstractRoom.firstTimeRealized && (!game.IsArenaSession || game.GetArenaGameSession.GameTypeSetup.levelItems))
		{
			string[] array4 = lines[5].Split('|');
			for (int j = 0; j < array4.Length - 1; j++)
			{
				IntVector2 intVector = new IntVector2(Convert.ToInt32(array4[j].Split(',')[1], CultureInfo.InvariantCulture) - 1, Height - Convert.ToInt32(array4[j].Split(',')[2], CultureInfo.InvariantCulture));
				bool flag = true;
				if ((Convert.ToInt32(array4[j].Split(',')[0], CultureInfo.InvariantCulture) != 1) ? (UnityEngine.Random.value < 0.75f) : (UnityEngine.Random.value < 0.6f))
				{
					EntityID newID = game.GetNewID(-abstractRoom.index);
					switch (Convert.ToInt32(array4[j].Split(',')[0], CultureInfo.InvariantCulture))
					{
					case 0:
						abstractRoom.AddEntity(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID));
						break;
					case 1:
						abstractRoom.AddEntity(new AbstractSpear(world, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID, explosive: false));
						break;
					}
				}
			}
		}
		Tiles = new Tile[Width, Height];
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				Tiles[k, l] = new Tile(k, l, Tile.TerrainType.Air, vBeam: false, hBeam: false, wbhnd: false, 0, ((l <= defaultWaterLevel) ? 1 : 0) + ((l == defaultWaterLevel) ? 1 : 0));
			}
		}
		List<IntVector2> hiveTiles = new List<IntVector2>();
		List<IntVector2> list = new List<IntVector2>();
		List<int> hiveTileIndexes = new List<int>();
		int currentHiveTileIndex = -1;
		List<IntVector2> list2 = new List<IntVector2>();
		IntVector2 intVector2 = new IntVector2(0, Height - 1);
		List<IntVector2> list3 = new List<IntVector2>();
		string[] array5 = lines[11].Split('|');
		for (int m = 0; m < array5.Length - 1; m++)
		{
			string[] array6 = array5[m].Split(',');
			Tiles[intVector2.x, intVector2.y].Terrain = (Tile.TerrainType)int.Parse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			for (int n = 1; n < array6.Length; n++)
			{
				switch (array6[n])
				{
				case "1":
					Tiles[intVector2.x, intVector2.y].verticalBeam = true;
					break;
				case "2":
					Tiles[intVector2.x, intVector2.y].horizontalBeam = true;
					break;
				case "3":
					if (Tiles[intVector2.x, intVector2.y].shortCut < 1)
					{
						Tiles[intVector2.x, intVector2.y].shortCut = 1;
					}
					break;
				case "4":
					Tiles[intVector2.x, intVector2.y].shortCut = 2;
					break;
				case "5":
					Tiles[intVector2.x, intVector2.y].shortCut = 3;
					break;
				case "9":
					Tiles[intVector2.x, intVector2.y].shortCut = 4;
					break;
				case "12":
					Tiles[intVector2.x, intVector2.y].shortCut = 5;
					break;
				case "6":
					Tiles[intVector2.x, intVector2.y].wallbehind = true;
					break;
				case "7":
					AddHiveTile(ref hiveTiles, ref hiveTileIndexes, ref currentHiveTileIndex, intVector2);
					Tiles[intVector2.x, intVector2.y].hive = true;
					break;
				case "8":
					list2.Add(intVector2);
					break;
				case "10":
					list.Add(intVector2);
					break;
				case "11":
					list3.Add(intVector2);
					break;
				}
			}
			intVector2.y--;
			if (intVector2.y < 0)
			{
				intVector2.x++;
				intVector2.y = Height - 1;
			}
		}
		if (list.Count > 0)
		{
			garbageHoles = list.ToArray();
		}
		if (currentHiveTileIndex > -1)
		{
			hives = new IntVector2[currentHiveTileIndex + 1][];
			for (int num = 0; num <= currentHiveTileIndex; num++)
			{
				int num2 = 0;
				for (int num3 = 0; num3 < hiveTileIndexes.Count; num3++)
				{
					if (hiveTileIndexes[num3] == num)
					{
						num2++;
					}
				}
				hives[num] = new IntVector2[num2];
				int num4 = 0;
				for (int num5 = 0; num5 < hiveTileIndexes.Count; num5++)
				{
					if (hiveTileIndexes[num5] == num)
					{
						hives[num][num4] = hiveTiles[num5];
						num4++;
					}
				}
			}
		}
		else
		{
			hives = new IntVector2[0][];
		}
		List<WaterFall> list4 = new List<WaterFall>();
		while (list2.Count > 0)
		{
			IntVector2 intVector3 = list2[0];
			for (int num6 = 0; num6 < list2.Count; num6++)
			{
				intVector3 = list2[num6];
				if (!list2.Contains(intVector3 + new IntVector2(-1, 0)) && !list2.Contains(intVector3 + new IntVector2(0, 1)))
				{
					break;
				}
			}
			bool flag2 = list2.Contains(intVector3 + new IntVector2(0, -1));
			int num7 = 0;
			for (int num8 = intVector3.x; num8 < TileWidth && list2.Contains(new IntVector2(num8, intVector3.y)) && list2.Contains(new IntVector2(num8, intVector3.y - 1)) == flag2; num8++)
			{
				num7++;
				list2.Remove(new IntVector2(num8, intVector3.y));
				if (flag2)
				{
					list2.Remove(new IntVector2(num8, intVector3.y - 1));
				}
			}
			list4.Add(new WaterFall(this, intVector3, flag2 ? 1f : 0.5f, num7));
		}
		waterFalls = list4.ToArray();
		for (int num9 = 0; num9 < waterFalls.Length; num9++)
		{
			AddObject(waterFalls[num9]);
		}
		if (list3.Count > 0)
		{
			AddObject(new WormGrass(this, list3));
		}
		Loaded();
	}

	private static void AddHiveTile(ref List<IntVector2> hiveTiles, ref List<int> hiveTileIndexes, ref int currentHiveTileIndex, IntVector2 tile)
	{
		int num = -1;
		for (int i = 0; i < hiveTiles.Count; i++)
		{
			if (num != -1)
			{
				break;
			}
			for (int j = 0; j < 8; j++)
			{
				if (tile + Custom.eightDirections[j] == hiveTiles[i])
				{
					num = hiveTileIndexes[i];
					break;
				}
			}
		}
		if (num == -1)
		{
			currentHiveTileIndex++;
			num = currentHiveTileIndex;
		}
		hiveTiles.Add(tile);
		hiveTileIndexes.Add(num);
	}

	private void AddWaterfall(IntVector2 tile, ref List<IntVector2> usedWFtiles)
	{
	}

	public bool ViewedByAnyCamera(Rect rect, float margin)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this && game.cameras[i].RectCurrentlyVisible(rect, margin, widescreen: false))
			{
				return true;
			}
		}
		return false;
	}

	public bool ViewedByAnyCamera(Vector2 pos, float margin)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this && game.cameras[i].PositionCurrentlyVisible(pos, margin, widescreen: false))
			{
				return true;
			}
		}
		return false;
	}

	public void ScreenMovement(Vector2? pos, Vector2 bump, float shake)
	{
		if (ModManager.MMF && !MMF.cfgVanillaExploits.Value)
		{
			if (bump.magnitude > 50f)
			{
				bump = bump.normalized * 50f;
			}
			shake = Mathf.Clamp(shake, 0f, 20f);
		}
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].ScreenMovement(pos, bump, shake);
			}
		}
		if (!(bump.magnitude * 0.25f + shake * 3f > 2f) || ceilingTiles.Length == 0)
		{
			return;
		}
		for (int num = Math.Min(40, Mathf.RoundToInt(Mathf.Lerp(ceilingTiles.Length, 150f, 0.25f) / 100f * roomSettings.CeilingDrips * Mathf.Clamp(bump.magnitude * 0.25f + shake * 3f, 2f, 8f))); num > 0; num--)
		{
			Vector2 pos2 = MiddleOfTile(ceilingTiles[UnityEngine.Random.Range(0, ceilingTiles.Length)]) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 9f);
			if (ViewedByAnyCamera(pos2, 300f))
			{
				AddObject(new WaterDrip(pos2, new Vector2(Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value), 0f), waterColor: false));
			}
		}
	}

	public ShortcutData shortcutData(IntVector2 pos)
	{
		int num = -1;
		num = ((!(pos == default(IntVector2)) && shortcutsIndex != null) ? shortcutsIndex.IndexfOf(pos) : (-1));
		if (num < 0 || num > shortcuts.Length)
		{
			return new ShortcutData(this, ShortcutData.Type.DeadEnd, 0, pos, pos, -1, new IntVector2[0]);
		}
		return shortcuts[num];
	}

	public ShortcutData shortcutData(Vector2 pos)
	{
		return shortcutData(GetTilePosition(pos));
	}

	public WorldCoordinate ToWorldCoordinate(Vector2 vector2)
	{
		return ToWorldCoordinate(GetTilePosition(vector2));
	}

	public WorldCoordinate ToWorldCoordinate(IntVector2 intVector2)
	{
		return new WorldCoordinate(abstractRoom.index, intVector2.x, intVector2.y, -1);
	}

	public WorldCoordinate LocalCoordinateOfNode(int node)
	{
		if (node > -1 && node < abstractRoom.nodes.Length)
		{
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.Den || abstractRoom.nodes[node].type == AbstractRoomNode.Type.Exit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.RegionTransportation)
			{
				return ShortcutLeadingToNode(node).startCoord;
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.SideExit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.SkyExit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.SeaExit)
			{
				RoomBorderExit roomBorderExit = borderExits[node - exitAndDenIndex.Length];
				return GetWorldCoordinate(roomBorderExit.borderTiles[UnityEngine.Random.Range(0, roomBorderExit.borderTiles.Length)]);
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.BatHive)
			{
				int num = node - exitAndDenIndex.Length - borderExits.Length;
				return GetWorldCoordinate(hives[num][UnityEngine.Random.Range(0, hives[num].Length)]);
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.GarbageHoles)
			{
				return GetWorldCoordinate(garbageHoles[UnityEngine.Random.Range(0, garbageHoles.Length)]);
			}
		}
		Custom.LogWarning("Unidentified entrance node!", node.ToString(), "/", abstractRoom.nodes.Length.ToString());
		return new WorldCoordinate(-1, -1, -1, -1);
	}

	public float DarknessOfPoint(RoomCamera rCam, Vector2 pos)
	{
		float num = Mathf.InverseLerp(0.2f, 1f, Darkness(pos));
		if (num <= 0f)
		{
			return 0f;
		}
		return num * (1f - LightSourceExposure(pos));
	}

	public float LightSourceExposure(Vector2 pos)
	{
		float num = 0f;
		for (int i = 0; i < lightSources.Count; i++)
		{
			if (Custom.DistLess(pos, lightSources[i].Pos, lightSources[i].Rad))
			{
				num += Custom.SCurve(Mathf.InverseLerp(lightSources[i].Rad, 0f, Vector2.Distance(pos, lightSources[i].Pos)), 0.5f) * lightSources[i].Lightness;
			}
		}
		return Mathf.Clamp(num, 0f, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Color LightSourceColor(Vector2 pos)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < lightSources.Count; i++)
		{
			if (Custom.DistLess(pos, lightSources[i].Pos, lightSources[i].Rad))
			{
				float num4 = Custom.SCurve(Mathf.InverseLerp(lightSources[i].Rad, 0f, Vector2.Distance(pos, lightSources[i].Pos)), 0.5f) * lightSources[i].Lightness;
				num = Mathf.Max(num, lightSources[i].color.r * num4);
				num2 = Mathf.Max(num2, lightSources[i].color.g * num4);
				num3 = Mathf.Max(num3, lightSources[i].color.b * num4);
			}
		}
		return new Color(num, num2, num3);
	}

	public bool CompleteDarkness(Vector2 pos, float margin, float reqDarkness, bool checkForPlayers)
	{
		if (Darkness(pos) < reqDarkness)
		{
			return false;
		}
		if (checkForPlayers)
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature?.room == this && Custom.DistLess(pos, game.Players[i].realizedCreature.mainBodyChunk.pos, 60f + margin))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < lightSources.Count; j++)
		{
			if (Custom.DistLess(pos, lightSources[j].Pos, lightSources[j].Rad + margin))
			{
				return false;
			}
		}
		return true;
	}

	public float WaterShinyness(Vector2 pos, float timeStacker)
	{
		float num = pos.x - pos.y;
		return Mathf.Pow(0.5f + Mathf.Sin((0f - Mathf.Lerp(lastWaterGlitterCycle, waterGlitterCycle, timeStacker) + num / 800f) * (float)Math.PI * 2f) * 0.5f, 7f);
	}

	private void PlaceQuantifiedCreaturesInRoom(CreatureTemplate.Type critType)
	{
		if (game == null)
		{
			return;
		}
		CreatureSpecificAImap creatureSpecificAImap = aimap.CreatureSpecificAImap(StaticWorld.GetCreatureTemplate(critType));
		for (int i = 0; i < abstractRoom.nodes.Length; i++)
		{
			int num = abstractRoom.NumberOfQuantifiedCreatureInNode(critType, i);
			if (world.rainCycle.CycleStartUp < 1f && critType != CreatureTemplate.Type.Fly && abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
			{
				num = (int)(world.rainCycle.CycleStartUp * (float)num);
				for (int j = 0; j < abstractRoom.NumberOfQuantifiedCreatureInNode(critType, i) - num; j++)
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, new WorldCoordinate(abstractRoom.index, -1, -1, i), game.GetNewID());
					abstractRoom.entitiesInDens.Add(abstractCreature);
					abstractCreature.remainInDenCounter = UnityEngine.Random.Range(0, game.IsStorySession ? ((int)(4800f * (1f - world.rainCycle.CycleStartUp))) : 400);
				}
			}
			List<IntVector2> list = new List<IntVector2>();
			List<IntVector2> list2 = new List<IntVector2>();
			for (int k = 0; k < creatureSpecificAImap.accessableTiles.Length; k++)
			{
				if (creatureSpecificAImap.GetDistanceToExit(creatureSpecificAImap.accessableTiles[k].x, creatureSpecificAImap.accessableTiles[k].y, i) > 0)
				{
					list2.Add(creatureSpecificAImap.accessableTiles[k]);
				}
			}
			if (list2.Count == 0)
			{
				list2.Add(LocalCoordinateOfNode(i).Tile);
			}
			for (int l = 0; l < num; l++)
			{
				list.Add(list2[UnityEngine.Random.Range(0, list2.Count)]);
			}
			for (int m = 0; m < list.Count; m++)
			{
				for (int n = 0; n < 20; n++)
				{
					IntVector2 intVector = list2[UnityEngine.Random.Range(0, list2.Count)];
					if (PlaceQCScore(intVector, creatureSpecificAImap, i, critType, list) > PlaceQCScore(list[m], creatureSpecificAImap, i, critType, list))
					{
						list[m] = intVector;
					}
				}
			}
			foreach (IntVector2 item in list)
			{
				AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, GetWorldCoordinate(item), game.GetNewID());
				abstractRoom.AddEntity(abstractCreature2);
				abstractCreature2.RealizeInRoom();
				if (critType == CreatureTemplate.Type.Spider && abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
				{
					(abstractCreature2.realizedCreature as Spider).denPos = new WorldCoordinate(abstractRoom.index, -1, -1, i);
				}
			}
		}
	}

	private float PlaceQCScore(IntVector2 pos, CreatureSpecificAImap critMap, int node, CreatureTemplate.Type critType, List<IntVector2> QCPositions)
	{
		float num = 0f;
		if (critType == CreatureTemplate.Type.Fly)
		{
			num += (float)aimap.getAItile(pos).visibility;
			num += (float)aimap.getTerrainProximity(pos) * 0.2f;
			num += (float)aimap.getAItile(pos).floorAltitude * 0.1f;
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				num /= (float)(critMap.GetDistanceToExit(pos.x, pos.y, i) + 10);
			}
			num /= Mathf.Lerp(critMap.GetDistanceToExit(pos.x, pos.y, node) + 10, 1f, 0.95f);
		}
		else if (critType == CreatureTemplate.Type.Leech)
		{
			num += (float)aimap.getAItile(pos).visibility;
			num += (float)aimap.getTerrainProximity(pos) * 0.2f;
		}
		else if (critType == CreatureTemplate.Type.SeaLeech)
		{
			num -= (float)Math.Abs(ShortcutLeadingToNode(node).StartTile.x - pos.x) * 10f;
		}
		else if (critType == CreatureTemplate.Type.Spider)
		{
			num += 1000f - Mathf.Abs(600f - (float)aimap.getAItile(pos).visibility) / 250f;
			num /= Mathf.Lerp(critMap.GetDistanceToExit(pos.x, pos.y, node) + 10, 200f, 0.999f);
		}
		return num;
	}

	public int CameraViewingNode(int node)
	{
		Vector2 a = MiddleOfTile(exitAndDenIndex[node]);
		int num = -1;
		float num2 = float.MaxValue;
		num = -1;
		for (int i = 0; i < cameraPositions.Length; i++)
		{
			if (Vector2.Distance(a, cameraPositions[i] + new Vector2(700f, 402f)) < num2)
			{
				num2 = Vector2.Distance(a, cameraPositions[i] + new Vector2(700f, 402f));
				num = i;
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		return num;
	}

	public int CameraViewingPoint(Vector2 p)
	{
		for (int i = 0; i < cameraPositions.Length; i++)
		{
			if (p.x > cameraPositions[i].x && p.x < cameraPositions[i].x + 1366f && p.y > cameraPositions[i].y && p.y < cameraPositions[i].y + 768f)
			{
				return i;
			}
		}
		return -1;
	}

	public float Darkness(Vector2 pos)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				return game.cameras[i].PaletteDarkness();
			}
		}
		return 0f;
	}

	public void PlaySound(SoundID soundId, float pan, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, pan, vol, pitch);
			}
		}
	}

	public void PlaySound(SoundID soundId, Vector2 pos)
	{
		PlaySound(soundId, pos, 1f, 1f);
	}

	public void PlaySound(SoundID soundId, Vector2 pos, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, pos, vol, pitch);
			}
		}
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk)
	{
		return PlaySound(soundId, chunk, loop: false, 1f, 1f);
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch)
	{
		return PlaySound(soundId, chunk, loop, vol, pitch, randomStartPosition: false);
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch, bool randomStartPosition)
	{
		ChunkSoundEmitter chunkSoundEmitter = new ChunkSoundEmitter(chunk, vol, pitch);
		PlaySound(soundId, chunkSoundEmitter, loop, vol, pitch, randomStartPosition);
		return chunkSoundEmitter;
	}

	public DisembodiedLoopEmitter PlayDisembodiedLoop(SoundID soundID, float vol, float pitch, float pan)
	{
		DisembodiedLoopEmitter disembodiedLoopEmitter = new DisembodiedLoopEmitter(vol, pitch, pan);
		AddObject(disembodiedLoopEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayDisembodiedLoop(soundID, disembodiedLoopEmitter, pan, vol, pitch);
			}
		}
		return disembodiedLoopEmitter;
	}

	public RectSoundEmitter PlayRectSound(SoundID soundID, FloatRect rect, bool loop, float vol, float pitch)
	{
		RectSoundEmitter rectSoundEmitter = new RectSoundEmitter(rect, vol, pitch);
		PlaySound(soundID, rectSoundEmitter, loop, vol, pitch, randomStartPosition: false);
		return rectSoundEmitter;
	}

	public void PlaySound(SoundID soundId, PositionedSoundEmitter em, bool loop, float vol, float pitch, bool randomStartPosition)
	{
		AddObject(em);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, em, loop, vol, pitch, randomStartPosition);
			}
		}
	}

	public void InGameNoise(InGameNoise noise)
	{
		if (noise == default(InGameNoise))
		{
			return;
		}
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			if (abstractRoom.creatures[i].realizedCreature != null && abstractRoom.creatures[i].realizedCreature.room == this)
			{
				abstractRoom.creatures[i].realizedCreature.HeardNoise(noise);
			}
		}
		for (int j = 0; j < updateList.Count; j++)
		{
			if (updateList[j] != null && updateList[j] is IReactToNoises)
			{
				(updateList[j] as IReactToNoises).NoiseInRoom(noise);
			}
		}
	}

	public void NewMessageInRoom(string text, int extraLinger)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].hud != null && game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && game.cameras[i].followAbstractCreature.Room == abstractRoom)
			{
				if (game.cameras[i].hud.dialogBox == null)
				{
					game.cameras[i].hud.InitDialogBox();
				}
				game.cameras[i].hud.dialogBox.NewMessage(text, extraLinger);
			}
		}
	}

	public void NewMessageInRoom(string text, float xOrientation, float yPos, int extraLinger)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].hud != null && game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && game.cameras[i].followAbstractCreature.Room == abstractRoom)
			{
				if (game.cameras[i].hud.dialogBox == null)
				{
					game.cameras[i].hud.InitDialogBox();
				}
				game.cameras[i].hud.dialogBox.NewMessage(text, xOrientation, yPos, extraLinger);
			}
		}
	}

	public void TriggerCombatArena()
	{
		if (abstractRoom.isBattleArena)
		{
			Custom.Log("TRIGGLERED BATTLE ROOM", abstractRoom.name);
			abstractRoom.isBattleArena = false;
			abstractRoom.battleArenaTriggeredTime = 5;
		}
	}

	public void SpawnMultiplayerItem(PlacedObject placedObj)
	{
		if (!(UnityEngine.Random.value > (placedObj.data as PlacedObject.MultiplayerItemData).chance))
		{
			AbstractPhysicalObject.AbstractObjectType abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Rock;
			bool explosive = false;
			if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Spear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.ExplosiveSpear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
				explosive = true;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Bomb)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.ScavengerBomb;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.SporePlant)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.SporePlant;
			}
			if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Spear)
			{
				AbstractSpear item = new AbstractSpear(world, null, GetWorldCoordinate(placedObj.pos), game.GetNewID(), explosive);
				abstractRoom.entities.Add(item);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Rock)
			{
				AbstractPhysicalObject item2 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, GetWorldCoordinate(placedObj.pos), game.GetNewID());
				abstractRoom.entities.Add(item2);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
			{
				AbstractPhysicalObject item3 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, GetWorldCoordinate(placedObj.pos), game.GetNewID());
				abstractRoom.entities.Add(item3);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
			{
				AbstractPhysicalObject item4 = new SporePlant.AbstractSporePlant(world, null, GetWorldCoordinate(placedObj.pos), game.GetNewID(), -2, -2, null, used: false, pacified: true);
				abstractRoom.entities.Add(item4);
			}
		}
	}

	public bool ShouldBeDeferred(UpdatableAndDeletable obj)
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (deferred && obj is PhysicalObject && !(obj is DangleFruit) && !(obj is Vulture))
		{
			AbstractCreature firstAlivePlayer = game.FirstAlivePlayer;
			PhysicalObject physicalObject = obj as PhysicalObject;
			if (physicalObject.grabbedBy.Count > 0)
			{
				for (int i = 0; i < physicalObject.grabbedBy.Count; i++)
				{
					if (ShouldBeDeferred(physicalObject.grabbedBy[i]?.grabber))
					{
						return true;
					}
				}
			}
			else if (game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && !firstAlivePlayer.realizedCreature.inShortcut && (Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.x - physicalObject.firstChunk.pos.x) >= 2049f || Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.y - physicalObject.firstChunk.pos.y) >= 1152f))
			{
				return true;
			}
		}
		return false;
	}

	public bool PointDeferred(Vector2 pos)
	{
		if (!ModManager.MSC || !deferred)
		{
			return false;
		}
		AbstractCreature firstAlivePlayer = game.FirstAlivePlayer;
		if (game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && !firstAlivePlayer.realizedCreature.inShortcut)
		{
			if (!(Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.x - pos.x) >= 2049f))
			{
				return Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.y - pos.y) >= 1152f;
			}
			return true;
		}
		return false;
	}

	public void PausedUpdate()
	{
		for (int i = 0; i < updateList.Count; i++)
		{
			updateList[i].PausedUpdate();
		}
	}

	public void PlayCustomSound(string soundName, Vector2 pos, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomSound(soundName, pos, vol, pitch);
			}
		}
	}

	public void PlayCustomSoundDisembodied(string soundName, float pan, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomSoundDisembodied(soundName, pan, vol, pitch);
			}
		}
	}

	public DisembodiedLoopEmitter PlayCustomDisembodiedLoop(string soundName, float pan, float vol, float pitch)
	{
		DisembodiedLoopEmitter disembodiedLoopEmitter = new DisembodiedLoopEmitter(vol, pitch, pan);
		AddObject(disembodiedLoopEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomDisembodiedLoop(soundName, disembodiedLoopEmitter, pan, vol, pitch);
			}
		}
		return disembodiedLoopEmitter;
	}

	public ChunkSoundEmitter PlayCustomChunkSound(string soundName, BodyChunk chunk, float vol, float pitch)
	{
		ChunkSoundEmitter chunkSoundEmitter = new ChunkSoundEmitter(chunk, vol, pitch);
		AddObject(chunkSoundEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomPositionedSound(soundName, chunkSoundEmitter, vol, pitch, loop: false, randomPosition: false);
			}
		}
		return chunkSoundEmitter;
	}

	public void SetLightSourceBlink(LightSource lightSource, int ind)
	{
		lightSource.setBlinkProperties((roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).blinkType, (roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).blinkRate);
		lightSource.nightLight = (roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).nightLight;
		if (lightSource.nightLight)
		{
			lightSource.nightFade = 0f;
		}
	}

	public void SetLightBeamBlink(LightBeam lightBeam, int ind)
	{
		lightBeam.SetBlinkProperties((roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).blinkType, (roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).blinkRate);
		lightBeam.nightLight = (roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).nightLight;
		if (lightBeam.nightLight)
		{
			lightBeam.nightFade = 0f;
		}
	}

	public bool IsGateRoom()
	{
		if (abstractRoom.gate)
		{
			return true;
		}
		return roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f;
	}

	public void AddSnow()
	{
		if (snowObject == null)
		{
			snowObject = new Snow(this);
			drawableObjects.Add(snowObject);
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					game.cameras[i].NewObjectInRoom(snowObject);
				}
			}
		}
		snow = true;
	}

	public void SlugcatGamemodeUniqueRoomSettings(RainWorldGame game)
	{
		if (ModManager.MSC && game != null)
		{
			if (game.IsStorySession && game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				roomSettings.wetTerrain = false;
				roomSettings.CeilingDrips = 0f;
				roomSettings.WaveAmplitude = 0.0001f;
				roomSettings.WaveLength = 1f;
				roomSettings.WaveSpeed = 0.51f;
				roomSettings.SecondWaveAmplitude = 0.0001f;
				roomSettings.SecondWaveLength = 1f;
			}
			if (game.rainWorld.safariMode)
			{
				roomSettings.roomSpecificScript = false;
			}
		}
	}

	public float WaterLevelDisplacement(Vector2 pos)
	{
		if (waterInverted)
		{
			return 0f - (FloatWaterLevel(pos.x) - pos.y);
		}
		return FloatWaterLevel(pos.x) - pos.y;
	}

	public bool PointSubmerged(Vector2 pos, float yDisplacement)
	{
		if (ModManager.MSC && waterInverted)
		{
			if (waterObject != null)
			{
				return pos.y > waterObject.DetailedWaterLevel(pos.x) + yDisplacement;
			}
			return pos.y > floatWaterLevel + yDisplacement;
		}
		if (waterObject != null)
		{
			return pos.y < waterObject.DetailedWaterLevel(pos.x) - yDisplacement;
		}
		return pos.y < floatWaterLevel - yDisplacement;
	}

	public bool HasAnySolidTileInXRange(int y, int xA, int xB)
	{
		if (xA > -1 && xA < Width && xB > -1 && xB < Width && y > -1 && y < Height)
		{
			for (int i = xA; i <= xB; i++)
			{
				if (Tiles[i, y].IsSolid())
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = xA; j <= xB; j++)
			{
				if (GetTile(j, y).IsSolid())
				{
					return true;
				}
			}
		}
		return false;
	}
}
