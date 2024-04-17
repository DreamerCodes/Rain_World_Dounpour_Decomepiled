using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class HealthState : CreatureState
{
	private float h;

	public float health
	{
		get
		{
			return h;
		}
		set
		{
			h = value;
		}
	}

	public float ClampedHealth => Mathf.Clamp(h, 0f, 1f);

	public HealthState(AbstractCreature creature)
		: base(creature)
	{
		health = 1f;
	}

	public string HealthBaseSaveString()
	{
		return BaseSaveString() + ((health < 1f) ? string.Format(CultureInfo.InvariantCulture, "<cB>Health<cC>{0}", health) : "");
	}

	public override string ToString()
	{
		string text = HealthBaseSaveString();
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
			if (text != null && text == "Health")
			{
				health = float.Parse(Regex.Split(s[i], "<cC>")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
		}
		unrecognizedSaveStrings.Remove("Health");
	}

	public override void CycleTick()
	{
		health = Mathf.Min(1f, health + 0.25f);
		base.CycleTick();
	}
}
