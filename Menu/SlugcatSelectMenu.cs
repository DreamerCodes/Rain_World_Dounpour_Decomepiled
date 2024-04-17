using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HUD;
using JollyCoop;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SlugcatSelectMenu : Menu, CheckBox.IOwnCheckBox
{
	public class SaveGameData
	{
		public int karma;

		public int karmaCap;

		public int food;

		public int cycle;

		public bool karmaReinforced;

		public bool hasGlow;

		public bool hasMark;

		public bool redsExtraCycles;

		public bool redsDeath;

		public bool ascended;

		public string shelterName;

		public bool altEnding;

		public bool hasRobo;

		public bool pebblesEnergyTaken;

		public bool moonGivenRobe;

		public int gameTimeAlive;

		public int gameTimeDead;
	}

	public abstract class SlugcatPage : Page
	{
		public InteractiveMenuScene slugcatImage;

		public SlugcatStats.Name slugcatNumber;

		public FSprite markSquare;

		public FSprite markGlow;

		public FSprite glowSpriteA;

		public FSprite glowSpriteB;

		public float markAlpha;

		public float lastMarkAlpha;

		public float markFlicker;

		public float glowAlpha;

		public float lastGlowAlpha;

		public float glowSinCounter;

		public float inPosition;

		public float lastInposition;

		public Vector2 imagePos;

		public Vector2 sceneOffset;

		public Vector2 markOffset;

		public Vector2 glowOffset;

		private float slugcatDepth;

		public Color effectColor;

		public int SlugcatPageIndex => index - 1;

		public float MidXpos => menu.manager.rainWorld.screenSize.x / 2f;

		public float ScrollMagnitude => 500f;

		public virtual bool HasMark => false;

		public virtual bool HasGlow => false;

		private int MinOffset => -1;

		private int MaxOffset => 1;

		public SlugcatPage(Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
			: base(menu, owner, "Slugcat_Page_" + slugcatNumber, pageIndex)
		{
			this.slugcatNumber = slugcatNumber;
			effectColor = PlayerGraphics.DefaultSlugcatColor(slugcatNumber);
			if (slugcatNumber == SlugcatStats.Name.Red)
			{
				effectColor = Color.Lerp(effectColor, Color.red, 0.2f);
			}
		}

		public void AddImage(bool ascended)
		{
			imagePos = new Vector2(683f, 484f);
			sceneOffset = default(Vector2);
			slugcatDepth = 1f;
			MenuScene.SceneID sceneID = MenuScene.SceneID.Slugcat_White;
			if (slugcatNumber == SlugcatStats.Name.White)
			{
				sceneID = ((!ascended) ? MenuScene.SceneID.Slugcat_White : MenuScene.SceneID.Ghost_White);
				sceneOffset = new Vector2(-10f, 100f);
				slugcatDepth = 3.1000001f;
				markOffset = new Vector2(-15f, -2f);
				glowOffset = new Vector2(-30f, -50f);
			}
			else if (slugcatNumber == SlugcatStats.Name.Yellow)
			{
				sceneID = ((!ascended) ? MenuScene.SceneID.Slugcat_Yellow : MenuScene.SceneID.Ghost_Yellow);
				sceneOffset = new Vector2(10f, 75f);
				slugcatDepth = 3f;
				markOffset = new Vector2(24f, -19f);
				glowOffset = new Vector2(0f, -50f);
			}
			else if (slugcatNumber == SlugcatStats.Name.Red)
			{
				sceneID = (ascended ? MenuScene.SceneID.Ghost_Red : ((!(menu as SlugcatSelectMenu).redIsDead) ? MenuScene.SceneID.Slugcat_Red : MenuScene.SceneID.Slugcat_Dead_Red));
				sceneOffset = new Vector2(10f, 45f);
				slugcatDepth = 2.7f;
				markOffset = new Vector2(-3f, -73f);
				glowOffset = new Vector2(-20f, -90f);
			}
			else if (ModManager.MSC)
			{
				if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
				{
					sceneID = ((!ascended) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Inv : MoreSlugcatsEnums.MenuSceneID.End_Inv);
					sceneOffset = new Vector2(-10f, 100f);
					slugcatDepth = 3.1000001f;
					markOffset = new Vector2(-15f, -2f);
					glowOffset = new Vector2(-30f, -50f);
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					sceneID = (ascended ? MoreSlugcatsEnums.MenuSceneID.End_Rivulet : ((!(this is SlugcatPageContinue) || !(this as SlugcatPageContinue).saveGameData.pebblesEnergyTaken) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet : MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet_Cell));
					sceneOffset = new Vector2(10f, 75f);
					slugcatDepth = 3f;
					markOffset = new Vector2(-120f, -144f);
					glowOffset = new Vector2(0f, -25f);
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					sceneID = (ascended ? MoreSlugcatsEnums.MenuSceneID.End_Artificer : ((!(this is SlugcatPageContinue) || !(this as SlugcatPageContinue).saveGameData.hasRobo) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer : MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo));
					sceneOffset = new Vector2(10f, 75f);
					slugcatDepth = 3f;
					markOffset = new Vector2(24f, -19f);
					glowOffset = new Vector2(0f, -50f);
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					sceneID = ((ascended || menu.manager.rainWorld.progression.miscProgressionData.hasDoneHeartReboot || menu.manager.rainWorld.progression.miscProgressionData.beaten_Saint) ? MoreSlugcatsEnums.MenuSceneID.End_Saint : ((!(this is SlugcatPageContinue) || (this as SlugcatPageContinue).saveGameData.karmaCap != 9) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint : MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint_Max));
					sceneOffset = new Vector2(10f, 75f);
					slugcatDepth = 3f;
					markOffset = new Vector2(14f, 70f);
					glowOffset = new Vector2(0f, 0f);
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					sceneID = ((!ascended) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Spear : MoreSlugcatsEnums.MenuSceneID.End_Spear);
					sceneOffset = new Vector2(10f, 75f);
					slugcatDepth = 3f;
					markOffset = new Vector2(-100f, 10f);
					glowOffset = new Vector2(-24f, -24f);
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
				{
					sceneID = ((!ascended) ? MoreSlugcatsEnums.MenuSceneID.Slugcat_Gourmand : MoreSlugcatsEnums.MenuSceneID.End_Gourmand);
					sceneOffset = new Vector2(10f, 75f);
					slugcatDepth = 3f;
					markOffset = new Vector2(0f, -19f);
					glowOffset = new Vector2(0f, -10f);
				}
			}
			sceneOffset.x -= (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
			slugcatImage = new InteractiveMenuScene(menu, this, sceneID);
			subObjects.Add(slugcatImage);
			if (HasMark)
			{
				markSquare = new FSprite("pixel");
				markSquare.scale = 14f;
				markSquare.color = Color.Lerp(effectColor, Color.white, 0.7f);
				Container.AddChild(markSquare);
				markGlow = new FSprite("Futile_White");
				markGlow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
				markGlow.color = effectColor;
				Container.AddChild(markGlow);
			}
		}

		public void AddAltEndingImage()
		{
			imagePos = new Vector2(683f, 484f);
			sceneOffset = default(Vector2);
			slugcatDepth = 1f;
			MenuScene.SceneID sceneID = MoreSlugcatsEnums.MenuSceneID.AltEnd_Spearmaster;
			if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.AltEnd_Spearmaster;
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				sceneID = ((!menu.manager.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full) ? MoreSlugcatsEnums.MenuSceneID.AltEnd_Gourmand : MoreSlugcatsEnums.MenuSceneID.AltEnd_Gourmand_Full);
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			else if (slugcatNumber == SlugcatStats.Name.White)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.AltEnd_Survivor;
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			else if (slugcatNumber == SlugcatStats.Name.Yellow)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.AltEnd_Monk;
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				sceneID = MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_Portrait;
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				sceneID = ((!(this is SlugcatPageContinue) || !(this as SlugcatPageContinue).saveGameData.moonGivenRobe) ? MoreSlugcatsEnums.MenuSceneID.AltEnd_Rivulet : MoreSlugcatsEnums.MenuSceneID.AltEnd_Rivulet_Robe);
				slugcatDepth = 3f;
				sceneOffset = new Vector2(10f, 75f);
			}
			sceneOffset.x -= (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
			slugcatImage = new InteractiveMenuScene(menu, this, sceneID);
			subObjects.Add(slugcatImage);
		}

		public void AddGlow()
		{
			if (HasGlow)
			{
				glowSpriteB = new FSprite("Futile_White");
				glowSpriteB.shader = menu.manager.rainWorld.Shaders["FlatLightNoisy"];
				Container.AddChild(glowSpriteB);
				glowSpriteA = new FSprite("Futile_White");
				glowSpriteA.shader = menu.manager.rainWorld.Shaders["FlatLightNoisy"];
				Container.AddChild(glowSpriteA);
			}
		}

		public float Scroll(float timeStacker)
		{
			float num = (float)(SlugcatPageIndex - (menu as SlugcatSelectMenu).slugcatPageIndex) - Mathf.Lerp((menu as SlugcatSelectMenu).lastScroll, (menu as SlugcatSelectMenu).scroll, timeStacker);
			if (num < (float)MinOffset)
			{
				num += (float)(menu as SlugcatSelectMenu).slugcatPages.Count;
			}
			else if (num > (float)MaxOffset)
			{
				num -= (float)(menu as SlugcatSelectMenu).slugcatPages.Count;
			}
			return num;
		}

		public float NextScroll(float timeStacker)
		{
			float num = (float)(SlugcatPageIndex - (menu as SlugcatSelectMenu).slugcatPageIndex) - Mathf.Lerp((menu as SlugcatSelectMenu).scroll, (menu as SlugcatSelectMenu).NextScroll, timeStacker);
			if (num < (float)MinOffset)
			{
				num += (float)(menu as SlugcatSelectMenu).slugcatPages.Count;
			}
			else if (num > (float)MaxOffset)
			{
				num -= (float)(menu as SlugcatSelectMenu).slugcatPages.Count;
			}
			return num;
		}

		public float UseAlpha(float timeStacker)
		{
			return Custom.SCurve(Mathf.Clamp01(1f - Mathf.Abs(Scroll(timeStacker))), 0.5f);
		}

		public override void Update()
		{
			base.Update();
			float num = UseAlpha(1f);
			float f = Scroll(1f);
			slugcatImage.cameraRange = 0.5f * num;
			float num2 = Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, 1f - Mathf.Abs(f)), 5f);
			lastInposition = inPosition;
			if (num2 < inPosition)
			{
				inPosition = num2;
			}
			else
			{
				inPosition = Custom.LerpAndTick(inPosition, num2, 0.01f, 0.0033333334f);
			}
			lastMarkAlpha = markAlpha;
			if (HasMark)
			{
				markAlpha = Custom.LerpAndTick(markAlpha, num * inPosition, 0.07f, 0.0033333334f);
				if (UnityEngine.Random.value > num * 0.95f)
				{
					markFlicker = Mathf.Max(markFlicker, UnityEngine.Random.value * Mathf.Lerp(1f, 0.4f, num));
				}
				else
				{
					markFlicker = Mathf.Max(0f, markFlicker - UnityEngine.Random.value * markAlpha / 10f);
				}
				if (Mathf.Pow(UnityEngine.Random.value, 2f) < markFlicker)
				{
					markAlpha = Mathf.Lerp(markAlpha, 1f - UnityEngine.Random.value * markFlicker, UnityEngine.Random.value * inPosition);
				}
			}
			lastGlowAlpha = glowAlpha;
			if (HasGlow)
			{
				glowSinCounter += UnityEngine.Random.value * inPosition;
				glowAlpha = (0.5f + 0.5f * Mathf.Sin(glowSinCounter / 25f)) * inPosition;
				if (UnityEngine.Random.value < 0.0125f)
				{
					glowAlpha *= 1f - 0.2f * Mathf.Pow(UnityEngine.Random.value, 5f);
				}
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			float num = Scroll(timeStacker);
			float num2 = UseAlpha(timeStacker);
			if (HasMark && markSquare != null && markGlow != null)
			{
				float num3 = Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker) * num2;
				if (slugcatNumber == SlugcatStats.Name.Red)
				{
					num3 *= ((this is SlugcatPageContinue) ? Mathf.Pow(Mathf.InverseLerp(4f, 14f, (this as SlugcatPageContinue).saveGameData.cycle), 3.5f) : 0f);
				}
				Vector2 vector = new Vector2(MidXpos + num * ScrollMagnitude, imagePos.y + 150f) + markOffset;
				vector -= slugcatImage.CamPos(timeStacker) * 80f / slugcatDepth;
				markSquare.x = vector.x;
				markSquare.y = vector.y;
				markSquare.alpha = Mathf.Pow(num3, 0.75f);
				markGlow.x = vector.x;
				markGlow.y = vector.y;
				markGlow.scale = Mathf.Lerp(3f, 3.3f, Mathf.Pow(num3, 0.2f)) + (HasGlow ? (-0.5f) : 0f) + Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value) * markFlicker;
				markGlow.alpha = ((slugcatNumber == SlugcatStats.Name.White) ? 0.4f : 0.6f) * Mathf.Pow(num3, 0.75f);
			}
			if (HasGlow && glowSpriteA != null && glowSpriteB != null)
			{
				float num4 = Mathf.Lerp(0.8f, 1f, Mathf.Lerp(lastGlowAlpha, glowAlpha, timeStacker)) * num2 * Mathf.Lerp(lastInposition, inPosition, timeStacker);
				Vector2 vector2 = new Vector2(MidXpos + num * ScrollMagnitude, imagePos.y) + glowOffset;
				vector2 -= slugcatImage.CamPos(timeStacker) * 80f / slugcatDepth;
				glowSpriteB.color = Color.Lerp(effectColor, new Color(1f, 1f, 1f), 0.3f * num4);
				glowSpriteB.x = vector2.x;
				glowSpriteB.y = vector2.y;
				glowSpriteB.scale = Mathf.Lerp(20f, 38f, Mathf.Pow(num4, 0.75f));
				glowSpriteB.alpha = Mathf.Pow(num4, 0.25f) * Mathf.Lerp(0.394f, 0.406f, UnityEngine.Random.value * (1f - Mathf.Lerp(lastGlowAlpha, glowAlpha, timeStacker)));
				glowSpriteA.color = Color.Lerp(effectColor, new Color(1f, 1f, 1f), 0.9f * num4);
				glowSpriteA.x = vector2.x;
				glowSpriteA.y = vector2.y;
				glowSpriteA.scale = Mathf.Lerp(10f, 17f, Mathf.Pow(num4, 1.2f));
				glowSpriteA.alpha = num4 * 0.6f;
			}
			for (int i = 0; i < slugcatImage.depthIllustrations.Count; i++)
			{
				Vector2 vector3 = slugcatImage.depthIllustrations[i].pos;
				vector3 -= slugcatImage.CamPos(timeStacker) * 80f / slugcatImage.depthIllustrations[i].depth;
				vector3 += sceneOffset;
				vector3.x += num * ScrollMagnitude;
				slugcatImage.depthIllustrations[i].sprite.x = vector3.x;
				slugcatImage.depthIllustrations[i].sprite.y = vector3.y;
				slugcatImage.depthIllustrations[i].sprite.alpha = slugcatImage.depthIllustrations[i].alpha * num2;
			}
			for (int j = 0; j < slugcatImage.flatIllustrations.Count; j++)
			{
				Vector2 vector4 = slugcatImage.flatIllustrations[j].pos;
				vector4 += sceneOffset;
				vector4.x += num * ScrollMagnitude;
				slugcatImage.flatIllustrations[j].sprite.x = vector4.x;
				slugcatImage.flatIllustrations[j].sprite.y = vector4.y;
				slugcatImage.flatIllustrations[j].sprite.alpha = slugcatImage.flatIllustrations[j].alpha * num2;
			}
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			if (markSquare != null)
			{
				markSquare.RemoveFromContainer();
			}
			if (markGlow != null)
			{
				markGlow.RemoveFromContainer();
			}
			if (glowSpriteA != null)
			{
				glowSpriteA.RemoveFromContainer();
			}
			if (glowSpriteB != null)
			{
				glowSpriteB.RemoveFromContainer();
			}
		}
	}

	public class SlugcatPageNewGame : SlugcatPage
	{
		public MenuLabel difficultyLabel;

		public MenuLabel infoLabel;

		public SlugcatPageNewGame(Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
			: base(menu, owner, pageIndex, slugcatNumber)
		{
			AddImage(ascended: false);
			string text = "";
			string s = "";
			SlugcatSelectMenu slugcatSelectMenu = menu as SlugcatSelectMenu;
			if (slugcatNumber == SlugcatStats.Name.White)
			{
				text = menu.Translate("THE SURVIVOR");
				s = menu.Translate("A nimble omnivore, both predator and prey. Lost in a harsh and indifferent<LINE>land you must make your own way, with wit and caution as your greatest assets.");
			}
			else if (slugcatNumber == SlugcatStats.Name.Yellow)
			{
				text = menu.Translate("THE MONK");
				s = menu.Translate("Weak of body but strong of spirit. In tune with the mysteries of the world and<LINE>empathetic to its creatures, your journey will be a significantly more peaceful one.");
			}
			else if (slugcatNumber == SlugcatStats.Name.Red)
			{
				text = menu.Translate("THE HUNTER");
				if (ModManager.MMF)
				{
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("Strong and quick, requiring a steady diet of meat. But the stomach wont be your only<LINE>concern... this path is one of extreme peril where every cycle eats away at an ominous countdown.") : menu.Translate("Clear the game as Survivor or Monk to unlock."));
				}
				else
				{
					s = menu.Translate("Strong and quick, with a fierce metabolism requiring a steady diet of meat. But the<LINE>stomach wont be your only concern, as the path of the hunter is one of extreme peril.");
					if (menu.manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.English)
					{
						s = "Strong and quick, with a fierce metabolism requiring a steady diet of meat. But the<LINE>stomach won't be your only concern, as the path of the hunter is one of extreme peril.";
					}
				}
			}
			else if (ModManager.MSC)
			{
				if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
				{
					text = "???";
					s = menu.Translate("Thanks Andrew.");
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					text = menu.Translate("THE RIVULET");
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("Breathes underwater, and moves through the world with ease. These adaptations are essential,<LINE>as you'll be pitted against a world of increasingly frequent floods, where time is of the essence.") : menu.Translate("Clear the game as Gourmand, Artificer, or Hunter to unlock."));
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					text = menu.Translate("THE ARTIFICER");
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("A fierce combatant, master of pyrotechnics and explosives. Keen to move up in<LINE>the foodchain, your journey will surely be one lined with constant bloodshed and warfare.") : menu.Translate("Clear the game as Survivor or Monk to unlock."));
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					text = menu.Translate("THE SAINT");
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("Frail and armed with a long tongue. Your journey will be one of perfect enlightenment,<LINE>but walking this path requires patience, caution, and complete attunement with the world.") : menu.Translate("Clear the game as Rivulet and Spearmaster to unlock."));
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					text = menu.Translate("THE SPEARMASTER");
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("An abnormality who feeds using needles pulled from its body. A traveller from a<LINE>far away land; A feeling in your depths sets you out once again, messenger...") : menu.Translate("Clear the game as Gourmand, Artificer, or Hunter to unlock."));
				}
				else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
				{
					text = menu.Translate("THE GOURMAND");
					s = (slugcatSelectMenu.SlugcatUnlocked(slugcatNumber) ? menu.Translate("An indulger of the simpler pleasures in life. Carrying the world in your stomach gives<LINE>many tactical advantages, but comes at an increased cost of sustainability.") : menu.Translate("Clear the game as Survivor or Monk to unlock."));
				}
			}
			s = Custom.ReplaceLineDelimeters(s);
			int num = s.Count((char f) => f == '\n');
			float num2 = 0f;
			if (num > 1)
			{
				num2 = 30f;
			}
			difficultyLabel = new MenuLabel(menu, this, text, new Vector2(-1000f, imagePos.y - 249f + num2), new Vector2(200f, 30f), bigText: true);
			difficultyLabel.label.alignment = FLabelAlignment.Center;
			subObjects.Add(difficultyLabel);
			infoLabel = new MenuLabel(menu, this, s, new Vector2(-1000f, imagePos.y - 249f - 60f + num2 / 2f), new Vector2(400f, 60f), bigText: true);
			infoLabel.label.alignment = FLabelAlignment.Center;
			subObjects.Add(infoLabel);
			if (num > 1)
			{
				imagePos.y += 30f;
				sceneOffset.y += 30f;
			}
			if (!slugcatSelectMenu.SlugcatUnlocked(slugcatNumber))
			{
				difficultyLabel.label.color = Menu.MenuRGB(MenuColors.VeryDarkGrey);
				infoLabel.label.color = Menu.MenuRGB(MenuColors.VeryDarkGrey);
			}
			else
			{
				difficultyLabel.label.color = Menu.MenuRGB(MenuColors.MediumGrey);
				infoLabel.label.color = Menu.MenuRGB(MenuColors.DarkGrey);
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			float num = Scroll(timeStacker);
			float alpha = UseAlpha(timeStacker);
			difficultyLabel.label.alpha = alpha;
			difficultyLabel.label.x = base.MidXpos + num * base.ScrollMagnitude + 0.01f;
			infoLabel.label.alpha = alpha;
			infoLabel.label.x = base.MidXpos + num * base.ScrollMagnitude + 0.01f;
		}
	}

	public class SlugcatPageContinue : SlugcatPage, IOwnAHUD
	{
		private global::HUD.HUD hud;

		public FContainer[] hudContainers;

		public MenuLabel regionLabel;

		public Vector2 KarmaSymbolPos => new Vector2(base.MidXpos - hud.foodMeter.TotalWidth(1f) * 0.5f - hud.karmaMeter.Radius * 0.5f + NextScroll(1f) * base.ScrollMagnitude, imagePos.y - 290f);

		public SaveGameData saveGameData => (menu as SlugcatSelectMenu).GetSaveGameData(index - 1);

		public override bool HasGlow
		{
			get
			{
				if (saveGameData.ascended || menu.manager.rainWorld.flatIllustrations)
				{
					return false;
				}
				if (ModManager.MMF && (menu.manager.rainWorld.options.quality == Options.Quality.MEDIUM || menu.manager.rainWorld.options.quality == Options.Quality.LOW))
				{
					return false;
				}
				if (slugcatNumber == SlugcatStats.Name.Red && (menu as SlugcatSelectMenu).redIsDead)
				{
					return false;
				}
				return saveGameData.hasGlow;
			}
		}

		public override bool HasMark
		{
			get
			{
				if (saveGameData.ascended || menu.manager.rainWorld.flatIllustrations)
				{
					return false;
				}
				if (ModManager.MMF && (menu.manager.rainWorld.options.quality == Options.Quality.MEDIUM || menu.manager.rainWorld.options.quality == Options.Quality.LOW))
				{
					return false;
				}
				if (slugcatNumber == SlugcatStats.Name.Red && (menu as SlugcatSelectMenu).redIsDead)
				{
					return false;
				}
				return saveGameData.hasMark;
			}
		}

		public int CurrentFood => saveGameData.food;

		public Player.InputPackage MapInput => default(Player.InputPackage);

		public bool RevealMap => false;

		public Vector2 MapOwnerInRoomPosition => default(Vector2);

		public bool MapDiscoveryActive => false;

		public int MapOwnerRoom => -1;

		public SlugcatPageContinue(Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
			: base(menu, owner, pageIndex, slugcatNumber)
		{
			if (ModManager.MSC && saveGameData.altEnding && ((slugcatNumber == SlugcatStats.Name.White && menu.manager.rainWorld.progression.miscProgressionData.survivorEndingID > 1) || (slugcatNumber == SlugcatStats.Name.Yellow && menu.manager.rainWorld.progression.miscProgressionData.monkEndingID > 1) || (slugcatNumber != SlugcatStats.Name.White && slugcatNumber != SlugcatStats.Name.Yellow && slugcatNumber != SlugcatStats.Name.Red)))
			{
				AddAltEndingImage();
			}
			else
			{
				AddImage(saveGameData.ascended);
			}
			hudContainers = new FContainer[2];
			for (int i = 0; i < hudContainers.Length; i++)
			{
				hudContainers[i] = new FContainer();
				Container.AddChild(hudContainers[i]);
			}
			hud = new global::HUD.HUD(hudContainers, menu.manager.rainWorld, this);
			saveGameData.karma = Custom.IntClamp(saveGameData.karma, 0, saveGameData.karmaCap);
			if (ModManager.MSC && slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && saveGameData.altEnding && menu.manager.rainWorld.progression.miscProgressionData.artificerEndingID != 1)
			{
				saveGameData.karma = 0;
				saveGameData.karmaCap = 0;
			}
			if (ModManager.MSC && slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && saveGameData.ascended)
			{
				saveGameData.karma = 1;
				saveGameData.karmaCap = 1;
			}
			saveGameData.food = Custom.IntClamp(saveGameData.food, 0, SlugcatStats.SlugcatFoodMeter(slugcatNumber).y);
			hud.AddPart(new KarmaMeter(hud, hudContainers[1], new IntVector2(saveGameData.karma, saveGameData.karmaCap), saveGameData.karmaReinforced));
			hud.AddPart(new FoodMeter(hud, SlugcatStats.SlugcatFoodMeter(slugcatNumber).x, SlugcatStats.SlugcatFoodMeter(slugcatNumber).y));
			string text = "";
			if (saveGameData.shelterName != null && saveGameData.shelterName.Length > 2)
			{
				text = Region.GetRegionFullName(saveGameData.shelterName.Substring(0, 2), slugcatNumber);
				if (text.Length > 0)
				{
					text = menu.Translate(text);
					text = text + " - " + menu.Translate("Cycle") + " " + ((slugcatNumber == SlugcatStats.Name.Red) ? (RedsIllness.RedsCycles(saveGameData.redsExtraCycles) - saveGameData.cycle) : saveGameData.cycle);
					SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(slugcatNumber);
					if (campaignTimeTracker != null)
					{
						if (campaignTimeTracker.TotalFreeTime == 0.0 || campaignTimeTracker.TotalFixedTime == 0.0)
						{
							campaignTimeTracker.LoadOldTimings(saveGameData.gameTimeAlive, saveGameData.gameTimeDead);
						}
						if (ModManager.MMF)
						{
							text = text + " (" + campaignTimeTracker.TotalFreeTimeSpan.GetIGTFormat(MMF.cfgSpeedrunTimer.Value || menu.manager.rainWorld.options.validation) + ")";
						}
					}
				}
			}
			regionLabel = new MenuLabel(menu, this, text, new Vector2(-1000f, imagePos.y - 249f), new Vector2(200f, 30f), bigText: true);
			regionLabel.label.alignment = FLabelAlignment.Center;
			subObjects.Add(regionLabel);
		}

		public override void Update()
		{
			base.Update();
			hud.Update();
			hud.foodMeter.fade = Mathf.InverseLerp(0.5f, 0f, Mathf.Abs(NextScroll(1f)));
			if (hud.foodMeter.fade == 0f)
			{
				hud.foodMeter.initPlopCircle = -1;
				hud.foodMeter.initPlopDelay = 0;
			}
			hud.karmaMeter.fade = Mathf.InverseLerp(0.5f, 0f, Mathf.Abs(NextScroll(1f)));
			hud.karmaMeter.pos = KarmaSymbolPos + new Vector2(0.01f, 0.01f);
			hud.foodMeter.pos = KarmaSymbolPos + new Vector2(hud.karmaMeter.Radius + 22f + 0.01f, 0.01f);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			hud.Draw(timeStacker);
			float num = Scroll(timeStacker);
			float alpha = UseAlpha(timeStacker);
			regionLabel.label.alpha = alpha;
			regionLabel.label.x = base.MidXpos + num * base.ScrollMagnitude + 0.01f;
			regionLabel.label.color = Menu.MenuRGB(MenuColors.MediumGrey);
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			hud.ClearAllSprites();
		}

		public global::HUD.HUD.OwnerType GetOwnerType()
		{
			return global::HUD.HUD.OwnerType.CharacterSelect;
		}

		public void PlayHUDSound(SoundID soundID)
		{
			menu.PlaySound(soundID);
		}

		public void FoodCountDownDone()
		{
		}
	}

	public class CustomColorInterface : MenuObject
	{
		public const string SIGNAL = "MMFCUSTOMCOLOR";

		public SimpleButton[] bodyButtons;

		public MenuIllustration[] bodyColors;

		public RoundedRect[] bodyColorBorders;

		public SlugcatStats.Name slugcatID;

		public int activeColorChooser;

		private new MenuObject owner;

		public List<string> defaultColors;

		public CustomColorInterface(Menu menu, MenuObject owner, Vector2 pos, SlugcatStats.Name slugcatID, List<string> names, List<string> defaultColors)
			: base(menu, owner)
		{
			bodyColors = new MenuIllustration[names.Count];
			bodyColorBorders = new RoundedRect[names.Count];
			bodyButtons = new SimpleButton[names.Count];
			this.slugcatID = slugcatID;
			this.owner = owner;
			this.defaultColors = defaultColors;
			for (int i = 0; i < names.Count; i++)
			{
				bodyButtons[i] = new SimpleButton(menu, owner, menu.Translate(names[i]), "MMFCUSTOMCOLOR" + i, new Vector2(pos.x + (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, pos.y - (float)i * 46f + 7f), new Vector2(110f, 30f));
				bodyColorBorders[i] = new RoundedRect(menu, owner, new Vector2(bodyButtons[i].pos.x - 50f, bodyButtons[i].pos.y - 6f), new Vector2(40f, 40f), filled: false);
				bodyColors[i] = new MenuIllustration(menu, owner, "", "square", bodyColorBorders[i].pos + new Vector2(2f, 2f), crispPixels: false, anchorCenter: false);
				if (!menu.manager.rainWorld.progression.miscProgressionData.colorChoices.ContainsKey(slugcatID.value))
				{
					menu.manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatID.value] = new List<string>();
				}
				if (menu.manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatID.value].Count <= i)
				{
					menu.manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatID.value].Add(defaultColors[i]);
				}
				Vector3 vector = new Vector3(1f, 1f, 1f);
				if (menu.manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatID.value][i].Contains(","))
				{
					string[] array = menu.manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatID.value][i].Split(',');
					vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				bodyColors[i].color = Custom.HSL2RGB(vector[0], vector[1], vector[2]);
				owner.subObjects.Add(bodyButtons[i]);
				owner.subObjects.Add(bodyColorBorders[i]);
				owner.subObjects.Add(bodyColors[i]);
			}
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			for (int i = 0; i < bodyColors.Length; i++)
			{
				owner.RemoveSubObject(bodyButtons[i]);
				bodyButtons[i].RemoveSprites();
				bodyColorBorders[i].RemoveSprites();
				bodyColors[i].RemoveSprites();
			}
		}
	}

	public int slugcatPageIndex;

	public float scroll;

	public float lastScroll;

	public int quedSideInput;

	public List<SlugcatPage> slugcatPages;

	public bool lastPauseButton;

	public Dictionary<SlugcatStats.Name, SaveGameData> saveGameData;

	public List<SlugcatStats.Name> slugcatColorOrder;

	public HoldButton startButton;

	public RainEffect rainEffect;

	public CheckBox restartCheckbox;

	public bool redIsDead;

	private bool requestingControllerConnections;

	private bool restartChecked;

	private bool restartAvailable;

	private float restartUp;

	private bool pendingStart;

	private SaveState redSaveState;

	public SimpleButton jollyToggleConfigMenu;

	public MenuLabel jollyOptionsLabel;

	private MenuLabel jollyPlayerCountLabel;

	private List<MenuLabel> playerSummaries;

	private MenuLabel jollySummaryP2;

	private MenuLabel jollySummaryP3;

	private MenuLabel jollySummaryP4;

	private bool forceActivateMSCJolly;

	public CustomColorInterface colorInterface;

	public CheckBox colorsCheckbox;

	public bool colorChecked;

	public int timeOnSof;

	public HorizontalSlider hueSlider;

	public HorizontalSlider satSlider;

	public HorizontalSlider litSlider;

	public int activeColorChooser;

	public BigArrowButton nextButton;

	public BigArrowButton prevButton;

	public SimpleButton defaultColorButton;

	public bool artificerIsDead;

	public bool saintIsDead;

	public float NextScroll => Custom.LerpAndTick(scroll, 0f, 0.07f, (float)(Math.Abs(quedSideInput) + 1) / 30f);

	public SaveGameData GetSaveGameData(int pageIndex)
	{
		return saveGameData[slugcatColorOrder[pageIndex]];
	}

	public void ComingFromRedsStatistics()
	{
		slugcatPageIndex = indexFromColor(SlugcatStats.Name.Red);
		redIsDead = true;
		UpdateSelectedSlugcatInMiscProg();
	}

	public static bool CheckUnlockRed()
	{
		return File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "unlockred.txt").ToLowerInvariant());
	}

	public bool CheckJollyCoopAvailable(SlugcatStats.Name slugcat)
	{
		if (!ModManager.JollyCoop)
		{
			return false;
		}
		bool num = SlugcatUnlocked(slugcat);
		bool flag = SlugcatStats.IsSlugcatFromMSC(slugcat);
		if (num)
		{
			if (flag)
			{
				if (flag)
				{
					return forceActivateMSCJolly;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetSlugcatColorOrder()
	{
		slugcatColorOrder = new List<SlugcatStats.Name>
		{
			SlugcatStats.Name.Yellow,
			SlugcatStats.Name.White,
			SlugcatStats.Name.Red
		};
		SlugcatStats.Name name = null;
		if (ModManager.MSC)
		{
			if (manager.rainWorld.setup.betaTestSlugcat != null && manager.rainWorld.setup.betaTestSlugcat != SlugcatStats.Name.White && manager.rainWorld.setup.betaTestSlugcat != SlugcatStats.Name.Yellow && manager.rainWorld.setup.betaTestSlugcat != SlugcatStats.Name.Red)
			{
				name = manager.rainWorld.setup.betaTestSlugcat;
			}
			if (ModManager.MSC && manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				slugcatColorOrder = new List<SlugcatStats.Name> { MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel };
			}
			else if (name == null)
			{
				slugcatColorOrder = new List<SlugcatStats.Name>
				{
					SlugcatStats.Name.Yellow,
					SlugcatStats.Name.White,
					SlugcatStats.Name.Red,
					MoreSlugcatsEnums.SlugcatStatsName.Gourmand,
					MoreSlugcatsEnums.SlugcatStatsName.Artificer,
					MoreSlugcatsEnums.SlugcatStatsName.Rivulet,
					MoreSlugcatsEnums.SlugcatStatsName.Spear,
					MoreSlugcatsEnums.SlugcatStatsName.Saint
				};
			}
			else if (name == SlugcatStats.Name.White || name == SlugcatStats.Name.Yellow || name == SlugcatStats.Name.Red)
			{
				slugcatColorOrder = new List<SlugcatStats.Name>
				{
					SlugcatStats.Name.Yellow,
					SlugcatStats.Name.White,
					SlugcatStats.Name.Red
				};
			}
			else
			{
				slugcatColorOrder = new List<SlugcatStats.Name>
				{
					SlugcatStats.Name.Yellow,
					SlugcatStats.Name.White,
					SlugcatStats.Name.Red,
					name
				};
			}
		}
		for (int i = 0; i < slugcatColorOrder.Count; i++)
		{
			if (slugcatColorOrder[i] == manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat)
			{
				slugcatPageIndex = i;
				break;
			}
		}
	}

	public SlugcatSelectMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.SlugcatSelect)
	{
		pages.Add(new Page(this, null, "main", 0));
		activeColorChooser = -1;
		if (!manager.rainWorld.flatIllustrations && (!ModManager.MMF || manager.rainWorld.options.quality == Options.Quality.HIGH))
		{
			rainEffect = new RainEffect(this, pages[0]);
			pages[0].subObjects.Add(rainEffect);
		}
		if (CheckUnlockRed() || SlugcatStats.SlugcatUnlocked(SlugcatStats.Name.Red, manager.rainWorld))
		{
			manager.rainWorld.progression.miscProgressionData.redUnlocked = true;
		}
		manager.sceneSlot = null;
		manager.statsAfterCredits = false;
		SetSlugcatColorOrder();
		this.saveGameData = new Dictionary<SlugcatStats.Name, SaveGameData>();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < slugcatColorOrder.Count; i++)
		{
			this.saveGameData[slugcatColorOrder[i]] = MineForSaveData(manager, slugcatColorOrder[i]);
			if (this.saveGameData[slugcatColorOrder[i]] != null)
			{
				num2 = i;
				num++;
			}
		}
		if (ModManager.MSC && this.saveGameData.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			SaveGameData saveGameData = this.saveGameData[MoreSlugcatsEnums.SlugcatStatsName.Artificer];
			if (saveGameData != null && saveGameData.ascended)
			{
				artificerIsDead = true;
			}
		}
		if (ModManager.MSC && this.saveGameData.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			SaveGameData saveGameData2 = this.saveGameData[MoreSlugcatsEnums.SlugcatStatsName.Saint];
			if (saveGameData2 != null && saveGameData2.ascended)
			{
				saintIsDead = true;
			}
		}
		if (this.saveGameData.ContainsKey(SlugcatStats.Name.Red) && this.saveGameData[SlugcatStats.Name.Red] != null && ((this.saveGameData[SlugcatStats.Name.Red].redsDeath && this.saveGameData[SlugcatStats.Name.Red].cycle >= RedsIllness.RedsCycles(this.saveGameData[SlugcatStats.Name.Red].redsExtraCycles)) || this.saveGameData[SlugcatStats.Name.Red].ascended))
		{
			redIsDead = true;
			if (this.saveGameData[SlugcatStats.Name.Red].ascended)
			{
				manager.CueAchievement(RainWorld.AchievementID.HunterWin, 1f);
				manager.rainWorld.progression.miscProgressionData.beaten_Hunter = true;
			}
		}
		if (ModManager.MSC)
		{
			if (manager.rainWorld.progression.miscProgressionData.beaten_Rivulet)
			{
				manager.CueAchievement(RainWorld.AchievementID.RivuletEnding, 1f);
			}
			if (manager.rainWorld.progression.miscProgressionData.beaten_Saint)
			{
				manager.CueAchievement(RainWorld.AchievementID.SaintEnding, 1f);
			}
			if (manager.rainWorld.progression.miscProgressionData.beaten_Gourmand || manager.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full)
			{
				manager.CueAchievement(RainWorld.AchievementID.GourmandEnding, 1f);
			}
			if (manager.rainWorld.progression.miscProgressionData.beaten_Artificer)
			{
				manager.CueAchievement(RainWorld.AchievementID.ArtificerEnding, 1f);
			}
			if (manager.rainWorld.progression.miscProgressionData.beaten_SpearMaster)
			{
				manager.CueAchievement(RainWorld.AchievementID.SpearmasterEnding, 1f);
			}
		}
		if (num == 1)
		{
			slugcatPageIndex = num2;
		}
		slugcatPages = new List<SlugcatPage>();
		for (int j = 0; j < slugcatColorOrder.Count; j++)
		{
			if (this.saveGameData[slugcatColorOrder[j]] != null)
			{
				slugcatPages.Add(new SlugcatPageContinue(this, null, 1 + j, slugcatColorOrder[j]));
			}
			else
			{
				slugcatPages.Add(new SlugcatPageNewGame(this, null, 1 + j, slugcatColorOrder[j]));
			}
			pages.Add(slugcatPages[j]);
		}
		startButton = new HoldButton(this, pages[0], "", "START", new Vector2(683f, 85f), 40f);
		pages[0].subObjects.Add(startButton);
		SimpleButton item = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 668f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(item);
		backObject = item;
		prevButton = new BigArrowButton(this, pages[0], "PREV", new Vector2(200f, 50f), -1);
		pages[0].subObjects.Add(prevButton);
		nextButton = new BigArrowButton(this, pages[0], "NEXT", new Vector2(1116f, 50f), 1);
		pages[0].subObjects.Add(nextButton);
		float restartTextWidth = GetRestartTextWidth(base.CurrLang);
		float restartTextOffset = GetRestartTextOffset(base.CurrLang);
		restartCheckbox = new CheckBox(this, pages[0], this, new Vector2(startButton.pos.x + 200f + restartTextOffset, Mathf.Max(30f, manager.rainWorld.options.SafeScreenOffset.y)), restartTextWidth, Translate("Restart game"), "RESTART");
		restartCheckbox.label.pos.x += restartTextWidth - restartCheckbox.label.label.textRect.width - 5f;
		pages[0].subObjects.Add(restartCheckbox);
		if (ModManager.MMF)
		{
			colorsCheckbox = new CheckBox(this, pages[0], this, new Vector2(startButton.pos.x + 200f + restartTextOffset, 60f), restartTextWidth, Translate("Custom colors"), "COLORS");
			colorsCheckbox.label.pos.x += restartTextWidth - colorsCheckbox.label.label.textRect.width - 5f;
			colorsCheckbox.selectable = true;
			pages[0].subObjects.Add(colorsCheckbox);
		}
		forceActivateMSCJolly = JollyCustom.ForceActivateWithMSC();
		if (ModManager.JollyCoop && CheckJollyCoopAvailable(colorFromIndex(slugcatPageIndex)))
		{
			AddJollyButtons();
		}
		UpdateStartButtonText();
		UpdateSelectedSlugcatInMiscProg();
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	private static float GetRestartTextWidth(InGameTranslator.LanguageID lang)
	{
		float result = 85f;
		if (lang == InGameTranslator.LanguageID.Chinese)
		{
			result = 110f;
		}
		else if (lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.German)
		{
			result = 155f;
		}
		else if (lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Portuguese)
		{
			result = 140f;
		}
		else if (lang == InGameTranslator.LanguageID.Japanese)
		{
			result = 180f;
		}
		return result;
	}

	private static float GetRestartTextOffset(InGameTranslator.LanguageID lang)
	{
		float result = 0f;
		if (lang == InGameTranslator.LanguageID.French)
		{
			result = 35f;
		}
		else if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.Italian || lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Portuguese)
		{
			result = 25f;
		}
		else if (lang == InGameTranslator.LanguageID.German)
		{
			result = 50f;
		}
		return result;
	}

	protected override void Init()
	{
		base.Init();
		selectedObject = startButton;
	}

	public override void Update()
	{
		base.Update();
		if (rainEffect != null)
		{
			rainEffect.rainFade = Mathf.Min(0.3f, rainEffect.rainFade + 0.006f);
		}
		if (manager.rainWorld.options.IsJollyProfileRequesting())
		{
			StartGame(slugcatPages[slugcatPageIndex].slugcatNumber);
		}
		if (restartAvailable)
		{
			restartCheckbox.buttonBehav.greyedOut = false;
			restartCheckbox.selectable = true;
		}
		else
		{
			restartCheckbox.buttonBehav.greyedOut = true;
			restartCheckbox.selectable = false;
			restartCheckbox.Checked = false;
			restartChecked = false;
			if (saveGameData[colorFromIndex(slugcatPageIndex)] != null)
			{
				restartUp = 1f;
				restartAvailable = true;
			}
			else
			{
				restartUp = 0f;
				restartAvailable = false;
			}
		}
		if (ModManager.JollyCoop)
		{
			ModManager.CoopAvailable = CheckJollyCoopAvailable(colorFromIndex(slugcatPageIndex));
			if (ModManager.CoopAvailable && jollyToggleConfigMenu == null)
			{
				AddJollyButtons();
			}
			if (!ModManager.CoopAvailable && jollyToggleConfigMenu != null)
			{
				RemoveJollyButtons();
			}
			if (jollyToggleConfigMenu != null)
			{
				jollyToggleConfigMenu.GetButtonBehavior.greyedOut = !ModManager.CoopAvailable || scroll != 0f;
				if (colorsCheckbox != null)
				{
					if (ModManager.CoopAvailable && colorsCheckbox.Checked)
					{
						colorsCheckbox.GetButtonBehavior.greyedOut = false;
						SetChecked(colorsCheckbox, c: false);
					}
					colorsCheckbox.GetButtonBehavior.greyedOut = ModManager.CoopAvailable;
				}
				jollyPlayerCountLabel.label.alpha = (ModManager.CoopAvailable ? 1f : 0f);
				jollyPlayerCountLabel.text = Translate("Players: <num_p>").Replace("<num_p>", Custom.rainWorld.options.JollyPlayerCount.ToString());
				RefreshJollySummary();
			}
		}
		startButton.warningMode = restartChecked;
		if (ModManager.MSC && slugcatPages[slugcatPageIndex].slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			timeOnSof++;
			if (timeOnSof == 2400)
			{
				PlaySound(MoreSlugcatsEnums.MSCSoundID.Apple, 0f, 1f, 1f);
			}
		}
		restartCheckbox.pos.y = Mathf.Lerp(-50f, Mathf.Max(30f, manager.rainWorld.options.SafeScreenOffset.y), restartUp);
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null)
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
		lastPauseButton = flag;
		lastScroll = scroll;
		scroll = NextScroll;
		startButton.GetButtonBehavior.greyedOut = Mathf.Abs(scroll) > 0.1f || !SlugcatUnlocked(colorFromIndex(slugcatPageIndex));
		if (Mathf.Abs(lastScroll) > 0.5f && Mathf.Abs(scroll) <= 0.5f)
		{
			UpdateStartButtonText();
		}
		if (scroll != 0f || lastScroll != 0f)
		{
			return;
		}
		if (quedSideInput < 0)
		{
			quedSideInput++;
			slugcatPageIndex--;
			if (slugcatPageIndex < 0)
			{
				slugcatPageIndex = slugcatPages.Count - 1;
			}
			UpdateSelectedSlugcatInMiscProg();
			if (ModManager.JollyCoop)
			{
				manager.rainWorld.options.jollyPlayerOptionsArray[0].playerClass = null;
			}
			scroll = 1f;
			lastScroll = 1f;
			restartAvailable = false;
		}
		else if (quedSideInput > 0)
		{
			quedSideInput--;
			slugcatPageIndex++;
			if (slugcatPageIndex >= slugcatPages.Count)
			{
				slugcatPageIndex = 0;
			}
			UpdateSelectedSlugcatInMiscProg();
			if (ModManager.JollyCoop)
			{
				manager.rainWorld.options.jollyPlayerOptionsArray[0].playerClass = null;
			}
			scroll = -1f;
			lastScroll = -1f;
			restartAvailable = false;
		}
	}

	private void UpdateSelectedSlugcatInMiscProg()
	{
		if (slugcatPageIndex < 0 || slugcatPageIndex >= slugcatPages.Count)
		{
			return;
		}
		manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = slugcatPages[slugcatPageIndex].slugcatNumber;
		manager.rainWorld.options.ResetJollyProfileRequest();
		if (!ModManager.MMF)
		{
			return;
		}
		RemoveColorButtons();
		if (colorsCheckbox != null && !CheckJollyCoopAvailable(colorFromIndex(slugcatPageIndex)))
		{
			if (colorsCheckbox.buttonBehav.greyedOut)
			{
				colorsCheckbox.buttonBehav.greyedOut = false;
			}
			if (!SlugcatUnlocked(colorFromIndex(slugcatPageIndex)))
			{
				SetChecked(colorsCheckbox, c: false);
				colorsCheckbox.buttonBehav.greyedOut = true;
			}
			else
			{
				SetChecked(colorsCheckbox, manager.rainWorld.progression.miscProgressionData.colorsEnabled.ContainsKey(colorFromIndex(slugcatPageIndex).value) && manager.rainWorld.progression.miscProgressionData.colorsEnabled[colorFromIndex(slugcatPageIndex).value]);
				colorsCheckbox.buttonBehav.greyedOut = false;
			}
		}
	}

	private void UpdateStartButtonText()
	{
		startButton.fillTime = (restartChecked ? 120f : 40f);
		if (GetSaveGameData(slugcatPageIndex) == null)
		{
			startButton.menuLabel.text = Translate("NEW GAME");
		}
		else if (restartChecked)
		{
			startButton.menuLabel.text = Translate("DELETE SAVE").Replace(" ", "\r\n");
		}
		else if (slugcatPages[slugcatPageIndex].slugcatNumber == SlugcatStats.Name.Red && redIsDead)
		{
			startButton.menuLabel.text = Translate("STATISTICS");
		}
		else if (ModManager.MSC && slugcatPages[slugcatPageIndex].slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && artificerIsDead)
		{
			startButton.menuLabel.text = Translate("STATISTICS");
		}
		else if (ModManager.MSC && slugcatPages[slugcatPageIndex].slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && saintIsDead)
		{
			startButton.menuLabel.text = Translate("STATISTICS");
		}
		else
		{
			startButton.menuLabel.text = Translate("CONTINUE");
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess.ID == ProcessManager.ProcessID.FastTravelScreen)
		{
			(nextProcess as FastTravelScreen).initiateCharacterFastTravel = true;
		}
		else if (nextProcess.ID == ProcessManager.ProcessID.Statistics)
		{
			KarmaLadderScreen.SleepDeathScreenDataPackage package = new KarmaLadderScreen.SleepDeathScreenDataPackage(redSaveState.food, new IntVector2(redSaveState.deathPersistentSaveData.karma, redSaveState.deathPersistentSaveData.karmaCap), redSaveState.deathPersistentSaveData.reinforcedKarma, -1, new Vector2(0f, 0f), null, redSaveState, new SlugcatStats(ModManager.MSC ? redSaveState.saveStateNumber : SlugcatStats.Name.Red, malnourished: false), null, startMalnourished: false, goalMalnourished: false);
			(nextProcess as StoryGameStatisticsScreen).GetDataFromGame(package);
		}
		else if (ModManager.CoopAvailable && nextProcess is InputOptionsMenu inputOptionsMenu)
		{
			JollyCustom.Log("Going to input menu, setting flag...");
			inputOptionsMenu.fromJollyMenu = true;
			inputOptionsMenu.previousMenu = ProcessManager.ProcessID.SlugcatSelect;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (requestingControllerConnections)
		{
			return;
		}
		switch (message)
		{
		case "BACK":
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
			break;
		case "START":
			manager.rainWorld.options.ResetJollyProfileRequest();
			StartGame(slugcatPages[slugcatPageIndex].slugcatNumber);
			break;
		case "PREV":
			quedSideInput = Math.Max(-3, quedSideInput - 1);
			PlaySound(SoundID.MENU_Next_Slugcat);
			break;
		case "NEXT":
			quedSideInput = Math.Min(3, quedSideInput + 1);
			PlaySound(SoundID.MENU_Next_Slugcat);
			break;
		case "DEFAULTCOL":
		{
			SlugcatStats.Name name = slugcatColorOrder[slugcatPageIndex];
			int index = activeColorChooser;
			manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index] = colorInterface.defaultColors[activeColorChooser];
			float f = ValueOfSlider(hueSlider);
			float f2 = ValueOfSlider(satSlider);
			float f3 = ValueOfSlider(litSlider);
			SliderSetValue(hueSlider, f);
			SliderSetValue(satSlider, f2);
			SliderSetValue(litSlider, f3);
			PlaySound(SoundID.MENU_Remove_Level);
			break;
		}
		case "JOLLY_TOGGLE_CONFIG":
		{
			JollySetupDialog dialog = new JollySetupDialog(colorFromIndex(slugcatPageIndex), manager, jollyToggleConfigMenu.pos);
			manager.ShowDialog(dialog);
			manager.rainWorld.options.playersBeforeEnterJollyMenu = manager.rainWorld.options.JollyPlayerCount;
			manager.rainWorld.options.ResetJollyProfileRequest();
			PlaySound(SoundID.MENU_Switch_Page_In);
			break;
		}
		}
		if (message.StartsWith("MMFCUSTOMCOLOR"))
		{
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			int num = int.Parse(message.Substring("MMFCUSTOMCOLOR".Length), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (num == activeColorChooser)
			{
				RemoveColorInterface();
				PlaySound(SoundID.MENU_Remove_Level);
			}
			else
			{
				activeColorChooser = num;
				AddColorInterface();
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			}
		}
	}

	public static SaveGameData MineForSaveData(ProcessManager manager, SlugcatStats.Name slugcat)
	{
		if (!manager.rainWorld.progression.IsThereASavedGame(slugcat))
		{
			return null;
		}
		if (manager.rainWorld.progression.currentSaveState != null && manager.rainWorld.progression.currentSaveState.saveStateNumber == slugcat)
		{
			SaveGameData saveGameData = new SaveGameData();
			saveGameData.karmaCap = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.karmaCap;
			saveGameData.karma = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.karma;
			saveGameData.karmaReinforced = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.reinforcedKarma;
			saveGameData.shelterName = manager.rainWorld.progression.currentSaveState.GetSaveStateDenToUse();
			saveGameData.cycle = manager.rainWorld.progression.currentSaveState.cycleNumber;
			saveGameData.hasGlow = manager.rainWorld.progression.currentSaveState.theGlow;
			saveGameData.hasMark = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.theMark;
			saveGameData.redsExtraCycles = manager.rainWorld.progression.currentSaveState.redExtraCycles;
			saveGameData.food = manager.rainWorld.progression.currentSaveState.food;
			saveGameData.redsDeath = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.redsDeath;
			saveGameData.ascended = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.ascended;
			if (ModManager.MSC)
			{
				saveGameData.altEnding = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding;
				saveGameData.hasRobo = manager.rainWorld.progression.currentSaveState.hasRobo;
				saveGameData.pebblesEnergyTaken = manager.rainWorld.progression.currentSaveState.miscWorldSaveData.pebblesEnergyTaken;
				saveGameData.moonGivenRobe = manager.rainWorld.progression.currentSaveState.miscWorldSaveData.moonGivenRobe;
			}
			if (ModManager.MMF)
			{
				saveGameData.gameTimeAlive = manager.rainWorld.progression.currentSaveState.totTime;
				saveGameData.gameTimeDead = manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.deathTime;
			}
			return saveGameData;
		}
		if (!manager.rainWorld.progression.HasSaveData)
		{
			return null;
		}
		string[] progLinesFromMemory = manager.rainWorld.progression.GetProgLinesFromMemory();
		if (progLinesFromMemory.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length != 2 || !(array[0] == "SAVE STATE") || !(BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == slugcat))
			{
				continue;
			}
			List<SaveStateMiner.Target> list = new List<SaveStateMiner.Target>();
			list.Add(new SaveStateMiner.Target(">DENPOS", "<svB>", "<svA>", 35));
			list.Add(new SaveStateMiner.Target(">LASTVDENPOS", "<svB>", "<svA>", 20));
			list.Add(new SaveStateMiner.Target(">CYCLENUM", "<svB>", "<svA>", 50));
			list.Add(new SaveStateMiner.Target(">FOOD", "<svB>", "<svA>", 20));
			list.Add(new SaveStateMiner.Target(">HASTHEGLOW", null, "<svA>", 20));
			list.Add(new SaveStateMiner.Target(">REINFORCEDKARMA", "<dpB>", "<dpA>", 20));
			list.Add(new SaveStateMiner.Target(">KARMA", "<dpB>", "<dpA>", 20));
			list.Add(new SaveStateMiner.Target(">KARMACAP", "<dpB>", "<dpA>", 20));
			list.Add(new SaveStateMiner.Target(">HASTHEMARK", null, "<dpA>", 20));
			list.Add(new SaveStateMiner.Target(">REDEXTRACYCLES", null, "<svA>", 20));
			if (slugcat == SlugcatStats.Name.Red)
			{
				list.Add(new SaveStateMiner.Target(">REDSDEATH", null, "<dpA>", 20));
			}
			list.Add(new SaveStateMiner.Target(">ASCENDED", null, "<dpA>", 20));
			if (ModManager.MSC)
			{
				list.Add(new SaveStateMiner.Target(">HASROBO", null, "<svA>", 20));
				list.Add(new SaveStateMiner.Target(">ALTENDING", null, "<dpA>", 20));
				list.Add(new SaveStateMiner.Target(">ENERGYRAILOFF", null, "<mwA>", 20));
				list.Add(new SaveStateMiner.Target(">MOONROBE", null, "<mwA>", 20));
			}
			if (ModManager.MMF)
			{
				list.Add(new SaveStateMiner.Target(">TOTTIME", "<svB>", "<svA>", 20));
				list.Add(new SaveStateMiner.Target(">DEATHTIME", "<dpB>", "<dpA>", 20));
			}
			List<SaveStateMiner.Result> list2 = SaveStateMiner.Mine(manager.rainWorld, array[1], list);
			SaveGameData saveGameData2 = new SaveGameData();
			for (int j = 0; j < list2.Count; j++)
			{
				switch (list2[j].name)
				{
				case ">DENPOS":
					saveGameData2.shelterName = list2[j].data;
					break;
				case ">LASTVDENPOS":
					if (saveGameData2.shelterName == null || !RainWorld.roomNameToIndex.ContainsKey(saveGameData2.shelterName))
					{
						saveGameData2.shelterName = list2[j].data;
					}
					break;
				case ">CYCLENUM":
					try
					{
						saveGameData2.cycle = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign cycle num. Data:", list2[j].data);
					}
					break;
				case ">FOOD":
					try
					{
						saveGameData2.food = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign food. Data:", list2[j].data);
					}
					break;
				case ">HASTHEGLOW":
					saveGameData2.hasGlow = true;
					break;
				case ">REINFORCEDKARMA":
					saveGameData2.karmaReinforced = list2[j].data == "1";
					break;
				case ">KARMA":
					try
					{
						saveGameData2.karma = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign karma. Data:", list2[j].data);
					}
					break;
				case ">KARMACAP":
					try
					{
						saveGameData2.karmaCap = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign karma cap. Data:", list2[j].data);
					}
					break;
				case ">TOTTIME":
					try
					{
						saveGameData2.gameTimeAlive = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign time alive. Data:", list2[j].data);
					}
					break;
				case ">DEATHTIME":
					try
					{
						saveGameData2.gameTimeDead = int.Parse(list2[j].data, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					catch
					{
						Custom.LogWarning("failed to assign time dead. Data:", list2[j].data);
					}
					break;
				case ">HASTHEMARK":
					saveGameData2.hasMark = true;
					break;
				case ">REDEXTRACYCLES":
					saveGameData2.redsExtraCycles = true;
					break;
				case ">REDSDEATH":
					saveGameData2.redsDeath = true;
					break;
				case ">ASCENDED":
					saveGameData2.ascended = true;
					break;
				case ">ALTENDING":
					saveGameData2.altEnding = true;
					break;
				case ">HASROBO":
					saveGameData2.hasRobo = true;
					break;
				case ">ENERGYRAILOFF":
					saveGameData2.pebblesEnergyTaken = true;
					break;
				case ">MOONROBE":
					saveGameData2.moonGivenRobe = true;
					break;
				}
			}
			if (saveGameData2.shelterName == null || !RainWorld.roomNameToIndex.ContainsKey(saveGameData2.shelterName))
			{
				saveGameData2.shelterName = SaveState.GetFinalFallbackShelter(slugcat);
			}
			return saveGameData2;
		}
		return null;
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
		requestingControllerConnections = false;
		if (pendingStart)
		{
			StartGame(slugcatPages[slugcatPageIndex].slugcatNumber);
		}
	}

	public void StartGame(SlugcatStats.Name storyGameCharacter)
	{
		if (storyGameCharacter == SlugcatStats.Name.Red && !SlugcatStats.SlugcatUnlocked(storyGameCharacter, manager.rainWorld))
		{
			return;
		}
		manager.rainWorld.inGameSlugCat = storyGameCharacter;
		if (ModManager.MMF && manager.rainWorld.progression.miscProgressionData.colorsEnabled.ContainsKey(slugcatColorOrder[slugcatPageIndex].value) && manager.rainWorld.progression.miscProgressionData.colorsEnabled[slugcatColorOrder[slugcatPageIndex].value])
		{
			List<Color> list = new List<Color>();
			for (int i = 0; i < manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatColorOrder[slugcatPageIndex].value].Count; i++)
			{
				Vector3 vector = new Vector3(1f, 1f, 1f);
				if (manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatColorOrder[slugcatPageIndex].value][i].Contains(","))
				{
					string[] array = manager.rainWorld.progression.miscProgressionData.colorChoices[slugcatColorOrder[slugcatPageIndex].value][i].Split(',');
					vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				list.Add(Custom.HSL2RGB(vector[0], vector[1], vector[2]));
			}
			PlayerGraphics.customColors = list;
		}
		else
		{
			PlayerGraphics.customColors = null;
		}
		manager.arenaSitting = null;
		manager.rainWorld.progression.currentSaveState = null;
		manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = storyGameCharacter;
		if (ModManager.CoopAvailable)
		{
			for (int j = 1; j < manager.rainWorld.options.JollyPlayerCount; j++)
			{
				manager.rainWorld.ActivatePlayer(j);
			}
			for (int k = manager.rainWorld.options.JollyPlayerCount; k < 4; k++)
			{
				manager.rainWorld.DeactivatePlayer(k);
			}
		}
		if (!restartChecked && manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter))
		{
			ContinueStartedGame(storyGameCharacter);
		}
		else
		{
			manager.rainWorld.progression.WipeSaveState(storyGameCharacter);
			manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
			if ((storyGameCharacter != SlugcatStats.Name.White && storyGameCharacter != SlugcatStats.Name.Yellow && (!ModManager.MSC || storyGameCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint)) || Input.GetKey("s") || RWInput.CheckSpecificButton(0, 11))
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			}
			else
			{
				if (storyGameCharacter == SlugcatStats.Name.Yellow)
				{
					manager.nextSlideshow = SlideShow.SlideShowID.YellowIntro;
				}
				else if (ModManager.MSC && storyGameCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.SaintIntro;
				}
				else
				{
					manager.nextSlideshow = SlideShow.SlideShowID.WhiteIntro;
				}
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
			}
			PlaySound(SoundID.MENU_Start_New_Game);
		}
		if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song is IntroRollMusic)
		{
			manager.musicPlayer.song.FadeOut(20f);
		}
	}

	public void ContinueStartedGame(SlugcatStats.Name storyGameCharacter)
	{
		if (storyGameCharacter == SlugcatStats.Name.Red && redIsDead)
		{
			redSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(SlugcatStats.Name.Red, null, manager.menuSetup, saveAsDeathOrQuit: false);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
		else if (ModManager.MSC && storyGameCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && artificerIsDead)
		{
			redSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(MoreSlugcatsEnums.SlugcatStatsName.Artificer, null, manager.menuSetup, saveAsDeathOrQuit: false);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
		else if (ModManager.MSC && storyGameCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && saintIsDead)
		{
			redSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(MoreSlugcatsEnums.SlugcatStatsName.Saint, null, manager.menuSetup, saveAsDeathOrQuit: false);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
		else
		{
			manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			PlaySound(SoundID.MENU_Continue_Game);
		}
	}

	public bool GetChecked(CheckBox box)
	{
		if (box.IDString == "COLORS")
		{
			return colorChecked;
		}
		return restartChecked;
	}

	public void SetChecked(CheckBox box, bool c)
	{
		if (box.IDString == "COLORS")
		{
			colorChecked = c;
			if (colorChecked && !CheckJollyCoopAvailable(colorFromIndex(slugcatPageIndex)))
			{
				AddColorButtons();
				manager.rainWorld.progression.miscProgressionData.colorsEnabled[slugcatColorOrder[slugcatPageIndex].value] = true;
			}
			else
			{
				RemoveColorButtons();
				manager.rainWorld.progression.miscProgressionData.colorsEnabled[slugcatColorOrder[slugcatPageIndex].value] = false;
			}
		}
		else
		{
			restartChecked = c;
			UpdateStartButtonText();
		}
	}

	public int indexFromColor(SlugcatStats.Name color)
	{
		for (int i = 0; i < slugcatColorOrder.Count; i++)
		{
			if (slugcatColorOrder[i] == color)
			{
				return i;
			}
		}
		return -1;
	}

	public SlugcatStats.Name colorFromIndex(int index)
	{
		return slugcatColorOrder[index];
	}

	public void ComingFromArtificerStatistics()
	{
		slugcatPageIndex = indexFromColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
		artificerIsDead = saveGameData[MoreSlugcatsEnums.SlugcatStatsName.Artificer].ascended;
		UpdateSelectedSlugcatInMiscProg();
	}

	public void ComingFromSaintStatistics()
	{
		slugcatPageIndex = indexFromColor(MoreSlugcatsEnums.SlugcatStatsName.Saint);
		saintIsDead = saveGameData[MoreSlugcatsEnums.SlugcatStatsName.Saint].ascended;
		UpdateSelectedSlugcatInMiscProg();
	}

	public bool SlugcatUnlocked(SlugcatStats.Name i)
	{
		if (saveGameData[i] != null)
		{
			return true;
		}
		return SlugcatStats.SlugcatUnlocked(i, manager.rainWorld);
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (colorInterface != null)
		{
			colorInterface.RemoveSprites();
		}
		RemoveColorInterface();
	}

	public void AddColorInterface()
	{
		Vector2 vector = new Vector2(1000f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, manager.rainWorld.options.ScreenSize.y - 100f);
		if (ModManager.JollyCoop)
		{
			vector[1] -= 40f;
		}
		if (colorInterface != null)
		{
			vector[1] -= (float)colorInterface.bodyColors.Length * 40f;
		}
		if (hueSlider == null)
		{
			hueSlider = new HorizontalSlider(this, pages[0], Translate("HUE"), vector, new Vector2(200f, 30f), MMFEnums.SliderID.Hue, subtleSlider: false);
			pages[0].subObjects.Add(hueSlider);
		}
		if (satSlider == null)
		{
			satSlider = new HorizontalSlider(this, pages[0], Translate("SAT"), vector + new Vector2(0f, -40f), new Vector2(200f, 30f), MMFEnums.SliderID.Saturation, subtleSlider: false);
			pages[0].subObjects.Add(satSlider);
		}
		if (litSlider == null)
		{
			litSlider = new HorizontalSlider(this, pages[0], Translate("LIT"), vector + new Vector2(0f, -80f), new Vector2(200f, 30f), MMFEnums.SliderID.Lightness, subtleSlider: false);
			pages[0].subObjects.Add(litSlider);
		}
		float x = 110f;
		if (base.CurrLang == InGameTranslator.LanguageID.Japanese || base.CurrLang == InGameTranslator.LanguageID.French)
		{
			x = 140f;
		}
		else if (base.CurrLang == InGameTranslator.LanguageID.Italian || base.CurrLang == InGameTranslator.LanguageID.Spanish)
		{
			x = 180f;
		}
		if (defaultColorButton == null)
		{
			defaultColorButton = new SimpleButton(this, pages[0], Translate("Restore Default"), "DEFAULTCOL", vector + new Vector2(0f, -120f), new Vector2(x, 30f));
			pages[0].subObjects.Add(defaultColorButton);
		}
		MutualVerticalButtonBind(hueSlider, colorInterface.bodyButtons[colorInterface.bodyButtons.Length - 1]);
		MutualVerticalButtonBind(satSlider, hueSlider);
		MutualVerticalButtonBind(litSlider, satSlider);
		MutualVerticalButtonBind(defaultColorButton, litSlider);
		MutualVerticalButtonBind(nextButton, defaultColorButton);
		nextButton.nextSelectable[3] = colorInterface.bodyButtons[0];
	}

	public void RemoveColorInterface()
	{
		if (hueSlider != null)
		{
			pages[0].RemoveSubObject(hueSlider);
			hueSlider.RemoveSprites();
			hueSlider = null;
		}
		if (satSlider != null)
		{
			pages[0].RemoveSubObject(satSlider);
			satSlider.RemoveSprites();
			satSlider = null;
		}
		if (litSlider != null)
		{
			pages[0].RemoveSubObject(litSlider);
			litSlider.RemoveSprites();
			litSlider = null;
		}
		if (defaultColorButton != null)
		{
			pages[0].RemoveSubObject(defaultColorButton);
			defaultColorButton.RemoveSprites();
			defaultColorButton = null;
		}
		activeColorChooser = -1;
		if (colorInterface != null)
		{
			MutualVerticalButtonBind(nextButton, colorInterface.bodyButtons[colorInterface.bodyButtons.Length - 1]);
			nextButton.nextSelectable[3] = colorInterface.bodyButtons[0];
		}
		else
		{
			nextButton.nextSelectable[1] = null;
			nextButton.nextSelectable[3] = null;
		}
	}

	public CustomColorInterface GetColorInterfaceForSlugcat(SlugcatStats.Name slugcatID, Vector2 pos)
	{
		List<string> names = PlayerGraphics.ColoredBodyPartList(slugcatID);
		List<string> list = PlayerGraphics.DefaultBodyPartColorHex(slugcatID);
		for (int i = 0; i < list.Count; i++)
		{
			Vector3 vector = Custom.RGB2HSL(Custom.hexToColor(list[i]));
			list[i] = vector[0] + "," + vector[1] + "," + vector[2];
		}
		return new CustomColorInterface(this, pages[0], pos, slugcatID, names, list);
	}

	public override void SliderSetValue(Slider slider, float f)
	{
		SlugcatStats.Name name = slugcatColorOrder[slugcatPageIndex];
		int num = activeColorChooser;
		Vector3 vector = new Vector3(1f, 1f, 1f);
		if (manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num].Contains(","))
		{
			string[] array = manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num].Split(',');
			vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
		}
		if (slider.ID == MMFEnums.SliderID.Hue)
		{
			vector[0] = Mathf.Clamp(f, 0f, 0.99f);
			manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0] + "," + vector[1] + "," + vector[2];
		}
		else if (slider.ID == MMFEnums.SliderID.Saturation)
		{
			vector[1] = Mathf.Clamp(f, 0f, 1f);
			Custom.colorToHex(Custom.HSL2RGB(vector[0], vector[1], vector[2]));
			manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0] + "," + vector[1] + "," + vector[2];
		}
		else if (slider.ID == MMFEnums.SliderID.Lightness)
		{
			vector[2] = Mathf.Clamp(f, 0.01f, 1f);
			Custom.colorToHex(Custom.HSL2RGB(vector[0], vector[1], vector[2]));
			manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][num] = vector[0] + "," + vector[1] + "," + vector[2];
		}
		if (colorInterface != null)
		{
			colorInterface.bodyColors[num].color = Custom.HSL2RGB(vector[0], vector[1], vector[2]);
		}
		selectedObject = slider;
	}

	public override float ValueOfSlider(Slider slider)
	{
		SlugcatStats.Name name = slugcatColorOrder[slugcatPageIndex];
		int index = activeColorChooser;
		Vector3 vector = new Vector3(1f, 1f, 1f);
		if (manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index].Contains(","))
		{
			string[] array = manager.rainWorld.progression.miscProgressionData.colorChoices[name.value][index].Split(',');
			vector = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
		}
		if (slider.ID == MMFEnums.SliderID.Hue)
		{
			return vector[0];
		}
		if (slider.ID == MMFEnums.SliderID.Saturation)
		{
			return vector[1];
		}
		if (slider.ID == MMFEnums.SliderID.Lightness)
		{
			return vector[2];
		}
		return 0f;
	}

	public void AddColorButtons()
	{
		if (colorInterface == null)
		{
			colorInterface = GetColorInterfaceForSlugcat(pos: new Vector2(1000f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, manager.rainWorld.options.ScreenSize.y - 100f), slugcatID: slugcatColorOrder[slugcatPageIndex]);
			pages[0].subObjects.Add(colorInterface);
		}
	}

	public void RemoveColorButtons()
	{
		if (colorInterface != null)
		{
			colorInterface.RemoveSprites();
			pages[0].RemoveSubObject(colorInterface);
			colorInterface = null;
		}
		RemoveColorInterface();
	}

	public void AddJollyButtons()
	{
		Vector2 vector = new Vector2(1056f, manager.rainWorld.screenSize.y - 100f);
		jollyToggleConfigMenu = new SimpleButton(this, pages[0], Translate("SHOW"), "JOLLY_TOGGLE_CONFIG", vector, new Vector2(110f, 30f));
		pages[0].subObjects.Add(jollyToggleConfigMenu);
		jollyOptionsLabel = new MenuLabel(this, pages[0], Translate("JOLLY COOP"), jollyToggleConfigMenu.pos + new Vector2(-75f, jollyToggleConfigMenu.size.y / 2f), Vector2.zero, bigText: true);
		pages[0].subObjects.Add(jollyOptionsLabel);
		jollyOptionsLabel.pos.x = jollyToggleConfigMenu.pos.x - jollyOptionsLabel.label.textRect.width / 2f - 20f;
		jollyPlayerCountLabel = new MenuLabel(this, pages[0], Translate("Players: <num_p>").Replace("<num_p>", Custom.rainWorld.options.JollyPlayerCount.ToString()), vector + new Vector2(0f, 30f), new Vector2(110f, 30f), bigText: false);
		pages[0].subObjects.Add(jollyPlayerCountLabel);
		RefreshJollySummary();
	}

	public void RefreshJollySummary()
	{
		Vector2 vector = new Vector2(1056f, manager.rainWorld.screenSize.y - 100f);
		bool flag = false;
		if (playerSummaries == null || playerSummaries.Count != Custom.rainWorld.options.JollyPlayerCount)
		{
			flag = true;
			RemoveJollySummary();
		}
		if (flag)
		{
			playerSummaries = new List<MenuLabel>();
		}
		for (int i = 0; i < Custom.rainWorld.options.JollyPlayerCount; i++)
		{
			SlugcatStats.Name i2 = JollyCustom.SlugClassMenu(i, colorFromIndex(slugcatPageIndex));
			string text = Translate("The " + SlugcatStats.getSlugcatName(i2));
			Vector2 pos = vector - new Vector2(0f, 30f + 25f * (float)i);
			if (flag)
			{
				MenuLabel item = new MenuLabel(this, pages[0], text, pos, new Vector2(110f, 30f), bigText: false);
				playerSummaries.Add(item);
				pages[0].subObjects.Add(item);
			}
			else
			{
				playerSummaries[i].text = text;
			}
		}
	}

	public void RemoveJollySummary()
	{
		if (playerSummaries == null)
		{
			return;
		}
		foreach (MenuLabel playerSummary in playerSummaries)
		{
			playerSummary.RemoveSprites();
			pages[0].RemoveSubObject(playerSummary);
		}
		playerSummaries.Clear();
		playerSummaries = null;
	}

	public void RemoveJollyButtons()
	{
		if (jollyToggleConfigMenu != null)
		{
			jollyToggleConfigMenu.RemoveSprites();
			pages[0].RemoveSubObject(jollyToggleConfigMenu);
			jollyToggleConfigMenu = null;
		}
		if (jollyOptionsLabel != null)
		{
			jollyOptionsLabel.RemoveSprites();
			pages[0].RemoveSubObject(jollyOptionsLabel);
			jollyOptionsLabel = null;
		}
		if (jollyPlayerCountLabel != null)
		{
			jollyPlayerCountLabel.RemoveSprites();
			pages[0].RemoveSubObject(jollyPlayerCountLabel);
			jollyPlayerCountLabel = null;
		}
		RemoveJollySummary();
	}
}
