using Expedition;
using RWCustom;
using UnityEngine;

namespace Menu;

public class QuestTooltip : PositionedMenuObject
{
	public FSprite[] boxSprites;

	public FSprite shadow;

	public FSprite separator;

	public Vector2 size;

	public float alpha;

	public MenuLabel questType;

	public MenuLabel questTypeDesc;

	public MenuLabel requirementHeading;

	public MenuLabel rewardHeading;

	public MenuLabel questRequirements;

	public MenuLabel questRewards;

	public float xOffset;

	public bool posAdjusted;

	public float leftAnchor;

	public float rightAnchor;

	public ExpeditionProgression.Quest Quest => (owner as QuestButton).quest;

	public QuestTooltip(Menu menu, MenuObject owner, Vector2 pos, Vector2 size)
		: base(menu, owner, pos)
	{
		this.size = size;
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		shadow = new FSprite("Futile_White");
		shadow.color = new Color(0f, 0f, 0f);
		shadow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		shadow.scaleX = size.x / 6f;
		shadow.scaleY = size.y / 6f;
		Container.AddChild(shadow);
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
				boxSprites[i].color = (owner as QuestButton).color;
			}
		}
		separator = new FSprite("pixel");
		separator.SetAnchor(0.5f, 0f);
		separator.scaleX = 2f;
		separator.scaleY = size.y - 75f;
		separator.color = (owner as QuestButton).color;
		Container.AddChild(separator);
		questType = new MenuLabel(menu, this, (Quest.type == "expedition") ? menu.Translate("EXPEDITION") : menu.Translate("MILESTONE"), new Vector2((owner as QuestButton).size.x / 2f, (owner as QuestButton).size.y + size.y - 10f), default(Vector2), bigText: false);
		questType.label.color = (owner as QuestButton).color;
		subObjects.Add(questType);
		string text = ((Quest.type == "expedition") ? menu.Translate("Complete an Expedition with the following requirements:") : menu.Translate("Reach these milestones across multiple Expeditions:"));
		if (ExpeditionData.completedQuests.Contains(Quest.key))
		{
			text = menu.Translate("This quest has been completed");
		}
		questTypeDesc = new MenuLabel(menu, this, text, new Vector2((owner as QuestButton).size.x / 2f, (owner as QuestButton).size.y + size.y - 33f), default(Vector2), bigText: false);
		questTypeDesc.label.color = new Color(0.8f, 0.8f, 0.8f);
		subObjects.Add(questTypeDesc);
		requirementHeading = new MenuLabel(menu, this, menu.Translate("REQUIREMENTS"), new Vector2((owner as QuestButton).size.x / 2f - size.x / 4f, (owner as QuestButton).size.y + size.y - 60f), default(Vector2), bigText: false);
		requirementHeading.label.color = (owner as QuestButton).color;
		subObjects.Add(requirementHeading);
		rewardHeading = new MenuLabel(menu, this, menu.Translate("REWARDS"), new Vector2((owner as QuestButton).size.x / 2f + size.x / 4f, (owner as QuestButton).size.y + size.y - 60f), default(Vector2), bigText: false);
		rewardHeading.label.color = (owner as QuestButton).color;
		subObjects.Add(rewardHeading);
		questRequirements = new MenuLabel(menu, this, "", new Vector2((owner as QuestButton).size.x / 2f - size.x / 4f + xOffset, (owner as QuestButton).size.y + size.y * 0.38f), default(Vector2), bigText: false);
		questRequirements.label.color = new Color(0.8f, 0.8f, 0.8f);
		subObjects.Add(questRequirements);
		questRewards = new MenuLabel(menu, this, "", new Vector2((owner as QuestButton).size.x / 2f + size.x / 4f + xOffset, (owner as QuestButton).size.y + size.y * 0.38f), default(Vector2), bigText: false);
		questRewards.label.color = new Color(0.8f, 0.8f, 0.8f);
		subObjects.Add(questRewards);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < boxSprites.Length; i++)
		{
			boxSprites[i].RemoveFromContainer();
		}
		if (separator != null)
		{
			separator.RemoveFromContainer();
		}
		shadow.RemoveFromContainer();
	}

	public override void Update()
	{
		base.Update();
		if (Quest.conditions != null && Quest.conditions.Length != 0 && questRequirements.text == "")
		{
			string text = "";
			for (int i = 0; i < Quest.conditions.Length; i++)
			{
				string text2 = ExpeditionProgression.TooltipRequirementDescription(Quest.conditions[i]);
				if (text2 == null)
				{
					text2 = "INVALID REQUIREMENT";
				}
				text += text2;
				if (i < Quest.conditions.Length - 1)
				{
					text += "\n";
				}
			}
			questRequirements.text = text;
		}
		if (Quest.reward == null || Quest.reward.Length == 0 || !(questRewards.text == ""))
		{
			return;
		}
		string text3 = "";
		for (int j = 0; j < Quest.reward.Length; j++)
		{
			string text4 = ExpeditionProgression.TooltipRewardDescription(Quest.reward[j]);
			if (text4 == null)
			{
				text4 = "INVALID REWARD";
			}
			text3 += text4;
			if (j < Quest.reward.Length - 1)
			{
				text3 += "\n";
			}
		}
		questRewards.text = text3;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if ((owner as QuestButton).Selected)
		{
			alpha = 1f;
			shadow.alpha = 0.75f;
			questType.label.alpha = 1f;
			questTypeDesc.label.alpha = 1f;
			questRequirements.label.alpha = 1f;
			requirementHeading.label.alpha = 1f;
			rewardHeading.label.alpha = 1f;
			questRewards.label.alpha = 1f;
			separator.alpha = 1f;
			separator.shader = (owner as QuestButton).roundedRect.sprites[9].shader;
			for (int i = 0; i < boxSprites.Length; i++)
			{
				if (i > 0)
				{
					boxSprites[i].shader = (owner as QuestButton).roundedRect.sprites[9].shader;
				}
			}
		}
		else
		{
			alpha = 0f;
			shadow.alpha = 0f;
			questType.label.alpha = 0f;
			questTypeDesc.label.alpha = 0f;
			questRequirements.label.alpha = 0f;
			requirementHeading.label.alpha = 0f;
			rewardHeading.label.alpha = 0f;
			questRewards.label.alpha = 0f;
			separator.alpha = 0f;
			separator.shader = menu.manager.rainWorld.Shaders["Basic"];
			for (int j = 0; j < boxSprites.Length; j++)
			{
				if (j > 0)
				{
					boxSprites[j].shader = menu.manager.rainWorld.Shaders["Basic"];
				}
			}
		}
		for (int k = 0; k < boxSprites.Length; k++)
		{
			if (k == 0)
			{
				boxSprites[k].alpha = alpha * 0.75f;
			}
			else
			{
				boxSprites[k].alpha = alpha;
			}
		}
		shadow.x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f;
		shadow.y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 10f + size.y / 2f;
		separator.x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f;
		separator.y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 25f;
		boxSprites[0].x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f - size.x / 2f;
		boxSprites[0].y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 10f;
		boxSprites[1].x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f - size.x / 2f;
		boxSprites[1].y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 10f;
		boxSprites[2].x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f - size.x / 2f;
		boxSprites[2].y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 10f;
		boxSprites[3].x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f - size.x / 2f;
		boxSprites[3].y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + size.y + 10f;
		boxSprites[4].x = owner.page.pos.x + (owner as QuestButton).pos.x + xOffset + (owner as QuestButton).size.x / 2f + size.x / 2f;
		boxSprites[4].y = owner.page.pos.y + (owner as QuestButton).pos.y + (owner as QuestButton).size.y + 10f;
		questType.pos.x = (owner as QuestButton).size.x / 2f + xOffset;
		questTypeDesc.pos.x = (owner as QuestButton).size.x / 2f + xOffset;
		requirementHeading.pos.x = (owner as QuestButton).size.x / 2f - size.x / 4f + xOffset;
		questRequirements.pos.x = (owner as QuestButton).size.x / 2f - size.x / 4f + xOffset;
		rewardHeading.pos.x = (owner as QuestButton).size.x / 2f + size.x / 4f + xOffset;
		questRewards.pos.x = (owner as QuestButton).size.x / 2f + size.x / 4f + xOffset;
		if (!posAdjusted)
		{
			if ((owner as QuestButton).pos.x + (owner as QuestButton).size.x / 2f - size.x / 2f < leftAnchor)
			{
				xOffset = Mathf.Abs((owner as QuestButton).pos.x + (owner as QuestButton).size.x / 2f + (0f - size.x / 2f) - leftAnchor - 5f);
				posAdjusted = true;
			}
			else if ((owner as QuestButton).pos.x + (owner as QuestButton).size.x / 2f + size.x / 2f > rightAnchor)
			{
				xOffset = 0f - Mathf.Abs((owner as QuestButton).pos.x + (owner as QuestButton).size.x / 2f + (0f - size.x / 2f) - rightAnchor + size.x + 5f);
				posAdjusted = true;
			}
			else
			{
				posAdjusted = true;
			}
		}
	}
}
