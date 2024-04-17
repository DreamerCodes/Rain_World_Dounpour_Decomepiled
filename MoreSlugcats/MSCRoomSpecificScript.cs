using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoralBrain;
using JollyCoop;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public static class MSCRoomSpecificScript
{
	public class GW_C05ArtificerMessage : UpdatableAndDeletable
	{
		public GW_C05ArtificerMessage(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ArtificerTutorialMessage && player != null && player.room == room)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ArtificerTutorialMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("While in the air, press jump and pick-up together, to propel yourself explosively."), 20, 440, darken: true, hideHud: true);
				if (room.game.cameras[0].hud.textPrompt.subregionTracker != null)
				{
					room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
				}
				CutsceneArtificer.ToggleScavengerAccessToArtyIntro(room, ScavAccess: true);
				Destroy();
			}
		}
	}

	public class SpearmasterGateLocation : UpdatableAndDeletable
	{
		public int ticker;

		public SpearmasterGateLocation(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.Players.Count <= 0 || !room.game.AllPlayersRealized)
			{
				return;
			}
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (ticker == 0)
				{
					room.regionGate.mode = RegionGate.Mode.ClosingMiddle;
					room.regionGate.doors[1].closedFac = 0f;
					(room.game.Players[i].realizedCreature as Player).bodyChunks[0].HardSetPosition(new Vector2(570f, 160f));
					(room.game.Players[i].realizedCreature as Player).bodyChunks[1].HardSetPosition(new Vector2(570f, 150f));
					(room.game.Players[i].realizedCreature as Player).bodyChunks[0].vel *= 0f;
					(room.game.Players[i].realizedCreature as Player).bodyChunks[1].vel *= 0f;
					(room.game.Players[i].realizedCreature as Player).sleepCounter = 0;
					(room.game.Players[i].realizedCreature as Player).standing = true;
					(room.game.Players[i].realizedCreature as Player).controller = new Player.NullController();
					(room.game.Players[i].realizedCreature as Player).playerState.foodInStomach = 4;
				}
				else if (ticker == 120)
				{
					(room.game.Players[i].realizedCreature as Player).controller = null;
					room.game.GetStorySession.saveState.miscWorldSaveData.playerGuideState.likesPlayer = 0.94f;
					room.game.GetStorySession.saveState.miscWorldSaveData.playerGuideState.wantDirectionHandHoldingThisCycle = 0.96f;
					Destroy();
				}
			}
			ticker++;
		}
	}

	public class SU_SMIntroMessage : UpdatableAndDeletable
	{
		public SU_SMIntroMessage(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SMTutorialMessage && player != null && player.room == room)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SMTutorialMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold the pick-up button to pull needles from your body."), 20, 240, darken: true, hideHud: true);
				if (room.game.cameras[0].hud.textPrompt.subregionTracker != null)
				{
					room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
				}
				Destroy();
			}
		}
	}

	public class SU_A42Message : UpdatableAndDeletable
	{
		public SU_A42Message(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = room.game.RealizedPlayerFollowedByCamera;
			}
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SMEatTutorial && player != null && player.room == room)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SMEatTutorial = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Stab creatures with your white needles to leech energy from their bodies."), 20, 320, darken: true, hideHud: true);
				if (room.game.cameras[0].hud.textPrompt.subregionTracker != null)
				{
					room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
				}
				Destroy();
			}
		}
	}

	public class VS_E05WrapAround : UpdatableAndDeletable, ISpecialWarp
	{
		public float target_blend;

		public bool loadStarted;

		public FadeOut fadeObj;

		private RoomSettings.RoomEffect StoredEffect;

		private bool clearedSpawn;

		public KarmaVectorX karmaObj;

		public FadeOut blackFade;

		public int phaseTimer;

		public int karmaSymbolWait;

		public VS_E05WrapAround(Room room)
		{
			base.room = room;
			for (int i = 0; i < room.roomSettings.effects.Count; i++)
			{
				if (room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.VoidSpawn)
				{
					StoredEffect = room.roomSettings.effects[i];
					return;
				}
			}
			clearedSpawn = false;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature == null || (room.game.Players[i].realizedCreature as Player).room != room)
				{
					continue;
				}
				Player player = room.game.Players[i].realizedCreature as Player;
				player.allowOutOfBounds = true;
				if (player.mainBodyChunk.pos.x < -248f)
				{
					player.SuperHardSetPosition(new Vector2(room.RoomRect.right + 232f, player.mainBodyChunk.pos.y));
				}
				if (player.mainBodyChunk.pos.x > room.RoomRect.right + 248f)
				{
					player.SuperHardSetPosition(new Vector2(-232f, player.mainBodyChunk.pos.y));
				}
				if (player.KarmaCap >= 9)
				{
					if (room.game.cameras[0].paletteBlend != target_blend)
					{
						if (Mathf.Abs(room.game.cameras[0].paletteBlend - target_blend) < 0.01f)
						{
							room.game.cameras[0].ChangeFadePalette(room.game.cameras[0].paletteB, target_blend);
						}
						else
						{
							room.game.cameras[0].ChangeFadePalette(room.game.cameras[0].paletteB, Mathf.Lerp(room.game.cameras[0].paletteBlend, target_blend, 0.1f));
						}
					}
					if (player.mainBodyChunk.pos.y < -118f)
					{
						target_blend = Mathf.Clamp(target_blend + 0.1f, 0f, 1f);
						room.game.cameras[0].ChangeFadePalette(room.game.cameras[0].paletteB, Mathf.Clamp(room.game.cameras[0].paletteBlend + 0.05f, 0f, 1f));
						player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, 742f));
						player.Stun(300);
						ClearAllVoidSpawn();
						StoredEffect.amount = 0f;
						for (int j = 0; j < player.bodyChunks.Length; j++)
						{
							player.bodyChunks[j].vel.y = Mathf.Clamp(player.bodyChunks[j].vel.y, -15f, 15f);
						}
					}
					if (player.mainBodyChunk.pos.y > 768f && target_blend > 0f)
					{
						if (target_blend < 1f)
						{
							target_blend = Mathf.Clamp(target_blend - 0.1f, 0f, 1f);
						}
						player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, -102f));
						player.Stun(300);
						ClearAllVoidSpawn();
						StoredEffect.amount = 0f;
					}
					if (target_blend >= 0.9f && blackFade == null)
					{
						player.hideGodPips = true;
					}
					if (target_blend == 1f && fadeObj == null)
					{
						fadeObj = new FadeOut(room, Color.white, 130f, fadeIn: false);
						room.AddObject(fadeObj);
					}
					if (fadeObj != null && fadeObj.IsDoneFading() && blackFade == null)
					{
						karmaSymbolWait++;
					}
					if (karmaSymbolWait > 40 && blackFade == null)
					{
						if (karmaObj == null)
						{
							karmaObj = new KarmaVectorX(room.game.cameras[0].pos + room.game.cameras[0].sSize / 2f, 600f, 20f, 1f);
							karmaObj.segments = new Vector2[48, 3];
							karmaObj.color = new Color(1f, 1f, 1f);
							karmaObj.container = "HUD";
							room.AddObject(karmaObj);
						}
						phaseTimer++;
						float num = Math.Min(1f, (float)phaseTimer / 130f);
						karmaObj.color = new Color(1f - num, 1f - num, 1f - num);
					}
					if (fadeObj != null && blackFade == null && phaseTimer > 130)
					{
						phaseTimer = 0;
						karmaObj.Destroy();
						karmaObj = new KarmaVectorX(room.game.cameras[0].pos + room.game.cameras[0].sSize / 2f, 600f, 20f, 1f);
						karmaObj.segments = new Vector2[48, 3];
						karmaObj.color = new Color(0f, 0f, 0f);
						karmaObj.container = "Bloom";
						blackFade = new FadeOut(room, Color.black, 130f, fadeIn: false);
						room.AddObject(blackFade);
						room.AddObject(karmaObj);
					}
					if (blackFade != null && karmaObj != null)
					{
						karmaObj.color = new Color(blackFade.fade, blackFade.fade, blackFade.fade);
					}
					if (blackFade != null && blackFade.IsDoneFading())
					{
						phaseTimer++;
						karmaObj.alpha = Mathf.Max(0f, 1f - (float)phaseTimer / 20f);
					}
					if (blackFade != null && blackFade.IsDoneFading() && phaseTimer > 20 && !loadStarted)
					{
						loadStarted = true;
						room.world.game.globalRain.ResetRain();
						room.game.overWorld.InitiateSpecialWarp(OverWorld.SpecialWarpType.WARP_VS_HR, this);
						RainWorldGame.ForceSaveNewDenLocation(room.game, "HR_C01", saveWorldStates: true);
						break;
					}
					continue;
				}
				if (player.mainBodyChunk.pos.y > room.RoomRect.top + 48f)
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, 798f));
				}
				if (!(player.mainBodyChunk.pos.y < 782f))
				{
					continue;
				}
				if (player.mainBodyChunk.pos.x > 300f && player.mainBodyChunk.pos.x < 620f)
				{
					if (player.mainBodyChunk.pos.x < 460f)
					{
						player.SuperHardSetPosition(new Vector2(300f, room.RoomRect.top + 32f));
					}
					else
					{
						player.SuperHardSetPosition(new Vector2(620f, room.RoomRect.top + 32f));
					}
				}
				else
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, room.RoomRect.top + 32f));
				}
				for (int k = 0; k < player.bodyChunks.Length; k++)
				{
					player.bodyChunks[k].vel.y = Mathf.Clamp(player.bodyChunks[k].vel.y, -15f, 15f);
				}
			}
		}

		public Room getSourceRoom()
		{
			return room;
		}

		public void NewWorldLoaded()
		{
		}

		private void ClearAllVoidSpawn()
		{
			if (clearedSpawn)
			{
				return;
			}
			clearedSpawn = true;
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is VoidSpawn)
				{
					room.updateList[i].slatedForDeletetion = true;
				}
			}
		}
	}

	public class HR_C01RegionIntro : UpdatableAndDeletable
	{
		public int waitBeforeDrop;

		public int waitBeforeFade;

		public FadeOut fadeObj;

		public HR_C01RegionIntro(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (fadeObj == null)
			{
				fadeObj = new FadeOut(room, Color.black, 130f, fadeIn: true);
				room.AddObject(fadeObj);
			}
			if (!room.game.AllPlayersRealized)
			{
				return;
			}
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				Player player = room.game.Players[i].realizedCreature as Player;
				player.hideGodPips = false;
				if (waitBeforeDrop < 45)
				{
					player.SuperHardSetPosition(new Vector2(1520f, 840f));
					for (int j = 0; j < player.bodyChunks.Length; j++)
					{
						player.bodyChunks[j].vel.y = Mathf.Clamp(player.bodyChunks[j].vel.y, -5f, 5f);
						player.bodyChunks[j].vel.x = 0f;
					}
					if (room.game.cameras[0].currentCameraPosition != 1)
					{
						room.game.cameras[0].MoveCamera(1);
					}
				}
				else if (waitBeforeDrop == 45)
				{
					for (int k = 0; k < player.bodyChunks.Length; k++)
					{
						player.bodyChunks[k].vel.y = -35f;
						player.bodyChunks[k].vel.x = 0f;
					}
					player.graphicsModule.Reset();
				}
			}
			if (waitBeforeDrop == 45)
			{
				Destroy();
			}
			if (fadeObj != null && fadeObj.IsDoneFading())
			{
				waitBeforeDrop++;
			}
			if (fadeObj != null && waitBeforeFade < 30)
			{
				fadeObj.fade = 1f;
			}
			waitBeforeFade++;
		}
	}

	public class SpearmasterEnding : UpdatableAndDeletable, IRunDuringDialog
	{
		public enum SMEndingState
		{
			START,
			PEARLDATA,
			BROADCAST,
			SCROLL,
			CHATLOG,
			END
		}

		public class AttractedGlyph : UpdatableAndDeletable, IDrawable
		{
			public Vector2 pos;

			public Vector2 vel;

			public Vector2 attractPos;

			public int liveTime;

			public AttractedGlyph(Vector2 pos, Vector2 vel, Vector2 attractPos, int liveTime)
			{
				this.pos = pos;
				this.vel = vel;
				this.attractPos = attractPos;
				this.liveTime = liveTime;
			}

			public override void Update(bool eu)
			{
				base.Update(eu);
				liveTime--;
				pos += vel;
				Custom.Dist(pos, attractPos);
				vel.x += (attractPos.x - pos.x) * 0.075f * (1f - 0.1f * UnityEngine.Random.value);
				vel.y += (attractPos.y - pos.y) * 0.075f * (1f - 0.1f * UnityEngine.Random.value);
				if (vel.magnitude > 20f)
				{
					vel /= 4f;
				}
				if (liveTime == 0)
				{
					Destroy();
				}
			}

			public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				sLeaser.sprites = new FSprite[1];
				sLeaser.sprites[0] = new FSprite("haloGlyph" + UnityEngine.Random.Range(0, 7));
				sLeaser.sprites[0].x = pos.x;
				sLeaser.sprites[0].y = pos.y;
				sLeaser.sprites[0].scale = 0.5f;
				float value = UnityEngine.Random.value;
				sLeaser.sprites[0].color = new Color(value, value, value);
				AddToContainer(sLeaser, rCam, null);
			}

			public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
			{
				sLeaser.sprites[0].x = pos.x - camPos.x;
				sLeaser.sprites[0].y = pos.y - camPos.y;
				if (base.slatedForDeletetion || room != rCam.room)
				{
					sLeaser.CleanSpritesAndRemove();
				}
			}

			public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
			{
			}

			public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
			{
				newContatiner = rCam.ReturnFContainer("Bloom");
				sLeaser.sprites[0].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[0]);
			}
		}

		public SpearMasterPearl SMEndingPearl;

		public AboveCloudsView SMEndingCloudsView;

		public SMEndingState SMEndingPhase;

		public FadeOut SMEndingFade;

		public float SMEndingTimer;

		public SpearmasterEnding(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count <= 0 || firstAlivePlayer == null || firstAlivePlayer.realizedCreature == null || firstAlivePlayer.realizedCreature.room != room)
			{
				return;
			}
			Player player = firstAlivePlayer.realizedCreature as Player;
			if (room.game.GetStorySession.saveState.denPosition == room.abstractRoom.name && room.world.rainCycle.timer < 400)
			{
				player.SuperHardSetPosition(new Vector2(540f, 147f));
				player.abstractCreature.pos = room.ToWorldCoordinate(new Vector2(540f, 147f));
				player.sleepCurlUp = 1f;
				player.sleepCounter = 99;
				Destroy();
			}
			if (SMEndingPhase != 0)
			{
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					if (room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat)
					{
						room.abstractRoom.creatures[i].realizedCreature.stun = Math.Max(room.abstractRoom.creatures[i].realizedCreature.stun, 20);
					}
				}
			}
			if (SMEndingPhase == SMEndingState.START && player.mainBodyChunk.pos.y < 430f && UnityEngine.Random.value < 0.02f)
			{
				for (int j = 0; j < player.grasps.Length; j++)
				{
					if (player.grasps[j] != null && player.grasps[j].grabbed is SpearMasterPearl)
					{
						SpearMasterPearl spearMasterPearl = player.grasps[j].grabbed as SpearMasterPearl;
						if ((spearMasterPearl.abstractPhysicalObject as SpearMasterPearl.AbstractSpearMasterPearl).broadcastTagged)
						{
							Vector2 attractPos = new Vector2(470f, 560f);
							room.AddObject(new AttractedGlyph(spearMasterPearl.firstChunk.pos, -spearMasterPearl.firstChunk.vel + Custom.RNV() * 3f, attractPos, 80));
							room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, spearMasterPearl.firstChunk.pos, 1f, 0.5f + UnityEngine.Random.value * 2f);
							break;
						}
					}
				}
			}
			if (player.mainBodyChunk.pos.y > 430f && player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam && SMEndingPhase == SMEndingState.START)
			{
				SMEndingPearl = null;
				int grasp = 0;
				for (int k = 0; k < player.grasps.Length; k++)
				{
					if (player.grasps[k] != null && player.grasps[k].grabbed is SpearMasterPearl)
					{
						grasp = k;
						SMEndingPearl = player.grasps[k].grabbed as SpearMasterPearl;
						if (!(SMEndingPearl.abstractPhysicalObject as SpearMasterPearl.AbstractSpearMasterPearl).broadcastTagged)
						{
							SMEndingPearl = null;
						}
					}
				}
				if (SMEndingPearl != null)
				{
					RainWorld.lockGameTimer = true;
					player.controller = new Player.NullController();
					player.ReleaseGrasp(grasp);
					SMEndingPearl.ChangeCollisionLayer(1);
					SMEndingPearl.SetLocalGravity(0f);
					SMEndingPearl.firstChunk.vel.x = 8f;
					SMEndingPearl.firstChunk.vel.y = -5f;
					SMEndingPhase = SMEndingState.PEARLDATA;
					room.game.manager.CueAchievement(RainWorld.AchievementID.SpearmasterEnding, 5f);
					room.AddObject(new ElectricDeath.SparkFlash(SMEndingPearl.firstChunk.pos, 0.75f + UnityEngine.Random.value));
					room.AddObject(new ShockWave(SMEndingPearl.firstChunk.pos, 50f, 1f, 30));
					room.PlaySound(SoundID.HUD_Exit_Game, SMEndingPearl.firstChunk.pos, 1f, 2f);
				}
			}
			if (SMEndingPhase == SMEndingState.PEARLDATA)
			{
				SMEndingTimer += 1f;
				Vector2 vector = new Vector2(470f, 560f);
				float num = Custom.Dist(SMEndingPearl.firstChunk.pos, vector) * 0.01f + Mathf.Max(1f, 20f * (1f - SMEndingTimer / 240f));
				BodyChunk firstChunk = SMEndingPearl.firstChunk;
				firstChunk.vel.x = firstChunk.vel.x + (vector.x - SMEndingPearl.firstChunk.pos.x) * (0.0015f + SMEndingTimer * 0.0001f);
				firstChunk.vel.y = firstChunk.vel.y + (vector.y - SMEndingPearl.firstChunk.pos.y) * (0.0003f + SMEndingTimer * 0.0002f);
				if (firstChunk.pos.y < 530f && firstChunk.pos.x > 445f && firstChunk.pos.x < 500f)
				{
					BodyChunk bodyChunk = firstChunk;
					bodyChunk.vel.x = bodyChunk.vel.x + 1f;
				}
				if (SMEndingPearl.firstChunk.vel.magnitude > num)
				{
					SMEndingPearl.firstChunk.vel = SMEndingPearl.firstChunk.vel.normalized * num;
				}
				if (SMEndingTimer == 120f)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference, 0f, 1f, 1f);
				}
				if (SMEndingTimer == 320f)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference2, 0f, 1f, 1f);
				}
				if (SMEndingTimer > 120f && ((SMEndingTimer < 320f && SMEndingTimer % 6f == 0f) || (SMEndingTimer > 320f && SMEndingTimer % 2f == 0f)))
				{
					room.AddObject(new AttractedGlyph(SMEndingPearl.firstChunk.pos, -SMEndingPearl.firstChunk.vel + Custom.RNV() * ((SMEndingTimer < 320f) ? 10f : 5f), vector, (SMEndingTimer < 320f) ? 90 : 30));
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, SMEndingPearl.firstChunk.pos, 1f, 0.5f + UnityEngine.Random.value * 2f);
				}
				if (SMEndingTimer > 480f)
				{
					RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
					SMEndingTimer = 0f;
					SMEndingPhase = SMEndingState.SCROLL;
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 1f);
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 1f);
					SMEndingPearl.firstChunk.vel = Vector2.zero;
					SMEndingPearl.SetLocalGravity(0.9f);
					room.AddObject(new ElectricDeath.SparkFlash(SMEndingPearl.firstChunk.pos, 0.75f + UnityEngine.Random.value));
					room.PlaySound(SoundID.HUD_Exit_Game, SMEndingPearl.firstChunk.pos, 1f, 1f);
				}
			}
			if (SMEndingPhase == SMEndingState.SCROLL && SMEndingCloudsView == null)
			{
				SMEndingTimer += 1f;
				if (SMEndingTimer > 60f && room.game.manager.musicPlayer != null && !(room.game.manager.musicPlayer.song is SMEndingSong))
				{
					room.game.manager.musicPlayer.RequestSMEndingSong();
					(room.game.manager.musicPlayer.song as SMEndingSong).baseVolume = 0f;
				}
				if (SMEndingTimer > 120f)
				{
					foreach (UpdatableAndDeletable update in room.updateList)
					{
						if (update is AboveCloudsView)
						{
							SMEndingCloudsView = update as AboveCloudsView;
							break;
						}
					}
					if (SMEndingCloudsView != null)
					{
						SMEndingCloudsView.animateSMEndingScroll = true;
					}
				}
			}
			if (SMEndingCloudsView != null && SMEndingCloudsView.yShift >= 19900f && SMEndingPhase == SMEndingState.SCROLL)
			{
				SMEndingPhase = SMEndingState.CHATLOG;
				SMEndingTimer = 0f;
			}
			if (SMEndingPhase == SMEndingState.CHATLOG && SMEndingTimer < 120f)
			{
				SMEndingTimer += 1f;
				if (SMEndingTimer >= 120f)
				{
					player.InitChatLog(ChatlogData.ChatlogID.Chatlog_SI9);
				}
			}
			if (SMEndingPhase == SMEndingState.CHATLOG && !player.chatlog && SMEndingTimer >= 120f)
			{
				SMEndingPhase = SMEndingState.END;
				SMEndingFade = new FadeOut(room, Color.black, 640f, fadeIn: false);
				room.AddObject(SMEndingFade);
			}
			if (SMEndingFade != null)
			{
				room.game.cameras[0].virtualMicrophone.globalSoundMuffle = SMEndingFade.fade;
			}
			if (SMEndingPhase == SMEndingState.END && SMEndingFade != null && SMEndingFade.IsDoneFading())
			{
				room.game.manager.statsAfterCredits = true;
				room.game.manager.desiredCreditsSong = "NA_11 - Digital Sundown";
				room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
				SMEndingFade = null;
			}
			if (room.game.manager.musicPlayer != null && room.game.manager.musicPlayer.song is SMEndingSong)
			{
				if (SMEndingFade != null)
				{
					(room.game.manager.musicPlayer.song as SMEndingSong).setVolume = Mathf.Max(0f, 1f - SMEndingFade.fade * 2f) * 0.2f;
					return;
				}
				if (SMEndingPhase != SMEndingState.END)
				{
					(room.game.manager.musicPlayer.song as SMEndingSong).setVolume = 0.2f;
				}
			}
			if (room.game.cameras[0].hud.chatLog != null)
			{
				room.game.cameras[0].hud.chatLog.disable_fastDisplay = true;
			}
		}
	}

	public class InvSpawnLocation : UpdatableAndDeletable
	{
		public InvSpawnLocation(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!room.game.AllPlayersRealized)
			{
				return;
			}
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					(room.game.Players[i].realizedCreature as Player).bodyChunks[j].HardSetPosition(room.MiddleOfTile(34, 10));
				}
				(room.game.Players[i].realizedCreature as Player).standing = false;
				if (i == 0)
				{
					AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.game.Players[i].Room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate((room.game.Players[i].realizedCreature as Player).mainBodyChunk.pos), room.world.game.GetNewID());
					room.game.Players[i].Room.AddEntity(abstractPhysicalObject);
					abstractPhysicalObject.RealizeInRoom();
				}
			}
			Destroy();
		}
	}

	public class RM_CORE_EnergyCell : UpdatableAndDeletable
	{
		private EnergyCell myEnergyCell;

		public Player player;

		public RM_CORE_EnergyCell(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Vector2 vector = new Vector2(469f, 373f);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				player = firstAlivePlayer.realizedCreature as Player;
			}
			else
			{
				player = null;
			}
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken && myEnergyCell == null)
			{
				AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.EnergyCell, null, room.GetWorldCoordinate(vector), room.game.GetNewID());
				abstractPhysicalObject.destroyOnAbstraction = true;
				room.abstractRoom.AddEntity(abstractPhysicalObject);
				abstractPhysicalObject.RealizeInRoom();
				myEnergyCell = abstractPhysicalObject.realizedObject as EnergyCell;
				myEnergyCell.firstChunk.pos = vector;
			}
			if (myEnergyCell != null && player != null && player.room == myEnergyCell.room && !(room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken)
			{
				myEnergyCell.customAnimation = true;
				float num = Custom.Dist(player.firstChunk.pos, myEnergyCell.firstChunk.pos);
				myEnergyCell.moveToTarget = Mathf.Clamp(num / 150f, 0f, 1f) * 0.3f;
				myEnergyCell.firstChunk.pos = vector;
				myEnergyCell.firstChunk.vel = Vector2.zero;
				if (myEnergyCell.grabbedBy.Count > 0)
				{
					(room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken = true;
					if (room.world.overseersWorldAI != null)
					{
						room.world.overseersWorldAI.DitchDirectionGuidance();
					}
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Removed, 0f, 1f, 1f);
					room.PlaySound(SoundID.Zapper_Zap, 0f, 1f, 1f);
					room.AddObject(new ShockWave(vector, 150f, 1f, 60));
					for (int i = 0; i < 10; i++)
					{
						room.AddObject(new Spark(vector + Custom.RNV() * UnityEngine.Random.value * 40f, Custom.RNV() * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
					}
					room.AddObject(new ElectricDeath.SparkFlash(vector, 1.5f));
					for (int j = 0; j < room.lightningMachines.Count; j++)
					{
						room.lightningMachines[j].intensity = 0f;
						room.lightningMachines[j].chance = 0f;
					}
					for (int k = 0; k < room.energySwirls.Count; k++)
					{
						room.energySwirls[k].Depth = 30f;
						room.energySwirls[k].color = Color.black;
					}
					myEnergyCell.moveToTarget = 0f;
					player.Stun(150);
					player.bodyChunks[0].vel = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
					player.bodyChunks[1].vel = player.bodyChunks[0].vel;
					room.RemoveObject(myEnergyCell);
					AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.EnergyCell, null, room.GetWorldCoordinate(Vector2.zero), room.game.GetNewID());
					room.abstractRoom.AddEntity(abstractPhysicalObject2);
					abstractPhysicalObject2.RealizeInRoom();
					if (AbstractPhysicalObject.UsesAPersistantTracker(abstractPhysicalObject2))
					{
						room.game.GetStorySession.AddNewPersistentTracker(abstractPhysicalObject2);
					}
					myEnergyCell = abstractPhysicalObject2.realizedObject as EnergyCell;
					myEnergyCell.firstChunk.pos = vector;
					myEnergyCell.bodyChunks[0].vel = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
					ReloadRooms();
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold grab to activate."), 300, 160, darken: true, hideHud: true);
					return;
				}
			}
			else if ((room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken && myEnergyCell != null)
			{
				myEnergyCell.customAnimation = false;
				RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG);
				if (effect != null)
				{
					effect.amount = Mathf.Lerp(effect.amount, 0f, 0.01f);
				}
				for (int l = 0; l < room.roomSettings.ambientSounds.Count; l++)
				{
					AmbientSound ambientSound = room.roomSettings.ambientSounds[l];
					if (ambientSound.sample == "MSC-FlangePad.ogg" || ambientSound.sample == "SO_SFX-AlphaWaves.ogg")
					{
						ambientSound.volume = Mathf.Lerp(ambientSound.volume, 0f, 0.05f);
					}
				}
			}
			if ((room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken)
			{
				for (int m = 0; m < room.energySwirls.Count; m++)
				{
					room.energySwirls[m].Depth = 30f;
					room.energySwirls[m].color = Color.black;
				}
			}
		}

		public void ReloadRooms()
		{
			for (int num = room.world.activeRooms.Count - 1; num >= 0; num--)
			{
				if (room.world.activeRooms[num] != room.game.cameras[0].room)
				{
					if (room.game.roomRealizer != null)
					{
						room.game.roomRealizer.KillRoom(room.world.activeRooms[num].abstractRoom);
					}
					else
					{
						room.world.activeRooms[num].abstractRoom.Abstractize();
					}
				}
			}
		}
	}

	public class OE_PUMP01_pusher : UpdatableAndDeletable
	{
		private int giveUpCounter;

		private bool started;

		public FadeOut fadeIn;

		private int numFinished;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!started)
			{
				if ((from x in room.game.NonPermaDeadPlayers
					where x.Room != room.abstractRoom
					select x.realizedCreature as Player).Count() > 0)
				{
					giveUpCounter++;
					if ((float)giveUpCounter < 250f)
					{
						return;
					}
					JollyCustom.Log("Error! OE_PUMP! gave up because missing players!");
				}
				started = true;
				JollyCustom.Log("Starting PUMP cutscene!");
				if (room.game.cameras[0].currentCameraPosition == 0 && fadeIn == null)
				{
					fadeIn = new FadeOut(room, Color.black, 120f, fadeIn: true);
					room.AddObject(fadeIn);
				}
				foreach (Player item in room.PlayersInRoom)
				{
					if (!room.game.cameras[0].InCutscene)
					{
						room.game.cameras[0].EnterCutsceneMode(item.abstractCreature, RoomCamera.CameraCutsceneType.EndingOE);
					}
					item.bodyChunks[0].pos = new Vector2(-5f, 170f - 5f * (float)item.playerState.playerNumber);
					item.bodyChunks[1].pos = new Vector2(-5f, 170f - 5f * (float)item.playerState.playerNumber);
					item.bodyChunks[0].vel = new Vector2(30f, 0f);
					item.bodyChunks[1].vel = new Vector2(30f, 0f);
					item.Stun(150);
					item.allowOutOfBounds = true;
				}
			}
			else
			{
				foreach (Player item2 in room.PlayersInRoom)
				{
					item2.allowOutOfBounds = true;
				}
			}
			List<Player> playersInRoom = room.PlayersInRoom;
			if (playersInRoom.Count == 0)
			{
				return;
			}
			numFinished = 0;
			foreach (Player item3 in playersInRoom)
			{
				Bubble bubble = new Bubble(new Vector2(UnityEngine.Random.Range(room.game.cameras[0].pos.x - 100f, room.game.cameras[0].pos.x + 500f), UnityEngine.Random.Range(60f, 250f)), new Vector2(30f, -5f), bottomBubble: false, fakeWaterBubble: false);
				bubble.doNotSlow = true;
				bubble.age = 10;
				room.AddObject(bubble);
				if (numFinished < playersInRoom.Count)
				{
					if (item3.mainBodyChunk.pos.x < 3900f && item3.mainBodyChunk.vel.x < 20f)
					{
						BodyChunk mainBodyChunk = item3.mainBodyChunk;
						mainBodyChunk.vel.x = mainBodyChunk.vel.x + 2f;
						item3.airInLungs = 0.25f;
						if (item3.stun > 10)
						{
							item3.Stun(250);
						}
					}
					else if (item3.mainBodyChunk.pos.x < 3920f)
					{
						BodyChunk mainBodyChunk2 = item3.mainBodyChunk;
						mainBodyChunk2.vel.x = mainBodyChunk2.vel.x + 1f;
						item3.airInLungs = 0.5f;
					}
					else if (item3.mainBodyChunk.pos.x < 3930f && item3.mainBodyChunk.vel.x < 1f)
					{
						BodyChunk mainBodyChunk3 = item3.mainBodyChunk;
						mainBodyChunk3.vel.x = mainBodyChunk3.vel.x + 1f;
					}
					if (item3.mainBodyChunk.pos.x < 3910f && item3.mainBodyChunk.pos.y > 170f)
					{
						BodyChunk mainBodyChunk4 = item3.mainBodyChunk;
						mainBodyChunk4.pos.y = mainBodyChunk4.pos.y - 3f;
					}
					if (item3.mainBodyChunk.pos.x > 4050f && item3.stun > 10)
					{
						if (item3.airInLungs < 0.2f)
						{
							item3.airInLungs = 0.2f;
						}
						if (item3.mainBodyChunk.pos.x > 4132f)
						{
							BodyChunk mainBodyChunk5 = item3.mainBodyChunk;
							mainBodyChunk5.vel.x = mainBodyChunk5.vel.x - 1f;
							if (item3.stun > 0)
							{
								item3.stun--;
							}
						}
					}
					if (item3.stun < 5)
					{
						numFinished++;
						item3.airInLungs = 0.3f;
					}
				}
				else
				{
					if (item3.mainBodyChunk.pos.x < 3930f)
					{
						BodyChunk mainBodyChunk6 = item3.mainBodyChunk;
						mainBodyChunk6.vel.x = mainBodyChunk6.vel.x + 1f;
					}
					if (item3.mainBodyChunk.pos.y < 250f)
					{
						BodyChunk mainBodyChunk7 = item3.mainBodyChunk;
						mainBodyChunk7.vel.x = mainBodyChunk7.vel.x + 0.15f;
					}
				}
			}
			if (numFinished >= playersInRoom.Count)
			{
				base.slatedForDeletetion = true;
				room.game.cameras[0].ExitCutsceneMode();
			}
		}
	}

	public class OE_CAVE03_warp : UpdatableAndDeletable
	{
		private bool triggered;

		public FadeOut fadeOut;

		public float afterFadeTime;

		private bool warpedPlayers;

		public override void Update(bool eu)
		{
			base.Update(eu);
			foreach (AbstractCreature nonPermaDeadPlayer in room.game.NonPermaDeadPlayers)
			{
				if (nonPermaDeadPlayer.realizedCreature == null || nonPermaDeadPlayer.Room != room.abstractRoom)
				{
					continue;
				}
				Player player = (Player)nonPermaDeadPlayer.realizedCreature;
				if (player.mainBodyChunk.pos.y < 5f && !room.game.cameras[0].InCutscene)
				{
					room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.EndingOE);
				}
				if (player.mainBodyChunk.pos.y < -100f)
				{
					player.mainBodyChunk.pos.y = -150f;
					player.mainBodyChunk.vel.y = 0f;
					if (fadeOut == null)
					{
						fadeOut = new FadeOut(room, Color.black, 60f, fadeIn: false);
						room.AddObject(fadeOut);
					}
				}
			}
			if (fadeOut != null && !warpedPlayers && fadeOut.fade > 0.5f)
			{
				foreach (Player item in from x in room.game.NonPermaDeadPlayers
					where x.Room.name != room.abstractRoom.name
					select x.realizedCreature as Player into x
					orderby x.slugOnBack != null
					select x)
				{
					if (item.room.abstractRoom.name == "OE_PUMP01")
					{
						continue;
					}
					if (item.slugOnBack != null && item.slugOnBack.HasASlug)
					{
						item.slugOnBack.DropSlug();
					}
					warpedPlayers = true;
					JollyCustom.Log($"Warping player to OE {room.abstractRoom.name} from {item.room.abstractRoom.name}, {item} - back occupied: {item.slugOnBack.HasASlug}");
					try
					{
						WorldCoordinate worldCoordinate = room.LocalCoordinateOfNode(0);
						JollyCustom.MovePlayerWithItems(item, item.room, room.abstractRoom.name, worldCoordinate);
						Vector2 down = Vector2.down;
						for (int i = 0; i < item.bodyChunks.Length; i++)
						{
							item.bodyChunks[i].HardSetPosition(room.MiddleOfTile(worldCoordinate) - down * (-0.5f + (float)i) * 5f);
							item.bodyChunks[i].vel = down * 2f;
						}
					}
					catch (Exception ex)
					{
						JollyCustom.Log("Failed to move player " + ex, throwException: true);
						warpedPlayers = false;
					}
				}
			}
			WorldCoordinate newCoord = new WorldCoordinate(room.world.GetAbstractRoom("OE_PUMP01").index, 236, 16, -1);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			for (int j = 0; j < room.abstractRoom.entities.Count; j++)
			{
				if (!(room.abstractRoom.entities[j] is AbstractCreature))
				{
					continue;
				}
				AbstractCreature abstractCreature = (room.abstractRoom.entities[j] as AbstractPhysicalObject) as AbstractCreature;
				if (firstAlivePlayer != null && abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.firstChunk.pos.y < -100f)
				{
					if (abstractCreature.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && abstractCreature.state.socialMemory.GetLike(firstAlivePlayer.ID) > 0.5f)
					{
						abstractCreature.LoseAllStuckObjects();
						room.RemoveObject(abstractCreature.realizedCreature);
						abstractCreature.Abstractize(abstractCreature.pos);
						abstractCreature.Move(newCoord);
						break;
					}
					if (abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && abstractCreature.state.alive && abstractCreature.state.socialMemory.GetLike(firstAlivePlayer.ID) > 0.5f)
					{
						abstractCreature.LoseAllStuckObjects();
						room.RemoveObject(abstractCreature.realizedCreature);
						abstractCreature.Abstractize(abstractCreature.pos);
						abstractCreature.Move(newCoord);
						break;
					}
				}
			}
			if (fadeOut == null || !fadeOut.IsDoneFading() || triggered)
			{
				return;
			}
			afterFadeTime += 1f;
			if (!(afterFadeTime > 120f))
			{
				return;
			}
			triggered = true;
			if (room.world.rainCycle.cycleLength - room.world.rainCycle.timer < 4800)
			{
				room.world.game.globalRain.ResetRain();
				room.world.rainCycle.timer = room.world.rainCycle.cycleLength - 4800;
			}
			for (int k = 0; k < room.abstractRoom.entities.Count; k++)
			{
				if (!(room.abstractRoom.entities[k] is AbstractCreature))
				{
					continue;
				}
				AbstractCreature abstractCreature2 = (room.abstractRoom.entities[k] as AbstractPhysicalObject) as AbstractCreature;
				if (firstAlivePlayer != null && abstractCreature2.realizedCreature != null)
				{
					if (abstractCreature2.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && abstractCreature2.state.socialMemory.GetLike(firstAlivePlayer.ID) > 0.5f)
					{
						abstractCreature2.LoseAllStuckObjects();
						room.RemoveObject(abstractCreature2.realizedCreature);
						abstractCreature2.Abstractize(abstractCreature2.pos);
						abstractCreature2.Move(newCoord);
						break;
					}
					if (abstractCreature2.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && abstractCreature2.state.alive && abstractCreature2.state.socialMemory.GetLike(firstAlivePlayer.ID) > 0.5f)
					{
						abstractCreature2.LoseAllStuckObjects();
						room.RemoveObject(abstractCreature2.realizedCreature);
						abstractCreature2.Abstractize(abstractCreature2.pos);
						abstractCreature2.Move(newCoord);
						break;
					}
				}
			}
			foreach (Player item2 in room.PlayersInRoom)
			{
				RoomWarp(item2, room, "OE_PUMP01");
			}
		}
	}

	public class SU_PMPSTATION01_safety : UpdatableAndDeletable
	{
		private Player player;

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (!ModManager.CoopAvailable)
			{
				if (this.player == null && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
				{
					this.player = firstAlivePlayer.realizedCreature as Player;
				}
				if (this.player != null)
				{
					if (this.player.room != room)
					{
						Destroy();
					}
					if (this.player.mainBodyChunk.pos.y > 1422f)
					{
						Destroy();
					}
					this.player.airInLungs = 1f;
				}
				return;
			}
			bool flag = false;
			foreach (AbstractCreature nonPermaDeadPlayer in room.game.NonPermaDeadPlayers)
			{
				if (this.player == null && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
				{
					this.player = firstAlivePlayer.realizedCreature as Player;
				}
				if (nonPermaDeadPlayer.realizedCreature != null)
				{
					Player player = (Player)nonPermaDeadPlayer.realizedCreature;
					if (player.room == room && player.mainBodyChunk.pos.y < 1422f)
					{
						flag = true;
						player.airInLungs = 1f;
					}
				}
			}
			if (!flag)
			{
				Destroy();
			}
		}
	}

	public class randomGodsSoundSource : UpdatableAndDeletable
	{
		private float MaxVol;

		private Vector2 Pos;

		private Room Room;

		private SSMusicTrigger foundMusic;

		private float Range;

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature followAbstractCreature = room.game.cameras[0].followAbstractCreature;
			if (followAbstractCreature != null && followAbstractCreature.Room == room.abstractRoom && foundMusic != null && followAbstractCreature.realizedCreature != null)
			{
				float val = Vector2.Distance(followAbstractCreature.realizedCreature.firstChunk.pos, Pos);
				foundMusic.effect.amount = Custom.LerpMap(val, Range, 0f, 0f, MaxVol);
			}
		}

		public randomGodsSoundSource(float maxvol, Vector2 pos, float range, Room room)
		{
			MaxVol = maxvol;
			Pos = pos;
			Room = room;
			Range = range;
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is SSMusicTrigger)
				{
					foundMusic = room.updateList[i] as SSMusicTrigger;
					break;
				}
			}
		}
	}

	public class DM_ROOF03GradientGravity : UpdatableAndDeletable
	{
		public DM_ROOF03GradientGravity(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
			{
				float value = Vector2.Distance((firstAlivePlayer.realizedCreature as Player).firstChunk.pos, new Vector2(3666f, 0f));
				room.gravity = Mathf.InverseLerp(0f, 2770f, value);
			}
		}
	}

	public class DS_RIVSTARTcutscene : UpdatableAndDeletable
	{
		private int timer;

		public DS_RIVSTARTcutscene(Room room)
		{
			timer = 0;
			base.room = room;
			room.world.ToggleCreatureAccessFromCutscene("DS_RIVSTART", CreatureTemplate.Type.Scavenger, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_RIVSTART", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER03", CreatureTemplate.Type.Scavenger, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER03", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER04", CreatureTemplate.Type.Scavenger, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER04", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER05", CreatureTemplate.Type.Scavenger, allowAccess: false);
			room.world.ToggleCreatureAccessFromCutscene("DS_GUTTER05", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, allowAccess: false);
		}

		public override void Update(bool eu)
		{
			int num = 300;
			base.Update(eu);
			if (timer == num + 10)
			{
				room.PlaySound(SoundID.Fire_Spear_Explode, new Vector2(490f, 3300f));
			}
			if (timer == num + 20)
			{
				room.PlaySound(SoundID.Fire_Spear_Explode, new Vector2(490f, 3400f));
			}
			if (timer == num + 120)
			{
				room.PlaySound(SoundID.Fire_Spear_Explode, new Vector2(490f, 2900f));
				for (int i = 0; i < 2; i++)
				{
					room.AddObject(new ExplosiveSpear.SpearFragment(new Vector2(506f, 1900f), Custom.RNV() * Mathf.Lerp(20f, 40f, UnityEngine.Random.value)));
				}
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.ToWorldCoordinate(new Vector2(526f, 1000f)), room.world.game.GetNewID());
				abstractCreature.Die();
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
			}
			if (timer == num + 180)
			{
				room.PlaySound(SoundID.Fire_Spear_Explode, new Vector2(506f, 2100f));
				room.AddObject(new Explosion(room, null, new Vector2(400f, 1000f), 10, 200f, 1f, 0f, 0f, 0.1f, null, 0f, 0f, 1f));
				for (int j = 0; j < 2; j++)
				{
					room.AddObject(new ExplosiveSpear.SpearFragment(new Vector2(506f, 1800f), Custom.RNV() * Mathf.Lerp(20f, 40f, UnityEngine.Random.value)));
				}
			}
			if (timer == num + 170 || timer == num + 180 || timer == num + 190)
			{
				AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(room.world, null, room.ToWorldCoordinate(new Vector2(506f, 1000f)), room.world.game.GetNewID(), explosive: false);
				room.abstractRoom.AddEntity(abstractPhysicalObject);
				abstractPhysicalObject.RealizeInRoom();
				if (abstractPhysicalObject.realizedObject != null)
				{
					(abstractPhysicalObject.realizedObject as Spear).firstChunk.vel = new Vector2(UnityEngine.Random.Range(-10, 10), -20f);
					(abstractPhysicalObject.realizedObject as Spear).mode = Weapon.Mode.Free;
					(abstractPhysicalObject.realizedObject as Spear).SetRandomSpin();
				}
			}
			if (timer >= num + 250)
			{
				Destroy();
				return;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.session is StoryGameSession && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && room.game.GetStorySession.saveState.cycleNumber == 0)
			{
				Player obj = firstAlivePlayer.realizedCreature as Player;
				obj.objectInStomach = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, MoreSlugcatsEnums.DataPearlType.Rivulet_stomach);
				obj.SuperHardSetPosition(new Vector2(506f, 1000f));
				obj.mainBodyChunk.vel = new Vector2(0f, -10f);
				obj.Stun(100);
			}
			timer++;
		}
	}

	public class SI_SAINTINTRO_tut : UpdatableAndDeletable
	{
		private int counter;

		public bool shownAttachedMessage;

		public int awakeCounter;

		private bool setStomach;

		public override void Update(bool eu)
		{
			if (room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.subregionTracker != null)
			{
				room.game.cameras[0].hud.textPrompt.subregionTracker.showCycleNumber = false;
				room.game.cameras[0].hud.textPrompt.subregionTracker.lastRegion = 1;
				room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
			}
			base.Update(eu);
			bool flag = false;
			bool flag2 = false;
			if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.realizedCreature != null)
			{
				Player player = room.game.cameras[0].followAbstractCreature.realizedCreature as Player;
				if (!setStomach)
				{
					string saintStomachRolloverObject = room.game.rainWorld.progression.miscProgressionData.saintStomachRolloverObject;
					if (saintStomachRolloverObject.Contains("<oA>"))
					{
						player.objectInStomach = SaveState.AbstractPhysicalObjectFromString(room.world, saintStomachRolloverObject);
					}
					else if (saintStomachRolloverObject.Contains("<cA>"))
					{
						player.objectInStomach = SaveState.AbstractCreatureFromString(room.world, saintStomachRolloverObject, onlyInCurrentRegion: false);
					}
					if (player.objectInStomach != null)
					{
						player.objectInStomach.pos = player.abstractCreature.pos;
					}
					setStomach = true;
				}
				if (counter < 200)
				{
					player.Hypothermia = 0f;
					if (counter < 40)
					{
						if (player.sleepCounter < 10)
						{
							player.sleepCounter = 99;
							player.SuperHardSetPosition(new Vector2(360f, 90f));
							player.bodyChunks[1].pos.x = player.bodyChunks[0].pos.x + 3f;
							player.bodyChunks[1].pos.y = player.bodyChunks[0].pos.y;
							player.bodyChunks[0].vel *= 0f;
							player.bodyChunks[1].vel *= 0f;
						}
					}
					else
					{
						player.sleepCounter = 99;
					}
					room.world.rainCycle.timer = room.world.rainCycle.cycleLength + 200;
				}
				if (player.sleepCounter != 0)
				{
					player.standing = false;
					player.flipDirection = 1;
					player.touchedNoInputCounter = 10;
					player.sleepCurlUp = 1f;
					if (player.tongue != null && player.tongue.Attached)
					{
						player.tongue.Release();
					}
				}
				flag = player.tongue.Attached;
				if (player.mainBodyChunk.pos.y < 200f)
				{
					flag2 = true;
				}
				if (!player.Sleeping)
				{
					awakeCounter++;
				}
			}
			if (awakeCounter == 1200 && room.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap < 9 && !shownAttachedMessage && flag2)
			{
				if (MMF.cfgOldTongue.Value)
				{
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Tap the throw button to extend your tongue. Press jump while attached to let go."), 140, 500, darken: true, hideHud: true);
				}
				else
				{
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Tap jump in the air to extend your tongue. Press jump again while attached to let go."), 140, 500, darken: true, hideHud: true);
				}
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("While you are attached to something, use up and down to extend or retract your tongue."), 20, 500, darken: true, hideHud: true);
				shownAttachedMessage = true;
			}
			if (flag && !shownAttachedMessage)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("While you are attached to something, use up and down to extend or retract your tongue."), 20, 500, darken: true, hideHud: true);
				shownAttachedMessage = true;
			}
			if (room.game.cameras[0].followAbstractCreature != null)
			{
				if (room.game.cameras[0].followAbstractCreature.realizedCreature != null)
				{
					Player player2 = room.game.cameras[0].followAbstractCreature.realizedCreature as Player;
					if ((double)player2.Hypothermia > 0.15)
					{
						player2.Hypothermia = 0.15f;
					}
				}
				if (room.game.cameras[0].followAbstractCreature.Room != room.abstractRoom && room.game.cameras[0].followAbstractCreature.Room.realizedRoom != null && room.game.cameras[0].followAbstractCreature.Room.realizedRoom.readyForAI)
				{
					if (room.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap < 9)
					{
						room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("You are cold and hungry. Find shelter from the storm soon, or you will pass out from the cold."), 20, 300, darken: true, hideHud: true);
						room.game.cameras[0].hud.HypoMeter.ShowTutorial();
					}
					AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.game.cameras[0].followAbstractCreature.Room.realizedRoom.ToWorldCoordinate(new Vector2(1789f, 169f)), room.world.game.GetNewID());
					abstractCreature.Die();
					abstractCreature.Hypothermia = 3f;
					room.game.cameras[0].followAbstractCreature.Room.AddEntity(abstractCreature);
					abstractCreature.RealizeInRoom();
					AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, room.game.cameras[0].followAbstractCreature.Room.realizedRoom.ToWorldCoordinate(new Vector2(1789f, 169f)), room.world.game.GetNewID());
					room.game.cameras[0].followAbstractCreature.Room.AddEntity(abstractPhysicalObject);
					abstractPhysicalObject.RealizeInRoom();
					Destroy();
					{
						foreach (PhysicalObject item in room.game.cameras[0].followAbstractCreature.Room.realizedRoom.physicalObjects[1])
						{
							if (item is SeedCob && (item as SeedCob).open == 0f)
							{
								Custom.Log("Cob tutorial triggered");
								(item as SeedCob).AllPlantsFrozenCycleTime = room.world.rainCycle.cycleLength + 200 + UnityEngine.Random.Range(0, 100);
								break;
							}
						}
						return;
					}
				}
				room.world.rainCycle.timer = room.world.rainCycle.cycleLength + 20;
			}
			counter++;
		}

		public SI_SAINTINTRO_tut(Room room)
		{
			base.room = room;
			setStomach = false;
		}
	}

	public class SI_C02_tut : UpdatableAndDeletable
	{
		private int counter;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (counter == 1)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Only warm objects can rejuvenate your body heat in this extreme cold, but they will not last forever!"), 20, 500, darken: true, hideHud: true);
				Destroy();
			}
			if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.Room == room.abstractRoom && room.game.cameras[0].followAbstractCreature.realizedCreature != null && room.game.cameras[0].followAbstractCreature.realizedCreature.mainBodyChunk.pos.x > 1200f)
			{
				counter++;
			}
		}

		public SI_C02_tut(Room room)
		{
			base.room = room;
		}
	}

	public class SH_GOR02 : UpdatableAndDeletable
	{
		private bool pushRight;

		private int textStage;

		private bool stoodUp;

		private float rightMost;

		private bool firstStart;

		private bool spawnedScribble;

		public ScribbleDreamScreen scribble;

		public SH_GOR02(Room room)
		{
			base.room = room;
			textStage = -1;
			stoodUp = false;
			firstStart = true;
		}

		public override void PausedUpdate()
		{
			base.PausedUpdate();
			if (room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return;
			}
			if (scribble == null && room != null && !spawnedScribble)
			{
				scribble = new ScribbleDreamScreen(room.game.manager);
				scribble.GetDataFromGame(MoreSlugcatsEnums.DreamID.Gourmand0, null);
				scribble.SetupFadeOut(room, 40, 80, 220, 160);
				room.game.grafUpdateMenus.Add(scribble);
				spawnedScribble = true;
				Vector2[] array = new Vector2[2]
				{
					new Vector2(170f, 250f),
					new Vector2(270f, 310f)
				};
				for (int i = 0; i < array.Length; i++)
				{
					WorldCoordinate worldCoordinate = room.GetWorldCoordinate(array[i]);
					AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, worldCoordinate, room.game.GetNewID(), -1, -1, null);
					abstractConsumable.RealizeInRoom();
					(abstractConsumable.realizedObject as SlimeMold).big = true;
				}
			}
			if (scribble != null)
			{
				scribble.Update();
			}
			if (scribble != null && scribble.slatedForDeletetion)
			{
				scribble.ShutDownProcess();
				room.game.grafUpdateMenus.Remove(scribble);
				scribble = null;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return;
			}
			if (textStage == 0)
			{
				Custom.Log("Begin Gourmand Text");
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold up and grab with differing objects in each hand, in order to procure something new."), 40, 540, darken: true, hideHud: true);
				textStage = 1;
			}
			if (textStage >= 1 && room.game.cameras[0].room != room)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Holding up and grab with two differing food items, will eat them together as a meal."), 100, 380, darken: true, hideHud: true);
				Destroy();
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count <= 0 || firstAlivePlayer == null || firstAlivePlayer.realizedCreature == null || firstAlivePlayer.realizedCreature.room != room)
			{
				return;
			}
			if (firstStart && scribble != null)
			{
				Vector2 vector = new Vector2(450f, 170f);
				firstAlivePlayer.realizedCreature.bodyChunks[0].HardSetPosition(vector + new Vector2(9f, 0f));
				firstAlivePlayer.realizedCreature.bodyChunks[1].HardSetPosition(vector + new Vector2(-5f, 0f));
				WorldCoordinate worldCoordinate = room.GetWorldCoordinate(vector);
				AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, worldCoordinate, room.game.GetNewID(), -1, -1, null);
				abstractConsumable.RealizeInRoom();
				(abstractConsumable.realizedObject as SlimeMold).big = true;
				(firstAlivePlayer.realizedCreature as Player).SlugcatGrab(abstractConsumable.realizedObject, 1);
				firstStart = false;
			}
			if (textStage < 0)
			{
				(firstAlivePlayer.realizedCreature as Player).craftingTutorial = false;
				if (firstAlivePlayer.realizedCreature.firstChunk.pos.x > 1210f)
				{
					textStage = 0;
				}
			}
			if (textStage == 1 && (firstAlivePlayer.realizedCreature as Player).craftingTutorial)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("You can also spit up random objects at the cost of food, if nothing is stored inside of you."), 20, 540, darken: true, hideHud: true);
				textStage = 2;
			}
			if (scribble != null && !scribble.slatedForDeletetion)
			{
				(firstAlivePlayer.realizedCreature as Player).eatCounter = 40;
			}
			if (firstAlivePlayer.realizedCreature.firstChunk.pos.y < 170f && !stoodUp)
			{
				(firstAlivePlayer.realizedCreature as Player).standing = true;
			}
			if ((firstAlivePlayer.realizedCreature as Player).standing)
			{
				stoodUp = true;
			}
		}
	}

	public class MS_CORESTARTUPHEART : UpdatableAndDeletable
	{
		private bool primed;

		private EnergyCell foundCell;

		private bool finalPhase;

		private bool lethalMode;

		private int noCellPresenceTime;

		public MS_CORESTARTUPHEART(Room room)
		{
			base.room = room;
			primed = false;
			IntVector2 pos = new IntVector2(23, 18);
			if (base.room.game.session is StoryGameSession && base.room.game.IsMoonHeartActive())
			{
				AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(base.room.world, MoreSlugcatsEnums.AbstractObjectType.EnergyCell, null, base.room.GetWorldCoordinate(pos), base.room.game.GetNewID())
				{
					destroyOnAbstraction = true
				};
				base.room.abstractRoom.AddEntity(abstractPhysicalObject);
				abstractPhysicalObject.RealizeInRoom();
				foundCell = abstractPhysicalObject.realizedObject as EnergyCell;
				foundCell.firstChunk.pos = base.room.MiddleOfTile(pos);
				foundCell.customAnimation = true;
				foundCell.SetLocalGravity(0f);
				foundCell.canBeHitByWeapons = false;
				foundCell.FXCounter = 10000f;
				RoomSettings.RoomEffect item = new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.SilenceWater, 1f, inherited: false);
				base.room.roomSettings.effects.Add(item);
				item = new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.ZeroG, 1f, inherited: false);
				base.room.roomSettings.effects.Add(item);
				lethalMode = true;
				return;
			}
			for (int i = 0; i < base.room.lightningMachines.Count; i++)
			{
				base.room.lightningMachines[i].intensity = 0f;
				base.room.lightningMachines[i].chance = 0f;
			}
			for (int j = 0; j < base.room.energySwirls.Count; j++)
			{
				base.room.energySwirls[j].Destroy();
			}
			for (int k = 0; k < base.room.roomSettings.ambientSounds.Count; k++)
			{
				AmbientSound ambientSound = base.room.roomSettings.ambientSounds[k];
				if (ambientSound.sample == "MSC-FlangePad.ogg" || ambientSound.sample == "SO_SFX-AlphaWaves.ogg")
				{
					ambientSound.volume = 0f;
				}
			}
			if (base.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
			{
				Destroy();
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (lethalMode)
			{
				Vector2 vector = room.MiddleOfTile(new IntVector2(23, 18));
				if (foundCell != null)
				{
					foundCell.customAnimation = true;
					foundCell.moveToTarget = 0.9f;
					foundCell.scale = 20f;
					foundCell.firstChunk.pos = vector;
					foundCell.firstChunk.vel = Vector2.zero;
				}
				{
					foreach (AbstractCreature creature in room.abstractRoom.creatures)
					{
						if (creature.creatureTemplate.type != CreatureTemplate.Type.Overseer && creature.realizedCreature != null && !creature.state.dead && Vector2.Distance(creature.realizedCreature.DangerPos, vector) < 100f)
						{
							room.AddObject(new ShockWave(creature.realizedCreature.DangerPos, 150f, 0.4f, 30));
							for (int i = 0; i < 30; i++)
							{
								room.AddObject(new Spark(creature.realizedCreature.DangerPos + Custom.RNV() * UnityEngine.Random.value * 40f, Custom.RNV() * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
							}
							room.AddObject(new ElectricDeath.SparkFlash(creature.realizedCreature.DangerPos, 10.5f));
							room.PlaySound(SoundID.Zapper_Zap, 0f, 0.8f, 0.5f);
							creature.realizedCreature.Die();
							creature.realizedCreature.Destroy();
						}
					}
					return;
				}
			}
			if (finalPhase)
			{
				if (room.world.rainCycle.timer >= room.world.rainCycle.cycleLength && room.game.globalRain.flood < room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y)
				{
					room.game.globalRain.forceSlowFlood = true;
					room.game.globalRain.flood = room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y;
				}
				if (room.world.rainCycle.timer % 4 != 0)
				{
					room.world.rainCycle.timer++;
				}
				room.world.rainCycle.brokenAntiGrav.on = false;
				Vector2 vector2 = room.MiddleOfTile(new IntVector2(23, 18));
				for (int j = 0; j < room.game.Players.Count; j++)
				{
					if (room.game.Players[j].realizedCreature != null && room.game.Players[j].realizedCreature.Submersion < 0.8f)
					{
						float num = Mathf.InverseLerp(0f, 10f, room.game.Players[j].realizedCreature.firstChunk.vel.magnitude) * 3f;
						if (num < 0.3f)
						{
							num = 0.3f;
						}
						(room.game.Players[j].realizedCreature as Player).airInLungs = 1f;
						room.game.Players[j].realizedCreature.firstChunk.vel += Custom.DirVec(vector2, room.game.Players[j].realizedCreature.firstChunk.pos) * Custom.LerpMap(Vector2.Distance(vector2, room.game.Players[j].realizedCreature.firstChunk.pos), 95f, 100f, num, 0f - num);
						room.game.Players[j].realizedCreature.firstChunk.vel += Custom.PerpendicularVector(Custom.DirVec(room.game.Players[j].realizedCreature.firstChunk.pos, vector2)) / 20f;
					}
				}
				if (room.game.cameras[0].room != room)
				{
					Destroy();
				}
			}
			else if (!primed)
			{
				bool flag = false;
				for (int k = 0; k < room.game.Players.Count; k++)
				{
					if (room.game.Players[k].Room == room.abstractRoom)
					{
						flag = true;
					}
				}
				if (foundCell != null)
				{
					while (foundCell.grabbedBy.Count > 0)
					{
						Creature grabber = foundCell.grabbedBy[0].grabber;
						foundCell.grabbedBy[0].Release();
						grabber.Stun(10);
						grabber.firstChunk.vel += new Vector2(0f, -5f);
					}
					foundCell.KeepOff();
					primed = true;
				}
				else
				{
					if (!flag)
					{
						return;
					}
					if (noCellPresenceTime >= 20 && room.world.rainCycle.TimeUntilRain > 600)
					{
						room.world.rainCycle.timer = room.world.rainCycle.cycleLength - 600;
					}
					using List<UpdatableAndDeletable>.Enumerator enumerator2 = room.updateList.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						UpdatableAndDeletable current2 = enumerator2.Current;
						if (current2 is EnergyCell)
						{
							foundCell = current2 as EnergyCell;
							break;
						}
					}
					if (foundCell == null)
					{
						noCellPresenceTime++;
					}
					else
					{
						noCellPresenceTime = 0;
					}
				}
			}
			else if (primed && foundCell.room == room)
			{
				if (foundCell.usingTime > 0f)
				{
					foundCell.KeepOff();
					foundCell.FireUp(room.MiddleOfTile(new IntVector2(23, 18)));
					foundCell.AllGraspsLetGoOfThisObject(evenNonExlusive: true);
					(room.game.session as StoryGameSession).RemovePersistentTracker(foundCell.abstractPhysicalObject);
					foundCell.canBeHitByWeapons = false;
					room.game.cameras[0].hud.rainMeter.SuppressHalfTime();
					room.world.rainCycle.preTimer = 0;
					room.world.rainCycle.timer = room.world.rainCycle.cycleLength - 2500;
					room.game.GetStorySession.saveState.miscWorldSaveData.moonHeartRestored = true;
					finalPhase = true;
					RainWorldGame.ForceSaveNewDenLocation(room.game, "MS_bitterstart", saveWorldStates: true);
				}
			}
			else if (primed && foundCell.room != room)
			{
				primed = false;
				foundCell = null;
			}
		}
	}

	public class MS_HEARTWARP : UpdatableAndDeletable
	{
		private Player player;

		private bool triggered;

		public FadeOut fadeOut;

		public float afterFadeTime;

		public MS_HEARTWARP(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (!room.game.IsMoonHeartActive())
			{
				foreach (EnergySwirl energySwirl in room.energySwirls)
				{
					energySwirl.Depth = 1f;
				}
				if (room.world.rainCycle.TimeUntilRain < 400)
				{
					if (player == null && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
					{
						player = firstAlivePlayer.realizedCreature as Player;
					}
					if (player != null && !player.dead)
					{
						room.AddObject(new ElectricDeath.SparkFlash(player.mainBodyChunk.pos, 1f));
						room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, player.mainBodyChunk.pos, 1f, 1f);
						player.Die();
					}
				}
				return;
			}
			foreach (EnergySwirl energySwirl2 in room.energySwirls)
			{
				energySwirl2.Depth = 0.054f;
			}
			if (player == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
			{
				player = firstAlivePlayer.realizedCreature as Player;
			}
			if (player != null && player.mainBodyChunk.pos.y > 3780f)
			{
				player.mainBodyChunk.pos.y = 3880f;
				player.mainBodyChunk.vel.y = 0f;
				if (fadeOut == null)
				{
					fadeOut = new FadeOut(room, Color.black, 60f, fadeIn: false);
					room.AddObject(fadeOut);
				}
			}
			if (fadeOut == null || !fadeOut.IsDoneFading() || triggered)
			{
				return;
			}
			afterFadeTime += 1f;
			if (afterFadeTime > 120f)
			{
				triggered = true;
				if (room.world.rainCycle.cycleLength - room.world.rainCycle.timer < 4800)
				{
					room.world.game.globalRain.ResetRain();
					room.world.rainCycle.timer = room.world.rainCycle.cycleLength - 4800;
				}
				RoomWarp(player, room, "MS_bitterstart");
			}
		}
	}

	public class MS_bitterstart : UpdatableAndDeletable
	{
		private int waitCounter;

		private FadeOut fadeIn;

		public MS_bitterstart(Room room)
		{
			base.room = room;
			waitCounter = 400;
			Custom.Log("bitter start!");
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.cameras[0].room != room)
			{
				return;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			Player player = ((room.game.Players.Count > 0 && firstAlivePlayer != null) ? (firstAlivePlayer.realizedCreature as Player) : null);
			if (player == null)
			{
				return;
			}
			player.allowOutOfBounds = true;
			if (waitCounter > 0)
			{
				if (room.game.Players.Count <= 0 || player == null || player.inShortcut)
				{
					return;
				}
				waitCounter--;
				if (player.firstChunk.pos.x > 700f)
				{
					waitCounter = 0;
					return;
				}
				player.touchedNoInputCounter = -300;
				if (room.game.cameras[0].currentCameraPosition == 0 && fadeIn == null)
				{
					fadeIn = new FadeOut(room, Color.black, 320f, fadeIn: true);
					room.AddObject(fadeIn);
					player.firstChunk.pos = new Vector2(-500f, 154f);
					player.Stun(260);
					player.airInLungs = 1f;
				}
			}
			else
			{
				if (player.firstChunk.pos.x < 600f)
				{
					player.firstChunk.vel += new Vector2(0f, player.Submersion / 10f);
				}
				if (player.firstChunk.pos.x < 340f)
				{
					player.firstChunk.vel = new Vector2(13f, 0f);
				}
			}
		}
	}

	public class SI_SAINTENDING : UpdatableAndDeletable
	{
		private AboveCloudsView cloudView;

		private int counter;

		private bool fadeOne;

		private FadeOut currentFade;

		private AbstractCreature abstractPlayer;

		private bool readyLoad;

		private bool playerSet;

		private bool ghostFlash;

		private bool slugFlicker;

		private FadeOut finalEndingFade;

		public SI_SAINTENDING(Room room)
		{
			Custom.Log("Saint ending controller");
			base.room = room;
			base.room.AddObject(new FadeOut(base.room, Color.white, 300f, fadeIn: true));
			base.room.abstractRoom.mapPos = new Vector2(153.5f, 964.7f);
			abstractPlayer = base.room.game.cameras[0].followAbstractCreature;
			base.room.game.cameras[0].followAbstractCreature = null;
			base.room.game.cameras[0].voidSeaMode = false;
			base.room.game.cameras[0].offset = default(Vector2);
			base.room.game.cameras[0].hardLevelGfxOffset = default(Vector2);
			base.room.game.cameras[0].MoveCamera(0);
			if (base.room.game.manager.musicPlayer != null)
			{
				base.room.game.manager.musicPlayer.RainRequestStopSong();
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.fullyLoaded && !playerSet)
			{
				if (abstractPlayer.realizedCreature == null)
				{
					abstractPlayer.RealizeInRoom();
				}
				if (abstractPlayer.realizedCreature != null && abstractPlayer.realizedCreature.graphicsModule != null)
				{
					playerSet = true;
					abstractPlayer.pos = new WorldCoordinate(room.abstractRoom.index, 18, 4, -1);
					(abstractPlayer.realizedCreature as Player).SuperHardSetPosition(new Vector2(360f, 100f));
					BodyChunk firstChunk = (abstractPlayer.realizedCreature as Player).firstChunk;
					firstChunk.pos.x = firstChunk.pos.x + 1f;
					((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).tentaclesVisible = 0;
					((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).tentacles[0].SetPosition(new Vector2(0f, -600f));
					((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).tentacles[1].SetPosition(new Vector2(0f, -600f));
					((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).tentacles[2].SetPosition(new Vector2(0f, -600f));
					((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).tentacles[3].SetPosition(new Vector2(0f, -600f));
				}
			}
			room.world.rainCycle.timer = 0;
			if (cloudView == null)
			{
				foreach (UpdatableAndDeletable update in room.updateList)
				{
					if (update is AboveCloudsView)
					{
						cloudView = update as AboveCloudsView;
						cloudView.animateSMEndingScroll = true;
						cloudView.yShift = 31000f;
						break;
					}
				}
				return;
			}
			if (counter > 200)
			{
				cloudView.yShift -= 29f * Mathf.InverseLerp(200f, 2000f, counter);
				cloudView.yShift += 5f * Mathf.InverseLerp(5000f, 10000f, counter);
			}
			if (counter < 1000 && room.game.manager.musicPlayer != null)
			{
				room.game.manager.musicPlayer.RequestSaintEndingSong();
			}
			if (cloudView.yShift < 10000f)
			{
				room.world.rainCycle.timer = (int)((float)room.world.rainCycle.cycleLength * Mathf.InverseLerp(21000f, 3000f, cloudView.yShift));
			}
			if (room.game.manager.musicPlayer != null && room.game.manager.musicPlayer.song is SaintEndingSong)
			{
				(room.game.manager.musicPlayer.song as SaintEndingSong).setVolume = 0.35f;
			}
			if (cloudView.yShift <= 8000f && !fadeOne)
			{
				fadeOne = true;
				currentFade = new FadeOut(room, Color.white, 265f, fadeIn: false);
				room.AddObject(currentFade);
			}
			if (cloudView.yShift <= 0f && fadeOne)
			{
				cloudView.yShift = 0f;
				if (currentFade.fade == 1f)
				{
					currentFade.Destroy();
					currentFade = new FadeOut(room, Color.white, 400f, fadeIn: true);
					room.AddObject(currentFade);
					room.game.cameras[0].MoveCamera(2);
				}
			}
			abstractPlayer.Hypothermia = 0f;
			if (!ghostFlash && playerSet)
			{
				(abstractPlayer.realizedCreature as Player).sleepCounter = 99;
				(abstractPlayer.realizedCreature as Player).sleepCurlUp = 1f;
				(abstractPlayer.realizedCreature as Player).flipDirection = 1;
				(abstractPlayer.realizedCreature as Player).standing = false;
				((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).darkenFactor = Mathf.InverseLerp(2600f, 2400f, counter);
				if (((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).darkenFactor < 0.9f)
				{
					room.game.manager.CueAchievement(RainWorld.AchievementID.SaintEnding, 5f);
					ghostFlash = true;
					room.AddObject(new GhostHunch(room, null));
				}
			}
			else
			{
				if (((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).darkenFactor < 0.22f && !slugFlicker)
				{
					room.AddObject(new SlugcatGhost((abstractPlayer.realizedCreature as Player).firstChunk.pos, room));
					slugFlicker = true;
				}
				(abstractPlayer.realizedCreature as Player).sleepCounter = 99;
				(abstractPlayer.realizedCreature as Player).sleepCurlUp = 1f;
				((abstractPlayer.realizedCreature as Player).graphicsModule as PlayerGraphics).darkenFactor = Mathf.InverseLerp(2600f, 2400f, counter);
				if (finalEndingFade != null)
				{
					room.game.cameras[0].virtualMicrophone.globalSoundMuffle = finalEndingFade.fade;
					if (finalEndingFade.IsDoneFading())
					{
						room.game.GetStorySession.saveState.deathPersistentSaveData.karma = 1;
						room.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap = 1;
						room.game.manager.statsAfterCredits = true;
						room.game.manager.desiredCreditsSong = "BLIZZARD";
						room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
						finalEndingFade = null;
					}
				}
				else if (finalEndingFade == null && (float)counter > 2700f)
				{
					Custom.Log("FINAL FADE!");
					finalEndingFade = new FadeOut(room, Color.black, 250f, fadeIn: false);
					room.AddObject(finalEndingFade);
				}
			}
			counter++;
		}
	}

	public class MS_COMMS_RivEnding : UpdatableAndDeletable, IRunDuringDialog
	{
		private int timer;

		private FadeOut fadeIn;

		private FadeOut fadeOut;

		public bool cameraInit;

		public bool warpExecuted;

		public MS_COMMS_RivEnding(Room room)
		{
			base.room = room;
			fadeIn = new FadeOut(base.room, Color.black, 200f, fadeIn: true);
			base.room.AddObject(fadeIn);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = null;
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				player = firstAlivePlayer.realizedCreature as Player;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].Room.name == room.abstractRoom.name)
					{
						Player player2 = room.game.Players[i].realizedCreature as Player;
						player2.LoseAllGrasps();
						player2.SuperHardSetPosition(new Vector2(-500f, 2000f));
						player2.Stun(40);
						for (int j = 0; j < player2.bodyChunks.Length; j++)
						{
							player2.bodyChunks[j].vel.y = Mathf.Clamp(player2.bodyChunks[j].vel.y, -5f, 5f);
						}
					}
				}
				HardSetStunAllObjects(room, new Vector2(-500f, 3000f));
			}
			if (fadeIn != null && room.game.cameras[0].currentCameraPosition != 1)
			{
				fadeIn.fade = 1f;
			}
			if (fadeIn != null && fadeIn.IsDoneFading() && player != null)
			{
				timer++;
				if (timer == 1)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.95f);
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.98f);
				}
				if (timer == 120)
				{
					player.InitChatLog(ChatlogData.ChatlogID.Chatlog_LM7);
				}
				if (timer > 120 && !player.chatlog && fadeOut == null)
				{
					fadeOut = new FadeOut(room, Color.black, 120f, fadeIn: false);
					room.AddObject(fadeOut);
				}
			}
			if (room.fullyLoaded && !cameraInit)
			{
				room.game.cameras[0].followAbstractCreature = null;
				room.game.cameras[0].MoveCamera(1);
				cameraInit = true;
			}
			if (fadeOut != null && fadeOut.IsDoneFading() && !warpExecuted)
			{
				room.game.overWorld.InitiateSpecialWarp_SingleRoom(null, "GW_E02");
				if (room.blizzardGraphics != null)
				{
					room.RemoveObject(room.blizzardGraphics);
					room.blizzardGraphics.Destroy();
					room.blizzardGraphics = null;
					room.blizzard = false;
				}
				warpExecuted = true;
			}
		}
	}

	public class GW_E02_RivEnding : UpdatableAndDeletable, IRunDuringDialog
	{
		private int timer;

		private FadeOut fadeIn;

		private FadeOut fadeOut;

		public bool cameraInit;

		public bool warpExecuted;

		public GW_E02_RivEnding(Room room)
		{
			base.room = room;
			fadeIn = new FadeOut(base.room, Color.black, 120f, fadeIn: true);
			base.room.AddObject(fadeIn);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = null;
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				player = firstAlivePlayer.realizedCreature as Player;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].Room.name == room.abstractRoom.name)
					{
						Player player2 = room.game.Players[i].realizedCreature as Player;
						player2.SuperHardSetPosition(new Vector2(2600f, 1200f));
						player2.Stun(40);
						for (int j = 0; j < player2.bodyChunks.Length; j++)
						{
							player2.bodyChunks[j].vel.y = Mathf.Clamp(player2.bodyChunks[j].vel.y, -5f, 5f);
						}
					}
				}
				HardSetStunAllObjects(room, new Vector2(2600f, 2000f));
			}
			if (fadeIn != null && room.game.cameras[0].currentCameraPosition != 2)
			{
				fadeIn.fade = 1f;
			}
			if (fadeIn != null && fadeIn.IsDoneFading() && player != null)
			{
				timer++;
				if (timer == 1)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.95f);
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.98f);
					player.InitChatLog(ChatlogData.ChatlogID.Chatlog_LM8);
				}
				if (timer > 1 && !player.chatlog && fadeOut == null)
				{
					fadeOut = new FadeOut(room, Color.black, 120f, fadeIn: false);
					room.AddObject(fadeOut);
				}
			}
			if (room.fullyLoaded && !cameraInit)
			{
				room.game.cameras[0].followAbstractCreature = null;
				room.game.cameras[0].MoveCamera(2);
				cameraInit = true;
			}
			if (fadeOut != null && fadeOut.IsDoneFading() && !warpExecuted)
			{
				room.game.overWorld.InitiateSpecialWarp_SingleRoom(null, "SI_A07");
				warpExecuted = true;
			}
		}
	}

	public class SI_A07_RivEnding : UpdatableAndDeletable, IRunDuringDialog
	{
		private int timer;

		private FadeOut fadeIn;

		private FadeOut fadeOut;

		private bool warpExecuted;

		public SI_A07_RivEnding(Room room)
		{
			base.room = room;
			base.room.abstractRoom.mapPos = new Vector2(335f, 1800f);
			fadeIn = new FadeOut(base.room, Color.black, 120f, fadeIn: true);
			base.room.AddObject(fadeIn);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			Player player = null;
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				player = firstAlivePlayer.realizedCreature as Player;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].Room.name == room.abstractRoom.name)
					{
						Player player2 = room.game.Players[i].realizedCreature as Player;
						player2.SuperHardSetPosition(new Vector2(-500f, 350f));
						player2.Stun(40);
						for (int j = 0; j < player2.bodyChunks.Length; j++)
						{
							player2.bodyChunks[j].vel.y = Mathf.Clamp(player2.bodyChunks[j].vel.y, -5f, 5f);
						}
					}
				}
				HardSetStunAllObjects(room, new Vector2(-500f, 2000f));
			}
			if (fadeIn != null && fadeIn.IsDoneFading() && player != null)
			{
				timer++;
				if (timer == 1)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.95f);
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.98f);
					player.InitChatLog(ChatlogData.ChatlogID.Chatlog_LM9);
				}
				if (timer > 1 && !player.chatlog && fadeOut == null)
				{
					fadeOut = new FadeOut(room, Color.black, 200f, fadeIn: false);
					room.AddObject(fadeOut);
				}
			}
			if (fadeOut == null || !fadeOut.IsDoneFading() || warpExecuted)
			{
				return;
			}
			room.game.manager.pebblesHasHalcyon = true;
			room.game.manager.desiredCreditsSong = "NA_19 - Halcyon Memories";
			foreach (PersistentObjectTracker objectTracker in room.game.GetStorySession.saveState.objectTrackers)
			{
				if (objectTracker.repType == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl && objectTracker.lastSeenRoom != "RM_AI")
				{
					room.game.manager.pebblesHasHalcyon = false;
					room.game.manager.desiredCreditsSong = "NA_43 - Isolation";
					break;
				}
			}
			room.game.manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.RivuletAltEnd;
			room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
			warpExecuted = true;
		}
	}

	public class SL_AI_Behavior : UpdatableAndDeletable
	{
		private int counter;

		private FadeOut fadeIn;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.realizedCreature != null)
			{
				Player player = room.game.cameras[0].followAbstractCreature.realizedCreature as Player;
				if (player.sceneFlag)
				{
					Destroy();
				}
				else
				{
					room.world.rainCycle.preTimer = 0;
					player.sleepCounter = 99;
					if (counter < 40)
					{
						player.SuperHardSetPosition(new Vector2(1530f, 155f));
					}
					if (fadeIn == null)
					{
						fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
						room.AddObject(fadeIn);
					}
					if (fadeIn != null)
					{
						if (counter < 80)
						{
							fadeIn.fade = 1f;
						}
						if (fadeIn.IsDoneFading())
						{
							player.sceneFlag = true;
							Destroy();
						}
					}
				}
			}
			counter++;
		}

		public SL_AI_Behavior(Room room)
		{
			base.room = room;
		}
	}

	public class ArtificerDream_1 : ArtificerDream
	{
		private Player artyPlayerPuppet;

		private Player pupPlayerPuppet;

		private AbstractCreature artificerPuppet;

		private AbstractCreature pup2Puppet;

		private Vector2 ArtyGoalPos;

		public ArtificerDream_1(Room room)
		{
			base.room = room;
			base.room.game.cameras[0].followAbstractCreature = null;
			Custom.Log("Artificer dream 1, intro");
		}

		public override Player.InputPackage GetInput(int index)
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (sceneTimer < 160)
			{
				if (firstAlivePlayer != null)
				{
					(firstAlivePlayer.realizedCreature as Player).SuperHardSetPosition(artyPlayerPuppet.firstChunk.pos);
				}
				return default(Player.InputPackage);
			}
			if (sceneTimer == 160)
			{
				artyPlayerPuppet.bodyChunks[0].vel *= 0f;
				artyPlayerPuppet.bodyChunks[1].vel *= 0f;
				artyPlayerPuppet.bodyChunks[0].pos = new Vector2(1900f, 340f);
				artyPlayerPuppet.bodyChunks[1].pos = new Vector2(1900f, 320f);
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 165)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer < 166)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 166)
			{
				artyPlayerPuppet.bodyChunks[0].vel += new Vector2(10f, 13f);
				artyPlayerPuppet.bodyChunks[1].vel += new Vector2(10f, 13f);
				room.AddObject(new ExplosionSpikes(room, artyPlayerPuppet.bodyChunks[0].pos + new Vector2(0f, 0f - artyPlayerPuppet.bodyChunks[0].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
				room.PlaySound(SoundID.Slugcat_Rocket_Jump, artyPlayerPuppet.bodyChunks[0], loop: false, 1f, 1f);
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= 190)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= 210)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 211)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 212)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= 239)
			{
				return default(Player.InputPackage);
			}
			if (sceneTimer <= 300)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 301)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == 302)
			{
				artyPlayerPuppet.slugOnBack.DropSlug();
				if (firstAlivePlayer != null)
				{
					(firstAlivePlayer.realizedCreature as Player).Stun(5);
					(firstAlivePlayer.realizedCreature as Player).firstChunk.vel = new Vector2(5f, 5f);
					(firstAlivePlayer.realizedCreature as Player).standing = true;
				}
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= 304)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= 325)
			{
				ArtyGoalPos = room.MiddleOfTile(116, 18);
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer > 325)
			{
				bool jmp = false;
				int x = 0;
				if (artyPlayerPuppet.firstChunk.pos.x < ArtyGoalPos.x - 9f)
				{
					x = 1;
				}
				else if (artyPlayerPuppet.firstChunk.pos.x > ArtyGoalPos.x + 9f)
				{
					x = -1;
					jmp = sceneTimer % 20 <= 5 && artyPlayerPuppet.bodyMode != Player.BodyModeIndex.ClimbingOnBeam;
				}
				int y = 0;
				if (artyPlayerPuppet.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
				{
					if (artyPlayerPuppet.firstChunk.pos.y < ArtyGoalPos.y - 5f)
					{
						y = ((!(artyPlayerPuppet.firstChunk.pos.x > ArtyGoalPos.x + 9f) || sceneTimer % 20 > 5) ? 1 : 0);
					}
					else if (artyPlayerPuppet.firstChunk.pos.y > ArtyGoalPos.y + 5f)
					{
						y = -1;
					}
				}
				else
				{
					y = UnityEngine.Random.Range(0, 2);
				}
				if (firstAlivePlayer != null && Mathf.Abs(room.cameraPositions[2].x + room.game.cameras[0].sSize.x / 2f - (firstAlivePlayer.realizedCreature as Player).firstChunk.pos.x) > 1000f && sceneTimer < 2000)
				{
					Custom.Log("Pup out of camera, cut early");
					sceneTimer = 1999;
				}
				else if (pup2Puppet.state.dead && sceneTimer < 2000)
				{
					Custom.Log("Other pup died! cut early");
					sceneTimer = 1999;
				}
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, y, jmp, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			return default(Player.InputPackage);
		}

		private void SpawnAmbientCritters()
		{
			for (int i = 0; i < 3; i++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA), null, new WorldCoordinate(room.abstractRoom.index, 200 + (int)(UnityEngine.Random.value * 80f), 32 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
			}
			for (int j = 0; j < 2; j++)
			{
				AbstractCreature abstractCreature2 = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaB), null, new WorldCoordinate(room.abstractRoom.index, 200 + (int)(UnityEngine.Random.value * 80f), 36 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature2);
				abstractCreature2.RealizeInRoom();
			}
			for (int k = 0; k < 15; k++)
			{
				AbstractCreature abstractCreature3 = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, new WorldCoordinate(room.abstractRoom.index, 90 + (int)(UnityEngine.Random.value * 80f), 22 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature3);
				abstractCreature3.RealizeInRoom();
			}
		}

		public override void SceneSetup()
		{
			if (artificerPuppet == null)
			{
				artificerPuppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.abstractRoom.index, 87, 8, -1), room.game.GetNewID());
				artificerPuppet.state = new PlayerState(artificerPuppet, 0, MoreSlugcatsEnums.SlugcatStatsName.Artificer, isGhost: true);
				room.abstractRoom.AddEntity(artificerPuppet);
				pup2Puppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(room.abstractRoom.index, 87, 8, -1), room.game.GetNewID());
				pup2Puppet.ID.setAltSeed(1001);
				pup2Puppet.state = new PlayerNPCState(pup2Puppet, 0);
				room.abstractRoom.AddEntity(pup2Puppet);
				artificerPuppet.RealizeInRoom();
				pup2Puppet.RealizeInRoom();
			}
			if (artyPlayerPuppet == null && artificerPuppet.realizedCreature != null)
			{
				artyPlayerPuppet = artificerPuppet.realizedCreature as Player;
			}
			if (pupPlayerPuppet == null && pup2Puppet.realizedCreature != null)
			{
				pupPlayerPuppet = pup2Puppet.realizedCreature as Player;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && artyPlayerPuppet != null && pupPlayerPuppet != null && firstAlivePlayer.realizedCreature != null)
			{
				Custom.Log("scene start");
				SpawnAmbientCritters();
				pup2Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceLike(1f);
				pup2Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceTempLike(1f);
				artyPlayerPuppet.controller = new StartController(this, 0);
				artyPlayerPuppet.standing = true;
				artyPlayerPuppet.slugcatStats.visualStealthInSneakMode = 2f;
				if (firstAlivePlayer.realizedCreature != null)
				{
					(firstAlivePlayer.realizedCreature as Player).SuperHardSetPosition(artyPlayerPuppet.firstChunk.pos);
					firstAlivePlayer.pos = artyPlayerPuppet.abstractCreature.pos;
				}
				artyPlayerPuppet.slugOnBack.SlugToBack(firstAlivePlayer.realizedCreature as Player);
				sceneStarted = true;
			}
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(2);
		}

		public override void TimedUpdate(int timer)
		{
			if (artyPlayerPuppet.firstChunk.pos.y < 220f)
			{
				ArtyGoalPos = artyPlayerPuppet.firstChunk.pos;
			}
			if (sceneTimer == 2000)
			{
				Custom.Log("Dream over");
				room.game.ArtificerDreamEnd();
			}
		}
	}

	public class ArtificerDream_2 : ArtificerDream
	{
		private AbstractCreature pup1Puppet;

		private AbstractCreature pup2Puppet;

		private Player pup1PlayerPuppet;

		private Player pup2PlayerPuppet;

		public ArtificerDream_2(Room room)
		{
			base.room = room;
			Custom.Log("Artificer dream 2, family safety");
		}

		public override void SceneSetup()
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (pup1Puppet == null)
			{
				pup1Puppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
				pup1Puppet.remainInDenCounter = 120;
				pup1Puppet.ID.setAltSeed(1000);
				pup1Puppet.state = new PlayerNPCState(pup1Puppet, 0);
				room.world.offScreenDen.AddEntity(pup1Puppet);
				if (firstAlivePlayer != null)
				{
					pup1Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceLike(1f);
					pup1Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceTempLike(1f);
				}
			}
			if (pup2Puppet == null)
			{
				pup2Puppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
				pup2Puppet.remainInDenCounter = 120;
				pup2Puppet.ID.setAltSeed(1001);
				pup2Puppet.state = new PlayerNPCState(pup2Puppet, 0);
				room.world.offScreenDen.AddEntity(pup2Puppet);
				if (firstAlivePlayer != null)
				{
					pup2Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceLike(1f);
					pup2Puppet.state.socialMemory.GetOrInitiateRelationship(firstAlivePlayer.ID).InfluenceTempLike(1f);
				}
			}
			if (pup1Puppet != null && pup2Puppet != null)
			{
				SpawnAmbientCritters();
				Custom.Log("Scene started");
				sceneStarted = true;
			}
		}

		private void SpawnAmbientCritters()
		{
			for (int i = 0; i < 2; i++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA), null, new WorldCoordinate(room.abstractRoom.index, (int)(UnityEngine.Random.value * 40f), 19 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
			}
			for (int j = 0; j < 11; j++)
			{
				AbstractCreature abstractCreature2 = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, new WorldCoordinate(room.abstractRoom.index, (int)(UnityEngine.Random.value * 40f), 35 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature2);
				abstractCreature2.RealizeInRoom();
			}
		}

		public override Player.InputPackage GetInput(int index)
		{
			return default(Player.InputPackage);
		}

		public override void TimedUpdate(int timer)
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (timer == 390)
			{
				Custom.Log("ACTORS ENTER");
				firstAlivePlayer?.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 0));
			}
			if (timer == 420)
			{
				Custom.Log("ACTORS ENTER");
				pup1Puppet.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 0));
			}
			if (timer == 490)
			{
				Custom.Log("ACTORS ENTER");
				pup2Puppet.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 0));
			}
			if (timer == 1400)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
				abstractCreature.ignoreCycle = true;
				room.abstractRoom.AddEntity(abstractCreature);
			}
			if (pup1Puppet.state.dead || pup2Puppet.state.dead)
			{
				timer = 1700;
			}
			if (timer == 1700)
			{
				room.game.ArtificerDreamEnd();
			}
			if (timer > 460 && firstAlivePlayer != null && firstAlivePlayer.Room == room.abstractRoom && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.enteringShortCut.HasValue && room.shortcutData(firstAlivePlayer.realizedCreature.enteringShortCut.Value).shortCutType == ShortcutData.Type.RoomExit)
			{
				room.game.ArtificerDreamEnd();
				room.abstractRoom.connections = new int[2];
				room.abstractRoom.connections[0] = room.world.offScreenDen.index;
				room.abstractRoom.connections[1] = room.world.offScreenDen.index;
			}
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(0);
		}
	}

	public class ArtificerDream_3 : ArtificerDream
	{
		private AbstractCreature artyPuppet;

		private AbstractCreature pup2Puppet;

		private Player artyPlayerPuppet;

		private Player pup1PlayerPuppet;

		private Player pup2PlayerPuppet;

		private DataPearl fatePearl;

		public ArtificerDream_3(Room room)
		{
			base.room = room;
			base.room.game.cameras[0].followAbstractCreature = null;
			base.room.world.rainCycle.timer = 400;
			base.room.world.game.session.creatureCommunities.scavengerShyness = 0f;
			base.room.world.game.session.creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0, -10f);
			Custom.Log("Artificer dream 3, treasures");
		}

		public override void SceneSetup()
		{
			if (artyPuppet == null)
			{
				artyPuppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
				artyPuppet.remainInDenCounter = 120;
				artyPuppet.state = new PlayerState(artyPuppet, 1, MoreSlugcatsEnums.SlugcatStatsName.Artificer, isGhost: false);
				room.world.offScreenDen.AddEntity(artyPuppet);
			}
			if (pup2Puppet == null)
			{
				pup2Puppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID());
				pup2Puppet.remainInDenCounter = 120;
				pup2Puppet.ID.setAltSeed(1000);
				pup2Puppet.state = new PlayerState(pup2Puppet, 2, MoreSlugcatsEnums.SlugcatStatsName.Slugpup, isGhost: false);
				room.world.offScreenDen.AddEntity(pup2Puppet);
			}
			if (artyPuppet == null || pup2Puppet == null)
			{
				return;
			}
			Custom.Log("Scene started");
			Vector2 b = new Vector2(755f, 1216f);
			float num = float.PositiveInfinity;
			DataPearl.AbstractDataPearl abstractDataPearl = null;
			foreach (UpdatableAndDeletable update in room.updateList)
			{
				if (update is DataPearl && Vector2.Distance((update as DataPearl).firstChunk.pos, b) < num)
				{
					abstractDataPearl = (update as DataPearl).AbstractPearl;
					num = Vector2.Distance((update as DataPearl).firstChunk.pos, b);
				}
			}
			abstractDataPearl.dataPearlType = DataPearl.AbstractDataPearl.DataPearlType.Misc2;
			Custom.Log($"Reformatted pearl {abstractDataPearl}");
			(abstractDataPearl.realizedObject as DataPearl).forceReapplyPalette = true;
			fatePearl = abstractDataPearl.realizedObject as DataPearl;
			SpawnAmbientCritters();
			sceneStarted = true;
		}

		public override void TimedUpdate(int timer)
		{
			if (artyPuppet.realizedCreature != null && (artyPuppet.realizedCreature as Player).controller == null)
			{
				artyPlayerPuppet = artyPuppet.realizedCreature as Player;
				artyPlayerPuppet.controller = new StartController(this, 0);
				artyPlayerPuppet.aerobicLevel = 1f;
			}
			else if (artyPlayerPuppet != null && timer < 500)
			{
				artyPlayerPuppet.standing = true;
				if (artyPlayerPuppet.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
				{
					artyPlayerPuppet.Jump();
				}
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && (firstAlivePlayer.realizedCreature as Player).controller == null)
			{
				pup1PlayerPuppet = firstAlivePlayer.realizedCreature as Player;
				pup1PlayerPuppet.controller = new StartController(this, 1);
			}
			else if (pup1PlayerPuppet != null)
			{
				pup1PlayerPuppet.standing = true;
				if (pup1PlayerPuppet.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || Vector2.Distance(pup1PlayerPuppet.firstChunk.pos, new Vector2(830f, pup1PlayerPuppet.firstChunk.pos.y)) < 10f)
				{
					pup1PlayerPuppet.Jump();
				}
			}
			if (pup2Puppet.realizedCreature != null && (pup2Puppet.realizedCreature as Player).controller == null)
			{
				pup2PlayerPuppet = pup2Puppet.realizedCreature as Player;
				pup2PlayerPuppet.controller = new StartController(this, 2);
			}
			else if (pup2PlayerPuppet != null)
			{
				pup2PlayerPuppet.standing = true;
				if ((pup2PlayerPuppet.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || Vector2.Distance(pup2PlayerPuppet.firstChunk.pos, new Vector2(830f, pup2PlayerPuppet.firstChunk.pos.y)) < 10f) && pup1PlayerPuppet.firstChunk.pos.x < 700f)
				{
					pup2PlayerPuppet.Jump();
				}
			}
			if (timer == 340)
			{
				pup2Puppet.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 1));
			}
			if (timer == 230)
			{
				artyPuppet.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 1));
			}
			if (timer == 190)
			{
				firstAlivePlayer?.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, 1));
			}
		}

		public override Player.InputPackage GetInput(int index)
		{
			bool pckp = false;
			if (sceneTimer < 2000)
			{
				switch (index)
				{
				case 0:
				{
					bool jmp2 = Mathf.Abs(825f - artyPlayerPuppet.firstChunk.pos.x) < 11f;
					if (sceneTimer < 400 || sceneTimer > 470)
					{
						if (sceneTimer == 650 || sceneTimer == 700 || sceneTimer == 750 || sceneTimer == 800)
						{
							pckp = true;
						}
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp2, thrw: false, pckp, mp: false, crouchToggle: false);
					}
					return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: true, crouchToggle: false);
				}
				case 1:
				{
					bool flag2 = Mathf.Abs(825f - pup1PlayerPuppet.firstChunk.pos.x) < 14f;
					return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, flag2 ? 1 : 0, flag2, thrw: false, pckp: false, mp: false, crouchToggle: false);
				}
				case 2:
					if (sceneTimer < 400)
					{
						pup2PlayerPuppet.standing = false;
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
					}
					if (sceneTimer == 410)
					{
						pup2PlayerPuppet.standing = true;
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
					}
					if (sceneTimer > 520)
					{
						if (sceneTimer == 521)
						{
							room.game.ArtificerDreamEnd();
						}
						pup2PlayerPuppet.standing = true;
						(pup2PlayerPuppet.graphicsModule as PlayerGraphics).LookAtPoint(fatePearl.firstChunk.pos, 3000f);
						if (sceneTimer <= 580)
						{
							return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
						}
						if (sceneTimer < 700)
						{
							return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
						}
						bool flag = pup2PlayerPuppet.firstChunk.pos.x > 680f && UnityEngine.Random.value < 0.96f;
						bool jmp = pup2PlayerPuppet.animation == Player.AnimationIndex.BeamTip && UnityEngine.Random.value < 0.16f && pup2PlayerPuppet.canJump == 0;
						bool num = pup2PlayerPuppet.firstChunk.pos.x > 800f;
						bool num2 = pup2PlayerPuppet.bodyMode != Player.BodyModeIndex.ClimbingOnBeam;
						int x = 0;
						if (num2)
						{
							x = 1;
						}
						if (num)
						{
							x = -1;
						}
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, flag ? 1 : 0, jmp, thrw: false, pckp: false, mp: false, crouchToggle: false);
					}
					break;
				}
			}
			return default(Player.InputPackage);
		}

		private void SpawnAmbientCritters()
		{
			for (int i = 0; i < 21; i++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, new WorldCoordinate(room.abstractRoom.index, 5 + (int)(UnityEngine.Random.value * 80f), 68 + (int)(UnityEngine.Random.value * 10f), -1), room.game.GetNewID());
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
			}
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(0);
		}
	}

	public class ArtificerDream_4 : ArtificerDream
	{
		private AbstractCreature artificerPuppet;

		private AbstractCreature pup2Puppet;

		private Player artyPlayerPuppet;

		private Player pup2PlayerPuppet;

		private List<AbstractCreature> scavengers;

		public ArtificerDream_4(Room room)
		{
			base.room = room;
			base.room.game.cameras[0].followAbstractCreature = null;
			base.room.world.rainCycle.timer = base.room.world.rainCycle.cycleLength - 2400;
			base.room.world.game.session.creatureCommunities.scavengerShyness = 0f;
			base.room.world.game.session.creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0, -10f);
			scavengers = new List<AbstractCreature>();
			Custom.Log("Artificer dream 4, escape!");
		}

		public override void SceneSetup()
		{
			if (artificerPuppet == null)
			{
				artificerPuppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.abstractRoom.index, 52, 42, -1), room.game.GetNewID());
				artificerPuppet.state = new PlayerState(artificerPuppet, 0, MoreSlugcatsEnums.SlugcatStatsName.Artificer, isGhost: true);
				room.abstractRoom.AddEntity(artificerPuppet);
				artificerPuppet.RealizeInRoom();
				pup2Puppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.abstractRoom.index, 52, 42, -1), room.game.GetNewID());
				pup2Puppet.ID.setAltSeed(1001);
				pup2Puppet.state = new PlayerState(pup2Puppet, 0, MoreSlugcatsEnums.SlugcatStatsName.Slugpup, isGhost: true);
				room.abstractRoom.AddEntity(pup2Puppet);
				pup2Puppet.RealizeInRoom();
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				(firstAlivePlayer.realizedCreature as Player).SuperHardSetPosition(room.MiddleOfTile(new WorldCoordinate(room.abstractRoom.index, 50, 44, -1).Tile));
			}
			if (artyPlayerPuppet == null && artificerPuppet.realizedCreature != null)
			{
				artyPlayerPuppet = artificerPuppet.realizedCreature as Player;
			}
			if (pup2PlayerPuppet == null && pup2Puppet.realizedCreature != null)
			{
				pup2PlayerPuppet = pup2Puppet.realizedCreature as Player;
				pup2PlayerPuppet.controller = new StartController(this, 1);
			}
			if (firstAlivePlayer != null && artyPlayerPuppet != null && pup2PlayerPuppet != null && firstAlivePlayer.realizedCreature != null)
			{
				artyPlayerPuppet.controller = new StartController(this, 0);
				artyPlayerPuppet.standing = true;
				(firstAlivePlayer.realizedCreature as Player).controller = new StartController(this, 2);
				artyPlayerPuppet.slugOnBack.SlugToBack(pup2PlayerPuppet);
				DataPearl.AbstractDataPearl abstractDataPearl = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(room.abstractRoom.index, 50, 42, -1), room.game.GetNewID(), room.abstractRoom.index, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc2);
				abstractDataPearl.RealizeInRoom();
				(firstAlivePlayer.realizedCreature as Player).Grab(abstractDataPearl.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, 1f, overrideEquallyDominant: true, pacifying: false);
				sceneStarted = true;
			}
		}

		public override Player.InputPackage GetInput(int index)
		{
			int num = 219;
			int num2 = num + 60;
			int num3 = 400;
			if (index == 2)
			{
				AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
				if (sceneTimer < num - 109)
				{
					return default(Player.InputPackage);
				}
				if (sceneTimer != num2 + 6)
				{
					if (firstAlivePlayer != null)
					{
						(firstAlivePlayer.realizedCreature as Player).standing = true;
					}
					return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
				}
				if (firstAlivePlayer != null)
				{
					(firstAlivePlayer.realizedCreature as Player).controller = null;
				}
			}
			if (sceneTimer < num - 50 && (index == 0 || index == 2))
			{
				int pyroJumpCounter = Mathf.Max(1, MoreSlugcats.cfgArtificerExplosionCapacity.Value - 5);
				artyPlayerPuppet.standing = true;
				artyPlayerPuppet.pyroJumpCounter = pyroJumpCounter;
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer < num + 25)
			{
				artyPlayerPuppet.standing = true;
				pup2PlayerPuppet.standing = true;
				if (index == 0 && sceneTimer > num - 10)
				{
					if (sceneTimer >= num && sceneTimer < num + 10)
					{
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
					}
					if (sceneTimer == num + 10)
					{
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
					}
					if (sceneTimer == num + 11)
					{
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: true, mp: false, crouchToggle: false);
					}
					if (sceneTimer > num + 11)
					{
						return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: true, mp: false, crouchToggle: false);
					}
					return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
				}
			}
			if (sceneTimer < num2 && index == 0)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == num2 && index == 0)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= num2 + 15 && index == 0)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer == num2 + 16 && index == 0)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer <= num2 + 20 && index == 0)
			{
				if (sceneTimer == num2 + 17)
				{
					Custom.Log("pup release");
					if (artyPlayerPuppet.slugOnBack.HasASlug)
					{
						artyPlayerPuppet.slugOnBack.DropSlug();
					}
				}
				if (sceneTimer == num2 + 18)
				{
					Custom.Log("pup launch");
					pup2PlayerPuppet.Stun(60);
					pup2PlayerPuppet.bodyChunks[0].vel = new Vector2(14f, 16f);
					pup2PlayerPuppet.bodyChunks[1].vel = new Vector2(14f, 16f);
					artyPlayerPuppet.bodyChunks[0].vel = new Vector2(17f, 38f);
					artyPlayerPuppet.bodyChunks[1].vel = new Vector2(17f, 38f);
				}
				artyPlayerPuppet.playerState.permanentDamageTracking = 0.5;
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			if (sceneTimer < num2 + 100 && index == 1)
			{
				pup2PlayerPuppet.standing = true;
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, (sceneTimer < num2 + 102) ? 1 : 0, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer > num3 && sceneTimer < 800 && index == 0)
			{
				if (artyPlayerPuppet.firstChunk.pos.x < 1939f)
				{
					artyPlayerPuppet.firstChunk.vel += new Vector2(0f, 1f);
					artyPlayerPuppet.playerState.permanentDamageTracking = 0.0;
				}
				else
				{
					artyPlayerPuppet.playerState.permanentDamageTracking = 0.5;
				}
				int y = ((sceneTimer < num3 + 2) ? 1 : 0);
				if (!artyPlayerPuppet.standing && (double)UnityEngine.Random.value < 0.1)
				{
					y = 1;
				}
				if (!pup2PlayerPuppet.standing && (double)UnityEngine.Random.value < 0.1)
				{
					y = 1;
				}
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, y, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer > num2 + 100 && sceneTimer < 700 && index == 1)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (sceneTimer > num2 + 101 && sceneTimer < 700 && index == 1)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, Mathf.Floor(Mathf.Pow(UnityEngine.Random.value + 0.5f, 4f)) == 1f, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			return default(Player.InputPackage);
		}

		public override void TimedUpdate(int timer)
		{
			if (sceneTimer < 200 && (sceneTimer % 36 == 0 || sceneTimer % 24 == 0 || sceneTimer % 19 == 0) && sceneTimer % 48 == 0)
			{
				Custom.Log("SPAWN SPEAR");
				AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(room.world, null, new WorldCoordinate(room.abstractRoom.index, 50, 52 + UnityEngine.Random.Range(-2, 6), -1), room.game.GetNewID(), explosive: false);
				room.abstractRoom.AddEntity(abstractPhysicalObject);
				abstractPhysicalObject.RealizeInRoom();
				if (abstractPhysicalObject.realizedObject != null)
				{
					Spear spear = abstractPhysicalObject.realizedObject as Spear;
					spear.Thrown(artyPlayerPuppet, spear.firstChunk.pos, null, new IntVector2(1, 0), 1.2f, eu: true);
				}
			}
			if (timer > 100 && timer < 350 && artyPlayerPuppet.firstChunk.pos.x > 1900f)
			{
				artyPlayerPuppet.Stun(60);
			}
			if (timer == 450)
			{
				Custom.Log("Spawn attacking scavs");
				WorldCoordinate coord = new WorldCoordinate(room.abstractRoom.index, 50, 55, -1);
				scavengers.Add(SpawnUniqueScav(8227, coord, inRoomRealize: true));
				coord = new WorldCoordinate(room.abstractRoom.index, 50, 59, -1);
				scavengers.Add(SpawnUniqueScav(3917, coord, inRoomRealize: true));
				coord = new WorldCoordinate(room.abstractRoom.index, 50, 58, -1);
				scavengers.Add(SpawnUniqueScav(9958, coord, inRoomRealize: true));
				coord = new WorldCoordinate(room.abstractRoom.index, 50, 57, -1);
				scavengers.Add(SpawnUniqueScav(6566, coord, inRoomRealize: true));
				coord = new WorldCoordinate(room.abstractRoom.index, 50, 56, -1);
				scavengers.Add(SpawnUniqueScav(7778, coord, inRoomRealize: true));
				for (int i = 0; i < 7; i++)
				{
					coord = new WorldCoordinate(room.abstractRoom.index, 50, 55 - i, -1);
					scavengers.Add(SpawnUniqueScav(-1, coord, inRoomRealize: true));
				}
				foreach (AbstractCreature scavenger in scavengers)
				{
					if (scavenger.realizedCreature != null)
					{
						Scavenger obj = scavenger.realizedCreature as Scavenger;
						obj.AI.agitation = 1f;
						obj.AI.currentViolenceType = ScavengerAI.ViolenceType.Lethal;
						obj.AI.tracker.SeeCreature(room.game.Players[0]);
					}
				}
			}
			if (timer > 810)
			{
				artyPlayerPuppet.SuperHardSetPosition(new Vector2(90000f, 3000f));
				pup2PlayerPuppet.SuperHardSetPosition(new Vector2(90000f, 3000f));
				if (timer == 811)
				{
					room.game.ArtificerDreamEnd();
				}
			}
			if (timer == 690)
			{
				ShortcutData shortcutData = room.shortcutData(new IntVector2(106, 56));
				WorldCoordinate coord2 = new WorldCoordinate(room.abstractRoom.index, -1, -1, shortcutData.destNode);
				Custom.Log("Spawn chasing scavs");
				for (int j = 0; j < 3; j++)
				{
					AbstractCreature abstractCreature = SpawnUniqueScav(-1, coord2, inRoomRealize: false);
					abstractCreature.realizedCreature.SpitOutOfShortCut(new IntVector2(106, 56), room, spitOutAllSticks: true);
					scavengers.Add(abstractCreature);
				}
			}
			if (timer < 2000)
			{
				return;
			}
			WorldCoordinate destination = new WorldCoordinate(room.abstractRoom.index, -1, -1, 1);
			foreach (AbstractCreature scavenger2 in scavengers)
			{
				if (timer == 2000)
				{
					scavenger2.creatureTemplate = new CreatureTemplate(scavenger2.creatureTemplate);
					scavenger2.creatureTemplate.doesNotUseDens = false;
				}
				scavenger2.abstractAI.SetDestination(destination);
			}
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(1);
		}
	}

	public class ArtificerDream_5 : ArtificerDream
	{
		private Player artyPlayerPuppet;

		private AbstractCreature artificerPuppet;

		private bool bombExploded;

		private ScavengerBomb recordedBomb;

		public ArtificerDream_5(Room room)
		{
			base.room = room;
			base.room.game.cameras[0].followAbstractCreature = null;
			UpdateCycle(0);
			base.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SilenceWater).amount = 0f;
			base.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.WaterDepth).amount = 0.04f;
			Custom.Log("Artificer dream 5, drown");
		}

		private void UpdateCycle(int timer)
		{
			float t = Mathf.InverseLerp(0f, 1500f, timer);
			room.world.rainCycle.timer = (int)Mathf.Lerp(room.world.rainCycle.cycleLength - 1000, (float)room.world.rainCycle.cycleLength + 1f, t);
		}

		private void SpawnLeeches()
		{
			for (int i = 0; i < 25; i++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech), null, new WorldCoordinate(room.abstractRoom.index, 158 + (int)(UnityEngine.Random.value * 10f), 34, -1), room.game.GetNewID());
				abstractCreature.ignoreCycle = true;
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
			}
		}

		public override void SceneSetup()
		{
			UpdateCycle(0);
			if (artificerPuppet == null)
			{
				artificerPuppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, new WorldCoordinate(room.abstractRoom.index, 120, 46, -1), room.game.GetNewID());
				artificerPuppet.state = new PlayerState(artificerPuppet, 0, MoreSlugcatsEnums.SlugcatStatsName.Artificer, isGhost: true);
				room.abstractRoom.AddEntity(artificerPuppet);
				artificerPuppet.RealizeInRoom();
			}
			if (artyPlayerPuppet == null && artificerPuppet.realizedCreature != null)
			{
				artyPlayerPuppet = artificerPuppet.realizedCreature as Player;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && artyPlayerPuppet != null && artyPlayerPuppet != null && firstAlivePlayer.realizedCreature != null)
			{
				artyPlayerPuppet.controller = new StartController(this, 0);
				artyPlayerPuppet.standing = true;
				(firstAlivePlayer.realizedCreature as Player).SuperHardSetPosition(artyPlayerPuppet.firstChunk.pos);
				(firstAlivePlayer.realizedCreature as Player).controller = new StartController(this, 1);
				artyPlayerPuppet.slugOnBack.SlugToBack(firstAlivePlayer.realizedCreature as Player);
				sceneStarted = true;
				SpawnLeeches();
			}
		}

		public override Player.InputPackage GetInput(int index)
		{
			if (index == 1)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			UpdateCycle(sceneTimer);
			int num = 380;
			artyPlayerPuppet.standing = true;
			if (sceneTimer < 160)
			{
				return default(Player.InputPackage);
			}
			if (sceneTimer < num - 50 && (sceneTimer % 36 == 0 || sceneTimer % 24 == 0 || sceneTimer % 19 == 0) && sceneTimer % 48 == 0)
			{
				Custom.Log("SPAWN SPEAR");
				AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(room.world, null, new WorldCoordinate(room.abstractRoom.index, 129, 50 + UnityEngine.Random.Range(-2, 9), -1), room.game.GetNewID(), explosive: false);
				room.abstractRoom.AddEntity(abstractPhysicalObject);
				abstractPhysicalObject.RealizeInRoom();
				if (abstractPhysicalObject.realizedObject != null)
				{
					Spear spear = abstractPhysicalObject.realizedObject as Spear;
					spear.Thrown(artyPlayerPuppet, spear.firstChunk.pos, null, new IntVector2(1, 0), 1.2f, eu: true);
				}
			}
			if (sceneTimer >= num)
			{
				if (sceneTimer == num)
				{
					Custom.Log("SPAWN BOMB");
					AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, new WorldCoordinate(room.abstractRoom.index, 129, 51, -1), room.game.GetNewID());
					room.abstractRoom.AddEntity(abstractPhysicalObject2);
					abstractPhysicalObject2.RealizeInRoom();
					if (abstractPhysicalObject2.realizedObject != null)
					{
						recordedBomb = abstractPhysicalObject2.realizedObject as ScavengerBomb;
						recordedBomb.firstChunk.vel = new Vector2(45f, 0f);
						recordedBomb.ignited = true;
						recordedBomb.throwModeFrames = 30;
						recordedBomb.explosionIsForShow = true;
					}
				}
				bool flag = bombExploded;
				if (recordedBomb != null)
				{
					if (recordedBomb.slatedForDeletetion)
					{
						recordedBomb = null;
						bombExploded = true;
					}
				}
				else
				{
					bombExploded = true;
				}
				if (bombExploded)
				{
					AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
					if (bombExploded != flag)
					{
						Custom.Log("BOMB EXPLODE!");
						artyPlayerPuppet.bodyChunks[0].vel = new Vector2(18f, 12f);
						artyPlayerPuppet.bodyChunks[1].vel = new Vector2(18f, 12f);
						artyPlayerPuppet.ReleaseGrasp(0);
						artyPlayerPuppet.Stun(30);
						if (firstAlivePlayer != null)
						{
							firstAlivePlayer.realizedCreature.bodyChunks[0].vel = new Vector2(3f, -2f);
							firstAlivePlayer.realizedCreature.bodyChunks[1].vel = new Vector2(3f, -2f);
							firstAlivePlayer.realizedCreature.Deafen(370);
							firstAlivePlayer.realizedCreature.Stun(500);
						}
					}
					artyPlayerPuppet.exhausted = true;
					artyPlayerPuppet.aerobicLevel = 3f;
					artyPlayerPuppet.playerState.permanentDamageTracking = 0.5;
					artyPlayerPuppet.standing = false;
					if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature.bodyChunks[0].pos.x < 3200f)
					{
						firstAlivePlayer.realizedCreature.bodyChunks[0].vel += new Vector2(1f, 0f);
					}
					if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature.bodyChunks[0].pos.x > 3400f)
					{
						firstAlivePlayer.realizedCreature.bodyChunks[0].vel += new Vector2(-1f, 0f);
					}
					if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature.bodyChunks[0].pos.y > 720f && firstAlivePlayer.realizedCreature.bodyChunks[0].pos.y < 829f)
					{
						BodyChunk bodyChunk = firstAlivePlayer.realizedCreature.bodyChunks[0];
						bodyChunk.vel.y = bodyChunk.vel.y * 0.25f;
						bodyChunk = firstAlivePlayer.realizedCreature.bodyChunks[1];
						bodyChunk.vel.y = bodyChunk.vel.y * 0.25f;
					}
					if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature.Submersion == 1f)
					{
						if (firstAlivePlayer.realizedCreature.bodyChunks[0].pos.y < 670f)
						{
							room.game.cameras[0].followAbstractCreature = room.game.Players[0];
						}
						else
						{
							(firstAlivePlayer.realizedCreature as Player).airInLungs *= 0.8f;
						}
					}
					return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
				}
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (artyPlayerPuppet.firstChunk.pos.x <= 3062f)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (artyPlayerPuppet.firstChunk.pos.x <= 3066f)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 0, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (artyPlayerPuppet.firstChunk.pos.x > 3070f)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: true, thrw: false, pckp: true, mp: false, crouchToggle: false);
			}
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 1, 1, jmp: false, thrw: false, pckp: true, mp: false, crouchToggle: false);
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(2);
		}
	}

	public class ArtificerDream_6 : ArtificerDream
	{
		private bool playAsScav;

		private int choosenArena;

		private AbstractCreature artyPuppet;

		private Player artyPlayerPuppet;

		private AbstractCreature scavPuppet;

		private Scavenger scavPlayerPuppet;

		public ArtificerDream_6(Room room)
		{
			base.room = room;
			Custom.Log("Artificer dream 6, fight nightmares");
			base.room.game.cameras[0].followAbstractCreature = null;
			base.room.world.game.session.creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0, -10f);
			choosenArena = UnityEngine.Random.Range(0, room.cameraPositions.Length - 1);
			playAsScav = false;
			if (UnityEngine.Random.value < 0.001f)
			{
				playAsScav = true;
			}
			if (base.room.game.rainWorld.processManager.artificerDreamNumber == 5)
			{
				Custom.Log("Post king, or last family dream, nightmare flag");
				playAsScav = true;
			}
		}

		private int GetArenaNode(int screen, bool playerOrScav)
		{
			switch (screen)
			{
			case 0:
				if (!playerOrScav)
				{
					return 2;
				}
				return 1;
			case 1:
				if (!playerOrScav)
				{
					return 3;
				}
				return 0;
			default:
				return 0;
			case 2:
				if (!playerOrScav)
				{
					return 5;
				}
				return 4;
			}
		}

		public override void SceneSetup()
		{
			int arenaNode = GetArenaNode(room.game.cameras[0].currentCameraPosition, playerOrScav: true);
			int arenaNode2 = GetArenaNode(room.game.cameras[0].currentCameraPosition, playerOrScav: false);
			if (artyPuppet == null)
			{
				if (playAsScav)
				{
					artyPuppet = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, arenaNode), room.game.GetNewID());
					artyPuppet.state = new PlayerNPCState(artyPuppet, 0);
					(artyPuppet.state as PlayerState).isPup = false;
				}
				else
				{
					artyPuppet = room.world.game.Players[0];
					artyPuppet.Realize();
					artyPlayerPuppet = artyPuppet.realizedCreature as Player;
					artyPlayerPuppet.SpitOutOfShortCut(room.ShortcutLeadingToNode(arenaNode).StartTile, room, spitOutAllSticks: true);
				}
			}
			if (scavPuppet == null)
			{
				scavPuppet = SpawnUniqueScav(-1, new WorldCoordinate(room.abstractRoom.index, -1, -1, arenaNode2), inRoomRealize: false);
				scavPuppet.controlled = playAsScav;
				scavPuppet.Realize();
				scavPlayerPuppet = scavPuppet.realizedCreature as Scavenger;
				scavPlayerPuppet.SpitOutOfShortCut(room.ShortcutLeadingToNode(arenaNode2).StartTile, room, spitOutAllSticks: true);
			}
			if (scavPuppet != null && scavPlayerPuppet != null)
			{
				sceneStarted = true;
			}
		}

		public override void TimedUpdate(int timer)
		{
			if (artyPuppet != null)
			{
				if (artyPuppet.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
				{
					artyPlayerPuppet.playerState.permanentDamageTracking = Mathf.InverseLerp(0f, 3000f, timer);
				}
				else
				{
					if (timer == 600 && artyPuppet.Room.index != room.abstractRoom.index)
					{
						int arenaNode = GetArenaNode(room.game.cameras[0].currentCameraPosition, playerOrScav: true);
						artyPuppet.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, arenaNode));
					}
					if (artyPuppet.realizedCreature != null)
					{
						if (artyPlayerPuppet == null)
						{
							artyPlayerPuppet = artyPuppet.realizedCreature as Player;
							if (artyPlayerPuppet.graphicsModule != null)
							{
								artyPlayerPuppet.DisposeGraphicsModule();
							}
							artyPlayerPuppet.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
							artyPlayerPuppet.playerState.slugcatCharacter = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
							artyPlayerPuppet.playerState.isPup = false;
							artyPlayerPuppet.InitiateGraphicsModule();
						}
						else
						{
							artyPlayerPuppet.abstractCreature.personality.aggression = 1f;
							artyPlayerPuppet.abstractCreature.personality.dominance = 1f;
							artyPlayerPuppet.abstractCreature.personality.energy = 1f;
							artyPlayerPuppet.abstractCreature.personality.nervous = 1f;
							artyPlayerPuppet.abstractCreature.personality.bravery = 1f;
							artyPlayerPuppet.AI.tracker.SeeCreature(scavPlayerPuppet.abstractCreature);
						}
						if (artyPuppet.realizedCreature.dead && timer < 3001)
						{
							timer = 3001;
						}
					}
				}
			}
			if (scavPuppet.state.dead && timer < 3001)
			{
				timer = 3001;
			}
			if (timer == 3001)
			{
				room.game.ArtificerDreamEnd();
			}
		}

		public override void CameraSetup()
		{
			room.game.cameras[0].MoveCamera(choosenArena);
		}
	}

	public class ArtificerDream : UpdatableAndDeletable
	{
		public class StartController : Player.PlayerController
		{
			public ArtificerDream owner;

			public int index;

			public override Player.InputPackage GetInput()
			{
				return owner.GetInput(index);
			}

			public StartController(ArtificerDream owner, int index)
			{
				this.index = index;
				this.owner = owner;
			}
		}

		public bool sceneStarted;

		public int sceneTimer;

		private bool CameraDone;

		public virtual Player.InputPackage GetInput(int index)
		{
			return default(Player.InputPackage);
		}

		public virtual void SceneSetup()
		{
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!CameraDone && room.shortCutsReady && room.game.cameras[0].room != null)
			{
				CameraSetup();
				CameraDone = true;
			}
			else if (sceneStarted)
			{
				TimedUpdate(sceneTimer);
				sceneTimer++;
			}
			else if (room.fullyLoaded && room.ReadyForPlayer)
			{
				SceneSetup();
			}
		}

		public virtual void TimedUpdate(int timer)
		{
		}

		public AbstractCreature SpawnUniqueScav(int seedId, WorldCoordinate coord, bool inRoomRealize)
		{
			AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, coord, room.game.GetNewID());
			abstractCreature.ID.setAltSeed(seedId);
			abstractCreature.ignoreCycle = true;
			abstractCreature.creatureTemplate = new CreatureTemplate(abstractCreature.creatureTemplate);
			abstractCreature.creatureTemplate.doesNotUseDens = true;
			room.abstractRoom.AddEntity(abstractCreature);
			if (inRoomRealize)
			{
				abstractCreature.RealizeInRoom();
			}
			else
			{
				abstractCreature.Realize();
			}
			return abstractCreature;
		}

		public virtual void CameraSetup()
		{
		}
	}

	public class InterlinkControl : UpdatableAndDeletable
	{
		public Love love;

		public Vector2 lastPlayerPos;

		public bool lastKillPressed;

		public List<int>[] seqreq;

		public int seqind;

		public float triggerAnim;

		public float actAnim;

		public InterlinkControl(Room room)
		{
			base.room = room;
			string text = "3282017";
			int[] array = new int[8] { 0, 4, 1, 5, 2, 6, 3, 7 };
			char[] array2 = text.ToCharArray();
			char[] array3 = new char[array2.Length];
			for (int i = 0; i < array2.Length && i < array.Length; i++)
			{
				if (array[i] < array2.Length)
				{
					array3[i] = array2[array[i]];
				}
			}
			Array.Reverse(array3);
			seqreq = new List<int>[array3.Length];
			for (int j = 0; j < seqreq.Length; j++)
			{
				seqreq[j] = new List<int>();
				int num = int.Parse(array3[j].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
				if (num <= 4)
				{
					seqreq[j].Add(num);
					continue;
				}
				if (num <= 7)
				{
					seqreq[j].Add(1);
					seqreq[j].Add(2);
					seqreq[j].Add(3);
					seqreq[j].Add(4);
					continue;
				}
				switch (num)
				{
				case 8:
					seqreq[j].Add(1);
					seqreq[j].Add(3);
					seqreq[j].Add(4);
					break;
				case 9:
					seqreq[j].Add(2);
					seqreq[j].Add(3);
					seqreq[j].Add(4);
					break;
				}
			}
		}

		public void TriggerAttempt(int condition)
		{
			triggerAnim = 1f;
			if (seqreq[seqind].Contains(condition))
			{
				seqind++;
			}
			else if (seqreq[0].Contains(condition))
			{
				seqind = 1;
			}
			else
			{
				seqind = 0;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer == null || room == null || room.game == null || !(room.game.session is StoryGameSession) || room.game.Players.Count <= 0 || firstAlivePlayer.realizedCreature == null || firstAlivePlayer.realizedCreature.room != room)
			{
				return;
			}
			if (room.game.rainWorld.progression != null && room.game.rainWorld.progression.miscProgressionData != null && room.game.rainWorld.progression.miscProgressionData.hasDoneHeartReboot)
			{
				Destroy();
				return;
			}
			Player player = firstAlivePlayer.realizedCreature as Player;
			if (love == null)
			{
				love = new Love(new Vector2(488f, 530f));
				room.AddObject(love);
			}
			if (love.activated)
			{
				triggerAnim = 0f;
			}
			float num = 0f;
			float num2 = 0.09f;
			float num3 = 0.03f;
			if (actAnim > 0f)
			{
				actAnim = Mathf.Min(actAnim + 1f, 120f);
				num2 += 0.41f * (actAnim / 120f);
				num3 += 0.07f * (actAnim / 120f);
				num += 0.15f * (actAnim / 120f);
			}
			else if (triggerAnim > 0f)
			{
				triggerAnim += 1f;
				if (triggerAnim > 40f)
				{
					triggerAnim = 0f;
				}
				num2 -= Mathf.Sin((float)Math.PI * (triggerAnim / 40f)) * 0.04f;
				num3 += Mathf.Sin((float)Math.PI * (triggerAnim / 40f)) * 0.05f;
			}
			RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Brightness);
			RoomSettings.RoomEffect effect2 = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast);
			RoomSettings.RoomEffect effect3 = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness);
			if (effect != null)
			{
				effect.amount = num3;
			}
			if (effect2 != null)
			{
				effect2.amount = num2;
			}
			if (effect3 != null)
			{
				effect3.amount = num;
			}
			if (seqind < seqreq.Length)
			{
				if (player.killPressed && !lastKillPressed)
				{
					TriggerAttempt(0);
				}
				if (player.mainBodyChunk.pos.x < 200f && lastPlayerPos.x > 600f)
				{
					TriggerAttempt(1);
				}
				else if (player.mainBodyChunk.pos.x > 600f && lastPlayerPos.x < 200f && lastPlayerPos != Vector2.zero)
				{
					TriggerAttempt(3);
				}
				else if (player.mainBodyChunk.pos.y > 400f && lastPlayerPos.y < 150f && lastPlayerPos != Vector2.zero)
				{
					TriggerAttempt(2);
				}
				else if (player.mainBodyChunk.pos.y < 150f && lastPlayerPos.y > 400f)
				{
					TriggerAttempt(4);
				}
				if (seqind == seqreq.Length)
				{
					love.Activate();
					actAnim = 1f;
				}
			}
			lastKillPressed = player.killPressed;
			lastPlayerPos = new Vector2(player.mainBodyChunk.pos.x, player.mainBodyChunk.pos.y);
		}
	}

	public class GW_PIPE02_SCAVTUT : UpdatableAndDeletable
	{
		public GW_PIPE02_SCAVTUT(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room != null)
			{
				if (!GW_EDGE03_SCAVTUT.ArtificerLeftGW(room))
				{
					AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.ToWorldCoordinate(new Vector2(829f, 278f)), room.world.game.GetNewID());
					abstractCreature.personality.bravery = 0.9f;
					abstractCreature.personality.energy = 0.75f;
					abstractCreature.personality.sympathy = 0.8f;
					abstractCreature.personality.dominance = 0.5f;
					abstractCreature.Die();
					room.abstractRoom.AddEntity(abstractCreature);
				}
				Destroy();
			}
		}
	}

	public class GW_TOWER01_SCAVTUT : UpdatableAndDeletable
	{
		public GW_TOWER01_SCAVTUT(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room != null)
			{
				if (!GW_EDGE03_SCAVTUT.ArtificerLeftGW(room))
				{
					AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.ToWorldCoordinate(new Vector2(290f, 229f)), room.world.game.GetNewID());
					abstractCreature.personality.bravery = 0.75f;
					abstractCreature.personality.energy = 0.6f;
					abstractCreature.personality.sympathy = 0.5f;
					abstractCreature.personality.dominance = 0.45f;
					abstractCreature.Die();
					room.abstractRoom.AddEntity(abstractCreature);
				}
				Destroy();
			}
		}
	}

	public class GW_EDGE03_SCAVTUT : UpdatableAndDeletable
	{
		public GW_EDGE03_SCAVTUT(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room != null)
			{
				if (!ArtificerLeftGW(room))
				{
					AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.ToWorldCoordinate(new Vector2(970f, 1272f)), room.world.game.GetNewID());
					abstractCreature.personality.bravery = 0.9f;
					abstractCreature.personality.energy = 0.9f;
					abstractCreature.personality.sympathy = 0.9f;
					abstractCreature.personality.dominance = 0.9f;
					abstractCreature.Die();
					room.abstractRoom.AddEntity(abstractCreature);
				}
				Destroy();
			}
		}

		public static bool ArtificerLeftGW(Room room)
		{
			bool result = false;
			foreach (KeyValuePair<string, List<string>> item in room.game.GetStorySession.saveState.progression.miscProgressionData.regionsVisited)
			{
				if (item.Key != "GW" && item.Value.Contains(room.game.GetStorySession.saveStateNumber.value))
				{
					result = true;
					break;
				}
			}
			Custom.Log("Artificer has left GW", result.ToString());
			return result;
		}
	}

	public class OE_GourmandEnding : UpdatableAndDeletable
	{
		public class EndingController : Player.PlayerController
		{
			private OE_GourmandEnding owner;

			public EndingController(OE_GourmandEnding owner)
			{
				this.owner = owner;
			}

			public override Player.InputPackage GetInput()
			{
				return owner.GetInput();
			}
		}

		private Player foundPlayer;

		private bool endTrigger;

		private bool setController;

		private int endTriggerTimer;

		public bool spawnedNPCs;

		private List<AbstractCreature> npcs;

		public FadeOut fadeOut;

		private bool doneFinalSave;

		public OE_GourmandEnding(Room room)
		{
			base.room = room;
			npcs = new List<AbstractCreature>();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!ModManager.CoopAvailable)
			{
				if (foundPlayer == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null && room.game.Players[0].realizedCreature.room == room)
				{
					foundPlayer = room.game.Players[0].realizedCreature as Player;
				}
				if (foundPlayer == null || foundPlayer.inShortcut || room.game.Players[0].realizedCreature.room != room)
				{
					return;
				}
			}
			else
			{
				if (foundPlayer == null && room.PlayersInRoom.Count > 0 && room.PlayersInRoom[0] != null && room.PlayersInRoom[0].room == room)
				{
					foundPlayer = room.PlayersInRoom[0];
				}
				if (foundPlayer == null || foundPlayer.inShortcut || foundPlayer.room != room)
				{
					return;
				}
				if (ModManager.CoopAvailable)
				{
					room.game.cameras[0].EnterCutsceneMode(foundPlayer.abstractCreature, RoomCamera.CameraCutsceneType.EndingOE);
				}
			}
			float num = 2000f;
			if (room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				num = 4000f;
			}
			if (foundPlayer.firstChunk.pos.x < num && !setController)
			{
				Custom.Log("Cutscene triggered");
				RainWorld.lockGameTimer = true;
				setController = true;
				foundPlayer.controller = new EndingController(this);
			}
			if (foundPlayer != null && !spawnedNPCs && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && foundPlayer.firstChunk.pos.x < num)
			{
				for (int i = 0; i < 8; i++)
				{
					Vector2 vector = new Vector2(UnityEngine.Random.Range(480f, 3450f), UnityEngine.Random.Range(230f, 300f));
					AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, room.ToWorldCoordinate(vector), room.game.GetNewID());
					if (!room.world.game.rainWorld.setup.forcePup)
					{
						(abstractCreature.state as PlayerState).forceFullGrown = true;
					}
					room.abstractRoom.AddEntity(abstractCreature);
					abstractCreature.RealizeInRoom();
					npcs.Add(abstractCreature);
				}
				for (int j = 0; j < npcs.Count; j += 2)
				{
					int num2 = UnityEngine.Random.Range(0, npcs.Count);
					while (num2 == j || num2 >= npcs.Count || num2 % 2 == 0)
					{
						num2 = UnityEngine.Random.Range(0, npcs.Count);
					}
					npcs[j].state.socialMemory.GetOrInitiateRelationship(npcs[num2].ID).InfluenceLike(1f);
					npcs[j].state.socialMemory.GetOrInitiateRelationship(npcs[num2].ID).InfluenceTempLike(1f);
				}
				for (int k = 1; k < npcs.Count; k += 2)
				{
					Vector2 vector2 = new Vector2(UnityEngine.Random.Range(480f, 3450f), UnityEngine.Random.Range(230f, 300f));
					(npcs[k].abstractAI as SlugNPCAbstractAI).toldToStay = room.ToWorldCoordinate(vector2);
				}
				spawnedNPCs = true;
			}
			if (spawnedNPCs)
			{
				for (int l = 0; l < npcs.Count; l++)
				{
					WorldCoordinate? toldToStay = (npcs[l].abstractAI as SlugNPCAbstractAI).toldToStay;
					if (!(UnityEngine.Random.value <= 0.005f) && (!toldToStay.HasValue || (!(toldToStay.Value.Tile.FloatDist(npcs[l].pos.Tile) <= 5f) && (toldToStay.Value.Tile.x <= npcs[l].pos.Tile.x || !(foundPlayer.firstChunk.pos.x < npcs[l].realizedCreature.firstChunk.pos.x)))))
					{
						continue;
					}
					float value = UnityEngine.Random.value;
					if (value <= 0.125f && foundPlayer.firstChunk.pos.x > 2000f)
					{
						int num3 = UnityEngine.Random.Range(0, npcs.Count);
						while (num3 == l || num3 >= npcs.Count)
						{
							num3 = UnityEngine.Random.Range(0, npcs.Count);
						}
						npcs[l].state.socialMemory.GetOrInitiateRelationship(npcs[num3].ID).InfluenceLike(1f);
						npcs[l].state.socialMemory.GetOrInitiateRelationship(npcs[num3].ID).InfluenceTempLike(1f);
						(npcs[l].abstractAI as SlugNPCAbstractAI).toldToStay = null;
					}
					else if (value <= 0.25f)
					{
						npcs[l].state.socialMemory.GetOrInitiateRelationship(foundPlayer.abstractCreature.ID).InfluenceLike(1f);
						npcs[l].state.socialMemory.GetOrInitiateRelationship(foundPlayer.abstractCreature.ID).InfluenceTempLike(1f);
						(npcs[l].abstractAI as SlugNPCAbstractAI).toldToStay = null;
					}
					else
					{
						Vector2 vector3 = new Vector2((foundPlayer.firstChunk.pos.x > 2000f && UnityEngine.Random.value > 0.25f) ? UnityEngine.Random.Range(2600f, 3750f) : UnityEngine.Random.Range(340f, 900f), UnityEngine.Random.Range(230f, 300f));
						(npcs[l].abstractAI as SlugNPCAbstractAI).toldToStay = room.ToWorldCoordinate(vector3);
					}
				}
			}
			if (foundPlayer.firstChunk.pos.x < 1020f && !endTrigger)
			{
				endTriggerTimer++;
				SlugcatStats.Name saveStateNumber = room.game.GetStorySession.saveStateNumber;
				float num4 = 80f;
				if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
				{
					num4 = 200f;
				}
				if ((float)endTriggerTimer >= num4)
				{
					endTrigger = true;
					room.game.manager.sceneSlot = room.game.GetStorySession.saveStateNumber;
					if (room.game.GetStorySession.saveStateNumber == SlugcatStats.Name.White)
					{
						int num5 = 0;
						for (int m = 0; m < room.physicalObjects.Length; m++)
						{
							for (int n = 0; n < room.physicalObjects[m].Count; n++)
							{
								if (room.physicalObjects[m][n] is Player && (room.physicalObjects[m][n] as Player).isNPC)
								{
									(room.physicalObjects[m][n] as Player).AddFood(3);
									num5++;
								}
							}
						}
						room.game.rainWorld.progression.miscProgressionData.survivorPupsAtEnding = num5;
					}
					if (fadeOut == null)
					{
						fadeOut = new FadeOut(room, Color.black, 200f, fadeIn: false);
						room.AddObject(fadeOut);
					}
				}
			}
			if (fadeOut == null || !fadeOut.IsDoneFading() || doneFinalSave)
			{
				return;
			}
			if (room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts = 0;
			}
			foreach (AbstractCreature npc in npcs)
			{
				npc.realizedCreature.Destroy();
				npc.Move(new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0));
			}
			room.game.GoToRedsGameOver();
			RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
			doneFinalSave = true;
		}

		public Player.InputPackage GetInput()
		{
			return new Player.InputPackage(gamePad: true, Options.ControlSetup.Preset.None, (foundPlayer.firstChunk.pos.x > 950f) ? (-1) : 0, ((!foundPlayer.standing || foundPlayer.bodyMode == Player.BodyModeIndex.Crawl) && UnityEngine.Random.value < 0.5f) ? 1 : 0, ((!foundPlayer.standing || foundPlayer.bodyMode == Player.BodyModeIndex.Crawl) && UnityEngine.Random.value < 0.5f) ? true : false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public class LC_FINAL_Expedition : UpdatableAndDeletable
	{
		public Player player;

		public int counter;

		public LC_FINAL_Expedition(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer == null)
			{
				return;
			}
			if (player == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
			{
				player = firstAlivePlayer.realizedCreature as Player;
			}
			if (player == null || player.room == null || player.room.abstractRoom.index != room.abstractRoom.index)
			{
				return;
			}
			counter++;
			if (counter == 1)
			{
				ClearThroneDebris();
				VultureMask.AbstractVultureMask abstractVultureMask = new VultureMask.AbstractVultureMask(room.world, null, room.GetWorldCoordinate(new Vector2(2700f, 500f)), room.game.GetNewID(), 0, king: false, scavKing: true, "");
				room.abstractRoom.AddEntity(abstractVultureMask);
				abstractVultureMask.RealizeInRoom();
				if (AbstractPhysicalObject.UsesAPersistantTracker(abstractVultureMask))
				{
					room.game.GetStorySession.AddNewPersistentTracker(abstractVultureMask);
				}
			}
			if (counter > 1)
			{
				Destroy();
			}
		}

		public void ClearThroneDebris()
		{
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					if ((room.physicalObjects[i][j] is Spear || room.physicalObjects[i][j] is Rock) && Custom.Dist(new Vector2(2700f, 500f), room.physicalObjects[i][j].firstChunk.pos) < 150f)
					{
						room.physicalObjects[i][j].slatedForDeletetion = true;
					}
				}
			}
		}
	}

	public class LC_FINAL : UpdatableAndDeletable
	{
		private bool triggeredBoss;

		public Player player;

		public Scavenger king;

		public bool endingTriggered;

		public int endingTriggerTime;

		public int counter;

		public FadeOut fadeIn;

		public bool firstSummon;

		public int timeSinceDead;

		public LC_FINAL(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (firstAlivePlayer == null)
			{
				return;
			}
			if (player == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
			{
				player = firstAlivePlayer.realizedCreature as Player;
			}
			if (room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && !triggeredBoss && !endingTriggered)
			{
				if (!(room.game.GetStorySession.saveState.denPosition == "LC_FINAL") || player == null)
				{
					return;
				}
				if (player.sceneFlag)
				{
					Destroy();
					return;
				}
				player.sleepCounter = 99;
				if (counter < 40)
				{
					player.SuperHardSetPosition(new Vector2(2700f, 500f));
				}
				counter++;
				if (counter == 1)
				{
					ClearThroneDebris();
					VultureMask.AbstractVultureMask abstractVultureMask = new VultureMask.AbstractVultureMask(room.world, null, room.GetWorldCoordinate(player.firstChunk.pos), room.game.GetNewID(), 0, king: false, scavKing: true, "");
					room.abstractRoom.AddEntity(abstractVultureMask);
					abstractVultureMask.RealizeInRoom();
					if (AbstractPhysicalObject.UsesAPersistantTracker(abstractVultureMask))
					{
						room.game.GetStorySession.AddNewPersistentTracker(abstractVultureMask);
					}
				}
				if (fadeIn == null)
				{
					fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
					room.AddObject(fadeIn);
				}
				if (fadeIn != null)
				{
					if (counter < 80)
					{
						fadeIn.fade = 1f;
					}
					if (fadeIn.IsDoneFading())
					{
						player.sceneFlag = true;
						Destroy();
					}
				}
				return;
			}
			if (king != null && king.dead)
			{
				if (player == null && room.game.Players.Count > 0 && firstAlivePlayer.realizedCreature != null)
				{
					player = firstAlivePlayer.realizedCreature as Player;
				}
				if (player != null)
				{
					timeSinceDead++;
					for (int i = 0; i < player.grasps.Length; i++)
					{
						if (player.grasps[i] != null && player.grasps[i].grabbed == king)
						{
							TriggerFadeToEnding();
						}
						if (player.grasps[i] != null && player.grasps[i].grabbed is VultureMask && (player.grasps[i].grabbed as VultureMask).maskGfx.ScavKing)
						{
							TriggerFadeToEnding();
						}
					}
					if (player.room == null || player.room != room)
					{
						TriggerFadeToEnding();
					}
					if (timeSinceDead > 1200)
					{
						TriggerFadeToEnding();
					}
				}
			}
			if (player != null && player.abstractCreature.Room == room.abstractRoom && player.room != null)
			{
				if (player.room.game.cameras[0] != null && player.room.game.cameras[0].currentCameraPosition == 1 && !player.sceneFlag)
				{
					TriggerBossFight();
				}
			}
			else
			{
				player = null;
			}
			if (triggeredBoss && king != null && !king.kingWaiting)
			{
				if (counter % 400 == 0)
				{
					SummonScavengers();
				}
				counter++;
				if (player != null && player.abstractCreature.Room == room.abstractRoom && player.enteringShortCut.HasValue)
				{
					Custom.Log("SPAWN EXIT BLOCK SCAV, DENY PLAYER EXIT");
					new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0), room.game.GetNewID()).Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, room.shortcutData(player.enteringShortCut.Value).destNode));
					player.enteringShortCut = null;
					player.firstChunk.vel = new Vector2(10f, 2f);
					player.Stun(5);
				}
			}
			if (endingTriggered)
			{
				endingTriggerTime++;
				if (endingTriggerTime == 80)
				{
					room.game.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma = false;
					room.game.GoToRedsGameOver();
					RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
				}
			}
		}

		public void TriggerBossFight()
		{
			if (!triggeredBoss)
			{
				triggeredBoss = true;
				player.sceneFlag = true;
				room.TriggerCombatArena();
				ClearThroneDebris();
				AbstractCreature abstractCreature = new AbstractCreature(pos: new WorldCoordinate(room.abstractRoom.index, 135, 27, -1), world: room.world, creatureTemplate: StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing), realizedCreature: null, ID: room.game.GetNewID());
				abstractCreature.ID.setAltSeed(8875);
				abstractCreature.ignoreCycle = true;
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
				king = abstractCreature.realizedCreature as Scavenger;
			}
		}

		public void TriggerFadeToEnding()
		{
			player.controller = new Player.NullController();
			room.game.manager.sceneSlot = room.game.GetStorySession.saveStateNumber;
			endingTriggered = true;
		}

		public void ClearThroneDebris()
		{
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					if ((room.physicalObjects[i][j] is Spear || room.physicalObjects[i][j] is Rock) && Custom.Dist(new Vector2(2700f, 500f), room.physicalObjects[i][j].firstChunk.pos) < 150f)
					{
						room.physicalObjects[i][j].slatedForDeletetion = true;
					}
				}
			}
		}

		public void SummonScavengers()
		{
			if (!firstSummon)
			{
				for (int i = 0; i < 3; i++)
				{
					AbstractCreature abstractCreature = new AbstractCreature(pos: new WorldCoordinate(room.abstractRoom.index, 6 + i, 8, -1), world: room.world, creatureTemplate: StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite), realizedCreature: null, ID: room.game.GetNewID());
					(abstractCreature.abstractAI as ScavengerAbstractAI).InitGearUp();
					room.abstractRoom.AddEntity(abstractCreature);
					abstractCreature.RealizeInRoom();
				}
				firstSummon = true;
			}
			int num = 0;
			for (int j = 0; j < room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < room.physicalObjects[j].Count; k++)
				{
					if (room.physicalObjects[j][k] is Scavenger && !(room.physicalObjects[j][k] as Scavenger).King && !(room.physicalObjects[j][k] as Scavenger).dead)
					{
						num++;
					}
				}
			}
			int num2 = Mathf.Min(4, 8 - num);
			if (num2 <= 0)
			{
				return;
			}
			for (int num3 = room.world.NumberOfRooms - 1; num3 >= 0; num3--)
			{
				AbstractRoom abstractRoom = room.world.GetAbstractRoom(num3 + room.world.firstRoomIndex);
				if (abstractRoom.index != room.abstractRoom.index)
				{
					foreach (AbstractCreature creature in abstractRoom.creatures)
					{
						if ((creature.creatureTemplate.type == CreatureTemplate.Type.Scavenger || creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) && creature.abstractAI != null)
						{
							IntVector2 tilePosition = room.world.activeRooms[0].GetTilePosition(new Vector2(UnityEngine.Random.Range(120, 1000), 150f));
							creature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, tilePosition.x, tilePosition.y, -1));
							num2--;
							if (num2 <= 0)
							{
								break;
							}
						}
					}
					if (num2 <= 0)
					{
						break;
					}
				}
			}
		}
	}

	public class OE_NPCControl : UpdatableAndDeletable
	{
		public class NPCController : Player.PlayerController
		{
			public OE_NPCControl owner;

			public override Player.InputPackage GetInput()
			{
				return owner.ControllerInput();
			}

			public NPCController(OE_NPCControl owner)
			{
				this.owner = owner;
			}
		}

		public List<int> entryPipes;

		public Vector2 targetPos;

		public Vector2 spawnPos;

		public int waitTillWalk;

		public int waitTimer;

		public AbstractCreature npc;

		public bool npcSpawned;

		public float xTarget;

		public Player foundPlayer;

		public bool playerZoneTriggered;

		public int foundPlayerEntryNode;

		public bool useController;

		public OE_NPCControl(Room room)
		{
			base.room = room;
			entryPipes = new List<int>();
		}

		public override void Update(bool eu)
		{
			if (room == null)
			{
				return;
			}
			base.Update(eu);
			if (!room.game.IsStorySession || (!npcSpawned && room.game.GetStorySession.saveState.oeEncounters.Contains(room.abstractRoom.name)) || room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Gourmand || room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				Destroy();
			}
			else
			{
				if (!room.readyForAI)
				{
					return;
				}
				if (foundPlayer == null)
				{
					foundPlayerEntryNode = -1;
				}
				AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
				if (foundPlayer == null && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
				{
					foundPlayer = firstAlivePlayer.realizedCreature as Player;
				}
				if (foundPlayer != null && foundPlayer.room != null && foundPlayer.room.abstractRoom.index != room.abstractRoom.index)
				{
					foundPlayerEntryNode = -1;
				}
				if (foundPlayer != null && foundPlayerEntryNode == -1)
				{
					for (int i = 0; i < room.game.shortcuts.transportVessels.Count; i++)
					{
						ShortcutHandler.ShortCutVessel shortCutVessel = room.game.shortcuts.transportVessels[i];
						if (shortCutVessel.room.index != room.abstractRoom.index || shortCutVessel.creature != foundPlayer)
						{
							continue;
						}
						int num = -1;
						float num2 = -1f;
						for (int j = 0; j < shortCutVessel.room.nodes.Length; j++)
						{
							if (shortCutVessel.room.nodes[j].type == AbstractRoomNode.Type.Exit)
							{
								float num3 = shortCutVessel.room.realizedRoom.ShortcutLeadingToNode(j).StartTile.FloatDist(shortCutVessel.pos);
								if (num2 == -1f || num3 < num2)
								{
									num = j;
									num2 = num3;
								}
							}
						}
						foundPlayerEntryNode = num;
					}
				}
				if (!entryPipes.Contains(foundPlayerEntryNode))
				{
					return;
				}
				if (!npcSpawned)
				{
					npc = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, room.ToWorldCoordinate(spawnPos), room.game.GetNewID());
					if (!room.world.game.rainWorld.setup.forcePup)
					{
						(npc.state as PlayerState).forceFullGrown = true;
					}
					room.abstractRoom.AddEntity(npc);
					npc.RealizeInRoom();
					npcSpawned = true;
					room.game.GetStorySession.saveState.LogOEEncounter(room.abstractRoom.name);
					npc.saveCreature = false;
					npc.destroyOnAbstraction = true;
				}
				if (!npcSpawned || npc == null)
				{
					return;
				}
				if (npc.state.dead)
				{
					npc.destroyOnAbstraction = false;
					npc.saveCreature = true;
				}
				if (useController)
				{
					if ((npc.realizedCreature as Player).controller == null)
					{
						(npc.realizedCreature as Player).controller = new NPCController(this);
					}
					(npc.abstractAI as SlugNPCAbstractAI).toldToStay = null;
				}
				else if ((npc.realizedCreature as Player).controller != null)
				{
					(npc.realizedCreature as Player).controller = null;
				}
				if (xTarget == 0f || (foundPlayer.mainBodyChunk.pos.x <= xTarget && foundPlayer.room == room))
				{
					playerZoneTriggered = true;
				}
				if (!playerZoneTriggered)
				{
					return;
				}
				waitTimer++;
				if (isWalking())
				{
					if (!useController && !(npc.abstractAI as SlugNPCAbstractAI).toldToStay.HasValue)
					{
						(npc.abstractAI as SlugNPCAbstractAI).toldToStay = room.ToWorldCoordinate(targetPos);
					}
					WhileWalking();
				}
				else
				{
					WhileWaiting();
				}
				if (npc.Room != room.abstractRoom && (npc.realizedCreature == null || !npc.realizedCreature.inShortcut))
				{
					Custom.Log("Removed NPC and cleaned event");
					npc.realizedCreature.room.RemoveObject(npc.realizedCreature);
					npc.Room.RemoveEntity(npc);
					npc.Destroy();
					npc = null;
					Destroy();
				}
			}
		}

		public virtual void WhileWaiting()
		{
			Player player = npc.realizedCreature as Player;
			if (!player.standing)
			{
				player.animation = Player.AnimationIndex.StandUp;
				player.standing = true;
			}
		}

		public virtual void WhileWalking()
		{
		}

		public virtual Player.InputPackage ControllerInput()
		{
			return default(Player.InputPackage);
		}

		public bool isWalking()
		{
			return waitTimer >= waitTillWalk;
		}
	}

	public class OE_TREETOP : OE_NPCControl
	{
		public OE_TREETOP(Room room)
			: base(room)
		{
			spawnPos = new Vector2(580f, 330f);
			targetPos = new Vector2(430f, 110f);
			waitTillWalk = 120;
			useController = true;
			entryPipes.Add(0);
		}

		public override void Update(bool eu)
		{
			if (WarpCondition())
			{
				Player player = npc.realizedCreature as Player;
				if (player.Consious && !player.inShortcut && player.room == room && waitTimer % 20 == 0)
				{
					for (int i = 0; i < 2; i++)
					{
						player.bodyChunks[i].HardSetPosition(new Vector2(430f, 130f));
					}
				}
			}
			base.Update(eu);
		}

		public override Player.InputPackage ControllerInput()
		{
			if (WarpCondition())
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, -1, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (isWalking())
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, (!((npc.realizedCreature as Player).mainBodyChunk.pos.x > targetPos.x)) ? 1 : (-1), 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (waitTimer % 40 < 10)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 1, jmp: true, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			return base.ControllerInput();
		}

		public override void WhileWalking()
		{
			base.WhileWalking();
			useController = Math.Abs((npc.realizedCreature as Player).mainBodyChunk.pos.x - targetPos.x) > 20f || WarpCondition();
		}

		public bool WarpCondition()
		{
			if (isWalking())
			{
				return (npc.realizedCreature as Player).mainBodyChunk.pos.y <= 190f;
			}
			return false;
		}
	}

	public class OE_BACKFILTER : OE_NPCControl
	{
		public OE_BACKFILTER(Room room)
			: base(room)
		{
			spawnPos = new Vector2(350f, 450f);
			targetPos = new Vector2(90f, 560f);
			waitTillWalk = 30;
			entryPipes.Add(1);
			entryPipes.Add(2);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}
	}

	public class OE_WORMPIT : OE_NPCControl
	{
		public OE_WORMPIT(Room room)
			: base(room)
		{
			spawnPos = new Vector2(340f, 640f);
			targetPos = new Vector2(100f, 710f);
			waitTillWalk = 0;
			entryPipes.Add(1);
			entryPipes.Add(2);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}
	}

	public class OE_FINAL01 : OE_NPCControl
	{
		public OE_FINAL01(Room room)
			: base(room)
		{
			spawnPos = new Vector2(375f, 540f);
			targetPos = new Vector2(60f, 310f);
			waitTillWalk = 5;
			useController = true;
			entryPipes.Add(3);
			xTarget = 940f;
		}

		public override void Update(bool eu)
		{
			if (isWalking() && ((foundPlayer.mainBodyChunk.pos.x < 420f && foundPlayer.mainBodyChunk.pos.y < 420f) || (npc.realizedCreature as Player).mainBodyChunk.pos.x < 120f))
			{
				Player player = npc.realizedCreature as Player;
				if (player.Consious && !player.inShortcut && player.room == room && waitTimer % 20 == 0)
				{
					for (int i = 0; i < 2; i++)
					{
						player.bodyChunks[i].HardSetPosition(new Vector2(90f, 310f));
					}
				}
			}
			base.Update(eu);
		}

		public override Player.InputPackage ControllerInput()
		{
			Player player = npc.realizedCreature as Player;
			if (!isWalking())
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, (!(player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam) || !(player.mainBodyChunk.pos.y >= 480f)) ? 1 : 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			if (player.mainBodyChunk.pos.y < 340f && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, -1, (!player.standing) ? 1 : 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, (waitTimer % 20 >= 5) ? (-1) : 0, waitTimer % 20 < 5, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public class OE_CAVE10 : OE_NPCControl
	{
		public OE_CAVE10(Room room)
			: base(room)
		{
			spawnPos = new Vector2(1310f, 310f);
			targetPos = new Vector2(150f, 330f);
			waitTillWalk = 40;
			entryPipes.Add(0);
			entryPipes.Add(2);
			useController = true;
			xTarget = 1950f;
		}

		public override Player.InputPackage ControllerInput()
		{
			if (isWalking())
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, -1, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			return base.ControllerInput();
		}
	}

	public class RifleTutorial : UpdatableAndDeletable
	{
		private Player player;

		public int message;

		public bool foundVulture;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room == null)
			{
				return;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (player == null && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				player = firstAlivePlayer.realizedCreature as Player;
			}
			if (!foundVulture)
			{
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (room.physicalObjects[i][j] is Vulture)
						{
							(room.physicalObjects[i][j] as Vulture).Stun(600);
							foundVulture = true;
						}
					}
				}
			}
			if (player != null && room.game.cameras[0].hud != null && player.grasps[0] != null && player.grasps[0].grabbed is JokeRifle)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold pick-up with compatible ammunition in hand to load the rifle."), 0, 320, darken: true, hideHud: true);
				Destroy();
			}
		}
	}

	public static void AddRoomSpecificScript(Room room)
	{
		string name = room.abstractRoom.name;
		switch (name)
		{
		case "SL_ROOF03":
			Custom.Log("SL_ROOF03 script");
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.game.IsMoonActive() && !room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				Custom.Log("Rivulet epilogue time filter");
				if (room.game.world.rainCycle.RainApproaching < 1f)
				{
					Custom.LogWarning("failed, forcing rain");
					room.game.world.rainCycle.ArenaEndSessionRain();
				}
				else
				{
					Custom.Log("ZA WARUDO");
					room.game.rivuletEpilogueRainPause = true;
				}
			}
			break;
		case "GW_A24":
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "GW_A24")
			{
				room.AddObject(new CutsceneArtificer(room));
			}
			break;
		case "GW_A25":
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !room.game.GetStorySession.saveState.hasRobo && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "GW_A24")
			{
				room.AddObject(new CutsceneArtificerRobo(room));
			}
			break;
		case "GW_C05":
		case "GW_EDGE02":
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				room.AddObject(new GW_C05ArtificerMessage(room));
				break;
			}
			goto default;
		default:
			if (name == "GATE_OE_SU" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
			{
				room.AddObject(new SpearmasterGateLocation(room));
			}
			else if (name == "SU_INTRO01" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				room.AddObject(new SU_SMIntroMessage(room));
			}
			else if (name == "SU_PMPSTATION01" && room.game.IsStorySession)
			{
				room.AddObject(new SU_PMPSTATION01_safety());
			}
			else if (name == "SU_CAVE02" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				room.AddObject(new SU_A42Message(room));
			}
			else if (name == "SI_A07" && room.game.IsStorySession)
			{
				if (room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					room.AddObject(new SpearmasterEnding(room));
				}
				else if (room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.world.singleRoomWorld)
				{
					room.AddObject(new SI_A07_RivEnding(room));
				}
			}
			break;
		}
		if (name == "SB_E05SAINT")
		{
			if (room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				room.AddObject(new VS_E05WrapAround(room));
			}
			room.AddObject(new RoomSpecificScript.SB_A14KarmaIncrease(room));
		}
		if (name == "SI_SAINTINTRO" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (room.world.singleRoomWorld)
			{
				room.AddObject(new SI_SAINTENDING(room));
			}
			else if (room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
			{
				room.AddObject(new SI_SAINTINTRO_tut(room));
			}
		}
		if (name == "SI_C02" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
		{
			room.AddObject(new SI_C02_tut(room));
		}
		if (name == "SH_E01" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "SH_E01")
		{
			room.AddObject(new InvSpawnLocation(room));
			return;
		}
		if (name == "RM_CORE" && room.game.IsStorySession)
		{
			room.AddObject(new RM_CORE_EnergyCell(room));
		}
		if (name == "HR_LAYERS_OF_REALITY")
		{
			room.AddObject(new InterlinkControl(room));
		}
		if (name == "OE_PUMP01" && room.game.IsStorySession)
		{
			room.AddObject(new OE_PUMP01_pusher());
		}
		if (name == "OE_CAVE03" && room.game.IsStorySession)
		{
			room.AddObject(new OE_CAVE03_warp());
		}
		if (name == "DM_ROOF04")
		{
			room.AddObject(new randomGodsSoundSource(0.45f, new Vector2(460f, 70f), 3000f, room));
		}
		if (name == "DM_ROOF03")
		{
			room.AddObject(new DM_ROOF03GradientGravity(room));
		}
		if (name == "DM_C13")
		{
			room.AddObject(new randomGodsSoundSource(0.45f, new Vector2(0f, 380f), 2800f, room));
		}
		if (name == "DS_RIVSTART" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
		{
			room.AddObject(new DS_RIVSTARTcutscene(room));
		}
		if (name == "SH_GOR02" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
		{
			room.AddObject(new SH_GOR02(room));
		}
		if (name == "SL_AI" && room.game.IsStorySession && room.game.GetStorySession.saveState.denPosition == "SL_AI" && room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			room.AddObject(new SL_AI_Behavior(room));
		}
		if (name == "MS_CORE" && room.game.IsStorySession)
		{
			room.AddObject(new MS_CORESTARTUPHEART(room));
		}
		if (name == "MS_HEART" && room.game.IsStorySession)
		{
			room.AddObject(new MS_HEARTWARP(room));
		}
		if (name == "MS_bitterstart" && room.game.IsStorySession)
		{
			room.AddObject(new MS_bitterstart(room));
		}
		if (name == "MS_COMMS" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.world.singleRoomWorld)
		{
			room.AddObject(new MS_COMMS_RivEnding(room));
		}
		if (name == "GW_E02" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.world.singleRoomWorld)
		{
			room.AddObject(new GW_E02_RivEnding(room));
		}
		if (name == "GW_E02_PAST" && room.game.IsStorySession && room.game.wasAnArtificerDream)
		{
			room.AddObject(new ArtificerDream_1(room));
		}
		if (name == "GW_TOWER06" && room.game.IsStorySession && room.game.wasAnArtificerDream)
		{
			room.AddObject(new ArtificerDream_2(room));
		}
		if (name == "GW_ARTYSCENES" && room.game.IsStorySession && room.game.wasAnArtificerDream)
		{
			if (room.game.manager.artificerDreamNumber == 2)
			{
				room.AddObject(new ArtificerDream_3(room));
			}
			else if (room.game.manager.artificerDreamNumber == 3)
			{
				room.AddObject(new ArtificerDream_4(room));
			}
			else if (room.game.manager.artificerDreamNumber == 4)
			{
				room.AddObject(new ArtificerDream_5(room));
			}
		}
		if (name == "GW_ARTYNIGHTMARE" && room.game.IsStorySession && room.game.wasAnArtificerDream)
		{
			room.AddObject(new ArtificerDream_6(room));
		}
		if (name == "GW_EDGE03" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && room.abstractRoom.firstTimeRealized)
		{
			room.AddObject(new GW_EDGE03_SCAVTUT(room));
		}
		if (name == "GW_TOWER01" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && room.abstractRoom.firstTimeRealized)
		{
			room.AddObject(new GW_TOWER01_SCAVTUT(room));
		}
		if (name == "GW_PIPE02" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && room.abstractRoom.firstTimeRealized)
		{
			room.AddObject(new GW_PIPE02_SCAVTUT(room));
		}
		if (name == "OE_FINAL03" && room.game.IsStorySession)
		{
			room.AddObject(new OE_GourmandEnding(room));
		}
		if (name == "LC_FINAL" && room.game.IsStorySession && ModManager.Expedition && room.game.rainWorld.ExpeditionMode && room.abstractRoom.firstTimeRealized)
		{
			room.AddObject(new LC_FINAL_Expedition(room));
		}
		if (name == "LC_FINAL" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			room.AddObject(new LC_FINAL(room));
		}
		if (name == "HR_C01" && room.game.GetStorySession.saveState.denPosition == "HR_C01" && room.abstractRoom.firstTimeRealized)
		{
			room.AddObject(new HR_C01RegionIntro(room));
		}
		if (name == "OE_TREETOP" && room.game.IsStorySession)
		{
			room.AddObject(new OE_TREETOP(room));
		}
		if (name == "OE_BACKFILTER" && room.game.IsStorySession)
		{
			room.AddObject(new OE_BACKFILTER(room));
		}
		if (name == "OE_WORMPIT" && room.game.IsStorySession)
		{
			room.AddObject(new OE_WORMPIT(room));
		}
		if (name == "OE_FINAL01" && room.game.IsStorySession)
		{
			room.AddObject(new OE_FINAL01(room));
		}
		if (name == "OE_CAVE10" && room.game.IsStorySession)
		{
			room.AddObject(new OE_CAVE10(room));
		}
		if (name == "Rock Bottom")
		{
			room.AddObject(new RifleTutorial());
		}
	}

	public static void RoomWarp(Player player, Room currentRoom, string newRoomName, WorldCoordinate specificCoord = default(WorldCoordinate), bool releaseGrasps = true)
	{
		if (releaseGrasps)
		{
			if (player.grasps[0] != null)
			{
				player.ReleaseGrasp(0);
			}
			if (player.grasps[1] != null)
			{
				player.ReleaseGrasp(1);
			}
			List<AbstractPhysicalObject> allConnectedObjects = player.abstractCreature.GetAllConnectedObjects();
			for (int i = 0; i < allConnectedObjects.Count; i++)
			{
				if (allConnectedObjects[i].realizedObject != null)
				{
					currentRoom.RemoveObject(allConnectedObjects[i].realizedObject);
				}
			}
		}
		currentRoom.RemoveObject(player);
		AbstractRoom abstractRoom = currentRoom.world.GetAbstractRoom(newRoomName);
		if (abstractRoom.realizedRoom == null)
		{
			currentRoom.game.world.ActivateRoom(abstractRoom);
		}
		if (specificCoord == default(WorldCoordinate))
		{
			specificCoord = new WorldCoordinate(abstractRoom.index, 0, 0, -1);
		}
		player.abstractCreature.Move(specificCoord);
		player.PlaceInRoom(abstractRoom.realizedRoom);
		if (abstractRoom.realizedRoom.game.session is StoryGameSession && abstractRoom.realizedRoom.world.region != null && !(abstractRoom.realizedRoom.game.session as StoryGameSession).saveState.regionStates[abstractRoom.realizedRoom.world.region.regionNumber].roomsVisited.Contains(abstractRoom.realizedRoom.abstractRoom.name))
		{
			(abstractRoom.realizedRoom.game.session as StoryGameSession).saveState.regionStates[abstractRoom.realizedRoom.world.region.regionNumber].roomsVisited.Add(abstractRoom.realizedRoom.abstractRoom.name);
		}
		currentRoom.game.cameras[0].virtualMicrophone.AllQuiet();
		currentRoom.game.cameras[0].MoveCamera(abstractRoom.realizedRoom, 0);
	}

	public static void HardSetStunAllObjects(Room room, Vector2 pos)
	{
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (!(room.physicalObjects[i][j] is Player))
				{
					for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						room.physicalObjects[i][j].bodyChunks[k].HardSetPosition(pos);
						room.physicalObjects[i][j].bodyChunks[k].vel.y = Mathf.Clamp(room.physicalObjects[i][j].bodyChunks[k].vel.y, -5f, 5f);
						room.physicalObjects[i][j].bodyChunks[k].vel.x = Mathf.Clamp(room.physicalObjects[i][j].bodyChunks[k].vel.x, -5f, 5f);
					}
					if (room.physicalObjects[i][j] is Creature)
					{
						(room.physicalObjects[i][j] as Creature).Stun(40);
					}
				}
			}
		}
	}
}
