using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class Rainbow : CosmeticSprite
{
	public class RainbowData : PlacedObject.ResizableObjectData
	{
		public Vector2 panelPos;

		public float[] fades;

		public float Chance => fades[5];

		public RainbowData(PlacedObject owner)
			: base(owner)
		{
			fades = new float[6];
			for (int i = 0; i < 4; i++)
			{
				fades[i] = 1f;
			}
			fades[4] = 0.5f;
			fades[5] = 0.15f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			string[] array2 = array[4].Split(',');
			for (int i = 0; i < fades.Length && i < array2.Length; i++)
			{
				fades[i] = float.Parse(array2[i], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			string text = "";
			for (int i = 0; i < fades.Length; i++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", fades[i], (i < fades.Length - 1) ? "," : "");
			}
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, text);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject placedObject;

	private float rad;

	private float fade;

	public bool alwaysShow;

	private RainbowData RBData => placedObject.data as RainbowData;

	public Rainbow(Room room, PlacedObject placedObject)
	{
		base.room = room;
		this.placedObject = placedObject;
		Futile.atlasManager.LoadAtlasFromTexture("rainbow", Resources.Load("Atlases/Rainbow") as Texture2D, textureFromAsset: true);
		Refresh();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (alwaysShow || room.game.IsArenaSession)
		{
			fade = 1f;
			return;
		}
		fade = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(room.world.rainCycle.CycleStartUp, 0.75f) * (float)Math.PI)), 0.6f);
		if (room.world.rainCycle.CycleStartUp >= 1f)
		{
			Destroy();
		}
	}

	public void Refresh()
	{
		pos = placedObject.pos - RBData.handlePos;
		rad = RBData.handlePos.magnitude * 2f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new CustomFSprite("rainbow");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Rainbow"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = fade * Mathf.InverseLerp(0.2f, 0f, rCam.ghostMode);
		for (int i = 0; i < 4; i++)
		{
			(sLeaser.sprites[0] as CustomFSprite).MoveVertice(i, pos + Custom.eightDirections[1 + i * 2].ToVector2() * rad - camPos);
			(sLeaser.sprites[0] as CustomFSprite).verticeColors[i] = new Color(RBData.fades[4], 0f, 0f, num * RBData.fades[i]);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
