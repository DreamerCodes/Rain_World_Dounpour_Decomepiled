using System;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class InfoWindow : RectangularMenuObject
{
	public RoundedRect roundedRect;

	public float lastFade;

	public float fade;

	public float labelFade;

	public float lastLabelFade;

	public bool wantToGoAway;

	private FSprite fadeSprite;

	public int fadeCounter;

	private MenuLabel label;

	public Vector2 goalSize;

	public int counter;

	public InfoWindow(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos, new Vector2(24f, 24f))
	{
		fadeSprite = new FSprite("Futile_White");
		fadeSprite.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		fadeSprite.color = Color.black;
		fadeSprite.alpha = 0f;
		Container.AddChild(fadeSprite);
		roundedRect = new RoundedRect(menu, this, default(Vector2), size, filled: true);
		subObjects.Add(roundedRect);
		string text = "";
		if ((menu as MultiplayerMenu).GetGameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive)
		{
			text = Regex.Replace(menu.Translate("Compete in a battle against each other and the elements.<LINE>In this mode points are awarded for food items consumed,<LINE>with surviving players always ranking above dead players.<LINE>New levels, items and creatures can be unlocked in the<LINE>single player campaigns."), "<LINE>", "\r\n");
		}
		else if ((menu as MultiplayerMenu).GetGameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
		{
			text = Regex.Replace(menu.Translate("Create custom scenarios using levels, items and creatures<LINE>that have been unlocked in the single player campaigns.<LINE>Defeating creatures or other actions can be set to award points,<LINE>but Sandbox is mostly about customization and fun!"), "<LINE>", "\r\n");
		}
		else if (ModManager.MSC && (menu as MultiplayerMenu).GetGameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			text = Regex.Replace(menu.Translate("Try to meet the win requirements on a series of pre-constructed arena<LINE>scenarios. More challenges will be unlocked as progression is made by<LINE>clearing the single player campaigns."), "<LINE>", "\r\n");
		}
		else if (ModManager.MSC && (menu as MultiplayerMenu).GetGameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Safari)
		{
			text = Regex.Replace(menu.Translate("Revisit regions that were explored in the single player campaigns,<LINE>this time as an outside observer that watches the ecosystem<LINE>unfold, uninterrupted by the antics of the player."), "<LINE>", "\r\n");
		}
		string[] array = Regex.Split(text, "\r\n");
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num = Math.Max(num, array[i].Length);
		}
		if (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang))
		{
			num *= 2;
		}
		goalSize = new Vector2((float)num * 10.5f, (float)array.Length * 30f) + new Vector2(20f, 20f);
		label = new MenuLabel(menu, this, text, new Vector2(20f, 20f), goalSize, bigText: true);
		label.label.alignment = FLabelAlignment.Left;
		subObjects.Add(label);
	}

	public override void Update()
	{
		lastFade = fade;
		lastLabelFade = labelFade;
		counter++;
		fade = Custom.LerpAndTick(fade, wantToGoAway ? 0f : 1f, 0.05f, 0.025f);
		if (labelFade > Mathf.InverseLerp(0.8f, 1f, fade))
		{
			labelFade = Mathf.InverseLerp(0.8f, 1f, fade);
		}
		else
		{
			labelFade = Custom.LerpAndTick(labelFade, Mathf.InverseLerp(0.8f, 1f, fade) * Mathf.InverseLerp(30f, 5f, fadeCounter), 0.03f, 0.025f);
		}
		float num = Custom.SCurve(fade, 0.65f);
		size = Vector2.Lerp(new Vector2(24f, 24f), goalSize, num);
		size = Vector2.Lerp(size, new Vector2(Mathf.Sqrt(size.x * size.y), Mathf.Sqrt(size.x * size.y)), Mathf.Pow(1f - num, 1.5f));
		pos = new Vector2(24f, 24f) - size;
		roundedRect.size = size;
		roundedRect.addSize = (owner as SymbolButton).roundedRect.addSize;
		if (wantToGoAway && fade == 0f && lastFade == 0f)
		{
			owner.RemoveSubObject(this);
			RemoveSprites();
			(menu as MultiplayerMenu).infoWindow = null;
		}
		if (owner.Selected)
		{
			fadeCounter = 0;
		}
		else
		{
			fadeCounter++;
			if (fadeCounter > 30)
			{
				wantToGoAway = true;
			}
		}
		label.pos = new Vector2((0f - goalSize.x) / 2f + 20f, 0f);
		base.Update();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.65f);
		fadeSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
		fadeSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
		fadeSprite.scaleX = (DrawSize(timeStacker).x * 1.5f + 600f) / 16f;
		fadeSprite.scaleY = (DrawSize(timeStacker).y * 1.5f + 600f) / 16f;
		fadeSprite.alpha = Mathf.Pow(num, 2f) * 0.85f;
		label.label.alpha = Mathf.Min(Custom.SCurve(Mathf.Lerp(lastLabelFade, labelFade, timeStacker), 0.5f), Mathf.InverseLerp(0.8f, 1f, num));
		label.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), 0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 60f * (float)Math.PI * 2f));
		for (int i = 0; i < 9; i++)
		{
			roundedRect.sprites[i].color = Color.black;
			roundedRect.sprites[i].alpha = Mathf.Pow(num, 2f) * 0.5f;
		}
		for (int j = 9; j < 17; j++)
		{
			roundedRect.sprites[j].color = color;
			roundedRect.sprites[j].alpha = num;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		fadeSprite.RemoveFromContainer();
	}
}
