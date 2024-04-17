using System;
using RWCustom;
using UnityEngine;

public class VultureGrubGraphics : GraphicsModule
{
	public float[,] headFlaps;

	private Color eyeColor;

	private float laserActive;

	private float lastLaserActive;

	private Color laserColor;

	private Color lastLaserColor;

	public float flash;

	public float lastFlash;

	public int blinking;

	public LightSource lightsource;

	public ChunkDynamicSoundLoop soundLoop;

	public float deadColor;

	public float lastDeadColor;

	public VultureGrub worm => base.owner as VultureGrub;

	public int MeshSprite => 0;

	public int HeadSprite => 1;

	public int EyeSprite => 4;

	public int LaserSprite => 5;

	public int FlashSprite => 6;

	public int TotalSprites => 7;

	public int HeadFlapSprite(int s)
	{
		return 2 + s;
	}

	public VultureGrubGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		headFlaps = new float[2, 3];
		laserColor = new Color(1f, 0.9f, 0f);
		lastLaserColor = laserColor;
		deadColor = (worm.State.alive ? 0f : 1f);
		lastDeadColor = deadColor;
	}

	public override void Update()
	{
		base.Update();
		lastDeadColor = deadColor;
		if (worm.dead)
		{
			deadColor = Mathf.Min(1f, deadColor + 1f / 154f);
		}
		if (soundLoop == null && laserActive > 0f)
		{
			soundLoop = new ChunkDynamicSoundLoop(worm.bodyChunks[1]);
			soundLoop.sound = SoundID.Vulture_Grub_Laser_LOOP;
		}
		else if (soundLoop != null)
		{
			soundLoop.Volume = Mathf.InverseLerp(0.3f, 1f, laserActive);
			soundLoop.Pitch = 0.2f + 0.8f * Mathf.Pow(laserActive, 0.6f);
			soundLoop.Update();
			if (laserActive == 0f)
			{
				if (soundLoop.emitter != null)
				{
					soundLoop.emitter.slatedForDeletetion = true;
				}
				soundLoop = null;
			}
		}
		if (blinking > 0 && blinking % 30 == 15)
		{
			worm.room.PlaySound((worm.callingMode == 1) ? SoundID.Vulture_Grub_Green_Blink : SoundID.Vulture_Grub_Red_Blink, worm.bodyChunks[1]);
		}
		for (int i = 0; i < headFlaps.GetLength(0); i++)
		{
			headFlaps[i, 1] = headFlaps[i, 0];
			headFlaps[i, 0] = Custom.LerpAndTick(headFlaps[i, 0], headFlaps[i, 2], 0.02f, 0.05f);
			if (worm.Consious && UnityEngine.Random.value < 0.1f)
			{
				headFlaps[i, 2] = Mathf.Clamp(headFlaps[i, 2] + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value), 0f, 1f);
			}
		}
		lastLaserActive = laserActive;
		laserActive = Custom.LerpAndTick(laserActive, worm.Singalling ? 1f : 0f, (blinking > 0) ? 0f : 0.05f, 1f / ((blinking > 0) ? 120f : 20f));
		lastLaserColor = laserColor;
		if (worm.callingMode == -1)
		{
			laserColor = Color.Lerp(laserColor, new Color(1f, 0f, 0.1f), 0.3f);
		}
		else if (worm.callingMode == 1)
		{
			laserColor = Color.Lerp(laserColor, new Color(0f, 1f, 0.1f), 0.3f);
		}
		else
		{
			laserColor = Color.Lerp(laserColor, new Color(1f, 0.9f, 0f), 0.3f);
		}
		lastFlash = flash;
		if (blinking > 0)
		{
			blinking--;
			flash = Custom.LerpAndTick(flash, (blinking % 30 < 15) ? 1f : 0f, 0.2f, 0.05f);
			if (worm.callingMode == 0)
			{
				blinking = 0;
			}
		}
		else
		{
			flash = Custom.LerpAndTick(flash, 0f, 0.02f, 0.025f);
		}
		if (blinking > 0 && lightsource == null)
		{
			lightsource = new LightSource(worm.ChunkInOrder(0).pos, environmentalLight: false, laserColor, worm);
			lightsource.affectedByPaletteDarkness = 0.5f;
			worm.room.AddObject(lightsource);
		}
		else if (lightsource != null && (blinking < 1 || lightsource.room != worm.room))
		{
			lightsource.Destroy();
			lightsource = null;
		}
		if (lightsource != null)
		{
			lightsource.setPos = worm.ChunkInOrder(0).pos;
			lightsource.setAlpha = Mathf.Pow(flash, 0.5f);
			lightsource.setRad = Mathf.Lerp(50f, 120f, Mathf.Pow(flash, 1.2f));
			lightsource.color = laserColor;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		sLeaser.sprites[LaserSprite] = new CustomFSprite("Futile_White");
		sLeaser.sprites[LaserSprite].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
		sLeaser.sprites[HeadSprite] = new FSprite("pixel");
		sLeaser.sprites[HeadSprite].scaleX = 5f;
		sLeaser.sprites[HeadSprite].scaleY = 8f;
		sLeaser.sprites[EyeSprite] = new FSprite("tinyStar");
		sLeaser.sprites[FlashSprite] = new FSprite("Futile_White");
		sLeaser.sprites[FlashSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[HeadFlapSprite(i)] = new FSprite("pixel");
			sLeaser.sprites[HeadFlapSprite(i)].scaleX = 2f;
			sLeaser.sprites[HeadFlapSprite(i)].scaleY = 6f;
			sLeaser.sprites[HeadFlapSprite(i)].anchorY = 0.25f;
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i == LaserSprite || i == FlashSprite)
			{
				rCam.ReturnFContainer(ModManager.MMF ? "Bloom" : "Foreground").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(worm.ChunkInOrder(0).lastPos, worm.ChunkInOrder(0).pos, timeStacker);
		vector += Custom.DirVec(Vector2.Lerp(worm.ChunkInOrder(1).lastPos, worm.ChunkInOrder(1).pos, timeStacker), vector) * 5f;
		Vector2 vector2 = Vector2.Lerp(worm.ChunkInOrder(1).lastPos, worm.ChunkInOrder(1).pos, timeStacker);
		Vector2 a = vector;
		Vector2 a2 = Vector2.Lerp(worm.ChunkInOrder(2).lastPos, worm.ChunkInOrder(2).pos, timeStacker);
		a = Vector2.Lerp(a, vector2, worm.swallowed * 0.9f);
		a2 = Vector2.Lerp(a2, vector2, worm.swallowed * 0.9f);
		Vector2 vector3 = HeadDir(timeStacker);
		a2 += Custom.DirVec(vector2, a2) * 5f;
		sLeaser.sprites[HeadSprite].x = a.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = a.y - camPos.y;
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(vector3);
		sLeaser.sprites[EyeSprite].x = a.x - camPos.x;
		sLeaser.sprites[EyeSprite].y = a.y - camPos.y;
		float num = Mathf.Lerp(lastLaserActive, laserActive, timeStacker);
		Color color = Color.Lerp(lastLaserColor, laserColor, timeStacker);
		float num2 = Mathf.Lerp(lastFlash, flash, timeStacker);
		if (num <= 0f)
		{
			sLeaser.sprites[LaserSprite].isVisible = false;
		}
		else
		{
			sLeaser.sprites[LaserSprite].isVisible = true;
			sLeaser.sprites[LaserSprite].alpha = num;
			Vector2 corner = Custom.RectCollision(a, a - vector3 * 100000f, rCam.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(rCam.room, a, corner);
			if (intVector.HasValue)
			{
				corner = Custom.RectCollision(corner, a, rCam.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
			}
			(sLeaser.sprites[LaserSprite] as CustomFSprite).verticeColors[0] = Custom.RGB2RGBA(color, num);
			(sLeaser.sprites[LaserSprite] as CustomFSprite).verticeColors[1] = Custom.RGB2RGBA(color, num);
			(sLeaser.sprites[LaserSprite] as CustomFSprite).verticeColors[2] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, num2));
			(sLeaser.sprites[LaserSprite] as CustomFSprite).verticeColors[3] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, num2));
			(sLeaser.sprites[LaserSprite] as CustomFSprite).MoveVertice(0, a - vector3 * 4f + Custom.PerpendicularVector(vector3) * 0.5f - camPos);
			(sLeaser.sprites[LaserSprite] as CustomFSprite).MoveVertice(1, a - vector3 * 4f - Custom.PerpendicularVector(vector3) * 0.5f - camPos);
			(sLeaser.sprites[LaserSprite] as CustomFSprite).MoveVertice(2, corner - Custom.PerpendicularVector(vector3) * 0.5f - camPos);
			(sLeaser.sprites[LaserSprite] as CustomFSprite).MoveVertice(3, corner + Custom.PerpendicularVector(vector3) * 0.5f - camPos);
		}
		sLeaser.sprites[EyeSprite].color = Color.Lerp(eyeColor, color, num * UnityEngine.Random.value);
		sLeaser.sprites[FlashSprite].x = a.x - camPos.x;
		sLeaser.sprites[FlashSprite].y = a.y - camPos.y;
		sLeaser.sprites[FlashSprite].color = color;
		sLeaser.sprites[FlashSprite].alpha = Mathf.Pow(num2, 0.5f);
		sLeaser.sprites[FlashSprite].scale = Mathf.Pow(num2, 1.2f) * 1.5f;
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector4 = a + vector3 * 2f + Custom.PerpendicularVector(vector3) * 3f * (((float)i == 0f) ? (-1f) : 1f);
			sLeaser.sprites[HeadFlapSprite(i)].x = vector4.x - camPos.x;
			sLeaser.sprites[HeadFlapSprite(i)].y = vector4.y - camPos.y;
			sLeaser.sprites[HeadFlapSprite(i)].rotation = Custom.VecToDeg((vector3 + Custom.PerpendicularVector(vector3) * (((float)i == 0f) ? (-1f) : 1f)).normalized) + Mathf.Lerp(-30f, 30f, Mathf.Lerp(headFlaps[i, 1], headFlaps[i, 0], timeStacker));
		}
		for (int j = 0; j < 6; j++)
		{
			float num3 = (float)j / 5f;
			float f = (float)(j + 1) / 5f;
			Vector2 vector5 = Bez(a, a2, vector2, num3);
			Vector2 b = Bez(a, a2, vector2, f);
			if (j > worm.BitesLeft * 2)
			{
				vector5 = a;
				b = a;
			}
			Vector2 normalized = (vector - vector5).normalized;
			Vector2 vector6 = Custom.PerpendicularVector(normalized);
			float num4 = Vector2.Distance(vector5, vector) / 5f;
			float num5 = Vector2.Distance(vector5, b) / 5f;
			float num6 = 2f + Mathf.Sin(num3 * (float)Math.PI);
			float num7 = 2f + Mathf.Sin(num3 * (float)Math.PI);
			if (num3 == 0f)
			{
				num6 *= 0.5f;
			}
			else if (num3 == 1f)
			{
				num7 *= 0.5f;
			}
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector6 * num6 - normalized * num4 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector6 * num6 - normalized * num4 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector5 - vector6 * num7 + normalized * num5 - camPos);
			(sLeaser.sprites[MeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector5 + vector6 * num7 + normalized * num5 - camPos);
			vector = vector5;
		}
		if (deadColor != lastDeadColor)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
	}

	private Vector2 Bez(Vector2 A, Vector2 B, Vector2 C, float f)
	{
		if (f < 0.5f)
		{
			return Custom.Bezier(A, (A + C) / 2f, C, C + Custom.DirVec(B, A) * Vector2.Distance(A, C) / 4f, f);
		}
		return Custom.Bezier(C, C + Custom.DirVec(A, B) * Vector2.Distance(C, B) / 2f, B, (B + C) / 2f, f);
	}

	private Vector2 HeadDir(float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(worm.ChunkInOrder(0).lastPos, worm.ChunkInOrder(0).pos, timeStacker);
		vector += Custom.DirVec(Vector2.Lerp(worm.ChunkInOrder(1).lastPos, worm.ChunkInOrder(1).pos, timeStacker), vector) * 5f;
		Vector2 vector2 = Vector2.Lerp(worm.ChunkInOrder(1).lastPos, worm.ChunkInOrder(1).pos, timeStacker);
		Vector2 vector3 = Vector2.Lerp(worm.ChunkInOrder(2).lastPos, worm.ChunkInOrder(2).pos, timeStacker);
		vector3 += Custom.DirVec(vector2, vector3) * 5f;
		Vector2 vector4 = Custom.DirVec(vector, Bez(vector, vector3, vector2, 0.1f));
		Vector2 vector5 = Vector2.Lerp(worm.lastHeadDir, worm.headDir, timeStacker);
		return Vector2.Lerp(vector4, -vector5, vector5.magnitude * Custom.LerpMap(Vector2.Dot(vector4, -vector5.normalized), -1f, 0f, 0f, 0.8f)).normalized;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		Color a = Color.Lerp(palette.fogColor, new Color(1f, 1f, 0f), Mathf.Lerp(0.4f, 0.3f, deadColor));
		a = Color.Lerp(a, palette.blackColor, Mathf.Lerp(Mathf.Pow(palette.darkness, 2f), 1f, 0.5f * deadColor));
		sLeaser.sprites[MeshSprite].color = a;
		sLeaser.sprites[HeadSprite].color = Color.Lerp(palette.fogColor, palette.blackColor, 0.7f + 0.3f * palette.darkness);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[HeadFlapSprite(i)].color = Color.Lerp(palette.fogColor, palette.blackColor, 0.7f + 0.3f * palette.darkness);
		}
		eyeColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.92f + 0.08f * palette.darkness);
	}
}
