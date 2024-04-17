using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class GlowWeed : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public GlowWeed fruit;

		public Vector2 stuckPos;

		public float ropeLength;

		public Vector2[] displacements;

		public Vector2[,] segs;

		public int releaseCounter;

		private float connRad;

		private Vector2[] initialDisplacements;

		private Color color;

		public Stalk(GlowWeed fruit, Room room, Vector2 fruitPos)
		{
			this.fruit = fruit;
			color = fruit.color;
			fruit.firstChunk.HardSetPosition(fruitPos);
			stuckPos.x = fruitPos.x;
			ropeLength = -1f;
			int x = room.GetTilePosition(fruitPos).x;
			for (int num = room.GetTilePosition(fruitPos).y; num >= 0; num--)
			{
				if (room.GetTile(x, num).Solid || num == 0)
				{
					stuckPos.y = room.MiddleOfTile(x, num).y + 10f;
					ropeLength = Mathf.Abs(stuckPos.y - fruitPos.y);
					break;
				}
			}
			segs = new Vector2[Math.Max(1, (int)(ropeLength / 15f)), 3];
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float t = (float)i / (float)(segs.GetLength(0) - 1);
				segs[i, 0] = Vector2.Lerp(stuckPos, fruitPos, t);
				segs[i, 1] = segs[i, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			displacements = new Vector2[segs.GetLength(0)];
			initialDisplacements = new Vector2[segs.GetLength(0)];
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
			for (int j = 0; j < displacements.Length; j++)
			{
				displacements[j] = Custom.RNV();
				initialDisplacements[j] = displacements[j];
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
			for (int i = 0; i < displacements.Length; i++)
			{
				if (Vector2.Distance(displacements[i], initialDisplacements[i]) < 2f)
				{
					initialDisplacements[i] = Custom.RNV() * 3f;
					Vector2[] array = initialDisplacements;
					int num = i;
					array[num].y = array[num].y * 0.5f;
					if (Mathf.Sign(initialDisplacements[i].x) == Mathf.Sign(displacements[i].x))
					{
						Vector2[] array2 = initialDisplacements;
						int num2 = i;
						array2[num2].x = array2[num2].x * -1f;
					}
				}
				displacements[i] = Vector2.Lerp(displacements[i], initialDisplacements[i], 0.01f);
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				_ = (float)j / (float)(segs.GetLength(0) - 1);
				segs[j, 1] = segs[j, 0];
				segs[j, 0] += segs[j, 2];
				segs[j, 2] *= 0.99f;
				segs[j, 2].y += 0.9f;
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			List<Vector2> list = new List<Vector2>();
			list.Add(stuckPos);
			for (int k = 0; k < segs.GetLength(0); k++)
			{
				list.Add(segs[k, 0]);
			}
			if (releaseCounter > 0)
			{
				releaseCounter--;
			}
			if (fruit != null)
			{
				fruit.rotation = Vector3.Slerp(fruit.rotation, new Vector2(displacements[displacements.Length - 1].x / 2f, 1f), 0.4f);
				list.Add(fruit.firstChunk.pos + fruit.rotation * 35f);
				if (!Custom.DistLess(fruit.firstChunk.pos, stuckPos, ropeLength * 1.4f + 10f) || fruit.slatedForDeletetion || fruit.bites < 3 || fruit.room != room || releaseCounter == 1)
				{
					fruit.AbstrConsumable.Consume();
					fruit.stalk = null;
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
				float num3 = Custom.LerpMap(num2, 0f, 0.5f, 1.4f, 0f) + Mathf.Lerp(1f, 0.5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * (float)Math.PI));
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1 && fruit != null)
				{
					vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
				}
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				if (i < segs.GetLength(0) - 1)
				{
					vector2 += (normalized * displacements[i].y + vector3 * displacements[i].x) * Custom.LerpMap(Vector2.Distance(vector, vector2), connRad, connRad * 25f, 10f, 6f);
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
				sLeaser.sprites[i].color = Color.Lerp(color, palette.blackColor, 0.6f + (1f - (float)i / (float)sLeaser.sprites.Length) * 0.4f);
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

	public int bites;

	private LightSource myLight;

	private float LightCounter;

	private float LightRad;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public GlowWeed(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		bites = 3;
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
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		LightRad = UnityEngine.Random.Range(15f, 35f);
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Input.mousePosition) * 3f;
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
		float num = 7f + Mathf.Sin(LightCounter) * 0.3f;
		LightCounter += UnityEngine.Random.Range(0.01f, 0.2f);
		if (myLight != null && (myLight.room != room || !myLight.room.BeingViewed))
		{
			myLight.slatedForDeletetion = true;
			myLight = null;
		}
		if (myLight == null && room.BeingViewed)
		{
			LightCounter = UnityEngine.Random.Range(0f, 100f);
			myLight = new LightSource(base.firstChunk.pos, environmentalLight: true, color, this);
			room.AddObject(myLight);
			myLight.colorFromEnvironment = false;
			myLight.noGameplayImpact = true;
			myLight.stayAlive = true;
			myLight.requireUpKeep = true;
			return;
		}
		if (myLight != null)
		{
			float num2 = (float)bites / 3f;
			myLight.HardSetPos(base.firstChunk.pos);
			myLight.HardSetRad(LightRad * num * num2);
			myLight.HardSetAlpha(Mathf.Lerp(0f, num * num2 / 2f, room.Darkness(myLight.Pos)) / 4f);
			if (myLight.rad > 5f)
			{
				myLight.stayAlive = true;
			}
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			BodyChunk bodyChunk = base.firstChunk;
			bodyChunk.vel.x = bodyChunk.vel.x * 0.8f;
		}
		float num3 = 0.04f;
		if (AbstrConsumable.isConsumed)
		{
			num3 = 1f;
		}
		if (UnityEngine.Random.value < num3 && base.Submersion > 0.5f && room.abstractRoom.creatures.Count > 0 && grabbedBy.Count == 0)
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
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["WaterNut"];
		sLeaser.sprites[0].scaleX = 1.2f;
		sLeaser.sprites[0].scaleY = 1.6f;
		sLeaser.sprites[1] = new FSprite("DangleFruit0A");
		sLeaser.sprites[2] = new FSprite("DangleFruit0B");
		for (int i = 1; i < 3; i++)
		{
			sLeaser.sprites[i].scaleX = 0.9f;
			sLeaser.sprites[i].scaleY = 1.3f;
		}
		sLeaser.sprites[3] = new FSprite("DangleFruit2A");
		sLeaser.sprites[3].scaleX = 1.1f;
		sLeaser.sprites[3].scaleY = -1.4f;
		sLeaser.sprites[4] = new FSprite("DangleFruit2A");
		sLeaser.sprites[4].scaleY = 1.4f;
		sLeaser.sprites[4].scaleX = 1.1f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = (float)bites / 3f;
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[0].alpha = 0.6f + rCam.PaletteDarkness() / 2f;
		sLeaser.sprites[0].scaleX = 1.2f * num;
		sLeaser.sprites[0].scaleY = 1.6f * num;
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[1].scaleX = 0.9f * num;
		sLeaser.sprites[1].scaleY = 1.3f * num;
		sLeaser.sprites[2].x = vector.x - camPos.x;
		sLeaser.sprites[2].y = vector.y - camPos.y;
		sLeaser.sprites[2].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[2].scaleX = 0.9f * num;
		sLeaser.sprites[2].scaleY = 1.3f * num;
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = base.blinkColor;
			sLeaser.sprites[2].color = base.blinkColor;
			sLeaser.sprites[3].color = base.blinkColor;
			sLeaser.sprites[4].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[2].color = color;
			sLeaser.sprites[3].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.4f);
			sLeaser.sprites[4].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.4f);
		}
		vector2 = Custom.DirVec(default(Vector2), vector2);
		sLeaser.sprites[3].x = vector.x + vector2.x * (10f * num) - camPos.x;
		sLeaser.sprites[3].y = vector.y + vector2.y * (10f * num) - camPos.y;
		sLeaser.sprites[3].rotation = sLeaser.sprites[0].rotation;
		sLeaser.sprites[4].x = vector.x + vector2.x * (-10f * num) - camPos.x;
		sLeaser.sprites[4].y = vector.y + vector2.y * (-10f * num) - camPos.y;
		sLeaser.sprites[4].rotation = sLeaser.sprites[0].rotation;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = new Color(0.8f, 1f, 0.4f);
		sLeaser.sprites[0].color = Color.Lerp(palette.waterColor1, palette.waterColor2, 0.5f);
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
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites != 0) ? SoundID.Slugcat_Bite_Dangle_Fruit : SoundID.Slugcat_Eat_Dangle_Fruit, base.firstChunk.pos);
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
