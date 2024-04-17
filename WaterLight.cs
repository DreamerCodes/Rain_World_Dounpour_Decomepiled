using UnityEngine;

public class WaterLight
{
	private FSprite sprite;

	private RoomCamera camera;

	private Water waterObject;

	public WaterLight(RoomCamera camera, FShader shader)
	{
		this.camera = camera;
		sprite = new FSprite("Futile_White");
		sprite.anchorX = 0f;
		sprite.anchorY = 0f;
		sprite.scaleY = 18.75f;
		sprite.shader = shader;
		camera.ReturnFContainer("Foreground").AddChild(sprite);
	}

	public void NewRoom(Water waterObject)
	{
		this.waterObject = waterObject;
		if (waterObject.room != null && waterObject.room.roomRain != null && Futile.atlasManager.DoesContainElementWithName("RainMask_" + waterObject.room.abstractRoom.name))
		{
			sprite.element = Futile.atlasManager.GetElementWithName("RainMask_" + waterObject.room.abstractRoom.name);
		}
		else
		{
			sprite.element = Futile.atlasManager.GetElementWithName("Futile_White");
		}
		sprite.scaleX = (waterObject.SurfaceLeftAndRightBoundries().y - waterObject.SurfaceLeftAndRightBoundries().x) / sprite.element.sourcePixelSize.x;
	}

	public void CleanOut()
	{
		sprite.RemoveFromContainer();
	}

	public void DrawUpdate(Vector2 camPos)
	{
		sprite.x = waterObject.SurfaceLeftAndRightBoundries().x - camPos.x;
		sprite.y = camera.room.waterObject.fWaterLevel - camPos.y;
		sprite.alpha = (1f - camera.PaletteDarkness()) * camera.room.roomSettings.WaterReflectionAlpha;
	}
}
