using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using UnityEngine;

public class LizardState : HealthState
{
	public float throatHealth;

	public float[] limbHealth;

	public LizardState(AbstractCreature creature)
		: base(creature)
	{
		throatHealth = 1f;
		if (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
		{
			limbHealth = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };
		}
		else
		{
			limbHealth = new float[4] { 1f, 1f, 1f, 1f };
		}
	}

	public override string ToString()
	{
		string text = HealthBaseSaveString();
		if (throatHealth < 1f)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<cB>ThroatHealth<cC>{0}", throatHealth);
		}
		bool flag = false;
		for (int i = 0; i < limbHealth.Length; i++)
		{
			if (limbHealth[i] < 1f)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<cB>LimbHealth<cC>{0}", string.Join("<cC>", limbHealth));
		}
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
			string[] array = Regex.Split(s[i], "<cC>");
			switch (array[0])
			{
			case "ThroatHealth":
				throatHealth = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "LimbHealth":
			{
				for (int j = 0; j < array.Length - 1; j++)
				{
					limbHealth[j] = float.Parse(array[j + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				break;
			}
			}
		}
		unrecognizedSaveStrings.Remove("ThroatHealth");
		unrecognizedSaveStrings.Remove("LimbHealth");
	}

	public override void CycleTick()
	{
		throatHealth = Mathf.Min(throatHealth + 0.25f, 1f);
		for (int i = 0; i < limbHealth.Length; i++)
		{
			limbHealth[i] = Mathf.Min(limbHealth[i] + 0.5f, 1f);
		}
		base.CycleTick();
	}
}
