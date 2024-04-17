using System;
using System.Collections.Generic;
using System.IO;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class LevelSelector : PositionedMenuObject
{
	public class LevelItem : ButtonTemplate
	{
		public FSprite imageSprite;

		public FSprite dividerSprite;

		public FSprite dividerSprite2;

		public LevelItem dividerAbove;

		public LevelItem dividerBelow;

		public bool divGraphic;

		public MenuLabel label;

		public float fade;

		public float lastFade;

		public float thumbChangeFade;

		public float lastThumbChangeFade;

		public bool active;

		public bool thumbLoaded;

		public float fadeAway;

		public string name;

		private bool doAThumbFade;

		private bool sleep;

		public RoundedRect roundedRect;

		private float selectedBlink;

		private float lastSelectedBlink;

		private bool lastSelected;

		public int neverPlayed;

		private float blackConst => 0f;

		public override FContainer Container
		{
			get
			{
				return (owner as LevelDisplay).levelsContainer;
			}
			set
			{
			}
		}

		public override bool CurrentlySelectableMouse
		{
			get
			{
				if (fade > 0.5f)
				{
					return active;
				}
				return false;
			}
		}

		public override bool CurrentlySelectableNonMouse
		{
			get
			{
				if (fade > 0.5f)
				{
					return active;
				}
				return false;
			}
		}

		private float ShowThumbs(float timeStacker)
		{
			return (owner as LevelDisplay).ShowThumbs(timeStacker);
		}

		public override Color MyColor(float timeStacker)
		{
			if (owner is SingleLevelDisplay)
			{
				return Menu.MenuRGB(Menu.MenuColors.MediumGrey);
			}
			if (buttonBehav.greyedOut)
			{
				return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.Black), blackConst).rgb;
			}
			float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
			a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
			HSLColor from = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.White), a);
			return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), blackConst).rgb;
		}

		public LevelItem(Menu menu, MenuObject owner, string name)
			: base(menu, owner, new Vector2(0f, 0f), new Vector2(120f, 20f))
		{
			this.name = name;
			roundedRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), new Vector2(size.x, size.y), filled: true);
			subObjects.Add(roundedRect);
			if ((owner.owner as LevelSelector).IsThumbnailLoaded(name))
			{
				imageSprite = new FSprite(name + "_Thumb");
				thumbLoaded = true;
			}
			else
			{
				imageSprite = new FSprite("Menu_Empty_Level_Thumb");
				imageSprite.color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			}
			Container.AddChild(imageSprite);
			buttonBehav = new ButtonBehavior(this);
			label = new MenuLabel(menu, this, MultiplayerUnlocks.LevelDisplayName(name), new Vector2(0.01f, 0.01f), new Vector2(size.x, 20f), bigText: false);
			subObjects.Add(label);
		}

		public void AddDividers(LevelItem nxt)
		{
			divGraphic = ShowThumbs(1f) > 0.5f;
			dividerSprite = new FSprite(divGraphic ? "listDivider2" : "listDivider");
			dividerSprite2 = new FSprite("listDivider2bkg");
			dividerSprite.color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			dividerSprite2.color = Color.black;
			(owner as LevelsList).dividerContainer.AddChild(dividerSprite2);
			(owner as LevelsList).dividerContainer.AddChild(dividerSprite);
			dividerBelow = nxt;
			nxt.dividerAbove = this;
		}

		public void ThumbnailHasBeenLoaded()
		{
			thumbLoaded = true;
			doAThumbFade = true;
		}

		public override void Update()
		{
			base.Update();
			lastFade = fade;
			if (neverPlayed > 0)
			{
				neverPlayed++;
			}
			lastSelectedBlink = selectedBlink;
			if (Selected)
			{
				if (!lastSelected)
				{
					selectedBlink = 1f - ShowThumbs(1f);
				}
				selectedBlink = Mathf.Max(0f, selectedBlink - 1f / Mathf.Lerp(10f, 40f, selectedBlink));
			}
			else
			{
				selectedBlink = 0f;
			}
			lastSelected = Selected;
			float num = 1f;
			if (owner is LevelsList)
			{
				int num2 = -1;
				for (int i = 0; i < (owner as LevelsList).levelItems.Count; i++)
				{
					if ((owner as LevelsList).levelItems[i] == this)
					{
						num2 = i;
						break;
					}
				}
				active = num2 >= (owner as LevelsList).ScrollPos && num2 < (owner as LevelsList).ScrollPos + (owner as LevelsList).MaxVisibleItems;
				if (sleep)
				{
					if (doAThumbFade)
					{
						imageSprite.element = Futile.atlasManager.GetElementWithName(name + "_Thumb");
						imageSprite.color = new Color(1f, 1f, 1f);
						doAThumbFade = false;
						thumbChangeFade = 0f;
						lastThumbChangeFade = 0f;
					}
					if (!active)
					{
						return;
					}
					sleep = false;
				}
				active = active && fadeAway == 0f;
				float value = (owner as LevelsList).StepsDownOfItem(num2) - 1f;
				if ((float)num2 < (owner as LevelsList).floatScrollPos)
				{
					num = Mathf.InverseLerp((owner as LevelsList).floatScrollPos - 1f, (owner as LevelsList).floatScrollPos, value);
				}
				else if ((float)num2 > (owner as LevelsList).floatScrollPos + (float)(owner as LevelsList).MaxVisibleItems - 1f)
				{
					num = Mathf.InverseLerp((owner as LevelsList).floatScrollPos + (float)(owner as LevelsList).MaxVisibleItems, (owner as LevelsList).floatScrollPos + (float)(owner as LevelsList).MaxVisibleItems - 1f, value);
				}
			}
			lastThumbChangeFade = thumbChangeFade;
			if (doAThumbFade)
			{
				thumbChangeFade = Custom.LerpAndTick(thumbChangeFade, 1f, 0.08f, 1f / 30f);
				if (thumbChangeFade == 1f)
				{
					imageSprite.element = Futile.atlasManager.GetElementWithName(name + "_Thumb");
					imageSprite.color = new Color(1f, 1f, 1f);
					doAThumbFade = false;
				}
			}
			else
			{
				thumbChangeFade = Custom.LerpAndTick(thumbChangeFade, 0f, 0.08f, 1f / 30f);
			}
			size.y = (owner as LevelDisplay).OneListItemHeight - 10f * ShowThumbs(1f);
			if (fadeAway > 0f)
			{
				fadeAway += 0.1f;
				if (fadeAway >= 1f)
				{
					fadeAway = 1f;
					(owner as LevelDisplay).LevelItemFaded(this);
					return;
				}
				num *= 1f - fadeAway;
			}
			fade = Custom.LerpAndTick(fade, num, 0.08f, 0.1f);
			fade = Mathf.Lerp(fade, num, Mathf.InverseLerp(0.5f, 0.45f, Mathf.Abs(0.5f - ShowThumbs(1f))));
			float num3 = Custom.SCurve(ShowThumbs(1f) * Mathf.InverseLerp(0f, 0.8f, fade), 0.5f);
			roundedRect.size = new Vector2(size.x, size.y * (0.3f + 0.7f * Mathf.Pow(num3, 0.5f)));
			roundedRect.pos = new Vector2(0.01f, -0.49f + size.y * 0.125f * Mathf.Pow(1f - num3, 1.5f));
			roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
			roundedRect.addSize = new Vector2(10f, 6f) * 0.5f * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
			if (!thumbLoaded && Selected)
			{
				(owner.owner as LevelSelector).BumpUpThumbnailLoad(name);
			}
			if (fade == 0f && lastFade == 0f && fadeAway == 0f)
			{
				sleep = true;
				imageSprite.isVisible = false;
				label.label.isVisible = false;
				if (dividerSprite != null)
				{
					dividerSprite.isVisible = false;
					dividerSprite2.isVisible = false;
				}
				for (int j = 0; j < 9; j++)
				{
					roundedRect.sprites[j].isVisible = false;
				}
				for (int k = 9; k < 17; k++)
				{
					roundedRect.sprites[k].isVisible = false;
				}
			}
			if (dividerSprite != null && divGraphic != ShowThumbs(1f) > 0.5f)
			{
				divGraphic = ShowThumbs(1f) > 0.5f;
				dividerSprite.element = Futile.atlasManager.GetElementWithName(divGraphic ? "listDivider2" : "listDivider");
			}
		}

		public override void RemoveSprites()
		{
			imageSprite.RemoveFromContainer();
			if (dividerSprite != null)
			{
				dividerSprite.RemoveFromContainer();
				dividerSprite2.RemoveFromContainer();
			}
			base.RemoveSprites();
		}

		public override void GrafUpdate(float timeStacker)
		{
			if (sleep)
			{
				return;
			}
			imageSprite.isVisible = true;
			label.label.isVisible = true;
			if (dividerSprite != null)
			{
				dividerSprite.isVisible = true;
				dividerSprite2.isVisible = divGraphic;
			}
			base.GrafUpdate(timeStacker);
			float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.3f);
			float num2 = ShowThumbs(timeStacker);
			float num3 = num2 * Mathf.InverseLerp(0f, 0.8f, num);
			imageSprite.x = 0.01f + DrawX(timeStacker) + size.x / 2f;
			imageSprite.y = 0.01f + DrawY(timeStacker) + 20f + (float)ThumbHeight * 1.01f * num3 / 2f;
			imageSprite.alpha = num * (0.85f + 0.15f * Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker)) * Mathf.Pow(num2, 1.5f) * (1f - Mathf.Lerp(lastThumbChangeFade, thumbChangeFade, timeStacker));
			imageSprite.scaleX = (float)ThumbWidth * (0.5f + 0.5f * Mathf.Pow(num3, 0.3f)) / imageSprite.element.sourcePixelSize.x;
			imageSprite.scaleY = (float)ThumbHeight * num3 / imageSprite.element.sourcePixelSize.y;
			if (dividerSprite != null)
			{
				dividerSprite.x = imageSprite.x;
				dividerSprite.y = Mathf.Lerp(DrawY(timeStacker), dividerBelow.DrawY(timeStacker) + dividerBelow.DrawSize(timeStacker).y, 0.5f) - 1f * num2;
				dividerSprite.alpha = Mathf.Min(num, Custom.SCurve(Mathf.Lerp(dividerBelow.lastFade, dividerBelow.fade, timeStacker), 0.3f));
				if (divGraphic)
				{
					dividerSprite.alpha *= Mathf.InverseLerp(0.75f, 1f, num2);
					dividerSprite.scaleY = 0.5f + 0.5f * Mathf.InverseLerp(0.75f, 1f, num2);
					dividerSprite2.x = dividerSprite.x;
					dividerSprite2.y = dividerSprite.y;
					dividerSprite2.scaleY = dividerSprite.scaleY;
					dividerSprite2.alpha = dividerSprite.alpha;
				}
				else
				{
					dividerSprite.alpha *= Mathf.InverseLerp(0.25f, 0f, num2);
					dividerSprite.scaleY = 1f;
				}
			}
			Color color;
			if (owner is SingleLevelDisplay)
			{
				color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), (owner as SingleLevelDisplay).Pulse(1f));
				label.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), UnityEngine.Random.value * (owner as SingleLevelDisplay).Pulse(1f));
				imageSprite.alpha = num * (1f - Mathf.Lerp(lastThumbChangeFade, thumbChangeFade, timeStacker));
			}
			else
			{
				float num4 = Mathf.Lerp(1f, 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f), Mathf.Lerp(buttonBehav.lastExtraSizeBump, buttonBehav.extraSizeBump, timeStacker) * num * Mathf.Lerp(1f, 0.5f, num2));
				label.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), MyColor(timeStacker), Mathf.Lerp(num * num4, UnityEngine.Random.value, Mathf.Lerp(lastSelectedBlink, selectedBlink, timeStacker)));
				float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
				a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
				color = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.MediumGrey), num4), a).rgb;
				if (neverPlayed > 0)
				{
					color = Color.Lerp(color, Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), a), 0.5f + 0.5f * Mathf.Sin(((float)neverPlayed + timeStacker) / 30f * (float)Math.PI * 2f));
					label.label.color = color;
				}
			}
			label.label.alpha = Mathf.Pow(num, 2f);
			if (!(owner is SingleLevelDisplay) && num2 * num > 0f)
			{
				Color color2 = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
				for (int i = 0; i < 9; i++)
				{
					roundedRect.sprites[i].color = color2;
					roundedRect.sprites[i].alpha = num2 * num * 0.5f;
					roundedRect.sprites[i].isVisible = true;
				}
				for (int j = 9; j < 17; j++)
				{
					roundedRect.sprites[j].color = color;
					roundedRect.sprites[j].alpha = num2 * num;
					roundedRect.sprites[j].isVisible = true;
				}
			}
			else
			{
				for (int k = 0; k < 9; k++)
				{
					roundedRect.sprites[k].isVisible = false;
				}
				for (int l = 9; l < 17; l++)
				{
					roundedRect.sprites[l].isVisible = false;
				}
			}
		}

		public override void Clicked()
		{
			if (neverPlayed > 0)
			{
				neverPlayed = 0;
			}
			if (!active || fade <= 0.5f)
			{
				return;
			}
			for (int i = 0; i < (owner as LevelsList).levelItems.Count; i++)
			{
				if ((owner as LevelsList).levelItems[i] == this)
				{
					(owner as LevelsList).ItemClicked(i);
					break;
				}
			}
		}

		public void StartFadeAway()
		{
			sleep = false;
			if (!(fadeAway > 0f))
			{
				fadeAway = 0.01f;
			}
		}
	}

	public abstract class LevelDisplay : RectangularMenuObject
	{
		public FContainer levelsContainer;

		public float OneListItemHeight => Mathf.Lerp(20f, 40f + (float)ThumbHeight, ShowThumbs(1f));

		public virtual float ShowThumbs(float timeStacker)
		{
			return 1f;
		}

		public LevelDisplay(Menu menu, MenuObject owner, Vector2 pos, Vector2 size)
			: base(menu, owner, pos, size)
		{
		}

		public virtual void LevelItemFaded(LevelItem item)
		{
		}

		public override void RemoveSprites()
		{
			levelsContainer.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public class SingleLevelDisplay : LevelDisplay
	{
		public LevelItem currentLevelItem;

		public RoundedRect roundedRect;

		public FSprite fadeSprite;

		private int counter;

		public float lightUp;

		public float lastLightUp;

		public string LevelName
		{
			get
			{
				if ((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList.Count != 1)
				{
					return "";
				}
				return (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList[0];
			}
		}

		public HSLColor MyColor(float timeStacker)
		{
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.MediumGrey), Pulse(timeStacker));
		}

		public float Pulse(float timeStacker)
		{
			return Mathf.Lerp(lastLightUp, lightUp, timeStacker) * (0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 30f * (float)Math.PI * 2f));
		}

		public SingleLevelDisplay(Menu menu, MenuObject owner, Vector2 pos)
			: base(menu, owner, pos, new Vector2(156f, 76f))
		{
			fadeSprite = new FSprite("LinearGradient200");
			fadeSprite.anchorY = 0f;
			fadeSprite.color = Color.black;
			fadeSprite.scaleY = -0.25f;
			fadeSprite.scaleX = size.x;
			Container.AddChild(fadeSprite);
			(owner as LevelSelector).allLevelsList.scrollSlider.subtleSliderNob.outerCircle.RemoveFromContainer();
			Container.AddChild((owner as LevelSelector).allLevelsList.scrollSlider.subtleSliderNob.outerCircle);
			roundedRect = new RoundedRect(menu, this, new Vector2(-1.99f, -1.99f), size + new Vector2(4f, 4f), filled: true);
			subObjects.Add(roundedRect);
			levelsContainer = new FContainer();
			Container.AddChild(levelsContainer);
			currentLevelItem = new LevelItem(menu, this, LevelName);
			currentLevelItem.pos = new Vector2(size.x / 2f - currentLevelItem.size.x / 2f, 0f);
			subObjects.Add(currentLevelItem);
		}

		public override void Update()
		{
			base.Update();
			lastLightUp = lightUp;
			if (currentLevelItem.fade < 1f)
			{
				lightUp = Custom.LerpAndTick(lightUp, 1f, 0.07f, 0.05f);
			}
			else
			{
				lightUp = Custom.LerpAndTick(lightUp, 0f, 0.01f, 1f / 90f);
			}
			counter++;
			if ((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList.Count != 1)
			{
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList.Clear();
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList.Add((owner as LevelSelector).GetMultiplayerMenu.allLevels[0]);
			}
			if (LevelName != currentLevelItem.name)
			{
				currentLevelItem.StartFadeAway();
			}
		}

		public override void LevelItemFaded(LevelItem item)
		{
			currentLevelItem.RemoveSprites();
			RemoveSubObject(currentLevelItem);
			currentLevelItem = new LevelItem(menu, this, LevelName);
			currentLevelItem.pos = new Vector2(size.x / 2f - currentLevelItem.size.x / 2f, 0f);
			subObjects.Add(currentLevelItem);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			fadeSprite.x = DrawX(timeStacker) + size.x / 2f + 0.5f;
			fadeSprite.y = roundedRect.DrawPos(timeStacker).y + 10f;
			for (int i = 0; i < 9; i++)
			{
				roundedRect.sprites[i].color = Color.black;
				roundedRect.sprites[i].alpha = 1f;
				roundedRect.sprites[i].isVisible = true;
			}
			Color rgb = MyColor(timeStacker).rgb;
			for (int j = 9; j < 17; j++)
			{
				roundedRect.sprites[j].color = rgb;
				roundedRect.sprites[j].alpha = 1f;
				roundedRect.sprites[j].isVisible = true;
			}
		}

		public override void RemoveSprites()
		{
			fadeSprite.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public abstract class LevelsList : LevelDisplay, Slider.ISliderOwner
	{
		public class LevelPreview : RectangularMenuObject
		{
			public RoundedRect roundedRect;

			public FSprite imageSprite;

			private float totFade;

			private float lastTotFade;

			private float yPos;

			private float goalYPos;

			private string levelName;

			private LevelItem lastSelected;

			private int awakeCounter;

			private int sleepCounter;

			private LevelsList levelsList => owner as LevelsList;

			public LevelPreview(Menu menu, LevelsList owner, bool rightFacing)
				: base(menu, owner, default(Vector2), new Vector2((float)ThumbWidth + 20f, (float)ThumbHeight + 20f))
			{
				roundedRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), size, filled: true);
				subObjects.Add(roundedRect);
				imageSprite = new FSprite("Menu_Empty_Level_Thumb");
				imageSprite.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
				Container.AddChild(imageSprite);
				levelName = "";
				if (rightFacing)
				{
					pos.x = levelsList.size.x + 20f + 0.01f;
				}
				else
				{
					pos.x = 0f - (size.x + 20f) + 0.01f;
				}
			}

			public override void Update()
			{
				lastPos = pos;
				lastTotFade = totFade;
				awakeCounter++;
				sleepCounter++;
				bool flag = false;
				if (levelsList.ShowThumbs(1f) == 0f)
				{
					LevelItem levelItem = null;
					for (int i = 0; i < levelsList.levelItems.Count; i++)
					{
						if (levelsList.levelItems[i].Selected)
						{
							levelItem = levelsList.levelItems[i];
							break;
						}
					}
					if (levelItem != null)
					{
						flag = true;
						if (levelItem != lastSelected)
						{
							awakeCounter = 0;
							sleepCounter = 0;
							if (levelItem.thumbLoaded)
							{
								levelName = levelItem.name;
								imageSprite.element = Futile.atlasManager.GetElementWithName(levelName + "_Thumb");
								imageSprite.color = new Color(1f, 1f, 1f);
							}
						}
						lastSelected = levelItem;
					}
					else
					{
						sleepCounter = 0;
					}
				}
				if (flag && ((float)awakeCounter > 60f || totFade > 0f) && sleepCounter < 200)
				{
					totFade = Custom.LerpAndTick(totFade, 1f, 0.03f, 1f / 30f);
				}
				else
				{
					totFade = Custom.LerpAndTick(totFade, 0f, 0.015f, 1f / 60f);
				}
				if (menu.manager.menuesMouseMode)
				{
					yPos = Mathf.Clamp(Futile.mousePosition.y, levelsList.pos.y + size.y * 0.5f, levelsList.pos.y + levelsList.size.y - size.y * 0.5f);
					goalYPos = yPos;
				}
				else
				{
					if (lastSelected != null && lastSelected.active)
					{
						goalYPos = Mathf.Clamp(lastSelected.DrawY(1f) + lastSelected.size.y * 0.5f, levelsList.pos.y + size.y * 0.5f, levelsList.pos.y + levelsList.size.y - size.y * 0.5f);
					}
					yPos = Custom.LerpAndTick(yPos, goalYPos, 0.09f, 1.25f);
				}
				pos.y = yPos - levelsList.pos.y - size.y * 0.5f + 0.01f;
				base.Update();
			}

			public override void GrafUpdate(float timeStacker)
			{
				imageSprite.x = DrawX(1f) + DrawSize(1f).x / 2f;
				imageSprite.y = DrawY(1f) + DrawSize(1f).y / 2f;
				float num = Custom.SCurve(Mathf.Lerp(lastTotFade, totFade, timeStacker), 0.75f);
				imageSprite.alpha = Mathf.Pow(num, 0.7f);
				base.GrafUpdate(timeStacker);
				for (int i = 0; i < 9; i++)
				{
					roundedRect.sprites[i].color = Color.black;
					roundedRect.sprites[i].alpha = 0.5f * Mathf.Pow(num, 1.5f);
					roundedRect.sprites[i].isVisible = true;
				}
				Color color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
				for (int j = 9; j < 17; j++)
				{
					roundedRect.sprites[j].color = color;
					roundedRect.sprites[j].alpha = num;
					roundedRect.sprites[j].isVisible = true;
				}
			}

			public override void RemoveSprites()
			{
				imageSprite.RemoveFromContainer();
				base.RemoveSprites();
			}
		}

		public List<LevelItem> levelItems;

		public ScrollButton scrollUpButton;

		public ScrollButton scrollDownButton;

		public SymbolButton[] sideButtons;

		public FContainer dividerContainer;

		public FSprite[] rightHandLines;

		private float showThumbs;

		private float lastShowThumbs;

		public float floatScrollPos;

		public float floatScrollVel;

		public VerticalSlider scrollSlider;

		private float sliderValue;

		private float sliderValueCap;

		private bool sliderPulled;

		private bool shortList;

		public SymbolButton ShowThumbsButton => sideButtons[0];

		public virtual int ScrollPos { get; set; }

		public virtual int TotalItems => 0;

		public virtual bool ShowThumbsStatus { get; set; }

		public int MaxVisibleItems => (int)(size.y / base.OneListItemHeight + 0.4999f * showThumbs);

		public int LastPossibleScroll => Math.Max(0, TotalItems - (MaxVisibleItems - 1));

		public override float ShowThumbs(float timeStacker)
		{
			return Custom.SCurve(Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastShowThumbs, showThumbs, timeStacker)), 0.7f), 0.3f);
		}

		public float ValueOfSlider(Slider slider)
		{
			return 1f - sliderValue;
		}

		public void SliderSetValue(Slider slider, float setValue)
		{
			sliderValue = 1f - setValue;
			sliderPulled = true;
		}

		public float StepsDownOfItem(int itemIndex)
		{
			float num = 0f;
			for (int i = 0; i <= Math.Min(itemIndex, levelItems.Count - 1); i++)
			{
				num += ((i > 0) ? Mathf.Pow(Custom.SCurve(1f - levelItems[i - 1].fadeAway, 0.3f), 0.5f) : 1f);
			}
			return num;
		}

		public float IdealYPosForItem(int itemIndex)
		{
			float num = StepsDownOfItem(itemIndex);
			num -= floatScrollPos;
			if (shortList)
			{
				num += 0.5f * (1f - showThumbs);
			}
			return size.y - num * base.OneListItemHeight;
		}

		public void ConstrainScroll()
		{
			if (ScrollPos > LastPossibleScroll)
			{
				ScrollPos = LastPossibleScroll;
			}
			if (ScrollPos < 0)
			{
				ScrollPos = 0;
			}
		}

		public LevelsList(Menu menu, MenuObject owner, Vector2 pos, int extraSideButtons, bool shortList)
			: base(menu, owner, pos, new Vector2(156f, shortList ? 370f : 460f))
		{
			this.shortList = shortList;
			myContainer = new FContainer();
			owner.Container.AddChild(myContainer);
			levelItems = new List<LevelItem>();
			dividerContainer = new FContainer();
			Container.AddChild(dividerContainer);
			levelsContainer = new FContainer();
			Container.AddChild(levelsContainer);
			scrollUpButton = new ScrollButton(menu, this, "UP", new Vector2(0.01f + size.x / 2f - 10f, 0.01f + size.y + (shortList ? 90f : 0f)), 0);
			subObjects.Add(scrollUpButton);
			scrollDownButton = new ScrollButton(menu, this, "DOWN", new Vector2(0.01f + size.x / 2f - 10f, -25.99f), 2);
			subObjects.Add(scrollDownButton);
			rightHandLines = new FSprite[2 + extraSideButtons + (shortList ? 1 : 0)];
			for (int i = 0; i < rightHandLines.Length; i++)
			{
				rightHandLines[i] = new FSprite("pixel");
				rightHandLines[i].anchorX = 0f;
				rightHandLines[i].anchorY = 0f;
				rightHandLines[i].scaleX = 2f;
				Container.AddChild(rightHandLines[i]);
			}
			sideButtons = new SymbolButton[1 + extraSideButtons];
			sideButtons[0] = new SymbolButton(menu, this, ShowThumbsStatus ? "Menu_Symbol_Show_Thumbs" : "Menu_Symbol_Show_List", "THUMBS", new Vector2(size.x - 8f + 0.01f, 14.01f));
			subObjects.Add(sideButtons[0]);
			scrollSlider = new VerticalSlider(menu, this, "Slider", new Vector2(-16f, 9f), new Vector2(30f, size.y - 40f), Slider.SliderID.LevelsListScroll, subtleSlider: true);
			subObjects.Add(scrollSlider);
			floatScrollPos = ScrollPos;
			showThumbs = (ShowThumbsStatus ? 1f : 0f);
			lastShowThumbs = showThumbs;
			if (menu.manager.rainWorld.options.ScreenSize.x > 1280f)
			{
				subObjects.Add(new LevelPreview(menu, this, this is LevelsPlaylist));
			}
		}

		public void AddLevelItem(LevelItem item)
		{
			item.pos.x = size.x / 2f - item.size.x / 2f;
			item.pos.y = IdealYPosForItem(levelItems.Count);
			levelItems.Add(item);
			subObjects.Add(item);
		}

		public void RemoveLevelItem(LevelItem item)
		{
			item.RemoveSprites();
			RemoveSubObject(item);
			levelItems.Remove(item);
			ConstrainScroll();
		}

		public override void RemoveSprites()
		{
			dividerContainer.RemoveFromContainer();
			base.RemoveSprites();
		}

		public override void Update()
		{
			base.Update();
			if (MouseOver && menu.manager.menuesMouseMode && menu.mouseScrollWheelMovement != 0)
			{
				AddScroll(menu.mouseScrollWheelMovement);
			}
			for (int i = 0; i < levelItems.Count; i++)
			{
				levelItems[i].pos.y = IdealYPosForItem(i) + (((levelItems[i].dividerBelow != null) ? 3f : 0f) - ((levelItems[i].dividerAbove != null) ? 3f : 0f)) * showThumbs;
			}
			lastShowThumbs = showThumbs;
			showThumbs = Custom.LerpAndTick(showThumbs, ShowThumbsStatus ? 1f : 0f, 0.015f, 1f / 30f);
			if (showThumbs > 0f && showThumbs < 1f)
			{
				ConstrainScroll();
			}
			scrollDownButton.buttonBehav.greyedOut = ScrollPos == LastPossibleScroll;
			scrollUpButton.buttonBehav.greyedOut = ScrollPos == 0;
			float num = ScrollPos;
			if (ScrollPos > 0 && ScrollPos == Math.Max(0, levelItems.Count - (MaxVisibleItems - 1)))
			{
				for (int j = ScrollPos; j < levelItems.Count; j++)
				{
					num -= levelItems[j].fadeAway;
				}
			}
			floatScrollPos = Custom.LerpAndTick(floatScrollPos, num, 0.01f, 0.01f);
			floatScrollVel *= Custom.LerpMap(Math.Abs(num - floatScrollPos), 0.25f, 1.5f, 0.45f, 0.99f);
			floatScrollVel += Mathf.Clamp(num - floatScrollPos, -2.5f, 2.5f) / 2.5f * 0.15f;
			floatScrollVel = Mathf.Clamp(floatScrollVel, -1.2f, 1.2f);
			floatScrollPos += floatScrollVel;
			sliderValueCap = Custom.LerpAndTick(sliderValueCap, LastPossibleScroll, 0.02f, (float)levelItems.Count / 40f);
			if (LastPossibleScroll == 0)
			{
				sliderValue = Custom.LerpAndTick(sliderValue, 0.5f, 0.02f, 0.05f);
				scrollSlider.buttonBehav.greyedOut = true;
				return;
			}
			scrollSlider.buttonBehav.greyedOut = false;
			if (sliderPulled)
			{
				floatScrollPos = Mathf.Lerp(0f, sliderValueCap, sliderValue);
				ScrollPos = Custom.IntClamp(Mathf.RoundToInt(floatScrollPos), 0, LastPossibleScroll);
				sliderPulled = false;
			}
			else
			{
				sliderValue = Custom.LerpAndTick(sliderValue, Mathf.InverseLerp(0f, sliderValueCap, floatScrollPos), 0.02f, 0.05f);
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			for (int i = 0; i < rightHandLines.Length - (shortList ? 1 : 0); i++)
			{
				rightHandLines[i].x = DrawX(timeStacker) + size.x + 0.01f;
				float num = ((i != 0) ? (sideButtons[i - 1].DrawY(timeStacker) + sideButtons[i - 1].DrawSize(timeStacker).y + 0.01f) : (DrawY(timeStacker) + 9.01f));
				float num2 = ((i != rightHandLines.Length - 1 - (shortList ? 1 : 0)) ? (sideButtons[i].DrawY(timeStacker) + 0.01f) : (DrawY(timeStacker) + DrawSize(timeStacker).y + 0.01f + (shortList ? 20f : (-11f))));
				rightHandLines[i].y = num;
				rightHandLines[i].scaleY = num2 - num;
				rightHandLines[i].color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			}
			if (shortList)
			{
				rightHandLines[rightHandLines.Length - 1].x = DrawX(timeStacker) - 2f + 0.01f;
				rightHandLines[rightHandLines.Length - 1].y = DrawY(timeStacker) + size.y + 0.01f - 11f;
				rightHandLines[rightHandLines.Length - 1].scaleY = 30f;
				rightHandLines[rightHandLines.Length - 1].color = scrollSlider.MyColor(timeStacker);
			}
		}

		public virtual void ItemClicked(int index)
		{
		}

		public void AddScroll(int scrollDir)
		{
			ScrollPos += scrollDir;
			ConstrainScroll();
		}

		public override void Singal(MenuObject sender, string message)
		{
			base.Singal(sender, message);
			switch (message)
			{
			case "UP":
				AddScroll(-1);
				break;
			case "DOWN":
				AddScroll(1);
				break;
			case "THUMBS":
				ShowThumbsStatus = !ShowThumbsStatus;
				ShowThumbsButton.UpdateSymbol(ShowThumbsStatus ? "Menu_Symbol_Show_Thumbs" : "Menu_Symbol_Show_List");
				menu.PlaySound(ShowThumbsStatus ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
				break;
			}
		}

		public void BumpVisible()
		{
			if (ShowThumbs(1f) < 0.5f)
			{
				return;
			}
			for (int i = 0; i < levelItems.Count; i++)
			{
				if (levelItems[i].active)
				{
					(owner as LevelSelector).BumpUpThumbnailLoad(levelItems[i].name);
				}
			}
		}
	}

	public class AllLevelsSelectionList : LevelsList
	{
		public List<string> AllLevelsList => (owner as LevelSelector).GetMultiplayerMenu.allLevels;

		public override int TotalItems => AllLevelsList.Count;

		public override int ScrollPos
		{
			get
			{
				return (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.allLevelsScroll;
			}
			set
			{
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.allLevelsScroll = value;
			}
		}

		public override bool ShowThumbsStatus
		{
			get
			{
				return (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.allLevelsThumbs;
			}
			set
			{
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.allLevelsThumbs = value;
			}
		}

		public AllLevelsSelectionList(Menu menu, LevelSelector owner, Vector2 pos, bool shortList)
			: base(menu, owner, pos, 0, shortList)
		{
			for (int i = 0; i < AllLevelsList.Count; i++)
			{
				AddLevelItem(new LevelItem(menu, this, AllLevelsList[i]));
			}
			for (int j = 0; j < levelItems.Count - 1; j++)
			{
				if (owner.GetMultiplayerMenu.multiplayerUnlocks.LevelListSortNumber(AllLevelsList[j]) != owner.GetMultiplayerMenu.multiplayerUnlocks.LevelListSortNumber(AllLevelsList[j + 1]))
				{
					levelItems[j].AddDividers(levelItems[j + 1]);
				}
			}
		}

		public override void ItemClicked(int index)
		{
			base.ItemClicked(index);
			(owner as LevelSelector).LevelToPlaylist(AllLevelsList[index]);
		}

		public override void Singal(MenuObject sender, string message)
		{
			base.Singal(sender, message);
		}
	}

	public class LevelsPlaylist : LevelsList
	{
		public MenuLabel[] labels;

		private float[,] labelsFade;

		public int clearAllCounter;

		public int mismatchCounter;

		public List<string> PlayList => (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList;

		public override int TotalItems => PlayList.Count;

		public override int ScrollPos
		{
			get
			{
				return (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playListScroll;
			}
			set
			{
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playListScroll = value;
			}
		}

		public override bool ShowThumbsStatus
		{
			get
			{
				return (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playListThumbs;
			}
			set
			{
				(owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playListThumbs = value;
			}
		}

		private SymbolButton ClearButton => sideButtons[1];

		private SymbolButton ShuffleButton => sideButtons[2];

		public LevelsPlaylist(Menu menu, LevelSelector owner, Vector2 pos)
			: base(menu, owner, pos, 2, shortList: false)
		{
			for (int i = 0; i < PlayList.Count; i++)
			{
				AddLevelItem(new LevelItem(menu, this, PlayList[i]));
			}
			sideButtons[1] = new SymbolButton(menu, this, "Menu_Symbol_Clear_All", "CLEAR", sideButtons[0].pos + new Vector2(0f, 30f));
			sideButtons[1].maintainOutlineColorWhenGreyedOut = true;
			subObjects.Add(sideButtons[1]);
			sideButtons[2] = new SymbolButton(menu, this, owner.GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist ? "Menu_Symbol_Shuffle" : "Menu_Symbol_Dont_Shuffle", "SHUFFLE", sideButtons[1].pos + new Vector2(0f, 30f));
			subObjects.Add(sideButtons[2]);
			labels = new MenuLabel[2];
			labelsFade = new float[labels.Length, 2];
			for (int j = 0; j < 2; j++)
			{
				labels[j] = new MenuLabel(menu, this, "", sideButtons[j + 1].pos + new Vector2(10f, -3f), new Vector2(50f, 30f), bigText: false);
				labels[j].label.alignment = FLabelAlignment.Left;
				subObjects.Add(labels[j]);
			}
			labels[0].text = menu.Translate("Clear playlist");
		}

		public override void LevelItemFaded(LevelItem item)
		{
			RemoveLevelItem(item);
		}

		public override void Update()
		{
			base.Update();
			ClearButton.buttonBehav.greyedOut = levelItems.Count == 0 || clearAllCounter > 0;
			for (int i = 0; i < labels.Length; i++)
			{
				labelsFade[i, 1] = labelsFade[i, 0];
				if (sideButtons[i + 1].Selected)
				{
					labelsFade[i, 0] = Custom.LerpAndTick(labelsFade[i, 0], 0.33f, 0.04f, 1f / 60f);
					switch (i)
					{
					case 1:
						labels[i].text = ((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist ? menu.Translate("Shuffling levels") : menu.Translate("Playing in order"));
						break;
					case 2:
					{
						string text = "";
						text = (((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.levelRepeats == 1) ? menu.Translate("round per level") : (((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.levelRepeats < 2 && (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.levelRepeats > 4) ? menu.Translate("rounds per level") : menu.Translate("rounds per level-ru2")));
						labels[i].text = (owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.levelRepeats + " " + text;
						break;
					}
					}
				}
				else
				{
					labelsFade[i, 0] = Custom.LerpAndTick(labelsFade[i, 0], 0f, 0.04f, 1f / 60f);
				}
			}
			if (clearAllCounter > 0)
			{
				clearAllCounter--;
				if (clearAllCounter >= 1 || levelItems.Count <= 0)
				{
					return;
				}
				clearAllCounter = 4;
				bool flag = false;
				for (int num = levelItems.Count - 1; num >= 0; num--)
				{
					if (levelItems[num].fadeAway == 0f)
					{
						flag = true;
						levelItems[num].StartFadeAway();
						int num2 = -1;
						for (int num3 = PlayList.Count - 1; num3 >= 0; num3--)
						{
							if (PlayList[num3] == levelItems[num].name)
							{
								num2 = num3;
								break;
							}
						}
						if (num2 > -1)
						{
							(owner as LevelSelector).LevelFromPlayList(num2);
						}
						ConstrainScroll();
						break;
					}
				}
				if (!flag)
				{
					PlayList.Clear();
				}
			}
			else if (levelItems.Count != PlayList.Count)
			{
				mismatchCounter++;
				if (mismatchCounter == 80)
				{
					ResolveMismatch();
					mismatchCounter = 0;
				}
			}
			else
			{
				mismatchCounter = 0;
			}
		}

		private void ResolveMismatch()
		{
			for (int num = levelItems.Count - 1; num >= 0; num--)
			{
				RemoveLevelItem(levelItems[num]);
			}
			for (int i = 0; i < PlayList.Count; i++)
			{
				AddLevelItem(new LevelItem(menu, this, PlayList[i]));
			}
			ConstrainScroll();
		}

		public override void ItemClicked(int index)
		{
			base.ItemClicked(index);
			(owner as LevelSelector).LevelFromPlayList(index);
			menu.selectedObject = null;
			if (!menu.manager.menuesMouseMode)
			{
				int num = index - 1;
				while (num >= 0 && num < levelItems.Count)
				{
					if (levelItems[num].fadeAway == 0f)
					{
						menu.selectedObject = levelItems[num];
						break;
					}
					num--;
				}
				if (menu.selectedObject == null)
				{
					for (int i = index + 1; i >= 0 && i < levelItems.Count; i++)
					{
						if (levelItems[i].fadeAway == 0f)
						{
							menu.selectedObject = levelItems[i];
							break;
						}
					}
				}
			}
			levelItems[index].StartFadeAway();
		}

		public override void Singal(MenuObject sender, string message)
		{
			base.Singal(sender, message);
			switch (message)
			{
			case "SHUFFLE":
				ShuffleButton.UpdateSymbol((owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist ? "Menu_Symbol_Shuffle" : "Menu_Symbol_Dont_Shuffle");
				labelsFade[1, 0] = 1f;
				break;
			case "REPEATS":
				labelsFade[2, 0] = 1f;
				break;
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i].label.alpha = Mathf.Lerp(labelsFade[i, 1], labelsFade[i, 0], timeStacker);
			}
		}
	}

	public class ScrollButton : SymbolButton
	{
		public int direction;

		private int heldCounter;

		public ScrollButton(Menu menu, MenuObject owner, string singalText, Vector2 pos, int direction)
			: base(menu, owner, "Menu_Symbol_Arrow", singalText, pos)
		{
			this.direction = direction;
		}

		public override void Update()
		{
			base.Update();
			if (buttonBehav.clicked && !buttonBehav.greyedOut)
			{
				heldCounter++;
				if (heldCounter > 20 && heldCounter % 4 == 0)
				{
					menu.PlaySound(SoundID.MENU_Scroll_Tick);
					Singal(this, signalText);
					buttonBehav.sin = 0.5f;
				}
			}
			else
			{
				heldCounter = 0;
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbolSprite.rotation = 90f * (float)direction;
		}

		public override void Clicked()
		{
			if (heldCounter < 20)
			{
				menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
				Singal(this, signalText);
			}
		}
	}

	private static readonly AGLog<LevelSelector> Log = new AGLog<LevelSelector>();

	public AllLevelsSelectionList allLevelsList;

	public LevelsPlaylist levelsPlaylist;

	public SingleLevelDisplay singleLevelDisplay;

	public static int ThumbWidth = 100;

	public static int ThumbHeight = 50;

	protected int thumbLoadDelay;

	private bool bumpVisible = true;

	public MultiplayerMenu GetMultiplayerMenu => menu as MultiplayerMenu;

	public LevelSelector(Menu menu, MenuObject owner, bool singleLevelGameType)
		: base(menu, owner, new Vector2(0f, 0f))
	{
		Vector2 vector = new Vector2(102f, -69f);
		allLevelsList = new AllLevelsSelectionList(menu, this, new Vector2(100f, 200f) + vector, singleLevelGameType);
		subObjects.Add(allLevelsList);
		if (singleLevelGameType)
		{
			singleLevelDisplay = new SingleLevelDisplay(menu, this, new Vector2(100f, 571f) + vector);
			subObjects.Add(singleLevelDisplay);
			BumpUpThumbnailLoad(singleLevelDisplay.LevelName);
		}
		else
		{
			levelsPlaylist = new LevelsPlaylist(menu, this, new Vector2(290f, 200f) + vector);
			subObjects.Add(levelsPlaylist);
		}
		if (!GetMultiplayerMenu.multiplayerUnlocks.unlockAll && !GetMultiplayerMenu.multiplayerUnlocks.unlockNoSpoilers && (!ModManager.MSC || !global::MoreSlugcats.MoreSlugcats.chtUnlockArenas.Value))
		{
			int num = -1;
			for (int i = 0; i < allLevelsList.AllLevelsList.Count; i++)
			{
				if (MultiplayerUnlocks.LevelLockID(allLevelsList.AllLevelsList[i]) != MultiplayerUnlocks.LevelUnlockID.Default && !GetMultiplayerMenu.manager.rainWorld.progression.miscProgressionData.everPlayedArenaLevels.Contains(allLevelsList.AllLevelsList[i]))
				{
					allLevelsList.levelItems[i].neverPlayed = 1000 - i;
					allLevelsList.levelItems[i].label.text = "*" + allLevelsList.levelItems[i].label.text + "*";
					if (num < 0)
					{
						num = i;
					}
				}
			}
			if (num >= 0 && GetMultiplayerMenu.GetGameTypeSetup.gameType.Index >= 0 && !GetMultiplayerMenu.GetArenaSetup.scrolledToShowNewLevels[GetMultiplayerMenu.GetGameTypeSetup.gameType.Index])
			{
				allLevelsList.ScrollPos = num + 1 - allLevelsList.MaxVisibleItems / 2;
				GetMultiplayerMenu.GetArenaSetup.scrolledToShowNewLevels[GetMultiplayerMenu.GetGameTypeSetup.gameType.Index] = true;
			}
		}
		allLevelsList.ConstrainScroll();
	}

	public override void Update()
	{
		base.Update();
		if (bumpVisible)
		{
			bumpVisible = false;
			allLevelsList.BumpVisible();
			if (levelsPlaylist != null)
			{
				levelsPlaylist.BumpVisible();
			}
		}
		if (thumbLoadDelay > 0)
		{
			thumbLoadDelay--;
		}
		if (GetMultiplayerMenu.thumbsToBeLoaded.Count <= 0 || thumbLoadDelay >= 1)
		{
			return;
		}
		string text = GetMultiplayerMenu.thumbsToBeLoaded[0];
		GetMultiplayerMenu.thumbsToBeLoaded.RemoveAt(0);
		bool flag = false;
		string text2 = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + text + "_Thumb.png");
		string text3;
		if (File.Exists(text2))
		{
			text3 = text2;
			flag = true;
		}
		else
		{
			text3 = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + text + "_1.png");
			flag = false;
		}
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		if (File.Exists(text3))
		{
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text3, clampWrapMode: true, crispPixels: false);
			if (!flag)
			{
				TextureScale.Bilinear(texture2D, ThumbWidth, ThumbHeight);
			}
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					texture2D.SetPixel((i <= 1) ? (ThumbWidth - 1) : 0, (i % 2 == 0) ? j : (ThumbHeight - 1 - j), new Color(0f, 0f, 0f, 0f));
				}
				for (int k = 0; k < 3; k++)
				{
					texture2D.SetPixel((i > 1) ? 1 : (ThumbWidth - 2), (i % 2 == 0) ? k : (ThumbHeight - 1 - k), new Color(0f, 0f, 0f, 0f));
				}
				for (int l = 0; l < 2; l++)
				{
					texture2D.SetPixel((i > 1) ? 2 : (ThumbWidth - 3), (i % 2 == 0) ? l : (ThumbHeight - 1 - l), new Color(0f, 0f, 0f, 0f));
				}
				texture2D.SetPixel((i > 1) ? 3 : (ThumbWidth - 4), (i % 2 != 0) ? (ThumbHeight - 1) : 0, new Color(0f, 0f, 0f, 0f));
				texture2D.SetPixel((i > 1) ? 4 : (ThumbWidth - 5), (i % 2 != 0) ? (ThumbHeight - 1) : 0, new Color(0f, 0f, 0f, 0f));
			}
		}
		texture2D.filterMode = FilterMode.Point;
		texture2D.Apply();
		GetMultiplayerMenu.loadedThumbTextures.Add(text);
		HeavyTexturesCache.LoadAndCacheAtlasFromTexture(text + "_Thumb", texture2D, textureFromAsset: false);
		for (int m = 0; m < subObjects.Count; m++)
		{
			if (subObjects[m] is LevelsList)
			{
				for (int n = 0; n < (subObjects[m] as LevelsList).levelItems.Count; n++)
				{
					if ((subObjects[m] as LevelsList).levelItems[n].name == text)
					{
						(subObjects[m] as LevelsList).levelItems[n].ThumbnailHasBeenLoaded();
					}
				}
			}
			else if (subObjects[m] is SingleLevelDisplay && (subObjects[m] as SingleLevelDisplay).currentLevelItem.name == text)
			{
				(subObjects[m] as SingleLevelDisplay).currentLevelItem.ThumbnailHasBeenLoaded();
			}
		}
		thumbLoadDelay = 2;
	}

	public bool IsThumbnailLoaded(string levelName)
	{
		for (int i = 0; i < GetMultiplayerMenu.loadedThumbTextures.Count; i++)
		{
			if (GetMultiplayerMenu.loadedThumbTextures[i] == levelName)
			{
				return true;
			}
		}
		return false;
	}

	public void BumpUpThumbnailLoad(string levelName)
	{
		if (GetMultiplayerMenu.thumbsToBeLoaded.Count > 0 && GetMultiplayerMenu.thumbsToBeLoaded[0] == levelName)
		{
			return;
		}
		for (int i = 0; i < GetMultiplayerMenu.loadedThumbTextures.Count; i++)
		{
			if (GetMultiplayerMenu.loadedThumbTextures[i] == levelName)
			{
				return;
			}
		}
		for (int num = GetMultiplayerMenu.thumbsToBeLoaded.Count - 1; num >= 0; num--)
		{
			if (GetMultiplayerMenu.thumbsToBeLoaded[num] == levelName)
			{
				GetMultiplayerMenu.thumbsToBeLoaded.RemoveAt(num);
			}
		}
		GetMultiplayerMenu.thumbsToBeLoaded.Insert(0, levelName);
	}

	public void LevelToPlaylist(string levelName)
	{
		if (singleLevelDisplay != null)
		{
			(menu as MultiplayerMenu).GetGameTypeSetup.playList.Clear();
			(menu as MultiplayerMenu).GetGameTypeSetup.playList.Add(levelName);
		}
		else if (levelsPlaylist != null)
		{
			(menu as MultiplayerMenu).GetGameTypeSetup.playList.Add(levelName);
			levelsPlaylist.AddLevelItem(new LevelItem(menu, levelsPlaylist, levelName));
			levelsPlaylist.ScrollPos = levelsPlaylist.LastPossibleScroll;
			levelsPlaylist.ConstrainScroll();
		}
		menu.PlaySound(SoundID.MENU_Add_Level);
	}

	public void LevelFromPlayList(int index)
	{
		if (index >= 0 && index < GetMultiplayerMenu.GetGameTypeSetup.playList.Count)
		{
			GetMultiplayerMenu.GetGameTypeSetup.playList.RemoveAt(index);
			menu.PlaySound(SoundID.MENU_Remove_Level);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "REPEATS":
			GetMultiplayerMenu.GetGameTypeSetup.levelRepeats++;
			if (GetMultiplayerMenu.GetGameTypeSetup.levelRepeats > 5)
			{
				GetMultiplayerMenu.GetGameTypeSetup.levelRepeats = 1;
			}
			menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			break;
		case "SHUFFLE":
			GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist = !GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist;
			menu.PlaySound(GetMultiplayerMenu.GetGameTypeSetup.shufflePlaylist ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
			break;
		case "CLEAR":
			levelsPlaylist.clearAllCounter = 1;
			menu.PlaySound((levelsPlaylist.levelItems.Count > 0) ? SoundID.MENU_Button_Standard_Button_Pressed : SoundID.MENU_Greyed_Out_Button_Clicked);
			break;
		}
	}
}
