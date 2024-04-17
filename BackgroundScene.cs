using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BackgroundScene : UpdatableAndDeletable
{
	public abstract class BackgroundSceneElement : UpdatableAndDeletable, IDrawable
	{
		public Vector2 pos;

		public float depth;

		public BackgroundScene scene;

		public Color? blendColor;

		public BackgroundSceneElement(BackgroundScene scene, Vector2 pos, float depth)
		{
			this.scene = scene;
			this.pos = pos;
			this.depth = depth;
		}

		public Vector2 DrawPos(Vector2 camPos, float hDisplace)
		{
			return scene.DrawPos(pos, depth, camPos, hDisplace);
		}

		public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
		}

		public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
			else if (blendColor.HasValue)
			{
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					sLeaser.sprites[i].color = new Color(sLeaser.sprites[i].color.r * blendColor.Value.r, sLeaser.sprites[i].color.g * blendColor.Value.g, sLeaser.sprites[i].color.b * blendColor.Value.b);
				}
			}
		}

		public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Water");
			}
			FSprite[] sprites = sLeaser.sprites;
			foreach (FSprite fSprite in sprites)
			{
				fSprite.RemoveFromContainer();
				newContatiner.AddChild(fSprite);
			}
		}
	}

	public class Simple2DBackgroundIllustration : BackgroundSceneElement
	{
		private string illustrationName;

		public float alpha;

		public Simple2DBackgroundIllustration(BackgroundScene scene, string illustrationName, Vector2 pos)
			: base(scene, pos, float.MaxValue)
		{
			this.illustrationName = illustrationName;
			alpha = 1f;
			scene.LoadGraphic(illustrationName, crispPixels: true, clampWrapMode: true);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(illustrationName);
			sLeaser.sprites[0].x = pos.x;
			sLeaser.sprites[0].y = pos.y;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].alpha = alpha;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class FullScreenSingleColor : BackgroundSceneElement
	{
		public Color color;

		public float alpha;

		private bool singlePixelTexture;

		public FullScreenSingleColor(BackgroundScene scene, Color color, float alpha, bool singlePixelTexture, float renderDepth)
			: base(scene, new Vector2(0f, 0f), renderDepth)
		{
			this.color = color;
			this.alpha = alpha;
			this.singlePixelTexture = singlePixelTexture;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(singlePixelTexture ? "pixel" : "Futile_White");
			sLeaser.sprites[0].scaleX = (rCam.game.rainWorld.screenSize.x + 20f) / (singlePixelTexture ? 1f : 16f);
			sLeaser.sprites[0].scaleY = (rCam.game.rainWorld.screenSize.y + 20f) / (singlePixelTexture ? 1f : 16f);
			sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
			sLeaser.sprites[0].y = rCam.game.rainWorld.screenSize.y / 2f;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].alpha = alpha;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].alpha = alpha;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class SaintsJourneyIllustration : BackgroundSceneElement
	{
		private string illustrationName;

		public float alpha;

		private bool imageDirty;

		public float fadeCounter;

		public SaintsJourneyIllustration(int karmaLevel, BackgroundScene scene, Vector2 pos)
			: base(scene, pos, float.MaxValue)
		{
			illustrationName = "";
			UpdateImageJourney(karmaLevel, scene);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(illustrationName);
			sLeaser.sprites[0].x = pos.x;
			sLeaser.sprites[0].y = pos.y;
			sLeaser.sprites[0].anchorX = 0.5f;
			sLeaser.sprites[0].anchorY = 0.5f;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Basic"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (imageDirty)
			{
				sLeaser.RemoveAllSpritesFromContainer();
				InitiateSprites(sLeaser, rCam);
			}
			imageDirty = false;
			sLeaser.sprites[0].alpha = Mathf.Clamp(fadeCounter, 0f, 1f);
			sLeaser.sprites[0].scaleX = Mathf.Lerp(1f + Random.value * 0.5f, 1.1f, fadeCounter);
			sLeaser.sprites[0].scaleY = Mathf.Lerp(1f + Random.value * 0.5f, 1.1f, fadeCounter);
			sLeaser.sprites[0].rotation = Random.Range(-2f, 2f);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public void UpdateImageJourney(int karmaLevel, BackgroundScene scene)
		{
			if (illustrationName != "")
			{
				imageDirty = true;
			}
			string text = Application.persistentDataPath + Path.DirectorySeparatorChar + "SJ_" + scene.room.game.rainWorld.options.saveSlot;
			string text2 = string.Concat(new object[2] { "karcap", karmaLevel });
			if (Directory.Exists(text) && File.Exists(text + Path.DirectorySeparatorChar + text2 + ".png"))
			{
				illustrationName = text2;
				scene.ManualLoadGraphic(text, text2, crispPixels: true, clampWrapMode: true);
				alpha = 1f;
			}
			else
			{
				illustrationName = "VOIDED";
				scene.LoadGraphic(illustrationName, crispPixels: true, clampWrapMode: true);
				alpha = 1f;
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer("HUD");
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public List<BackgroundSceneElement> elements;

	public Vector2 sceneOrigo;

	private bool elementsAddedToRoom;

	public Vector2 convergencePoint;

	public BackgroundScene(Room room)
	{
		base.room = room;
		elements = new List<BackgroundSceneElement>();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!elementsAddedToRoom)
		{
			List<BackgroundSceneElement> list = elements.OrderByDescending((BackgroundSceneElement pet) => pet.depth).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				room.AddObject(list[i]);
			}
			if (ModManager.MSC && room.waterInverted)
			{
				convergencePoint = new Vector2(room.game.rainWorld.screenSize.x / 2f, room.game.rainWorld.screenSize.y * (1f / 3f));
			}
			else
			{
				convergencePoint = new Vector2(room.game.rainWorld.screenSize.x / 2f, room.game.rainWorld.screenSize.y * 2f / 3f);
			}
			elementsAddedToRoom = true;
		}
	}

	public virtual void AddElement(BackgroundSceneElement element)
	{
		elements.Add(element);
	}

	public override void Destroy()
	{
		for (int i = 0; i < elements.Count; i++)
		{
			elements[i].Destroy();
		}
		base.Destroy();
	}

	public Vector2 RoomToWorldPos(Vector2 inRoomPos)
	{
		Vector2 vector = room.world.GetAbstractRoom(room.abstractRoom.index).mapPos;
		if (ModManager.MSC && room.world.region != null && room.world.region.name == "LC")
		{
			vector = new Vector2(500f, 5000f);
		}
		return (vector / 3f + new Vector2(10f, 10f)) * 20f + inRoomPos - new Vector2((float)room.world.GetAbstractRoom(room.abstractRoom.index).size.x * 20f, (float)room.world.GetAbstractRoom(room.abstractRoom.index).size.y * 20f) / 2f;
	}

	public Vector2 DrawPos(Vector2 pos, float depth, Vector2 camPos, float hDisplace)
	{
		Vector2 vector = pos + sceneOrigo - RoomToWorldPos(camPos);
		vector.x += hDisplace - 8f;
		return (vector - convergencePoint) / depth + convergencePoint;
	}

	public Vector2 PosFromDrawPosAtNeutralCamPos(Vector2 input, float depth)
	{
		Vector2 vector = sceneOrigo - convergencePoint;
		return (sceneOrigo + input - vector - convergencePoint) * depth + convergencePoint;
	}

	public void LoadGraphic(string elementName, bool crispPixels, bool clampWrapMode)
	{
		if (Futile.atlasManager.GetAtlasWithName(elementName) == null)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + elementName + ".png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode, crispPixels);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(elementName, texture2D, textureFromAsset: false);
		}
	}

	public void ManualLoadGraphic(string fullPath, string name, bool crispPixels, bool clampWrapMode)
	{
		if (Futile.atlasManager.GetAtlasWithName(name) == null)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + fullPath + Path.DirectorySeparatorChar + name + ".png", clampWrapMode, crispPixels);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(name, texture2D, textureFromAsset: false);
		}
	}

	public void ManualLoadGraphicFullpath(string fullFilePath, string atlasName, bool crispPixels, bool clampWrapMode)
	{
		if (Futile.atlasManager.GetAtlasWithName(atlasName) == null)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, string.Concat(new object[2] { "file:///", fullFilePath }), clampWrapMode, crispPixels);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(atlasName, texture2D, textureFromAsset: false);
		}
	}
}
