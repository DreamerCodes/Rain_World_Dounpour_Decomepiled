using RWCustom;
using UnityEngine;

public abstract class InsectoidCreature : AirBreatherCreature
{
	public float poison;

	public InsectoidCreature(AbstractCreature abstrCrit, World world)
		: base(abstrCrit, world)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.dead || !(poison > 0f) || room == null || !room.readyForAI)
		{
			return;
		}
		if (poison > 0.25f)
		{
			poison = Mathf.Clamp01(poison + 0.0025f);
		}
		else
		{
			poison = Mathf.Clamp01(poison - 0.0016666667f);
		}
		if (!(Random.value < Mathf.Max(0.2f, poison * 0.5f)) || !(Random.value < poison))
		{
			return;
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (room.aimap.TileAccessibleToCreature(base.bodyChunks[i].pos, base.Template))
			{
				base.bodyChunks[i].vel += Custom.RNV() * Random.value * base.bodyChunks[i].rad * (0.5f + 0.5f * poison);
			}
		}
		if (Random.value < 0.5f)
		{
			Stun(Random.Range(2, (int)Mathf.Lerp(8f, 16f, poison)));
		}
		else if (base.State is HealthState)
		{
			(base.State as HealthState).health -= Random.value * poison / (16f * Mathf.Max(1f, base.TotalMass));
		}
		if (poison >= 1f && !(base.State is HealthState))
		{
			Die();
		}
	}
}
