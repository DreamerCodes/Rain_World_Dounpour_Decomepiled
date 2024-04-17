using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CutsceneArtificerRobo : UpdatableAndDeletable
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Init = new Phase("Init", register: true);

		public static readonly Phase PlayerRun = new Phase("PlayerRun", register: true);

		public static readonly Phase ActivateRobo = new Phase("ActivateRobo", register: true);

		public static readonly Phase End = new Phase("End", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class StartController : Player.PlayerController
	{
		private CutsceneArtificerRobo owner;

		public StartController(CutsceneArtificerRobo owner)
		{
			this.owner = owner;
		}

		public override Player.InputPackage GetInput()
		{
			return owner.GetInput();
		}
	}

	public AncientBot bot;

	public bool initController;

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

	public CutsceneArtificerRobo(Room room)
	{
		Custom.Log("ARTIFICER CUTSCENE 2 START!");
		base.room = room;
		phase = Phase.Init;
		bot = new AncientBot(new Vector2(470f, 1790f), new Color(1f, 0f, 0f), null, online: false);
		room.AddObject(bot);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (phase == Phase.Init)
		{
			if (player != null && !initController && player.controller == null)
			{
				startController = new StartController(this);
				player.controller = startController;
				bot.tiedToObject = player;
				initController = true;
			}
			if (initController)
			{
				phase = Phase.PlayerRun;
			}
		}
		else if (phase == Phase.PlayerRun || phase == Phase.ActivateRobo)
		{
			cutsceneTimer++;
		}
		else if (phase == Phase.End)
		{
			Custom.Log("ARTIFICER CUTSCENE 2 END!");
			if (player != null)
			{
				player.controller = null;
			}
			RainWorldGame.ForceSaveNewDenLocation(room.game, "GW_A24", saveWorldStates: true);
			room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold pick-up to transform rocks or spears into explosives, at the cost of food."), 20, 500, darken: true, hideHud: true);
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
		if (phase == Phase.PlayerRun)
		{
			x = 1;
			if (!player.standing && cutsceneTimer % 2 == 0)
			{
				y = 1;
			}
			if (player.mainBodyChunk.pos.x >= 420f)
			{
				phase = Phase.ActivateRobo;
				cutsceneTimer = 0;
			}
		}
		else if (phase == Phase.ActivateRobo)
		{
			if (bot.myAnimation == AncientBot.Animation.IdleOffline && cutsceneTimer >= 45)
			{
				bot.myAnimation = AncientBot.Animation.TurnOn;
			}
			if (bot.myMovement != AncientBot.FollowMode.Offline)
			{
				phase = Phase.End;
				if (room.game.IsStorySession)
				{
					(room.game.session as StoryGameSession).saveState.hasRobo = true;
				}
				player.myRobot = bot;
			}
		}
		return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, y, jmp, thrw, pckp, mp: false, crouchToggle: false);
	}
}
