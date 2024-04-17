using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SlimeMold : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class CosmeticSlimeMold : UpdatableAndDeletable, IDrawable
	{
		public Vector2[] positions;

		public float[] rads;

		public float[] brightnesses;

		public CosmeticSlimeMold(Room room, Vector2 pos, float rad, bool throughWalls)
		{
			base.room = room;
			Random.State state = Random.state;
			Random.InitState((int)(pos.x + pos.y + rad));
			List<Vector2> list = new List<Vector2>();
			List<float> list2 = new List<float>();
			List<float> list3 = new List<float>();
			IntVector2 tilePosition = room.GetTilePosition(pos);
			for (int i = tilePosition.x - (int)(rad / 20f); i <= tilePosition.x + (int)(rad / 20f); i++)
			{
				for (int j = tilePosition.y - (int)(rad / 20f); j <= tilePosition.y + (int)(rad / 20f); j++)
				{
					if (i <= 0 || i >= room.TileWidth || j <= 0 || j >= room.TileHeight || room.GetTile(i, j).Solid)
					{
						continue;
					}
					IntVector2 intVector = new IntVector2(i, j);
					if (throughWalls)
					{
						for (int k = 0; k < 4; k++)
						{
							if (!room.GetTile(intVector + Custom.fourDirections[k]).Solid)
							{
								continue;
							}
							bool solid = room.GetTile(intVector - Custom.fourDirections[k]).Solid;
							int num = (int)Custom.LerpMap(Vector2.Distance(room.MiddleOfTile(i, j), pos), rad / 3f, rad, solid ? Mathf.Lerp(6f, 8f, Random.value) : Mathf.Lerp(4f, 7f, Random.value), 0f);
							for (int l = 0; l < num; l++)
							{
								Vector2 vector = room.MiddleOfTile(intVector) + Custom.fourDirections[k].ToVector2() * 10f;
								if (Custom.fourDirections[k].x != 0)
								{
									vector.y += Mathf.Lerp(-9f, 9f, Random.value);
								}
								else
								{
									vector.x += Mathf.Lerp(-9f, 9f, Random.value);
								}
								list.Add(vector);
								list2.Add(Custom.LerpMap(Vector2.Distance(vector, pos), rad / 3f, rad, 1.5f + 3f * Random.value, 0.75f + 0.25f * Random.value));
								list3.Add(Mathf.InverseLerp(0.75f, Mathf.Lerp(2f, 4f, Mathf.Pow(Random.value, 1.5f)), list2[list2.Count - 1]));
								if (solid)
								{
									list[list.Count - 1] += Custom.DirVec(room.MiddleOfTile(intVector + Custom.fourDirections[k]), list[list.Count - 1]) * Mathf.Max(0f, list2[list2.Count - 1] - 1f);
								}
							}
						}
						continue;
					}
					Vector2 vector2 = new Vector2(0f, 0f);
					for (int m = 0; m < 8; m++)
					{
						if (!room.GetTile(intVector + Custom.eightDirections[m]).Solid && room.GetTile(intVector - Custom.eightDirections[m]).Solid)
						{
							vector2 += Custom.eightDirections[m].ToVector2().normalized;
						}
					}
					if (!(vector2.magnitude > 0f) || !room.RayTraceTilesForTerrain(tilePosition.x, tilePosition.y, intVector.x, intVector.y))
					{
						continue;
					}
					int num2 = (int)Custom.LerpMap(Vector2.Distance(room.MiddleOfTile(i, j), pos), rad / 3f, rad, Mathf.Lerp(2f, 4f, Random.value), 0f);
					for (int n = 0; n < num2; n++)
					{
						Vector2 vector3 = room.MiddleOfTile(i, j) + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
						vector2.Normalize();
						Vector2? vector4 = SharedPhysics.ExactTerrainRayTracePos(room, vector3, vector3 - vector2 * 25f);
						if (vector4.HasValue)
						{
							list.Add(vector4.Value);
							list2.Add(Custom.LerpMap(Vector2.Distance(vector4.Value, pos), rad / 3f, rad, 1.5f + 3f * Random.value, 0.75f + 0.25f * Random.value));
							list3.Add(Mathf.InverseLerp(0.75f, Mathf.Lerp(2f, 4f, Mathf.Pow(Random.value, 1.5f)), list2[list2.Count - 1]));
						}
					}
				}
			}
			Random.state = state;
			positions = list.ToArray();
			rads = list2.ToArray();
			brightnesses = list3.ToArray();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[positions.Length];
			for (int i = 0; i < positions.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("Circle20");
				sLeaser.sprites[i].scale = rads[i] / 10f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < positions.Length; i++)
			{
				sLeaser.sprites[i].x = positions[i].x - camPos.x;
				sLeaser.sprites[i].y = positions[i].y - camPos.y;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color b = SlimeMoldColorFromPalette(palette);
			Color a = Color.Lerp(palette.blackColor, palette.fogColor, 0.15f + 0.1f * palette.fogAmount);
			for (int i = 0; i < positions.Length; i++)
			{
				sLeaser.sprites[i].color = Color.Lerp(a, b, brightnesses[i]);
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public Vector2? stuckPos;

	public Vector2? gravitateToPos;

	private Vector2[,] slime;

	public int stuckPosSlime;

	private float darkMode;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public bool JellyfishMode;

	public bool big;

	public int bites = 3;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int MainSprite => 0;

	public int HighLightSprite => slime.GetLength(0) + 1;

	public int LightSprite => slime.GetLength(0) + 2;

	public int BloomSprite => slime.GetLength(0) + 3;

	public int TotalSprites => slime.GetLength(0) + 4;

	public int BitesLeft => bites;

	public int FoodPoints
	{
		get
		{
			if (!big)
			{
				return 1;
			}
			return 2;
		}
	}

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public int SlimeSprite(int s)
	{
		return 1 + s;
	}

	public SlimeMold(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.12f);
		bodyChunkConnections = new BodyChunkConnection[0];
		canBeHitByWeapons = false;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		slime = new Vector2[(int)Mathf.Lerp(8f, 15f, Random.value), 5];
		stuckPosSlime = Random.Range(0, slime.GetLength(0));
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			int num = -1;
			num = ((i != 0 && !(Random.value < 0.5f)) ? ((!(Random.value < 0.2f)) ? Random.Range(0, i) : (i - 1)) : (-1));
			slime[i, 3] = new Vector2(num, Mathf.Lerp(3f, 8f, Random.value));
			slime[i, 4] = Custom.RNV();
		}
		Random.state = state;
	}

	public void ResetSlime()
	{
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			slime[i, 0] = base.firstChunk.pos + Custom.RNV() * 4f * Random.value;
			slime[i, 1] = slime[i, 0];
			slime[i, 2] = base.firstChunk.vel;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < slime.GetLength(0); i++)
		{
			slime[i, 1] = slime[i, 0];
			slime[i, 0] += slime[i, 2];
			slime[i, 2] *= 0.98f;
			slime[i, 2].y -= 0.9f;
			if ((int)slime[i, 3].x < 0 || (int)slime[i, 3].x >= slime.GetLength(0))
			{
				Vector2 pos = base.firstChunk.pos;
				Vector2 vector = Custom.DirVec(slime[i, 0], pos);
				float num = Vector2.Distance(slime[i, 0], pos);
				float num2 = slime[i, 3].y * Custom.LerpMap(bites, 3f, 1f, 1f, 0.1f);
				slime[i, 0] -= vector * (num2 - num) * 0.9f;
				slime[i, 2] -= vector * (num2 - num) * 0.9f;
				if (!stuckPos.HasValue)
				{
					slime[i, 2] -= rotation * 2f;
				}
			}
			else
			{
				Vector2 vector2 = Custom.DirVec(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
				float num3 = Vector2.Distance(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
				float num4 = slime[i, 3].y * Custom.LerpMap(bites, 3f, 1f, 1f, 0.1f);
				slime[i, 0] -= vector2 * (num4 - num3) * 0.5f;
				slime[i, 2] -= vector2 * (num4 - num3) * 0.5f;
				slime[(int)slime[i, 3].x, 0] += vector2 * (num4 - num3) * 0.5f;
				slime[(int)slime[i, 3].x, 2] += vector2 * (num4 - num3) * 0.5f;
				Vector2 vector3 = Custom.RotateAroundOrigo(slime[i, 4], Custom.VecToDeg(rotation));
				slime[i, 2] += vector3;
				slime[(int)slime[i, 3].x, 2] -= vector3;
				if (!stuckPos.HasValue)
				{
					slime[i, 2] -= rotation * Custom.LerpMap(Vector2.Distance(slime[i, 0], base.firstChunk.pos), 4f, 12f, 2f, 0f);
				}
			}
			if (Custom.DistLess(slime[i, 0], base.firstChunk.pos, 100f))
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(slime[i, 0], slime[i, 1], slime[i, 2], 3f, new IntVector2(0, 0), base.firstChunk.goThroughFloors);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				slime[i, 0] = cd.pos;
				slime[i, 2] = cd.vel;
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			base.firstChunk.vel.x *= 0.8f;
		}
		if (!stuckPos.HasValue)
		{
			return;
		}
		Vector2 vector4 = Custom.DirVec(base.firstChunk.pos, stuckPos.Value);
		float num5 = Vector2.Distance(base.firstChunk.pos, stuckPos.Value);
		base.firstChunk.vel += vector4 * (num5 - 7f) * 0.2f;
		base.firstChunk.pos += vector4 * (num5 - 7f) * 0.2f;
		rotation = -vector4;
		if (gravitateToPos.HasValue)
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, gravitateToPos.Value) * Vector2.Distance(base.firstChunk.pos, gravitateToPos.Value) * 0.02f;
		}
		slime[stuckPosSlime, 0] = stuckPos.Value;
		slime[stuckPosSlime, 2] *= 0f;
		for (int j = 0; j < slime.GetLength(0); j++)
		{
			if (j != stuckPosSlime)
			{
				slime[j, 2] += Custom.DirVec(stuckPos.Value, slime[j, 0]) * Custom.LerpMap(Vector2.Distance(stuckPos.Value, slime[j, 0]), 3f, 24f, 6f, 0f);
			}
		}
		if (grabbedBy.Count > 0)
		{
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
			stuckPos = null;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			gravitateToPos = placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos;
			IntVector2 tilePosition = room.GetTilePosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			Random.State state = Random.state;
			Random.InitState(tilePosition.x + tilePosition.y);
			List<IntVector2> list = new List<IntVector2>();
			for (int i = 0; i < 8; i++)
			{
				list.Add(Custom.eightDirections[i]);
			}
			List<IntVector2> list2 = new List<IntVector2>();
			while (list.Count > 0)
			{
				int index = Random.Range(0, list.Count);
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
			Vector2 vector = room.MiddleOfTile(tilePosition) + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
			for (int j = 0; j < list2.Count; j++)
			{
				if (room.GetTile(tilePosition + list2[j]).Solid)
				{
					Vector2 pos = room.MiddleOfTile(tilePosition + list2[j]) + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
					FloatRect? floatRect = Custom.RectCollision(pos, vector, room.TileRect(tilePosition + list2[j]));
					if (floatRect.HasValue)
					{
						stuckPos = floatRect.Value.GetCorner(FloatRect.CornerLabel.D);
						break;
					}
				}
			}
			base.firstChunk.HardSetPosition(stuckPos.HasValue ? stuckPos.Value : vector);
			Random.state = state;
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
		ResetSlime();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetSlime();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 1.2f && firstContact)
		{
			room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, base.firstChunk, loop: false, Custom.LerpMap(speed, 1.2f, 6f, 0.2f, 1f), 1f);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (ModManager.MSC && abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
		{
			sLeaser.sprites = new FSprite[3];
			sLeaser.sprites[0] = new FSprite("JetFishEyeA");
			sLeaser.sprites[1] = new FSprite("JetFishEyeA");
			sLeaser.sprites[2] = new FSprite("tinyStar");
		}
		else
		{
			sLeaser.sprites = new FSprite[TotalSprites];
			if (big)
			{
				sLeaser.sprites[MainSprite] = new FSprite("Cicada8body");
			}
			else
			{
				sLeaser.sprites[MainSprite] = new FSprite("DangleFruit0A");
				sLeaser.sprites[MainSprite].scaleY = 0.85f;
			}
			for (int i = 0; i < slime.GetLength(0); i++)
			{
				sLeaser.sprites[SlimeSprite(i)] = new FSprite("Futile_White");
				sLeaser.sprites[SlimeSprite(i)].anchorY = 0.05f;
				sLeaser.sprites[SlimeSprite(i)].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
				sLeaser.sprites[SlimeSprite(i)].alpha = Random.value;
			}
			sLeaser.sprites[HighLightSprite] = new FSprite("Circle20");
			sLeaser.sprites[LightSprite] = new FSprite("Futile_White");
			sLeaser.sprites[LightSprite].shader = rCam.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[BloomSprite] = new FSprite("Futile_White");
			sLeaser.sprites[BloomSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		if (ModManager.MSC && abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
		{
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[1].x = vector.x - camPos.x;
			sLeaser.sprites[1].y = vector.y - camPos.y;
			sLeaser.sprites[2].x = vector.x - camPos.x;
			sLeaser.sprites[2].y = vector.y - camPos.y;
		}
		else
		{
			Color a = color;
			if (blink > 0 && Random.value < 0.5f)
			{
				a = base.blinkColor;
			}
			float num = 1f;
			if (JellyfishMode)
			{
				num = 2.4f;
			}
			if (big)
			{
				num = 1.3f;
			}
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			sLeaser.sprites[MainSprite].x = vector.x - camPos.x;
			sLeaser.sprites[MainSprite].y = vector.y - camPos.y;
			sLeaser.sprites[MainSprite].rotation = Custom.VecToDeg(v) + 180f;
			if (big)
			{
				switch (Custom.IntClamp(3 - bites, 0, 2))
				{
				case 0:
					sLeaser.sprites[MainSprite].element = Futile.atlasManager.GetElementWithName("Cicada8body");
					break;
				case 1:
					sLeaser.sprites[MainSprite].element = Futile.atlasManager.GetElementWithName("KrakenShield0");
					sLeaser.sprites[MainSprite].scale = 0.8f;
					break;
				default:
					sLeaser.sprites[MainSprite].element = Futile.atlasManager.GetElementWithName("Cicada0head");
					sLeaser.sprites[MainSprite].scale = 1f;
					sLeaser.sprites[MainSprite].rotation = Custom.VecToDeg(v);
					break;
				}
			}
			else
			{
				sLeaser.sprites[MainSprite].element = Futile.atlasManager.GetElementWithName("DangleFruit" + Custom.IntClamp(3 - bites, 0, 2) + "A");
			}
			sLeaser.sprites[MainSprite].color = a;
			sLeaser.sprites[HighLightSprite].x = vector.x - camPos.x - 2f * (1f - darkMode) + v.x * (1f + darkMode);
			sLeaser.sprites[HighLightSprite].y = vector.y - camPos.y + 2f * (1f - darkMode) + v.y * (1f + darkMode);
			sLeaser.sprites[HighLightSprite].rotation = Custom.VecToDeg(v) + 180f;
			sLeaser.sprites[HighLightSprite].color = Color.Lerp(a, new Color(1f, 1f, 1f), Mathf.Lerp(0.5f, 0.2f, darkMode));
			sLeaser.sprites[HighLightSprite].scaleY = num * Custom.LerpMap(bites, 3f, 1f, 0.3f, 0.05f);
			sLeaser.sprites[HighLightSprite].scaleX = num * Custom.LerpMap(bites, 3f, 1f, 0.25f, 0.15f);
			sLeaser.sprites[LightSprite].isVisible = darkMode > 0f;
			sLeaser.sprites[BloomSprite].isVisible = darkMode > 0f;
			if (darkMode > 0f)
			{
				sLeaser.sprites[LightSprite].x = vector.x - camPos.x;
				sLeaser.sprites[LightSprite].y = vector.y - camPos.y;
				sLeaser.sprites[LightSprite].scale = num * Custom.LerpMap(bites, 3f, 1f, 140f, 40f) / 16f;
				sLeaser.sprites[LightSprite].color = Custom.Saturate(color, 1f);
				sLeaser.sprites[LightSprite].alpha = 0.2f * darkMode;
				sLeaser.sprites[BloomSprite].x = vector.x - camPos.x;
				sLeaser.sprites[BloomSprite].y = vector.y - camPos.y;
				sLeaser.sprites[BloomSprite].scale = num * Custom.LerpMap(bites, 3f, 1f, 30f, 10f) / 16f;
				sLeaser.sprites[BloomSprite].color = Custom.Saturate(color, 1f);
				sLeaser.sprites[BloomSprite].alpha = 0.5f * darkMode;
			}
			if (JellyfishMode)
			{
				sLeaser.sprites[LightSprite].alpha = 0f;
				sLeaser.sprites[BloomSprite].alpha = 0f;
			}
			if (big)
			{
				num = 1.75f;
			}
			for (int i = 0; i < slime.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(slime[i, 1], slime[i, 0], timeStacker);
				Vector2 vector3 = StuckPosOfSlime(i, timeStacker);
				sLeaser.sprites[SlimeSprite(i)].x = vector2.x - camPos.x;
				sLeaser.sprites[SlimeSprite(i)].y = vector2.y - camPos.y;
				sLeaser.sprites[SlimeSprite(i)].scaleY = num * (Vector2.Distance(vector2, vector3) + 3f) / 16f;
				sLeaser.sprites[SlimeSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
				sLeaser.sprites[SlimeSprite(i)].scaleX = num * Custom.LerpMap(Vector2.Distance(vector2, vector3), 0f, slime[i, 3].y * 3.5f, 4f, 1.5f, 2f) / 16f;
				sLeaser.sprites[SlimeSprite(i)].color = a;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkMode = Mathf.InverseLerp(0.3f, 0.7f, palette.darkness);
		if (JellyfishMode)
		{
			base.color = Color.Lerp(Color.white, palette.fogColor, 0.4f);
		}
		else
		{
			base.color = SlimeMoldColorFromPalette(palette);
		}
		if (ModManager.MSC && abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
		{
			Color color = Color.Lerp(new Color(0.9f, 0.83f, 0.5f), palette.blackColor, 0.18f + 0.7f * rCam.PaletteDarkness());
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color + new Color(0.3f, 0.3f, 0.3f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
			sLeaser.sprites[2].color = Color.Lerp(new Color(1f, 0f, 0f), palette.blackColor, 0.3f);
		}
	}

	public static Color SlimeMoldColorFromPalette(RoomPalette palette)
	{
		return Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.07f, 0.05f, palette.darkness), 1f, 0.55f), palette.fogColor, Mathf.Lerp(0.25f, 0.35f, palette.fogAmount) * Mathf.Lerp(0.1f, 1f, palette.darkness));
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == LightSprite || i == BloomSprite)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	private Vector2 StuckPosOfSlime(int s, float timeStacker)
	{
		if ((int)slime[s, 3].x < 0 || (int)slime[s, 3].x >= slime.GetLength(0))
		{
			return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		}
		return Vector2.Lerp(slime[(int)slime[s, 3].x, 1], slime[(int)slime[s, 3].x, 0], timeStacker);
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
