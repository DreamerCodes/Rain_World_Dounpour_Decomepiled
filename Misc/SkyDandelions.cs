using RWCustom;
using UnityEngine;

public class SkyDandelions : UpdatableAndDeletable
{
	public class SkyDandelion : CosmeticSprite
	{
		public Vector2 driftDir;

		public float lastHealth;

		public float health;

		public float depth;

		public bool stuck;

		public bool InPlayLayer => depth == 0f;

		public SkyDandelion(Vector2 pos, bool playLayerOnly)
		{
			base.pos = pos;
			lastPos = pos;
			driftDir = Custom.DegToVec(Random.value * 360f);
			lastHealth = 1f;
			health = 1f;
			if (playLayerOnly || Random.value < 0.2f)
			{
				depth = 0f;
			}
			else if (Random.value < 0.5f)
			{
				depth = -0.5f * Random.value;
			}
			else
			{
				depth = Mathf.Pow(Random.value, 1.5f) * 5f;
			}
		}

		public override void Update(bool eu)
		{
			evenUpdate = eu;
			vel *= 0.9f;
			driftDir = (driftDir + Custom.DegToVec(Random.value * 360f) * 0.2f).normalized;
			if (!stuck)
			{
				vel += driftDir * 0.1f;
			}
			vel.y += 0.2f;
			lastPos = pos;
			pos += vel / (depth + 1f);
			lastHealth = health;
			if (InPlayLayer)
			{
				if (room.GetTile(pos).Solid)
				{
					if (!room.GetTile(lastPos).Solid)
					{
						pos = lastPos;
					}
					health -= 1f / 30f;
					if (lastHealth < 0f)
					{
						Destroy();
						return;
					}
				}
				else
				{
					health = Mathf.Min(1f, health + 1f / 70f);
				}
				if (room.PointSubmerged(pos))
				{
					pos.y = room.FloatWaterLevel(pos.x);
				}
			}
			if (pos.y > room.PixelHeight + 300f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[(!(depth < 0f)) ? 1 : 2];
			sLeaser.sprites[0] = new FSprite("SkyDandelion");
			if (depth > 0f)
			{
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
				sLeaser.sprites[0].alpha = 0f;
			}
			else if (depth < 0f)
			{
				sLeaser.sprites[1] = new FSprite("SkyDandelion");
				sLeaser.sprites[1].color = new Color(0.003921569f, 0f, 0f);
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastHealth, health, timeStacker)), 0.2f) / (depth * 0.6f + 1f);
			if (sLeaser.sprites.Length == 2)
			{
				sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x + Mathf.InverseLerp(0f, -0.5f, depth) * rCam.room.lightAngle.x * 15f;
				sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y + Mathf.InverseLerp(0f, -0.5f, depth) * (0f - rCam.room.lightAngle.y) * 15f;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[0].color = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer(InPlayLayer ? "Items" : "Foreground");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
			if (sLeaser.sprites.Length == 2)
			{
				sLeaser.sprites[1].RemoveFromContainer();
				rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[1]);
			}
		}
	}

	public class DandelionStalk : CosmeticSprite
	{
		public SkyDandelion dandelion;

		public float stalkLength;

		public int segs;

		private Vector2 stalkDir;

		private Vector2 tip;

		private Vector2 lastTip;

		private float straightStalk;

		private float lastStraightStalk;

		public DandelionStalk(SkyDandelion dandelion, Room room)
		{
			this.dandelion = dandelion;
			base.room = room;
			pos = new Vector2(dandelion.pos.x, room.MiddleOfTile(dandelion.pos).y - 11f);
			if (!room.GetTile(pos).Solid)
			{
				pos.y -= 20f;
			}
			lastPos = pos;
			stalkLength = Vector2.Distance(pos, dandelion.pos);
			segs = Custom.IntClamp((int)(stalkLength / 2f), 2, 15);
			stalkDir = Custom.DegToVec(Mathf.Lerp(-45f, 45f, room.game.SeededRandom((int)pos.x + (int)pos.y + room.abstractRoom.index)));
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (dandelion != null)
			{
				Vector2 vector = pos + new Vector2(0f, stalkLength);
				dandelion.vel += (vector - dandelion.pos) * 0.05f;
				if (!Custom.DistLess(dandelion.pos, pos, stalkLength))
				{
					Vector2 vector2 = Custom.DirVec(dandelion.pos, pos);
					float num = Vector2.Distance(dandelion.pos, pos);
					dandelion.pos += (num - stalkLength) * vector2;
					dandelion.vel += (num - stalkLength) * vector2;
				}
				if (!room.ViewedByAnyCamera(dandelion.pos, 100f))
				{
					return;
				}
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					if (dandelion == null)
					{
						break;
					}
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (dandelion == null)
						{
							break;
						}
						for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
						{
							if (dandelion == null)
							{
								break;
							}
							if (Custom.DistLess(room.physicalObjects[i][j].bodyChunks[k].pos, dandelion.pos, room.physicalObjects[i][j].bodyChunks[k].rad + 3f))
							{
								Vector2 vector3 = room.physicalObjects[i][j].bodyChunks[k].pos - room.physicalObjects[i][j].bodyChunks[k].lastPos;
								dandelion.vel = Vector2.Lerp(dandelion.vel, vector3, Random.value);
								if (Random.value < Mathf.InverseLerp(1f, 8f, vector3.magnitude))
								{
									dandelion.vel += Custom.DirVec(room.physicalObjects[i][j].bodyChunks[k].pos, dandelion.pos) * vector3.magnitude * 0.5f;
									dandelion.vel += vector3 * 0.5f;
									dandelion.stuck = false;
									tip = dandelion.pos;
									lastTip = dandelion.pos;
									dandelion = null;
								}
							}
						}
					}
				}
			}
			else
			{
				lastTip = tip;
				tip = Vector2.Lerp(tip, pos, 0.1f);
				lastStraightStalk = straightStalk;
				straightStalk = Mathf.Min(1f, straightStalk + Random.value / 15f);
				if (Custom.DistLess(tip, pos, 1f))
				{
					Destroy();
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs, pointyTip: false, customColor: true);
			base.InitiateSprites(sLeaser, rCam);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = ((dandelion == null) ? Vector2.Lerp(lastTip, tip, timeStacker) : Vector2.Lerp(dandelion.lastPos, dandelion.pos, timeStacker));
			float num = 0.5f;
			Vector2 vector2 = pos;
			float num2 = 1f - Mathf.Lerp(lastStraightStalk, straightStalk, timeStacker);
			for (int i = 0; i < segs; i++)
			{
				float f = (float)i / (float)(segs - 1);
				Vector2 vector3 = Custom.Bezier(pos, pos + stalkDir * stalkLength * 0.5f * num2, vector, vector - stalkDir * stalkLength * 0.5f * num2, f);
				Vector2 normalized = (vector2 - vector3).normalized;
				Vector2 vector4 = Custom.PerpendicularVector(normalized);
				float num3 = Vector2.Distance(vector2, vector3) / 5f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector2 - normalized * num3 - vector4 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 - normalized * num3 + vector4 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 + normalized * num3 - vector4 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + normalized * num3 + vector4 * num - camPos);
				vector2 = vector3;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color pixel = palette.texture.GetPixel(5, 2);
			for (int i = 0; i < segs * 4; i++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = Color.Lerp(pixel, new Color(1f, 1f, 1f), (float)i / (float)(segs * 4 - 1));
			}
			base.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public RoomSettings.RoomEffect effect;

	public float wait;

	private float SpawnChance => effect.amount * Mathf.Pow(1f - room.world.rainCycle.ProximityToMiddleOfCycle, 0.15f) * (1f - Mathf.Pow(room.game.globalRain.Intensity, 0.1f));

	public SkyDandelions(RoomSettings.RoomEffect effect, Room rm)
	{
		this.effect = effect;
		room = rm;
		for (int i = 0; (float)i < (float)(rm.TileWidth * rm.TileHeight * 20) * SpawnChance / 1000f; i++)
		{
			IntVector2 pos = new IntVector2(Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight));
			if (!rm.GetTile(pos).Solid)
			{
				rm.AddObject(new SkyDandelion(rm.MiddleOfTile(pos) + new Vector2(-10f + 20f * Random.value, -10f + 20f * Random.value), playLayerOnly: false));
			}
		}
		int num = rm.abstractRoom.index;
		for (int j = 0; j < rm.roomSettings.placedObjects.Count; j++)
		{
			if (!(rm.roomSettings.placedObjects[j].type == PlacedObject.Type.DandelionPatch))
			{
				continue;
			}
			PlacedObject.ResizableObjectData resizableObjectData = rm.roomSettings.placedObjects[j].data as PlacedObject.ResizableObjectData;
			IntVector2 tilePosition = rm.GetTilePosition(rm.roomSettings.placedObjects[j].pos);
			for (int k = tilePosition.x - (int)(resizableObjectData.Rad / 20f); k <= tilePosition.x + (int)(resizableObjectData.Rad / 20f); k++)
			{
				for (int l = tilePosition.y - (int)(resizableObjectData.Rad / 20f); l <= tilePosition.y + (int)(resizableObjectData.Rad / 20f); l++)
				{
					if (k <= 0 || k >= rm.TileWidth || l <= 0 || l >= rm.TileHeight || rm.GetTile(k, l).Solid)
					{
						continue;
					}
					IntVector2 intVector = new IntVector2(k, l);
					if (rm.GetTile(intVector).Solid || rm.GetTile(intVector + new IntVector2(0, 1)).Solid || !rm.GetTile(intVector + new IntVector2(0, -1)).Solid || (rm.GetTile(intVector + new IntVector2(-1, 0)).Solid && rm.GetTile(intVector + new IntVector2(1, 0)).Solid))
					{
						continue;
					}
					float num2 = Mathf.InverseLerp(resizableObjectData.Rad, resizableObjectData.Rad / 3f, Vector2.Distance(rm.roomSettings.placedObjects[j].pos, rm.MiddleOfTile(intVector)));
					num2 *= Custom.LerpMap(resizableObjectData.Rad, 40f, 200f, 0.5f, 1f);
					for (int m = 0; (float)m < 3f * num2; m++)
					{
						if (rm.game.SeededRandom(num) < Mathf.Pow(num2, 0.5f))
						{
							float num3 = 0.1f + 0.9f * Mathf.Pow(rm.game.SeededRandom(num + 1000), Mathf.Lerp(0.5f, 2.5f, num2));
							SkyDandelion skyDandelion = new SkyDandelion(rm.MiddleOfTile(intVector) + new Vector2(-10f + rm.game.SeededRandom(num - 1000) * 20f, -10f + num3 * 30f), playLayerOnly: true)
							{
								stuck = true
							};
							DandelionStalk obj = new DandelionStalk(skyDandelion, rm);
							rm.AddObject(obj);
							rm.AddObject(skyDandelion);
						}
						num++;
					}
				}
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (wait > 0f)
		{
			wait -= 1f;
			return;
		}
		wait += Mathf.Lerp(30f, 0.4f, effect.amount);
		if (Random.value < SpawnChance)
		{
			room.AddObject(new SkyDandelion(new Vector2(Mathf.Lerp(-150f, room.PixelWidth + 150f, Random.value), -100f), playLayerOnly: false));
		}
	}
}
