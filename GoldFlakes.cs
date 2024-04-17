using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class GoldFlakes : UpdatableAndDeletable
{
	public class GoldFlake : CosmeticSprite
	{
		private float scale;

		private float rot;

		private float lastRot;

		private float yRot;

		private float lastYRot;

		private float rotSpeed;

		private float yRotSpeed;

		private float velRotAdd;

		public int savedCamPos;

		public bool reset;

		public bool active;

		public GoldFlake()
		{
			savedCamPos = -1;
			ResetMe();
		}

		public override void Update(bool eu)
		{
			if (!active)
			{
				savedCamPos = -1;
				return;
			}
			base.Update(eu);
			vel *= 0.82f;
			vel.y -= 0.25f;
			vel += Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, UnityEngine.Random.value)) * 0.1f;
			vel += Custom.DegToVec(rot + velRotAdd + yRot) * Mathf.Lerp(0.1f, 0.25f, UnityEngine.Random.value);
			if (room.GetTile(pos).Solid && room.GetTile(lastPos).Solid)
			{
				reset = true;
			}
			if (reset)
			{
				pos = room.game.cameras[0].pos + new Vector2(Mathf.Lerp(-20f, 1386f, UnityEngine.Random.value), 868f);
				lastPos = pos;
				ResetMe();
				reset = false;
				vel *= 0f;
				return;
			}
			if (pos.x < room.game.cameras[0].pos.x - 20f)
			{
				reset = true;
			}
			if (pos.x > room.game.cameras[0].pos.x + 1366f + 20f)
			{
				reset = true;
			}
			if (pos.y < room.game.cameras[0].pos.y - 200f)
			{
				reset = true;
			}
			if (pos.y > room.game.cameras[0].pos.y + 768f + 200f)
			{
				reset = true;
			}
			if (room.game.cameras[0].currentCameraPosition != savedCamPos)
			{
				PlaceRandomlyInRoom();
				savedCamPos = room.game.cameras[0].currentCameraPosition;
			}
			if (!room.BeingViewed)
			{
				Destroy();
			}
			lastRot = rot;
			rot += rotSpeed;
			rotSpeed = Mathf.Clamp(rotSpeed + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 30f, -10f, 10f);
			lastYRot = yRot;
			yRot += yRotSpeed;
			yRotSpeed = Mathf.Clamp(yRotSpeed + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 320f, -0.05f, 0.05f);
		}

		public void PlaceRandomlyInRoom()
		{
			ResetMe();
			pos = room.game.cameras[0].pos + new Vector2(Mathf.Lerp(-20f, 1386f, UnityEngine.Random.value), Mathf.Lerp(-200f, 968f, UnityEngine.Random.value));
			lastPos = pos;
		}

		public void ResetMe()
		{
			velRotAdd = UnityEngine.Random.value * 360f;
			vel = Custom.RNV();
			scale = UnityEngine.Random.value;
			rot = UnityEngine.Random.value * 360f;
			lastRot = rot;
			rotSpeed = Mathf.Lerp(2f, 10f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			yRot = UnityEngine.Random.value * (float)Math.PI;
			lastYRot = yRot;
			yRotSpeed = Mathf.Lerp(0.02f, 0.05f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Pebble" + UnityEngine.Random.Range(1, 15));
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].isVisible = active && !reset;
			if (active)
			{
				float t = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DegToVec(45f), Custom.DegToVec(Mathf.Lerp(lastYRot, yRot, timeStacker) * 57.29578f + Mathf.Lerp(lastRot, rot, timeStacker))));
				float ghostMode = rCam.ghostMode;
				Color a = Custom.HSL2RGB(0.08611111f, 0.65f, Mathf.Lerp(0.53f, 0f, ghostMode));
				Color b = Custom.HSL2RGB(0.08611111f, Mathf.Lerp(1f, 0.65f, ghostMode), Mathf.Lerp(1f, 0.53f, ghostMode));
				sLeaser.sprites[0].color = Color.Lerp(a, b, t);
				sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
				sLeaser.sprites[0].scaleX = Mathf.Lerp(0.25f, 0.45f, scale) * Mathf.Sin(Mathf.Lerp(lastYRot, yRot, timeStacker) * (float)Math.PI);
				sLeaser.sprites[0].scaleY = Mathf.Lerp(0.35f, 0.65f, scale);
				sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
				base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			}
		}
	}

	public List<GoldFlake> flakes;

	private int savedCamPos = -1;

	public GoldFlakes(Room room)
	{
		float num = 0f;
		for (int i = 0; i < room.cameraPositions.Length; i++)
		{
			num = Mathf.Max(num, room.world.worldGhost.GhostMode(room, i));
		}
		flakes = new List<GoldFlake>();
		for (int j = 0; j < NumberOfFlakes(num); j++)
		{
			GoldFlake goldFlake = new GoldFlake();
			flakes.Add(goldFlake);
			room.AddObject(goldFlake);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.cameras[0].room == room && room.game.cameras[0].currentCameraPosition != savedCamPos)
		{
			savedCamPos = room.game.cameras[0].currentCameraPosition;
			int num = NumberOfFlakes(room.world.worldGhost.GhostMode(room, savedCamPos));
			for (int i = 0; i < flakes.Count; i++)
			{
				if (i <= num)
				{
					flakes[i].active = true;
					flakes[i].PlaceRandomlyInRoom();
					flakes[i].savedCamPos = savedCamPos;
					flakes[i].reset = false;
				}
				else
				{
					flakes[i].active = false;
				}
			}
		}
		if (!room.BeingViewed)
		{
			for (int j = 0; j < flakes.Count; j++)
			{
				flakes[j].Destroy();
			}
			Destroy();
		}
	}

	private int NumberOfFlakes(float ghostMode)
	{
		return (int)(200f * Mathf.Pow(ghostMode, 2f));
	}
}
