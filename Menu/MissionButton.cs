using Expedition;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MissionButton : BigSimpleButton
{
	public ExpeditionProgression.Mission mission;

	public MissionTooltip tooltip;

	public Vector2 tooltipSize;

	public FSprite completedTick;

	public bool completed;

	public MissionButton(Menu menu, MenuObject owner, string signal, string key, Vector2 pos, Vector2 size, FLabelAlignment alignment, bool bigText)
		: base(menu, owner, ExpeditionProgression.GetMissionName(key).WrapText(bigText: true, size.x), "mis-" + key, pos, size, alignment, bigText)
	{
		signalText = signal;
		foreach (ExpeditionProgression.Mission mission in ExpeditionProgression.missionList)
		{
			if (mission.key == key)
			{
				this.mission = mission;
			}
		}
		if (!ExpeditionGame.unlockedExpeditionSlugcats.Contains(new SlugcatStats.Name(this.mission.slugcat)))
		{
			buttonBehav.greyedOut = true;
		}
		float width = LabelTest.GetWidth(menuLabel.label.text, bigText: true);
		float num = size.x - 10f;
		float num2 = 1f;
		if (width > num)
		{
			num2 -= width / num - 1f;
			menuLabel.label.scaleX = num2;
		}
		Vector3 vector = Custom.RGB2HSL(Color.Lerp(PlayerGraphics.DefaultSlugcatColor(new SlugcatStats.Name(this.mission.slugcat)), new Color(0.75f, 0.75f, 0.75f), 0.2f));
		HSLColor value = new HSLColor(vector.x, vector.y, vector.z);
		rectColor = value;
		labelColor = value;
		tooltipSize = default(Vector2);
		for (int i = 0; i < this.mission.challenges.Count; i++)
		{
			if (tooltipSize.x < LabelTest.GetWidth(this.mission.challenges[i].description))
			{
				tooltipSize.x = LabelTest.GetWidth(this.mission.challenges[i].description);
			}
		}
		if (tooltipSize.x < 230f)
		{
			tooltipSize.x = 230f;
		}
		tooltipSize.x += 30f;
		tooltipSize.y = 300f + 16f * (float)this.mission.challenges.Count;
		if (this.mission.challenges.Count == 1)
		{
			tooltipSize.y += 4f;
		}
		completed = ExpeditionData.completedMissions.Contains(this.mission.key);
		if (completed)
		{
			ExpLog.Log(this.mission.key + " complete, tick added");
			completedTick = new FSprite("tick");
			completedTick.color = Color.Lerp(value.rgb, Color.white, 0.2f);
			completedTick.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
			Container.AddChild(completedTick);
		}
	}

	public override void Update()
	{
		base.Update();
		if (Selected && base.page.lastPos == base.page.pos)
		{
			if (tooltip == null)
			{
				tooltip = new MissionTooltip(menu, this, default(Vector2), tooltipSize, mission.key);
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
		if (completedTick != null)
		{
			completedTick.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + pos.x + size.x - 2f;
			completedTick.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + pos.y + size.y - 10f;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		if (completedTick != null)
		{
			completedTick.RemoveFromContainer();
		}
	}
}
