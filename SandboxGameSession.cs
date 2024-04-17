using System.Collections.Generic;
using System.Globalization;
using ArenaBehaviors;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SandboxGameSession : ArenaGameSession
{
	public SandboxEditor editor;

	public bool PlayMode;

	public SandboxOverlay overlay;

	public bool overlaySpawned;

	public List<int> playerSpawnDens;

	private bool sandboxInitiated;

	private bool winLoseGameOver;

	private bool checkWinLose;

	public List<AbstractCreature> ChallengeKillList;

	public override bool SpawnDefaultRoomItems
	{
		get
		{
			if (PlayMode)
			{
				return base.GameTypeSetup.levelItems;
			}
			return false;
		}
	}

	public SandboxGameSession(RainWorldGame game)
		: base(game)
	{
		PlayMode = arenaSitting.sandboxPlayMode;
		if (!PlayMode)
		{
			arenaSitting.currentLevel = 0;
			if (noRain == null)
			{
				AddBehavior(new NoRain(this));
			}
		}
		else
		{
			arenaSitting.gameTypeSetup.UpdateCustomWinCondition();
			checkWinLose = arenaSitting.players.Count > 0 && arenaSitting.gameTypeSetup.customWinCondition;
		}
	}

	public override void ProcessShutDown()
	{
		base.ProcessShutDown();
		overlay.ShutDownProcess();
	}

	public override void SpawnCreatures()
	{
		base.SpawnCreatures();
		if (PlayMode)
		{
			int currConfNumber = -1;
			List<SandboxEditor.PlacedIconData> list = null;
			if (ModManager.MSC && arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				list = ChallengeInformation.LoadChallengeConfiguration(arenaSitting.gameTypeSetup.challengeID);
				ChallengeKillList = new List<AbstractCreature>();
			}
			else
			{
				list = SandboxEditor.LoadConfiguration(ref currConfNumber, arenaSitting.GetCurrentLevel, game.manager.rainWorld);
			}
			for (int i = 0; i < list.Count; i++)
			{
				SpawnEntity(list[i]);
			}
			if (ModManager.MSC)
			{
				ChallengeKillList = new List<AbstractCreature>(game.world.GetAbstractRoom(0).creatures);
			}
			return;
		}
		bool flag = false;
		if (arenaSitting.players.Count < 1)
		{
			flag = true;
			arenaSitting.players.Add(new ArenaSitting.ArenaPlayer(0));
		}
		for (int j = 0; j < arenaSitting.players.Count; j++)
		{
			AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(0, 0, -50, 0), game.GetNewID());
			game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
			abstractCreature.abstractAI.SetDestinationNoPathing(new WorldCoordinate(0, -1, -1, Random.Range(0, game.world.GetAbstractRoom(0).nodes.Length)), migrate: true);
			(abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator = 10 + arenaSitting.players[j].playerNumber;
		}
		if (flag)
		{
			arenaSitting.players.Clear();
		}
	}

	public override void Initiate()
	{
		base.Initiate();
		overlay.Initiate(PlayMode);
		if (PlayMode)
		{
			for (int i = 0; i < 30; i++)
			{
				game.Update();
				counter = 0;
			}
			if (ModManager.MSC && chMeta != null)
			{
				List<int> list = new List<int>();
				list.Add(chMeta.spawnDen);
				if (chMeta.spawnDen2 >= 0)
				{
					list.Add(chMeta.spawnDen2);
				}
				if (chMeta.spawnDen3 >= 0)
				{
					list.Add(chMeta.spawnDen3);
				}
				if (chMeta.spawnDen4 >= 0)
				{
					list.Add(chMeta.spawnDen4);
				}
				SpawnPlayers(base.room, list);
			}
			else
			{
				SpawnPlayers(base.room, playerSpawnDens);
			}
			AddHUD();
		}
		else
		{
			AddHUD();
			editor = new SandboxEditor(this);
			overlay.sandboxEditorSelector.ConnectToEditor(editor);
		}
		sandboxInitiated = true;
		overlay.fadingOut = true;
	}

	public override void Update()
	{
		if (!overlaySpawned && base.room != null)
		{
			overlaySpawned = true;
			base.room.AddObject(new SandboxOverlayOwner(base.room, this, !PlayMode));
		}
		base.Update();
		if (checkWinLose && playersSpawned)
		{
			if (Players.Count == 1)
			{
				if (arenaSitting.players[0].sandboxWin != 0)
				{
					CustomGameOver();
				}
			}
			else
			{
				if (PlayersStillActive(addToAliveTime: false, dontCountSandboxLosers: true) < 2)
				{
					CustomGameOver();
				}
				for (int i = 0; i < arenaSitting.players.Count; i++)
				{
					if (arenaSitting.players[i].sandboxWin > 0)
					{
						CustomGameOver();
						break;
					}
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Q) && !PlayMode)
		{
			if (game.cameras[0].currentCameraPosition < base.room.cameraPositions.Length - 1)
			{
				game.cameras[0].MoveCamera(game.cameras[0].currentCameraPosition + 1);
			}
			else
			{
				game.cameras[0].MoveCamera(0);
			}
		}
	}

	public void CustomGameOver()
	{
		winLoseGameOver = true;
		outsidePlayersCountAsDead = false;
		checkWinLose = false;
	}

	public override bool ShouldSessionEnd()
	{
		if (arenaSitting.players.Count == 0)
		{
			return game.world.rainCycle.TimeUntilRain < -200;
		}
		if (winLoseGameOver)
		{
			return true;
		}
		if (PlayMode && initiated && sandboxInitiated)
		{
			return thisFrameActivePlayers == 0;
		}
		return false;
	}

	private void SpawnEntity(SandboxEditor.PlacedIconData placedIconData)
	{
		IconSymbol.IconSymbolData data = placedIconData.data;
		if (data.critType == null || data.critType.Index >= StaticWorld.creatureTemplates.Length)
		{
			return;
		}
		WorldCoordinate pos = new WorldCoordinate(0, -1, -1, -1);
		pos.x = Mathf.RoundToInt(placedIconData.pos.x / 20f);
		pos.y = Mathf.RoundToInt(placedIconData.pos.y / 20f);
		EntityID entityID = (base.GameTypeSetup.saveCreatures ? placedIconData.ID : game.GetNewID());
		if (data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
		{
			AbstractCreature abstractCreature = null;
			if (base.GameTypeSetup.saveCreatures)
			{
				for (int i = 0; i < arenaSitting.creatures.Count; i++)
				{
					if (arenaSitting.creatures[i].creatureTemplate.type == data.critType && arenaSitting.creatures[i].ID == entityID)
					{
						abstractCreature = arenaSitting.creatures[i];
						arenaSitting.creatures.RemoveAt(i);
						for (int j = 0; j < 2; j++)
						{
							abstractCreature.state.CycleTick();
						}
						string creatureString = SaveState.AbstractCreatureToStringSingleRoomWorld(abstractCreature);
						abstractCreature = SaveState.AbstractCreatureFromString(game.world, creatureString, onlyInCurrentRegion: false);
						if (abstractCreature != null)
						{
							abstractCreature.pos = pos;
							break;
						}
					}
				}
			}
			if (abstractCreature == null)
			{
				abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID);
			}
			if (data.critType == CreatureTemplate.Type.Slugcat)
			{
				if (playerSpawnDens == null)
				{
					playerSpawnDens = new List<int>();
				}
				playerSpawnDens.Add(data.intData);
			}
			else if (data.critType == CreatureTemplate.Type.TentaclePlant || data.critType == CreatureTemplate.Type.PoleMimic || (ModManager.MSC && data.critType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug))
			{
				abstractCreature.pos.x = -1;
				abstractCreature.pos.y = -1;
				abstractCreature.pos.abstractNode = data.intData;
				game.world.GetAbstractRoom(0).entitiesInDens.Add(abstractCreature);
			}
			else if (data.critType == CreatureTemplate.Type.Centipede)
			{
				float num = 0f;
				if (data.intData == 2)
				{
					num = Mathf.Lerp(0.265f, 0.55f, Mathf.Pow(Custom.ClampedRandomVariation(0.5f, 0.5f, 0.7f), 1.2f));
				}
				else if (data.intData == 3)
				{
					num = Mathf.Lerp(0.7f, 1f, Mathf.Pow(Random.value, 0.6f));
				}
				abstractCreature.spawnData = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", num);
				game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
			}
			else if (data.critType == CreatureTemplate.Type.Fly || data.critType == CreatureTemplate.Type.Spider || data.critType == CreatureTemplate.Type.Leech || data.critType == CreatureTemplate.Type.SeaLeech || (ModManager.MSC && data.critType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech))
			{
				for (int k = 0; k < 5; k++)
				{
					game.world.GetAbstractRoom(0).AddEntity(new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID));
				}
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
			}
		}
		else
		{
			SpawnItems(data, pos, entityID);
		}
	}

	public void SpawnItems(IconSymbol.IconSymbolData data, WorldCoordinate pos, EntityID entityID)
	{
		AbstractPhysicalObject.AbstractObjectType itemType = data.itemType;
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if (data.intData == 3)
			{
				game.world.GetAbstractRoom(0).AddEntity(new AbstractSpear(game.world, null, pos, entityID, explosive: false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f))));
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(new AbstractSpear(game.world, null, pos, entityID, data.intData == 1, ModManager.MSC && data.intData == 2));
			}
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
		{
			game.world.GetAbstractRoom(0).AddEntity(new WaterNut.AbstractWaterNut(game.world, null, pos, entityID, -1, -1, null, swollen: false));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			game.world.GetAbstractRoom(0).AddEntity(new SporePlant.AbstractSporePlant(game.world, null, pos, entityID, -1, -1, null, used: false, pacified: true));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
		{
			game.world.GetAbstractRoom(0).AddEntity(new BubbleGrass.AbstractBubbleGrass(game.world, null, pos, entityID, 1f, -1, -1, null));
		}
		else if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
		{
			game.world.GetAbstractRoom(0).AddEntity(new FireEgg.AbstractBugEgg(game.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f))));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.DataPearl)
		{
			DataPearl.AbstractDataPearl ent = new DataPearl.AbstractDataPearl(game.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)]));
			game.world.GetAbstractRoom(0).AddEntity(ent);
		}
		else if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
		{
			game.world.GetAbstractRoom(0).AddEntity(new JokeRifle.AbstractRifle(game.world, null, pos, entityID, JokeRifle.AbstractRifle.AmmoType.Rock));
		}
		else if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
		{
			game.world.GetAbstractRoom(0).AddEntity(new LillyPuck.AbstractLillyPuck(game.world, null, pos, entityID, 3, -1, -1, null));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			if (ModManager.MSC && Random.value < 0.25f && (global::MoreSlugcats.MoreSlugcats.chtUnlockCreatures.Value || game.rainWorld.progression.miscProgressionData.GetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite)))
			{
				string spriteOverride = "KrakenMask";
				switch (Random.Range(0, 4))
				{
				case 1:
					spriteOverride = "SpikeMask";
					break;
				case 2:
					spriteOverride = "HornedMask";
					break;
				case 3:
					spriteOverride = "SadMask";
					break;
				}
				game.world.GetAbstractRoom(0).AddEntity(new VultureMask.AbstractVultureMask(game.world, null, pos, entityID, entityID.RandomSeed, king: false, scavKing: false, spriteOverride));
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(new VultureMask.AbstractVultureMask(game.world, null, pos, entityID, entityID.RandomSeed, Random.value < 0.25f && (MultiplayerUnlocks.CheckUnlockAll() || game.rainWorld.progression.miscProgressionData.GetTokenCollected(MultiplayerUnlocks.SandboxUnlockID.KingVulture))));
			}
		}
		else if (AbstractConsumable.IsTypeConsumable(data.itemType))
		{
			game.world.GetAbstractRoom(0).AddEntity(new AbstractConsumable(game.world, data.itemType, null, pos, entityID, -1, -1, null));
		}
		else
		{
			game.world.GetAbstractRoom(0).AddEntity(new AbstractPhysicalObject(game.world, data.itemType, null, pos, entityID));
		}
	}
}
