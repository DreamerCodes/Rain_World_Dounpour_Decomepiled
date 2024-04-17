using Menu;

namespace Expedition;

public static class ExpeditionEnums
{
	public class ProcessID
	{
		public static ProcessManager.ProcessID ExpeditionMenu;

		public static ProcessManager.ProcessID ExpeditionGameOver;

		public static ProcessManager.ProcessID ExpeditionWinScreen;

		public static ProcessManager.ProcessID ExpeditionJukebox;

		public static void RegisterValues()
		{
			ExpeditionMenu = new ProcessManager.ProcessID("ExpeditionMenu", register: true);
			ExpeditionGameOver = new ProcessManager.ProcessID("ExpeditionGameOver", register: true);
			ExpeditionWinScreen = new ProcessManager.ProcessID("ExpeditionWinScreen", register: true);
			ExpeditionJukebox = new ProcessManager.ProcessID("ExpeditionJukebox", register: true);
		}

		public static void UnregisterValues()
		{
			if (ExpeditionMenu != null)
			{
				ExpeditionMenu.Unregister();
				ExpeditionMenu = null;
			}
			if (ExpeditionGameOver != null)
			{
				ExpeditionGameOver.Unregister();
				ExpeditionGameOver = null;
			}
			if (ExpeditionWinScreen != null)
			{
				ExpeditionWinScreen.Unregister();
				ExpeditionWinScreen = null;
			}
			if (ExpeditionJukebox != null)
			{
				ExpeditionJukebox.Unregister();
				ExpeditionJukebox = null;
			}
		}
	}

	public class SliderID
	{
		public static Slider.SliderID ChallengeDifficulty;

		public static Slider.SliderID Playback;

		public static void RegisterValues()
		{
			ChallengeDifficulty = new Slider.SliderID("ChallengeDifficulty", register: true);
			Playback = new Slider.SliderID("Playback", register: true);
		}

		public static void UnregisterValues()
		{
			if (ChallengeDifficulty != null)
			{
				ChallengeDifficulty.Unregister();
				ChallengeDifficulty = null;
			}
			if (Playback != null)
			{
				Playback.Unregister();
				Playback = null;
			}
		}
	}

	public static void InitExtEnumTypes()
	{
		_ = ScoreCalculatorPhase.Setup;
	}

	public static void RegisterAllEnumExtensions()
	{
		ProcessID.RegisterValues();
		SliderID.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		ProcessID.UnregisterValues();
		SliderID.UnregisterValues();
	}
}
