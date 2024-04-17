using System.Globalization;
using System.Text.RegularExpressions;

public class MusicEvent : TriggeredEvent
{
	public string songName = "NO SONG";

	public float prio = 0.5f;

	public float maxThreatLevel = 1f;

	public float droneTolerance;

	public float volume = 0.3f;

	public float fadeInTime;

	public bool loop;

	public bool oneSongPerCycle;

	public bool stopAtDeath;

	public bool stopAtGate;

	public int roomsRange = -1;

	public int cyclesRest = 5;

	public MusicEvent()
		: base(EventType.MusicEvent)
	{
	}

	public override string ToString()
	{
		string text = base.ToString() + "<eA>songName<eB>" + songName + string.Format(CultureInfo.InvariantCulture, "<eA>vol<eB>{0}<eA>prio<eB>", volume) + string.Format(CultureInfo.InvariantCulture, "{0}<eA>mxThrt<eB>{1}", prio, maxThreatLevel) + "<eA>loop<eB>" + (loop ? "1" : "0") + string.Format(CultureInfo.InvariantCulture, "<eA>droneTol<eB>{0}", droneTolerance) + string.Format(CultureInfo.InvariantCulture, "<eA>rooms<eB>{0}", roomsRange) + string.Format(CultureInfo.InvariantCulture, "<eA>rest<eB>{0}", cyclesRest) + string.Format(CultureInfo.InvariantCulture, "<eA>fadeIn<eB>{0}", fadeInTime) + "<eA>oneSongPerCycle<eB>" + (oneSongPerCycle ? "1" : "0") + "<eA>stopAtDeath<eB>" + (stopAtDeath ? "1" : "0") + "<eA>stopAtGate<eB>" + (stopAtGate ? "1" : "0");
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<eA>" + unrecognizedSaveString;
		}
		return text;
	}

	public override void FromString(string[] s)
	{
		base.FromString(s);
		unrecognizedSaveStrings.Clear();
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<eB>");
			switch (array[0])
			{
			case "songName":
				songName = array[1];
				continue;
			case "vol":
				volume = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "prio":
				prio = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "mxThrt":
				maxThreatLevel = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "loop":
				loop = array[1] == "1";
				continue;
			case "droneTol":
				droneTolerance = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "rooms":
				roomsRange = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "rest":
				cyclesRest = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "fadeIn":
				fadeInTime = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "oneSongPerCycle":
				oneSongPerCycle = array[1] == "1";
				continue;
			case "stopAtDeath":
				stopAtDeath = array[1] == "1";
				continue;
			case "stopAtGate":
				stopAtGate = array[1] == "1";
				continue;
			}
			if (s[i].Trim().Length > 0 && array.Length >= 2)
			{
				unrecognizedSaveStrings.Add(s[i]);
			}
		}
	}
}
