using System.Globalization;
using RWCustom;
using UnityEngine;

public class EggBugEgg : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class AbstractBugEgg : AbstractPhysicalObject
	{
		public float hue;

		public AbstractBugEgg(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, float hue)
			: base(world, AbstractObjectType.EggBugEgg, obj, pos, ID)
		{
			this.hue = hue;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}", ID.ToString(), type.ToString(), pos.SaveToString(), hue);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public class LiquidDrip : CosmeticSprite
	{
		private float life;

		private float lifeTime;

		public Color color;

		private Vector2 lastLastPos;

		public LiquidDrip(Vector2 pos, Vector2 vel, Color color)
		{
			base.pos = pos;
			base.vel = vel;
			this.color = color;
			lastPos = pos;
			lastLastPos = pos;
			lifeTime = Mathf.Lerp(20f, 40f, Random.value);
			life = 1f;
		}

		public override void Update(bool eu)
		{
			lastLastPos = lastPos;
			base.Update(eu);
			vel.y -= 0.9f;
			life -= 1f / lifeTime;
			if (life < 0f || room.GetTile(lastPos).Solid)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 a = Vector2.Lerp(lastLastPos, lastPos, timeStacker);
			a = Vector2.Lerp(a, vector, 0.5f);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].scaleY = (Vector2.Distance(vector, a) + 2f) * Mathf.InverseLerp(0f, 0.5f, life);
			sLeaser.sprites[0].scaleX = 1.5f * Mathf.InverseLerp(0f, 0.5f, life);
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(vector, a);
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

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	private Vector2[,] segments;

	public float swell = 1f;

	public float liquid;

	public float liquidDeplete;

	public float rotVel;

	private Color[] eggColors;

	private Color blackColor;

	public int bites = 2;

	public float SwellFac => 1f + 0.15f * swell;

	public AbstractBugEgg abstractBugEgg => abstractPhysicalObject as AbstractBugEgg;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public EggBugEgg(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f * SwellFac, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 0;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		liquidDeplete = Mathf.Lerp(40f, 80f, Random.value);
		segments = new Vector2[5, 3];
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
		rotation = Custom.RNV();
		lastRotation = rotation;
		ResetSegments();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetSegments();
	}

	private void ResetSegments()
	{
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			segments[i, 0] = base.firstChunk.pos + rotation * i;
			segments[i, 1] = segments[i, 0];
			segments[i, 2] *= 0f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		base.firstChunk.rad = 4f * SwellFac;
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		lastRotation = rotation;
		swell = Custom.LerpAndTick(swell, 1f, 0.01f, Random.value / 80f);
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
			rotVel = Mathf.Lerp(rotVel, 0.12f * base.firstChunk.vel.x, 0.8f);
			base.firstChunk.vel.x *= 0.8f;
		}
		if (rotVel > 0f)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * rotVel).normalized;
		}
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			float value = (float)i / (float)(segments.GetLength(0) - 1);
			segments[i, 1] = segments[i, 0];
			segments[i, 0] += segments[i, 2];
			segments[i, 2] *= 0.995f;
			segments[i, 2].y -= 0.9f * Mathf.InverseLerp(0.5f, 1f, value);
			segments[i, 2] += rotation * 5f * Mathf.InverseLerp(0.5f, 0f, value);
			if (i > 1)
			{
				segments[i, 2] += Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
				segments[i - 2, 2] -= Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
			}
			ConnectSegment(i);
		}
		for (int num = segments.GetLength(0) - 1; num >= 0; num--)
		{
			ConnectSegment(num);
		}
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			ConnectSegment(j);
		}
		if (!(liquid > 0f) || eggColors == null)
		{
			return;
		}
		liquid -= 1f / liquidDeplete;
		if (Random.value < 0.25f && Random.value < Mathf.InverseLerp(0f, 0.5f, liquid))
		{
			if (bites < 2)
			{
				room.AddObject(new LiquidDrip(base.firstChunk.pos, rotation * 3f + Custom.RNV() * Random.value * 3f, Color.Lerp(eggColors[1], blackColor, 0.4f)));
				return;
			}
			Vector2 vel = Custom.DirVec(segments[segments.GetLength(0) - 2, 0], segments[segments.GetLength(0) - 1, 0]) * Mathf.Lerp(8f, 1f, liquid);
			vel += segments[segments.GetLength(0) - 1, 2];
			vel += Custom.RNV() * 4f * Random.value;
			room.AddObject(new LiquidDrip(Vector2.Lerp(segments[segments.GetLength(0) - 1, 1], segments[segments.GetLength(0) - 1, 0], Random.value), vel, Color.Lerp(eggColors[1], blackColor, 0.4f)));
		}
	}

	private void ConnectSegment(int i)
	{
		if (i == 0)
		{
			Vector2 vector = base.firstChunk.pos + rotation * 7f * SwellFac;
			Vector2 vector2 = Custom.DirVec(segments[i, 0], vector);
			float num = Vector2.Distance(segments[i, 0], vector);
			segments[i, 0] -= vector2 * (2f - num);
			segments[i, 2] -= vector2 * (2f - num);
		}
		else
		{
			Vector2 vector3 = Custom.DirVec(segments[i, 0], segments[i - 1, 0]);
			float num2 = Vector2.Distance(segments[i, 0], segments[i - 1, 0]);
			segments[i, 0] -= vector3 * (2f - num2) * 0.5f;
			segments[i, 2] -= vector3 * (2f - num2) * 0.5f;
			segments[i - 1, 0] += vector3 * (2f - num2) * 0.5f;
			segments[i - 1, 2] += vector3 * (2f - num2) * 0.5f;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[1] = new FSprite("DangleFruit0A");
		sLeaser.sprites[2] = new FSprite("EggBugEggColor");
		sLeaser.sprites[3] = new FSprite("JetFishEyeA");
		sLeaser.sprites[1].anchorY = 0.3f;
		sLeaser.sprites[2].anchorY = 0.3f;
		sLeaser.sprites[3].anchorY = 0.7f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		vector -= vector2 * 3f * SwellFac;
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i + 1].x = vector.x - camPos.x;
			sLeaser.sprites[i + 1].y = vector.y - camPos.y;
			sLeaser.sprites[i + 1].rotation = Custom.VecToDeg(vector2);
		}
		sLeaser.sprites[1].scaleX = 0.7f * SwellFac;
		sLeaser.sprites[1].scaleY = 0.75f * SwellFac;
		sLeaser.sprites[2].scaleX = 0.7f * SwellFac;
		sLeaser.sprites[2].scaleY = 0.75f * SwellFac;
		sLeaser.sprites[3].scale = 0.45f * SwellFac;
		if (bites < 2)
		{
			sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("DangleFruit1A");
			sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("EggBugEggColorEaten");
			sLeaser.sprites[0].isVisible = false;
			sLeaser.sprites[3].anchorY = 0.4f;
		}
		if (blink > 0 && Random.value < 0.5f)
		{
			sLeaser.sprites[2].color = new Color(1f, 1f, 1f);
			sLeaser.sprites[3].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[2].color = eggColors[1];
			sLeaser.sprites[3].color = eggColors[2];
		}
		Vector2 vector3 = vector + vector2 * 5f * SwellFac;
		float num = 1f;
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			float f = (float)j / (float)(segments.GetLength(0) - 1);
			Vector2 vector4 = Vector2.Lerp(segments[j, 1], segments[j, 0], timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Mathf.Lerp(1f, 0.5f, Mathf.Pow(f, 0.25f));
			float num3 = Vector2.Distance(vector4, vector3) / 5f;
			if (j == 0)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * (num2 + num) * 0.5f + normalized * num3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * (num2 + num) * 0.5f + normalized * num3 - camPos);
			}
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * num2 - normalized * num3 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * num2 - normalized * num3 - camPos);
			vector3 = vector4;
			num = num2;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		eggColors = EggBugGraphics.EggColors(palette, abstractBugEgg.hue, darkness);
		sLeaser.sprites[0].color = eggColors[0];
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i + 1].color = eggColors[i];
		}
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
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		liquid = 1f;
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
