using System.Globalization;
using System.IO;
using System.Text;
using RWCustom;
using UnityEngine;

namespace Menu;

public class LongScrollingCredits : CreditsObject
{
	private string[] names;

	public FLabel heading;

	public FLabel[,] labels;

	public FSprite[] specials;

	public override float CurrentDefaultScrollSpeed => Custom.LerpMap(Mathf.Abs(scroll + 400f), slowDownZone, slowDownZone + 100f, slowDownScrollSpeed, defaulScrollSpeed, 0.6f);

	public override bool OutOfScreen => scroll > BottomLocation();

	public float BottomLocation()
	{
		return (float)names.Length / (float)labels.GetLength(0) * (800f / (float)labels.GetLength(1));
	}

	public string TextOfListItem(int i)
	{
		if (i < 0 || i >= names.Length)
		{
			return "";
		}
		return names[i];
	}

	public LongScrollingCredits(Menu menu, MenuObject owner, EndCredits.Stage stage, int columns, string fileName, string headingText, bool startFromBottom)
		: base(menu, owner, stage, startFromBottom)
	{
		string path = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "Credits" + Path.DirectorySeparatorChar + fileName + ".txt");
		if (File.Exists(path))
		{
			names = File.ReadAllLines(path, Encoding.Default);
		}
		else
		{
			Custom.LogWarning("NO FILE");
			names = new string[0];
		}
		labels = new FLabel[columns, 20];
		for (int i = 0; i < labels.GetLength(0); i++)
		{
			for (int j = 0; j < labels.GetLength(1); j++)
			{
				labels[i, j] = new FLabel(Custom.GetFont(), names[i + j * labels.GetLength(0)]);
				Futile.stage.AddChild(labels[i, j]);
				labels[i, j].x = -1000f;
			}
		}
		heading = new FLabel(Custom.GetFont(), headingText);
		Futile.stage.AddChild(heading);
		heading.x = -1000f;
		scroll = (startFromBottom ? (BottomLocation() - 50f) : (-1000f));
		lastScroll = scroll;
		specials = new FSprite[6];
		for (int k = 0; k < specials.Length; k++)
		{
			ImportSpecialSprite(k);
			specials[k] = new FSprite("spec" + k);
			Futile.stage.AddChild(specials[k]);
			specials[k].y = 500f;
			specials[k].x = 100f + 100f * (float)k;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Mathf.Lerp(lastScroll, scroll, timeStacker) / (800f / (float)labels.GetLength(1));
		heading.x = 683f - (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f + 0.01f;
		heading.y = 384f + Mathf.Lerp(lastScroll, scroll, timeStacker) + 500f + 0.1f;
		for (int i = 0; i < specials.Length; i++)
		{
			specials[i].x = -1000f;
		}
		float num2 = ((Custom.rainWorld.options.ScreenSize.x < 1360f) ? 400f : 500f);
		for (int j = 0; j < labels.GetLength(0); j++)
		{
			for (int k = 0; k < labels.GetLength(1); k++)
			{
				labels[j, k].x = 683f - (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f + ((labels.GetLength(0) > 1) ? Custom.LerpMap(j, 0f, labels.GetLength(0) - 1, 0f - num2, num2) : 0f) + 0.01f;
				labels[j, k].y = 384f - Custom.LerpMap((float)k - num + Mathf.Floor(num), 0f, labels.GetLength(1) - 1, -400f, 400f) + 0.01f;
				string text = TextOfListItem(j + (k + Mathf.FloorToInt(num)) * labels.GetLength(0));
				if (text.Length == 7 && text.Substring(0, 6) == "<SPEC>")
				{
					specials[int.Parse(text.Substring(6, 1), NumberStyles.Any, CultureInfo.InvariantCulture)].x = labels[j, k].x;
					specials[int.Parse(text.Substring(6, 1), NumberStyles.Any, CultureInfo.InvariantCulture)].y = labels[j, k].y;
					text = "";
				}
				labels[j, k].text = text;
			}
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		heading.RemoveFromContainer();
		for (int i = 0; i < labels.GetLength(0); i++)
		{
			for (int j = 0; j < labels.GetLength(1); j++)
			{
				labels[i, j].RemoveFromContainer();
			}
		}
		for (int k = 0; k < specials.Length; k++)
		{
			specials[k].RemoveFromContainer();
			Futile.atlasManager.UnloadAtlas("spec" + k);
		}
	}

	public void ImportSpecialSprite(int num)
	{
		if (Futile.atlasManager.GetAtlasWithName("spec" + num) == null)
		{
			string text = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "Credits" + Path.DirectorySeparatorChar + "spec" + num + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
			Futile.atlasManager.LoadAtlasFromTexture("spec" + num, texture2D, textureFromAsset: false);
		}
	}
}
