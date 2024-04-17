using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace ArenaBehaviors;

public class ChallengeBehavior : ArenaGameBehavior
{
	public FadeOut fadeIn;

	private FLabel loadingLabel;

	public int loadingTime;

	public int aliveCheckTimer;

	public int firstAliveCheck;

	public int respawnTimer;

	public ChallengeBehavior(ArenaGameSession gameSession)
		: base(gameSession)
	{
	}

	public override void Update()
	{
		if (fadeIn == null)
		{
			fadeIn = new FadeOut(base.room, Color.black, 40f, fadeIn: true);
			base.room.AddObject(fadeIn);
		}
		if (!base.room.ReadyForPlayer)
		{
			fadeIn.fade = 1f;
			fadeIn.freezeFade = true;
			loadingTime++;
			if (loadingTime > 105 && loadingLabel == null)
			{
				loadingLabel = new FLabel(Custom.GetFont(), base.room.game.rainWorld.inGameTranslator.Translate("Loading..."));
				loadingLabel.x = 100.2f;
				loadingLabel.y = 50.2f;
				Futile.stage.AddChild(loadingLabel);
			}
			return;
		}
		fadeIn.freezeFade = false;
		if (loadingLabel != null)
		{
			loadingLabel.RemoveFromContainer();
			loadingLabel = null;
		}
		ArenaSitting arenaSitting = base.room.game.GetArenaGameSession.arenaSitting;
		if (arenaSitting.gameTypeSetup.challengeMeta == null || !(arenaSitting.gameTypeSetup.challengeMeta.arenaSpawns != ArenaSetup.GameTypeSetup.WildLifeSetting.Off))
		{
			return;
		}
		if (!base.game.GamePaused)
		{
			firstAliveCheck++;
		}
		if (firstAliveCheck < 400)
		{
			return;
		}
		int num = 0;
		aliveCheckTimer++;
		if (aliveCheckTimer >= 40)
		{
			bool flag = false;
			for (int i = 0; i < base.room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < base.room.physicalObjects[i].Count; j++)
				{
					if (base.room.physicalObjects[i][j] is Creature && !(base.room.physicalObjects[i][j] is Player) && !(base.room.physicalObjects[i][j] is Leech) && !(base.room.physicalObjects[i][j] is Spider) && !(base.room.physicalObjects[i][j] is Snail) && !(base.room.physicalObjects[i][j] is TubeWorm) && !(base.room.physicalObjects[i][j] is Fly) && !(base.room.physicalObjects[i][j] as Creature).dead)
					{
						num++;
					}
					if (base.room.physicalObjects[i][j] is Creature && !(base.room.physicalObjects[i][j] as Creature).dead && ArenaCreatureSpawner.IsMajorCreature((base.room.physicalObjects[i][j] as Creature).abstractCreature.creatureTemplate.type))
					{
						flag = true;
					}
				}
			}
			if (num <= ((!flag) ? 1 : 2))
			{
				respawnTimer++;
			}
			else
			{
				respawnTimer = 0;
			}
			aliveCheckTimer = 0;
		}
		if (base.game.GamePaused)
		{
			respawnTimer = 0;
		}
		if (respawnTimer > 5)
		{
			ArenaCreatureSpawner.allowLockedCreatures = true;
			ArenaCreatureSpawner.SpawnArenaCreatures(base.game, arenaSitting.gameTypeSetup.challengeMeta.arenaSpawns, ref arenaSitting.creatures, ref arenaSitting.multiplayerUnlocks);
			respawnTimer = 0;
		}
	}
}
