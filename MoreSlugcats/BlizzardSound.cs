namespace MoreSlugcats;

public class BlizzardSound : UpdatableAndDeletable
{
	public float windStrength;

	public float blizzHowlPan;

	public float windVol;

	public float windDir;

	public DisembodiedDynamicSoundLoop blizzWind;

	public DisembodiedDynamicSoundLoop blizzHowl;

	private RoomCamera rCam;

	public override void Update(bool eu)
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		if (blizzHowl == null)
		{
			blizzHowl = new DisembodiedDynamicSoundLoop(this);
			blizzHowl.VolumeGroup = 3;
			blizzHowl.Volume = 0f;
			blizzHowl.sound = SoundID.Blizzard_Wind_LOOP;
			return;
		}
		blizzHowl.Update();
		blizzHowl.Volume = windStrength;
		blizzHowl.Pan = blizzHowlPan;
		blizzHowl.Pitch = 0.5f + 0.5f * windStrength;
		if (blizzWind == null)
		{
			blizzWind = new DisembodiedDynamicSoundLoop(this);
			blizzWind.VolumeGroup = 3;
			blizzWind.Volume = 0f;
			blizzWind.sound = SoundID.Blizzard_Face_LOOP;
		}
		else
		{
			blizzWind.Update();
			blizzWind.Volume = windVol;
			blizzWind.Pan = windDir;
			blizzWind.Pitch = 0.5f + 0.5f * windVol;
		}
	}

	public BlizzardSound(RoomCamera rCam)
	{
		this.rCam = rCam;
	}
}
