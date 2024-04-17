using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Lantern : PlayerCarryableItem, IDrawable, IProvideWarmth
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public LightSource lightSource;

	public float[,] flicker;

	public LanternStick stick;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth;

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 350f;

	public Lantern(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.8f;
		collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 0.8f;
		flicker = new float[2, 3];
		for (int i = 0; i < flicker.GetLength(0); i++)
		{
			flicker[i, 0] = 1f;
			flicker[i, 1] = 1f;
			flicker[i, 2] = 1f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < flicker.GetLength(0); i++)
		{
			flicker[i, 1] = flicker[i, 0];
			flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * ((Random.value < 0.5f) ? (-1f) : 1f);
			flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 1f / 30f);
			if (Random.value < 0.2f)
			{
				flicker[i, 2] = 1f + Mathf.Pow(Random.value, 3f) * 0.2f * ((Random.value < 0.5f) ? (-1f) : 1f);
			}
			flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
		}
		if (lightSource == null)
		{
			lightSource = new LightSource(base.firstChunk.pos, environmentalLight: false, new Color(1f, 0.2f, 0f), this);
			lightSource.affectedByPaletteDarkness = 0.5f;
			room.AddObject(lightSource);
		}
		else
		{
			lightSource.setPos = base.firstChunk.pos;
			lightSource.setRad = 250f * flicker[0, 0];
			lightSource.setAlpha = 1f;
			if (lightSource.slatedForDeletetion || lightSource.room != room)
			{
				lightSource = null;
			}
		}
		lastRotation = rotation;
		if (stick != null)
		{
			base.firstChunk.pos = stick.po.pos;
			base.firstChunk.vel *= 0f;
			rotation = (stick.po.data as PlacedObject.ResizableObjectData).handlePos.normalized;
			base.firstChunk.collideWithTerrain = false;
			base.firstChunk.collideWithObjects = false;
			canBeHitByWeapons = false;
			return;
		}
		base.firstChunk.collideWithTerrain = grabbedBy.Count == 0;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = 0f - Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		rotation = (rotation - Custom.PerpendicularVector(rotation) * ((base.firstChunk.ContactPoint.y < 0) ? 0.15f : 0.05f) * base.firstChunk.vel.x).normalized;
		if (base.firstChunk.ContactPoint.y < 0)
		{
			base.firstChunk.vel.x *= 0.8f;
		}
		if (abstractPhysicalObject is AbstractConsumable && grabbedBy.Count > 0 && !(abstractPhysicalObject as AbstractConsumable).isConsumed)
		{
			(abstractPhysicalObject as AbstractConsumable).Consume();
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		rotation = Custom.RNV();
		lastRotation = rotation;
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 5f && firstContact)
		{
			Vector2 pos = base.bodyChunks[chunk].pos + direction.ToVector2() * base.bodyChunks[chunk].rad * 0.9f;
			for (int i = 0; (float)i < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)); i++)
			{
				room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 1f, 1f), Random.value * 0.5f), null, 19, 47));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[(stick != null) ? 5 : 4];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A");
		sLeaser.sprites[1] = new FSprite("DangleFruit0B");
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].scaleX = 0.8f;
			sLeaser.sprites[i].scaleY = 0.9f;
		}
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["LightSource"];
		if (stick != null)
		{
			sLeaser.sprites[4] = TriangleMesh.MakeLongMesh(stick.stickPositions.Length, pointyTip: false, customColor: false);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(v);
		}
		sLeaser.sprites[2].x = vector.x - v.x * 3f - camPos.x;
		sLeaser.sprites[2].y = vector.y - v.y * 3f - camPos.y;
		sLeaser.sprites[2].scale = Mathf.Lerp(flicker[0, 1], flicker[0, 0], timeStacker) * 2f;
		sLeaser.sprites[3].x = vector.x - v.x * 3f - camPos.x;
		sLeaser.sprites[3].y = vector.y - v.y * 3f - camPos.y;
		sLeaser.sprites[3].scale = Mathf.Lerp(flicker[1, 1], flicker[1, 0], timeStacker) * 200f / 8f;
		if (stick != null)
		{
			Vector2 vector2 = stick.po.pos + (stick.po.data as PlacedObject.ResizableObjectData).handlePos;
			Vector2 vector3 = vector + Custom.DirVec(vector2, stick.po.pos) * 25f;
			Vector2 vector4 = vector2 + Custom.DirVec(vector3, vector2) * 5f;
			float num = 1f;
			for (int j = 0; j < stick.stickPositions.Length; j++)
			{
				float t = (float)j / (float)(stick.stickPositions.Length - 1);
				float num2 = Mathf.Lerp(1f + Mathf.Min((stick.po.data as PlacedObject.ResizableObjectData).handlePos.magnitude / 190f, 3f), 0.5f, t);
				Vector2 vector5 = Vector2.Lerp(vector2, vector3, t) + stick.stickPositions[j] * Mathf.Lerp(num2 * 0.6f, 1f, t);
				Vector2 normalized = (vector4 - vector5).normalized;
				Vector2 vector6 = Custom.PerpendicularVector(normalized);
				float num3 = Vector2.Distance(vector4, vector5) / 5f;
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4, vector4 - normalized * num3 - vector6 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 1, vector4 - normalized * num3 + vector6 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 2, vector5 + normalized * num3 - vector6 * num2 - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 3, vector5 + normalized * num3 + vector6 * num2 - camPos);
				vector4 = vector5;
				num = num2;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = new Color(1f, 0.2f, 0f);
		sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		sLeaser.sprites[2].color = Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 1f, 1f), 0.3f);
		sLeaser.sprites[3].color = new Color(1f, 0.4f, 0.3f);
		if (stick != null)
		{
			sLeaser.sprites[4].color = palette.blackColor;
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
		if (stick != null)
		{
			newContatiner.AddChild(sLeaser.sprites[4]);
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[3]);
	}

	Vector2 IProvideWarmth.Position()
	{
		return base.firstChunk.pos;
	}
}
