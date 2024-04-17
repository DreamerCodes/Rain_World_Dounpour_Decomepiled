using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public abstract class CreatureState
{
	public SocialMemory socialMemory;

	public int meatLeft;

	private AbstractCreature creature;

	public Dictionary<string, string> unrecognizedSaveStrings;

	public int miscSaveFlags;

	public string miscSaveString;

	public bool alive { get; private set; }

	public bool dead => !alive;

	public CreatureState(AbstractCreature creature)
	{
		this.creature = creature;
		alive = true;
		meatLeft = creature.creatureTemplate.meatPoints;
		unrecognizedSaveStrings = new Dictionary<string, string>();
		miscSaveString = "";
		if (creature.creatureTemplate.socialMemory)
		{
			socialMemory = new SocialMemory();
		}
	}

	public void Die()
	{
		alive = false;
	}

	public string BaseSaveString()
	{
		string text = "";
		if (dead)
		{
			text += "Dead<cB>";
		}
		if (alive && socialMemory != null && socialMemory.relationShips.Count > 0)
		{
			text = text + "Social<cC>" + socialMemory.ToString() + "<cB>";
		}
		if (creature.spawnData != null)
		{
			text = text + "SpawnData<cC>" + creature.spawnData + "<cB>";
		}
		if (meatLeft != creature.creatureTemplate.meatPoints)
		{
			text += string.Format(CultureInfo.InvariantCulture, "MeatLeft<cC>{0}<cB>", meatLeft);
		}
		if (ModManager.MSC)
		{
			if (creature.RemovedKarma > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "ExtractKarma<cC>{0}<cB>", creature.RemovedKarma);
			}
			if (miscSaveFlags > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "MiscFlags<cC>{0}<cB>", miscSaveFlags);
			}
			if (miscSaveString != "")
			{
				text = text + "MiscData<cC>" + miscSaveString + "<cB>";
			}
		}
		return text;
	}

	public override string ToString()
	{
		string text = BaseSaveString();
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value + "<cB>";
		}
		return text;
	}

	public virtual void LoadFromString(string[] s)
	{
		unrecognizedSaveStrings.Clear();
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<cC>");
			switch (array[0])
			{
			case "Dead":
				alive = false;
				break;
			case "Social":
				socialMemory = SocialMemory.FromString(array[1]);
				break;
			case "SpawnData":
				creature.spawnData = array[1];
				break;
			case "MeatLeft":
				meatLeft = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "ExtractKarma":
				if (ModManager.MSC)
				{
					creature.RemovedKarma = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				break;
			case "MiscFlags":
				if (ModManager.MSC)
				{
					miscSaveFlags = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				break;
			case "MiscData":
				if (ModManager.MSC)
				{
					miscSaveString = array[1];
				}
				break;
			default:
				if (array.Length >= 2)
				{
					unrecognizedSaveStrings[array[0]] = array[1];
				}
				break;
			}
		}
		if (dead && ModManager.MSC)
		{
			creature.extractKarma();
		}
	}

	public virtual void CycleTick()
	{
		if (socialMemory != null)
		{
			socialMemory.CycleTick();
		}
	}
}
