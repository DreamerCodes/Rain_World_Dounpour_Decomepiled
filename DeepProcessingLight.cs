using UnityEngine;

public class DeepProcessingLight : LightFixture
{
	private LightSource lightSource;

	public DeepProcessingLight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
		lightSource = new LightSource(placedObject.pos, environmentalLight: false, new Color(0f, 0f, 1f), this);
		placedInRoom.AddObject(lightSource);
		lightSource.setAlpha = 1f;
		lightSource.affectedByPaletteDarkness = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lightSource.setRad = Mathf.Lerp(80f, 800f, (float)(placedObject.data as PlacedObject.LightFixtureData).randomSeed / 100f) * room.ElectricPower * Mathf.Lerp(0.9f, 1.1f, Random.value);
		lightSource.setPos = placedObject.pos;
		lightSource.setAlpha = room.ElectricPower * 0.5f;
	}
}
