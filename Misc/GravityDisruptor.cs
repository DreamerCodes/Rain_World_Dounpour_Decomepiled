using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class GravityDisruptor : UpdatableAndDeletable, IDrawable, INotifyWhenRoomIsReady
{
	public class DisruptorSpeck : CosmeticSprite
	{
		private GravityDisruptor disruptor;

		private float myFloatSpeed;

		private float myOrbitRad;

		private float newOrbitRad;

		public DisruptorSpeck(Vector2 initPos, GravityDisruptor disruptor)
		{
			pos = initPos;
			lastPos = pos;
			this.disruptor = disruptor;
			myFloatSpeed = Mathf.Lerp(2f, 8f, Random.value);
			vel = Custom.RNV() * myFloatSpeed;
			myOrbitRad = Vector2.Distance(disruptor.pos, pos);
			newOrbitRad = myOrbitRad;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (Random.value < 0.01f)
			{
				newOrbitRad = Mathf.Lerp(50f, 400f, Random.value);
			}
			myOrbitRad = Mathf.Lerp(myOrbitRad, newOrbitRad, 0.01f);
			Vector2 vector = Custom.DirVec(disruptor.pos, pos);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			float num = Mathf.Sign(Custom.DistanceToLine(pos + vel, disruptor.pos, pos));
			float num2 = Vector2.Distance(disruptor.pos, pos);
			pos += vector * (myOrbitRad - num2) * 0.05f;
			vel += vector * (myOrbitRad - num2) * 0.05f;
			vel += vector2 * num * Custom.LerpMap(num2, 400f, 100f, 0.01f, 0.15f);
			vel = Vector2.Lerp(vel, vel.normalized * (myFloatSpeed / (Mathf.Lerp(myOrbitRad, 100f, 0.5f) * 0.01f)), 0.01f) * disruptor.power;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].color = new Color(disruptor.power, disruptor.power, disruptor.power);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public PlacedObject placedObject;

	private int depth;

	public Vector2 pos;

	public Vector2 lastPos;

	public int debugMode;

	public List<GenericZeroGSpeck> roomSpecks;

	public List<DisruptorSpeck> mySpecks;

	public float[,] lights;

	private Vector2 pointDir;

	private Vector2 getToPointDir;

	private float dirFac;

	private float dirFacGetTo;

	private float power;

	private float lastPower;

	public GravityDisruptor(PlacedObject placedObject, Room room)
	{
		this.placedObject = placedObject;
		pos = placedObject.pos;
		lastPos = pos;
		depth = 0;
		if (!room.GetTile(placedObject.pos).Solid)
		{
			depth = (room.GetTile(placedObject.pos).wallbehind ? 1 : 2);
		}
		roomSpecks = new List<GenericZeroGSpeck>();
		mySpecks = new List<DisruptorSpeck>();
		for (int i = 0; i < 20; i++)
		{
			room.AddObject(new DisruptorSpeck(pos + Custom.RNV() * 400f * Random.value, this));
		}
		lights = new float[16, 4];
		for (int j = 0; j < lights.GetLength(0); j++)
		{
			lights[j, 3] = Random.value;
		}
		pointDir = Custom.RNV();
		getToPointDir = pointDir;
		dirFac = Random.value;
		dirFacGetTo = dirFac;
		power = 1f;
		lastPower = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		pos = placedObject.pos;
		if (lastPos != pos)
		{
			debugMode = 80;
		}
		if (debugMode > 0)
		{
			debugMode--;
		}
		if (power > 0f)
		{
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						if (Custom.DistLess(pos, room.physicalObjects[i][j].bodyChunks[k].pos, 450f))
						{
							BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[k];
							Vector2 vector = Custom.DirVec(pos, bodyChunk.pos);
							Vector2 vector2 = Custom.PerpendicularVector(vector);
							float num = Mathf.Sign(Custom.DistanceToLine(bodyChunk.pos + bodyChunk.vel, pos, bodyChunk.pos));
							float num2 = Vector2.Distance(pos, bodyChunk.pos);
							float num3 = Mathf.Pow(Mathf.InverseLerp(450f, 200f, num2), 2f);
							bodyChunk.pos += vector * (260f - num2) * 0.05f * num3 * ((num2 < 260f) ? 1f : 0.5f) * power;
							bodyChunk.vel += vector * (260f - num2) * 0.05f * num3 * ((num2 < 260f) ? 1f : 0.5f) * power;
							bodyChunk.vel += vector2 * num * 0.1f * num3 * power;
						}
					}
				}
			}
			for (int l = 0; l < roomSpecks.Count; l++)
			{
				if (Custom.DistLess(pos, roomSpecks[l].pos, 450f))
				{
					GenericZeroGSpeck genericZeroGSpeck = roomSpecks[l];
					Vector2 vector3 = Custom.PerpendicularVector(Custom.DirVec(pos, genericZeroGSpeck.pos));
					float num4 = Mathf.Pow(Mathf.InverseLerp(450f, 200f, Vector2.Distance(pos, genericZeroGSpeck.pos)), 2f);
					genericZeroGSpeck.vel += vector3 * Mathf.Sign(Custom.DistanceToLine(genericZeroGSpeck.pos + genericZeroGSpeck.vel, pos, genericZeroGSpeck.pos)) * 0.5f * num4 * power;
				}
			}
		}
		pointDir = Vector3.Slerp(pointDir, getToPointDir, 0.01f);
		if (Random.value < 0.0125f)
		{
			getToPointDir = Custom.RNV();
		}
		dirFac = Mathf.Lerp(dirFac, dirFacGetTo, 0.01f);
		if (Random.value < 0.0125f)
		{
			dirFacGetTo = Random.value;
		}
		for (int m = 0; m < lights.GetLength(0); m++)
		{
			lights[m, 2] = lights[m, 1];
			lights[m, 1] = lights[m, 0];
			lights[m, 0] = Mathf.Clamp(lights[m, 0] + Mathf.Lerp(-1f, 1f, Random.value) / 120f + Mathf.Lerp(-1f, 1f, lights[m, 3]) / 60f, 0f, 1f);
			float num5 = Vector2.Dot(Custom.DegToVec((float)m / 16f * 360f), pointDir);
			lights[m, 0] = Mathf.Lerp(lights[m, 0], Mathf.Pow(Mathf.InverseLerp(-1f, 1f, num5), 1.5f), Mathf.Pow(Mathf.Abs(num5), 8f) * 0.3f * dirFac * Mathf.InverseLerp(0.5f, 0f, Mathf.Abs(0.5f - lights[m, 3])));
			float num6 = 0f;
			num6 += lights[(m < lights.GetLength(0) - 1) ? (m + 1) : 0, 2];
			num6 += lights[(m > 0) ? (m - 1) : (lights.GetLength(0) - 1), 2];
			lights[m, 0] = Mathf.Lerp(lights[m, 0], num6 / 2f, 0.05f);
			if (Random.value < 0.005f)
			{
				lights[m, 3] = Random.value;
			}
		}
		lastPower = power;
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
		{
			power = room.world.rainCycle.brokenAntiGrav.CurrentAntiGravity;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[17];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["GravityDisruptor"];
		sLeaser.sprites[0].scale = 37.5f;
		for (int i = 0; i < 16; i++)
		{
			sLeaser.sprites[i + 1] = new FSprite("Futile_White");
			sLeaser.sprites[i + 1].scaleX = 0.9375f;
			sLeaser.sprites[i + 1].scaleY = 3.125f;
			sLeaser.sprites[i + 1].anchorY = -0.55f;
			sLeaser.sprites[i + 1].rotation = (float)i / 16f * 360f;
			sLeaser.sprites[i + 1].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
			sLeaser.sprites[i + 1].alpha = 1f - (10f * (float)depth + 2f) / 30f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Lerp(lastPower, power, timeStacker);
		Vector2 vector = rCam.ApplyDepth(placedObject.pos, -3f + 10f * (float)depth);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].alpha = num;
		for (int i = 0; i < 16; i++)
		{
			sLeaser.sprites[i + 1].x = vector.x - camPos.x;
			sLeaser.sprites[i + 1].y = vector.y - camPos.y;
			if (debugMode > 0)
			{
				sLeaser.sprites[i + 1].color = new Color((i % 2 == 0) ? 1f : 0f, (i % 2 == 1) ? 1f : 0f, 0f);
			}
			else
			{
				sLeaser.sprites[i + 1].color = new Color(0f, 0f, Mathf.Lerp(lights[i, 1], lights[i, 0], timeStacker) * num);
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Foreground");
		}
		for (int i = 1; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is GenericZeroGSpeck)
			{
				roomSpecks.Add(room.updateList[i] as GenericZeroGSpeck);
			}
		}
	}
}
