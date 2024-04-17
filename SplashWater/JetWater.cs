using RWCustom;
using UnityEngine;

namespace SplashWater;

public class JetWater : WaterParticle
{
	public WaterJet jet;

	public JetWater op;

	public Vector2 lingerPos;

	public Vector2 lastLingerPos;

	public Vector2 lingerPosVel;

	private Vector2 lingerOtherDir;

	private float lingerOtherRad;

	private float lingerOpacity;

	private float lastLingerOpacity;

	public bool goToRest;

	public int killCounter;

	public JetWater otherParticle
	{
		get
		{
			if (op != null && op.goToRest)
			{
				op = null;
			}
			return op;
		}
		set
		{
			op = value;
		}
	}

	public JetWater(WaterJet jet)
	{
	}

	public override void Reset(Vector2 pos, Vector2 vel, float amount, float initRad)
	{
		base.Reset(pos, vel, amount, initRad);
		lingerPos = pos;
		lastLingerPos = lingerPos;
		lingerPosVel = vel;
		lingerOtherDir = vel.normalized;
		lingerOtherRad = initRad;
		lingerOpacity = 1f;
		lastLingerOpacity = 1f;
		killCounter = 0;
		goToRest = false;
	}

	public override void Update(bool eu)
	{
		if (lastLife < 0f)
		{
			goToRest = true;
			lastLingerPos = lingerPos;
			lastPos = pos;
			lastLife = life;
			lastRad = rad;
			otherParticle = null;
		}
		if (goToRest)
		{
			killCounter++;
			if (killCounter == 100)
			{
				Destroy();
			}
			return;
		}
		if (room.PointSubmerged(pos))
		{
			life -= 0.02f;
			room.waterObject.WaterfallHitSurface(pos.x - 5f, pos.x + 5f, Mathf.InverseLerp(-2f, -8f, vel.y));
			vel.y = Mathf.Abs(vel.y) * 0.4f;
			rad = (rad + 2f) * 1.5f;
			if (makeSoundCounter <= 0 && vel.magnitude > 4f)
			{
				room.PlaySound(SoundID.Splashing_Water_Into_Water_Surface, pos, Mathf.InverseLerp(4f, 14f, vel.magnitude), 1f);
				makeSoundCounter = int.MaxValue;
			}
		}
		else
		{
			vel.y -= 0.9f;
		}
		base.Update(eu);
		lastLingerPos = lingerPos;
		if (otherParticle != null)
		{
			Vector2 b = Vector2.Lerp(vel, otherParticle.vel, 0.5f);
			vel = Vector2.Lerp(vel, b, 0.1f);
			otherParticle.vel = Vector2.Lerp(otherParticle.vel, b, 0.1f);
			lastLingerPos = otherParticle.lastPos;
			lingerPos = otherParticle.pos;
			lingerPosVel = otherParticle.vel;
			lastLingerOpacity = otherParticle.Opactiy(0f);
			lingerOpacity = otherParticle.Opactiy(1f);
			if (Random.value < 0.025f)
			{
				otherParticle = null;
			}
		}
		else
		{
			lingerPos += lingerPosVel;
			if (room.PointSubmerged(lingerPos))
			{
				lingerPos.y = room.FloatWaterLevel(lingerPos.x);
			}
			lingerPosVel.y -= 0.9f;
			lastLingerOpacity = lingerOpacity;
			lingerOpacity = Mathf.Max(lingerOpacity - 0.025f, 0f);
		}
	}

	public Vector2 OtherPos(float timeStacker)
	{
		if (otherParticle != null)
		{
			lastLingerPos = otherParticle.lastPos;
			lingerPos = otherParticle.pos;
			lingerPosVel = otherParticle.vel;
		}
		return Vector2.Lerp(lastLingerPos, lingerPos, timeStacker);
	}

	private Vector2 OtherDir(float timeStacker)
	{
		if (otherParticle != null)
		{
			lingerOtherDir = Custom.DirVec(Vector2.Lerp(otherParticle.lastPos, otherParticle.pos, timeStacker), otherParticle.OtherPos(timeStacker));
		}
		return lingerOtherDir;
	}

	public float Rad(float timeStacker)
	{
		return Mathf.Lerp(lastRad, rad, timeStacker);
	}

	private float OtherRad(float timeStacker)
	{
		if (otherParticle != null)
		{
			lingerOtherRad = otherParticle.Rad(timeStacker);
		}
		return lingerOtherRad;
	}

	public float Opactiy(float timeStacker)
	{
		if (goToRest)
		{
			return 0f;
		}
		return Mathf.InverseLerp(amount * 2f, amount / 2f, Mathf.Lerp(lastRad, rad, timeStacker)) * Mathf.Pow(1f - Mathf.Lerp(lastLife, life, timeStacker), 0.1f) / Mathf.Lerp(Mathf.Max(1f, Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), OtherPos(timeStacker)) - 40f), 1f, 0.95f);
	}

	public float OtherOpacity(float timeStacker)
	{
		if (goToRest)
		{
			return 0f;
		}
		if (otherParticle != null)
		{
			lingerOpacity = otherParticle.Opactiy(timeStacker);
			lastLingerOpacity = lingerOpacity;
			return lingerOpacity;
		}
		return Mathf.Lerp(lastLingerOpacity, lingerOpacity, timeStacker);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[4]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(2, 3, 4),
				new TriangleMesh.Triangle(3, 4, 5)
			};
			sLeaser.sprites[i] = new TriangleMesh("Futile_White", tris, customColor: true);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[0] = new Vector2(0f, 0f);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[1] = new Vector2(1f, 0f);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[2] = new Vector2(0f, 0.5f);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[3] = new Vector2(1f, 0.5f);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[4] = new Vector2(0f, 1f);
			(sLeaser.sprites[i] as TriangleMesh).UVvertices[5] = new Vector2(1f, 1f);
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["WaterSplash"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 vector2 = OtherPos(timeStacker);
		Vector2 vector3 = Custom.DirVec(vector, vector2);
		Vector2 vector4 = Custom.PerpendicularVector(vector3);
		float num = Rad(timeStacker);
		float num2 = OtherRad(timeStacker);
		Vector2 vector5 = Custom.PerpendicularVector(OtherDir(timeStacker));
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector6 = new Vector2(-1.5f, 1.5f) * i;
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(0, vector - vector4 * num - camPos + vector6);
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(1, vector + vector4 * num - camPos + vector6);
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(2, vector - vector4 * (num + num2) * 0.5f + vector3 * Vector2.Distance(vector, vector2) * 0.5f - camPos + vector6);
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(3, vector + vector4 * (num + num2) * 0.5f + vector3 * Vector2.Distance(vector, vector2) * 0.5f - camPos + vector6);
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(4, vector2 - vector5 * num2 - camPos + vector6);
			(sLeaser.sprites[i] as TriangleMesh).MoveVertice(5, vector2 + vector5 * num2 - camPos + vector6);
			for (int j = 0; j < 4; j++)
			{
				(sLeaser.sprites[i] as TriangleMesh).verticeColors[j] = new Color(1f, 1f, i, Opactiy(timeStacker));
			}
			for (int k = 4; k < 6; k++)
			{
				(sLeaser.sprites[i] as TriangleMesh).verticeColors[k] = new Color(1f, 1f, i, OtherOpacity(timeStacker));
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
	}
}
