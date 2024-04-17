public abstract class DopplerAffectedSound : AmbientSound
{
	public float dopplerFac;

	public DopplerAffectedSound(string sample, bool inherited)
		: base(sample, inherited)
	{
	}
}
