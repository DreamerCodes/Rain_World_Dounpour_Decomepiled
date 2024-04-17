using HUD;
using Menu;
using OverseerHolograms;
using VoidSea;

namespace MoreSlugcats;

public class MoreSlugcatsEnums
{
	public class AbstractObjectType
	{
		public static AbstractPhysicalObject.AbstractObjectType JokeRifle;

		public static AbstractPhysicalObject.AbstractObjectType Bullet;

		public static AbstractPhysicalObject.AbstractObjectType SingularityBomb;

		public static AbstractPhysicalObject.AbstractObjectType Spearmasterpearl;

		public static AbstractPhysicalObject.AbstractObjectType FireEgg;

		public static AbstractPhysicalObject.AbstractObjectType EnergyCell;

		public static AbstractPhysicalObject.AbstractObjectType Germinator;

		public static AbstractPhysicalObject.AbstractObjectType Seed;

		public static AbstractPhysicalObject.AbstractObjectType GooieDuck;

		public static AbstractPhysicalObject.AbstractObjectType LillyPuck;

		public static AbstractPhysicalObject.AbstractObjectType GlowWeed;

		public static AbstractPhysicalObject.AbstractObjectType MoonCloak;

		public static AbstractPhysicalObject.AbstractObjectType HalcyonPearl;

		public static AbstractPhysicalObject.AbstractObjectType DandelionPeach;

		public static AbstractPhysicalObject.AbstractObjectType HRGuard;

		public static void RegisterValues()
		{
			JokeRifle = new AbstractPhysicalObject.AbstractObjectType("JokeRifle", register: true);
			Bullet = new AbstractPhysicalObject.AbstractObjectType("Bullet", register: true);
			SingularityBomb = new AbstractPhysicalObject.AbstractObjectType("SingularityBomb", register: true);
			Spearmasterpearl = new AbstractPhysicalObject.AbstractObjectType("Spearmasterpearl", register: true);
			FireEgg = new AbstractPhysicalObject.AbstractObjectType("FireEgg", register: true);
			EnergyCell = new AbstractPhysicalObject.AbstractObjectType("EnergyCell", register: true);
			Germinator = new AbstractPhysicalObject.AbstractObjectType("Germinator", register: true);
			Seed = new AbstractPhysicalObject.AbstractObjectType("Seed", register: true);
			GooieDuck = new AbstractPhysicalObject.AbstractObjectType("GooieDuck", register: true);
			LillyPuck = new AbstractPhysicalObject.AbstractObjectType("LillyPuck", register: true);
			GlowWeed = new AbstractPhysicalObject.AbstractObjectType("GlowWeed", register: true);
			MoonCloak = new AbstractPhysicalObject.AbstractObjectType("MoonCloak", register: true);
			HalcyonPearl = new AbstractPhysicalObject.AbstractObjectType("HalcyonPearl", register: true);
			DandelionPeach = new AbstractPhysicalObject.AbstractObjectType("DandelionPeach", register: true);
			HRGuard = new AbstractPhysicalObject.AbstractObjectType("HRGuard", register: true);
		}

		public static void UnregisterValues()
		{
			JokeRifle?.Unregister();
			JokeRifle = null;
			Bullet?.Unregister();
			Bullet = null;
			SingularityBomb?.Unregister();
			SingularityBomb = null;
			Spearmasterpearl?.Unregister();
			Spearmasterpearl = null;
			FireEgg?.Unregister();
			FireEgg = null;
			EnergyCell?.Unregister();
			EnergyCell = null;
			Germinator?.Unregister();
			Germinator = null;
			Seed?.Unregister();
			Seed = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			MoonCloak?.Unregister();
			MoonCloak = null;
			HalcyonPearl?.Unregister();
			HalcyonPearl = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
			HRGuard?.Unregister();
			HRGuard = null;
		}
	}

	public class GameTypeID
	{
		public static ArenaSetup.GameTypeID Challenge;

		public static ArenaSetup.GameTypeID Safari;

		public static void RegisterValues()
		{
			Challenge = new ArenaSetup.GameTypeID("Challenge", register: true);
			Safari = new ArenaSetup.GameTypeID("Safari", register: true);
		}

		public static void UnregisterValues()
		{
			Challenge?.Unregister();
			Challenge = null;
			Safari?.Unregister();
			Safari = null;
		}
	}

	public class ConversationID
	{
		public static Conversation.ID MoonGiveMark;

		public static Conversation.ID MoonGiveMarkAfter;

		public static Conversation.ID Pebbles_Spearmaster_Read_Pearl;

		public static Conversation.ID Pebbles_Spearmaster_Angry;

		public static Conversation.ID Moon_Pearl_SI_chat3;

		public static Conversation.ID Moon_Pearl_SI_chat4;

		public static Conversation.ID Moon_Pearl_SI_chat5;

		public static Conversation.ID Moon_Pearl_SU_filt;

		public static Conversation.ID Moon_Pearl_DM;

		public static Conversation.ID Moon_Pearl_LC;

		public static Conversation.ID Moon_Pearl_OE;

		public static Conversation.ID Moon_Pearl_MS;

		public static Conversation.ID Moon_Pearl_RM;

		public static Conversation.ID Moon_Pearl_Rivulet_stomach;

		public static Conversation.ID Moon_Pearl_LC_second;

		public static Conversation.ID Moon_Spearmaster_Pearl;

		public static Conversation.ID Commercial;

		public static Conversation.ID Ghost_LC;

		public static Conversation.ID Ghost_UG;

		public static Conversation.ID Ghost_SL;

		public static Conversation.ID Ghost_CL;

		public static Conversation.ID Pebbles_Arty;

		public static Conversation.ID Ghost_MS;

		public static Conversation.ID Pebbles_Pearl_RM;

		public static Conversation.ID Moon_RipPebbles;

		public static Conversation.ID Moon_RipPebbles_MeetingRiv;

		public static Conversation.ID Moon_Rivulet_First_Conversation;

		public static Conversation.ID Moon_Pearl_VS;

		public static Conversation.ID Moon_PearlBleaching;

		public static Conversation.ID Moon_RivuletEnding;

		public static Conversation.ID Moon_RivuletPostgame;

		public static Conversation.ID Pebbles_RM_FirstMeeting;

		public static Conversation.ID Pebbles_Gourmand;

		public static Conversation.ID Moon_Pearl_BroadcastMisc;

		public static Conversation.ID Moon_Saint_Echo_Blocked;

		public static Conversation.ID Moon_Saint_First_Conversation;

		public static Conversation.ID Moon_Gourmand_First_Conversation;

		public static Conversation.ID Moon_HR;

		public static Conversation.ID Pebbles_HR;

		public static Conversation.ID Moon_Pebbles_HR;

		public static void RegisterValues()
		{
			MoonGiveMark = new Conversation.ID("MoonGiveMark", register: true);
			MoonGiveMarkAfter = new Conversation.ID("MoonGiveMarkAfter", register: true);
			Pebbles_Spearmaster_Read_Pearl = new Conversation.ID("Pebbles_Spearmaster_Read_Pearl", register: true);
			Pebbles_Spearmaster_Angry = new Conversation.ID("Pebbles_Spearmaster_Angry", register: true);
			Moon_Pearl_SI_chat3 = new Conversation.ID("Moon_Pearl_SI_chat3", register: true);
			Moon_Pearl_SI_chat4 = new Conversation.ID("Moon_Pearl_SI_chat4", register: true);
			Moon_Pearl_SI_chat5 = new Conversation.ID("Moon_Pearl_SI_chat5", register: true);
			Moon_Pearl_SU_filt = new Conversation.ID("Moon_Pearl_SU_filt", register: true);
			Moon_Pearl_DM = new Conversation.ID("Moon_Pearl_DM", register: true);
			Moon_Pearl_LC = new Conversation.ID("Moon_Pearl_LC", register: true);
			Moon_Pearl_OE = new Conversation.ID("Moon_Pearl_OE", register: true);
			Moon_Pearl_MS = new Conversation.ID("Moon_Pearl_MS", register: true);
			Moon_Pearl_RM = new Conversation.ID("Moon_Pearl_RM", register: true);
			Moon_Pearl_Rivulet_stomach = new Conversation.ID("Moon_Pearl_Rivulet_stomach", register: true);
			Moon_Pearl_LC_second = new Conversation.ID("Moon_Pearl_LC_second", register: true);
			Moon_Spearmaster_Pearl = new Conversation.ID("Moon_Spearmaster_Pearl", register: true);
			Commercial = new Conversation.ID("Commercial", register: true);
			Ghost_LC = new Conversation.ID("Ghost_LC", register: true);
			Ghost_UG = new Conversation.ID("Ghost_UG", register: true);
			Ghost_SL = new Conversation.ID("Ghost_SL", register: true);
			Ghost_CL = new Conversation.ID("Ghost_CL", register: true);
			Pebbles_Arty = new Conversation.ID("Pebbles_Arty", register: true);
			Ghost_MS = new Conversation.ID("Ghost_MS", register: true);
			Pebbles_Pearl_RM = new Conversation.ID("Pebbles_Pearl_RM", register: true);
			Moon_RipPebbles = new Conversation.ID("Moon_RipPebbles", register: true);
			Moon_RipPebbles_MeetingRiv = new Conversation.ID("Moon_RipPebbles_MeetingRiv", register: true);
			Moon_Rivulet_First_Conversation = new Conversation.ID("Moon_Rivulet_First_Conversation", register: true);
			Moon_Pearl_VS = new Conversation.ID("Moon_Pearl_VS", register: true);
			Moon_PearlBleaching = new Conversation.ID("Moon_PearlBleaching", register: true);
			Moon_RivuletEnding = new Conversation.ID("Moon_RivuletEnding", register: true);
			Moon_RivuletPostgame = new Conversation.ID("Moon_RivuletPostgame", register: true);
			Pebbles_RM_FirstMeeting = new Conversation.ID("Pebbles_RM_FirstMeeting", register: true);
			Pebbles_Gourmand = new Conversation.ID("Pebbles_Gourmand", register: true);
			Moon_Pearl_BroadcastMisc = new Conversation.ID("Moon_Pearl_BroadcastMisc", register: true);
			Moon_Saint_Echo_Blocked = new Conversation.ID("Moon_Saint_Echo_Blocked", register: true);
			Moon_Saint_First_Conversation = new Conversation.ID("Moon_Saint_First_Conversation", register: true);
			Moon_Gourmand_First_Conversation = new Conversation.ID("Moon_Gourmand_First_Conversation", register: true);
			Moon_HR = new Conversation.ID("Moon_HR", register: true);
			Pebbles_HR = new Conversation.ID("Pebbles_HR", register: true);
			Moon_Pebbles_HR = new Conversation.ID("Moon_Pebbles_HR", register: true);
		}

		public static void UnregisterValues()
		{
			MoonGiveMark?.Unregister();
			MoonGiveMark = null;
			MoonGiveMarkAfter?.Unregister();
			MoonGiveMarkAfter = null;
			Pebbles_Spearmaster_Read_Pearl?.Unregister();
			Pebbles_Spearmaster_Read_Pearl = null;
			Pebbles_Spearmaster_Angry?.Unregister();
			Pebbles_Spearmaster_Angry = null;
			Moon_Pearl_SI_chat3?.Unregister();
			Moon_Pearl_SI_chat3 = null;
			Moon_Pearl_SI_chat4?.Unregister();
			Moon_Pearl_SI_chat4 = null;
			Moon_Pearl_SI_chat5?.Unregister();
			Moon_Pearl_SI_chat5 = null;
			Moon_Pearl_SU_filt?.Unregister();
			Moon_Pearl_SU_filt = null;
			Moon_Pearl_DM?.Unregister();
			Moon_Pearl_DM = null;
			Moon_Pearl_LC?.Unregister();
			Moon_Pearl_LC = null;
			Moon_Pearl_OE?.Unregister();
			Moon_Pearl_OE = null;
			Moon_Pearl_MS?.Unregister();
			Moon_Pearl_MS = null;
			Moon_Pearl_RM?.Unregister();
			Moon_Pearl_RM = null;
			Moon_Pearl_Rivulet_stomach?.Unregister();
			Moon_Pearl_Rivulet_stomach = null;
			Moon_Pearl_LC_second?.Unregister();
			Moon_Pearl_LC_second = null;
			Moon_Spearmaster_Pearl?.Unregister();
			Moon_Spearmaster_Pearl = null;
			Commercial?.Unregister();
			Commercial = null;
			Ghost_LC?.Unregister();
			Ghost_LC = null;
			Ghost_UG?.Unregister();
			Ghost_UG = null;
			Ghost_SL?.Unregister();
			Ghost_SL = null;
			Ghost_CL?.Unregister();
			Ghost_CL = null;
			Pebbles_Arty?.Unregister();
			Pebbles_Arty = null;
			Ghost_MS?.Unregister();
			Ghost_MS = null;
			Pebbles_Pearl_RM?.Unregister();
			Pebbles_Pearl_RM = null;
			Moon_RipPebbles?.Unregister();
			Moon_RipPebbles = null;
			Moon_RipPebbles_MeetingRiv?.Unregister();
			Moon_RipPebbles_MeetingRiv = null;
			Moon_Rivulet_First_Conversation?.Unregister();
			Moon_Rivulet_First_Conversation = null;
			Moon_Pearl_VS?.Unregister();
			Moon_Pearl_VS = null;
			Moon_PearlBleaching?.Unregister();
			Moon_PearlBleaching = null;
			Moon_RivuletEnding?.Unregister();
			Moon_RivuletEnding = null;
			Moon_RivuletPostgame?.Unregister();
			Moon_RivuletPostgame = null;
			Pebbles_RM_FirstMeeting?.Unregister();
			Pebbles_RM_FirstMeeting = null;
			Pebbles_Gourmand?.Unregister();
			Pebbles_Gourmand = null;
			Moon_Pearl_BroadcastMisc?.Unregister();
			Moon_Pearl_BroadcastMisc = null;
			Moon_Saint_Echo_Blocked?.Unregister();
			Moon_Saint_Echo_Blocked = null;
			Moon_Saint_First_Conversation?.Unregister();
			Moon_Saint_First_Conversation = null;
			Moon_Gourmand_First_Conversation?.Unregister();
			Moon_Gourmand_First_Conversation = null;
			Moon_HR?.Unregister();
			Moon_HR = null;
			Pebbles_HR?.Unregister();
			Pebbles_HR = null;
			Moon_Pebbles_HR?.Unregister();
			Moon_Pebbles_HR = null;
		}
	}

	public class GhostID
	{
		public static GhostWorldPresence.GhostID LC;

		public static GhostWorldPresence.GhostID UG;

		public static GhostWorldPresence.GhostID SL;

		public static GhostWorldPresence.GhostID CL;

		public static GhostWorldPresence.GhostID MS;

		public static void RegisterValues()
		{
			LC = new GhostWorldPresence.GhostID("LC", register: true);
			UG = new GhostWorldPresence.GhostID("UG", register: true);
			SL = new GhostWorldPresence.GhostID("SL", register: true);
			CL = new GhostWorldPresence.GhostID("CL", register: true);
			MS = new GhostWorldPresence.GhostID("MS", register: true);
		}

		public static void UnregisterValues()
		{
			LC?.Unregister();
			LC = null;
			UG?.Unregister();
			UG = null;
			SL?.Unregister();
			SL = null;
			CL?.Unregister();
			CL = null;
			MS?.Unregister();
			MS = null;
		}
	}

	public class CreatureTemplateType
	{
		public static CreatureTemplate.Type MirosVulture;

		public static CreatureTemplate.Type SpitLizard;

		public static CreatureTemplate.Type EelLizard;

		public static CreatureTemplate.Type MotherSpider;

		public static CreatureTemplate.Type TerrorLongLegs;

		public static CreatureTemplate.Type AquaCenti;

		public static CreatureTemplate.Type HunterDaddy;

		public static CreatureTemplate.Type FireBug;

		public static CreatureTemplate.Type StowawayBug;

		public static CreatureTemplate.Type ScavengerElite;

		public static CreatureTemplate.Type Inspector;

		public static CreatureTemplate.Type Yeek;

		public static CreatureTemplate.Type BigJelly;

		public static CreatureTemplate.Type SlugNPC;

		public static CreatureTemplate.Type JungleLeech;

		public static CreatureTemplate.Type ZoopLizard;

		public static CreatureTemplate.Type ScavengerKing;

		public static CreatureTemplate.Type TrainLizard;

		public static void RegisterValues()
		{
			MirosVulture = new CreatureTemplate.Type("MirosVulture", register: true);
			SpitLizard = new CreatureTemplate.Type("SpitLizard", register: true);
			EelLizard = new CreatureTemplate.Type("EelLizard", register: true);
			MotherSpider = new CreatureTemplate.Type("MotherSpider", register: true);
			TerrorLongLegs = new CreatureTemplate.Type("TerrorLongLegs", register: true);
			AquaCenti = new CreatureTemplate.Type("AquaCenti", register: true);
			HunterDaddy = new CreatureTemplate.Type("HunterDaddy", register: true);
			FireBug = new CreatureTemplate.Type("FireBug", register: true);
			StowawayBug = new CreatureTemplate.Type("StowawayBug", register: true);
			ScavengerElite = new CreatureTemplate.Type("ScavengerElite", register: true);
			Inspector = new CreatureTemplate.Type("Inspector", register: true);
			Yeek = new CreatureTemplate.Type("Yeek", register: true);
			BigJelly = new CreatureTemplate.Type("BigJelly", register: true);
			SlugNPC = new CreatureTemplate.Type("SlugNPC", register: true);
			JungleLeech = new CreatureTemplate.Type("JungleLeech", register: true);
			ZoopLizard = new CreatureTemplate.Type("ZoopLizard", register: true);
			ScavengerKing = new CreatureTemplate.Type("ScavengerKing", register: true);
			TrainLizard = new CreatureTemplate.Type("TrainLizard", register: true);
		}

		public static void UnregisterValues()
		{
			MirosVulture?.Unregister();
			MirosVulture = null;
			SpitLizard?.Unregister();
			SpitLizard = null;
			EelLizard?.Unregister();
			EelLizard = null;
			MotherSpider?.Unregister();
			MotherSpider = null;
			TerrorLongLegs?.Unregister();
			TerrorLongLegs = null;
			AquaCenti?.Unregister();
			AquaCenti = null;
			HunterDaddy?.Unregister();
			HunterDaddy = null;
			FireBug?.Unregister();
			FireBug = null;
			StowawayBug?.Unregister();
			StowawayBug = null;
			ScavengerElite?.Unregister();
			ScavengerElite = null;
			Inspector?.Unregister();
			Inspector = null;
			Yeek?.Unregister();
			Yeek = null;
			BigJelly?.Unregister();
			BigJelly = null;
			SlugNPC?.Unregister();
			SlugNPC = null;
			JungleLeech?.Unregister();
			JungleLeech = null;
			ZoopLizard?.Unregister();
			ZoopLizard = null;
			ScavengerKing?.Unregister();
			ScavengerKing = null;
			TrainLizard?.Unregister();
			TrainLizard = null;
		}
	}

	public class DataPearlType
	{
		public static DataPearl.AbstractDataPearl.DataPearlType Spearmasterpearl;

		public static DataPearl.AbstractDataPearl.DataPearlType SU_filt;

		public static DataPearl.AbstractDataPearl.DataPearlType SI_chat3;

		public static DataPearl.AbstractDataPearl.DataPearlType SI_chat4;

		public static DataPearl.AbstractDataPearl.DataPearlType SI_chat5;

		public static DataPearl.AbstractDataPearl.DataPearlType DM;

		public static DataPearl.AbstractDataPearl.DataPearlType LC;

		public static DataPearl.AbstractDataPearl.DataPearlType OE;

		public static DataPearl.AbstractDataPearl.DataPearlType MS;

		public static DataPearl.AbstractDataPearl.DataPearlType RM;

		public static DataPearl.AbstractDataPearl.DataPearlType Rivulet_stomach;

		public static DataPearl.AbstractDataPearl.DataPearlType LC_second;

		public static DataPearl.AbstractDataPearl.DataPearlType CL;

		public static DataPearl.AbstractDataPearl.DataPearlType VS;

		public static DataPearl.AbstractDataPearl.DataPearlType BroadcastMisc;

		public static void RegisterValues()
		{
			Spearmasterpearl = new DataPearl.AbstractDataPearl.DataPearlType("Spearmasterpearl", register: true);
			SU_filt = new DataPearl.AbstractDataPearl.DataPearlType("SU_filt", register: true);
			SI_chat3 = new DataPearl.AbstractDataPearl.DataPearlType("SI_chat3", register: true);
			SI_chat4 = new DataPearl.AbstractDataPearl.DataPearlType("SI_chat4", register: true);
			SI_chat5 = new DataPearl.AbstractDataPearl.DataPearlType("SI_chat5", register: true);
			DM = new DataPearl.AbstractDataPearl.DataPearlType("DM", register: true);
			LC = new DataPearl.AbstractDataPearl.DataPearlType("LC", register: true);
			OE = new DataPearl.AbstractDataPearl.DataPearlType("OE", register: true);
			MS = new DataPearl.AbstractDataPearl.DataPearlType("MS", register: true);
			RM = new DataPearl.AbstractDataPearl.DataPearlType("RM", register: true);
			Rivulet_stomach = new DataPearl.AbstractDataPearl.DataPearlType("Rivulet_stomach", register: true);
			LC_second = new DataPearl.AbstractDataPearl.DataPearlType("LC_second", register: true);
			CL = new DataPearl.AbstractDataPearl.DataPearlType("CL", register: true);
			VS = new DataPearl.AbstractDataPearl.DataPearlType("VS", register: true);
			BroadcastMisc = new DataPearl.AbstractDataPearl.DataPearlType("BroadcastMisc", register: true);
		}

		public static void UnregisterValues()
		{
			Spearmasterpearl?.Unregister();
			Spearmasterpearl = null;
			SU_filt?.Unregister();
			SU_filt = null;
			SI_chat3?.Unregister();
			SI_chat3 = null;
			SI_chat4?.Unregister();
			SI_chat4 = null;
			SI_chat5?.Unregister();
			SI_chat5 = null;
			DM?.Unregister();
			DM = null;
			LC?.Unregister();
			LC = null;
			OE?.Unregister();
			OE = null;
			MS?.Unregister();
			MS = null;
			RM?.Unregister();
			RM = null;
			Rivulet_stomach?.Unregister();
			Rivulet_stomach = null;
			LC_second?.Unregister();
			LC_second = null;
			CL?.Unregister();
			CL = null;
			VS?.Unregister();
			VS = null;
			BroadcastMisc?.Unregister();
			BroadcastMisc = null;
		}
	}

	public class DreamID
	{
		public static DreamsState.DreamID SaintKarma;

		public static DreamsState.DreamID ArtificerFamilyA;

		public static DreamsState.DreamID ArtificerFamilyB;

		public static DreamsState.DreamID ArtificerFamilyC;

		public static DreamsState.DreamID ArtificerFamilyD;

		public static DreamsState.DreamID ArtificerFamilyE;

		public static DreamsState.DreamID ArtificerFamilyF;

		public static DreamsState.DreamID ArtificerNightmare;

		public static DreamsState.DreamID Gourmand1;

		public static DreamsState.DreamID Gourmand2;

		public static DreamsState.DreamID Gourmand3;

		public static DreamsState.DreamID Gourmand4;

		public static DreamsState.DreamID Gourmand5;

		public static DreamsState.DreamID Gourmand0;

		public static void RegisterValues()
		{
			SaintKarma = new DreamsState.DreamID("SaintKarma", register: true);
			ArtificerFamilyA = new DreamsState.DreamID("ArtificerFamilyA", register: true);
			ArtificerFamilyB = new DreamsState.DreamID("ArtificerFamilyB", register: true);
			ArtificerFamilyC = new DreamsState.DreamID("ArtificerFamilyC", register: true);
			ArtificerFamilyD = new DreamsState.DreamID("ArtificerFamilyD", register: true);
			ArtificerFamilyE = new DreamsState.DreamID("ArtificerFamilyE", register: true);
			ArtificerFamilyF = new DreamsState.DreamID("ArtificerFamilyF", register: true);
			ArtificerNightmare = new DreamsState.DreamID("ArtificerNightmare", register: true);
			Gourmand1 = new DreamsState.DreamID("Gourmand1", register: true);
			Gourmand2 = new DreamsState.DreamID("Gourmand2", register: true);
			Gourmand3 = new DreamsState.DreamID("Gourmand3", register: true);
			Gourmand4 = new DreamsState.DreamID("Gourmand4", register: true);
			Gourmand5 = new DreamsState.DreamID("Gourmand5", register: true);
			Gourmand0 = new DreamsState.DreamID("Gourmand0", register: true);
		}

		public static void UnregisterValues()
		{
			SaintKarma?.Unregister();
			SaintKarma = null;
			ArtificerFamilyA?.Unregister();
			ArtificerFamilyA = null;
			ArtificerFamilyB?.Unregister();
			ArtificerFamilyB = null;
			ArtificerFamilyC?.Unregister();
			ArtificerFamilyC = null;
			ArtificerFamilyD?.Unregister();
			ArtificerFamilyD = null;
			ArtificerFamilyE?.Unregister();
			ArtificerFamilyE = null;
			ArtificerFamilyF?.Unregister();
			ArtificerFamilyF = null;
			ArtificerNightmare?.Unregister();
			ArtificerNightmare = null;
			Gourmand1?.Unregister();
			Gourmand1 = null;
			Gourmand2?.Unregister();
			Gourmand2 = null;
			Gourmand3?.Unregister();
			Gourmand3 = null;
			Gourmand4?.Unregister();
			Gourmand4 = null;
			Gourmand5?.Unregister();
			Gourmand5 = null;
			Gourmand0?.Unregister();
			Gourmand0 = null;
		}
	}

	public class DeathRainMode
	{
		public static GlobalRain.DeathRain.DeathRainMode Pulses;

		public static void RegisterValues()
		{
			Pulses = new GlobalRain.DeathRain.DeathRainMode("Pulses", register: true);
		}

		public static void UnregisterValues()
		{
			if (Pulses != null)
			{
				Pulses.Unregister();
				Pulses = null;
			}
		}
	}

	public class OracleID
	{
		public static Oracle.OracleID SS_Cutscene;

		public static Oracle.OracleID SL_Cutscene;

		public static Oracle.OracleID ST_Cutscene;

		public static Oracle.OracleID DM;

		public static Oracle.OracleID ST;

		public static Oracle.OracleID CL;

		public static void RegisterValues()
		{
			SS_Cutscene = new Oracle.OracleID("SS_Cutscene", register: true);
			SL_Cutscene = new Oracle.OracleID("SL_Cutscene", register: true);
			ST_Cutscene = new Oracle.OracleID("ST_Cutscene", register: true);
			DM = new Oracle.OracleID("DM", register: true);
			ST = new Oracle.OracleID("ST", register: true);
			CL = new Oracle.OracleID("CL", register: true);
		}

		public static void UnregisterValues()
		{
			SS_Cutscene?.Unregister();
			SS_Cutscene = null;
			SL_Cutscene?.Unregister();
			SL_Cutscene = null;
			ST_Cutscene?.Unregister();
			ST_Cutscene = null;
			DM?.Unregister();
			DM = null;
			ST?.Unregister();
			ST = null;
			CL?.Unregister();
			CL = null;
		}
	}

	public class PlacedObjectType
	{
		public static PlacedObject.Type GreenToken;

		public static PlacedObject.Type WhiteToken;

		public static PlacedObject.Type Germinator;

		public static PlacedObject.Type RedToken;

		public static PlacedObject.Type OEsphere;

		public static PlacedObject.Type MSArteryPush;

		public static PlacedObject.Type GooieDuck;

		public static PlacedObject.Type LillyPuck;

		public static PlacedObject.Type GlowWeed;

		public static PlacedObject.Type BigJellyFish;

		public static PlacedObject.Type RotFlyPaper;

		public static PlacedObject.Type MoonCloak;

		public static PlacedObject.Type DandelionPeach;

		public static PlacedObject.Type KarmaShrine;

		public static PlacedObject.Type Stowaway;

		public static PlacedObject.Type HRGuard;

		public static PlacedObject.Type DevToken;

		public static void RegisterValues()
		{
			GreenToken = new PlacedObject.Type("GreenToken", register: true);
			WhiteToken = new PlacedObject.Type("WhiteToken", register: true);
			Germinator = new PlacedObject.Type("Germinator", register: true);
			RedToken = new PlacedObject.Type("RedToken", register: true);
			OEsphere = new PlacedObject.Type("OEsphere", register: true);
			MSArteryPush = new PlacedObject.Type("MSArteryPush", register: true);
			GooieDuck = new PlacedObject.Type("GooieDuck", register: true);
			LillyPuck = new PlacedObject.Type("LillyPuck", register: true);
			GlowWeed = new PlacedObject.Type("GlowWeed", register: true);
			BigJellyFish = new PlacedObject.Type("BigJellyFish", register: true);
			RotFlyPaper = new PlacedObject.Type("RotFlyPaper", register: true);
			MoonCloak = new PlacedObject.Type("MoonCloak", register: true);
			DandelionPeach = new PlacedObject.Type("DandelionPeach", register: true);
			KarmaShrine = new PlacedObject.Type("KarmaShrine", register: true);
			Stowaway = new PlacedObject.Type("Stowaway", register: true);
			HRGuard = new PlacedObject.Type("HRGuard", register: true);
			DevToken = new PlacedObject.Type("DevToken", register: true);
		}

		public static void UnregisterValues()
		{
			GreenToken?.Unregister();
			GreenToken = null;
			WhiteToken?.Unregister();
			WhiteToken = null;
			Germinator?.Unregister();
			Germinator = null;
			RedToken?.Unregister();
			RedToken = null;
			OEsphere?.Unregister();
			OEsphere = null;
			MSArteryPush?.Unregister();
			MSArteryPush = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			BigJellyFish?.Unregister();
			BigJellyFish = null;
			RotFlyPaper?.Unregister();
			RotFlyPaper = null;
			MoonCloak?.Unregister();
			MoonCloak = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
			KarmaShrine?.Unregister();
			KarmaShrine = null;
			Stowaway?.Unregister();
			Stowaway = null;
			HRGuard?.Unregister();
			HRGuard = null;
			DevToken?.Unregister();
			DevToken = null;
		}
	}

	public class ProcessID
	{
		public static ProcessManager.ProcessID DemoEnd;

		public static ProcessManager.ProcessID DatingSim;

		public static ProcessManager.ProcessID Collections;

		public static ProcessManager.ProcessID RandomizedGame;

		public static ProcessManager.ProcessID SpecialUnlock;

		public static ProcessManager.ProcessID KarmaToMinScreen;

		public static ProcessManager.ProcessID VengeanceGhostScreen;

		public static void RegisterValues()
		{
			DemoEnd = new ProcessManager.ProcessID("DemoEnd", register: true);
			DatingSim = new ProcessManager.ProcessID("DatingSim", register: true);
			Collections = new ProcessManager.ProcessID("Collections", register: true);
			RandomizedGame = new ProcessManager.ProcessID("RandomizedGame", register: true);
			SpecialUnlock = new ProcessManager.ProcessID("SpecialUnlock", register: true);
			KarmaToMinScreen = new ProcessManager.ProcessID("KarmaToMinScreen", register: true);
			VengeanceGhostScreen = new ProcessManager.ProcessID("VengeanceGhostScreen", register: true);
		}

		public static void UnregisterValues()
		{
			DemoEnd?.Unregister();
			DemoEnd = null;
			DatingSim?.Unregister();
			DatingSim = null;
			Collections?.Unregister();
			Collections = null;
			RandomizedGame?.Unregister();
			RandomizedGame = null;
			SpecialUnlock?.Unregister();
			SpecialUnlock = null;
			KarmaToMinScreen?.Unregister();
			KarmaToMinScreen = null;
			VengeanceGhostScreen?.Unregister();
			VengeanceGhostScreen = null;
		}
	}

	public class RoomRainDangerType
	{
		public static RoomRain.DangerType Blizzard;

		public static void RegisterValues()
		{
			Blizzard = new RoomRain.DangerType("Blizzard", register: true);
		}

		public static void UnregisterValues()
		{
			if (Blizzard != null)
			{
				Blizzard.Unregister();
				Blizzard = null;
			}
		}
	}

	public class RoomEffectType
	{
		public static RoomSettings.RoomEffect.Type Advertisements;

		public static RoomSettings.RoomEffect.Type InvertedWater;

		public static RoomSettings.RoomEffect.Type RoomWrap;

		public static RoomSettings.RoomEffect.Type FastFloodDrain;

		public static RoomSettings.RoomEffect.Type FastFloodPullDown;

		public static RoomSettings.RoomEffect.Type DustWave;

		public static RoomSettings.RoomEffect.Type BrokenPalette;

		public static void RegisterValues()
		{
			Advertisements = new RoomSettings.RoomEffect.Type("Advertisements", register: true);
			InvertedWater = new RoomSettings.RoomEffect.Type("InvertedWater", register: true);
			RoomWrap = new RoomSettings.RoomEffect.Type("RoomWrap", register: true);
			FastFloodDrain = new RoomSettings.RoomEffect.Type("FastFloodDrain", register: true);
			FastFloodPullDown = new RoomSettings.RoomEffect.Type("FastFloodPullDown", register: true);
			DustWave = new RoomSettings.RoomEffect.Type("DustWave", register: true);
			BrokenPalette = new RoomSettings.RoomEffect.Type("BrokenPalette", register: true);
		}

		public static void UnregisterValues()
		{
			Advertisements?.Unregister();
			Advertisements = null;
			InvertedWater?.Unregister();
			InvertedWater = null;
			RoomWrap?.Unregister();
			RoomWrap = null;
			FastFloodDrain?.Unregister();
			FastFloodDrain = null;
			FastFloodPullDown?.Unregister();
			FastFloodPullDown = null;
			DustWave?.Unregister();
			DustWave = null;
			BrokenPalette?.Unregister();
			BrokenPalette = null;
		}
	}

	public class MiscItemType
	{
		public static SLOracleBehaviorHasMark.MiscItemType SingularityGrenade;

		public static SLOracleBehaviorHasMark.MiscItemType EnergyCell;

		public static SLOracleBehaviorHasMark.MiscItemType ElectricSpear;

		public static SLOracleBehaviorHasMark.MiscItemType InspectorEye;

		public static SLOracleBehaviorHasMark.MiscItemType GooieDuck;

		public static SLOracleBehaviorHasMark.MiscItemType NeedleEgg;

		public static SLOracleBehaviorHasMark.MiscItemType LillyPuck;

		public static SLOracleBehaviorHasMark.MiscItemType GlowWeed;

		public static SLOracleBehaviorHasMark.MiscItemType DandelionPeach;

		public static SLOracleBehaviorHasMark.MiscItemType MoonCloak;

		public static SLOracleBehaviorHasMark.MiscItemType EliteMask;

		public static SLOracleBehaviorHasMark.MiscItemType KingMask;

		public static SLOracleBehaviorHasMark.MiscItemType FireEgg;

		public static SLOracleBehaviorHasMark.MiscItemType SpearmasterSpear;

		public static SLOracleBehaviorHasMark.MiscItemType Seed;

		public static void RegisterValues()
		{
			SingularityGrenade = new SLOracleBehaviorHasMark.MiscItemType("SingularityGrenade", register: true);
			EnergyCell = new SLOracleBehaviorHasMark.MiscItemType("EnergyCell", register: true);
			ElectricSpear = new SLOracleBehaviorHasMark.MiscItemType("ElectricSpear", register: true);
			InspectorEye = new SLOracleBehaviorHasMark.MiscItemType("InspectorEye", register: true);
			GooieDuck = new SLOracleBehaviorHasMark.MiscItemType("GooieDuck", register: true);
			NeedleEgg = new SLOracleBehaviorHasMark.MiscItemType("NeedleEgg", register: true);
			LillyPuck = new SLOracleBehaviorHasMark.MiscItemType("LillyPuck", register: true);
			GlowWeed = new SLOracleBehaviorHasMark.MiscItemType("GlowWeed", register: true);
			DandelionPeach = new SLOracleBehaviorHasMark.MiscItemType("DandelionPeach", register: true);
			MoonCloak = new SLOracleBehaviorHasMark.MiscItemType("MoonCloak", register: true);
			EliteMask = new SLOracleBehaviorHasMark.MiscItemType("EliteMask", register: true);
			KingMask = new SLOracleBehaviorHasMark.MiscItemType("KingMask", register: true);
			FireEgg = new SLOracleBehaviorHasMark.MiscItemType("FireEgg", register: true);
			SpearmasterSpear = new SLOracleBehaviorHasMark.MiscItemType("SpearmasterSpear", register: true);
			Seed = new SLOracleBehaviorHasMark.MiscItemType("Seed", register: true);
		}

		public static void UnregisterValues()
		{
			SingularityGrenade?.Unregister();
			SingularityGrenade = null;
			EnergyCell?.Unregister();
			EnergyCell = null;
			ElectricSpear?.Unregister();
			ElectricSpear = null;
			InspectorEye?.Unregister();
			InspectorEye = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			NeedleEgg?.Unregister();
			NeedleEgg = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
			MoonCloak?.Unregister();
			MoonCloak = null;
			EliteMask?.Unregister();
			EliteMask = null;
			KingMask?.Unregister();
			KingMask = null;
			FireEgg?.Unregister();
			FireEgg = null;
			SpearmasterSpear?.Unregister();
			SpearmasterSpear = null;
			Seed?.Unregister();
			Seed = null;
		}
	}

	public class SlugcatStatsName
	{
		public static SlugcatStats.Name Rivulet;

		public static SlugcatStats.Name Artificer;

		public static SlugcatStats.Name Saint;

		public static SlugcatStats.Name Spear;

		public static SlugcatStats.Name Gourmand;

		public static SlugcatStats.Name Slugpup;

		public static SlugcatStats.Name Sofanthiel;

		public static void RegisterValues()
		{
			Rivulet = new SlugcatStats.Name("Rivulet", register: true);
			Artificer = new SlugcatStats.Name("Artificer", register: true);
			Saint = new SlugcatStats.Name("Saint", register: true);
			Spear = new SlugcatStats.Name("Spear", register: true);
			Gourmand = new SlugcatStats.Name("Gourmand", register: true);
			Slugpup = new SlugcatStats.Name("Slugpup", register: true);
			Sofanthiel = new SlugcatStats.Name("Inv", register: true);
		}

		public static void UnregisterValues()
		{
			Rivulet?.Unregister();
			Rivulet = null;
			Artificer?.Unregister();
			Artificer = null;
			Saint?.Unregister();
			Saint = null;
			Spear?.Unregister();
			Spear = null;
			Gourmand?.Unregister();
			Gourmand = null;
			Slugpup?.Unregister();
			Slugpup = null;
			Sofanthiel?.Unregister();
			Sofanthiel = null;
		}
	}

	public class SSOracleBehaviorAction
	{
		public static SSOracleBehavior.Action MeetPurple_Init;

		public static SSOracleBehavior.Action MeetPurple_GetPearl;

		public static SSOracleBehavior.Action MeetPurple_InspectPearl;

		public static SSOracleBehavior.Action MeetPurple_anger;

		public static SSOracleBehavior.Action MeetPurple_killoverseer;

		public static SSOracleBehavior.Action MeetPurple_getout;

		public static SSOracleBehavior.Action MeetPurple_markeddialog;

		public static SSOracleBehavior.Action Moon_SlumberParty;

		public static SSOracleBehavior.Action Moon_BeforeGiveMark;

		public static SSOracleBehavior.Action Moon_AfterGiveMark;

		public static SSOracleBehavior.Action MeetWhite_ThirdCurious;

		public static SSOracleBehavior.Action MeetWhite_SecondImages;

		public static SSOracleBehavior.Action MeetWhite_StartDialog;

		public static SSOracleBehavior.Action MeetInv_Init;

		public static SSOracleBehavior.Action MeetArty_Init;

		public static SSOracleBehavior.Action MeetArty_Talking;

		public static SSOracleBehavior.Action Pebbles_SlumberParty;

		public static SSOracleBehavior.Action ThrowOut_Singularity;

		public static SSOracleBehavior.Action MeetGourmand_Init;

		public static SSOracleBehavior.Action Rubicon;

		public static void RegisterValues()
		{
			MeetPurple_Init = new SSOracleBehavior.Action("MeetPurple_Init", register: true);
			MeetPurple_GetPearl = new SSOracleBehavior.Action("MeetPurple_GetPearl", register: true);
			MeetPurple_InspectPearl = new SSOracleBehavior.Action("MeetPurple_InspectPearl", register: true);
			MeetPurple_anger = new SSOracleBehavior.Action("MeetPurple_anger", register: true);
			MeetPurple_killoverseer = new SSOracleBehavior.Action("MeetPurple_killoverseer", register: true);
			MeetPurple_getout = new SSOracleBehavior.Action("MeetPurple_getout", register: true);
			MeetPurple_markeddialog = new SSOracleBehavior.Action("MeetPurple_markeddialog", register: true);
			Moon_SlumberParty = new SSOracleBehavior.Action("Moon_SlumberParty", register: true);
			Moon_BeforeGiveMark = new SSOracleBehavior.Action("Moon_BeforeGiveMark", register: true);
			Moon_AfterGiveMark = new SSOracleBehavior.Action("Moon_AfterGiveMark", register: true);
			MeetWhite_ThirdCurious = new SSOracleBehavior.Action("MeetWhite_ThirdCurious", register: true);
			MeetWhite_SecondImages = new SSOracleBehavior.Action("MeetWhite_SecondImages", register: true);
			MeetWhite_StartDialog = new SSOracleBehavior.Action("MeetWhite_StartDialog", register: true);
			MeetInv_Init = new SSOracleBehavior.Action("MeetInv_Init", register: true);
			MeetArty_Init = new SSOracleBehavior.Action("MeetArty_Init", register: true);
			MeetArty_Talking = new SSOracleBehavior.Action("MeetArty_Talking", register: true);
			Pebbles_SlumberParty = new SSOracleBehavior.Action("Pebbles_SlumberParty", register: true);
			ThrowOut_Singularity = new SSOracleBehavior.Action("ThrowOut_Singularity", register: true);
			MeetGourmand_Init = new SSOracleBehavior.Action("MeetGourmand_Init", register: true);
			Rubicon = new SSOracleBehavior.Action("Rubicon", register: true);
		}

		public static void UnregisterValues()
		{
			MeetPurple_Init?.Unregister();
			MeetPurple_Init = null;
			MeetPurple_GetPearl?.Unregister();
			MeetPurple_GetPearl = null;
			MeetPurple_InspectPearl?.Unregister();
			MeetPurple_InspectPearl = null;
			MeetPurple_anger?.Unregister();
			MeetPurple_anger = null;
			MeetPurple_killoverseer?.Unregister();
			MeetPurple_killoverseer = null;
			MeetPurple_getout?.Unregister();
			MeetPurple_getout = null;
			MeetPurple_markeddialog?.Unregister();
			MeetPurple_markeddialog = null;
			Moon_SlumberParty?.Unregister();
			Moon_SlumberParty = null;
			Moon_BeforeGiveMark?.Unregister();
			Moon_BeforeGiveMark = null;
			Moon_AfterGiveMark?.Unregister();
			Moon_AfterGiveMark = null;
			MeetWhite_ThirdCurious?.Unregister();
			MeetWhite_ThirdCurious = null;
			MeetWhite_SecondImages?.Unregister();
			MeetWhite_SecondImages = null;
			MeetWhite_StartDialog?.Unregister();
			MeetWhite_StartDialog = null;
			MeetInv_Init?.Unregister();
			MeetInv_Init = null;
			MeetArty_Init?.Unregister();
			MeetArty_Init = null;
			MeetArty_Talking?.Unregister();
			MeetArty_Talking = null;
			Pebbles_SlumberParty?.Unregister();
			Pebbles_SlumberParty = null;
			ThrowOut_Singularity?.Unregister();
			ThrowOut_Singularity = null;
			MeetGourmand_Init?.Unregister();
			MeetGourmand_Init = null;
			Rubicon?.Unregister();
			Rubicon = null;
		}
	}

	public class SSOracleBehaviorSubBehavID
	{
		public static SSOracleBehavior.SubBehavior.SubBehavID MeetPurple;

		public static SSOracleBehavior.SubBehavior.SubBehavID SlumberParty;

		public static SSOracleBehavior.SubBehavior.SubBehavID Commercial;

		public static SSOracleBehavior.SubBehavior.SubBehavID MeetArty;

		public static SSOracleBehavior.SubBehavior.SubBehavID MeetGourmand;

		public static SSOracleBehavior.SubBehavior.SubBehavID Rubicon;

		public static void RegisterValues()
		{
			MeetPurple = new SSOracleBehavior.SubBehavior.SubBehavID("MeetPurple", register: true);
			SlumberParty = new SSOracleBehavior.SubBehavior.SubBehavID("SlumberParty", register: true);
			Commercial = new SSOracleBehavior.SubBehavior.SubBehavID("Commercial", register: true);
			MeetArty = new SSOracleBehavior.SubBehavior.SubBehavID("MeetArty", register: true);
			MeetGourmand = new SSOracleBehavior.SubBehavior.SubBehavID("MeetGourmand", register: true);
			Rubicon = new SSOracleBehavior.SubBehavior.SubBehavID("Rubicon", register: true);
		}

		public static void UnregisterValues()
		{
			MeetPurple?.Unregister();
			MeetPurple = null;
			SlumberParty?.Unregister();
			SlumberParty = null;
			Commercial?.Unregister();
			Commercial = null;
			MeetArty?.Unregister();
			MeetArty = null;
			MeetGourmand?.Unregister();
			MeetGourmand = null;
			Rubicon?.Unregister();
			Rubicon = null;
		}
	}

	public class TickerID
	{
		public static StoryGameStatisticsScreen.TickerID MetMoon;

		public static StoryGameStatisticsScreen.TickerID MetPebbles;

		public static StoryGameStatisticsScreen.TickerID PearlsRead;

		public static StoryGameStatisticsScreen.TickerID GourmandQuestFinished;

		public static StoryGameStatisticsScreen.TickerID FriendsSaved;

		public static void RegisterValues()
		{
			MetMoon = new StoryGameStatisticsScreen.TickerID("MetMoon", register: true);
			MetPebbles = new StoryGameStatisticsScreen.TickerID("MetPebbles", register: true);
			PearlsRead = new StoryGameStatisticsScreen.TickerID("PearlsRead", register: true);
			GourmandQuestFinished = new StoryGameStatisticsScreen.TickerID("GourmandQuestFinished", register: true);
			FriendsSaved = new StoryGameStatisticsScreen.TickerID("FriendsSaved", register: true);
		}

		public static void UnregisterValues()
		{
			MetMoon?.Unregister();
			MetMoon = null;
			MetPebbles?.Unregister();
			MetPebbles = null;
			PearlsRead?.Unregister();
			PearlsRead = null;
			GourmandQuestFinished?.Unregister();
			GourmandQuestFinished = null;
			FriendsSaved?.Unregister();
			FriendsSaved = null;
		}
	}

	public class OverseerHologramMessage
	{
		public static OverseerHologram.Message Advertisement;

		public static void RegisterValues()
		{
			Advertisement = new OverseerHologram.Message("Advertisement", register: true);
		}

		public static void UnregisterValues()
		{
			Advertisement?.Unregister();
			Advertisement = null;
		}
	}

	public class MainWormBehaviorPhase
	{
		public static VoidWorm.MainWormBehavior.Phase Ascended;

		public static void RegisterValues()
		{
			Ascended = new VoidWorm.MainWormBehavior.Phase("Ascended", register: true);
		}

		public static void UnregisterValues()
		{
			Ascended?.Unregister();
			Ascended = null;
		}
	}

	public class MenuSceneID
	{
		public static MenuScene.SceneID Slugcat_Gourmand;

		public static MenuScene.SceneID Slugcat_Rivulet;

		public static MenuScene.SceneID Slugcat_Artificer;

		public static MenuScene.SceneID Slugcat_Saint;

		public static MenuScene.SceneID Slugcat_Saint_Max;

		public static MenuScene.SceneID End_Gourmand;

		public static MenuScene.SceneID End_Rivulet;

		public static MenuScene.SceneID End_Artificer;

		public static MenuScene.SceneID End_Saint;

		public static MenuScene.SceneID Landscape_MS;

		public static MenuScene.SceneID Landscape_LC;

		public static MenuScene.SceneID Landscape_HR;

		public static MenuScene.SceneID Landscape_OE;

		public static MenuScene.SceneID Intro_S1;

		public static MenuScene.SceneID Intro_S2;

		public static MenuScene.SceneID Intro_S3;

		public static MenuScene.SceneID Intro_S4;

		public static MenuScene.SceneID Slugcat_Spear;

		public static MenuScene.SceneID End_Spear;

		public static MenuScene.SceneID Landscape_DM;

		public static MenuScene.SceneID Landscape_LM;

		public static MenuScene.SceneID SaintMaxKarma;

		public static MenuScene.SceneID AltEnd_Spearmaster;

		public static MenuScene.SceneID AltEnd_Rivulet;

		public static MenuScene.SceneID AltEnd_Gourmand;

		public static MenuScene.SceneID Slugcat_Inv;

		public static MenuScene.SceneID End_Inv;

		public static MenuScene.SceneID Slugcat_Artificer_Robo;

		public static MenuScene.SceneID Slugcat_Artificer_Robo2;

		public static MenuScene.SceneID Landscape_CL;

		public static MenuScene.SceneID Landscape_RM;

		public static MenuScene.SceneID Landscape_VS;

		public static MenuScene.SceneID Landscape_UG;

		public static MenuScene.SceneID Slugcat_Rivulet_Cell;

		public static MenuScene.SceneID AltEnd_Rivulet_Robe;

		public static MenuScene.SceneID Endgame_Nomad;

		public static MenuScene.SceneID Endgame_Pilgrim;

		public static MenuScene.SceneID Outro_Rivulet1;

		public static MenuScene.SceneID Outro_Rivulet2L;

		public static MenuScene.SceneID Outro_Rivulet2L2;

		public static MenuScene.SceneID Outro_Rivulet2L3;

		public static MenuScene.SceneID Outro_Rivulet2L0;

		public static MenuScene.SceneID Outro_Artificer1;

		public static MenuScene.SceneID Outro_Artificer2;

		public static MenuScene.SceneID Outro_Artificer3;

		public static MenuScene.SceneID Outro_Artificer4;

		public static MenuScene.SceneID Outro_Artificer5;

		public static MenuScene.SceneID Outro_Hunter_1_Swim;

		public static MenuScene.SceneID Outro_Hunter_2_Sink;

		public static MenuScene.SceneID Outro_Hunter_3_Embrace;

		public static MenuScene.SceneID Outro_Monk_1_Swim;

		public static MenuScene.SceneID Outro_Monk_2_Reach;

		public static MenuScene.SceneID Outro_Monk_3_Stop;

		public static MenuScene.SceneID AltEnd_Artificer_1;

		public static MenuScene.SceneID AltEnd_Artificer_2;

		public static MenuScene.SceneID AltEnd_Artificer_3;

		public static MenuScene.SceneID AltEnd_Vanilla_1;

		public static MenuScene.SceneID AltEnd_Vanilla_2;

		public static MenuScene.SceneID AltEnd_Vanilla_3;

		public static MenuScene.SceneID AltEnd_Vanilla_4;

		public static MenuScene.SceneID AltEnd_Survivor;

		public static MenuScene.SceneID AltEnd_Monk;

		public static MenuScene.SceneID AltEnd_Gourmand_Full;

		public static MenuScene.SceneID AltEnd_Artificer_Portrait;

		public static MenuScene.SceneID Outro_Gourmand1;

		public static MenuScene.SceneID Outro_Gourmand2;

		public static MenuScene.SceneID Outro_Gourmand3;

		public static MenuScene.SceneID Gourmand_Dream1;

		public static MenuScene.SceneID Gourmand_Dream2;

		public static MenuScene.SceneID Gourmand_Dream3;

		public static MenuScene.SceneID Gourmand_Dream4;

		public static MenuScene.SceneID Gourmand_Dream5;

		public static MenuScene.SceneID Gourmand_Dream_Start;

		public static void RegisterValues()
		{
			Slugcat_Gourmand = new MenuScene.SceneID("Slugcat_Gourmand", register: true);
			Slugcat_Rivulet = new MenuScene.SceneID("Slugcat_Rivulet", register: true);
			Slugcat_Artificer = new MenuScene.SceneID("Slugcat_Artificer", register: true);
			Slugcat_Saint = new MenuScene.SceneID("Slugcat_Saint", register: true);
			Slugcat_Saint_Max = new MenuScene.SceneID("Slugcat_Saint_Max", register: true);
			End_Gourmand = new MenuScene.SceneID("End_Gourmand", register: true);
			End_Rivulet = new MenuScene.SceneID("End_Rivulet", register: true);
			End_Artificer = new MenuScene.SceneID("End_Artificer", register: true);
			End_Saint = new MenuScene.SceneID("End_Saint", register: true);
			Landscape_MS = new MenuScene.SceneID("Landscape_MS", register: true);
			Landscape_LC = new MenuScene.SceneID("Landscape_LC", register: true);
			Landscape_HR = new MenuScene.SceneID("Landscape_HR", register: true);
			Landscape_OE = new MenuScene.SceneID("Landscape_OE", register: true);
			Intro_S1 = new MenuScene.SceneID("Intro_S1", register: true);
			Intro_S2 = new MenuScene.SceneID("Intro_S2", register: true);
			Intro_S3 = new MenuScene.SceneID("Intro_S3", register: true);
			Intro_S4 = new MenuScene.SceneID("Intro_S4", register: true);
			Slugcat_Spear = new MenuScene.SceneID("Slugcat_Spear", register: true);
			End_Spear = new MenuScene.SceneID("End_Spear", register: true);
			Landscape_DM = new MenuScene.SceneID("Landscape_DM", register: true);
			Landscape_LM = new MenuScene.SceneID("Landscape_LM", register: true);
			SaintMaxKarma = new MenuScene.SceneID("SaintMaxKarma", register: true);
			AltEnd_Spearmaster = new MenuScene.SceneID("AltEnd_Spearmaster", register: true);
			AltEnd_Rivulet = new MenuScene.SceneID("AltEnd_Rivulet", register: true);
			AltEnd_Gourmand = new MenuScene.SceneID("AltEnd_Gourmand", register: true);
			Slugcat_Inv = new MenuScene.SceneID("Slugcat_Inv", register: true);
			End_Inv = new MenuScene.SceneID("End_Inv", register: true);
			Slugcat_Artificer_Robo = new MenuScene.SceneID("Slugcat_Artificer_Robo", register: true);
			Slugcat_Artificer_Robo2 = new MenuScene.SceneID("Slugcat_Artificer_Robo2", register: true);
			Landscape_CL = new MenuScene.SceneID("Landscape_CL", register: true);
			Landscape_RM = new MenuScene.SceneID("Landscape_RM", register: true);
			Landscape_VS = new MenuScene.SceneID("Landscape_VS", register: true);
			Landscape_UG = new MenuScene.SceneID("Landscape_UG", register: true);
			Slugcat_Rivulet_Cell = new MenuScene.SceneID("Slugcat_Rivulet_Cell", register: true);
			AltEnd_Rivulet_Robe = new MenuScene.SceneID("AltEnd_Rivulet_Robe", register: true);
			Endgame_Nomad = new MenuScene.SceneID("Endgame_Nomad", register: true);
			Endgame_Pilgrim = new MenuScene.SceneID("Endgame_Pilgrim", register: true);
			Outro_Rivulet1 = new MenuScene.SceneID("Outro_Rivulet1", register: true);
			Outro_Rivulet2L = new MenuScene.SceneID("Outro_Rivulet2L", register: true);
			Outro_Rivulet2L2 = new MenuScene.SceneID("Outro_Rivulet2L2", register: true);
			Outro_Rivulet2L3 = new MenuScene.SceneID("Outro_Rivulet2L3", register: true);
			Outro_Rivulet2L0 = new MenuScene.SceneID("Outro_Rivulet2L0", register: true);
			Outro_Artificer1 = new MenuScene.SceneID("Outro_Artificer1", register: true);
			Outro_Artificer2 = new MenuScene.SceneID("Outro_Artificer2", register: true);
			Outro_Artificer3 = new MenuScene.SceneID("Outro_Artificer3", register: true);
			Outro_Artificer4 = new MenuScene.SceneID("Outro_Artificer4", register: true);
			Outro_Artificer5 = new MenuScene.SceneID("Outro_Artificer5", register: true);
			Outro_Hunter_1_Swim = new MenuScene.SceneID("Outro_Hunter_1_Swim", register: true);
			Outro_Hunter_2_Sink = new MenuScene.SceneID("Outro_Hunter_2_Sink", register: true);
			Outro_Hunter_3_Embrace = new MenuScene.SceneID("Outro_Hunter_3_Embrace", register: true);
			Outro_Monk_1_Swim = new MenuScene.SceneID("Outro_Monk_1_Swim", register: true);
			Outro_Monk_2_Reach = new MenuScene.SceneID("Outro_Monk_2_Reach", register: true);
			Outro_Monk_3_Stop = new MenuScene.SceneID("Outro_Monk_3_Stop", register: true);
			AltEnd_Artificer_1 = new MenuScene.SceneID("AltEnd_Artificer_1", register: true);
			AltEnd_Artificer_2 = new MenuScene.SceneID("AltEnd_Artificer_2", register: true);
			AltEnd_Artificer_3 = new MenuScene.SceneID("AltEnd_Artificer_3", register: true);
			AltEnd_Vanilla_1 = new MenuScene.SceneID("AltEnd_Vanilla_1", register: true);
			AltEnd_Vanilla_2 = new MenuScene.SceneID("AltEnd_Vanilla_2", register: true);
			AltEnd_Vanilla_3 = new MenuScene.SceneID("AltEnd_Vanilla_3", register: true);
			AltEnd_Vanilla_4 = new MenuScene.SceneID("AltEnd_Vanilla_4", register: true);
			AltEnd_Survivor = new MenuScene.SceneID("AltEnd_Survivor", register: true);
			AltEnd_Monk = new MenuScene.SceneID("AltEnd_Monk", register: true);
			AltEnd_Gourmand_Full = new MenuScene.SceneID("AltEnd_Gourmand_Full", register: true);
			AltEnd_Artificer_Portrait = new MenuScene.SceneID("AltEnd_Artificer_Portrait", register: true);
			Outro_Gourmand1 = new MenuScene.SceneID("Outro_Gourmand1", register: true);
			Outro_Gourmand2 = new MenuScene.SceneID("Outro_Gourmand2", register: true);
			Outro_Gourmand3 = new MenuScene.SceneID("Outro_Gourmand3", register: true);
			Gourmand_Dream1 = new MenuScene.SceneID("Gourmand_Dream1", register: true);
			Gourmand_Dream2 = new MenuScene.SceneID("Gourmand_Dream2", register: true);
			Gourmand_Dream3 = new MenuScene.SceneID("Gourmand_Dream3", register: true);
			Gourmand_Dream4 = new MenuScene.SceneID("Gourmand_Dream4", register: true);
			Gourmand_Dream5 = new MenuScene.SceneID("Gourmand_Dream5", register: true);
			Gourmand_Dream_Start = new MenuScene.SceneID("Gourmand_Dream_Start", register: true);
		}

		public static void UnregisterValues()
		{
			Slugcat_Gourmand?.Unregister();
			Slugcat_Gourmand = null;
			Slugcat_Rivulet?.Unregister();
			Slugcat_Rivulet = null;
			Slugcat_Artificer?.Unregister();
			Slugcat_Artificer = null;
			Slugcat_Saint?.Unregister();
			Slugcat_Saint = null;
			Slugcat_Saint_Max?.Unregister();
			Slugcat_Saint_Max = null;
			End_Gourmand?.Unregister();
			End_Gourmand = null;
			End_Rivulet?.Unregister();
			End_Rivulet = null;
			End_Artificer?.Unregister();
			End_Artificer = null;
			End_Saint?.Unregister();
			End_Saint = null;
			Landscape_MS?.Unregister();
			Landscape_MS = null;
			Landscape_LC?.Unregister();
			Landscape_LC = null;
			Landscape_HR?.Unregister();
			Landscape_HR = null;
			Landscape_OE?.Unregister();
			Landscape_OE = null;
			Intro_S1?.Unregister();
			Intro_S1 = null;
			Intro_S2?.Unregister();
			Intro_S2 = null;
			Intro_S3?.Unregister();
			Intro_S3 = null;
			Intro_S4?.Unregister();
			Intro_S4 = null;
			Slugcat_Spear?.Unregister();
			Slugcat_Spear = null;
			End_Spear?.Unregister();
			End_Spear = null;
			Landscape_DM?.Unregister();
			Landscape_DM = null;
			Landscape_LM?.Unregister();
			Landscape_LM = null;
			SaintMaxKarma?.Unregister();
			SaintMaxKarma = null;
			AltEnd_Spearmaster?.Unregister();
			AltEnd_Spearmaster = null;
			AltEnd_Rivulet?.Unregister();
			AltEnd_Rivulet = null;
			AltEnd_Gourmand?.Unregister();
			AltEnd_Gourmand = null;
			Slugcat_Inv?.Unregister();
			Slugcat_Inv = null;
			End_Inv?.Unregister();
			End_Inv = null;
			Slugcat_Artificer_Robo?.Unregister();
			Slugcat_Artificer_Robo = null;
			Slugcat_Artificer_Robo2?.Unregister();
			Slugcat_Artificer_Robo2 = null;
			Landscape_CL?.Unregister();
			Landscape_CL = null;
			Landscape_RM?.Unregister();
			Landscape_RM = null;
			Landscape_VS?.Unregister();
			Landscape_VS = null;
			Landscape_UG?.Unregister();
			Landscape_UG = null;
			Slugcat_Rivulet_Cell?.Unregister();
			Slugcat_Rivulet_Cell = null;
			AltEnd_Rivulet_Robe?.Unregister();
			AltEnd_Rivulet_Robe = null;
			Endgame_Nomad?.Unregister();
			Endgame_Nomad = null;
			Endgame_Pilgrim?.Unregister();
			Endgame_Pilgrim = null;
			Outro_Rivulet1?.Unregister();
			Outro_Rivulet1 = null;
			Outro_Rivulet2L?.Unregister();
			Outro_Rivulet2L = null;
			Outro_Rivulet2L2?.Unregister();
			Outro_Rivulet2L2 = null;
			Outro_Rivulet2L3?.Unregister();
			Outro_Rivulet2L3 = null;
			Outro_Rivulet2L0?.Unregister();
			Outro_Rivulet2L0 = null;
			Outro_Artificer1?.Unregister();
			Outro_Artificer1 = null;
			Outro_Artificer2?.Unregister();
			Outro_Artificer2 = null;
			Outro_Artificer3?.Unregister();
			Outro_Artificer3 = null;
			Outro_Artificer4?.Unregister();
			Outro_Artificer4 = null;
			Outro_Artificer5?.Unregister();
			Outro_Artificer5 = null;
			Outro_Hunter_1_Swim?.Unregister();
			Outro_Hunter_1_Swim = null;
			Outro_Hunter_2_Sink?.Unregister();
			Outro_Hunter_2_Sink = null;
			Outro_Hunter_3_Embrace?.Unregister();
			Outro_Hunter_3_Embrace = null;
			Outro_Monk_1_Swim?.Unregister();
			Outro_Monk_1_Swim = null;
			Outro_Monk_2_Reach?.Unregister();
			Outro_Monk_2_Reach = null;
			Outro_Monk_3_Stop?.Unregister();
			Outro_Monk_3_Stop = null;
			AltEnd_Artificer_1?.Unregister();
			AltEnd_Artificer_1 = null;
			AltEnd_Artificer_2?.Unregister();
			AltEnd_Artificer_2 = null;
			AltEnd_Artificer_3?.Unregister();
			AltEnd_Artificer_3 = null;
			AltEnd_Vanilla_1?.Unregister();
			AltEnd_Vanilla_1 = null;
			AltEnd_Vanilla_2?.Unregister();
			AltEnd_Vanilla_2 = null;
			AltEnd_Vanilla_3?.Unregister();
			AltEnd_Vanilla_3 = null;
			AltEnd_Vanilla_4?.Unregister();
			AltEnd_Vanilla_4 = null;
			AltEnd_Survivor?.Unregister();
			AltEnd_Survivor = null;
			AltEnd_Monk?.Unregister();
			AltEnd_Monk = null;
			AltEnd_Gourmand_Full?.Unregister();
			AltEnd_Gourmand_Full = null;
			AltEnd_Artificer_Portrait?.Unregister();
			AltEnd_Artificer_Portrait = null;
			Outro_Gourmand1?.Unregister();
			Outro_Gourmand1 = null;
			Outro_Gourmand2?.Unregister();
			Outro_Gourmand2 = null;
			Outro_Gourmand3?.Unregister();
			Outro_Gourmand3 = null;
			Gourmand_Dream1?.Unregister();
			Gourmand_Dream1 = null;
			Gourmand_Dream2?.Unregister();
			Gourmand_Dream2 = null;
			Gourmand_Dream3?.Unregister();
			Gourmand_Dream3 = null;
			Gourmand_Dream4?.Unregister();
			Gourmand_Dream4 = null;
			Gourmand_Dream5?.Unregister();
			Gourmand_Dream5 = null;
			Gourmand_Dream_Start?.Unregister();
			Gourmand_Dream_Start = null;
		}
	}

	public class SlideShowID
	{
		public static SlideShow.SlideShowID SaintIntro;

		public static SlideShow.SlideShowID SpearmasterOutro;

		public static SlideShow.SlideShowID GourmandOutro;

		public static SlideShow.SlideShowID RivuletOutro;

		public static SlideShow.SlideShowID ArtificerOutro;

		public static SlideShow.SlideShowID InvOutro;

		public static SlideShow.SlideShowID RivuletAltEnd;

		public static SlideShow.SlideShowID ArtificerAltEnd;

		public static SlideShow.SlideShowID SurvivorAltEnd;

		public static SlideShow.SlideShowID MonkAltEnd;

		public static SlideShow.SlideShowID GourmandAltEnd;

		public static void RegisterValues()
		{
			SaintIntro = new SlideShow.SlideShowID("SaintIntro", register: true);
			SpearmasterOutro = new SlideShow.SlideShowID("SpearmasterOutro", register: true);
			GourmandOutro = new SlideShow.SlideShowID("GourmandOutro", register: true);
			RivuletOutro = new SlideShow.SlideShowID("RivuletOutro", register: true);
			ArtificerOutro = new SlideShow.SlideShowID("ArtificerOutro", register: true);
			InvOutro = new SlideShow.SlideShowID("InvOutro", register: true);
			RivuletAltEnd = new SlideShow.SlideShowID("RivuletAltEnd", register: true);
			ArtificerAltEnd = new SlideShow.SlideShowID("ArtificerAltEnd", register: true);
			SurvivorAltEnd = new SlideShow.SlideShowID("SurvivorAltEnd", register: true);
			MonkAltEnd = new SlideShow.SlideShowID("MonkAltEnd", register: true);
			GourmandAltEnd = new SlideShow.SlideShowID("GourmandAltEnd", register: true);
		}

		public static void UnregisterValues()
		{
			SaintIntro?.Unregister();
			SaintIntro = null;
			SpearmasterOutro?.Unregister();
			SpearmasterOutro = null;
			GourmandOutro?.Unregister();
			GourmandOutro = null;
			RivuletOutro?.Unregister();
			RivuletOutro = null;
			ArtificerOutro?.Unregister();
			ArtificerOutro = null;
			InvOutro?.Unregister();
			InvOutro = null;
			RivuletAltEnd?.Unregister();
			RivuletAltEnd = null;
			ArtificerAltEnd?.Unregister();
			ArtificerAltEnd = null;
			SurvivorAltEnd?.Unregister();
			SurvivorAltEnd = null;
			MonkAltEnd?.Unregister();
			MonkAltEnd = null;
			GourmandAltEnd?.Unregister();
			GourmandAltEnd = null;
		}
	}

	public class LevelUnlockID
	{
		public static MultiplayerUnlocks.LevelUnlockID LC;

		public static MultiplayerUnlocks.LevelUnlockID MS;

		public static MultiplayerUnlocks.LevelUnlockID HR;

		public static MultiplayerUnlocks.LevelUnlockID DM;

		public static MultiplayerUnlocks.LevelUnlockID OE;

		public static MultiplayerUnlocks.LevelUnlockID RM;

		public static MultiplayerUnlocks.LevelUnlockID Challenge;

		public static MultiplayerUnlocks.LevelUnlockID ChallengeOnly;

		public static MultiplayerUnlocks.LevelUnlockID CL;

		public static MultiplayerUnlocks.LevelUnlockID VS;

		public static MultiplayerUnlocks.LevelUnlockID LM;

		public static MultiplayerUnlocks.LevelUnlockID GWold;

		public static MultiplayerUnlocks.LevelUnlockID UG;

		public static MultiplayerUnlocks.LevelUnlockID gutter;

		public static MultiplayerUnlocks.LevelUnlockID filter;

		public static void RegisterValues()
		{
			LC = new MultiplayerUnlocks.LevelUnlockID("LC", register: true);
			MS = new MultiplayerUnlocks.LevelUnlockID("MS", register: true);
			HR = new MultiplayerUnlocks.LevelUnlockID("HR", register: true);
			DM = new MultiplayerUnlocks.LevelUnlockID("DM", register: true);
			OE = new MultiplayerUnlocks.LevelUnlockID("OE", register: true);
			RM = new MultiplayerUnlocks.LevelUnlockID("RM", register: true);
			Challenge = new MultiplayerUnlocks.LevelUnlockID("Challenge", register: true);
			ChallengeOnly = new MultiplayerUnlocks.LevelUnlockID("ChallengeOnly", register: true);
			CL = new MultiplayerUnlocks.LevelUnlockID("CL", register: true);
			VS = new MultiplayerUnlocks.LevelUnlockID("VS", register: true);
			LM = new MultiplayerUnlocks.LevelUnlockID("LM", register: true);
			GWold = new MultiplayerUnlocks.LevelUnlockID("GWold", register: true);
			UG = new MultiplayerUnlocks.LevelUnlockID("UG", register: true);
			gutter = new MultiplayerUnlocks.LevelUnlockID("gutter", register: true);
			filter = new MultiplayerUnlocks.LevelUnlockID("filter", register: true);
		}

		public static void UnregisterValues()
		{
			LC?.Unregister();
			LC = null;
			MS?.Unregister();
			MS = null;
			HR?.Unregister();
			HR = null;
			DM?.Unregister();
			DM = null;
			OE?.Unregister();
			OE = null;
			RM?.Unregister();
			RM = null;
			Challenge?.Unregister();
			Challenge = null;
			ChallengeOnly?.Unregister();
			ChallengeOnly = null;
			CL?.Unregister();
			CL = null;
			VS?.Unregister();
			VS = null;
			LM?.Unregister();
			LM = null;
			GWold?.Unregister();
			GWold = null;
			UG?.Unregister();
			UG = null;
			gutter?.Unregister();
			gutter = null;
			filter?.Unregister();
			filter = null;
		}
	}

	public class SandboxUnlockID
	{
		public static MultiplayerUnlocks.SandboxUnlockID MirosVulture;

		public static MultiplayerUnlocks.SandboxUnlockID SpitLizard;

		public static MultiplayerUnlocks.SandboxUnlockID EelLizard;

		public static MultiplayerUnlocks.SandboxUnlockID MotherSpider;

		public static MultiplayerUnlocks.SandboxUnlockID TerrorLongLegs;

		public static MultiplayerUnlocks.SandboxUnlockID AquaCenti;

		public static MultiplayerUnlocks.SandboxUnlockID FireBug;

		public static MultiplayerUnlocks.SandboxUnlockID FireEgg;

		public static MultiplayerUnlocks.SandboxUnlockID HellSpear;

		public static MultiplayerUnlocks.SandboxUnlockID SingularityBomb;

		public static MultiplayerUnlocks.SandboxUnlockID JokeRifle;

		public static MultiplayerUnlocks.SandboxUnlockID SlugNPC;

		public static MultiplayerUnlocks.SandboxUnlockID VultureMask;

		public static MultiplayerUnlocks.SandboxUnlockID ScavengerElite;

		public static MultiplayerUnlocks.SandboxUnlockID EnergyCell;

		public static MultiplayerUnlocks.SandboxUnlockID ElectricSpear;

		public static MultiplayerUnlocks.SandboxUnlockID Inspector;

		public static MultiplayerUnlocks.SandboxUnlockID GooieDuck;

		public static MultiplayerUnlocks.SandboxUnlockID Pearl;

		public static MultiplayerUnlocks.SandboxUnlockID Yeek;

		public static MultiplayerUnlocks.SandboxUnlockID LillyPuck;

		public static MultiplayerUnlocks.SandboxUnlockID GlowWeed;

		public static MultiplayerUnlocks.SandboxUnlockID BigJelly;

		public static MultiplayerUnlocks.SandboxUnlockID DandelionPeach;

		public static MultiplayerUnlocks.SandboxUnlockID JungleLeech;

		public static MultiplayerUnlocks.SandboxUnlockID StowawayBug;

		public static MultiplayerUnlocks.SandboxUnlockID ZoopLizard;

		public static void RegisterValues()
		{
			MirosVulture = new MultiplayerUnlocks.SandboxUnlockID("MirosVulture", register: true);
			SpitLizard = new MultiplayerUnlocks.SandboxUnlockID("SpitLizard", register: true);
			EelLizard = new MultiplayerUnlocks.SandboxUnlockID("EelLizard", register: true);
			MotherSpider = new MultiplayerUnlocks.SandboxUnlockID("MotherSpider", register: true);
			TerrorLongLegs = new MultiplayerUnlocks.SandboxUnlockID("TerrorLongLegs", register: true);
			AquaCenti = new MultiplayerUnlocks.SandboxUnlockID("AquaCenti", register: true);
			FireBug = new MultiplayerUnlocks.SandboxUnlockID("FireBug", register: true);
			FireEgg = new MultiplayerUnlocks.SandboxUnlockID("FireEgg", register: true);
			HellSpear = new MultiplayerUnlocks.SandboxUnlockID("HellSpear", register: true);
			SingularityBomb = new MultiplayerUnlocks.SandboxUnlockID("SingularityBomb", register: true);
			JokeRifle = new MultiplayerUnlocks.SandboxUnlockID("JokeRifle", register: true);
			SlugNPC = new MultiplayerUnlocks.SandboxUnlockID("SlugNPC", register: true);
			VultureMask = new MultiplayerUnlocks.SandboxUnlockID("VultureMask", register: true);
			ScavengerElite = new MultiplayerUnlocks.SandboxUnlockID("ScavengerElite", register: true);
			EnergyCell = new MultiplayerUnlocks.SandboxUnlockID("EnergyCell", register: true);
			ElectricSpear = new MultiplayerUnlocks.SandboxUnlockID("ElectricSpear", register: true);
			Inspector = new MultiplayerUnlocks.SandboxUnlockID("Inspector", register: true);
			GooieDuck = new MultiplayerUnlocks.SandboxUnlockID("GooieDuck", register: true);
			Pearl = new MultiplayerUnlocks.SandboxUnlockID("Pearl", register: true);
			Yeek = new MultiplayerUnlocks.SandboxUnlockID("Yeek", register: true);
			LillyPuck = new MultiplayerUnlocks.SandboxUnlockID("LillyPuck", register: true);
			GlowWeed = new MultiplayerUnlocks.SandboxUnlockID("GlowWeed", register: true);
			BigJelly = new MultiplayerUnlocks.SandboxUnlockID("BigJelly", register: true);
			DandelionPeach = new MultiplayerUnlocks.SandboxUnlockID("DandelionPeach", register: true);
			JungleLeech = new MultiplayerUnlocks.SandboxUnlockID("JungleLeech", register: true);
			StowawayBug = new MultiplayerUnlocks.SandboxUnlockID("StowawayBug", register: true);
			ZoopLizard = new MultiplayerUnlocks.SandboxUnlockID("ZoopLizard", register: true);
		}

		public static void UnregisterValues()
		{
			MirosVulture?.Unregister();
			MirosVulture = null;
			SpitLizard?.Unregister();
			SpitLizard = null;
			EelLizard?.Unregister();
			EelLizard = null;
			MotherSpider?.Unregister();
			MotherSpider = null;
			TerrorLongLegs?.Unregister();
			TerrorLongLegs = null;
			AquaCenti?.Unregister();
			AquaCenti = null;
			FireBug?.Unregister();
			FireBug = null;
			FireEgg?.Unregister();
			FireEgg = null;
			HellSpear?.Unregister();
			HellSpear = null;
			SingularityBomb?.Unregister();
			SingularityBomb = null;
			JokeRifle?.Unregister();
			JokeRifle = null;
			SlugNPC?.Unregister();
			SlugNPC = null;
			VultureMask?.Unregister();
			VultureMask = null;
			ScavengerElite?.Unregister();
			ScavengerElite = null;
			EnergyCell?.Unregister();
			EnergyCell = null;
			ElectricSpear?.Unregister();
			ElectricSpear = null;
			Inspector?.Unregister();
			Inspector = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			Pearl?.Unregister();
			Pearl = null;
			Yeek?.Unregister();
			Yeek = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			BigJelly?.Unregister();
			BigJelly = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
			JungleLeech?.Unregister();
			JungleLeech = null;
			StowawayBug?.Unregister();
			StowawayBug = null;
			ZoopLizard?.Unregister();
			ZoopLizard = null;
		}
	}

	public class EndgameID
	{
		public static WinState.EndgameID Gourmand;

		public static WinState.EndgameID Nomad;

		public static WinState.EndgameID Martyr;

		public static WinState.EndgameID Pilgrim;

		public static WinState.EndgameID Mother;

		public static void RegisterValues()
		{
			Gourmand = new WinState.EndgameID("Gourmand", register: true);
			Nomad = new WinState.EndgameID("Nomad", register: true);
			Martyr = new WinState.EndgameID("Martyr", register: true);
			Pilgrim = new WinState.EndgameID("Pilgrim", register: true);
			Mother = new WinState.EndgameID("Mother", register: true);
		}

		public static void UnregisterValues()
		{
			Gourmand?.Unregister();
			Gourmand = null;
			Nomad?.Unregister();
			Nomad = null;
			Martyr?.Unregister();
			Martyr = null;
			Pilgrim?.Unregister();
			Pilgrim = null;
			Mother?.Unregister();
			Mother = null;
		}
	}

	public class Tutorial
	{
		public static DeathPersistentSaveData.Tutorial Artificer;

		public static DeathPersistentSaveData.Tutorial SaintTongue;

		public static DeathPersistentSaveData.Tutorial SaintEnlight;

		public static DeathPersistentSaveData.Tutorial KarmicBurst;

		public static DeathPersistentSaveData.Tutorial Spearmaster;

		public static DeathPersistentSaveData.Tutorial SpearmasterEat;

		public static DeathPersistentSaveData.Tutorial ArtificerMaul;

		public static void RegisterValues()
		{
			Artificer = new DeathPersistentSaveData.Tutorial("Artificer", register: true);
			SaintTongue = new DeathPersistentSaveData.Tutorial("SaintTongue", register: true);
			SaintEnlight = new DeathPersistentSaveData.Tutorial("SaintEnlight", register: true);
			KarmicBurst = new DeathPersistentSaveData.Tutorial("KarmicBurst", register: true);
			Spearmaster = new DeathPersistentSaveData.Tutorial("Spearmaster", register: true);
			SpearmasterEat = new DeathPersistentSaveData.Tutorial("SpearmasterEat", register: true);
			ArtificerMaul = new DeathPersistentSaveData.Tutorial("ArtificerMaul", register: true);
		}

		public static void UnregisterValues()
		{
			Artificer?.Unregister();
			Artificer = null;
			SaintTongue?.Unregister();
			SaintTongue = null;
			SaintEnlight?.Unregister();
			SaintEnlight = null;
			KarmicBurst?.Unregister();
			KarmicBurst = null;
			Spearmaster?.Unregister();
			Spearmaster = null;
			SpearmasterEat?.Unregister();
			SpearmasterEat = null;
			ArtificerMaul?.Unregister();
			ArtificerMaul = null;
		}
	}

	public class OwnerType
	{
		public static global::HUD.HUD.OwnerType SafariOverseer;

		public static void RegisterValues()
		{
			SafariOverseer = new global::HUD.HUD.OwnerType("SafariOverseer", register: true);
		}

		public static void UnregisterValues()
		{
			SafariOverseer?.Unregister();
			SafariOverseer = null;
		}
	}

	public class GateRequirement
	{
		public static RegionGate.GateRequirement OELock;

		public static RegionGate.GateRequirement RoboLock;

		public static void RegisterValues()
		{
			OELock = new RegionGate.GateRequirement("L", register: true);
			RoboLock = new RegionGate.GateRequirement("R", register: true);
		}

		public static void UnregisterValues()
		{
			OELock?.Unregister();
			OELock = null;
			RoboLock?.Unregister();
			RoboLock = null;
		}
	}

	public class ScavengerAnimationID
	{
		public static Scavenger.ScavengerAnimation.ID PrepareToJump;

		public static Scavenger.ScavengerAnimation.ID Jumping;

		public static void RegisterValues()
		{
			PrepareToJump = new Scavenger.ScavengerAnimation.ID("PrepareToJump", register: true);
			Jumping = new Scavenger.ScavengerAnimation.ID("Jumping", register: true);
		}

		public static void UnregisterValues()
		{
			PrepareToJump?.Unregister();
			PrepareToJump = null;
			Jumping?.Unregister();
			Jumping = null;
		}
	}

	public class EggBugBehavior
	{
		public static EggBugAI.Behavior Kill;

		public static void RegisterValues()
		{
			Kill = new EggBugAI.Behavior("Kill", register: true);
		}

		public static void UnregisterValues()
		{
			Kill?.Unregister();
			Kill = null;
		}
	}

	public class MSCSoundID
	{
		public static SoundID MENU_Karma_Ladder_Start_Moving_Saint;

		public static SoundID Apple;

		public static SoundID BM_GOR01;

		public static SoundID BM_GOR02;

		public static SoundID Cap_Bump_Vengeance;

		public static SoundID Chain_Break;

		public static SoundID Chain_Lock;

		public static SoundID Core_Off;

		public static SoundID Core_On;

		public static SoundID Core_Ready;

		public static SoundID Core_Removed;

		public static SoundID Data_Bit;

		public static SoundID DreamDN;

		public static SoundID DreamN;

		public static SoundID Duck_Pop;

		public static SoundID Ghost_Ping_Base;

		public static SoundID Ghost_Ping_Start;

		public static SoundID Karma_Pitch_Discovery;

		public static SoundID Moon_Panic_Attack;

		public static SoundID Sat_Interference;

		public static SoundID Sat_Interference2;

		public static SoundID Sat_Interference3;

		public static SoundID Singularity;

		public static SoundID Sleep_Blizzard_Loop;

		public static SoundID SM_Spear_Grab;

		public static SoundID SM_Spear_Pull;

		public static SoundID Inv_GO;

		public static SoundID Inv_Hit;

		public static SoundID ST_Cry_Intro;

		public static SoundID ST_Cry;

		public static SoundID ST_Talk1;

		public static SoundID ST_Talk2;

		public static SoundID Terror_Moo;

		public static SoundID Throw_FireSpear;

		public static SoundID Volt_Shock;

		public static void RegisterValues()
		{
			MENU_Karma_Ladder_Start_Moving_Saint = new SoundID("MENU_Karma_Ladder_Start_Moving_Saint", register: true);
			Apple = new SoundID("Apple", register: true);
			BM_GOR01 = new SoundID("BM_GOR01", register: true);
			BM_GOR02 = new SoundID("BM_GOR02", register: true);
			Cap_Bump_Vengeance = new SoundID("Cap_Bump_Vengeance", register: true);
			Chain_Break = new SoundID("Chain_Break", register: true);
			Chain_Lock = new SoundID("Chain_Lock", register: true);
			Core_Off = new SoundID("Core_Off", register: true);
			Core_On = new SoundID("Core_On", register: true);
			Core_Ready = new SoundID("Core_Ready", register: true);
			Core_Removed = new SoundID("Core_Removed", register: true);
			Data_Bit = new SoundID("Data_Bit", register: true);
			DreamDN = new SoundID("DreamDN", register: true);
			DreamN = new SoundID("DreamN", register: true);
			Duck_Pop = new SoundID("Duck_Pop", register: true);
			Ghost_Ping_Base = new SoundID("Ghost_Ping_Base", register: true);
			Ghost_Ping_Start = new SoundID("Ghost_Ping_Start", register: true);
			Karma_Pitch_Discovery = new SoundID("Karma_Pitch_Discovery", register: true);
			Moon_Panic_Attack = new SoundID("Moon_Panic_Attack", register: true);
			Sat_Interference = new SoundID("Sat_Interference", register: true);
			Sat_Interference2 = new SoundID("Sat_Interference2", register: true);
			Sat_Interference3 = new SoundID("Sat_Interference3", register: true);
			Singularity = new SoundID("Singularity", register: true);
			Sleep_Blizzard_Loop = new SoundID("Sleep_Blizzard_Loop", register: true);
			SM_Spear_Grab = new SoundID("SM_Spear_Grab", register: true);
			SM_Spear_Pull = new SoundID("SM_Spear_Pull", register: true);
			Inv_GO = new SoundID("Inv_GO", register: true);
			Inv_Hit = new SoundID("Inv_Hit", register: true);
			ST_Cry_Intro = new SoundID("ST_Cry_Intro", register: true);
			ST_Cry = new SoundID("ST_Cry", register: true);
			ST_Talk1 = new SoundID("ST_Talk1", register: true);
			ST_Talk2 = new SoundID("ST_Talk2", register: true);
			Terror_Moo = new SoundID("Terror_Moo", register: true);
			Throw_FireSpear = new SoundID("Throw_FireSpear", register: true);
			Volt_Shock = new SoundID("Volt_Shock", register: true);
		}

		public static void UnregisterValues()
		{
			MENU_Karma_Ladder_Start_Moving_Saint?.Unregister();
			MENU_Karma_Ladder_Start_Moving_Saint = null;
			Apple?.Unregister();
			Apple = null;
			BM_GOR01?.Unregister();
			BM_GOR01 = null;
			BM_GOR02?.Unregister();
			BM_GOR02 = null;
			Cap_Bump_Vengeance?.Unregister();
			Cap_Bump_Vengeance = null;
			Chain_Break?.Unregister();
			Chain_Break = null;
			Chain_Lock?.Unregister();
			Chain_Lock = null;
			Core_Off?.Unregister();
			Core_Off = null;
			Core_On?.Unregister();
			Core_On = null;
			Core_Ready?.Unregister();
			Core_Ready = null;
			Core_Removed?.Unregister();
			Core_Removed = null;
			Data_Bit?.Unregister();
			Data_Bit = null;
			DreamDN?.Unregister();
			DreamDN = null;
			DreamN?.Unregister();
			DreamN = null;
			Duck_Pop?.Unregister();
			Duck_Pop = null;
			Ghost_Ping_Base?.Unregister();
			Ghost_Ping_Base = null;
			Ghost_Ping_Start?.Unregister();
			Ghost_Ping_Start = null;
			Karma_Pitch_Discovery?.Unregister();
			Karma_Pitch_Discovery = null;
			Moon_Panic_Attack?.Unregister();
			Moon_Panic_Attack = null;
			Sat_Interference?.Unregister();
			Sat_Interference = null;
			Sat_Interference2?.Unregister();
			Sat_Interference2 = null;
			Sat_Interference3?.Unregister();
			Sat_Interference3 = null;
			Singularity?.Unregister();
			Singularity = null;
			Sleep_Blizzard_Loop?.Unregister();
			Sleep_Blizzard_Loop = null;
			SM_Spear_Grab?.Unregister();
			SM_Spear_Grab = null;
			SM_Spear_Pull?.Unregister();
			SM_Spear_Pull = null;
			Inv_GO?.Unregister();
			Inv_GO = null;
			Inv_Hit?.Unregister();
			Inv_Hit = null;
			ST_Cry_Intro?.Unregister();
			ST_Cry_Intro = null;
			ST_Cry?.Unregister();
			ST_Cry = null;
			ST_Talk1?.Unregister();
			ST_Talk1 = null;
			ST_Talk2?.Unregister();
			ST_Talk2 = null;
			Terror_Moo?.Unregister();
			Terror_Moo = null;
			Throw_FireSpear?.Unregister();
			Throw_FireSpear = null;
			Volt_Shock?.Unregister();
			Volt_Shock = null;
		}
	}

	public static void InitExtEnumTypes()
	{
		_ = JokeRifle.AbstractRifle.AmmoType.Rock;
		_ = MultiplayerUnlocks.SlugcatUnlockID.Hunter;
		_ = MultiplayerUnlocks.SafariUnlockID.SU;
		_ = OverWorld.SpecialWarpType.WARP_VS_HR;
		_ = Player.Tongue.Mode.Retracted;
		_ = PlayerGraphics.AxolotlGills.SpritesOverlap.Behind;
		_ = ShelterDoor.Clamp.Mode.Stacked;
		_ = SLOracleBehavior.MovementBehavior.Idle;
		_ = VultureMask.MaskType.NORMAL;
		_ = VoidSeaScene.SaintEndingPhase.Inactive;
		_ = AncientBot.Animation.Idle;
		_ = AncientBot.FollowMode.MoveTowards;
		_ = ChatlogData.ChatlogID.Chatlog_SU0;
		_ = CollisionField.Type.POISON_SMOKE;
		_ = CutsceneArtificer.Phase.Init;
		_ = CutsceneArtificerRobo.Phase.Init;
		_ = InspectorAI.Behavior.Idle;
		_ = SlugNPCAI.Food.DangleFruit;
		_ = SlugNPCAI.BehaviorType.Idle;
		_ = STOracleBehavior.Phase.Inactive;
		_ = STOracleBehavior.Laser.Type.EXPLODE;
		_ = STOracleBehavior.SimpleDan.DestroyType.BORDER;
		_ = StowawayBugAI.Behavior.Idle;
		_ = YeekAI.Behavior.Idle;
		_ = YeekGraphics.YeekLeg.AnimState.Sit;
		_ = ChallengeInformation.ChallengeMeta.WinCondition.KILL;
		_ = CollectionsMenu.PearlReadContext.StandardMoon;
	}

	public static void RegisterAllEnumExtensions()
	{
		AbstractObjectType.RegisterValues();
		GameTypeID.RegisterValues();
		ConversationID.RegisterValues();
		CreatureTemplateType.RegisterValues();
		DataPearlType.RegisterValues();
		DreamID.RegisterValues();
		DeathRainMode.RegisterValues();
		OracleID.RegisterValues();
		PlacedObjectType.RegisterValues();
		ProcessID.RegisterValues();
		RoomRainDangerType.RegisterValues();
		RoomEffectType.RegisterValues();
		MiscItemType.RegisterValues();
		SlugcatStatsName.RegisterValues();
		SSOracleBehaviorAction.RegisterValues();
		SSOracleBehaviorSubBehavID.RegisterValues();
		TickerID.RegisterValues();
		OverseerHologramMessage.RegisterValues();
		MainWormBehaviorPhase.RegisterValues();
		MenuSceneID.RegisterValues();
		SlideShowID.RegisterValues();
		LevelUnlockID.RegisterValues();
		SandboxUnlockID.RegisterValues();
		EndgameID.RegisterValues();
		Tutorial.RegisterValues();
		GateRequirement.RegisterValues();
		GhostID.RegisterValues();
		OwnerType.RegisterValues();
		ScavengerAnimationID.RegisterValues();
		EggBugBehavior.RegisterValues();
		MSCSoundID.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		AbstractObjectType.UnregisterValues();
		GameTypeID.UnregisterValues();
		ConversationID.UnregisterValues();
		CreatureTemplateType.UnregisterValues();
		DataPearlType.UnregisterValues();
		DreamID.UnregisterValues();
		DeathRainMode.UnregisterValues();
		OracleID.UnregisterValues();
		PlacedObjectType.UnregisterValues();
		ProcessID.UnregisterValues();
		RoomRainDangerType.UnregisterValues();
		RoomEffectType.UnregisterValues();
		MiscItemType.UnregisterValues();
		SlugcatStatsName.UnregisterValues();
		SSOracleBehaviorAction.UnregisterValues();
		SSOracleBehaviorSubBehavID.UnregisterValues();
		TickerID.UnregisterValues();
		OverseerHologramMessage.UnregisterValues();
		MainWormBehaviorPhase.UnregisterValues();
		MenuSceneID.UnregisterValues();
		SlideShowID.UnregisterValues();
		LevelUnlockID.UnregisterValues();
		SandboxUnlockID.UnregisterValues();
		EndgameID.UnregisterValues();
		Tutorial.UnregisterValues();
		GateRequirement.UnregisterValues();
		GhostID.UnregisterValues();
		OwnerType.UnregisterValues();
		ScavengerAnimationID.UnregisterValues();
		EggBugBehavior.UnregisterValues();
		MSCSoundID.UnregisterValues();
	}
}
