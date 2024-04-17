using System;
using RWCustom;
using UnityEngine;

public class SwollenWaterNut : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float prop;

	public float lastProp;

	public float propSpeed;

	public float darkness;

	public float lastDarkness;

	public int bites = 3;

	public bool addAbstractEntity;

	public float plop;

	public float lastPlop;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int BitesLeft => 1;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public SwollenWaterNut(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, 0.34f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.95f;
		collisionLayer = 1;
		base.waterFriction = 0.91f;
		base.buoyancy = 1.2f;
		prop = 0f;
		lastProp = 0f;
		plop = 1f;
		lastPlop = 1f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (addAbstractEntity)
		{
			bool flag = false;
			int num = 0;
			while (!flag && num < room.abstractRoom.entities.Count)
			{
				if (room.abstractRoom.entities[num].ID == abstractPhysicalObject.ID)
				{
					flag = true;
				}
				num++;
			}
			if (!flag)
			{
				room.abstractRoom.entities.Add(abstractPhysicalObject);
				abstractPhysicalObject.slatedForDeletion = false;
				addAbstractEntity = false;
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			base.firstChunk.vel.x *= 0.8f;
		}
		lastProp = prop;
		prop += propSpeed;
		propSpeed *= 0.85f;
		propSpeed -= prop / 10f;
		prop = Mathf.Clamp(prop, -15f, 15f);
		if (grabbedBy.Count == 0)
		{
			prop += (base.firstChunk.lastPos.x - base.firstChunk.pos.x) / 15f;
			prop -= (base.firstChunk.lastPos.y - base.firstChunk.pos.y) / 15f;
		}
		lastPlop = plop;
		if (plop > 0f && plop < 1f)
		{
			plop = Mathf.Min(1f, plop + 0.1f);
		}
		if (base.Submersion > 0.5f && room.abstractRoom.creatures.Count > 0 && grabbedBy.Count == 0)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.dead && (abstractCreature.realizedCreature as JetFish).AI.goToFood == null && (abstractCreature.realizedCreature as JetFish).AI.WantToEatObject(this))
			{
				(abstractCreature.realizedCreature as JetFish).AI.goToFood = this;
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (direction.y != 0)
		{
			prop += speed;
			propSpeed += speed / 10f;
		}
		else
		{
			prop -= speed;
			propSpeed -= speed / 10f;
		}
		if (speed > 1.2f && firstContact)
		{
			Vector2 pos = base.firstChunk.pos + direction.ToVector2() * base.firstChunk.rad;
			for (int i = 0; i < Mathf.RoundToInt(Custom.LerpMap(speed, 1.2f, 6f, 2f, 5f, 1.2f)); i++)
			{
				room.AddObject(new WaterDrip(pos, Custom.RNV() * (2f + speed) * UnityEngine.Random.value * 0.5f + -direction.ToVector2() * (3f + speed) * 0.35f, waterColor: true));
			}
			room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, pos, Custom.LerpMap(speed, 1.2f, 6f, 0.2f, 1f), 1f);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("JetFishEyeA");
		sLeaser.sprites[0].scaleX = 1.2f;
		sLeaser.sprites[0].scaleY = 1.4f;
		sLeaser.sprites[1] = new FSprite("tinyStar");
		sLeaser.sprites[1].scaleX = 1.5f;
		sLeaser.sprites[1].scaleY = 2.4f;
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["WaterNut"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i].x = pos.x - camPos.x;
			sLeaser.sprites[i].y = pos.y - camPos.y;
		}
		sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[1].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[2].alpha = (1f - darkness) * (1f - base.firstChunk.submersion);
		float num = Mathf.Lerp(lastPlop, plop, timeStacker);
		num = Mathf.Lerp(0f, 1f + Mathf.Sin(num * (float)Math.PI), num);
		sLeaser.sprites[2].scaleX = (1.2f * Custom.LerpMap(bites, 3f, 1f, 1f, 0.2f) * 1f + Mathf.Lerp(lastProp, prop, timeStacker) / 20f) * num;
		sLeaser.sprites[2].scaleY = (1.2f * Custom.LerpMap(bites, 3f, 1f, 1f, 0.2f) * 1f - Mathf.Lerp(lastProp, prop, timeStacker) / 20f) * num;
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		sLeaser.sprites[1].color = Color.Lerp(new Color(0f, 0.4f, 1f), palette.blackColor, Mathf.Lerp(0f, 0.5f, rCam.PaletteDarkness()));
		sLeaser.sprites[2].color = Color.Lerp(palette.waterColor1, palette.waterColor2, 0.5f);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Water_Nut : SoundID.Slugcat_Bite_Water_Nut, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
		propSpeed += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 7f;
		base.bodyChunks[0].rad = Mathf.InverseLerp(3f, 0f, bites) * 9.5f;
	}

	public void ThrowByPlayer()
	{
	}
}
