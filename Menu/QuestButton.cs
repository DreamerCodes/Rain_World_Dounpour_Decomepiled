using System.Collections.Generic;
using Expedition;
using UnityEngine;

namespace Menu;

public class QuestButton : BigSimpleButton
{
	public string questKey;

	public Color color;

	public ExpeditionProgression.Quest quest;

	public bool questAssigned;

	public FSprite glow;

	public FSprite tick;

	public float indicate;

	public QuestTooltip tooltip;

	public QuestButton(Menu menu, MenuObject owner, string displayText, string singalText, Vector2 pos, Vector2 size, string questKey)
		: base(menu, owner, displayText, singalText, pos, size, FLabelAlignment.Center, bigText: true)
	{
		glow = new FSprite("Futile_White");
		glow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
		glow.alpha = 0f;
		glow.scale = 7f;
		Container.AddChild(glow);
		tick = new FSprite("tick");
		tick.alpha = 0f;
		Container.AddChild(tick);
		this.questKey = questKey;
		List<ExpeditionProgression.Quest> list = new List<ExpeditionProgression.Quest>();
		list.AddRange(ExpeditionProgression.questList);
		if (ExpeditionProgression.customQuests.Count > 0)
		{
			foreach (KeyValuePair<string, List<ExpeditionProgression.Quest>> customQuest in ExpeditionProgression.customQuests)
			{
				if (customQuest.Value.Count > 0)
				{
					list.AddRange(customQuest.Value);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i].key == questKey))
			{
				continue;
			}
			quest = list[i];
			questAssigned = CheckQuestValid();
			if (!questAssigned)
			{
				break;
			}
			color = quest.color;
			if (ExpeditionData.completedQuests.Contains(questKey))
			{
				menuLabel.label.text = "";
				glow.alpha = 0.1f;
				tick.alpha = 1f;
				tick.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
				for (int j = 0; j < roundedRect.sprites.Length; j++)
				{
					if (j > 8)
					{
						roundedRect.sprites[j].shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
					}
				}
			}
			bool flag = false;
			if (quest.reward != null && quest.reward.Length != 0)
			{
				for (int k = 0; k < quest.reward.Length; k++)
				{
					if (quest.reward[k].StartsWith("unl-") || quest.reward[k].StartsWith("bur-"))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				break;
			}
			for (int l = 0; l < roundedRect.sprites.Length; l++)
			{
				if (l > 8)
				{
					roundedRect.sprites[l].shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
				}
			}
			break;
		}
		buttonBehav.greyedOut = !questAssigned;
	}

	private bool CheckQuestValid()
	{
		bool flag = false;
		if (quest.conditions == null || quest.conditions.Length == 0)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < quest.conditions.Length; i++)
			{
				if (ExpeditionProgression.TooltipRequirementDescription(quest.conditions[i]) == null)
				{
					flag = true;
					break;
				}
			}
		}
		if (quest.reward == null || quest.reward.Length == 0)
		{
			flag = true;
		}
		else if (!flag)
		{
			for (int j = 0; j < quest.reward.Length; j++)
			{
				if (ExpeditionProgression.TooltipRewardDescription(quest.reward[j]) == null)
				{
					flag = true;
					break;
				}
			}
		}
		return !flag;
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (!Input.GetKey(KeyCode.Space) || !(message == signalText) || !ExpeditionData.devMode)
		{
			return;
		}
		if (!ExpeditionData.completedQuests.Contains(quest.key))
		{
			ExpeditionData.completedQuests.Add(quest.key);
			for (int i = 0; i < quest.reward.Length; i++)
			{
				if (!ExpeditionData.unlockables.Contains(quest.reward[i]))
				{
					ExpeditionData.unlockables.Add(quest.reward[i]);
				}
			}
			tick.alpha = 1f;
			menu.PlaySound(SoundID.MENU_Player_Join_Game);
			return;
		}
		ExpeditionData.completedQuests.Remove(quest.key);
		for (int j = 0; j < quest.reward.Length; j++)
		{
			if (ExpeditionData.unlockables.Contains(quest.reward[j]))
			{
				ExpeditionData.unlockables.Remove(quest.reward[j]);
			}
		}
		tick.alpha = 0f;
		menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
	}

	public override void Update()
	{
		base.Update();
		if (Selected && base.page.lastPos == base.page.pos)
		{
			if (tooltip == null)
			{
				tooltip = new QuestTooltip(menu, this, default(Vector2), new Vector2(460f, 180f));
				subObjects.Add(tooltip);
			}
		}
		else if (tooltip != null)
		{
			tooltip.RemoveSprites();
			tooltip.RemoveSubObject(tooltip);
			tooltip = null;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		indicate -= 1f * timeStacker;
		indicate = Mathf.Clamp(indicate, 0f, 1f);
		if (indicate == 0f)
		{
			color = quest.color;
		}
		glow.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + pos.x + size.x / 2f;
		glow.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + pos.y + size.y / 2f;
		glow.color = MyColor(timeStacker);
		tick.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + pos.x + size.x / 2f;
		tick.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + pos.y + size.y / 2f;
		tick.color = MyColor(timeStacker);
		menuLabel.label.color = MyColor(timeStacker);
	}

	public override Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			return Color.Lerp(color, new Color(0.05f, 0.05f, 0.05f), black);
		}
		float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		return Color.Lerp(Color.Lerp(color, Menu.MenuColor(Menu.MenuColors.White).rgb, a), new Color(0.05f, 0.05f, 0.05f), black);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		if (tick != null)
		{
			tick.RemoveFromContainer();
		}
		if (glow != null)
		{
			glow.RemoveFromContainer();
		}
		if (tooltip != null)
		{
			tooltip.RemoveSprites();
			RemoveSubObject(tooltip);
		}
	}
}
