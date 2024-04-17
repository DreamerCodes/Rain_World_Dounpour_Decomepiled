using RWCustom;
using UnityEngine;

public class SlimeMoldLight : LightFixture, IDrawable
{
	private Vector2[] positions;

	private float[,] sizesAndAlphas;

	private Vector2 lstPos;

	private float darkness;

	public SlimeMoldLight(Room placedInRoom, PlacedObject placedObject, PlacedObject.LightFixtureData lightData)
		: base(placedInRoom, placedObject, lightData)
	{
		positions = new Vector2[5];
		sizesAndAlphas = new float[positions.Length, 2];
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (placedObject.pos != lstPos)
		{
			lstPos = placedObject.pos;
			ResetPositions();
		}
	}

	private void ResetPositions()
	{
		Random.State state = Random.state;
		Random.InitState((int)(placedObject.pos.x + placedObject.pos.y));
		for (int i = 0; i < positions.Length; i++)
		{
			positions[i] = Custom.RNV() * Random.value;
			sizesAndAlphas[i, 0] = Mathf.Lerp(70f, 200f, Random.value);
			sizesAndAlphas[i, 1] = Mathf.Lerp(0.2f, 0.35f, Random.value);
		}
		Random.state = state;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[positions.Length];
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["LightSource"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = placedObject.pos.x + positions[i].x * (float)lightData.randomSeed * 2f - camPos.x;
			sLeaser.sprites[i].y = placedObject.pos.y + positions[i].y * (float)lightData.randomSeed * 2f - camPos.y;
			sLeaser.sprites[i].scale = sizesAndAlphas[i, 0] / 16f;
			sLeaser.sprites[i].alpha = sizesAndAlphas[i, 1] * Mathf.Pow(darkness, 0.5f);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkness = palette.darkness;
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = Custom.Saturate(SlimeMold.SlimeMoldColorFromPalette(palette), 1f);
		}
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
