using System.IO;
using RWCustom;

public static class WorldChecksumController
{
	public static bool ControlCheckSum(RainWorld.BuildType buildType)
	{
		if (buildType == RainWorld.BuildType.Development)
		{
			return true;
		}
		return WorldCheckSum() == File.ReadAllText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "checksum.txt").ToLowerInvariant());
	}

	private static string WorldCheckSum()
	{
		string text = "";
		text += ReadText("default alignments.txt");
		text += ReadText("Gates" + Path.DirectorySeparatorChar + "locks.txt");
		string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "regions.txt").ToLowerInvariant());
		for (int i = 0; i < array.Length; i++)
		{
			text += ReadText(array[i] + Path.DirectorySeparatorChar + "Properties.txt");
			text += ReadText(array[i] + Path.DirectorySeparatorChar + "world_" + array[i] + ".txt");
		}
		string text2 = Custom.Md5Sum(text);
		Custom.LogImportant("world checksum:", text2);
		return text2;
	}

	private static string ReadText(string pth)
	{
		if (!File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + pth).ToLowerInvariant()))
		{
			Custom.LogWarning("world text file doesn't exist:", pth);
			return "";
		}
		return File.ReadAllText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + pth).ToLowerInvariant());
	}
}
