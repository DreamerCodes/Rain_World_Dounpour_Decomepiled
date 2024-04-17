using System.Collections.Generic;
using System.Text.RegularExpressions;
using Menu;
using UnityEngine;

namespace MoreSlugcats;

public class CollectiblesTracker : PositionedMenuObject
{
	public class SaveGameData
	{
		public List<string> regionsVisited;

		public List<MultiplayerUnlocks.SandboxUnlockID> unlockedBlues;

		public List<MultiplayerUnlocks.LevelUnlockID> unlockedGolds;

		public List<MultiplayerUnlocks.SlugcatUnlockID> unlockedGreens;

		public List<ChatlogData.ChatlogID> unlockedGreys;

		public List<MultiplayerUnlocks.SafariUnlockID> unlockedReds;

		public string currentRegion;
	}

	public List<string> displayRegions;

	public SaveGameData collectionData;

	public Dictionary<string, List<FSprite>> sprites;

	public Dictionary<string, List<Color>> spriteColors;

	public FSprite[] regionIcons;

	public CollectiblesTracker(global::Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, SlugcatStats.Name saveSlot)
		: base(menu, owner, pos)
	{
		RainWorld rainWorld = menu.manager.rainWorld;
		displayRegions = SlugcatStats.SlugcatStoryRegions(saveSlot);
		collectionData = MineForSaveData(menu.manager, saveSlot);
		List<string> list = SlugcatStats.SlugcatOptionalRegions(saveSlot);
		if (collectionData == null)
		{
			collectionData = new SaveGameData();
			collectionData.currentRegion = "??";
			collectionData.regionsVisited = new List<string>();
		}
		for (int i = 0; i < displayRegions.Count; i++)
		{
			displayRegions[i] = displayRegions[i].ToLowerInvariant();
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j] = list[j].ToLowerInvariant();
			for (int k = 0; k < collectionData.regionsVisited.Count; k++)
			{
				if (collectionData.regionsVisited[k] == list[j])
				{
					displayRegions.Add(list[j]);
					break;
				}
			}
		}
		sprites = new Dictionary<string, List<FSprite>>();
		spriteColors = new Dictionary<string, List<Color>>();
		regionIcons = new FSprite[displayRegions.Count];
		for (int l = 0; l < displayRegions.Count; l++)
		{
			if (collectionData != null && displayRegions[l] == collectionData.currentRegion)
			{
				regionIcons[l] = new FSprite("keyShiftB");
				regionIcons[l].rotation = 180f;
				regionIcons[l].scale = 0.5f;
			}
			else
			{
				regionIcons[l] = new FSprite("Circle4");
			}
			regionIcons[l].color = Color.Lerp(Region.RegionColor(displayRegions[l]), Color.white, 0.25f);
			container.AddChild(regionIcons[l]);
			sprites[displayRegions[l]] = new List<FSprite>();
			spriteColors[displayRegions[l]] = new List<Color>();
			if (collectionData == null || !collectionData.regionsVisited.Contains(displayRegions[l]))
			{
				regionIcons[l].isVisible = false;
				spriteColors[displayRegions[l]].Add(CollectToken.WhiteColor.rgb);
				sprites[displayRegions[l]].Add(new FSprite("ctNone"));
			}
			else
			{
				for (int m = 0; m < rainWorld.regionGoldTokens[displayRegions[l]].Count; m++)
				{
					if (rainWorld.regionGoldTokensAccessibility[displayRegions[l]][m].Contains(saveSlot))
					{
						spriteColors[displayRegions[l]].Add(new Color(1f, 0.6f, 0.05f));
						if (!collectionData.unlockedGolds.Contains(rainWorld.regionGoldTokens[displayRegions[l]][m]))
						{
							sprites[displayRegions[l]].Add(new FSprite("ctOff"));
						}
						else
						{
							sprites[displayRegions[l]].Add(new FSprite("ctOn"));
						}
					}
				}
				for (int n = 0; n < rainWorld.regionBlueTokens[displayRegions[l]].Count; n++)
				{
					if (rainWorld.regionBlueTokensAccessibility[displayRegions[l]][n].Contains(saveSlot))
					{
						spriteColors[displayRegions[l]].Add(RainWorld.AntiGold.rgb);
						if (!collectionData.unlockedBlues.Contains(rainWorld.regionBlueTokens[displayRegions[l]][n]))
						{
							sprites[displayRegions[l]].Add(new FSprite("ctOff"));
						}
						else
						{
							sprites[displayRegions[l]].Add(new FSprite("ctOn"));
						}
					}
				}
				if (ModManager.MSC)
				{
					for (int num = 0; num < rainWorld.regionGreenTokens[displayRegions[l]].Count; num++)
					{
						if (rainWorld.regionGreenTokensAccessibility[displayRegions[l]][num].Contains(saveSlot))
						{
							spriteColors[displayRegions[l]].Add(CollectToken.GreenColor.rgb);
							if (!collectionData.unlockedGreens.Contains(rainWorld.regionGreenTokens[displayRegions[l]][num]))
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOff"));
							}
							else
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOn"));
							}
						}
					}
					if (saveSlot == MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						for (int num2 = 0; num2 < rainWorld.regionGreyTokens[displayRegions[l]].Count; num2++)
						{
							spriteColors[displayRegions[l]].Add(CollectToken.WhiteColor.rgb);
							if (!collectionData.unlockedGreys.Contains(rainWorld.regionGreyTokens[displayRegions[l]][num2]))
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOff"));
							}
							else
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOn"));
							}
						}
					}
					for (int num3 = 0; num3 < rainWorld.regionRedTokens[displayRegions[l]].Count; num3++)
					{
						if (rainWorld.regionRedTokensAccessibility[displayRegions[l]][num3].Contains(saveSlot))
						{
							spriteColors[displayRegions[l]].Add(CollectToken.RedColor.rgb);
							if (!collectionData.unlockedReds.Contains(rainWorld.regionRedTokens[displayRegions[l]][num3]))
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOff"));
							}
							else
							{
								sprites[displayRegions[l]].Add(new FSprite("ctOn"));
							}
						}
					}
				}
			}
			for (int num4 = 0; num4 < sprites[displayRegions[l]].Count; num4++)
			{
				sprites[displayRegions[l]][num4].color = spriteColors[displayRegions[l]][num4];
				container.AddChild(sprites[displayRegions[l]][num4]);
			}
		}
	}

	public SaveGameData MineForSaveData(ProcessManager manager, SlugcatStats.Name slugcat)
	{
		if (!manager.rainWorld.progression.IsThereASavedGame(slugcat))
		{
			return null;
		}
		SaveState saveState = null;
		if (manager.rainWorld.progression.currentSaveState != null)
		{
			saveState = manager.rainWorld.progression.currentSaveState;
		}
		else if (manager.rainWorld.progression.starvedSaveState != null)
		{
			saveState = manager.rainWorld.progression.starvedSaveState;
		}
		if (saveState != null && saveState.saveStateNumber == slugcat)
		{
			SaveGameData saveGameData = new SaveGameData();
			saveGameData.currentRegion = saveState.denPosition;
			if (saveGameData.currentRegion.Contains("_"))
			{
				saveGameData.currentRegion = saveGameData.currentRegion.Substring(0, saveGameData.currentRegion.IndexOf("_"));
			}
			saveGameData.currentRegion = saveGameData.currentRegion.ToLowerInvariant();
			saveGameData.regionsVisited = new List<string>();
			for (int i = 0; i < saveState.regionStates.Length; i++)
			{
				if (saveState.regionStates[i] != null)
				{
					saveGameData.regionsVisited.Add(saveState.regionStates[i].regionName.ToLowerInvariant());
				}
				if (saveState.regionLoadStrings[i] != null)
				{
					string[] array = Regex.Split(Regex.Split(saveState.regionLoadStrings[i], "<rgA>")[0], "<rgB>");
					saveGameData.regionsVisited.Add(array[1].ToLowerInvariant());
				}
			}
			saveGameData.unlockedBlues = new List<MultiplayerUnlocks.SandboxUnlockID>();
			foreach (string entry in ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.entries)
			{
				MultiplayerUnlocks.SandboxUnlockID sandboxUnlockID = new MultiplayerUnlocks.SandboxUnlockID(entry);
				if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(sandboxUnlockID))
				{
					saveGameData.unlockedBlues.Add(sandboxUnlockID);
				}
			}
			saveGameData.unlockedGolds = new List<MultiplayerUnlocks.LevelUnlockID>();
			foreach (string entry2 in ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.entries)
			{
				MultiplayerUnlocks.LevelUnlockID levelUnlockID = new MultiplayerUnlocks.LevelUnlockID(entry2);
				if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(levelUnlockID))
				{
					saveGameData.unlockedGolds.Add(levelUnlockID);
				}
			}
			saveGameData.unlockedGreens = new List<MultiplayerUnlocks.SlugcatUnlockID>();
			if (ModManager.MSC)
			{
				foreach (string entry3 in ExtEnum<MultiplayerUnlocks.SlugcatUnlockID>.values.entries)
				{
					MultiplayerUnlocks.SlugcatUnlockID slugcatUnlockID = new MultiplayerUnlocks.SlugcatUnlockID(entry3);
					if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(slugcatUnlockID))
					{
						saveGameData.unlockedGreens.Add(slugcatUnlockID);
					}
				}
			}
			saveGameData.unlockedGreys = new List<ChatlogData.ChatlogID>();
			if (ModManager.MSC)
			{
				foreach (string entry4 in ExtEnum<ChatlogData.ChatlogID>.values.entries)
				{
					ChatlogData.ChatlogID item = new ChatlogData.ChatlogID(entry4);
					if (saveState.deathPersistentSaveData.chatlogsRead.Contains(item))
					{
						saveGameData.unlockedGreys.Add(item);
					}
				}
			}
			saveGameData.unlockedReds = new List<MultiplayerUnlocks.SafariUnlockID>();
			if (ModManager.MSC)
			{
				foreach (string entry5 in ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries)
				{
					MultiplayerUnlocks.SafariUnlockID safariUnlockID = new MultiplayerUnlocks.SafariUnlockID(entry5);
					if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(safariUnlockID))
					{
						saveGameData.unlockedReds.Add(safariUnlockID);
					}
				}
			}
			return saveGameData;
		}
		return null;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Vector2 vector = DrawPos(timeStacker);
		vector.x -= (float)displayRegions.Count * 13f;
		Vector2 vector2 = new Vector2(vector.x, vector.y - 18f);
		for (int i = 0; i < displayRegions.Count; i++)
		{
			for (int j = 0; j < sprites[displayRegions[i]].Count; j++)
			{
				sprites[displayRegions[i]][j].x = vector2.x + (float)i * 13f;
				sprites[displayRegions[i]][j].y = vector2.y - (float)j * 13f;
			}
			regionIcons[i].x = vector.x + (float)i * 13f;
			regionIcons[i].y = vector2.y + 13f;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < displayRegions.Count; i++)
		{
			for (int j = 0; j < sprites[displayRegions[i]].Count; j++)
			{
				sprites[displayRegions[i]][j].RemoveFromContainer();
			}
			regionIcons[i].RemoveFromContainer();
		}
	}
}
