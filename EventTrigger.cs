using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class EventTrigger
{
	public class TriggerType : ExtEnum<TriggerType>
	{
		public static readonly TriggerType Spot = new TriggerType("Spot", register: true);

		public static readonly TriggerType SeeCreature = new TriggerType("SeeCreature", register: true);

		public static readonly TriggerType PreRegionBump = new TriggerType("PreRegionBump", register: true);

		public static readonly TriggerType RegionBump = new TriggerType("RegionBump", register: true);

		public TriggerType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public TriggerType type;

	public TriggeredEvent tEvent;

	public int activeToCycle = -1;

	public int activeFromCycle;

	public int delay;

	public bool multiUse;

	public float fireChance = 1f;

	public int entrance = -1;

	public int karma;

	public List<SlugcatStats.Name> slugcats;

	public Dictionary<string, string> unrecognizedSaveStrings = new Dictionary<string, string>();

	public Vector2 panelPosition;

	public EventTrigger(TriggerType type)
	{
		this.type = type;
		slugcats = new List<SlugcatStats.Name>();
		for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
		{
			SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
			if (!SlugcatStats.HiddenOrUnplayableSlugcat(name))
			{
				slugcats.Add(name);
			}
		}
	}

	public string BaseSaveString()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
		{
			string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
			SlugcatStats.Name name = new SlugcatStats.Name(entry);
			if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !slugcats.Contains(name))
			{
				list.Add(entry);
			}
		}
		return type.ToString() + "<tA>event<tB>" + ((tEvent != null) ? tEvent.ToString() : "NONE") + "<tA>multiUse<tB>" + (multiUse ? "1" : "0") + string.Format(CultureInfo.InvariantCulture, "<tA>toCycle<tB>{0}", activeToCycle) + string.Format(CultureInfo.InvariantCulture, "<tA>fromCycle<tB>{0}", activeFromCycle) + string.Format(CultureInfo.InvariantCulture, "<tA>delay<tB>{0}", delay) + string.Format(CultureInfo.InvariantCulture, "<tA>panPos<tB>{0}<tB>{1}", panelPosition.x, panelPosition.y) + string.Format(CultureInfo.InvariantCulture, "<tA>fireChance<tB>{0}", fireChance) + string.Format(CultureInfo.InvariantCulture, "<tA>entrance<tB>{0}", entrance) + string.Format(CultureInfo.InvariantCulture, "<tA>karma<tB>{0}", karma) + "<tA>slugcats<tB>" + string.Join("|", list.ToArray());
	}

	public override string ToString()
	{
		string text = BaseSaveString();
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<tA>" + unrecognizedSaveString.Key + "<tB>" + unrecognizedSaveString.Value;
		}
		return text;
	}

	public virtual void FromString(string[] s)
	{
		unrecognizedSaveStrings.Clear();
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<tB>");
			switch (array[0])
			{
			case "event":
				if (array[1] != "NONE")
				{
					string[] array2 = Regex.Split(array[1], "<eA>");
					switch (array2[0])
					{
					case "MusicEvent":
						tEvent = new MusicEvent();
						tEvent.FromString(array2);
						break;
					case "StopMusicEvent":
						tEvent = new StopMusicEvent();
						tEvent.FromString(array2);
						break;
					case "ShowProjectedImageEvent":
						tEvent = new ShowProjectedImageEvent();
						tEvent.FromString(array2);
						break;
					default:
						tEvent = new TriggeredEvent(new TriggeredEvent.EventType(array[1]));
						tEvent.FromString(array2);
						break;
					}
				}
				break;
			case "multiUse":
				multiUse = array[1] == "1";
				break;
			case "toCycle":
				activeToCycle = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "fromCycle":
				activeFromCycle = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "delay":
				delay = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "panPos":
				panelPosition = new Vector2(float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				break;
			case "fireChance":
				fireChance = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "entrance":
				entrance = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "karma":
				karma = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "slugcats":
			{
				if (Custom.IsDigitString(array[1]))
				{
					BackwardsCompatibilityRemix.ParsePlayerAvailability(array[1], slugcats);
					break;
				}
				slugcats.Clear();
				List<string> list = new List<string>(array[1].Split('|'));
				for (int j = 0; j < ExtEnum<SlugcatStats.Name>.values.Count; j++)
				{
					string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(j);
					SlugcatStats.Name name = new SlugcatStats.Name(entry);
					if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !list.Contains(entry))
					{
						slugcats.Add(name);
					}
				}
				break;
			}
			default:
				if (array.Length >= 2)
				{
					unrecognizedSaveStrings[array[0]] = array[1];
				}
				break;
			}
		}
	}
}
