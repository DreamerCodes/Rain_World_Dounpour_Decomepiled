using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class HangingPearlString : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public List<AbstractConsumable> pearls;

	public List<float> connectionRads;

	public List<bool> activeConnections;

	private Vector2 AttachedPos => pos;

	public HangingPearlString(Room room, float length, Vector2 pos)
	{
		this.pos = pos;
		Vector2 attachedPos = AttachedPos;
		pearls = new List<AbstractConsumable>();
		connectionRads = new List<float>();
		activeConnections = new List<bool>();
		Random.State state = Random.state;
		Random.InitState((int)this.pos.x * (int)this.pos.y);
		float num = Mathf.Lerp(40f, 70f, Random.value);
		float num2 = 0f;
		for (int i = 0; (float)i < length; i++)
		{
			if (Random.value < 0.5f || i == 1)
			{
				num = Mathf.Lerp(5f, 40f, Random.value);
			}
			num2 += num;
			attachedPos.y -= num;
			DataPearl.AbstractDataPearl item = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, room.GetWorldCoordinate(attachedPos), room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc)
			{
				destroyOnAbstraction = true
			};
			room.abstractRoom.entities.Add(item);
			pearls.Add(item);
			connectionRads.Add(num);
			activeConnections.Add(item: true);
			if (num2 > length)
			{
				break;
			}
		}
		Random.state = state;
	}

	public void Initiate()
	{
		for (int i = 0; i < pearls.Count; i++)
		{
			if (pearls[i].realizedObject != null)
			{
				pearls[i].realizedObject.ChangeCollisionLayer(0);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.shortCutsReady)
		{
			return;
		}
		for (int i = 0; i < pearls.Count; i++)
		{
			if (pearls[i].realizedObject == null || pearls[i].realizedObject.grabbedBy.Count > 0)
			{
				activeConnections[i] = false;
			}
		}
		for (int j = 1; j < pearls.Count; j++)
		{
			if (activeConnections[j] && activeConnections[j - 1])
			{
				float num = Vector2.Distance(PearlPos(j), PearlPos(j - 1));
				if (num > connectionRads[j])
				{
					Vector2 normalized = (PearlPos(j) - PearlPos(j - 1)).normalized;
					pearls[j].realizedObject.firstChunk.pos += normalized * (connectionRads[j] - num) * 0.5f;
					pearls[j].realizedObject.firstChunk.vel += normalized * (connectionRads[j] - num) * 0.5f;
					pearls[j - 1].realizedObject.firstChunk.pos -= normalized * (connectionRads[j] - num) * 0.5f;
					pearls[j - 1].realizedObject.firstChunk.vel -= normalized * (connectionRads[j] - num) * 0.5f;
				}
			}
		}
		Attach();
		for (int num2 = pearls.Count - 2; num2 >= 0; num2--)
		{
			if (activeConnections[num2] && activeConnections[num2 + 1])
			{
				float num3 = Vector2.Distance(PearlPos(num2), PearlPos(num2 + 1));
				if (num3 > connectionRads[num2])
				{
					Vector2 normalized2 = (PearlPos(num2) - PearlPos(num2 + 1)).normalized;
					pearls[num2].realizedObject.firstChunk.pos += normalized2 * (connectionRads[num2] - num3) * 0.5f;
					pearls[num2].realizedObject.firstChunk.vel += normalized2 * (connectionRads[num2] - num3) * 0.5f;
					pearls[num2 + 1].realizedObject.firstChunk.pos -= normalized2 * (connectionRads[num2] - num3) * 0.5f;
					pearls[num2 + 1].realizedObject.firstChunk.vel -= normalized2 * (connectionRads[num2] - num3) * 0.5f;
				}
			}
		}
		Attach();
	}

	private Vector2 PearlPos(int i)
	{
		return pearls[i].realizedObject.firstChunk.pos;
	}

	private void Attach()
	{
		if (activeConnections[0])
		{
			Vector2 normalized = (pearls[0].realizedObject.firstChunk.pos - AttachedPos).normalized;
			float num = Vector2.Distance(pearls[0].realizedObject.firstChunk.pos, AttachedPos);
			pearls[0].realizedObject.firstChunk.pos += normalized * (connectionRads[0] - num);
			pearls[0].realizedObject.firstChunk.vel += normalized * (connectionRads[0] - num);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[pearls.Count];
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = AttachedPos;
		bool isVisible = true;
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (!activeConnections[i] || pearls[i].realizedObject == null || pearls[i].realizedObject.room != room)
			{
				sLeaser.sprites[i].isVisible = false;
				isVisible = false;
				continue;
			}
			sLeaser.sprites[i].isVisible = isVisible;
			isVisible = true;
			Vector2 vector2 = Vector2.Lerp(pearls[i].realizedObject.firstChunk.lastPos, pearls[i].realizedObject.firstChunk.pos, timeStacker);
			sLeaser.sprites[i].x = vector2.x - camPos.x;
			sLeaser.sprites[i].y = vector2.y - camPos.y;
			sLeaser.sprites[i].scaleY = Vector2.Distance(vector2, vector);
			sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
			vector = vector2;
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
}
