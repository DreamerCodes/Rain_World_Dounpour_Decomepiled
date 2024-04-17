using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DangleFruit : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public DangleFruit fruit;

		public Vector2 stuckPos;

		public float ropeLength;

		public Vector2[] displacements;

		public Vector2[,] segs;

		public int releaseCounter;

		private float connRad;

		public Stalk(DangleFruit fruit, Room room, Vector2 fruitPos)
		{
			this.fruit = fruit;
			fruit.firstChunk.HardSetPosition(fruitPos);
			stuckPos.x = fruitPos.x;
			ropeLength = -1f;
			int x = room.GetTilePosition(fruitPos).x;
			for (int i = room.GetTilePosition(fruitPos).y; i < room.TileHeight; i++)
			{
				if (room.GetTile(x, i).Solid)
				{
					stuckPos.y = room.MiddleOfTile(x, i).y - 10f;
					ropeLength = Mathf.Abs(stuckPos.y - fruitPos.y);
					break;
				}
			}
			segs = new Vector2[Math.Max(1, (int)(ropeLength / 15f)), 3];
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				float t = (float)j / (float)(segs.GetLength(0) - 1);
				segs[j, 0] = Vector2.Lerp(stuckPos, fruitPos, t);
				segs[j, 1] = segs[j, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			displacements = new Vector2[segs.GetLength(0)];
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
			for (int k = 0; k < displacements.Length; k++)
			{
				displacements[k] = Custom.RNV();
			}
			UnityEngine.Random.state = state;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (ropeLength == -1f)
			{
				Destroy();
				return;
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				segs[i, 1] = segs[i, 0];
				segs[i, 0] += segs[i, 2];
				segs[i, 2] *= 0.99f;
				segs[i, 2].y -= 0.9f;
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			if (releaseCounter > 0)
			{
				releaseCounter--;
			}
			if (fruit != null)
			{
				fruit.setRotation = Custom.DirVec(fruit.firstChunk.pos, segs[segs.GetLength(0) - 1, 0]);
				if (!Custom.DistLess(fruit.firstChunk.pos, stuckPos, ropeLength * 1.4f + 10f) || fruit.slatedForDeletetion || fruit.bites < 3 || fruit.room != room || releaseCounter == 1)
				{
					fruit.AbstrConsumable.Consume();
					fruit = null;
				}
			}
		}

		private void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			bool flag = false;
			while (!flag)
			{
				if (num == 0)
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad))
					{
						Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad);
						segs[num, 0] += vector2 * 0.5f;
						segs[num, 2] += vector2 * 0.5f;
						segs[num - 1, 0] -= vector2 * 0.5f;
						segs[num - 1, 2] -= vector2 * 0.5f;
					}
					if (num == segs.GetLength(0) - 1 && fruit != null && !Custom.DistLess(segs[num, 0], fruit.firstChunk.pos, connRad))
					{
						Vector2 vector3 = Custom.DirVec(segs[num, 0], fruit.firstChunk.pos) * (Vector2.Distance(segs[num, 0], fruit.firstChunk.pos) - connRad);
						segs[num, 0] += vector3 * 0.75f;
						segs[num, 2] += vector3 * 0.75f;
						fruit.firstChunk.vel -= vector3 * 0.25f;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: false);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = stuckPos;
			float num = 1.5f;
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num2 = (float)i / (float)(segs.GetLength(0) - 1);
				float num3 = Custom.LerpMap(num2, 0f, 0.5f, 1f, 0f) + Mathf.Lerp(1f, 0.5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * (float)Math.PI));
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1 && fruit != null)
				{
					vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
				}
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				if (i < segs.GetLength(0) - 1)
				{
					vector2 += (normalized * displacements[i].y + vector3 * displacements[i].x) * Custom.LerpMap(Vector2.Distance(vector, vector2), connRad, connRad * 5f, 4f, 0f);
				}
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - camPos);
				vector = vector2;
				num = num3;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public Stalk stalk;

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public int bites = 3;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public DangleFruit(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
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
		if (base.Submersion > 0.5f && room.abstractRoom.creatures.Count > 0 && grabbedBy.Count == 0)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.dead && (abstractCreature.realizedCreature as JetFish).AI.goToFood == null && (abstractCreature.realizedCreature as JetFish).AI.WantToEatObject(this))
			{
				(abstractCreature.realizedCreature as JetFish).AI.goToFood = this;
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			stalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(stalk);
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			stalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(stalk);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (stalk != null && stalk.releaseCounter == 0)
		{
			stalk.releaseCounter = UnityEngine.Random.Range(30, 50);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A");
		sLeaser.sprites[1] = new FSprite("DangleFruit0B");
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = pos.x - camPos.x;
			sLeaser.sprites[i].y = pos.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(v);
			sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("DangleFruit" + Custom.IntClamp(3 - bites, 0, 2) + ((i == 0) ? "A" : "B"));
		}
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[1].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
		if (ModManager.MSC && rCam.room.game.session is StoryGameSession && rCam.room.world.name == "HR")
		{
			color = Color.Lerp(RainWorld.SaturatedGold, palette.blackColor, darkness);
		}
		else
		{
			color = Color.Lerp(new Color(0f, 0f, 1f), palette.blackColor, darkness);
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
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk.pos);
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
