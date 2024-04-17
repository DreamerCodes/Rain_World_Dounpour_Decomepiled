using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SnowSource : UpdatableAndDeletable
{
	public int visibility;

	private int lastCam;

	public Vector2 pos;

	private Vector2 lastPos;

	public float rad;

	private float lastRad;

	public float intensity;

	private float lastIntensity;

	public float noisiness;

	private float lastNoisiness;

	public PlacedObject.SnowSourceData.Shape shape;

	private PlacedObject.SnowSourceData.Shape lastShape;

	public SnowSource(Vector2 initPos)
	{
		pos = initPos;
		rad = 100f;
		intensity = 1f;
		noisiness = 0f;
		shape = PlacedObject.SnowSourceData.Shape.Radial;
		visibility = 2;
	}

	public SnowSource(Vector2 initPos, float initRad)
		: this(initPos)
	{
		rad = initRad;
	}

	public SnowSource(Vector2 initPos, float initRad, float initIntensity)
		: this(initPos, initRad)
	{
		intensity = initIntensity;
	}

	public SnowSource(Vector2 initPos, float initRad, float initIntensity, float initNoisiness)
		: this(initPos, initRad, initIntensity)
	{
		noisiness = initNoisiness;
	}

	public SnowSource(Vector2 initPos, float initRad, float initIntensity, float initNoisiness, PlacedObject.SnowSourceData.Shape initShape)
		: this(initPos, initRad, initIntensity, initNoisiness)
	{
		shape = initShape;
	}

	public int CheckVisibility(int camIndex)
	{
		Vector2 vector = room.cameraPositions[camIndex];
		if (pos.x > vector.x - rad && pos.x < vector.x + rad + 1400f && pos.y > vector.y - rad && pos.y < vector.y + rad + 800f)
		{
			return 1;
		}
		return 0;
	}

	public Vector4[] PackSnowData()
	{
		Vector2 vector = room.cameraPositions[room.game.cameras[0].currentCameraPosition];
		Vector4[] array = new Vector4[3];
		Vector2 vector2 = Custom.EncodeFloatRG((pos.x - vector.x) / 1400f * 0.3f + 0.3f);
		Vector2 vector3 = Custom.EncodeFloatRG((pos.y - vector.y) / 800f * 0.3f + 0.3f);
		Vector2 vector4 = Custom.EncodeFloatRG(rad / 1600f);
		array[0] = new Vector4(vector2.x, vector2.y, vector3.x, vector3.y);
		array[1] = new Vector4(vector4.x, vector4.y, intensity, noisiness);
		array[2] = new Vector4(0f, 0f, 0f, (float)(int)shape / 5f);
		return array;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		int currentCameraPosition = room.game.cameras[0].currentCameraPosition;
		bool flag = false;
		if (currentCameraPosition != lastCam || shape != lastShape || noisiness != lastNoisiness || intensity != lastIntensity || pos != lastPos || rad != lastRad || (room.BeingViewed && visibility == 2))
		{
			flag = true;
		}
		if (flag && room.snow && room.BeingViewed)
		{
			visibility = CheckVisibility(currentCameraPosition);
			room.game.cameras[0].snowChange = true;
		}
		lastCam = currentCameraPosition;
		lastPos = pos;
		lastRad = rad;
		lastIntensity = intensity;
		lastNoisiness = noisiness;
		lastShape = shape;
	}
}
