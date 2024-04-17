using System;
using Expedition;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MusicTrackButton : SelectOneButton
{
	public Color color;

	public Color trackColor;

	public Color nameColor;

	public MenuLabel trackName;

	public FSprite sprite;

	public MenuLabel newIndicator;

	public bool unlocked;

	public float leftAnchor;

	public float rightAnchor;

	public MusicTrackButton(Menu menu, MenuObject owner, string displayText, string singalText, Vector2 pos, Vector2 size, SelectOneButton[] buttonArray, int index)
		: base(menu, owner, displayText, singalText, pos, size, buttonArray, index)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		try
		{
			unlocked = ExpeditionData.unlockables.Contains(singalText) || (menu as ExpeditionJukebox).demoMode;
			buttonBehav.greyedOut = !unlocked;
		}
		catch (Exception ex)
		{
			ExpLog.Log("JUKEBOX: " + singalText.ToLower() + " was not found in the unlockedSongs Dictionary\n" + ex.ToString());
		}
		signalText = singalText;
		menuLabel.label.text = menu.Translate("Track:") + " " + (index + 1);
		menuLabel.label.alignment = FLabelAlignment.Left;
		menuLabel.pos = new Vector2(-70f, 10f);
		trackColor = new Color(1f, 1f, 1f);
		menuLabel.label.color = trackColor;
		trackName = new MenuLabel(menu, this, unlocked ? ExpeditionProgression.TrackName(displayText) : menu.Translate("LOCKED"), new Vector2(53f, 16f), default(Vector2), bigText: false);
		trackName.label.alignment = FLabelAlignment.Left;
		nameColor = new Color(1f, 1f, 1f);
		trackName.label.color = nameColor;
		subObjects.Add(trackName);
		sprite = new FSprite("Futile_White");
		sprite.SetAnchor(0.5f, 0.5f);
		sprite.x = (owner as MusicTrackContainer).pos.x + base.pos.x + 25f - leftAnchor;
		sprite.y = (owner as MusicTrackContainer).pos.y + base.pos.y + 25f;
		Container.AddChild(sprite);
		if (ExpeditionData.newSongs.Contains(singalText))
		{
			newIndicator = new MenuLabel(menu, this, menu.Translate("NEW!"), new Vector2(size.x - 13f, size.y - 18f), default(Vector2), bigText: false);
			newIndicator.label.alignment = FLabelAlignment.Right;
			newIndicator.label.shader = menu.manager.rainWorld.Shaders["MenuTextGold"];
			subObjects.Add(newIndicator);
		}
		color = new Color(0.5f, 0.5f, 0.5f);
		trackName.label.color = (unlocked ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f));
		sprite.color = (unlocked ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f));
		for (int i = 0; i < roundedRect.sprites.Length; i++)
		{
			if (i > 8 && unlocked)
			{
				roundedRect.sprites[i].shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		sprite.x = (owner as MusicTrackContainer).pos.x + pos.x + 25f - leftAnchor;
		sprite.y = (owner as MusicTrackContainer).pos.y + pos.y + 27f;
		for (int i = 0; i < 8; i++)
		{
			selectRect.sprites[i].color = MyColor(timeStacker);
			if (base.AmISelected)
			{
				selectRect.sprites[i].shader = menu.manager.rainWorld.Shaders["MenuText"];
			}
			else
			{
				selectRect.sprites[i].shader = menu.manager.rainWorld.Shaders["Basic"];
			}
		}
		if (base.AmISelected && unlocked)
		{
			if ((menu as ExpeditionJukebox).isPlaying)
			{
				sprite.rotation += 150f * Time.deltaTime;
				if (sprite.element.name != "mediadisc")
				{
					sprite.SetElementByName("mediadisc");
				}
			}
			else
			{
				sprite.rotation = 0f;
				if (sprite.element.name != "musicSymbol")
				{
					sprite.SetElementByName("musicSymbol");
				}
			}
			if (newIndicator != null)
			{
				newIndicator.RemoveSprites();
				newIndicator.RemoveSubObject(newIndicator);
			}
			sprite.alpha = 1f;
			menuLabel.pos.x = -70f;
			trackName.pos.x = 52f;
			sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
			menuLabel.label.color = trackColor;
			trackName.label.color = nameColor;
			if (newIndicator != null)
			{
				newIndicator.RemoveSprites();
				newIndicator.RemoveSubObject(newIndicator);
			}
		}
		else
		{
			sprite.alpha = 0f;
			trackName.pos.x = 12f;
			menuLabel.pos.x = -110f;
			sprite.rotation = 0f;
			sprite.shader = menu.manager.rainWorld.Shaders["Basic"];
			menuLabel.label.color = Color.Lerp(trackColor, Color.black, 0.7f);
			trackName.label.color = Color.Lerp(nameColor, Color.black, 0.7f);
		}
	}
}
