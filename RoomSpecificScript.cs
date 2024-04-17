using MoreSlugcats;
using RWCustom;
using UnityEngine;

public static class RoomSpecificScript
{
	public class SS_E08GradientGravity : UpdatableAndDeletable
	{
		public SS_E08GradientGravity(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					if (room.physicalObjects[i][j] is Player)
					{
						room.gravity = Mathf.InverseLerp(700f, room.PixelHeight - 400f, room.physicalObjects[i][j].bodyChunks[0].pos.y);
					}
				}
			}
		}
	}

	public class DeathPit : UpdatableAndDeletable
	{
		private int counter;

		public DeathPit(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			counter++;
			bool flag = true;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.firstChunk.pos.y >= 630f)
				{
					room.game.Players[i].realizedCreature.firstChunk.pos.y -= 3f;
					flag = false;
				}
			}
			if (counter > 80 && flag)
			{
				Destroy();
			}
		}
	}

	public class SU_C04StartUp : UpdatableAndDeletable
	{
		private float[] rightMost;

		private bool stoodUp;

		private bool[] pushRight;

		private int counter;

		private bool showedControls;

		private int showControlsCounter;

		public TutorialControlsPageOwner tutCntrlPgOwner;

		public SU_C04StartUp(Room room)
		{
			base.room = room;
			pushRight = new bool[4];
			rightMost = new float[4];
			for (int i = 0; i < pushRight.Length; i++)
			{
				pushRight[i] = true;
				rightMost[i] = 0f;
			}
			counter = 0;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			counter++;
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					if (room.physicalObjects[i][j] is Player)
					{
						Player player = room.physicalObjects[i][j] as Player;
						if (pushRight[player.playerState.playerNumber] && player.controller == null)
						{
							player.controller = new Player.NullController();
						}
						if (!pushRight[player.playerState.playerNumber] && player.controller != null && player.controller is Player.NullController)
						{
							player.controller = null;
						}
					}
				}
			}
			if (room.game.manager.FadeDelayInProgress)
			{
				return;
			}
			if (showControlsCounter > 0)
			{
				showControlsCounter++;
			}
			for (int k = 0; k < room.physicalObjects.Length; k++)
			{
				for (int l = 0; l < room.physicalObjects[k].Count; l++)
				{
					if (!(room.physicalObjects[k][l] is Player player2))
					{
						continue;
					}
					int playerNumber = player2.playerState.playerNumber;
					if (pushRight[playerNumber] && room.physicalObjects[k][l].firstChunk.pos.y > 550f && room.physicalObjects[k][l].firstChunk.pos.x < 350f)
					{
						if (playerNumber == 0 || (ModManager.CoopAvailable && counter > 5 * playerNumber))
						{
							Vector2 vector = (player2.bodyChunks[0].pos + player2.bodyChunks[1].pos) / 2f;
							rightMost[playerNumber] = Mathf.Max(vector.x, rightMost[playerNumber]);
							vector.x = rightMost[playerNumber];
							player2.bodyChunks[0].HardSetPosition(vector + new Vector2(9f, 0f));
							player2.bodyChunks[1].HardSetPosition(vector + new Vector2(-5f, 0f));
						}
						if (room.physicalObjects[k][l].graphicsModule != null)
						{
							for (int m = 0; m < (room.physicalObjects[k][l].graphicsModule as PlayerGraphics).tail.Length; m++)
							{
								(room.physicalObjects[k][l].graphicsModule as PlayerGraphics).tail[m].vel.x -= 1f;
							}
						}
					}
					else
					{
						pushRight[playerNumber] = false;
					}
					if (room.physicalObjects[k][l].firstChunk.pos.y < 350f && !stoodUp)
					{
						stoodUp = true;
						(room.physicalObjects[k][l] as Player).standing = true;
						showControlsCounter = 1;
					}
					RWInput.PlayerRecentController(0);
					if (!showedControls && showControlsCounter > 100 && room.game.cameras[0].hud != null && room.game.rainWorld.options.controls[0].GetActivePreset() != Options.ControlSetup.Preset.None)
					{
						tutCntrlPgOwner = new TutorialControlsPageOwner(room.game);
						room.AddObject(tutCntrlPgOwner);
						showedControls = true;
					}
					if (tutCntrlPgOwner != null && room.physicalObjects[k][l].firstChunk.pos.x > 800f)
					{
						tutCntrlPgOwner.controlsPage.wantToContinue = true;
					}
				}
			}
		}
	}

	public class SU_A23FirstCycleMessage : UpdatableAndDeletable
	{
		public SU_A23FirstCycleMessage(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.GoExploreMessage && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null && room.game.Players[0].realizedCreature.room == room)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.GoExploreMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Go explore! There is food and shelter to be found, but beware of predators."), 20, 160, darken: true, hideHud: true);
				if (room.game.cameras[0].hud.textPrompt.subregionTracker != null)
				{
					room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
				}
				Destroy();
			}
		}
	}

	public class SL_C12JetFish : UpdatableAndDeletable
	{
		private AbstractCreature fish;

		private bool message;

		public SL_C12JetFish(Room room)
		{
			base.room = room;
			fish = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate("Jet Fish"), null, new WorldCoordinate(room.abstractRoom.index, 123, 16, -1), room.game.GetNewID());
			room.abstractRoom.AddEntity(fish);
			if (room.game.Players.Count <= 0)
			{
				return;
			}
			fish.state.socialMemory.GetOrInitiateRelationship(room.game.Players[0].ID).like = 1f;
			fish.state.socialMemory.GetOrInitiateRelationship(room.game.Players[0].ID).tempLike = 1f;
			if (ModManager.CoopAvailable && room.game.Players.Count > 1)
			{
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					fish.state.socialMemory.GetOrInitiateRelationship(room.game.Players[i].ID).like = 1f;
					fish.state.socialMemory.GetOrInitiateRelationship(room.game.Players[i].ID).tempLike = 1f;
				}
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (fish.realizedCreature == null)
			{
				return;
			}
			if (fish.realizedCreature.dead)
			{
				Destroy();
				return;
			}
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (!message && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == fish.realizedCreature.room && Custom.DistLess(firstAlivePlayer.realizedCreature.DangerPos, fish.realizedCreature.DangerPos, 200f) && room.ViewedByAnyCamera(fish.realizedCreature.DangerPos, -50f))
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("This creature is dehydrated. You can carry it back to water."), 20, 200, darken: true, hideHud: true);
				message = true;
			}
		}
	}

	public class SB_A14KarmaIncrease : UpdatableAndDeletable
	{
		private bool addKarma = true;

		private RoomSettings.RoomEffect meltEffect;

		private float effectAdd;

		private float effectInitLevel;

		public SB_A14KarmaIncrease(Room room)
		{
			base.room = room;
			for (int i = 0; i < room.roomSettings.effects.Count; i++)
			{
				if (room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.VoidMelt)
				{
					meltEffect = room.roomSettings.effects[i];
					effectInitLevel = meltEffect.amount;
					break;
				}
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < room.game.Players.Count && (ModManager.CoopAvailable || i == 0); i++)
			{
				AbstractCreature abstractCreature = room.game.Players[i];
				if (addKarma && room.game.session is StoryGameSession && abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room == room && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap == 9 && abstractCreature.realizedCreature.firstChunk.pos.x < 550f)
				{
					(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
					room.game.cameras[0].hud.karmaMeter.reinforceAnimation = 1;
					room.PlaySound(SoundID.SB_A14, 0f, 1f, 1f);
					addKarma = false;
					for (int j = 0; j < 20; j++)
					{
						room.AddObject(new MeltLights.MeltLight(1f, room.RandomPos(), room, RainWorld.GoldRGB));
					}
					effectAdd = 1f;
					break;
				}
			}
			effectAdd = Mathf.Max(0f, effectAdd - 1f / 60f);
			meltEffect.amount = Mathf.Lerp(effectInitLevel, 1f, Custom.SCurve(effectAdd, 0.6f));
		}
	}

	public class SB_D03ShortcutLock : UpdatableAndDeletable
	{
		public SB_D03ShortcutLock(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!ModManager.MMF || MMF.cfgVanillaExploits.Value || !room.game.IsStorySession || (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap >= 9)
			{
				if (room != null)
				{
					room.lockedShortcuts.Clear();
				}
				Destroy();
			}
			else
			{
				if (room == null || room.shortcutsIndex == null || base.slatedForDeletetion)
				{
					return;
				}
				room.lockedShortcuts.Clear();
				bool flag = false;
				if (ModManager.MSC)
				{
					for (int i = 0; i < room.physicalObjects.Length; i++)
					{
						for (int j = 0; j < room.physicalObjects[i].Count; j++)
						{
							if (room.physicalObjects[i][j] is Player)
							{
								Player player = room.physicalObjects[i][j] as Player;
								if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && player.grasps.Length != 0)
								{
									for (int k = 0; k < player.grasps.Length; k++)
									{
										if (player.grasps[k] != null && player.grasps[k].grabbedChunk != null && player.grasps[k].grabbedChunk.owner is Scavenger && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma + (player.grasps[k].grabbedChunk.owner as Scavenger).abstractCreature.karmicPotential >= 9)
										{
											flag = true;
										}
									}
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					return;
				}
				IntVector2? intVector = null;
				for (int l = 0; l < room.shortcutsIndex.Length; l++)
				{
					if (!(room.shortcutData(room.shortcutsIndex[l]).shortCutType != ShortcutData.Type.RoomExit) && (!intVector.HasValue || room.shortcutsIndex[l].x < intVector.Value.x))
					{
						intVector = room.shortcutsIndex[l];
					}
				}
				room.lockedShortcuts.Add(intVector.Value);
			}
		}
	}

	public class SU_A43SuperJumpOnly : UpdatableAndDeletable
	{
		private Player player;

		public int allowPassage;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (player == null)
			{
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (room.physicalObjects[i][j] is Player)
						{
							player = room.physicalObjects[i][j] as Player;
							if (player.mainBodyChunk.pos.x > 580f)
							{
								Destroy();
							}
							break;
						}
					}
				}
			}
			else
			{
				if (player.room != room)
				{
					return;
				}
				if (player.superLaunchJump >= 20)
				{
					allowPassage = 40;
				}
				if (allowPassage > 0)
				{
					allowPassage--;
					if (player.mainBodyChunk.pos.x > 480f && player.mainBodyChunk.pos.y > 420f)
					{
						Destroy();
					}
				}
				else
				{
					for (int k = 0; k < player.bodyChunks.Length; k++)
					{
						player.bodyChunks[k].pos.x -= Mathf.InverseLerp(400f, 430f, player.bodyChunks[k].pos.y) * Mathf.InverseLerp(438f, 478f, player.bodyChunks[k].pos.x) * 4f;
						player.bodyChunks[k].vel.x -= Mathf.InverseLerp(400f, 430f, player.bodyChunks[k].pos.y) * Mathf.InverseLerp(448f, 478f, player.bodyChunks[k].pos.x) * 2f;
					}
				}
			}
		}
	}

	public class LF_A03 : UpdatableAndDeletable
	{
		private Player player;

		public int message;

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (player == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null)
			{
				player = room.game.Players[0].realizedCreature as Player;
			}
			if (player != null && player.room == room && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
			{
				if (message == 0)
				{
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("You need to eat meat to sustain yourself"), 120, 200, darken: true, hideHud: true);
					message++;
					return;
				}
				if (message == 1)
				{
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Grab your prey and hold the eat button to feed"), 0, 160, darken: true, hideHud: true);
					message++;
					return;
				}
				if (message == 2)
				{
					room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Some small creatures can be eaten immediately"), 0, 160, darken: true, hideHud: true);
					message++;
					return;
				}
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Larger prey needs to be incapacitated first"), 0, 160, darken: true, hideHud: true);
				room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial++;
				Custom.Log("MEAT EAT TUTORIAL SHOWED:", room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial.ToString(), "times");
				Destroy();
			}
		}
	}

	public static void AddRoomSpecificScript(Room room)
	{
		bool flag = !ModManager.MSC || (room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.cycleNumber < 2 && room.game.GetStorySession.saveState.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Spear);
		switch (room.abstractRoom.name)
		{
		case "SS_E08":
			room.AddObject(new SS_E08GradientGravity(room));
			break;
		case "SU_C04":
			if (flag)
			{
				room.AddObject(new SU_C04StartUp(room));
			}
			break;
		case "SU_A22":
			if (room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.cycleNumber == 1)
			{
				room.AddObject(new SU_A23FirstCycleMessage(room));
			}
			break;
		case "SL_C12":
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited.Contains(room.abstractRoom.name) && (room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited.Contains("SL_A08"))
			{
				room.AddObject(new SL_C12JetFish(room));
			}
			break;
		case "SB_A14":
			room.AddObject(new SB_A14KarmaIncrease(room));
			break;
		case "SB_D03":
			room.AddObject(new SB_D03ShortcutLock(room));
			break;
		case "SU_A43":
			if (flag)
			{
				room.AddObject(new SU_A43SuperJumpOnly());
			}
			break;
		case "deathPit":
			room.AddObject(new DeathPit(room));
			break;
		case "LF_H01":
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Red && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "LF_H01")
			{
				room.AddObject(new HardmodeStart(room));
			}
			break;
		case "LF_A03":
			if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Red && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial < 2)
			{
				room.AddObject(new LF_A03());
			}
			break;
		case "SS_D02":
			if (room.game.IsStorySession && !room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked)
			{
				room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked = true;
				Custom.Log("----MEMORY FROLICK!");
			}
			break;
		}
		if (ModManager.MSC)
		{
			MSCRoomSpecificScript.AddRoomSpecificScript(room);
		}
	}
}
