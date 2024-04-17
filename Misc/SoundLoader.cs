using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetBundles;
using RWCustom;
using UnityEngine;

public class SoundLoader
{
	private class VolumeGroup
	{
		public string name;

		public float vol;

		public List<int> affectLines;

		public VolumeGroup(string name, float vol)
		{
			this.name = name;
			this.vol = vol;
			affectLines = new List<int>();
		}
	}

	public struct SoundPlayInstruction
	{
		public int audioClip;

		public float minVol;

		public float maxVol;

		public float minPitch;

		public float maxPitch;

		public SoundPlayInstruction(int audioClip, string[] initStringSplit)
		{
			this.audioClip = audioClip;
			minVol = 1f;
			maxVol = 1f;
			minPitch = 1f;
			maxPitch = 1f;
			for (int i = 1; i < initStringSplit.Length; i++)
			{
				string[] array = Regex.Split(initStringSplit[i], "=");
				switch (array[0])
				{
				case "vol":
					minVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxVol = minVol;
					break;
				case "pitch":
					minPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxPitch = minPitch;
					break;
				case "minVol":
					minVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "maxVol":
					maxVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "minPitch":
					minPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "maxPitch":
					maxPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				}
			}
		}
	}

	private class SoundTrigger
	{
		private SoundPlayInstruction[] sounds;

		public float GROUPVOL;

		private float minVol;

		private float maxVol;

		private float minPitch;

		private float maxPitch;

		private float range;

		private float dopplerFactor;

		public bool PlayAll;

		public bool DontLog;

		public float silentChance;

		public bool cache;

		public SoundID soundID;

		public int Instructions => sounds.Length;

		public SoundTrigger(SoundID soundID, SoundPlayInstruction[] sounds, float gVol, SoundLoader soundLoader, string[] initString)
		{
			this.soundID = soundID;
			this.sounds = sounds;
			GROUPVOL = gVol;
			minVol = GROUPVOL;
			maxVol = GROUPVOL;
			minPitch = 1f;
			maxPitch = 1f;
			PlayAll = false;
			DontLog = false;
			cache = false;
			range = 1f;
			dopplerFactor = 1f;
			for (int i = 1; i < initString.Length; i++)
			{
				string[] array = Regex.Split(initString[i], "=");
				switch (array[0])
				{
				case "vol":
					minVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture) * GROUPVOL;
					maxVol = minVol;
					break;
				case "pitch":
					minPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxPitch = minPitch;
					break;
				case "minVol":
					minVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture) * GROUPVOL;
					break;
				case "maxVol":
					maxVol = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture) * GROUPVOL;
					break;
				case "minPitch":
					minPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "maxPitch":
					maxPitch = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "rangeFac":
					range = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "dopplerFac":
					dopplerFactor = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "PLAYALL":
					PlayAll = true;
					break;
				case "DONTLOG":
					DontLog = true;
					break;
				case "silentChance":
					silentChance = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "CACHE":
					cache = true;
					break;
				}
			}
			cache = true;
		}

		public SoundData GetRandomSoundData()
		{
			return GetSoundData(Random.Range(0, sounds.Length));
		}

		public SoundData GetSoundData(int i)
		{
			SoundPlayInstruction soundPlayInstruction = sounds[i];
			return new SoundData(soundID, soundPlayInstruction.audioClip, Mathf.Lerp(soundPlayInstruction.minVol, soundPlayInstruction.maxVol, Random.value) * Mathf.Lerp(minVol, maxVol, Random.value), Mathf.Lerp(soundPlayInstruction.minPitch, soundPlayInstruction.maxPitch, Random.value) * Mathf.Lerp(minPitch, maxPitch, Random.value), range, dopplerFactor);
		}
	}

	public struct SoundData
	{
		public SoundID soundID;

		public int audioClip;

		public float vol;

		public float pitch;

		public float range;

		public float dopplerFac;

		public bool dontAutoPlay;

		public string soundName;

		public SoundData(SoundID soundID, int audioClip, float vol, float pitch, float range, float dopplerFac)
		{
			this.soundID = soundID;
			this.audioClip = audioClip;
			this.vol = vol;
			this.pitch = pitch;
			this.range = range;
			this.dopplerFac = dopplerFac;
			soundName = "";
			dontAutoPlay = false;
		}
	}

	public class SoundImporter : MonoBehaviour
	{
		public class ClipAndIndex
		{
			public AudioClip clip;

			public IntVector2 index;

			public ClipAndIndex(IntVector2 index, AudioClip clip)
			{
				this.index = index;
				this.clip = clip;
			}
		}

		public List<ClipAndIndex> loadingClips = new List<ClipAndIndex>();

		private string[] fileTypes = new string[2] { "ogg", "wav" };

		private SoundLoader owner;

		public void Init(SoundLoader owner)
		{
			this.owner = owner;
			reloadSounds();
		}

		private void reloadSounds()
		{
			string[] array = AssetManager.ListDirectory("soundeffects");
			int num = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!validFileType(text))
				{
					continue;
				}
				string text2 = Path.GetFileNameWithoutExtension(text);
				int num2 = 1;
				bool flag = false;
				if (text2.Split('_').Length > 1)
				{
					num2 = int.Parse(text2.Split('_')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					text2 = text2.Split('_')[0];
					flag = true;
				}
				int num3 = -1;
				for (int j = 0; j < owner.audioClipNames.Length; j++)
				{
					if (!owner.audioClipsThroughUnity[j] && string.Compare(owner.audioClipNames[j], text2, ignoreCase: true) == 0)
					{
						num3 = j;
						break;
					}
				}
				if (num3 > -1)
				{
					StartCoroutine(loadFile(text, text2 + (flag ? ("_" + num2) : ""), new IntVector2(num3, num2 - 1)));
					num++;
				}
			}
			owner.errors.Add("Initiating import of " + num + " samples");
			Custom.Log("------- Initiating import of", num.ToString(), "samples");
		}

		private bool validFileType(string filename)
		{
			string[] array = fileTypes;
			foreach (string value in array)
			{
				if (filename.IndexOf(value) > -1)
				{
					return true;
				}
			}
			return false;
		}

		private IEnumerator loadFile(string path, string name, IntVector2 index)
		{
			WWW www = new WWW("file://" + path);
			AudioClip myAudioClip = www.GetAudioClip();
			while (myAudioClip.loadState != AudioDataLoadState.Loaded && myAudioClip.loadState != AudioDataLoadState.Failed)
			{
				yield return www;
			}
			AudioClip audioClip = www.GetAudioClip(threeD: false);
			audioClip.name = name;
			loadingClips.Add(new ClipAndIndex(index, audioClip));
		}
	}

	public class AmbientImporter : MonoBehaviour
	{
		public string fileName;

		public AudioClip loadedClip;

		private string[] fileTypes = new string[2] { "ogg", "wav" };

		private bool isWav;

		public bool initiated;

		public void Init(SoundLoader owner)
		{
			initiated = true;
			string[] array = AssetManager.ListDirectory("soundeffects/ambient");
			foreach (string text in array)
			{
				if (validFileType(text) && Path.GetFileName(text) == fileName)
				{
					StartCoroutine(loadFile(this, text, base.name));
					break;
				}
			}
		}

		private bool validFileType(string filename)
		{
			string[] array = fileTypes;
			foreach (string text in array)
			{
				if (filename.IndexOf(text) > -1)
				{
					isWav = text == "wav";
					return true;
				}
			}
			return false;
		}

		private IEnumerator loadFile(AmbientImporter importer, string path, string name)
		{
			WWW www = new WWW("file://" + path);
			AudioClip myAudioClip = www.GetAudioClip(threeD: false, stream: false, isWav ? AudioType.WAV : AudioType.OGGVORBIS);
			while (myAudioClip.loadState != AudioDataLoadState.Loaded && myAudioClip.loadState != AudioDataLoadState.Failed)
			{
				yield return www;
			}
			myAudioClip.name = name;
			importer.loadedClip = myAudioClip;
		}
	}

	private static readonly AGLog<SoundLoader> Log = new AGLog<SoundLoader>();

	public const string ASSETBUNDLE_LOADEDSOUNDEFFECTS = "loadedsoundeffects";

	public const string ASSETBUNDLE_LOADEDSOUNDEFFECTS_AMBIENT = "loadedsoundeffects_ambient";

	private RainWorld rainWorld;

	public bool[] workingTriggers;

	private bool[] audioClipsThroughUnity;

	private AudioClip[][] externalAudio;

	private AudioClip[][] unityAudio;

	private AssetBundleLoadAssetOperation[][] unityAudioLoaders;

	private bool[] unityAudioCached;

	private string[] audioClipNames;

	private int[] soundVariations;

	public List<AudioClip> ambientClipsThroughUnity;

	public List<AmbientImporter> ambientImporters;

	private Dictionary<string, AssetBundleLoadAssetOperation> ambientClipsThroughUnityLoaders;

	private SoundTrigger[] soundTriggers;

	public float volume;

	public float volumeExponent;

	public bool loadingDone;

	private SoundImporter soundImporter;

	private GameObject gameObject;

	private int clipsToBeLoaded;

	private bool requestedAssetBundlesLoad;

	private LoadedAssetBundle loadedSoundEffectsAssetBundle;

	private LoadedAssetBundle loadedSoundEffectsAmbientAssetBundle;

	private bool requestLoadSounds;

	private bool requestLoadAmbientSounds;

	private bool requestReleaseUnityAudio;

	private List<VolumeGroup> volumeGroups;

	public List<string> errors;

	public bool assetBundlesLoaded { get; private set; }

	public SoundLoader(bool loadAllAmbientSounds, RainWorld rainWorld)
	{
		Custom.Log("INIT SOUND LOADER");
		this.rainWorld = rainWorld;
		ambientImporters = new List<AmbientImporter>();
		ambientClipsThroughUnity = new List<AudioClip>();
		ambientClipsThroughUnityLoaders = new Dictionary<string, AssetBundleLoadAssetOperation>();
		LoadSounds();
		if (requestLoadSounds)
		{
			string text = AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + "Sounds.txt");
			Custom.Log(text);
			string[] array = File.ReadAllLines(text);
			volume = float.Parse(Regex.Split(array[0], ": ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			volumeExponent = float.Parse(Regex.Split(array[1], ": ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		if (loadAllAmbientSounds)
		{
			LoadAllAmbientSounds();
		}
	}

	public void ReleaseAllUnityAudio()
	{
		if (!assetBundlesLoaded)
		{
			requestReleaseUnityAudio = true;
			return;
		}
		for (int i = 0; i < unityAudio.Length; i++)
		{
			if (unityAudio[i] != null && !unityAudioCached[i])
			{
				for (int j = 0; j < unityAudio[i].Length; j++)
				{
					unityAudio[i][j] = null;
				}
			}
		}
		for (int k = 0; k < unityAudioLoaders.Length; k++)
		{
			if (unityAudioLoaders[k] != null && !unityAudioCached[k])
			{
				for (int l = 0; l < unityAudioLoaders[k].Length; l++)
				{
					unityAudioLoaders[k][l] = null;
				}
			}
		}
		ambientImporters.Clear();
		ambientClipsThroughUnity.Clear();
		ambientClipsThroughUnityLoaders.Clear();
	}

	public void Update()
	{
		if (!assetBundlesLoaded && rainWorld.assetBundlesInitialized)
		{
			if (!requestedAssetBundlesLoad)
			{
				requestedAssetBundlesLoad = true;
				AssetBundleManager.LoadAssetBundle("loadedsoundeffects");
				AssetBundleManager.LoadAssetBundle("loadedsoundeffects_ambient");
			}
			else
			{
				string error;
				LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("loadedsoundeffects", out error);
				LoadedAssetBundle loadedAssetBundle2 = AssetBundleManager.GetLoadedAssetBundle("loadedsoundeffects_ambient", out error);
				if (loadedAssetBundle != null && loadedAssetBundle2 != null)
				{
					loadedSoundEffectsAssetBundle = loadedAssetBundle;
					loadedSoundEffectsAmbientAssetBundle = loadedAssetBundle2;
					assetBundlesLoaded = true;
				}
			}
		}
		if (requestLoadSounds && assetBundlesLoaded)
		{
			requestLoadSounds = false;
			LoadSounds();
		}
		if (requestLoadAmbientSounds && assetBundlesLoaded)
		{
			requestLoadAmbientSounds = false;
			LoadAllAmbientSounds();
		}
		if (requestReleaseUnityAudio && assetBundlesLoaded)
		{
			requestReleaseUnityAudio = false;
			ReleaseAllUnityAudio();
		}
		if (assetBundlesLoaded)
		{
			for (int i = 0; i < unityAudioLoaders.Length; i++)
			{
				if (unityAudioLoaders[i] == null)
				{
					continue;
				}
				for (int j = 0; j < unityAudioLoaders[i].Length; j++)
				{
					if (unityAudioLoaders[i][j] != null && unityAudioLoaders[i][j].IsDone())
					{
						if (i < unityAudio.Length && unityAudio[i] != null && j < unityAudio[i].Length)
						{
							unityAudio[i][j] = unityAudioLoaders[i][j].GetAsset<AudioClip>();
						}
						unityAudioLoaders[i][j] = null;
					}
				}
			}
			if (ambientClipsThroughUnityLoaders.Count > 0)
			{
				List<string> list = null;
				foreach (KeyValuePair<string, AssetBundleLoadAssetOperation> ambientClipsThroughUnityLoader in ambientClipsThroughUnityLoaders)
				{
					if (ambientClipsThroughUnityLoader.Value.IsDone())
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(ambientClipsThroughUnityLoader.Key);
						AudioClip asset = ambientClipsThroughUnityLoader.Value.GetAsset<AudioClip>();
						asset.name = ambientClipsThroughUnityLoader.Key;
						ambientClipsThroughUnity.Add(asset);
					}
				}
				if (list != null)
				{
					for (int k = 0; k < list.Count; k++)
					{
						ambientClipsThroughUnityLoaders.Remove(list[k]);
					}
				}
			}
		}
		if (!(soundImporter != null) || !assetBundlesLoaded)
		{
			return;
		}
		for (int num = soundImporter.loadingClips.Count - 1; num >= 0; num--)
		{
			if (soundImporter.loadingClips[0].clip.loadState == AudioDataLoadState.Loaded || soundImporter.loadingClips[0].clip.loadState == AudioDataLoadState.Failed)
			{
				externalAudio[soundImporter.loadingClips[num].index.x][soundImporter.loadingClips[num].index.y] = soundImporter.loadingClips[num].clip;
				soundImporter.loadingClips.RemoveAt(num);
				clipsToBeLoaded--;
			}
		}
		if (clipsToBeLoaded < 1)
		{
			Custom.Log("All clips loaded and assigned!");
			soundImporter = null;
			Object.Destroy(gameObject);
			gameObject = null;
			loadingDone = true;
		}
	}

	private void RecordLineToVolumeGroups(List<VolumeGroup> activeVolumeGroups, int l)
	{
		foreach (VolumeGroup activeVolumeGroup in activeVolumeGroups)
		{
			activeVolumeGroup.affectLines.Add(l);
		}
	}

	private void VolumeGroupStopRecording(List<VolumeGroup> activeVolumeGroups, string name)
	{
		foreach (VolumeGroup activeVolumeGroup in activeVolumeGroups)
		{
			if (activeVolumeGroup.name == name)
			{
				activeVolumeGroups.Remove(activeVolumeGroup);
				break;
			}
		}
	}

	public float GroupVolume(int line)
	{
		float num = 1f;
		foreach (VolumeGroup volumeGroup in volumeGroups)
		{
			if (volumeGroup.affectLines.IndexOf(line) > -1)
			{
				num *= volumeGroup.vol;
			}
		}
		return num;
	}

	public void LoadSounds()
	{
		if (!assetBundlesLoaded)
		{
			requestLoadSounds = true;
			return;
		}
		errors = new List<string>();
		Custom.Log("Loading sounds");
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + "Sounds.txt"));
		volume = float.Parse(Regex.Split(array[0], ": ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		volumeExponent = float.Parse(Regex.Split(array[1], ": ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		volumeGroups = new List<VolumeGroup>();
		List<VolumeGroup> list = new List<VolumeGroup>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], " : ");
			if (array2[0] == "START VOLUME GROUP")
			{
				volumeGroups.Add(new VolumeGroup(array2[1], float.Parse(array2[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
				list.Add(volumeGroups[volumeGroups.Count - 1]);
			}
			if (array2[0] == "END VOLUME GROUP")
			{
				VolumeGroupStopRecording(list, array2[1]);
			}
			else if (array2.Length == 2 && (array2[0][0] != '/' || array2[0][1] != '/'))
			{
				list2.Add(i);
				RecordLineToVolumeGroups(list, i);
			}
		}
		int count = ExtEnum<SoundID>.values.Count;
		soundTriggers = new SoundTrigger[count];
		workingTriggers = new bool[count];
		List<string> list3 = new List<string>();
		List<bool> list4 = new List<bool>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Dictionary<int, string[]> dictionary2 = new Dictionary<int, string[]>();
		Dictionary<int, string[]> dictionary3 = new Dictionary<int, string[]>();
		Dictionary<int, string> dictionary4 = new Dictionary<int, string>();
		for (int j = 0; j < count; j++)
		{
			SoundID soundID = new SoundID(ExtEnum<SoundID>.values.GetEntry(j));
			string text = soundID.ToString().ToLowerInvariant();
			for (int k = 0; k < list2.Count; k++)
			{
				string[] array3;
				string[] array4;
				string text2;
				if (dictionary2.ContainsKey(list2[k]))
				{
					array3 = dictionary2[list2[k]];
					array4 = dictionary3[list2[k]];
					text2 = dictionary4[list2[k]];
				}
				else
				{
					array3 = Regex.Split(array[list2[k]], " : ");
					array4 = Regex.Split(array3[0], "/");
					text2 = array4[0].ToLowerInvariant();
					dictionary2.Add(list2[k], array3);
					dictionary3.Add(list2[k], array4);
					dictionary4.Add(list2[k], text2);
				}
				if (array3.Length == 0 || !(text2 == text))
				{
					continue;
				}
				bool flag = false;
				List<SoundPlayInstruction> list5 = new List<SoundPlayInstruction>();
				string[] array5 = Regex.Split(Custom.ValidateSpacedDelimiter(array3[1], ","), ", ");
				for (int l = 0; l < array5.Length; l++)
				{
					string[] array6 = Regex.Split(array5[l], "/");
					string text3 = array6[0];
					string text4 = text3.ToLowerInvariant();
					if (text4 == "samename")
					{
						text3 = array4[0];
						text4 = text3.ToLowerInvariant();
					}
					int num = -1;
					if (dictionary.ContainsKey(text4))
					{
						num = dictionary[text4];
					}
					if (num == -1)
					{
						if (CheckIfFileExistsAsUnityResource(text3))
						{
							list3.Add(text3);
							list4.Add(item: true);
							num = list3.Count - 1;
							dictionary.Add(text4, num);
						}
						else
						{
							if (!CheckIfFileExistsAsExternal(text3))
							{
								if (text3 != "")
								{
									errors.Add("Can't find file: " + text3);
								}
								flag = true;
								break;
							}
							list3.Add(text3);
							list4.Add(item: false);
							num = list3.Count - 1;
							dictionary.Add(text4, num);
						}
					}
					list5.Add(new SoundPlayInstruction(num, array6));
				}
				if (!flag)
				{
					soundTriggers[j] = new SoundTrigger(soundID, list5.ToArray(), GroupVolume(list2[k]), this, array4);
					workingTriggers[j] = true;
				}
				dictionary2.Remove(list2[k]);
				dictionary3.Remove(list2[k]);
				dictionary4.Remove(list2[k]);
				list2.RemoveAt(k);
				break;
			}
		}
		if (list2.Count > 0)
		{
			errors.Add("Non existent triggers:");
			for (int m = 0; m < list2.Count; m++)
			{
				errors.Add("     " + array[list2[m]]);
			}
		}
		audioClipNames = list3.ToArray();
		externalAudio = new AudioClip[audioClipNames.Length][];
		unityAudio = new AudioClip[audioClipNames.Length][];
		unityAudioLoaders = new AssetBundleLoadAssetOperation[audioClipNames.Length][];
		unityAudioCached = new bool[audioClipNames.Length];
		audioClipsThroughUnity = list4.ToArray();
		soundVariations = new int[audioClipNames.Length];
		for (int n = 0; n < soundVariations.Length; n++)
		{
			soundVariations[n] = VariationsForSound(audioClipNames[n]);
			if (!audioClipsThroughUnity[n])
			{
				externalAudio[n] = new AudioClip[soundVariations[n]];
				continue;
			}
			unityAudio[n] = new AudioClip[soundVariations[n]];
			unityAudioLoaders[n] = new AssetBundleLoadAssetOperation[soundVariations[n]];
		}
		SoundTrigger soundTrigger = soundTriggers.FirstOrDefault((SoundTrigger st) => st != null && st.soundID == SoundID.MENU_Main_Menu_LOOP);
		for (int num2 = 0; num2 < soundTriggers.Length; num2++)
		{
			if (soundTriggers[num2] != null)
			{
				_ = soundTriggers[num2];
			}
			if (soundTriggers[num2] == null || !soundTriggers[num2].cache)
			{
				continue;
			}
			for (int num3 = 0; num3 < soundTriggers[num2].Instructions; num3++)
			{
				int audioClip = soundTriggers[num2].GetSoundData(num3).audioClip;
				unityAudioCached[audioClip] = true;
				for (int num4 = 0; num4 < soundVariations[audioClip]; num4++)
				{
					string text5 = audioClipNames[audioClip];
					if (num4 > 0)
					{
						text5 = text5 + "_" + (1 + num4);
					}
					if (unityAudio[audioClip] == null || unityAudio[audioClip][num4] != null)
					{
						continue;
					}
					if (unityAudioLoaders[audioClip][num4] != null)
					{
						if (unityAudioLoaders[audioClip][num4].IsDone())
						{
							unityAudio[audioClip][num4] = unityAudioLoaders[audioClip][num4].GetAsset<AudioClip>();
							unityAudioLoaders[audioClip][num4] = null;
						}
					}
					else
					{
						unityAudioLoaders[audioClip][num4] = AssetBundleManager.LoadAssetAsync("loadedsoundeffects", text5, typeof(AudioClip));
					}
				}
			}
		}
		if (gameObject != null)
		{
			Object.Destroy(gameObject);
			gameObject = null;
		}
		gameObject = new GameObject("SoundLoader");
		soundImporter = gameObject.AddComponent<SoundImporter>();
		soundImporter.Init(this);
		clipsToBeLoaded = 0;
		for (int num5 = 0; num5 < externalAudio.Length; num5++)
		{
			if (!audioClipsThroughUnity[num5])
			{
				clipsToBeLoaded += externalAudio[num5].Length;
			}
		}
	}

	private bool CheckIfFileExistsAsExternal(string name)
	{
		if (!File.Exists(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + name + ".wav")) && !File.Exists(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + name + "_1.wav")) && !File.Exists(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + name + ".ogg")))
		{
			return File.Exists(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + name + "_1.ogg"));
		}
		return true;
	}

	private bool CheckIfFileExistsAsUnityResource(string name)
	{
		if (!loadedSoundEffectsAssetBundle.m_AssetBundle.Contains(name))
		{
			return loadedSoundEffectsAssetBundle.m_AssetBundle.Contains(name + "_1");
		}
		return true;
	}

	private int VariationsForSound(string name)
	{
		int num = 1;
		if (CheckIfFileExistsAsUnityResource(name))
		{
			for (int i = 0; i < 100; i++)
			{
				if (!loadedSoundEffectsAssetBundle.m_AssetBundle.Contains(name + "_" + (num + 1)))
				{
					break;
				}
				num++;
			}
		}
		else
		{
			for (int j = 0; j < 100; j++)
			{
				if (!File.Exists(AssetManager.ResolveFilePath("SoundEffects" + Path.DirectorySeparatorChar + name + "_" + (num + 1) + ".wav")))
				{
					break;
				}
				num++;
			}
		}
		return num;
	}

	public bool ShouldSoundPlay(SoundID soundID)
	{
		if (!assetBundlesLoaded)
		{
			return false;
		}
		if (soundID == null || soundID.Index == -1 || !workingTriggers[soundID.Index])
		{
			return false;
		}
		if (soundTriggers[soundID.Index].silentChance == 0f)
		{
			return true;
		}
		return Random.value > soundTriggers[soundID.Index].silentChance;
	}

	public SoundData GetSoundData(SoundID soundID)
	{
		if (soundID == null || soundID.Index == -1)
		{
			return new SoundData(SoundID.None, 0, 0f, 0f, 0f, 0f);
		}
		return soundTriggers[soundID.Index].GetRandomSoundData();
	}

	public SoundData GetSoundData(SoundID soundID, int i)
	{
		if (soundID == null || soundID.Index == -1)
		{
			return new SoundData(SoundID.None, 0, 0f, 0f, 0f, 0f);
		}
		return soundTriggers[soundID.Index].GetSoundData(i);
	}

	public bool TriggerPlayAll(SoundID soundID)
	{
		if (soundID == null || soundID.Index == -1)
		{
			return false;
		}
		return soundTriggers[soundID.Index].PlayAll;
	}

	public int TriggerSamples(SoundID soundID)
	{
		if (soundID == null || soundID.Index == -1)
		{
			return 0;
		}
		return soundTriggers[soundID.Index].Instructions;
	}

	public float TriggerGroupVolume(SoundID soundID)
	{
		if (soundID == null || soundID.Index == -1)
		{
			return 0f;
		}
		return soundTriggers[soundID.Index].GROUPVOL;
	}

	public bool DontLog(SoundID soundID)
	{
		if (!assetBundlesLoaded || soundID == null || soundID.Index == -1)
		{
			return true;
		}
		return soundTriggers[soundID.Index].DontLog;
	}

	public AudioClip GetAudioClip(int i, out AssetBundleLoadAssetOperation loadOp, out string name)
	{
		loadOp = null;
		if (audioClipsThroughUnity[i])
		{
			if (soundVariations[i] == 1)
			{
				name = audioClipNames[i];
				if (unityAudio[i][0] != null)
				{
					return unityAudio[i][0];
				}
				string text = "LoadedSoundEffects" + Path.DirectorySeparatorChar + name + ".wav";
				string text2 = AssetManager.ResolveFilePath(text);
				if (text2 != Path.Combine(Custom.RootFolderDirectory(), text.ToLowerInvariant()) && !File.Exists(text2))
				{
					text2 = AssetManager.ResolveFilePath("LoadedSoundEffects" + Path.DirectorySeparatorChar + name + ".ogg");
				}
				if (text2 != Path.Combine(Custom.RootFolderDirectory(), text.ToLowerInvariant()) && File.Exists(text2))
				{
					unityAudio[i][0] = AssetManager.SafeWWWAudioClip("file://" + text2, threeD: false, stream: true, text2.EndsWith("ogg") ? AudioType.OGGVORBIS : AudioType.WAV);
					return unityAudio[i][0];
				}
				if (unityAudioLoaders[i][0] != null)
				{
					if (unityAudioLoaders[i][0].IsDone())
					{
						unityAudio[i][0] = unityAudioLoaders[i][0].GetAsset<AudioClip>();
						unityAudioLoaders[i][0] = null;
						return unityAudio[i][0];
					}
					loadOp = unityAudioLoaders[i][0];
					return null;
				}
				string error;
				LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("loadedsoundeffects", out error);
				if (loadedAssetBundle != null)
				{
					unityAudio[i][0] = loadedAssetBundle.m_AssetBundle.LoadAsset<AudioClip>(audioClipNames[i]);
					return unityAudio[i][0];
				}
				unityAudioLoaders[i][0] = AssetBundleManager.LoadAssetAsync("loadedsoundeffects", audioClipNames[i], typeof(AudioClip));
				loadOp = unityAudioLoaders[i][0];
				return null;
			}
			int num = Random.Range(0, soundVariations[i]);
			name = audioClipNames[i] + "_" + (1 + num);
			if (unityAudio[i][num] != null)
			{
				return unityAudio[i][num];
			}
			string text3 = "LoadedSoundEffects" + Path.DirectorySeparatorChar + name + ".wav";
			string text4 = AssetManager.ResolveFilePath(text3);
			if (text4 != Path.Combine(Custom.RootFolderDirectory(), text3.ToLowerInvariant()) && !File.Exists(text4))
			{
				text4 = AssetManager.ResolveFilePath("LoadedSoundEffects" + Path.DirectorySeparatorChar + name + ".ogg");
			}
			if (text4 != Path.Combine(Custom.RootFolderDirectory(), text3.ToLowerInvariant()) && File.Exists(text4))
			{
				unityAudio[i][num] = AssetManager.SafeWWWAudioClip("file://" + text4, threeD: false, stream: true, text4.EndsWith("ogg") ? AudioType.OGGVORBIS : AudioType.WAV);
				return unityAudio[i][num];
			}
			if (unityAudioLoaders[i][num] != null)
			{
				if (unityAudioLoaders[i][num].IsDone())
				{
					unityAudio[i][num] = unityAudioLoaders[i][num].GetAsset<AudioClip>();
					unityAudioLoaders[i][num] = null;
					return unityAudio[i][num];
				}
				loadOp = unityAudioLoaders[i][num];
				return null;
			}
			string error2;
			LoadedAssetBundle loadedAssetBundle2 = AssetBundleManager.GetLoadedAssetBundle("loadedsoundeffects", out error2);
			if (loadedAssetBundle2 != null)
			{
				unityAudio[i][num] = loadedAssetBundle2.m_AssetBundle.LoadAsset<AudioClip>(audioClipNames[i] + "_" + (1 + num));
				return unityAudio[i][num];
			}
			unityAudioLoaders[i][num] = AssetBundleManager.LoadAssetAsync("loadedsoundeffects", audioClipNames[i] + "_" + (1 + num), typeof(AudioClip));
			loadOp = unityAudioLoaders[i][num];
			return null;
		}
		if (soundVariations[i] == 1)
		{
			name = externalAudio[i][0].name;
			return externalAudio[i][0];
		}
		AudioClip audioClip = externalAudio[i][Random.Range(0, soundVariations[i])];
		name = audioClip.name;
		return audioClip;
	}

	public void LoadAllAmbientSounds()
	{
		if (!assetBundlesLoaded)
		{
			requestLoadAmbientSounds = true;
			return;
		}
		string[] array = AssetManager.ListDirectory("soundeffects/ambient");
		for (int i = 0; i < array.Length; i++)
		{
			RequestAmbientAudioClip(Path.GetFileName(array[i]));
		}
	}

	public AudioClip RequestAmbientAudioClip(string clipName)
	{
		string text = "LoadedSoundEffects" + Path.DirectorySeparatorChar + "Ambient" + Path.DirectorySeparatorChar + clipName;
		string text2 = AssetManager.ResolveFilePath(text);
		if (text2 != Path.Combine(Custom.RootFolderDirectory(), text.ToLowerInvariant()) && File.Exists(text2))
		{
			for (int i = 0; i < ambientClipsThroughUnity.Count; i++)
			{
				if (ambientClipsThroughUnity[i].name == clipName)
				{
					return ambientClipsThroughUnity[i];
				}
			}
			AudioClip audioClip = AssetManager.SafeWWWAudioClip("file://" + text2, threeD: false, stream: true, text2.ToLower().EndsWith("wav") ? AudioType.WAV : AudioType.OGGVORBIS);
			audioClip.name = clipName;
			ambientClipsThroughUnity.Add(audioClip);
			return audioClip;
		}
		if (loadedSoundEffectsAmbientAssetBundle.m_AssetBundle.Contains(clipName))
		{
			for (int j = 0; j < ambientClipsThroughUnity.Count; j++)
			{
				if (ambientClipsThroughUnity[j].name == clipName)
				{
					return ambientClipsThroughUnity[j];
				}
			}
			if (ambientClipsThroughUnityLoaders.ContainsKey(clipName))
			{
				AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = ambientClipsThroughUnityLoaders[clipName];
				if (assetBundleLoadAssetOperation.IsDone())
				{
					AudioClip asset = assetBundleLoadAssetOperation.GetAsset<AudioClip>();
					asset.name = clipName;
					ambientClipsThroughUnity.Add(asset);
					ambientClipsThroughUnityLoaders.Remove(clipName);
					return asset;
				}
				return null;
			}
			string error;
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("loadedsoundeffects_ambient", out error);
			if (loadedAssetBundle != null)
			{
				AudioClip audioClip2 = loadedAssetBundle.m_AssetBundle.LoadAsset<AudioClip>(clipName.Substring(0, clipName.Length - 4));
				ambientClipsThroughUnity.Add(audioClip2);
				return audioClip2;
			}
			ambientClipsThroughUnityLoaders.Add(clipName, AssetBundleManager.LoadAssetAsync("loadedsoundeffects_ambient", clipName.Substring(0, clipName.Length - 4), typeof(AudioClip)));
			return null;
		}
		for (int k = 0; k < ambientImporters.Count; k++)
		{
			if (ambientImporters[k].fileName == clipName)
			{
				return ambientImporters[k].loadedClip;
			}
		}
		if (gameObject == null)
		{
			gameObject = new GameObject("SoundLoader");
		}
		AmbientImporter ambientImporter = gameObject.AddComponent<AmbientImporter>();
		ambientImporter.fileName = clipName;
		ambientImporters.Add(ambientImporter);
		ambientImporter.Init(this);
		return null;
	}
}
