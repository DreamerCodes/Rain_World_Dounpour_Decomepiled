using System;
using System.Reflection;
using Menu;
using UnityEngine;

public class FTexture : FSprite
{
	internal static uint _garbage;

	private readonly string _salt;

	private readonly int _seed;

	private bool _even;

	private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public string salt => _salt + _seed.ToString("D5") + (_even ? "A" : "B");

	public FTexture(Texture2D texture, string salt = "")
	{
		texture.wrapMode = TextureWrapMode.Clamp;
		_salt = salt;
		_even = false;
		do
		{
			_seed = Mathf.FloorToInt(UnityEngine.Random.value * (float)(string.IsNullOrEmpty(salt) ? 100000 : 1000));
		}
		while (Futile.atlasManager.DoesContainAtlas(this.salt));
		_AddTexture2DToFManager(texture, this.salt);
		_facetTypeQuad = true;
		_localVertices = new Vector2[4];
		Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(this.salt), 1);
		_isAlphaDirty = true;
		base.UpdateLocalVertices();
	}

	public FTexture(Texture texture, string salt = "")
	{
		texture.wrapMode = TextureWrapMode.Clamp;
		_salt = salt;
		_even = false;
		do
		{
			_seed = Mathf.FloorToInt(UnityEngine.Random.value * (float)(string.IsNullOrEmpty(salt) ? 100000 : 1000));
		}
		while (Futile.atlasManager.DoesContainAtlas(this.salt));
		_AddTextureToFManager(texture, this.salt);
		_facetTypeQuad = true;
		_localVertices = new Vector2[4];
		Init(FFacetType.Quad, Futile.atlasManager.GetAtlasWithName(this.salt).elements[0], 1);
		_isAlphaDirty = true;
		base.UpdateLocalVertices();
	}

	private static void _AddTexture2DToFManager(Texture2D texture, string name)
	{
		if (Futile.atlasManager.DoesContainAtlas(name))
		{
			RemoveTextureFromFManager(name);
		}
		Futile.atlasManager.LoadAtlasFromTexture(name, texture.Clone(), textureFromAsset: false);
		_garbage += (uint)(texture.width * texture.height / 100);
		if (_garbage > 1000000)
		{
			GarbageCollect();
		}
	}

	private static void _AddTextureToFManager(Texture texture, string name)
	{
		if (Futile.atlasManager.DoesContainAtlas(name))
		{
			RemoveTextureFromFManager(name);
		}
		FAtlas fAtlas = Futile.atlasManager.LoadAtlasFromTexture(name, texture, textureFromAsset: false);
		typeof(FAtlas).GetField("_texture", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(fAtlas, texture);
		typeof(FAtlas).GetField("_textureSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(fAtlas, new Vector2(texture.width, texture.height));
		fAtlas.elements[0].sourceSize = new Vector2((float)texture.width * Futile.resourceScaleInverse, (float)texture.height * Futile.resourceScaleInverse);
		fAtlas.elements[0].sourcePixelSize = new Vector2(texture.width, texture.height);
		fAtlas.elements[0].sourceRect = new Rect(0f, 0f, (float)texture.width * Futile.resourceScaleInverse, (float)texture.height * Futile.resourceScaleInverse);
		_garbage += (uint)(texture.width * texture.height / 100);
		if (_garbage > 1000000)
		{
			GarbageCollect();
		}
	}

	public static void GarbageCollect(bool actual = true)
	{
		if (actual)
		{
			MachineConnector.LogMessage("FTexture called GarbageCollect");
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		_garbage = 0u;
	}

	private static void RemoveTextureFromFManager(string name)
	{
		Futile.atlasManager.UnloadAtlas(name);
	}

	public static string AddTexture2DToFManager(Texture2D texture, string salt)
	{
		string text;
		do
		{
			text = salt + Mathf.FloorToInt(UnityEngine.Random.value * (float)(string.IsNullOrEmpty(salt) ? 100000 : 1000)).ToString("D5");
		}
		while (Futile.atlasManager.DoesContainAtlas(text));
		_AddTexture2DToFManager(texture, salt);
		return text;
	}

	public void SetTexture(Texture2D newTexture)
	{
		_even = !_even;
		_AddTexture2DToFManager(newTexture, salt);
		SetElementByName(salt);
	}

	public void SetTexture(Texture newTexture)
	{
		_even = !_even;
		_AddTextureToFManager(newTexture, salt);
		base.element = Futile.atlasManager.GetAtlasWithName(salt).elements[0];
	}

	public void Destroy()
	{
		RemoveFromContainer();
		RemoveTextureFromFManager(salt);
		_even = !_even;
		if (Futile.atlasManager.DoesContainAtlas(salt))
		{
			RemoveTextureFromFManager(salt);
		}
	}
}
