using System.Globalization;
using System.Text.RegularExpressions;

public class StopMusicEvent : TriggeredEvent
{
	public class Type : ExtEnum<Type>
	{
		public static readonly Type AllSongs = new Type("AllSongs", register: true);

		public static readonly Type SpecificSong = new Type("SpecificSong", register: true);

		public static readonly Type AllButSpecific = new Type("AllButSpecific", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public string songName = "NO SONG";

	public float prio = 1f;

	public float fadeOutTime = 200f;

	public new Type type = Type.AllSongs;

	public StopMusicEvent()
		: base(EventType.StopMusicEvent)
	{
	}

	public override string ToString()
	{
		string text = base.ToString() + string.Format(CultureInfo.InvariantCulture, "<eA>songName<eB>{0}<eA>prio<eB>{1}<eA>type<eB>{2}<eA>fadeOut<eB>{3}", songName, prio, type, fadeOutTime);
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
				break;
			case "prio":
				prio = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "type":
			{
				if (int.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					type = BackwardsCompatibilityRemix.ParseStopMusicType(result);
				}
				else
				{
					type = new Type(array[1]);
				}
				break;
			}
			case "fadeOut":
				fadeOutTime = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			default:
				if (s[i].Trim().Length > 0 && array.Length >= 2)
				{
					unrecognizedSaveStrings.Add(s[i]);
				}
				break;
			}
		}
	}
}
