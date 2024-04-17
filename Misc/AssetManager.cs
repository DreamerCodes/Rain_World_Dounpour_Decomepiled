using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kittehface.Framework20;
using RWCustom;
using UnityEngine;
using UnityEngine.Networking;

public class AssetManager
{
	public static string ResolveFilePath(string path, bool skipMergedMods = false)
	{
		path = path.Replace('/', Path.DirectorySeparatorChar);
		if (!skipMergedMods)
		{
			string text = Path.Combine(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"), path.ToLowerInvariant());
			if (File.Exists(text))
			{
				return text;
			}
		}
		for (int num = ModManager.ActiveMods.Count - 1; num >= 0; num--)
		{
			if (ModManager.ActiveMods[num].hasTargetedVersionFolder)
			{
				string text2 = Path.Combine(ModManager.ActiveMods[num].TargetedPath, path.ToLowerInvariant());
				if (File.Exists(text2))
				{
					return text2;
				}
			}
			if (ModManager.ActiveMods[num].hasNewestFolder)
			{
				string text3 = Path.Combine(ModManager.ActiveMods[num].NewestPath, path.ToLowerInvariant());
				if (File.Exists(text3))
				{
					return text3;
				}
			}
			string text4 = Path.Combine(ModManager.ActiveMods[num].path, path.ToLowerInvariant());
			if (File.Exists(text4))
			{
				return text4;
			}
		}
		return Path.Combine(Custom.RootFolderDirectory(), path.ToLowerInvariant());
	}

	public static string ResolveDirectory(string path)
	{
		string text = Path.Combine(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"), path.ToLowerInvariant());
		if (Directory.Exists(text))
		{
			return text;
		}
		if (AOC.CheckMounted())
		{
			string text2 = Path.Combine(AOC.GetAOCMount(), path.ToLowerInvariant());
			if (File.Exists(text2))
			{
				return text2;
			}
			text2 = Path.Combine(AOC.GetAOCMount() + Path.DirectorySeparatorChar + "consolefiles", path.ToLowerInvariant());
			if (File.Exists(text2))
			{
				return text2;
			}
		}
		for (int num = ModManager.ActiveMods.Count - 1; num >= 0; num--)
		{
			if (ModManager.ActiveMods[num].hasTargetedVersionFolder)
			{
				string text3 = Path.Combine(ModManager.ActiveMods[num].TargetedPath, path.ToLowerInvariant());
				if (Directory.Exists(text3))
				{
					return text3;
				}
			}
			if (ModManager.ActiveMods[num].hasNewestFolder)
			{
				string text4 = Path.Combine(ModManager.ActiveMods[num].NewestPath, path.ToLowerInvariant());
				if (Directory.Exists(text4))
				{
					return text4;
				}
			}
			string text5 = Path.Combine(ModManager.ActiveMods[num].path, path.ToLowerInvariant());
			if (Directory.Exists(text5))
			{
				return text5;
			}
		}
		return Path.Combine(Custom.RootFolderDirectory(), path.ToLowerInvariant());
	}

	public static string[] ListDirectory(string path, bool directories = false, bool includeAll = false, bool moddedOnly = false)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		list3.Add(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"));
		for (int num = ModManager.ActiveMods.Count - 1; num >= 0; num--)
		{
			if (ModManager.ActiveMods[num].hasTargetedVersionFolder)
			{
				list3.Add(ModManager.ActiveMods[num].TargetedPath);
			}
			if (ModManager.ActiveMods[num].hasNewestFolder)
			{
				list3.Add(ModManager.ActiveMods[num].NewestPath);
			}
			list3.Add(ModManager.ActiveMods[num].path);
		}
		if (!moddedOnly)
		{
			list3.Add(Custom.RootFolderDirectory());
		}
		foreach (string item in list3)
		{
			string path2 = Path.Combine(item, path.ToLowerInvariant());
			if (!Directory.Exists(path2))
			{
				continue;
			}
			string[] array = (directories ? Directory.GetDirectories(path2) : Directory.GetFiles(path2));
			foreach (string text in array)
			{
				string fileName = Path.GetFileName(text);
				if (!list2.Contains(fileName) || includeAll)
				{
					list.Add(text);
					if (!includeAll)
					{
						list2.Add(fileName);
					}
				}
			}
		}
		return list.ToArray();
	}

	public static void HardCleanFutileAssets()
	{
		GC.Collect();
		HeavyTexturesCache.ClearRegisteredFutileAtlases();
		Futile.stage.renderer.Clear();
		Resources.UnloadUnusedAssets();
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == "FRenderLayer (Futile.stage)")
			{
				UnityEngine.Object.Destroy(array[i]);
				array[i] = null;
			}
		}
		UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(Material));
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j].name == "Unlit Transparent Vertex Colored")
			{
				UnityEngine.Object.Destroy(array2[j]);
				array2[j] = null;
			}
		}
	}

	private static WWW InternalWWWSafeLoad(string path)
	{
		WWW wWW = new WWW(path);
		while (!wWW.isDone)
		{
		}
		return wWW;
	}

	private static UnityWebRequest InternalTextureWebRQ(string path)
	{
		UnityWebRequest unityWebRequest = UnityWebRequest.Get(path);
		unityWebRequest.SendWebRequest();
		while (!unityWebRequest.isDone)
		{
		}
		return unityWebRequest;
	}

	public static Texture2D SafeWWWLoadTexture(ref Texture2D texture2D, string path, bool clampWrapMode, bool crispPixels)
	{
		byte[] data;
		if (path.Contains("://"))
		{
			UnityWebRequest unityWebRequest = InternalTextureWebRQ(path);
			if (unityWebRequest.error != null)
			{
				Custom.LogWarning("WWW file FAILED:", path, "error:", unityWebRequest.error);
				throw new FileLoadException("WWW loading error: " + unityWebRequest.error + " with file: " + path);
			}
			data = unityWebRequest.downloadHandler.data;
			unityWebRequest.Dispose();
		}
		else
		{
			data = File.ReadAllBytes(path);
		}
		texture2D.wrapMode = (clampWrapMode ? TextureWrapMode.Clamp : TextureWrapMode.Repeat);
		if (crispPixels)
		{
			texture2D.anisoLevel = 0;
			texture2D.filterMode = FilterMode.Point;
		}
		texture2D.LoadImage(data);
		if ((path.Contains("://") || path.Contains(":///")) && texture2D == null)
		{
			throw new FutileException("Couldn't load the atlas texture from: " + path);
		}
		return texture2D;
	}

	public static byte[] PreLoadTexture(string path)
	{
		if (path.Contains("://"))
		{
			UnityWebRequest unityWebRequest = InternalTextureWebRQ(path);
			if (unityWebRequest.error != null)
			{
				Custom.LogWarning("WWW file FAILED:", path, "error:", unityWebRequest.error);
				throw new FileLoadException("WWW loading error: " + unityWebRequest.error + " with file: " + path);
			}
			byte[] data = unityWebRequest.downloadHandler.data;
			unityWebRequest.Dispose();
			return data;
		}
		return File.ReadAllBytes(path);
	}

	public static AudioClip SafeWWWAudioClip(string path, bool threeD, bool stream, AudioType audioType)
	{
		UnityWebRequest audioClip = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
		audioClip.SendWebRequest();
		while (!audioClip.isDone)
		{
		}
		AudioClip content = DownloadHandlerAudioClip.GetContent(audioClip);
		audioClip.Dispose();
		return content;
	}

	public static string CreateDirectoryMd5(string srcPath, string relativeRoot)
	{
		string[] array = (from p in Directory.GetFiles(srcPath, "*.txt", SearchOption.AllDirectories)
			orderby p
			select p).ToArray();
		using MD5 mD = MD5.Create();
		string[] array2 = array;
		foreach (string text in array2)
		{
			string s = text;
			if (relativeRoot != "" && text.Contains(relativeRoot))
			{
				s = text.Substring(text.IndexOf(relativeRoot) + relativeRoot.Length);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			mD.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
			byte[] array3 = File.ReadAllBytes(text);
			mD.TransformBlock(array3, 0, array3.Length, array3, 0);
		}
		mD.TransformFinalBlock(new byte[0], 0, 0);
		return BitConverter.ToString(mD.Hash).Replace("-", "").ToLower();
	}

	[Obsolete("Use two parameter function instead.")]
	public static string CreateDirectoryMd5(string srcPath, string relativeRoot, List<string> obsolete)
	{
		return CreateDirectoryMd5(srcPath, relativeRoot);
	}

	public static string[] ListDirectory(string path, bool directories, bool includeAll)
	{
		return ListDirectory(path, directories, includeAll, moddedOnly: false);
	}

	public static string ResolveFilePath(string path)
	{
		return ResolveFilePath(path, skipMergedMods: false);
	}
}
