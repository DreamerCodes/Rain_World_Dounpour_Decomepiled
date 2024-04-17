using System.Globalization;
using UnityEngine;

namespace MoreSlugcats;

public class CollisionField : PhysicalObject, IDrawable
{
	public class Type : ExtEnum<Type>
	{
		public static readonly Type POISON_SMOKE = new Type("POISON_SMOKE", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class AbstractCollisionField : AbstractPhysicalObject
	{
		public int liveTime;

		public float radius;

		public Type fieldType;

		public AbstractCollisionField(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, Type fieldType, float radius, int liveTime)
			: base(world, AbstractObjectType.CollisionField, realizedObject, pos, ID)
		{
			this.fieldType = fieldType;
			this.liveTime = liveTime;
			this.radius = radius;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), liveTime.ToString(), radius.ToString(), fieldType.ToString());
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public Vector2? lastOutsideTerrainPos;

	public int liveTime;

	private float radius;

	public Type fieldType;

	public CollisionField(AbstractPhysicalObject abstractPhysicalObject, Type fieldType, float rad, int liveTime)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), rad, 0.01f);
		base.bodyChunks[0].actAsTrigger = true;
		bodyChunkConnections = new BodyChunkConnection[0];
		this.liveTime = liveTime;
		this.fieldType = fieldType;
		radius = rad;
		base.airFriction = 0.999f;
		base.gravity = 0f;
		bounce = 0f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.4f;
		base.firstChunk.loudness = 0f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		NewRoom(placeRoom);
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		lastOutsideTerrainPos = null;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		liveTime--;
		if (liveTime <= 0)
		{
			RemoveFromRoom();
			Destroy();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Circle20");
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].scaleX = radius / 10f;
		sLeaser.sprites[0].scaleY = radius / 10f;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = Color.white;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
		{
			sLeaser.sprites[num].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[num]);
		}
	}
}
