using Music;
using RWCustom;

namespace MoreSlugcats;

public class MSSirenSong : Song
{
	public float? setVolume;

	public int destroyCounter;

	public MSSirenSong(MusicPlayer musicPlayer, bool muffled)
		: base(musicPlayer, muffled ? "MOON_SIREN_MS" : "MOON_SIREN", MusicPlayer.MusicContext.StoryMode)
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
			if (destroyCounter > 150 || musicPlayer == null || musicPlayer.manager == null || musicPlayer.manager.currentMainLoop == null || musicPlayer.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game)
			{
				FadeOut(200f);
			}
		}
		setVolume = null;
	}
}
