using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MissionTooltip : PositionedMenuObject
{
	public FSprite[] boxSprites;

	public FSprite shadow;

	public Vector2 size;

	public float alpha;

	public string missionKey;

	public MenuLabel missionName;

	public FSprite slugcatSprite;

	public MenuLabel slugcatName;

	public MenuLabel challengeHeading;

	public List<MenuLabel> challengeLabels;

	public MenuLabel perkHeading;

	public List<FSprite> perkIcons;

	public MenuLabel burdenHeading;

	public List<MenuLabel> burdenLabels;

	public List<MenuLabel> warningLabels;

	public FSprite slugSeparator;

	public FSprite challengeSeparator;

	public FSprite unlockSeparator;

	public float xOffset;

	public bool posAdjusted;

	public bool missingReqs;

	public bool ongoing;

	public bool ongoingCheck;

	public float leftAnchor;

	public float rightAnchor;

	public ExpeditionProgression.Mission Mission => ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission m) => m.key == missionKey);

	public bool RequiresPerks
	{
		get
		{
			for (int i = 0; i < Mission.requirements.Count; i++)
			{
				if (Mission.requirements[i].StartsWith("unl-"))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool RequiresBurdens
	{
		get
		{
			for (int i = 0; i < Mission.requirements.Count; i++)
			{
				if (Mission.requirements[i].StartsWith("bur-"))
				{
					return true;
				}
			}
			return false;
		}
	}

	public MissionTooltip(Menu menu, MenuObject owner, Vector2 pos, Vector2 size, string key)
		: base(menu, owner, pos)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		posAdjusted = false;
		missionKey = key;
		this.size = size;
		shadow = new FSprite("Futile_White");
		shadow.color = new Color(0f, 0f, 0f);
		shadow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		shadow.scaleX = size.x / 6f;
		shadow.scaleY = size.y / 6f;
		Container.AddChild(shadow);
		Color rgb = (owner as MissionButton).labelColor.rgb;
		boxSprites = new FSprite[5];
		boxSprites[0] = new FSprite("pixel");
		boxSprites[0].SetAnchor(0f, 0f);
		boxSprites[0].scaleX = size.x;
		boxSprites[0].scaleY = size.y;
		boxSprites[0].color = new Color(0f, 0f, 0f);
		boxSprites[1] = new FSprite("pixel");
		boxSprites[1].SetAnchor(0f, 0f);
		boxSprites[1].scaleY = size.y;
		boxSprites[2] = new FSprite("pixel");
		boxSprites[2].SetAnchor(0f, 0f);
		boxSprites[2].scaleX = size.x;
		boxSprites[3] = new FSprite("pixel");
		boxSprites[3].SetAnchor(0f, 0f);
		boxSprites[3].scaleX = size.x;
		boxSprites[4] = new FSprite("pixel");
		boxSprites[4].SetAnchor(0f, 0f);
		boxSprites[4].scaleY = size.y;
		for (int i = 0; i < boxSprites.Length; i++)
		{
			Container.AddChild(boxSprites[i]);
			if (i > 0)
			{
				boxSprites[i].scaleX += 1f;
				boxSprites[i].scaleY += 1f;
				FSprite obj = boxSprites[i];
				Color color2 = (boxSprites[1].color = rgb);
				obj.color = color2;
			}
		}
		slugcatSprite = new FSprite("Kill_Slugcat");
		slugcatSprite.color = rgb;
		Container.AddChild(slugcatSprite);
		slugcatName = new MenuLabel(menu, owner, menu.Translate(SlugcatStats.getSlugcatName(new SlugcatStats.Name(Mission.slugcat))), new Vector2((owner as MissionButton).size.x / 2f, -65f), default(Vector2), bigText: false);
		slugcatName.label.color = rgb;
		subObjects.Add(slugcatName);
		slugSeparator = new FSprite("pixel");
		slugSeparator.scaleX = size.x;
		slugSeparator.color = rgb;
		Container.AddChild(slugSeparator);
		challengeHeading = new MenuLabel(menu, owner, menu.Translate("Challenge list").ToUpper(), new Vector2((owner as MissionButton).size.x / 2f, -100f), default(Vector2), bigText: false);
		challengeHeading.label.color = (owner as MissionButton).labelColor.rgb;
		subObjects.Add(challengeHeading);
		challengeLabels = new List<MenuLabel>();
		if (Mission.challenges != null)
		{
			for (int j = 0; j < Mission.challenges.Count; j++)
			{
				MenuLabel item = new MenuLabel(menu, owner, Mission.challenges[j].description, new Vector2((owner as MissionButton).size.x / 2f + xOffset, challengeHeading.pos.y - 20f - 16f * (float)j), default(Vector2), bigText: false)
				{
					label = 
					{
						color = (owner as MissionButton).labelColor.rgb
					}
				};
				subObjects.Add(item);
				challengeLabels.Add(item);
			}
		}
		perkIcons = new List<FSprite>();
		burdenLabels = new List<MenuLabel>();
		for (int k = 0; k < Mission.requirements.Count; k++)
		{
			if (Mission.requirements[k].StartsWith("unl-"))
			{
				FSprite fSprite = new FSprite(ExpeditionProgression.UnlockSprite(Mission.requirements[k], alwaysShow: false))
				{
					color = ExpeditionProgression.UnlockColor(Mission.requirements[k])
				};
				Container.AddChild(fSprite);
				perkIcons.Add(fSprite);
			}
			if (Mission.requirements[k].StartsWith("bur-"))
			{
				MenuLabel item2 = new MenuLabel(menu, owner, ExpeditionProgression.BurdenName(Mission.requirements[k]), default(Vector2), default(Vector2), bigText: true)
				{
					label = 
					{
						color = ExpeditionProgression.BurdenMenuColor(Mission.requirements[k])
					}
				};
				burdenLabels.Add(item2);
				subObjects.Add(item2);
			}
		}
		if (!RequiresPerks && !RequiresBurdens)
		{
			return;
		}
		challengeSeparator = new FSprite("pixel");
		challengeSeparator.scaleX = size.x;
		challengeSeparator.color = rgb;
		Container.AddChild(challengeSeparator);
		if (perkIcons.Count > 0)
		{
			perkHeading = new MenuLabel(menu, owner, menu.Translate("PERKS"), new Vector2((owner as MissionButton).size.x / 2f, challengeLabels.Last().pos.y - 35f), default(Vector2), bigText: false);
			perkHeading.label.color = rgb;
			subObjects.Add(perkHeading);
		}
		if (burdenLabels.Count > 0)
		{
			float num = ((perkIcons.Count > 0) ? (perkHeading.pos.y - 35f) : (challengeLabels.Last().pos.y - 35f));
			burdenHeading = new MenuLabel(menu, owner, menu.Translate("BURDENS"), new Vector2((owner as MissionButton).size.x / 2f, num), default(Vector2), bigText: false);
			burdenHeading.label.color = rgb;
			subObjects.Add(burdenHeading);
			for (int l = 0; l < burdenLabels.Count; l++)
			{
				burdenLabels[l].pos = new Vector2((owner as MissionButton).size.x / 2f, num - 25f);
			}
		}
		if (RequiresPerks && RequiresBurdens)
		{
			unlockSeparator = new FSprite("pixel");
			unlockSeparator.scaleX = size.x;
			unlockSeparator.color = rgb;
			Container.AddChild(unlockSeparator);
		}
		for (int m = 0; m < Mission.requirements.Count; m++)
		{
			if (!ExpeditionData.unlockables.Contains(Mission.requirements[m]))
			{
				missingReqs = true;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if ((owner as MissionButton).Selected)
		{
			if (ongoingCheck || warningLabels != null)
			{
				return;
			}
			ongoingCheck = true;
			ongoing = menu.manager.rainWorld.progression.IsThereASavedGame(new SlugcatStats.Name(Mission.slugcat));
			if (missingReqs || ongoing)
			{
				warningLabels = new List<MenuLabel>();
				float num = 0f;
				if (perkIcons.Count > 0)
				{
					num = perkHeading.pos.y - 55f;
				}
				if (burdenHeading != null)
				{
					num = burdenHeading.pos.y - 55f;
				}
				string s = (ongoing ? "The required character has an on-going expedition, finish or abandon it to continue" : "You are missing one or more of the mission's requirements");
				string[] array = Regex.Split(menu.Translate(s).WrapText(bigText: false, size.x - 10f), "\n");
				for (int i = 0; i < array.Length; i++)
				{
					MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2((owner as MissionButton).size.x / 2f, num - 15f * (float)i), default(Vector2), bigText: false);
					menuLabel.label.SetAnchor(0.5f, 1f);
					menuLabel.label.color = new Color(0.6f, 0.2f, 0.2f);
					subObjects.Add(menuLabel);
					warningLabels.Add(menuLabel);
				}
			}
		}
		else
		{
			ongoingCheck = false;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if ((owner as MissionButton).Selected && base.page.lastPos == base.page.pos)
		{
			alpha = 1f;
			shadow.alpha = 0.8f;
			challengeHeading.label.alpha = 1f;
			slugcatName.label.alpha = 1f;
			slugcatSprite.alpha = 1f;
			slugSeparator.alpha = 1f;
			if (perkHeading != null)
			{
				perkHeading.label.alpha = 1f;
			}
			if (burdenHeading != null)
			{
				burdenHeading.label.alpha = 1f;
			}
			if (challengeSeparator != null)
			{
				challengeSeparator.alpha = 1f;
			}
			if (unlockSeparator != null)
			{
				unlockSeparator.alpha = 1f;
			}
			for (int i = 0; i < challengeLabels.Count; i++)
			{
				challengeLabels[i].label.alpha = 1f;
			}
			for (int j = 0; j < burdenLabels.Count; j++)
			{
				burdenLabels[j].label.alpha = 1f;
			}
			for (int k = 0; k < perkIcons.Count; k++)
			{
				perkIcons[k].alpha = 1f;
			}
			if (warningLabels != null)
			{
				for (int l = 0; l < warningLabels.Count; l++)
				{
					warningLabels[l].label.alpha = 1f;
				}
			}
		}
		else
		{
			alpha = 0f;
			shadow.alpha = 0f;
			challengeHeading.label.alpha = 0f;
			slugcatName.label.alpha = 0f;
			slugcatSprite.alpha = 0f;
			slugSeparator.alpha = 0f;
			if (perkHeading != null)
			{
				perkHeading.label.alpha = 0f;
			}
			if (burdenHeading != null)
			{
				burdenHeading.label.alpha = 0f;
			}
			if (challengeSeparator != null)
			{
				challengeSeparator.alpha = 0f;
			}
			if (unlockSeparator != null)
			{
				unlockSeparator.alpha = 0f;
			}
			for (int m = 0; m < challengeLabels.Count; m++)
			{
				challengeLabels[m].label.alpha = 0f;
			}
			for (int n = 0; n < burdenLabels.Count; n++)
			{
				burdenLabels[n].label.alpha = 0f;
			}
			for (int num = 0; num < perkIcons.Count; num++)
			{
				perkIcons[num].alpha = 0f;
			}
			if (warningLabels != null)
			{
				for (int num2 = 0; num2 < warningLabels.Count; num2++)
				{
					warningLabels[num2].label.alpha = 0f;
				}
			}
			for (int num3 = 0; num3 < boxSprites.Length; num3++)
			{
				if (num3 > 0)
				{
					boxSprites[num3].shader = menu.manager.rainWorld.Shaders["Basic"];
				}
			}
		}
		for (int num4 = 0; num4 < boxSprites.Length; num4++)
		{
			if (num4 == 0)
			{
				boxSprites[num4].alpha = alpha * 0.75f;
			}
			else
			{
				boxSprites[num4].alpha = alpha;
			}
		}
		slugcatSprite.x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f;
		slugcatSprite.y = owner.page.pos.y + (owner as MissionButton).pos.y - 45f;
		slugcatName.pos.x = (owner as MissionButton).size.x / 2f + xOffset;
		slugSeparator.x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f;
		slugSeparator.y = owner.page.pos.y + (owner as MissionButton).pos.y - 80f;
		challengeHeading.pos.x = (owner as MissionButton).size.x / 2f + xOffset;
		for (int num5 = 0; num5 < challengeLabels.Count; num5++)
		{
			challengeLabels[num5].pos.x = (owner as MissionButton).size.x / 2f + xOffset;
		}
		if (perkHeading != null || burdenHeading != null)
		{
			challengeSeparator.x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f;
			challengeSeparator.y = owner.page.pos.y + (owner as MissionButton).pos.y + challengeLabels.Last().pos.y - 20f;
		}
		int num6 = ((perkIcons.Count > 5) ? 5 : perkIcons.Count);
		int num7 = 0;
		int num8 = 0;
		if (perkHeading != null)
		{
			float num9 = 50f;
			float num10 = perkHeading.pos.x - num9 * (float)(num6 - 1) / 2f;
			perkHeading.pos.x = (owner as MissionButton).size.x / 2f + xOffset;
			for (int num11 = 0; num11 < perkIcons.Count; num11++)
			{
				perkIcons[num11].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + ((perkIcons.Count > 1) ? (num10 + num9 * (float)num7) : ((owner as MissionButton).size.x / 2f));
				perkIcons[num11].y = owner.page.pos.y + (owner as MissionButton).pos.y + perkHeading.pos.y - 30f - 35f * (float)num8;
				num7++;
				if (num7 == num6)
				{
					num8++;
					num7 = 0;
				}
			}
		}
		if (burdenHeading != null)
		{
			if (perkHeading != null)
			{
				unlockSeparator.x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f;
				unlockSeparator.y = owner.page.pos.y + (owner as MissionButton).pos.y + perkHeading.pos.y - 55f - 35f * (float)num8;
			}
			float num12 = ((perkIcons.Count > 0) ? (perkHeading.pos.y - 70f - 35f * (float)num8) : (challengeLabels.Last().pos.y - 35f));
			burdenHeading.pos.x = (owner as MissionButton).size.x / 2f + xOffset;
			burdenHeading.pos.y = num12;
			int num13 = 0;
			int num14 = 0;
			for (int num15 = 0; num15 < burdenLabels.Count; num15++)
			{
				burdenLabels[num15].pos.x = (owner as MissionButton).size.x / 2f + xOffset + ((burdenLabels.Count <= 1) ? 0f : ((num13 > 0) ? (-50f) : 50f));
				burdenLabels[num15].pos.y = num12 - 30f - 30f * (float)num14;
				num13++;
				if (num13 == 2)
				{
					num13 = 0;
					num14++;
				}
			}
		}
		if (warningLabels != null && warningLabels.Count > 0)
		{
			float num16 = challengeLabels.Last().pos.y - 25f;
			if (perkIcons.Count > 0)
			{
				num16 = perkHeading.pos.y - 50f;
			}
			if (burdenHeading != null)
			{
				num16 = burdenHeading.pos.y - 50f;
			}
			for (int num17 = 0; num17 < warningLabels.Count; num17++)
			{
				warningLabels[num17].pos.x = (owner as MissionButton).size.x / 2f + xOffset + 3f;
				warningLabels[num17].pos.y = num16 - 15f * (float)num17;
			}
		}
		float f = challengeLabels.Last().pos.y - 25f;
		if (perkHeading != null && burdenHeading == null)
		{
			f = perkHeading.pos.y - 35f - 35f * (float)num8;
		}
		if (burdenHeading != null)
		{
			f = burdenLabels.Last().pos.y - 35f;
		}
		if (warningLabels != null && warningLabels.Count > 0)
		{
			f = warningLabels.Last().pos.y - 35f;
		}
		shadow.x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f;
		shadow.y = owner.page.pos.y + (owner as MissionButton).pos.y - Mathf.Abs(f) / 2f;
		shadow.scaleY = Mathf.Abs(f) / 6f;
		boxSprites[0].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f - size.x / 2f;
		boxSprites[0].y = owner.page.pos.y + (owner as MissionButton).pos.y - Mathf.Abs(f);
		boxSprites[0].scaleY = Mathf.Abs(f) - 25f;
		boxSprites[1].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f - size.x / 2f;
		boxSprites[1].y = owner.page.pos.y + (owner as MissionButton).pos.y - Mathf.Abs(f);
		boxSprites[1].scaleY = Mathf.Abs(f) - 25f;
		boxSprites[2].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f - size.x / 2f;
		boxSprites[2].y = owner.page.pos.y + (owner as MissionButton).pos.y - Mathf.Abs(f);
		boxSprites[3].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f - size.x / 2f;
		boxSprites[3].y = owner.page.pos.y + (owner as MissionButton).pos.y - 25f;
		boxSprites[4].x = owner.page.pos.x + (owner as MissionButton).pos.x + xOffset + (owner as MissionButton).size.x / 2f + size.x / 2f;
		boxSprites[4].y = owner.page.pos.y + (owner as MissionButton).pos.y - Mathf.Abs(f);
		boxSprites[4].scaleY = Mathf.Abs(f) - 25f;
		if (!posAdjusted)
		{
			if ((owner as MissionButton).pos.x + (owner as MissionButton).size.x / 2f - size.x / 2f < leftAnchor)
			{
				xOffset = Mathf.Abs((owner as MissionButton).pos.x + (owner as MissionButton).size.x / 2f + (0f - size.x / 2f) - leftAnchor - 5f);
				posAdjusted = true;
			}
			else if ((owner as MissionButton).pos.x + (owner as MissionButton).size.x / 2f + size.x / 2f > rightAnchor)
			{
				xOffset = 0f - Mathf.Abs((owner as MissionButton).pos.x + (owner as MissionButton).size.x / 2f + (0f - size.x / 2f) - rightAnchor + size.x + 5f);
				posAdjusted = true;
			}
			else
			{
				posAdjusted = true;
			}
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < boxSprites.Length; i++)
		{
			boxSprites[i].RemoveFromContainer();
		}
		if (perkIcons != null && perkIcons.Count > 0)
		{
			for (int j = 0; j < perkIcons.Count; j++)
			{
				perkIcons[j].RemoveFromContainer();
			}
		}
		if (challengeSeparator != null)
		{
			challengeSeparator.RemoveFromContainer();
		}
		if (slugSeparator != null)
		{
			slugSeparator.RemoveFromContainer();
		}
		if (unlockSeparator != null)
		{
			unlockSeparator.RemoveFromContainer();
		}
		if (slugcatSprite != null)
		{
			slugcatSprite.RemoveFromContainer();
		}
		shadow.RemoveFromContainer();
	}
}
