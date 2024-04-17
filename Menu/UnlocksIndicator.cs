using System.Collections.Generic;
using Expedition;
using UnityEngine;

namespace Menu;

public class UnlocksIndicator : PositionedMenuObject
{
	public bool centered;

	public bool perksUsed;

	public bool burdensUsed;

	public Color color = new Color(1f, 1f, 1f);

	public bool forceUpdate;

	public int unlocks = -1;

	public List<string> unlocksList;

	public MenuLabel perkHeading;

	public MenuLabel burdenHeading;

	public FSprite shadow;

	public List<FSprite> perkIcons;

	public List<MenuLabel> burdenLabels;

	public UnlocksIndicator(Menu menu, MenuObject owner, Vector2 pos, bool centered)
		: base(menu, owner, pos)
	{
		this.centered = centered;
		base.pos = pos;
		unlocksList = new List<string>();
		shadow = new FSprite("Futile_White");
		shadow.color = new Color(0f, 0f, 0f);
		shadow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		shadow.scaleX = ((menu is PauseMenu) ? 80f : 60f);
		shadow.scaleY = 20f;
		shadow.alpha = 0.4f;
		shadow.SetAnchor(centered ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f));
		Container.AddChild(shadow);
		perkHeading = new MenuLabel(menu, this, menu.Translate("PERKS"), default(Vector2), default(Vector2), bigText: false);
		perkHeading.label.alignment = ((!centered) ? FLabelAlignment.Left : FLabelAlignment.Center);
		subObjects.Add(perkHeading);
		burdenHeading = new MenuLabel(menu, this, menu.Translate("BURDENS"), default(Vector2), default(Vector2), bigText: false);
		burdenHeading.label.alignment = ((!centered) ? FLabelAlignment.Left : FLabelAlignment.Center);
		subObjects.Add(burdenHeading);
		perkIcons = new List<FSprite>();
		burdenLabels = new List<MenuLabel>();
		forceUpdate = true;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = 0f;
		if (!centered)
		{
			num = 250f;
		}
		shadow.x = owner.page.pos.x + pos.x + perkHeading.pos.x - num;
		shadow.y = owner.page.pos.y + pos.y + perkHeading.pos.y + 100f;
		if (unlocks != ExpeditionGame.activeUnlocks.Count || forceUpdate)
		{
			unlocks = ExpeditionGame.activeUnlocks.Count;
			forceUpdate = false;
			for (int i = 0; i < perkIcons.Count; i++)
			{
				perkIcons[i].RemoveFromContainer();
			}
			perkIcons = new List<FSprite>();
			for (int j = 0; j < burdenLabels.Count; j++)
			{
				burdenLabels[j].RemoveSprites();
				burdenLabels[j].RemoveSubObject(burdenLabels[j]);
			}
			burdenLabels = new List<MenuLabel>();
			perksUsed = false;
			burdensUsed = false;
			for (int k = 0; k < ExpeditionGame.activeUnlocks.Count; k++)
			{
				if (ExpeditionGame.activeUnlocks[k].StartsWith("unl-"))
				{
					perksUsed = true;
					FSprite fSprite = new FSprite(ExpeditionProgression.UnlockSprite(ExpeditionGame.activeUnlocks[k], alwaysShow: false));
					fSprite.color = ExpeditionProgression.UnlockColor(ExpeditionGame.activeUnlocks[k]);
					Container.AddChild(fSprite);
					perkIcons.Add(fSprite);
				}
				if (ExpeditionGame.activeUnlocks[k].StartsWith("bur-"))
				{
					burdensUsed = true;
					MenuLabel menuLabel = new MenuLabel(menu, this, ExpeditionProgression.BurdenName(ExpeditionGame.activeUnlocks[k]), default(Vector2), default(Vector2), bigText: true);
					menuLabel.label.color = ExpeditionProgression.BurdenMenuColor(ExpeditionGame.activeUnlocks[k]);
					menuLabel.label.alignment = ((!centered) ? FLabelAlignment.Left : FLabelAlignment.Center);
					burdenLabels.Add(menuLabel);
					subObjects.Add(menuLabel);
				}
			}
		}
		if (perksUsed)
		{
			perkHeading.label.color = color;
			perkHeading.label.alpha = 1f;
			perkHeading.pos = default(Vector2);
			perkHeading.lastPos = perkHeading.pos;
			if (perkIcons != null && perkIcons.Count > 0)
			{
				float num2 = 50f;
				float num3 = perkHeading.pos.x - num2 * (float)(perkIcons.Count - 1) / 2f;
				for (int l = 0; l < perkIcons.Count; l++)
				{
					if (centered)
					{
						perkIcons[l].x = owner.page.pos.x + pos.x + ((perkIcons.Count > 1) ? (num3 + num2 * (float)l) : perkHeading.pos.x);
						perkIcons[l].y = owner.page.pos.y + pos.y + (perkHeading.pos.y - 35f);
					}
					else
					{
						perkIcons[l].x = owner.page.pos.x + pos.x + (perkHeading.pos.x + 10f + num2 * (float)l);
						perkIcons[l].y = owner.page.pos.y + pos.y + (perkHeading.pos.y - 35f);
					}
				}
			}
		}
		else
		{
			perkHeading.label.alpha = 0f;
		}
		if (burdensUsed)
		{
			burdenHeading.label.color = color;
			burdenHeading.label.alpha = 1f;
			burdenHeading.pos = (perksUsed ? (perkHeading.pos + new Vector2(0f, -70f)) : default(Vector2));
			burdenHeading.lastPos = burdenHeading.pos;
			if (burdenLabels == null)
			{
				return;
			}
			float num4 = 110f;
			float num5 = burdenHeading.pos.x - num4 * (float)(burdenLabels.Count - 1) / 2f;
			for (int m = 0; m < burdenLabels.Count; m++)
			{
				if (centered)
				{
					burdenLabels[m].pos.x = ((burdenLabels.Count > 1) ? (num5 + num4 * (float)m) : burdenHeading.pos.x);
					burdenLabels[m].pos.y = burdenHeading.pos.y - 30f;
				}
				else
				{
					burdenLabels[m].pos.x = burdenHeading.pos.x + num4 * (float)m;
					burdenLabels[m].pos.y = burdenHeading.pos.y - 30f;
				}
			}
		}
		else
		{
			burdenHeading.label.alpha = 0f;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		shadow.RemoveFromContainer();
		for (int i = 0; i < perkIcons.Count; i++)
		{
			perkIcons[i].RemoveFromContainer();
		}
	}
}
