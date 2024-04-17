using RWCustom;
using UnityEngine;

public class TileVisualizer : IDrawable
{
	public class vizType : ExtEnum<vizType>
	{
		public static readonly vizType block = new vizType("block", register: true);

		public static readonly vizType floor = new vizType("floor", register: true);

		public static readonly vizType vBeam = new vizType("vBeam", register: true);

		public static readonly vizType hBeam = new vizType("hBeam", register: true);

		public static readonly vizType sCut = new vizType("sCut", register: true);

		public static readonly vizType levelExit = new vizType("levelExit", register: true);

		public static readonly vizType sCutEntrance = new vizType("sCutEntrance", register: true);

		public vizType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public bool deleteMe;

	public vizType type;

	public IntVector2 pos { get; private set; }

	public Room room { get; private set; }

	public TileVisualizer(vizType tp, int x, int y, Room rm)
	{
		type = tp;
		pos = new IntVector2(x, y);
		room = rm;
		room.drawableObjects.Add(this);
	}

	public void delete()
	{
		deleteMe = true;
		room.drawableObjects.Remove(this);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		FSprite fSprite = new FSprite("pixel");
		sLeaser.sprites[0] = fSprite;
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
		fSprite.x = -10000f;
		if (type == vizType.block)
		{
			fSprite.scale = 20f;
		}
		else if (type == vizType.floor)
		{
			fSprite.scaleX = 20f;
			fSprite.scaleY = 10f;
			fSprite.anchorY = 0f;
			fSprite.color = new Color(0.8f, 0.8f, 0.8f);
		}
		else if (type == vizType.vBeam)
		{
			fSprite.scaleX = 4f;
			fSprite.scaleY = 20f;
			fSprite.color = new Color(0.8f, 0.5f, 0.5f);
		}
		else if (type == vizType.hBeam)
		{
			fSprite.scaleX = 20f;
			fSprite.scaleY = 4f;
			fSprite.color = new Color(0.8f, 0.5f, 0.5f);
		}
		else if (type == vizType.sCut)
		{
			fSprite.scaleX = 5f;
			fSprite.scaleY = 5f;
			fSprite.color = new Color(1f, 0f, 0f);
		}
		else if (type == vizType.levelExit)
		{
			fSprite.scaleX = 7f;
			fSprite.scaleY = 7f;
			fSprite.color = new Color(0f, 0f, 1f);
		}
		else if (type == vizType.sCutEntrance)
		{
			fSprite.scale = 20f;
			fSprite.color = new Color(0f, 0f, 1f);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = (float)pos.x * 20f - camPos.x + 10f;
		sLeaser.sprites[0].y = (float)pos.y * 20f - camPos.y + 10f;
		if (deleteMe)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
