using Music;
using RWCustom;

namespace MoreSlugcats;

public class HalcyonSong : Song
{
	public float? setVolume;

	public int destroyCounter;

	public HalcyonSong(MusicPlayer musicPlayer, string song)
		: base(musicPlayer, song, MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 20f;
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
				FadeOut(200f);
			}
		}
		setVolume = null;
	}
}
