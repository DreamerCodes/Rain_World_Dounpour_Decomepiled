using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class DandelionPeach : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public DandelionPeach nut;

		public Vector2[,] segments;

		public Vector2 rootPos;

		public Vector2 direction;

		public Vector2 nutPos;

		public Stalk(DandelionPeach nut, Room room)
		{
			this.nut = nut;
			nutPos = nut.firstChunk.pos;
			base.room = room;
			IntVector2 tilePosition = room.GetTilePosition(nut.firstChunk.pos);
			while (tilePosition.y >= 0 && !room.GetTile(tilePosition).Solid)
			{
				tilePosition.y--;
			}
			rootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(nut.firstChunk.pos, rootPos) / 15f), 4, 60), 3];
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 0] = Vector2.Lerp(rootPos, nut.firstChunk.pos, (float)i / (float)segments.GetLength(0));
				segments[i, 1] = segments[i, 0];
			}
			direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(nutPos.x + nutPos.y))));
			for (int j = 0; j < 100; j++)
			{
				Update(eu: false);
			}
			nut.ChangeCollisionLayer(0);
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
				else if (i == segments.GetLength(0) - 1 && nut != null)
				{
					segments[i, 0] = nut.firstChunk.pos;
					segments[i, 2] *= 0f;
				}
				else
				{
					segments[i, 0] += segments[i, 2];
					segments[i, 2] *= 0.7f;
					segments[i, 2].y += 0.3f;
					segments[i, 2] += direction * 0.4f * (1f - ((float)i + 1f) / (float)segments.GetLength(0));
				}
				if (i < segments.GetLength(0) - 1)
				{
					Vector2 normalized = (segments[i, 0] - segments[i + 1, 0]).normalized;
					float num = 15f;
					float num2 = Vector2.Distance(segments[i, 0], segments[i + 1, 0]);
					segments[i, 0] += normalized * (num - num2) * 0.5f;
					segments[i, 2] += normalized * (num - num2) * 0.5f;
					segments[i + 1, 0] -= normalized * (num - num2) * 0.5f;
					segments[i + 1, 2] -= normalized * (num - num2) * 0.5f;
				}
				if (i < segments.GetLength(0) - 2)
				{
					Vector2 normalized2 = (segments[i, 0] - segments[i + 2, 0]).normalized;
					segments[i, 2] += normalized2 * 1.5f;
					segments[i + 2, 2] -= normalized2 * 1.5f;
				}
				if (i == 0)
				{
					segments[i, 0] = rootPos;
					segments[i, 2] *= 0f;
				}
				if (Custom.DistLess(segments[i, 1], segments[i, 0], 10f))
				{
					segments[i, 1] = segments[i, 0];
				}
			}
			if (nut != null)
			{
				if (!Custom.DistLess(nutPos, nut.firstChunk.pos, (nut.grabbedBy.Count != 0) ? 20f : 100f) || nut.room != room || nut.slatedForDeletetion || nut.firstChunk.vel.magnitude > 15f)
				{
					nut.AbstrConsumable.Consume();
					nut.stalk = null;
					nut = null;
				}
				else
				{
					BodyChunk firstChunk = nut.firstChunk;
					firstChunk.vel.y = firstChunk.vel.y + nut.gravity;
					nut.firstChunk.vel *= 0.6f;
					nut.firstChunk.vel += (nutPos - nut.firstChunk.pos) / 20f;
					nut.setRotation = Custom.DirVec(segments[segments.GetLength(0) - 2, 0], nut.firstChunk.pos);
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num = Vector2.Distance(vector2, vector) / 5f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * 0.5f + normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * 0.5f + normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * 0.5f - normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * 0.5f - normalized * num - camPos);
				vector = vector2;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color pixel = palette.texture.GetPixel(5, 2);
			Vector2 pos = segments[segments.GetLength(0) - 1, 0];
			Color b = Color.Lerp(new Color(0.59f, 0.78f, 0.96f), palette.blackColor, rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)));
			int length = segments.GetLength(0);
			for (int i = 0; i < length * 4; i++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = Color.Lerp(pixel, b, Mathf.Pow((float)i / (float)(length * 4 - 1), 6f));
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			sLeaser.sprites[0].RemoveFromContainer();
			rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]);
		}
	}

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public int bites;

	public Stalk stalk;

	private int puffCount;

	private float swayer;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public DandelionPeach(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		bites = 3;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5.5f, 0.34f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.996f;
		base.gravity = 0.3f;
		bounce = 0.4f;
		surfaceFriction = 0.95f;
		collisionLayer = 1;
		base.waterFriction = 0.91f;
		base.buoyancy = 1.2f;
		Random.State state = Random.state;
		Random.InitState(base.abstractPhysicalObject.ID.RandomSeed);
		puffCount = Mathf.Max(5, (int)(Random.value * 8f));
		Random.state = state;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
			return;
		}
		if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
			return;
		}
		if (AbstrConsumable.isConsumed)
		{
			ChangeCollisionLayer(1);
		}
		else
		{
			ChangeCollisionLayer(0);
		}
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Input.mousePosition) * 3f;
		}
		swayer += base.firstChunk.vel.magnitude / 3f;
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			if (collisionLayer != 1)
			{
				ChangeCollisionLayer(1);
			}
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
			BodyChunk bodyChunk = base.firstChunk;
			bodyChunk.vel.x = bodyChunk.vel.x * 0.8f;
			BodyChunk bodyChunk2 = bodyChunk;
			bodyChunk2.vel.x = bodyChunk2.vel.x + Mathf.Sign(bodyChunk.vel.x) * (Mathf.InverseLerp(0.2f, 1f, rotation.y) / 10f);
		}
		else if (AbstrConsumable.isConsumed)
		{
			if (base.firstChunk.vel.y > 0f)
			{
				rotation = Vector3.Slerp(rotation, Custom.DirVec(base.firstChunk.pos + base.firstChunk.vel, base.firstChunk.pos), 0.2f);
			}
			else
			{
				rotation = Vector3.Slerp(rotation, new Vector2(0f, 1f), 0.1f);
			}
			base.airFriction = Mathf.Lerp(0.999f, 0.93f, Mathf.InverseLerp(0f, 1f, rotation.y));
		}
		if (base.Submersion > 0.5f && room.abstractRoom.creatures.Count > 0 && grabbedBy.Count == 0)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.dead && (abstractCreature.realizedCreature as JetFish).AI.goToFood == null && (abstractCreature.realizedCreature as JetFish).AI.WantToEatObject(this))
			{
				(abstractCreature.realizedCreature as JetFish).AI.goToFood = this;
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3 + puffCount];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A");
		sLeaser.sprites[0].scaleX = 0.92f;
		sLeaser.sprites[0].scaleY = 1.11f;
		sLeaser.sprites[0].alpha = 0.6f;
		sLeaser.sprites[1] = new FSprite("JellyFish0B");
		sLeaser.sprites[1].scaleX = 0.92f;
		sLeaser.sprites[1].scaleY = 1.11f;
		sLeaser.sprites[1].alpha = 1f;
		sLeaser.sprites[2] = new FSprite("tinyStar");
		sLeaser.sprites[2].scaleY = 4f;
		sLeaser.sprites[2].alpha = 0.9f;
		for (int i = 0; i < puffCount; i++)
		{
			sLeaser.sprites[3 + i] = new FSprite("SkyDandelion");
			sLeaser.sprites[3 + i].scale = 0.9f + Mathf.Sin(AbstrConsumable.ID.RandomSeed + i * puffCount) / 10f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector = Vector3.Slerp(lastRotation, rotation, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;
		sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("DangleFruit" + Custom.IntClamp(3 - bites, 0, 2) + "A");
		sLeaser.sprites[1].x = pos.x - camPos.x;
		sLeaser.sprites[1].y = pos.y - camPos.y;
		sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("JellyFish" + Custom.IntClamp(3 - bites, 0, 2) + "B");
		sLeaser.sprites[2].x = pos.x - camPos.x;
		sLeaser.sprites[2].y = pos.y - camPos.y;
		float num = 8f + Mathf.Sin(AbstrConsumable.ID.RandomSeed);
		sLeaser.sprites[0].rotation = Custom.VecToDeg(vector) + 180f;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(vector) + 180f;
		sLeaser.sprites[2].rotation = Custom.VecToDeg(vector) + 180f;
		for (int i = 0; i < puffCount; i++)
		{
			float num2 = Mathf.Lerp(90f, 170f, (float)(puffCount - 1) / 8f);
			float num3 = 0f - num2 / 2f;
			Vector2 vector2 = Custom.RotateAroundOrigo(vector, num3 + num2 * ((float)i / (float)(puffCount - 1)));
			sLeaser.sprites[3 + i].rotation = Custom.VecToDeg(vector);
			float num4 = num * sLeaser.sprites[2 + i].scale + 0.2f * Mathf.Sin(swayer + (float)(i * puffCount));
			sLeaser.sprites[3 + i].x = pos.x + vector2.x * num4 - camPos.x;
			sLeaser.sprites[3 + i].y = pos.y + vector2.y * num4 - camPos.y;
		}
		if (blink > 0 && Random.value < 0.5f)
		{
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = Color.Lerp(new Color(0.59f, 0.78f, 0.96f), palette.blackColor, Mathf.Pow(darkness, 2f));
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, darkness);
		sLeaser.sprites[2].color = Color.Lerp(color, sLeaser.sprites[1].color, 0.3f);
		for (int i = 0; i < puffCount; i++)
		{
			sLeaser.sprites[3 + i].color = Color.Lerp(Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f), palette.blackColor, darkness);
			sLeaser.sprites[3 + i].alpha = Mathf.Lerp(0.8f, 0.5f, i / puffCount);
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

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites != 0) ? SoundID.Slugcat_Bite_Water_Nut : SoundID.Slugcat_Eat_Water_Nut, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
		base.bodyChunks[0].rad = Mathf.InverseLerp(3f, 0f, bites) * 9.5f;
	}

	public void ThrowByPlayer()
	{
		base.firstChunk.vel += new Vector2(0f, 8f);
	}
}
