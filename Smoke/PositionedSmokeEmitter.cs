using RWCustom;
using UnityEngine;

namespace Smoke;

public abstract class PositionedSmokeEmitter : MeshSmoke
{
	public float maxParticleDistance;

	public Vector2 lastPos;

	public Vector2 pos;

	private Vector2? setPos;

	public int maxFrameSpawnParticles;

	public bool autoSpawn;

	public void MoveTo(Vector2 moveTo, bool eu)
	{
		if (evenUpdate == eu)
		{
			pos = moveTo;
		}
		else
		{
			setPos = moveTo;
		}
	}

	public PositionedSmokeEmitter(SmokeType smokeType, Room room, Vector2 pos, int connectParticlesTime, float minParticleDistance, bool autoSpawn, float maxParticleDistance, int maxFrameSpawnParticles)
		: base(smokeType, room, connectParticlesTime, minParticleDistance)
	{
		this.pos = pos;
		lastPos = pos;
		this.maxParticleDistance = maxParticleDistance;
		this.maxFrameSpawnParticles = maxFrameSpawnParticles;
		this.autoSpawn = autoSpawn;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		if (!autoSpawn)
		{
			return;
		}
		if (Custom.DistLess(lastPos, pos, maxParticleDistance))
		{
			AddParticle(pos, Custom.RNV() * Random.value * 0.01f, ParticleLifeTime);
			return;
		}
		int num = Custom.IntClamp((int)(Vector2.Distance(lastPos, pos) / maxParticleDistance), 2, maxFrameSpawnParticles);
		for (int i = 0; i < num; i++)
		{
			AddParticle(Vector2.Lerp(lastPos, pos, (float)(i + 1) / (float)num), Custom.RNV() * Random.value * 0.01f, ParticleLifeTime);
		}
	}

	public virtual void EmitWithMyLifeTime(Vector2 addPos, Vector2 vel)
	{
		AddParticle(addPos, vel + Custom.RNV() * Random.value * 0.01f, ParticleLifeTime);
	}
}
