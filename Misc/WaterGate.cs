using RWCustom;
using UnityEngine;

public class WaterGate : RegionGate
{
	public WaterFall[] waterFalls;

	public float waterLeft;

	protected float outletLag;

	private WaterFall WashWaterFall => waterFalls[(!letThroughDir) ? 1u : 0u];

	private WaterFall OutletWaterFall => waterFalls[letThroughDir ? 1 : 0];

	private float WaterPressure => Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, waterLeft), 0.6f);

	public override bool EnergyEnoughToOpen => waterLeft > 0.5f;

	public WaterGate(Room room)
		: base(room)
	{
		waterFalls = new WaterFall[2];
		for (int i = 0; i < 2; i++)
		{
			waterFalls[i] = new WaterFall(room, new IntVector2(18 + 9 * i, 23), 0f, 3);
			room.AddObject(waterFalls[i]);
			waterFalls[i].setFlow = 0f;
		}
	}

	private void WaterRunning(float flow)
	{
		waterLeft = Mathf.Max(0f, waterLeft - flow / 1450f);
	}

	public override void Update(bool eu)
	{
		if (mode == Mode.MiddleClosed)
		{
			OutletWaterFall.setFlow = Mathf.InverseLerp(0f, 60f, startCounter) * 0.5f * WaterPressure;
		}
		else if (mode == Mode.ClosingAirLock)
		{
			OutletWaterFall.setFlow = 1f * WaterPressure;
			WaterRunning(1f);
		}
		else if (mode == Mode.Waiting)
		{
			washingCounter++;
			WashWaterFall.setFlow = Mathf.Pow(Mathf.InverseLerp(0f, 160f, washingCounter), 1.5f) * 0.5f * WaterPressure;
			outletLag = Mathf.Max(outletLag - 1f / 60f, 0f);
			OutletWaterFall.setFlow = outletLag * WaterPressure;
			if (!waitingForWorldLoader && washingCounter > 400)
			{
				WashWaterFall.setFlow = 0f;
			}
		}
		else if (mode == Mode.OpeningMiddle)
		{
			OutletWaterFall.setFlow = 1f * WaterPressure;
			WaterRunning(1f);
			if (AllDoorsInPosition())
			{
				outletLag = 1f;
			}
		}
		else if (mode == Mode.MiddleOpen)
		{
			outletLag = Mathf.Max(outletLag - 1f / 60f, 0f);
			OutletWaterFall.setFlow = outletLag * WaterPressure;
		}
		else if (mode == Mode.ClosingMiddle)
		{
			WashWaterFall.setFlow = 1f * WaterPressure;
			OutletWaterFall.setFlow = 0f;
			WaterRunning(1f);
		}
		else if (mode == Mode.OpeningSide)
		{
			WashWaterFall.setFlow = 1f * WaterPressure;
			OutletWaterFall.setFlow = 0f;
			WaterRunning(1f);
			if (AllDoorsInPosition())
			{
				WashWaterFall.setFlow = 0f;
				outletLag = 1f;
			}
		}
		else if (mode == Mode.Closed)
		{
			outletLag = Mathf.Max(outletLag - 1f / 60f, 0.1f);
			if (outletLag > 0.05f)
			{
				WashWaterFall.setFlow = outletLag * WaterPressure;
			}
			else
			{
				WashWaterFall.setFlow = 0f;
			}
			WaterRunning(outletLag * 0.5f);
		}
		base.Update(eu);
	}
}
