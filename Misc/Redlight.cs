using System;
using UnityEngine;

public class Redlight : LightFixture
{
	public class RedlightLight : LightSource
	{
		public override string ElementName => "LightMask0";

		public RedlightLight(Vector2 pos, Color col, UpdatableAndDeletable attached)
			: base(pos, environmentalLight: false, col, attached)
		{
		}

		public RedlightLight(Vector2 pos, Color col, UpdatableAndDeletable attached, bool submersible)
			: base(pos, environmentalLight: false, col, attached, submersible)
		{
		}
	}

	private RedlightLight lightSource;

	private LightSource flatLightSource;

	public int flickerWait;

	public int flicker;

	public float sin;

	public float switchOn;

	public bool gravityDependent;

	public bool powered;

	public bool submersible;

	public Room placedRoom;

	private float NoElectricity
	{
		get
		{
			if (room == null)
			{
				return 0f;
			}
			return 1f - room.ElectricPower;
		}
	}

	public Redlight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
		sin = UnityEngine.Random.value;
		flickerWait = UnityEngine.Random.Range(0, 700);
		placedRoom = placedInRoom;
		flatLightSource = new LightSource(placedObject.pos, environmentalLight: false, new Color(1f, 0.05f, 0.05f), this);
		flatLightSource.flat = true;
		placedInRoom.AddObject(flatLightSource);
		gravityDependent = placedInRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f && (float)lightData.randomSeed > 0f;
		powered = NoElectricity > 0.5f || !gravityDependent;
		switchOn = (float)lightData.randomSeed / 100f;
	}

	public Redlight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData, bool submersible)
		: this(placedInRoom, placedObject, lightData)
	{
		this.submersible = submersible;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (lightSource == null)
		{
			lightSource = new RedlightLight(placedObject.pos, new Color(1f, 0f, 0f), this, submersible);
			placedRoom.AddObject(lightSource);
		}
		if (gravityDependent)
		{
			if (!powered)
			{
				lightSource.setAlpha = 0f;
				flatLightSource.setAlpha = 0f;
				if (!(NoElectricity > Mathf.Lerp(0.65f, 0.95f, switchOn)) || !(UnityEngine.Random.value < 1f / Mathf.Lerp(20f, 80f, switchOn)))
				{
					return;
				}
				powered = true;
				flicker = UnityEngine.Random.Range(1, 15);
				room.PlaySound(SoundID.Red_Light_On, placedObject.pos, 1f, 1f);
			}
			else if (NoElectricity < 0.6f && UnityEngine.Random.value < 0.05f)
			{
				powered = false;
			}
		}
		float num = (gravityDependent ? NoElectricity : 1f);
		flickerWait--;
		sin += 1f / Mathf.Lerp(60f, 80f, UnityEngine.Random.value);
		lightSource.setRad = Mathf.Lerp(290f, 310f, 0.5f + Mathf.Sin(sin * (float)Math.PI * 2f) * 0.5f) * 0.16f;
		if (flickerWait < 1)
		{
			flickerWait = UnityEngine.Random.Range(0, 700);
			flicker = UnityEngine.Random.Range(1, 15);
		}
		if (flicker > 0)
		{
			flicker--;
			if (UnityEngine.Random.value < 1f / 3f)
			{
				float num2 = Mathf.Pow(UnityEngine.Random.value, 0.5f);
				lightSource.setAlpha = num2 * num;
				flatLightSource.setAlpha = num2 * 0.25f * num;
				flatLightSource.setRad = num2 * 30f;
			}
		}
		else
		{
			lightSource.setAlpha = Mathf.Lerp(0.9f, 1f, 0.5f + Mathf.Sin(sin * (float)Math.PI * 2f) * 0.5f * UnityEngine.Random.value) * num;
			flatLightSource.setAlpha = 0.25f * num;
			flatLightSource.setRad = Mathf.Lerp(28f, 32f, UnityEngine.Random.value);
		}
		lightSource.setPos = placedObject.pos;
		flatLightSource.setPos = placedObject.pos;
	}
}
