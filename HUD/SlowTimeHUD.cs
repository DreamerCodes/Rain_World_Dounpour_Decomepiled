using Expedition;
using RWCustom;
using UnityEngine;

namespace HUD;

public class SlowTimeHUD : HudPart
{
	private Vector2 pos;

	private Vector2 lastPos;

	private float fade;

	private float ringFade;

	private float displayFade;

	private float hideFade;

	private float rotation;

	private float lastRotation;

	public FSprite symbolSprite;

	public FSprite progressSprite;

	public FSprite backingSprite;

	public FSprite glowSprite;

	public ExpeditionGame.SlowTimeTracker tracker;

	public ExpeditionHUD expHUD;

	public FoodMeter foodMeter;

	public bool IsVisible => expHUD.fade > 0f;

	public SlowTimeHUD(ExpeditionGame.SlowTimeTracker tracker, ExpeditionHUD owner, HUD hud, float challengeHeight)
		: base(hud)
	{
		this.tracker = tracker;
		expHUD = owner;
		pos = new Vector2(40f, challengeHeight);
		symbolSprite = new FSprite("Multiplayer_Time");
		symbolSprite.x = pos.x;
		symbolSprite.y = pos.y;
		symbolSprite.scale = 1f;
		progressSprite = new FSprite("Futile_White");
		progressSprite.x = pos.x;
		progressSprite.y = pos.y;
		progressSprite.scale = 3.1f;
		progressSprite.color = new Color(0.7f, 0.7f, 0.7f);
		progressSprite.shader = hud.rainWorld.Shaders["HoldButtonCircle"];
		backingSprite = new FSprite("Futile_White");
		backingSprite.x = pos.x;
		backingSprite.y = pos.y;
		backingSprite.scale = 3.1f;
		backingSprite.color = new Color(0.15f, 0.15f, 0.15f);
		backingSprite.shader = hud.rainWorld.Shaders["HoldButtonCircle"];
		glowSprite = new FSprite("Futile_White");
		glowSprite.x = pos.x;
		glowSprite.y = pos.y;
		glowSprite.scale = 6f;
		glowSprite.shader = hud.rainWorld.Shaders["FlatLight"];
		hud.fContainers[1].AddChild(glowSprite);
		hud.fContainers[1].AddChild(backingSprite);
		hud.fContainers[1].AddChild(progressSprite);
		hud.fContainers[1].AddChild(symbolSprite);
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		if (tracker != null)
		{
			pos = Vector2.Lerp(lastPos, pos, timeStacker);
			fade = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0f, 10f, tracker.cooldown));
			if (tracker.cooldown == 0f)
			{
				if (glowSprite.alpha == 0f && ringFade == 1f)
				{
					glowSprite.alpha = 1f;
				}
				symbolSprite.rotation = 0f;
				symbolSprite.color = Color.white;
				progressSprite.color = Color.white;
				glowSprite.alpha = ringFade;
			}
			else
			{
				symbolSprite.rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
				symbolSprite.color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f), new Color(0.7f, 0.7f, 0.7f), fade);
				progressSprite.color = Color.Lerp(new Color(0.35f, 0.35f, 0.35f), new Color(0.7f, 0.7f, 0.7f), fade);
				glowSprite.alpha = 0f;
			}
			pos.x = Mathf.Lerp(55f, -60f, Custom.SCurve(Mathf.InverseLerp(0f, 5f, hideFade), 0.2f));
			if (foodMeter != null)
			{
				pos.y = foodMeter.pos.y + 80f;
			}
			progressSprite.alpha = Mathf.Lerp(0f, 1f, fade);
			progressSprite.x = DrawPos(timeStacker).x;
			progressSprite.y = DrawPos(timeStacker).y;
			symbolSprite.x = DrawPos(timeStacker).x;
			symbolSprite.y = DrawPos(timeStacker).y;
			backingSprite.x = DrawPos(timeStacker).x;
			backingSprite.y = DrawPos(timeStacker).y;
			glowSprite.x = DrawPos(timeStacker).x;
			glowSprite.y = DrawPos(timeStacker).y;
		}
		if (IsVisible)
		{
			displayFade = 0f;
			hideFade -= 0.2f * timeStacker;
		}
		hideFade = Mathf.Clamp(hideFade, 0f, 5f);
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Update()
	{
		base.Update();
		if (foodMeter == null)
		{
			for (int i = 0; i < hud.parts.Count; i++)
			{
				if (hud.parts[i] is FoodMeter)
				{
					foodMeter = hud.parts[i] as FoodMeter;
				}
			}
		}
		lastPos = pos;
		lastRotation = rotation;
		if (tracker.cooldown == 0f)
		{
			displayFade += 0.1f;
			if (displayFade >= 5f)
			{
				hideFade += 0.1f;
			}
			ringFade -= 0.25f;
			ringFade = Mathf.Clamp(ringFade, 0.2f, 1f);
		}
		else
		{
			rotation += 5f;
			displayFade = 0f;
			hideFade -= 0.2f;
			ringFade = 1f;
		}
	}
}
