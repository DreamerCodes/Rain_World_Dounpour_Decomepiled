using Music;
using RWCustom;

namespace MoreSlugcats;

public class SaintEndingSong : Song
{
	public float? setVolume;

	public int destroyCounter;

	public SaintEndingSong(MusicPlayer musicPlayer)
		: base(musicPlayer, "RW_92 - Reclaiming Entropy", MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 1000f;
		base.Loop = true;
	}

	public override void Update()
	{
		base.Update();
		if (setVolume.HasValue)
		{
			destroyCounter = 0;
			baseVolume = Custom.LerpAndTick(baseVolume, setVolume.Value, 0.005f, 0.0025f);
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
