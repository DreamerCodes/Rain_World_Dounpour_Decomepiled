using RWCustom;
using UnityEngine;

public class Bubble : CosmeticSprite
{
	public class Mode : ExtEnum<Mode>
	{
		public static readonly Mode Growing = new Mode("Growing", register: true);

		public static readonly Mode Free = new Mode("Free", register: true);

		public static readonly Mode Trapped = new Mode("Trapped", register: true);

		public Mode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private float fullSize;

	private float size;

	private float lastSize;

	private float growthSpeed;

	private bool fakeWaterBubble;

	public int age;

	private Mode mode;

	private bool initPop;

	public bool doNotSlow;

	public bool ignoreWalls;

	public Bubble(Vector2 pos, Vector2 vel, bool bottomBubble, bool fakeWaterBubble)
	{
		base.pos = pos;
		base.vel = vel;
		this.fakeWaterBubble = fakeWaterBubble;
		fullSize = Mathf.Lerp(0.5f, 1.5f, Random.value);
		lastSize = 0f;
		growthSpeed = 1f / Mathf.Lerp(5f, 120f, Random.value);
		if (bottomBubble)
		{
			mode = Mode.Growing;
			size = 0f;
		}
		else
		{
			mode = Mode.Free;
			size = 0f;
			initPop = true;
		}
	}

	public override void Update(bool eu)
	{
		lastSize = size;
		if (!doNotSlow)
		{
			vel *= 0.8f;
		}
		if (mode == Mode.Growing)
		{
			vel *= 0f;
			if (size < 1f && Random.value > 0.0125f)
			{
				size = Mathf.Clamp(size + growthSpeed, 0f, 1f);
				pos.y += fullSize * growthSpeed * 10f;
			}
			else if (Random.value < 0.05f)
			{
				mode = Mode.Free;
			}
		}
		else if (mode == Mode.Trapped)
		{
			vel.x += Mathf.Lerp(-0.2f, 0.2f, Random.value);
			float num = ((Random.value < 0.5f) ? (-1f) : 1f);
			if (room.GetTile(pos + new Vector2(20f * num, 10f)).Terrain != Room.Tile.TerrainType.Solid)
			{
				vel.x += num;
			}
			if (room.waterInverted)
			{
				pos.y = room.MiddleOfTile(pos).y - 9f;
			}
			else
			{
				pos.y = room.MiddleOfTile(pos).y + 9f;
			}
			if (room.GetTile(pos + new Vector2(0f, 20f)).Terrain != Room.Tile.TerrainType.Solid)
			{
				mode = Mode.Free;
			}
			size -= growthSpeed;
			if (size <= 0f)
			{
				Destroy();
			}
		}
		else if (mode == Mode.Free)
		{
			if (size == 0f && initPop)
			{
				size = 1f;
				initPop = false;
			}
			float num2 = 0f;
			if (room.waterObject != null)
			{
				num2 = room.waterObject.viscosity;
			}
			Vector2 vector = Custom.DegToVec(-90f + 180f * Random.value) * Random.value * 1.2f * (1f - num2);
			if (room.waterInverted)
			{
				vel -= vector;
			}
			else
			{
				vel += vector;
			}
			age++;
			if ((float)age > 600f - 570f * num2)
			{
				Destroy();
			}
			if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid && !fakeWaterBubble && !ignoreWalls)
			{
				if (room.GetTile(lastPos).Terrain != Room.Tile.TerrainType.Solid)
				{
					if (room.GetTilePosition(pos).y == room.GetTilePosition(lastPos).y + 1)
					{
						mode = Mode.Trapped;
						vel.y = 0f;
						pos.y = room.MiddleOfTile(pos).y - 11f;
					}
					else if (room.GetTilePosition(pos).x != room.GetTilePosition(lastPos).x)
					{
						pos.x = room.MiddleOfTile(lastPos).x + ((pos.x < lastPos.x) ? (-10f) : 10f);
					}
				}
				else
				{
					Destroy();
				}
			}
		}
		bool flag = ((!ModManager.MSC) ? (pos.y > room.FloatWaterLevel(pos.x) - 10f) : (!room.PointSubmerged(pos, 10f)));
		if (flag && !fakeWaterBubble)
		{
			if (size == 0f)
			{
				Destroy();
			}
			else
			{
				size = 0f;
			}
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("LizardBubble5");
		sLeaser.sprites[0].color = new Color(0f, 0.003921569f, 0f);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].scale = Mathf.Lerp(lastSize, size, timeStacker) * fullSize * 0.45f;
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
