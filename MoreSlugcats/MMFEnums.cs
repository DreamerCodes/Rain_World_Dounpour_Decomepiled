using Menu;
using OverseerHolograms;

namespace MoreSlugcats;

public class MMFEnums
{
	public class ProcessID
	{
		public static ProcessManager.ProcessID Tips;

		public static ProcessManager.ProcessID BackgroundOptions;

		public static void RegisterValues()
		{
			Tips = new ProcessManager.ProcessID("Tips", register: true);
			BackgroundOptions = new ProcessManager.ProcessID("BackgroundOptions", register: true);
		}

		public static void UnregisterValues()
		{
			Tips?.Unregister();
			Tips = null;
			BackgroundOptions?.Unregister();
			BackgroundOptions = null;
		}
	}

	public class SliderID
	{
		public static Slider.SliderID FPSLimit;

		public static Slider.SliderID AnalogSensitivity;

		public static Slider.SliderID Hue;

		public static Slider.SliderID Saturation;

		public static Slider.SliderID Lightness;

		public static void RegisterValues()
		{
			FPSLimit = new Slider.SliderID("FPSLimit", register: true);
			AnalogSensitivity = new Slider.SliderID("AnalogSensitivity", register: true);
			Hue = new Slider.SliderID("Hue", register: true);
			Saturation = new Slider.SliderID("Saturation", register: true);
			Lightness = new Slider.SliderID("Lightness", register: true);
		}

		public static void UnregisterValues()
		{
			FPSLimit?.Unregister();
			FPSLimit = null;
			AnalogSensitivity?.Unregister();
			AnalogSensitivity = null;
			Hue?.Unregister();
			Hue = null;
			Saturation?.Unregister();
			Saturation = null;
			Lightness?.Unregister();
			Lightness = null;
		}
	}

	public class Tutorial
	{
		public static DeathPersistentSaveData.Tutorial GateStand;

		public static void RegisterValues()
		{
			GateStand = new DeathPersistentSaveData.Tutorial("GateStand", register: true);
		}

		public static void UnregisterValues()
		{
			GateStand?.Unregister();
			GateStand = null;
		}
	}

	public class LizardVoiceEmotion
	{
		public static LizardVoice.Emotion Love;

		public static void RegisterValues()
		{
			Love = new LizardVoice.Emotion("Love", register: true);
		}

		public static void UnregisterValues()
		{
			Love?.Unregister();
			Love = null;
		}
	}

	public class PlayerConcern
	{
		public static OverseerCommunicationModule.PlayerConcern ProtectMoon;

		public static void RegisterValues()
		{
			ProtectMoon = new OverseerCommunicationModule.PlayerConcern("ProtectMoon", register: true);
		}

		public static void UnregisterValues()
		{
			ProtectMoon?.Unregister();
			ProtectMoon = null;
		}
	}

	public class OverseerHologramMessage
	{
		public static OverseerHologram.Message TutorialGate;

		public static void RegisterValues()
		{
			TutorialGate = new OverseerHologram.Message("TutorialGate", register: true);
		}

		public static void UnregisterValues()
		{
			TutorialGate?.Unregister();
			TutorialGate = null;
		}
	}

	public class MMFSoundID
	{
		public static SoundID Lizard_Love_Voice_Cyan_A;

		public static SoundID Lizard_Love_Voice_Cyan_B;

		public static SoundID Lizard_Love_Voice_Cyan_C;

		public static SoundID Lizard_Love_Voice_Gen_A;

		public static SoundID Lizard_Love_Voice_Gen_B;

		public static SoundID Lizard_Love_Voice_Red_A;

		public static SoundID Lizard_Love_Voice_Red_B;

		public static SoundID Lizard_Love_Voice_Red_C;

		public static SoundID Lizard_Voice_Black_A;

		public static SoundID Lizard_Voice_Caramel_A;

		public static SoundID Lizard_Voice_Cyan_A;

		public static SoundID Lizard_Voice_Cyan_B;

		public static SoundID Lizard_Voice_Cyan_C;

		public static SoundID Lizard_Voice_Eel_A;

		public static SoundID Lizard_Voice_Eel_B;

		public static SoundID Lizard_Voice_Salamander_A;

		public static SoundID Lizard_Voice_Salamander_B;

		public static SoundID Lizard_Voice_White_A;

		public static SoundID Lizard_Voice_White_B;

		public static SoundID Lizard_Voice_White_C;

		public static SoundID Lizard_Voice_Yellow_A;

		public static SoundID Lizard_Voice_Red_A;

		public static SoundID Tick;

		public static SoundID Tock;

		public static void RegisterValues()
		{
			Lizard_Love_Voice_Cyan_A = new SoundID("Lizard_Love_Voice_Cyan_A", register: true);
			Lizard_Love_Voice_Cyan_B = new SoundID("Lizard_Love_Voice_Cyan_B", register: true);
			Lizard_Love_Voice_Cyan_C = new SoundID("Lizard_Love_Voice_Cyan_C", register: true);
			Lizard_Love_Voice_Gen_A = new SoundID("Lizard_Love_Voice_Gen_A", register: true);
			Lizard_Love_Voice_Gen_B = new SoundID("Lizard_Love_Voice_Gen_B", register: true);
			Lizard_Love_Voice_Red_A = new SoundID("Lizard_Love_Voice_Red_A", register: true);
			Lizard_Love_Voice_Red_B = new SoundID("Lizard_Love_Voice_Red_B", register: true);
			Lizard_Love_Voice_Red_C = new SoundID("Lizard_Love_Voice_Red_C", register: true);
			Lizard_Voice_Black_A = new SoundID("Lizard_Voice_Black_A", register: true);
			Lizard_Voice_Caramel_A = new SoundID("Lizard_Voice_Caramel_A", register: true);
			Lizard_Voice_Cyan_A = new SoundID("Lizard_Voice_Cyan_A", register: true);
			Lizard_Voice_Cyan_B = new SoundID("Lizard_Voice_Cyan_B", register: true);
			Lizard_Voice_Cyan_C = new SoundID("Lizard_Voice_Cyan_C", register: true);
			Lizard_Voice_Eel_A = new SoundID("Lizard_Voice_Eel_A", register: true);
			Lizard_Voice_Eel_B = new SoundID("Lizard_Voice_Eel_B", register: true);
			Lizard_Voice_Salamander_A = new SoundID("Lizard_Voice_Salamander_A", register: true);
			Lizard_Voice_Salamander_B = new SoundID("Lizard_Voice_Salamander_B", register: true);
			Lizard_Voice_White_A = new SoundID("Lizard_Voice_White_A", register: true);
			Lizard_Voice_White_B = new SoundID("Lizard_Voice_White_B", register: true);
			Lizard_Voice_White_C = new SoundID("Lizard_Voice_White_C", register: true);
			Lizard_Voice_Yellow_A = new SoundID("Lizard_Voice_Yellow_A", register: true);
			Lizard_Voice_Red_A = new SoundID("Lizard_Voice_Red_A", register: true);
			Tick = new SoundID("Tick", register: true);
			Tock = new SoundID("Tock", register: true);
		}

		public static void UnregisterValues()
		{
			Lizard_Love_Voice_Cyan_A?.Unregister();
			Lizard_Love_Voice_Cyan_A = null;
			Lizard_Love_Voice_Cyan_B?.Unregister();
			Lizard_Love_Voice_Cyan_B = null;
			Lizard_Love_Voice_Cyan_C?.Unregister();
			Lizard_Love_Voice_Cyan_C = null;
			Lizard_Love_Voice_Gen_A?.Unregister();
			Lizard_Love_Voice_Gen_A = null;
			Lizard_Love_Voice_Gen_B?.Unregister();
			Lizard_Love_Voice_Gen_B = null;
			Lizard_Love_Voice_Red_A?.Unregister();
			Lizard_Love_Voice_Red_A = null;
			Lizard_Love_Voice_Red_B?.Unregister();
			Lizard_Love_Voice_Red_B = null;
			Lizard_Love_Voice_Red_C?.Unregister();
			Lizard_Love_Voice_Red_C = null;
			Lizard_Voice_Black_A?.Unregister();
			Lizard_Voice_Black_A = null;
			Lizard_Voice_Caramel_A?.Unregister();
			Lizard_Voice_Caramel_A = null;
			Lizard_Voice_Cyan_A?.Unregister();
			Lizard_Voice_Cyan_A = null;
			Lizard_Voice_Cyan_B?.Unregister();
			Lizard_Voice_Cyan_B = null;
			Lizard_Voice_Cyan_C?.Unregister();
			Lizard_Voice_Cyan_C = null;
			Lizard_Voice_Eel_A?.Unregister();
			Lizard_Voice_Eel_A = null;
			Lizard_Voice_Eel_B?.Unregister();
			Lizard_Voice_Eel_B = null;
			Lizard_Voice_Salamander_A?.Unregister();
			Lizard_Voice_Salamander_A = null;
			Lizard_Voice_Salamander_B?.Unregister();
			Lizard_Voice_Salamander_B = null;
			Lizard_Voice_White_A?.Unregister();
			Lizard_Voice_White_A = null;
			Lizard_Voice_White_B?.Unregister();
			Lizard_Voice_White_B = null;
			Lizard_Voice_White_C?.Unregister();
			Lizard_Voice_White_C = null;
			Lizard_Voice_Yellow_A?.Unregister();
			Lizard_Voice_Yellow_A = null;
			Lizard_Voice_Red_A?.Unregister();
			Lizard_Voice_Red_A = null;
			Tick?.Unregister();
			Tick = null;
			Tock?.Unregister();
			Tock = null;
		}
	}

	public static void InitExtEnumTypes()
	{
		_ = Options.Quality.LOW;
		_ = WorldLoader.LoadingContext.FULL;
	}

	public static void RegisterAllEnumExtensions()
	{
		ProcessID.RegisterValues();
		SliderID.RegisterValues();
		Tutorial.RegisterValues();
		LizardVoiceEmotion.RegisterValues();
		PlayerConcern.RegisterValues();
		OverseerHologramMessage.RegisterValues();
		MMFSoundID.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		ProcessID.UnregisterValues();
		SliderID.UnregisterValues();
		Tutorial.UnregisterValues();
		LizardVoiceEmotion.UnregisterValues();
		PlayerConcern.UnregisterValues();
		OverseerHologramMessage.UnregisterValues();
		MMFSoundID.UnregisterValues();
	}
}
