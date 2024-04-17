using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MapRenderOutput : Panel, IDevUISignals
{
	public Texture2D texture;

	public Dictionary<string, Rect> imageMeta;

	public FAtlasElement mapTex;

	public MapPage mapPage;

	public World world;

	public string altPNGPath;

	public MapRenderOutput(DevUI owner, World world, string IDstring, DevUINode parentNode, Vector2 pos, string title, MapPage mapPage)
		: base(owner, IDstring, parentNode, pos, new Vector2(100f, 100f), title)
	{
		this.world = world;
		this.mapPage = mapPage;
		CreateMapTexture();
		fSprites.Add(new FSprite(mapTex.name));
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		subNodes.Add(new Button(owner, "Output_Texture", this, new Vector2(5f, 5f), 100f, "Output Texture"));
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(fSprites.Count - 1, absPos + new Vector2(5f, 25f));
	}

	public void CreateMapTexture()
	{
		float num = 1f;
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		List<MapRenderDefaultMaterial> list = new List<MapRenderDefaultMaterial>();
		for (int i = 0; i < mapPage.subNodes.Count; i++)
		{
			if (mapPage.subNodes[i] is RoomPanel)
			{
				bool flag = true;
				foreach (string disabledMapRoom in mapPage.map.world.DisabledMapRooms)
				{
					if (disabledMapRoom == (mapPage.subNodes[i] as RoomPanel).roomRep.room.name)
					{
						flag = false;
						Custom.Log("MAP SIZER IGNORED HIDDEN ROOM:", disabledMapRoom);
						break;
					}
				}
				if (flag)
				{
					MiniMap miniMap = (mapPage.subNodes[i] as RoomPanel).miniMap;
					Vector2 vector3 = (mapPage.subNodes[i] as RoomPanel).pos;
					if (vector3.x - miniMap.size.x * 0.5f < vector.x)
					{
						vector.x = vector3.x - miniMap.size.x * 0.5f;
					}
					if (vector3.x + miniMap.size.x * 0.5f > vector2.x)
					{
						vector2.x = vector3.x + miniMap.size.x * 0.5f;
					}
					if (vector3.y - miniMap.size.y * 0.5f < vector.y)
					{
						vector.y = vector3.y - miniMap.size.y * 0.5f;
					}
					if (vector3.y + miniMap.size.y * 0.5f > vector2.y)
					{
						vector2.y = vector3.y + miniMap.size.y * 0.5f;
					}
					num = miniMap.Scale;
				}
			}
			else if (mapPage.subNodes[i] is MapRenderDefaultMaterial)
			{
				list.Add(mapPage.subNodes[i] as MapRenderDefaultMaterial);
			}
		}
		int num2 = (int)((vector2.y - vector.y) / num) + 20;
		imageMeta = new Dictionary<string, Rect>();
		texture = new Texture2D((int)((vector2.x - vector.x) / num) + 20, num2 * 3);
		for (int j = 0; j < texture.width; j++)
		{
			for (int num3 = texture.height - 1; num3 >= 0; num3--)
			{
				Color color = new Color(0f, 1f, 0f);
				texture.SetPixel(j, num3, color);
			}
		}
		for (int k = 0; k < mapPage.subNodes.Count; k++)
		{
			if (!(mapPage.subNodes[k] is RoomPanel))
			{
				continue;
			}
			bool flag2 = true;
			foreach (string disabledMapRoom2 in mapPage.map.world.DisabledMapRooms)
			{
				if (disabledMapRoom2 == (mapPage.subNodes[k] as RoomPanel).roomRep.room.name)
				{
					flag2 = false;
					Custom.Log("MAP RENDER IGNORED HIDDEN ROOM:", disabledMapRoom2);
					break;
				}
			}
			MiniMap miniMap2 = (mapPage.subNodes[k] as RoomPanel).miniMap;
			IntVector2 intVector = IntVector2.FromVector2(((mapPage.subNodes[k] as RoomPanel).pos - vector - miniMap2.size * 0.5f) / num);
			int num4 = (mapPage.subNodes[k] as RoomPanel).layer * num2 + 10;
			int num5 = 10;
			if (!(miniMap2.roomRep.texture != null && flag2))
			{
				continue;
			}
			imageMeta[miniMap2.roomRep.room.name] = new Rect(new Vector2(intVector.x + num5, intVector.y + num4), new Vector2(miniMap2.roomRep.texture.width, miniMap2.roomRep.texture.height));
			for (int l = 0; l < miniMap2.roomRep.texture.width; l++)
			{
				for (int m = 0; m < miniMap2.roomRep.texture.height; m++)
				{
					if (intVector.x + l + num5 < 0 || intVector.x + l + num5 >= texture.width || intVector.y + m + num4 < 0 || intVector.y + m + num4 >= texture.height)
					{
						continue;
					}
					int num6 = 1;
					for (int n = 0; n < list.Count; n++)
					{
						if (list[n].rect.Vector2Inside((intVector.ToVector2() + new Vector2(l + 10, m + 10)) * num + vector))
						{
							num6 = (list[n].materialIsAir ? 2 : 0);
						}
					}
					Color pixel = miniMap2.roomRep.texture.GetPixel(l, m);
					if (((int)(pixel.r * 255f) == 77 && (int)(pixel.g * 255f) == 77 && (int)(pixel.b * 255f) == 77) || ((int)(pixel.r * 255f) == 54 && (int)(pixel.g * 255f) == 54 && (int)(pixel.b * 255f) == 130))
					{
						if (num6 != 1 || texture.GetPixel(intVector.x + l + num5, intVector.y + m + num4) == new Color(0f, 1f, 0f))
						{
							texture.SetPixel(intVector.x + l + num5, intVector.y + m + num4, new Color(0f, 0f, 0f));
						}
					}
					else if (num6 != 2 || texture.GetPixel(intVector.x + l + num5, intVector.y + m + num4) == new Color(0f, 1f, 0f))
					{
						if (((int)(pixel.r * 255f) == 128 && (int)(pixel.g * 255f) == 77 && (int)(pixel.b * 255f) == 77) || ((int)(pixel.r * 255f) == 89 && (int)(pixel.g * 255f) == 54 && (int)(pixel.b * 255f) == 130))
						{
							texture.SetPixel(intVector.x + l + num5, intVector.y + m + num4, new Color(0.6f, 0f, (m <= miniMap2.roomRep.waterLevel) ? 1f : 0f));
						}
						else
						{
							texture.SetPixel(intVector.x + l + num5, intVector.y + m + num4, new Color(1f, 0f, (m <= miniMap2.roomRep.waterLevel) ? 1f : 0f));
						}
					}
				}
			}
		}
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		texture.Apply();
		HeavyTexturesCache.LoadAndCacheAtlasFromTexture("Region_Map_" + world.name, texture, textureFromAsset: false);
		mapTex = Futile.atlasManager.GetElementWithName("Region_Map_" + world.name);
		size = new Vector2(mapTex.sourcePixelSize.x, mapTex.sourcePixelSize.y) + new Vector2(10f, 40f);
		Refresh();
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		string text = "";
		if (altPNGPath != null)
		{
			text = altPNGPath;
		}
		else
		{
			text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "map_" + world.name + "-" + (owner.game.rainWorld.processManager.currentMainLoop as RainWorldGame).GetStorySession.saveState.saveStateNumber?.ToString() + ".png");
			if (!File.Exists(text))
			{
				text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + world.name + Path.DirectorySeparatorChar + "map_" + world.name + ".png");
			}
		}
		PNGSaver.SaveTextureToFile(texture, text);
		string path = Path.GetDirectoryName(text) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(text).Replace("map_", "map_image_") + ".txt";
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Rect> imageMetum in imageMeta)
		{
			list.Add(imageMetum.Key + ": " + imageMetum.Value.x + "," + imageMetum.Value.y + "," + imageMetum.Value.width + "," + imageMetum.Value.height);
		}
		File.WriteAllLines(path, list);
		ClearSprites();
		(parentNode as MapPage).renderOutput = null;
		(parentNode as MapPage).modeSpecificNodes.Remove(this);
		parentNode.subNodes.Remove(this);
	}
}
