using System.Globalization;
using System.Text.RegularExpressions;

public class ShowProjectedImageEvent : TriggeredEvent
{
	public bool afterEncounter;

	public bool onlyWhenShowingDirection;

	public int fromCycle;

	public ShowProjectedImageEvent()
		: base(EventType.ShowProjectedImageEvent)
	{
	}

	public override string ToString()
	{
		string text = base.ToString() + "<eA>afterEncounter<eB>" + (afterEncounter ? "1" : "0") + string.Format(CultureInfo.InvariantCulture, "<eA>fromCycle<eB>{0}", fromCycle) + "<eA>onlyWhenShowingDirection<eB>" + (onlyWhenShowingDirection ? "1" : "0");
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<eA>" + unrecognizedSaveString;
		}
		return text;
	}

	public override void FromString(string[] s)
	{
		base.FromString(s);
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<eB>");
			switch (array[0])
			{
			case "afterEncounter":
				afterEncounter = array[1] == "1";
				continue;
			case "fromCycle":
				fromCycle = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				continue;
			case "onlyWhenShowingDirection":
				onlyWhenShowingDirection = array[1] == "1";
				continue;
			}
			if (s[i].Trim().Length > 0 && array.Length >= 2)
			{
				unrecognizedSaveStrings.Add(s[i]);
			}
		}
	}
}
