using System.IO;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MenuIllustration : RectangularMenuObject
{
	public class CrossfadeType : ExtEnum<CrossfadeType>
	{
		public static readonly CrossfadeType Standard = new CrossfadeType("Standard", register: true);

		public static readonly CrossfadeType MaintainBackground = new CrossfadeType("MaintainBackground", register: true);

		public CrossfadeType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public FSprite sprite;

	protected Texture2D texture;

	public string folderName;

	public string fileName;

	private bool crispPixels;

	public float alpha;

	public float lastAlpha;

	public float? setAlpha;

	private bool spriteAdded;

	public CrossfadeType crossfadeMethod;

	public bool anchorCenter;

	public bool visible
	{
		set
		{
			if (value && !spriteAdded)
			{
				Container.AddChild(sprite);
			}
			else if (!value && spriteAdded)
			{
				sprite.RemoveFromContainer();
			}
			spriteAdded = value;
		}
	}

	public Color color
	{
		get
		{
			return sprite.color;
		}
		set
		{
			sprite.color = value;
		}
	}

	public MenuIllustration(Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
		: base(menu, owner, pos, new Vector2(0f, 0f))
	{
		this.folderName = folderName;
		this.fileName = fileName;
		this.crispPixels = crispPixels;
		this.anchorCenter = anchorCenter;
		if (folderName != "")
		{
			LoadFile(folderName);
		}
		else
		{
			LoadFile();
		}
		sprite = new FSprite(fileName);
		if (!anchorCenter)
		{
			sprite.anchorX = 0f;
			sprite.anchorY = 0f;
		}
		sprite.alpha = 0f;
		spriteAdded = true;
		Container.AddChild(sprite);
		size = new Vector2(texture.width, texture.height);
		alpha = 1f;
		lastAlpha = 1f;
		crossfadeMethod = CrossfadeType.Standard;
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = alpha;
		if (setAlpha.HasValue)
		{
			alpha = setAlpha.Value;
			setAlpha = null;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		sprite.x = DrawX(timeStacker);
		sprite.y = DrawY(timeStacker);
		sprite.alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		sprite.RemoveFromContainer();
		texture = null;
	}

	public void UnloadFile()
	{
		Object.Destroy(texture);
	}

	public void LoadFile()
	{
		LoadFile("Illustrations");
	}

	public void LoadFile(string folder)
	{
		if (Futile.atlasManager.GetAtlasWithName(fileName) != null)
		{
			FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(fileName);
			texture = (Texture2D)atlasWithName.texture;
			return;
		}
		texture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		string text = AssetManager.ResolveFilePath(folder + Path.DirectorySeparatorChar + fileName + ".png");
		string text2 = "file:///";
		try
		{
			AssetManager.SafeWWWLoadTexture(ref texture, text2 + text, clampWrapMode: true, crispPixels);
		}
		catch (FileLoadException arg)
		{
			Custom.LogWarning($"Error loading file: {arg}");
		}
		HeavyTexturesCache.LoadAndCacheAtlasFromTexture(fileName, texture, textureFromAsset: false);
	}
}
