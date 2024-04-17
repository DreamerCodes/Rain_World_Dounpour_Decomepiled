using System.Globalization;
using RWCustom;
using UnityEngine;

public class DirectionalSound : DopplerAffectedSound
{
	public Vector2 direction = new Vector2(0f, -1f);

	public DirectionalSound(string sample, bool inherited)
		: base(sample, inherited)
	{
		type = Type.Directional;
	}

	public override string ToString()
	{
		return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "DIR><{0}><{1}><{2}><{3}><{4}><{5}><{6}", sample, panelPosition.x, panelPosition.y, volume, pitch, Custom.VecToDeg(direction), dopplerFac), "><", unrecognizedAttributes);
	}

	public override void FromString(string[] s)
	{
		sample = s[1];
		panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		volume = float.Parse(s[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		pitch = float.Parse(s[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		direction = Custom.DegToVec(float.Parse(s[6], NumberStyles.Any, CultureInfo.InvariantCulture));
		if (s.Length > 7)
		{
			dopplerFac = float.Parse(s[7], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 8);
	}
}
