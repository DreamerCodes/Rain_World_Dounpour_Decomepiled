using System.Globalization;
using RWCustom;
using UnityEngine;

public class SpotSound : DopplerAffectedSound
{
	public Vector2 pos;

	public Vector2 radHandlePosition = Custom.DegToVec(-135f) * 50f;

	public float rad = 50f;

	public float taper = 0.1f;

	public float TaperRad => rad + (rad * 2f + 1000f) * taper;

	public SpotSound(string sample, bool inherited)
		: base(sample, inherited)
	{
		type = Type.Spot;
	}

	public override string ToString()
	{
		return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "SPOT><{0}><{1}><{2}><{3}><{4}><{5}><{6}><{7}><{8}><{9}><{10}", sample, panelPosition.x, panelPosition.y, volume, pitch, pos.x, pos.y, radHandlePosition.x, radHandlePosition.y, dopplerFac, taper), "><", unrecognizedAttributes);
	}

	public override void FromString(string[] s)
	{
		string text = "L" + s.Length + " " + string.Join(",", s) + "##";
		for (int i = 0; i < s.Length; i++)
		{
			text = text + i + ";" + s[i] + "::";
		}
		sample = s[1];
		panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		volume = float.Parse(s[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		pitch = float.Parse(s[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		pos.x = float.Parse(s[6], NumberStyles.Any, CultureInfo.InvariantCulture);
		pos.y = float.Parse(s[7], NumberStyles.Any, CultureInfo.InvariantCulture);
		radHandlePosition.x = float.Parse(s[8], NumberStyles.Any, CultureInfo.InvariantCulture);
		radHandlePosition.y = float.Parse(s[9], NumberStyles.Any, CultureInfo.InvariantCulture);
		if (s.Length > 10)
		{
			dopplerFac = float.Parse(s[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			taper = float.Parse(s[11], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		rad = radHandlePosition.magnitude;
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 12);
	}
}
