using UnityEngine;

namespace Music;

public class IntroRollMusic : Song
{
	private float rainVol;

	public bool fadeOutRain;

	public bool musicTrackStarted;

	public IntroRollMusic(MusicPlayer musicPlayer)
		: base(musicPlayer, "TitleRollRain", MusicPlayer.MusicContext.Menu)
	{
		subTracks.Add(new SubTrack(this, 1, "RW_8 - Sundown"));
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].volume = 1f;
		}
		priority = 0f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 10f;
		rainVol = 1f;
	}

	protected override void StartPlaying()
	{
		startedPlaying = true;
		subTracks[0].StartPlaying();
	}

	public void StartMusic()
	{
		if (!musicTrackStarted)
		{
			musicTrackStarted = true;
			subTracks[1].StartPlaying();
		}
	}

	public override void Update()
	{
		base.Update();
		if (fadeOutRain)
		{
			rainVol = Mathf.Max(0f, rainVol - 0.02f);
		}
		subTracks[0].volume = rainVol;
	}
}
