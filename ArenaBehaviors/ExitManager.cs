using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace ArenaBehaviors;

public class ExitManager : ArenaGameBehavior
{
	public float[] exitSymbolDarken;

	private bool showExitsOpen;

	public List<ShortcutHandler.ShortCutVessel> playersInDens;

	public bool anyPlayerHasEnoughScore;

	public int allPlayersExitedCounter = -1;

	private ChallengeInformation.ChallengeMeta challengeMeta;

	public bool challengeCompleted;

	public bool challengeCompletedA;

	public bool challengeCompletedB;

	public int challengeCompletedTime;

	public ExitManager(ArenaGameSession gameSession)
		: base(gameSession)
	{
		playersInDens = new List<ShortcutHandler.ShortCutVessel>();
		if (ModManager.MSC && gameSession.GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			challengeMeta = gameSession.GameTypeSetup.challengeMeta;
		}
		else
		{
			challengeMeta = null;
		}
	}

	public override void Initiate()
	{
		base.Initiate();
		exitSymbolDarken = new float[base.game.world.GetAbstractRoom(0).exits];
	}

	public override void Update()
	{
		if (!base.room.shortCutsReady)
		{
			return;
		}
		if (gameSession.GameTypeSetup.denEntryRule == ArenaSetup.GameTypeSetup.DenEntryRule.Score && gameSession.counter % 20 == 0)
		{
			anyPlayerHasEnoughScore = false;
			for (int i = 0; i < gameSession.Players.Count; i++)
			{
				if (gameSession.Players[i].realizedCreature != null && gameSession.ScoreOfPlayer(gameSession.Players[i].realizedCreature as Player, inHands: true) >= gameSession.GameTypeSetup.ScoreToEnterDen)
				{
					anyPlayerHasEnoughScore = true;
					break;
				}
			}
		}
		if (challengeMeta != null)
		{
			List<Player> list = new List<Player>();
			for (int j = 0; j < gameSession.Players.Count; j++)
			{
				if (gameSession.Players[j].realizedCreature != null)
				{
					list.Add(gameSession.Players[j].realizedCreature as Player);
				}
			}
			if (list.Count > 0)
			{
				float num = 0f;
				for (int k = 0; k < list.Count; k++)
				{
					num += (float)gameSession.ScoreOfPlayer(list[k], inHands: true);
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.KILL || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.KILL) && !gameSession.arenaSitting.gameTypeSetup.challengeMeta.aiIcon)
				{
					bool flag = true;
					if (list[0].room != null)
					{
						List<AbstractCreature> challengeKillList = (gameSession as SandboxGameSession).ChallengeKillList;
						flag = false;
						for (int l = 0; l < challengeKillList.Count; l++)
						{
							if (challengeMeta.killCreature == null || challengeMeta.killCreature == "")
							{
								if (challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Slugcat && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.VultureGrub && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Fly && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Leech && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.SeaLeech && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Hazer && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Overseer && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.SmallCentipede && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.TubeWorm && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.Spider && challengeKillList[l].creatureTemplate.type != CreatureTemplate.Type.SmallNeedleWorm && !challengeKillList[l].state.dead)
								{
									flag = true;
									break;
								}
							}
							else if (challengeKillList[l].creatureTemplate.type.ToString().ToLower() == challengeMeta.killCreature.ToLower() && !challengeKillList[l].state.dead)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						challengeCompletedTime++;
						if (challengeCompletedTime >= 40 && base.game.world.rainCycle.AmountLeft > 0f)
						{
							if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.KILL)
							{
								challengeCompletedA = true;
							}
							else
							{
								challengeCompletedB = true;
							}
						}
					}
					else
					{
						challengeCompletedTime = 0;
					}
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.SURVIVE || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.SURVIVE) && gameSession.arenaSitting.players[0].timeAlive / 40 >= challengeMeta.surviveTime)
				{
					if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.SURVIVE)
					{
						challengeCompletedA = true;
					}
					else
					{
						challengeCompletedB = true;
					}
				}
				if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.POINTS || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.POINTS)
				{
					if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.POINTS)
					{
						challengeCompletedA = num >= (float)challengeMeta.points;
					}
					else
					{
						challengeCompletedB = num >= (float)challengeMeta.points;
					}
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.ARMOR || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.ARMOR) && list[0].room != null)
				{
					for (int m = 0; m < list[0].room.abstractRoom.creatures.Count; m++)
					{
						if (!(list[0].room.abstractRoom.creatures[m].creatureTemplate.type == CreatureTemplate.Type.RedCentipede) || list[0].room.abstractRoom.creatures[m].realizedCreature == null)
						{
							continue;
						}
						Centipede centipede = list[0].room.abstractRoom.creatures[m].realizedCreature as Centipede;
						bool flag2 = false;
						for (int n = 0; n < centipede.CentiState.shells.Length; n++)
						{
							if (centipede.CentiState.shells[n])
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.ARMOR)
							{
								challengeCompletedA = true;
							}
							else
							{
								challengeCompletedB = true;
							}
							break;
						}
					}
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PARRY || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PARRY) && gameSession.arenaSitting.players[0].parries >= challengeMeta.parries)
				{
					if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PARRY)
					{
						challengeCompletedA = true;
					}
					else
					{
						challengeCompletedB = true;
					}
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.TAME || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.TAME) && list[0].room != null)
				{
					for (int num2 = 0; num2 < list[0].room.abstractRoom.creatures.Count; num2++)
					{
						for (int num3 = 0; num3 < list.Count; num3++)
						{
							if (list[0].room.abstractRoom.creatures[num2].state.socialMemory == null)
							{
								continue;
							}
							SocialMemory.Relationship orInitiateRelationship = list[0].room.abstractRoom.creatures[num2].state.socialMemory.GetOrInitiateRelationship(list[num3].abstractCreature.ID);
							if ((orInitiateRelationship.like >= 0.5f || orInitiateRelationship.tempLike >= 0.5f) && (challengeMeta.tameCreature == null || challengeMeta.tameCreature == "" || list[0].room.abstractRoom.creatures[num2].creatureTemplate.type.ToString().ToLower() == challengeMeta.tameCreature.ToLower()))
							{
								if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.TAME)
								{
									challengeCompletedA = true;
								}
								else
								{
									challengeCompletedB = true;
								}
							}
						}
					}
				}
				if ((challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.POPCORN || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.POPCORN) && list[0].room != null)
				{
					bool flag3 = true;
					for (int num4 = 0; num4 < list[0].room.physicalObjects.Length; num4++)
					{
						for (int num5 = 0; num5 < list[0].room.physicalObjects[num4].Count; num5++)
						{
							if (list[0].room.physicalObjects[num4][num5] is SeedCob && !(list[0].room.physicalObjects[num4][num5] as SeedCob).AbstractCob.opened)
							{
								flag3 = false;
								break;
							}
						}
						if (!flag3)
						{
							break;
						}
					}
					if (flag3)
					{
						if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.POPCORN)
						{
							challengeCompletedA = true;
						}
						else
						{
							challengeCompletedB = true;
						}
					}
				}
				if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.BRING || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.BRING)
				{
					if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.BRING)
					{
						challengeCompletedA = false;
					}
					else
					{
						challengeCompletedB = false;
					}
					for (int num6 = 0; num6 < list.Count; num6++)
					{
						for (int num7 = 0; num7 < list[num6].grasps.Length; num7++)
						{
							string text = "";
							if (list[num6].grasps[num7] != null && list[num6].grasps[num7].grabbed.abstractPhysicalObject is AbstractCreature)
							{
								if ((list[num6].grasps[num7].grabbed.abstractPhysicalObject as AbstractCreature).realizedCreature != null && !(list[num6].grasps[num7].grabbed.abstractPhysicalObject as AbstractCreature).realizedCreature.dead)
								{
									text = (list[num6].grasps[num7].grabbed.abstractPhysicalObject as AbstractCreature).creatureTemplate.type.ToString();
								}
							}
							else if (list[num6].grasps[num7] != null)
							{
								text = list[num6].grasps[num7].grabbed.abstractPhysicalObject.type.ToString();
							}
							if (text.ToLower() == challengeMeta.bringItem.ToLower())
							{
								if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.BRING)
								{
									challengeCompletedA = true;
								}
								else
								{
									challengeCompletedB = true;
								}
							}
						}
						string text2 = "";
						if (list[num6].objectInStomach != null)
						{
							text2 = list[num6].objectInStomach.type.ToString();
						}
						if (text2.ToLower() == challengeMeta.bringItem.ToLower())
						{
							if (challengeMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.BRING)
							{
								challengeCompletedA = true;
							}
							else
							{
								challengeCompletedB = true;
							}
						}
					}
				}
				if (challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.NONE || challengeMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT)
				{
					challengeCompleted = challengeCompletedA;
				}
				else
				{
					challengeCompleted = challengeCompletedA && challengeCompletedB;
				}
			}
		}
		if (allPlayersExitedCounter < 0)
		{
			if (gameSession.Players.Count > 0 && gameSession.counter > 40)
			{
				bool flag4 = true;
				if (flag4)
				{
					for (int num8 = 0; num8 < gameSession.arenaSitting.players.Count; num8++)
					{
						if (!gameSession.arenaSitting.players[num8].hasEnteredGameArea)
						{
							flag4 = false;
							break;
						}
					}
				}
				if (flag4)
				{
					for (int num9 = 0; num9 < gameSession.Players.Count; num9++)
					{
						if (gameSession.Players[num9].realizedCreature != null && gameSession.Players[num9].pos.NodeDefined && gameSession.Players[num9].state.alive && Custom.DistLess(gameSession.room.ShortcutLeadingToNode(gameSession.Players[num9].pos.abstractNode).startCoord, gameSession.Players[num9].pos, 3f))
						{
							flag4 = false;
							break;
						}
					}
				}
				if (flag4)
				{
					allPlayersExitedCounter = 0;
				}
			}
		}
		else if (allPlayersExitedCounter < 40)
		{
			allPlayersExitedCounter++;
		}
		bool flag5 = ExitsOpen();
		if (exitSymbolDarken != null)
		{
			for (int num10 = 0; num10 < exitSymbolDarken.Length; num10++)
			{
				if (ExitOccupied(num10))
				{
					exitSymbolDarken[num10] = Custom.LerpAndTick(exitSymbolDarken[num10], 1f, 0.04f, 1f / 30f);
				}
				else if (flag5)
				{
					exitSymbolDarken[num10] = Custom.LerpAndTick(exitSymbolDarken[num10], 0f, 0.04f, 1f / 30f);
				}
				else
				{
					exitSymbolDarken[num10] = Custom.LerpAndTick(exitSymbolDarken[num10], Mathf.InverseLerp(80f, 240f, base.game.world.rainCycle.timer) * 0.7f, 0.04f, 1f / 30f);
				}
			}
		}
		if (flag5 != showExitsOpen)
		{
			for (int num11 = 0; num11 < base.game.cameras.Length; num11++)
			{
				base.game.cameras[num11].shortcutGraphics.ChangeAllExitsToSheltersOrDots(flag5);
			}
			showExitsOpen = flag5;
		}
	}

	public float DarkenExitSymbol(int exit)
	{
		if (exitSymbolDarken == null)
		{
			return 0f;
		}
		return exitSymbolDarken[exit];
	}

	public bool IsPlayerInDen(AbstractCreature crit)
	{
		for (int i = 0; i < playersInDens.Count; i++)
		{
			if (playersInDens[i].creature.abstractCreature == crit)
			{
				return true;
			}
		}
		return false;
	}

	public bool PlayerTryingToEnterDen(ShortcutHandler.ShortCutVessel shortcutVessel)
	{
		if (!(shortcutVessel.creature is Player))
		{
			return false;
		}
		if (ModManager.MSC && shortcutVessel.creature.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
		{
			return false;
		}
		if (gameSession.GameTypeSetup.denEntryRule == ArenaSetup.GameTypeSetup.DenEntryRule.Score && gameSession.ScoreOfPlayer(shortcutVessel.creature as Player, inHands: true) < gameSession.GameTypeSetup.ScoreToEnterDen)
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < shortcutVessel.room.realizedRoom.exitAndDenIndex.Length; i++)
		{
			if (shortcutVessel.pos == shortcutVessel.room.realizedRoom.exitAndDenIndex[i])
			{
				num = i;
				break;
			}
		}
		if (ExitsOpen() && !ExitOccupied(num))
		{
			shortcutVessel.entranceNode = num;
			playersInDens.Add(shortcutVessel);
			return true;
		}
		return false;
	}

	private bool ExitOccupied(int exit)
	{
		for (int i = 0; i < playersInDens.Count; i++)
		{
			if (playersInDens[i].entranceNode == exit)
			{
				return true;
			}
		}
		return false;
	}

	public bool ExitsOpen()
	{
		if (challengeMeta != null)
		{
			if (challengeCompleted)
			{
				return gameSession.Players[0].state.alive;
			}
			return false;
		}
		if (allPlayersExitedCounter < 40)
		{
			return false;
		}
		if (gameSession.GameTypeSetup.denEntryRule == ArenaSetup.GameTypeSetup.DenEntryRule.Always)
		{
			return true;
		}
		if (gameSession.GameTypeSetup.denEntryRule == ArenaSetup.GameTypeSetup.DenEntryRule.Score)
		{
			return anyPlayerHasEnoughScore;
		}
		if (gameSession.Players.Count == 1)
		{
			return gameSession.ScoreOfPlayer(gameSession.Players[0].realizedCreature as Player, inHands: true) > 0;
		}
		if ((gameSession.earlyRain == null || gameSession.earlyRain.earlyRainCounter <= -1) && base.game.world.rainCycle.TimeUntilRain >= 800)
		{
			return gameSession.thisFrameActivePlayers == 1;
		}
		return true;
	}
}
