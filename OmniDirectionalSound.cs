using System.Globalization;

public class OmniDirectionalSound : AmbientSound
{
	public OmniDirectionalSound(string sample, bool inherited)
		: base(sample, inherited)
	{
		type = Type.Omnidirectional;
	}

	public override string ToString()
	{
		return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "OMNI><{0}><{1}><{2}><{3}><{4}", sample, panelPosition.x, panelPosition.y, volume, pitch), "><", unrecognizedAttributes);
	}

	public override void FromString(string[] s)
	{
		sample = s[1];
		panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		volume = float.Parse(s[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		pitch = float.Parse(s[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 6);
	}
}
