using System.Collections.Generic;
using UnityEngine;

namespace SplashWater;

public class WaterJet
{
	private List<JetWater> particles;

	private List<JetWater> restingParticles;

	public Room room;

	private int counter;

	public bool Dead
	{
		get
		{
			if (particles.Count == 0 && restingParticles.Count == 0)
			{
				return counter > 20;
			}
			return false;
		}
	}

	public WaterJet(Room room)
	{
		particles = new List<JetWater>();
		restingParticles = new List<JetWater>();
		this.room = room;
	}

	public void Update()
	{
		counter++;
		for (int num = particles.Count - 1; num >= 0; num--)
		{
			if (particles[num].slatedForDeletetion || particles[num].goToRest)
			{
				restingParticles.Add(particles[num]);
				particles.RemoveAt(num);
			}
		}
		for (int num2 = restingParticles.Count - 1; num2 >= 0; num2--)
		{
			if (restingParticles[num2].slatedForDeletetion)
			{
				restingParticles.RemoveAt(num2);
			}
		}
	}

	public void NewParticle(Vector2 emissionPoint, Vector2 emissionForce, float amount, float initRad)
	{
		JetWater jetWater;
		if (restingParticles.Count > 0 && restingParticles[0].killCounter > 1)
		{
			jetWater = restingParticles[0];
			restingParticles.RemoveAt(0);
		}
		else
		{
			jetWater = new JetWater(this);
			room.AddObject(jetWater);
		}
		particles.Add(jetWater);
		jetWater.Reset(emissionPoint, emissionForce, amount, initRad);
		if (counter < 2 && particles.Count > 2)
		{
			jetWater.otherParticle = particles[particles.Count - 2];
		}
		counter = 0;
	}
}
