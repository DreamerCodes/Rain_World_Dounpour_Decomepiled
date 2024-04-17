using UnityEngine;

public class GlyphLabel : CosmeticSprite
{
	public Vector2? setPos;

	public float? setScale;

	public int[] glyphs;

	public int visibleGlyphs;

	public float scale = 1f;

	public float lastScale = 1f;

	public Color color;

	public bool inverted;

	public GlyphLabel(Vector2 pos, int[] glyphs)
	{
		base.pos = pos;
		lastPos = pos;
		this.glyphs = glyphs;
		visibleGlyphs = glyphs.Length;
		color = new Color(0f, 0f, 0f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		lastScale = scale;
		if (setScale.HasValue)
		{
			scale = setScale.Value;
			setScale = null;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[glyphs.Length];
		for (int i = 0; i < glyphs.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("glyphs");
			sLeaser.sprites[i].color = color;
			sLeaser.sprites[i].anchorX = 0f;
			sLeaser.sprites[i].anchorY = 0f;
			if (inverted)
			{
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["SingleGlyphHologram"];
			}
			else
			{
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["SingleGlyph"];
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastScale, scale, timeStacker);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = vector.x + (float)i * 15f * num - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].isVisible = i < visibleGlyphs && glyphs[i] >= 0;
			sLeaser.sprites[i].alpha = (float)glyphs[i] / 50f;
			sLeaser.sprites[i].color = color;
			sLeaser.sprites[i].scaleX = 15f / Futile.atlasManager.GetElementWithName("glyphs").sourcePixelSize.x * num;
			sLeaser.sprites[i].scaleY = num;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("BackgroundShortcuts");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public static int[] RandomString(int minLength, int maxLength, int seed, bool cyrillic)
	{
		Random.State state = Random.state;
		Random.InitState(seed);
		int[] array = new int[Random.Range(minLength, maxLength)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (cyrillic ? Random.Range(26, 47) : Random.Range(0, 14));
		}
		Random.state = state;
		return array;
	}

	public static int[] RandomString(int length, int seed, bool cyrillic)
	{
		Random.State state = Random.state;
		Random.InitState(seed);
		int[] array = new int[length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (cyrillic ? Random.Range(26, 47) : Random.Range(0, 14));
		}
		Random.state = state;
		return array;
	}
}
