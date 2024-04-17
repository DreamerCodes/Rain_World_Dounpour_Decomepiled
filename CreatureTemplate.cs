using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using UnityEngine;

public class CreatureTemplate
{
	public class WaterRelationship : ExtEnum<WaterRelationship>
	{
		public static readonly WaterRelationship AirOnly = new WaterRelationship("AirOnly", register: true);

		public static readonly WaterRelationship AirAndSurface = new WaterRelationship("AirAndSurface", register: true);

		public static readonly WaterRelationship Amphibious = new WaterRelationship("Amphibious", register: true);

		public static readonly WaterRelationship WaterOnly = new WaterRelationship("WaterOnly", register: true);

		public WaterRelationship(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Type : ExtEnum<Type>
	{
		public static readonly Type StandardGroundCreature = new Type("StandardGroundCreature", register: true);

		public static readonly Type Slugcat = new Type("Slugcat", register: true);

		public static readonly Type LizardTemplate = new Type("LizardTemplate", register: true);

		public static readonly Type PinkLizard = new Type("PinkLizard", register: true);

		public static readonly Type GreenLizard = new Type("GreenLizard", register: true);

		public static readonly Type BlueLizard = new Type("BlueLizard", register: true);

		public static readonly Type YellowLizard = new Type("YellowLizard", register: true);

		public static readonly Type WhiteLizard = new Type("WhiteLizard", register: true);

		public static readonly Type RedLizard = new Type("RedLizard", register: true);

		public static readonly Type BlackLizard = new Type("BlackLizard", register: true);

		public static readonly Type Salamander = new Type("Salamander", register: true);

		public static readonly Type CyanLizard = new Type("CyanLizard", register: true);

		public static readonly Type Fly = new Type("Fly", register: true);

		public static readonly Type Leech = new Type("Leech", register: true);

		public static readonly Type SeaLeech = new Type("SeaLeech", register: true);

		public static readonly Type Snail = new Type("Snail", register: true);

		public static readonly Type Vulture = new Type("Vulture", register: true);

		public static readonly Type GarbageWorm = new Type("GarbageWorm", register: true);

		public static readonly Type LanternMouse = new Type("LanternMouse", register: true);

		public static readonly Type CicadaA = new Type("CicadaA", register: true);

		public static readonly Type CicadaB = new Type("CicadaB", register: true);

		public static readonly Type Spider = new Type("Spider", register: true);

		public static readonly Type JetFish = new Type("JetFish", register: true);

		public static readonly Type BigEel = new Type("BigEel", register: true);

		public static readonly Type Deer = new Type("Deer", register: true);

		public static readonly Type TubeWorm = new Type("TubeWorm", register: true);

		public static readonly Type DaddyLongLegs = new Type("DaddyLongLegs", register: true);

		public static readonly Type BrotherLongLegs = new Type("BrotherLongLegs", register: true);

		public static readonly Type TentaclePlant = new Type("TentaclePlant", register: true);

		public static readonly Type PoleMimic = new Type("PoleMimic", register: true);

		public static readonly Type MirosBird = new Type("MirosBird", register: true);

		public static readonly Type TempleGuard = new Type("TempleGuard", register: true);

		public static readonly Type Centipede = new Type("Centipede", register: true);

		public static readonly Type RedCentipede = new Type("RedCentipede", register: true);

		public static readonly Type Centiwing = new Type("Centiwing", register: true);

		public static readonly Type SmallCentipede = new Type("SmallCentipede", register: true);

		public static readonly Type Scavenger = new Type("Scavenger", register: true);

		public static readonly Type Overseer = new Type("Overseer", register: true);

		public static readonly Type VultureGrub = new Type("VultureGrub", register: true);

		public static readonly Type EggBug = new Type("EggBug", register: true);

		public static readonly Type BigSpider = new Type("BigSpider", register: true);

		public static readonly Type SpitterSpider = new Type("SpitterSpider", register: true);

		public static readonly Type SmallNeedleWorm = new Type("SmallNeedleWorm", register: true);

		public static readonly Type BigNeedleWorm = new Type("BigNeedleWorm", register: true);

		public static readonly Type DropBug = new Type("DropBug", register: true);

		public static readonly Type KingVulture = new Type("KingVulture", register: true);

		public static readonly Type Hazer = new Type("Hazer", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public struct Relationship
	{
		public class Type : ExtEnum<Type>
		{
			public static readonly Type DoesntTrack = new Type("DoesntTrack", register: true);

			public static readonly Type Ignores = new Type("Ignores", register: true);

			public static readonly Type Eats = new Type("Eats", register: true);

			public static readonly Type Afraid = new Type("Afraid", register: true);

			public static readonly Type StayOutOfWay = new Type("StayOutOfWay", register: true);

			public static readonly Type AgressiveRival = new Type("AgressiveRival", register: true);

			public static readonly Type Attacks = new Type("Attacks", register: true);

			public static readonly Type Uncomfortable = new Type("Uncomfortable", register: true);

			public static readonly Type Antagonizes = new Type("Antagonizes", register: true);

			public static readonly Type PlaysWith = new Type("PlaysWith", register: true);

			public static readonly Type SocialDependent = new Type("SocialDependent", register: true);

			public static readonly Type Pack = new Type("Pack", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Type type;

		public float intensity;

		public bool GoForKill
		{
			get
			{
				if (type == Type.Eats || type == Type.Attacks)
				{
					return intensity > 0f;
				}
				return false;
			}
		}

		public Relationship(Type type, float intensity)
		{
			this.type = type;
			this.intensity = intensity;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Relationship))
			{
				return false;
			}
			return Equals((Relationship)obj);
		}

		public bool Equals(Relationship relationship)
		{
			if (type == relationship.type)
			{
				return intensity == relationship.intensity;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(Relationship a, Relationship b)
		{
			if (a.type == b.type)
			{
				return a.intensity == b.intensity;
			}
			return false;
		}

		public static bool operator !=(Relationship a, Relationship b)
		{
			return !(a == b);
		}

		public Relationship Duplicate()
		{
			return new Relationship(type, intensity);
		}

		public override string ToString()
		{
			return type.ToString() + " " + intensity;
		}
	}

	public Type type;

	public string name;

	public CreatureTemplate ancestor;

	public CreatureTemplate preBakedPathingAncestor;

	public int index;

	private int prebakedPathingIndex;

	public bool virtualCreature;

	public bool doPreBakedPathing;

	public PathCost[] pathingPreferencesTiles;

	public PathCost[] pathingPreferencesConnections;

	public float baseDamageResistance;

	public float baseStunResistance;

	public float instantDeathDamageLimit;

	public float[,] damageRestistances;

	public int maxAccessibleTerrain;

	public bool AI;

	public bool requireAImap;

	public bool quantified;

	public float offScreenSpeed;

	public int abstractedLaziness;

	public BreedParameters breedParameters;

	public bool canFly;

	public int grasps;

	public bool stowFoodInDen;

	public bool smallCreature;

	public float dangerousToPlayer;

	public float roamInRoomChance = 0.1f;

	public float roamBetweenRoomsChance = 0.1f;

	public float visualRadius;

	public float waterVision = 0.4f;

	public float throughSurfaceVision = 0.8f;

	public float movementBasedVision = 0.2f;

	public CreatureCommunities.CommunityID communityID = CreatureCommunities.CommunityID.All;

	public float communityInfluence = 0.5f;

	public int countsAsAKill = 2;

	public int meatPoints;

	public float lungCapacity = 520f;

	public bool quickDeath = true;

	public bool wormGrassImmune;

	public bool saveCreature = true;

	public bool hibernateOffScreen;

	public bool[] mappedNodeTypes;

	public PathCost shortcutAversion;

	public PathCost NPCTravelAversion;

	public float bodySize;

	public float scaryness;

	public float deliciousness;

	public Color shortcutColor;

	public int shortcutSegments;

	public Relationship[] relationships;

	public int quantifiedIndex;

	public WaterRelationship waterRelationship = WaterRelationship.AirOnly;

	public float waterPathingResistance;

	public bool canSwim;

	public bool socialMemory;

	public int[] doubleReachUpConnectionParams;

	public bool BlizzardAdapted;

	public bool BlizzardWanderer;

	public bool usesNPCTransportation;

	public bool usesRegionTransportation;

	public bool usesCreatureHoles;

	public string pickupAction;

	public string throwAction;

	public string jumpAction;

	public bool wormgrassTilesIgnored;

	public bool doesNotUseDens;

	public bool abstractImmobile;

	public float interestInOtherCreaturesCatches;

	public float interestInOtherAncestorsCatches;

	public bool forbidStandardShortcutEntry;

	public bool UseAnyRoomBorderExit
	{
		get
		{
			if (!mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit] && !mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit])
			{
				return mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit];
			}
			return true;
		}
	}

	public bool IsCicada
	{
		get
		{
			if (!(type == Type.CicadaA))
			{
				return type == Type.CicadaB;
			}
			return true;
		}
	}

	public bool IsLizard
	{
		get
		{
			if (ancestor != null)
			{
				return ancestor.type == Type.LizardTemplate;
			}
			return false;
		}
	}

	public bool IsVulture
	{
		get
		{
			if (!(type == Type.Vulture))
			{
				return type == Type.KingVulture;
			}
			return true;
		}
	}

	public int PreBakedPathingIndex
	{
		get
		{
			if (doPreBakedPathing)
			{
				return prebakedPathingIndex;
			}
			if (preBakedPathingAncestor != null)
			{
				return preBakedPathingAncestor.prebakedPathingIndex;
			}
			return -1;
		}
		set
		{
			if (doPreBakedPathing)
			{
				prebakedPathingIndex = value;
			}
		}
	}

	private void SetDoubleReachUpConnectionParams(AItile.Accessibility groundTile, AItile.Accessibility betweenTiles, AItile.Accessibility destinationTile)
	{
		doubleReachUpConnectionParams = new int[3]
		{
			(int)groundTile,
			(int)betweenTiles,
			(int)destinationTile
		};
	}

	public CreatureTemplate(Type type, CreatureTemplate ancestor, List<TileTypeResistance> tileResistances, List<TileConnectionResistance> connectionResistances, Relationship defaultRelationship)
	{
		this.type = type;
		name = "???";
		if (type == Type.StandardGroundCreature)
		{
			name = "StandardGroundCreature";
		}
		else if (type == Type.Slugcat)
		{
			name = "Slugcat";
		}
		else if (type == Type.LizardTemplate)
		{
			name = "Lizard";
			SetDoubleReachUpConnectionParams(AItile.Accessibility.Floor, AItile.Accessibility.Wall, AItile.Accessibility.Floor);
		}
		else if (type == Type.PinkLizard)
		{
			name = "Pink Lizard";
		}
		else if (type == Type.GreenLizard)
		{
			name = "Green Lizard";
		}
		else if (type == Type.BlueLizard)
		{
			name = "Blue Lizard";
		}
		else if (type == Type.YellowLizard)
		{
			name = "Yellow Lizard";
		}
		else if (type == Type.WhiteLizard)
		{
			name = "White Lizard";
		}
		else if (type == Type.RedLizard)
		{
			name = "Red Lizard";
		}
		else if (type == Type.BlackLizard)
		{
			name = "Black Lizard";
		}
		else if (type == Type.Salamander)
		{
			name = "Salamander";
		}
		else if (type == Type.CyanLizard)
		{
			name = "Cyan Lizard";
		}
		else if (type == Type.Fly)
		{
			name = "Fly";
		}
		else if (type == Type.Leech)
		{
			name = "Leech";
		}
		else if (type == Type.SeaLeech)
		{
			name = "Sea Leech";
		}
		else if (type == Type.Snail)
		{
			name = "Snail";
		}
		else if (type == Type.Vulture)
		{
			name = "Vulture";
		}
		else if (type == Type.GarbageWorm)
		{
			name = "Garbage Worm";
		}
		else if (type == Type.LanternMouse)
		{
			name = "Lantern Mouse";
		}
		else if (type == Type.CicadaA)
		{
			name = "Cicada A";
		}
		else if (type == Type.CicadaB)
		{
			name = "Cicada B";
		}
		else if (type == Type.Spider)
		{
			name = "Spider";
		}
		else if (type == Type.JetFish)
		{
			name = "Jet Fish";
		}
		else if (type == Type.BigEel)
		{
			name = "Big Eel";
		}
		else if (type == Type.Deer)
		{
			name = "Deer";
		}
		else if (type == Type.TubeWorm)
		{
			name = "Tube Worm";
			SetDoubleReachUpConnectionParams(AItile.Accessibility.Floor, AItile.Accessibility.Wall, AItile.Accessibility.Floor);
		}
		else if (type == Type.DaddyLongLegs)
		{
			name = "Daddy Long Legs";
		}
		else if (type == Type.BrotherLongLegs)
		{
			name = "Brother Long Legs";
		}
		else if (type == Type.TentaclePlant)
		{
			name = "Tentacle Plant";
		}
		else if (type == Type.PoleMimic)
		{
			name = "Pole Mimic";
		}
		else if (type == Type.MirosBird)
		{
			name = "Miros Bird";
		}
		else if (type == Type.TempleGuard)
		{
			name = "Temple Guard";
		}
		else if (type == Type.Centipede)
		{
			name = "Centipede";
		}
		else if (type == Type.RedCentipede)
		{
			name = "Red Centipede";
		}
		else if (type == Type.Scavenger)
		{
			name = "Scavenger";
			SetDoubleReachUpConnectionParams(AItile.Accessibility.Climb, AItile.Accessibility.Air, AItile.Accessibility.Climb);
		}
		else if (type == Type.Centiwing)
		{
			name = "Centiwing";
		}
		else if (type == Type.SmallCentipede)
		{
			name = "Small Centipede";
		}
		else if (type == Type.Overseer)
		{
			name = "Overseer";
		}
		else if (type == Type.VultureGrub)
		{
			name = "Vulture Grub";
		}
		else if (type == Type.BigSpider)
		{
			name = "Big Spider";
		}
		else if (type == Type.SpitterSpider)
		{
			name = "Spitter Spider";
		}
		else if (type == Type.EggBug)
		{
			name = "Egg Bug";
		}
		else if (type == Type.SmallNeedleWorm)
		{
			name = "Small Needle";
		}
		else if (type == Type.BigNeedleWorm)
		{
			name = "Big Needle";
		}
		else if (type == Type.DropBug)
		{
			name = "Drop Bug";
		}
		else if (type == Type.KingVulture)
		{
			name = "King Vulture";
		}
		else if (type == Type.Hazer)
		{
			name = "Hazer";
		}
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
			{
				name = "Miros Vulture";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
			{
				name = "Caramel Lizard";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
			{
				name = "Eel";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
			{
				name = "Strawberry";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
			{
				name = "Mother Spider";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				name = "Terror Long Legs";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
			{
				name = "Aquapede";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				name = "Hunter";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				name = "Hell Bug";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
			{
				name = "Stowaway Bug";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
			{
				name = "Elite Scavenger";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
			{
				name = "King Scavenger";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
			{
				name = "Inspector";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
			{
				name = "Yeek";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly)
			{
				name = "Big Jellyfish";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				name = "Slugcat NPC";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
			{
				name = "Jungle Leech";
			}
			else if (type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				name = "Train Lizard";
			}
		}
		relationships = new Relationship[ExtEnum<Type>.values.Count];
		for (int i = 0; i < relationships.Length; i++)
		{
			relationships[i] = defaultRelationship;
		}
		virtualCreature = false;
		doPreBakedPathing = false;
		AI = false;
		requireAImap = false;
		quantified = false;
		canFly = false;
		grasps = 0;
		offScreenSpeed = 1f;
		abstractedLaziness = 1;
		smallCreature = false;
		mappedNodeTypes = new bool[ExtEnum<AbstractRoomNode.Type>.values.Count];
		bodySize = 1f;
		scaryness = 1f;
		deliciousness = 1f;
		shortcutColor = new Color(1f, 1f, 1f);
		shortcutSegments = 1;
		waterRelationship = WaterRelationship.Amphibious;
		waterPathingResistance = 1f;
		canSwim = false;
		shortcutAversion = new PathCost(0f, PathCost.Legality.Allowed);
		NPCTravelAversion = new PathCost(100f, PathCost.Legality.Allowed);
		damageRestistances = new float[ExtEnum<Creature.DamageType>.values.Count, 2];
		instantDeathDamageLimit = float.MaxValue;
		this.ancestor = ancestor;
		if (ancestor != null)
		{
			preBakedPathingAncestor = ancestor.preBakedPathingAncestor;
			virtualCreature = ancestor.virtualCreature;
			doPreBakedPathing = ancestor.doPreBakedPathing;
			AI = ancestor.AI;
			requireAImap = ancestor.requireAImap;
			quantified = ancestor.quantified;
			canFly = ancestor.canFly;
			grasps = ancestor.grasps;
			offScreenSpeed = ancestor.offScreenSpeed;
			abstractedLaziness = ancestor.abstractedLaziness;
			breedParameters = ancestor.breedParameters;
			stowFoodInDen = ancestor.stowFoodInDen;
			smallCreature = ancestor.smallCreature;
			roamInRoomChance = ancestor.roamInRoomChance;
			roamBetweenRoomsChance = ancestor.roamBetweenRoomsChance;
			visualRadius = ancestor.visualRadius;
			waterVision = ancestor.waterVision;
			throughSurfaceVision = ancestor.throughSurfaceVision;
			movementBasedVision = ancestor.movementBasedVision;
			dangerousToPlayer = ancestor.dangerousToPlayer;
			communityID = ancestor.communityID;
			communityInfluence = ancestor.communityInfluence;
			countsAsAKill = ancestor.countsAsAKill;
			quickDeath = ancestor.quickDeath;
			meatPoints = ancestor.meatPoints;
			wormGrassImmune = ancestor.wormGrassImmune;
			saveCreature = ancestor.saveCreature;
			hibernateOffScreen = ancestor.hibernateOffScreen;
			mappedNodeTypes = (bool[])ancestor.mappedNodeTypes.Clone();
			bodySize = ancestor.bodySize;
			scaryness = ancestor.scaryness;
			deliciousness = ancestor.deliciousness;
			shortcutColor = ancestor.shortcutColor;
			shortcutSegments = ancestor.shortcutSegments;
			waterRelationship = ancestor.waterRelationship;
			waterPathingResistance = ancestor.waterPathingResistance;
			canSwim = ancestor.canSwim;
			socialMemory = ancestor.socialMemory;
			shortcutAversion = ancestor.shortcutAversion;
			NPCTravelAversion = ancestor.NPCTravelAversion;
			doubleReachUpConnectionParams = ancestor.doubleReachUpConnectionParams;
			relationships = (Relationship[])ancestor.relationships.Clone();
			baseDamageResistance = ancestor.baseDamageResistance;
			baseStunResistance = ancestor.baseStunResistance;
			instantDeathDamageLimit = ancestor.instantDeathDamageLimit;
			lungCapacity = ancestor.lungCapacity;
			if (ModManager.MSC)
			{
				BlizzardAdapted = ancestor.BlizzardAdapted;
				BlizzardWanderer = ancestor.BlizzardWanderer;
				usesNPCTransportation = ancestor.usesNPCTransportation;
				usesRegionTransportation = ancestor.usesRegionTransportation;
				usesCreatureHoles = ancestor.usesCreatureHoles;
				wormgrassTilesIgnored = ancestor.wormgrassTilesIgnored;
				doesNotUseDens = ancestor.doesNotUseDens;
				abstractImmobile = ancestor.abstractImmobile;
				interestInOtherCreaturesCatches = ancestor.interestInOtherCreaturesCatches;
				interestInOtherAncestorsCatches = ancestor.interestInOtherAncestorsCatches;
				forbidStandardShortcutEntry = ancestor.forbidStandardShortcutEntry;
				if (throwAction == null || throwAction == "")
				{
					throwAction = ancestor.throwAction;
				}
				if (jumpAction == null || jumpAction == "")
				{
					jumpAction = ancestor.jumpAction;
				}
				if (pickupAction == null || pickupAction == "")
				{
					pickupAction = ancestor.pickupAction;
				}
			}
		}
		maxAccessibleTerrain = 0;
		pathingPreferencesTiles = new PathCost[Enum.GetNames(typeof(AItile.Accessibility)).Length];
		pathingPreferencesConnections = new PathCost[Enum.GetNames(typeof(MovementConnection.MovementType)).Length];
		if (ancestor == null)
		{
			for (int j = 0; j < pathingPreferencesTiles.Length; j++)
			{
				pathingPreferencesTiles[j] = new PathCost(10f * (float)j, PathCost.Legality.IllegalTile);
			}
			pathingPreferencesTiles[7] = new PathCost(100f, PathCost.Legality.SolidTile);
			for (int k = 0; k < pathingPreferencesConnections.Length; k++)
			{
				pathingPreferencesConnections[k] = new PathCost(100f, PathCost.Legality.IllegalConnection);
			}
		}
		else
		{
			pathingPreferencesTiles = (PathCost[])ancestor.pathingPreferencesTiles.Clone();
			pathingPreferencesConnections = (PathCost[])ancestor.pathingPreferencesConnections.Clone();
		}
		for (int l = 0; l < tileResistances.Count; l++)
		{
			pathingPreferencesTiles[(int)tileResistances[l].accessibility] = tileResistances[l].cost;
			if (tileResistances[l].cost.legality == PathCost.Legality.Allowed && maxAccessibleTerrain < (int)tileResistances[l].accessibility)
			{
				maxAccessibleTerrain = (int)tileResistances[l].accessibility;
			}
		}
		for (int m = 0; m <= maxAccessibleTerrain; m++)
		{
			if (pathingPreferencesTiles[m] > pathingPreferencesTiles[maxAccessibleTerrain])
			{
				pathingPreferencesTiles[m] = pathingPreferencesTiles[maxAccessibleTerrain];
			}
		}
		for (int n = 0; n < connectionResistances.Count; n++)
		{
			pathingPreferencesConnections[(int)connectionResistances[n].movementType] = connectionResistances[n].cost;
		}
		SetNodeType(AbstractRoomNode.Type.Exit, ConnectionResistance(MovementConnection.MovementType.ShortCut).Allowed);
		SetNodeType(AbstractRoomNode.Type.Den, ConnectionResistance(MovementConnection.MovementType.ShortCut).Allowed);
		SetNodeType(AbstractRoomNode.Type.SkyExit, ConnectionResistance(MovementConnection.MovementType.SkyHighway).Allowed);
		SetNodeType(AbstractRoomNode.Type.SeaExit, ConnectionResistance(MovementConnection.MovementType.SeaHighway).Allowed);
		SetNodeType(AbstractRoomNode.Type.SideExit, ConnectionResistance(MovementConnection.MovementType.SideHighway).Allowed);
		if (type == Type.Scavenger)
		{
			SetNodeType(AbstractRoomNode.Type.Den, b: false);
			SetNodeType(AbstractRoomNode.Type.RegionTransportation, b: true);
		}
		if (type == Type.Fly)
		{
			SetNodeType(AbstractRoomNode.Type.BatHive, b: true);
		}
		if (type == Type.GarbageWorm)
		{
			for (int num = 0; num < mappedNodeTypes.Length; num++)
			{
				mappedNodeTypes[num] = false;
			}
			SetNodeType(AbstractRoomNode.Type.GarbageHoles, b: true);
		}
	}

	public CreatureTemplate(CreatureTemplate copy)
	{
		type = copy.type;
		name = copy.name;
		ancestor = copy.ancestor;
		preBakedPathingAncestor = copy.preBakedPathingAncestor;
		index = copy.index;
		prebakedPathingIndex = copy.prebakedPathingIndex;
		virtualCreature = copy.virtualCreature;
		doPreBakedPathing = copy.doPreBakedPathing;
		if (copy.pathingPreferencesConnections != null)
		{
			pathingPreferencesConnections = new PathCost[copy.pathingPreferencesConnections.Length];
			for (int i = 0; i < pathingPreferencesConnections.Length; i++)
			{
				pathingPreferencesConnections[i] = copy.pathingPreferencesConnections[i];
			}
		}
		if (copy.pathingPreferencesTiles != null)
		{
			pathingPreferencesTiles = new PathCost[copy.pathingPreferencesTiles.Length];
			for (int j = 0; j < pathingPreferencesTiles.Length; j++)
			{
				pathingPreferencesTiles[j] = copy.pathingPreferencesTiles[j];
			}
		}
		baseDamageResistance = copy.baseDamageResistance;
		baseStunResistance = copy.baseStunResistance;
		instantDeathDamageLimit = copy.instantDeathDamageLimit;
		maxAccessibleTerrain = copy.maxAccessibleTerrain;
		AI = copy.AI;
		requireAImap = copy.requireAImap;
		quantified = copy.quantified;
		offScreenSpeed = copy.offScreenSpeed;
		abstractedLaziness = copy.abstractedLaziness;
		breedParameters = copy.breedParameters;
		canFly = copy.canFly;
		grasps = copy.grasps;
		stowFoodInDen = copy.stowFoodInDen;
		smallCreature = copy.smallCreature;
		dangerousToPlayer = copy.dangerousToPlayer;
		roamInRoomChance = copy.roamInRoomChance;
		roamBetweenRoomsChance = copy.roamBetweenRoomsChance;
		visualRadius = copy.visualRadius;
		waterVision = copy.waterVision;
		throughSurfaceVision = copy.throughSurfaceVision;
		movementBasedVision = copy.movementBasedVision;
		communityID = copy.communityID;
		communityInfluence = copy.communityInfluence;
		countsAsAKill = copy.countsAsAKill;
		meatPoints = copy.meatPoints;
		lungCapacity = copy.lungCapacity;
		quickDeath = copy.quickDeath;
		wormGrassImmune = copy.wormGrassImmune;
		saveCreature = copy.saveCreature;
		hibernateOffScreen = copy.hibernateOffScreen;
		mappedNodeTypes = new bool[copy.mappedNodeTypes.Length];
		for (int k = 0; k < mappedNodeTypes.Length; k++)
		{
			mappedNodeTypes[k] = copy.mappedNodeTypes[k];
		}
		damageRestistances = new float[ExtEnum<Creature.DamageType>.values.Count, 2];
		for (int l = 0; l < ExtEnum<Creature.DamageType>.values.Count; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				damageRestistances[l, m] = copy.damageRestistances[l, m];
			}
		}
		shortcutAversion = copy.shortcutAversion;
		NPCTravelAversion = copy.NPCTravelAversion;
		bodySize = copy.bodySize;
		scaryness = copy.scaryness;
		deliciousness = copy.deliciousness;
		shortcutColor = copy.shortcutColor;
		shortcutSegments = copy.shortcutSegments;
		if (copy.relationships != null)
		{
			relationships = new Relationship[copy.relationships.Length];
			for (int n = 0; n < relationships.Length; n++)
			{
				relationships[n] = copy.relationships[n];
			}
		}
		quantifiedIndex = copy.quantifiedIndex;
		waterRelationship = copy.waterRelationship;
		waterPathingResistance = copy.waterPathingResistance;
		canSwim = copy.canSwim;
		socialMemory = copy.socialMemory;
		if (copy.doubleReachUpConnectionParams != null)
		{
			doubleReachUpConnectionParams = new int[copy.doubleReachUpConnectionParams.Length];
			for (int num = 0; num < copy.doubleReachUpConnectionParams.Length; num++)
			{
				doubleReachUpConnectionParams[num] = copy.doubleReachUpConnectionParams[num];
			}
		}
		BlizzardAdapted = copy.BlizzardAdapted;
		BlizzardWanderer = copy.BlizzardWanderer;
		usesNPCTransportation = copy.usesNPCTransportation;
		usesRegionTransportation = copy.usesRegionTransportation;
		usesCreatureHoles = copy.usesCreatureHoles;
		pickupAction = copy.pickupAction;
		throwAction = copy.throwAction;
		jumpAction = copy.jumpAction;
		wormgrassTilesIgnored = copy.wormgrassTilesIgnored;
		doesNotUseDens = copy.doesNotUseDens;
		abstractImmobile = copy.abstractImmobile;
		interestInOtherCreaturesCatches = copy.interestInOtherCreaturesCatches;
		interestInOtherAncestorsCatches = copy.interestInOtherAncestorsCatches;
		forbidStandardShortcutEntry = copy.forbidStandardShortcutEntry;
	}

	public void SetNodeType(AbstractRoomNode.Type tp, bool b)
	{
		if (tp.Index != -1)
		{
			mappedNodeTypes[tp.Index] = b;
		}
	}

	public PathCost ConnectionResistance(MovementConnection.MovementType v)
	{
		return pathingPreferencesConnections[(int)v];
	}

	public PathCost AccessibilityResistance(AItile.Accessibility v)
	{
		return pathingPreferencesTiles[(int)v];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MovementLegalInRelationToWater(bool deepWater, bool waterSurface)
	{
		if (waterRelationship == WaterRelationship.AirAndSurface)
		{
			return !deepWater;
		}
		if (waterRelationship == WaterRelationship.AirOnly)
		{
			if (!deepWater)
			{
				return !waterSurface;
			}
			return false;
		}
		if (waterRelationship == WaterRelationship.Amphibious)
		{
			return true;
		}
		if (waterRelationship == WaterRelationship.WaterOnly)
		{
			return deepWater;
		}
		return false;
	}

	public bool AbstractSubmersionLegal(bool nodeSubmerged)
	{
		if (nodeSubmerged)
		{
			if (!(waterRelationship == WaterRelationship.WaterOnly))
			{
				return waterRelationship == WaterRelationship.Amphibious;
			}
			return true;
		}
		return waterRelationship != WaterRelationship.WaterOnly;
	}

	public CreatureTemplate TopAncestor()
	{
		if (ancestor == null)
		{
			return this;
		}
		return ancestor.TopAncestor();
	}

	public Relationship CreatureRelationship(Creature crit)
	{
		return CreatureRelationship(crit.Template);
	}

	public Relationship CreatureRelationship(CreatureTemplate crit)
	{
		if (crit == null || crit.type.Index == -1)
		{
			return new Relationship(Relationship.Type.Ignores, 0f);
		}
		return relationships[crit.type.Index];
	}
}
