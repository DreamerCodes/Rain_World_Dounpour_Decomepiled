using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class MouseState : HealthState
{
	public int battery;

	public MouseState(AbstractCreature creature)
		: base(creature)
	{
		battery = 4000;
	}

	public override string ToString()
	{
		string text = HealthBaseSaveString() + ((battery < 4000) ? string.Format(CultureInfo.InvariantCulture, "<cB>Battery<cC>{0}", battery) : "");
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
		}
		return text;
	}

	public override void LoadFromString(string[] s)
	{
		base.LoadFromString(s);
		for (int i = 0; i < s.Length; i++)
		{
			string text = Regex.Split(s[i], "<cC>")[0];
			if (text != null && text == "Battery")
			{
				battery = int.Parse(Regex.Split(s[i], "<cC>")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
		}
		unrecognizedSaveStrings.Remove("Battery");
	}
}
