using System.Collections.Generic;
using RWCustom;
using Smoke;
using UnityEngine;

namespace MoreSlugcats;

public class GooieDuck : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public int bites;

	private Smolder smolder;

	private float lastSmoulder;

	private int segmentCount;

	private Color CoreColor;

	private float PulserA;

	private float PulserB;

	private Color HuskColor;

	private Vector2 HomePos;

	private bool StringsBroke;

	private float[] StringSnapPercent;

	private Vector2[] StringGoals;

	private bool[] failedStrings;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int BitesLeft => bites;

	public int FoodPoints => 2;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public GooieDuck(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.5f;
		surfaceFriction = 0.7f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		Random.InitState(base.abstractPhysicalObject.ID.RandomSeed);
		segmentCount = Mathf.Max(5, (int)(Random.value * 7f));
		bites = 6;
		HomePos = default(Vector2);
		StringsBroke = false;
		StringSnapPercent = new float[Mathf.Max(8, (int)(Random.value * 14f))];
		failedStrings = new bool[StringSnapPercent.Length];
		PulserA = Random.value * 14f;
		PulserB = Random.value * 32f;
		lastSmoulder = Random.Range(0f, 900f);
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
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			BodyChunk bodyChunk = base.firstChunk;
			bodyChunk.vel.x = bodyChunk.vel.x * 0.8f;
		}
		if (!AbstrConsumable.isConsumed)
		{
			base.gravity = 0.1f;
			rotation = Custom.DirVec(base.firstChunk.pos, HomePos + new Vector2(0f, 50f));
			if (Vector2.Distance(base.firstChunk.pos, HomePos) > 1f)
			{
				base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, HomePos) / 1.3f;
				if (Vector2.Distance(base.firstChunk.pos, HomePos) > 10f && base.firstChunk.vel.magnitude < 2f && Random.value < 0.2f)
				{
					room.PlaySound(SoundID.Mouse_Light_Flicker, base.firstChunk, loop: false, 1f, 0.6f + Random.value / 10f);
				}
			}
			base.firstChunk.vel *= 0.98f;
			if (Vector2.Distance(base.firstChunk.pos, HomePos) < 4f && base.firstChunk.vel.magnitude < 2f)
			{
				base.firstChunk.vel *= 0.5f;
			}
			if (Vector2.Distance(base.firstChunk.pos, HomePos) > 110f || grabbedBy.Count > 0)
			{
				AbstrConsumable.Consume();
				room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, base.firstChunk, loop: false, 0.8f, 1.6f + Random.value / 10f);
			}
			int num = 0;
			for (int i = 0; i < failedStrings.Length; i++)
			{
				if (!failedStrings[i])
				{
					num++;
				}
			}
			if (StringGoals != null && num == 0)
			{
				StringsBroke = true;
				AbstrConsumable.Consume();
			}
			else
			{
				StringsBroke = false;
			}
			if (StringGoals == null)
			{
				StringGoals = new Vector2[StringSnapPercent.Length];
				for (int j = 0; j < StringGoals.Length; j++)
				{
					int num2 = 20;
					for (int k = 0; k < num2 + 1; k++)
					{
						failedStrings[j] = false;
						if (k == num2)
						{
							StringGoals[j] = HomePos;
							failedStrings[j] = true;
							break;
						}
						float num3 = Mathf.Max(0.3f, k / num2);
						Vector2 vector = new Vector2(0f, 0f);
						float num4 = (float)k / (float)num2;
						vector += new Vector2(Mathf.Lerp(-18f, 18f, Mathf.Sin((float)(segmentCount * j) * Mathf.Max(0.4f, num4))) * num3, Mathf.Lerp(10f, -3f, num4));
						IntVector2 tilePosition = room.GetTilePosition(HomePos);
						List<IntVector2> path = new List<IntVector2>();
						room.RayTraceTilesList(tilePosition.x, tilePosition.y, tilePosition.x + (int)vector.x, tilePosition.y + (int)vector.y, ref path);
						if (path.Count <= 0)
						{
							continue;
						}
						int l = 0;
						bool flag = false;
						for (; l < path.Count; l++)
						{
							if (flag)
							{
								break;
							}
							if (room.GetTile(path[l]).Solid && (j == 0 || (j >= 1 && StringGoals[j - 1] != room.MiddleOfTile(path[l]))))
							{
								StringGoals[j] = room.MiddleOfTile(path[l]);
								flag = true;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			base.gravity = 1f;
			if (!StringsBroke)
			{
				float num5 = 0f;
				for (int m = 0; m < StringSnapPercent.Length; m++)
				{
					StringSnapPercent[m] += 0.35f;
					if (StringSnapPercent[m] > 1f)
					{
						StringSnapPercent[m] = 1f;
					}
					num5 += StringSnapPercent[m];
				}
				if (num5 >= (float)StringSnapPercent.Length)
				{
					StringsBroke = true;
				}
			}
		}
		if (smolder != null && smolder.slatedForDeletetion)
		{
			smolder = null;
		}
		if (smolder == null && lastSmoulder <= 0f)
		{
			lastSmoulder = Random.Range(500f, 5200f);
			smolder = new Smolder(room, base.firstChunk.pos + Custom.RNV() * 14f, base.firstChunk, null);
			room.AddObject(smolder);
			smolder.life = 40 + (int)(Random.value * 60f);
			smolder.WindPuff(base.firstChunk.pos + Custom.RNV() * 14f, 300f, 80f);
		}
		else
		{
			lastSmoulder -= 1f;
		}
		PulserA += 0.02f;
		PulserB += 0.01f;
		PulserA += Mathf.Sin(PulserB) / 100f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		foreach (UpdatableAndDeletable update in placeRoom.updateList)
		{
			if (update is WormGrass)
			{
				(update as WormGrass).AddNewRepulsiveObject(this);
				break;
			}
		}
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			HomePos = placeRoom.MiddleOfTile(AbstrConsumable.pos);
			AbstrConsumable.isConsumed = false;
			StringsBroke = false;
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			HomePos = placeRoom.MiddleOfTile(AbstrConsumable.pos);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		lastOutsideTerrainPos = null;
		foreach (UpdatableAndDeletable update in newRoom.updateList)
		{
			if (update is WormGrass)
			{
				(update as WormGrass).AddNewRepulsiveObject(this);
				break;
			}
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		sLeaser.sprites = new FSprite[StringSnapPercent.Length + 2 + segmentCount];
		for (int i = 0; i < StringSnapPercent.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].scale = 1f;
			sLeaser.sprites[i].anchorY = ((Random.value < 0.5f) ? 1f : 0f);
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
		}
		sLeaser.sprites[StringSnapPercent.Length] = new FSprite("DangleFruit0A");
		sLeaser.sprites[StringSnapPercent.Length].scale = 0.9f;
		for (int j = 0; j < segmentCount; j++)
		{
			sLeaser.sprites[StringSnapPercent.Length + 1 + j] = new FSprite("LegsA" + (int)(Random.value * 6f));
			sLeaser.sprites[StringSnapPercent.Length + 1 + j].scaleX = 0.35f;
			sLeaser.sprites[StringSnapPercent.Length + 1 + j].scaleY = 1.4f;
		}
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount] = new FSprite("Futile_White");
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].scale = 10f;
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].alpha = 1f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (StringGoals != null)
		{
			for (int i = 0; i < StringSnapPercent.Length; i++)
			{
				sLeaser.sprites[i].isVisible = !StringsBroke && !failedStrings[i];
				if (!sLeaser.sprites[i].isVisible)
				{
					sLeaser.sprites[i].x = vector.x - camPos.x;
					sLeaser.sprites[i].y = vector.y - camPos.y;
					sLeaser.sprites[i].scale = 1f;
					sLeaser.sprites[i].color = Color.black;
					sLeaser.sprites[i].rotation = 0f;
				}
				else if (!StringsBroke)
				{
					if (sLeaser.sprites[i].anchorY == 0f)
					{
						sLeaser.sprites[i].x = vector.x - camPos.x;
						sLeaser.sprites[i].y = vector.y - camPos.y;
					}
					else
					{
						sLeaser.sprites[i].x = StringGoals[i].x - camPos.x;
						sLeaser.sprites[i].y = StringGoals[i].y - camPos.y;
					}
					sLeaser.sprites[i].rotation = Custom.VecToDeg(Custom.DirVec(vector, StringGoals[i]));
					sLeaser.sprites[i].scaleY = Mathf.Max(1f, Vector2.Distance(vector, StringGoals[i]) * (1f - StringSnapPercent[i]));
					sLeaser.sprites[i].scaleX = 1f;
					sLeaser.sprites[i].color = CoreColor;
				}
			}
		}
		float num = (float)bites / 6f;
		float num2 = 360f / (float)segmentCount;
		for (int j = 0; j < segmentCount + 1; j++)
		{
			if (j == 0)
			{
				sLeaser.sprites[StringSnapPercent.Length + j].isVisible = BitesLeft > 0;
				sLeaser.sprites[StringSnapPercent.Length + j].x = vector.x + Custom.DegToVec(PulserB * 100f).x - camPos.x;
				sLeaser.sprites[StringSnapPercent.Length + j].y = vector.y + Custom.DegToVec(PulserB * 100f).y - camPos.y;
				sLeaser.sprites[StringSnapPercent.Length + j].rotation = Custom.VecToDeg(v);
				sLeaser.sprites[StringSnapPercent.Length + j].scale = 0.85f + Mathf.Sin(PulserA) / 6f;
				if (blink > 0 && Random.value < 0.5f)
				{
					sLeaser.sprites[StringSnapPercent.Length + j].color = base.blinkColor;
				}
				else
				{
					sLeaser.sprites[StringSnapPercent.Length + j].color = Color.Lerp(CoreColor, Color.white, 0.1f + Mathf.Sin(PulserA) / 10f);
				}
				continue;
			}
			float num3 = Custom.VecToDeg(v) + num2 * (float)j + 270f;
			sLeaser.sprites[StringSnapPercent.Length + j].isVisible = (float)j / (float)segmentCount <= num;
			sLeaser.sprites[StringSnapPercent.Length + j].rotation = num3 + Mathf.Sin(PulserA * (float)j) / 8f;
			sLeaser.sprites[StringSnapPercent.Length + j].x = vector.x + (Custom.DegToVec(num3) * -0.1f).x - camPos.x;
			sLeaser.sprites[StringSnapPercent.Length + j].y = vector.y + (Custom.DegToVec(num3) * -0.1f).y - camPos.y;
			sLeaser.sprites[StringSnapPercent.Length + j].scaleX = 0.25f + Mathf.Sin(PulserB) / 8f;
			sLeaser.sprites[StringSnapPercent.Length + j].scaleY = 1.3f + Mathf.Sin(PulserB + PulserA) / 10f;
			Color a = Color.Lerp(HuskColor, rCam.currentPalette.blackColor, 0.5f + Mathf.Sin((float)j * 2f) / 2f);
			if (blink > 0 && Random.value < 0.5f)
			{
				sLeaser.sprites[StringSnapPercent.Length + j].color = Color.Lerp(a, base.blinkColor, 0.6f);
			}
			else
			{
				sLeaser.sprites[StringSnapPercent.Length + j].color = a;
			}
		}
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].x = vector.x - camPos.x;
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].y = vector.y - camPos.y;
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].scale = 0.95f + Mathf.Sin(PulserA) / 25f;
		sLeaser.sprites[StringSnapPercent.Length + 1 + segmentCount].color = Color.Lerp(sLeaser.sprites[StringSnapPercent.Length + 1].color, Color.Lerp(Color.white, rCam.currentPalette.blackColor, darkness / 1.4f), 0.34f + Mathf.Sin(PulserA) / 20f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		CoreColor = Color.Lerp(new Color(0.35f, 0.8f, 0.82f), palette.blackColor, darkness / 3f);
		HuskColor = Color.Lerp(new Color(0.19f, 0.16f, 0.07f), palette.blackColor, darkness);
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
			if (i < StringSnapPercent.Length + segmentCount + 1)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			else
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		if (bites == 6)
		{
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Duck_Pop, grasp.grabber.mainBodyChunk.pos, 1f, 0.5f + Random.value * 0.5f);
			for (int i = 0; i < 3; i++)
			{
				room.AddObject(new WaterDrip(base.firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), waterColor: false));
			}
		}
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
