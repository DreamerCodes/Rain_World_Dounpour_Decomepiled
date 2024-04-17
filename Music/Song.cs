using UnityEngine;

namespace Music;

public class Song : MusicPiece
{
	public float priority = 0.5f;

	public float fadeOutAtThreat = 1f;

	public float droneVolume;

	public float fadeInTime;

	public float baseVolume = 0.3f;

	public int roomTransitions = -1;

	public float fadeOutTime = -1f;

	public bool stopAtGate;

	public bool stopAtDeath;

	public bool FadingOut => fadeOutTime >= 0f;

	public Song(MusicPlayer musicPlayer, string name, MusicPlayer.MusicContext context)
		: base(musicPlayer, name, context)
	{
		subTracks.Add(new SubTrack(this, 0, name));
	}

	public override void Update()
	{
		base.Update();
		if (startedPlaying && !subTracks[0].source.isPlaying && subTracks[0].readyToPlay)
		{
			bool flag = true;
			for (int i = 1; i < subTracks.Count; i++)
			{
				if (subTracks[i].source.isPlaying || !subTracks[i].readyToPlay)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				FadeOut(10f);
			}
		}
		volume = musicPlayer.mainSongMix * baseVolume;
	}

	public void PlayerToNewRoom()
	{
		if (roomTransitions >= 0)
		{
			roomTransitions--;
			if (roomTransitions <= 0)
			{
				FadeOut(400f);
			}
		}
	}

	public void FadeOut(float speed)
	{
		if (fadeOutTime < 0f)
		{
			fadeOutTime = speed;
		}
		else
		{
			fadeOutTime = Mathf.Min(fadeOutTime, speed);
		}
	}

	public void ResetSongStream()
	{
		for (int i = 0; i < subTracks.Count; i++)
		{
			if (subTracks[i].isStreamed && subTracks[i].source.isPlaying)
			{
				subTracks[i].source.timeSamples = 0;
			}
		}
	}
}
