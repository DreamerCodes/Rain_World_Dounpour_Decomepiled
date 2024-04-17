using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class VultureMask : PlayerCarryableItem, IDrawable
{
	public class AbstractVultureMask : AbstractPhysicalObject
	{
		public int colorSeed;

		public bool king;

		public bool scavKing;

		public string spriteOverride;

		public AbstractVultureMask(World world, VultureMask realizedObject, WorldCoordinate pos, EntityID ID, int colorSeed, bool king)
			: base(world, AbstractObjectType.VultureMask, realizedObject, pos, ID)
		{
			this.colorSeed = colorSeed;
			this.king = king;
			spriteOverride = "";
		}

		public AbstractVultureMask(World world, VultureMask realizedObject, WorldCoordinate pos, EntityID ID, int colorSeed, bool king, bool scavKing, string spriteOverride)
			: this(world, realizedObject, pos, ID, colorSeed, king)
		{
			this.scavKing = scavKing;
			this.spriteOverride = spriteOverride;
		}

		public override string ToString()
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), colorSeed, king ? "1" : "0");
			if (ModManager.MSC)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}", scavKing ? "1" : "0", spriteOverride);
			}
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
		}
	}

	public class MaskType : ExtEnum<MaskType>
	{
		public static readonly MaskType NORMAL = new MaskType("NORMAL", register: true);

		public static readonly MaskType KING = new MaskType("KING", register: true);

		public static readonly MaskType SCAVKING = new MaskType("SCAVKING", register: true);

		public MaskType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Vector2 rotationA;

	public Vector2 lastRotationA;

	public Vector2 rotationB;

	public Vector2 lastRotationB;

	public Vector2 rotVel;

	public int onGroundPos;

	public float donned;

	public float lastDonned;

	public float viewFromSide;

	public float lastViewFromSide;

	public float fallOffVultureMode;

	public VultureMaskGraphics maskGfx;

	public AbstractVultureMask AbstrMsk => abstractPhysicalObject as AbstractVultureMask;

	public bool King => maskGfx.maskType == MaskType.KING;

	public VultureMask(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.14f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.3f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.6f;
		maskGfx = new VultureMaskGraphics(this, AbstrMsk, 0);
		maskGfx.GenerateColor(AbstrMsk.colorSeed);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		onGroundPos = Random.Range(0, 3) - 1;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastRotationA = rotationA;
		lastRotationB = rotationB;
		lastDonned = donned;
		lastViewFromSide = viewFromSide;
		float to = 0f;
		float to2 = 0f;
		rotationA = Custom.DegToVec(Custom.VecToDeg(rotationA) + rotVel.x);
		rotationB = Custom.DegToVec(Custom.VecToDeg(rotationB) + rotVel.y);
		rotVel = Vector2.ClampMagnitude(rotVel, 50f);
		rotVel *= Custom.LerpMap(rotVel.magnitude, 5f, 50f, 1f, 0.8f);
		fallOffVultureMode = Mathf.Max(0f, fallOffVultureMode - 1f / 160f);
		base.CollideWithTerrain = grabbedBy.Count == 0;
		base.CollideWithObjects = grabbedBy.Count == 0;
		if (grabbedBy.Count > 0)
		{
			Vector2 vector = Custom.PerpendicularVector(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos);
			if (grabbedBy[0].grabber is Player)
			{
				vector *= Mathf.Sign(Custom.DistanceToLine(base.firstChunk.pos, grabbedBy[0].grabber.bodyChunks[0].pos, grabbedBy[0].grabber.bodyChunks[1].pos));
				if ((grabbedBy[0].grabber as Player).graphicsModule != null && (grabbedBy[0].grabber as Player).standing && ((grabbedBy[0].grabber as Player).bodyMode != Player.BodyModeIndex.ClimbingOnBeam || (grabbedBy[0].grabber as Player).animation == Player.AnimationIndex.StandOnBeam) && (grabbedBy[0].grabber as Player).bodyMode != Player.BodyModeIndex.Swimming && (grabbedBy[0].graspUsed == 1 || grabbedBy[0].grabber.grasps[1] == null || grabbedBy[0].grabber.grasps[1].grabbed.abstractPhysicalObject.type != AbstractPhysicalObject.AbstractObjectType.VultureMask))
				{
					to = Mathf.InverseLerp(15f, 10f, Vector2.Distance((grabbedBy[0].grabber.graphicsModule as PlayerGraphics).hands[grabbedBy[0].graspUsed].pos, grabbedBy[0].grabber.mainBodyChunk.pos));
					if ((grabbedBy[0].grabber as Player).input[0].x != 0 && Mathf.Abs(grabbedBy[0].grabber.bodyChunks[1].lastPos.x - grabbedBy[0].grabber.bodyChunks[1].pos.x) > 2f)
					{
						to2 = (grabbedBy[0].grabber as Player).input[0].x;
					}
				}
			}
			rotationA = Vector3.Slerp(rotationA, vector, 0.5f);
			rotationB = new Vector2(0f, 1f);
		}
		else if (base.firstChunk.ContactPoint.y < 0)
		{
			Vector2 b;
			Vector2 b2;
			if (onGroundPos == 0)
			{
				b = new Vector2(0f, 1f);
				b2 = new Vector2(0f, -1f);
			}
			else
			{
				b = Custom.DegToVec(15f * (float)onGroundPos);
				b2 = Custom.DegToVec(120f * (float)onGroundPos);
			}
			rotationA = Vector2.Lerp(rotationA, b, Random.value);
			rotationB = Vector2.Lerp(rotationB, b2, Random.value);
			rotVel *= Random.value;
		}
		else if (Vector2.Distance(base.firstChunk.lastPos, base.firstChunk.pos) > 5f && rotVel.magnitude < 7f)
		{
			rotVel += Custom.RNV() * (Mathf.Lerp(7f, 25f, Random.value) + base.firstChunk.vel.magnitude * 2f);
			onGroundPos = Random.Range(0, 3) - 1;
		}
		donned = Custom.LerpAndTick(donned, to, 0.11f, 1f / 30f);
		viewFromSide = Custom.LerpAndTick(viewFromSide, to2, 0.11f, 1f / 30f);
		maskGfx.rotationA = rotationA;
		maskGfx.rotationB = rotationB;
		maskGfx.fallOffVultureMode = fallOffVultureMode;
		maskGfx.Update();
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Vulture_Mask_Pick_Up, base.firstChunk);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (grabbedBy.Count == 0 && speed > 4f && firstContact)
		{
			room.PlaySound(SoundID.Vulture_Mask_Terrain_Impact, base.firstChunk, loop: false, Custom.LerpMap(speed, 4f, 9f, 0.2f, 1f), 1f);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[maskGfx.TotalSprites];
		maskGfx.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		float num = Mathf.Lerp(lastDonned, donned, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotationA, rotationA, timeStacker);
		Vector2 vector3 = Vector3.Slerp(lastRotationB, rotationB, timeStacker);
		if (num > 0f && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && grabbedBy[0].grabber.graphicsModule is PlayerGraphics playerGraphics)
		{
			float num2 = Mathf.Lerp(lastViewFromSide, viewFromSide, timeStacker);
			Vector2 vector4 = Custom.DirVec(Vector2.Lerp(playerGraphics.drawPositions[1, 1], playerGraphics.drawPositions[1, 0], timeStacker), Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker));
			Vector2 a = Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker) + vector4 * 3f;
			a = Vector2.Lerp(a, Vector2.Lerp(playerGraphics.head.lastPos, playerGraphics.head.pos, timeStacker) + vector4 * 3f, 0.5f);
			a += Vector2.Lerp(playerGraphics.lastLookDir, playerGraphics.lookDirection, timeStacker) * 1.5f;
			vector2 = Vector3.Slerp(vector2, vector4, num);
			if ((playerGraphics.owner as Player).eatCounter < 35)
			{
				vector3 = Vector3.Slerp(vector3, new Vector2(0f, -1f), num);
				a += vector4 * Mathf.InverseLerp(35f, 15f, (playerGraphics.owner as Player).eatCounter) * 7f;
			}
			else
			{
				vector3 = Vector3.Slerp(vector3, new Vector2(0f, 1f), num);
			}
			if (num2 != 0f)
			{
				vector2 = Custom.DegToVec(Custom.VecToDeg(vector2) - 20f * num2);
				vector3 = Vector3.Slerp(vector3, Custom.DegToVec(-50f * num2), Mathf.Abs(num2));
				a += vector4 * 2f * Mathf.Abs(num2);
				a -= Custom.PerpendicularVector(vector4) * 4f * num2;
			}
			vector = Vector2.Lerp(vector, a, num);
		}
		maskGfx.overrideDrawVector = vector;
		maskGfx.overrideRotationVector = vector2;
		maskGfx.overrideAnchorVector = vector3;
		maskGfx.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		maskGfx.ApplyPalette(sLeaser, rCam, palette);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		maskGfx.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
