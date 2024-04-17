using RWCustom;
using UnityEngine;

internal class DebugGraphDrawer
{
	private FSprite[] dots;

	private FSprite[] dots2;

	public DebugGraphDrawer()
	{
		FSprite node = new FSprite("pixel")
		{
			scale = 504f,
			color = new Color(0.5f, 0f, 0f),
			x = 300f,
			y = 300f
		};
		FSprite node2 = new FSprite("pixel")
		{
			scale = 500f,
			color = new Color(0f, 0f, 0f),
			x = 300f,
			y = 300f
		};
		Futile.stage.AddChild(node);
		Futile.stage.AddChild(node2);
		dots = new FSprite[500];
		dots2 = new FSprite[500];
		for (int i = 0; i < 500; i++)
		{
			dots[i] = new FSprite("pixel");
			dots[i].scale = 2f;
			dots[i].color = new Color(1f, 0f, 0f);
			dots[i].x = 50f + (float)i;
			Futile.stage.AddChild(dots[i]);
			dots2[i] = new FSprite("pixel");
			dots2[i].scale = 2f;
			dots2[i].color = new Color(0f, 1f, 0f);
			dots2[i].x = 50f + (float)i;
			Futile.stage.AddChild(dots2[i]);
		}
		Update();
	}

	public void Update()
	{
		float k = Mathf.InverseLerp(50f, 550f, Futile.mousePosition.x);
		for (int i = 0; i < 500; i++)
		{
			dots[i].y = 50f + Function((float)i / 500f, k) * 500f;
		}
		for (int j = 0; j < 500; j++)
		{
			dots2[j].y = 50f + Function2((float)j / 500f, k) * 500f;
		}
	}

	private float Function(float x, float k)
	{
		return Custom.SCurve(x, 0.5f + Mathf.InverseLerp(0f, 1000f, Futile.mousePosition.x));
	}

	private float Function2(float x, float k)
	{
		return 1f / (10f + x) * 10f;
	}
}
