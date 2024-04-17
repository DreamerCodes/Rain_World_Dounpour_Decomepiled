using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class LightBeam : UpdatableAndDeletable, IDrawable
{
	public class LightBeamData : PlacedObject.QuadObjectData
	{
		public class BlinkType : ExtEnum<BlinkType>
		{
			public static readonly BlinkType None = new BlinkType("None", register: true);

			public static readonly BlinkType Flash = new BlinkType("Flash", register: true);

			public static readonly BlinkType Fade = new BlinkType("Fade", register: true);

			public BlinkType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 panelPos;

		public float alpha;

		public float colorA;

		public float colorB;

		public bool sun;

		public BlinkType blinkType;

		public float blinkRate;

		public bool nightLight;

		public LightBeamData(PlacedObject owner)
			: base(owner)
		{
			alpha = 0.4f;
			sun = true;
			blinkType = BlinkType.None;
			blinkRate = 0f;
			nightLight = false;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			alpha = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorA = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorB = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 11)
			{
				sun = array[11] == "1";
			}
			if (array.Length > 12)
			{
				blinkType = new BlinkType(array[12]);
				blinkRate = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
				nightLight = int.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 15);
		}

		public override string ToString()
		{
			string text = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}", panelPos.x, panelPos.y, alpha, colorA, colorB, sun ? "1" : "0");
			text += string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}", blinkType.ToString(), blinkRate.ToString(), nightLight ? "1" : "0");
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject placedObject;

	public Vector2[] quad;

	public Vector2[] verts;

	public bool meshDirty;

	private float lastAlpha;

	private int gridDiv = 1;

	public int lastCamPos = -1;

	private Color c;

	private float colorAlpha;

	public Color environmentColor;

	public Color paletteLitColor;

	public int blinkTicker;

	public LightBeamData.BlinkType blinkType;

	public float blinkRate;

	public bool nightLight;

	public float nightFade;

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
			c /= colorAlpha;
		}
	}

	public LightBeam(PlacedObject placedObject)
	{
		this.placedObject = placedObject;
		quad = new Vector2[4];
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		gridDiv = GetIdealGridDiv();
		meshDirty = true;
		blinkType = LightBeamData.BlinkType.None;
		nightFade = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if ((placedObject.data as LightBeamData).colorB > 0f && room.game.cameras[0].room == room)
		{
			environmentColor = room.game.cameras[0].PixelColorAtCoordinate(quad[1]);
		}
		if (blinkType != LightBeamData.BlinkType.None)
		{
			blinkTicker = room.syncTicker;
		}
		if (nightLight && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) > 0f && (float)room.world.rainCycle.dayNightCounter >= 6000f * room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) * 1.75f)
		{
			nightFade = Mathf.Lerp(nightFade, 1f, 0.005f);
		}
	}

	public int GetIdealGridDiv()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			if (Vector2.Distance(quad[i], quad[i + 1]) > num)
			{
				num = Vector2.Distance(quad[i], quad[i + 1]);
			}
		}
		if (Vector2.Distance(quad[0], quad[3]) > num)
		{
			num = Vector2.Distance(quad[0], quad[3]);
		}
		return Mathf.Clamp(Mathf.RoundToInt(num / 250f), 1, 20);
	}

	public float BlinkFade()
	{
		float result = 1f;
		float num = (1.01f - blinkRate) * 1000f;
		if (blinkType == LightBeamData.BlinkType.Flash)
		{
			num /= 4f;
		}
		if (blinkType == LightBeamData.BlinkType.Flash && (float)blinkTicker % (num * 2f) <= num)
		{
			result = 0f;
		}
		else if (blinkType == LightBeamData.BlinkType.Fade)
		{
			result = (Mathf.Sin((float)blinkTicker % num / num * (float)Math.PI * 2f) + 1f) / 2f;
		}
		return result;
	}

	public void SetBlinkProperties(LightBeamData.BlinkType type, float rate)
	{
		blinkType = type;
		blinkRate = rate;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", gridDiv);
		meshDirty = true;
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightBeam"];
		verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
		AddToContainer(sLeaser, rCam, null);
	}

	public void UpdateColor(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float a)
	{
		this.color = Color.Lerp(Color.Lerp(paletteLitColor, Color.white, (placedObject.data as LightBeamData).colorA), environmentColor, (placedObject.data as LightBeamData).colorB);
		Color color = Custom.RGB2RGBA(this.color, a);
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = color;
		}
	}

	private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		int idealGridDiv = GetIdealGridDiv();
		if (idealGridDiv != gridDiv)
		{
			gridDiv = idealGridDiv;
			sLeaser.sprites[0].RemoveFromContainer();
			InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i <= gridDiv; i++)
		{
			for (int j = 0; j <= gridDiv; j++)
			{
				Vector2 a = Vector2.Lerp(quad[0], quad[1], (float)j / (float)gridDiv);
				Vector2 b = Vector2.Lerp(quad[1], quad[2], (float)i / (float)gridDiv);
				Vector2 b2 = Vector2.Lerp(quad[3], quad[2], (float)j / (float)gridDiv);
				Vector2 a2 = Vector2.Lerp(quad[0], quad[3], (float)i / (float)gridDiv);
				verts[j * (gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
			}
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (meshDirty)
		{
			UpdateVerts(sLeaser, rCam);
			UpdateColor(sLeaser, rCam, lastAlpha);
			meshDirty = false;
		}
		for (int i = 0; i < verts.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, verts[i] - camPos);
		}
		float num = Mathf.FloorToInt((placedObject.data as LightBeamData).alpha * 3f);
		float num2 = Mathf.InverseLerp(1f / 3f * num, 1f / 3f * (num + 1f), (placedObject.data as LightBeamData).alpha);
		if ((placedObject.data as LightBeamData).sun)
		{
			num2 *= Mathf.Pow(Mathf.InverseLerp(-0.2f, 0f, rCam.room.world.rainCycle.ShaderLight), 1.2f);
		}
		num2 = Mathf.Lerp(1f / 3f * num, 1f / 3f * (num + 1f), num2);
		num2 = (num2 - 1f / 3f * num) * nightFade * BlinkFade() + 1f / 3f * num;
		num2 *= 1f - rCam.room.darkenLightsFactor;
		if (num2 != lastAlpha)
		{
			UpdateColor(sLeaser, rCam, num2);
			lastAlpha = num2;
		}
		if (rCam.currentCameraPosition != lastCamPos)
		{
			lastCamPos = rCam.currentCameraPosition;
			UpdateColor(sLeaser, rCam, num2);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		paletteLitColor = palette.texture.GetPixel(8, 5);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer((rCam.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterLights) > 0f) ? "Water" : "ForegroundLights").AddChild(sLeaser.sprites[0]);
	}
}
