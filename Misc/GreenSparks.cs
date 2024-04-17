using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class GreenSparks : UpdatableAndDeletable
{
	public class GreenSpark : CosmeticSprite
	{
		private Vector2 dir;

		private Vector2 lastLastPos;

		private LightSource light;

		public Color col;

		public float life;

		public float lifeTime;

		public float depth;

		public bool InPlayLayer => depth == 0f;

		public GreenSpark(Vector2 pos)
		{
			base.pos = pos;
			lastLastPos = pos;
			lastPos = pos;
			life = 1f;
			lifeTime = Mathf.Lerp(600f, 1200f, Random.value);
			col = new Color(0f, 1f, 1f / 85f);
			if (Random.value < 0.4f)
			{
				depth = 0f;
			}
			else if (Random.value < 0.3f)
			{
				depth = -0.5f * Random.value;
			}
			else
			{
				depth = Mathf.Pow(Random.value, 1.5f) * 3f;
			}
		}

		public override void Update(bool eu)
		{
			vel *= 0.99f;
			vel += new Vector2(0.11f * (ModManager.MSC ? (-1f) : 1f), Custom.LerpMap(life, 0f, 0.5f, -0.1f, 0.05f));
			vel += dir * 0.2f;
			dir = (dir + Custom.RNV() * 0.6f * (ModManager.MSC ? (-1f) : 1f)).normalized;
			life -= 1f / lifeTime;
			lastLastPos = lastPos;
			lastPos = pos;
			pos += vel / (depth + 1f);
			if (InPlayLayer)
			{
				if (room.GetTile(pos).Solid)
				{
					life -= 0.025f;
					if (!room.GetTile(lastPos).Solid)
					{
						IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
						FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
						pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
						float num = 0.3f;
						if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
						{
							vel.x = Mathf.Abs(vel.x) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
						{
							vel.x = (0f - Mathf.Abs(vel.x)) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
						{
							vel.y = Mathf.Abs(vel.y) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
						{
							vel.y = (0f - Mathf.Abs(vel.y)) * num;
						}
					}
					else
					{
						pos.y = room.MiddleOfTile(pos).y + 10f;
					}
				}
				if (room.PointSubmerged(pos))
				{
					pos.y = room.FloatWaterLevel(pos.x);
					life -= 0.025f;
				}
			}
			if (life < 0f || (Custom.VectorRectDistance(pos, room.RoomRect) > 100f && !room.ViewedByAnyCamera(pos, 400f)))
			{
				Destroy();
			}
			if (depth <= 0f && room.Darkness(pos) > 0f)
			{
				if (light == null)
				{
					light = new LightSource(pos, environmentalLight: false, col, this);
					if (ModManager.MMF)
					{
						light.noGameplayImpact = true;
					}
					room.AddObject(light);
					light.requireUpKeep = true;
				}
				light.setPos = pos;
				light.setAlpha = 0.4f * Mathf.InverseLerp(0f, 0.2f, life) * Mathf.InverseLerp(-0.6f, 0f, depth);
				light.setRad = 80f;
				light.stayAlive = true;
			}
			else if (light != null)
			{
				light.Destroy();
				light = null;
			}
			if (!room.BeingViewed)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			if (depth < 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(depth, 0f, -0.5f, 1.5f, 2f);
			}
			else if (depth > 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(depth, 0f, 5f, 1.5f, 0.1f);
			}
			else
			{
				sLeaser.sprites[0].scaleX = 1.5f;
			}
			sLeaser.sprites[0].anchorY = 0f;
			if (depth > 0f)
			{
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
				sLeaser.sprites[0].alpha = 0f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker));
			sLeaser.sprites[0].scaleY = Mathf.Max(2f, 2f + 1.1f * Vector2.Distance(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker)));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			if (depth <= 0f)
			{
				sLeaser.sprites[0].color = col;
			}
			else
			{
				sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, col, Mathf.InverseLerp(0f, 5f, depth));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer(InPlayLayer ? "Items" : "Foreground");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
		}
	}

	public List<GreenSpark> sparks;

	private int totSparks;

	public Vector2 wind;

	public GreenSparks(Room room, float amount)
	{
		base.room = room;
		sparks = new List<GreenSpark>();
		float num = Mathf.Lerp(0f, 0.1f, amount);
		totSparks = Custom.IntClamp((int)((float)(room.TileWidth * room.TileHeight) * num), 1, 300);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int num = sparks.Count - 1; num >= 0; num--)
		{
			if (sparks[num].slatedForDeletetion)
			{
				sparks.RemoveAt(num);
			}
			else
			{
				sparks[num].vel += wind * 0.2f;
			}
		}
		if (sparks.Count < totSparks)
		{
			AddSpark();
		}
		wind += Custom.RNV() * 0.1f * (ModManager.MSC ? (-1f) : 1f);
		wind *= 0.98f;
		wind = Vector2.ClampMagnitude(wind, 1f);
	}

	private void AddSpark()
	{
		IntVector2 pos = new IntVector2(0, 0);
		pos = ((!(Random.value < (float)room.TileHeight / (float)room.TileWidth)) ? new IntVector2(Random.Range(0, room.TileWidth), 0) : new IntVector2(ModManager.MSC ? room.TileWidth : 0, Random.Range(0, room.TileHeight)));
		if (room.GetTile(pos).Solid || room.GetTile(pos).AnyWater)
		{
			return;
		}
		Vector2 vector = room.MiddleOfTile(pos);
		for (int i = 0; i < 10; i++)
		{
			if (!room.ViewedByAnyCamera(vector, 200f))
			{
				break;
			}
			vector += Custom.DirVec(room.RoomRect.Center, vector) * 100f;
		}
		GreenSpark greenSpark = new GreenSpark(vector);
		room.AddObject(greenSpark);
		sparks.Add(greenSpark);
	}
}
