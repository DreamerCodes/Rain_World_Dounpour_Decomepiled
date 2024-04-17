using System;
using System.Collections.Generic;
using Expedition;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class FilterDialog : Dialog, CheckBox.IOwnCheckBox
{
	public FSprite pageTitle;

	public SimpleButton cancelButton;

	public float leftAnchor;

	public float rightAnchor;

	public ChallengeSelectPage owner;

	public bool opening;

	public bool closing;

	public float movementCounter;

	public MenuLabel heading;

	public MenuLabel description;

	public List<FSprite> dividers;

	public List<MenuLabel> challengeTypes;

	public List<CheckBox> checkBoxes;

	public float lastAlpha;

	public float currentAlpha;

	public float uAlpha;

	public float targetAlpha;

	public int counter;

	public int solo;

	public int lastSelectedCheck;

	public MenuLabel localizedSubtitle;

	public bool doubleClick;

	public FilterDialog(ProcessManager manager, ChallengeSelectPage owner)
		: base(manager)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		this.owner = owner;
		pages[0].pos.y += 2000f;
		if (ChallengeOrganizer.filterChallengeTypes == null)
		{
			ChallengeOrganizer.filterChallengeTypes = new List<string>();
		}
		pageTitle = new FSprite("filters");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 720f;
		pageTitle.y = 680f;
		pages[0].Container.AddChild(pageTitle);
		if (manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(this, pages[0], Translate("-FILTERS-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			pages[0].subObjects.Add(localizedSubtitle);
		}
		float num = 85f;
		float num2 = LabelTest.GetWidth(Translate("CLOSE")) + 10f;
		if (num2 > num)
		{
			num = num2;
		}
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", new Vector2(683f - num / 2f, 120f), new Vector2(num, 35f));
		pages[0].subObjects.Add(cancelButton);
		heading = new MenuLabel(this, pages[0], Translate("CHALLENGE FILTER"), new Vector2(683f, 655f), default(Vector2), bigText: false);
		heading.label.color = new Color(0.7f, 0.7f, 0.7f);
		pages[0].subObjects.Add(heading);
		description = new MenuLabel(this, pages[0], Translate("Toggle which challenges can appear when randomising"), new Vector2(683f, 635f), default(Vector2), bigText: false);
		description.label.color = new Color(0.7f, 0.7f, 0.7f);
		pages[0].subObjects.Add(description);
		dividers = new List<FSprite>();
		challengeTypes = new List<MenuLabel>();
		checkBoxes = new List<CheckBox>();
		List<Challenge> list = new List<Challenge>();
		for (int i = 0; i < ChallengeOrganizer.availableChallengeTypes.Count; i++)
		{
			if (ChallengeOrganizer.availableChallengeTypes[i].ValidForThisSlugcat(ExpeditionData.slugcatPlayer))
			{
				list.Add(ChallengeOrganizer.availableChallengeTypes[i]);
			}
		}
		cancelButton.nextSelectable[0] = cancelButton;
		cancelButton.nextSelectable[2] = cancelButton;
		for (int j = 0; j < list.Count; j++)
		{
			MenuLabel item = new MenuLabel(this, pages[0], list[j].ChallengeName(), new Vector2(553f, 590f - 37f * (float)j), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			challengeTypes.Add(item);
			pages[0].subObjects.Add(item);
			CheckBox checkBox = new CheckBox(this, pages[0], this, new Vector2(793f, 577f - 37f * (float)j), 0f, "", list[j].GetType().Name);
			checkBox.nextSelectable[0] = checkBox;
			checkBox.nextSelectable[2] = checkBox;
			if (j == 0)
			{
				checkBox.nextSelectable[1] = cancelButton;
				cancelButton.nextSelectable[3] = checkBox;
			}
			if (j == list.Count - 1)
			{
				checkBox.nextSelectable[3] = cancelButton;
				cancelButton.nextSelectable[1] = checkBox;
			}
			pages[0].subObjects.Add(checkBox);
			checkBoxes.Add(checkBox);
			if (j < list.Count - 1)
			{
				FSprite fSprite = new FSprite("pixel")
				{
					x = 684f - leftAnchor,
					y = 571f - 37f * (float)j,
					scaleX = 270f,
					color = new Color(0.4f, 0.4f, 0.4f)
				};
				container.AddChild(fSprite);
				dividers.Add(fSprite);
			}
		}
		opening = true;
		targetAlpha = 1f;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (opening || closing)
		{
			uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
			darkSprite.alpha = uAlpha * 0.95f;
		}
		pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
		heading.label.alpha = darkSprite.alpha;
		description.label.alpha = darkSprite.alpha;
		for (int i = 0; i < dividers.Count; i++)
		{
			dividers[i].alpha = darkSprite.alpha;
			dividers[i].x = 684f + Vector2.Lerp(pages[0].pos, pages[0].lastPos, timeStacker).x;
			dividers[i].y = 571f - 37f * (float)i + Vector2.Lerp(pages[0].pos, pages[0].lastPos, timeStacker).y;
		}
		for (int j = 0; j < challengeTypes.Count; j++)
		{
			challengeTypes[j].label.alpha = darkSprite.alpha;
		}
		pageTitle.x = owner.page.pos.x + 685f;
		pageTitle.y = owner.page.pos.y + 680f;
		pageTitle.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
	}

	public bool GetChecked(CheckBox box)
	{
		if (ChallengeOrganizer.filterChallengeTypes != null && ChallengeOrganizer.filterChallengeTypes.Contains(box.IDString))
		{
			challengeTypes[checkBoxes.IndexOf(box)].label.color = new Color(0.25f, 0.25f, 0.25f);
			return false;
		}
		challengeTypes[checkBoxes.IndexOf(box)].label.color = new Color(0.83f, 0.83f, 0.83f);
		return true;
	}

	public void SetChecked(CheckBox box, bool c)
	{
		if (!doubleClick && lastSelectedCheck == checkBoxes.IndexOf(box) && counter > 0)
		{
			doubleClick = true;
			counter = 100;
			solo = checkBoxes.IndexOf(box);
			PlaySound(SoundID.MENU_Player_Join_Game);
			SetChecked(box, c: true);
			for (int i = 0; i < checkBoxes.Count; i++)
			{
				if (i != solo)
				{
					SetChecked(checkBoxes[i], c: false);
				}
			}
		}
		lastSelectedCheck = checkBoxes.IndexOf(box);
		counter = 10;
		if (ChallengeOrganizer.filterChallengeTypes == null)
		{
			return;
		}
		if (!c && !ChallengeOrganizer.filterChallengeTypes.Contains(box.IDString))
		{
			int num = 0;
			for (int j = 0; j < checkBoxes.Count; j++)
			{
				if (checkBoxes[j].Checked)
				{
					num++;
				}
			}
			if (num == 1)
			{
				PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			ExpLog.Log("Add " + box.IDString);
			ChallengeOrganizer.filterChallengeTypes.Add(box.IDString);
		}
		if (c && ChallengeOrganizer.filterChallengeTypes.Contains(box.IDString))
		{
			ExpLog.Log("Remove " + box.IDString);
			ChallengeOrganizer.filterChallengeTypes.Remove(box.IDString);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE")
		{
			owner.unlocksButton.greyedOut = false;
			owner.startButton.greyedOut = false;
			closing = true;
			targetAlpha = 0f;
		}
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = currentAlpha;
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
		if (opening && pages[0].pos.y <= 0.011f)
		{
			opening = false;
		}
		if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
		{
			pageTitle.RemoveFromContainer();
			manager.StopSideProcess(this);
			closing = false;
		}
		cancelButton.buttonBehav.greyedOut = opening;
		if (counter > 0)
		{
			counter--;
		}
		else
		{
			doubleClick = false;
		}
	}
}
