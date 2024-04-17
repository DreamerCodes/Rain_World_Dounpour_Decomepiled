using RWCustom;
using UnityEngine;

public class CreatureSpasmer : UpdatableAndDeletable
{
	private Creature crit;

	private int counter;

	private bool allowDead;

	public CreatureSpasmer(Creature crit, bool allowDead, int duration)
	{
		this.crit = crit;
		this.allowDead = allowDead;
		counter = duration;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		counter--;
		if (counter < 1 || crit.Consious || (!allowDead && crit.dead))
		{
			Destroy();
			return;
		}
		Vector2 vector = Custom.RNV();
		for (int i = 0; i < crit.bodyChunks.Length; i++)
		{
			vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
			vector *= Mathf.Min(3f, Random.value * 3f / Mathf.Lerp(crit.bodyChunks[i].mass, 1f, 0.5f)) * Mathf.InverseLerp(0f, 160f, counter);
			crit.bodyChunks[i].pos += vector;
			crit.bodyChunks[i].vel += vector * 0.5f;
		}
		if (crit.graphicsModule == null || crit.graphicsModule.bodyParts == null)
		{
			return;
		}
		for (int j = 0; j < crit.graphicsModule.bodyParts.Length; j++)
		{
			vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
			vector *= Random.value * 2f * Mathf.InverseLerp(0f, 120f, counter);
			crit.graphicsModule.bodyParts[j].pos += vector;
			crit.graphicsModule.bodyParts[j].vel += vector;
			if (crit.graphicsModule.bodyParts[j] is Limb)
			{
				(crit.graphicsModule.bodyParts[j] as Limb).mode = Limb.Mode.Dangle;
			}
		}
	}
}
