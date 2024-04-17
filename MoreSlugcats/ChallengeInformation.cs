using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ArenaBehaviors;
using Menu;
using UnityEngine;

namespace MoreSlugcats;

public class ChallengeInformation : PositionedMenuObject
{
	public class ChallengeMeta
	{
		public class WinCondition : ExtEnum<WinCondition>
		{
			public static readonly WinCondition KILL = new WinCondition("KILL", register: true);

			public static readonly WinCondition POINTS = new WinCondition("POINTS", register: true);

			public static readonly WinCondition SURVIVE = new WinCondition("SURVIVE", register: true);

			public static readonly WinCondition TAME = new WinCondition("TAME", register: true);

			public static readonly WinCondition NONE = new WinCondition("NONE", register: true);

			public static readonly WinCondition BRING = new WinCondition("BRING", register: true);

			public static readonly WinCondition ARMOR = new WinCondition("ARMOR", register: true);

			public static readonly WinCondition PROTECT = new WinCondition("PROTECT", register: true);

			public static readonly WinCondition POPCORN = new WinCondition("POPCORN", register: true);

			public static readonly WinCondition PARRY = new WinCondition("PARRY", register: true);

			public WinCondition(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public string arena;

		public int spawnDen;

		public SlugcatStats.Name slugcatClass;

		public int rainTime;

		public WinCondition winMethod;

		public int points;

		public int surviveTime;

		public bool batflies;

		public string globalTag;

		public string name;

		public bool precycle;

		public string tameCreature;

		public WinCondition secondaryWinMethod;

		public string bringItem;

		public bool ascended;

		public bool musicMuted;

		public string specificMusic;

		public bool specialUnlock;

		public bool aiIcon;

		public bool deferred;

		public string killCreature;

		public bool fly_no_burrow;

		public int fly_max_y;

		public int fly_min_y;

		public bool invincibleCreatures;

		public string protectCreature;

		public int tamingDifficultyMultiplier;

		public bool nonAggressive;

		public bool oobProtect;

		public float resistMultiplier;

		public ArenaSetup.GameTypeSetup.WildLifeSetting arenaSpawns;

		public string threatMusic;

		public int challengeNumber;

		public int seed;

		public int parries;

		public int spawnDen2;

		public int spawnDen3;

		public int spawnDen4;

		public bool unlimitedDanger;

		public ChallengeMeta(int challengeID)
		{
			winMethod = WinCondition.NONE;
			secondaryWinMethod = WinCondition.NONE;
			arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.Off;
			fly_min_y = -1;
			fly_max_y = -1;
			seed = -1;
			spawnDen2 = -1;
			spawnDen3 = -1;
			spawnDen4 = -1;
			challengeNumber = challengeID;
			string path = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + "Challenges" + Path.DirectorySeparatorChar + "Challenge" + challengeID + "_Meta.txt");
			Dictionary<string, object> dictionary = null;
			if (File.Exists(path))
			{
				dictionary = File.ReadAllText(path).dictionaryFromJson();
			}
			if (dictionary == null)
			{
				return;
			}
			if (dictionary.ContainsKey("name"))
			{
				name = dictionary["name"].ToString();
			}
			if (dictionary.ContainsKey("arena"))
			{
				arena = dictionary["arena"].ToString();
			}
			if (dictionary.ContainsKey("spawn"))
			{
				spawnDen = (int)(long)dictionary["spawn"];
			}
			if (dictionary.ContainsKey("spawn2"))
			{
				spawnDen2 = (int)(long)dictionary["spawn2"];
			}
			if (dictionary.ContainsKey("spawn3"))
			{
				spawnDen3 = (int)(long)dictionary["spawn3"];
			}
			if (dictionary.ContainsKey("spawn4"))
			{
				spawnDen4 = (int)(long)dictionary["spawn4"];
			}
			if (dictionary.ContainsKey("seed"))
			{
				seed = (int)(long)dictionary["seed"];
			}
			if (dictionary.ContainsKey("class"))
			{
				slugcatClass = SlugcatStats.Name.White;
				if (ExtEnum<SlugcatStats.Name>.values.entries.Contains(dictionary["class"].ToString()))
				{
					slugcatClass = new SlugcatStats.Name(dictionary["class"].ToString());
				}
			}
			if (dictionary.ContainsKey("rain_time"))
			{
				rainTime = (int)(long)dictionary["rain_time"];
			}
			if (dictionary.ContainsKey("survive_time"))
			{
				surviveTime = (int)(long)dictionary["survive_time"];
			}
			if (dictionary.ContainsKey("points"))
			{
				points = (int)(long)dictionary["points"];
			}
			if (dictionary.ContainsKey("parries"))
			{
				parries = (int)(long)dictionary["parries"];
			}
			if (dictionary.ContainsKey("tame_creature"))
			{
				tameCreature = dictionary["tame_creature"].ToString();
			}
			if (dictionary.ContainsKey("kill_creature"))
			{
				killCreature = dictionary["kill_creature"].ToString();
			}
			if (dictionary.ContainsKey("protect_creature"))
			{
				protectCreature = dictionary["protect_creature"].ToString();
			}
			if (dictionary.ContainsKey("item"))
			{
				bringItem = dictionary["item"].ToString();
			}
			if (dictionary.ContainsKey("batflies"))
			{
				batflies = (bool)dictionary["batflies"];
			}
			if (dictionary.ContainsKey("fly_max_y"))
			{
				fly_max_y = (int)(long)dictionary["fly_max_y"];
			}
			if (dictionary.ContainsKey("fly_min_y"))
			{
				fly_min_y = (int)(long)dictionary["fly_min_y"];
			}
			if (dictionary.ContainsKey("fly_no_burrow"))
			{
				fly_no_burrow = (bool)dictionary["fly_no_burrow"];
			}
			if (dictionary.ContainsKey("taming_difficulty"))
			{
				tamingDifficultyMultiplier = (int)(long)dictionary["taming_difficulty"];
			}
			if (dictionary.ContainsKey("resist_multiplier"))
			{
				try
				{
					resistMultiplier = (float)(double)dictionary["resist_multiplier"];
				}
				catch
				{
					resistMultiplier = (long)dictionary["resist_multiplier"];
				}
			}
			if (dictionary.ContainsKey("precycle"))
			{
				precycle = (bool)dictionary["precycle"];
			}
			if (dictionary.ContainsKey("unlimited_danger"))
			{
				unlimitedDanger = (bool)dictionary["unlimited_danger"];
			}
			if (dictionary.ContainsKey("deferred"))
			{
				deferred = (bool)dictionary["deferred"];
			}
			if (dictionary.ContainsKey("invincible_creatures"))
			{
				invincibleCreatures = (bool)dictionary["invincible_creatures"];
			}
			if (dictionary.ContainsKey("ascended"))
			{
				ascended = (bool)dictionary["ascended"];
			}
			if (dictionary.ContainsKey("arena_spawns"))
			{
				if (dictionary["arena_spawns"].ToString().ToLower() == "high")
				{
					arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.High;
				}
				else if (dictionary["arena_spawns"].ToString().ToLower() == "medium" || dictionary["arena_spawns"].ToString().ToLower() == "med" || dictionary["arena_spawns"].ToString().ToLower() == "mid")
				{
					arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.Medium;
				}
				else if (dictionary["arena_spawns"].ToString().ToLower() == "low")
				{
					arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.Low;
				}
				else
				{
					arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.Off;
				}
			}
			else
			{
				arenaSpawns = ArenaSetup.GameTypeSetup.WildLifeSetting.Off;
			}
			if (dictionary.ContainsKey("music_mute"))
			{
				musicMuted = (bool)dictionary["music_mute"];
			}
			if (dictionary.ContainsKey("music"))
			{
				specificMusic = dictionary["music"].ToString();
			}
			if (dictionary.ContainsKey("music_threat"))
			{
				threatMusic = dictionary["music_threat"].ToString();
			}
			if (dictionary.ContainsKey("global_tag"))
			{
				globalTag = dictionary["global_tag"].ToString();
			}
			if (dictionary.ContainsKey("ai_icon"))
			{
				aiIcon = (bool)dictionary["ai_icon"];
			}
			if (dictionary.ContainsKey("special_unlock"))
			{
				specialUnlock = (bool)dictionary["special_unlock"];
			}
			if (dictionary.ContainsKey("nonaggressive"))
			{
				nonAggressive = (bool)dictionary["nonaggressive"];
			}
			if (dictionary.ContainsKey("allow_oob_creatures"))
			{
				oobProtect = (bool)dictionary["allow_oob_creatures"];
			}
			if (dictionary.ContainsKey("win"))
			{
				string text = dictionary["win"].ToString().ToLower();
				if (text.Contains("kill"))
				{
					winMethod = WinCondition.KILL;
				}
				else if (text.Contains("point"))
				{
					winMethod = WinCondition.POINTS;
				}
				else if (text.Contains("tame"))
				{
					winMethod = WinCondition.TAME;
				}
				else if (text.Contains("bring"))
				{
					winMethod = WinCondition.BRING;
				}
				else if (text.Contains("armor"))
				{
					winMethod = WinCondition.ARMOR;
				}
				else if (text.Contains("popcorn"))
				{
					winMethod = WinCondition.POPCORN;
				}
				else if (text.Contains("parry"))
				{
					winMethod = WinCondition.PARRY;
				}
				else
				{
					winMethod = WinCondition.SURVIVE;
				}
			}
			if (dictionary.ContainsKey("win2"))
			{
				string text2 = dictionary["win2"].ToString().ToLower();
				if (text2.Contains("kill"))
				{
					secondaryWinMethod = WinCondition.KILL;
				}
				else if (text2.Contains("protect"))
				{
					secondaryWinMethod = WinCondition.PROTECT;
				}
				else if (text2.Contains("point"))
				{
					secondaryWinMethod = WinCondition.POINTS;
				}
				else if (text2.Contains("tame"))
				{
					secondaryWinMethod = WinCondition.TAME;
				}
				else if (text2.Contains("armor"))
				{
					secondaryWinMethod = WinCondition.ARMOR;
				}
				else if (text2.Contains("bring"))
				{
					secondaryWinMethod = WinCondition.BRING;
				}
				else if (text2.Contains("popcorn"))
				{
					secondaryWinMethod = WinCondition.POPCORN;
				}
				else if (text2.Contains("parry"))
				{
					secondaryWinMethod = WinCondition.PARRY;
				}
				else
				{
					secondaryWinMethod = WinCondition.SURVIVE;
				}
			}
		}

		public string GetMetaDescription(global::Menu.Menu menu)
		{
			string text = "";
			if (winMethod == WinCondition.KILL)
			{
				string text2 = "Kill";
				if (ascended)
				{
					text2 = "Ascend";
				}
				if (killCreature == null || killCreature == "")
				{
					text = menu.Translate(text2 + " all creatures");
				}
				else
				{
					string baseName = killCreature;
					text = menu.Translate(text2 + " every ##").Replace("##", ResolveCreatureName(menu, baseName));
				}
			}
			else if (winMethod == WinCondition.TAME)
			{
				if (tameCreature == null || tameCreature == "")
				{
					text = menu.Translate("Befriend a creature");
				}
				else
				{
					string baseName2 = tameCreature;
					text = menu.Translate("Befriend a ##").Replace("##", ResolveCreatureName(menu, baseName2));
				}
			}
			else if (winMethod == WinCondition.POINTS)
			{
				text = ((points != 1) ? menu.Translate("Gain ## points").Replace("##", points.ToString()) : menu.Translate("Gain ## point").Replace("##", points.ToString()));
			}
			else if (winMethod == WinCondition.SURVIVE)
			{
				text = menu.Translate("Survive for ## seconds").Replace("##", surviveTime.ToString());
			}
			else if (winMethod == WinCondition.ARMOR)
			{
				text = menu.Translate("Destroy all armored plates");
			}
			else if (winMethod == WinCondition.POPCORN)
			{
				text = menu.Translate("Pop every plant");
			}
			else if (winMethod == WinCondition.PARRY)
			{
				text = ((parries != 1) ? menu.Translate("Parry ## attacks").Replace("##", parries.ToString()) : menu.Translate("Parry ## attack").Replace("##", parries.ToString()));
			}
			else if (winMethod == WinCondition.BRING)
			{
				string text3 = "";
				for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
				{
					if (StaticWorld.creatureTemplates[i].type.ToString() == bringItem)
					{
						text3 = StaticWorld.creatureTemplates[i].name;
						break;
					}
				}
				text = ((!(text3 == "")) ? menu.Translate("Escort a ## to a den").Replace("##", ResolveCreatureName(menu, bringItem)) : menu.Translate("Put a ## in a den").Replace("##", ResolveItemName(menu, bringItem)));
			}
			if (secondaryWinMethod != WinCondition.NONE)
			{
				string text4 = "";
				if (secondaryWinMethod == WinCondition.KILL)
				{
					string text5 = "Kill";
					if (ascended)
					{
						text5 = "Ascend";
					}
					if (killCreature == null || killCreature == "")
					{
						text4 += menu.Translate(text5 + " all creatures");
					}
					else
					{
						string baseName3 = killCreature;
						text4 += menu.Translate(text5 + " every ##").Replace("##", ResolveCreatureName(menu, baseName3));
					}
				}
				else if (secondaryWinMethod == WinCondition.PROTECT)
				{
					if (protectCreature == null || protectCreature == "")
					{
						text = menu.Translate("##<LINE>without anything dying").Replace("##", text).Replace("<LINE>", "\r\n");
					}
					else
					{
						string baseName4 = protectCreature;
						text = menu.Translate("##<LINE>without letting a @@ die").Replace("##", text).Replace("@@", ResolveCreatureName(menu, baseName4))
							.Replace("<LINE>", "\r\n");
					}
				}
				else if (secondaryWinMethod == WinCondition.TAME)
				{
					if (tameCreature == null || tameCreature == "")
					{
						text4 += menu.Translate("Befriend a creature");
					}
					else
					{
						string baseName5 = tameCreature;
						text4 += menu.Translate("Befriend a ##").Replace("##", ResolveCreatureName(menu, baseName5));
					}
				}
				else if (secondaryWinMethod == WinCondition.POINTS)
				{
					text4 = ((points != 1) ? (text4 + menu.Translate("Gain ## points").Replace("##", points.ToString())) : (text4 + menu.Translate("Gain ## point").Replace("##", points.ToString())));
				}
				else if (secondaryWinMethod == WinCondition.SURVIVE)
				{
					text4 += menu.Translate("Survive for ## seconds").Replace("##", surviveTime.ToString());
				}
				else if (secondaryWinMethod == WinCondition.ARMOR)
				{
					text4 += menu.Translate("Destroy all armored plates");
				}
				else if (secondaryWinMethod == WinCondition.POPCORN)
				{
					text4 += menu.Translate("Pop every plant");
				}
				else if (secondaryWinMethod == WinCondition.PARRY)
				{
					text4 = ((parries != 1) ? (text4 + menu.Translate("Parry ## attacks").Replace("##", parries.ToString())) : (text4 + menu.Translate("Parry ## attack").Replace("##", parries.ToString())));
				}
				else if (secondaryWinMethod == WinCondition.BRING)
				{
					string text6 = "";
					for (int j = 0; j < StaticWorld.creatureTemplates.Length; j++)
					{
						if (StaticWorld.creatureTemplates[j].type.ToString() == bringItem)
						{
							text6 = StaticWorld.creatureTemplates[j].name;
							break;
						}
					}
					text4 = ((!(text6 == "")) ? (text4 + menu.Translate("Escort a ## to a den").Replace("##", ResolveCreatureName(menu, bringItem))) : (text4 + menu.Translate("Put a ## in a den").Replace("##", ResolveItemName(menu, bringItem))));
				}
				if (secondaryWinMethod != WinCondition.PROTECT)
				{
					text = menu.Translate("<X> and <Y>").Replace("<X>", text + Environment.NewLine).Replace("<Y>", text4.ToLowerInvariant());
				}
			}
			return text;
		}

		public string ResolveCreatureName(global::Menu.Menu menu, string baseName)
		{
			if (menu.manager.rainWorld.inGameTranslator.HasShortstringTranslation("creaturetype-" + baseName))
			{
				return menu.Translate("creaturetype-" + baseName);
			}
			return baseName;
		}

		public string ResolveItemName(global::Menu.Menu menu, string baseName)
		{
			if (menu.manager.rainWorld.inGameTranslator.HasShortstringTranslation("objecttype-" + baseName))
			{
				return menu.Translate("objecttype-" + baseName);
			}
			if (menu.manager.rainWorld.inGameTranslator.HasShortstringTranslation("creaturetype-" + baseName))
			{
				return menu.Translate("creaturetype-" + baseName);
			}
			return baseName;
		}
	}

	public ChallengeMeta meta;

	public FSprite arenaPreview;

	private Vector2 posOffset;

	public RoundedRect thumbnailRect;

	public List<SandboxEditor.PlacedIconData> contents;

	public List<CreatureSymbol> creatureSymbols;

	public List<ItemSymbol> itemSymbols;

	public MenuLabel creatureLabel;

	public MenuLabel itemLabel;

	public FSprite textBoxBack;

	public FSprite stIcon;

	public bool unlocked;

	public MultiplayerMenu GetMultiplayerMenu => menu as MultiplayerMenu;

	public ChallengeInformation(global::Menu.Menu menu, MenuObject owner, int challengeID)
		: base(menu, owner, new Vector2(0f, 0f))
	{
		bool flag = GetMultiplayerMenu.IsChallengeUnlocked(GetMultiplayerMenu.manager.rainWorld.progression, challengeID);
		unlocked = flag;
		pos = Vector2.zero;
		posOffset = new Vector2(350f, 115f);
		if (!unlocked)
		{
			meta = new ChallengeMeta(-1);
			contents = new List<SandboxEditor.PlacedIconData>();
		}
		else
		{
			meta = new ChallengeMeta(challengeID);
			contents = LoadChallengeConfiguration(challengeID);
		}
		creatureSymbols = new List<CreatureSymbol>();
		itemSymbols = new List<ItemSymbol>();
		float num = posOffset.x + 200f;
		float num2 = posOffset.y + 70f;
		float num3 = 38f;
		RoundedRect roundedRect = new RoundedRect(menu, this, posOffset, new Vector2(660f, 200f), filled: false);
		textBoxBack = new FSprite("pixel");
		textBoxBack.color = new Color(0f, 0f, 0f);
		textBoxBack.anchorX = 0f;
		textBoxBack.anchorY = 0f;
		textBoxBack.scaleX = roundedRect.size.x - 6f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
		textBoxBack.scaleY = roundedRect.size.y - 12f;
		textBoxBack.x = roundedRect.pos.x + 6f;
		textBoxBack.y = roundedRect.pos.y + 6f;
		textBoxBack.alpha = 0.65f;
		Container.AddChild(textBoxBack);
		for (int i = 0; i < contents.Count; i++)
		{
			IconSymbol iconSymbol = IconSymbol.CreateIconSymbol(contents[i].data, Container);
			iconSymbol.Show(showShadowSprites: true);
			if (iconSymbol is CreatureSymbol)
			{
				creatureSymbols.Add(iconSymbol as CreatureSymbol);
			}
			else if (iconSymbol is ItemSymbol)
			{
				itemSymbols.Add(iconSymbol as ItemSymbol);
			}
		}
		if (meta.arenaSpawns != ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
		{
			for (int j = 0; j < creatureSymbols.Count; j++)
			{
				creatureSymbols[j].RemoveSprites();
			}
			creatureSymbols.Clear();
		}
		string text = "";
		if (meta.name != null)
		{
			text = ": " + meta.name;
		}
		MenuLabel item = new MenuLabel(menu, this, menu.Translate("Challenge #<X>").Replace("<X>", challengeID.ToString()) + text, new Vector2(roundedRect.pos.x + roundedRect.size.x / 2f, roundedRect.pos.y + roundedRect.size.y - 85f), new Vector2(0f, 100f), bigText: true);
		subObjects.Add(item);
		subObjects.Add(roundedRect);
		if (unlocked)
		{
			thumbnailRect = new RoundedRect(menu, this, posOffset + new Vector2(50f, 30f), new Vector2((float)LevelSelector.ThumbWidth + 20f, (float)LevelSelector.ThumbHeight + 20f), filled: false);
			subObjects.Add(thumbnailRect);
			MenuLabel item2 = new MenuLabel(menu, this, menu.Translate("Arena") + ":", new Vector2(thumbnailRect.pos.x + thumbnailRect.size.x / 2f, thumbnailRect.pos.y + num3), new Vector2(0f, 100f), bigText: false);
			subObjects.Add(item2);
			MenuLabel item3 = new MenuLabel(menu, this, menu.Translate("Class") + ":  " + menu.Translate(SlugcatStats.getSlugcatName(meta.slugcatClass)) + (meta.ascended ? (" (" + menu.Translate("Ascended") + ")") : ""), new Vector2(num, num2), new Vector2(0f, 100f), bigText: false)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(item3);
			string metaDescription = meta.GetMetaDescription(menu);
			MenuLabel item4 = new MenuLabel(menu, this, menu.Translate("Win Condition") + ": " + Environment.NewLine + metaDescription, new Vector2(num, num2 - num3 * 2f), new Vector2(0f, 100f), bigText: false)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(item4);
			string text2 = menu.Translate("## Seconds").Replace("##", meta.rainTime.ToString());
			if (meta.rainTime <= 0)
			{
				text2 = menu.Translate("None");
			}
			MenuLabel item5 = new MenuLabel(menu, this, menu.Translate("Time Limit") + ":  " + text2, new Vector2(num, num2 - num3), new Vector2(0f, 100f), bigText: false)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(item5);
			creatureLabel = new MenuLabel(menu, this, menu.Translate("Creatures") + ":", new Vector2(num + 160f, num2 - 20f), new Vector2(0f, 100f), bigText: false);
			creatureLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(creatureLabel);
			itemLabel = new MenuLabel(menu, this, menu.Translate("Items") + ":", new Vector2(num + 160f, num2 - num3 - 20f), new Vector2(0f, 100f), bigText: false);
			itemLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(itemLabel);
			if (meta.arenaSpawns != ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
			{
				stIcon = new FSprite("Sandbox_SmallQuestionmark");
				Container.AddChild(stIcon);
			}
			else if (meta.aiIcon)
			{
				if (meta.arena == "Chal_AI")
				{
					stIcon = new FSprite("GuidanceST");
				}
				else
				{
					stIcon = new FSprite("GuidanceMoon");
				}
				Container.AddChild(stIcon);
			}
			GetMultiplayerMenu.levelSelector.BumpUpThumbnailLoad(meta.arena);
		}
		else
		{
			MenuLabel item6 = new MenuLabel(menu, this, GetMultiplayerMenu.ChallengeUnlockDescription(challengeID), new Vector2(roundedRect.pos.x + roundedRect.size.x / 2f, roundedRect.pos.y + roundedRect.size.y / 2f - 16f), new Vector2(0f, 32f), bigText: false);
			subObjects.Add(item6);
			Update();
			GrafUpdate(0f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (!unlocked)
		{
			return;
		}
		if (arenaPreview == null)
		{
			if (IsThumbnailLoaded(meta.arena))
			{
				arenaPreview = new FSprite(meta.arena + "_Thumb");
				Container.AddChild(arenaPreview);
				arenaPreview.x = thumbnailRect.pos.x + (float)LevelSelector.ThumbWidth * 0.5f + 10f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
				arenaPreview.y = thumbnailRect.pos.y + (float)LevelSelector.ThumbHeight * 0.5f + 10f;
				return;
			}
		}
		else
		{
			arenaPreview.x = thumbnailRect.pos.x + (float)LevelSelector.ThumbWidth * 0.5f + 10f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
			arenaPreview.y = thumbnailRect.pos.y + (float)LevelSelector.ThumbHeight * 0.5f + 10f;
		}
		float creatureOffset = 80f;
		float itemOffset = 50f;
		GetOffset(ref creatureOffset, ref itemOffset);
		int num = 13;
		for (int i = 0; i < creatureSymbols.Count; i++)
		{
			creatureSymbols[i].Draw(timeStacker, new Vector2(creatureLabel.pos.x + creatureOffset + (float)(i % num) * 16f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, creatureLabel.pos.y + 48f + 16f * Mathf.Floor(i / num)));
		}
		int num2 = 23 - (int)((itemOffset - 50f) / 5f);
		for (int j = 0; j < itemSymbols.Count; j++)
		{
			itemSymbols[j].Draw(timeStacker, new Vector2(itemLabel.pos.x + itemOffset + (float)(j % num2) * 8f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, itemLabel.pos.y + 48f - 16f * Mathf.Floor(j / num2)));
		}
		if (stIcon != null)
		{
			stIcon.x = creatureLabel.pos.x + creatureOffset - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f;
			stIcon.y = creatureLabel.pos.y + 48f;
		}
	}

	private void GetOffset(ref float creatureOffset, ref float itemOffset)
	{
		if (menu.CurrLang == InGameTranslator.LanguageID.Italian)
		{
			itemOffset = 65f;
		}
		else if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			itemOffset = 90f;
		}
		else if (menu.CurrLang == InGameTranslator.LanguageID.Japanese || menu.CurrLang == InGameTranslator.LanguageID.Korean)
		{
			itemOffset = 80f;
		}
		else if (menu.CurrLang == InGameTranslator.LanguageID.Spanish)
		{
			itemOffset = 60f;
		}
		else if (menu.CurrLang == InGameTranslator.LanguageID.Russian)
		{
			itemOffset = 85f;
			creatureOffset = 85f;
		}
		else if (menu.CurrLang == InGameTranslator.LanguageID.Chinese)
		{
			creatureOffset = 55f;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		textBoxBack.RemoveFromContainer();
		if (arenaPreview != null)
		{
			arenaPreview.RemoveFromContainer();
		}
		if (stIcon != null)
		{
			stIcon.RemoveFromContainer();
		}
		for (int i = 0; i < creatureSymbols.Count; i++)
		{
			creatureSymbols[i].RemoveSprites();
		}
		for (int j = 0; j < itemSymbols.Count; j++)
		{
			itemSymbols[j].RemoveSprites();
		}
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

	public static List<SandboxEditor.PlacedIconData> LoadChallengeConfiguration(int challengeID)
	{
		List<SandboxEditor.PlacedIconData> list = new List<SandboxEditor.PlacedIconData>();
		string path = ChallengePath(challengeID);
		if (!File.Exists(path))
		{
			return list;
		}
		string[] array = Regex.Split(File.ReadAllText(path), "<sbA>");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 0)
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], "<sbB>");
			if (!(array2[0] == "CONFIG") || int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) != 0)
			{
				continue;
			}
			for (int j = 2; j < array2.Length; j++)
			{
				string[] array3 = Regex.Split(array2[j], "<sbC>");
				if (array3.Length >= 3)
				{
					Vector2 vector = new Vector2(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
					IconSymbol.IconSymbolData data = IconSymbol.IconSymbolData.IconSymbolDataFromString(array3[2]);
					EntityID iD = EntityID.FromString(array3[3]);
					list.Add(new SandboxEditor.PlacedIconData(vector, data, iD));
				}
			}
		}
		return list;
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < creatureSymbols.Count; i++)
		{
			creatureSymbols[i].Update();
		}
		for (int j = 0; j < itemSymbols.Count; j++)
		{
			itemSymbols[j].Update();
		}
	}

	public static string ChallengePath(int challengeID)
	{
		return AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + "Challenges" + Path.DirectorySeparatorChar + "Challenge" + challengeID + ".txt");
	}
}
