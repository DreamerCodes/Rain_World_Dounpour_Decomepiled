using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Expedition;
using Menu;
using Modding.Passages;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class WinState
{
	public class EndgameID : ExtEnum<EndgameID>
	{
		public static readonly EndgameID Survivor = new EndgameID("Survivor", register: true);

		public static readonly EndgameID Hunter = new EndgameID("Hunter", register: true);

		public static readonly EndgameID Saint = new EndgameID("Saint", register: true);

		public static readonly EndgameID Traveller = new EndgameID("Traveller", register: true);

		public static readonly EndgameID Chieftain = new EndgameID("Chieftain", register: true);

		public static readonly EndgameID Monk = new EndgameID("Monk", register: true);

		public static readonly EndgameID Outlaw = new EndgameID("Outlaw", register: true);

		public static readonly EndgameID DragonSlayer = new EndgameID("DragonSlayer", register: true);

		public static readonly EndgameID Scholar = new EndgameID("Scholar", register: true);

		public static readonly EndgameID Friend = new EndgameID("Friend", register: true);

		public EndgameID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class EndgameTracker
	{
		public EndgameID ID;

		public bool consumed;

		public virtual bool AnyProgressToSave => false;

		public virtual bool AnyProgressToShow => false;

		public virtual bool GoalFullfilled => false;

		public virtual bool GoalAlreadyFullfilled => false;

		public EndgameTracker(EndgameID ID)
		{
			this.ID = ID;
		}

		public override string ToString()
		{
			return ID.ToString() + "<egA>" + (consumed ? "1" : "0");
		}

		public virtual void FromString(string[] splt)
		{
			if (splt[0] != ID.ToString())
			{
				Custom.LogWarning("Problem with loading EndgameTracker! Inconsistent ID");
			}
			consumed = splt[1] == "1";
		}
	}

	public class IntegerTracker : EndgameTracker
	{
		public int dflt;

		public int min;

		public int max;

		public int showFrom;

		public int progress;

		public int lastShownProgress;

		public override bool AnyProgressToSave
		{
			get
			{
				if (!GoalFullfilled && progress == dflt)
				{
					return lastShownProgress != dflt;
				}
				return true;
			}
		}

		public override bool AnyProgressToShow
		{
			get
			{
				if (progress <= showFrom)
				{
					return lastShownProgress > showFrom;
				}
				return true;
			}
		}

		public override bool GoalFullfilled => progress >= max;

		public override bool GoalAlreadyFullfilled => lastShownProgress >= max;

		public void SetProgress(int v)
		{
			progress = Custom.IntClamp(v, min, max);
		}

		public IntegerTracker(EndgameID ID, int dflt, int min, int showFrom, int max)
			: base(ID)
		{
			this.dflt = dflt;
			progress = dflt;
			lastShownProgress = dflt;
			this.min = min;
			this.showFrom = showFrom;
			this.max = max;
		}

		public override string ToString()
		{
			return base.ToString() + string.Format(CultureInfo.InvariantCulture, "<egA>{0}", progress);
		}

		public override void FromString(string[] splt)
		{
			base.FromString(splt);
			if (splt.Length > 2)
			{
				progress = int.Parse(splt[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				lastShownProgress = progress;
			}
		}
	}

	public class FloatTracker : EndgameTracker
	{
		public float dflt;

		public float min;

		public float showFrom;

		public float max;

		public float progress;

		public float lastShownProgress;

		public override bool AnyProgressToSave
		{
			get
			{
				if (!GoalFullfilled && progress == dflt)
				{
					return lastShownProgress != dflt;
				}
				return true;
			}
		}

		public override bool AnyProgressToShow
		{
			get
			{
				if (!(progress > showFrom))
				{
					return lastShownProgress > showFrom;
				}
				return true;
			}
		}

		public override bool GoalFullfilled => progress >= max;

		public override bool GoalAlreadyFullfilled => lastShownProgress >= max;

		public void SetProgress(float v)
		{
			progress = Mathf.Clamp(v, min, max);
		}

		public FloatTracker(EndgameID ID, float dflt, float min, float showFrom, float max)
			: base(ID)
		{
			this.dflt = dflt;
			progress = dflt;
			lastShownProgress = dflt;
			this.min = min;
			this.showFrom = showFrom;
			this.max = max;
		}

		public override string ToString()
		{
			return base.ToString() + string.Format(CultureInfo.InvariantCulture, "<egA>{0}", progress);
		}

		public override void FromString(string[] splt)
		{
			base.FromString(splt);
			if (splt.Length > 2)
			{
				progress = float.Parse(splt[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				lastShownProgress = progress;
			}
		}
	}

	public class BoolArrayTracker : EndgameTracker
	{
		public bool[] progress;

		public bool[] lastShownProgress;

		public override bool AnyProgressToSave
		{
			get
			{
				for (int i = 0; i < progress.Length; i++)
				{
					if (progress[i])
					{
						return true;
					}
				}
				for (int j = 0; j < lastShownProgress.Length; j++)
				{
					if (lastShownProgress[j])
					{
						return true;
					}
				}
				return false;
			}
		}

		public override bool AnyProgressToShow => AnyProgressToSave;

		public override bool GoalFullfilled
		{
			get
			{
				for (int i = 0; i < progress.Length; i++)
				{
					if (!progress[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		public override bool GoalAlreadyFullfilled
		{
			get
			{
				for (int i = 0; i < lastShownProgress.Length; i++)
				{
					if (!lastShownProgress[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		public BoolArrayTracker(EndgameID ID, int slots)
			: base(ID)
		{
			progress = new bool[slots];
			lastShownProgress = new bool[slots];
		}

		public override string ToString()
		{
			string text = base.ToString() + "<egA>";
			for (int i = 0; i < progress.Length; i++)
			{
				text = text + (progress[i] ? "1" : "0") + ".";
			}
			return text;
		}

		public override void FromString(string[] splt)
		{
			base.FromString(splt);
			if (splt.Length > 2)
			{
				string[] array = splt[2].Split('.');
				for (int i = 0; i < array.Length && i < progress.Length; i++)
				{
					progress[i] = array[i] == "1";
					lastShownProgress[i] = array[i] == "1";
				}
			}
		}
	}

	public class ListTracker : EndgameTracker
	{
		public List<int> myList;

		public List<int> myLastList;

		public int totItemsToWin;

		public override bool AnyProgressToSave => myList.Count > 0;

		public override bool AnyProgressToShow
		{
			get
			{
				if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Nomad)
				{
					if (!AnyProgressToSave)
					{
						return myList.Count < myLastList.Count;
					}
					return true;
				}
				return AnyProgressToSave;
			}
		}

		public override bool GoalFullfilled => myList.Count >= totItemsToWin;

		public override bool GoalAlreadyFullfilled => myLastList.Count >= totItemsToWin;

		public ListTracker(EndgameID ID, int totItemsToWin)
			: base(ID)
		{
			myList = new List<int>();
			myLastList = new List<int>();
			this.totItemsToWin = totItemsToWin;
		}

		public void AddItemToList(int item)
		{
			if (myList.Count >= totItemsToWin)
			{
				return;
			}
			for (int i = 0; i < myList.Count; i++)
			{
				if (item == myList[i])
				{
					return;
				}
			}
			myList.Add(item);
		}

		public override string ToString()
		{
			string text = base.ToString() + "<egA>";
			for (int i = 0; i < myList.Count; i++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", myList[i], (i == myList.Count - 1) ? "" : ".");
			}
			return text;
		}

		public override void FromString(string[] splt)
		{
			base.FromString(splt);
			if (splt.Length <= 2)
			{
				return;
			}
			string[] array = splt[2].Split('.');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != "")
				{
					myList.Add(int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture));
					myLastList.Add(int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
			}
		}
	}

	public struct GourmandTrackerData
	{
		public AbstractPhysicalObject.AbstractObjectType type;

		public CreatureTemplate.Type[] crits;

		public GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType type, CreatureTemplate.Type[] crits)
		{
			if (crits != null)
			{
				this.type = AbstractPhysicalObject.AbstractObjectType.Creature;
				this.crits = crits;
			}
			else
			{
				this.type = type;
				this.crits = null;
			}
		}
	}

	public class GourFeastTracker : EndgameTracker
	{
		public int[] currentCycleProgress;

		public int[] progress;

		public int[] lastShownProgress;

		public override bool AnyProgressToSave
		{
			get
			{
				for (int i = 0; i < progress.Length; i++)
				{
					if (progress[i] > 0)
					{
						return true;
					}
				}
				for (int j = 0; j < lastShownProgress.Length; j++)
				{
					if (lastShownProgress[j] > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override bool AnyProgressToShow => AnyProgressToSave;

		public override bool GoalFullfilled
		{
			get
			{
				for (int i = 0; i < progress.Length; i++)
				{
					if (progress[i] <= 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override bool GoalAlreadyFullfilled
		{
			get
			{
				for (int i = 0; i < lastShownProgress.Length; i++)
				{
					if (lastShownProgress[i] <= 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		public GourFeastTracker(EndgameID ID, int slots)
			: base(ID)
		{
			currentCycleProgress = new int[slots];
			progress = new int[slots];
			lastShownProgress = new int[slots];
		}

		public override string ToString()
		{
			string text = base.ToString() + "<egA>";
			for (int i = 0; i < progress.Length; i++)
			{
				text = text + progress[i] + ".";
			}
			return text;
		}

		public override void FromString(string[] splt)
		{
			base.FromString(splt);
			if (splt.Length > 2)
			{
				string[] array = splt[2].Split('.');
				for (int i = 0; i < array.Length && i < progress.Length; i++)
				{
					int num = int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
					progress[i] = num;
					lastShownProgress[i] = num;
					currentCycleProgress[i] = num;
				}
			}
		}
	}

	public List<EndgameTracker> endgameTrackers;

	public List<string> unrecognizedTrackers;

	public static CreatureTemplate.Type[] lizardsOrder = new CreatureTemplate.Type[6]
	{
		CreatureTemplate.Type.GreenLizard,
		CreatureTemplate.Type.PinkLizard,
		CreatureTemplate.Type.BlueLizard,
		CreatureTemplate.Type.WhiteLizard,
		CreatureTemplate.Type.YellowLizard,
		CreatureTemplate.Type.BlackLizard
	};

	public static GourmandTrackerData[] GourmandPassageTracker;

	public static string PassageDisplayName(EndgameID ID)
	{
		if (ID == EndgameID.Survivor)
		{
			return "The Survivor";
		}
		if (ID == EndgameID.Hunter)
		{
			return "The Hunter";
		}
		if (ID == EndgameID.Saint)
		{
			return "The Saint";
		}
		if (ID == EndgameID.Traveller)
		{
			return "The Wanderer";
		}
		if (ID == EndgameID.Chieftain)
		{
			return "The Chieftain";
		}
		if (ID == EndgameID.Monk)
		{
			return "The Monk";
		}
		if (ID == EndgameID.Outlaw)
		{
			return "The Outlaw";
		}
		if (ID == EndgameID.DragonSlayer)
		{
			return "The Dragon Slayer";
		}
		if (ID == EndgameID.Scholar)
		{
			return "The Scholar";
		}
		if (ID == EndgameID.Friend)
		{
			return "The Friend";
		}
		if (ModManager.MSC)
		{
			if (ID == MoreSlugcatsEnums.EndgameID.Nomad)
			{
				return "The Nomad";
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Martyr)
			{
				return "The Martyr";
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Pilgrim)
			{
				return "The Pilgrim";
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Mother)
			{
				return "The Mother";
			}
		}
		return CustomPassages.PassageForID(ID)?.DisplayName ?? "";
	}

	public static RainWorld.AchievementID PassageAchievementID(EndgameID ID)
	{
		if (ID == EndgameID.Survivor)
		{
			return RainWorld.AchievementID.PassageSurvivor;
		}
		if (ID == EndgameID.Hunter)
		{
			return RainWorld.AchievementID.PassageHunter;
		}
		if (ID == EndgameID.Saint)
		{
			return RainWorld.AchievementID.PassageSaint;
		}
		if (ID == EndgameID.Traveller)
		{
			return RainWorld.AchievementID.PassageTraveller;
		}
		if (ID == EndgameID.Chieftain)
		{
			return RainWorld.AchievementID.PassageChieftain;
		}
		if (ID == EndgameID.Monk)
		{
			return RainWorld.AchievementID.PassageMonk;
		}
		if (ID == EndgameID.Outlaw)
		{
			return RainWorld.AchievementID.PassageOutlaw;
		}
		if (ID == EndgameID.DragonSlayer)
		{
			return RainWorld.AchievementID.PassageDragonSlayer;
		}
		if (ID == EndgameID.Scholar)
		{
			return RainWorld.AchievementID.PassageScholar;
		}
		if (ID == EndgameID.Friend)
		{
			return RainWorld.AchievementID.PassageFriend;
		}
		if (ModManager.MSC)
		{
			if (ID == MoreSlugcatsEnums.EndgameID.Martyr)
			{
				return RainWorld.AchievementID.PassageMartyr;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Mother)
			{
				return RainWorld.AchievementID.PassageMother;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Nomad)
			{
				return RainWorld.AchievementID.PassageNomad;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Pilgrim)
			{
				return RainWorld.AchievementID.PassagePilgrim;
			}
		}
		return RainWorld.AchievementID.None;
	}

	public WinState()
	{
		endgameTrackers = new List<EndgameTracker>();
		unrecognizedTrackers = new List<string>();
		if (ModManager.MSC)
		{
			CreateAndAddTracker(MoreSlugcatsEnums.EndgameID.Gourmand, endgameTrackers);
		}
	}

	public EndgameID GetNextEndGame()
	{
		for (int i = 0; i < endgameTrackers.Count; i++)
		{
			if (endgameTrackers[i].GoalFullfilled && !endgameTrackers[i].consumed && (!ModManager.MSC || endgameTrackers[i].ID != MoreSlugcatsEnums.EndgameID.Gourmand))
			{
				return endgameTrackers[i].ID;
			}
		}
		return null;
	}

	public void ConsumeEndGame()
	{
		for (int i = 0; i < endgameTrackers.Count; i++)
		{
			if (endgameTrackers[i].GoalFullfilled && !endgameTrackers[i].consumed)
			{
				endgameTrackers[i].consumed = true;
				break;
			}
		}
	}

	public void ResetLastShownValues()
	{
		Custom.Log("~~Resetting last shown values of win state");
		for (int i = 0; i < endgameTrackers.Count; i++)
		{
			if (endgameTrackers[i] is FloatTracker)
			{
				(endgameTrackers[i] as FloatTracker).lastShownProgress = (endgameTrackers[i] as FloatTracker).progress;
			}
			else if (endgameTrackers[i] is IntegerTracker)
			{
				(endgameTrackers[i] as IntegerTracker).lastShownProgress = (endgameTrackers[i] as IntegerTracker).progress;
			}
			else if (endgameTrackers[i] is BoolArrayTracker)
			{
				for (int j = 0; j < (endgameTrackers[i] as BoolArrayTracker).progress.Length && j < (endgameTrackers[i] as BoolArrayTracker).lastShownProgress.Length; j++)
				{
					(endgameTrackers[i] as BoolArrayTracker).lastShownProgress[j] = (endgameTrackers[i] as BoolArrayTracker).progress[j];
				}
			}
			else if (ModManager.MSC && endgameTrackers[i] is GourFeastTracker)
			{
				for (int k = 0; k < (endgameTrackers[i] as GourFeastTracker).progress.Length && k < (endgameTrackers[i] as GourFeastTracker).lastShownProgress.Length; k++)
				{
					(endgameTrackers[i] as GourFeastTracker).lastShownProgress[k] = (endgameTrackers[i] as GourFeastTracker).progress[k];
					(endgameTrackers[i] as GourFeastTracker).currentCycleProgress[k] = (endgameTrackers[i] as GourFeastTracker).progress[k];
				}
			}
		}
	}

	public string SaveToString(bool saveAsIfPlayerDied)
	{
		string text = "";
		Custom.Log("WIN STATE SAVED (as death:", saveAsIfPlayerDied.ToString(), ")");
		if (saveAsIfPlayerDied)
		{
			float progress = 0f;
			int progress2 = 0;
			bool[] array = null;
			for (int i = 0; i < endgameTrackers.Count; i++)
			{
				if (endgameTrackers[i] is FloatTracker)
				{
					progress = (endgameTrackers[i] as FloatTracker).progress;
				}
				else if (endgameTrackers[i] is IntegerTracker)
				{
					progress2 = (endgameTrackers[i] as IntegerTracker).progress;
				}
				else if (endgameTrackers[i] is BoolArrayTracker)
				{
					array = new bool[(endgameTrackers[i] as BoolArrayTracker).progress.Length];
					for (int j = 0; j < (endgameTrackers[i] as BoolArrayTracker).progress.Length; j++)
					{
						array[j] = (endgameTrackers[i] as BoolArrayTracker).progress[j];
					}
				}
				if (!endgameTrackers[i].GoalAlreadyFullfilled)
				{
					DeathModifyTracker(endgameTrackers[i]);
				}
				if (endgameTrackers[i].AnyProgressToSave)
				{
					text = text + endgameTrackers[i].ToString() + "<wsA>";
				}
				if (endgameTrackers[i] is FloatTracker)
				{
					(endgameTrackers[i] as FloatTracker).progress = progress;
				}
				else if (endgameTrackers[i] is IntegerTracker)
				{
					(endgameTrackers[i] as IntegerTracker).progress = progress2;
				}
				else if (endgameTrackers[i] is BoolArrayTracker)
				{
					for (int k = 0; k < (endgameTrackers[i] as BoolArrayTracker).progress.Length; k++)
					{
						(endgameTrackers[i] as BoolArrayTracker).progress[k] = array[k];
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < endgameTrackers.Count; l++)
			{
				if (endgameTrackers[l].AnyProgressToSave)
				{
					text = text + endgameTrackers[l].ToString() + "<wsA>";
				}
			}
		}
		foreach (string unrecognizedTracker in unrecognizedTrackers)
		{
			text = text + unrecognizedTracker + "<wsA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		string[] array = Regex.Split(s, "<wsA>");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 0)
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], "<egA>");
			if (array2[0].Length <= 0)
			{
				continue;
			}
			try
			{
				EndgameID endgameID = new EndgameID(array2[0]);
				if (endgameID.Index == -1)
				{
					unrecognizedTrackers.Add(array[i]);
				}
				else
				{
					LoadTracker(endgameID, array2);
				}
			}
			catch
			{
				unrecognizedTrackers.Add(array[i]);
			}
		}
	}

	private void LoadTracker(EndgameID ID, string[] saveString)
	{
		CreateAndAddTracker(ID, endgameTrackers)?.FromString(saveString);
	}

	public static EndgameTracker CreateAndAddTracker(EndgameID ID, List<EndgameTracker> endgameTrackers)
	{
		EndgameTracker endgameTracker = null;
		if (ID == EndgameID.Survivor)
		{
			endgameTracker = new IntegerTracker(ID, 0, 0, 0, 5);
		}
		else if (ID == EndgameID.Hunter)
		{
			endgameTracker = new IntegerTracker(ID, 0, 0, 2, 12);
		}
		else if (ID == EndgameID.Saint)
		{
			endgameTracker = new IntegerTracker(ID, 0, 0, 2, 12);
		}
		else if (ID == EndgameID.Traveller)
		{
			endgameTracker = new BoolArrayTracker(ID, SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot).Count);
		}
		else if (ID == EndgameID.Chieftain)
		{
			endgameTracker = new FloatTracker(ID, 0f, 0f, 0f, 1f);
		}
		else if (ID == EndgameID.Monk)
		{
			endgameTracker = new IntegerTracker(ID, 0, 0, 2, 12);
		}
		else if (ID == EndgameID.Outlaw)
		{
			endgameTracker = new IntegerTracker(ID, 0, 0, 2, 7);
		}
		else if (ID == EndgameID.DragonSlayer)
		{
			endgameTracker = ((!ModManager.MSC) ? ((EndgameTracker)new BoolArrayTracker(ID, 6)) : ((EndgameTracker)new ListTracker(ID, 6)));
		}
		else if (ID == EndgameID.Scholar)
		{
			endgameTracker = new ListTracker(EndgameID.Scholar, 3);
		}
		else if (ID == EndgameID.Friend)
		{
			endgameTracker = new FloatTracker(ID, 0f, 0f, 0f, 1f);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Gourmand && GourmandPassageTracker != null)
		{
			endgameTracker = new GourFeastTracker(ID, GourmandPassageTracker.Length);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Nomad)
		{
			endgameTracker = new ListTracker(ID, 4);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Martyr)
		{
			endgameTracker = new FloatTracker(ID, 0f, 0f, 0.05f, 1f);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Pilgrim)
		{
			int num = 0;
			List<string> list = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != "MS" && World.CheckForRegionGhost(RainWorld.lastActiveSaveSlot, list[i]))
				{
					num++;
				}
			}
			endgameTracker = new BoolArrayTracker(ID, num);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.EndgameID.Mother)
		{
			endgameTracker = new FloatTracker(ID, 0f, 0f, 0f, 1f);
		}
		if (endgameTracker == null)
		{
			endgameTracker = CustomPassages.PassageForID(ID)?.CreateTracker();
		}
		if (endgameTracker != null && endgameTrackers != null)
		{
			bool flag = false;
			for (int j = 0; j < endgameTrackers.Count; j++)
			{
				if (endgameTrackers[j].ID == ID)
				{
					flag = true;
					endgameTrackers[j] = endgameTracker;
					break;
				}
			}
			if (!flag)
			{
				endgameTrackers.Add(endgameTracker);
			}
		}
		return endgameTracker;
	}

	public EndgameTracker GetTracker(EndgameID ID, bool addIfMissing)
	{
		EndgameTracker endgameTracker = null;
		int num = 0;
		while (endgameTracker == null && num < endgameTrackers.Count)
		{
			if (endgameTrackers[num].ID == ID)
			{
				endgameTracker = endgameTrackers[num];
			}
			num++;
		}
		if (endgameTracker == null && addIfMissing)
		{
			endgameTracker = CreateAndAddTracker(ID, endgameTrackers);
		}
		return endgameTracker;
	}

	private void DeathModifyTracker(EndgameTracker tracker)
	{
		if (tracker.ID == EndgameID.Hunter || tracker.ID == EndgameID.Saint || tracker.ID == EndgameID.Monk || tracker.ID == EndgameID.Outlaw)
		{
			(tracker as IntegerTracker).progress--;
		}
		else if (ModManager.MSC)
		{
			if (tracker.ID == MoreSlugcatsEnums.EndgameID.Gourmand)
			{
				for (int i = 0; i < (tracker as GourFeastTracker).progress.Length; i++)
				{
					(tracker as GourFeastTracker).progress[i] = (tracker as GourFeastTracker).lastShownProgress[i];
					(tracker as GourFeastTracker).currentCycleProgress[i] = (tracker as GourFeastTracker).progress[i];
				}
			}
			if (tracker.ID == MoreSlugcatsEnums.EndgameID.Nomad)
			{
				(tracker as ListTracker).myList = new List<int>((tracker as ListTracker).myLastList);
			}
			if (tracker.ID == MoreSlugcatsEnums.EndgameID.Martyr)
			{
				(tracker as FloatTracker).progress *= 0.5f;
			}
		}
		CustomPassage customPassage = CustomPassages.PassageForID(tracker.ID);
		if (customPassage != null && customPassage.IsAvailableForSlugcat(Custom.rainWorld.progression.currentSaveState.saveStateNumber) && customPassage.RequirementsMet(endgameTrackers))
		{
			customPassage.OnDeath(this, tracker);
		}
	}

	public void CycleCompleted(RainWorldGame game)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		int num2 = 0;
		bool flag5 = true;
		int num3 = -1;
		float value = game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, 0);
		value = ((!(game.StoryCharacter == SlugcatStats.Name.Yellow)) ? Mathf.InverseLerp(0.1f, 0.8f, value) : Mathf.InverseLerp(0.42f, 0.9f, value));
		value = Mathf.Floor(value * 20f) / 20f;
		List<DataPearl.AbstractDataPearl.DataPearlType> list = new List<DataPearl.AbstractDataPearl.DataPearlType>();
		float num4 = -1f;
		List<int> list2 = new List<int>();
		for (int i = 0; i < game.GetStorySession.playerSessionRecords.Length; i++)
		{
			if (game.GetStorySession.playerSessionRecords[i] == null)
			{
				continue;
			}
			PlayerSessionRecord playerSessionRecord = game.GetStorySession.playerSessionRecords[i];
			if (playerSessionRecord.ateAnything)
			{
				flag = true;
			}
			if (!playerSessionRecord.vegetarian)
			{
				flag2 = false;
			}
			if (!playerSessionRecord.carnivorous)
			{
				flag3 = false;
			}
			if (!playerSessionRecord.peaceful)
			{
				flag4 = false;
			}
			for (int j = 0; j < playerSessionRecord.kills.Count; j++)
			{
				if (StaticWorld.GetCreatureTemplate(playerSessionRecord.kills[j].symbolData.critType).countsAsAKill > 0)
				{
					flag5 = false;
				}
				if (StaticWorld.GetCreatureTemplate(playerSessionRecord.kills[j].symbolData.critType).countsAsAKill > 1)
				{
					num2++;
				}
				if (!playerSessionRecord.kills[j].lizard)
				{
					continue;
				}
				for (int k = 0; k < lizardsOrder.Length; k++)
				{
					if (playerSessionRecord.kills[j].symbolData.critType == lizardsOrder[k])
					{
						if (!list2.Contains(k))
						{
							list2.Add(k);
						}
						break;
					}
				}
			}
			num3 = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot).IndexOf(playerSessionRecord.wentToSleepInRegion);
			if (num3 != -1)
			{
				game.rainWorld.progression.miscProgressionData.menuRegion = playerSessionRecord.wentToSleepInRegion;
			}
			for (int l = 0; l < playerSessionRecord.pearlsFound.Count; l++)
			{
				if (!list.Contains(playerSessionRecord.pearlsFound[l]))
				{
					list.Add(playerSessionRecord.pearlsFound[l]);
				}
			}
			if (playerSessionRecord.friendInDen != null)
			{
				num4 = playerSessionRecord.friendInDen.state.socialMemory.GetLike(game.Players[i].ID);
			}
			num = playerSessionRecord.pupCountInDen;
		}
		if (ModManager.MSC)
		{
			UpdateGhostTracker(game.GetStorySession.saveState, GetTracker(MoreSlugcatsEnums.EndgameID.Pilgrim, addIfMissing: true) as BoolArrayTracker);
		}
		IntegerTracker integerTracker = GetTracker(EndgameID.Survivor, addIfMissing: true) as IntegerTracker;
		if ((game.session as StoryGameSession).saveState.deathPersistentSaveData.karma >= (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap - 1 && (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap >= 4)
		{
			integerTracker.SetProgress(integerTracker.progress + 1);
		}
		if (Custom.rainWorld.ExpeditionMode && !integerTracker.GoalFullfilled)
		{
			integerTracker.progress = integerTracker.max;
			integerTracker.lastShownProgress = integerTracker.max;
			integerTracker.consumed = true;
		}
		if (ModManager.MSC)
		{
			FloatTracker floatTracker = GetTracker(MoreSlugcatsEnums.EndgameID.Martyr, addIfMissing: true) as FloatTracker;
			if (game.GetStorySession.saveState.malnourished)
			{
				floatTracker.SetProgress(floatTracker.progress + 0.27f);
			}
			else if (!floatTracker.GoalAlreadyFullfilled)
			{
				floatTracker.SetProgress(floatTracker.progress - 0.02f);
			}
			if (GetTracker(MoreSlugcatsEnums.EndgameID.Mother, num > 0) is FloatTracker floatTracker2)
			{
				if (num == 0 && !floatTracker2.GoalAlreadyFullfilled)
				{
					floatTracker2.SetProgress(0f);
				}
				else
				{
					floatTracker2.SetProgress(floatTracker2.progress + (float)num * 0.2f);
				}
			}
			if (GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, addIfMissing: false) is GourFeastTracker gourFeastTracker)
			{
				for (int m = 0; m < gourFeastTracker.progress.Length; m++)
				{
					gourFeastTracker.progress[m] = gourFeastTracker.currentCycleProgress[m];
				}
			}
		}
		if ((!ModManager.MMF || !MMF.cfgSurvivorPassageNotRequired.Value) && !integerTracker.GoalAlreadyFullfilled)
		{
			return;
		}
		if (GetTracker(EndgameID.Hunter, flag3) is IntegerTracker integerTracker2 && integerTracker.GoalAlreadyFullfilled)
		{
			if (flag3)
			{
				if (flag)
				{
					integerTracker2.SetProgress(integerTracker2.progress + 2);
				}
			}
			else if (integerTracker2 != null && !integerTracker2.GoalAlreadyFullfilled)
			{
				integerTracker2.SetProgress(0);
			}
		}
		if (GetTracker(EndgameID.Saint, flag4) is IntegerTracker integerTracker3 && integerTracker.GoalAlreadyFullfilled)
		{
			if (flag4)
			{
				integerTracker3.SetProgress(integerTracker3.progress + 2);
			}
			else if (integerTracker3 != null && !integerTracker3.GoalAlreadyFullfilled)
			{
				integerTracker3.SetProgress(0);
			}
		}
		if (num3 >= 0)
		{
			BoolArrayTracker boolArrayTracker = GetTracker(EndgameID.Traveller, addIfMissing: true) as BoolArrayTracker;
			if (num3 < boolArrayTracker.progress.Length)
			{
				boolArrayTracker.progress[num3] = true;
			}
		}
		if (GetTracker(EndgameID.Chieftain, value > 0f) is FloatTracker floatTracker3 && integerTracker.GoalAlreadyFullfilled)
		{
			floatTracker3.SetProgress(value);
		}
		if (GetTracker(EndgameID.Monk, flag2) is IntegerTracker integerTracker4 && integerTracker.GoalAlreadyFullfilled)
		{
			if (flag2)
			{
				if (flag)
				{
					integerTracker4.SetProgress(integerTracker4.progress + 2);
				}
			}
			else if (integerTracker4 != null && !integerTracker4.GoalAlreadyFullfilled)
			{
				integerTracker4.SetProgress(0);
			}
		}
		if (GetTracker(EndgameID.Outlaw, num2 > 0) is IntegerTracker integerTracker5 && integerTracker.GoalAlreadyFullfilled)
		{
			if (num2 > 0)
			{
				integerTracker5.SetProgress(integerTracker5.progress + num2);
			}
			else if (integerTracker5 != null && !integerTracker5.GoalAlreadyFullfilled && flag5)
			{
				integerTracker5.SetProgress(0);
			}
		}
		if (list2.Count > 0)
		{
			if (ModManager.MSC)
			{
				ListTracker listTracker = GetTracker(EndgameID.DragonSlayer, addIfMissing: true) as ListTracker;
				foreach (int item2 in list2)
				{
					listTracker.AddItemToList(item2);
				}
			}
			else
			{
				BoolArrayTracker boolArrayTracker2 = GetTracker(EndgameID.DragonSlayer, addIfMissing: true) as BoolArrayTracker;
				foreach (int item3 in list2)
				{
					if (item3 < boolArrayTracker2.progress.Length)
					{
						boolArrayTracker2.progress[item3] = true;
					}
				}
			}
		}
		if (list.Count > 0 && integerTracker.GoalAlreadyFullfilled)
		{
			ListTracker listTracker2 = GetTracker(EndgameID.Scholar, addIfMissing: true) as ListTracker;
			foreach (DataPearl.AbstractDataPearl.DataPearlType item4 in list)
			{
				int item = (int)item4;
				listTracker2.AddItemToList(item);
			}
		}
		if (GetTracker(EndgameID.Friend, num4 > 0f) is FloatTracker floatTracker4)
		{
			if (num4 == -1f && !floatTracker4.GoalAlreadyFullfilled)
			{
				floatTracker4.SetProgress(0f);
			}
			else if (num4 > 0f)
			{
				floatTracker4.SetProgress(floatTracker4.progress + num4 * 0.34f);
			}
		}
		if (ModManager.MSC)
		{
			ListTracker listTracker3 = GetTracker(MoreSlugcatsEnums.EndgameID.Nomad, addIfMissing: true) as ListTracker;
			if (!integerTracker.GoalAlreadyFullfilled)
			{
				listTracker3.myLastList.Clear();
				listTracker3.myList.Clear();
			}
			else if (!listTracker3.GoalAlreadyFullfilled && listTracker3.myList.Count > 0 && listTracker3.myList.Count <= listTracker3.myLastList.Count)
			{
				Custom.Log("Made no gate progress!");
				listTracker3.myList.Clear();
			}
		}
		foreach (CustomPassage registeredPassage in CustomPassages.RegisteredPassages)
		{
			if (registeredPassage.IsAvailableForSlugcat(game.StoryCharacter) && registeredPassage.RequirementsMet(endgameTrackers))
			{
				registeredPassage.OnWin(this, game, GetTracker(registeredPassage.ID, addIfMissing: true));
			}
		}
		if (!ModManager.Expedition || !game.rainWorld.ExpeditionMode || game.GetStorySession.saveState.malnourished || ExpeditionData.challengeList == null)
		{
			return;
		}
		int num5 = 0;
		foreach (Challenge challenge in ExpeditionData.challengeList)
		{
			if (challenge is AchievementChallenge)
			{
				(challenge as AchievementChallenge).CheckAchievementProgress(this);
			}
			if (challenge.completed)
			{
				num5++;
			}
		}
		if (num5 >= ExpeditionData.challengeList.Count)
		{
			ExpeditionGame.expeditionComplete = true;
		}
		ExpLog.Log("Cycle complete, saving run data");
		ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, ended: false);
		global::Expedition.Expedition.coreFile.Save(runEnded: false);
		if (ExpeditionGame.expeditionComplete)
		{
			if (ExpeditionGame.runKills == null)
			{
				ExpeditionGame.runKills = game.rainWorld.progression.currentSaveState.kills;
			}
			if (ExpeditionGame.runData == null)
			{
				ExpeditionGame.runData = SlugcatSelectMenu.MineForSaveData(game.manager, ExpeditionData.slugcatPlayer);
			}
			game.manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionWinScreen);
			game.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
		}
	}

	public void PlayerDied()
	{
		for (int i = 0; i < endgameTrackers.Count; i++)
		{
			if (!endgameTrackers[i].GoalAlreadyFullfilled)
			{
				DeathModifyTracker(endgameTrackers[i]);
			}
		}
	}

	public static bool MultiplyWinConditionWithNumberOfPlayers(EndgameID ID)
	{
		if (ID == EndgameID.Survivor)
		{
			return false;
		}
		if (ID == EndgameID.Monk)
		{
			return false;
		}
		if (ID == EndgameID.Outlaw)
		{
			return true;
		}
		_ = ID == EndgameID.Saint;
		return false;
	}

	public static int GourmandPassageIndex(AbstractPhysicalObject.AbstractObjectType inputObjectType, CreatureTemplate.Type inputCreatureType)
	{
		if (inputObjectType != null)
		{
			int num = 0;
			AbstractPhysicalObject.AbstractObjectType abstractObjectType = GourmandPassageRequirementAtIndex(num);
			while (abstractObjectType != inputObjectType)
			{
				num++;
				abstractObjectType = GourmandPassageRequirementAtIndex(num);
				if (abstractObjectType == null)
				{
					break;
				}
			}
			if (abstractObjectType != null)
			{
				return num;
			}
		}
		if (inputCreatureType != null)
		{
			int num2 = 0;
			AbstractPhysicalObject.AbstractObjectType abstractObjectType2 = GourmandPassageRequirementAtIndex(num2);
			while (abstractObjectType2 != AbstractPhysicalObject.AbstractObjectType.Creature || GourmandPassageCreaturesAtIndexContains(inputCreatureType, num2) == 0)
			{
				num2++;
				abstractObjectType2 = GourmandPassageRequirementAtIndex(num2);
				if (abstractObjectType2 == null)
				{
					return -1;
				}
			}
			return num2;
		}
		return -1;
	}

	public static AbstractPhysicalObject.AbstractObjectType GourmandPassageRequirementAtIndex(int index)
	{
		if (index < GourmandPassageTracker.Length)
		{
			return GourmandPassageTracker[index].type;
		}
		return null;
	}

	public static CreatureTemplate.Type[] GourmandPassageCreaturesAtIndex(int index)
	{
		if (index < GourmandPassageTracker.Length)
		{
			return GourmandPassageTracker[index].crits;
		}
		return null;
	}

	public static int GourmandPassageCreaturesAtIndexContains(CreatureTemplate.Type inputCreatureType, int index)
	{
		CreatureTemplate.Type[] array = GourmandPassageCreaturesAtIndex(index);
		if (array == null)
		{
			return 0;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == inputCreatureType)
			{
				return i + 1;
			}
		}
		return 0;
	}

	public void UpdateGhostTracker(SaveState saveState, BoolArrayTracker GhostTracker)
	{
		if (GhostTracker == null)
		{
			return;
		}
		List<string> list = SlugcatStats.SlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (World.CheckForRegionGhost(RainWorld.lastActiveSaveSlot, list[i]))
			{
				GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(list[i]);
				if (saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) && saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 2)
				{
					GhostTracker.progress[num] = true;
				}
				num++;
			}
		}
	}
}
