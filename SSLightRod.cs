using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class SSLightRod : UpdatableAndDeletable, IDrawable
{
	public class LightVessel
	{
		private SSLightRod rod;

		public LightSource light;

		public float progression;

		public float speed;

		public float strength;

		public float size;

		public float visible;

		public PlacedObject placedObject => rod.placedObject;

		public PlacedObject.SSLightRodData rodData => placedObject.data as PlacedObject.SSLightRodData;

		public LightVessel(SSLightRod rod)
		{
			this.rod = rod;
			light = new LightSource(placedObject.pos, environmentalLight: false, rod.color, rod);
			rod.room.AddObject(light);
			Reset();
			progression = UnityEngine.Random.value;
		}

		public void Update()
		{
			strength = Mathf.InverseLerp(0.1f, 1f, Mathf.Pow(Mathf.Sin(progression * (float)Math.PI), 0.5f));
			Vector2 vector = placedObject.pos + Custom.DegToVec(rodData.rotation) * progression * rodData.length;
			float b = 0.7f;
			if (rod.room.ViewedByAnyCamera(vector, 100f))
			{
				b = 0f;
				for (int i = -3; i < 4; i++)
				{
					Vector2 coord = vector + Custom.DegToVec(rodData.rotation) * 2f * i;
					if (rod.room.game.cameras[0].DepthAtCoordinate(coord) >= rodData.depth)
					{
						b += 1f;
					}
				}
				b /= 7f;
			}
			visible = Mathf.Lerp(visible, b, Mathf.Lerp(0.2f, 0.05f, rodData.brightness));
			strength *= visible;
			light.setAlpha = Mathf.Lerp(1f, 0.5f, size) * strength * rod.room.ElectricPower;
			light.setPos = vector;
			light.setRad = Mathf.Lerp(100f, 400f, size) * visible * Mathf.Lerp(0.2f, 1.5f, rodData.brightness);
			progression = Mathf.Min(1f, progression + speed / rodData.length);
			if (progression >= 1f)
			{
				Reset();
			}
		}

		private void Reset()
		{
			progression = 0f;
			speed = Mathf.Lerp(0.5f, 2f, UnityEngine.Random.value);
			size = UnityEngine.Random.value;
		}
	}

	public PlacedObject placedObject;

	public List<LightVessel> lights;

	public Color color;

	private float lastLength;

	public PlacedObject.SSLightRodData rodData => placedObject.data as PlacedObject.SSLightRodData;

	public SSLightRod(PlacedObject placedObject, Room room)
	{
		base.room = room;
		this.placedObject = placedObject;
		if (room.game.IsArenaSession)
		{
			color = new Color(0.4f, 1f, 0.8f);
		}
		else if (ModManager.MSC && (room.world.region.name == "DM" || room.world.region.name == "MS" || room.world.region.name == "LM" || room.world.region.name == "SL"))
		{
			color = new Color(0.8f, 1f, 0.4f);
		}
		else if (ModManager.MSC && room.world.region.name == "SB")
		{
			color = Color.Lerp(new Color(0.8f, 0.8f, 0f), new Color(0.9f, 0.9f, 0.9f), 0.29f);
		}
		else
		{
			color = new Color(0.4f, 1f, 0.8f);
		}
		lights = new List<LightVessel>();
		UpdateLightAmount();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < lights.Count; i++)
		{
			lights[i].Update();
		}
		if (rodData.length != lastLength)
		{
			UpdateLightAmount();
		}
	}

	private void UpdateLightAmount()
	{
		int num = Custom.IntClamp((int)(rodData.length / 45f), 2, 30);
		if (num != lights.Count)
		{
			for (int i = 0; i < lights.Count; i++)
			{
				lights[i].light.Destroy();
			}
			lights.Clear();
			for (int j = 0; j < num; j++)
			{
				lights.Add(new LightVessel(this));
			}
			lastLength = rodData.length;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].anchorY = 0f;
		sLeaser.sprites[0].scaleX = 4f;
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = placedObject.pos.x - camPos.x;
		sLeaser.sprites[0].y = placedObject.pos.y - camPos.y;
		sLeaser.sprites[0].alpha = 1f - rodData.depth;
		if (ModManager.MSC)
		{
			float t = Mathf.Lerp(rCam.currentPalette.fogAmount / 4f, rCam.currentPalette.fogAmount, rodData.depth / 1.1f);
			sLeaser.sprites[0].color = Color.Lerp(color, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(color, new Color(0f, 0f, 0f), (1f - rCam.room.ElectricPower) * 0.9f);
		}
		sLeaser.sprites[0].scaleY = rodData.length;
		sLeaser.sprites[0].rotation = rodData.rotation;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Water");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
