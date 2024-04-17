using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public abstract class BackDecals : Template
{
	public class Pattern : ExtEnum<Pattern>
	{
		public static readonly Pattern SpineRidge = new Pattern("SpineRidge", register: true);

		public static readonly Pattern DoubleSpineRidge = new Pattern("DoubleSpineRidge", register: true);

		public static readonly Pattern RandomBackBlotch = new Pattern("RandomBackBlotch", register: true);

		public Pattern(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Vector2[] positions;

	public float top;

	public float bottom;

	public Pattern pattern;

	public BackDecals(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
	}

	public void GeneratePattern(Pattern newPattern)
	{
		pattern = newPattern;
		if (pattern == Pattern.SpineRidge)
		{
			top = Mathf.Lerp(0.07f, 0.3f, Random.value);
			bottom = Mathf.Lerp(0.6f, 1f, Random.value);
			float num = Mathf.Lerp(2.5f, 12f, Random.value);
			int num2 = (int)((bottom - top) * 100f / num);
			positions = new Vector2[num2];
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = new Vector2(0f, Mathf.Lerp(top, bottom, (float)i / (float)(num2 - 1)));
			}
		}
		else if (pattern == Pattern.DoubleSpineRidge)
		{
			top = Mathf.Lerp(0.07f, 0.3f, Random.value);
			bottom = Mathf.Lerp(0.6f, 1f, Random.value);
			if (this is WobblyBackTufts)
			{
				bottom = Mathf.Lerp(bottom, 0.5f, Random.value);
			}
			float num3 = Mathf.Lerp(4.5f, 12f, Random.value);
			int num4 = (int)((bottom - top) * 100f / num3);
			positions = new Vector2[num4 * 2];
			for (int j = 0; j < num4; j++)
			{
				positions[j * 2] = new Vector2(-0.9f, Mathf.Lerp(top, bottom, (float)j / (float)(num4 - 1)));
				positions[j * 2 + 1] = new Vector2(0.9f, Mathf.Lerp(top, bottom, (float)j / (float)(num4 - 1)));
			}
		}
		else if (pattern == Pattern.RandomBackBlotch)
		{
			float value = Random.value;
			int num5 = (int)Mathf.Lerp(Mathf.Lerp(20f, 4f, base.scavGrphs.iVars.scruffy), 40f, Mathf.Lerp(value, Random.value, 0.5f * Random.value));
			positions = new Vector2[num5];
			for (int k = 0; k < num5; k++)
			{
				positions[k] = Custom.RNV();
			}
			top = Mathf.Lerp(0.02f, 0.2f, Random.value);
			bottom = Mathf.Lerp(0.4f, 0.9f, Mathf.Pow(Random.value, 1.5f));
			for (int l = 0; l < num5; l++)
			{
				positions[l].y = Custom.LerpMap(positions[l].y, -1f, 1f, top, bottom);
			}
		}
		List<Vector2> list = new List<Vector2>();
		for (int m = 0; m < positions.Length; m++)
		{
			list.Add(positions[m]);
		}
		IEnumerable<Vector2> source = list.OrderByDescending((Vector2 pet) => pet.y);
		positions = source.ToArray();
	}
}
