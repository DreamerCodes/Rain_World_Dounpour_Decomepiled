using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class VoidParticle : CosmeticSprite
{
	public int timer;

	public float lifetime;

	public new Vector2 vel;

	public int segs;

	public VoidParticle(Vector2 position, Vector2 velocity, float lifetime)
	{
		pos = position;
		vel = velocity;
		this.lifetime = lifetime;
		segs = 4;
	}

	public override void Update(bool eu)
	{
		timer++;
		if ((float)timer >= lifetime)
		{
			Destroy();
		}
		pos += vel;
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs, pointyTip: false, customColor: false);
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["VoidWormFin"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = new Vector2(pos.x - camPos.x, pos.y - camPos.y);
		for (int i = 0; i < segs; i++)
		{
			Vector2 normalized = vel.normalized;
			Vector2 vector2 = Custom.PerpendicularVector(normalized) * Math.Max(1f, 4f * (float)(i / (segs - 1)));
			normalized *= Mathf.Max(1f, Mathf.Max(8f, vel.magnitude * 9f) * ((float)timer / lifetime));
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector2);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector2);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized - vector2);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized + vector2);
			vector += normalized + new Vector2(Mathf.Sin((float)timer / 20f * (float)Math.PI * 2f + (float)Math.PI * (float)i) * 3f, Mathf.Cos((float)timer / 20f * (float)Math.PI * 2f + (float)Math.PI * (float)i) * 3f);
		}
		sLeaser.sprites[0].alpha = (float)timer / lifetime;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
