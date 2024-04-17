using HUD;
using JollyCoop;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class BreathMeter : HudPart
{
	private AbstractCreature sessionPlayer;

	private FContainer myContainer;

	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public HUDCircle[] circles;

	public FLabel playerName;

	public Player hudPlayer
	{
		get
		{
			if (sessionPlayer == null)
			{
				return hud.owner as Player;
			}
			return sessionPlayer.realizedCreature as Player;
		}
	}

	private bool Show
	{
		get
		{
			if (hudPlayer != null && !hudPlayer.abstractCreature.world.game.GameOverModeActive && !hudPlayer.dead)
			{
				return hudPlayer.airInLungs < 1f;
			}
			return false;
		}
	}

	public BreathMeter(global::HUD.HUD hud, FContainer fContainer)
		: base(hud)
	{
		circles = new HUDCircle[5];
		pos = new Vector2(hud.rainWorld.options.ScreenSize.x / 2f - (float)circles.Length * 21.6f / 2f, 40f);
		lastPos = pos;
		fade = 0f;
		lastFade = 0f;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
			circles[i].fade = 0f;
			circles[i].lastFade = 0f;
		}
		myContainer = fContainer;
	}

	public BreathMeter(global::HUD.HUD hud, FContainer fContainer, AbstractCreature player)
		: this(hud, fContainer)
	{
		sessionPlayer = player;
	}

	public override void Update()
	{
		float num = 0f;
		if (hudPlayer != null && ModManager.MSC && hudPlayer.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			num = Player.PyroDeathThreshold(hudPlayer.abstractCreature.world.game);
		}
		float a = 1f - num;
		a = Mathf.Min(a, (1f - hudPlayer?.rainDeath).GetValueOrDefault());
		if (a == 0f)
		{
			a = 0.01f;
		}
		float b = ((hudPlayer == null) ? 1f : ((hudPlayer.airInLungs - num) / a));
		b = Mathf.Max(0f, Mathf.Min(1f, b));
		if (b >= 0.95f || b <= 0f)
		{
			fade = Mathf.Lerp(fade, 0f, 0.2f);
		}
		else
		{
			fade = Mathf.Lerp(fade, Show ? 1f : 0f, 0.2f);
		}
		lastPos = pos;
		lastFade = fade;
		if (hudPlayer != null)
		{
			pos.y = 40f + (float)hudPlayer.playerState.playerNumber * 21.6f;
		}
		if (ModManager.CoopAvailable && Custom.rainWorld.options.JollyPlayerCount > 1)
		{
			int playerNumber = hudPlayer?.playerState.playerNumber ?? 0;
			if (playerName == null)
			{
				playerName = new FLabel(Custom.GetFont(), JollyCustom.GetPlayerName(playerNumber));
				myContainer.AddChild(playerName);
				playerName.alignment = FLabelAlignment.Left;
				playerName.x = pos.x + (float)(circles.Length + 1) * 21.6f / 2f + 10f + playerName.textRect.width / 2f;
				playerName.y = pos.y;
			}
		}
		if (playerName != null)
		{
			playerName.alpha = fade;
		}
		float num2 = 1f / (float)circles.Length;
		float num3 = Mathf.InverseLerp(0f, 1.1f, b);
		for (int i = 0; i < circles.Length; i++)
		{
			float value = num3 - num2 * (float)i;
			circles[i].Update();
			circles[i].thickness = Mathf.Lerp(6f, 1f, Mathf.InverseLerp(num2, 0f, value));
			circles[i].fade = Mathf.Lerp(circles[i].fade, Mathf.InverseLerp(num2 * (float)i, num2 * ((float)i + 1f), fade), 0.1f);
			circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
			circles[i].snapRad = 0.45f;
			circles[i].snapThickness = 0.45f;
			circles[i].rad = Mathf.Lerp(0.1f, 5f, circles[i].fade);
			circles[i].pos = pos + new Vector2((float)i * 21.6f, 0f);
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].Draw(timeStacker);
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		if (playerName != null)
		{
			playerName.RemoveFromContainer();
		}
	}
}
