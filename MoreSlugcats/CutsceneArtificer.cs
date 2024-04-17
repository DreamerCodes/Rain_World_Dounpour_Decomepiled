using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CutsceneArtificer : UpdatableAndDeletable
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Init = new Phase("Init", register: true);

		public static readonly Phase PlayerRun = new Phase("PlayerRun", register: true);

		public static readonly Phase EatScavenger = new Phase("EatScavenger", register: true);

		public static readonly Phase EnsureNextRoom = new Phase("EnsureNextRoom", register: true);

		public static readonly Phase End = new Phase("End", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class StartController : Player.PlayerController
	{
		private CutsceneArtificer owner;

		public StartController(CutsceneArtificer owner)
		{
			this.owner = owner;
		}

		public override Player.InputPackage GetInput()
		{
			return owner.GetInput();
		}
	}

	public bool foodMeterInit;

	public bool playerPosCorrect;

	public Phase phase;

	public StartController startController;

	public int cutsceneTimer;

	public Player player
	{
		get
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				return firstAlivePlayer.realizedCreature as Player;
			}
			return null;
		}
	}

	public CutsceneArtificer(Room room)
	{
		Custom.Log("ARTIFICER CUTSCENE START!");
		base.room = room;
		phase = Phase.Init;
		if (base.room.world.rainCycle.timer < 400)
		{
			room.game.cameras[0].followAbstractCreature = null;
			Random.InitState(100);
			int[] array = new int[8] { 13, 38, 23, 16, 9, 46, 16, 5 };
			int[] array2 = new int[8] { 19, 6, 6, 6, 29, 29, 29, 8 };
			for (int i = 0; i < array.Length; i++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, new WorldCoordinate(room.abstractRoom.index, array[i], array2[i], -1), room.game.GetNewID());
				abstractCreature.state.Die();
				room.abstractRoom.AddEntity(abstractCreature);
			}
			ToggleScavengerAccessToArtyIntro(room, ScavAccess: false);
		}
		else
		{
			ToggleScavengerAccessToArtyIntro(room, ScavAccess: true);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (player != null && player.myRobot != null)
		{
			if (room.world.rainCycle.timer < 400)
			{
				for (int i = 0; i < 2; i++)
				{
					player.bodyChunks[i].HardSetPosition(room.MiddleOfTile(19, 6));
				}
				room.game.cameras[0].followAbstractCreature = player.abstractCreature;
				player.standing = true;
				room.game.GetStorySession.saveState.deathPersistentSaveData.deathTime = 30;
				player.AddFood(4);
				room.world.rainCycle.timer = 400;
			}
			Destroy();
			return;
		}
		if (phase == Phase.Init)
		{
			if (player != null && !foodMeterInit)
			{
				if (room.game.cameras[0].hud == null)
				{
					room.game.cameras[0].FireUpSinglePlayerHUD(player);
				}
				foodMeterInit = true;
				room.game.cameras[0].hud.foodMeter.NewShowCount(player.FoodInStomach);
				room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
				room.game.cameras[0].hud.foodMeter.fade = 0f;
				room.game.cameras[0].hud.foodMeter.lastFade = 0f;
				room.game.cameras[0].followAbstractCreature = player.abstractCreature;
			}
			if (player != null && !playerPosCorrect)
			{
				for (int j = 0; j < 2; j++)
				{
					player.bodyChunks[j].HardSetPosition(room.MiddleOfTile(47, 29));
				}
				playerPosCorrect = true;
				if (player.graphicsModule != null)
				{
					player.graphicsModule.Reset();
				}
				startController = new StartController(this);
				player.controller = startController;
				player.standing = true;
			}
			if (playerPosCorrect && foodMeterInit)
			{
				phase = Phase.EatScavenger;
			}
			return;
		}
		if (phase == Phase.EatScavenger || phase == Phase.PlayerRun)
		{
			cutsceneTimer++;
			return;
		}
		if (phase == Phase.EnsureNextRoom && player != null)
		{
			if (player.room != null && player.room == room)
			{
				cutsceneTimer++;
				if (cutsceneTimer == 1 || cutsceneTimer % 120 == 0)
				{
					for (int k = 0; k < 2; k++)
					{
						player.bodyChunks[k].vel = Custom.DegToVec(Random.value * 360f) * 12f;
						player.bodyChunks[k].pos = new Vector2(988f, 452f);
						player.bodyChunks[k].lastPos = new Vector2(988f, 452f);
					}
				}
			}
			else if (player.room != null && player.room != room)
			{
				phase = Phase.End;
			}
		}
		if (phase == Phase.End)
		{
			Custom.Log("ARTIFICER CUTSCENE END!");
			if (player != null)
			{
				player.controller = null;
			}
			Destroy();
		}
	}

	public Player.InputPackage GetInput()
	{
		if (player == null)
		{
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
		int x = 0;
		int y = 0;
		bool jmp = false;
		bool pckp = false;
		bool thrw = false;
		if (phase == Phase.EatScavenger)
		{
			int[] array = new int[6] { 160, 40, 30, 200, 10, 5 };
			if (cutsceneTimer == 1)
			{
				x = -1;
			}
			int num = array[0];
			if (cutsceneTimer >= num && cutsceneTimer < num + array[1])
			{
				y = -1;
			}
			num += array[1];
			if (cutsceneTimer >= num && cutsceneTimer < num + array[2])
			{
				x = -1;
			}
			num += array[2];
			if (cutsceneTimer >= num && cutsceneTimer < num + array[3])
			{
				pckp = true;
			}
			num += array[3];
			if (cutsceneTimer >= num && cutsceneTimer < num + array[4])
			{
				thrw = true;
				if (player.mainBodyChunk.pos.x >= 910f)
				{
					y = 1;
				}
			}
			num += array[4];
			if (cutsceneTimer >= num && (cutsceneTimer < num + array[5] || player.mainBodyChunk.pos.x < 910f))
			{
				x = 1;
			}
			num += array[5];
			if (cutsceneTimer >= num + 30)
			{
				y = 1;
				cutsceneTimer = 0;
				phase = Phase.PlayerRun;
			}
		}
		else if (phase == Phase.PlayerRun)
		{
			int[] array2 = new int[9] { 7, 55, 40, 7, 70, 40, 40, 10, 60 };
			int num2 = 0;
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[0])
			{
				jmp = true;
			}
			num2 += array2[0];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[1])
			{
				if (cutsceneTimer < num2 + 2)
				{
					pckp = true;
				}
				else if (cutsceneTimer < num2 + 20)
				{
					x = -1;
					pckp = true;
					jmp = true;
				}
				else if (cutsceneTimer < num2 + array2[1] - 10)
				{
					x = -1;
					y = 1;
				}
				else
				{
					y = 1;
				}
			}
			num2 += array2[1];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[2] && cutsceneTimer >= num2 + 5)
			{
				y = 1;
			}
			num2 += array2[2];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[3])
			{
				jmp = true;
			}
			num2 += array2[3];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[4])
			{
				if (cutsceneTimer < num2 + 2)
				{
					pckp = true;
				}
				else if (cutsceneTimer < num2 + 5)
				{
					x = 1;
					pckp = true;
					jmp = true;
				}
				else
				{
					x = 1;
				}
				if (player.mainBodyChunk.pos.x > 715f)
				{
					y = 1;
				}
			}
			num2 += array2[4];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[5] && cutsceneTimer >= num2 + 5)
			{
				y = 1;
			}
			num2 += array2[5];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[6])
			{
				jmp = true;
				x = 1;
			}
			num2 += array2[6];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[7])
			{
				x = 1;
			}
			num2 += array2[7];
			if (cutsceneTimer >= num2 && cutsceneTimer < num2 + array2[8])
			{
				jmp = true;
				x = 1;
			}
			num2 += array2[8];
			if (cutsceneTimer >= num2)
			{
				cutsceneTimer = 0;
				phase = Phase.EnsureNextRoom;
			}
		}
		else if (phase == Phase.EnsureNextRoom)
		{
			x = 1;
		}
		return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, y, jmp, thrw, pckp, mp: false, crouchToggle: false);
	}

	public static void ToggleScavengerAccessToArtyIntro(Room room, bool ScavAccess)
	{
		room.world.ToggleCreatureAccessFromCutscene("GW_A24", CreatureTemplate.Type.Scavenger, ScavAccess);
		room.world.ToggleCreatureAccessFromCutscene("GW_A24", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, ScavAccess);
		room.world.ToggleCreatureAccessFromCutscene("GW_A25", CreatureTemplate.Type.Scavenger, ScavAccess);
		room.world.ToggleCreatureAccessFromCutscene("GW_A25", MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, ScavAccess);
	}
}
