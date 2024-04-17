using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using UnityEngine;

public class HardmodeStart : UpdatableAndDeletable
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Init = new Phase("Init", register: true);

		public static readonly Phase Wait = new Phase("Wait", register: true);

		public static readonly Phase PlayerRun = new Phase("PlayerRun", register: true);

		public static readonly Phase CleanUp = new Phase("CleanUp", register: true);

		public static readonly Phase End = new Phase("End", register: true);

		public static readonly Phase ResumedControl = new Phase("ResumedControl", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class StartController : Player.PlayerController
	{
		public HardmodePlayer owner;

		public StartController(HardmodePlayer owner)
		{
			this.owner = owner;
		}

		public override Player.InputPackage GetInput()
		{
			return owner.GetInput();
		}
	}

	public class HardmodePlayer
	{
		private HardmodeStart owner;

		public Phase phase;

		public int jumpCounter = -1;

		public int playerMovGiveUpCounter;

		public int playerAction;

		public int inActionCounter;

		public int delayBetweenPlayers = 55;

		public bool playerPosCorrect;

		public int playerNumber;

		public float backUpRunSpeed;

		public bool MainPlayer
		{
			get
			{
				Player player = Player;
				if (player == null)
				{
					return false;
				}
				return player.playerState.playerNumber == 0;
			}
		}

		public Player Player => (Player)(owner.room?.game.Players[playerNumber].realizedCreature);

		public HardmodePlayer(HardmodeStart owner, int playerNumber)
		{
			phase = Phase.Init;
			this.owner = owner;
			this.playerNumber = playerNumber;
		}

		public void Update()
		{
			if (phase == Phase.Init)
			{
				if (MainPlayer && !owner.room.game.cameras[0].InCutscene && Player != null)
				{
					owner.room.game.cameras[0].EnterCutsceneMode(Player.abstractCreature, RoomCamera.CameraCutsceneType.HunterStart);
				}
				if (owner.room.game.cameras[0].room == owner.room && Player != null)
				{
					if (owner.room.game.cameras[0].currentCameraPosition == 7)
					{
						owner.camPosCorrect = true;
					}
					else
					{
						owner.room.game.cameras[0].MoveCamera(7);
					}
				}
				if (!owner.mapAdded && owner.camPosCorrect && Player != null)
				{
					if (owner.room.game.cameras[0].hud == null)
					{
						owner.room.game.cameras[0].FireUpSinglePlayerHUD(Player);
					}
					else if (owner.room.game.cameras[0].hud.map != null && owner.room.game.cameras[0].hud.map.discLoaded)
					{
						owner.room.game.cameras[0].hud.map.AddDiscoveryTexture(Resources.Load("Illustrations/redsJourney") as Texture2D);
						owner.mapAdded = true;
						owner.room.game.cameras[0].hud.foodMeter.NewShowCount(Player.FoodInStomach);
						owner.room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
						owner.room.game.cameras[0].hud.foodMeter.fade = 0f;
						owner.room.game.cameras[0].hud.foodMeter.lastFade = 0f;
					}
				}
				if (Player != null)
				{
					if (!playerPosCorrect)
					{
						backUpRunSpeed = Player.slugcatStats.runspeedFac;
						Player.slugcatStats.runspeedFac = 1.2f;
						Player.SuperHardSetPosition(owner.room.MiddleOfTile(350 - playerNumber, 13));
						playerPosCorrect = true;
						if (Player.graphicsModule != null)
						{
							Player.graphicsModule.Reset();
						}
						Player.controller = new StartController(this);
						Player.standing = true;
						Player.playerState.foodInStomach = 5;
						if (MainPlayer)
						{
							Player.objectInStomach = new DataPearl.AbstractDataPearl(owner.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(owner.room.abstractRoom.index, -1, -1, 0), owner.room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Red_stomach);
						}
					}
					if (MainPlayer && owner.nshSwarmer != null && owner.nshSwarmer.realizedObject != null)
					{
						owner.nshSwarmer.realizedObject.firstChunk.HardSetPosition(Player.mainBodyChunk.pos + new Vector2(-30f, 0f));
						Player.SlugcatGrab(owner.nshSwarmer.realizedObject, 0);
						if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && AbstractPhysicalObject.UsesAPersistantTracker(owner.nshSwarmer))
						{
							(owner.room.game.session as StoryGameSession).AddNewPersistentTracker(owner.nshSwarmer);
						}
						owner.nshSwarmer = null;
					}
					if (MainPlayer && owner.spear != null && owner.spear.realizedObject != null)
					{
						if (Player.spearOnBack != null)
						{
							owner.spear.realizedObject.firstChunk.HardSetPosition(Player.mainBodyChunk.pos + new Vector2(-30f, 0f));
							Player.spearOnBack.SpearToBack(owner.spear.realizedObject as Spear);
						}
						owner.spear = null;
					}
				}
				if (owner.camPosCorrect && playerPosCorrect && owner.nshSwarmer == null && owner.spear == null && owner.mapAdded)
				{
					phase = Phase.Wait;
				}
			}
			else if (phase == Phase.Wait)
			{
				int num = 3;
				if (Player.slugcatStats.runspeedFac > 1.2f)
				{
					num = 5;
				}
				else if (Player.slugcatStats.runspeedFac < 0.7f)
				{
					num = 2;
				}
				if (MainPlayer || owner.hardmodePlayers[playerNumber - 1].playerAction == num)
				{
					phase = Phase.PlayerRun;
				}
				else
				{
					playerAction = 0;
				}
			}
			else if (phase == Phase.PlayerRun)
			{
				playerMovGiveUpCounter++;
				if (playerAction > 8 || playerMovGiveUpCounter > 400)
				{
					phase = Phase.CleanUp;
				}
			}
			else
			{
				if (!(phase == Phase.CleanUp))
				{
					return;
				}
				owner.room.game.cameras[0].ExitCutsceneMode();
				if (Player != null)
				{
					Player.controller = null;
					Player.slugcatStats.runspeedFac = backUpRunSpeed;
					if (MainPlayer)
					{
						owner.room.game.cameras[0].followAbstractCreature = Player.abstractCreature;
					}
					phase = Phase.ResumedControl;
				}
			}
		}

		public Player.InputPackage GetInput()
		{
			if (Player == null)
			{
				return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
			}
			int x = 0;
			int y = 0;
			bool jmp = false;
			inActionCounter++;
			if (jumpCounter > 0)
			{
				jmp = true;
				jumpCounter--;
			}
			switch (playerAction)
			{
			case 0:
				if (!Player.standing || Player.mainBodyChunk.pos.y < Player.bodyChunks[1].pos.y + 2f)
				{
					y = 1;
					break;
				}
				playerAction++;
				inActionCounter = 0;
				break;
			case 1:
				if (Player.mainBodyChunk.pos.x <= owner.room.MiddleOfTile(352, 0).x + 5f)
				{
					x = 1;
					break;
				}
				y = -1;
				playerAction++;
				inActionCounter = 0;
				break;
			case 2:
				if (inActionCounter > 9)
				{
					jmp = true;
				}
				if (inActionCounter > 30)
				{
					jmp = false;
					x = 1;
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 3:
				x = 1;
				if (owner.room.GetTilePosition(Player.mainBodyChunk.pos).x >= 362)
				{
					y = 1;
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 4:
				if (inActionCounter == 15)
				{
					x = -1;
				}
				else if (Player.mainBodyChunk.pos.x < owner.room.MiddleOfTile(367, 0).x + 7f)
				{
					x = 1;
				}
				if (!Player.standing || Player.mainBodyChunk.pos.y < Player.bodyChunks[1].pos.y + 2f)
				{
					y = 1;
				}
				else if (MainPlayer && owner.room.GetTilePosition(Player.mainBodyChunk.pos).x == 363)
				{
					jmp = true;
					jumpCounter = 3;
				}
				else if (Player.mainBodyChunk.pos.x >= owner.room.MiddleOfTile(367, 0).x + 7f)
				{
					y = -1;
					x = 0;
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 5:
				if (inActionCounter > 9)
				{
					jmp = true;
				}
				if (inActionCounter > 30)
				{
					jmp = false;
					x = 1;
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 6:
				x = 1;
				if (owner.room.GetTilePosition(Player.mainBodyChunk.pos).x >= 378)
				{
					y = 1;
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 7:
				x = 1;
				if (MainPlayer && Player.mainBodyChunk.pos.x > owner.room.MiddleOfTile(384, 0).x - 7f && owner.room.GetTilePosition(Player.mainBodyChunk.pos).x < 386)
				{
					jumpCounter = 4;
				}
				if (Player.mainBodyChunk.pos.x >= owner.room.MiddleOfTile(390 - (int)((float)playerNumber * 3.8f), 0).x)
				{
					playerAction++;
					inActionCounter = 0;
				}
				break;
			case 8:
				x = -1;
				if (owner.room.GetTilePosition(Player.mainBodyChunk.pos).x <= 389 - playerNumber)
				{
					playerAction++;
					inActionCounter = 0;
				}
				break;
			}
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.KeyboardSinglePlayer, x, y, jmp, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public bool camPosCorrect;

	public bool playerPosCorrect;

	public StartController startController;

	public List<Player> playerList;

	public List<HardmodePlayer> hardmodePlayers;

	public AbstractPhysicalObject nshSwarmer;

	public AbstractSpear spear;

	public bool mapAdded;

	private Phase phase;

	public HardmodeStart(Room room)
	{
		base.room = room;
		phase = Phase.Init;
		room.game.cameras[0].followAbstractCreature = null;
		nshSwarmer = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, new WorldCoordinate(room.abstractRoom.index, 350, 13, 0), room.game.GetNewID());
		room.abstractRoom.AddEntity(nshSwarmer);
		spear = new AbstractSpear(room.world, null, new WorldCoordinate(room.abstractRoom.index, 350, 13, 0), room.game.GetNewID(), explosive: false);
		room.abstractRoom.AddEntity(spear);
		AbstractCreature ent = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer), null, new WorldCoordinate(room.abstractRoom.index, 337, 20, -1), room.game.GetNewID());
		room.abstractRoom.AddEntity(ent);
		playerList = room.game.Players.Select((AbstractCreature x) => (Player)x.realizedCreature).ToList();
		if (ModManager.CoopAvailable)
		{
			hardmodePlayers = new List<HardmodePlayer>();
			for (int i = 0; i < playerList.Count; i++)
			{
				hardmodePlayers.Add(new HardmodePlayer(this, i));
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.subregionTracker != null)
		{
			room.game.cameras[0].hud.textPrompt.subregionTracker.counter = 0;
		}
		if (room.game.manager.fadeToBlack < 1f)
		{
			if (ModManager.CoopAvailable)
			{
				CoopModeUpdate();
			}
			else
			{
				SinglePlayerUpdate();
			}
		}
	}

	private void SinglePlayerUpdate()
	{
		Player player = room.game.Players[0].realizedCreature as Player;
		if (phase == Phase.Init)
		{
			if (room.game.cameras[0].room == room)
			{
				if (room.game.cameras[0].currentCameraPosition == 7)
				{
					camPosCorrect = true;
				}
				else
				{
					room.game.cameras[0].MoveCamera(7);
				}
			}
			if (!mapAdded && camPosCorrect && player != null)
			{
				if (room.game.cameras[0].hud == null)
				{
					room.game.cameras[0].FireUpSinglePlayerHUD(player);
				}
				else if (room.game.cameras[0].hud.map != null && room.game.cameras[0].hud.map.discLoaded)
				{
					room.game.cameras[0].hud.map.AddDiscoveryTexture(Resources.Load("Illustrations/redsJourney") as Texture2D);
					mapAdded = true;
					room.game.cameras[0].hud.foodMeter.NewShowCount(player.FoodInStomach);
					room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
					room.game.cameras[0].hud.foodMeter.fade = 0f;
					room.game.cameras[0].hud.foodMeter.lastFade = 0f;
				}
			}
			if (player != null)
			{
				if (!playerPosCorrect)
				{
					player.SuperHardSetPosition(room.MiddleOfTile(350, 13));
					playerPosCorrect = true;
					if (player.graphicsModule != null)
					{
						player.graphicsModule.Reset();
					}
					startController = new StartController(new HardmodePlayer(this, 0));
					player.controller = startController;
					player.standing = true;
					player.playerState.foodInStomach = 5;
					player.objectInStomach = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Red_stomach);
				}
				if (nshSwarmer != null && nshSwarmer.realizedObject != null)
				{
					nshSwarmer.realizedObject.firstChunk.HardSetPosition(player.mainBodyChunk.pos + new Vector2(-30f, 0f));
					player.SlugcatGrab(nshSwarmer.realizedObject, 0);
					nshSwarmer = null;
				}
				if (spear != null && spear.realizedObject != null)
				{
					if (player.spearOnBack != null)
					{
						spear.realizedObject.firstChunk.HardSetPosition(player.mainBodyChunk.pos + new Vector2(-30f, 0f));
						player.spearOnBack.SpearToBack(spear.realizedObject as Spear);
					}
					spear = null;
				}
			}
			if (camPosCorrect && playerPosCorrect && nshSwarmer == null && spear == null && mapAdded)
			{
				phase = Phase.PlayerRun;
			}
		}
		else if (phase == Phase.PlayerRun)
		{
			startController.owner.playerMovGiveUpCounter++;
			if (startController.owner.playerAction > 8 || startController.owner.playerMovGiveUpCounter > 400)
			{
				phase = Phase.End;
			}
		}
		else if (phase == Phase.End)
		{
			if (player != null)
			{
				player.controller = null;
				room.game.cameras[0].followAbstractCreature = player.abstractCreature;
			}
			Destroy();
		}
	}

	private void CoopModeUpdate()
	{
		bool flag = true;
		foreach (HardmodePlayer hardmodePlayer in hardmodePlayers)
		{
			hardmodePlayer.Update();
			if (hardmodePlayer.phase != Phase.ResumedControl)
			{
				flag = false;
			}
		}
		if (flag)
		{
			Destroy();
		}
	}
}
