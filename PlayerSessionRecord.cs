using System.Collections.Generic;
using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class PlayerSessionRecord
{
	public struct KillRecord
	{
		public IconSymbol.IconSymbolData symbolData;

		public bool lizard;

		public EntityID ID;

		public KillRecord(IconSymbol.IconSymbolData symbolData, EntityID ID, bool lizard)
		{
			this.symbolData = symbolData;
			this.lizard = lizard;
			this.ID = ID;
		}
	}

	public struct EatRecord
	{
		public CreatureTemplate.Type creatureType;

		public AbstractPhysicalObject.AbstractObjectType objType;

		public EntityID ID;

		public EatRecord(CreatureTemplate.Type creatureType, AbstractPhysicalObject.AbstractObjectType objType, EntityID ID)
		{
			this.creatureType = creatureType;
			this.objType = objType;
			this.ID = ID;
		}
	}

	public int playerNumber;

	public List<KillRecord> kills;

	public List<EatRecord> eats;

	public bool ateAnything;

	public bool vegetarian = true;

	public bool peaceful = true;

	public bool carnivorous = true;

	public string wokeUpInRegion;

	public string wentToSleepInRegion;

	public List<DataPearl.AbstractDataPearl.DataPearlType> pearlsFound;

	public AbstractCreature friendInDen;

	public int time;

	public int playerGrabbedTime;

	public int pupCountInDen;

	public PlayerSessionRecord(int playerNumber)
	{
		this.playerNumber = playerNumber;
		kills = new List<KillRecord>();
		eats = new List<EatRecord>();
		pearlsFound = new List<DataPearl.AbstractDataPearl.DataPearlType>();
	}

	public void BreakPeaceful(Creature victim)
	{
		if (victim == null)
		{
			Custom.LogWarning("BreakPeaceful for a null victim was called! Please, review it. (will skip this call since it's nul)");
		}
		else if (victim.Template.countsAsAKill > 0)
		{
			peaceful = false;
		}
	}

	public void AddKill(Creature victim)
	{
		BreakPeaceful(victim);
		if (victim == null || victim.abstractCreature == null)
		{
			return;
		}
		_ = victim.abstractCreature.ID;
		for (int i = 0; i < kills.Count; i++)
		{
			if (kills[i].ID == victim.abstractCreature.ID)
			{
				return;
			}
		}
		Custom.Log($"player kill added: {victim.Template.type} {victim.abstractCreature.ID} {victim.Template.IsLizard}");
		kills.Add(new KillRecord(CreatureSymbol.SymbolDataFromCreature(victim.abstractCreature), victim.abstractCreature.ID, victim.Template.IsLizard));
		if (!ModManager.Expedition || !Custom.rainWorld.ExpeditionMode)
		{
			return;
		}
		for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
		{
			if (ExpeditionData.challengeList[j].RespondToCreatureKill())
			{
				ExpeditionData.challengeList[j].CreatureKilled(victim, playerNumber);
			}
		}
	}

	public void AddEat(PhysicalObject eatenObject)
	{
		for (int num = eats.Count - 1; num >= 0; num--)
		{
			if (eats[num].ID == eatenObject.abstractPhysicalObject.ID)
			{
				return;
			}
		}
		if (ModManager.MSC && eatenObject.room != null && eatenObject.room.game.IsStorySession && eatenObject.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && eatenObject.room.game.Players[playerNumber] != null && eatenObject.room.game.Players[playerNumber].realizedCreature != null && (eatenObject.room.game.Players[playerNumber].realizedCreature as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			Custom.Log("Checking gourmand tracker!");
			WinState.GourFeastTracker gourFeastTracker = eatenObject.room.game.GetStorySession.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, addIfMissing: true) as WinState.GourFeastTracker;
			for (int i = 0; i < gourFeastTracker.currentCycleProgress.Length; i++)
			{
				if (eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Creature)
				{
					int num2 = WinState.GourmandPassageCreaturesAtIndexContains((eatenObject as Creature).Template.type, i);
					if (num2 > 0)
					{
						Custom.Log($"Creature flagged for gourmand collection {(eatenObject as Creature).Template.type}");
						if (gourFeastTracker.currentCycleProgress[i] <= 0)
						{
							(eatenObject.room.game.Players[playerNumber].realizedCreature as Player).showKarmaFoodRainTime = 300;
							eatenObject.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Karma_Pitch_Discovery, 0f, 1f, 0.9f + Random.value * 0.3f);
						}
						gourFeastTracker.currentCycleProgress[i] = num2;
					}
				}
				else if (WinState.GourmandPassageRequirementAtIndex(i) == eatenObject.abstractPhysicalObject.type || (WinState.GourmandPassageRequirementAtIndex(i) == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer && eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer))
				{
					Custom.Log($"Item flagged for gourmand collection {eatenObject.abstractPhysicalObject.type}");
					if (gourFeastTracker.currentCycleProgress[i] <= 0)
					{
						(eatenObject.room.game.Players[playerNumber].realizedCreature as Player).showKarmaFoodRainTime = 300;
						eatenObject.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Karma_Pitch_Discovery, 0f, 1f, 0.9f + Random.value * 0.3f);
					}
					gourFeastTracker.currentCycleProgress[i] = 1;
				}
			}
		}
		if (eatenObject is Creature)
		{
			eats.Add(new EatRecord((eatenObject as Creature).Template.type, eatenObject.abstractPhysicalObject.type, eatenObject.abstractPhysicalObject.ID));
		}
		else
		{
			eats.Add(new EatRecord(null, eatenObject.abstractPhysicalObject.type, eatenObject.abstractPhysicalObject.ID));
		}
		if (!(eatenObject is KarmaFlower) && !(eatenObject is Mushroom))
		{
			ateAnything = true;
			if (eatenObject is Creature || eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.JellyFish || eatenObject.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
			{
				vegetarian = false;
			}
			else
			{
				carnivorous = false;
			}
		}
	}
}
