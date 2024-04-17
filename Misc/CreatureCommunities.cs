using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class CreatureCommunities
{
	public class CommunityID : ExtEnum<CommunityID>
	{
		public static readonly CommunityID None = new CommunityID("None", register: true);

		public static readonly CommunityID All = new CommunityID("All", register: true);

		public static readonly CommunityID Scavengers = new CommunityID("Scavengers", register: true);

		public static readonly CommunityID Lizards = new CommunityID("Lizards", register: true);

		public static readonly CommunityID Cicadas = new CommunityID("Cicadas", register: true);

		public static readonly CommunityID GarbageWorms = new CommunityID("GarbageWorms", register: true);

		public static readonly CommunityID Deer = new CommunityID("Deer", register: true);

		public static readonly CommunityID JetFish = new CommunityID("JetFish", register: true);

		public CommunityID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public GameSession session;

	public float scavengerShyness;

	public float[,,] playerOpinions;

	private float[,,] loadedPlayerOpinions;

	public bool locked;

	public CreatureCommunities(GameSession session)
	{
		this.session = session;
		if (session is StoryGameSession)
		{
			playerOpinions = new float[ExtEnum<CommunityID>.values.Count - 1, session.game.rainWorld.progression.regionNames.Length + 1, 1];
		}
		else
		{
			playerOpinions = new float[ExtEnum<CommunityID>.values.Count - 1, 1, 4];
		}
		loadedPlayerOpinions = new float[playerOpinions.GetLength(0), playerOpinions.GetLength(1), playerOpinions.GetLength(2)];
	}

	public void LoadDefaultCommunityAlignments(SlugcatStats.Name saveStateNumber)
	{
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "default alignments.txt"));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 2 || !(array[i].Substring(0, 2) != "//"))
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], " : ");
			for (int j = 0; j < playerOpinions.GetLength(0); j++)
			{
				if (!(new CommunityID(ExtEnum<CommunityID>.values.GetEntry(j + 1)).ToString() == array2[0]))
				{
					continue;
				}
				string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
				for (int k = 0; k < array3.Length; k++)
				{
					int num = -1;
					if (array3[k].Split('=')[0] != "EVERYWHERE")
					{
						for (int l = 0; l < session.game.rainWorld.progression.regionNames.Length; l++)
						{
							if (session.game.rainWorld.progression.regionNames[l] == array3[k].Split('=')[0])
							{
								num = l;
								break;
							}
						}
					}
					int num2 = int.Parse(array3[k].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					playerOpinions[j, num + 1, 0] = (float)num2 / 100f;
					loadedPlayerOpinions[j, num + 1, 0] = (float)num2 / 100f;
				}
			}
		}
		if (saveStateNumber == SlugcatStats.Name.Yellow)
		{
			for (int m = 0; m < playerOpinions.GetLength(0); m++)
			{
				for (int n = 0; n < playerOpinions.GetLength(1); n++)
				{
					for (int num3 = 0; num3 < playerOpinions.GetLength(2); num3++)
					{
						playerOpinions[m, n, num3] = Mathf.Lerp(playerOpinions[m, n, num3], 1f, 0.25f);
					}
				}
			}
		}
		else if (saveStateNumber == SlugcatStats.Name.Red)
		{
			for (int num4 = 0; num4 < playerOpinions.GetLength(0); num4++)
			{
				for (int num5 = 0; num5 < playerOpinions.GetLength(1); num5++)
				{
					for (int num6 = 0; num6 < playerOpinions.GetLength(2); num6++)
					{
						playerOpinions[num4, num5, num6] = Mathf.Lerp(playerOpinions[num4, num5, num6], -1f, 0.35f);
					}
				}
			}
		}
		else if (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			for (int num7 = 0; num7 < playerOpinions.GetLength(1); num7++)
			{
				for (int num8 = 0; num8 < playerOpinions.GetLength(2); num8++)
				{
					playerOpinions[1, num7, num8] = -0.21f;
				}
			}
		}
		else
		{
			if (!ModManager.MSC || (!(saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && !(saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
			{
				return;
			}
			for (int num9 = 0; num9 < playerOpinions.GetLength(0); num9++)
			{
				for (int num10 = 0; num10 < playerOpinions.GetLength(1); num10++)
				{
					for (int num11 = 0; num11 < playerOpinions.GetLength(2); num11++)
					{
						playerOpinions[num9, num10, num11] = Mathf.Lerp(playerOpinions[num9, num10, num11], -1f, 0.45f);
					}
				}
			}
		}
	}

	public float LikeOfPlayer(CommunityID commID, int region, int playerNumber)
	{
		if (commID == CommunityID.None || session == null || commID.Index == -1)
		{
			return 0f;
		}
		if (session is StoryGameSession)
		{
			playerNumber = 0;
			if (ModManager.MSC && commID == CommunityID.Scavengers && (session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return -1f;
			}
		}
		if (session is ArenaGameSession)
		{
			region = -1;
		}
		float num = playerOpinions[commID.Index - 1, region + 1, playerNumber];
		if (region > -1)
		{
			num = Custom.MinusOneToOneRangeFloatInfluence(num, playerOpinions[commID.Index - 1, 0, playerNumber]);
		}
		if (commID != CommunityID.All && commID != CommunityID.Scavengers)
		{
			num = Custom.MinusOneToOneRangeFloatInfluence(num, playerOpinions[0, region + 1, playerNumber]);
		}
		return Custom.ExponentMap(num, -1f, 1f, 1f + 0.2f * session.difficulty);
	}

	public void InfluenceLikeOfPlayer(CommunityID commID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
	{
		if (!locked && !(commID == CommunityID.None) && commID.Index != -1 && session != null)
		{
			if (session is StoryGameSession)
			{
				playerNumber = 0;
			}
			if (session is ArenaGameSession)
			{
				region = -1;
				influence *= 1.5f;
			}
			InfluenceCell(commID.Index - 1, region + 1, playerNumber, influence);
			if (region > -1)
			{
				InfluenceCell(commID.Index - 1, 0, playerNumber, influence * interRegionBleed);
			}
			if (commID != CommunityID.All)
			{
				InfluenceCell(0, region + 1, playerNumber, influence * interCommunityBleed * ((session is StoryGameSession) ? 1f : 0.25f));
			}
			if (region > -1 && commID != CommunityID.All)
			{
				InfluenceCell(0, 0, playerNumber, influence * interRegionBleed * interCommunityBleed);
			}
		}
	}

	private void InfluenceCell(int comm, int reg, int plr, float infl)
	{
		if (!locked)
		{
			if (session is StoryGameSession)
			{
				playerOpinions[comm, reg, plr] = Mathf.Clamp(Mathf.Clamp(playerOpinions[comm, reg, plr] + infl, loadedPlayerOpinions[comm, reg, plr] - 0.5f, loadedPlayerOpinions[comm, reg, plr] + 0.5f), -1f, 1f);
			}
			else
			{
				playerOpinions[comm, reg, plr] = Mathf.Clamp(playerOpinions[comm, reg, plr] + infl, -1f, 1f);
			}
		}
	}

	public void SetLikeOfPlayer(CommunityID commID, int region, int playerNumber, float newLike)
	{
		if (!locked && !(commID == CommunityID.None) && commID.Index != -1 && session != null)
		{
			if (session is StoryGameSession)
			{
				playerNumber = 0;
			}
			if (session is ArenaGameSession)
			{
				region = -1;
			}
			if (region + 1 < playerOpinions.GetLength(1))
			{
				playerOpinions[commID.Index - 1, region + 1, playerNumber] = Mathf.Clamp(newLike, -1f, 1f);
				loadedPlayerOpinions[commID.Index - 1, region + 1, playerNumber] = Mathf.Clamp(newLike, -1f, 1f);
			}
		}
	}

	public override string ToString()
	{
		string text = "";
		text += string.Format(CultureInfo.InvariantCulture, "SCAVSHY<coB>{0}<coA>", scavengerShyness);
		for (int i = 0; i < playerOpinions.GetLength(0); i++)
		{
			text = text + new CommunityID(ExtEnum<CommunityID>.values.GetEntry(i + 1))?.ToString() + "<coB>";
			for (int j = 0; j < playerOpinions.GetLength(1); j++)
			{
				string text2 = "EVERY";
				if (session is StoryGameSession && j > 0)
				{
					text2 = session.game.overWorld.regions[j - 1].name;
				}
				text = text + string.Format(CultureInfo.InvariantCulture, "{0}", text2 + ":" + playerOpinions[i, j, 0]) + ((j < playerOpinions.GetLength(1) - 1) ? "|" : "");
			}
			if (i < playerOpinions.GetLength(0) - 1)
			{
				text += "<coA>";
			}
		}
		return text;
	}

	public void FromString(string s)
	{
		if (!s.Contains("<ccA>") && s.Contains("<coA>"))
		{
			Region[] array = null;
			if (session is StoryGameSession)
			{
				array = ((session.game.overWorld == null) ? Region.LoadAllRegions(session.game.StoryCharacter) : session.game.overWorld.regions);
			}
			string[] array2 = Regex.Split(s, "<coA>");
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] == string.Empty)
				{
					continue;
				}
				string text = Regex.Split(array2[i], "<coB>")[0];
				if (text != null && text == "SCAVSHY")
				{
					scavengerShyness = float.Parse(Regex.Split(array2[i], "<coB>")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					if (text.Length <= 0)
					{
						continue;
					}
					try
					{
						CommunityID commID = new CommunityID(text);
						string[] array3 = Regex.Split(array2[i], "<coB>")[1].Split('|');
						for (int j = 0; j < array3.Length; j++)
						{
							string[] array4 = array3[j].Split(':');
							string text2 = array4[0];
							float newLike = float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
							int num = -1;
							if (session is StoryGameSession)
							{
								for (int k = 0; k < array.Length; k++)
								{
									if (text2 == array[k].name)
									{
										num = k;
										break;
									}
								}
							}
							if (num >= 0 || text2 == "EVERY")
							{
								SetLikeOfPlayer(commID, num, 0, newLike);
							}
						}
					}
					catch (Exception arg)
					{
						Custom.LogWarning($"tried to load invalid creature community: {array2[i]} :: {arg}");
					}
				}
			}
			return;
		}
		string[] array5 = Regex.Split(s, "<ccA>");
		for (int l = 0; l < array5.Length; l++)
		{
			string text3 = Regex.Split(array5[l], "<ccB>")[0];
			if (text3 != null && text3 == "SCAVSHY")
			{
				scavengerShyness = float.Parse(Regex.Split(array5[l], "<ccB>")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			else
			{
				if (Regex.Split(array5[l], "<ccB>")[0].Length <= 0)
				{
					continue;
				}
				try
				{
					CommunityID commID2 = new CommunityID(Regex.Split(array5[l], "<ccB>")[0]);
					string[] array6 = Regex.Split(Regex.Split(array5[l], "<ccB>")[1], ",");
					if (NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == "," && array6.Length > playerOpinions.GetLength(1))
					{
						List<string> list = new List<string>();
						bool flag = true;
						string text4 = "";
						for (int m = 0; m < array6.Length; m++)
						{
							if (flag)
							{
								text4 += array6[m];
								flag = false;
							}
							else if (array6[m].Length > 0 && array6[m][0] == '-')
							{
								list.Add(text4);
								m--;
								flag = true;
							}
							else if (array6[m].Length > 1)
							{
								text4 = text4 + "." + array6[m];
								list.Add(text4);
								flag = true;
							}
							else if (int.Parse(array6[m], NumberStyles.Any, CultureInfo.InvariantCulture) > 1)
							{
								text4 = text4 + "." + array6[m];
								list.Add(text4);
								flag = true;
							}
							else
							{
								list.Add(text4);
								m--;
								flag = true;
							}
						}
						array6 = list.ToArray();
					}
					for (int n = 0; n < array6.Length && n < playerOpinions.GetLength(1); n++)
					{
						SetLikeOfPlayer(commID2, n - 1, 0, float.Parse(array6[n], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
				}
				catch (Exception arg2)
				{
					Custom.LogWarning($"tried to load invalid creature community: {arg2}");
				}
			}
		}
	}

	public void CycleTick(int cycle, SlugcatStats.Name saveStateNumber)
	{
		if (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			if (session.game.IsStorySession && session.game.world.name == "LC")
			{
				scavengerShyness = 0f;
			}
			else if (session.game.IsStorySession && session.game.world.region.name == "HI")
			{
				scavengerShyness = Mathf.InverseLerp(4f, 0f, cycle);
			}
			else if (session.game.IsStorySession && (session.game.world.region.name == "UW" || session.game.world.region.name == "SH"))
			{
				scavengerShyness = Mathf.InverseLerp(9f, 0f, cycle);
			}
			else if (session.game.IsStorySession && session.game.world.region.name == "GW")
			{
				scavengerShyness = Mathf.InverseLerp(5f, -20f, cycle);
			}
			else
			{
				scavengerShyness = Mathf.Max(0f, scavengerShyness - 0.07f);
			}
		}
		else if (cycle > 5)
		{
			scavengerShyness = Mathf.Max(0f, scavengerShyness - 0.07f);
		}
		if (saveStateNumber == SlugcatStats.Name.Yellow)
		{
			for (int i = 0; i < playerOpinions.GetLength(0); i++)
			{
				for (int j = 0; j < playerOpinions.GetLength(1); j++)
				{
					for (int k = 0; k < playerOpinions.GetLength(2); k++)
					{
						if (playerOpinions[i, j, k] < 0.25f)
						{
							playerOpinions[i, j, k] = Mathf.Min(0.25f, playerOpinions[i, j, k] + 0.08f);
						}
					}
				}
			}
			return;
		}
		for (int l = 0; l < playerOpinions.GetLength(0); l++)
		{
			for (int m = 0; m < playerOpinions.GetLength(1); m++)
			{
				for (int n = 0; n < playerOpinions.GetLength(2); n++)
				{
					if (playerOpinions[l, m, n] < 0f)
					{
						playerOpinions[l, m, n] = Mathf.Min(0f, playerOpinions[l, m, n] + Custom.LerpMap(playerOpinions[l, m, n], -1f, 0f, 0.01f, 0.1f));
					}
				}
			}
		}
	}
}
