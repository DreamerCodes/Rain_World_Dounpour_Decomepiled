using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ManualDialog : Dialog, SelectOneButton.SelectOneButtonOwner
{
	public MenuIllustration title;

	public SimpleButton cancelButton;

	public float leftAnchor;

	public float rightAnchor;

	public bool opening;

	public bool closing;

	public float movementCounter;

	public SelectOneButton[] topicButtons;

	public SymbolButton backPage;

	public SymbolButton forwardPage;

	public MenuLabel pageLabel;

	public ManualPage currentTopicPage;

	public int index;

	public int pageNumber;

	public string currentTopic;

	public Dictionary<string, int> topics;

	public float sin;

	public bool firstView;

	public float lastAlpha;

	public float currentAlpha;

	public float uAlpha;

	public float targetAlpha;

	public float globalOffX;

	public float contentOffX;

	public float wrapTextMargin;

	public ManualDialog(ProcessManager manager, Dictionary<string, int> topics)
		: base(manager)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		this.topics = topics;
		pages.Add(new Page(this, null, "TOPIC", 1));
		pages[0].pos = new Vector2(0.01f, 0f);
		pages[1].pos = new Vector2(520.01f, 155f);
		pages[0].pos.y += 2000f;
		pages[1].pos.y += 2155f;
		index = 0;
		title = new MenuIllustration(this, pages[0], "illustrations", "manual", new Vector2(683f, 690f), crispPixels: true, anchorCenter: true);
		pages[0].subObjects.Add(title);
		float num = 250f;
		if (base.CurrLang == InGameTranslator.LanguageID.French)
		{
			num = 290f;
		}
		else if (base.CurrLang == InGameTranslator.LanguageID.Russian)
		{
			num = 270f;
		}
		wrapTextMargin = 0f;
		if (base.CurrLang == InGameTranslator.LanguageID.Chinese)
		{
			wrapTextMargin = -20f;
		}
		globalOffX = (int)((num - 250f) / -2f);
		contentOffX = num - 250f + globalOffX;
		RoundedRect roundedRect = new RoundedRect(this, pages[0], new Vector2(243f + globalOffX, 100f), new Vector2(num, 550f), filled: true)
		{
			borderColor = new HSLColor(1f, 0f, 0.7f)
		};
		pages[0].subObjects.Add(roundedRect);
		float cancelButtonWidth = GetCancelButtonWidth(base.CurrLang);
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE MANUAL"), "CLOSE", new Vector2(roundedRect.pos.x + roundedRect.size.x / 2f - cancelButtonWidth / 2f, roundedRect.pos.y + 30f), new Vector2(cancelButtonWidth, 30f));
		cancelButton.rectColor = Menu.MenuColor(MenuColors.MediumGrey);
		cancelButton.labelColor = Menu.MenuColor(MenuColors.MediumGrey);
		pages[0].subObjects.Add(cancelButton);
		RoundedRect roundedRect2 = new RoundedRect(this, pages[0], new Vector2(515f + contentOffX, 100f), new Vector2(600f, 550f), filled: true)
		{
			borderColor = new HSLColor(1f, 0f, 0.7f)
		};
		pages[0].subObjects.Add(roundedRect2);
		MenuLabel item = new MenuLabel(this, pages[0], Translate("TOPICS"), new Vector2(roundedRect.pos.x + roundedRect.size.x / 2f, roundedRect.pos.y + roundedRect.size.y - 20f), default(Vector2), bigText: false)
		{
			label = 
			{
				color = new Color(0.5f, 0.5f, 0.5f)
			}
		};
		pages[0].subObjects.Add(item);
		if (topics != null)
		{
			topicButtons = new SelectOneButton[topics.Keys.Count];
			for (int i = 0; i < topicButtons.Length; i++)
			{
				topicButtons[i] = new SelectOneButton(this, pages[0], Translate(TopicName(topics.Keys.ElementAt(i))), topics.Keys.ElementAt(i), roundedRect.pos + new Vector2(17f, roundedRect.size.y - 70f - 40f * (float)i), new Vector2(num - 35f, 30f), topicButtons, i);
				pages[0].subObjects.Add(topicButtons[i]);
			}
		}
		backPage = new SymbolButton(this, pages[0], "pageleft", "PAGEBACK", roundedRect2.pos + new Vector2(roundedRect2.size.x - 150f, roundedRect2.size.y - 40f));
		backPage.size = new Vector2(50f, 35f);
		backPage.roundedRect.size = backPage.size;
		pages[0].subObjects.Add(backPage);
		pageLabel = new MenuLabel(this, pages[0], "1", roundedRect2.pos + new Vector2(roundedRect2.size.x - 80f, roundedRect2.size.y - 22f), default(Vector2), bigText: true);
		pageLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
		pages[0].subObjects.Add(pageLabel);
		forwardPage = new SymbolButton(this, pages[0], "pageright", "PAGENEXT", roundedRect2.pos + new Vector2(roundedRect2.size.x - 60f, roundedRect2.size.y - 40f));
		forwardPage.size = new Vector2(50f, 35f);
		forwardPage.roundedRect.size = forwardPage.size;
		pages[0].subObjects.Add(forwardPage);
		opening = true;
		targetAlpha = 1f;
	}

	private static float GetCancelButtonWidth(InGameTranslator.LanguageID lang)
	{
		float result = 120f;
		if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.German || lang == InGameTranslator.LanguageID.Russian)
		{
			result = 160f;
		}
		return result;
	}

	public virtual string TopicName(string topic)
	{
		return "NULL";
	}

	public virtual void GetManualPage(string topic, int pageNumber)
	{
	}

	public void UpdateSelectables()
	{
		for (int i = 0; i < topicButtons.Length; i++)
		{
			topicButtons[i].nextSelectable[0] = topicButtons[i];
			if (topics[currentTopic] > 1 && pageNumber > 0)
			{
				topicButtons[i].nextSelectable[2] = backPage;
			}
			else if (topics[currentTopic] > 1)
			{
				topicButtons[i].nextSelectable[2] = forwardPage;
			}
			else
			{
				topicButtons[i].nextSelectable[2] = topicButtons[i];
			}
		}
		backPage.nextSelectable[0] = topicButtons[index];
		backPage.nextSelectable[1] = backPage;
		backPage.nextSelectable[2] = forwardPage;
		backPage.nextSelectable[3] = topicButtons[index];
		forwardPage.nextSelectable[0] = backPage;
		forwardPage.nextSelectable[1] = forwardPage;
		forwardPage.nextSelectable[2] = forwardPage;
		forwardPage.nextSelectable[3] = topicButtons[index];
		if (topics[currentTopic] > 1)
		{
			selectedObject = forwardPage;
		}
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
		pages[1].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 255f, 155.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
		if (currentTopicPage == null)
		{
			backPage.symbolSprite.alpha = 0f;
			forwardPage.symbolSprite.alpha = 0f;
			pageLabel.label.alpha = 0f;
		}
		else
		{
			if (firstView && topics[currentTopic] > 1 && pageNumber == 0 && !forwardPage.Selected && !forwardPage.MouseOver)
			{
				forwardPage.symbolSprite.color = Color.Lerp(new Color(0.7f, 0.7f, 0.7f), new Color(1f, 0.7f, 0f), Mathf.Sin(sin / 5f));
			}
			backPage.symbolSprite.alpha = 1f;
			forwardPage.symbolSprite.alpha = 1f;
			pageLabel.label.alpha = ((topics[currentTopic] == 1) ? 0.4f : 1f);
		}
		if (forwardPage != null && backPage != null)
		{
			for (int i = 0; i < forwardPage.roundedRect.sprites.Length; i++)
			{
				forwardPage.roundedRect.sprites[i].alpha = 0f;
			}
			for (int j = 0; j < backPage.roundedRect.sprites.Length; j++)
			{
				backPage.roundedRect.sprites[j].alpha = 0f;
			}
		}
		title.sprite.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, darkSprite.alpha));
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		currentTopic = topics.Keys.ElementAt(to);
		pageNumber = 0;
		firstView = true;
		GetManualPage(currentTopic, pageNumber);
		index = to;
		UpdateSelectables();
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		return index;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLOSE")
		{
			closing = true;
			targetAlpha = 0f;
		}
		if (message == "PAGEBACK")
		{
			if (pageNumber > 0)
			{
				pageNumber--;
				GetManualPage(currentTopic, pageNumber);
			}
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message == "PAGENEXT")
		{
			if (pageNumber < topics[currentTopic] - 1)
			{
				pageNumber++;
				firstView = false;
				GetManualPage(currentTopic, pageNumber);
			}
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = currentAlpha;
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
		if (opening && pages[0].pos.y <= 0.01f)
		{
			opening = false;
		}
		if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
		{
			manager.StopSideProcess(this);
			closing = false;
		}
		if (currentTopicPage != null)
		{
			backPage.buttonBehav.greyedOut = pageNumber == 0;
			forwardPage.buttonBehav.greyedOut = pageNumber == topics[currentTopic] - 1;
			if (firstView && topics[currentTopic] > 1 && pageNumber == 0 && !forwardPage.Selected && !forwardPage.MouseOver)
			{
				sin += 1f;
			}
		}
		else
		{
			backPage.buttonBehav.greyedOut = true;
			forwardPage.buttonBehav.greyedOut = true;
		}
		pageLabel.text = ValueConverter.ConvertToString(pageNumber + 1);
	}
}
