using RWCustom;
using UnityEngine;

namespace Music;

public class GhostSong : Song
{
	public int outOfGhostRangeCounter;

	public GhostSong(MusicPlayer musicPlayer, string ghostSongName)
		: base(musicPlayer, ghostSongName, MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
	}

	public override void Update()
	{
		base.Update();
		baseVolume = Custom.LerpAndTick(baseVolume, Mathf.Pow((musicPlayer.threatTracker != null) ? musicPlayer.threatTracker.ghostMode : 0f, 0.5f) * 0.4f, 0.0005f, 0.0025f);
		if (musicPlayer.threatTracker == null || musicPlayer.threatTracker.ghostMode == 0f)
		{
			outOfGhostRangeCounter++;
			if (outOfGhostRangeCounter > 200)
			{
				FadeOut(10f);
			}
		}
		else
		{
			outOfGhostRangeCounter = 0;
		}
	}
}
