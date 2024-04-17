using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public abstract class OracleSwarmer : PhysicalObject, IDrawable, IPlayerEdible
{
	public List<OracleSwarmer> otherSwarmers;

	private bool hasFoundOthers;

	public Vector2 drift;

	public int bites = 3;

	public int waitToFindOthers;

	public bool lastVisible;

	public Vector2 direction;

	public Vector2 lastDirection;

	public Vector2 lazyDirection;

	public Vector2 lastLazyDirection;

	public float rotation;

	public float lastRotation;

	public float revolveSpeed;

	public float affectedByGravity = 1f;

	public int ping;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public virtual bool Edible => true;

	public bool AutomaticPickUp => false;

	public OracleSwarmer(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		otherSwarmers = new List<OracleSwarmer>(200);
		collisionLayer = ((this is SLOracleSwarmer) ? 1 : 0);
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 3f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.1f;
		lastVisible = true;
		rotation = 0.25f;
		lastRotation = rotation;
	}

	public virtual void ExplodeSwarmer()
	{
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		hasFoundOthers = false;
	}

	protected virtual void UpdateOtherSwarmers()
	{
		for (int i = 0; i < otherSwarmers.Count; i++)
		{
			otherSwarmers[i].otherSwarmers.Remove(this);
		}
		otherSwarmers.Clear();
		for (int j = 0; j < room.physicalObjects[collisionLayer].Count; j++)
		{
			if (room.physicalObjects[collisionLayer][j] is OracleSwarmer && room.physicalObjects[collisionLayer][j].abstractPhysicalObject.type == abstractPhysicalObject.type && room.physicalObjects[collisionLayer][j] != this)
			{
				if (!(room.physicalObjects[collisionLayer][j] as OracleSwarmer).otherSwarmers.Contains(this))
				{
					(room.physicalObjects[collisionLayer][j] as OracleSwarmer).otherSwarmers.Add(this);
				}
				if (!otherSwarmers.Contains(room.physicalObjects[collisionLayer][j] as OracleSwarmer))
				{
					otherSwarmers.Add(room.physicalObjects[collisionLayer][j] as OracleSwarmer);
				}
			}
		}
	}

	public override void Update(bool eu)
	{
		if (!hasFoundOthers)
		{
			if (waitToFindOthers > 0)
			{
				waitToFindOthers--;
			}
			else
			{
				UpdateOtherSwarmers();
				hasFoundOthers = true;
			}
		}
		base.firstChunk.vel.y -= room.gravity * affectedByGravity;
		lastDirection = direction;
		lastLazyDirection = lazyDirection;
		lazyDirection = Vector3.Slerp(lazyDirection, direction, 0.06f);
		lastRotation = rotation;
		rotation += revolveSpeed;
		if (room.gravity * affectedByGravity > 0.5f)
		{
			if (base.firstChunk.ContactPoint.y < 0)
			{
				direction = Vector3.Slerp(direction, new Vector2(Mathf.Sign(direction.x), 0f), 0.4f);
				revolveSpeed *= 0.8f;
			}
			else if (grabbedBy.Count > 0)
			{
				direction = Custom.PerpendicularVector(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos) * ((grabbedBy[0].graspUsed != 0) ? 1 : (-1));
			}
			else
			{
				direction = Vector3.Slerp(direction, Custom.DirVec(base.firstChunk.lastLastPos, base.firstChunk.pos), 0.4f);
			}
			revolveSpeed *= 0.5f;
			rotation = Mathf.Lerp(rotation, Mathf.Floor(rotation) + 0.25f, Mathf.InverseLerp(0.5f, 1f, room.gravity * affectedByGravity) * 0.1f);
		}
		base.Update(eu);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[0].scale = 1.5f;
		sLeaser.sprites[0].alpha = 0.2f;
		sLeaser.sprites[1] = new FSprite("JetFishEyeA");
		sLeaser.sprites[1].scaleY = 1.2f;
		sLeaser.sprites[1].scaleX = 0.75f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[2 + i] = new FSprite("deerEyeA2");
			sLeaser.sprites[2 + i].anchorX = 0f;
		}
		sLeaser.sprites[4] = new FSprite("JetFishEyeB");
		sLeaser.sprites[4].color = new Color(0.5f, 0.5f, 0.5f);
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		bool flag = rCam.room.ViewedByAnyCamera(pos, 48f);
		if (flag != lastVisible)
		{
			for (int i = 0; i <= 4; i++)
			{
				sLeaser.sprites[i].isVisible = flag;
			}
			lastVisible = flag;
		}
		if (flag)
		{
			Vector2 vector = Vector3.Slerp(lastDirection, direction, timeStacker);
			Vector2 vector2 = Vector3.Slerp(lastLazyDirection, lazyDirection, timeStacker);
			Vector3 vector3 = Custom.PerpendicularVector(vector);
			float num = Mathf.Sin(Mathf.Lerp(lastRotation, rotation, timeStacker) * (float)Math.PI * 2f);
			float num2 = Mathf.Cos(Mathf.Lerp(lastRotation, rotation, timeStacker) * (float)Math.PI * 2f);
			sLeaser.sprites[0].x = pos.x - camPos.x;
			sLeaser.sprites[0].y = pos.y - camPos.y;
			sLeaser.sprites[1].x = pos.x - camPos.x;
			sLeaser.sprites[1].y = pos.y - camPos.y;
			sLeaser.sprites[4].x = pos.x + vector3.x * 2f * num2 * Mathf.Sign(num) - camPos.x;
			sLeaser.sprites[4].y = pos.y + vector3.y * 2f * num2 * Mathf.Sign(num) - camPos.y;
			sLeaser.sprites[1].rotation = Custom.VecToDeg(vector);
			sLeaser.sprites[4].rotation = Custom.VecToDeg(vector);
			sLeaser.sprites[4].scaleX = 1f - Mathf.Abs(num2);
			sLeaser.sprites[1].isVisible = true;
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[2 + j].x = pos.x - vector.x * 4f - camPos.x;
				sLeaser.sprites[2 + j].y = pos.y - vector.y * 4f - camPos.y;
				sLeaser.sprites[2 + j].rotation = Custom.VecToDeg(vector2) + 90f + ((j == 0) ? (-1f) : 1f) * Custom.LerpMap(Vector2.Distance(vector, vector2), 0.06f, 0.7f, 10f, 45f, 2f) * num;
			}
			sLeaser.sprites[2].scaleY = -1f * num;
			sLeaser.sprites[3].scaleY = num;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = new Color(1f, 1f, 1f);
		sLeaser.sprites[0].color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.8f);
		sLeaser.sprites[1].color = color;
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
			if (i == 0)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Swarmer : SoundID.Slugcat_Bite_Swarmer, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites >= 1)
		{
			return;
		}
		(grasp.grabber as Player).ObjectEaten(this);
		if (!ModManager.MSC || !(grasp.grabber as Player).isNPC)
		{
			if (room.game.session is StoryGameSession)
			{
				(room.game.session as StoryGameSession).saveState.theGlow = true;
			}
		}
		else
		{
			((grasp.grabber as Player).State as PlayerNPCState).Glowing = true;
		}
		(grasp.grabber as Player).glowing = true;
		grasp.Release();
		Destroy();
	}

	public void ThrowByPlayer()
	{
	}
}
