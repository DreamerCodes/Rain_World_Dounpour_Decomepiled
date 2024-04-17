using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu;
using Modding.Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Expedition;

public static class ExpeditionGame
{
	public abstract class BurdenTracker
	{
		public RainWorldGame game;

		public BurdenTracker(RainWorldGame g)
		{
		}

		public virtual void Update()
		{
		}
	}

	public class PursuedTracker : BurdenTracker
	{
		public Player targetPlayer;

		public AbstractCreature pursuer;

		public string currentRoom;

		public string region;

		public WorldCoordinate spawnPos;

		public WorldCoordinate destination;

		public bool warning;

		public int unrealizedCounter;

		public List<string> regionCooldowns;

		public int regionSwitchCooldown;

		public PursuedTracker(RainWorldGame g)
			: base(g)
		{
			ExpLog.Log("PURSUED TRACKER INIT");
			regionCooldowns = new List<string>();
			game = g;
			unrealizedCounter = 0;
			region = game.world.region.name;
		}

		public void SpawnPosition()
		{
			if (game == null || game.world == null || game.Players == null || game.Players.Count <= 0 || game.Players[0] == null || game.Players[0].Room == null)
			{
				return;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < game.world.NumberOfRooms; i++)
			{
				AbstractRoom abstractRoom = game.world.GetAbstractRoom(game.world.firstRoomIndex + i);
				if (abstractRoom != null && !abstractRoom.shelter && !abstractRoom.gate && abstractRoom.name != game.Players[0].Room.name)
				{
					list.Add(i);
				}
			}
			AbstractRoom abstractRoom2 = game.world.GetAbstractRoom(game.world.firstRoomIndex + list[UnityEngine.Random.Range(0, list.Count)]);
			spawnPos = abstractRoom2.RandomNodeInRoom();
			ExpLog.Log("HUNTER LOCATION: " + abstractRoom2.name);
		}

		public void SetUpPlayer()
		{
			if (game == null)
			{
				return;
			}
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i] != null && game.Players[i].realizedCreature != null && !game.Players[i].realizedCreature.dead)
				{
					targetPlayer = game.Players[i].realizedCreature as Player;
					return;
				}
			}
			if (game.manager.musicPlayer != null && game.manager.musicPlayer.song?.name == "RW_20 - Polybius")
			{
				game.manager.musicPlayer.FadeOutAllNonGhostSongs(100f);
			}
		}

		public void SetUpHunter()
		{
			if (game != null && game.world != null && regionSwitchCooldown <= 0)
			{
				pursuer = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.RedCentipede), null, spawnPos, game.GetNewID());
				pursuer.voidCreature = true;
				pursuer.saveCreature = false;
				pursuer.ignoreCycle = true;
				pursuer.HypothermiaImmune = true;
				game.world.GetAbstractRoom(spawnPos).AddEntity(pursuer);
			}
		}

		public override void Update()
		{
			if (game.world == null)
			{
				return;
			}
			if (region != game.world.region.name)
			{
				region = game.world.region.name;
				regionSwitchCooldown = 2400;
			}
			regionSwitchCooldown--;
			if (pursuer != null && pursuer.Room != null)
			{
				if (pursuer.state.dead || region != pursuer.world.region.name || (pursuer.Room.shelter && pursuer.Room.realizedRoom != null && pursuer.Room.realizedRoom.shelterDoor.IsClosing))
				{
					if (pursuer.realizedCreature != null && pursuer.realizedCreature.room != null)
					{
						pursuer.realizedCreature.room.AddObject(new ShockWave(pursuer.realizedCreature.mainBodyChunk.pos, 300f, 5f, 100, highLayer: true));
						pursuer.realizedCreature.room.PlaySound(SoundID.Coral_Circuit_Break, pursuer.realizedCreature.mainBodyChunk);
						pursuer.realizedCreature.RemoveFromRoom();
						game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("The pursuer retreats..."), 10, 250, darken: true, hideHud: true);
					}
					pursuer.Destroy();
					if (pursuer.state.dead && !regionCooldowns.Contains(region))
					{
						regionCooldowns.Add(region);
					}
					pursuer = null;
					return;
				}
				SetUpPlayer();
				if (currentRoom != pursuer.Room.name)
				{
					currentRoom = pursuer.Room.name;
					ExpLog.Log("Pursuer moving to: " + currentRoom);
					for (int i = 0; i < pursuer.Room.connections.Length; i++)
					{
						for (int j = 0; j < pursuer.world.game.AlivePlayers.Count; j++)
						{
							if (pursuer.Room.connections[i] == pursuer.world.game.AlivePlayers[j].pos.room && !warning)
							{
								game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("You are being pursued..."), 10, 250, darken: true, hideHud: true);
								warning = true;
							}
						}
					}
				}
				if (pursuer.abstractAI != null && targetPlayer != null)
				{
					_ = destination;
					if (destination.room != pursuer.pos.room)
					{
						destination = targetPlayer.abstractCreature.pos;
						pursuer.abstractAI.SetDestination(destination);
					}
				}
				if (pursuer.realizedCreature == null && pursuer.timeSpentHere > 2500)
				{
					ExpLog.Log("RE-LOCATING PURSUER!");
					SpawnPosition();
					pursuer.Move(spawnPos);
					if (warning)
					{
						warning = false;
						game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("The pursuer retreats..."), 10, 250, darken: true, hideHud: true);
					}
				}
				if (pursuer.abstractAI.RealAI != null && targetPlayer != null)
				{
					pursuer.abstractAI.RealAI.tracker.SeeCreature(targetPlayer.abstractCreature);
					if (ExpeditionData.devMode && !pursuer.state.dead && Input.GetKey(KeyCode.Backspace))
					{
						pursuer.Die();
					}
					if (!warning)
					{
						game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("You are being pursued..."), 10, 250, darken: true, hideHud: true);
						warning = true;
					}
				}
			}
			else if (!regionCooldowns.Contains(region) && game.world.rainCycle.CycleProgression > 0.1f)
			{
				SetUpPlayer();
				SpawnPosition();
				SetUpHunter();
			}
		}
	}

	public class UnlockTracker
	{
		public RainWorldGame game;

		public SlugcatStats.Name slugcatPlayer;

		public int playerNumber;

		public virtual void Update()
		{
		}
	}

	public class SlowTimeTracker : UnlockTracker
	{
		public float triggerDelay;

		public float cooldown;

		public SlowTimeTracker(RainWorldGame game)
		{
			ExpLog.Log("TRACKER: SlowTimeTracker added");
			base.game = game;
		}

		public override void Update()
		{
			bool flag = false;
			if (game != null)
			{
				bool flag2 = false;
				for (int i = 0; i < game.Players.Count; i++)
				{
					if (game.Players[i].realizedCreature != null)
					{
						Player player = game.Players[i].realizedCreature as Player;
						if (player.mushroomCounter > 0)
						{
							flag = true;
						}
						if (((player.input[0].mp && player.input[1].pckp) || (player.input[0].pckp && player.input[1].mp)) && cooldown <= 0f)
						{
							flag2 = true;
							cooldown = 10f;
						}
					}
				}
				if (flag2)
				{
					for (int j = 0; j < game.Players.Count; j++)
					{
						if (game.Players[j].realizedCreature != null)
						{
							(game.Players[j].realizedCreature as Player).mushroomCounter = 100;
						}
					}
				}
			}
			if (!flag)
			{
				cooldown -= 0.025f;
				cooldown = Mathf.Clamp(cooldown, 0f, 10f);
			}
		}
	}

	public static float endGameCounter;

	public static int tempKarma = 1;

	public static bool tempReinforce;

	public static WorldCoordinate? tempKarmaPos;

	public static bool expeditionComplete = false;

	public static bool startingItemsSpawned = false;

	public static bool voidSeaFinish = false;

	public static List<string> pendingCompletedQuests = new List<string>();

	public static Dictionary<string, string> challengeNames = new Dictionary<string, string>();

	public static List<UnlockTracker> unlockTrackers = new List<UnlockTracker>();

	public static List<BurdenTracker> burdenTrackers = new List<BurdenTracker>();

	public static List<string> unlockedBurdens = new List<string>();

	public static Dictionary<SlugcatStats.Name, List<string>> allUnlocks = new Dictionary<SlugcatStats.Name, List<string>>();

	public static List<KeyValuePair<IconSymbol.IconSymbolData, int>> runKills;

	public static SlugcatSelectMenu.SaveGameData runData;

	public static List<SlugcatStats.Name> playableCharacters = new List<SlugcatStats.Name>();

	public static List<SlugcatStats.Name> unlockedExpeditionSlugcats = new List<SlugcatStats.Name>();

	public static Eggspedition egg;

	public static string lastRandomRegion;

	public static Dictionary<string, Vector2> ePos = new Dictionary<string, Vector2>
	{
		{
			"U1VfQjA3",
			new Vector2(90f, 629f)
		},
		{
			"U0lfQTA2",
			new Vector2(250f, 497f)
		},
		{
			"TEZfRjAy",
			new Vector2(1840f, 962f)
		},
		{
			"TENfZWxldmF0b3Jsb3dlcg==",
			new Vector2(896f, 385f)
		},
		{
			"VlNfQzA0",
			new Vector2(2714f, 579f)
		},
		{
			"RE1fUk9PRjAx",
			new Vector2(5791f, 599f)
		},
		{
			"TVNfYml0dGVyYWVyaWU2",
			new Vector2(2785f, 183f)
		},
		{
			"VUdfR1VUVEVSMDM=",
			new Vector2(1076f, 1825f)
		}
	};

	public static List<string> activeUnlocks
	{
		get
		{
			if (!allUnlocks.ContainsKey(ExpeditionData.slugcatPlayer))
			{
				allUnlocks[ExpeditionData.slugcatPlayer] = new List<string>();
			}
			return allUnlocks[ExpeditionData.slugcatPlayer];
		}
	}

	public static bool explosivejump
	{
		get
		{
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
			{
				return activeUnlocks.Contains("unl-explosivejump");
			}
			return false;
		}
	}

	public static void FinishExpedition()
	{
		int num = CalculateScore(predict: false);
		ExpeditionData.currentPoints += num;
		ExpeditionProgression.CheckLevelUp();
		ExpeditionProgression.EvaluateExpedition(new ExpeditionProgression.WinPackage
		{
			points = num,
			challenges = ExpeditionData.challengeList,
			slugcat = ExpeditionData.slugcatPlayer
		});
		ExpeditionData.completedChallengeList = new List<Challenge>();
		for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
		{
			ExpeditionData.completedChallengeList.Add(ExpeditionData.challengeList[i]);
		}
		if (ExpeditionData.activeMission != "" && !ExpeditionData.completedMissions.Contains(ExpeditionData.activeMission))
		{
			ExpeditionData.completedMissions.Add(ExpeditionData.activeMission);
		}
		ExpeditionData.ClearActiveChallengeList();
		Expedition.coreFile.Save(runEnded: true);
		ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, ended: true);
	}

	public static void PrepareExpedition()
	{
		expeditionComplete = false;
		startingItemsSpawned = false;
		voidSeaFinish = false;
		tempKarma = 1;
		tempReinforce = true;
		tempKarmaPos = null;
		ExpeditionData.earnedPassages = 0;
		ExpeditionData.newGame = true;
	}

	public static int CalculateScore(bool predict)
	{
		float num = 0f;
		int num2 = 0;
		float num3 = 100f;
		if (ExpeditionData.challengeList != null)
		{
			for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
			{
				if (ExpeditionData.challengeList[i].completed || predict)
				{
					num += (float)ExpeditionData.challengeList[i].Points();
					num2++;
					num3 += ((num2 == 1) ? 0f : 10f);
				}
				else
				{
					num += (float)(-(ExpeditionData.challengeList[i].Points() / 2));
				}
			}
			for (int j = 0; j < activeUnlocks.Count; j++)
			{
				if (activeUnlocks[j].StartsWith("bur-"))
				{
					num3 += ExpeditionProgression.BurdenScoreMultiplier(activeUnlocks[j]);
				}
			}
			num *= num3 / 100f;
			if (num < 0f)
			{
				num = 0f;
			}
			return Mathf.RoundToInt(num);
		}
		return 0;
	}

	public static string ExpeditionRandomStarts(RainWorld rainWorld, SlugcatStats.Name slug)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
		List<string> list2 = SlugcatStats.SlugcatStoryRegions(slug);
		if (File.Exists(AssetManager.ResolveFilePath("randomstarts.txt")))
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("randomstarts.txt"));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].StartsWith("//") || array[i].Length <= 0)
				{
					continue;
				}
				string text = Regex.Split(array[i], "_")[0];
				if (lastRandomRegion == text)
				{
					continue;
				}
				if (!dictionary2.ContainsKey(text))
				{
					dictionary2.Add(text, new List<string>());
				}
				if (list2.Contains(text))
				{
					dictionary2[text].Add(array[i]);
				}
				else if (ModManager.MSC && (slug == SlugcatStats.Name.White || slug == SlugcatStats.Name.Yellow))
				{
					if (text == "OE" && unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
					{
						dictionary2[text].Add(array[i]);
					}
					if (text == "LC" && unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
					{
						dictionary2[text].Add(array[i]);
					}
					if (text == "MS" && unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && array[i] != "MS_S07")
					{
						dictionary2[text].Add(array[i]);
					}
				}
				if (dictionary2[text].Contains(array[i]) && !dictionary.ContainsKey(text))
				{
					dictionary.Add(text, GetRegionWeight(text));
				}
			}
			System.Random random = new System.Random();
			int maxValue = dictionary.Values.Sum();
			int randomIndex = random.Next(0, maxValue);
			string key = (lastRandomRegion = dictionary.First(delegate(KeyValuePair<string, int> x)
			{
				randomIndex -= x.Value;
				return randomIndex < 0;
			}).Key);
			int num = dictionary2.Values.Select((List<string> list) => list.Count).Sum();
			string text2 = dictionary2[key].ElementAt(UnityEngine.Random.Range(0, dictionary2[key].Count - 1));
			ExpLog.Log($"{text2} | {dictionary.Keys.Count} valid regions for {slug.value} with {num} possible dens");
			return text2;
		}
		return "SU_S01";
	}

	public static int GetRegionWeight(string region)
	{
		return region switch
		{
			"GW" => 2, 
			"SS" => 1, 
			"SH" => 2, 
			_ => 5, 
		};
	}

	public static void SetUpUnlockTrackers(RainWorldGame game)
	{
		unlockTrackers = new List<UnlockTracker>();
		if (game != null && activeUnlocks.Contains("unl-slow"))
		{
			unlockTrackers.Add(new SlowTimeTracker(game));
		}
	}

	public static void SetUpBurdenTrackers(RainWorldGame game)
	{
		burdenTrackers = new List<BurdenTracker>();
		if (game == null)
		{
			return;
		}
		if (activeUnlocks.Contains("bur-pursued"))
		{
			burdenTrackers.Add(new PursuedTracker(game));
		}
		foreach (string activeUnlock in activeUnlocks)
		{
			BurdenTracker burdenTracker = CustomBurdens.BurdenForID(activeUnlock)?.CreateTracker(game);
			if (burdenTracker != null)
			{
				burdenTrackers.Add(burdenTracker);
			}
		}
	}

	public static bool IsUndesirableRoomScript(UpdatableAndDeletable item)
	{
		if (item is RoomSpecificScript.SU_C04StartUp)
		{
			return true;
		}
		if (item is RoomSpecificScript.SL_C12JetFish)
		{
			return true;
		}
		if (item is RoomSpecificScript.SU_A23FirstCycleMessage)
		{
			return true;
		}
		if (item is RoomSpecificScript.SU_A43SuperJumpOnly)
		{
			return true;
		}
		if (item is RoomSpecificScript.LF_A03)
		{
			return true;
		}
		return false;
	}

	public static bool IsMSCRoomScript(UpdatableAndDeletable item)
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (item is CutsceneArtificer)
		{
			return true;
		}
		if (item is CutsceneArtificerRobo)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SpearmasterGateLocation)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SU_SMIntroMessage)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SI_SAINTENDING)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SI_SAINTINTRO_tut)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SI_C02_tut)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.DS_RIVSTARTcutscene)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.SH_GOR02)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.MS_CORESTARTUPHEART)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.MS_COMMS_RivEnding)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.GW_E02_RivEnding)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.LC_FINAL)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_GourmandEnding)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_WORMPIT)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_FINAL01)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_CAVE10)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_TREETOP)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.OE_BACKFILTER)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.GW_EDGE03_SCAVTUT)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.GW_TOWER01_SCAVTUT)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.GW_PIPE02_SCAVTUT)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.MS_CORESTARTUPHEART)
		{
			return true;
		}
		if (item is MSCRoomSpecificScript.RM_CORE_EnergyCell)
		{
			return true;
		}
		return false;
	}

	public static int ExIndex(SlugcatStats.Name slug)
	{
		if (slug == SlugcatStats.Name.White)
		{
			return 0;
		}
		if (slug == SlugcatStats.Name.Yellow)
		{
			return 1;
		}
		if (slug == SlugcatStats.Name.Red)
		{
			return 2;
		}
		if (ModManager.MSC)
		{
			if (slug == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return 3;
			}
			if (slug == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return 4;
			}
			if (slug == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return 5;
			}
			if (slug == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return 6;
			}
			if (slug == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return 7;
			}
		}
		return -1;
	}

	public static Color ExIndexToColor(int i)
	{
		switch (i)
		{
		case 0:
			return PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.White);
		case 1:
			return PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Yellow);
		case 2:
			return PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red);
		default:
			if (ModManager.MSC)
			{
				switch (i)
				{
				case 3:
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
				case 4:
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
				case 5:
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Spear);
				case 6:
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
				case 7:
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Saint);
				}
			}
			return new Color(0f, 0f, 0f);
		}
	}

	public static void ExSpawn(Room room)
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is EggSpot)
			{
				return;
			}
		}
		if (ExIndex(ExpeditionData.slugcatPlayer) >= 0 && ExpeditionData.ints[ExIndex(ExpeditionData.slugcatPlayer)] == 0 && Encoding.UTF8.GetString(Convert.FromBase64String(ePos.Keys.ElementAt(ExIndex(ExpeditionData.slugcatPlayer)))) == room.abstractRoom.name)
		{
			room.AddObject(new EggSpot(ePos.Values.ElementAt(ExIndex(ExpeditionData.slugcatPlayer)), ExpeditionData.slugcatPlayer));
		}
	}
}
