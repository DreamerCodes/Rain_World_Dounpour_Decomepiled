public class DisembodiedLoopEmitter : SoundEmitter
{
	public float pan;

	public DisembodiedLoopEmitter(float vol, float ptch, float pan)
		: base(vol, ptch)
	{
		this.pan = pan;
		volume = vol;
		pitch = ptch;
		requireActiveUpkeep = true;
	}
}
