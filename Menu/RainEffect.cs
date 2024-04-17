using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class RainEffect : MenuObject
{
	public class RainDrop
	{
		private RainEffect owner;

		public int index;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 lastLastPos;

		public float depth;

		public RainDrop(RainEffect owner, int index)
		{
			this.owner = owner;
			this.index = index;
			Reset();
			pos = new Vector2(1366f * UnityEngine.Random.value, 900f * UnityEngine.Random.value);
			lastPos = pos + new Vector2(0f, 300f) / depth;
			lastLastPos = lastPos + new Vector2(0f, 300f) / depth;
		}

		public void Reset()
		{
			pos = new Vector2(Mathf.Lerp(-100f, 1466f, UnityEngine.Random.value), 770f + UnityEngine.Random.value * 100f);
			lastPos = pos;
			lastLastPos = lastPos;
			depth = Mathf.Lerp(0.5f, 7f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
		}

		public void Update()
		{
			lastLastPos = lastPos;
			lastPos = pos;
			pos += (owner.RainDir(Mathf.InverseLerp(0.5f, 7f, depth)) + Custom.RNV() * UnityEngine.Random.value * 14f) / depth;
			if (lastPos.y < 0f)
			{
				Reset();
			}
		}
	}

	public FSprite[] sprites;

	public RainDrop[] drops;

	private Vector2[,] rainDirs;

	public float highlightPos;

	public float lightning;

	public float lastLightning;

	public float lightningIntensity;

	public float LIdropOff = 1f;

	public float extraLightningChance;

	public float rainFade;

	public FSprite bkg;

	public FSprite fadeSprite;

	public FSprite lightsource;

	public Vector2 RainDir(float dp)
	{
		return new Vector2(0f, -300f) + Vector2.Lerp(rainDirs[0, 0], rainDirs[1, 0], dp);
	}

	public RainEffect(Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		bkg = new FSprite("Futile_White");
		bkg.scaleX = 1400f;
		bkg.scaleY = 800f;
		bkg.x = menu.manager.rainWorld.screenSize.x / 2f;
		bkg.y = menu.manager.rainWorld.screenSize.y / 2f;
		Container.AddChild(bkg);
		lightsource = new FSprite("Futile_White");
		lightsource.shader = menu.manager.rainWorld.Shaders["FlatLightNoisy"];
		Container.AddChild(lightsource);
		drops = new RainDrop[500];
		sprites = new FSprite[drops.Length];
		for (int i = 0; i < drops.Length; i++)
		{
			drops[i] = new RainDrop(this, i);
			sprites[i] = new FSprite("pixel");
			sprites[i].anchorY = 0f;
			Container.AddChild(sprites[i]);
		}
		rainDirs = new Vector2[2, 2];
		for (int j = 0; j < 2; j++)
		{
			rainDirs[j, 0] = Custom.RNV() * UnityEngine.Random.value * 20f;
			rainDirs[j, 1] = Custom.RNV() * UnityEngine.Random.value * 10f;
		}
		fadeSprite = new FSprite("Futile_White");
		fadeSprite.x = menu.manager.rainWorld.screenSize.x / 2f;
		fadeSprite.y = menu.manager.rainWorld.screenSize.y / 2f;
		fadeSprite.shader = menu.manager.rainWorld.Shaders["EdgeFade"];
		fadeSprite.color = new Color(0f, 0f, 0f);
		Container.AddChild(fadeSprite);
	}

	public override void Update()
	{
		base.Update();
		lastLightning = lightning;
		for (int i = 0; i < drops.Length; i++)
		{
			drops[i].Update();
		}
		for (int j = 0; j < 2; j++)
		{
			rainDirs[j, 0] += Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 3f) * 5f + rainDirs[j, 1];
			rainDirs[j, 0] = Vector2.ClampMagnitude(rainDirs[j, 0], 30f);
			rainDirs[j, 1] *= 0.9f;
			rainDirs[j, 1] -= rainDirs[j, 0] * 0.04f;
			rainDirs[j, 1] += Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 15f) * 5f;
			rainDirs[j, 1] = Vector2.ClampMagnitude(rainDirs[j, 1], 10f);
		}
		highlightPos -= 40f;
		if (highlightPos < -500f)
		{
			highlightPos = Mathf.Lerp(1000f, 3000f, UnityEngine.Random.value);
		}
		lightning = Custom.LerpAndTick(lightning, 0f, 0.06f, 1f / Mathf.Lerp(8f, 14f, lightningIntensity));
		if (UnityEngine.Random.value < lightningIntensity * Mathf.Lerp(1f, 2f, extraLightningChance))
		{
			lightning = Mathf.Max(lightning, UnityEngine.Random.value * lightningIntensity);
		}
		if (LIdropOff > 0f)
		{
			lightningIntensity = Mathf.Max(0f, lightningIntensity - 1f / LIdropOff);
		}
		extraLightningChance = Mathf.Max(0f, extraLightningChance - 0.1f);
	}

	public void LightningSpike(float newInt, float dropOffFrames)
	{
		lightningIntensity = Mathf.Max(lightningIntensity, newInt);
		extraLightningChance = 1f;
		LIdropOff = dropOffFrames;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLightning, lightning, timeStacker)), 1.5f);
		float num2 = Mathf.Lerp(rainFade, 1f, num);
		for (int i = 0; i < drops.Length; i++)
		{
			Vector2 vector = Vector2.Lerp(drops[i].lastPos, drops[i].lastLastPos, 0.2f);
			float f = Mathf.InverseLerp(2000f, 50f, Mathf.Abs(drops[i].pos.y + drops[i].pos.x * 0.23f - highlightPos));
			f = Mathf.Pow(f, 14f);
			f = Math.Max(f, num);
			sprites[i].x = drops[i].pos.x;
			sprites[i].y = drops[i].pos.y;
			sprites[i].scaleX = 2.2f / Mathf.Lerp(drops[i].depth, 1f, 0.7f);
			sprites[i].rotation = Custom.AimFromOneVectorToAnother(drops[i].pos, vector);
			sprites[i].scaleY = Vector2.Distance(drops[i].pos, vector);
			sprites[i].alpha = Custom.LerpMap(drops[i].depth, 0.5f, 7f, 1f, 0.1f, Mathf.Lerp(1f, 0.5f, f)) * Mathf.Lerp(0.3f * num2, 0.8f * num2 + 0.2f * num, f);
		}
		fadeSprite.alpha = Mathf.Lerp(0.37f, 0.17f, num);
		fadeSprite.scaleX = Mathf.Lerp(1600f, 2100f, num) / 16f;
		fadeSprite.scaleY = Mathf.Lerp(1300f, 1800f, num) / 16f;
		bkg.alpha = Mathf.Pow(num, 1.1f) * 0.5f;
		lightsource.alpha = Mathf.Pow(num, 0.9f) * 0.5f;
		lightsource.x = 800f;
		lightsource.y = 600f;
		lightsource.scaleX = 75f;
		lightsource.scaleY = 68.75f;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		bkg.RemoveFromContainer();
		lightsource.RemoveFromContainer();
		fadeSprite.RemoveFromContainer();
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
	}
}
