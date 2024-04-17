using UnityEngine;

public class ElectricGate : RegionGate
{
	public LightSource[,] lamps;

	public bool[] lampsOn;

	private int bustedLamp = -1;

	public float batteryLeft;

	public bool batteryChanging;

	public int lampBlink;

	public float meterHeight;

	public override bool EnergyEnoughToOpen => batteryLeft > 0.5f;

	public ElectricGate(Room room)
		: base(room)
	{
		lamps = new LightSource[4, 2];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				bool flag = i < 2;
				bool flag2 = i % 2 == 0;
				if (flag)
				{
					flag2 = !flag2;
				}
				lamps[i, j] = new LightSource(new Vector2(10f + (flag2 ? 19f : 28f) * 20f, 10f + (flag ? 9f : 15f) * 20f), environmentalLight: false, new Color(1f, (j == 0) ? 0.4f : 0.6f, 0f), this);
				lamps[i, j].affectedByPaletteDarkness = 0f;
				lamps[i, j].flat = j == 1;
				room.AddObject(lamps[i, j]);
			}
		}
		if (room.game.SeededRandom(room.abstractRoom.index) < 0.2f)
		{
			bustedLamp = room.game.SeededRandomRange(room.abstractRoom.index, 0, 4);
		}
		switch (room.abstractRoom.name)
		{
		case "GATE_SI_CC":
			meterHeight = 558f;
			break;
		case "GATE_HI_CC":
			meterHeight = -46f;
			break;
		case "GATE_SS_UW":
		case "GATE_UW_SS":
			meterHeight = -1000f;
			break;
		default:
			meterHeight = -64f;
			break;
		}
		lampsOn = new bool[4];
	}

	private void BatteryRunning(float flow)
	{
		batteryLeft = Mathf.Max(0f, batteryLeft - flow / 1300f);
		batteryChanging = true;
	}

	public override void Update(bool eu)
	{
		lampBlink++;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (lampsOn[i] && i != bustedLamp)
				{
					lamps[i, j].setAlpha = ((j == 0) ? 1f : 0.5f) * Mathf.Lerp(0.65f, 1f, Random.value);
					lamps[i, j].setRad = Mathf.Lerp(lamps[i, j].Rad, (j == 0) ? 100f : 20f, 0.5f) * Mathf.Lerp(0.95f, 1f, Random.value);
				}
				else
				{
					lamps[i, j].setAlpha = 0f;
					lamps[i, j].setRad = 0f;
				}
			}
		}
		batteryChanging = false;
		for (int k = 0; k < 4; k++)
		{
			if (Random.value < 1f / 60f)
			{
				lampsOn[k] = false;
			}
		}
		if (mode == Mode.MiddleClosed)
		{
			if (startCounter > 0)
			{
				lampsOn[letThroughDir ? 1 : 0] = lampBlink % 20 < 10;
				lampsOn[letThroughDir ? 2 : 3] = lampBlink % 20 >= 10;
				lampsOn[(!letThroughDir) ? 1u : 0u] = false;
				lampsOn[(!letThroughDir) ? 2 : 3] = false;
			}
		}
		else if (mode == Mode.ClosingAirLock)
		{
			BatteryRunning(1f);
		}
		else if (mode == Mode.Waiting)
		{
			washingCounter++;
		}
		else if (mode == Mode.OpeningMiddle)
		{
			BatteryRunning(1f);
		}
		else if (mode == Mode.MiddleOpen)
		{
			for (int l = 0; l < 4; l++)
			{
				lampsOn[letThroughDir ? l : (3 - l)] = lampBlink % 40 >= l * 10 && lampBlink % 40 < (l + 1) * 10;
			}
		}
		else if (mode == Mode.ClosingMiddle)
		{
			BatteryRunning(1f);
			for (int m = 0; m < 4; m++)
			{
				lampsOn[m] = lampBlink % 20 < 10;
			}
		}
		else if (mode == Mode.OpeningSide)
		{
			BatteryRunning(1f);
			for (int n = 0; n < 4; n++)
			{
				lampsOn[n] = lampBlink % 20 < 10;
			}
		}
		else
		{
			_ = mode == Mode.Closed;
		}
		base.Update(eu);
	}
}
