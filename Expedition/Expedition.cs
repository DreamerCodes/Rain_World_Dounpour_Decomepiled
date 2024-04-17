using System.IO;
using RWCustom;
using UnityEngine;

namespace Expedition;

public class Expedition
{
	public static ExpeditionCoreFile coreFile;

	public static string MOD_ID = "expedition";

	public static void OnInit(RainWorld rainWorld)
	{
		coreFile = new ExpeditionCoreFile(rainWorld);
		if (!Futile.atlasManager.DoesContainElementWithName("expeditiontitle"))
		{
			Texture2D texture2D = new Texture2D(0, 0);
			texture2D.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/expeditiontitle.png")));
			texture2D.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("expeditiontitle", texture2D, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("expeditionshadow"))
		{
			Texture2D texture2D2 = new Texture2D(0, 0);
			texture2D2.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/expeditionshadow.png")));
			texture2D2.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("expeditionshadow", texture2D2, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("jukebox"))
		{
			Texture2D texture2D3 = new Texture2D(0, 0);
			texture2D3.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/jukebox.png")));
			texture2D3.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("jukebox", texture2D3, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("expeditionpage"))
		{
			Texture2D texture2D4 = new Texture2D(0, 0);
			texture2D4.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/expeditionpage.png")));
			texture2D4.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("expeditionpage", texture2D4, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("slugcatselect"))
		{
			Texture2D texture2D5 = new Texture2D(0, 0);
			texture2D5.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/slugcatselect.png")));
			texture2D5.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("slugcatselect", texture2D5, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("challengeselect"))
		{
			Texture2D texture2D6 = new Texture2D(0, 0);
			texture2D6.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/challengeselect.png")));
			texture2D6.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("challengeselect", texture2D6, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("unlockables"))
		{
			Texture2D texture2D7 = new Texture2D(0, 0);
			texture2D7.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/unlockables.png")));
			texture2D7.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("unlockables", texture2D7, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("progression"))
		{
			Texture2D texture2D8 = new Texture2D(0, 0);
			texture2D8.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/progression.png")));
			texture2D8.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("progression", texture2D8, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("mission"))
		{
			Texture2D texture2D9 = new Texture2D(0, 0);
			texture2D9.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/mission.png")));
			texture2D9.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("mission", texture2D9, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("milestones"))
		{
			Texture2D texture2D10 = new Texture2D(0, 0);
			texture2D10.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/milestones.png")));
			texture2D10.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("milestones", texture2D10, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("filters"))
		{
			Texture2D texture2D11 = new Texture2D(0, 0);
			texture2D11.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/filters.png")));
			texture2D11.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("filters", texture2D11, textureFromAsset: false);
		}
		if (!Futile.atlasManager.DoesContainElementWithName("custom"))
		{
			Texture2D texture2D12 = new Texture2D(0, 0);
			texture2D12.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/custom.png")));
			texture2D12.filterMode = FilterMode.Point;
			Futile.atlasManager.LoadAtlasFromTexture("custom", texture2D12, textureFromAsset: false);
		}
		ExpLog.ClearLog();
		ChallengeOrganizer.SetupChallengeTypes();
		if (File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "expeddebug.txt").ToLowerInvariant()))
		{
			ExpeditionData.devMode = true;
		}
	}

	public static void OnDisable()
	{
		if (Futile.atlasManager.DoesContainElementWithName("expeditiontitle"))
		{
			Futile.atlasManager.UnloadAtlas("expeditiontitle");
		}
		if (Futile.atlasManager.DoesContainElementWithName("expeditionshadow"))
		{
			Futile.atlasManager.UnloadAtlas("expeditionshadow");
		}
		if (Futile.atlasManager.DoesContainElementWithName("jukebox"))
		{
			Futile.atlasManager.UnloadAtlas("jukebox");
		}
		if (Futile.atlasManager.DoesContainElementWithName("slugcatselect"))
		{
			Futile.atlasManager.UnloadAtlas("slugcatselect");
		}
		if (Futile.atlasManager.DoesContainElementWithName("expeditionpage"))
		{
			Futile.atlasManager.UnloadAtlas("expeditionpage");
		}
		if (Futile.atlasManager.DoesContainElementWithName("challengeselect"))
		{
			Futile.atlasManager.UnloadAtlas("challengeselect");
		}
		if (Futile.atlasManager.DoesContainElementWithName("unlockables"))
		{
			Futile.atlasManager.UnloadAtlas("unlockables");
		}
		if (Futile.atlasManager.DoesContainElementWithName("progression"))
		{
			Futile.atlasManager.UnloadAtlas("progression");
		}
		if (Futile.atlasManager.DoesContainElementWithName("mission"))
		{
			Futile.atlasManager.UnloadAtlas("mission");
		}
		if (Futile.atlasManager.DoesContainElementWithName("milestones"))
		{
			Futile.atlasManager.UnloadAtlas("milestones");
		}
		if (Futile.atlasManager.DoesContainElementWithName("filters"))
		{
			Futile.atlasManager.UnloadAtlas("filters");
		}
		if (Futile.atlasManager.DoesContainElementWithName("custom"))
		{
			Futile.atlasManager.UnloadAtlas("custom");
		}
		coreFile = null;
	}
}
