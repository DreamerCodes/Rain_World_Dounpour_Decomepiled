namespace Music;

public class MenuOrSlideShowSong : Song
{
	public MenuOrSlideShowSong(MusicPlayer musicPlayer, string name, float priority, float fadeInTime)
		: base(musicPlayer, name, MusicPlayer.MusicContext.Menu)
	{
		base.fadeInTime = fadeInTime;
		base.priority = priority;
	}

	public override void Update()
	{
		base.Update();
	}
}
