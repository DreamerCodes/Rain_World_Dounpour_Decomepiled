using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class SuperStructureProjector : UpdatableAndDeletable
{
	public abstract class SuperStructureProjectorPart : UpdatableAndDeletable, IDrawable
	{
		public SuperStructureProjector projector;

		public SuperStructureProjectorPart(SuperStructureProjector projector)
		{
			this.projector = projector;
			projector.parts.Add(this);
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
		}

		public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class ScanLine : SuperStructureProjectorPart
	{
		public bool alignment;

		public float pos;

		public float lastPos;

		public float speed;

		public ScanLine(SuperStructureProjector projector, bool alignment, float pos, float dir)
			: base(projector)
		{
			projector.scanLines.Add(this);
			speed = Mathf.Lerp(1f, 7f, UnityEngine.Random.value) * dir;
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
				pos += projector.gridPos.x - projector.lastGridPos.x;
				if ((pos < -200f && lastPos < -200f && speed < 0f) || (pos > room.PixelWidth + 200f && lastPos > room.PixelWidth + 200f && speed > 0f))
				{
					Destroy();
				}
			}
			else
			{
				pos += projector.gridPos.y - projector.lastGridPos.y;
				if ((pos < -200f && lastPos < -200f && speed < 0f) || (pos > room.PixelHeight + 200f && lastPos > room.PixelHeight + 200f && speed > 0f))
				{
					Destroy();
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i] = new FSprite("pixel");
				if (projector.debugColor)
				{
					sLeaser.sprites[i].color = new Color(1f, 0f, 0f);
				}
				else
				{
					sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
				}
				if (alignment)
				{
					sLeaser.sprites[i].scaleX = 2f;
					sLeaser.sprites[i].scaleY = 768f;
					sLeaser.sprites[i].anchorY = 0f;
				}
				else
				{
					sLeaser.sprites[i].scaleX = 1366f;
					sLeaser.sprites[i].scaleY = 2f;
					sLeaser.sprites[i].anchorX = 0f;
				}
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = projector.visible;
			}
			if (!projector.visible)
			{
				return;
			}
			if (alignment)
			{
				for (int j = 0; j < 2; j++)
				{
					sLeaser.sprites[j].x = Mathf.Lerp(lastPos, pos, timeStacker) - camPos.x + 15f * (float)j;
				}
			}
			else
			{
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[k].y = Mathf.Lerp(lastPos, pos, timeStacker) - camPos.y + 15f * (float)k;
				}
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Cursor : SuperStructureProjectorPart
	{
		public IntVector2 gPos;

		public Vector2 pos;

		public Vector2 lastPos;

		public int size;

		public Cursor(SuperStructureProjector projector, IntVector2 gPos)
			: base(projector)
		{
			this.gPos = gPos;
			pos = projector.GridPos(gPos, 1f);
			lastPos = pos;
			size = UnityEngine.Random.Range(1, 4);
			projector.cursors.Add(this);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastPos = pos;
			pos += Vector2.ClampMagnitude(projector.GridPos(gPos, 1f) - pos, 12f);
			if (UnityEngine.Random.value < 0.1f)
			{
				gPos.x += UnityEngine.Random.Range(-1, 2);
			}
			if (UnityEngine.Random.value < 0.1f)
			{
				gPos.y += UnityEngine.Random.Range(-1, 2);
			}
			if (gPos.x < 0 || gPos.x >= projector.entireRoomSize.x || gPos.y < 0 || gPos.y >= projector.entireRoomSize.y)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[5];
			for (int i = 0; i < 4; i++)
			{
				sLeaser.sprites[i] = new FSprite("pixel");
				sLeaser.sprites[i].anchorX = 0f;
				sLeaser.sprites[i].anchorY = 0f;
				sLeaser.sprites[i].color = new Color(0f, 0f, 0f);
				sLeaser.sprites[i].scaleX = 2f;
				sLeaser.sprites[i].scaleY = 2f;
			}
			sLeaser.sprites[0].scaleY = 768f;
			sLeaser.sprites[1].scaleY = 768f;
			sLeaser.sprites[2].scaleX = 1366f;
			sLeaser.sprites[3].scaleX = 1366f;
			sLeaser.sprites[4] = new FSprite("glyphs");
			sLeaser.sprites[4].scaleX = 15f * (float)size / 750f;
			sLeaser.sprites[4].scaleY = 15f * (float)size / 15f;
			sLeaser.sprites[4].shader = rCam.room.game.rainWorld.Shaders["GlyphProjection"];
			sLeaser.sprites[4].anchorX = 0f;
			sLeaser.sprites[4].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = projector.visible;
			}
			if (projector.visible)
			{
				Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
				sLeaser.sprites[4].x = vector.x - camPos.x;
				sLeaser.sprites[4].y = vector.y - camPos.y;
				sLeaser.sprites[0].x = vector.x - camPos.x;
				sLeaser.sprites[1].x = vector.x - camPos.x + 15f * (float)size;
				sLeaser.sprites[2].y = vector.y - camPos.y;
				sLeaser.sprites[3].y = vector.y - camPos.y + 15f * (float)size;
				if (base.slatedForDeletetion || room != rCam.room)
				{
					sLeaser.CleanSpritesAndRemove();
				}
			}
		}
	}

	public abstract class Glyph : SuperStructureProjectorPart
	{
		public IntVector2 pos;

		public float glyphCol;

		public int life;

		public int counter;

		public Glyph(SuperStructureProjector projector, IntVector2 pos)
			: base(projector)
		{
			glyphCol = UnityEngine.Random.value;
			this.pos = pos;
			life = Math.Max(life, UnityEngine.Random.Range(20, 200));
			projector.glyphsList.Add(this);
			projector.glyphGrid[pos.x, pos.y] = this;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			counter++;
			if (counter >= life)
			{
				if (this is SingleGlyph)
				{
					(this as SingleGlyph).DeActivate();
				}
				else
				{
					Destroy();
				}
			}
		}

		public void Move(IntVector2 movement)
		{
			if (pos.x >= 0 && pos.y >= 0 && pos.x < projector.glyphGrid.GetLength(0) && pos.y < projector.glyphGrid.GetLength(1))
			{
				projector.glyphGrid[pos.x, pos.y] = null;
			}
			pos += movement;
			if (pos.x < 0)
			{
				pos.x += projector.glyphGrid.GetLength(0);
			}
			else if (pos.x >= projector.glyphGrid.GetLength(0))
			{
				pos.x -= projector.glyphGrid.GetLength(0);
			}
			if (pos.y < 0)
			{
				pos.y += projector.glyphGrid.GetLength(1);
			}
			else if (pos.y >= projector.glyphGrid.GetLength(1))
			{
				pos.y -= projector.glyphGrid.GetLength(1);
			}
			projector.glyphGrid[pos.x, pos.y] = this;
		}

		public override void Destroy()
		{
			if (pos.x >= 0 && pos.y >= 0 && pos.x < projector.glyphGrid.GetLength(0) && pos.y < projector.glyphGrid.GetLength(1))
			{
				projector.glyphGrid[pos.x, pos.y] = null;
			}
			base.Destroy();
		}
	}

	public class SingleGlyph : Glyph
	{
		public int procreate;

		private bool selected;

		public bool inactive;

		public SingleGlyph(SuperStructureProjector projector, IntVector2 pos)
			: base(projector, pos)
		{
			Reset(pos);
		}

		public void Reset(IntVector2 pos)
		{
			life = UnityEngine.Random.Range(10, 600);
			if (UnityEngine.Random.value < 0.5f)
			{
				procreate = -1;
			}
			else
			{
				procreate = UnityEngine.Random.Range(1, life);
			}
			selected = UnityEngine.Random.value < 0.02f;
			projector.glyphGrid[pos.x, pos.y] = this;
			counter = 0;
		}

		public override void Update(bool eu)
		{
			if (!inactive)
			{
				base.Update(eu);
				if (counter == procreate || (counter == 10 && UnityEngine.Random.value > 1f / 3f && projector.glyphsList.Count < projector.idealGlyphNumber))
				{
					Procreate();
				}
			}
		}

		private void Procreate()
		{
			List<IntVector2> list = new List<IntVector2>();
			for (int i = 0; i < 4; i++)
			{
				IntVector2 item = pos + Custom.fourDirections[i];
				if (item.x >= 0 && item.y >= 0 && item.x < projector.glyphGrid.GetLength(0) && item.y < projector.glyphGrid.GetLength(1) && projector.glyphGrid[item.x, item.y] == null)
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				projector.AddSingleGlyphAt(list[UnityEngine.Random.Range(0, list.Count)]);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("glyphs");
			sLeaser.sprites[0].scaleX = 0.02f;
			sLeaser.sprites[0].scaleY = 1f;
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["GlyphProjection"];
			sLeaser.sprites[0].anchorX = 0f;
			sLeaser.sprites[0].anchorY = 0f;
			sLeaser.sprites[0].color = new Color(selected ? 1f : 0f, 0f, 0f);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = projector.visible && !inactive;
			}
			if (projector.visible && !inactive)
			{
				Vector2 vector = projector.GridPos(pos, timeStacker) + rCam.room.cameraPositions[rCam.currentCameraPosition];
				sLeaser.sprites[0].isVisible = true;
				sLeaser.sprites[0].x = vector.x - camPos.x;
				sLeaser.sprites[0].y = vector.y - camPos.y;
				projector.UpdateOffset(timeStacker, rCam);
				if (base.slatedForDeletetion || room != rCam.room)
				{
					sLeaser.CleanSpritesAndRemove();
				}
			}
		}

		public void DeActivate()
		{
			if (!inactive)
			{
				projector.inActiveGlyphsWaitingRoom.Add(this);
				projector.glyphGrid[pos.x, pos.y] = null;
				inactive = true;
			}
		}
	}

	public class GlyphMatrix : Glyph
	{
		public IntVector2 size;

		public IntVector2 maxSize;

		public GlyphMatrix(SuperStructureProjector projector, IntVector2 pos)
			: base(projector, pos)
		{
			maxSize = new IntVector2((int)Mathf.Lerp(1f, 70f, UnityEngine.Random.value * projector.effect.amount), (int)Mathf.Lerp(1f, 70f, UnityEngine.Random.value * projector.effect.amount));
			size = new IntVector2(UnityEngine.Random.Range(1, maxSize.x), UnityEngine.Random.Range(1, maxSize.y));
			life = UnityEngine.Random.Range(10, 600);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (UnityEngine.Random.value < 0.05f)
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					size.x = Custom.IntClamp(size.x + ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)), 1, maxSize.x);
				}
				else
				{
					size.y = Custom.IntClamp(size.y + ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1)), 1, maxSize.y);
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("glyphs");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["GlyphProjection"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = projector.visible;
			}
			if (projector.visible)
			{
				Vector2 vector = projector.GridPos(pos, timeStacker) + rCam.room.cameraPositions[rCam.currentCameraPosition];
				if (size.x % 2 == 1)
				{
					vector.x -= 7.5f;
				}
				if (size.y % 2 == 1)
				{
					vector.y -= 7.5f;
				}
				sLeaser.sprites[0].scaleX = (float)size.x * 15f / 750f;
				sLeaser.sprites[0].scaleY = (float)size.y * 15f / 15f;
				sLeaser.sprites[0].x = vector.x - camPos.x;
				sLeaser.sprites[0].y = vector.y - camPos.y;
				sLeaser.sprites[0].color = new Color(0f, 0f, glyphCol);
				sLeaser.sprites[0].alpha = Mathf.InverseLerp(life, 0f, counter) * 0.5f;
				if (base.slatedForDeletetion || room != rCam.room)
				{
					sLeaser.CleanSpritesAndRemove();
				}
			}
		}
	}

	public RoomSettings.RoomEffect effect;

	public List<SuperStructureProjectorPart> parts;

	public List<ScanLine> scanLines;

	public List<Glyph> glyphsList;

	public List<Cursor> cursors;

	public Glyph[,] glyphGrid;

	public IntVector2 entireRoomSize;

	public Vector2 gridPos;

	public Vector2 lastGridPos;

	public Vector2 gridSpeed;

	public IntVector2 intOffset;

	public List<SingleGlyph> inActiveGlyphsWaitingRoom = new List<SingleGlyph>();

	public int lastCamPos;

	public bool debugColor;

	private bool visible = true;

	public int idealGlyphNumber => (int)(520f * effect.amount);

	public int idealScanLineNumber => (int)(Mathf.Lerp(room.TileWidth * room.TileHeight, Mathf.Max(room.TileWidth, room.TileHeight), 0.5f) / 2000f * 10f * effect.amount);

	public int idealCursorNumber
	{
		get
		{
			int val = 0;
			if (effect.amount > 0.1f)
			{
				val = 1;
			}
			return Math.Max(val, (int)((float)(room.TileWidth * room.TileHeight) / 2000f * 1.2f * effect.amount));
		}
	}

	public SuperStructureProjector(Room room, RoomSettings.RoomEffect effect)
	{
		this.effect = effect;
		base.room = room;
		parts = new List<SuperStructureProjectorPart>();
		scanLines = new List<ScanLine>();
		glyphsList = new List<Glyph>();
		glyphGrid = new Glyph[95, 55];
		entireRoomSize = new IntVector2(Mathf.FloorToInt(room.PixelWidth / 15f) + 1, Mathf.FloorToInt(room.PixelHeight / 15f) + 1);
		cursors = new List<Cursor>();
		visible = true;
		if (ModManager.MSC && room.world.region != null && room.world.region.name == "MS" && !room.game.IsMoonHeartActive())
		{
			visible = false;
		}
		for (int i = 0; i < idealScanLineNumber; i++)
		{
			bool flag = UnityEngine.Random.value > room.PixelHeight / (room.PixelHeight + room.PixelWidth);
			room.AddObject(new ScanLine(this, flag, UnityEngine.Random.value * (flag ? room.PixelWidth : room.PixelHeight), (UnityEngine.Random.value < 0.5f) ? (-1f) : 1f));
		}
		for (int j = 0; j < idealGlyphNumber / 2; j++)
		{
			AddRandomGlyph();
		}
		intOffset = new IntVector2(UnityEngine.Random.Range(100, 1000), UnityEngine.Random.Range(100, 1000));
	}

	public override void Update(bool eu)
	{
		lastGridPos = gridPos;
		gridPos += gridSpeed;
		gridSpeed = Vector2.ClampMagnitude(gridSpeed + Custom.RNV(), 5f);
		if (UnityEngine.Random.value < 0.1f)
		{
			visible = UnityEngine.Random.value < room.ElectricPower;
		}
		if (gridPos.x < -15f)
		{
			for (int i = 0; i < glyphsList.Count; i++)
			{
				glyphsList[i].Move(new IntVector2(-2, 0));
			}
			gridPos.x += 30f;
			lastGridPos.x += 30f;
			intOffset.x -= 2;
		}
		if (gridPos.x > 15f)
		{
			for (int j = 0; j < glyphsList.Count; j++)
			{
				glyphsList[j].Move(new IntVector2(2, 0));
			}
			gridPos.x -= 30f;
			lastGridPos.x -= 30f;
			intOffset.x += 2;
		}
		if (gridPos.y < -15f)
		{
			for (int k = 0; k < glyphsList.Count; k++)
			{
				glyphsList[k].Move(new IntVector2(0, -2));
			}
			gridPos.y += 30f;
			lastGridPos.y += 30f;
			intOffset.y -= 2;
		}
		if (gridPos.y > 15f)
		{
			for (int l = 0; l < glyphsList.Count; l++)
			{
				glyphsList[l].Move(new IntVector2(0, 2));
			}
			gridPos.y -= 30f;
			lastGridPos.y -= 30f;
			intOffset.y += 2;
		}
		base.Update(eu);
		for (int num = parts.Count - 1; num >= 0; num--)
		{
			if (parts[num].slatedForDeletetion)
			{
				parts.RemoveAt(num);
			}
		}
		for (int num2 = scanLines.Count - 1; num2 >= 0; num2--)
		{
			if (scanLines[num2].slatedForDeletetion)
			{
				scanLines.RemoveAt(num2);
			}
		}
		for (int num3 = glyphsList.Count - 1; num3 >= 0; num3--)
		{
			if (glyphsList[num3].slatedForDeletetion || (glyphsList[num3] is SingleGlyph && (glyphsList[num3] as SingleGlyph).inactive))
			{
				glyphsList.RemoveAt(num3);
			}
		}
		for (int num4 = cursors.Count - 1; num4 >= 0; num4--)
		{
			if (cursors[num4].slatedForDeletetion)
			{
				cursors.RemoveAt(num4);
			}
		}
		if (scanLines.Count < idealScanLineNumber)
		{
			AddScanLine();
		}
		if (glyphsList.Count < idealGlyphNumber)
		{
			AddRandomGlyph();
		}
		if (cursors.Count < idealCursorNumber)
		{
			AddCursor();
		}
		if (!room.BeingViewed)
		{
			Destroy();
		}
		if (room.game.cameras[0].room == room)
		{
			if (room.game.cameras[0].currentCameraPosition != lastCamPos)
			{
				ScrambleAllGlyphs();
			}
			lastCamPos = room.game.cameras[0].currentCameraPosition;
		}
	}

	public void ScrambleAllGlyphs()
	{
		for (int i = 0; i < glyphsList.Count; i++)
		{
			if (glyphsList[i].pos.x > 0 && glyphsList[i].pos.y > 0 && glyphsList[i].pos.x < glyphGrid.GetLength(0) && glyphsList[i].pos.y < glyphGrid.GetLength(1))
			{
				glyphGrid[glyphsList[i].pos.x, glyphsList[i].pos.y] = null;
			}
			glyphsList[i].pos = new IntVector2(UnityEngine.Random.Range(0, glyphGrid.GetLength(0)), UnityEngine.Random.Range(0, glyphGrid.GetLength(1)));
			glyphGrid[glyphsList[i].pos.x, glyphsList[i].pos.y] = glyphsList[i];
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Destroy();
		}
		base.Destroy();
	}

	private void AddScanLine()
	{
		bool flag = UnityEngine.Random.value > room.PixelHeight / (room.PixelHeight + room.PixelWidth);
		bool flag2 = UnityEngine.Random.value < 0.5f;
		room.AddObject(new ScanLine(this, flag, flag2 ? 0f : (flag ? room.PixelWidth : room.PixelHeight), flag2 ? 1f : (-1f)));
	}

	private void AddRandomGlyph()
	{
		IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, glyphGrid.GetLength(0)), UnityEngine.Random.Range(0, glyphGrid.GetLength(1)));
		if (UnityEngine.Random.value < 0.05f)
		{
			room.AddObject(new GlyphMatrix(this, intVector));
		}
		else
		{
			AddSingleGlyphAt(intVector);
		}
	}

	public void AddSingleGlyphAt(IntVector2 ps)
	{
		if (inActiveGlyphsWaitingRoom.Count > 0)
		{
			SingleGlyph singleGlyph = inActiveGlyphsWaitingRoom[0];
			inActiveGlyphsWaitingRoom.RemoveAt(0);
			singleGlyph.inactive = false;
			singleGlyph.Reset(ps);
			glyphsList.Add(singleGlyph);
		}
		else
		{
			room.AddObject(new SingleGlyph(this, ps));
		}
	}

	private void AddCursor()
	{
		room.AddObject(new Cursor(this, new IntVector2(UnityEngine.Random.Range(0, entireRoomSize.x), UnityEngine.Random.Range(0, entireRoomSize.y))));
	}

	public void UpdateOffset(float timeStacker, RoomCamera rCam)
	{
		Shader.SetGlobalVector(RainWorld.ShadPropGridOffset, Vector2.Lerp(lastGridPos, gridPos, timeStacker) + new Vector2(0f, -1f) + 15f * intOffset.ToVector2());
	}

	public Vector2 GridPos(IntVector2 gPos, float timeStacker)
	{
		return new Vector2((float)gPos.x * 15f, (float)gPos.y * 15f) + Vector2.Lerp(lastGridPos, gridPos, timeStacker);
	}
}
