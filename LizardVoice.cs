using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LizardVoice : CreatureVoice
{
	public class Emotion : ExtEnum<Emotion>
	{
		public static readonly Emotion SpottedPreyFirstTime = new Emotion("SpottedPreyFirstTime", register: true);

		public static readonly Emotion ReSpottedPrey = new Emotion("ReSpottedPrey", register: true);

		public static readonly Emotion GeneralSmallNoise = new Emotion("GeneralSmallNoise", register: true);

		public static readonly Emotion Curious = new Emotion("Curious", register: true);

		public static readonly Emotion Dominance = new Emotion("Dominance", register: true);

		public static readonly Emotion Submission = new Emotion("Submission", register: true);

		public static readonly Emotion PainImpact = new Emotion("PainImpact", register: true);

		public static readonly Emotion PainIdle = new Emotion("PainIdle", register: true);

		public static readonly Emotion Frustration = new Emotion("Frustration", register: true);

		public static readonly Emotion Fear = new Emotion("Fear", register: true);

		public static readonly Emotion Boredom = new Emotion("Boredom", register: true);

		public static readonly Emotion BloodLust = new Emotion("BloodLust", register: true);

		public static readonly Emotion OutOfShortcut = new Emotion("OutOfShortcut", register: true);

		public Emotion(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class VoiceArticulation
	{
		public LizardVoice voice;

		public float length;

		public float maxVolume;

		public float[,] modifier;

		public VoiceArticulation(LizardVoice voice, float length, float maxVolume)
		{
			this.voice = voice;
			this.length = length;
			this.maxVolume = maxVolume;
			modifier = new float[2 + (int)(length / 20f), 2];
			for (int i = 0; i < modifier.GetLength(0); i++)
			{
				modifier[i, 0] = UnityEngine.Random.value;
				modifier[i, 1] = UnityEngine.Random.value;
			}
		}

		public float ReturnMod(float f, int m)
		{
			f *= (float)(modifier.GetLength(0) - 1);
			int num = Mathf.FloorToInt(f);
			f -= (float)num;
			return Mathf.Lerp(modifier[num, m], modifier[Math.Min(num + 1, modifier.GetLength(0) - 1), m], f);
		}
	}

	public Lizard lizard;

	public float myPitch;

	public float currentArticulationProgression;

	public int articulationIndex = -1;

	public float currentEmotionIntensity;

	public SoundID myVoiceTrigger;

	public VoiceArticulation[] articulations;

	public VoiceArticulation currentArt => articulations[articulationIndex];

	public LizardVoice(Lizard lizard)
		: base(lizard, 0)
	{
		this.lizard = lizard;
		InitSoundVisualizer(0.0002f, 0.003f, 0.5f);
		articulations = new VoiceArticulation[ExtEnum<Emotion>.values.Count];
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(lizard.abstractCreature.ID.RandomSeed);
		myVoiceTrigger = GetMyVoiceTrigger();
		myPitch = Custom.ClampedRandomVariation(0.5f, 0.2f, 0.8f) * 2f;
		float num = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		float num2 = Mathf.Lerp(0.7f, 1.3f, UnityEngine.Random.value);
		if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value && this.lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			num2 = Mathf.Lerp(1f, 1.3f, UnityEngine.Random.value);
		}
		VoiceArticulation voiceArticulation = new VoiceArticulation(this, 80f, 1f);
		for (int i = 0; i < articulations.Length; i++)
		{
			float num3 = 40f;
			float num4 = 1f;
			Emotion emotion = new Emotion(ExtEnum<Emotion>.values.GetEntry(i));
			if (emotion == Emotion.SpottedPreyFirstTime)
			{
				num3 = 50f;
				num4 = 1f;
			}
			else if (emotion == Emotion.GeneralSmallNoise || emotion == Emotion.ReSpottedPrey)
			{
				num3 = 12f;
				num4 = 0.5f;
			}
			else if (emotion == Emotion.Curious)
			{
				num3 = 40f;
				num4 = 0.4f;
			}
			else if (emotion == Emotion.Dominance)
			{
				num3 = 40f;
				num4 = 0.8f;
			}
			else if (emotion == Emotion.Submission)
			{
				num3 = 20f;
				num4 = 0.5f;
			}
			else if (emotion == Emotion.PainImpact)
			{
				num3 = 20f;
				num4 = 1f;
			}
			else if (emotion == Emotion.PainIdle)
			{
				num3 = 50f;
				num4 = 0.1f;
			}
			else if (emotion == Emotion.Boredom)
			{
				num3 = 30f;
				num4 = 0.2f;
			}
			else if (emotion == Emotion.BloodLust)
			{
				num3 = 35f;
				num4 = 0.5f;
			}
			else if (emotion == Emotion.OutOfShortcut)
			{
				num3 = 30f;
				num4 = 0.5f;
			}
			else if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value && emotion == MMFEnums.LizardVoiceEmotion.Love)
			{
				num3 = 50f;
				num4 = 0.5f;
			}
			if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value && emotion == MMFEnums.LizardVoiceEmotion.Love)
			{
				articulations[i] = new VoiceArticulation(this, num3 * num, num4);
			}
			else
			{
				articulations[i] = new VoiceArticulation(this, num3 * num, num4 * num2);
			}
			for (int j = 0; j < articulations[i].modifier.GetLength(0); j++)
			{
				float num5 = (float)j / (float)(articulations[i].modifier.GetLength(0) - 1);
				if (emotion == Emotion.SpottedPreyFirstTime)
				{
					articulations[i].modifier[j, 0] = Mathf.Max(articulations[i].modifier[j, 1], Mathf.Sin(num5 * (float)Math.PI));
					articulations[i].modifier[j, 1] = Mathf.Lerp(articulations[i].modifier[j, 1], Mathf.Sin(num5 * (float)Math.PI), 0.5f);
				}
				else if (emotion == Emotion.ReSpottedPrey)
				{
					articulations[i].modifier[1, 0] = 1f;
					if (num5 == 0f)
					{
						articulations[i].modifier[j, 1] = 1f;
					}
					else if (num5 == 1f)
					{
						articulations[i].modifier[j, 1] = 0f;
					}
				}
				else if (emotion == Emotion.Curious)
				{
					articulations[i].modifier[j, 1] = Mathf.Lerp(articulations[i].modifier[j, 1], num5, 0.5f);
					if (num5 == 1f)
					{
						articulations[i].modifier[j, 1] = 1f;
					}
				}
				else if (emotion == Emotion.Dominance)
				{
					articulations[i].modifier[j, 0] = Mathf.Lerp(articulations[i].modifier[j, 0], 1f, Mathf.Pow(1f - num5, 0.5f));
					articulations[i].modifier[j, 1] = Mathf.Lerp(articulations[i].modifier[j, 1], Mathf.Sin(num5 * (float)Math.PI) * 0.5f, 0.5f);
				}
				else if (emotion == Emotion.Submission)
				{
					articulations[i].modifier[j, 0] = Mathf.Lerp(articulations[i].modifier[j, 0], 0f, num5);
					articulations[i].modifier[j, 1] = Mathf.Lerp(articulations[i].modifier[j, 1], 1f, (1f - num5) * 0.5f);
				}
				for (int k = 0; k < 2; k++)
				{
					articulations[i].modifier[j, k] = Mathf.Lerp(articulations[i].modifier[j, k], voiceArticulation.ReturnMod(num5, k), 0.5f);
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	public override void Update()
	{
		base.Update();
		if (articulationIndex > -1)
		{
			currentArticulationProgression += 1f / (currentArt.length * Custom.LerpMap(currentEmotionIntensity, 0f, 2f, 0.5f, 1.5f));
			if (currentArticulationProgression >= 1f)
			{
				soundID = SoundID.None;
				articulationIndex = -1;
			}
			else
			{
				base.Volume = currentArt.ReturnMod(currentArticulationProgression, 0) * currentArt.maxVolume * Custom.LerpMap(currentEmotionIntensity, 0f, 2f, 0.4f, 3f) * (1f - lizard.jawForcedShut);
				base.Pitch = Mathf.Lerp(0.5f, 1.5f, currentArt.ReturnMod(currentArticulationProgression, 1)) * myPitch;
			}
		}
		if (lizard.grasps[0] != null && (!ModManager.MMF || !MMF.cfgExtraLizardSounds.Value || articulationIndex != MMFEnums.LizardVoiceEmotion.Love.Index))
		{
			soundA = null;
		}
	}

	public void MakeSound(Emotion emotion)
	{
		MakeSound(emotion, Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value));
	}

	public void MakeSound(Emotion emotion, float intensity)
	{
		bool flag = ModManager.MMF && MMF.cfgExtraLizardSounds.Value && emotion == MMFEnums.LizardVoiceEmotion.Love;
		if (lizard.Consious && (flag || (!(intensity < currentEmotionIntensity / 2f) && lizard.grasps[0] == null && (!ModManager.MMF || !MMF.cfgExtraLizardSounds.Value || !(lizard.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.WhiteLizard) || lizard.graphicsModule == null || !((lizard.graphicsModule as LizardGraphics).Camouflaged > 0.2f)))))
		{
			SoundID myLoveTrigger = myVoiceTrigger;
			if (flag)
			{
				myLoveTrigger = GetMyLoveTrigger();
			}
			currentArticulationProgression = 0f;
			articulationIndex = emotion.Index;
			soundID = myLoveTrigger;
			currentEmotionIntensity = intensity;
		}
	}

	private SoundID GetMyVoiceTrigger()
	{
		string text = "";
		string[] array = new string[0];
		if (lizard.Template.type == CreatureTemplate.Type.PinkLizard)
		{
			text = "Pink";
			array = new string[5] { "A", "B", "C", "D", "E" };
		}
		else if (lizard.Template.type == CreatureTemplate.Type.GreenLizard)
		{
			text = "Green";
			array = new string[1] { "A" };
		}
		else if (lizard.Template.type == CreatureTemplate.Type.BlueLizard)
		{
			text = "Blue";
			array = new string[1] { "A" };
		}
		if (ModManager.MMF && MMF.cfgExtraLizardSounds.Value)
		{
			if (lizard.Template.type == CreatureTemplate.Type.YellowLizard)
			{
				text = "Yellow";
				array = new string[1] { "A" };
			}
			else if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
			{
				text = "White";
				array = new string[3] { "A", "B", "C" };
			}
			else if (lizard.Template.type == CreatureTemplate.Type.RedLizard)
			{
				text = "Red";
				array = new string[1] { "A" };
			}
			else if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				text = "Black";
				array = new string[1] { "A" };
			}
			else if (lizard.Template.type == CreatureTemplate.Type.Salamander)
			{
				text = "Salamander";
				array = new string[2] { "A", "B" };
			}
			else if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
			{
				text = "Eel";
				array = new string[2] { "A", "B" };
			}
			else if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
			{
				text = "Cyan";
				array = new string[3] { "A", "B", "C" };
			}
			else if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
			{
				text = "Caramel";
				array = new string[1] { "A" };
			}
		}
		List<SoundID> list = new List<SoundID>();
		for (int i = 0; i < array.Length; i++)
		{
			SoundID soundID = SoundID.None;
			string text2 = "Lizard_Voice_" + text + "_" + array[i];
			if (ExtEnum<SoundID>.values.entries.Contains(text2))
			{
				soundID = new SoundID(text2);
			}
			if (soundID != SoundID.None && soundID.Index != -1 && lizard.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
			{
				list.Add(soundID);
			}
		}
		if (list.Count == 0)
		{
			return SoundID.None;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private SoundID GetMyLoveTrigger()
	{
		string text = "";
		string[] array = new string[0];
		if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			text = "Cyan";
			array = new string[3] { "A", "B", "C" };
		}
		else if (lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard))
		{
			text = "Red";
			array = new string[3] { "A", "B", "C" };
		}
		else
		{
			text = "Gen";
			array = new string[2] { "A", "B" };
		}
		List<SoundID> list = new List<SoundID>();
		for (int i = 0; i < array.Length; i++)
		{
			SoundID soundID = SoundID.None;
			string text2 = "Lizard_Love_Voice_" + text + "_" + array[i];
			if (ExtEnum<SoundID>.values.entries.Contains(text2))
			{
				soundID = new SoundID(text2);
			}
			if (soundID != SoundID.None && soundID.Index != -1 && lizard.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
			{
				list.Add(soundID);
			}
		}
		if (list.Count == 0)
		{
			return SoundID.None;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}
}
