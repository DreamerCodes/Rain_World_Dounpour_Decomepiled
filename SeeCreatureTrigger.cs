using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SeeCreatureTrigger : EventTrigger
{
	public CreatureTemplate.Type creatureType;

	public SeeCreatureTrigger(CreatureTemplate.Type creatureType)
		: base(TriggerType.SeeCreature)
	{
		this.creatureType = creatureType;
	}

	public override string ToString()
	{
		string text = BaseSaveString() + "<tA>critType<tB>" + creatureType.ToString();
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
			string text = array[0];
			if (text != null && text == "critType")
			{
				creatureType = new CreatureTemplate.Type(array[1]);
			}
		}
		unrecognizedSaveStrings.Remove("critType");
	}
}
