using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DevInterface;
using Expedition;
using JollyCoop;
using Kittehface.Framework20;
using Menu;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

public class ProcessManager
{
	public class ProcessID : ExtEnum<ProcessID>
	{
		public static readonly ProcessID MainMenu = new ProcessID("MainMenu", register: true);

		public static readonly ProcessID Game = new ProcessID("Game", register: true);

		public static readonly ProcessID SleepScreen = new ProcessID("SleepScreen", register: true);

		public static readonly ProcessID DeathScreen = new ProcessID("DeathScreen", register: true);

		public static readonly ProcessID StarveScreen = new ProcessID("StarveScreen", register: true);

		public static readonly ProcessID RegionSelect = new ProcessID("RegionSelect", register: true);

		public static readonly ProcessID OptionsMenu = new ProcessID("OptionsMenu", register: true);

		public static readonly ProcessID MusicPlayer = new ProcessID("MusicPlayer", register: true);

		public static readonly ProcessID GhostScreen = new ProcessID("GhostScreen", register: true);

		public static readonly ProcessID KarmaToMaxScreen = new ProcessID("KarmaToMaxScreen", register: true);

		public static readonly ProcessID SlideShow = new ProcessID("SlideShow", register: true);

		public static readonly ProcessID MenuMic = new ProcessID("MenuMic", register: true);

		public static readonly ProcessID PauseMenu = new ProcessID("PauseMenu", register: true);

		public static readonly ProcessID FastTravelScreen = new ProcessID("FastTravelScreen", register: true);

		public static readonly ProcessID RegionsOverviewScreen = new ProcessID("RegionsOverviewScreen", register: true);

		public static readonly ProcessID CustomEndGameScreen = new ProcessID("CustomEndGameScreen", register: true);

		public static readonly ProcessID InputSelect = new ProcessID("InputSelect", register: true);

		public static readonly ProcessID TutorialControlsPage = new ProcessID("TutorialControlsPage", register: true);

		public static readonly ProcessID SlugcatSelect = new ProcessID("SlugcatSelect", register: true);

		public static readonly ProcessID IntroRoll = new ProcessID("IntroRoll", register: true);

		public static readonly ProcessID Credits = new ProcessID("Credits", register: true);

		public static readonly ProcessID ConsoleOptionsMenu = new ProcessID("ConsoleOptionsMenu", register: true);

		public static readonly ProcessID Dream = new ProcessID("Dream", register: true);

		public static readonly ProcessID RainWorldSteamManager = new ProcessID("RainWorldSteamManager", register: true);

		public static readonly ProcessID MultiplayerMenu = new ProcessID("MultiplayerMenu", register: true);

		public static readonly ProcessID MultiplayerResults = new ProcessID("MultiplayerResults", register: true);

		public static readonly ProcessID InputOptions = new ProcessID("InputOptions", register: true);

		public static readonly ProcessID Statistics = new ProcessID("Statistics", register: true);

		public static readonly ProcessID Dialog = new ProcessID("Dialog", register: true);

		public static readonly ProcessID ModdingMenu = new ProcessID("ModdingMenu", register: true);

		public static readonly ProcessID DemoModeEnd = new ProcessID("DemoModeEnd", register: true);

		public static readonly ProcessID Initialization = new ProcessID("Initialization", register: true);

		public static readonly ProcessID MonkActivity = new ProcessID("MonkActivity", register: true);

		public static readonly ProcessID BackupManager = new ProcessID("BackupManager", register: true);

		public ProcessID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private struct ProcessSwitchRequestData
	{
		public ProcessID ID;

		public float fadeOutSeconds;
	}

	public struct ShowDialogRequestData
	{
		public Dialog Dialog;
	}

	public class MenuSetup
	{
		public class StoryGameInitCondition : ExtEnum<StoryGameInitCondition>
		{
			public static readonly StoryGameInitCondition Dev = new StoryGameInitCondition("Dev", register: true);

			public static readonly StoryGameInitCondition New = new StoryGameInitCondition("New", register: true);

			public static readonly StoryGameInitCondition Load = new StoryGameInitCondition("Load", register: true);

			public static readonly StoryGameInitCondition RegionSelect = new StoryGameInitCondition("RegionSelect", register: true);

			public static readonly StoryGameInitCondition FastTravel = new StoryGameInitCondition("FastTravel", register: true);

			public static readonly StoryGameInitCondition StartWithFastTravel = new StoryGameInitCondition("StartWithFastTravel", register: true);

			public StoryGameInitCondition(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public StoryGameInitCondition startGameCondition;

		public string regionSelectRoom = "";

		public bool LoadInitCondition
		{
			get
			{
				if (!(startGameCondition == StoryGameInitCondition.Load))
				{
					return startGameCondition == StoryGameInitCondition.FastTravel;
				}
				return true;
			}
		}

		public bool FastTravelInitCondition
		{
			get
			{
				if (!(startGameCondition == StoryGameInitCondition.FastTravel))
				{
					return startGameCondition == StoryGameInitCondition.StartWithFastTravel;
				}
				return true;
			}
		}

		public MenuSetup(RainWorldGame.SetupValues fileSetup)
		{
			startGameCondition = (fileSetup.loadGame ? StoryGameInitCondition.Load : StoryGameInitCondition.Dev);
		}
	}

	public RainWorld rainWorld;

	public MainLoopProcess currentMainLoop;

	public MusicPlayer musicPlayer;

	public ThreatDetermination fallbackThreatDetermination;

	public MenuMicrophone menuMic;

	public List<MainLoopProcess> sideProcesses;

	public List<ModManager.Mod> enableModsOnProcessSwitch;

	public List<ModManager.Mod> disableModsOnProcessSwitch;

	private MainLoopProcess oldProcess;

	private bool lastMultiplayerContext;

	public static bool fontHasBeenLoaded;

	public static bool initializationAndIntroRollStarted;

	public static string activityID;

	public bool menuesMouseMode;

	public ProcessID upcomingProcess;

	public ProcessID processAfterModFinalization;

	public bool modFinalizationDone;

	public SlideShow.SlideShowID nextSlideshow;

	public SoundLoader soundLoader;

	public float shadersTime;

	private float blackDelay;

	private float blackFadeTime = 0.45f;

	public float fadeToBlack;

	public FSprite fadeSprite;

	private FLabel loadingLabel;

	private FLabel finalizeModsLabel;

	private FLabel demoTimerLabel;

	private FLabel validationLabel;

	private int finalizeModsStep;

	private int finalizeModsDelay;

	private string[] recomputeTokenRegions;

	private int recomputeTokenIndex;

	private bool pauseFadeUpdate;

	public MenuSetup menuSetup;

	internal RainWorldSteamManager mySteamManager;

	public ArenaSetup arenaSetup;

	public ArenaSitting arenaSitting;

	private List<Dialog> dialogStack = new List<Dialog>();

	private bool requestPlayMusic;

	public KarmaLadderScreen.SleepDeathScreenDataPackage dataBeforeArtificerDream;

	public int artificerDreamNumber;

	public bool pebblesHasHalcyon;

	public SlugcatStats.Name sceneSlot;

	public SlugcatStats.Name slugcatLeaving;

	public ProcessID specialUnlockDestination;

	public string specialUnlockText;

	public bool fakeGlitchedEnding;

	public bool statsAfterCredits;

	public bool foodTrackerCompletedThisSession;

	public string desiredCreditsSong;

	private List<Thread> buildTokenCacheThreads = new List<Thread>();

	private readonly Queue<ProcessSwitchRequestData> _processSwitchQueue = new Queue<ProcessSwitchRequestData>();

	private readonly Queue<ShowDialogRequestData> _showDialogQueue = new Queue<ShowDialogRequestData>();

	public RainWorld.AchievementID waitingAchievement;

	public float waitingAchievementDelay;

	public RainWorld.AchievementID waitingAchievementGOG;

	public float waitingAchievementGOGDelay;

	private bool _switchingProcess;

	public Dialog dialog { get; private set; }

	public bool FadeDelayInProgress => blackDelay > 0f;

	public bool IsRunningAnyDialog
	{
		get
		{
			if (dialog == null)
			{
				return dialogStack.Count > 0;
			}
			return true;
		}
	}

	public void StopSideProcess(MainLoopProcess process)
	{
		for (int num = sideProcesses.Count - 1; num >= 0; num--)
		{
			if (sideProcesses[num] == process)
			{
				sideProcesses.RemoveAt(num);
			}
		}
		process.ShutDownProcess();
		if (process == musicPlayer)
		{
			musicPlayer = null;
		}
		else if (process == menuMic)
		{
			menuMic = null;
		}
		else if (process == mySteamManager)
		{
			mySteamManager = null;
		}
		else if (process == dialog)
		{
			dialog = null;
			ShowNextDialog();
		}
	}

	public static void ReadActivity(Profiles.Profile profile, string activityName)
	{
		activityID = activityName;
		Custom.Log("ReadActivity ran with:", profile.GetDisplayName(), "and activity of:", activityName);
	}

	public void ShowDialog(Dialog dialog)
	{
		if (dialog != null)
		{
			if (!fontHasBeenLoaded || IsSwitchingProcesses())
			{
				dialog.HackHide();
				_showDialogQueue.Enqueue(new ShowDialogRequestData
				{
					Dialog = dialog
				});
			}
			else
			{
				ActualShowDialog(dialog);
			}
		}
	}

	public void ActualShowDialog(Dialog dialog)
	{
		if (dialog == null)
		{
			return;
		}
		if ((currentMainLoop == null || currentMainLoop.AllowDialogs) && loadingLabel == null)
		{
			if (currentMainLoop != null && currentMainLoop is global::Menu.Menu)
			{
				dialog.mySoundLoopID = (currentMainLoop as global::Menu.Menu).mySoundLoopID;
				dialog.mySoundLoopName = (currentMainLoop as global::Menu.Menu).mySoundLoopName;
			}
			if (this.dialog == null)
			{
				this.dialog = dialog;
				sideProcesses.Add(dialog);
				return;
			}
			dialogStack.Add(this.dialog);
			sideProcesses.Remove(this.dialog);
			this.dialog = dialog;
			sideProcesses.Add(dialog);
		}
		else
		{
			dialogStack.Add(dialog);
		}
	}

	public ProcessManager(RainWorld rainWorld)
	{
		this.rainWorld = rainWorld;
		sideProcesses = new List<MainLoopProcess>();
		soundLoader = null;
		menuSetup = new MenuSetup(rainWorld.setup);
		mySteamManager = new RainWorldSteamManager(this);
		if (!mySteamManager.shutdown)
		{
			sideProcesses.Add(mySteamManager);
		}
		else
		{
			mySteamManager = null;
		}
		requestPlayMusic = rainWorld.setup.playMusic;
		sceneSlot = null;
		artificerDreamNumber = -1;
		specialUnlockText = "";
		fakeGlitchedEnding = false;
		desiredCreditsSong = "RW_8 - Sundown";
		slugcatLeaving = null;
		InitFadeSprite();
		fadeToBlack = 1f;
		PreSwitchMainProcess(ProcessID.Initialization);
		PostSwitchMainProcess(ProcessID.Initialization);
	}

	public void InitSoundLoader()
	{
		soundLoader = new SoundLoader(rainWorld.setup.loadAllAmbientSounds, rainWorld);
	}

	public void BuildTokenCacheThread(object index)
	{
		int num = (int)index;
		if (num < recomputeTokenRegions.Length)
		{
			rainWorld.BuildTokenCache(modded: true, recomputeTokenRegions[num]);
		}
	}

	public bool AreBuildTokenCacheThreadsDead()
	{
		foreach (Thread buildTokenCacheThread in buildTokenCacheThreads)
		{
			if (buildTokenCacheThread.IsAlive)
			{
				return false;
			}
		}
		return true;
	}

	public void Update(float deltaTime)
	{
		if (fontHasBeenLoaded && _showDialogQueue.Count > 0 && !IsSwitchingProcesses())
		{
			ShowDialogRequestData showDialogRequestData = _showDialogQueue.Dequeue();
			showDialogRequestData.Dialog.HackShow();
			ActualShowDialog(showDialogRequestData.Dialog);
		}
		if (_processSwitchQueue.Count > 0 && !IsRunningAnyDialog)
		{
			ProcessSwitchRequestData processSwitchRequestData = _processSwitchQueue.Dequeue();
			ActualProcessSwitch(processSwitchRequestData.ID, processSwitchRequestData.fadeOutSeconds);
		}
		if (requestPlayMusic && rainWorld.OptionsReady)
		{
			requestPlayMusic = false;
			if (rainWorld.options.musicVolume > 0f)
			{
				musicPlayer = new MusicPlayer(this);
				sideProcesses.Add(musicPlayer);
				fallbackThreatDetermination = null;
			}
			else if (ModManager.MMF && MMF.cfgThreatMusicPulse != null && MMF.cfgThreatMusicPulse.Value && fallbackThreatDetermination == null)
			{
				fallbackThreatDetermination = new ThreatDetermination(0);
			}
		}
		if (ModManager.MSC && currentMainLoop != null && rainWorld.progression != null && rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].clip != null && array[i].pitch != 0.5f && (array[i].clip.name.StartsWith("RW_") || array[i].clip.name.StartsWith("NA_") || currentMainLoop.ID == ProcessID.IntroRoll) && currentMainLoop.ID != ProcessID.SlideShow)
				{
					array[i].pitch = 0.5f;
				}
			}
		}
		if (initializationAndIntroRollStarted)
		{
			switch (activityID)
			{
			case "CharacterSelect":
				RequestMainProcessSwitch(ProcessID.SlugcatSelect);
				activityID = null;
				break;
			case "ArenaMode":
				RequestMainProcessSwitch(ProcessID.MultiplayerMenu);
				activityID = null;
				break;
			case "monk":
				if (rainWorld.progression != null && rainWorld.progression.progressionLoaded && rainWorld.inGameSlugCat != SlugcatStats.Name.Yellow)
				{
					PreSwitchMainProcess(ProcessID.SlugcatSelect);
					currentMainLoop = new SlugcatSelectMenu(this);
					((SlugcatSelectMenu)currentMainLoop).StartGame(SlugcatStats.Name.Yellow);
				}
				activityID = null;
				break;
			case "surv":
				if (rainWorld.progression != null && rainWorld.progression.progressionLoaded && rainWorld.inGameSlugCat != SlugcatStats.Name.White)
				{
					PreSwitchMainProcess(ProcessID.SlugcatSelect);
					currentMainLoop = new SlugcatSelectMenu(this);
					((SlugcatSelectMenu)currentMainLoop).StartGame(SlugcatStats.Name.White);
				}
				activityID = null;
				break;
			}
		}
		if (currentMainLoop != null)
		{
			shadersTime += deltaTime * currentMainLoop.TimeSpeedFac;
			Shader.SetGlobalFloat(RainWorld.ShadPropRain, shadersTime / 5f);
			currentMainLoop.RawUpdate(deltaTime);
		}
		for (int j = 0; j < sideProcesses.Count; j++)
		{
			sideProcesses[j].RawUpdate(deltaTime);
		}
		if (soundLoader != null)
		{
			soundLoader.Update();
		}
		bool flag = IsGameInMultiplayerContext();
		lastMultiplayerContext = flag;
		if (processAfterModFinalization != null && !modFinalizationDone)
		{
			if (disableModsOnProcessSwitch != null && enableModsOnProcessSwitch != null)
			{
				if (finalizeModsDelay > 0)
				{
					finalizeModsDelay--;
				}
				else if (finalizeModsStep == 0)
				{
					ModManager.WrapModInitHooks();
					Action<string> onIssue = delegate(string restartText)
					{
						processAfterModFinalization = null;
						_switchingProcess = false;
						ShowDialog(new WrappedDialogBoxNotify(this, restartText, delegate
						{
							Application.Quit();
						}));
					};
					try
					{
						rainWorld.UnloadResources();
					}
					catch (Exception ex)
					{
						Custom.LogWarning("EXCEPTION IN UnloadResources", ex.Message, "::", ex.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.LoadResources();
					}
					catch (Exception ex2)
					{
						Custom.LogWarning("EXCEPTION IN LoadResources", ex2.Message, "::", ex2.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.PreModsDisabledEnabled();
					}
					catch (Exception ex3)
					{
						Custom.LogWarning("EXCEPTION IN PreModsDisabledEnabled", ex3.Message, "::", ex3.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.OnModsDisabled(disableModsOnProcessSwitch.ToArray());
					}
					catch (Exception ex4)
					{
						Custom.LogWarning("EXCEPTION IN OnModsDisabled", ex4.Message, "::", ex4.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.PreModsInit();
					}
					catch (Exception ex5)
					{
						Custom.LogWarning("EXCEPTION IN PreModsInit", ex5.Message, "::", ex5.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.OnModsInit();
					}
					catch (Exception ex6)
					{
						Custom.LogWarning("EXCEPTION IN OnModsInit", ex6.Message, "::", ex6.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					try
					{
						rainWorld.PostModsInit();
					}
					catch (Exception ex7)
					{
						Custom.LogWarning("EXCEPTION IN PostModsInit", ex7.Message, "::", ex7.StackTrace);
					}
					if (ModManager.CheckInitIssues(onIssue))
					{
						return;
					}
					CleanSlate();
					recomputeTokenRegions = null;
					recomputeTokenIndex = 0;
					finalizeModsStep++;
				}
				else if (finalizeModsStep == 1)
				{
					if (ModManager.ModdingEnabled)
					{
						if (recomputeTokenRegions == null)
						{
							string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + "recomputetokencache.txt");
							if (File.Exists(path))
							{
								rainWorld.ClearTokenCacheInMemory();
								recomputeTokenRegions = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt"));
								File.Delete(path);
								finalizeModsDelay = 4;
							}
							else
							{
								finalizeModsStep++;
							}
						}
						else
						{
							if (recomputeTokenIndex < recomputeTokenRegions.Length)
							{
								if (buildTokenCacheThreads.Count == 0)
								{
									int num = 6;
									for (int k = 0; k < num; k++)
									{
										if (recomputeTokenIndex >= recomputeTokenRegions.Length)
										{
											break;
										}
										Thread thread = new Thread(BuildTokenCacheThread);
										buildTokenCacheThreads.Add(thread);
										thread.Start(recomputeTokenIndex);
										recomputeTokenIndex++;
									}
								}
								for (int l = 0; l < buildTokenCacheThreads.Count; l++)
								{
									if (!buildTokenCacheThreads[l].IsAlive)
									{
										buildTokenCacheThreads[l] = new Thread(BuildTokenCacheThread);
										buildTokenCacheThreads[l].Start(recomputeTokenIndex);
										recomputeTokenIndex++;
									}
								}
							}
							if (recomputeTokenIndex >= recomputeTokenRegions.Length && AreBuildTokenCacheThreadsDead())
							{
								buildTokenCacheThreads.Clear();
								finalizeModsLabel.text = rainWorld.inGameTranslator.Translate("mod_menu_finalize");
								finalizeModsStep++;
							}
							finalizeModsDelay = 4;
						}
						if (recomputeTokenRegions != null && recomputeTokenIndex < recomputeTokenRegions.Length)
						{
							finalizeModsLabel.text = rainWorld.inGameTranslator.Translate("mod_menu_finalize") + Environment.NewLine + rainWorld.inGameTranslator.Translate("Building token cache") + ": " + recomputeTokenRegions[recomputeTokenIndex] + " (" + ((double)((float)recomputeTokenIndex / (float)recomputeTokenRegions.Length) * 100.0).ToString("0") + "%)";
						}
					}
					else
					{
						finalizeModsStep++;
					}
				}
				else if (finalizeModsStep == 2)
				{
					finalizeModsLabel.text = rainWorld.inGameTranslator.Translate("Reloading player progression");
					rainWorld.progression.Destroy();
					rainWorld.progression = new PlayerProgression(rainWorld, tryLoad: true, saveAfterLoad: false);
					finalizeModsStep++;
				}
				else if (finalizeModsStep == 3)
				{
					if (rainWorld.progression.progressionLoaded)
					{
						finalizeModsStep++;
					}
				}
				else if (finalizeModsStep == 4)
				{
					rainWorld.OnModsEnabled(enableModsOnProcessSwitch.ToArray());
					rainWorld.PostModsDisabledEnabled();
					rainWorld.LoadModResourcesCheck = true;
					rainWorld.inGameTranslator.loadedAOC = false;
					disableModsOnProcessSwitch = null;
					enableModsOnProcessSwitch = null;
					modFinalizationDone = true;
				}
			}
			else
			{
				modFinalizationDone = true;
			}
		}
		ShowNextDialog();
		if (currentMainLoop != null && (currentMainLoop.ID == ProcessID.SlideShow || currentMainLoop.ID == ProcessID.Dream) && upcomingProcess == null && processAfterModFinalization == null)
		{
			UpdateFade();
		}
		else if (pauseFadeUpdate)
		{
			pauseFadeUpdate = false;
		}
		else if (upcomingProcess != null)
		{
			fadeToBlack += deltaTime / blackFadeTime;
			UpdateFade();
			if (fadeToBlack > 1f)
			{
				fadeToBlack = 1f;
				if (dialog == null && !Platform.systemMenuShowing && !rainWorld.progression.SaveDataBusy)
				{
					PreSwitchMainProcess(upcomingProcess);
					finalizeModsStep = 0;
					finalizeModsDelay = 0;
					modFinalizationDone = false;
					processAfterModFinalization = upcomingProcess;
					upcomingProcess = null;
				}
			}
		}
		else if (processAfterModFinalization != null)
		{
			UpdateFade();
			if (modFinalizationDone)
			{
				PostSwitchMainProcess(processAfterModFinalization);
				processAfterModFinalization = null;
			}
		}
		else if (fadeToBlack > 0f)
		{
			if (blackDelay > 0f)
			{
				blackDelay -= deltaTime;
			}
			else
			{
				fadeToBlack -= deltaTime / blackFadeTime;
			}
			UpdateFade();
			if (fadeToBlack < 0f)
			{
				fadeToBlack = 0f;
				fadeSprite.RemoveFromContainer();
				fadeSprite = null;
				if (loadingLabel != null)
				{
					loadingLabel.RemoveFromContainer();
					loadingLabel = null;
				}
				if (finalizeModsLabel != null)
				{
					finalizeModsLabel.RemoveFromContainer();
					finalizeModsLabel = null;
				}
				if (validationLabel != null)
				{
					validationLabel.RemoveFromContainer();
					validationLabel = null;
				}
			}
		}
		if (waitingAchievement != 0)
		{
			waitingAchievementDelay -= deltaTime;
			if (waitingAchievementDelay < 0f)
			{
				rainWorld.PingAchievement(waitingAchievement);
				waitingAchievement = RainWorld.AchievementID.None;
			}
		}
		if (waitingAchievementGOG != 0)
		{
			waitingAchievementGOGDelay -= deltaTime;
			if (waitingAchievementGOGDelay < 0f)
			{
				rainWorld.PingAchievementGOG(waitingAchievementGOG);
				waitingAchievementGOG = RainWorld.AchievementID.None;
			}
		}
		if (fallbackThreatDetermination != null)
		{
			if (currentMainLoop is RainWorldGame)
			{
				RainWorldGame game = currentMainLoop as RainWorldGame;
				fallbackThreatDetermination.Update(game);
			}
			else
			{
				fallbackThreatDetermination.currentMusicAgnosticThreat = 0f;
				fallbackThreatDetermination.currentThreat = 0f;
			}
		}
	}

	private void CleanSlate()
	{
		StaticWorld.InitStaticWorld();
		rainWorld.inGameTranslator.shortStrings.Clear();
		InitSoundLoader();
		menuSetup = new MenuSetup(rainWorld.setup);
		rainWorld.progression.ReloadRegionsList();
		rainWorld.progression.ReloadLocksList();
		rainWorld.options.ReapplyUnrecognized();
		arenaSetup = null;
		arenaSitting = null;
		sceneSlot = null;
		artificerDreamNumber = -1;
		specialUnlockText = "";
		fakeGlitchedEnding = false;
		desiredCreditsSong = "RW_8 - Sundown";
		slugcatLeaving = null;
	}

	public void RequestMainProcessSwitch(ProcessID ID)
	{
		RequestMainProcessSwitch(ID, 0.45f);
	}

	public void RequestMainProcessSwitch(ProcessID ID, float fadeOutSeconds)
	{
		if (IsRunningAnyDialog)
		{
			_processSwitchQueue.Enqueue(new ProcessSwitchRequestData
			{
				ID = ID,
				fadeOutSeconds = fadeOutSeconds
			});
		}
		else
		{
			ActualProcessSwitch(ID, fadeOutSeconds);
		}
	}

	private void ActualProcessSwitch(ProcessID ID, float fadeOutSeconds)
	{
		if (!(upcomingProcess != null) && !(processAfterModFinalization != null))
		{
			blackFadeTime = fadeOutSeconds;
			upcomingProcess = ID;
			rainWorld.options.ResetJollyProfileRequest();
			InitFadeSprite();
		}
	}

	private void PreSwitchMainProcess(ProcessID ID)
	{
		_switchingProcess = true;
		rainWorld.setup = RainWorld.LoadSetupValues(rainWorld.buildType == RainWorld.BuildType.Distribution && !ModManager.DevTools);
		if (ModManager.MSC && ID == MoreSlugcatsEnums.ProcessID.RandomizedGame)
		{
			rainWorld.setup.randomStart = true;
			ID = ProcessID.Game;
		}
		if (currentMainLoop is RainWorldGame)
		{
			RainWorldGame rainWorldGame = currentMainLoop as RainWorldGame;
			rainWorldGame.abstractSpaceVisualizer.Visibility(visibility: false);
			rainWorldGame.console.Visibility(visibility: false);
			if (rainWorldGame.devUI != null)
			{
				rainWorldGame.devUI.ClearSprites();
			}
			if (rainWorldGame.AV != null)
			{
				rainWorldGame.AV.Destroy();
			}
			if (rainWorldGame.IsStorySession)
			{
				slugcatLeaving = rainWorldGame.GetStorySession.saveStateNumber;
			}
		}
		if (ModManager.Expedition && ID == ProcessID.MainMenu && rainWorld.options.saveSlot < 0)
		{
			int saveSlot = rainWorld.options.saveSlot;
			ExpLog.Log(saveSlot + " in use, returning to slot: " + (Math.Abs(rainWorld.options.saveSlot) - 1));
			rainWorld.options.saveSlot = Math.Abs(rainWorld.options.saveSlot) - 1;
			rainWorld.progression.Destroy(saveSlot);
			rainWorld.progression = new PlayerProgression(rainWorld, tryLoad: true, saveAfterLoad: true);
		}
		shadersTime = 0f;
		if (ID == ProcessID.Game && menuMic != null)
		{
			menuMic = null;
			sideProcesses.Remove(menuMic);
		}
		else if (ID != ProcessID.Game && menuMic == null && soundLoader != null)
		{
			menuMic = new MenuMicrophone(this, soundLoader);
			sideProcesses.Add(menuMic);
		}
		oldProcess = currentMainLoop;
		if (currentMainLoop != null)
		{
			currentMainLoop.ShutDownProcess();
			currentMainLoop.processActive = false;
			currentMainLoop = null;
			if (soundLoader != null)
			{
				soundLoader.ReleaseAllUnityAudio();
			}
			AssetManager.HardCleanFutileAssets();
		}
		for (int i = 0; i < 4; i++)
		{
			PlayerHandler playerHandler = rainWorld.GetPlayerHandler(i);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.ResetHandler(isPlayerDead: false);
			}
		}
	}

	private void PostSwitchMainProcess(ProcessID ID)
	{
		if (ModManager.MSC && ID == MoreSlugcatsEnums.ProcessID.RandomizedGame)
		{
			ID = ProcessID.Game;
		}
		if (ID != ProcessID.Initialization && ID != ProcessID.SleepScreen && ID != ProcessID.GhostScreen && ID != ProcessID.DeathScreen && ID != ProcessID.KarmaToMaxScreen && ID != ProcessID.Dream && ID != ProcessID.StarveScreen && (!ModManager.MSC || (ID != MoreSlugcatsEnums.ProcessID.KarmaToMinScreen && ID != MoreSlugcatsEnums.ProcessID.VengeanceGhostScreen)))
		{
			rainWorld.progression.Revert();
		}
		if (ModManager.MSC && specialUnlockText != "" && ID != ProcessID.Credits && ID != ProcessID.SlideShow && ID != ProcessID.Statistics)
		{
			if (ID == MoreSlugcatsEnums.ProcessID.SpecialUnlock)
			{
				specialUnlockDestination = oldProcess.ID;
			}
			else
			{
				specialUnlockDestination = ID;
			}
			ID = MoreSlugcatsEnums.ProcessID.SpecialUnlock;
		}
		if (ID == ProcessID.Game)
		{
			currentMainLoop = new RainWorldGame(this);
		}
		else if (ID == ProcessID.Initialization)
		{
			currentMainLoop = new InitializationScreen(this);
		}
		else if (ID == ProcessID.MainMenu)
		{
			rainWorld.inGameSlugCat = null;
			currentMainLoop = new MainMenu(this, oldProcess != null && oldProcess.ID == ProcessID.IntroRoll);
		}
		else if (ID == ProcessID.SleepScreen || ID == ProcessID.DeathScreen || ID == ProcessID.StarveScreen)
		{
			currentMainLoop = new SleepAndDeathScreen(this, ID);
		}
		else if (ID == ProcessID.GhostScreen || ID == ProcessID.KarmaToMaxScreen || (ModManager.MSC && (ID == MoreSlugcatsEnums.ProcessID.KarmaToMinScreen || ID == MoreSlugcatsEnums.ProcessID.VengeanceGhostScreen)))
		{
			currentMainLoop = new GhostEncounterScreen(this, ID);
		}
		else if (ID == ProcessID.RegionSelect)
		{
			currentMainLoop = new Menu.RegionSelectMenu(this);
		}
		else if (ID == ProcessID.OptionsMenu)
		{
			currentMainLoop = new OptionsMenu(this);
		}
		else if (ID == ProcessID.SlideShow)
		{
			currentMainLoop = new SlideShow(this, nextSlideshow);
		}
		else if (ID == ProcessID.FastTravelScreen || ID == ProcessID.RegionsOverviewScreen)
		{
			currentMainLoop = new FastTravelScreen(this, ID);
		}
		else if (ID == ProcessID.CustomEndGameScreen)
		{
			currentMainLoop = new CustomEndGameScreen(this);
		}
		else if (ID == ProcessID.SlugcatSelect)
		{
			currentMainLoop = new SlugcatSelectMenu(this);
		}
		else if (ID == ProcessID.MonkActivity)
		{
			if (rainWorld.inGameSlugCat != SlugcatStats.Name.Yellow)
			{
				currentMainLoop = new SlugcatSelectMenu(this);
				((SlugcatSelectMenu)currentMainLoop).StartGame(SlugcatStats.Name.Yellow);
			}
		}
		else if (ID == ProcessID.IntroRoll)
		{
			currentMainLoop = new IntroRoll(this);
		}
		else if (ID == ProcessID.Credits)
		{
			currentMainLoop = new EndCredits(this);
		}
		else if (ID == ProcessID.Dream)
		{
			if (ModManager.MSC && slugcatLeaving == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				currentMainLoop = new ScribbleDreamScreen(this);
			}
			else
			{
				currentMainLoop = new DreamScreen(this);
			}
		}
		else if (ID == ProcessID.MultiplayerMenu)
		{
			currentMainLoop = new MultiplayerMenu(this);
		}
		else if (ID == ProcessID.MultiplayerResults)
		{
			currentMainLoop = new MultiplayerResults(this);
		}
		else if (ID == ProcessID.InputOptions)
		{
			currentMainLoop = new InputOptionsMenu(this);
		}
		else if (ID == ProcessID.Statistics)
		{
			currentMainLoop = new StoryGameStatisticsScreen(this);
		}
		else if (ID == ProcessID.ModdingMenu)
		{
			currentMainLoop = new ModdingMenu(this);
		}
		else if (ID == ProcessID.BackupManager)
		{
			currentMainLoop = new BackupManager(this);
		}
		if (ModManager.MSC)
		{
			if (ID == MoreSlugcatsEnums.ProcessID.DemoEnd)
			{
				currentMainLoop = new DemoEndScreen(this);
			}
			else if (ID == MoreSlugcatsEnums.ProcessID.DatingSim)
			{
				currentMainLoop = new DatingSim(this);
			}
			else if (ID == MoreSlugcatsEnums.ProcessID.Collections)
			{
				currentMainLoop = new CollectionsMenu(this);
			}
			else if (ID == MoreSlugcatsEnums.ProcessID.SpecialUnlock)
			{
				currentMainLoop = new SpecialUnlockScreen(this);
			}
		}
		if (ModManager.MMF)
		{
			if (ID == MMFEnums.ProcessID.Tips)
			{
				currentMainLoop = new TipScreen(this);
			}
			else if (ID == MMFEnums.ProcessID.BackgroundOptions)
			{
				currentMainLoop = new BackgroundOptionsMenu(this);
			}
		}
		if (ModManager.Expedition)
		{
			if (ID == ExpeditionEnums.ProcessID.ExpeditionMenu)
			{
				currentMainLoop = new ExpeditionMenu(this);
			}
			else if (ID == ExpeditionEnums.ProcessID.ExpeditionGameOver)
			{
				currentMainLoop = new ExpeditionGameOver(this);
				rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
			}
			else if (ID == ExpeditionEnums.ProcessID.ExpeditionWinScreen)
			{
				currentMainLoop = new ExpeditionWinScreen(this);
				rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
			}
			else if (ID == ExpeditionEnums.ProcessID.ExpeditionJukebox)
			{
				currentMainLoop = new ExpeditionJukebox(this);
			}
		}
		if (oldProcess != null)
		{
			oldProcess.CommunicateWithUpcomingProcess(currentMainLoop);
		}
		blackFadeTime = currentMainLoop.FadeInTime;
		blackDelay = currentMainLoop.InitialBlackSeconds;
		if (fadeSprite != null)
		{
			fadeSprite.RemoveFromContainer();
			Futile.stage.AddChild(fadeSprite);
		}
		if (loadingLabel != null)
		{
			loadingLabel.RemoveFromContainer();
			Futile.stage.AddChild(loadingLabel);
		}
		if (finalizeModsLabel != null)
		{
			finalizeModsLabel.RemoveFromContainer();
			Futile.stage.AddChild(finalizeModsLabel);
		}
		if (validationLabel != null)
		{
			validationLabel.RemoveFromContainer();
			if (rainWorld.options.validation)
			{
				Futile.stage.AddChild(validationLabel);
			}
		}
		if (musicPlayer != null)
		{
			musicPlayer.UpdateMusicContext(currentMainLoop);
		}
		pauseFadeUpdate = true;
		modFinalizationDone = true;
		processAfterModFinalization = null;
		_switchingProcess = false;
	}

	private void InitFadeSprite()
	{
		if (fadeSprite != null)
		{
			fadeSprite.RemoveFromContainer();
		}
		if (loadingLabel != null)
		{
			loadingLabel.RemoveFromContainer();
		}
		if (finalizeModsLabel != null)
		{
			finalizeModsLabel.RemoveFromContainer();
		}
		if (validationLabel != null)
		{
			validationLabel.RemoveFromContainer();
		}
		fadeSprite = new FSprite("Futile_White");
		fadeSprite.color = new Color(0f, 0f, 0f);
		fadeSprite.x = rainWorld.screenSize.x / 2f;
		fadeSprite.y = rainWorld.screenSize.y / 2f;
		fadeSprite.alpha = 0f;
		fadeSprite.shader = rainWorld.Shaders["EdgeFade"];
		Futile.stage.AddChild(fadeSprite);
		if (upcomingProcess != null && (upcomingProcess == ProcessID.Game || upcomingProcess == ProcessID.SlideShow))
		{
			loadingLabel = new FLabel(Custom.GetFont(), rainWorld.inGameTranslator.Translate("Loading..."));
			loadingLabel.x = 100.2f;
			loadingLabel.y = 50.2f;
			Futile.stage.AddChild(loadingLabel);
			if (rainWorld.options.validation)
			{
				CreateValidationLabel();
			}
		}
		if (disableModsOnProcessSwitch != null && enableModsOnProcessSwitch != null)
		{
			finalizeModsLabel = new FLabel(Custom.GetFont(), rainWorld.inGameTranslator.Translate("mod_menu_finalize") + Environment.NewLine + " ");
			finalizeModsLabel.alignment = FLabelAlignment.Center;
			finalizeModsLabel.x = (float)(int)(rainWorld.options.ScreenSize.x * 0.5f) + 0.2f;
			finalizeModsLabel.y = (float)(int)(rainWorld.options.ScreenSize.y * 0.5f) + 0.2f;
			Futile.stage.AddChild(finalizeModsLabel);
		}
	}

	private void UpdateFade()
	{
		if (fadeSprite == null)
		{
			InitFadeSprite();
		}
		fadeSprite.scaleX = (rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, fadeToBlack) + 2f) / 16f;
		fadeSprite.scaleY = (rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, fadeToBlack) + 2f) / 16f;
		fadeSprite.alpha = Mathf.InverseLerp(0f, 0.9f, fadeToBlack);
		if (loadingLabel != null)
		{
			loadingLabel.alpha = Mathf.InverseLerp(0.5f, 0.9f, fadeToBlack);
		}
		if (finalizeModsLabel != null)
		{
			finalizeModsLabel.alpha = Mathf.InverseLerp(0.5f, 0.9f, fadeToBlack);
		}
		if (validationLabel != null)
		{
			validationLabel.alpha = Mathf.InverseLerp(0.5f, 0.9f, fadeToBlack);
		}
	}

	private void ShowNextDialog()
	{
		if (dialog == null && dialogStack.Count > 0 && (currentMainLoop == null || currentMainLoop.AllowDialogs) && loadingLabel == null)
		{
			dialog = dialogStack[dialogStack.Count - 1];
			dialogStack.RemoveAt(dialogStack.Count - 1);
			sideProcesses.Add(dialog);
		}
	}

	private void CreateValidationLabel()
	{
		string text = "";
		if (arenaSitting == null && (!ModManager.MSC || !rainWorld.safariMode))
		{
			SlugcatStats.Name currentlySelectedSinglePlayerSlugcat = rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
			SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(this, currentlySelectedSinglePlayerSlugcat);
			if (saveGameData != null)
			{
				int num = ((currentlySelectedSinglePlayerSlugcat == SlugcatStats.Name.Red) ? (RedsIllness.RedsCycles(saveGameData.redsExtraCycles) - saveGameData.cycle) : saveGameData.cycle);
				text = text + saveGameData.shelterName + " C" + num + " " + saveGameData.karma + "/" + saveGameData.karmaCap + (saveGameData.karmaReinforced ? "*" : "") + " " + saveGameData.food;
				if (ModManager.MMF)
				{
					SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(currentlySelectedSinglePlayerSlugcat);
					if (campaignTimeTracker != null)
					{
						TimeSpan.FromSeconds((double)saveGameData.gameTimeAlive + (double)saveGameData.gameTimeDead);
						text = text + " (" + campaignTimeTracker.TotalFreeTimeSpan.GetIGTFormat(includeMilliseconds: true) + ")";
					}
				}
			}
			else
			{
				text += "New Game!";
			}
			text += " STEAM";
			text += " v1.9.15b";
			text += Environment.NewLine;
		}
		for (int i = 0; i < ModManager.ActiveMods.Count; i++)
		{
			ModManager.Mod mod = ModManager.ActiveMods[i];
			if (mod.id == DevTools.MOD_ID)
			{
				text = text + DevTools.ValidationString() + Environment.NewLine;
				continue;
			}
			if (mod.id == global::JollyCoop.JollyCoop.MOD_ID)
			{
				text = text + global::JollyCoop.JollyCoop.ValidationString() + Environment.NewLine;
				continue;
			}
			OptionInterface registeredOI = MachineConnector.GetRegisteredOI(mod.id);
			text = ((registeredOI == null) ? (text + mod.id + Environment.NewLine) : (text + registeredOI.ValidationString() + Environment.NewLine));
		}
		validationLabel = new FLabel(Custom.GetFont(), text);
		validationLabel.alignment = FLabelAlignment.Left;
		validationLabel.x = 100.2f - loadingLabel.textRect.width / 2f;
		validationLabel.y = rainWorld.options.ScreenSize.y - 50.2f - validationLabel.textRect.height / 2f;
		Futile.stage.AddChild(validationLabel);
	}

	public bool IsGameInMultiplayerContext()
	{
		if (currentMainLoop != null && currentMainLoop is RainWorldGame)
		{
			RainWorldGame rainWorldGame = currentMainLoop as RainWorldGame;
			if (rainWorldGame.IsStorySession)
			{
				if (ModManager.CoopAvailable && rainWorld.options.JollyPlayerCount > 1)
				{
					return true;
				}
			}
			else if (rainWorldGame.IsArenaSession)
			{
				if (ModManager.MSC && rainWorldGame.GetArenaGameSession.chMeta != null)
				{
					return false;
				}
				if (rainWorldGame.GetArenaGameSession.arenaSitting.players.Count > 1)
				{
					return true;
				}
			}
		}
		else if (currentMainLoop != null && (currentMainLoop is MultiplayerResults || ((currentMainLoop is ExpeditionMenu || currentMainLoop is SlugcatSelectMenu) && ModManager.JollyCoop)))
		{
			return true;
		}
		if (currentMainLoop != null && currentMainLoop is InputOptionsMenu)
		{
			return true;
		}
		return false;
	}

	public void RemoveLoadingLabel()
	{
		if (loadingLabel != null)
		{
			loadingLabel.RemoveFromContainer();
			loadingLabel = null;
		}
		if (validationLabel != null)
		{
			validationLabel.RemoveFromContainer();
			validationLabel = null;
		}
	}

	public bool IsSwitchingProcesses()
	{
		if (!(upcomingProcess != null) && loadingLabel == null && fadeToBlack == 0f)
		{
			return _switchingProcess;
		}
		return true;
	}

	public void CueAchievement(RainWorld.AchievementID ID, float delay)
	{
		if (ID != 0)
		{
			CueAchievementPlatform(ID, delay);
			if (GogGalaxyManager.IsInitialized())
			{
				CueAchievementGOG(ID, delay);
			}
		}
	}

	private void CueAchievementPlatform(RainWorld.AchievementID ID, float delay)
	{
		if (rainWorld.AchievementAlreadyDisplayed(ID))
		{
			return;
		}
		if (waitingAchievement != 0)
		{
			if (ID == waitingAchievement)
			{
				return;
			}
			rainWorld.PingAchievement(waitingAchievement);
		}
		if (delay == 0f)
		{
			rainWorld.PingAchievement(ID);
			return;
		}
		waitingAchievement = ID;
		waitingAchievementDelay = delay;
	}

	private void CueAchievementGOG(RainWorld.AchievementID ID, float delay)
	{
		if (rainWorld.AchievementAlreadyDisplayedGOG(ID))
		{
			return;
		}
		if (waitingAchievementGOG != 0)
		{
			if (ID == waitingAchievementGOG)
			{
				return;
			}
			rainWorld.PingAchievementGOG(waitingAchievement);
		}
		if (delay == 0f)
		{
			rainWorld.PingAchievementGOG(ID);
			return;
		}
		waitingAchievementGOG = ID;
		waitingAchievementGOGDelay = delay;
	}
}
