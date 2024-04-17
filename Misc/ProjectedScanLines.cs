using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class ProjectedScanLines : UpdatableAndDeletable
{
	public class ScanLine : UpdatableAndDeletable, IDrawable
	{
		public ProjectedScanLines projector;

		public bool alignment;

		public float pos;

		public float lastPos;

		public float speed;

		public ScanLine(ProjectedScanLines projector, bool alignment, float pos, float dir)
		{
			this.projector = projector;
			projector.scanLines.Add(this);
			speed = Mathf.Lerp(0.5f, 3.5f, Random.value) * dir;
			this.alignment = alignment;
			this.pos = pos + Mathf.Sign(speed);
			lastPos = pos - Mathf.Sign(speed);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastPos = pos;
			pos += speed;
			if (alignment)
			{
				if ((pos < -200f && lastPos < -200f && speed < 0f) || (pos > room.PixelWidth + 200f && lastPos > room.PixelWidth + 200f && speed > 0f))
				{
					Destroy();
				}
			}
			else if ((pos < -200f && lastPos < -200f && speed < 0f) || (pos > room.PixelHeight + 200f && lastPos > room.PixelHeight + 200f && speed > 0f))
			{
				Destroy();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
			if (alignment)
			{
				sLeaser.sprites[0].scaleX = 2f;
				sLeaser.sprites[0].scaleY = 768f;
				sLeaser.sprites[0].anchorY = 0f;
			}
			else
			{
				sLeaser.sprites[0].scaleX = 1366f;
				sLeaser.sprites[0].scaleY = 2f;
				sLeaser.sprites[0].anchorX = 0f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].isVisible = projector.visible;
			if (projector.visible)
			{
				if (alignment)
				{
					sLeaser.sprites[0].x = Mathf.Lerp(lastPos, pos, timeStacker) - camPos.x;
				}
				else
				{
					sLeaser.sprites[0].y = Mathf.Lerp(lastPos, pos, timeStacker) - camPos.y;
				}
				if (base.slatedForDeletetion || room != rCam.room)
				{
					sLeaser.CleanSpritesAndRemove();
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[i]);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public RoomSettings.RoomEffect effect;

	public List<ScanLine> scanLines;

	public IntVector2 entireRoomSize;

	private bool visible = true;

	public int idealScanLineNumber => (int)(Mathf.Lerp(room.TileWidth * room.TileHeight, Mathf.Max(room.TileWidth, room.TileHeight), 0.5f) / 2000f * 10f * effect.amount);

	public ProjectedScanLines(Room room, RoomSettings.RoomEffect effect)
	{
		this.effect = effect;
		base.room = room;
		scanLines = new List<ScanLine>();
		entireRoomSize = new IntVector2(Mathf.FloorToInt(room.PixelWidth / 15f) + 1, Mathf.FloorToInt(room.PixelHeight / 15f) + 1);
		for (int i = 0; i < idealScanLineNumber; i++)
		{
			bool flag = Random.value > room.PixelHeight / (room.PixelHeight + room.PixelWidth);
			room.AddObject(new ScanLine(this, flag, Random.value * (flag ? room.PixelWidth : room.PixelHeight), (Random.value < 0.5f) ? (-1f) : 1f));
		}
	}

	public override void Update(bool eu)
	{
		if (Random.value < 0.1f)
		{
			visible = Random.value < room.ElectricPower;
		}
		base.Update(eu);
		for (int num = scanLines.Count - 1; num >= 0; num--)
		{
			if (scanLines[num].slatedForDeletetion)
			{
				scanLines.RemoveAt(num);
			}
		}
		if (scanLines.Count < idealScanLineNumber)
		{
			AddScanLine();
		}
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < scanLines.Count; i++)
		{
			scanLines[i].Destroy();
		}
		base.Destroy();
	}

	private void AddScanLine()
	{
		bool flag = Random.value > room.PixelHeight / (room.PixelHeight + room.PixelWidth);
		bool flag2 = Random.value < 0.5f;
		room.AddObject(new ScanLine(this, flag, flag2 ? 0f : (flag ? room.PixelWidth : room.PixelHeight), flag2 ? 1f : (-1f)));
	}
}
