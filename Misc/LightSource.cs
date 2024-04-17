using System;
using UnityEngine;

public class LightSource : UpdatableAndDeletable, IDrawable
{
	public Vector2? setPos;

	private Vector2 pos;

	private Vector2 lastPos;

	public float? setRad;

	public float rad;

	public float lastRad;

	public float? setAlpha;

	public float alpha;

	public float lastAlpha;

	public float affectedByPaletteDarkness = 1f;

	private Color c;

	private float colorAlpha;

	public bool colorFromEnvironment;

	public int effectColor = -1;

	public bool fadeWithSun;

	private bool shaderDirty;

	private bool f;

	public float waterSurfaceLevel;

	public bool environmentalLight;

	public UpdatableAndDeletable tiedToObject;

	public bool requireUpKeep;

	public bool stayAlive = true;

	public int blinkTicker;

	public PlacedObject.LightSourceData.BlinkType blinkType;

	public float blinkRate;

	public bool submersible;

	public bool nightLight;

	public float nightFade;

	public bool noGameplayImpact;

	public Vector2 Pos => pos;

	public float Rad => rad;

	public float Alpha
	{
		get
		{
			if (nightLight)
			{
				return alpha * nightFade;
			}
			if (fadeWithSun)
			{
				return alpha * Mathf.Pow(Mathf.InverseLerp(-0.5f, 0f, room.world.rainCycle.ShaderLight), 1.2f);
			}
			return alpha;
		}
	}

	public float Lightness
	{
		get
		{
			if (colorAlpha == 0f || alpha == 0f)
			{
				return 0f;
			}
			return (color.r + color.g + color.b) / 3f * colorAlpha * alpha;
		}
	}

	public Color color
	{
		get
		{
			return c;
		}
		set
		{
			c = value;
			colorAlpha = 0f;
			if (c.r > colorAlpha)
			{
				colorAlpha = c.r;
			}
			if (c.g > colorAlpha)
			{
				colorAlpha = c.g;
			}
			if (c.b > colorAlpha)
			{
				colorAlpha = c.b;
			}
			if (colorAlpha == 0f)
			{
				c = Color.white;
			}
			else
			{
				c /= colorAlpha;
			}
		}
	}

	public bool flat
	{
		get
		{
			return f;
		}
		set
		{
			if (f != value)
			{
				f = value;
				shaderDirty = true;
			}
		}
	}

	public virtual string ElementName => "Futile_White";

	public virtual string LayerName
	{
		get
		{
			if (submersible)
			{
				return "Water";
			}
			return "ForegroundLights";
		}
	}

	public void HardSetRad(float r)
	{
		lastRad = r;
		rad = r;
		setRad = null;
	}

	public void HardSetAlpha(float a)
	{
		lastAlpha = a;
		alpha = a;
		setAlpha = null;
	}

	public void HardSetPos(Vector2 p)
	{
		lastPos = p;
		pos = p;
		setPos = null;
	}

	public LightSource(Vector2 initPos, bool environmentalLight, Color color, UpdatableAndDeletable tiedToObject)
	{
		pos = initPos;
		lastPos = initPos;
		this.environmentalLight = environmentalLight;
		this.color = color;
		this.tiedToObject = tiedToObject;
		blinkType = PlacedObject.LightSourceData.BlinkType.None;
		nightFade = 1f;
		if (environmentalLight)
		{
			affectedByPaletteDarkness = 0f;
		}
	}

	public LightSource(Vector2 initPos, bool environmentalLight, Color color, UpdatableAndDeletable tiedToObject, bool submersible)
		: this(initPos, environmentalLight, color, tiedToObject)
	{
		this.submersible = submersible;
	}

	public void setBlinkProperties(PlacedObject.LightSourceData.BlinkType type, float rate)
	{
		blinkType = type;
		blinkRate = rate;
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
		lastRad = rad;
		if (setRad.HasValue)
		{
			rad = setRad.Value;
			setRad = null;
		}
		if (tiedToObject != null && (tiedToObject.slatedForDeletetion || tiedToObject.room != room))
		{
			if (alpha == 0f)
			{
				Destroy();
			}
			else
			{
				setAlpha = 0f;
				setRad = 0f;
			}
		}
		lastAlpha = Alpha;
		if (setAlpha.HasValue)
		{
			alpha = setAlpha.Value;
			setAlpha = null;
		}
		if (colorFromEnvironment && room.game.cameras[0].room == room)
		{
			color = room.game.cameras[0].PixelColorAtCoordinate(pos);
		}
		waterSurfaceLevel = room.FloatWaterLevel(pos.x);
		if (requireUpKeep)
		{
			if (stayAlive)
			{
				stayAlive = false;
			}
			else
			{
				Destroy();
			}
		}
		if (blinkType != PlacedObject.LightSourceData.BlinkType.None)
		{
			blinkTicker = room.syncTicker;
		}
		if (nightLight && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) > 0f && (float)room.world.rainCycle.dayNightCounter >= 6000f * room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) * 1.75f)
		{
			nightFade = Mathf.Lerp(nightFade, 1f, 0.005f);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[(!rCam.room.water) ? 1 : 2];
		sLeaser.sprites[0] = new FSprite(ElementName);
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders[flat ? "FlatLight" : "LightSource"];
		sLeaser.sprites[0].color = color;
		if (rCam.room.water)
		{
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["UnderWaterLight"];
			sLeaser.sprites[1].color = color;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = 1f;
		float num2 = (1.01f - blinkRate) * 1000f;
		if (blinkType == PlacedObject.LightSourceData.BlinkType.Flash)
		{
			num2 /= 4f;
		}
		if (blinkType == PlacedObject.LightSourceData.BlinkType.Flash && (float)blinkTicker % (num2 * 2f) <= num2)
		{
			num = 0f;
		}
		else if (blinkType == PlacedObject.LightSourceData.BlinkType.Fade)
		{
			num = (Mathf.Sin((float)blinkTicker % num2 / num2 * (float)Math.PI * 2f) + 1f) / 2f;
		}
		num *= 1f - rCam.room.darkenLightsFactor;
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = Mathf.Floor(Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x) + 0.5f;
			sLeaser.sprites[i].color = color;
			if (i == 0)
			{
				sLeaser.sprites[i].y = Mathf.Floor(Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y) + 0.5f;
				sLeaser.sprites[i].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
				sLeaser.sprites[i].alpha = Mathf.Lerp(lastAlpha, Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(pos), affectedByPaletteDarkness) * colorAlpha * num;
				continue;
			}
			float num3 = Mathf.Lerp(lastPos.y, pos.y, timeStacker);
			float num4 = Mathf.InverseLerp(waterSurfaceLevel - rad * 0.25f, waterSurfaceLevel + rad * 0.25f, num3);
			float num5 = Mathf.Lerp(lastRad, rad, timeStacker) * 0.5f * Mathf.Pow(1f - num4, 0.5f);
			sLeaser.sprites[i].y = Mathf.Floor(Mathf.Min(num3, Mathf.Lerp(num3, waterSurfaceLevel - num5 * 0.5f, 0.5f)) - camPos.y) + 0.5f;
			if (ModManager.MSC && rCam.room.waterInverted)
			{
				num4 = 1f - Mathf.InverseLerp(waterSurfaceLevel - rad * 0.25f, waterSurfaceLevel + rad * 0.25f, num3);
				num5 = Mathf.Lerp(lastRad, rad, timeStacker) * 0.5f * Mathf.Pow(1f - num4, 0.5f);
				sLeaser.sprites[i].y = Mathf.Floor(Mathf.Min(num3, Mathf.Lerp(num3, num5 - waterSurfaceLevel * 0.5f, 0.5f)) - camPos.y) + 0.5f;
			}
			sLeaser.sprites[i].scale = num5 / 8f;
			sLeaser.sprites[i].alpha = Mathf.Lerp(lastAlpha, Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(pos), affectedByPaletteDarkness) * Mathf.Pow(1f - num4, 0.5f) * colorAlpha * num;
		}
		if (shaderDirty)
		{
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders[flat ? "FlatLight" : "LightSource"];
			shaderDirty = false;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (effectColor >= 0)
		{
			color = palette.texture.GetPixel(30, 5 - effectColor * 2);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (rCam.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterLights) > 0f)
		{
			submersible = true;
		}
		rCam.ReturnFContainer(LayerName).AddChild(sLeaser.sprites[0]);
		if (sLeaser.sprites.Length > 1)
		{
			rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[1]);
		}
	}
}
