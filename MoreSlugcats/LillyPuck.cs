using System;
using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class LillyPuck : Weapon, IPlayerEdible
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public LillyPuck fruit;

		public Vector2 stuckPos;

		public float ropeLength;

		public Vector2[] displacements;

		public Vector2[,] segs;

		public int releaseCounter;

		private float connRad;

		public Stalk(LillyPuck fruit, Room room, Vector2 fruitPos)
		{
			this.fruit = fruit;
			fruit.firstChunk.HardSetPosition(fruitPos);
			stuckPos.x = fruitPos.x;
			ropeLength = -1f;
			int x = room.GetTilePosition(fruitPos).x;
			int num = room.GetTilePosition(fruitPos).y;
			if (room.defaultWaterLevel >= 0 && num < room.defaultWaterLevel)
			{
				num = room.defaultWaterLevel;
			}
			for (int num2 = num; num2 > 0; num2--)
			{
				if (room.GetTile(x, num2).Solid)
				{
					stuckPos.y = room.MiddleOfTile(x, num2).y + 10f;
					ropeLength = Mathf.Abs(stuckPos.y - fruitPos.y);
					break;
				}
			}
			segs = new Vector2[Math.Max(1, (int)(ropeLength / 15f)), 3];
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float t = (float)i / (float)(segs.GetLength(0) - 1);
				segs[i, 0] = Vector2.Lerp(stuckPos, fruitPos, t);
				segs[i, 1] = segs[i, 0];
			}
			connRad = ropeLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			displacements = new Vector2[segs.GetLength(0)];
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
			for (int j = 0; j < displacements.Length; j++)
			{
				displacements[j] = Custom.RNV();
			}
			UnityEngine.Random.state = state;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (ropeLength == -1f)
			{
				Destroy();
				return;
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				_ = (float)i / (float)(segs.GetLength(0) - 1);
				segs[i, 1] = segs[i, 0];
				segs[i, 0] += segs[i, 2];
				segs[i, 2] *= 0.99f;
				segs[i, 2].y += 0.9f;
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			List<Vector2> list = new List<Vector2>();
			list.Add(stuckPos);
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				list.Add(segs[j, 0]);
			}
			if (releaseCounter > 0)
			{
				releaseCounter--;
			}
			if (fruit != null)
			{
				list.Add(fruit.RootAttachPos(1f));
				fruit.setRotation = Custom.DirVec(fruit.firstChunk.pos, segs[segs.GetLength(0) - 1, 0]);
				if (!Custom.DistLess(fruit.firstChunk.pos, stuckPos, ropeLength * 1.4f + 10f) || fruit.slatedForDeletetion || fruit.AbstrLillyPuck.bites < 3 || fruit.room != room || releaseCounter == 1)
				{
					fruit.AbstrConsumable.Consume();
					fruit.myStalk = null;
					fruit = null;
				}
			}
		}

		private void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			bool flag = false;
			while (!flag)
			{
				if (num == 0)
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad))
					{
						Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad);
						segs[num, 0] += vector2 * 0.5f;
						segs[num, 2] += vector2 * 0.5f;
						segs[num - 1, 0] -= vector2 * 0.5f;
						segs[num - 1, 2] -= vector2 * 0.5f;
					}
					if (num == segs.GetLength(0) - 1 && fruit != null && !Custom.DistLess(segs[num, 0], fruit.firstChunk.pos, connRad))
					{
						Vector2 vector3 = Custom.DirVec(segs[num, 0], fruit.firstChunk.pos) * (Vector2.Distance(segs[num, 0], fruit.firstChunk.pos) - connRad);
						segs[num, 0] += vector3 * 0.75f;
						segs[num, 2] += vector3 * 0.75f;
						fruit.firstChunk.vel -= vector3 * 0.25f;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: false);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = stuckPos;
			float num = 1.5f;
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float num2 = (float)i / (float)(segs.GetLength(0) - 1);
				float num3 = Custom.LerpMap(num2, 0f, 0.5f, 1f, 0f) + Mathf.Lerp(1f, 0.5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * (float)Math.PI));
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1 && fruit != null)
				{
					vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
				}
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				if (i < segs.GetLength(0) - 1)
				{
					vector2 += (normalized * displacements[i].y + vector3 * displacements[i].x) * Custom.LerpMap(Vector2.Distance(vector, vector2), connRad, connRad * 5f, 4f, 0f);
				}
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - camPos);
				vector = vector2;
				num = num3;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class AbstractLillyPuck : AbstractConsumable
	{
		public int bites;

		public AbstractLillyPuck(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, int bites, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
			: base(world, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, obj, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			this.bites = bites;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, bites);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	private bool spinning;

	private int stillCounter;

	private int stuckBodyPart;

	private int stuckInChunkIndex;

	protected bool pivotAtTip;

	protected bool lastPivotAtTip;

	public PhysicalObject stuckInObject;

	public Appendage.Pos stuckInAppendage;

	public float spearDamageBonus;

	public float stuckRotation;

	public int unstickCounter;

	private float lastDarkness;

	private float darkness;

	private Stalk myStalk;

	private int flowerLeavesCount;

	private Color flowerColor;

	private LightSource light;

	private float lightFade;

	private float LightRad;

	private float oldLightFade;

	private AbstractLillyPuck AbstrLillyPuck => abstractPhysicalObject as AbstractLillyPuck;

	public int BitesLeft => AbstrLillyPuck.bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public LillyPuck(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.88f;
		base.buoyancy = 0.99f;
		pivotAtTip = false;
		lastPivotAtTip = false;
		stuckBodyPart = -1;
		base.firstChunk.loudness = 7f;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		lightFade = 1f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		flowerLeavesCount = 6 + UnityEngine.Random.Range(2, 5);
		float value = UnityEngine.Random.value;
		value = Mathf.Lerp(value, 0.4f, 0.1f);
		HSLColor hSLColor = new HSLColor(value, 1f, 0.5f);
		flowerColor = Custom.HSL2RGB(hSLColor.hue, hSLColor.saturation, hSLColor.lightness);
		flowerColor.b = Mathf.Lerp(flowerColor.b, flowerColor.g, flowerColor.g);
		flowerColor.g = Custom.LerpMap(flowerColor.g, 0f, 0.3f, 1f, 0.3f);
		float a = Mathf.Clamp(flowerColor.r + flowerColor.g / 2f + flowerColor.b / 3f, 0f, 1f) * 0.4f;
		a = Mathf.Lerp(a, 0.6f, flowerColor.b);
		if (UnityEngine.Random.value < 0.2f)
		{
			flowerColor = Color.Lerp(flowerColor, new Color(0.7f, 0.9f, 0.9f), 0.7f + a / 10f);
		}
		else
		{
			flowerColor = Color.Lerp(flowerColor, new Color(0.7f, 0.7f, 0.7f), a);
		}
		LightRad = UnityEngine.Random.Range(190f, 260f);
		UnityEngine.Random.state = state;
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		AbstrLillyPuck.bites--;
		room.PlaySound((AbstrLillyPuck.bites != 0) ? SoundID.Slugcat_Bite_Dangle_Fruit : SoundID.Slugcat_Eat_Dangle_Fruit, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (AbstrLillyPuck.bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			myStalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(myStalk);
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			myStalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(myStalk);
		}
	}

	public override void ChangeMode(Mode newMode)
	{
		if (newMode == Mode.StuckInWall)
		{
			newMode = Mode.Free;
		}
		if (base.mode == Mode.StuckInCreature)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
			}
			PulledOutOfStuckObject();
			ChangeOverlap(newOverlap: true);
		}
		else if (newMode == Mode.StuckInCreature)
		{
			ChangeOverlap(newOverlap: false);
		}
		if (newMode != Mode.Thrown)
		{
			spearDamageBonus = 0.85f;
		}
		if (newMode != Mode.Free)
		{
			spinning = false;
		}
		base.ChangeMode(newMode);
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		if (AbstrLillyPuck.bites == 3)
		{
			room?.PlaySound(SoundID.Slugcat_Throw_Spear, base.firstChunk);
			ChangeMode(Mode.Thrown);
			return;
		}
		room?.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk);
		spinning = true;
		base.firstChunk.vel *= Mathf.Lerp(0.3f, 0.75f, Mathf.InverseLerp(0f, 3f, AbstrLillyPuck.bites));
		ChangeMode(Mode.Free);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		soundLoop.sound = SoundID.None;
		oldLightFade = lightFade;
		if (base.Submersion > 0.5f && room.abstractRoom.creatures.Count > 0 && grabbedBy.Count == 0)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.dead && (abstractCreature.realizedCreature as JetFish).AI.goToFood == null && (abstractCreature.realizedCreature as JetFish).AI.WantToEatObject(this))
			{
				(abstractCreature.realizedCreature as JetFish).AI.goToFood = this;
			}
		}
		if (base.firstChunk.vel.magnitude > 5f)
		{
			if (base.mode == Mode.Thrown)
			{
				soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
			}
			else if (base.mode == Mode.Free)
			{
				soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
			}
			soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
		}
		soundLoop.Update();
		if (base.mode == Mode.StuckInCreature && stuckInObject != null)
		{
			unstickCounter++;
			if (unstickCounter > 40)
			{
				float ang = 0f;
				for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
				{
					if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
					{
						ang = (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).angle;
						break;
					}
				}
				ChangeMode(Mode.Free);
				if (room.BeingViewed)
				{
					base.firstChunk.vel = Custom.DegToVec(ang) * -4f;
					AbstrLillyPuck.bites--;
					SetRandomSpin();
					for (int j = 0; j < 4; j++)
					{
						room.AddObject(new WaterDrip(base.firstChunk.pos, base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
					}
				}
				unstickCounter = 0;
			}
		}
		lastPivotAtTip = pivotAtTip;
		pivotAtTip = base.mode == Mode.Thrown || base.mode == Mode.StuckInCreature;
		if (base.mode == Mode.Free)
		{
			if (spinning)
			{
				if (Custom.DistLess(base.firstChunk.pos, base.firstChunk.lastPos, 4f * room.gravity))
				{
					stillCounter++;
				}
				else
				{
					stillCounter = 0;
				}
				if (base.firstChunk.ContactPoint.y < 0 || stillCounter > 20)
				{
					spinning = false;
					rotationSpeed = 0f;
					if (myStalk == null)
					{
						rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value) + 180f);
					}
					base.firstChunk.vel *= 0f;
					room.PlaySound(SoundID.Spear_Stick_In_Ground, base.firstChunk);
				}
			}
			else if (!Custom.DistLess(base.firstChunk.lastPos, base.firstChunk.pos, 6f))
			{
				SetRandomSpin();
			}
			if (myStalk != null)
			{
				rotation = Custom.DegToVec(180f + base.firstChunk.vel.x * 5f);
			}
		}
		else if (base.mode == Mode.Thrown)
		{
			BodyChunk bodyChunk = base.firstChunk;
			bodyChunk.vel.y = bodyChunk.vel.y + 0.45f;
		}
		else if (base.mode == Mode.StuckInCreature)
		{
			if (stuckInAppendage != null)
			{
				setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage)));
				base.firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
			}
			else
			{
				base.firstChunk.vel = stuckInChunk.vel;
				if (stuckBodyPart == -1 || !room.BeingViewed || (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart) == null)
				{
					setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInChunk.Rotation));
					base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				}
				else
				{
					setRotation = Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos));
					base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, Vector2.Lerp(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos, 0.5f) - stuckInChunk.pos);
				}
			}
			if (stuckInChunk.owner.slatedForDeletetion)
			{
				ChangeMode(Mode.Free);
			}
		}
		for (int num = abstractPhysicalObject.stuckObjects.Count - 1; num >= 0; num--)
		{
			if (abstractPhysicalObject.stuckObjects[num] is AbstractPhysicalObject.ImpaledOnSpearStick)
			{
				if (abstractPhysicalObject.stuckObjects[num].B.realizedObject != null && (abstractPhysicalObject.stuckObjects[num].B.realizedObject.slatedForDeletetion || abstractPhysicalObject.stuckObjects[num].B.realizedObject.grabbedBy.Count > 0))
				{
					abstractPhysicalObject.stuckObjects[num].Deactivate();
				}
				else if (abstractPhysicalObject.stuckObjects[num].B.realizedObject != null && abstractPhysicalObject.stuckObjects[num].B.realizedObject.room == room)
				{
					abstractPhysicalObject.stuckObjects[num].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, base.firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[num] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
					abstractPhysicalObject.stuckObjects[num].B.realizedObject.firstChunk.vel *= 0f;
				}
			}
		}
		if (myStalk != null)
		{
			lightFade = 1f;
			base.buoyancy = 0f;
			BodyChunk bodyChunk2 = base.firstChunk;
			if (base.Submersion == 1f)
			{
				bodyChunk2.vel.y = bodyChunk2.vel.y + (room.PointSubmerged(bodyChunk2.pos + new Vector2(0f, 20f)) ? 0.91f : 0.9f);
			}
			if (light != null)
			{
				light.stayAlive = true;
			}
		}
		else
		{
			lightFade *= 0.99f;
			base.buoyancy = 0.99f;
		}
		if (light == null && lightFade > 0.1f)
		{
			light = new LightSource(base.firstChunk.pos, environmentalLight: true, flowerColor, this);
			room.AddObject(light);
			light.colorFromEnvironment = false;
			light.noGameplayImpact = true;
			light.stayAlive = true;
			light.requireUpKeep = true;
		}
		else if (light != null)
		{
			light.HardSetPos(RootAttachPos(1f));
			light.HardSetRad(LightRad * lightFade);
			light.HardSetAlpha(Mathf.Lerp(0f, lightFade, room.Darkness(light.Pos)) / 4f);
			if (light.rad > 5f)
			{
				light.stayAlive = true;
			}
		}
	}

	private void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
	{
		stuckInObject = result.obj;
		ChangeMode(Mode.StuckInCreature);
		if (result.chunk != null)
		{
			stuckInChunkIndex = result.chunk.index;
			if (stuckBodyPart == -1)
			{
				stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
			}
			base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
			Custom.Log("Add spear to creature chunk", stuckInChunk.index.ToString());
			new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, stuckInChunkIndex, stuckBodyPart, stuckRotation);
		}
		else if (result.onAppendagePos != null)
		{
			stuckInChunkIndex = 0;
			stuckInAppendage = result.onAppendagePos;
			stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
			Custom.Log("Add spear to creature Appendage");
			new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, stuckRotation);
		}
		if (room.BeingViewed)
		{
			for (int i = 0; i < 8; i++)
			{
				room.AddObject(new WaterDrip(result.collisionPoint, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		int num = flowerLeavesCount * 2;
		sLeaser.sprites = new FSprite[1 + num];
		sLeaser.sprites[0] = new FSprite("SmallSpear");
		sLeaser.sprites[0].scaleY = 2f;
		sLeaser.sprites[0].scaleX = 0.8f;
		for (int i = 0; i < flowerLeavesCount; i++)
		{
			sLeaser.sprites[1 + i * 2] = new FSprite("DangleFruit0A");
			sLeaser.sprites[2 + i * 2] = new FSprite("DangleFruit0B");
			sLeaser.sprites[1 + i * 2].anchorY = 0.02f;
			sLeaser.sprites[2 + i * 2].anchorY = 0.02f;
			sLeaser.sprites[1 + i * 2].scaleX = Mathf.Lerp(0.5f, 1f, (float)i / (float)flowerLeavesCount - 2f);
			sLeaser.sprites[2 + i * 2].scaleX = Mathf.Lerp(0.5f, 1f, (float)i / (float)flowerLeavesCount - 2f);
			sLeaser.sprites[1 + i * 2].scaleY = Mathf.Lerp(0.9f, 10f, (float)i / (float)flowerLeavesCount - 2f);
			sLeaser.sprites[2 + i * 2].scaleY = Mathf.Lerp(0.9f, 10f, (float)i / (float)flowerLeavesCount - 2f);
		}
		sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].scaleX = sLeaser.sprites[1].scaleX;
		sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].scaleY = sLeaser.sprites[1].scaleY;
		sLeaser.sprites[2 + (flowerLeavesCount - 2) * 2].scaleX = sLeaser.sprites[1].scaleX;
		sLeaser.sprites[2 + (flowerLeavesCount - 2) * 2].scaleY = sLeaser.sprites[1].scaleY;
		sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].scaleX = sLeaser.sprites[1].scaleX;
		sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].scaleY = sLeaser.sprites[1].scaleY;
		sLeaser.sprites[2 + (flowerLeavesCount - 1) * 2].scaleX = sLeaser.sprites[1].scaleX;
		sLeaser.sprites[2 + (flowerLeavesCount - 1) * 2].scaleY = sLeaser.sprites[1].scaleY;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = Color.Lerp(new Color(0.04f, 0.35f, 0.2f), palette.blackColor, darkness);
		sLeaser.sprites[0].color = color;
		for (int i = 0; i < flowerLeavesCount; i++)
		{
			Color a = Color.Lerp(color, flowerColor, Mathf.Clamp(lightFade, 0.3f - 0.3f * darkness, 1f));
			sLeaser.sprites[1 + i * 2].color = Color.Lerp(a, color, (float)i / ((float)flowerLeavesCount / 2f));
			sLeaser.sprites[2 + i * 2].color = Color.Lerp(sLeaser.sprites[1 + i * 2].color, Color.Lerp(new Color(1f, 1f, 1f), palette.blackColor, darkness), darkness / 20f);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (darkness != lastDarkness || oldLightFade != lightFade)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		if (vibrate > 0)
		{
			pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
		}
		Vector3 vector = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(vector);
		sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("DangleFruit" + Custom.IntClamp(3 - AbstrLillyPuck.bites, 0, 2) + "A");
		sLeaser.sprites[0].anchorY = Mathf.Lerp((!lastPivotAtTip) ? 0.5f : 0.85f, (!pivotAtTip) ? 0.5f : 0.85f, timeStacker);
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector);
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[0].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		Vector2 vector2 = RootAttachPos(timeStacker);
		for (int i = 0; i < flowerLeavesCount; i++)
		{
			float num = ((i % 2 == 0) ? 1f : (-1f)) * (90f * ((float)i / ((float)flowerLeavesCount - 2f)));
			sLeaser.sprites[1 + i * 2].x = vector2.x - camPos.x;
			sLeaser.sprites[1 + i * 2].y = vector2.y - camPos.y;
			sLeaser.sprites[1 + i * 2].rotation = sLeaser.sprites[0].rotation + 180f + base.firstChunk.vel.x * 4f + num;
			sLeaser.sprites[2 + i * 2].x = sLeaser.sprites[1 + i * 2].x;
			sLeaser.sprites[2 + i * 2].y = sLeaser.sprites[1 + i * 2].y;
			sLeaser.sprites[2 + i * 2].rotation = sLeaser.sprites[1 + i * 2].rotation;
		}
		sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].x = vector2.x - camPos.x;
		sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].y = vector2.y - camPos.y;
		sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].rotation = sLeaser.sprites[0].rotation - 8f;
		sLeaser.sprites[2 + (flowerLeavesCount - 2) * 2].x = sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].x;
		sLeaser.sprites[2 + (flowerLeavesCount - 2) * 2].y = sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].y;
		sLeaser.sprites[2 + (flowerLeavesCount - 2) * 2].rotation = sLeaser.sprites[1 + (flowerLeavesCount - 2) * 2].rotation;
		sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].x = vector2.x - camPos.x;
		sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].y = vector2.y - camPos.y;
		sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].rotation = sLeaser.sprites[0].rotation - 8f;
		sLeaser.sprites[2 + (flowerLeavesCount - 1) * 2].x = sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].x;
		sLeaser.sprites[2 + (flowerLeavesCount - 1) * 2].y = sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].y;
		sLeaser.sprites[2 + (flowerLeavesCount - 1) * 2].rotation = sLeaser.sprites[1 + (flowerLeavesCount - 1) * 2].rotation;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void PulledOutOfStuckObject()
	{
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
			{
				abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject)
			{
				abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
		}
		stuckInObject = null;
		stuckInAppendage = null;
		stuckInChunkIndex = 0;
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		bool flag = false;
		if (abstractPhysicalObject.world.game.IsArenaSession && abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && thrownBy != null && thrownBy is Player && result.obj is Creature)
		{
			flag = true;
			if ((result.obj as Creature).State is HealthState && ((result.obj as Creature).State as HealthState).health <= 0f)
			{
				flag = false;
			}
			else if (!((result.obj as Creature).State is HealthState) && (result.obj as Creature).State.dead)
			{
				flag = false;
			}
		}
		if (result.obj is Creature)
		{
			if (!(result.obj is Player) || (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel))
			{
				float num = spearDamageBonus;
				(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass * 2f, result.chunk, result.onAppendagePos, Creature.DamageType.Stab, num, 10f);
				if (result.obj is Player)
				{
					Player player = result.obj as Player;
					player.playerState.permanentDamageTracking += num / player.Template.baseDamageResistance;
					if (player.playerState.permanentDamageTracking >= 1.0)
					{
						player.Die();
					}
				}
			}
		}
		else if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel))
		{
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
			LodgeInCreature(result, eu);
			if (flag)
			{
				abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
			}
			return true;
		}
		room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
		vibrate = 20;
		ChangeMode(Mode.Free);
		base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
		SetRandomSpin();
		return false;
	}

	public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
	{
		base.HitSomethingWithoutStopping(obj, chunk, appendage);
	}

	public override void PickedUp(Creature upPicker)
	{
		ChangeMode(Mode.Carried);
		room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, base.firstChunk);
	}

	public void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
	{
		stuckBodyPart = bodyPart.bodyPartArrayIndex;
		stuckRotation = Custom.Angle(base.firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
		bodyPart.vel += base.firstChunk.vel;
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
			{
				AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
				stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
				stuckInChunkIndex = abstractSpearStick.chunk;
				stuckBodyPart = abstractSpearStick.bodyPart;
				stuckRotation = abstractSpearStick.angle;
				ChangeMode(Mode.StuckInCreature);
			}
			else if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
			{
				AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
				stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
				stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
				stuckRotation = abstractSpearAppendageStick.angle;
				ChangeMode(Mode.StuckInCreature);
			}
		}
	}

	public override void SetRandomSpin()
	{
		if (room != null)
		{
			rotationSpeed = ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f)) * Mathf.Lerp(50f, 150f, UnityEngine.Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
		}
		spinning = true;
	}

	public virtual void TryImpaleSmallCreature(Creature smallCrit)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick)
			{
				if ((abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == num2)
				{
					num2++;
				}
				num++;
			}
		}
		if (num <= 5 && num2 < 5)
		{
			new AbstractPhysicalObject.ImpaledOnSpearStick(abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
		}
	}

	private Vector2 RagAttachPos(float timeStacker)
	{
		Vector3 vector = Vector3.Slerp(lastRotation, rotation, timeStacker) * (15f * (lastPivotAtTip ? 2f : 1f));
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + new Vector2(vector.x, vector.y);
	}

	public new void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[i]);
		}
	}

	public Vector2 RootAttachPos(float timeStacker)
	{
		Vector3 vector = Vector3.Slerp(lastRotation, rotation, timeStacker) * (-15f * (lastPivotAtTip ? 0f : 1f));
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + new Vector2(vector.x, vector.y);
	}
}
