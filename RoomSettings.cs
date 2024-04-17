using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class RoomSettings
{
	public class FadePalette
	{
		public int palette;

		public float[] fades;

		public FadePalette(int palette, int screens)
		{
			this.palette = palette;
			fades = new float[screens];
		}
	}

	public class RoomEffect
	{
		public class Type : ExtEnum<Type>
		{
			public static readonly Type None = new Type("None", register: true);

			public static readonly Type SkyDandelions = new Type("SkyDandelions", register: true);

			public static readonly Type SkyBloom = new Type("SkyBloom", register: true);

			public static readonly Type LightBurn = new Type("LightBurn", register: true);

			public static readonly Type SkyAndLightBloom = new Type("SkyAndLightBloom", register: true);

			public static readonly Type Bloom = new Type("Bloom", register: true);

			public static readonly Type Fog = new Type("Fog", register: true);

			public static readonly Type Lightning = new Type("Lightning", register: true);

			public static readonly Type BkgOnlyLightning = new Type("BkgOnlyLightning", register: true);

			public static readonly Type ExtraLoudThunder = new Type("ExtraLoudThunder", register: true);

			public static readonly Type GreenSparks = new Type("GreenSparks", register: true);

			public static readonly Type VoidMelt = new Type("VoidMelt", register: true);

			public static readonly Type ZeroG = new Type("ZeroG", register: true);

			public static readonly Type BrokenZeroG = new Type("BrokenZeroG", register: true);

			public static readonly Type ZeroGSpecks = new Type("ZeroGSpecks", register: true);

			public static readonly Type SunBlock = new Type("SunBlock", register: true);

			public static readonly Type SuperStructureProjector = new Type("SuperStructureProjector", register: true);

			public static readonly Type ProjectedScanLines = new Type("ProjectedScanLines", register: true);

			public static readonly Type CorruptionSpores = new Type("CorruptionSpores", register: true);

			public static readonly Type SSSwarmers = new Type("SSSwarmers", register: true);

			public static readonly Type SSMusic = new Type("SSMusic", register: true);

			public static readonly Type AboveCloudsView = new Type("AboveCloudsView", register: true);

			public static readonly Type RoofTopView = new Type("RoofTopView", register: true);

			public static readonly Type VoidSea = new Type("VoidSea", register: true);

			public static readonly Type ElectricDeath = new Type("ElectricDeath", register: true);

			public static readonly Type VoidSpawn = new Type("VoidSpawn", register: true);

			public static readonly Type BorderPushBack = new Type("BorderPushBack", register: true);

			public static readonly Type Flies = new Type("Flies", register: true);

			public static readonly Type FireFlies = new Type("FireFlies", register: true);

			public static readonly Type TinyDragonFly = new Type("TinyDragonFly", register: true);

			public static readonly Type RockFlea = new Type("RockFlea", register: true);

			public static readonly Type RedSwarmer = new Type("RedSwarmer", register: true);

			public static readonly Type Ant = new Type("Ant", register: true);

			public static readonly Type Beetle = new Type("Beetle", register: true);

			public static readonly Type WaterGlowworm = new Type("WaterGlowworm", register: true);

			public static readonly Type Wasp = new Type("Wasp", register: true);

			public static readonly Type Moth = new Type("Moth", register: true);

			public static readonly Type LightRain = new Type("LightRain", register: true);

			public static readonly Type HeavyRain = new Type("HeavyRain", register: true);

			public static readonly Type HeavyRainFlux = new Type("HeavyRainFlux", register: true);

			public static readonly Type BulletRain = new Type("BulletRain", register: true);

			public static readonly Type BulletRainFlux = new Type("BulletRainFlux", register: true);

			public static readonly Type WaterFluxFrequency = new Type("WaterFluxFrequency", register: true);

			public static readonly Type WaterFluxMinLevel = new Type("WaterFluxMinLevel", register: true);

			public static readonly Type WaterFluxMaxLevel = new Type("WaterFluxMaxLevel", register: true);

			public static readonly Type WaterFluxMinDelay = new Type("WaterFluxMinDelay", register: true);

			public static readonly Type WaterFluxMaxDelay = new Type("WaterFluxMaxDelay", register: true);

			public static readonly Type WaterFluxSpeed = new Type("WaterFluxSpeed", register: true);

			public static readonly Type WaterFluxOffset = new Type("WaterFluxOffset", register: true);

			public static readonly Type WaterFluxRumble = new Type("WaterFluxRumble", register: true);

			public static readonly Type LethalWater = new Type("LethalWater", register: true);

			public static readonly Type HeatWave = new Type("HeatWave", register: true);

			public static readonly Type Coldness = new Type("Coldness", register: true);

			public static readonly Type DayNight = new Type("DayNight", register: true);

			public static readonly Type Darkness = new Type("Darkness", register: true);

			public static readonly Type Brightness = new Type("Brightness", register: true);

			public static readonly Type Contrast = new Type("Contrast", register: true);

			public static readonly Type Desaturation = new Type("Desaturation", register: true);

			public static readonly Type Hue = new Type("Hue", register: true);

			public static readonly Type FairyParticles = new Type("FairyParticles", register: true);

			public static readonly Type FakeGate = new Type("FakeGate", register: true);

			public static readonly Type Dustpuffs = new Type("Dustpuffs", register: true);

			public static readonly Type SilenceWater = new Type("SilenceWater", register: true);

			public static readonly Type WaterViscosity = new Type("WaterViscosity", register: true);

			public static readonly Type DarkenLights = new Type("DarkenLights", register: true);

			public static readonly Type WaterLights = new Type("WaterLights", register: true);

			public static readonly Type WaterDepth = new Type("WaterDepth", register: true);

			public static readonly Type DirtyWater = new Type("DirtyWater", register: true);

			public static readonly Type LavaSurface = new Type("LavaSurface", register: true);

			public static readonly Type PixelShift = new Type("PixelShift", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Type type;

		public float amount;

		public bool inherited;

		public bool overWrite;

		public Vector2 panelPosition;

		public string[] unrecognizedAttributes;

		public RoomEffect(Type type, float amount, bool inherited)
		{
			this.type = type;
			this.amount = amount;
			this.inherited = inherited;
		}

		public override string ToString()
		{
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}", type.ToString(), amount, panelPosition.x, panelPosition.y), "-", unrecognizedAttributes);
		}

		public void FromString(string[] s)
		{
			try
			{
				type = new Type(s[0]);
				amount = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 4);
			}
			catch
			{
				Custom.LogWarning("Wrong syntax effect loaded:", s[0]);
			}
		}
	}

	public bool isAncestor;

	public bool isTemplate;

	public bool isFirstTemplate;

	public RoomSettings parent;

	public RoomRain.DangerType dType;

	public float? rInts;

	public float? rumInts;

	public float? cDrips;

	public float? wSpeed;

	public float? wAmp;

	public float? wLength;

	public float? swAmp;

	public float? swLength;

	public float? clds;

	public float? grm;

	public float? bkgDrnVl;

	public float? bkgDrnNoThreatVol;

	public float? rndItmDns;

	public float? rndItmSprChnc;

	public float? wtrRflctAlpha;

	public int? pal;

	public int? eColA;

	public int? eColB;

	public bool roomSpecificScript;

	public bool wetTerrain = true;

	public FadePalette fadePalette;

	public List<RoomEffect> effects;

	public List<AmbientSound> ambientSounds;

	public List<EventTrigger> triggers;

	public List<PlacedObject> placedObjects;

	public string name;

	public string filePath;

	public RoomRain.DangerType DangerType
	{
		get
		{
			if (dType != null)
			{
				return dType;
			}
			return parent.DangerType;
		}
		set
		{
			dType = value;
		}
	}

	public float RainIntensity
	{
		get
		{
			if (rInts.HasValue)
			{
				return rInts.Value;
			}
			return parent.RainIntensity;
		}
		set
		{
			rInts = value;
		}
	}

	public float RumbleIntensity
	{
		get
		{
			if (rumInts.HasValue)
			{
				return rumInts.Value;
			}
			return parent.RumbleIntensity;
		}
		set
		{
			rumInts = value;
		}
	}

	public float CeilingDrips
	{
		get
		{
			if (cDrips.HasValue)
			{
				return cDrips.Value;
			}
			return parent.CeilingDrips;
		}
		set
		{
			cDrips = value;
		}
	}

	public float WaveSpeed
	{
		get
		{
			if (wSpeed.HasValue)
			{
				return wSpeed.Value;
			}
			return parent.WaveSpeed;
		}
		set
		{
			wSpeed = value;
		}
	}

	public float WaveAmplitude
	{
		get
		{
			if (wAmp.HasValue)
			{
				return wAmp.Value;
			}
			return parent.WaveAmplitude;
		}
		set
		{
			wAmp = value;
		}
	}

	public float WaveLength
	{
		get
		{
			if (wLength.HasValue)
			{
				return wLength.Value;
			}
			return parent.WaveLength;
		}
		set
		{
			wLength = value;
		}
	}

	public float SecondWaveAmplitude
	{
		get
		{
			if (swAmp.HasValue)
			{
				return swAmp.Value;
			}
			return parent.SecondWaveAmplitude;
		}
		set
		{
			swAmp = value;
		}
	}

	public float SecondWaveLength
	{
		get
		{
			if (swLength.HasValue)
			{
				return swLength.Value;
			}
			return parent.SecondWaveLength;
		}
		set
		{
			swLength = value;
		}
	}

	public float Clouds
	{
		get
		{
			if (clds.HasValue)
			{
				return clds.Value;
			}
			return parent.Clouds;
		}
		set
		{
			clds = value;
		}
	}

	public float Grime
	{
		get
		{
			if (grm.HasValue)
			{
				return grm.Value;
			}
			return parent.Grime;
		}
		set
		{
			grm = value;
		}
	}

	public float BkgDroneVolume
	{
		get
		{
			if (bkgDrnVl.HasValue)
			{
				return bkgDrnVl.Value;
			}
			return parent.BkgDroneVolume;
		}
		set
		{
			bkgDrnVl = value;
		}
	}

	public float BkgDroneNoThreatVolume
	{
		get
		{
			if (bkgDrnNoThreatVol.HasValue)
			{
				return bkgDrnNoThreatVol.Value;
			}
			return parent.BkgDroneNoThreatVolume;
		}
		set
		{
			bkgDrnNoThreatVol = value;
		}
	}

	public float RandomItemDensity
	{
		get
		{
			if (rndItmDns.HasValue)
			{
				return rndItmDns.Value;
			}
			return parent.RandomItemDensity;
		}
		set
		{
			rndItmDns = value;
		}
	}

	public float RandomItemSpearChance
	{
		get
		{
			if (rndItmSprChnc.HasValue)
			{
				return rndItmSprChnc.Value;
			}
			return parent.RandomItemSpearChance;
		}
		set
		{
			rndItmSprChnc = value;
		}
	}

	public float WaterReflectionAlpha
	{
		get
		{
			if (wtrRflctAlpha.HasValue)
			{
				return wtrRflctAlpha.Value;
			}
			return parent.WaterReflectionAlpha;
		}
		set
		{
			wtrRflctAlpha = value;
		}
	}

	public int Palette
	{
		get
		{
			if (pal.HasValue)
			{
				return pal.Value;
			}
			return parent.Palette;
		}
		set
		{
			pal = value;
		}
	}

	public int EffectColorA
	{
		get
		{
			if (eColA.HasValue)
			{
				return eColA.Value;
			}
			return parent.EffectColorA;
		}
		set
		{
			eColA = value;
		}
	}

	public int EffectColorB
	{
		get
		{
			if (eColB.HasValue)
			{
				return eColB.Value;
			}
			return parent.EffectColorB;
		}
		set
		{
			eColB = value;
		}
	}

	public void Reset()
	{
		dType = null;
		cDrips = null;
		pal = null;
		eColA = null;
		eColB = null;
		effects = new List<RoomEffect>();
		ambientSounds = new List<AmbientSound>();
		placedObjects = new List<PlacedObject>();
		triggers = new List<EventTrigger>();
	}

	public RoomSettings(string name, Region region, bool template, bool firstTemplate, SlugcatStats.Name playerChar)
	{
		this.name = name;
		effects = new List<RoomEffect>();
		ambientSounds = new List<AmbientSound>();
		placedObjects = new List<PlacedObject>();
		triggers = new List<EventTrigger>();
		isTemplate = template;
		isFirstTemplate = firstTemplate;
		if (name == "RootTemplate")
		{
			filePath = "";
			return;
		}
		if (template)
		{
			filePath = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + region.name + Path.DirectorySeparatorChar + name + ".txt");
		}
		else if (!File.Exists(filePath = ((playerChar == null) ? "" : WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings-" + playerChar.value + ".txt"))))
		{
			string path = ((!ModManager.MSC || !name.EndsWith("-2")) ? WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings.txt") : WorldLoader.FindRoomFile(name.Substring(0, name.Length - 2), includeRootDirectory: false, "-2_settings.txt"));
			if (File.Exists(path))
			{
				filePath = path;
			}
			else if (name.EndsWith("-2"))
			{
				filePath = WorldLoader.FindRoomFile(name.Substring(0, name.Length - 2), includeRootDirectory: false, "_settings.txt");
			}
			else
			{
				filePath = path;
			}
		}
		Reset();
		FindParent(region);
		if (!Load(playerChar))
		{
			string text = WorldLoader.FindRoomFile(name, includeRootDirectory: false, ".txt");
			if (File.Exists(text))
			{
				filePath = text.Substring(0, text.Length - 4) + "_settings.txt";
			}
		}
	}

	private void FindParent(Region region)
	{
		parent = DefaultRoomSettings.ancestor;
		if (region == null)
		{
			return;
		}
		if (!isTemplate && !isAncestor && region.roomSettingsTemplates.Length != 0)
		{
			parent = region.roomSettingsTemplates[0];
		}
		if (!isTemplate && region != null && File.Exists(filePath))
		{
			string[] array = File.ReadAllLines(filePath);
			for (int i = 0; i < array.Length; i++)
			{
				if (Regex.Split(array[i], ": ")[0] == "Template")
				{
					if (Regex.Split(array[i], ": ")[1] == "NONE")
					{
						parent = DefaultRoomSettings.ancestor;
					}
					else
					{
						parent = region.GetRoomSettingsTemplate(Regex.Split(Regex.Split(array[i], ": ")[1], "_")[2]);
					}
					break;
				}
			}
		}
		InheritEffects();
		InheritAmbientSounds();
	}

	public void Save()
	{
		Save(filePath, saveAsTemplate: false);
	}

	public void Save(SlugcatStats.Name slugcat)
	{
		Save(SpecificPath(slugcat), saveAsTemplate: false);
		filePath = WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings-" + slugcat.value + ".txt");
	}

	public string SpecificPath(SlugcatStats.Name id)
	{
		if (!filePath.Contains(".txt"))
		{
			return "";
		}
		return filePath.Substring(0, filePath.IndexOf("settings") + "settings".Length) + "-" + id.value + ".txt";
	}

	private void Save(string path, bool saveAsTemplate)
	{
		using StreamWriter streamWriter = File.CreateText(path);
		if (!saveAsTemplate)
		{
			if (parent.isAncestor)
			{
				streamWriter.WriteLine("Template: NONE");
			}
			else if (parent.isTemplate && !parent.isFirstTemplate)
			{
				streamWriter.WriteLine("Template: " + parent.name);
			}
		}
		if (dType != null || saveAsTemplate)
		{
			streamWriter.WriteLine("DangerType: " + DangerType.ToString());
		}
		if (cDrips.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("CeilingDrips: " + CeilingDrips);
		}
		if (rInts.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RainIntensity: " + RainIntensity);
		}
		if (rumInts.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RumbleIntensity: " + RumbleIntensity);
		}
		if (wSpeed.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveSpeed: " + WaveSpeed);
		}
		if (wLength.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveLength: " + WaveLength);
		}
		if (wAmp.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveAmplitude: " + WaveAmplitude);
		}
		if (wLength.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("SecondWaveLength: " + SecondWaveLength);
		}
		if (wAmp.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("SecondWaveAmplitude: " + SecondWaveAmplitude);
		}
		if (clds.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Clouds: " + Clouds);
		}
		if (grm.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Grime: " + Grime);
		}
		if (bkgDrnVl.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("BkgDroneVolume: " + BkgDroneVolume);
		}
		if (bkgDrnNoThreatVol.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("BkgDroneNoThreatVolume: " + BkgDroneNoThreatVolume);
		}
		if (rndItmDns.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RandomItemDensity: " + RandomItemDensity);
		}
		if (rndItmSprChnc.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RandomItemSpearChance: " + RandomItemSpearChance);
		}
		if (wtrRflctAlpha.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaterReflectionAlpha: " + WaterReflectionAlpha);
		}
		if (pal.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Palette: " + Palette);
		}
		if (eColA.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("EffectColorA: " + EffectColorA);
		}
		if (eColB.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("EffectColorB: " + EffectColorB);
		}
		if (effects.Count > 0)
		{
			string text = "";
			for (int i = 0; i < effects.Count; i++)
			{
				if (!effects[i].inherited || saveAsTemplate)
				{
					text = text + effects[i].ToString() + ", ";
				}
			}
			if (text != "")
			{
				streamWriter.WriteLine("Effects: " + text);
			}
		}
		if (!saveAsTemplate && placedObjects.Count > 0)
		{
			string text2 = "";
			for (int j = 0; j < placedObjects.Count; j++)
			{
				text2 = text2 + placedObjects[j].ToString() + ", ";
			}
			streamWriter.WriteLine("PlacedObjects: " + text2);
		}
		if (ambientSounds.Count > 0)
		{
			string text3 = "";
			for (int k = 0; k < ambientSounds.Count; k++)
			{
				if ((!ambientSounds[k].inherited || saveAsTemplate) && (ambientSounds[k].type != AmbientSound.Type.Spot || !saveAsTemplate))
				{
					text3 = text3 + ambientSounds[k].ToString() + ", ";
				}
			}
			if (text3 != "")
			{
				streamWriter.WriteLine("AmbientSounds: " + text3);
			}
		}
		if (triggers.Count > 0)
		{
			string text4 = "";
			for (int l = 0; l < triggers.Count; l++)
			{
				text4 = text4 + triggers[l].ToString() + ", ";
			}
			if (text4 != "")
			{
				streamWriter.WriteLine("Triggers: " + text4);
			}
		}
		if (fadePalette != null && !saveAsTemplate)
		{
			string text5 = "FadePalette: " + fadePalette.palette;
			for (int m = 0; m < fadePalette.fades.Length; m++)
			{
				text5 = text5 + ", " + fadePalette.fades[m];
			}
			streamWriter.WriteLine(text5);
		}
		if (roomSpecificScript && !saveAsTemplate)
		{
			streamWriter.WriteLine("RoomSpecificScriptActive: ON");
		}
		if (!wetTerrain && !saveAsTemplate)
		{
			streamWriter.WriteLine("WetTerrain: OFF");
		}
	}

	public bool Load(SlugcatStats.Name playerChar)
	{
		if (!File.Exists(filePath))
		{
			return false;
		}
		Reset();
		string[] array = File.ReadAllLines(filePath);
		List<string[]> list = new List<string[]>();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], ": ");
			if (array2.Length == 2)
			{
				list.Add(array2);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			switch (list[j][0])
			{
			case "DangerType":
				DangerType = new RoomRain.DangerType(list[j][1]);
				break;
			case "CeilingDrips":
				CeilingDrips = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RainIntensity":
				RainIntensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RumbleIntensity":
				RumbleIntensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveSpeed":
				WaveSpeed = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveLength":
				WaveLength = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveAmplitude":
				WaveAmplitude = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SecondWaveLength":
				SecondWaveLength = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SecondWaveAmplitude":
				SecondWaveAmplitude = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Clouds":
				Clouds = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Grime":
				Grime = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "BkgDroneVolume":
				BkgDroneVolume = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "BkgDroneNoThreatVolume":
				BkgDroneNoThreatVolume = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Palette":
				Palette = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "EffectColorA":
				EffectColorA = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "EffectColorB":
				EffectColorB = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Effects":
				LoadEffects(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "PlacedObjects":
				LoadPlacedObjects(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "), playerChar);
				break;
			case "AmbientSounds":
				LoadAmbientSounds(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "Triggers":
				LoadTriggers(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "FadePalette":
				LoadFadePalette(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "RoomSpecificScriptActive":
				roomSpecificScript = true;
				break;
			case "WetTerrain":
				wetTerrain = false;
				break;
			case "RandomItemDensity":
				RandomItemDensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RandomItemSpearChance":
				RandomItemSpearChance = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaterReflectionAlpha":
				WaterReflectionAlpha = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			}
		}
		InheritEffects();
		InheritAmbientSounds();
		return true;
	}

	private void LoadEffects(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "-");
			string text = array[0];
			if (text == null || !(text == ""))
			{
				effects.Add(new RoomEffect(RoomEffect.Type.None, 0f, isTemplate || isAncestor));
				effects[effects.Count - 1].FromString(array);
			}
		}
	}

	private void LoadPlacedObjects(string[] s, SlugcatStats.Name playerChar)
	{
		List<PlacedObject> list = new List<PlacedObject>();
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i].Trim(), "><");
			string text = array[0];
			if (text == null || !(text == ""))
			{
				placedObjects.Add(new PlacedObject(PlacedObject.Type.None, null));
				placedObjects[placedObjects.Count - 1].FromString(array);
				if (playerChar != null && placedObjects[placedObjects.Count - 1].data is PlacedObject.FilterData && !(placedObjects[placedObjects.Count - 1].data as PlacedObject.FilterData).availableToPlayers.Contains(playerChar))
				{
					list.Add(placedObjects[placedObjects.Count - 1]);
				}
			}
		}
		if (!(playerChar != null))
		{
			return;
		}
		for (int j = 0; j < placedObjects.Count; j++)
		{
			if (!placedObjects[j].deactivattable)
			{
				continue;
			}
			for (int k = 0; k < list.Count; k++)
			{
				if (Custom.DistLess(placedObjects[j].pos, list[k].pos, (list[k].data as PlacedObject.FilterData).Rad))
				{
					placedObjects[j].active = false;
					break;
				}
			}
		}
	}

	private void LoadAmbientSounds(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "><");
			string text = array[0];
			if (text != null && (text == null || text.Length != 0))
			{
				switch (text)
				{
				case "OMNI":
					ambientSounds.Add(new OmniDirectionalSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				case "DIR":
					ambientSounds.Add(new DirectionalSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				case "SPOT":
					ambientSounds.Add(new SpotSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				}
			}
		}
	}

	private void LoadTriggers(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<tA>");
			string text = array[0];
			if (text != null)
			{
				if (text != null && text.Length == 0)
				{
					continue;
				}
				if (text == "Spot")
				{
					triggers.Add(new SpotTrigger());
					triggers[triggers.Count - 1].FromString(array);
					continue;
				}
			}
			triggers.Add(new EventTrigger(new EventTrigger.TriggerType(array[0])));
			triggers[triggers.Count - 1].FromString(array);
		}
	}

	private void LoadFadePalette(string[] s)
	{
		fadePalette = new FadePalette(int.Parse(s[0], NumberStyles.Any, CultureInfo.InvariantCulture), s.Length - 1);
		for (int i = 0; i < s.Length - 1; i++)
		{
			fadePalette.fades[i] = float.Parse(s[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
	}

	public void InheritEffects()
	{
		if (isTemplate)
		{
			return;
		}
		for (int num = effects.Count - 1; num >= 0; num--)
		{
			if (effects[num].inherited)
			{
				effects.RemoveAt(num);
			}
		}
		for (int i = 0; i < parent.effects.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < effects.Count; j++)
			{
				if (effects[j].type == parent.effects[i].type)
				{
					effects[j].overWrite = true;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				effects.Add(parent.effects[i]);
			}
		}
	}

	public void InheritAmbientSounds()
	{
		if (isTemplate)
		{
			return;
		}
		for (int num = ambientSounds.Count - 1; num >= 0; num--)
		{
			if (ambientSounds[num].inherited)
			{
				ambientSounds.RemoveAt(num);
			}
		}
		for (int i = 0; i < parent.ambientSounds.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < ambientSounds.Count; j++)
			{
				if (ambientSounds[j].sample == parent.ambientSounds[i].sample && ambientSounds[j].type == parent.ambientSounds[i].type)
				{
					ambientSounds[j].overWrite = true;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ambientSounds.Add(parent.ambientSounds[i]);
			}
		}
	}

	public void RemoveEffect(RoomEffect.Type type)
	{
		for (int num = effects.Count - 1; num >= 0; num--)
		{
			if (effects[num].inherited || effects[num].type == type)
			{
				effects.RemoveAt(num);
			}
		}
		InheritEffects();
	}

	public void RemoveAmbientSound(AmbientSound.Type type, string sampleName)
	{
		for (int num = ambientSounds.Count - 1; num >= 0; num--)
		{
			if (ambientSounds[num].inherited || (ambientSounds[num].type == type && ambientSounds[num].sample == sampleName))
			{
				ambientSounds.RemoveAt(num);
			}
		}
		InheritAmbientSounds();
	}

	public void SetTemplate(string buttonText, Region region)
	{
		if (buttonText == "NONE")
		{
			parent = DefaultRoomSettings.ancestor;
		}
		else
		{
			parent = region.GetRoomSettingsTemplate(Regex.Split(buttonText, " - ")[1]);
		}
		InheritEffects();
		InheritAmbientSounds();
	}

	public void SaveAsTemplate(string buttonText, Region region)
	{
		SetTemplate(buttonText, region);
		if (parent.isAncestor || !parent.isTemplate)
		{
			Custom.LogWarning("Not a template!");
			return;
		}
		Save(parent.filePath, saveAsTemplate: true);
		parent.Load(null);
		for (int i = 0; i < parent.effects.Count; i++)
		{
			parent.effects[i].inherited = true;
		}
		for (int j = 0; j < parent.ambientSounds.Count; j++)
		{
			parent.ambientSounds[j].inherited = true;
		}
		Reset();
		InheritEffects();
		InheritAmbientSounds();
	}

	public RoomEffect GetEffect(RoomEffect.Type type)
	{
		for (int i = 0; i < effects.Count; i++)
		{
			if (effects[i].type == type)
			{
				return effects[i];
			}
		}
		return null;
	}

	public float GetEffectAmount(RoomEffect.Type type)
	{
		for (int i = 0; i < effects.Count; i++)
		{
			if (effects[i].type == type)
			{
				return effects[i].amount;
			}
		}
		return 0f;
	}
}
