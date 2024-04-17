using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using DevInterface;
using Expedition;
using JollyCoop;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MoreSlugcats;
using On;
using RWCustom;
using Steamworks;
using UnityEngine;

public class ModManager
{
	public enum ModApplyFallbackStep
	{
		None,
		PreviousEnabled,
		Vanilla
	}

	public class Mod
	{
		public string id = "Unknown";

		public string name = "Unknown";

		public string path = "Unknown";

		public string basePath = "Unknown";

		public string version = "";

		public string targetGameVersion = "v1.9.15b";

		public string authors = "Unknown";

		public string description = "No Description.";

		public string descBlank;

		public string trailerID;

		public string[] requirements = new string[0];

		public string[] requirementsNames = new string[0];

		public string[] tags = new string[0];

		public string[] priorities = new string[0];

		public bool enabled;

		public string checksum = "";

		public bool checksumChanged;

		public bool checksumOverrideVersion;

		public bool hideVersion;

		public int loadOrder;

		public bool modifiesRegions;

		public bool workshopMod;

		public ulong workshopId;

		public bool hasDLL;

		public bool hasNewestFolder;

		public bool hasTargetedVersionFolder;

		public const string authorBlank = "Unknown";

		public string LocalizedName
		{
			get
			{
				string text = OptionInterface.Translate(id + "-name");
				if (PrePackagedModIDs.Contains(id))
				{
					InGameTranslator.LanguageID currentLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
					if (currentLanguage != InGameTranslator.LanguageID.Chinese && currentLanguage != InGameTranslator.LanguageID.Korean && currentLanguage != InGameTranslator.LanguageID.Japanese)
					{
						if (!string.IsNullOrEmpty(name))
						{
							return name;
						}
						return id;
					}
				}
				if (text == id + "-name")
				{
					if (!string.IsNullOrEmpty(name))
					{
						return name;
					}
					return id;
				}
				return text;
			}
		}

		public string LocalizedDescription
		{
			get
			{
				string text = OptionInterface.Translate(id + "-description");
				if (text == id + "-description")
				{
					return description;
				}
				return text;
			}
		}

		public string TargetedPath => Path.Combine(path, "v1.9.15b");

		public string NewestPath => Path.Combine(path, "newest");

		public bool DLCMissing
		{
			get
			{
				if (Custom.rainWorld.dlcVersion < 1)
				{
					if (!(id == global::MoreSlugcats.MoreSlugcats.MOD_ID) && !(id == global::JollyCoop.JollyCoop.MOD_ID))
					{
						return id == global::Expedition.Expedition.MOD_ID;
					}
					return true;
				}
				return false;
			}
		}

		public string GetThumbnailPath()
		{
			string result = basePath + Path.DirectorySeparatorChar + "thumbnail.png";
			if (!File.Exists(result))
			{
				result = path + Path.DirectorySeparatorChar + "thumbnail.png";
			}
			return result;
		}
	}

	public static class MapMerger
	{
		public struct MergeMapData
		{
			public string region;

			public string slugcat;

			public string fileName;

			public Vector2 pngOffset;

			public Mod sourceMod;

			public string directory;

			public string MapKey => region + ((slugcat == "") ? "" : ("-" + slugcat));

			public string FullFilePath => Path.Combine(new string[4]
			{
				directory,
				"world",
				region,
				fileName + ".png"
			});

			public MergeMapData(string region, string slugcat, string fileName, string directory, Vector2 offset, Mod sourceMod)
			{
				this.region = region;
				this.slugcat = slugcat;
				this.fileName = fileName;
				this.directory = directory;
				pngOffset = offset;
				this.sourceMod = sourceMod;
			}

			public static MergeMapData GenerateDefault(string sourcePath, Mod modApplyFrom, bool baseFile = false)
			{
				string text = "";
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
				string text2 = fileNameWithoutExtension.Replace("map_", "");
				if (fileNameWithoutExtension.Contains('-') && fileNameWithoutExtension.Split('-').Length == 2)
				{
					text2 = fileNameWithoutExtension.Split('-')[0];
					text = fileNameWithoutExtension.Split('-')[1];
				}
				string text3 = ResolveVersionSource(modApplyFrom, Path.Combine(new string[3]
				{
					"world",
					text2,
					fileNameWithoutExtension + ".txt"
				}), modifyFile: true);
				return new MergeMapData(text2, text, Path.GetFileNameWithoutExtension(sourcePath), text3, default(Vector2), baseFile ? null : modApplyFrom);
			}
		}

		public struct MapRoomLine
		{
			public string roomName;

			public Vector2 canonPos;

			public Vector2 devPos;

			public IntVector2 roomSize;

			public string subregion;

			public int layer;

			public MapRoomLine(string roomName, Vector2 canonPos, Vector2 devPos, int layer, string subregion, IntVector2 roomSize)
			{
				this.roomName = roomName;
				this.canonPos = canonPos;
				this.devPos = devPos;
				this.subregion = subregion;
				this.layer = layer;
				this.roomSize = roomSize;
			}

			public override string ToString()
			{
				string text = roomName + ": " + string.Join("><", canonPos.x, canonPos.y, devPos.x, devPos.y, layer, subregion);
				if (roomSize != default(IntVector2))
				{
					text = string.Join("><", text, roomSize.x, roomSize.y);
				}
				return text;
			}

			public static bool TryParse(string line, out MapRoomLine roomLine)
			{
				roomLine = default(MapRoomLine);
				try
				{
					string[] array = Regex.Split(line, ": ");
					roomLine.roomName = array[0];
					string[] array2 = Regex.Split(array[1], "><");
					roomLine.canonPos.x = float.Parse(array2[0]);
					roomLine.canonPos.y = float.Parse(array2[1]);
					if (array2.Length >= 4)
					{
						roomLine.devPos.x = float.Parse(array2[2]);
						roomLine.devPos.y = float.Parse(array2[3]);
					}
					else
					{
						roomLine.devPos = roomLine.canonPos;
					}
					if (array2.Length >= 5)
					{
						roomLine.layer = int.Parse(array2[4]);
					}
					if (array2.Length >= 6)
					{
						roomLine.subregion = array2[5];
					}
					if (array2.Length >= 8)
					{
						roomLine.roomSize.x = int.Parse(array2[6]);
						roomLine.roomSize.y = int.Parse(array2[7]);
					}
				}
				catch (Exception)
				{
					if (line.Trim() != "" && !line.StartsWith("//"))
					{
						Custom.LogWarning("Failed to parse map room line", line);
					}
					return false;
				}
				return true;
			}

			public void RemoveOffset(Vector2 offset)
			{
				canonPos -= offset;
			}
		}

		public struct MapConnectionLine
		{
			public string roomNameA;

			public string roomNameB;

			public IntVector2 posA;

			public IntVector2 posB;

			public int dirA;

			public int dirB;

			public MapConnectionLine(string roomNameA, string roomNameB, IntVector2 posA, IntVector2 posB, int dirA, int dirB)
			{
				this.roomNameA = roomNameA;
				this.roomNameB = roomNameB;
				this.posA = posA;
				this.posB = posB;
				this.dirA = dirA;
				this.dirB = dirB;
			}

			public static bool TryParse(string line, out MapConnectionLine connectionLine)
			{
				connectionLine = default(MapConnectionLine);
				try
				{
					string[] array = Regex.Split(Regex.Split(line, ":")[1].Trim(), ",");
					connectionLine.roomNameA = array[0];
					connectionLine.roomNameB = array[1];
					connectionLine.posA = new IntVector2(int.Parse(array[2]), int.Parse(array[3]));
					connectionLine.posB = new IntVector2(int.Parse(array[4]), int.Parse(array[5]));
					connectionLine.dirA = int.Parse(array[6]);
					connectionLine.dirB = int.Parse(array[7]);
				}
				catch (Exception)
				{
					if (line.Trim() != "" && !line.StartsWith("//"))
					{
						Custom.LogWarning("Failed to parse map connection line " + line);
					}
					return false;
				}
				return true;
			}

			public override string ToString()
			{
				return "Connection: " + string.Join(",", roomNameA, roomNameB, posA.x, posA.y, posB.x, posB.y, dirA, dirB);
			}

			public bool SoftEquals(MapConnectionLine line)
			{
				if (!(roomNameA == line.roomNameB) || !(roomNameB == line.roomNameA) || dirA != line.dirB || dirB != line.dirA)
				{
					if (roomNameA == line.roomNameA && roomNameB == line.roomNameB && dirA == line.dirA)
					{
						return dirB == line.dirB;
					}
					return false;
				}
				return true;
			}

			public override bool Equals(object obj)
			{
				if (obj is MapConnectionLine mapConnectionLine && roomNameA == mapConnectionLine.roomNameB && roomNameB == mapConnectionLine.roomNameA && posA == mapConnectionLine.posB && posB == mapConnectionLine.posA && dirA == mapConnectionLine.dirB && dirB == mapConnectionLine.dirA)
				{
					return true;
				}
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return (roomNameA.GetHashCode() ^ posA.GetHashCode() ^ dirA.GetHashCode()) + (roomNameB.GetHashCode() ^ posB.GetHashCode() ^ dirB.GetHashCode());
			}
		}

		public static string ResolveVersionSource(Mod mod, string filePath, bool modifyFile = false)
		{
			if (mod == null)
			{
				return Custom.RootFolderDirectory();
			}
			string text = (modifyFile ? (Path.DirectorySeparatorChar + "modify") : "");
			if (mod.hasTargetedVersionFolder && File.Exists(Path.Combine(mod.TargetedPath, filePath.ToLowerInvariant())))
			{
				return mod.TargetedPath + text;
			}
			if (mod.hasNewestFolder && File.Exists(Path.Combine(mod.NewestPath, filePath.ToLowerInvariant())))
			{
				return mod.NewestPath + text;
			}
			return mod.path + text;
		}

		public static void MergeMapFiles(ModMerger merger, Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			Dictionary<string, MapRoomLine> dictionary = new Dictionary<string, MapRoomLine>();
			List<MapConnectionLine> list = new List<MapConnectionLine>();
			List<string> list2 = new List<string>();
			List<List<string>> list3 = new List<List<string>>();
			list3.Add(new List<string>());
			int num = 0;
			string[] array = mergeLines;
			foreach (string text in array)
			{
				if (text.StartsWith("R:"))
				{
					string[] array2 = text.Split(':');
					if (text.Split(':').Length >= 3 && MapRoomLine.TryParse(array2[1].Trim() + ": " + array2[2], out var _))
					{
						num++;
						list3.Add(new List<string>());
					}
					else
					{
						Custom.LogWarning("failed to recognize map merge Reference line:", text);
					}
				}
				list3[num].Add(text);
			}
			Vector2 vector = default(Vector2);
			for (int j = 0; j < list3.Count + 1; j++)
			{
				if (j > 0 && list3[j - 1].Count == 0)
				{
					continue;
				}
				Dictionary<string, MapRoomLine> dictionary2 = new Dictionary<string, MapRoomLine>();
				List<string> list4 = new List<string>();
				Vector2 vector2 = default(Vector2);
				MergeMapData item = MergeMapData.GenerateDefault(sourcePath, modApplyFrom, j == 0);
				array = ((j == 0) ? File.ReadAllLines(sourcePath) : list3[j - 1].ToArray());
				foreach (string text2 in array)
				{
					MapRoomLine roomLine3;
					if (text2.StartsWith("Def_Mat:"))
					{
						list2.Add(text2);
					}
					else if (text2.StartsWith("Connection:"))
					{
						if (!MapConnectionLine.TryParse(text2, out var connectionLine))
						{
							continue;
						}
						for (int num2 = list.Count - 1; num2 >= 0; num2--)
						{
							if (list[num2].SoftEquals(connectionLine))
							{
								list.RemoveAt(num2);
							}
						}
						list.Add(connectionLine);
					}
					else if (text2.StartsWith("R:"))
					{
						string[] array3 = text2.Split(':');
						if (array3.Length >= 3 && MapRoomLine.TryParse(array3[1].Trim() + ": " + array3[2], out var roomLine2) && TryFindRoomOffset(dictionary, roomLine2, out var offset))
						{
							vector2 = offset;
							dictionary2[roomLine2.roomName] = roomLine2;
						}
					}
					else if (text2.StartsWith("I:"))
					{
						string[] array4 = text2.Split(':');
						if (array4.Length >= 2)
						{
							item.fileName = array4[1].Trim().ToLower();
							Custom.Log("custom image file to merge:", item.FullFilePath);
						}
					}
					else if (MapRoomLine.TryParse(text2, out roomLine3))
					{
						dictionary2[roomLine3.roomName] = roomLine3;
						if (roomLine3.roomSize == default(IntVector2))
						{
							list4.Add(roomLine3.roomName);
						}
					}
				}
				if (list4.Count > 0)
				{
					LocateMissingRoomSizes(list4, dictionary2);
				}
				Vector2 vector3 = BottomLeftLocator(dictionary2);
				if (j == 0)
				{
					dictionary = dictionary2;
					vector = vector3;
					continue;
				}
				if (vector2 == default(Vector2))
				{
					vector2 = FindMapOffset(dictionary, dictionary2);
				}
				foreach (MapRoomLine value in dictionary2.Values)
				{
					value.RemoveOffset(vector2);
					dictionary[value.roomName] = value;
				}
				item.pngOffset = vector3 - vector2 - vector;
				item.pngOffset = new Vector2((float)Math.Ceiling(item.pngOffset.x / 3f), (float)Math.Ceiling(item.pngOffset.y / 3f));
				if (File.Exists(item.FullFilePath))
				{
					merger.modMapData.Add(item);
				}
			}
			string[] array5 = new string[dictionary.Keys.Count + list.Count + list2.Count];
			int num3 = 0;
			foreach (KeyValuePair<string, MapRoomLine> item2 in dictionary)
			{
				array5[num3] = item2.Value.ToString();
				num3++;
			}
			foreach (MapConnectionLine item3 in list)
			{
				array5[num3] = item3.ToString();
				num3++;
			}
			foreach (string item4 in list2)
			{
				array5[num3] = item4;
				num3++;
			}
			ModMerger.WriteMergedFile(modApplyFrom, sourcePath, array5);
		}

		private static bool TryFindRoomOffset(Dictionary<string, MapRoomLine> vanillaRoomLines, MapRoomLine room, out Vector2 offset)
		{
			offset = default(Vector2);
			if (vanillaRoomLines.TryGetValue(room.roomName, out var value))
			{
				offset = room.canonPos - value.canonPos;
				return true;
			}
			return false;
		}

		private static Vector2 FindMapOffset(Dictionary<string, MapRoomLine> vanillaRoomLines, Dictionary<string, MapRoomLine> newRoomLines)
		{
			if (vanillaRoomLines.Count <= 0 || newRoomLines.Count <= 0)
			{
				return Vector2.zero;
			}
			List<float> list = new List<float>();
			List<float> list2 = new List<float>();
			foreach (MapRoomLine value in newRoomLines.Values)
			{
				if (TryFindRoomOffset(vanillaRoomLines, value, out var offset))
				{
					list.Add(offset.x);
					list2.Add(offset.y);
				}
			}
			if (list.Count <= 1)
			{
				return Vector2.zero;
			}
			return new Vector2((float)Math.Round(GetMode(list), 5), (float)Math.Round(GetMode(list2), 5));
		}

		private static float GetMode(List<float> arrSource)
		{
			Dictionary<float, int> dictionary = new Dictionary<float, int>();
			foreach (float item in arrSource)
			{
				if (dictionary.ContainsKey(item))
				{
					dictionary[item]++;
				}
				else
				{
					dictionary.Add(item, 1);
				}
			}
			return dictionary.OrderByDescending((KeyValuePair<float, int> x) => x.Value).First().Key;
		}

		public static void LocateMissingRoomSizes(List<string> roomsToLocate, Dictionary<string, MapRoomLine> dict)
		{
			foreach (string item in roomsToLocate)
			{
				try
				{
					if (!item.StartsWith("offscreenden", StringComparison.InvariantCultureIgnoreCase))
					{
						string[] array = File.ReadAllLines(WorldLoader.FindRoomFile(item, includeRootDirectory: false, ".txt"))[1].Split('|')[0].Split('*');
						MapRoomLine value = dict[item];
						value.roomSize = new IntVector2(int.Parse(array[0]), int.Parse(array[1]));
						dict[item] = value;
					}
				}
				catch (Exception)
				{
					if (!item.StartsWith("offscreenden", StringComparison.InvariantCultureIgnoreCase))
					{
						Custom.LogWarning("Could not read file for room " + item + " while merging mods");
					}
				}
			}
		}

		public static Vector2 BottomLeftLocator(Dictionary<string, MapRoomLine> mapRoomLines)
		{
			Vector2 result = new Vector2(float.MaxValue, float.MaxValue);
			foreach (MapRoomLine value in mapRoomLines.Values)
			{
				Vector2 vector = value.canonPos - Custom.IntVector2ToVector2(value.roomSize) * 3f * 0.5f;
				if (vector.x < result.x)
				{
					result.x = vector.x;
				}
				if (vector.y < result.y)
				{
					result.y = vector.y;
				}
			}
			return result;
		}

		public static void MergeWorldMaps(ModMerger merger, ModApplyer applyer)
		{
			applyer.worldMapImages = new Dictionary<string, List<string>>();
			applyer.worldMapImageMetas = new Dictionary<string, List<string>>();
			List<string> list = new List<string>();
			foreach (string item in Region.GetFullRegionOrder())
			{
				string[] array = AssetManager.ListDirectory(Path.Combine("world", item));
				foreach (string text in array)
				{
					if (!Path.GetFileNameWithoutExtension(text).ToLower().Contains("map_") || Path.GetExtension(text) != ".png")
					{
						continue;
					}
					string text2 = Path.GetFileNameWithoutExtension(text).Replace("map_", "");
					foreach (MergeMapData modMapDatum in merger.modMapData)
					{
						if (!(modMapDatum.MapKey.ToLower() != text2))
						{
							if (!applyer.worldMaps.ContainsKey(text2))
							{
								applyer.worldMaps[text2] = new List<MergeMapData>();
							}
							applyer.worldMaps[text2].Add(modMapDatum);
							if (!list.Contains(text2))
							{
								list.Add(text2);
							}
							applyer.worldMapImages[text2] = new List<string> { text };
						}
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			applyer.mapsToGenerate = list.Count;
			applyer.mapsGeneratedSoFar = 0;
			foreach (string item2 in list)
			{
				applyer.nextMapKeyToGenerate = item2;
				while (applyer.nextMapKeyToGenerate != null)
				{
					Thread.Sleep(0);
				}
				applyer.mapsGeneratedSoFar++;
			}
			applyer.finalizeApplyingMaps = true;
			while (applyer.finalizeApplyingMaps)
			{
				Thread.Sleep(0);
			}
			applyer.mapsToGenerate = 0;
			applyer.worldMapImages = null;
			applyer.worldMapImageMetas = null;
		}

		public static void MergeMap(ModApplyer applyer, string mapKey)
		{
			Custom.Log("map merging:", mapKey);
			byte[] data = File.ReadAllBytes(applyer.worldMapImages[mapKey][0]);
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(data);
			Texture2D texture2D2 = new Texture2D(1, 1);
			Vector2 mapOrigin = Vector2.zero;
			foreach (MergeMapData item in applyer.worldMaps[mapKey])
			{
				Custom.Log($"applying modification from mod: {item.sourceMod}, image file: {item.fileName}, relative bottom left is: {item.pngOffset}");
				data = File.ReadAllBytes(item.FullFilePath);
				texture2D2.LoadImage(data);
				GraftOnto(texture2D, texture2D2, ref mapOrigin, item.pngOffset);
			}
			string path = mapKey.Split('-')[0];
			string path2 = Path.Combine(Custom.RootFolderDirectory(), "mergedmods", "world", path);
			PNGSaver.SaveTextureToFile(texture2D, Path.Combine(path2, "map_" + mapKey + ".png").ToLowerInvariant());
			UnityEngine.Object.Destroy(texture2D);
			UnityEngine.Object.Destroy(texture2D2);
		}

		private static void GraftOnto(Texture2D mapTexture, Texture2D localTexture, ref Vector2 mapOrigin, Vector2 localBottomLeft)
		{
			int num = mapTexture.height / 3;
			int num2 = localTexture.height / 3;
			int num3 = Mathf.Max(localTexture.width + (int)localBottomLeft.x, mapTexture.width + (int)mapOrigin.x) - Mathf.Min((int)mapOrigin.x, (int)localBottomLeft.x);
			int num4 = Mathf.Max(num2 + (int)localBottomLeft.y, num + (int)mapOrigin.y) - Mathf.Min((int)mapOrigin.y, (int)localBottomLeft.y);
			int num5 = (int)(localBottomLeft.x - mapOrigin.x);
			int num6 = (int)(localBottomLeft.y - mapOrigin.y);
			if (mapTexture.width < num3 || num < num4)
			{
				OmniResize(mapTexture, Math.Max(0, -num5), Math.Max(0, -num6), num3 - mapTexture.width, num4 - num);
			}
			if (localTexture.width < num3 || num2 < num4)
			{
				OmniResize(localTexture, Math.Max(0, num5), Math.Max(0, num6), num3 - localTexture.width, num4 - num2);
			}
			mapOrigin = new Vector2(Math.Min(mapOrigin.x, localBottomLeft.x), Math.Min(mapOrigin.y, localBottomLeft.y));
			TransBlit(mapTexture, localTexture);
		}

		public static void OmniResize(Texture2D texture, int xOrigin, int yOrigin, int xAmount, int yAmount)
		{
			Custom.Log($"Resize xOrigin [{xOrigin}], yOrigin [{yOrigin}], xAmount [{xAmount}], yAmount [{yAmount}]");
			Texture2D texture2D = new Texture2D(texture.width + xAmount, texture.height + yAmount * 3, texture.format, mipChain: false);
			FillGreen(texture2D);
			for (int num = 2; num >= 0; num--)
			{
				Graphics.CopyTexture(texture, 0, 0, 0, num * (texture.height / 3), texture.width, texture.height / 3, texture2D, 0, 0, xOrigin, yOrigin + num * (texture2D.height / 3));
			}
			texture.LoadImage(texture2D.EncodeToPNG());
			UnityEngine.Object.Destroy(texture2D);
		}

		public static void FillGreen(Texture2D texture, int X, int Y, int width, int height)
		{
			Color32[] array = Enumerable.Repeat((Color32)new Color(0f, 1f, 0f), width * height).ToArray();
			if (array.Count() != 0)
			{
				texture.SetPixels32(X, Y, width, height, array);
			}
		}

		public static void FillGreen(Texture2D texture)
		{
			Color32[] pixels = Enumerable.Repeat((Color32)new Color(0f, 1f, 0f), texture.width * texture.height).ToArray();
			texture.SetPixels32(pixels);
		}

		public static void TransBlit(Texture2D texture, Texture2D texture2)
		{
			Color32 color = new Color32(0, byte.MaxValue, 0, byte.MaxValue);
			Color32[] pixels = texture.GetPixels32();
			Color32[] pixels2 = texture2.GetPixels32();
			for (int i = 0; i < texture.width; i++)
			{
				for (int j = 0; j < texture.height; j++)
				{
					Color32 color2 = pixels2[i + j * texture.width];
					if (color2.r != color.r || color2.g != color.g || color2.b != color.b || color2.a != color.a)
					{
						pixels[i + j * texture.width] = color2;
					}
				}
			}
			texture.SetPixels32(pixels);
		}
	}

	public class ModApplyer
	{
		private bool started;

		private bool finished;

		public string applyError;

		private ProcessManager manager;

		private List<bool> pendingEnabled;

		private List<int> pendingLoadOrder;

		private Thread applyThread;

		private bool requiresRestart;

		public bool refreshingIndexMaps;

		public int activeMergingMod;

		public int mergeFileInd;

		public int mergeFileLength;

		public int applyFileInd;

		public int applyFileLength;

		public int mapsToGenerate;

		public int mapsGeneratedSoFar;

		public string statusText;

		public Dictionary<string, List<string>> worldMapImages;

		public Dictionary<string, List<string>> worldMapImageMetas;

		public Dictionary<string, List<string>> worldMapBasePaths;

		public string nextMapKeyToGenerate;

		public bool finalizeApplyingMaps;

		public int timeSinceLastMapGenerate;

		public bool filesInBadState;

		public Dictionary<string, List<MapMerger.MergeMapData>> worldMaps;

		public ModApplyer(ProcessManager manager, List<bool> pendingEnabled, List<int> pendingLoadOrder)
		{
			this.manager = manager;
			this.pendingEnabled = pendingEnabled;
			this.pendingLoadOrder = pendingLoadOrder;
			activeMergingMod = -1;
			mergeFileInd = 0;
			mergeFileLength = 0;
			applyFileInd = 0;
			applyFileLength = 0;
			mapsToGenerate = -1;
			mapsGeneratedSoFar = 0;
			refreshingIndexMaps = false;
			statusText = "";
			applyError = null;
			worldMaps = new Dictionary<string, List<MapMerger.MergeMapData>>();
		}

		public bool IsFinished()
		{
			if (!finished)
			{
				if (started)
				{
					return !applyThread.IsAlive;
				}
				return false;
			}
			return true;
		}

		public bool WasSuccessful()
		{
			if (finished)
			{
				return applyError == null;
			}
			return false;
		}

		public bool IsStarted()
		{
			return started;
		}

		public bool RequiresRestart()
		{
			return requiresRestart;
		}

		public void Start(bool filesInBadState)
		{
			if (!finished && !started)
			{
				this.filesInBadState = filesInBadState;
				applyThread = new Thread(ApplyModsThread);
				applyThread.Start();
				started = true;
			}
		}

		public void Update()
		{
			if (mapsToGenerate > 0)
			{
				statusText = manager.rainWorld.inGameTranslator.Translate("Generating map images") + " (" + ((float)mapsGeneratedSoFar / (float)mapsToGenerate * 100f).ToString("0.00") + "%)";
			}
			else if (applyFileLength > 0)
			{
				statusText = manager.rainWorld.inGameTranslator.Translate("Applying Changes") + " (" + ((float)applyFileInd / (float)applyFileLength * 100f).ToString("0.00") + "%)";
			}
			else if (activeMergingMod >= 0 && mergeFileLength > 0)
			{
				statusText = "[" + InstalledMods[activeMergingMod].LocalizedName + "] " + manager.rainWorld.inGameTranslator.Translate("Finding Changes") + " (" + ((float)mergeFileInd / (float)mergeFileLength * 100f).ToString("0.00") + "%)";
			}
			else if (refreshingIndexMaps)
			{
				statusText = manager.rainWorld.inGameTranslator.Translate("Creating index maps");
			}
			else
			{
				statusText = " ";
			}
			if (nextMapKeyToGenerate != null)
			{
				timeSinceLastMapGenerate++;
				if (timeSinceLastMapGenerate >= 3)
				{
					try
					{
						MapMerger.MergeMap(this, nextMapKeyToGenerate);
					}
					catch (Exception ex)
					{
						Custom.LogWarning("EXCEPTION IN MAP MERGING FOR", nextMapKeyToGenerate, ":", ex.Message, "::", ex.StackTrace);
					}
					nextMapKeyToGenerate = null;
					timeSinceLastMapGenerate = 0;
				}
			}
			if (finalizeApplyingMaps)
			{
				try
				{
					FinalizeMapMerging();
				}
				catch (Exception ex2)
				{
					Custom.LogWarning("EXCEPTION IN FINALIZING MAP MERGING :", ex2.Message, "::", ex2.StackTrace);
				}
				finalizeApplyingMaps = false;
			}
		}

		private void MergeMap(string mapKey)
		{
			string text = mapKey.Substring(mapKey.IndexOf("_") + 1, 2);
			string text2 = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mergedmods" + Path.DirectorySeparatorChar + "world" + Path.DirectorySeparatorChar + text).ToLowerInvariant();
			Directory.CreateDirectory(text2);
			Texture2D[] array = new Texture2D[worldMapImages[mapKey].Count];
			Texture2D[] array2 = new Texture2D[worldMapImages[mapKey].Count];
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			List<Vector2>[] array3 = new List<Vector2>[3];
			Color color = new Color(0f, 1f, 0f);
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = new List<Vector2>();
			}
			for (int j = 0; j < worldMapBasePaths[mapKey].Count; j++)
			{
				string path = (worldMapBasePaths[mapKey][j] + Path.DirectorySeparatorChar + "world" + Path.DirectorySeparatorChar + text + "-rooms").ToLowerInvariant();
				string path2 = (worldMapBasePaths[mapKey][j] + Path.DirectorySeparatorChar + "modify" + Path.DirectorySeparatorChar + "world" + Path.DirectorySeparatorChar + text + "-rooms").ToLowerInvariant();
				if (Directory.Exists(path))
				{
					string[] files = Directory.GetFiles(path);
					for (int k = 0; k < files.Length; k++)
					{
						if (files[k].ToLowerInvariant().EndsWith(".txt") && !files[k].ToLowerInvariant().Contains("_settings"))
						{
							string key = Path.GetFileNameWithoutExtension(files[k]).ToLowerInvariant();
							dictionary[key] = j;
						}
					}
				}
				if (!Directory.Exists(path2))
				{
					continue;
				}
				string[] files2 = Directory.GetFiles(path2);
				for (int l = 0; l < files2.Length; l++)
				{
					if (files2[l].ToLowerInvariant().EndsWith(".txt") && !files2[l].ToLowerInvariant().Contains("_settings"))
					{
						string key2 = Path.GetFileNameWithoutExtension(files2[l]).ToLowerInvariant();
						dictionary[key2] = j;
					}
				}
			}
			Dictionary<string, float[]>[] array4 = new Dictionary<string, float[]>[3];
			for (int m = 1; m < worldMapImages[mapKey].Count; m++)
			{
				byte[] data = File.ReadAllBytes(worldMapImages[mapKey][m]);
				Texture2D texture2D = new Texture2D(1, 1);
				texture2D.LoadImage(data);
				array[m] = texture2D;
				string[] array5 = File.ReadAllLines(worldMapImageMetas[mapKey][m - 1]);
				string[] array6 = File.ReadAllLines(worldMapImageMetas[mapKey][m]);
				Dictionary<string, Rect>[] array7 = new Dictionary<string, Rect>[3];
				Dictionary<string, Rect>[] array8 = new Dictionary<string, Rect>[3];
				for (int n = 0; n < array7.Length; n++)
				{
					array7[n] = new Dictionary<string, Rect>();
					array8[n] = new Dictionary<string, Rect>();
					if (array4[n] == null)
					{
						array4[n] = new Dictionary<string, float[]>();
					}
				}
				if (m >= 2)
				{
					for (int num = 0; num < array5.Length; num++)
					{
						if (!array5[num].Contains(": "))
						{
							continue;
						}
						string[] array9 = Regex.Split(array5[num], ": ");
						string key3 = array9[0];
						string[] array10 = Regex.Split(array9[1], ",");
						if (array10.Length >= 4)
						{
							Rect value = new Rect(float.Parse(array10[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array10[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array10[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array10[3], NumberStyles.Any, CultureInfo.InvariantCulture));
							if (value.y < (float)(array[m - 1].height / 3))
							{
								array7[0].Add(key3, value);
								array4[0][key3] = new float[5]
								{
									value.x,
									value.y,
									value.width,
									value.height,
									m - 1
								};
							}
							else if (value.y < (float)(array[m - 1].height / 3 * 2))
							{
								value.y -= array[m - 1].height / 3;
								array7[1].Add(key3, value);
								array4[1][key3] = new float[5]
								{
									value.x,
									value.y,
									value.width,
									value.height,
									m - 1
								};
							}
							else
							{
								value.y -= array[m - 1].height / 3 * 2;
								array7[2].Add(key3, value);
								array4[2][key3] = new float[5]
								{
									value.x,
									value.y,
									value.width,
									value.height,
									m - 1
								};
							}
						}
					}
				}
				for (int num2 = 0; num2 < array6.Length; num2++)
				{
					if (!array6[num2].Contains(": "))
					{
						continue;
					}
					string[] array11 = Regex.Split(array6[num2], ": ");
					string key4 = array11[0];
					string[] array12 = Regex.Split(array11[1], ",");
					if (array12.Length >= 4)
					{
						Rect value2 = new Rect(float.Parse(array12[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array12[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array12[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array12[3], NumberStyles.Any, CultureInfo.InvariantCulture));
						if (value2.y < (float)(array[m].height / 3))
						{
							array8[0].Add(key4, value2);
							array4[0][key4] = new float[5] { value2.x, value2.y, value2.width, value2.height, m };
						}
						else if (value2.y < (float)(array[m].height / 3 * 2))
						{
							value2.y -= array[m].height / 3;
							array8[1].Add(key4, value2);
							array4[1][key4] = new float[5] { value2.x, value2.y, value2.width, value2.height, m };
						}
						else
						{
							value2.y -= array[m].height / 3 * 2;
							array8[2].Add(key4, value2);
							array4[2][key4] = new float[5] { value2.x, value2.y, value2.width, value2.height, m };
						}
					}
				}
				for (int num3 = 0; num3 < array8.Length; num3++)
				{
					Rect? rect = null;
					Rect? rect2 = null;
					bool flag = false;
					bool flag2 = false;
					foreach (KeyValuePair<string, Rect> item in array8[num3])
					{
						if (m >= 2 && array7[num3].ContainsKey(item.Key))
						{
							rect2 = item.Value;
							rect = array7[num3][item.Key];
						}
						if (m == 1 && dictionary.ContainsKey(item.Key.ToLowerInvariant()) && dictionary[item.Key.ToLowerInvariant()] > m)
						{
							float num4 = item.Value.y + (float)(array[m].height / 3 * num3);
							for (int num5 = (int)num4; (float)num5 < num4 + item.Value.height; num5++)
							{
								for (int num6 = (int)item.Value.x; (float)num6 < item.Value.x + item.Value.width; num6++)
								{
									array[m].SetPixel(num6, num5, color);
								}
							}
							flag = true;
						}
						else
						{
							if (m < 2 || !dictionary.ContainsKey(item.Key.ToLowerInvariant()) || dictionary[item.Key.ToLowerInvariant()] != m)
							{
								continue;
							}
							if (array2[m] == null)
							{
								array2[m] = new Texture2D(array[m].width, array[m].height);
								Color[] pixels = Enumerable.Repeat(color, array[m].width * array[m].height).ToArray();
								array2[m].SetPixels(pixels);
							}
							float num7 = item.Value.y + (float)(array[m].height / 3 * num3);
							for (int num8 = (int)num7; (float)num8 < num7 + item.Value.height; num8++)
							{
								for (int num9 = (int)item.Value.x; (float)num9 < item.Value.x + item.Value.width; num9++)
								{
									array2[m].SetPixel(num9, num8, array[m].GetPixel(num9, num8));
								}
							}
							flag2 = true;
						}
					}
					if (flag)
					{
						array[m].Apply();
					}
					if (flag2)
					{
						array2[m].Apply();
					}
					if (m >= 2)
					{
						if (rect2.HasValue)
						{
							array3[num3].Add(new Vector2(rect2.Value.x - rect.Value.x, rect2.Value.y - rect.Value.y));
						}
						else
						{
							array3[num3].Add(Vector2.zero);
						}
					}
				}
			}
			float num10 = 0f;
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			for (int num14 = 0; num14 < array3.Length; num14++)
			{
				float num15 = 0f;
				float num16 = 0f;
				float? num17 = null;
				float? num18 = null;
				float? num19 = null;
				float? num20 = null;
				for (int num21 = 1; num21 < worldMapImages[mapKey].Count; num21++)
				{
					if (num21 >= 2)
					{
						num15 -= array3[num14][num21 - 2].x;
						num16 -= array3[num14][num21 - 2].y;
					}
					if (!num17.HasValue || num15 < num17)
					{
						num17 = num15;
					}
					if (!num18.HasValue || num16 < num18)
					{
						num18 = num16;
					}
					if (!num19.HasValue || (float)array[num21].width + num15 > num19)
					{
						num19 = (float)array[num21].width + num15;
					}
					if (!num20.HasValue || (float)(array[num21].height / 3) + num16 > num20)
					{
						num20 = (float)(array[num21].height / 3) + num16;
					}
				}
				if (num19.HasValue && num17.HasValue && num19 - num17 > num10)
				{
					num10 = num19.Value - num17.Value;
				}
				if (num20.HasValue && num18.HasValue && num20 - num18 > num11)
				{
					num11 = num20.Value - num18.Value;
				}
				if (num17.HasValue && num17 < num12)
				{
					num12 = num17.Value;
				}
				if (num18.HasValue && num18 < num13)
				{
					num13 = num18.Value;
				}
			}
			if (num10 == 0f)
			{
				num10 = array[0].width;
			}
			if (num11 == 0f)
			{
				num11 = (float)array[0].height / 3f;
			}
			Texture2D texture2D2 = new Texture2D((int)num10, (int)num11 * 3);
			Color[] pixels2 = Enumerable.Repeat(color, texture2D2.width * texture2D2.height).ToArray();
			texture2D2.SetPixels(pixels2);
			string path3 = (text2 + Path.DirectorySeparatorChar + mapKey.ToLowerInvariant().Replace("map_", "map_image_") + ".txt").ToLowerInvariant();
			List<string> list = new List<string>();
			for (int num22 = 0; num22 < array3.Length; num22++)
			{
				float num23 = 0f - num12;
				float num24 = 0f - num13;
				foreach (KeyValuePair<string, float[]> item2 in array4[num22])
				{
					if (item2.Value[4] == 0f)
					{
						list.Add(item2.Key + ": " + (int)(item2.Value[0] + (float)(int)num23) + "," + (item2.Value[1] + (float)(int)(num11 * (float)num22 + num24)) + "," + item2.Value[2] + "," + item2.Value[3]);
					}
				}
				for (int num25 = 1; num25 < worldMapImages[mapKey].Count; num25++)
				{
					Texture2D texture2D3 = ((num25 == 1) ? array[num25] : array2[num25]);
					float num26 = texture2D3.height / 3;
					if (num25 >= 2)
					{
						num23 -= array3[num22][num25 - 2].x;
						num24 -= array3[num22][num25 - 2].y;
					}
					for (int num27 = 0; num27 < texture2D3.width; num27++)
					{
						for (int num28 = 0; (float)num28 < num26; num28++)
						{
							Color pixel = texture2D3.GetPixel(num27, (int)(num26 * (float)num22) + num28);
							if (num25 == 1 || pixel != color)
							{
								texture2D2.SetPixel((int)num23 + num27, (int)(num11 * (float)num22 + num24) + num28, pixel);
							}
						}
					}
					foreach (KeyValuePair<string, float[]> item3 in array4[num22])
					{
						if (item3.Value[4] == (float)num25)
						{
							list.Add(item3.Key + ": " + (int)(item3.Value[0] + (float)(int)num23) + "," + (item3.Value[1] + (float)(int)(num11 * (float)num22 + num24)) + "," + item3.Value[2] + "," + item3.Value[3]);
						}
					}
				}
			}
			texture2D2.Apply();
			PNGSaver.SaveTextureToFile(texture2D2, (text2 + Path.DirectorySeparatorChar + mapKey + ".png").ToLowerInvariant());
			File.WriteAllLines(path3, list);
			UnityEngine.Object.Destroy(texture2D2);
		}

		private void FinalizeMapMerging()
		{
		}

		private void ApplyModsThread()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
			try
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				List<string> list3 = new List<string>();
				manager.enableModsOnProcessSwitch = new List<Mod>();
				manager.disableModsOnProcessSwitch = new List<Mod>();
				manager.rainWorld.options.enabledMods = new List<string>();
				manager.rainWorld.options.modLoadOrder = new Dictionary<string, int>();
				manager.rainWorld.options.modChecksums = new Dictionary<string, string>();
				for (int i = 0; i < InstalledMods.Count; i++)
				{
					if (InstalledMods[i].DLCMissing)
					{
						pendingEnabled[i] = false;
					}
					if (pendingEnabled[i] && !manager.rainWorld.options.enabledMods.Contains(InstalledMods[i].id))
					{
						manager.rainWorld.options.enabledMods.Add(InstalledMods[i].id);
					}
					manager.rainWorld.options.modLoadOrder[InstalledMods[i].id] = pendingLoadOrder[i];
					manager.rainWorld.options.modChecksums[InstalledMods[i].id] = InstalledMods[i].checksum;
					if (pendingEnabled[i] && InstalledMods[i].id == MoreSlugcats.MMF.MOD_ID)
					{
						flag3 = true;
					}
					string[] array = new string[6]
					{
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "v1.9.15b" + Path.DirectorySeparatorChar + "modify" + Path.DirectorySeparatorChar + "world").ToLowerInvariant(),
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "v1.9.15b" + Path.DirectorySeparatorChar + "world").ToLowerInvariant(),
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "newest" + Path.DirectorySeparatorChar + "modify" + Path.DirectorySeparatorChar + "world").ToLowerInvariant(),
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "newest" + Path.DirectorySeparatorChar + "world").ToLowerInvariant(),
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "modify" + Path.DirectorySeparatorChar + "world").ToLowerInvariant(),
						(InstalledMods[i].path + Path.DirectorySeparatorChar + "world").ToLowerInvariant()
					};
					bool flag5 = false;
					for (int j = 0; j < array.Length; j++)
					{
						if (Directory.Exists(array[j]))
						{
							flag5 = true;
							break;
						}
					}
					if (pendingEnabled[i] != InstalledMods[i].enabled || pendingLoadOrder[i] != InstalledMods[i].loadOrder || (pendingEnabled[i] && InstalledMods[i].checksumChanged) || (pendingEnabled[i] && filesInBadState))
					{
						if (((pendingEnabled[i] && !InstalledMods[i].enabled) || filesInBadState) && InstalledMods[i].id == MoreSlugcats.MMF.MOD_ID)
						{
							flag4 = true;
						}
						if ((pendingEnabled[i] != InstalledMods[i].enabled || filesInBadState) && !InstalledMods[i].checksumChanged)
						{
							if (!pendingEnabled[i])
							{
								manager.disableModsOnProcessSwitch.Add(InstalledMods[i]);
							}
							else
							{
								manager.enableModsOnProcessSwitch.Add(InstalledMods[i]);
							}
						}
						if (ModFolderHasDLLContent(InstalledMods[i].path))
						{
							if (pendingEnabled[i] && !InstalledMods[i].enabled)
							{
								list2.Add(Path.GetFileName(InstalledMods[i].path));
							}
							else if (!pendingEnabled[i] && InstalledMods[i].enabled)
							{
								list3.Add(Path.GetFileName(InstalledMods[i].path));
							}
						}
						InstalledMods[i].enabled = pendingEnabled[i];
						if (flag5)
						{
							flag2 = true;
						}
						flag = true;
					}
					if (filesInBadState && pendingEnabled[i] && flag5)
					{
						flag2 = true;
					}
					if (!InstalledMods[i].enabled)
					{
						continue;
					}
					for (int k = 0; k < array.Length; k++)
					{
						if (!Directory.Exists(array[k]))
						{
							continue;
						}
						string[] directories = Directory.GetDirectories(array[k]);
						for (int l = 0; l < directories.Length; l++)
						{
							string text = Path.GetFileName(directories[l]).ToLowerInvariant();
							if (text.Length == 2 && !list.Contains(text))
							{
								list.Add(text);
							}
						}
					}
				}
				RefreshModsLists(manager.rainWorld);
				string path = Path.Combine(Custom.RootFolderDirectory(), "mergedmods");
				if (manager.rainWorld.options.enabledMods.Count > 0 && !Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				else if (manager.rainWorld.options.enabledMods.Count == 0 && Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
				if (flag)
				{
					if (manager.rainWorld.options.enabledMods.Count > 0)
					{
						GenerateMergedMods(this, pendingEnabled, list.Count > 0);
					}
					activeMergingMod = -1;
					manager.rainWorld.progression.ReloadRegionsList();
					manager.rainWorld.progression.ReloadLocksList();
					refreshingIndexMaps = true;
					if (list.Count > 0)
					{
						string text2 = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods" + Path.DirectorySeparatorChar + "world" + Path.DirectorySeparatorChar + "indexmaps";
						Directory.CreateDirectory(text2);
						BackwardsCompability.IndexMapWorld(RainWorld.worldVersion, manager.rainWorld.progression, text2);
					}
					refreshingIndexMaps = false;
				}
				if (list2.Count > 0 || list3.Count > 0)
				{
					string text3 = "";
					List<string> list4 = new List<string>();
					for (int m = 0; m < ActiveMods.Count; m++)
					{
						if (ModFolderHasDLLContent(ActiveMods[m].path))
						{
							if (ActiveMods[m].workshopMod)
							{
								list4.Add("[WORKSHOP]" + ActiveMods[m].path);
							}
							else
							{
								list4.Add(Path.GetFileName(ActiveMods[m].path));
							}
						}
					}
					for (int n = 0; n < list4.Count; n++)
					{
						text3 = text3 + list4[n] + ((n < list4.Count - 1) ? "\r\n" : "");
					}
					File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "enabledMods.txt"), text3);
					File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "enabledModsVersion.txt"), "v1.9.15b");
					requiresRestart = true;
				}
				if (flag4 || (flag2 && flag3))
				{
					string text4 = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods" + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "IndexMaps").ToLowerInvariant();
					Directory.CreateDirectory(text4);
					File.WriteAllText(text4 + Path.DirectorySeparatorChar + "recomputetokencache.txt", "");
				}
			}
			catch (Exception ex)
			{
				applyError = ex.Message + Environment.NewLine + ex.StackTrace;
			}
			finished = true;
		}
	}

	public class ModMerger
	{
		public class PendingApply
		{
			private class ReplaceOperation
			{
				public enum ReplaceType
				{
					SUBSTITUTE,
					BEFORE,
					AFTER
				}

				public string findMatch;

				public string findMatchRegex;

				public string findMatchStart;

				public string findMatchEnd;

				public int findLineNumber = -1;

				public bool findLine;

				public string replaceWith = "";

				public ReplaceType replaceType;

				public int occurrence;

				public int occurrencesSeen;
			}

			public bool isVanilla;

			public bool isModification;

			public string filePath;

			public List<string> mergeLines;

			public Mod modApplyFrom;

			private List<string> addAtEnd;

			private Dictionary<int, List<string>> addAtLine;

			private List<ReplaceOperation> replacements;

			public PendingApply(Mod modApplyFrom, string filePath, bool isVanilla, bool isModification)
			{
				this.modApplyFrom = modApplyFrom;
				this.filePath = filePath;
				this.isVanilla = isVanilla;
				this.isModification = isModification;
				if (isModification)
				{
					CollectModifications();
				}
			}

			public void CollectModifications()
			{
				addAtEnd = new List<string>();
				addAtLine = new Dictionary<int, List<string>>();
				replacements = new List<ReplaceOperation>();
				string text = null;
				string text2 = null;
				string text3 = null;
				string text4 = null;
				int result = -1;
				bool findLine = false;
				string[] array = File.ReadAllLines(filePath);
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (flag)
					{
						string item;
						if (array[i].Contains("[ENDMERGE]"))
						{
							item = array[i].Replace("[ENDMERGE]", "");
							flag = false;
						}
						else
						{
							item = array[i];
						}
						mergeLines.Add(item);
					}
					else if (array[i].StartsWith("[MERGE]"))
					{
						flag = true;
						if (mergeLines == null)
						{
							mergeLines = new List<string>();
						}
						string text5 = array[i].Substring("[MERGE]".Length).Replace("\\n", Environment.NewLine).Trim();
						if (text5.Length > 0)
						{
							mergeLines.Add(text5);
						}
					}
					else if (array[i].StartsWith("[ADD]"))
					{
						addAtEnd.Add(array[i].Substring("[ADD]".Length).Replace("\\n", Environment.NewLine));
					}
					else if (array[i].StartsWith("[ADD_") && array[i].Contains("]"))
					{
						if (int.TryParse(array[i].Substring("[ADD_".Length, array[i].IndexOf("]") - "[ADD_".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
						{
							string item2 = array[i].Substring(array[i].IndexOf("]") + 1).Replace("\\n", Environment.NewLine);
							if (!addAtLine.ContainsKey(result2))
							{
								addAtLine[result2] = new List<string>();
							}
							addAtLine[result2].Add(item2);
						}
					}
					else if (array[i].StartsWith("[FIND]"))
					{
						text = array[i].Substring("[FIND]".Length).Replace("\\n", Environment.NewLine);
						text2 = null;
						text3 = null;
						text4 = null;
						result = -1;
						findLine = false;
					}
					else if (array[i].StartsWith("[FINDLINE]"))
					{
						text = array[i].Substring("[FINDLINE]".Length).Replace("\\n", Environment.NewLine);
						text2 = null;
						text3 = null;
						text4 = null;
						result = -1;
						findLine = true;
					}
					else if (array[i].StartsWith("[FINDREGEX]"))
					{
						text4 = array[i].Substring("[FINDREGEX]".Length).Replace("\\n", Environment.NewLine);
						text = null;
						text2 = null;
						text3 = null;
						result = -1;
						findLine = false;
					}
					else if (array[i].StartsWith("[FINDLINEREGEX]"))
					{
						text4 = array[i].Substring("[FINDLINEREGEX]".Length).Replace("\\n", Environment.NewLine);
						text = null;
						text2 = null;
						text3 = null;
						result = -1;
						findLine = true;
					}
					else if (array[i].StartsWith("[FINDLINESTART]"))
					{
						text2 = array[i].Substring("[FINDLINESTART]".Length).Replace("\\n", Environment.NewLine);
						text = null;
						text3 = null;
						text4 = null;
						result = -1;
						findLine = true;
					}
					else if (array[i].StartsWith("[FINDLINEEND]"))
					{
						text3 = array[i].Substring("[FINDLINEEND]".Length).Replace("\\n", Environment.NewLine);
						text2 = null;
						text = null;
						text4 = null;
						result = -1;
						findLine = true;
					}
					else if (array[i].StartsWith("[TARGETLINE]"))
					{
						if (int.TryParse(array[i].Substring("[TARGETLINE]".Length).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
						{
							if (result > 0)
							{
								text3 = null;
								text2 = null;
								text = null;
								text4 = null;
								findLine = true;
							}
							else
							{
								result = -1;
							}
						}
						else
						{
							result = -1;
						}
					}
					else if ((array[i].StartsWith("[REPLACE]") || array[i].StartsWith("[ADDBEFORE]") || array[i].StartsWith("[ADDAFTER]")) && (text != null || text4 != null || text2 != null || text3 != null || result > 0))
					{
						ReplaceOperation replaceOperation = new ReplaceOperation();
						replaceOperation.findMatch = text;
						replaceOperation.findMatchRegex = text4;
						replaceOperation.findMatchStart = text2;
						replaceOperation.findMatchEnd = text3;
						replaceOperation.findLineNumber = result;
						replaceOperation.findLine = findLine;
						if (array[i].StartsWith("[REPLACE]"))
						{
							replaceOperation.replaceWith = array[i].Substring("[REPLACE]".Length).Replace("\\n", Environment.NewLine);
							replaceOperation.replaceType = ReplaceOperation.ReplaceType.SUBSTITUTE;
						}
						else if (array[i].StartsWith("[ADDBEFORE]"))
						{
							replaceOperation.replaceWith = array[i].Substring("[ADDBEFORE]".Length).Replace("\\n", Environment.NewLine);
							replaceOperation.replaceType = ReplaceOperation.ReplaceType.BEFORE;
						}
						else if (array[i].StartsWith("[ADDAFTER]"))
						{
							replaceOperation.replaceWith = array[i].Substring("[ADDAFTER]".Length).Replace("\\n", Environment.NewLine);
							replaceOperation.replaceType = ReplaceOperation.ReplaceType.AFTER;
						}
						replacements.Add(replaceOperation);
					}
					else
					{
						if ((!array[i].StartsWith("[REPLACE_") && !array[i].StartsWith("[ADDBEFORE_") && !array[i].StartsWith("[ADDAFTER_")) || !array[i].Contains("]") || (text == null && text4 == null && text2 == null && text3 == null && result <= 0))
						{
							continue;
						}
						int result3 = 0;
						bool flag2 = false;
						if (array[i].StartsWith("[REPLACE_"))
						{
							flag2 = int.TryParse(array[i].Substring("[REPLACE_".Length, array[i].IndexOf("]") - "[REPLACE_".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out result3);
						}
						else if (array[i].StartsWith("[ADDBEFORE_"))
						{
							flag2 = int.TryParse(array[i].Substring("[ADDBEFORE_".Length, array[i].IndexOf("]") - "[ADDBEFORE_".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out result3);
						}
						else if (array[i].StartsWith("[ADDAFTER_"))
						{
							flag2 = int.TryParse(array[i].Substring("[ADDAFTER_".Length, array[i].IndexOf("]") - "[ADDAFTER_".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out result3);
						}
						if (flag2)
						{
							ReplaceOperation replaceOperation2 = new ReplaceOperation();
							replaceOperation2.findMatch = text;
							replaceOperation2.findMatchRegex = text4;
							replaceOperation2.findMatchStart = text2;
							replaceOperation2.findMatchEnd = text3;
							replaceOperation2.findLineNumber = result;
							replaceOperation2.findLine = findLine;
							replaceOperation2.replaceWith = array[i].Substring(array[i].IndexOf("]") + 1).Replace("\\n", Environment.NewLine);
							replaceOperation2.occurrence = result3;
							if (array[i].StartsWith("[REPLACE_"))
							{
								replaceOperation2.replaceType = ReplaceOperation.ReplaceType.SUBSTITUTE;
							}
							else if (array[i].StartsWith("[ADDBEFORE_"))
							{
								replaceOperation2.replaceType = ReplaceOperation.ReplaceType.BEFORE;
							}
							else if (array[i].StartsWith("[ADDAFTER_"))
							{
								replaceOperation2.replaceType = ReplaceOperation.ReplaceType.AFTER;
							}
							replacements.Add(replaceOperation2);
						}
					}
				}
			}

			public void ApplyMerges(Mod modApplyFrom, ModMerger merger, string pathToModify)
			{
				string fileName = Path.GetFileName(pathToModify.ToLowerInvariant());
				if (fileName.EndsWith("default alignments.txt") || fileName.EndsWith("sounds.txt") || fileName.EndsWith("locks.txt") || fileName.EndsWith("properties.txt") || (fileName.Contains("properties-") && fileName.EndsWith(".txt")))
				{
					MergeUniqueItemsUnordered(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else if (fileName.EndsWith("mpmusic.txt") || fileName.EndsWith("regions.txt") || fileName.EndsWith("egates.txt"))
				{
					merger.MergeUniqueLinesUnordered(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else if (fileName.Contains("world_") && fileName.EndsWith(".txt"))
				{
					merger.MergeWorldFiles(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else if (fileName.EndsWith("_settings.txt") || fileName.Contains("_settingstemplate_") || fileName.Contains("_settings-"))
				{
					merger.MergeRoomSettings(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else if (fileName.Contains("map_") && !fileName.Contains("map_image_") && fileName.EndsWith(".txt"))
				{
					MapMerger.MergeMapFiles(merger, modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else if (fileName.Contains("strings.txt"))
				{
					MergeShortStrings(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
				else
				{
					WriteMergedFile(modApplyFrom, pathToModify, mergeLines.ToArray());
				}
			}

			private string ApplySubstringReplacement(string sourceLine, string toReplace, string replaceWith, ReplaceOperation.ReplaceType replaceType)
			{
				return replaceType switch
				{
					ReplaceOperation.ReplaceType.SUBSTITUTE => sourceLine.Replace(toReplace, replaceWith), 
					ReplaceOperation.ReplaceType.BEFORE => sourceLine.Replace(toReplace, replaceWith + toReplace), 
					ReplaceOperation.ReplaceType.AFTER => sourceLine.Replace(toReplace, toReplace + replaceWith), 
					_ => sourceLine, 
				};
			}

			private string ApplySubstringRegexReplacement(string sourceLine, string toReplace, string replaceWith, ReplaceOperation.ReplaceType replaceType)
			{
				return replaceType switch
				{
					ReplaceOperation.ReplaceType.SUBSTITUTE => Regex.Replace(sourceLine, toReplace, replaceWith), 
					ReplaceOperation.ReplaceType.BEFORE => Regex.Replace(sourceLine, toReplace, replaceWith + "$+"), 
					ReplaceOperation.ReplaceType.AFTER => Regex.Replace(sourceLine, toReplace, "$+" + replaceWith), 
					_ => sourceLine, 
				};
			}

			private string ApplyLineReplacement(string sourceLine, string replaceWith, ReplaceOperation.ReplaceType replaceType)
			{
				return replaceType switch
				{
					ReplaceOperation.ReplaceType.SUBSTITUTE => replaceWith, 
					ReplaceOperation.ReplaceType.BEFORE => replaceWith + sourceLine, 
					ReplaceOperation.ReplaceType.AFTER => sourceLine + replaceWith, 
					_ => sourceLine, 
				};
			}

			public void ApplyModifications(string pathToModify)
			{
				if (!isModification)
				{
					return;
				}
				List<string> list = new List<string>();
				if (pathToModify.Contains("text_") && !pathToModify.Contains("strings.txt"))
				{
					InGameTranslator.EncryptDecryptFile(pathToModify, encryptMode: false);
				}
				string[] array = File.ReadAllLines(pathToModify);
				for (int i = 0; i < array.Length; i++)
				{
					if (addAtLine.ContainsKey(i))
					{
						list.AddRange(addAtLine[i]);
					}
					string text = array[i];
					for (int j = 0; j < replacements.Count; j++)
					{
						if (replacements[j].occurrence == 0)
						{
							if (replacements[j].findMatch != null)
							{
								if (!replacements[j].findLine)
								{
									text = ApplySubstringReplacement(text, replacements[j].findMatch, replacements[j].replaceWith, replacements[j].replaceType);
								}
								else if (text.Contains(replacements[j].findMatch))
								{
									text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
								}
							}
							else if (replacements[j].findMatchRegex != null)
							{
								if (!replacements[j].findLine)
								{
									text = ApplySubstringRegexReplacement(text, replacements[j].findMatchRegex, replacements[j].replaceWith, replacements[j].replaceType);
								}
								else if (Regex.Matches(text, replacements[j].findMatchRegex).Count > 0)
								{
									text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
								}
							}
							else if (replacements[j].findMatchStart != null)
							{
								if (text.StartsWith(replacements[j].findMatchStart))
								{
									text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
								}
							}
							else if (replacements[j].findMatchEnd != null)
							{
								if (text.EndsWith(replacements[j].findMatchEnd))
								{
									text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
								}
							}
							else if (replacements[j].findLineNumber == i + 1)
							{
								text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
							}
						}
						else
						{
							if (replacements[j].occurrencesSeen < 0)
							{
								continue;
							}
							if (!replacements[j].findLine)
							{
								MatchCollection matchCollection = null;
								if (replacements[j].findMatch != null)
								{
									matchCollection = Regex.Matches(array[i], "\\b(" + replacements[j].findMatch + ")\\b");
								}
								else if (replacements[j].findMatchRegex != null)
								{
									matchCollection = Regex.Matches(array[i], replacements[j].findMatchRegex);
								}
								if (matchCollection == null)
								{
									continue;
								}
								if (replacements[j].occurrencesSeen + matchCollection.Count < replacements[j].occurrence)
								{
									replacements[j].occurrencesSeen += matchCollection.Count;
									continue;
								}
								int num = replacements[j].occurrence - replacements[j].occurrencesSeen;
								MatchCollection matchCollection2 = null;
								if (replacements[j].findMatch != null)
								{
									matchCollection2 = Regex.Matches(text, "\\b(" + replacements[j].findMatch + ")\\b");
								}
								else if (replacements[j].findMatchRegex != null)
								{
									matchCollection2 = Regex.Matches(text, replacements[j].findMatchRegex);
								}
								if (num > matchCollection2.Count)
								{
									continue;
								}
								if (replacements[j].findMatch != null)
								{
									if (replacements[j].replaceType == ReplaceOperation.ReplaceType.SUBSTITUTE)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + replacements[j].replaceWith + text.Substring(matchCollection2[num - 1].Index + replacements[j].findMatch.Length);
									}
									else if (replacements[j].replaceType == ReplaceOperation.ReplaceType.BEFORE)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + replacements[j].replaceWith + replacements[j].findMatch + text.Substring(matchCollection2[num - 1].Index + replacements[j].findMatch.Length);
									}
									else if (replacements[j].replaceType == ReplaceOperation.ReplaceType.AFTER)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + replacements[j].findMatch + replacements[j].replaceWith + text.Substring(matchCollection2[num - 1].Index + replacements[j].findMatch.Length);
									}
								}
								else if (replacements[j].findMatchRegex != null)
								{
									Regex regex = new Regex(replacements[j].findMatchRegex);
									if (replacements[j].replaceType == ReplaceOperation.ReplaceType.SUBSTITUTE)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + regex.Replace(text.Substring(matchCollection2[num - 1].Index), replacements[j].replaceWith, 1);
									}
									else if (replacements[j].replaceType == ReplaceOperation.ReplaceType.BEFORE)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + regex.Replace(text.Substring(matchCollection2[num - 1].Index), replacements[j].replaceWith + matchCollection2[num - 1].Value, 1);
									}
									else if (replacements[j].replaceType == ReplaceOperation.ReplaceType.AFTER)
									{
										text = text.Substring(0, matchCollection2[num - 1].Index) + regex.Replace(text.Substring(matchCollection2[num - 1].Index), matchCollection2[num - 1].Value + replacements[j].replaceWith, 1);
									}
								}
								replacements[j].occurrencesSeen = -1;
							}
							else if (replacements[j].findMatch != null)
							{
								if (text.Contains(replacements[j].findMatch))
								{
									replacements[j].occurrencesSeen++;
									if (replacements[j].occurrencesSeen >= replacements[j].occurrence)
									{
										text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
										replacements[j].occurrencesSeen = -1;
									}
								}
							}
							else if (replacements[j].findMatchRegex != null)
							{
								if (Regex.Matches(text, replacements[j].findMatchRegex).Count > 0)
								{
									replacements[j].occurrencesSeen++;
									if (replacements[j].occurrencesSeen >= replacements[j].occurrence)
									{
										text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
										replacements[j].occurrencesSeen = -1;
									}
								}
							}
							else if (replacements[j].findMatchStart != null)
							{
								if (text.StartsWith(replacements[j].findMatchStart))
								{
									replacements[j].occurrencesSeen++;
									if (replacements[j].occurrencesSeen >= replacements[j].occurrence)
									{
										text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
										replacements[j].occurrencesSeen = -1;
									}
								}
							}
							else if (replacements[j].findMatchEnd != null && text.EndsWith(replacements[j].findMatchEnd))
							{
								replacements[j].occurrencesSeen++;
								if (replacements[j].occurrencesSeen >= replacements[j].occurrence)
								{
									text = ApplyLineReplacement(text, replacements[j].replaceWith, replacements[j].replaceType);
									replacements[j].occurrencesSeen = -1;
								}
							}
						}
					}
					if (text.Trim() != "")
					{
						list.Add(text);
					}
				}
				list.AddRange(addAtEnd);
				if (pathToModify.Contains("text_") && !pathToModify.Contains("strings.txt"))
				{
					InGameTranslator.EncryptDecryptFile(pathToModify, encryptMode: true);
				}
				WriteMergedFile(modApplyFrom, pathToModify, list.ToArray());
			}
		}

		public class WorldFile
		{
			public List<WorldRoomLink> rooms = new List<WorldRoomLink>();

			public List<WorldRoomSpawn> creatures = new List<WorldRoomSpawn>();

			public List<WorldExclusiveConditionalLink> exclusives = new List<WorldExclusiveConditionalLink>();

			public List<WorldHiddenConditionalLink> hidden = new List<WorldHiddenConditionalLink>();

			public List<WorldSubstituteConditionalLink> substitutes = new List<WorldSubstituteConditionalLink>();

			public List<string> migrationBlockages = new List<string>();

			public List<string> unknownContextLines = new List<string>();

			public WorldFile(string[] worldFileLines)
			{
				string text = "";
				for (int i = 0; i < worldFileLines.Length; i++)
				{
					string text2 = worldFileLines[i].Trim();
					if (text2 == "" || text2.StartsWith("//"))
					{
						continue;
					}
					switch (text2)
					{
					case "ROOMS":
					case "CREATURES":
					case "BAT MIGRATION BLOCKAGES":
					case "CONDITIONAL LINKS":
						text = text2;
						continue;
					}
					if (text2.StartsWith("END ") && text != "")
					{
						text = "";
						continue;
					}
					switch (text)
					{
					case "":
						unknownContextLines.Add(text2);
						break;
					case "ROOMS":
						rooms.Add(new WorldRoomLink(text2));
						break;
					case "CREATURES":
						if (text2.StartsWith("(") && text2.Contains(")"))
						{
							string text3 = text2.Substring(1, text2.IndexOf(")") - 1);
							bool flag = false;
							if (text3.StartsWith("X-"))
							{
								text3 = text3.Substring(2);
								flag = true;
							}
							if (flag)
							{
								creatures.Add(new WorldRoomSpawn(text3, text2, excludeMode: true));
								break;
							}
							string[] array2 = text3.Split(',');
							foreach (string character4 in array2)
							{
								creatures.Add(new WorldRoomSpawn(character4, text2, excludeMode: false));
							}
						}
						else
						{
							creatures.Add(new WorldRoomSpawn("", text2, excludeMode: false));
						}
						break;
					case "BAT MIGRATION BLOCKAGES":
						migrationBlockages.Add(text2);
						break;
					case "CONDITIONAL LINKS":
					{
						if (!text2.Contains(":"))
						{
							break;
						}
						string[] array = text2.Split(':')[0].Trim().Split(',');
						if (text2.Contains("EXCLUSIVEROOM"))
						{
							string[] array2 = array;
							foreach (string character in array2)
							{
								exclusives.Add(new WorldExclusiveConditionalLink(character, text2));
							}
						}
						else if (text2.Contains("HIDEROOM"))
						{
							string[] array2 = array;
							foreach (string character2 in array2)
							{
								hidden.Add(new WorldHiddenConditionalLink(character2, text2));
							}
						}
						else
						{
							string[] array2 = array;
							foreach (string character3 in array2)
							{
								substitutes.Add(new WorldSubstituteConditionalLink(character3, text2));
							}
						}
						break;
					}
					}
				}
			}

			public string[] ToFileLines()
			{
				List<string> list = new List<string>();
				list.Add("CONDITIONAL LINKS");
				foreach (WorldExclusiveConditionalLink exclusife in exclusives)
				{
					list.Add(exclusife.ToString());
				}
				foreach (WorldHiddenConditionalLink item in hidden)
				{
					list.Add(item.ToString());
				}
				foreach (WorldSubstituteConditionalLink substitute in substitutes)
				{
					list.Add(substitute.ToString());
				}
				list.Add("END CONDITIONAL LINKS");
				list.Add("");
				list.Add("ROOMS");
				foreach (WorldRoomLink room in rooms)
				{
					list.Add(room.ToString());
				}
				list.Add("END ROOMS");
				list.Add("");
				list.Add("CREATURES");
				foreach (WorldRoomSpawn creature in creatures)
				{
					list.Add(creature.ToString());
				}
				list.Add("END CREATURES");
				list.Add("");
				list.Add("BAT MIGRATION BLOCKAGES");
				foreach (string migrationBlockage in migrationBlockages)
				{
					list.Add(migrationBlockage);
				}
				list.Add("END BAT MIGRATION BLOCKAGES");
				if (unknownContextLines.Count > 0)
				{
					list.Add("");
					foreach (string unknownContextLine in unknownContextLines)
					{
						list.Add(unknownContextLine);
					}
				}
				return list.ToArray();
			}
		}

		public class WorldExclusiveConditionalLink
		{
			public string roomName = "";

			public string character;

			public WorldExclusiveConditionalLink(string character, string line)
			{
				this.character = character;
				string[] array = line.Split(':');
				if (array.Length >= 3)
				{
					roomName = array[2].Trim();
				}
			}

			public override string ToString()
			{
				return character + " : EXCLUSIVEROOM : " + roomName;
			}

			public string Key()
			{
				return character + "-" + roomName;
			}
		}

		public class WorldHiddenConditionalLink
		{
			public string roomName = "";

			public string character;

			public WorldHiddenConditionalLink(string character, string line)
			{
				this.character = character;
				string[] array = line.Split(':');
				if (array.Length >= 3)
				{
					roomName = array[2].Trim();
				}
			}

			public override string ToString()
			{
				return character + " : HIDEROOM : " + roomName;
			}

			public string Key()
			{
				return character + "-" + roomName;
			}
		}

		public class WorldSubstituteConditionalLink
		{
			public string roomName = "";

			public string targetRoom = "";

			public string substituteRoom = "";

			public string character;

			public WorldSubstituteConditionalLink(string character, string line)
			{
				this.character = character;
				string[] array = line.Split(':');
				if (array.Length >= 4)
				{
					roomName = array[1].Trim();
					targetRoom = array[2].Trim();
					substituteRoom = array[3].Trim();
				}
			}

			public override string ToString()
			{
				return character + " : " + roomName + " : " + targetRoom + " : " + substituteRoom;
			}

			public string Key()
			{
				return character + "-" + roomName + "-" + targetRoom;
			}
		}

		public class WorldRoomLink
		{
			public string roomName;

			public string specialType;

			public List<string> connections = new List<string>();

			public WorldRoomLink(string line)
			{
				string[] array = line.Split(':');
				roomName = array[0].Trim();
				if (array.Length > 2)
				{
					specialType = array[2].Trim();
				}
				else
				{
					specialType = "";
				}
				string[] array2 = array[1].Trim().Split(',');
				foreach (string text in array2)
				{
					connections.Add(text.Trim());
				}
			}

			public override string ToString()
			{
				string text = roomName + " : " + string.Join(", ", connections.ToArray());
				if (specialType != "")
				{
					text = text + " : " + specialType;
				}
				return text;
			}
		}

		public class WorldRoomSpawn
		{
			public string character;

			public string roomName;

			private List<WorldDen> dens;

			public int lineageDen;

			public List<WorldLineage> lineages;

			public bool excludeMode;

			public WorldRoomSpawn(string character, string line, bool excludeMode)
			{
				this.character = character;
				this.excludeMode = excludeMode;
				if (line.StartsWith("(") && line.Contains(")"))
				{
					line = line.Substring(line.IndexOf(")") + 1);
				}
				string[] array = Regex.Split(line, " : ");
				if (array[0].Trim() == "LINEAGE")
				{
					dens = null;
					lineages = new List<WorldLineage>();
					roomName = array[1].Trim();
					int.TryParse(array[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out lineageDen);
					string[] array2 = SplitSpawners(array[3]);
					for (int i = 0; i < array2.Length; i++)
					{
						lineages.Add(new WorldLineage(array2[i].Trim()));
					}
				}
				else
				{
					lineages = null;
					lineageDen = -1;
					dens = new List<WorldDen>();
					roomName = array[0].Trim();
					string[] array3 = SplitSpawners(array[1]);
					for (int j = 0; j < array3.Length; j++)
					{
						dens.Add(new WorldDen(array3[j].Trim()));
					}
				}
			}

			private string[] SplitSpawners(string spawnString)
			{
				if (!spawnString.Contains("{"))
				{
					return spawnString.Split(',');
				}
				string[] array = spawnString.Split(',');
				List<string> list = new List<string>();
				bool flag = false;
				string text = "";
				for (int i = 0; i < array.Length; i++)
				{
					if (!flag)
					{
						if (!array[i].Contains("{"))
						{
							list.Add(array[i]);
							continue;
						}
						if (array[i].Contains("}"))
						{
							list.Add(array[i]);
							continue;
						}
						text = array[i];
						flag = true;
					}
					else
					{
						text = text + "," + array[i];
						if (array[i].Contains("}"))
						{
							list.Add(text);
							text = "";
							flag = false;
						}
					}
				}
				return list.ToArray();
			}

			public override string ToString()
			{
				string text = "";
				if (character != "")
				{
					text = text + "(" + (excludeMode ? "X-" : "") + character + ")";
				}
				if (lineages != null)
				{
					text += "LINEAGE : ";
				}
				text = text + roomName + " : ";
				if (lineageDen >= 0)
				{
					text = text + lineageDen + " : ";
				}
				if (lineages != null)
				{
					for (int i = 0; i < lineages.Count; i++)
					{
						text += lineages[i].ToString();
						if (i < lineages.Count - 1)
						{
							text += ", ";
						}
					}
				}
				else if (dens != null)
				{
					for (int j = 0; j < dens.Count; j++)
					{
						text += dens[j].ToString();
						if (j < dens.Count - 1)
						{
							text += ", ";
						}
					}
				}
				return text;
			}
		}

		public class WorldDen
		{
			public int denNumber;

			public int spawnQuantity;

			public string creature;

			public string attributes;

			public WorldDen(string denString)
			{
				string[] array = denString.Split('-');
				int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out denNumber);
				creature = array[1];
				attributes = "";
				bool flag = false;
				for (int i = 2; i < array.Length; i++)
				{
					if (array[i].StartsWith("{"))
					{
						flag = true;
						attributes = array[i];
					}
					else if (flag)
					{
						attributes = attributes + "-" + array[i];
					}
					else
					{
						int.TryParse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture, out spawnQuantity);
					}
					if (array[i].Contains("}"))
					{
						flag = false;
					}
				}
			}

			public override string ToString()
			{
				string text = denNumber.ToString(CultureInfo.InvariantCulture) + "-" + creature;
				if (attributes != "")
				{
					text = text + "-" + attributes;
				}
				if (spawnQuantity > 1)
				{
					text = text + "-" + spawnQuantity.ToString(CultureInfo.InvariantCulture);
				}
				return text;
			}
		}

		public class WorldLineage
		{
			public string creature;

			public string attributes;

			public float chance;

			public WorldLineage(string lineageString)
			{
				string[] array = lineageString.Split('-');
				creature = array[0];
				attributes = "";
				bool flag = false;
				for (int i = 1; i < array.Length; i++)
				{
					if (array[i].StartsWith("{"))
					{
						flag = true;
						attributes = array[i];
					}
					else if (flag)
					{
						attributes = attributes + "-" + array[i];
					}
					else
					{
						chance = float.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					if (array[i].Contains("}"))
					{
						flag = false;
					}
				}
			}

			public override string ToString()
			{
				string text = creature;
				if (attributes != "")
				{
					text = text + "-" + attributes;
				}
				return text + "-" + chance.ToString(CultureInfo.InvariantCulture);
			}
		}

		public List<int> palettesUsedSoFar;

		public Dictionary<int, int> paletteRemapping;

		private Dictionary<string, List<PendingApply>> moddedFiles;

		private List<string> worldMapImagesTouchedByCurrentMod;

		public List<MapMerger.MergeMapData> modMapData;

		public ModMerger()
		{
			palettesUsedSoFar = new List<int>();
			paletteRemapping = new Dictionary<int, int>();
			moddedFiles = new Dictionary<string, List<PendingApply>>();
			modMapData = new List<MapMerger.MergeMapData>();
		}

		public void MergeWorldMaps(ModApplyer applyer)
		{
			applyer.worldMapImages = new Dictionary<string, List<string>>();
			applyer.worldMapImageMetas = new Dictionary<string, List<string>>();
			applyer.worldMapBasePaths = new Dictionary<string, List<string>>();
			PopulateWorldMapImageDictionary(applyer, Custom.RootFolderDirectory(), Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "world", populateRedundant: false, skipRedundant: false);
			List<Mod> list = ActiveMods.OrderBy((Mod o) => o.loadOrder).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				worldMapImagesTouchedByCurrentMod = new List<string>();
				List<string> list2 = new List<string>();
				if (list[i].hasTargetedVersionFolder)
				{
					list2.Add(list[i].TargetedPath + Path.DirectorySeparatorChar + "world");
				}
				if (list[i].hasNewestFolder)
				{
					list2.Add(list[i].NewestPath + Path.DirectorySeparatorChar + "world");
				}
				list2.Add(list[i].path + Path.DirectorySeparatorChar + "world");
				for (int j = 0; j < list2.Count; j++)
				{
					if (Directory.Exists(list2[j]))
					{
						bool flag = list2[j] != list[i].path + Path.DirectorySeparatorChar + "world";
						PopulateWorldMapImageDictionary(applyer, list[i].path, list2[j], flag, !flag);
					}
				}
			}
			worldMapImagesTouchedByCurrentMod.Clear();
			List<string> list3 = new List<string>();
			foreach (KeyValuePair<string, List<string>> worldMapImage in applyer.worldMapImages)
			{
				if (worldMapImage.Value.Count > 2 && applyer.worldMapImageMetas.ContainsKey(worldMapImage.Key) && applyer.worldMapImageMetas[worldMapImage.Key].Count == applyer.worldMapImages[worldMapImage.Key].Count)
				{
					list3.Add(worldMapImage.Key);
				}
			}
			if (list3.Count == 0)
			{
				return;
			}
			applyer.mapsToGenerate = list3.Count;
			applyer.mapsGeneratedSoFar = 0;
			foreach (string item in list3)
			{
				applyer.nextMapKeyToGenerate = item;
				while (applyer.nextMapKeyToGenerate != null)
				{
					Thread.Sleep(0);
				}
				applyer.mapsGeneratedSoFar++;
			}
			applyer.finalizeApplyingMaps = true;
			while (applyer.finalizeApplyingMaps)
			{
				Thread.Sleep(0);
			}
			applyer.mapsToGenerate = 0;
			applyer.worldMapImages = null;
			applyer.worldMapImageMetas = null;
		}

		private void PopulateWorldMapImageDictionary(ModApplyer applyer, string baseDir, string worldPath, bool populateRedundant, bool skipRedundant)
		{
			string[] directories = Directory.GetDirectories(worldPath);
			for (int i = 0; i < directories.Length; i++)
			{
				if (Path.GetFileName(directories[i]).Length != 2)
				{
					continue;
				}
				string[] files = Directory.GetFiles(directories[i]);
				for (int j = 0; j < files.Length; j++)
				{
					if (skipRedundant && worldMapImagesTouchedByCurrentMod.Contains(Path.GetFileName(files[j]).ToLowerInvariant()))
					{
						continue;
					}
					if (Path.GetFileName(files[j]).StartsWith("map_image_") && Path.GetFileName(files[j]).EndsWith(".txt"))
					{
						string key = Path.GetFileNameWithoutExtension(files[j]).Replace("image_", "");
						if (!applyer.worldMapImageMetas.ContainsKey(key))
						{
							applyer.worldMapImageMetas.Add(key, new List<string>());
						}
						applyer.worldMapImageMetas[key].Add(files[j]);
						if (populateRedundant)
						{
							worldMapImagesTouchedByCurrentMod.Add(Path.GetFileName(files[j]).ToLowerInvariant());
						}
					}
					else if (Path.GetFileName(files[j]).StartsWith("map_") && Path.GetFileName(files[j]).EndsWith(".png"))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[j]);
						if (!applyer.worldMapImages.ContainsKey(fileNameWithoutExtension))
						{
							applyer.worldMapImages.Add(fileNameWithoutExtension, new List<string>());
						}
						if (!applyer.worldMapBasePaths.ContainsKey(fileNameWithoutExtension))
						{
							applyer.worldMapBasePaths.Add(fileNameWithoutExtension, new List<string>());
						}
						applyer.worldMapImages[fileNameWithoutExtension].Add(files[j]);
						applyer.worldMapBasePaths[fileNameWithoutExtension].Add(baseDir);
						if (populateRedundant)
						{
							worldMapImagesTouchedByCurrentMod.Add(Path.GetFileName(files[j]).ToLowerInvariant());
						}
					}
				}
			}
		}

		[Obsolete("Use five parameter function instead")]
		private void PopulateWorldMapImageDictionary(ModApplyer applyer, string baseDir, string worldPath)
		{
			PopulateWorldMapImageDictionary(applyer, baseDir, worldPath, populateRedundant: false, skipRedundant: false);
		}

		public void AddPendingApply(Mod modApplyFrom, string basePath, string moddedPath, bool isModification)
		{
			if (!moddedFiles.ContainsKey(basePath))
			{
				moddedFiles[basePath] = new List<PendingApply>();
				string text = AssetManager.ResolveFilePath(basePath.Substring(1), skipMergedMods: true);
				if (File.Exists(text))
				{
					moddedFiles[basePath].Add(new PendingApply(modApplyFrom, text, isVanilla: true, isModification: false));
				}
			}
			moddedFiles[basePath].Add(new PendingApply(modApplyFrom, moddedPath, isVanilla: false, isModification));
		}

		[Obsolete("Use four parameter function instead")]
		public void AddPendingApply(Mod modApplyFrom, string basePath, string moddedPath, bool hasVanilla, bool isModification)
		{
			AddPendingApply(modApplyFrom, basePath, moddedPath, isModification);
		}

		public void ExecutePendingMerge(ModApplyer applyer)
		{
			try
			{
				applyer.applyFileInd = 0;
				applyer.applyFileLength = 0;
				foreach (KeyValuePair<string, List<PendingApply>> moddedFile in moddedFiles)
				{
					List<PendingApply> value = moddedFile.Value;
					if (value.Count > 1 && (value.Count != 2 || !value[0].isVanilla || value[1].isModification))
					{
						applyer.applyFileLength++;
					}
				}
				foreach (KeyValuePair<string, List<PendingApply>> moddedFile2 in moddedFiles)
				{
					string key = moddedFile2.Key;
					List<PendingApply> value2 = moddedFile2.Value;
					if (value2.Count <= 1 || (value2.Count == 2 && value2[0].isVanilla && !value2[1].isModification))
					{
						continue;
					}
					applyer.applyFileInd++;
					string text = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods" + key).ToLowerInvariant();
					if (text.Contains("strings.txt"))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text));
						File.Copy(value2[0].filePath, text, overwrite: true);
						for (int i = 1; i < value2.Count; i++)
						{
							string input = InGameTranslator.EncryptDecryptFile(value2[i].filePath, encryptMode: false, returnOnly: true);
							MergeShortStrings(value2[i].modApplyFrom, text, Regex.Split(input, "\r\n"));
						}
						continue;
					}
					AssetManager.ResolveFilePath(key.Substring(1));
					PendingApply pendingApply = null;
					if (value2[0].isVanilla)
					{
						pendingApply = value2[0];
					}
					PendingApply pendingApply2 = null;
					List<PendingApply> list = new List<PendingApply>();
					List<PendingApply> list2 = new List<PendingApply>();
					for (int j = ((pendingApply != null) ? 1 : 0); j < value2.Count; j++)
					{
						bool flag = false;
						if (value2[j].mergeLines != null)
						{
							list.Add(value2[j]);
							flag = true;
						}
						if (value2[j].isModification)
						{
							list2.Add(value2[j]);
							flag = true;
						}
						if (!flag)
						{
							pendingApply2 = value2[j];
						}
					}
					if (pendingApply == null && pendingApply2 == null)
					{
						continue;
					}
					if (pendingApply2 != null)
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text));
						File.Copy(pendingApply2.filePath, text, overwrite: true);
					}
					else
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text));
						File.Copy(pendingApply.filePath, text, overwrite: true);
					}
					foreach (PendingApply item in list)
					{
						item.ApplyMerges(item.modApplyFrom, this, text);
					}
					foreach (PendingApply item2 in list2)
					{
						item2.ApplyModifications(text);
					}
				}
				applyer.applyFileLength = 0;
				applyer.applyFileInd = 0;
			}
			catch (Exception ex)
			{
				Custom.LogWarning("EXCEPTION IN ExecutePendingMerge:", ex.Message, "::", ex.StackTrace);
				throw;
			}
		}

		public static void MergeConcat(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			string[] array = File.ReadAllLines(sourcePath);
			string[] array2 = new string[array.Length + mergeLines.Length];
			array.CopyTo(array2, 0);
			mergeLines.CopyTo(array2, array.Length);
			WriteMergedFile(modApplyFrom, sourcePath, array2);
		}

		public void MergeUniqueLinesUnordered(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			HashSet<string> hashSet = new HashSet<string>();
			string[] array = File.ReadAllLines(sourcePath);
			foreach (string item in array)
			{
				hashSet.Add(item);
			}
			array = mergeLines;
			foreach (string item2 in array)
			{
				hashSet.Add(item2);
			}
			string[] array2 = new string[hashSet.Count];
			hashSet.CopyTo(array2);
			WriteMergedFile(modApplyFrom, sourcePath, array2);
		}

		public static void MergeUniqueItemsUnordered(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			bool flag = sourcePath.ToLowerInvariant().Contains("sounds.txt");
			bool flag2 = sourcePath.ToLowerInvariant().Contains("properties.txt");
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < 2; i++)
			{
				string[] array = ((i == 0) ? File.ReadAllLines(sourcePath) : mergeLines);
				foreach (string text in array)
				{
					if (text.StartsWith("//"))
					{
						continue;
					}
					int num = text.IndexOf(":");
					if (num < 0)
					{
						continue;
					}
					string key = text.Substring(0, num);
					if (flag)
					{
						int num2 = text.IndexOf("/");
						if (num2 >= 0 && num2 < num)
						{
							key = text.Substring(0, num2);
						}
					}
					if (flag2)
					{
						if (text.StartsWith("Room_Attr:"))
						{
							int length = "Room_Attr:".Length;
							string text2 = text.Substring(length);
							if (text2.Contains(":"))
							{
								key = text.Substring(0, length + text2.IndexOf(":"));
							}
						}
						if (text.StartsWith("Broken Shelters:"))
						{
							int length2 = "Broken Shelters:".Length;
							string text3 = text.Substring(length2);
							if (text3.Contains(":"))
							{
								key = text.Substring(0, length2 + text3.IndexOf(":"));
							}
						}
						if (text.StartsWith("Subregion:"))
						{
							string item = text.Substring("Subregion:".Length).Trim();
							if (!list.Contains(item))
							{
								list.Add(item);
							}
							continue;
						}
						if (text.StartsWith("Room Setting Templates:"))
						{
							string[] array2 = text.Substring("Room Setting Templates:".Length).Trim().Split(',');
							for (int k = 0; k < array2.Length; k++)
							{
								string item2 = array2[k].Trim();
								if (!list2.Contains(item2))
								{
									list2.Add(item2);
								}
							}
							continue;
						}
					}
					dictionary[key] = text;
				}
			}
			if (list2.Count > 0)
			{
				dictionary["Room Setting Templates"] = "Room Setting Templates: " + string.Join(", ", list2);
			}
			for (int l = 0; l < list.Count; l++)
			{
				dictionary["Subregion_" + l] = "Subregion: " + list[l];
			}
			WriteMergedFile(modApplyFrom, sourcePath, dictionary.Values.ToArray());
		}

		public static void MergeShortStrings(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			string input = InGameTranslator.EncryptDecryptFile(sourcePath, encryptMode: false, returnOnly: true);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < 2; i++)
			{
				string[] obj = ((i == 0) ? Regex.Split(input, "\r\n") : mergeLines);
				bool flag = true;
				string[] array = obj;
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j];
					if (flag)
					{
						if (text.StartsWith("0"))
						{
							text = text.Remove(0, 1);
						}
						flag = false;
					}
					int num = text.IndexOf("|");
					if (num >= 0)
					{
						string key = text.Substring(0, num);
						dictionary[key] = text;
					}
				}
			}
			string[] array2 = dictionary.Values.ToArray();
			if (array2.Length != 0)
			{
				array2[0] = "0" + array2[0];
			}
			WriteMergedFile(modApplyFrom, sourcePath, array2);
		}

		public void MergeRoomSettings(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < 2; i++)
			{
				string[] array = ((i == 0) ? File.ReadAllLines(sourcePath) : mergeLines);
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j];
					int num = text.IndexOf(":");
					if (num < 0)
					{
						continue;
					}
					string text2 = text.Substring(0, num);
					if ((i == 1 && text2 == "Palette") || text2 == "FadePalette")
					{
						string text3 = UpdatePaletteLineWithConflict(text2, text.Substring(num + 1));
						if (text3 != null)
						{
							text = text3;
						}
					}
					switch (text2)
					{
					case "Effects":
					case "PlacedObjects":
					case "AmbientSounds":
					case "Triggers":
					{
						if (!dictionary.ContainsKey(text2))
						{
							break;
						}
						string[] array2 = dictionary[text2].Substring(num + 1).Split(',');
						string[] array3 = text.Substring(num + 1).Split(',');
						HashSet<string> hashSet = new HashSet<string>();
						string[] array4 = array2;
						foreach (string text4 in array4)
						{
							if (!(text4.Trim() == string.Empty))
							{
								hashSet.Add(text4);
							}
						}
						array4 = array3;
						foreach (string text5 in array4)
						{
							if (!(text5.Trim() == string.Empty))
							{
								hashSet.Add(text5);
							}
						}
						dictionary[text2] = text2 + ":" + string.Join(",", hashSet.ToArray()) + ", ";
						continue;
					}
					}
					if (text2 == "FadePalette" && dictionary.ContainsKey(text2))
					{
						string[] array5 = dictionary[text2].Substring(num + 1).Split(',');
						if (text.Substring(num + 1).Split(',').Length >= array5.Length)
						{
							dictionary[text2] = text;
						}
					}
					else
					{
						dictionary[text2] = text;
					}
				}
			}
			WriteMergedFile(modApplyFrom, sourcePath, dictionary.Values.ToArray());
		}

		public void MergeMapFiles(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			List<string> list = new List<string>();
			for (int i = 0; i < 2; i++)
			{
				string[] array = ((i == 0) ? File.ReadAllLines(sourcePath) : mergeLines);
				foreach (string text in array)
				{
					if (text.StartsWith("Connection:"))
					{
						string[] array2 = text.Split(':');
						if (array2.Length < 2)
						{
							continue;
						}
						string[] array3 = Regex.Split(array2[1], ",");
						if (array3.Length < 3)
						{
							continue;
						}
						string text2 = "";
						for (int k = 2; k < array3.Length; k++)
						{
							text2 += array3[k];
							if (k < array3.Length - 1)
							{
								text2 += ",";
							}
						}
						dictionary2["Connection: " + array3[0].Trim() + "," + array3[1].Trim()] = text2;
					}
					else if (!text.StartsWith("Def_Mat:"))
					{
						string[] array4 = text.Split(':');
						if (array4.Length >= 2)
						{
							string key = array4[0].Trim();
							string value = array4[1].Trim();
							dictionary[key] = value;
						}
					}
					else
					{
						list.Add(text);
					}
				}
			}
			string[] array5 = new string[dictionary.Keys.Count + dictionary2.Keys.Count + list.Count];
			int num = 0;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				array5[num] = item.Key + ": " + item.Value;
				num++;
			}
			foreach (KeyValuePair<string, string> item2 in dictionary2)
			{
				array5[num] = item2.Key + "," + item2.Value;
				num++;
			}
			foreach (string item3 in list)
			{
				array5[num] = item3;
				num++;
			}
			WriteMergedFile(modApplyFrom, sourcePath, array5);
		}

		public void MergeWorldFiles(Mod modApplyFrom, string sourcePath, string[] mergeLines)
		{
			WorldFile worldFile = new WorldFile(File.ReadAllLines(sourcePath));
			WorldFile worldFile2 = new WorldFile(mergeLines);
			for (int i = 0; i < worldFile2.exclusives.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < worldFile.exclusives.Count; j++)
				{
					if (worldFile.exclusives[j].ToString() == worldFile2.exclusives[i].ToString())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					worldFile.exclusives.Add(worldFile2.exclusives[i]);
				}
				for (int num = worldFile.hidden.Count - 1; num >= 0; num--)
				{
					if (worldFile.hidden[num].Key() == worldFile2.exclusives[i].Key())
					{
						worldFile.hidden.RemoveAt(num);
					}
				}
			}
			for (int k = 0; k < worldFile2.hidden.Count; k++)
			{
				bool flag2 = false;
				for (int l = 0; l < worldFile.hidden.Count; l++)
				{
					if (worldFile.hidden[l].ToString() == worldFile2.hidden[k].ToString())
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					worldFile.hidden.Add(worldFile2.hidden[k]);
				}
				for (int num2 = worldFile.exclusives.Count - 1; num2 >= 0; num2--)
				{
					if (worldFile.exclusives[num2].Key() == worldFile2.hidden[k].Key())
					{
						worldFile.exclusives.RemoveAt(num2);
					}
				}
			}
			for (int m = 0; m < worldFile2.substitutes.Count; m++)
			{
				bool flag3 = false;
				for (int n = 0; n < worldFile.substitutes.Count; n++)
				{
					if (worldFile.substitutes[n].Key() == worldFile2.substitutes[m].Key())
					{
						flag3 = true;
						worldFile.substitutes[n] = worldFile2.substitutes[m];
						break;
					}
				}
				if (!flag3)
				{
					worldFile.substitutes.Add(worldFile2.substitutes[m]);
				}
			}
			for (int num3 = 0; num3 < worldFile2.rooms.Count; num3++)
			{
				WorldRoomLink worldRoomLink = null;
				WorldRoomLink worldRoomLink2 = worldFile2.rooms[num3];
				for (int num4 = 0; num4 < worldFile.rooms.Count; num4++)
				{
					if (worldFile.rooms[num4].roomName == worldFile2.rooms[num3].roomName)
					{
						worldRoomLink = worldFile.rooms[num4];
						break;
					}
				}
				if (worldRoomLink == null)
				{
					worldFile.rooms.Add(worldRoomLink2);
					continue;
				}
				List<string> list = new List<string>();
				int num5 = 0;
				int num6 = 0;
				while (num5 < worldRoomLink2.connections.Count || num6 < worldRoomLink.connections.Count)
				{
					if (num5 >= worldRoomLink2.connections.Count)
					{
						if (worldRoomLink.connections[num6] != "DISCONNECTED" && !list.Contains(worldRoomLink.connections[num6]))
						{
							list.Add(worldRoomLink.connections[num6]);
						}
						num6++;
						continue;
					}
					if (num6 >= worldRoomLink.connections.Count)
					{
						if (worldRoomLink2.connections[num5] != "DISCONNECTED" && !list.Contains(worldRoomLink2.connections[num5]))
						{
							list.Add(worldRoomLink2.connections[num5]);
						}
						num5++;
						continue;
					}
					if (worldRoomLink.connections[num6] == worldRoomLink2.connections[num5] && !list.Contains(worldRoomLink.connections[num6]))
					{
						list.Add(worldRoomLink.connections[num6]);
						num6++;
						num5++;
						continue;
					}
					if (worldRoomLink.connections[num6] == "DISCONNECTED" && worldRoomLink.connections.Contains(worldRoomLink2.connections[num5]))
					{
						list.Add(worldRoomLink.connections[num6]);
						num6++;
						continue;
					}
					if (worldRoomLink2.connections[num5] != "DISCONNECTED" && worldRoomLink.connections.Contains(worldRoomLink2.connections[num5]) && !list.Contains(worldRoomLink2.connections[num5]))
					{
						list.Add(worldRoomLink2.connections[num5]);
						num5++;
						continue;
					}
					if (worldRoomLink2.connections[num5] == "DISCONNECTED" || (worldRoomLink2.connections[num5] != "DISCONNECTED" && !list.Contains(worldRoomLink2.connections[num5])))
					{
						bool flag4 = false;
						for (int num7 = 0; num7 < list.Count; num7++)
						{
							if (list[num7] == "DISCONNECTED")
							{
								list[num7] = worldRoomLink2.connections[num5];
								flag4 = true;
								break;
							}
						}
						if (!flag4)
						{
							for (int num8 = num6; num8 < worldRoomLink.connections.Count; num8++)
							{
								if (worldRoomLink.connections[num8] == "DISCONNECTED")
								{
									worldRoomLink.connections[num8] = worldRoomLink2.connections[num5];
									flag4 = true;
									if (num8 == num6)
									{
										list.Add(worldRoomLink2.connections[num5]);
									}
									break;
								}
							}
						}
						if (!flag4)
						{
							list.Add(worldRoomLink2.connections[num5]);
						}
					}
					num5++;
					num6++;
				}
				worldRoomLink.connections = list;
				if (worldRoomLink2.specialType != null && worldRoomLink2.specialType != "")
				{
					worldRoomLink.specialType = worldRoomLink2.specialType;
				}
			}
			List<WorldRoomSpawn> list2 = new List<WorldRoomSpawn>();
			List<WorldRoomSpawn> list3 = new List<WorldRoomSpawn>();
			for (int num9 = 0; num9 < worldFile2.creatures.Count; num9++)
			{
				if (worldFile2.creatures[num9].excludeMode)
				{
					list2.Add(worldFile2.creatures[num9]);
				}
				else
				{
					list3.Add(worldFile2.creatures[num9]);
				}
			}
			for (int num10 = 0; num10 < list2.Count; num10++)
			{
				WorldRoomSpawn worldRoomSpawn = list2[num10];
				bool flag5 = false;
				string[] source = worldRoomSpawn.character.Split(',');
				for (int num11 = worldFile.creatures.Count - 1; num11 >= 0; num11--)
				{
					if (worldFile.creatures[num11].roomName == worldRoomSpawn.roomName && worldFile.creatures[num11].lineageDen == worldRoomSpawn.lineageDen)
					{
						if (worldFile.creatures[num11].excludeMode)
						{
							flag5 = true;
							worldFile.creatures[num11] = worldRoomSpawn;
						}
						else if (worldFile.creatures[num11].character == "" || !source.Contains(worldFile.creatures[num11].character))
						{
							worldFile.creatures.RemoveAt(num11);
						}
					}
				}
				if (!flag5)
				{
					worldFile.creatures.Add(worldRoomSpawn);
				}
			}
			for (int num12 = 0; num12 < list3.Count; num12++)
			{
				WorldRoomSpawn worldRoomSpawn2 = list3[num12];
				if (worldRoomSpawn2.lineageDen >= 0 && worldRoomSpawn2.roomName == "OFFSCREEN")
				{
					worldFile.creatures.Add(worldRoomSpawn2);
					continue;
				}
				bool flag6 = false;
				for (int num13 = worldFile.creatures.Count - 1; num13 >= 0; num13--)
				{
					if (worldFile.creatures[num13].roomName == worldRoomSpawn2.roomName && worldFile.creatures[num13].lineageDen == worldRoomSpawn2.lineageDen)
					{
						if (!worldFile.creatures[num13].excludeMode && worldFile.creatures[num13].character == worldRoomSpawn2.character)
						{
							worldFile.creatures[num13] = worldRoomSpawn2;
							flag6 = true;
						}
						if (worldFile.creatures[num13].excludeMode && worldRoomSpawn2.character != "" && !worldFile.creatures[num13].character.Split(',').Contains(worldRoomSpawn2.character))
						{
							WorldRoomSpawn worldRoomSpawn3 = worldFile.creatures[num13];
							worldRoomSpawn3.character = worldRoomSpawn3.character + "," + worldRoomSpawn2.character;
						}
					}
				}
				if (!flag6)
				{
					worldFile.creatures.Add(worldRoomSpawn2);
				}
			}
			foreach (string migrationBlockage in worldFile2.migrationBlockages)
			{
				if (!worldFile.migrationBlockages.Contains(migrationBlockage))
				{
					worldFile.migrationBlockages.Add(migrationBlockage);
				}
			}
			foreach (string unknownContextLine in worldFile2.unknownContextLines)
			{
				if (!worldFile.unknownContextLines.Contains(unknownContextLine))
				{
					worldFile.unknownContextLines.Add(unknownContextLine);
				}
			}
			WriteMergedFile(modApplyFrom, sourcePath, worldFile.ToFileLines());
		}

		public void DeterminePaletteConflicts(string modPath)
		{
			paletteRemapping.Clear();
			string path = modPath + Path.DirectorySeparatorChar + "palettes";
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] files = Directory.GetFiles(path);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].ToLowerInvariant().EndsWith(".png") && !File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "palettes" + Path.DirectorySeparatorChar + Path.GetFileName(files[i])) && int.TryParse(Path.GetFileNameWithoutExtension(files[i].ToLowerInvariant()).Substring("palette".Length), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					int j;
					for (j = result; palettesUsedSoFar.Contains(j); j += 100)
					{
					}
					if (j != result)
					{
						paletteRemapping[result] = j;
					}
					palettesUsedSoFar.Add(j);
					string text = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods" + Path.DirectorySeparatorChar + "palettes";
					Directory.CreateDirectory(text);
					File.Copy(files[i], text + Path.DirectorySeparatorChar + "palette" + j + ".png", overwrite: true);
				}
			}
		}

		public string UpdatePaletteLineWithConflict(string lineKey, string lineValue)
		{
			if (lineKey == "Palette" && int.TryParse(lineValue.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && paletteRemapping.ContainsKey(result))
			{
				return "Palette: " + paletteRemapping[result];
			}
			if (lineKey == "FadePalette")
			{
				string[] array = lineValue.Split(',');
				if (int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) && paletteRemapping.ContainsKey(result2))
				{
					array[0] = paletteRemapping[result2].ToString(CultureInfo.InvariantCulture);
					return "FadePalette: " + string.Join(", ", array);
				}
			}
			return null;
		}

		public static void WriteMergedFile(Mod sourceMod, string sourcePath, string[] mergeLines)
		{
			string text = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods";
			string text2 = Directory.GetParent(sourceMod.path).FullName.ToLowerInvariant();
			string[] array = new string[5]
			{
				"streamingassets" + Path.DirectorySeparatorChar + "mergedmods",
				text2 + Path.DirectorySeparatorChar + "v1.9.15b",
				text2 + Path.DirectorySeparatorChar + "newest",
				text2,
				"streamingassets"
			};
			foreach (string text3 in array)
			{
				if (sourcePath.ToLowerInvariant().Contains(text3))
				{
					string path = (text + sourcePath.Substring(sourcePath.ToLowerInvariant().IndexOf(text3) + text3.Length)).ToLowerInvariant();
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					File.WriteAllLines(path, mergeLines);
					if (sourcePath.Contains("text_") && !sourcePath.Contains("strings.txt"))
					{
						InGameTranslator.EncryptDecryptFile(path, encryptMode: true);
					}
					break;
				}
			}
		}
	}

	public static List<Mod> ActiveMods = new List<Mod>();

	public static List<Mod> InstalledMods = new List<Mod>();

	public static List<string> FailedRequirementIds = new List<string>();

	public static List<string> PrePackagedModIDs = new List<string>();

	public static bool MSC;

	public static bool MMF;

	public static bool DevTools;

	public static bool Expedition;

	public static bool NonPrepackagedModsInstalled;

	public static bool InitializationScreenFinished;

	public static bool CoopAvailable;

	public static bool JollyCoop;

	public static bool GameVersionChangedOnThisLaunch;

	public static Dictionary<Assembly, List<string>> ProblematicAssemblies = new Dictionary<Assembly, List<string>>();

	public static bool ModdingEnabled => ActiveMods.Count > 0;

	public static bool ModdedRegionsEnabled
	{
		get
		{
			for (int i = 0; i < ActiveMods.Count; i++)
			{
				if (ActiveMods[i].modifiesRegions)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool MergedModsContentAvailable()
	{
		if (Directory.Exists(Path.Combine(Custom.RootFolderDirectory(), "mergedmods")))
		{
			return Directory.EnumerateFileSystemEntries(Path.Combine(Custom.RootFolderDirectory(), "mergedmods")).Any();
		}
		return false;
	}

	public static bool ModFolderHasDLLContent(string folder)
	{
		if (Directory.Exists(Path.Combine(folder, Path.Combine("v1.9.15b", "plugins"))) || Directory.Exists(Path.Combine(folder, Path.Combine("v1.9.15b", "patchers"))))
		{
			return true;
		}
		if (Directory.Exists(Path.Combine(folder, Path.Combine("newest", "plugins"))) || Directory.Exists(Path.Combine(folder, Path.Combine("newest", "patchers"))))
		{
			return true;
		}
		if (Directory.Exists(Path.Combine(folder, "plugins")) || Directory.Exists(Path.Combine(folder, "patchers")))
		{
			return true;
		}
		return false;
	}

	public static void RefreshModsLists(RainWorld rainWorld)
	{
		ActiveMods.Clear();
		InstalledMods.Clear();
		FailedRequirementIds.Clear();
		MSC = false;
		MMF = false;
		DevTools = false;
		JollyCoop = false;
		CoopAvailable = false;
		Expedition = false;
		NonPrepackagedModsInstalled = false;
		string[] directories = Directory.GetDirectories(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mods");
		for (int i = 0; i < 2; i++)
		{
			string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mods";
			if (i == 1)
			{
				break;
			}
			if (!Directory.Exists(path))
			{
				continue;
			}
			string[] directories2 = Directory.GetDirectories(path);
			for (int j = 0; j < directories2.Length; j++)
			{
				string fileName = Path.GetFileName(directories2[j]);
				string basepath = directories2[j];
				for (int k = 0; k < directories.Length; k++)
				{
					if (Path.GetFileName(directories[k]) == fileName)
					{
						basepath = directories[k];
						break;
					}
				}
				Mod mod = LoadModFromJson(rainWorld, basepath, directories2[j]);
				if (mod == null)
				{
					continue;
				}
				bool flag = false;
				for (int l = 0; l < InstalledMods.Count; l++)
				{
					if (InstalledMods[l].id == mod.id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					InstalledMods.Add(mod);
				}
			}
		}
		if (rainWorld.processManager != null && rainWorld.processManager.mySteamManager != null)
		{
			PublishedFileId_t[] subscribedItems = rainWorld.processManager.mySteamManager.GetSubscribedItems();
			for (int m = 0; m < subscribedItems.Length; m++)
			{
				if (!SteamUGC.GetItemInstallInfo(subscribedItems[m], out var _, out var pchFolder, 1024u, out var _))
				{
					continue;
				}
				Mod mod2 = LoadModFromJson(rainWorld, pchFolder, pchFolder);
				if (mod2 == null)
				{
					continue;
				}
				bool flag2 = false;
				for (int n = 0; n < InstalledMods.Count; n++)
				{
					if (InstalledMods[n].id == mod2.id)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					mod2.workshopId = subscribedItems[m].m_PublishedFileId;
					mod2.workshopMod = true;
					InstalledMods.Add(mod2);
				}
			}
		}
		MachineConnector._RefreshOIs();
		int num = 0;
		while (num < InstalledMods.Count)
		{
			if (rainWorld.options.modLoadOrder.ContainsKey(InstalledMods[num].id))
			{
				InstalledMods[num].loadOrder = rainWorld.options.modLoadOrder[InstalledMods[num].id];
			}
			Mod mod3 = InstalledMods[num];
			if (!FailedRequirementIds.Contains(mod3.id))
			{
				bool flag3 = false;
				for (int num2 = 0; num2 < mod3.requirements.Length; num2++)
				{
					if (FailedRequirementIds.Contains(mod3.requirements[num2]))
					{
						flag3 = true;
						break;
					}
					bool flag4 = false;
					for (int num3 = 0; num3 < InstalledMods.Count; num3++)
					{
						if (InstalledMods[num3].id == mod3.requirements[num2])
						{
							flag4 = true;
							break;
						}
					}
					if (!flag4)
					{
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					FailedRequirementIds.Add(mod3.id);
					num = 0;
					continue;
				}
			}
			num++;
		}
		for (int num4 = 0; num4 < rainWorld.options.enabledMods.Count; num4++)
		{
			for (int num5 = 0; num5 < InstalledMods.Count; num5++)
			{
				if (InstalledMods[num5].id == rainWorld.options.enabledMods[num4] && !InstalledMods[num5].DLCMissing)
				{
					InstalledMods[num5].enabled = true;
					break;
				}
			}
		}
		PrePackagedModIDs.Clear();
		for (int num6 = 0; num6 < InstalledMods.Count; num6++)
		{
			Mod mod4 = InstalledMods[num6];
			if (mod4.id == global::MoreSlugcats.MoreSlugcats.MOD_ID)
			{
				PrePackagedModIDs.Add(global::MoreSlugcats.MoreSlugcats.MOD_ID);
			}
			else if (mod4.id == DevInterface.DevTools.MOD_ID)
			{
				PrePackagedModIDs.Add(DevInterface.DevTools.MOD_ID);
			}
			else if (mod4.id == MoreSlugcats.MMF.MOD_ID)
			{
				PrePackagedModIDs.Add(MoreSlugcats.MMF.MOD_ID);
			}
			else if (mod4.id == global::JollyCoop.JollyCoop.MOD_ID)
			{
				PrePackagedModIDs.Add(global::JollyCoop.JollyCoop.MOD_ID);
			}
			else if (mod4.id == global::Expedition.Expedition.MOD_ID)
			{
				PrePackagedModIDs.Add(global::Expedition.Expedition.MOD_ID);
			}
			else
			{
				NonPrepackagedModsInstalled = true;
			}
		}
		if (rainWorld.options.enabledMods.Count > 0 && ((rainWorld.options.lastGameVersion != null && "v1.9.15b" != rainWorld.options.lastGameVersion) || rainWorld.options.lastGameVersion == null))
		{
			for (int num7 = 0; num7 < InstalledMods.Count; num7++)
			{
				if (!PrePackagedModIDs.Contains(InstalledMods[num7].id))
				{
					InstalledMods[num7].enabled = false;
					if (rainWorld.options.enabledMods.Contains(InstalledMods[num7].id))
					{
						rainWorld.options.enabledMods.Remove(InstalledMods[num7].id);
					}
				}
			}
			string path2 = Path.Combine(Custom.RootFolderDirectory(), "enabledMods.txt");
			if (File.Exists(path2))
			{
				File.WriteAllText(path2, "");
			}
			GameVersionChangedOnThisLaunch = true;
		}
		File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "enabledModsVersion.txt"), "v1.9.15b");
		for (int num8 = 0; num8 < InstalledMods.Count; num8++)
		{
			Mod mod5 = InstalledMods[num8];
			if (mod5.enabled && !FailedRequirementIds.Contains(mod5.id))
			{
				ActiveMods.Add(mod5);
				if (mod5.id == global::MoreSlugcats.MoreSlugcats.MOD_ID)
				{
					MSC = true;
				}
				if (mod5.id == DevInterface.DevTools.MOD_ID)
				{
					DevTools = true;
				}
				if (mod5.id == MoreSlugcats.MMF.MOD_ID)
				{
					MMF = true;
				}
				if (mod5.id == global::JollyCoop.JollyCoop.MOD_ID)
				{
					JollyCoop = true;
				}
				if (mod5.id == global::Expedition.Expedition.MOD_ID)
				{
					Expedition = true;
				}
			}
		}
		rainWorld.LoadModResourcesCheck = true;
		ActiveMods = ActiveMods.OrderBy((Mod o) => o.loadOrder).ToList();
	}

	public static string ComputeModChecksum(string modpath)
	{
		if (File.Exists(modpath + Path.DirectorySeparatorChar + "modinfo.json"))
		{
			Dictionary<string, object> dictionary = File.ReadAllText(modpath + Path.DirectorySeparatorChar + "modinfo.json").dictionaryFromJson();
			if (dictionary != null && dictionary.ContainsKey("checksum_override_version") && dictionary.ContainsKey("version") && (bool)dictionary["checksum_override_version"])
			{
				return dictionary["version"].ToString();
			}
		}
		return AssetManager.CreateDirectoryMd5(modpath.ToLowerInvariant(), Custom.RootFolderDirectory().ToLowerInvariant());
	}

	public static Mod LoadModFromJson(RainWorld rainWorld, string basepath, string modpath)
	{
		Mod mod = new Mod
		{
			id = Path.GetFileName(modpath),
			name = Path.GetFileName(modpath),
			version = "",
			hideVersion = false,
			targetGameVersion = "v1.9.15b",
			authors = "Unknown",
			description = "No Description.",
			path = modpath,
			basePath = basepath,
			checksum = "",
			checksumChanged = false,
			checksumOverrideVersion = false,
			requirements = new string[0],
			requirementsNames = new string[0],
			tags = new string[0],
			priorities = new string[0],
			modifiesRegions = false,
			workshopId = 0uL,
			workshopMod = false,
			hasDLL = false,
			loadOrder = 0,
			enabled = false
		};
		string path = basepath + Path.DirectorySeparatorChar + "modinfo.json";
		if (!File.Exists(path))
		{
			path = modpath + Path.DirectorySeparatorChar + "modinfo.json";
		}
		if (File.Exists(path))
		{
			Dictionary<string, object> dictionary = File.ReadAllText(path).dictionaryFromJson();
			if (dictionary == null)
			{
				return null;
			}
			if (dictionary.ContainsKey("id"))
			{
				mod.id = dictionary["id"].ToString();
			}
			if (dictionary.ContainsKey("name"))
			{
				mod.name = dictionary["name"].ToString();
			}
			if (dictionary.ContainsKey("version"))
			{
				mod.version = dictionary["version"].ToString();
			}
			if (dictionary.ContainsKey("hide_version"))
			{
				mod.hideVersion = (bool)dictionary["hide_version"];
			}
			if (dictionary.ContainsKey("target_game_version"))
			{
				mod.targetGameVersion = dictionary["target_game_version"].ToString();
			}
			if (dictionary.ContainsKey("authors"))
			{
				mod.authors = dictionary["authors"].ToString();
			}
			if (dictionary.ContainsKey("description"))
			{
				mod.description = dictionary["description"].ToString();
			}
			if (dictionary.ContainsKey("youtube_trailer_id"))
			{
				mod.trailerID = dictionary["youtube_trailer_id"].ToString();
			}
			if (dictionary.ContainsKey("requirements"))
			{
				mod.requirements = ((List<object>)dictionary["requirements"]).ConvertAll((object x) => x.ToString()).ToArray();
			}
			if (dictionary.ContainsKey("requirements_names"))
			{
				mod.requirementsNames = ((List<object>)dictionary["requirements_names"]).ConvertAll((object x) => x.ToString()).ToArray();
			}
			if (dictionary.ContainsKey("tags"))
			{
				mod.tags = ((List<object>)dictionary["tags"]).ConvertAll((object x) => x.ToString()).ToArray();
			}
			if (dictionary.ContainsKey("priorities"))
			{
				mod.priorities = ((List<object>)dictionary["priorities"]).ConvertAll((object x) => x.ToString()).ToArray();
			}
			if (dictionary.ContainsKey("checksum_override_version"))
			{
				mod.checksumOverrideVersion = (bool)dictionary["checksum_override_version"];
			}
		}
		if (Directory.Exists((modpath + Path.DirectorySeparatorChar + "world").ToLowerInvariant()))
		{
			mod.modifiesRegions = true;
		}
		if (ModFolderHasDLLContent(modpath))
		{
			mod.hasDLL = true;
		}
		if (Directory.Exists(Path.Combine(modpath, "v1.9.15b")))
		{
			if (Directory.GetFiles(Path.Combine(modpath, "v1.9.15b")).Length != 0)
			{
				mod.hasTargetedVersionFolder = true;
			}
			string[] directories = Directory.GetDirectories(Path.Combine(modpath, "v1.9.15b"));
			for (int i = 0; i < directories.Length; i++)
			{
				string text = Path.GetFileName(directories[i]).ToLower();
				if (text != "patchers" && text != "plugins")
				{
					mod.hasTargetedVersionFolder = true;
					break;
				}
			}
		}
		if (!mod.hasTargetedVersionFolder && Directory.Exists(Path.Combine(modpath, "newest")))
		{
			if (Directory.GetFiles(Path.Combine(modpath, "newest")).Length != 0)
			{
				mod.hasNewestFolder = true;
			}
			string[] directories2 = Directory.GetDirectories(Path.Combine(modpath, "newest"));
			for (int j = 0; j < directories2.Length; j++)
			{
				string text2 = Path.GetFileName(directories2[j]).ToLower();
				if (text2 != "patchers" && text2 != "plugins")
				{
					mod.hasNewestFolder = true;
					break;
				}
			}
		}
		string text3 = "";
		text3 = ((!mod.checksumOverrideVersion) ? ComputeModChecksum(mod.path) : mod.version);
		if (rainWorld.options.modChecksums.ContainsKey(mod.id))
		{
			mod.checksumChanged = text3 != rainWorld.options.modChecksums[mod.id];
			if (mod.checksumChanged)
			{
				Custom.LogImportant("MOD CHECKSUM CHANGED FOR", mod.name, ": Was", rainWorld.options.modChecksums[mod.id], ", is now", text3);
			}
		}
		else
		{
			Custom.LogImportant("MOD CHECKSUM DID NOT EXIST FOR", mod.name, ", NEWLY INSTALLED?");
			mod.checksumChanged = true;
		}
		mod.checksum = text3;
		return mod;
	}

	public static bool CheckForDeletedBepinexMods(RainWorld rainWorld)
	{
		string path = Path.Combine(Custom.RootFolderDirectory(), "enabledMods.txt");
		bool result = false;
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			string text = "";
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = "";
				text2 = ((!array[i].StartsWith("[WORKSHOP]")) ? Path.Combine(Custom.RootFolderDirectory(), "mods/" + array[i]) : array[i].Replace("[WORKSHOP]", "").Trim());
				if (!Directory.Exists(text2) || !ModFolderHasDLLContent(text2))
				{
					Custom.LogWarning(text2, "WAS ONE OF THE ENABLED MODS, BUT IT DOESN'T SEEM TO EXIST ANYMORE!");
					result = true;
				}
				else
				{
					text = ((!flag) ? (text + "\r\n" + array[i]) : (text + array[i]));
					flag = false;
				}
			}
			File.WriteAllText(path, text);
		}
		if (MergedModsContentAvailable() && rainWorld.options.enabledMods.Count == 0)
		{
			Custom.LogWarning("MERGEDMODS EXISTS, BUT THERE ARE NO ENABLED MODS!");
			return true;
		}
		if (!Directory.Exists(Path.Combine(Custom.RootFolderDirectory(), "mergedmods")) && rainWorld.options.enabledMods.Count > 0)
		{
			Custom.LogWarning("MERGEDMODS DOES NOT EXIST, BUT THERE ARE ENABLED MODS!");
			return true;
		}
		return result;
	}

	public static void GenerateMergedMods(ModApplyer applyer, List<bool> pendingEnabled, bool hasRegionMods)
	{
		try
		{
			string path = Path.Combine(Custom.RootFolderDirectory(), "mergedmods");
			if (Directory.Exists(path))
			{
				string[] directories = Directory.GetDirectories(path);
				for (int i = 0; i < directories.Length; i++)
				{
					if (hasRegionMods && Path.GetFileName(directories[i]).ToLowerInvariant() == "world")
					{
						string[] directories2 = Directory.GetDirectories(directories[i]);
						for (int j = 0; j < directories2.Length; j++)
						{
							if (!(Path.GetFileName(directories2[j]).ToLowerInvariant() == "indexmaps"))
							{
								Directory.Delete(directories2[j], recursive: true);
							}
						}
						string[] files = Directory.GetFiles(directories[i]);
						for (int k = 0; k < files.Length; k++)
						{
							File.Delete(files[k]);
						}
					}
					else
					{
						Directory.Delete(directories[i], recursive: true);
					}
				}
				string[] files2 = Directory.GetFiles(path);
				for (int l = 0; l < files2.Length; l++)
				{
					File.Delete(files2[l]);
				}
			}
			else
			{
				Directory.CreateDirectory(path);
			}
			ModMerger modMerger = new ModMerger();
			List<Mod> list = InstalledMods.OrderBy((Mod o) => o.loadOrder).ToList();
			for (int m = 0; m < list.Count; m++)
			{
				int num = -1;
				for (int n = 0; n < InstalledMods.Count; n++)
				{
					if (InstalledMods[n].id == list[m].id)
					{
						num = n;
						break;
					}
				}
				if (num == -1 || !pendingEnabled[num])
				{
					continue;
				}
				applyer.activeMergingMod = num;
				string modPath = list[m].path;
				if (list[m].hasTargetedVersionFolder && Directory.Exists(list[m].TargetedPath + Path.DirectorySeparatorChar + "palettes"))
				{
					modPath = list[m].TargetedPath;
				}
				else if (list[m].hasNewestFolder && Directory.Exists(list[m].NewestPath + Path.DirectorySeparatorChar + "palettes"))
				{
					modPath = list[m].NewestPath;
				}
				modMerger.DeterminePaletteConflicts(modPath);
				List<string> list2 = new List<string>();
				if (list[m].hasTargetedVersionFolder)
				{
					list2.Add(list[m].TargetedPath);
				}
				if (list[m].hasNewestFolder)
				{
					list2.Add(list[m].NewestPath);
				}
				list2.Add(list[m].path);
				applyer.mergeFileInd = 0;
				applyer.mergeFileLength = 0;
				List<string> list3 = new List<string>();
				List<string> list4 = new List<string>();
				List<bool> list5 = new List<bool>();
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					string[] files3 = Directory.GetFiles(list2[num2], "*.txt", SearchOption.AllDirectories);
					bool flag = num2 == list2.Count - 1;
					for (int num3 = 0; num3 < files3.Length; num3++)
					{
						if (!flag || (!files3[num3].Contains(list[m].TargetedPath) && !files3[num3].Contains(list[m].NewestPath)))
						{
							string text = files3[num3];
							string text2 = text.Substring(text.IndexOf(list2[num2]) + list2[num2].Length).ToLowerInvariant();
							bool item = false;
							if (text2.StartsWith(Path.DirectorySeparatorChar + "modify" + Path.DirectorySeparatorChar))
							{
								text2 = text2.Substring(text2.Substring(1).IndexOf(Path.DirectorySeparatorChar) + 1);
								item = true;
							}
							if (!list4.Contains(text2))
							{
								list3.Add(text);
								list4.Add(text2);
								list5.Add(item);
							}
						}
					}
				}
				applyer.mergeFileLength = list3.Count;
				for (int num4 = 0; num4 < list3.Count; num4++)
				{
					applyer.mergeFileInd = num4;
					modMerger.AddPendingApply(list[m], list4[num4], list3[num4], list5[num4]);
				}
			}
			modMerger.ExecutePendingMerge(applyer);
			MapMerger.MergeWorldMaps(modMerger, applyer);
		}
		catch (Exception ex)
		{
			Custom.LogWarning("EXCEPTION IN GenerateMergedMods:", ex.Message, "::", ex.StackTrace);
			throw;
		}
	}

	public static void WrapModInitHooks()
	{
		ProblematicAssemblies.Clear();
		if (File.Exists(Path.Combine(Custom.RootFolderDirectory(), "noinitwrap.txt")))
		{
			Custom.LogImportant("Skipping mod wrapping");
			return;
		}
		if ((object)AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly x) => x.GetName().Name == "HOOKS-Assembly-CSharp") == null)
		{
			Custom.LogImportant("Hooks assembly not loaded, skipping mod wrapping");
			return;
		}
		WrapUnloadResources();
		WrapLoadResources();
		WrapPreModsDisabledEnabled();
		WrapOnModsDisabled();
		WrapPreModsInit();
		WrapModsInit();
		WrapPostModsInit();
	}

	public static Dictionary<Delegate, Stack<IDetour>> GetHookMap(MethodBase method)
	{
		object obj = typeof(HookEndpointManager).GetMethod("GetEndpoint", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { method });
		return (Dictionary<Delegate, Stack<IDetour>>)typeof(HookEndpointManager).Assembly.GetType("MonoMod.RuntimeDetour.HookGen.HookEndpoint").GetField("HookMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
	}

	public static bool CheckInitIssues(Action<string> onIssue)
	{
		bool flag = false;
		Options options = Custom.rainWorld.options;
		string text = "";
		if (ProblematicAssemblies.Count > 0)
		{
			flag = true;
			text = Custom.rainWorld.inGameTranslator.Translate("Issues found while loading mods!<LINE>Disabling problematic mods.<LINE>Press continue and then launch the game again.") + "<LINE>";
			foreach (KeyValuePair<Assembly, List<string>> problematicAssembly in ProblematicAssemblies)
			{
				Assembly key = problematicAssembly.Key;
				List<string> value = problematicAssembly.Value;
				string directoryName = Path.GetDirectoryName(Path.GetDirectoryName(key.Location));
				if (!File.Exists(directoryName + Path.DirectorySeparatorChar + "modinfo.json"))
				{
					directoryName = Path.GetDirectoryName(directoryName);
				}
				Mod mod = LoadModFromJson(Custom.rainWorld, directoryName, directoryName);
				string text2 = mod?.name ?? key.GetName().Name;
				text = text + "\n" + text2 + ": " + string.Join("; ", value);
				if (mod != null)
				{
					ActiveMods.RemoveAll((Mod x) => x.id == mod.id);
					options.enabledMods.Remove(mod.id);
				}
			}
		}
		else if (ActiveMods.Count != options.enabledMods.Count || !ActiveMods.All((Mod x) => options.enabledMods.Contains(x.id)))
		{
			flag = true;
			text = Custom.rainWorld.inGameTranslator.Translate("Desync detected between user mod data and game mod data!<LINE>Re-sycing data.<LINE>Press continue and then launch the game again.<LINE>Disabled mods:") + " ";
			IEnumerable<string> collection = from x in ActiveMods.FindAll((Mod x) => !options.enabledMods.Contains(x.id))
				select x.id;
			List<string> collection2 = options.enabledMods.FindAll((string x) => !ActiveMods.Any((Mod y) => y.id == x));
			List<string> list = new List<string>();
			list.AddRange(collection);
			list.AddRange(collection2);
			text += string.Join(", ", list);
			ActiveMods.RemoveAll((Mod x) => !options.enabledMods.Contains(x.id));
			options.enabledMods.RemoveAll((string x) => !ActiveMods.Any((Mod y) => y.id == x));
		}
		if (flag)
		{
			options.modChecksums.Clear();
			options.Save();
			string text3 = "";
			List<string> list2 = new List<string>();
			foreach (Mod activeMod in ActiveMods)
			{
				if (Directory.Exists(Path.Combine(activeMod.path, "plugins")) || Directory.Exists(Path.Combine(activeMod.path, "patchers")))
				{
					if (activeMod.workshopMod)
					{
						list2.Add("[WORKSHOP]" + activeMod.path);
					}
					else
					{
						list2.Add(Path.GetFileName(activeMod.path));
					}
				}
			}
			for (int i = 0; i < list2.Count; i++)
			{
				text3 = text3 + list2[i] + ((i < list2.Count - 1) ? "\r\n" : "");
			}
			File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "enabledMods.txt"), text3);
			onIssue?.Invoke(text);
		}
		return flag;
	}

	private static void WrapUnloadResources()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("UnloadResources"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_UnloadResources item in hookMap.Keys.ToList())
			{
				hook_UnloadResources hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.UnloadResources -= hook;
				RainWorld.UnloadResources += (hook_UnloadResources)delegate(orig_UnloadResources orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_UnloadResources)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in UnloadResources");
						}
					}
				};
			}
		}
	}

	private static void WrapLoadResources()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("LoadResources"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_LoadResources item in hookMap.Keys.ToList())
			{
				hook_LoadResources hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.LoadResources -= hook;
				RainWorld.LoadResources += (hook_LoadResources)delegate(orig_LoadResources orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_LoadResources)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in LoadResources");
						}
					}
				};
			}
		}
	}

	private static void WrapPreModsDisabledEnabled()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("PreModsDisabledEnabled"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_PreModsDisabledEnabled item in hookMap.Keys.ToList())
			{
				hook_PreModsDisabledEnabled hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.PreModsDisabledEnabled -= hook;
				RainWorld.PreModsDisabledEnabled += (hook_PreModsDisabledEnabled)delegate(orig_PreModsDisabledEnabled orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_PreModsDisabledEnabled)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in PreModsDisabledEnabled");
						}
					}
				};
			}
		}
	}

	private static void WrapOnModsDisabled()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("OnModsDisabled"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_OnModsDisabled item in hookMap.Keys.ToList())
			{
				hook_OnModsDisabled hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.OnModsDisabled -= hook;
				RainWorld.OnModsDisabled += (hook_OnModsDisabled)delegate(orig_OnModsDisabled orig, RainWorld self, Mod[] newlyDisabledMods)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002d: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_OnModsDisabled)delegate(RainWorld originalSelf, Mod[] originalNewlyDisabledMods)
						{
							ranOrig = true;
							orig.Invoke(originalSelf, originalNewlyDisabledMods);
						}, self, newlyDisabledMods);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in OnModsDisabled");
						}
					}
				};
			}
		}
	}

	private static void WrapPreModsInit()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("PreModsInit"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_PreModsInit item in hookMap.Keys.ToList())
			{
				hook_PreModsInit hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.PreModsInit -= hook;
				RainWorld.PreModsInit += (hook_PreModsInit)delegate(orig_PreModsInit orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_PreModsInit)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in PreModsInit");
						}
					}
				};
			}
		}
	}

	private static void WrapModsInit()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("OnModsInit"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_OnModsInit item in hookMap.Keys.ToList())
			{
				hook_OnModsInit hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.OnModsInit -= hook;
				RainWorld.OnModsInit += (hook_OnModsInit)delegate(orig_OnModsInit orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_OnModsInit)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in OnModsInit");
						}
					}
				};
			}
		}
	}

	private static void WrapPostModsInit()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		Dictionary<Delegate, Stack<IDetour>> hookMap = GetHookMap(typeof(RainWorld).GetMethod("PostModsInit"));
		Assembly myAssembly = Assembly.GetExecutingAssembly();
		while (hookMap.Keys.Any((Delegate x) => x.Method.DeclaringType.Assembly != myAssembly))
		{
			foreach (hook_PostModsInit item in hookMap.Keys.ToList())
			{
				hook_PostModsInit hook = item;
				Assembly assembly = ((Delegate)(object)hook).Method.DeclaringType.Assembly;
				if (assembly == myAssembly)
				{
					continue;
				}
				RainWorld.PostModsInit -= hook;
				RainWorld.PostModsInit += (hook_PostModsInit)delegate(orig_PostModsInit orig, RainWorld self)
				{
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Expected O, but got Unknown
					bool ranOrig = false;
					try
					{
						hook.Invoke((orig_PostModsInit)delegate(RainWorld originalSelf)
						{
							ranOrig = true;
							orig.Invoke(originalSelf);
						}, self);
					}
					catch (Exception ex)
					{
						if (!ProblematicAssemblies.TryGetValue(assembly, out var value))
						{
							value = (ProblematicAssemblies[assembly] = new List<string>());
						}
						value.Add(ex.GetType().Name + ": " + ex.Message);
						Custom.LogWarning(ex.ToString());
					}
					finally
					{
						if (!ranOrig)
						{
							if (!ProblematicAssemblies.TryGetValue(assembly, out var value2))
							{
								value2 = (ProblematicAssemblies[assembly] = new List<string>());
							}
							value2.Add("Failed to call orig in PostModsInit");
						}
					}
				};
			}
		}
	}
}
