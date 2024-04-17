using RWCustom;

namespace Music;

public class SSSong : Song
{
	public float? setVolume;

	public int destroyCounter;

	public SSSong(MusicPlayer musicPlayer)
		: base(musicPlayer, "NA_41 - Random Gods", MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
		base.Loop = true;
	}

	public SSSong(MusicPlayer musicPlayer, string song)
		: base(musicPlayer, song, MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
		base.Loop = true;
	}

	public override void Update()
	{
		base.Update();
		if (setVolume.HasValue)
		{
			baseVolume = Custom.LerpAndTick(baseVolume, setVolume.Value, 0.005f, 0.0025f);
			destroyCounter = 0;
		}
		else
		{
			destroyCounter++;
			if (destroyCounter > 150)
			{
				FadeOut(400f);
			}
		}
		setVolume = null;
	}
}
