using RWCustom;
using UnityEngine;
using VoidSea;

public class MeltLights : UpdatableAndDeletable, IDrawable
{
	public class MeltLight : UpdatableAndDeletable
	{
		public Vector2 pos;

		public Vector2 vel;

		public Vector2 lastPos;

		public LightSource lightSource;

		private float speed;

		private float rad;

		public MeltLight(float effectAmount, Vector2 pos, Room room, Color color)
		{
			vel = new Vector2(0f, 0f);
			lightSource = new LightSource(pos, environmentalLight: false, color, this);
			room.AddObject(lightSource);
			lightSource.setAlpha = Random.value * Mathf.Pow(effectAmount, 0.5f);
			rad = Mathf.Lerp(200f, 600f, Random.value);
			lightSource.requireUpKeep = true;
			speed = Mathf.Lerp(4f, 8f, effectAmount) * Mathf.Lerp(0.5f, 1.5f, Random.value);
			this.pos = pos;
			lastPos = this.pos;
			lightSource.setPos = this.pos;
			lightSource.HardSetPos(this.pos);
			lightSource.HardSetRad(rad);
		}

		public override void Update(bool eu)
		{
			lastPos = pos;
			pos.y -= speed;
			lightSource.setRad = Mathf.Clamp(lightSource.Rad + Mathf.Lerp(-10f, 10f, Random.value), rad * 0.5f, rad * 1.5f);
			lightSource.setPos = pos;
			lightSource.setAlpha = Mathf.InverseLerp(0f - lightSource.Rad, 0f, pos.y);
			for (int i = 0; i < room.game.cameras.Length; i++)
			{
				if (room.game.cameras[i].voidSeaMode)
				{
					lightSource.setAlpha = 0f;
				}
			}
			lightSource.stayAlive = true;
			if (pos.y < 0f - lightSource.Rad)
			{
				Destroy();
			}
		}

		public override void Destroy()
		{
			lightSource.Destroy();
			base.Destroy();
		}
	}

	public VoidSeaScene voidSeaEffect;

	private bool hasLookedForVoidSea;

	public RoomSettings.RoomEffect effect;

	public float wait;

	public Color color;

	private float Amount => effect.amount;

	private float SpawnChance
	{
		get
		{
			if (ModManager.MSC && room.world.region != null && room.world.region.name == "HR")
			{
				return 0.9f;
			}
			return effect.amount;
		}
	}

	public MeltLights(RoomSettings.RoomEffect effect, Room rm)
	{
		this.effect = effect;
		room = rm;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!hasLookedForVoidSea)
		{
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is VoidSeaScene)
				{
					voidSeaEffect = room.updateList[i] as VoidSeaScene;
					break;
				}
			}
			hasLookedForVoidSea = true;
		}
		for (int j = 0; j < room.physicalObjects[0].Count; j++)
		{
			if (room.physicalObjects[0][j] is Fly && !(room.physicalObjects[0][j] as Fly).dead && room.physicalObjects[0][j].grabbedBy.Count < 1)
			{
				Custom.Log("melt lights killed a fly");
				room.physicalObjects[0][j].Destroy();
			}
		}
		if (wait > 0f)
		{
			wait -= 1f;
		}
		else
		{
			wait += Mathf.Lerp(40f, 10f, Amount) / ((float)room.TileWidth / 55f);
			if (Random.value < SpawnChance)
			{
				room.AddObject(new MeltLight(Amount, new Vector2(Mathf.Lerp(-150f, room.PixelWidth + 150f, Random.value), room.PixelHeight + 600f), room, Color.Lerp(color, Custom.HSL2RGB(Random.value, 1f, 0.5f), 0.1f * Mathf.Pow(effect.amount, 2f))));
			}
		}
		for (int k = 0; k < room.physicalObjects.Length; k++)
		{
			for (int l = 0; l < room.physicalObjects[k].Count; l++)
			{
				if (room.physicalObjects[k][l].room == room)
				{
					for (int m = 0; m < room.physicalObjects[k][l].bodyChunks.Length; m++)
					{
						room.physicalObjects[k][l].bodyChunks[m].vel.y += room.physicalObjects[k][l].gravity * 0.5f * Amount * (1f - room.physicalObjects[k][l].bodyChunks[m].submersion);
					}
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[0];
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = new Color(0.529f, 0.365f, 0.184f);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
	}
}
