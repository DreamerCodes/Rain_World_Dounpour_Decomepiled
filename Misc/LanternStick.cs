using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LanternStick : UpdatableAndDeletable, IDrawable, IProvideWarmth
{
	public PlacedObject po;

	public Lantern lantern;

	public Vector2[] stickPositions;

	float IProvideWarmth.warmth => ((IProvideWarmth)lantern).warmth;

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => ((IProvideWarmth)lantern).range;

	public LanternStick(Room room, PlacedObject po)
	{
		base.room = room;
		this.po = po;
		lantern = new Lantern(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, room.GetWorldCoordinate(po.pos), room.game.GetNewID()));
		lantern.room = room;
		lantern.stick = this;
		lantern.firstChunk.HardSetPosition(po.pos);
		stickPositions = new Vector2[(int)Mathf.Clamp((po.data as PlacedObject.ResizableObjectData).handlePos.magnitude / 11f, 3f, 30f)];
		Random.State state = Random.state;
		Random.InitState((int)po.pos.x);
		for (int i = 0; i < stickPositions.Length; i++)
		{
			stickPositions[i] = Custom.RNV() * Random.value;
		}
		Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lantern.Update(eu);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		lantern.InitiateSprites(sLeaser, rCam);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		lantern.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		lantern.ApplyPalette(sLeaser, rCam, palette);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		lantern.AddToContainer(sLeaser, rCam, newContatiner);
	}

	Vector2 IProvideWarmth.Position()
	{
		return lantern.firstChunk.pos;
	}
}
