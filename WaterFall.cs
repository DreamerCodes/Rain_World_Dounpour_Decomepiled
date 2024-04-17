using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class WaterFall : UpdatableAndDeletable, IDrawable
{
	public IntVector2 tilePos;

	public Vector2 pos;

	public int width;

	public float lastFlow;

	public float flow;

	public float setFlow;

	public float originalFlow;

	public float visualDensity;

	public float lastVisualDens;

	public float[] topPos;

	public float[] bottomPos;

	public Water water;

	public Vector2[,] bubbles;

	public int bubblesPerTile;

	public IntVector2[] hitTerrainTiles;

	public StaticSoundLoop smallWaterfallSound;

	public StaticSoundLoop fullWaterfallSound;

	public StaticSoundLoop smallWaterfallHitWaterSound;

	public StaticSoundLoop fullWaterfallHitWaterSound;

	private float FloatLeft => (float)tilePos.x * 20f;

	private float FloatRight => (float)(tilePos.x + width) * 20f;

	public float startLevel => pos.y;

	public float strikeLevel
	{
		get
		{
			if (ModManager.MSC && room.waterInverted)
			{
				if (room.waterObject != null)
				{
					return room.waterObject.fWaterLevel + room.roomSettings.WaveAmplitude * 40f * 1.5f;
				}
				return room.PixelHeight + 200f;
			}
			if (room.waterObject != null)
			{
				return room.waterObject.fWaterLevel - (ModManager.MMF ? (room.roomSettings.WaveAmplitude * 40f * 1.5f) : 10f);
			}
			return -200f;
		}
	}

	public float floatLength => Mathf.Abs(startLevel - strikeLevel);

	private float fallingWaterBottom
	{
		get
		{
			if (ModManager.MSC && room.waterInverted)
			{
				return Mathf.Max(startLevel, strikeLevel);
			}
			return Mathf.Min(startLevel, strikeLevel);
		}
	}

	public bool Flooded
	{
		get
		{
			if (ModManager.MSC && room.waterInverted)
			{
				return strikeLevel < startLevel;
			}
			return strikeLevel > startLevel;
		}
	}

	public WaterFall(Room room, IntVector2 tilePos, float flow, int width)
	{
		base.room = room;
		this.tilePos = tilePos;
		this.flow = flow;
		originalFlow = flow;
		lastFlow = flow;
		setFlow = flow;
		this.width = width;
		pos = room.MiddleOfTile(tilePos) + new Vector2(-10f, 15f);
		if (room.water)
		{
			bubblesPerTile = (int)Mathf.Lerp(5f, 10f, flow);
			bubbles = new Vector2[bubblesPerTile * width, 4];
		}
		else
		{
			bubbles = new Vector2[0, 0];
		}
		topPos = new float[3] { pos.y, pos.y, 0f };
		bottomPos = new float[3] { strikeLevel, strikeLevel, 0f };
		if (flow == 0f)
		{
			topPos[0] = bottomPos[0];
			topPos[1] = topPos[0];
		}
		List<IntVector2> list = new List<IntVector2>();
		for (int i = tilePos.x; i <= tilePos.x + width; i++)
		{
			if (ModManager.MSC && room.waterInverted)
			{
				for (int j = tilePos.y; (float)j < strikeLevel / 20f; j++)
				{
					if (room.GetTile(i, j).Solid && !room.GetTile(i, j + 1).Solid)
					{
						list.Add(new IntVector2(i, j));
					}
				}
				continue;
			}
			int num = tilePos.y;
			while ((float)num > strikeLevel / 20f)
			{
				if (room.GetTile(i, num).Solid && !room.GetTile(i, num + 1).Solid)
				{
					list.Add(new IntVector2(i, num));
				}
				num--;
			}
		}
		hitTerrainTiles = list.ToArray();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		bool flag = ModManager.MSC && room.waterInverted;
		lastFlow = flow;
		lastVisualDens = visualDensity;
		if (topPos[0] == pos.y)
		{
			visualDensity = Mathf.Lerp(visualDensity, flow, 0.1f);
		}
		if (topPos[0] == pos.y || (topPos[0] <= fallingWaterBottom && !flag) || (topPos[0] >= fallingWaterBottom && flag))
		{
			flow = setFlow;
		}
		bottomPos[1] = bottomPos[0];
		bottomPos[0] += bottomPos[2];
		bottomPos[2] += (flag ? 0.9f : (-0.9f));
		if ((bottomPos[0] < fallingWaterBottom && !flag) || (bottomPos[0] > fallingWaterBottom && flag))
		{
			bottomPos[0] = fallingWaterBottom;
			bottomPos[2] = 0f;
		}
		if (flow == 0f)
		{
			topPos[1] = topPos[0];
			topPos[0] += topPos[2];
			topPos[2] += (flag ? 0.9f : (-0.9f));
			if ((topPos[0] < fallingWaterBottom && !flag) || (topPos[0] > fallingWaterBottom && flag))
			{
				topPos[0] = fallingWaterBottom;
				topPos[2] = 0f;
				visualDensity = 0f;
			}
		}
		else
		{
			topPos[0] = pos.y;
			topPos[1] = pos.y;
			topPos[2] = 0f;
			if (lastFlow == 0f)
			{
				bottomPos[0] = pos.y;
				bottomPos[1] = pos.y;
				bottomPos[2] = 0f;
			}
		}
		if (!Flooded)
		{
			for (int i = 0; i < hitTerrainTiles.Length; i++)
			{
				if (Random.value < flow && Random.value < 1f / 3f)
				{
					Vector2 vector = room.MiddleOfTile(hitTerrainTiles[i]) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), Mathf.Lerp(-10f, 10f, Random.value));
					if (vector.y < topPos[0] && vector.y > bottomPos[0])
					{
						WaterDrip waterDrip = new WaterDrip(vector, (Custom.DegToVec(360f * Random.value) + new Vector2(0f, 1f)) * Random.value * 7f * Random.value * flow, waterColor: true);
						room.AddObject(waterDrip);
						waterDrip.mustExitTerrainOnceToBeDestroyedByTerrain = true;
					}
				}
			}
		}
		if (water != null)
		{
			if (!Flooded)
			{
				water.WaterfallHitSurface(pos.x, pos.x + (float)width * 10f, flow);
			}
			for (int j = tilePos.x; j < tilePos.x + width; j++)
			{
				if (!Flooded && Random.value > flow / 2f)
				{
					if (Random.value < 0.5f)
					{
						if (flag)
						{
							room.AddObject(new Bubble(room.MiddleOfTile(new IntVector2(j, 0)) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), strikeLevel - 20f), Custom.DegToVec(-20f + Random.value * 40f) * Random.value * 20f, bottomBubble: false, fakeWaterBubble: false));
						}
						else
						{
							room.AddObject(new Bubble(room.MiddleOfTile(new IntVector2(j, 0)) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), strikeLevel - 20f), Custom.DegToVec(160f + Random.value * 40f) * Random.value * 20f, bottomBubble: false, fakeWaterBubble: false));
						}
					}
					if (Random.value < 0.2f)
					{
						Vector2 vector2 = room.MiddleOfTile(new IntVector2(j, 0)) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), strikeLevel);
						vector2.y = room.FloatWaterLevel(vector2.x) + 2f;
						room.AddObject(new WaterDrip(vector2, new Vector2(vector2.x - ((float)tilePos.x * 20f + (float)width * 20f / 2f), 0f) * 0.4f / width + Custom.DegToVec(-45f + Random.value * 90f) * Random.value * 10f, waterColor: true));
					}
				}
				for (int k = (j - tilePos.x) * bubblesPerTile; k < (j - tilePos.x + 1) * bubblesPerTile; k++)
				{
					bubbles[k, 1] = bubbles[k, 0];
					bubbles[k, 0] += bubbles[k, 2];
					bubbles[k, 2] *= 0.9f;
					if (flag)
					{
						bubbles[k, 2].y -= 0.2f;
						bubbles[k, 3].x += 1f / bubbles[k, 3].y;
					}
					else
					{
						bubbles[k, 2].y += 0.2f;
						bubbles[k, 3].x -= 1f / bubbles[k, 3].y;
					}
					bool flag2 = ((!ModManager.MSC) ? (bubbles[k, 0].y > room.FloatWaterLevel(bubbles[k, 0].x)) : ((flag && !room.PointSubmerged(bubbles[k, 0], -40f)) || (!flag && !room.PointSubmerged(bubbles[k, 0]))));
					if (bubbles[k, 3].x <= 0f || flag2)
					{
						ResetBubble(k, j);
					}
				}
			}
		}
		for (int l = 0; l < room.physicalObjects.Length; l++)
		{
			foreach (PhysicalObject item in room.physicalObjects[l])
			{
				if (item.room != room || (item is Player && (item as Player).input[0].y > 0))
				{
					continue;
				}
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (bodyChunk.pos.y > room.FloatWaterLevel(bodyChunk.pos.x) - 120f && bodyChunk.pos.y < room.FloatWaterLevel(bodyChunk.pos.x) + 40f && bodyChunk.submersion > 0f)
					{
						float num = Mathf.Pow(Mathf.InverseLerp(room.FloatWaterLevel(bodyChunk.pos.x) - 120f, room.FloatWaterLevel(bodyChunk.pos.x) - 40f, bodyChunk.pos.y), 2f) * Mathf.Pow(bodyChunk.submersion, 0.1f);
						num *= flow;
						if (bodyChunk.pos.x > FloatLeft && bodyChunk.pos.x < FloatRight)
						{
							bodyChunk.vel.y -= num * 0.3f / bodyChunk.mass;
						}
						for (int n = 0; n < 2; n++)
						{
							float num2 = ((n == 0) ? (-1f) : 1f);
							float num3 = 0f;
							num3 = ((n != 0) ? ((bodyChunk.pos.x > FloatRight) ? Mathf.InverseLerp(FloatRight + 40f, FloatRight, bodyChunk.pos.x) : Mathf.InverseLerp(FloatRight - 20f, FloatRight, bodyChunk.pos.x)) : ((bodyChunk.pos.x < FloatLeft) ? Mathf.InverseLerp(FloatLeft - 40f, FloatLeft, bodyChunk.pos.x) : Mathf.InverseLerp(FloatLeft + 20f, FloatLeft, bodyChunk.pos.x)));
							num3 *= (Mathf.InverseLerp(room.FloatWaterLevel(bodyChunk.pos.x) - 60f, room.FloatWaterLevel(bodyChunk.pos.x) - 10f, bodyChunk.pos.y) - 0.5f) * 2f;
							bodyChunk.vel.x -= num * 0.17f * num2 * num3 / bodyChunk.mass;
						}
					}
				}
			}
		}
	}

	private void ResetBubble(int i, int t)
	{
		if (ModManager.MSC && room.waterInverted)
		{
			bubbles[i, 0] = room.MiddleOfTile(new IntVector2(t, 0)) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), strikeLevel + 25f);
			bubbles[i, 0].y -= 40f;
			bubbles[i, 2] = Custom.DegToVec(-20f + Random.value * 40f) * Random.value * Mathf.Lerp(8f, 12f, flow) + Custom.DegToVec(Random.value * 360f) * Random.value * 2f;
			bubbles[i, 3].y = Random.Range(-20, -5);
		}
		else
		{
			bubbles[i, 0] = room.MiddleOfTile(new IntVector2(t, 0)) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), strikeLevel - 25f);
			bubbles[i, 2] = Custom.DegToVec(160f + Random.value * 40f) * Random.value * Mathf.Lerp(8f, 12f, flow) + Custom.DegToVec(Random.value * 360f) * Random.value * 2f;
			bubbles[i, 3].y = Random.Range(5, 20);
		}
		bubbles[i, 1] = bubbles[i, 0];
		bubbles[i, 3].x = 1f;
		if (Flooded)
		{
			bubbles[i, 0] = new Vector2(-1000f, 1000f);
		}
	}

	public void ConnectToWaterObject(Water water)
	{
		this.water = water;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + bubbles.GetLength(0)];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		if (ModManager.MSC && rCam.room.waterInverted)
		{
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["WaterFallInverted"];
		}
		else
		{
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["WaterFall"];
		}
		sLeaser.sprites[0].scaleX = (float)width * 20f / 16f;
		sLeaser.sprites[0].anchorY = 1f;
		sLeaser.sprites[0].anchorX = 0f;
		for (int i = 0; i < bubbles.GetLength(0); i++)
		{
			sLeaser.sprites[1 + i] = new FSprite("LizardBubble5");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (Flooded)
		{
			sLeaser.sprites[0].isVisible = false;
		}
		else
		{
			sLeaser.sprites[0].isVisible = true;
			sLeaser.sprites[0].x = pos.x - camPos.x;
			float num;
			float num2;
			float num3;
			float f;
			float f2;
			if (ModManager.MSC && rCam.room.waterInverted)
			{
				num = Mathf.Lerp(bottomPos[1], bottomPos[0], timeStacker);
				num2 = Mathf.Lerp(topPos[1], topPos[0], timeStacker);
				num3 = Mathf.Min(Mathf.Abs(num - num2) / floatLength, 1f);
				f = Mathf.InverseLerp(strikeLevel, pos.y, num2);
				f2 = Mathf.InverseLerp(pos.y, strikeLevel, num);
				sLeaser.sprites[0].y = num2 - 20f - camPos.y;
			}
			else
			{
				num = Mathf.Lerp(topPos[1], topPos[0], timeStacker);
				num2 = Mathf.Lerp(bottomPos[1], bottomPos[0], timeStacker);
				num3 = Mathf.Min((num - num2) / floatLength, 1f);
				f = Mathf.InverseLerp(pos.y, strikeLevel, num);
				f2 = Mathf.InverseLerp(strikeLevel, pos.y, num2);
				sLeaser.sprites[0].y = num2 - camPos.y;
			}
			sLeaser.sprites[0].scaleY = (num2 - num) / 16f;
			f = Mathf.Lerp(Mathf.Pow(f, 0.2f), 1f, 1f - num3);
			f2 = Mathf.Lerp(Mathf.Pow(f2, 0.2f), 1f, 1f - num3);
			sLeaser.sprites[0].color = new Color(Mathf.Lerp(lastVisualDens, visualDensity, timeStacker), 1f / Mathf.Lerp(100f, 2f, f), 1f / Mathf.Lerp(100f, 2f, f2));
		}
		for (int i = 0; i < bubbles.GetLength(0); i++)
		{
			sLeaser.sprites[1 + i].x = Mathf.Lerp(bubbles[i, 1].x, bubbles[i, 0].x, timeStacker) - camPos.x;
			sLeaser.sprites[1 + i].y = Mathf.Lerp(bubbles[i, 1].y, bubbles[i, 0].y, timeStacker) - camPos.y;
			sLeaser.sprites[1 + i].scale = Mathf.Pow(bubbles[i, 3].x, 0.5f);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < bubbles.GetLength(0); i++)
		{
			sLeaser.sprites[1 + i].color = Color.Lerp(palette.waterColor1, palette.waterColor2, 0.3f);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Background");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
