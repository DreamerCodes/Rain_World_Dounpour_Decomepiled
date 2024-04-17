using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class FlareBomb : Weapon
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public FlareBomb fruit;

		public Vector2[,] segments;

		public Vector2 rootPos;

		public Vector2 direction;

		public Vector2 fruitPos;

		public Color color;

		public float contracted;

		public Stalk(FlareBomb fruit, Room room)
		{
			this.fruit = fruit;
			color = fruit.color;
			fruitPos = fruit.firstChunk.pos;
			base.room = room;
			IntVector2 tilePosition = room.GetTilePosition(fruit.firstChunk.pos);
			while (tilePosition.y >= 0 && !room.GetTile(tilePosition).Solid)
			{
				tilePosition.y--;
			}
			rootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(fruit.firstChunk.pos, rootPos) / 15f), 4, 60), 3];
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 0] = Vector2.Lerp(rootPos, fruit.firstChunk.pos, (float)i / (float)segments.GetLength(0));
				segments[i, 1] = segments[i, 0];
			}
			direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(fruitPos.x + fruitPos.y))));
			for (int j = 0; j < 100; j++)
			{
				Update(eu: false);
			}
			fruit.ChangeCollisionLayer(0);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
				if (i == 0)
				{
					segments[i, 0] = rootPos;
					segments[i, 2] *= 0f;
				}
				else if (i == segments.GetLength(0) - 1 && fruit != null)
				{
					segments[i, 0] = fruit.firstChunk.pos;
					segments[i, 2] *= 0f;
				}
				else
				{
					segments[i, 0] += segments[i, 2];
					segments[i, 2] *= 0.7f;
					segments[i, 2].y += 1.8f * (1f - contracted);
					segments[i, 2] += direction * 1.4f * (1f - ((float)i + 1f) / (float)segments.GetLength(0)) * (1f - contracted);
				}
				if (i < segments.GetLength(0) - 1)
				{
					Vector2 normalized = (segments[i, 0] - segments[i + 1, 0]).normalized;
					float num = 15f * (1f - contracted);
					float num2 = Vector2.Distance(segments[i, 0], segments[i + 1, 0]);
					segments[i, 0] += normalized * (num - num2) * 0.5f;
					segments[i, 2] += normalized * (num - num2) * 0.5f;
					segments[i + 1, 0] -= normalized * (num - num2) * 0.5f;
					segments[i + 1, 2] -= normalized * (num - num2) * 0.5f;
				}
				if (i < segments.GetLength(0) - 2)
				{
					Vector2 normalized2 = (segments[i, 0] - segments[i + 2, 0]).normalized;
					segments[i, 2] += normalized2 * 1.5f * (1f - contracted);
					segments[i + 2, 2] -= normalized2 * 1.5f * (1f - contracted);
				}
				if (i == 0)
				{
					segments[i, 0] = rootPos + new Vector2(0f, -100f * contracted);
					segments[i, 2] *= 0f;
				}
				if (Custom.DistLess(segments[i, 1], segments[i, 0], 10f))
				{
					segments[i, 1] = segments[i, 0];
				}
			}
			if (fruit != null)
			{
				if (!Custom.DistLess(fruitPos, fruit.firstChunk.pos, 240f) || fruit.room != room || fruit.slatedForDeletetion || fruit.firstChunk.vel.magnitude > 15f)
				{
					fruit.AbstrConsumable.Consume();
					fruit = null;
				}
				else
				{
					fruit.firstChunk.vel.y += fruit.gravity;
					fruit.firstChunk.vel *= 0.6f;
					fruit.firstChunk.vel += (fruitPos - fruit.firstChunk.pos) / 20f;
				}
			}
			else
			{
				contracted = Mathf.Clamp(contracted + 1f / 140f, 0f, 1f);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: true, customColor: true);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			float num = 4f;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector2, vector) / 5f;
				float num3 = Mathf.Lerp(4f, 1f, (float)i / (float)segments.GetLength(0));
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (num + num3) * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (num + num3) * 0.5f + normalized * num2 - camPos);
				if (i < segments.GetLength(0) - 1)
				{
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - normalized * num2 - camPos);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - normalized * num2 - camPos);
				}
				else
				{
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - camPos);
				}
				num = num3;
				vector = vector2;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++)
			{
				float value = (float)i / (float)(sLeaser.sprites[0] as TriangleMesh).verticeColors.Length;
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, color, Mathf.InverseLerp(0.3f, 1f, value));
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			sLeaser.sprites[0].RemoveFromContainer();
			rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[0]);
		}
	}

	private LightSource light;

	public Vector2 flickerDir;

	public Vector2 lastFlickerDir;

	public float flashRad;

	public float lastFlashRad;

	public float flashAplha;

	public float lastFlashAlpha;

	public float burning;

	private bool charged;

	public Stalk stalk;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public float LightIntensity => Mathf.Pow(Mathf.Sin(burning * (float)Math.PI), 0.4f);

	public FlareBomb(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.8f;
		tailPos = base.firstChunk.pos;
		color = new Color(0.2f, 0f, 1f);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
	}

	public override void ChangeMode(Mode newMode)
	{
		base.ChangeMode(newMode);
		if (newMode == Mode.Free && stalk != null)
		{
			ChangeCollisionLayer(0);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (charged && (base.firstChunk.ContactPoint.x != 0 || base.firstChunk.ContactPoint.y != 0))
		{
			StartBurn();
		}
		if (burning > 0f)
		{
			burning += 1f / 60f;
			if (burning > 1f)
			{
				Destroy();
			}
			lastFlickerDir = flickerDir;
			flickerDir = Custom.DegToVec(UnityEngine.Random.value * 360f) * 50f * LightIntensity;
			lastFlashAlpha = flashAplha;
			flashAplha = Mathf.Pow(UnityEngine.Random.value, 0.3f) * LightIntensity;
			lastFlashRad = flashRad;
			flashRad = Mathf.Pow(UnityEngine.Random.value, 0.3f) * LightIntensity * 200f * 16f;
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].realizedCreature == null || (!Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, LightIntensity * 600f) && (!Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, LightIntensity * 1600f) || !room.VisualContact(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos))))
				{
					continue;
				}
				if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Spider && !room.abstractRoom.creatures[i].realizedCreature.dead)
				{
					room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 7f;
					room.abstractRoom.creatures[i].realizedCreature.Die();
				}
				else if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigSpider)
				{
					(room.abstractRoom.creatures[i].realizedCreature as BigSpider).poison = 1f;
					(room.abstractRoom.creatures[i].realizedCreature as BigSpider).State.health -= UnityEngine.Random.value * 0.2f;
					room.abstractRoom.creatures[i].realizedCreature.Stun(UnityEngine.Random.Range(10, 20));
					if (thrownBy != null)
					{
						room.abstractRoom.creatures[i].realizedCreature.SetKillTag(thrownBy.abstractCreature);
					}
				}
				room.abstractRoom.creatures[i].realizedCreature.Blind((int)Custom.LerpMap(Vector2.Distance(base.firstChunk.pos, room.abstractRoom.creatures[i].realizedCreature.VisionPoint), 60f, 600f, 400f, 20f));
			}
		}
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
		if (light != null)
		{
			if (room.Darkness(base.firstChunk.pos) == 0f)
			{
				light.Destroy();
			}
			else
			{
				light.setPos = base.firstChunk.pos + base.firstChunk.vel;
				light.setAlpha = ((base.mode == Mode.Thrown) ? Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value) : 0.5f) * (1f - 0.6f * LightIntensity);
				light.setRad = Mathf.Max(flashRad, ((base.mode == Mode.Thrown) ? Mathf.Lerp(60f, 290f, UnityEngine.Random.value) : 60f) * 1f + LightIntensity * 10f);
				light.color = color;
			}
			if (light.slatedForDeletetion || light.room != room)
			{
				light = null;
			}
		}
		else if (room.Darkness(base.firstChunk.pos) > 0f)
		{
			light = new LightSource(base.firstChunk.pos, environmentalLight: false, color, this);
			room.AddObject(light);
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.chunk == null)
		{
			return false;
		}
		room.PlaySound(SoundID.Flare_Bomb_Hit_Creature, base.firstChunk);
		StartBurn();
		return base.HitSomething(result, eu);
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		charged = true;
		room?.PlaySound(SoundID.Slugcat_Throw_Flare_Bomb, base.firstChunk);
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, base.firstChunk);
	}

	public override void HitWall()
	{
		StartBurn();
		SetRandomSpin();
		ChangeMode(Mode.Free);
		base.forbiddenToPlayer = 10;
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		StartBurn();
	}

	public void StartBurn()
	{
		if (!(burning > 0f))
		{
			burning = 0.01f;
			room.PlaySound(SoundID.Flare_Bomb_Burn, base.firstChunk);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("Pebble5");
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true);
		sLeaser.sprites[1] = triangleMesh;
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].scale = 2.5f;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
		}
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		if (burning == 0f)
		{
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
			sLeaser.sprites[2].x = vector.x - camPos.x;
			sLeaser.sprites[2].y = vector.y - camPos.y;
		}
		else
		{
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
			sLeaser.sprites[2].x = vector.x - camPos.x + Mathf.Lerp(lastFlickerDir.x, flickerDir.x, timeStacker);
			sLeaser.sprites[2].y = vector.y - camPos.y + Mathf.Lerp(lastFlickerDir.y, flickerDir.y, timeStacker);
			sLeaser.sprites[2].scale = Mathf.Lerp(lastFlashRad, flashRad, timeStacker) / 16f;
			sLeaser.sprites[2].alpha = Mathf.Lerp(lastFlashAlpha, flashAplha, timeStacker);
		}
		if (base.mode == Mode.Thrown)
		{
			sLeaser.sprites[1].isVisible = true;
			Vector2 vector2 = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
			Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(0, vector + vector3 * 3f - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(1, vector - vector3 * 3f - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(2, vector2 - camPos);
			(sLeaser.sprites[1] as TriangleMesh).verticeColors[2] = color;
		}
		else
		{
			sLeaser.sprites[1].isVisible = false;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
		sLeaser.sprites[2].color = color;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		newContatiner.AddChild(sLeaser.sprites[1]);
		newContatiner.AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[2]);
	}
}
