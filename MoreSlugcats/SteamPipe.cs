using Smoke;
using UnityEngine;

namespace MoreSlugcats;

public class SteamPipe : UpdatableAndDeletable, IProvideWarmth
{
	private Vector2 direction;

	private float intensity;

	private SteamSmoke mySteam;

	public Vector2 pos;

	private int steamCounter;

	private float burst;

	private float timeBurst;

	private bool wallSteamer;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth * Mathf.InverseLerp(0.2f, 0.35f, intensity) * Mathf.InverseLerp(burst * 60f, 0f, steamCounter);

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 400f * intensity;

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (mySteam == null)
		{
			mySteam = new SteamSmoke(room);
			room.AddObject(mySteam);
		}
		float num = Mathf.Clamp(intensity, 0.01f, 1f);
		if (wallSteamer)
		{
			num *= 1f - Mathf.Clamp((float)room.game.world.rainCycle.TimeUntilRain / 1000f, 0f, 1f);
		}
		if (steamCounter > 0)
		{
			steamCounter++;
			if (wallSteamer)
			{
				steamCounter++;
			}
			if (steamCounter > (int)((200f + intensity * 800f) * timeBurst))
			{
				steamCounter = 0;
			}
			return;
		}
		if (intensity < 0.25f)
		{
			burst = Random.Range(0f, 0.1f);
		}
		else if (intensity < 0.5f)
		{
			burst = Random.Range(0.1f, 0.2f);
		}
		else
		{
			burst = Random.Range(0.2f, 0.35f);
		}
		timeBurst = Random.Range(0.2f, 0.35f);
		if (num > 0f)
		{
			if (room.PointSubmerged(pos))
			{
				for (int i = 0; (float)i < Random.Range(10f, 30f); i++)
				{
					room.AddObject(new Bubble(pos, direction, Random.Range(0f, 1f) < 0.2f, fakeWaterBubble: false));
				}
			}
			else if (room.game.cameras[0].room == room && Vector2.Distance(room.cameraPositions[room.game.cameras[0].currentCameraPosition], pos) < 1500f)
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					room.PlaySound(SoundID.Gate_Electric_Steam_Puff, pos, num / 3f + burst, Random.Range(0.45f, 1.4f));
				}
				else
				{
					room.PlaySound(SoundID.Gate_Water_Steam_Puff, pos, num / 3f + burst, Random.Range(0.45f, 1.4f));
				}
				mySteam.EmitSmoke(pos, Vector2.Scale(direction.normalized, new Vector2(intensity, intensity)), new FloatRect(pos.x - 150f, pos.y - 150f, pos.x + 150f, pos.y + 150f), num + burst);
			}
		}
		steamCounter = 1;
	}

	public SteamPipe(Vector2 pos, Vector2 direction, float intensity, bool isWallSteamer)
	{
		this.direction = direction;
		this.intensity = intensity;
		this.pos = pos;
		wallSteamer = isWallSteamer;
	}

	Vector2 IProvideWarmth.Position()
	{
		return pos;
	}
}
