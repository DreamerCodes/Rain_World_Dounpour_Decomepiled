using MoreSlugcats;
using UnityEngine;

public abstract class LightFixture : UpdatableAndDeletable, IProvideWarmth
{
	public PlacedObject placedObject;

	public PlacedObject.LightFixtureData lightData;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth * 0.3f;

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 320f;

	public LightFixture(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
	{
		this.placedObject = placedObject;
		this.lightData = lightData;
	}

	Vector2 IProvideWarmth.Position()
	{
		return placedObject.pos;
	}
}
