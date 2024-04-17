using Expedition;
using RWCustom;
using UnityEngine;

namespace Menu;

public class PerkButton : BigSimpleButton
{
	public FSprite unlockSprite;

	public Color unlockColor;

	public string key;

	public string name = "";

	public string desc = "";

	public PerkButton(Menu menu, MenuObject owner, Vector2 pos, Vector2 size, string key)
		: base(menu, owner, "", key, pos, size, FLabelAlignment.Center, bigText: true)
	{
		this.key = key;
		signalText = key;
		unlockColor = ExpeditionProgression.UnlockColor(key);
		unlockSprite = new FSprite(ExpeditionProgression.UnlockSprite(key, alwaysShow: false));
		unlockSprite.SetAnchor(0.5f, 0.5f);
		Container.AddChild(unlockSprite);
		if (ExpeditionData.unlockables.Contains(key))
		{
			name = ExpeditionProgression.UnlockName(key);
			desc = ExpeditionProgression.UnlockDescription(key);
		}
		else
		{
			name = "? ? ?";
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		unlockSprite.x = owner.page.pos.x + pos.x + size.x * 0.5f;
		unlockSprite.y = owner.page.pos.y + pos.y + size.y * 0.5f;
		Vector3 vector = Custom.RGB2HSL(unlockColor);
		rectColor = (ExpeditionGame.activeUnlocks.Contains(key) ? new HSLColor(vector.x, vector.y, vector.z) : new HSLColor(1f, 0f, 0.2f));
		unlockSprite.color = (ExpeditionGame.activeUnlocks.Contains(key) ? unlockColor : new Color(0.3f, 0.3f, 0.3f));
		unlockSprite.shader = (ExpeditionGame.activeUnlocks.Contains(key) ? menu.manager.rainWorld.Shaders["MenuTextCustom"] : menu.manager.rainWorld.Shaders["Basic"]);
	}

	public override void Update()
	{
		base.Update();
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		unlockSprite.RemoveFromContainer();
	}
}
