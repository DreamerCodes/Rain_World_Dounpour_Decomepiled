using RWCustom;
using VoidSea;

namespace Music;

public class VoidSeaMusic : Song
{
	private VoidSeaScene scene;

	public VoidSeaMusic(MusicPlayer musicPlayer, VoidSeaScene scene)
		: base(musicPlayer, "VS_A_GOLD_SWIM", MusicPlayer.MusicContext.StoryMode)
	{
		this.scene = scene;
		subTracks.Add(new SubTrack(this, 1, "VS_B_BIG_OPEN"));
		subTracks.Add(new SubTrack(this, 2, "VS_C_WORM_INFERNO"));
		subTracks.Add(new SubTrack(this, 3, "VS_D_THE_RIDE"));
		subTracks.Add(new SubTrack(this, 4, "VS_E_DEEP_GHOSTS"));
		subTracks.Add(new SubTrack(this, 5, "VS_F_THE_CORE"));
		if (ModManager.MSC)
		{
			SubTrack item = new SubTrack(this, 6, "VS_SA_THE_DEED")
			{
				isSynced = false
			};
			subTracks.Add(item);
			SubTrack item2 = new SubTrack(this, 7, "VS_SA_PULSE")
			{
				isSynced = false
			};
			subTracks.Add(item2);
			SubTrack item3 = new SubTrack(this, 8, "VS_SA_START")
			{
				isSynced = false
			};
			subTracks.Add(item3);
		}
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].volume = 0f;
		}
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
		base.Loop = true;
	}

	public override void Update()
	{
		base.Update();
		float lerp = 0.003f;
		float tick = 0.0033333334f;
		if (!ModManager.MSC || !scene.Inverted)
		{
			subTracks[0].volume = Custom.LerpAndTick(subTracks[0].volume, scene.SwimDownMusic, lerp, tick);
			subTracks[1].volume = Custom.LerpAndTick(subTracks[1].volume, scene.BigOpenMusic, lerp, tick);
		}
		subTracks[2].volume = Custom.LerpAndTick(subTracks[2].volume, scene.WormsMusic, lerp, tick);
		subTracks[3].volume = Custom.LerpAndTick(subTracks[3].volume, scene.TheRideMusic, lerp, tick);
		subTracks[4].volume = Custom.LerpAndTick(subTracks[4].volume, scene.SlugcatGhostMusic, lerp, tick);
		subTracks[5].volume = Custom.LerpAndTick(subTracks[5].volume, scene.TheLightMusic, lerp, tick);
		if (ModManager.MSC)
		{
			subTracks[6].volume = Custom.LerpAndTick(subTracks[6].volume, scene.VSS_DeathMusic * (1f - scene.musicFadeFac), lerp, tick);
			subTracks[7].volume = Custom.LerpAndTick(subTracks[7].volume, scene.VSS_TransformMusic * (1f - scene.musicFadeFac), lerp, tick);
			if (scene.Inverted)
			{
				subTracks[8].volume = Custom.LerpAndTick(subTracks[8].volume, scene.SwimDownMusic * (1f - scene.musicFadeFac), lerp, tick);
			}
		}
	}
}
