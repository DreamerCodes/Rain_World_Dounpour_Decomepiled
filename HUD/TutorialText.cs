using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace HUD;

public class TutorialText : HudPart
{
	public class Message
	{
		public string text;

		public int wait;

		public int time;

		public bool fadeOut;

		public Message(string text, int wait, int time)
		{
			this.text = text;
			this.wait = wait;
			this.time = time;
		}
	}

	private FLabel[] labels;

	private FSprite fullScreenFadeSprite;

	private FSprite[] fadeSprites;

	public List<Message> messages;

	public float show;

	public float lastShow;

	public TutorialText(HUD hud)
		: base(hud)
	{
		fullScreenFadeSprite = new FSprite("Futile_White");
		fullScreenFadeSprite.color = new Color(0f, 0f, 0f);
		fullScreenFadeSprite.x = hud.rainWorld.options.ScreenSize.x / 2f;
		fullScreenFadeSprite.y = hud.rainWorld.options.ScreenSize.y / 2f;
		fullScreenFadeSprite.scaleX = (hud.rainWorld.options.ScreenSize.x + 2f) / 16f;
		fullScreenFadeSprite.scaleY = (hud.rainWorld.options.ScreenSize.y + 2f) / 16f;
		hud.fContainers[1].AddChild(fullScreenFadeSprite);
		fadeSprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			fadeSprites[i] = new FSprite("Futile_White");
			fadeSprites[i].shader = hud.rainWorld.Shaders["FlatLight"];
			fadeSprites[i].color = new Color(0f, 0f, 0f);
			fadeSprites[i].x = hud.rainWorld.options.ScreenSize.x / 2f;
			fadeSprites[i].y = 100f;
			hud.fContainers[1].AddChild(fadeSprites[i]);
		}
		fadeSprites[0].scaleY = 2.5f;
		fadeSprites[1].scaleY = 5f;
		labels = new FLabel[2];
		for (int j = 0; j < 2; j++)
		{
			labels[j] = new FLabel(Custom.GetFont(), "");
			labels[j].alignment = FLabelAlignment.Center;
			labels[j].x = hud.rainWorld.options.ScreenSize.x / 2f + 0.01f + ((j == 0) ? 1f : 0f);
			labels[j].y = hud.rainWorld.options.SafeScreenOffset.y + 100f + 0.01f - ((j == 0) ? 1f : 0f);
			hud.fContainers[1].AddChild(labels[j]);
		}
		labels[0].color = new Color(0f, 0f, 0f);
		messages = new List<Message>();
	}

	public override void Update()
	{
		lastShow = show;
		if (messages.Count == 0 || messages[0].fadeOut || messages[0].wait > 0)
		{
			show = Mathf.Max(0f, show - 1f / 30f);
		}
		else
		{
			show = Mathf.Min(1f, show + 1f / 30f);
		}
		if (messages.Count <= 0)
		{
			return;
		}
		if (messages[0].wait > 0)
		{
			messages[0].wait--;
			if (messages.Count > 1)
			{
				messages[0].wait = 0;
			}
		}
		else if (show == 1f)
		{
			messages[0].time--;
			if (messages.Count > 1)
			{
				messages[0].time--;
			}
			if (messages[0].time < 1)
			{
				messages[0].fadeOut = true;
			}
		}
		else if (messages[0].fadeOut && show == 0f)
		{
			messages.RemoveAt(0);
			if (messages.Count > 0)
			{
				InitNextMessage();
			}
		}
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		float num = Mathf.Lerp(lastShow, show, timeStacker);
		for (int i = 0; i < 2; i++)
		{
			labels[i].alpha = num;
		}
		fadeSprites[0].alpha = num * 0.5f;
		fadeSprites[1].alpha = num * 0.35f;
		fullScreenFadeSprite.alpha = num * 0.25f;
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < 2; i++)
		{
			labels[i].RemoveFromContainer();
			fadeSprites[i].RemoveFromContainer();
		}
		fullScreenFadeSprite.RemoveFromContainer();
	}

	public void AddMessage(string text, int wait, int time)
	{
		messages.Add(new Message(text, wait, time));
		if (messages.Count == 1)
		{
			InitNextMessage();
		}
	}

	private void InitNextMessage()
	{
		for (int i = 0; i < 2; i++)
		{
			labels[i].text = messages[0].text;
		}
		fadeSprites[0].scaleX = ((float)messages[0].text.Length * 10f + 30f) / 16f;
		fadeSprites[1].scaleX = ((float)messages[0].text.Length * 10f + 30f) / 8f;
	}
}
