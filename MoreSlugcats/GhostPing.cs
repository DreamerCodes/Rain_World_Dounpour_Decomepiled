using System;
using UnityEngine;

namespace MoreSlugcats;

public class GhostPing : UpdatableAndDeletable, IDrawable
{
	public int counter;

	public int goAt;

	public float prog;

	public float lastProg;

	public float speed;

	public float alpha;

	public bool go;

	public GhostPing(Room room)
	{
		base.room = room;
		go = false;
		goAt = 40;
		speed = 0.0125f;
		alpha = 0.5f;
		if (!base.room.abstractRoom.shelter)
		{
			goAt = 80;
			speed = 0.04f;
			alpha = 0.3f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.BeingViewed)
		{
			if (go)
			{
				Destroy();
			}
			return;
		}
		counter++;
		if (!go && counter >= goAt)
		{
			go = true;
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Ghost_Ping_Start, 0f, 0.6f, 1f + UnityEngine.Random.value * 0.5f);
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Ghost_Ping_Base, 0f, 1f, 1f);
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Ghost_Ping_Base, 0f, 1f, 1f);
		}
		lastProg = prog;
		if (go)
		{
			prog = Mathf.Min(1f, prog + speed);
			if (prog >= 1f && lastProg >= 1f)
			{
				Destroy();
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].scaleX = 87.5f;
		sLeaser.sprites[0].scaleY = 50f;
		sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
		sLeaser.sprites[0].y = rCam.game.rainWorld.screenSize.y / 2f;
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LevelMelt2"];
		sLeaser.sprites[0].color = new Color(1f, 0f, 0f);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Sin(Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastProg, prog, timeStacker)), 2f) * (float)Math.PI);
		if (num == 0f)
		{
			sLeaser.sprites[0].isVisible = false;
			return;
		}
		sLeaser.sprites[0].isVisible = true;
		sLeaser.sprites[0].alpha = 0.8f * num * alpha;
		rCam.ghostMode = num * alpha;
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
