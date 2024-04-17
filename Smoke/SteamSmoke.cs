using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class SteamSmoke : SmokeSystem
{
	public class SteamParticle : SpriteSmoke
	{
		private float upForce;

		public float moveDir;

		public FloatRect confines;

		public float intensity;

		public override float ToMidSpeed => 0.4f;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			upForce = UnityEngine.Random.value * 100f / lifeTime;
			moveDir = UnityEngine.Random.value * 360f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!resting)
			{
				moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
				vel *= 0.8f;
				vel += Custom.DegToVec(moveDir) * 1.8f * intensity * life;
				vel.y += 2.8f * intensity * upForce;
				if (room.PointSubmerged(pos))
				{
					pos.y = room.FloatWaterLevel(pos.x);
				}
				if (pos.x < confines.left)
				{
					pos.x = confines.left;
				}
				else if (pos.x > confines.right)
				{
					pos.x = confines.right;
				}
				if (pos.y < confines.bottom)
				{
					pos.y = confines.bottom;
				}
				else if (pos.y > confines.top)
				{
					pos.y = confines.top;
				}
			}
		}

		public override float Rad(int type, float useLife, float useStretched, float timeStacker)
		{
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Sin(useLife * (float)Math.PI), 1f - useLife, 0.7f)), 0.8f);
			return type switch
			{
				0 => Mathf.Lerp(4f, rad, num + useStretched), 
				1 => 1.5f * Mathf.Lerp(2f, rad, num), 
				_ => Mathf.Lerp(4f, rad, num), 
			};
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].shader = room.game.rainWorld.Shaders["Steam"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				for (int i = 0; i < 2; i++)
				{
					sLeaser.sprites[i].alpha = life;
				}
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].color = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), Mathf.Lerp(0.03f, 0.35f, palette.texture.GetPixel(30, 7).r));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer("Water");
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public SteamSmoke(Room room)
		: base(SmokeType.Steam, room, 2, 0f)
	{
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new SteamParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, FloatRect confines, float intensity)
	{
		if (AddParticle(pos, vel, Mathf.Lerp(60f, 180f, UnityEngine.Random.value * intensity)) is SteamParticle steamParticle)
		{
			steamParticle.confines = confines;
			steamParticle.intensity = intensity;
			steamParticle.rad = Mathf.Lerp(108f, 286f, UnityEngine.Random.value) * Mathf.Lerp(0.5f, 1f, intensity);
		}
	}
}
