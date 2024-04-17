using System.Collections.Generic;
using UnityEngine;

namespace Smoke;

public class SmokePool : UpdatableAndDeletable
{
	public SmokeSystem.SmokeType type;

	public List<SmokeSystem.SmokeSystemParticle> restingParticles;

	public int ID;

	public static int KillCountToDestroyParticle = 600;

	public SmokePool(SmokeSystem.SmokeType type)
	{
		this.type = type;
		restingParticles = new List<SmokeSystem.SmokeSystemParticle>();
		ID = Random.Range(0, 100);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int num = restingParticles.Count - 1; num >= 0; num--)
		{
			if (restingParticles[num].slatedForDeletetion)
			{
				restingParticles.RemoveAt(num);
			}
		}
	}

	public void ParticleToRest(SmokeSystem.SmokeSystemParticle particle)
	{
		restingParticles.Add(particle);
		particle.owner = null;
	}

	public SmokeSystem.SmokeSystemParticle GetParticle()
	{
		for (int i = 0; i < restingParticles.Count; i++)
		{
			if (restingParticles[i].killCounter > 1)
			{
				SmokeSystem.SmokeSystemParticle result = restingParticles[i];
				restingParticles.RemoveAt(i);
				return result;
			}
		}
		return null;
	}
}
