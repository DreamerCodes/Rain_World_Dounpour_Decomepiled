using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Mushroom : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class StalkPart
	{
		public Mushroom owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public StalkPart(Mushroom owner)
		{
			this.owner = owner;
			pos = owner.firstChunk.pos;
			lastPos = owner.firstChunk.pos;
			vel *= 0f;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (owner.room.PointSubmerged(pos))
			{
				vel *= 0.7f;
			}
			else
			{
				vel *= 0.95f;
			}
		}

		public void Reset()
		{
			lastPos = owner.firstChunk.pos;
			pos = owner.firstChunk.pos;
			vel *= 0f;
		}
	}

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public Vector2? growPos;

	public Vector2 hoverPos;

	public float hoverDirAdd;

	public StalkPart[] stalk;

	public float hue;

	private Color stalkColor;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int StalkSprite => 0;

	public int HatSprite => 1;

	public int EffectSprite => 2;

	public int TotalSprites => 3;

	public int BitesLeft => 1;

	public int FoodPoints => 0;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public Mushroom(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 2f, 0.05f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.998f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 0;
		base.waterFriction = 0.95f;
		base.buoyancy = 0.9f;
		hue = 0.25f + 0.5f * abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed);
		stalk = new StalkPart[6];
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i] = new StalkPart(this);
		}
	}

	public void ResetParts()
	{
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].Reset();
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetParts();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastDarkness = darkness;
		darkness = room.Darkness(base.firstChunk.pos);
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
			if (growPos.HasValue)
			{
				growPos = null;
			}
		}
		else if (!growPos.HasValue && base.firstChunk.ContactPoint.y == 0 && base.firstChunk.ContactPoint.x == 0)
		{
			rotation += base.firstChunk.pos - stalk[2].pos;
			rotation.Normalize();
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
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].Update();
			if (!growPos.HasValue)
			{
				stalk[i].vel.y -= Mathf.InverseLerp(0f, stalk.Length - 1, i) * 0.4f;
			}
		}
		for (int j = 0; j < stalk.Length; j++)
		{
			ConnectStalkSegment(j);
		}
		for (int num = stalk.Length - 1; num >= 0; num--)
		{
			ConnectStalkSegment(num);
		}
		for (int k = 0; k < 4; k++)
		{
			stalk[k].vel -= rotation * Mathf.InverseLerp(4f, 0f, k);
			Vector2 vector = base.firstChunk.pos - rotation * (3 + k) * 5f;
			float val = Vector2.Dot((base.firstChunk.pos - stalk[k].pos).normalized, (base.firstChunk.pos - vector).normalized);
			stalk[k].vel = Vector2.Lerp(stalk[k].vel, base.firstChunk.pos - base.firstChunk.lastPos, Custom.LerpMap(val, 1f, -1f, 0f, 1f) * Mathf.Pow(Mathf.InverseLerp(4f, 0f, k), 0.2f));
			stalk[k].vel += (vector - stalk[k].pos) / Custom.LerpMap(val, -1f, 1f, 3f, 30f) * Mathf.InverseLerp(4f, 0f, k);
			stalk[k].pos += (vector - stalk[k].pos) / Custom.LerpMap(val, -1f, 1f, 3f, 60f) * Mathf.InverseLerp(4f, 0f, k);
		}
		for (int l = 0; l < stalk.Length; l++)
		{
			ConnectStalkSegment(l);
		}
		for (int num2 = stalk.Length - 1; num2 >= 0; num2--)
		{
			ConnectStalkSegment(num2);
		}
		if (growPos.HasValue)
		{
			stalk[stalk.Length - 1].pos = growPos.Value;
			stalk[stalk.Length - 1].vel *= 0f;
			base.firstChunk.vel.y += base.gravity;
			base.firstChunk.vel *= 0.7f;
			base.firstChunk.vel += (hoverPos - base.firstChunk.pos) / 20f;
			rotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(growPos.Value, base.firstChunk.pos) + hoverDirAdd);
		}
		for (int m = 2; m < stalk.Length; m++)
		{
			Vector2 vector2 = Custom.DirVec(stalk[m - 2].pos, stalk[m].pos);
			stalk[m].vel += vector2 * 3.3f;
			stalk[m - 2].vel -= vector2 * 3.3f;
		}
	}

	private void ConnectStalkSegment(int i)
	{
		float num = 2.5f;
		if (i == 0)
		{
			Vector2 vector = Custom.DirVec(stalk[i].pos, base.firstChunk.pos);
			float num2 = Vector2.Distance(stalk[i].pos, base.firstChunk.pos);
			stalk[i].pos -= (num - num2) * vector;
			stalk[i].vel -= (num - num2) * vector;
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(stalk[i].pos, stalk[i - 1].pos);
			float num3 = Vector2.Distance(stalk[i].pos, stalk[i - 1].pos);
			stalk[i].pos -= (num - num3) * vector2 * 0.5f;
			stalk[i].vel -= (num - num3) * vector2 * 0.5f;
			stalk[i - 1].pos += (num - num3) * vector2 * 0.5f;
			stalk[i - 1].vel += (num - num3) * vector2 * 0.5f;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if ((!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count) || (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10))
		{
			if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null))
			{
				base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			}
			else
			{
				base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			}
			int x = placeRoom.GetTilePosition(base.firstChunk.pos).x;
			int num = placeRoom.GetTilePosition(base.firstChunk.pos).y;
			while (num >= 0 && num >= placeRoom.GetTilePosition(base.firstChunk.pos).y - 4)
			{
				if (!placeRoom.GetTile(x, num).Solid && placeRoom.GetTile(x, num - 1).Solid)
				{
					growPos = new Vector2(placeRoom.MiddleOfTile(x, num).x + Mathf.Lerp(-9f, 9f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x + (int)base.firstChunk.pos.y)), placeRoom.MiddleOfTile(x, num).y - 10f);
					hoverPos = new Vector2(growPos.Value.x + Mathf.Lerp(-7f, 7f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x - (int)base.firstChunk.pos.y)), growPos.Value.y + Mathf.Lerp(18f, 36f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.y - (int)base.firstChunk.pos.x)));
					hoverDirAdd = Mathf.Lerp(-25f, 25f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x));
					base.firstChunk.HardSetPosition(hoverPos);
				}
				num--;
			}
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
		ResetParts();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (growPos.HasValue)
		{
			growPos = null;
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[StalkSprite] = TriangleMesh.MakeLongMesh(stalk.Length, pointyTip: false, customColor: true);
		sLeaser.sprites[HatSprite] = new FSprite("MushroomA");
		sLeaser.sprites[EffectSprite] = new FSprite("Futile_White");
		sLeaser.sprites[EffectSprite].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		bool flag = blink > 0 && Random.value < 0.5f;
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[HatSprite].x = vector.x - camPos.x;
		sLeaser.sprites[HatSprite].y = vector.y - camPos.y;
		sLeaser.sprites[HatSprite].rotation = Custom.VecToDeg(v);
		float num = Mathf.Lerp(lastDarkness, darkness, timeStacker);
		Color color = Color.Lerp(Custom.HSL2RGB(hue, 0.6f, 0.9f), base.color, 0.2f * (1f - num));
		sLeaser.sprites[HatSprite].color = (flag ? base.blinkColor : color);
		sLeaser.sprites[EffectSprite].x = vector.x - camPos.x;
		sLeaser.sprites[EffectSprite].y = vector.y - camPos.y;
		sLeaser.sprites[EffectSprite].scale = Mathf.Lerp(15f, 30f, num) / 16f;
		sLeaser.sprites[EffectSprite].alpha = 0.4f;
		sLeaser.sprites[EffectSprite].color = Custom.HSL2RGB(hue, 0.9f + 0.1f * num, 0.7f + 0.2f * num);
		Vector2 vector2 = vector;
		float num2 = 0.75f;
		for (int i = 0; i < stalk.Length; i++)
		{
			Vector2 vector3 = Vector2.Lerp(stalk[i].lastPos, stalk[i].pos, timeStacker);
			Vector2 normalized = (vector3 - vector2).normalized;
			Vector2 vector4 = Custom.PerpendicularVector(normalized);
			float num3 = Vector2.Distance(vector3, vector2) / 5f;
			if (i == 0)
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4, vector2 - vector4 * num2 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 + vector4 * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4, vector2 - vector4 * num2 + normalized * num3 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 + vector4 * num2 + normalized * num3 - camPos);
			}
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - vector4 * num2 - normalized * num3 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + vector4 * num2 - normalized * num3 - camPos);
			vector2 = vector3;
		}
		for (int j = 0; j < (sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length; j++)
		{
			float t = (float)j / (float)((sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors[j] = Color.Lerp(flag ? new Color(1f, 1f, 1f) : color, stalkColor, t);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.fogColor;
		stalkColor = Color.Lerp(palette.blackColor, palette.fogColor, 0.5f);
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
			if (i == EffectSprite)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		(grasp.grabber as Player).mushroomCounter += 320;
		(grasp.grabber as Player).ObjectEaten(this);
		grasp.Release();
		Destroy();
	}

	public void ThrowByPlayer()
	{
	}
}
