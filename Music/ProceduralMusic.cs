using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AssetBundles;
using RWCustom;
using UnityEngine;

namespace Music;

public class ProceduralMusic : MusicPiece
{
	public class ProceduralMusicInstruction
	{
		public class Track
		{
			public string name;

			public List<string> tags;

			public bool remainInPanicMode;

			public string[] subRegions;

			public int dayNight;

			public bool mushroom;

			public Track(string name)
			{
				this.name = name;
				tags = new List<string>();
				subRegions = null;
				mushroom = false;
			}

			public bool AllowedInSubRegion(string subRegion)
			{
				if (subRegions == null)
				{
					return true;
				}
				for (int i = 0; i < subRegions.Length; i++)
				{
					if (subRegion == subRegions[i])
					{
						return true;
					}
				}
				return false;
			}

			public bool AllowedInDayNight(int dayNight)
			{
				if (this.dayNight != 0)
				{
					return this.dayNight == dayNight;
				}
				return true;
			}
		}

		public class Layer
		{
			public int layerIndex;

			public List<Track> tracks;

			public Layer(int layerIndex)
			{
				this.layerIndex = layerIndex;
				tracks = new List<Track>();
			}
		}

		public string name;

		public List<Track> tracks;

		public List<Layer> layers;

		public ProceduralMusicInstruction(string name)
		{
			this.name = name;
			layers = new List<Layer>();
			tracks = new List<Track>();
			string path = AssetManager.ResolveFilePath(string.Concat("Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar, name, ".txt"));
			if (!File.Exists(path))
			{
				return;
			}
			string[] array = File.ReadAllLines(path);
			LoadedAssetBundle loadedAssetBundle = null;
			loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("music_procedural", out var _);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], " : ");
				if (array2.Length != 0 && array2[0].Length > 4 && array2[0] == "Layer")
				{
					layers.Add(new Layer(layers.Count));
					string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
					for (int j = 0; j < array3.Length; j++)
					{
						if (array3[j].Length <= 0)
						{
							continue;
						}
						for (int k = 0; k < tracks.Count; k++)
						{
							string text = "";
							string text2 = "";
							if (array3[j].Length > 3 && array3[j].Substring(0, 1) == "{" && array3[j].Contains("}"))
							{
								text = array3[j].Substring(1, array3[j].IndexOf("}") - 1);
								text2 = array3[j].Substring(array3[j].IndexOf("}") + 1);
							}
							else
							{
								text2 = array3[j];
							}
							if (!(text2 == tracks[k].name))
							{
								continue;
							}
							string[] subRegions = null;
							int dayNight = 0;
							bool mushroom = false;
							if (text != "")
							{
								switch (text)
								{
								case "D":
									dayNight = 1;
									break;
								case "N":
									dayNight = 2;
									break;
								case "M":
									mushroom = true;
									break;
								default:
									subRegions = text.Split('|');
									break;
								}
							}
							tracks[k].subRegions = subRegions;
							tracks[k].dayNight = dayNight;
							tracks[k].mushroom = mushroom;
							layers[layers.Count - 1].tracks.Add(tracks[k]);
							break;
						}
					}
				}
				else
				{
					if (array2.Length == 0 || array2[0].Length <= 0 || !loadedAssetBundle.m_AssetBundle.Contains(array2[0]))
					{
						continue;
					}
					tracks.Add(new Track(array2[0]));
					string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
					for (int l = 0; l < array4.Length; l++)
					{
						if (array4[l].Length > 0)
						{
							if (array4[l] == "<PA>")
							{
								tracks[tracks.Count - 1].remainInPanicMode = true;
							}
							else
							{
								tracks[tracks.Count - 1].tags.Add(array4[l]);
							}
						}
					}
				}
			}
		}
	}

	public ProceduralMusicInstruction instruction;

	public int audibleCounter;

	public int silentCounter;

	public bool reScrambleOnNextSilence;

	public List<bool> remainInPanicMode;

	private string lastValidSubregion;

	private int lastValidDaynight;

	public List<bool> mushroomSubTrack;

	public ProceduralMusic(MusicPlayer musicPlayer, string name)
		: base(musicPlayer, name, MusicPlayer.MusicContext.StoryMode)
	{
		remainInPanicMode = new List<bool>();
		mushroomSubTrack = new List<bool>();
		instruction = new ProceduralMusicInstruction(name);
		Reset();
		volume = 0f;
	}

	public void Reset()
	{
		if (musicPlayer.manager.currentMainLoop is RainWorldGame)
		{
			(musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].virtualMicrophone.Log("SCRAMBLING PROC. MUSIC");
		}
		for (int i = 0; i < subTracks.Count; i++)
		{
			subTracks[i].StopAndDestroy();
		}
		subTracks.Clear();
		remainInPanicMode.Clear();
		mushroomSubTrack.Clear();
		List<ProceduralMusicInstruction.Track> list = new List<ProceduralMusicInstruction.Track>();
		for (int j = 0; j < instruction.layers.Count; j++)
		{
			List<ProceduralMusicInstruction.Track> list2 = new List<ProceduralMusicInstruction.Track>();
			for (int k = 0; k < instruction.layers[j].tracks.Count; k++)
			{
				if (list.Contains(instruction.layers[j].tracks[k]))
				{
					continue;
				}
				if (musicPlayer.manager.currentMainLoop is RainWorldGame)
				{
					int num = -1;
					string subRegion = null;
					if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].game.world.region != null)
					{
						if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud == null || (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt == null || (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.subregionTracker == null)
						{
							for (int l = 1; l < (musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions.Count; l++)
							{
								if ((musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions[l] == (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].room.abstractRoom.subregionName)
								{
									num = l;
									reScrambleOnNextSilence = true;
									break;
								}
							}
						}
						else
						{
							num = (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.subregionTracker.lastShownRegion;
						}
						if (num >= 1 && num < (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].game.world.region.subRegions.Count)
						{
							subRegion = (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].game.world.region.subRegions[num];
						}
						if (instruction.layers[j].tracks[k].AllowedInSubRegion(subRegion) && instruction.layers[j].tracks[k].AllowedInDayNight(lastValidDaynight) && !instruction.layers[j].tracks[k].mushroom)
						{
							list2.Add(instruction.layers[j].tracks[k]);
							lastValidSubregion = subRegion;
						}
					}
					else
					{
						list2.Add(instruction.layers[j].tracks[k]);
					}
					if (instruction.layers[j].tracks[k].mushroom)
					{
						list2.Add(instruction.layers[j].tracks[k]);
					}
				}
				else
				{
					list2.Add(instruction.layers[j].tracks[k]);
				}
			}
			if (list2.Count > 0)
			{
				ProceduralMusicInstruction.Track track = list2[UnityEngine.Random.Range(0, list2.Count)];
				list.Add(track);
				if (musicPlayer.manager.currentMainLoop is RainWorldGame)
				{
					(musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].virtualMicrophone.Log("L" + subTracks.Count + " - " + track.name);
				}
				SubTrack subTrack = new SubTrack(this, j, track.name);
				subTrack.piece.volume = 0f;
				subTracks.Add(subTrack);
				remainInPanicMode.Add(track.remainInPanicMode);
				mushroomSubTrack.Add(track.mushroom);
			}
		}
		startedPlaying = false;
		base.Loop = true;
		playWhenReady = true;
	}

	public override void Update()
	{
		base.Update();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (musicPlayer.manager.currentMainLoop is RainWorldGame)
		{
			num3 = (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].mushroomMode;
		}
		float num4 = 0f;
		if (musicPlayer.threatTracker != null)
		{
			num4 = musicPlayer.threatTracker.currentThreat;
		}
		num4 = ((!(num4 < 0.05f)) ? ((num4 - 0.05f) / 0.95f) : 0f);
		for (int i = 0; i < subTracks.Count; i++)
		{
			if (musicPlayer.threatTracker != null)
			{
				if (!mushroomSubTrack[i])
				{
					subTracks[i].volume = Mathf.InverseLerp((float)i / (float)subTracks.Count, (float)(i + 1) / (float)subTracks.Count, num4);
					subTracks[i].volume = Mathf.InverseLerp(0f, 0.5f, subTracks[i].volume);
				}
				else if (num3 > 0f)
				{
					subTracks[i].volume = num3;
				}
				else
				{
					subTracks[i].volume = Math.Max(0f, subTracks[i].volume - 0.025f);
				}
			}
			else
			{
				subTracks[i].volume = Math.Max(0f, subTracks[i].volume - 0.025f);
			}
			num = Mathf.Max(num, subTracks[i].volume);
			if (i == 0)
			{
				num2 = subTracks[0].source.time;
			}
			else if (ModManager.MMF)
			{
				float num5 = num2 / subTracks[i].source.clip.length;
				float num6 = Mathf.DeltaAngle(subTracks[i].source.time / subTracks[i].source.clip.length * 360f, num5 * 360f);
				float num7 = 0.008f;
				float num8 = Mathf.Clamp(1f + num6 / 3f, 1f - num7, 1f + num7);
				if (num8 != float.NegativeInfinity && num8 != float.PositiveInfinity)
				{
					subTracks[i].source.pitch = num8;
				}
			}
		}
		volume = Custom.LerpAndTick(volume, musicPlayer.droneGoalMix, 0.0003f, 0.0023809525f);
		num *= volume;
		if (num > 0f)
		{
			audibleCounter++;
			silentCounter = 0;
			if (audibleCounter == 150)
			{
				reScrambleOnNextSilence = true;
			}
			return;
		}
		audibleCounter = 0;
		silentCounter++;
		if (silentCounter == 150 && reScrambleOnNextSilence)
		{
			Reset();
			reScrambleOnNextSilence = false;
		}
		if (silentCounter % 150 != 0)
		{
			return;
		}
		int num9 = 0;
		if (musicPlayer.manager.currentMainLoop is RainWorldGame)
		{
			if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud != null && (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt != null)
			{
				if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.subregionTracker == null)
				{
					if ((musicPlayer.manager.currentMainLoop as RainWorldGame).world.region != null)
					{
						for (int j = 1; j < (musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions.Count; j++)
						{
							if ((musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions[j] == (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].room.abstractRoom.subregionName)
							{
								num9 = j;
								reScrambleOnNextSilence = true;
								break;
							}
						}
					}
				}
				else
				{
					num9 = (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].hud.textPrompt.subregionTracker.lastShownRegion;
				}
			}
		}
		else
		{
			audibleCounter = 0;
			silentCounter = 150;
			reScrambleOnNextSilence = false;
			volume = 0f;
		}
		string text = null;
		if (musicPlayer.manager.currentMainLoop is RainWorldGame && (musicPlayer.manager.currentMainLoop as RainWorldGame).world.region != null && num9 >= 1 && num9 < (musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions.Count)
		{
			text = (musicPlayer.manager.currentMainLoop as RainWorldGame).world.region.subRegions[num9];
		}
		if (num9 != 0 && lastValidSubregion != text)
		{
			silentCounter = 100;
			reScrambleOnNextSilence = true;
			lastValidSubregion = text;
			Custom.Log("Reactivate scrambling, new sub region!");
		}
		if (!(musicPlayer.manager.currentMainLoop is RainWorldGame))
		{
			return;
		}
		if ((musicPlayer.manager.currentMainLoop as RainWorldGame).world.rainCycle.AmountLeft < 0f)
		{
			if (lastValidDaynight != 2)
			{
				reScrambleOnNextSilence = true;
				silentCounter = 100;
				lastValidDaynight = 2;
				Custom.Log("Procedural music night trigger");
			}
		}
		else if (lastValidDaynight != 1)
		{
			reScrambleOnNextSilence = true;
			silentCounter = 100;
			lastValidDaynight = 1;
			Custom.Log("Procedural music day trigger");
		}
	}
}
