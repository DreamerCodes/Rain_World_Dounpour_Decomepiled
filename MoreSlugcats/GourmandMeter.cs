using System.Collections.Generic;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class GourmandMeter : HudPart
{
	private class CollectionSymbol
	{
		private float greyPulse;

		private float FirstFade;

		private Vector2 Pos;

		public Vector2 GoalPos;

		private CreatureSymbol creatureSymbol;

		private ItemSymbol itemSymbol;

		private GourmandMeter Owner;

		public float Greyout = 1f;

		private int storedIndex;

		private Color storedColor;

		public int Index => storedIndex;

		public void Update()
		{
			greyPulse += 0.06f;
			if (creatureSymbol != null)
			{
				creatureSymbol.Update();
			}
			if (itemSymbol != null)
			{
				itemSymbol.Update();
			}
			if (Greyout < 1f)
			{
				Greyout = Mathf.Lerp(Greyout, 0f, 0.1f);
			}
			if (FirstFade < 0.995f)
			{
				FirstFade = Mathf.Lerp(FirstFade, 1f, 0.05f);
			}
			else
			{
				FirstFade = 1f;
			}
			Pos = Vector2.Lerp(Pos, GoalPos, 0.1f);
		}

		public void Draw(float timeStacker)
		{
			float num = Mathf.Sin(greyPulse) / 7f;
			if (creatureSymbol != null)
			{
				creatureSymbol.myColor = Color.Lerp(storedColor, new Color(0.22f + num, 0.22f + num, 0.22f + num), Mathf.Clamp(Greyout, 0f, 0.87f));
				creatureSymbol.Draw(timeStacker, Pos);
				creatureSymbol.symbolSprite.alpha = FirstFade * Owner.fade;
				if (FirstFade < 1f)
				{
					creatureSymbol.showFlash = Mathf.Lerp(creatureSymbol.showFlash, 0.5f, 0.1f);
				}
				else
				{
					creatureSymbol.showFlash = Mathf.Lerp(creatureSymbol.showFlash, 0f, 0.1f);
				}
				creatureSymbol.shadowSprite1.alpha = creatureSymbol.symbolSprite.alpha * 0.5f;
				creatureSymbol.shadowSprite2.alpha = creatureSymbol.symbolSprite.alpha * 0.5f;
			}
			if (itemSymbol != null)
			{
				itemSymbol.myColor = Color.Lerp(storedColor, new Color(0.22f + num, 0.22f + num, 0.22f + num), Mathf.Clamp(Greyout, 0f, 0.87f));
				itemSymbol.Draw(timeStacker, Pos);
				itemSymbol.symbolSprite.alpha = FirstFade * Owner.fade;
				if (FirstFade < 1f)
				{
					itemSymbol.showFlash = Mathf.Lerp(itemSymbol.showFlash, 0.5f, 0.1f);
					return;
				}
				itemSymbol.showFlash = Mathf.Lerp(itemSymbol.showFlash, 0f, 0.1f);
				itemSymbol.shadowSprite1.alpha = itemSymbol.symbolSprite.alpha * 0.5f;
				itemSymbol.shadowSprite2.alpha = itemSymbol.symbolSprite.alpha * 0.5f;
			}
		}

		public void UpdateShownSymbol(int showIndex)
		{
			if (WinState.GourmandPassageRequirementAtIndex(storedIndex) == AbstractPhysicalObject.AbstractObjectType.Creature)
			{
				IconSymbol.IconSymbolData iconData = new IconSymbol.IconSymbolData(WinState.GourmandPassageCreaturesAtIndex(storedIndex)[showIndex], AbstractPhysicalObject.AbstractObjectType.Creature, 0);
				if (creatureSymbol != null)
				{
					creatureSymbol.RemoveSprites();
				}
				creatureSymbol = new CreatureSymbol(iconData, Owner.HUDfContainer);
				storedColor = CreatureSymbol.ColorOfCreature(iconData);
				creatureSymbol.myColor = storedColor;
				creatureSymbol.Show(showShadowSprites: true);
				creatureSymbol.shadowSprite1.alpha = 0f;
				creatureSymbol.shadowSprite2.alpha = 0f;
			}
			else
			{
				IconSymbol.IconSymbolData iconData2 = new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, WinState.GourmandPassageRequirementAtIndex(storedIndex), 0);
				if (itemSymbol != null)
				{
					itemSymbol.RemoveSprites();
				}
				itemSymbol = new ItemSymbol(iconData2, Owner.HUDfContainer);
				storedColor = ItemSymbol.ColorForItem(WinState.GourmandPassageRequirementAtIndex(storedIndex), 0);
				itemSymbol.myColor = storedColor;
				itemSymbol.Show(showShadowSprites: true);
				itemSymbol.shadowSprite1.alpha = 0f;
				itemSymbol.shadowSprite2.alpha = 0f;
			}
		}

		public void RemoveSprites()
		{
			if (creatureSymbol != null)
			{
				creatureSymbol.RemoveSprites();
			}
			if (itemSymbol != null)
			{
				itemSymbol.RemoveSprites();
			}
		}

		public CollectionSymbol(Vector2 InitialPos, Vector2 InitialGoal, int gourmandIndex, int showIndex, GourmandMeter owner)
		{
			Pos = InitialPos;
			GoalPos = InitialGoal;
			FirstFade = 0f;
			Greyout = 0f;
			Owner = owner;
			storedIndex = gourmandIndex;
			UpdateShownSymbol(showIndex);
		}
	}

	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public int visibleRows;

	private FContainer HUDfContainer;

	private List<int> CurrentProgress;

	private List<int> PreviousProgress;

	private WinState GameWinState;

	private bool ShowingFromPersonalPriority;

	private int animationUpdateCounter;

	private int InternalCheckForUpdate;

	private int animationUpdateCounterMax;

	private List<CollectionSymbol> CollectedSymbols;

	private CollectionSymbol NextCollectionSymbol;

	private bool Show
	{
		get
		{
			if (!hud.showKarmaFoodRain && !hud.owner.RevealMap)
			{
				return ShowingFromPersonalPriority;
			}
			return true;
		}
	}

	public GourmandMeter(global::HUD.HUD hud, FContainer fContainer)
		: base(hud)
	{
		pos = new Vector2(80f, 20f);
		lastPos = pos;
		HUDfContainer = fContainer;
		animationUpdateCounterMax = 150;
		CollectedSymbols = new List<CollectionSymbol>();
	}

	public override void Update()
	{
		int num = WinState.GourmandPassageTracker.Length / 2;
		if (!(hud.rainWorld.processManager.currentMainLoop is RainWorldGame) || (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession == null || (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession.saveState == null)
		{
			return;
		}
		if (GameWinState == null)
		{
			GameWinState = (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession.saveState.deathPersistentSaveData.winState;
			if (UpdateFromTrackerState())
			{
				for (int i = 0; i < CurrentProgress.Count; i++)
				{
					int num2 = i / num;
					int num3 = i % num;
					if (CurrentProgress[i] > 0)
					{
						CollectedSymbols.Add(new CollectionSymbol(new Vector2(pos.x + 23f * (float)num3, pos.y + 23f * (float)num2), new Vector2(pos.x + 23f * (float)num3, pos.y + 23f * (float)num2), i, CurrentProgress[i] - 1, this));
					}
				}
			}
			InternalCheckForUpdate = 10;
		}
		if (animationUpdateCounter <= 0)
		{
			InternalCheckForUpdate--;
			if (InternalCheckForUpdate <= 0)
			{
				InternalCheckForUpdate = 10;
				if (UpdateFromTrackerState())
				{
					int num4 = -1;
					if (NextCollectionSymbol != null)
					{
						num4 = NextCollectionSymbol.Index;
					}
					UpdatePredictedNextItem();
					bool flag = false;
					int j;
					for (j = 0; j < CurrentProgress.Count; j++)
					{
						if (flag)
						{
							break;
						}
						if (CurrentProgress[j] > 0 && PreviousProgress[j] <= 0)
						{
							Custom.Log("Tracker changed at", j.ToString());
							flag = true;
							break;
						}
					}
					if (flag)
					{
						int num5 = j / num;
						int num6 = j % num;
						animationUpdateCounter = animationUpdateCounterMax;
						PreviousProgress[j] = CurrentProgress[j];
						int showIndex = 0;
						if (CurrentProgress[j] > 0)
						{
							showIndex = CurrentProgress[j] - 1;
						}
						if (j != num4)
						{
							CollectedSymbols.Add(new CollectionSymbol(new Vector2(pos.x + 23f * (float)num6 + 60f, pos.y + 23f * (float)num5), new Vector2(pos.x + 23f * (float)num6, pos.y + 23f * (float)num5), j, showIndex, this));
						}
					}
				}
			}
		}
		else
		{
			ShowingFromPersonalPriority = true;
			animationUpdateCounter--;
		}
		if (hud.foodMeter != null)
		{
			pos.x = hud.foodMeter.pos.x;
			pos.y = hud.foodMeter.pos.y + 25f;
			fade = Mathf.Lerp(fade, Show ? hud.foodMeter.fade : 0f, (fade < hud.foodMeter.fade) ? 0.15f : 0.25f);
		}
		int num7 = 0;
		visibleRows = 0;
		foreach (CollectionSymbol collectedSymbol in CollectedSymbols)
		{
			int num8 = num7 / num;
			int num9 = num7 % num;
			collectedSymbol.Update();
			collectedSymbol.GoalPos = new Vector2(pos.x + 20f * (float)num9, pos.y + 20f * (float)num8);
			num7++;
			if (num8 + 1 > visibleRows)
			{
				visibleRows = num8 + 1;
			}
		}
		lastPos = pos;
		lastFade = fade;
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		foreach (CollectionSymbol collectedSymbol in CollectedSymbols)
		{
			collectedSymbol.Draw(timeStacker);
		}
	}

	public bool UpdateFromTrackerState()
	{
		if (GameWinState.GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, addIfMissing: true) is WinState.GourFeastTracker gourFeastTracker)
		{
			if (CurrentProgress == null)
			{
				CurrentProgress = new List<int>();
				for (int i = 0; i < gourFeastTracker.currentCycleProgress.Length; i++)
				{
					CurrentProgress.Add(gourFeastTracker.currentCycleProgress[i]);
				}
				PreviousProgress = new List<int>();
				for (int j = 0; j < gourFeastTracker.currentCycleProgress.Length; j++)
				{
					PreviousProgress.Add(CurrentProgress[j]);
				}
			}
			CurrentProgress = new List<int>();
			for (int k = 0; k < gourFeastTracker.currentCycleProgress.Length; k++)
			{
				CurrentProgress.Add(gourFeastTracker.currentCycleProgress[k]);
			}
			return true;
		}
		return false;
	}

	public void UpdatePredictedNextItem()
	{
		int num = WinState.GourmandPassageTracker.Length / 2;
		if (NextCollectionSymbol != null && PreviousProgress[NextCollectionSymbol.Index] > 0)
		{
			Custom.Log("Updated prediction symbol", NextCollectionSymbol.Index.ToString());
			NextCollectionSymbol.UpdateShownSymbol(PreviousProgress[NextCollectionSymbol.Index] - 1);
			NextCollectionSymbol.Greyout = 0.99f;
			NextCollectionSymbol = null;
		}
		if (NextCollectionSymbol != null)
		{
			return;
		}
		for (int i = 0; i < CurrentProgress.Count; i++)
		{
			if (CurrentProgress[i] <= 0)
			{
				int num2 = i / num;
				int num3 = i % num;
				NextCollectionSymbol = new CollectionSymbol(new Vector2(pos.x + 23f * (float)num3 + 60f, pos.y + 23f * (float)num2), new Vector2(pos.x + 23f * (float)num3, pos.y + 23f * (float)num2), i, 0, this);
				CollectedSymbols.Add(NextCollectionSymbol);
				NextCollectionSymbol.Greyout = 1f;
				Custom.Log("Made new prediction symbol");
				break;
			}
		}
	}
}
