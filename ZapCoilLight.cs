using UnityEngine;

public class ZapCoilLight : LightFixture, INotifyWhenRoomIsReady
{
	private LightSource lightSource;

	private ZapCoil myCoil;

	public ZapCoilLight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
		lightSource = new LightSource(placedObject.pos, environmentalLight: false, new Color(0f, 0f, 1f), this);
		placedInRoom.AddObject(lightSource);
		lightSource.setRad = Mathf.Lerp(100f, 2000f, (float)lightData.randomSeed / 100f);
		lightSource.setAlpha = 1f;
		lightSource.affectedByPaletteDarkness = 0.5f;
		if (ModManager.MSC && placedInRoom.game.IsStorySession && placedInRoom.world.region.name == "MS" && !placedInRoom.game.IsMoonHeartActive())
		{
			lightSource.setAlpha = 0f;
			lightSource.setRad = 100f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lightSource.setPos = placedObject.pos;
		if (myCoil != null)
		{
			float num = Mathf.Lerp(1f, 0.85f, myCoil.smoothDisruption * Random.value) * myCoil.turnedOn;
			lightSource.setAlpha = Mathf.Pow(num, 0.8f);
			lightSource.setRad = Mathf.Lerp(100f, 2000f, (float)lightData.randomSeed / 100f) * Mathf.Lerp(0.5f, 1f, num);
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (myCoil != null)
			{
				break;
			}
			if (room.updateList[i] is ZapCoil && (room.updateList[i] as ZapCoil).GetFloatRect.Vector2Inside(placedObject.pos))
			{
				myCoil = room.updateList[i] as ZapCoil;
			}
		}
	}
}
