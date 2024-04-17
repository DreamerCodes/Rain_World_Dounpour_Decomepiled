using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class SpotTrigger : EventTrigger
{
	public Vector2 pos;

	public Vector2 radHandlePosition = Custom.DegToVec(-135f) * 50f;

	public float rad = 50f;

	public SpotTrigger()
		: base(TriggerType.Spot)
	{
	}

	public override string ToString()
	{
		string text = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "<tA>pos<tB>{0}<tB>{1}<tA>handlePos<tB>{2}<tB>{3}", pos.x, pos.y, radHandlePosition.x, radHandlePosition.y);
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<tA>" + unrecognizedSaveString.Key + "<tB>" + unrecognizedSaveString.Value;
		}
		return text;
	}

	public override void FromString(string[] s)
	{
		base.FromString(s);
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<tB>");
			switch (array[0])
			{
			case "pos":
				pos = new Vector2(float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				break;
			case "handlePos":
				radHandlePosition = new Vector2(float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				break;
			}
		}
		rad = radHandlePosition.magnitude;
		unrecognizedSaveStrings.Remove("pos");
		unrecognizedSaveStrings.Remove("handlePos");
	}
}
