using System;
using RWCustom;
using UnityEngine;

public abstract class AirBreatherCreature : Creature
{
	public float lungs = 1f;

	public AirBreatherCreature(AbstractCreature abstrCrit, World world)
		: base(abstrCrit, world)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.dead)
		{
			return;
		}
		if (lungs == 1f)
		{
			if (UnityEngine.Random.value < 1f / 60f && room != null && room.water && base.Submersion == 1f)
			{
				lungs = Mathf.Max(0f, lungs - 1f / base.Template.lungCapacity);
			}
			return;
		}
		if (room == null || !room.water || base.Submersion < 1f)
		{
			lungs = Mathf.Clamp01(lungs + 1f / 30f);
			return;
		}
		lungs = Mathf.Max(-1f, lungs - 1f / base.Template.lungCapacity);
		if (lungs < 0.3f)
		{
			if (UnityEngine.Random.value < Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, lungs) * (float)Math.PI) * 0.5f)
			{
				room.AddObject(new Bubble(base.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 6f, bottomBubble: false, fakeWaterBubble: false));
			}
			if (UnityEngine.Random.value < 0.025f)
			{
				LoseAllGrasps();
			}
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * base.bodyChunks[i].rad * 0.4f * UnityEngine.Random.value * Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, lungs) * (float)Math.PI) + Custom.DegToVec(Mathf.Lerp(-30f, 30f, UnityEngine.Random.value)) * UnityEngine.Random.value * ((i == base.mainBodyChunkIndex) ? 0.4f : 0.2f) * Mathf.Pow(Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, lungs) * (float)Math.PI), 2f);
			}
			if (lungs <= 0f && UnityEngine.Random.value < 0.1f)
			{
				Stun(UnityEngine.Random.Range(0, 18));
			}
			if (lungs < -0.5f && UnityEngine.Random.value < 1f / Custom.LerpMap(lungs, -0.5f, -1f, 90f, 30f))
			{
				Die();
			}
		}
	}
}
