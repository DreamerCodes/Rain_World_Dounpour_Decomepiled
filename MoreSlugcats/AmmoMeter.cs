using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class AmmoMeter : HudPart
{
	public Vector2 pos;

	public Vector2 lastPos;

	public FSprite ammoTypeSprite;

	public FLabel ammoLabel;

	private string lastAmmoSprite;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public PlayerSpecificMultiplayerHud multiHud;

	private JokeRifle rifleRef()
	{
		JokeRifle result = null;
		Player player = ThePlayer();
		if (player != null)
		{
			for (int i = 0; i < 2; i++)
			{
				if (player.grasps[i] != null && player.grasps[i].grabbed is JokeRifle)
				{
					result = player.grasps[i].grabbed as JokeRifle;
				}
			}
		}
		return result;
	}

	private bool Show()
	{
		JokeRifle jokeRifle = rifleRef();
		if (jokeRifle != null)
		{
			if (!hud.showKarmaFoodRain)
			{
				return jokeRifle.lastShotTime < 60;
			}
			return true;
		}
		return false;
	}

	public string AmmoSprite()
	{
		JokeRifle jokeRifle = rifleRef();
		if (jokeRifle != null)
		{
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Ash)
			{
				return "Symbol_PuffBall";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Bees)
			{
				return "Symbol_SporePlant";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Firecracker)
			{
				return "Symbol_Firecracker";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Grenade)
			{
				return "Symbol_StunBomb";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Light)
			{
				return "Symbol_FlashBomb";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Pearl)
			{
				return "Symbol_Pearl";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Void)
			{
				return "FlowerMarker";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Fruit)
			{
				return "Symbol_DangleFruit";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Noodle)
			{
				return "Kill_SmallNeedleWorm";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.FireEgg)
			{
				return "Symbol_FireEgg";
			}
			if (jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Singularity)
			{
				return "Symbol_Singularity";
			}
		}
		return "Symbol_Rock";
	}

	public int ammoLeft()
	{
		return rifleRef()?.abstractRifle.currentAmmo() ?? 0;
	}

	public override void Update()
	{
		lastPos = pos;
		if (multiHud == null)
		{
			pos = new Vector2(hud.rainWorld.options.ScreenSize.x - 40f, 40f);
		}
		else
		{
			Vector2 cornerPos = multiHud.cornerPos;
			cornerPos.x += 40 * multiHud.flip;
			if (cornerPos.y < 100f)
			{
				cornerPos.y += 40f;
			}
			else
			{
				cornerPos.y -= 40f;
			}
			pos = cornerPos;
		}
		JokeRifle jokeRifle = rifleRef();
		string text = AmmoSprite();
		if (text != lastAmmoSprite)
		{
			ammoTypeSprite.SetElementByName(text);
			lastAmmoSprite = text;
		}
		if (jokeRifle != null && jokeRifle.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Pearl)
		{
			ammoLabel.text = (10 + (int)(Random.value * 85f)).ToString();
		}
		else
		{
			ammoLabel.text = ammoLeft().ToString();
		}
		lastFade = fade;
		if (remainVisibleCounter > 0)
		{
			remainVisibleCounter--;
		}
		if (Show() || remainVisibleCounter > 0)
		{
			if (multiHud == null)
			{
				fade = Mathf.Max(Mathf.Min(1f, fade + 0.1f), hud.foodMeter.fade);
			}
			else
			{
				fade = 1f;
			}
		}
		else
		{
			fade = Mathf.Max(0f, fade - 0.1f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		float alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), 1.5f);
		ammoTypeSprite.x = DrawPos(timeStacker).x;
		ammoTypeSprite.y = DrawPos(timeStacker).y;
		ammoTypeSprite.alpha = alpha;
		ammoLabel.alignment = FLabelAlignment.Right;
		ammoLabel.x = DrawPos(timeStacker).x - 20f;
		ammoLabel.y = DrawPos(timeStacker).y;
		ammoLabel.alpha = alpha;
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		ammoTypeSprite.RemoveFromContainer();
		ammoLabel.RemoveFromContainer();
	}

	public AmmoMeter(global::HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud, FContainer fContainer)
		: base(hud)
	{
		this.multiHud = multiHud;
		lastPos = pos;
		lastAmmoSprite = AmmoSprite();
		ammoTypeSprite = new FSprite(lastAmmoSprite);
		ammoTypeSprite.color = new Color(1f, 1f, 1f);
		fContainer.AddChild(ammoTypeSprite);
		ammoLabel = new FLabel(Custom.GetDisplayFont(), "");
		fContainer.AddChild(ammoLabel);
	}

	public Player ThePlayer()
	{
		if (multiHud != null)
		{
			return multiHud.RealizedPlayer;
		}
		return hud.owner as Player;
	}
}
