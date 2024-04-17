using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu;

public class EndgameTokens : PositionedMenuObject
{
	public class Token : PositionedMenuObject
	{
		public EndgameMeter endgameMeter;

		private FSprite symbolSprite;

		private FSprite circleSprite;

		private FSprite glowSprite;

		public int index;

		public float fade;

		public float lastFade;

		private float getToX;

		private float getToY;

		private float superGlow;

		private bool activated;

		public EndgameTokens Tokens => owner as EndgameTokens;

		public bool WantToBeVisible => endgameMeter.showAsFullfilled > 0f;

		public bool InPosition
		{
			get
			{
				if (Mathf.Abs(pos.x - getToX) < 5f)
				{
					return Mathf.Abs(pos.y - getToY) < 5f;
				}
				return false;
			}
		}

		public Token(Menu menu, MenuObject owner, Vector2 pos, EndgameMeter endgameMeter, FContainer container, int index)
			: base(menu, owner, pos)
		{
			this.endgameMeter = endgameMeter;
			this.index = index;
			myContainer = container;
			symbolSprite = new FSprite(endgameMeter.tracker.ID.ToString() + "A");
			container.AddChild(symbolSprite);
			circleSprite = new FSprite("EndGameCircle");
			container.AddChild(circleSprite);
			glowSprite = new FSprite("Futile_White");
			glowSprite.shader = menu.manager.rainWorld.Shaders["FlatLight"];
			container.AddChild(glowSprite);
			int num = 0;
			for (int i = 0; i < index; i++)
			{
				if (Tokens.tokens[i].WantToBeVisible)
				{
					num++;
				}
			}
			float x = 20f + 40f * (float)(num % 5);
			float y = 15f + 40f * Mathf.Floor((float)num / 5f);
			pos = new Vector2(x, y);
			base.pos = pos;
		}

		public override void Update()
		{
			base.Update();
			lastFade = fade;
			int num = 0;
			for (int i = 0; i < index; i++)
			{
				if (Tokens.tokens[i].WantToBeVisible)
				{
					num++;
				}
			}
			getToX = 20f + 40f * (float)(num % 5);
			getToY = 15f + 40f * Mathf.Floor((float)num / 5f);
			pos.x = Custom.LerpAndTick(pos.x, getToX, 0.05f, 1f / 30f);
			pos.y = Custom.LerpAndTick(pos.y, getToY, 0.05f, 1f / 30f);
			if (endgameMeter.tracker.GoalAlreadyFullfilled)
			{
				fade = Custom.LerpAndTick(fade, 1f, 0.05f, 1f / 30f);
			}
			else if (index == Tokens.tokens.Count - 1 || Tokens.tokens[index + 1].InPosition)
			{
				fade = Custom.LerpAndTick(fade, endgameMeter.showAsFullfilled, 0.05f, 1f / 30f);
			}
			if (activated)
			{
				superGlow = Custom.LerpAndTick(superGlow, 1f, 0.07f, 0.0125f);
			}
		}

		public void Activate()
		{
			symbolSprite.RemoveFromContainer();
			circleSprite.RemoveFromContainer();
			glowSprite.RemoveFromContainer();
			owner.Container.AddChild(symbolSprite);
			owner.Container.AddChild(circleSprite);
			owner.Container.AddChild(glowSprite);
			activated = true;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Vector2 vector = DrawPos(timeStacker);
			float num = Mathf.Lerp(lastFade, fade, timeStacker);
			float num2 = Mathf.Lerp(endgameMeter.fullfilledNow ? 1f : 0f, 1f, superGlow);
			Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), superGlow), num2);
			symbolSprite.x = vector.x;
			symbolSprite.y = vector.y;
			circleSprite.x = vector.x;
			circleSprite.y = vector.y;
			glowSprite.x = vector.x;
			glowSprite.y = vector.y;
			symbolSprite.color = color;
			circleSprite.color = color;
			glowSprite.color = color;
			symbolSprite.alpha = num;
			circleSprite.alpha = num;
			glowSprite.scale = Mathf.Lerp(3f, 5f + num2, num) + superGlow * Mathf.Lerp(0.75f, 1f, Random.value);
			glowSprite.alpha = Mathf.Lerp(0f, 0.3f, num2) * num;
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			symbolSprite.RemoveFromContainer();
			circleSprite.RemoveFromContainer();
			glowSprite.RemoveFromContainer();
		}
	}

	public List<Token> tokens;

	private FSprite blackSprite;

	private float blackFade;

	private float lastBlackFade;

	public bool forceShowTokenAdd;

	private bool addPassageButtonWhenTokenBecomesVisible;

	public bool AllAnimationsDone
	{
		get
		{
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].fade < 0.9f || !tokens[i].InPosition)
				{
					return false;
				}
			}
			return true;
		}
	}

	public EndgameTokens(Menu menu, MenuObject owner, Vector2 pos, FContainer container, KarmaLadder ladder)
		: base(menu, owner, pos)
	{
		tokens = new List<Token>();
		bool flag = false;
		addPassageButtonWhenTokenBecomesVisible = false;
		int num = 0;
		for (int i = 0; i < ladder.endGameMeters.Count; i++)
		{
			if (ladder.endGameMeters[i].fullfilledNow)
			{
				addPassageButtonWhenTokenBecomesVisible = true;
			}
			if (ladder.endGameMeters[i].tracker.GoalFullfilled && !ladder.endGameMeters[i].tracker.consumed)
			{
				if (ladder.endGameMeters[i].tracker.GoalAlreadyFullfilled && !flag)
				{
					flag = true;
				}
				tokens.Add(new Token(menu, this, default(Vector2), ladder.endGameMeters[i], container, num));
				subObjects.Add(tokens[tokens.Count - 1]);
				num++;
			}
			if (ladder.endGameMeters[i].fullfilledNow)
			{
				forceShowTokenAdd = true;
			}
		}
		if (flag)
		{
			(menu as SleepAndDeathScreen).AddPassageButton(buttonBlack: false);
			addPassageButtonWhenTokenBecomesVisible = false;
		}
	}

	public void Passage(WinState.EndgameID ID)
	{
		blackSprite = new FSprite("pixel");
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = menu.manager.rainWorld.screenSize.x / 2f;
		blackSprite.y = menu.manager.rainWorld.screenSize.y / 2f;
		blackSprite.color = new Color(0f, 0f, 0f);
		blackSprite.alpha = 0f;
		blackFade = 0.01f;
		Container.AddChild(blackSprite);
		for (int i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].endgameMeter.tracker.ID == ID)
			{
				tokens[i].Activate();
				break;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		lastBlackFade = blackFade;
		if (blackFade > 0f)
		{
			blackFade = Mathf.Min(1f, blackFade + 0.025f);
		}
		if (!addPassageButtonWhenTokenBecomesVisible)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].endgameMeter.fullfilledNow && tokens[i].endgameMeter.showAsFullfilled < 0.9f)
			{
				flag = false;
			}
		}
		if (flag)
		{
			(menu as SleepAndDeathScreen).AddPassageButton(buttonBlack: true);
			addPassageButtonWhenTokenBecomesVisible = false;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (blackSprite != null)
		{
			blackSprite.alpha = Custom.SCurve(Mathf.Lerp(lastBlackFade, blackFade, timeStacker), 0.7f) * Custom.LerpMap((menu as SleepAndDeathScreen).endGameSceneCounter, 90f, 120f, 0.85f, 1f);
		}
	}
}
