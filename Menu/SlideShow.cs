using System;
using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;

namespace Menu;

public class SlideShow : Menu
{
	public class SlideShowID : ExtEnum<SlideShowID>
	{
		public static readonly SlideShowID WhiteIntro = new SlideShowID("WhiteIntro", register: true);

		public static readonly SlideShowID YellowIntro = new SlideShowID("YellowIntro", register: true);

		public static readonly SlideShowID WhiteOutro = new SlideShowID("WhiteOutro", register: true);

		public static readonly SlideShowID YellowOutro = new SlideShowID("YellowOutro", register: true);

		public static readonly SlideShowID RedOutro = new SlideShowID("RedOutro", register: true);

		public SlideShowID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Scene
	{
		public MenuScene.SceneID sceneID;

		public float startAt;

		public float fadeInDoneAt;

		public float fadeOutStartAt;

		public List<float> crossfades;

		public List<int> crossfadeDurations;

		public Scene(MenuScene.SceneID sceneID, float startAt, float fadeInDoneAt, float fadeOutStartAt)
		{
			this.sceneID = sceneID;
			this.startAt = startAt;
			this.fadeInDoneAt = fadeInDoneAt;
			this.fadeOutStartAt = fadeOutStartAt;
			crossfades = new List<float>();
			crossfadeDurations = new List<int>();
		}

		public void AddCrossFade(float timeStart, int duration)
		{
			crossfades.Add(timeStart);
			crossfadeDurations.Add(duration);
		}
	}

	public int current = -1;

	private float time;

	public List<Scene> playList;

	public SlideShowID slideShowID;

	public ProcessManager.ProcessID processAfterSlideShow;

	public string waitForMusic;

	public bool stall;

	public float inSceneTime;

	public SlideShowMenuScene[] preloadedScenes;

	public KarmaLadderScreen.SleepDeathScreenDataPackage endGameStatsPackage;

	public KarmaLadderScreen.SleepDeathScreenDataPackage passthroughPackage;

	public int crossfadeInd;

	private bool pauseButton;

	private bool lastPauseButton;

	private bool skip;

	public override bool ForceNoMouseMode => true;

	public SlideShow(ProcessManager manager, SlideShowID slideShowID)
		: base(manager, ProcessManager.ProcessID.SlideShow)
	{
		this.slideShowID = slideShowID;
		pages.Add(new Page(this, null, "main", 0));
		playList = new List<Scene>();
		if (slideShowID == SlideShowID.WhiteIntro || slideShowID == SlideShowID.YellowIntro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Intro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_1_Tree, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, 8, 6)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 9, 6), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_2_Branch, ConvertTime(0, 9, 19), ConvertTime(0, 10, 19), ConvertTime(0, 16, 2)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_3_In_Tree, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 24, 3)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_4_Walking, ConvertTime(0, 24, 26), ConvertTime(0, 25, 19), ConvertTime(0, 32, 2)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 34, 6), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_5_Hunting, ConvertTime(0, 35, 50), ConvertTime(0, 36, 54), ConvertTime(0, 42, 15)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_6_7_Rain_Drop, ConvertTime(0, 43, 0), ConvertTime(0, 44, 0), ConvertTime(0, 49, 29)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_8_Climbing, ConvertTime(0, 50, 19), ConvertTime(0, 51, 9), ConvertTime(0, 55, 21)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 56, 24), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_9_Rainy_Climb, ConvertTime(0, 57, 2), ConvertTime(0, 57, 80), ConvertTime(1, 1, 1)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_10_Fall, ConvertTime(1, 1, 1), ConvertTime(1, 1, 1), ConvertTime(1, 4, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_10_5_Separation, ConvertTime(1, 4, 29), ConvertTime(1, 5, 18), ConvertTime(1, 11, 10)));
			if (slideShowID == SlideShowID.WhiteIntro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Intro_11_Drowning, ConvertTime(1, 11, 25), ConvertTime(1, 12, 10), ConvertTime(1, 17, 28)));
				playList.Add(new Scene(MenuScene.SceneID.Intro_12_Waking, ConvertTime(1, 19, 2), ConvertTime(1, 20, 6), ConvertTime(1, 21, 29)));
				playList.Add(new Scene(MenuScene.SceneID.Intro_13_Alone, ConvertTime(1, 22, 22), ConvertTime(1, 23, 15), ConvertTime(1, 26, 24)));
			}
			else if (slideShowID == SlideShowID.YellowIntro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Yellow_Intro_A, ConvertTime(1, 11, 25), ConvertTime(1, 14, 0), ConvertTime(1, 21, 80)));
				playList.Add(new Scene(MenuScene.SceneID.Yellow_Intro_B, ConvertTime(1, 22, 60), ConvertTime(1, 23, 0), ConvertTime(1, 26, 80)));
			}
			playList.Add(new Scene(MenuScene.SceneID.Intro_14_Title, ConvertTime(1, 27, 24), ConvertTime(1, 31, 34), ConvertTime(1, 33, 60)));
			for (int i = 1; i < playList.Count; i++)
			{
				playList[i].startAt += 0.6f;
				playList[i].fadeInDoneAt += 0.6f;
				playList[i].fadeOutStartAt += 0.6f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Game;
		}
		else if (slideShowID == SlideShowID.WhiteOutro || (ModManager.MSC && (slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)))
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 37, 0)));
			if (slideShowID == SlideShowID.WhiteOutro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Outro_3_Face, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
				playList.Add(new Scene(MenuScene.SceneID.Outro_4_Tree, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			}
			else
			{
				playList.Add(new Scene(MenuScene.SceneID.Outro_3_Face, ConvertTime(0, 41, 10), ConvertTime(0, 51, 20), ConvertTime(0, 55, 60)));
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int j = 1; j < playList.Count; j++)
			{
				playList[j].startAt -= 1.1f;
				playList[j].fadeInDoneAt -= 1.1f;
				playList[j].fadeOutStartAt -= 1.1f;
			}
			if (ModManager.MSC && slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)
			{
				processAfterSlideShow = MoreSlugcatsEnums.ProcessID.DatingSim;
			}
			else
			{
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		else if (slideShowID == SlideShowID.YellowOutro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 27, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_1_Swim, ConvertTime(0, 30, 0), ConvertTime(0, 35, 0), ConvertTime(0, 37, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_2_Reach, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_3_Stop, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int k = 1; k < playList.Count; k++)
			{
				playList[k].startAt -= 1.1f;
				playList[k].fadeInDoneAt -= 1.1f;
				playList[k].fadeOutStartAt -= 1.1f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Credits;
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		else if (slideShowID == SlideShowID.RedOutro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 27, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_1_Swim, ConvertTime(0, 30, 0), ConvertTime(0, 35, 0), ConvertTime(0, 37, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_2_Sink, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_3_Embrace, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int l = 1; l < playList.Count; l++)
			{
				playList[l].startAt -= 1.1f;
				playList[l].fadeInDoneAt -= 1.1f;
				playList[l].fadeOutStartAt -= 1.1f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Credits;
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		else if (ModManager.MSC)
		{
			if (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 8, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer2, ConvertTime(0, 10, 0), ConvertTime(0, 13, 0), ConvertTime(0, 17, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer3, ConvertTime(0, 21, 10), ConvertTime(0, 25, 20), ConvertTime(0, 37, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer4, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer5, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 60)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
				for (int m = 1; m < playList.Count; m++)
				{
					playList[m].startAt -= 1.1f;
					playList[m].fadeInDoneAt -= 1.1f;
					playList[m].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.statsAfterCredits = true;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.SaintIntro)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "BM_SS_DOOR";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S1, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, 6, 26)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S2, ConvertTime(0, 10, 26), ConvertTime(0, 15, 0), ConvertTime(0, 20, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 21, 0), 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S4, ConvertTime(0, 21, 15), ConvertTime(0, 24, 0), ConvertTime(0, 27, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 28, 0), ConvertTime(0, 28, 0), ConvertTime(0, 30, 0)));
				for (int n = 1; n < playList.Count; n++)
				{
					playList[n].startAt += 0.6f;
					playList[n].fadeInDoneAt += 0.6f;
					playList[n].fadeOutStartAt += 0.6f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Game;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					if (manager.pebblesHasHalcyon)
					{
						waitForMusic = "NA_19 - Halcyon Memories";
					}
					else
					{
						waitForMusic = "NA_43 - Isolation";
					}
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				int num = 13;
				int num2 = 2;
				int num3 = 5;
				int num4 = 7;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet1, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, num, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, num + num2, 0), 0f, 0f));
				Scene scene = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L0, ConvertTime(0, num + num2, 15), ConvertTime(0, num + num2 + 4, 15), ConvertTime(0, num + num2 + num3 + num4 + 3, 0));
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3, 15), 20);
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3 + 1, 85), 20);
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3 + 3, 55), 20);
				playList.Add(scene);
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, num + num2 + num3 + num4 + 4, 0), ConvertTime(0, num + num2 + num3 + num4 + 4, 15), ConvertTime(0, num + num2 + num3 + num4 + 6, 0)));
				for (int num5 = 1; num5 < playList.Count; num5++)
				{
					playList[num5].startAt += 0.6f;
					playList[num5].fadeInDoneAt += 0.6f;
					playList[num5].fadeOutStartAt += 0.6f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.statsAfterCredits = true;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "BM_CC_CANOPY";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 8, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_2, ConvertTime(0, 10, 0), ConvertTime(0, 13, 0), ConvertTime(0, 17, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_3, ConvertTime(0, 21, 10), ConvertTime(0, 25, 20), ConvertTime(0, 29, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 32, 0), ConvertTime(0, 32, 0), ConvertTime(0, 36, 0)));
				for (int num6 = 1; num6 < playList.Count; num6++)
				{
					playList[num6].startAt -= 1.1f;
					playList[num6].fadeInDoneAt -= 1.1f;
					playList[num6].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.SurvivorAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme_B";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 16, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_2, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 32, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4, ConvertTime(0, 33, 21), ConvertTime(0, 34, 10), ConvertTime(0, 50, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 53, 0), ConvertTime(0, 53, 0), ConvertTime(0, 57, 0)));
				for (int num7 = 1; num7 < playList.Count; num7++)
				{
					playList[num7].startAt -= 1.1f;
					playList[num7].fadeInDoneAt -= 1.1f;
					playList[num7].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.MonkAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme_B";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 16, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_3, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 32, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4, ConvertTime(0, 33, 21), ConvertTime(0, 34, 10), ConvertTime(0, 50, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 53, 0), ConvertTime(0, 53, 0), ConvertTime(0, 57, 0)));
				for (int num8 = 1; num8 < playList.Count; num8++)
				{
					playList[num8].startAt -= 1.1f;
					playList[num8].fadeInDoneAt -= 1.1f;
					playList[num8].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = (manager.foodTrackerCompletedThisSession ? "RW_Outro_Theme_B" : "RW_Outro_Theme_B_Short");
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				if (!manager.foodTrackerCompletedThisSession)
				{
					Scene scene2 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 32, 2));
					scene2.AddCrossFade(ConvertTime(0, 16, 2), 20);
					playList.Add(scene2);
					playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 34, 0), ConvertTime(0, 34, 0), ConvertTime(0, 36, 0)));
				}
				else
				{
					Scene scene3 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 20, 2));
					scene3.AddCrossFade(ConvertTime(0, 10, 2), 30);
					playList.Add(scene3);
					Scene scene4 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand2, ConvertTime(0, 21, 21), ConvertTime(0, 22, 10), ConvertTime(0, 41, 2));
					scene4.AddCrossFade(ConvertTime(0, 31, 15), 20);
					playList.Add(scene4);
					playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand3, ConvertTime(0, 42, 21), ConvertTime(0, 43, 10), ConvertTime(0, 59, 0)));
					playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 61, 0), ConvertTime(0, 61, 0), ConvertTime(0, 63, 0)));
				}
				for (int num9 = 1; num9 < playList.Count; num9++)
				{
					playList[num9].startAt -= 1.1f;
					playList[num9].fadeInDoneAt -= 1.1f;
					playList[num9].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
		}
		preloadedScenes = new SlideShowMenuScene[playList.Count];
		if (Application.platform != RuntimePlatform.Switch)
		{
			for (int num10 = 0; num10 < preloadedScenes.Length; num10++)
			{
				preloadedScenes[num10] = new SlideShowMenuScene(this, pages[0], playList[num10].sceneID);
				preloadedScenes[num10].Hide();
			}
		}
		manager.RemoveLoadingLabel();
		NextScene();
	}

	private float ConvertTime(int minutes, int seconds, int pps)
	{
		return (float)minutes * 60f + (float)seconds + (float)pps / 100f;
	}

	public override void RawUpdate(float dt)
	{
		base.RawUpdate(dt);
		if (RWInput.CheckPauseButton(0, inMenu: false) && !lastPauseButton)
		{
			skip = true;
			manager.RequestMainProcessSwitch(processAfterSlideShow);
			if (manager.musicPlayer != null && manager.musicPlayer.song != null)
			{
				manager.musicPlayer.FadeOutAllSongs(40f);
			}
			return;
		}
		if (waitForMusic != null && stall && manager.musicPlayer.song != null && manager.musicPlayer.song.name == waitForMusic)
		{
			stall = false;
		}
		if (preloadedScenes[current].moveEditor != null)
		{
			stall = true;
		}
		if (!stall)
		{
			time += dt;
		}
		float num = ((current >= playList.Count - 1) ? (playList[current].fadeOutStartAt + 1f) : playList[current + 1].startAt);
		inSceneTime = Mathf.InverseLerp(playList[current].startAt, num, time);
		if (!skip)
		{
			manager.fadeToBlack = Math.Max(Mathf.InverseLerp(playList[current].fadeInDoneAt, playList[current].startAt, time), Mathf.InverseLerp(playList[current].fadeOutStartAt, num, time));
		}
		if (crossfadeInd < playList[current].crossfades.Count && time >= playList[current].crossfades[crossfadeInd])
		{
			preloadedScenes[current].TriggerCrossfade(playList[current].crossfadeDurations[crossfadeInd]);
			crossfadeInd++;
		}
		if (time >= num)
		{
			if (current < playList.Count - 1)
			{
				NextScene();
			}
			else
			{
				manager.RequestMainProcessSwitch(processAfterSlideShow);
			}
		}
	}

	private void NextScene()
	{
		inSceneTime = 0f;
		crossfadeInd = 0;
		if (current >= 0 && scene != null)
		{
			scene.RemoveSprites();
			pages[0].subObjects.Remove(scene);
			preloadedScenes[current].Hide();
			preloadedScenes[current].RemoveSprites();
			preloadedScenes[current].UnloadImages();
			pages[0].RemoveSprites();
			preloadedScenes[current] = null;
		}
		current++;
		if (current < playList.Count)
		{
			if (Application.platform == RuntimePlatform.Switch)
			{
				preloadedScenes[current] = new SlideShowMenuScene(this, pages[0], playList[current].sceneID);
			}
			scene = preloadedScenes[current];
			pages[0].subObjects.Add(scene);
			preloadedScenes[current].Show();
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is EndCredits && (slideShowID == SlideShowID.WhiteOutro || slideShowID == SlideShowID.YellowOutro))
		{
			manager.CueAchievement(RainWorld.AchievementID.Win, 5f);
		}
		else if (nextProcess is EndCredits && slideShowID == SlideShowID.RedOutro)
		{
			manager.CueAchievement(RainWorld.AchievementID.HunterWin, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && (slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro))
		{
			manager.CueAchievement(RainWorld.AchievementID.Win, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerAltEnd))
		{
			manager.CueAchievement(RainWorld.AchievementID.ArtificerEnding, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletAltEnd)
		{
			manager.CueAchievement(RainWorld.AchievementID.RivuletEnding, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandAltEnd)
		{
			manager.CueAchievement(RainWorld.AchievementID.GourmandEnding, 5f);
		}
		else if (!ModManager.MSC && nextProcess is StoryGameStatisticsScreen)
		{
			(nextProcess as StoryGameStatisticsScreen).GetDataFromGame(endGameStatsPackage);
			(nextProcess as StoryGameStatisticsScreen).forceWatch = true;
		}
		if (ModManager.MSC)
		{
			if (ModManager.MSC && nextProcess is EndCredits)
			{
				(nextProcess as EndCredits).passthroughPackage = passthroughPackage;
			}
			if (nextProcess is DatingSim)
			{
				(nextProcess as DatingSim).passthroughPackage = passthroughPackage;
			}
			if (passthroughPackage != null && nextProcess is KarmaLadderScreen)
			{
				(nextProcess as KarmaLadderScreen).GetDataFromGame(passthroughPackage);
			}
		}
	}
}
