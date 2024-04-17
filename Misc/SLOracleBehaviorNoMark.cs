using System;
using System.Collections.Generic;
using MoreSlugcats;
using OverseerHolograms;
using RWCustom;
using UnityEngine;

public class SLOracleBehaviorNoMark : SLOracleBehavior
{
	public List<SoundID> talkLines;

	public List<SoundID> protestLines;

	private bool initGreeting;

	public OverseerAbstractAI lockedOverseer;

	public int noticedPlayerTime;

	public bool hasProtested;

	public int timeSinceFirstInteraction;

	public int timeSinceSecondInteraction;

	public int secondInteractionCycle;

	public SLOracleBehaviorNoMark(Oracle oracle)
		: base(oracle)
	{
		talkLines = new List<SoundID>();
		if (oracle.room.game.session is StoryGameSession && (oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.SpeakingTerms)
		{
			switch ((oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters)
			{
			case 0:
				talkLines.Add(SoundID.SL_AI_Talk_1);
				talkLines.Add(SoundID.SL_AI_Talk_2);
				break;
			case 1:
				talkLines.Add(SoundID.SL_AI_Talk_3);
				break;
			case 2:
				talkLines.Add(SoundID.SL_AI_Talk_4);
				break;
			case 3:
				talkLines.Add(SoundID.SL_AI_Talk_5);
				break;
			default:
				switch (UnityEngine.Random.Range(0, 4))
				{
				case 0:
					talkLines.Add(SoundID.SL_AI_Talk_3);
					break;
				case 1:
					talkLines.Add(SoundID.SL_AI_Talk_4);
					break;
				case 2:
					talkLines.Add(SoundID.SL_AI_Talk_5);
					break;
				}
				break;
			}
		}
		else
		{
			talkLines.Add(SoundID.SL_AI_Talk_1);
		}
		protestLines = new List<SoundID>();
		protestLines.Add(SoundID.SL_AI_Protest_1);
		protestLines.Add(SoundID.SL_AI_Protest_2);
		protestLines.Add(SoundID.SL_AI_Protest_3);
		protestLines.Add(SoundID.SL_AI_Protest_4);
		protestLines.Add(SoundID.SL_AI_Protest_5);
		base.State.increaseLikeOnSave = false;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (timeSinceFirstInteraction > 0)
		{
			timeSinceFirstInteraction++;
		}
		if (timeSinceSecondInteraction > 0)
		{
			timeSinceSecondInteraction++;
		}
		if (ModManager.MMF && MMF.cfgExtraTutorials.Value && lockedOverseer != null)
		{
			if (lockedOverseer.parent.Room.name != "SL_AI")
			{
				lockedOverseer.ResetTargetCreature();
				lockedOverseer.SetDestinationNoPathing(new WorldCoordinate(oracle.room.world.GetAbstractRoom("SL_AI").index, 76, 6, -1), migrate: true);
			}
			lockedOverseer.goToPlayer = true;
			oracle.room.abstractRoom.roomAttractions[37] = AbstractRoom.CreatureRoomAttraction.Neutral;
			if (lockedOverseer.parent.realizedCreature != null && lockedOverseer.parent.Room.name == "SL_AI" && timeSinceFirstInteraction >= 400)
			{
				Overseer overseer = lockedOverseer.parent.realizedCreature as Overseer;
				overseer.AI.communication.hasAlreadyShowedPlayerToAShelter = false;
				overseer.AI.communication.GuideState.handHolding = 1f;
				overseer.AI.communication.GuideState.wantShelterHandHoldingThisCycle = 1f;
				overseer.AI.communication.firstFewCycles = true;
				overseer.AI.communication.currentConcern = OverseerCommunicationModule.PlayerConcern.Shelter;
				overseer.AI.communication.currentConcernWeight = 1f;
				overseer.AI.communication.showedImageTime = 0;
				overseer.forceShowHologram = false;
				if (timeSinceSecondInteraction >= 10 || protest || (base.player != null && base.player.mainBodyChunk.pos.x > 1090f))
				{
					secondInteractionCycle++;
					if (protest || (oracle.health < 1f && oracle.spasms > 0) || oracle.health < 0.75f)
					{
						overseer.mainBodyChunk.vel = Custom.RNV() * Mathf.Lerp(1.4f, 5f, 1f - oracle.health);
						overseer.AI.communication.currentConcern = OverseerCommunicationModule.PlayerConcern.Anger;
						overseer.AI.communication.currentConcernWeight = 1f;
						overseer.forceShowHologram = true;
						float likesPlayer = overseer.AI.communication.GuideState.likesPlayer;
						overseer.TryAddHologram(OverseerHologram.Message.Angry, base.player, float.MaxValue);
						overseer.AI.communication.GuideState.likesPlayer = likesPlayer;
						secondInteractionCycle = 0;
					}
					else if (timeSinceFirstInteraction >= 150 && secondInteractionCycle % 500 <= 300)
					{
						overseer.AI.communication.currentConcernWeight = 0.8f;
						overseer.forceShowHologram = true;
						overseer.TryAddHologram(OverseerHologram.Message.GateScene, base.player, float.MaxValue);
					}
				}
				if (base.player != null && base.player.mainBodyChunk.pos.x > 1090f && (overseer.firstChunk.pos.x < 1700f || overseer.firstChunk.pos.y < 100f || overseer.firstChunk.pos.y > 500f))
				{
					overseer.ZipToPosition(new Vector2(UnityEngine.Random.Range(1840f, 1860f), UnityEngine.Random.Range(120f, 300f)));
				}
				if (oracle.health == 0f)
				{
					overseer.TryAddHologram(OverseerHologram.Message.Angry, base.player, float.MaxValue);
					overseer.AI.communication.currentConcern = OverseerCommunicationModule.PlayerConcern.Anger;
					overseer.AI.communication.currentConcernWeight = 1f;
					overseer.forceShowHologram = false;
					overseer.forceShelterNeed = false;
					overseer.AI.communication.GuideState.handHolding = 0f;
					overseer.AI.communication.GuideState.wantShelterHandHoldingThisCycle = 0f;
					(overseer.abstractCreature.abstractAI as OverseerAbstractAI).PlayerGuideGoAway(400);
					lockedOverseer = null;
				}
			}
		}
		AbstractCreature firstAlivePlayer = oracle.room.game.FirstAlivePlayer;
		if (oracle.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.Room.index == oracle.room.abstractRoom.index && oracle.Consious && lockedOverseer == null)
		{
			AbstractRoom abstractRoom = oracle.room.world.GetAbstractRoom(UnityEngine.Random.Range(oracle.room.world.firstRoomIndex, oracle.room.world.firstRoomIndex + oracle.room.world.NumberOfRooms));
			for (int i = 0; i < abstractRoom.creatures.Count; i++)
			{
				if (abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Overseer && (abstractRoom.creatures[i].abstractAI as OverseerAbstractAI).playerGuide)
				{
					if (!ModManager.MMF || !MMF.cfgExtraTutorials.Value || base.State.playerEncounters > 1)
					{
						(abstractRoom.creatures[i].abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(abstractRoom.world.GetAbstractRoom("SL_A15").index);
					}
					else if (lockedOverseer == null)
					{
						lockedOverseer = abstractRoom.creatures[i].abstractAI as OverseerAbstractAI;
					}
					break;
				}
			}
		}
		if (ModManager.MMF && MMF.cfgExtraTutorials.Value && oracle.room.game.IsStorySession && (oracle.room.game.StoryCharacter == SlugcatStats.Name.White || oracle.room.game.StoryCharacter == SlugcatStats.Name.Yellow) && protest && base.State.playerEncounters <= 1 && lockedOverseer == null && oracle.Consious)
		{
			(oracle.room.world.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.InfluenceLike(1000f, print: false);
			WorldCoordinate worldCoordinate = new WorldCoordinate(oracle.room.world.offScreenDen.index, -1, -1, 0);
			AbstractCreature abstractCreature = new AbstractCreature(oracle.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, worldCoordinate, new EntityID(-1, 5));
			if (oracle.room.world.GetAbstractRoom(worldCoordinate).offScreenDen)
			{
				oracle.room.world.GetAbstractRoom(worldCoordinate).entitiesInDens.Add(abstractCreature);
			}
			else
			{
				oracle.room.world.GetAbstractRoom(worldCoordinate).AddEntity(abstractCreature);
			}
			abstractCreature.ignoreCycle = true;
			lockedOverseer = abstractCreature.abstractAI as OverseerAbstractAI;
			lockedOverseer.SetAsPlayerGuide(1);
			lockedOverseer.BringToRoomAndGuidePlayer(oracle.room.abstractRoom.index);
		}
		if (!oracle.Consious)
		{
			return;
		}
		if (!ModManager.MSC || !SingularityProtest())
		{
			protest = false;
		}
		if (base.player != null && hasNoticedPlayer)
		{
			noticedPlayerTime++;
			lookPoint = base.player.DangerPos;
			if (voice == null && UnityEngine.Random.value < 0.025f && Custom.DistLess(oracle.firstChunk.pos, base.player.DangerPos, 350f) && talkLines.Count > 0 && !initGreeting)
			{
				AirVoice(talkLines[0]);
				talkLines.RemoveAt(0);
				initGreeting = true;
				if (timeSinceFirstInteraction == 0)
				{
					timeSinceFirstInteraction = 1;
				}
			}
			if (oracle.room.game.session is StoryGameSession && !conversationAdded)
			{
				(oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters++;
				(oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.guideSymbol = 2;
				Custom.Log("player encounter with SL AI logged");
				if (oracle.room.world.overseersWorldAI != null)
				{
					oracle.room.world.overseersWorldAI.DitchDirectionGuidance();
				}
				conversationAdded = true;
			}
		}
		if (base.player != null)
		{
			if (base.player.room == oracle.room && Custom.DistLess(oracle.firstChunk.pos, base.player.DangerPos, Mathf.Lerp(180f, 50f, oracle.health)))
			{
				if (timeSinceSecondInteraction == 0)
				{
					timeSinceSecondInteraction = 1;
				}
				float num = Mathf.InverseLerp(Mathf.Lerp(180f, 50f, oracle.health), 30f, Vector2.Distance(oracle.firstChunk.pos, base.player.DangerPos));
				oracle.WeightedPush(0, 1, Custom.DirVec(base.player.DangerPos, oracle.firstChunk.pos), num * 1.5f);
				oracle.bodyChunks[1].vel *= 1f - 0.5f * num;
				oracle.bodyChunks[1].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.oracleBehavior.GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 30f) / 30f * 1.2f * num;
				holdKnees = false;
				if (voice == null && UnityEngine.Random.value < 0.05f && talkLines.Count > 0)
				{
					AirVoice(talkLines[0]);
					talkLines.RemoveAt(0);
				}
			}
			for (int j = 0; j < oracle.room.game.Players.Count; j++)
			{
				if (oracle.room.game.Players[j].realizedCreature == null || oracle.room.game.Players[j].realizedCreature.room != oracle.room)
				{
					continue;
				}
				Player player = oracle.room.game.Players[j].realizedCreature as Player;
				for (int k = 0; k < player.grasps.Length; k++)
				{
					if (player.grasps[k] != null && player.grasps[k].grabbed is SLOracleSwarmer)
					{
						protest = true;
						hasProtested = true;
						holdKnees = false;
						oracle.bodyChunks[0].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value;
						oracle.bodyChunks[1].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value * 2f;
						protestCounter += 1f / 22f;
						lookPoint = oracle.bodyChunks[0].pos + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * Mathf.Sin(protestCounter * (float)Math.PI * 2f) * 145f;
						if (UnityEngine.Random.value < 1f / 30f)
						{
							armsProtest = !armsProtest;
						}
						if ((voice == null && UnityEngine.Random.value < 0.003125f && protestLines.Count > 0) || protestLines.Count >= 5)
						{
							int index = UnityEngine.Random.Range(0, protestLines.Count);
							AirVoice(protestLines[index]);
							protestLines.RemoveAt(index);
						}
						break;
					}
				}
			}
		}
		if (!protest)
		{
			armsProtest = false;
		}
		if (holdingObject != null)
		{
			lookPoint = holdingObject.firstChunk.pos;
		}
	}
}
