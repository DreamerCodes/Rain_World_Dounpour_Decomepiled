using System.Collections.Generic;
using HUD;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyHUD;

public class JollyMeter : HudPart
{
	public class PlayerIcon
	{
		private JollyMeter meter;

		public int playerNumber;

		private FSprite gradient;

		public float baseGradScale;

		public float baseGradAlpha;

		public FSprite iconSprite;

		public Color color;

		public Vector2 pos;

		public Vector2 lastPos;

		private float blink;

		public int blinkRed;

		private bool dead;

		private float lastBlink;

		private AbstractCreature player;

		private float rad;

		private PlayerState playerState => player.state as PlayerState;

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}

		public void ClearSprites()
		{
			gradient.RemoveFromContainer();
			iconSprite.RemoveFromContainer();
		}

		public PlayerIcon(JollyMeter meter, AbstractCreature associatedPlayer, Color color)
		{
			player = associatedPlayer;
			this.meter = meter;
			lastPos = pos;
			AddGradient(JollyCustom.ColorClamp(color, -1f, 360f, 60f));
			iconSprite = new FSprite("Kill_Slugcat");
			this.color = color;
			this.meter.fContainer.AddChild(iconSprite);
			playerNumber = playerState?.playerNumber ?? 0;
			baseGradScale = 3.75f;
			baseGradAlpha = 0.45f;
		}

		public void AddGradient(Color color)
		{
			gradient = new FSprite("Futile_White");
			gradient.shader = meter.hud.rainWorld.Shaders["FlatLight"];
			gradient.color = color;
			gradient.scale = baseGradScale;
			gradient.alpha = baseGradAlpha;
			meter.fContainer.AddChild(gradient);
		}

		public void Draw(float timeStacker)
		{
			float num = Mathf.Lerp(meter.lastFade, meter.fade, timeStacker);
			iconSprite.alpha = num;
			gradient.alpha = Mathf.SmoothStep(0f, 1f, num) * baseGradAlpha;
			iconSprite.x = DrawPos(timeStacker).x;
			iconSprite.y = DrawPos(timeStacker).y + (float)(dead ? 7 : 0);
			gradient.x = iconSprite.x;
			gradient.y = iconSprite.y;
			if (meter.counter % 6 < 2 && lastBlink > 0f)
			{
				color = Color.Lerp(color, Custom.HSL2RGB(Custom.RGB2HSL(color).x, Custom.RGB2HSL(color).y, Custom.RGB2HSL(color).z + 0.2f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker)));
			}
			iconSprite.color = color;
		}

		public void Update()
		{
			blink = Mathf.Max(0f, blink - 0.05f);
			lastBlink = blink;
			lastPos = pos;
			color = PlayerGraphics.SlugcatColor(playerState.slugcatCharacter);
			rad = Custom.LerpAndTick(rad, Custom.LerpMap(meter.fade, 0f, 0.79f, 0.79f, 1f, 1.3f), 0.12f, 0.1f);
			if (blinkRed > 0)
			{
				blinkRed--;
				rad *= Mathf.SmoothStep(1.1f, 0.85f, (float)(meter.counter % 20) / 20f);
				color = Color.Lerp(color, JollyCustom.GenerateClippedInverseColor(color), rad / 4f);
			}
			iconSprite.scale = rad;
			gradient.scale = baseGradScale * rad;
			if (playerState.permaDead || playerState.dead)
			{
				color = Color.gray;
				if (!dead)
				{
					iconSprite.RemoveFromContainer();
					iconSprite = new FSprite("Multiplayer_Death");
					iconSprite.scale *= 0.8f;
					meter.fContainer.AddChild(iconSprite);
					dead = true;
					meter.customFade = 5f;
					blink = 3f;
					gradient.color = Color.Lerp(Color.red, Color.black, 0.5f);
				}
			}
		}
	}

	private Vector2 meterPos;

	private Vector2 meterLastPos;

	public Dictionary<int, PlayerIcon> playerIcons;

	private float fade;

	private float lastFade;

	public float customFade;

	public const float IconDistance = 30f;

	private int iconOffsetIndex;

	private int counter;

	private bool cutscene;

	private FContainer fContainer;

	private FSprite cameraArrowSprite;

	private PlayerState playerStateFocusedByCamera;

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(meterLastPos, meterPos, timeStacker);
	}

	public JollyMeter(global::HUD.HUD hud, FContainer fContainer)
		: base(hud)
	{
		meterPos = new Vector2(Custom.rainWorld.options.ScreenSize.x - 90f, 100f);
		meterLastPos = meterPos;
		List<AbstractCreature> players = (base.hud.owner as Player).abstractCreature.world.game.session.Players;
		playerIcons = new Dictionary<int, PlayerIcon>();
		base.hud = hud;
		this.fContainer = fContainer;
		for (int i = 0; i < players.Count; i++)
		{
			Color color = PlayerGraphics.SlugcatColor((players[i].state as PlayerState).slugcatCharacter);
			PlayerIcon value = new PlayerIcon(this, players[i], color);
			playerIcons.Add(i, value);
		}
		fade = 0f;
		lastFade = 0f;
		customFade = 0f;
		cameraArrowSprite = new FSprite("Multiplayer_Arrow");
		fContainer.AddChild(cameraArrowSprite);
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		foreach (PlayerIcon value in playerIcons.Values)
		{
			value.ClearSprites();
		}
		cameraArrowSprite.RemoveFromContainer();
		playerIcons = null;
		cameraArrowSprite = null;
		JollyCustom.Log("PlayerMeter: cleaning sprites");
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		foreach (PlayerIcon value in playerIcons.Values)
		{
			value.Draw(timeStacker);
		}
		int num = iconOffsetIndex;
		bool flag = playerStateFocusedByCamera?.dead ?? ((hud.rainWorld.processManager.currentMainLoop as RainWorldGame).AlivePlayers.Count == 0);
		if (playerStateFocusedByCamera != null)
		{
			iconOffsetIndex = playerStateFocusedByCamera.playerNumber;
			cameraArrowSprite.y = DrawPos(timeStacker).y + 15f + (float)(flag ? 5 : 0) + (float)(cutscene ? 5 : 0);
			cameraArrowSprite.color = PlayerGraphics.SlugcatColor(playerStateFocusedByCamera.slugcatCharacter);
		}
		if (num != iconOffsetIndex)
		{
			cameraArrowSprite.x = Mathf.Lerp(cameraArrowSprite.x, DrawPos(timeStacker).x + (float)iconOffsetIndex * 30f, timeStacker);
		}
		else
		{
			cameraArrowSprite.x = DrawPos(timeStacker).x + (float)iconOffsetIndex * 30f;
		}
		float alpha = Mathf.Lerp(lastFade, fade, timeStacker);
		cameraArrowSprite.alpha = alpha;
	}

	public override void Update()
	{
		base.Update();
		RainWorldGame rainWorldGame = hud.rainWorld.processManager.currentMainLoop as RainWorldGame;
		if (hud.foodMeter != null)
		{
			meterPos.x = rainWorldGame.rainWorld.options.ScreenSize.x - hud.foodMeter.pos.x - 75f + (float)(3 - Custom.rainWorld.options.JollyPlayerCount) * 30f;
			meterPos.y = hud.foodMeter.pos.y;
			fade = Mathf.Max(hud.foodMeter.fade, customFade);
		}
		customFade = Mathf.Max(0f, customFade - 0.05f);
		lastFade = fade;
		meterLastPos = meterPos;
		if (fade > 0f)
		{
			counter++;
		}
		else
		{
			counter = 0;
		}
		foreach (KeyValuePair<int, PlayerIcon> playerIcon in playerIcons)
		{
			playerIcon.Value.Update();
			playerIcon.Value.pos = meterPos + new Vector2((float)playerIcon.Key * 30f, 0f);
		}
		if (hud.owner is Player { room: not null } player)
		{
			AbstractCreature followAbstractCreature = player.room.game.cameras[0].followAbstractCreature;
			playerStateFocusedByCamera = ((followAbstractCreature != null) ? (followAbstractCreature.state as PlayerState) : null);
			cutscene = player.room.game.cameras[0].InCutscene;
		}
		else
		{
			playerStateFocusedByCamera = null;
		}
		cameraArrowSprite.element = Futile.atlasManager.GetElementWithName(cutscene ? "Jolly_Lock_1" : "Multiplayer_Arrow");
	}
}
