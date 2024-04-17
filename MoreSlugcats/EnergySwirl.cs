using UnityEngine;

namespace MoreSlugcats;

public class EnergySwirl : UpdatableAndDeletable, IDrawable
{
	private Vector2 pos;

	private Vector2 lastPos;

	private Color c;

	public float rad;

	public float lastRad;

	public float alpha;

	public int effectColor;

	public Vector2? setPos;

	public float? setRad;

	public UpdatableAndDeletable tiedToObject;

	public float depth;

	public float lastDepth;

	public float? setDepth;

	public bool colorFromEnviroment;

	public virtual string ElementName => "Futile_White";

	public Color color
	{
		get
		{
			return c;
		}
		set
		{
			c = value;
		}
	}

	public virtual string LayerName => "Foreground";

	public float Rad => rad;

	public Vector2 Pos => pos;

	public float Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
		}
	}

	public EnergySwirl(Vector2 initPos, Color color, UpdatableAndDeletable tiedToObject)
	{
		pos = initPos;
		lastPos = initPos;
		this.color = color;
		this.tiedToObject = tiedToObject;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer(LayerName).AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (effectColor >= 0)
		{
			color = palette.texture.GetPixel(30, 5 - effectColor * 2);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Floor(Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x) + 0.5f;
		sLeaser.sprites[0].y = Mathf.Floor(Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y) + 0.5f;
		sLeaser.sprites[0].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[0].alpha = Mathf.Lerp(lastDepth, Depth, timeStacker);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite(ElementName);
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["EnergySwirl"];
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[0].alpha = Depth;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		lastRad = rad;
		if (setRad.HasValue)
		{
			rad = setRad.Value;
			setRad = null;
		}
		if (setDepth.HasValue)
		{
			Depth = setDepth.Value;
			setDepth = null;
		}
		lastDepth = depth;
		if (colorFromEnviroment && room.game.cameras[0].room == room)
		{
			color = room.game.cameras[0].PixelColorAtCoordinate(pos);
		}
		if (tiedToObject != null && (tiedToObject.slatedForDeletetion || tiedToObject.room != room))
		{
			Destroy();
		}
	}
}
