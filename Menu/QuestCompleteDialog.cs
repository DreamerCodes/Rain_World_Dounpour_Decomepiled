using Expedition;
using RWCustom;
using UnityEngine;

namespace Menu;

public class QuestCompleteDialog : Dialog
{
	public string questKey;

	public float leftAnchor;

	public float rightAnchor;

	public float movementCounter;

	public bool opening;

	public bool closing;

	public float uAlpha;

	public float lastAlpha;

	public float currentAlpha;

	public float targetAlpha;

	public ExpeditionWinScreen owner;

	public FSprite[] borderSprites;

	public FSprite fillSprite;

	public FSprite separatorSprite;

	public MenuLabel title;

	public MenuLabel requirementHeading;

	public MenuLabel requirements;

	public MenuLabel rewardHeading;

	public MenuLabel rewards;

	public Color questColor;

	public QuestCompleteDialog(ProcessManager manager, ExpeditionWinScreen owner, string questKey)
		: base(manager)
	{
		this.owner = owner;
		this.questKey = questKey;
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		GetQuestColor();
		bool flag = owner.questsDisplayed % 2 == 0;
		borderSprites = new FSprite[2];
		borderSprites[0] = new FSprite("pixel");
		borderSprites[0].SetAnchor(flag ? 0f : 1f, 0.5f);
		borderSprites[0].scaleY = 5f;
		borderSprites[0].x = (flag ? 0f : rightAnchor);
		borderSprites[0].y = 550f;
		borderSprites[0].shader = manager.rainWorld.Shaders["MenuTextCustom"];
		borderSprites[0].color = questColor;
		borderSprites[1] = new FSprite("pixel");
		borderSprites[1].SetAnchor(flag ? 1f : 0f, 0.5f);
		borderSprites[1].scaleY = 5f;
		borderSprites[1].x = (flag ? rightAnchor : 0f);
		borderSprites[1].y = 218f;
		borderSprites[1].shader = manager.rainWorld.Shaders["MenuTextCustom"];
		borderSprites[1].color = questColor;
		separatorSprite = new FSprite("pixel");
		separatorSprite.SetAnchor(0.5f, 0.5f);
		separatorSprite.scaleX = 3f;
		separatorSprite.x = 683f - leftAnchor;
		separatorSprite.y = 400f;
		separatorSprite.shader = manager.rainWorld.Shaders["MenuTextCustom"];
		separatorSprite.color = questColor;
		fillSprite = new FSprite("pixel");
		fillSprite.SetAnchor(0f, 0f);
		fillSprite.scaleY = 332f;
		fillSprite.scaleX = rightAnchor;
		fillSprite.color = new Color(0f, 0f, 0f);
		pages[0].Container.AddChild(fillSprite);
		pages[0].Container.AddChild(borderSprites[0]);
		pages[0].Container.AddChild(borderSprites[1]);
		pages[0].Container.AddChild(separatorSprite);
		title = new MenuLabel(this, pages[0], Translate("Q U E S T   C O M P L E T E"), new Vector2(683f, 520f), default(Vector2), bigText: true);
		title.label.color = questColor;
		pages[0].subObjects.Add(title);
		requirementHeading = new MenuLabel(this, pages[0], Translate("REQUIREMENTS"), new Vector2(383f, 450f), default(Vector2), bigText: true);
		requirementHeading.label.color = questColor;
		pages[0].subObjects.Add(requirementHeading);
		string requirementString = GetRequirementString();
		requirements = new MenuLabel(this, pages[0], requirementString, new Vector2(383f, 430f), default(Vector2), bigText: true);
		requirements.label.SetAnchor(0.5f, 1f);
		requirements.label.color = new Color(0.8f, 0.8f, 0.8f);
		pages[0].subObjects.Add(requirements);
		rewardHeading = new MenuLabel(this, pages[0], Translate("REWARDS"), new Vector2(983f, 450f), default(Vector2), bigText: true);
		rewardHeading.label.color = questColor;
		pages[0].subObjects.Add(rewardHeading);
		string rewardString = GetRewardString();
		rewards = new MenuLabel(this, pages[0], rewardString, new Vector2(983f, 430f), default(Vector2), bigText: true);
		rewards.label.SetAnchor(0.5f, 1f);
		rewards.label.color = new Color(0.8f, 0.8f, 0.8f);
		pages[0].subObjects.Add(rewards);
		SimpleButton simpleButton = new SimpleButton(this, pages[0], Translate("DISMISS"), "CLOSE", new Vector2(633f, 250f), new Vector2(100f, 30f));
		Vector3 vector = Custom.RGB2HSL(questColor);
		HSLColor hSLColor = new HSLColor(vector.x, vector.y, vector.z);
		simpleButton.rectColor = hSLColor;
		simpleButton.labelColor = hSLColor;
		pages[0].subObjects.Add(simpleButton);
		PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump);
		targetAlpha = 1f;
		opening = true;
	}

	public string GetRewardString()
	{
		string text = "";
		foreach (ExpeditionProgression.Quest quest in ExpeditionProgression.questList)
		{
			if (!(quest.key == questKey))
			{
				continue;
			}
			for (int i = 0; i < quest.reward.Length; i++)
			{
				text += ExpeditionProgression.TooltipRewardDescription(quest.reward[i]);
				if (i < quest.reward.Length)
				{
					text += "\n";
				}
			}
		}
		return text;
	}

	public string GetRequirementString()
	{
		string text = "";
		foreach (ExpeditionProgression.Quest quest in ExpeditionProgression.questList)
		{
			if (!(quest.key == questKey))
			{
				continue;
			}
			for (int i = 0; i < quest.conditions.Length; i++)
			{
				text += ExpeditionProgression.TooltipRequirementDescription(quest.conditions[i]);
				if (i < quest.conditions.Length)
				{
					text += "\n";
				}
			}
		}
		return text;
	}

	public void GetQuestColor()
	{
		foreach (ExpeditionProgression.Quest quest in ExpeditionProgression.questList)
		{
			if (!(quest.key == questKey))
			{
				continue;
			}
			int num = quest.reward.Length - 1;
			if (num >= 0)
			{
				if (quest.reward[num].StartsWith("unl-"))
				{
					questColor = new Color(0f, 0.9f, 1f);
				}
				else if (quest.reward[num].StartsWith("bur-"))
				{
					questColor = new Color(0.9f, 0f, 0f);
				}
				else if (quest.reward[num].StartsWith("per-"))
				{
					questColor = new Color(0.15f, 0.85f, 0.15f);
				}
				else
				{
					questColor = new Color(1f, 0.75f, 0f);
				}
				return;
			}
		}
		questColor = new HSLColor(0.12f, 1f, 0.55f).rgb;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE" && !closing)
		{
			targetAlpha = 0f;
			closing = true;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (opening || closing)
		{
			uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 3f);
			darkSprite.alpha = uAlpha * 0.95f;
		}
		float num = Custom.SCurve(Mathf.InverseLerp(0f, 0.9f, darkSprite.alpha), 0.3f);
		fillSprite.alpha = num;
		title.label.alpha = num;
		requirementHeading.label.alpha = num;
		requirements.label.alpha = num;
		rewardHeading.label.alpha = num;
		rewards.label.alpha = num;
		borderSprites[0].scaleX = Mathf.Lerp(1f, rightAnchor, num);
		borderSprites[1].scaleX = Mathf.Lerp(1f, rightAnchor, num);
		separatorSprite.scaleY = Mathf.Lerp(1f, 170f, num);
		borderSprites[0].y = pages[0].pos.y + 550f;
		borderSprites[1].y = pages[0].pos.y + 218f;
		fillSprite.y = pages[0].pos.y + 218f;
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = currentAlpha;
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
		if (closing && darkSprite.alpha <= 0.05f)
		{
			manager.StopSideProcess(this);
			fillSprite.RemoveFromContainer();
			borderSprites[0].RemoveFromContainer();
			borderSprites[1].RemoveFromContainer();
			separatorSprite.RemoveFromContainer();
			owner.isShowingDialog = false;
		}
	}
}
