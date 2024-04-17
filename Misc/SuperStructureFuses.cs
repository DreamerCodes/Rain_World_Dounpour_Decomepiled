using RWCustom;
using UnityEngine;

public class SuperStructureFuses : UpdatableAndDeletable, IDrawable
{
	public PlacedObject placedObject;

	public Vector2 pos;

	public Vector2 lastPos;

	public IntRect rect;

	public float broken;

	public float[,,] lights;

	private int depth;

	public int debugMode;

	public float lightness;

	public float lastLightness;

	public int malFunction;

	private float smoothedMalfunction;

	private IntRect malfunctionRect;

	public float power;

	public float powerFlicker;

	public bool gravityDependent;

	public bool culled;

	public FloatRect GetFloatRect => new FloatRect((float)rect.left * 20f, (float)rect.bottom * 20f, (float)rect.right * 20f + 20f, (float)rect.top * 20f + 20f);

	public SuperStructureFuses(PlacedObject placedObject, IntRect rect, Room room)
	{
		this.placedObject = placedObject;
		pos = placedObject.pos;
		this.rect = rect;
		lights = new float[rect.Width * 2, rect.Height * 2, 5];
		depth = 0;
		for (int i = rect.left; i <= rect.right; i++)
		{
			for (int j = rect.bottom; j <= rect.top; j++)
			{
				if (!room.GetTile(i, j).Solid && (room.GetTile(i, j).wallbehind ? 1 : 2) > depth)
				{
					depth = (room.GetTile(i, j).wallbehind ? 1 : 2);
				}
			}
		}
		if (room.world.game.IsArenaSession)
		{
			broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
		}
		else
		{
			broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
			if (room.world.region.name != "SS" && room.world.region.name != "UW" && !room.world.game.IsArenaSession)
			{
				broken = 1f;
			}
		}
		gravityDependent = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f;
		power = 1f;
		powerFlicker = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		culled = true;
		for (int i = 0; i < 4; i++)
		{
			if (!culled)
			{
				break;
			}
			if (room.ViewedByAnyCamera(GetFloatRect.GetCorner(i), 40f))
			{
				culled = false;
			}
		}
		if (culled)
		{
			return;
		}
		if (gravityDependent)
		{
			power = Mathf.Lerp(power, (room.gravity > 0.5f) ? 0f : 1f, 0.2f * Random.value);
			if (Random.value < 0.1f)
			{
				powerFlicker = Mathf.Lerp(Random.value, power, Mathf.Abs(power - 0.5f) * 2f);
			}
		}
		lastLightness = lightness;
		lightness = Mathf.Clamp(lightness + Mathf.Lerp(-1f, 1f, Random.value) / 40f, 0f, 1f);
		lightness = Mathf.Min(Mathf.Min(lightness, smoothedMalfunction), 1f - power);
		for (int j = 0; j < lights.GetLength(0); j++)
		{
			for (int k = 0; k < lights.GetLength(1); k++)
			{
				if (!(Mathf.Pow(Random.value, 0.5f) > smoothedMalfunction))
				{
					continue;
				}
				lights[j, k, 2] = lights[j, k, 1];
				lights[j, k, 1] = lights[j, k, 0];
				if (lights[j, k, 3] > 0f)
				{
					lights[j, k, 4] -= 1f;
					if (lights[j, k, 4] <= 0f)
					{
						lights[j, k, 3] = 0f;
					}
				}
				if (Mathf.Pow(Random.value, 0.25f) < smoothedMalfunction)
				{
					lights[j, k, 0] = Random.value;
					lights[j, k, 3] = Random.Range(1, 6);
					lights[j, k, 4] = Random.Range(2, 10);
				}
				switch ((int)lights[j, k, 3])
				{
				case 1:
					lights[j, k, 0] = 1f;
					continue;
				case 2:
					lights[j, k, 0] = 0f;
					continue;
				case 3:
					lights[j, k, 0] = Random.value;
					continue;
				case 4:
					if ((int)lights[j, k, 4] % 12 > 5)
					{
						if (lights[j, k, 0] == 0f)
						{
							lights[j, k, 0] = 1f;
						}
						else
						{
							lights[j, k, 0] = 0f;
						}
					}
					continue;
				case 5:
					lights[j, k, 0] = 1f;
					continue;
				}
				lights[j, k, 0] = Mathf.Clamp(lights[j, k, 0] + Mathf.Lerp(-1f, 1f, Random.value) / 10f, 0f, 1f);
				float num = lights[j, k, 2];
				int num2 = 1;
				if (j > 0)
				{
					num += lights[j - 1, k, 2];
					num2++;
				}
				if (j < lights.GetLength(0) - 1)
				{
					num += lights[j + 1, k, 2];
					num2++;
				}
				if (k > 0)
				{
					num += lights[j, k - 1, 2];
					num2++;
				}
				if (k < lights.GetLength(1) - 1)
				{
					num += lights[j, k + 1, 2];
					num2++;
				}
				lights[j, k, 0] = Mathf.Lerp(lights[j, k, 0], num / (float)num2, 0.5f);
				if (Random.value < 0.0025f)
				{
					lights[j, k, 3] = Random.Range((Random.value < 1f / 3f) ? 1 : 2, 5);
					lights[j, k, 4] = Mathf.Lerp(2f, 80f, Random.value);
					if (Random.value < 1f / Mathf.Lerp(8000f, 50f, broken))
					{
						lights[j, k, 3] = 5f;
						lights[j, k, 4] = Mathf.Lerp(60f, 120f, Random.value);
					}
				}
			}
		}
		if (malFunction > 0)
		{
			malFunction--;
		}
		else if (Mathf.Pow(Random.value, broken) < 1f / Mathf.Lerp(600f, 120f, broken))
		{
			malFunction = Random.Range(20, 80);
			malfunctionRect = new IntRect(Random.Range(-10, lights.GetLength(0) + 10), Random.Range(-10, lights.GetLength(1) + 10), Random.Range(-10, lights.GetLength(0) + 10), Random.Range(-10, lights.GetLength(1) + 10));
			if (malfunctionRect.left > malfunctionRect.right)
			{
				int left = malfunctionRect.left;
				malfunctionRect.left = malfunctionRect.right;
				malfunctionRect.right = left;
			}
			if (malfunctionRect.bottom > malfunctionRect.top)
			{
				int bottom = malfunctionRect.bottom;
				malfunctionRect.bottom = malfunctionRect.top;
				malfunctionRect.top = bottom;
			}
		}
		smoothedMalfunction = Mathf.Lerp(smoothedMalfunction, ((float)malFunction > 0f) ? 1f : 0f, 0.02f);
		lastPos = pos;
		pos = placedObject.pos;
		if (lastPos != pos)
		{
			debugMode = 80;
		}
		if (debugMode > 0)
		{
			debugMode--;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[lights.GetLength(0) * lights.GetLength(1)];
		int num = 0;
		for (int i = 0; i < lights.GetLength(0); i++)
		{
			for (int j = 0; j < lights.GetLength(1); j++)
			{
				sLeaser.sprites[num] = new FSprite("Futile_White");
				sLeaser.sprites[num].scale = 0.625f;
				sLeaser.sprites[num].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
				if (depth == 0)
				{
					sLeaser.sprites[num].alpha = 59f / 60f;
				}
				else if (depth == 1)
				{
					sLeaser.sprites[num].alpha = 19f / 30f;
				}
				else
				{
					sLeaser.sprites[num].alpha = 0.3f;
				}
				num++;
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!culled != sLeaser.sprites[0].isVisible)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = !culled;
			}
		}
		if (culled)
		{
			return;
		}
		Vector2 vector = pos;
		float num = Mathf.Lerp(lastLightness, lightness, timeStacker);
		int num2 = 0;
		for (int j = 0; j < lights.GetLength(0); j++)
		{
			for (int k = 0; k < lights.GetLength(1); k++)
			{
				Vector2 vector2 = rCam.ApplyDepth(vector + new Vector2(5f + 10f * (float)j, 5f + 10f * (float)k), Mathf.Min(-4f + 10f * (float)depth, 12f));
				sLeaser.sprites[num2].x = vector2.x - camPos.x;
				sLeaser.sprites[num2].y = vector2.y - camPos.y;
				if (debugMode > 0)
				{
					sLeaser.sprites[num2].color = new Color((j % 2 == 0) ? 1f : 0f, (k % 2 == 0) ? 1f : 0f, (j % 2 == 1 && k % 2 == 1) ? 1f : 0f);
				}
				else if (smoothedMalfunction > 0.2f && lights[j, k, 0] > 0.5f && Custom.InsideRect(new IntVector2(j, k), malfunctionRect))
				{
					sLeaser.sprites[num2].color = new Color(Mathf.Lerp(1f, 0.25f, Mathf.Pow(smoothedMalfunction, 0.2f)) * powerFlicker, 0f, 0f);
				}
				else if (lights[j, k, 3] == 5f)
				{
					sLeaser.sprites[num2].color = new Color((((int)lights[j, k, 4] % 8 > 3) ? 1f : 0f) * Mathf.Lerp(1f, 0.25f, Mathf.Pow(smoothedMalfunction, 0.2f)) * powerFlicker, 0f, 0f);
				}
				else
				{
					sLeaser.sprites[num2].color = new Color(0f, 0f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lights[j, k, 1], lights[j, k, 0], timeStacker)), 2.5f - num) * Mathf.Lerp(0.3f, 1f, num) * Mathf.Lerp(1f, 0.5f, Mathf.Pow(smoothedMalfunction, 0.2f)) * powerFlicker);
				}
				num2++;
			}
		}
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
			newContatiner = rCam.ReturnFContainer("Foreground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
