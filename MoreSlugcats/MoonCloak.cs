using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class MoonCloak : PlayerCarryableItem, IDrawable
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public int bites;

	public Vector2[,,] clothPoints;

	private int divs;

	public bool free;

	public bool needsReset;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public bool AutomaticPickUp => true;

	public MoonCloak(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.08f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, 0.1f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 18f, BodyChunkConnection.Type.Normal, 0.9f, 0.3f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.2f;
		collisionLayer = 1;
		base.waterFriction = 0.6f;
		base.buoyancy = 1.1f;
		divs = 11;
		clothPoints = new Vector2[divs, divs, 3];
		needsReset = true;
		if (!(abstractPhysicalObject as AbstractConsumable).isConsumed && !free)
		{
			base.firstChunk.pos = abstractPhysicalObject.Room.realizedRoom.MiddleOfTile((abstractPhysicalObject as AbstractConsumable).pos);
			base.bodyChunks[1].pos = base.firstChunk.pos;
		}
	}

	public override void Update(bool eu)
	{
		if (room.game.MoonHasRobe())
		{
			base.slatedForDeletetion = true;
		}
		if (!AbstrConsumable.isConsumed && !free)
		{
			base.firstChunk.pos = room.MiddleOfTile(AbstrConsumable.pos);
			base.firstChunk.vel *= 0f;
			base.bodyChunks[1].vel = new Vector2(2f, 0f) + Custom.RNV();
		}
		if (needsReset)
		{
			for (int i = 0; i < divs; i++)
			{
				for (int j = 0; j < divs; j++)
				{
					clothPoints[i, j, 1] = base.bodyChunks[0].pos;
					clothPoints[i, j, 0] = base.bodyChunks[0].pos;
					clothPoints[i, j, 2] *= 0f;
				}
			}
			needsReset = false;
		}
		base.Update(eu);
		Vector2 pos = base.firstChunk.pos;
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos);
		Vector2 perp = Custom.PerpendicularVector(vector);
		for (int k = 0; k < divs; k++)
		{
			for (int l = 0; l < divs; l++)
			{
				Mathf.InverseLerp(0f, divs - 1, k);
				float t = Mathf.InverseLerp(0f, divs - 1, l);
				clothPoints[k, l, 1] = clothPoints[k, l, 0];
				clothPoints[k, l, 0] += clothPoints[k, l, 2];
				clothPoints[k, l, 2] *= 0.999f;
				clothPoints[k, l, 2].y -= 0.9f * room.gravity;
				Vector2 vector2 = IdealPosForPoint(k, l, pos, vector, perp);
				Vector3 vector3 = Vector3.Slerp(-vector, Custom.DirVec(base.bodyChunks[1].pos, vector2), t) * 0.02f;
				clothPoints[k, l, 2] += new Vector2(vector3.x, vector3.y);
				float num = Vector2.Distance(clothPoints[k, l, 0], vector2);
				float num2 = Mathf.Lerp(0f, 9f, t);
				Vector2 vector4 = Custom.DirVec(clothPoints[k, l, 0], vector2);
				if (num > num2)
				{
					clothPoints[k, l, 0] -= (num2 - num) * vector4;
					clothPoints[k, l, 2] -= (num2 - num) * vector4;
				}
				for (int m = 0; m < 4; m++)
				{
					IntVector2 intVector = new IntVector2(k, l) + Custom.fourDirections[m];
					if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < divs && intVector.y < divs)
					{
						num = Vector2.Distance(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
						vector4 = Custom.DirVec(clothPoints[k, l, 0], clothPoints[intVector.x, intVector.y, 0]);
						float num3 = Vector2.Distance(vector2, IdealPosForPoint(intVector.x, intVector.y, pos, vector, perp));
						clothPoints[k, l, 2] -= (num3 - num) * vector4 * 0.05f;
						clothPoints[intVector.x, intVector.y, 2] += (num3 - num) * vector4 * 0.05f;
					}
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("MoonCloakTex", divs - 1);
		for (int i = 0; i < divs; i++)
		{
			for (int j = 0; j < divs; j++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[j * divs + i] = Color((float)i / (float)(divs - 1));
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < divs; i++)
		{
			for (int j = 0; j < divs; j++)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * divs + j, Vector2.Lerp(clothPoints[i, j, 1], clothPoints[i, j, 0], timeStacker) - camPos);
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

	public void ThrowByPlayer()
	{
	}

	public Color Color(float f)
	{
		return Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f)));
	}

	private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
	{
		float num = Mathf.InverseLerp(0f, divs - 1, x);
		float num2 = Mathf.InverseLerp(0f, divs - 1, y);
		perp *= Mathf.Lerp(0.6f, 1.1f, num2);
		if (num2 < 0.1f)
		{
			perp *= 0f;
		}
		return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(5f, 11f, num2) + dir * Mathf.Lerp(5f, -18f, num2) * (1f + Mathf.Sin((float)Math.PI * num) * 0.35f * Mathf.Lerp(-1f, 1f, num2));
	}

	public override void PickedUp(Creature upPicker)
	{
		base.PickedUp(upPicker);
		AbstrConsumable.minCycles = 9999999;
		AbstrConsumable.maxCycles = 9999999;
		AbstrConsumable.Consume();
	}
}
