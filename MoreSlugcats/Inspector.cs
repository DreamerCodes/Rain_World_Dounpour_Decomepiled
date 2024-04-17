using System.Collections.Generic;
using CoralBrain;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class Inspector : Creature, PhysicalObject.IHaveAppendages, IOwnProjectedCircles
{
	public class InspectorState : HealthState
	{
		public float[] headHealth;

		public float[] headSize;

		public int Wingnumber;

		public InspectorState(AbstractCreature creature)
			: base(creature)
		{
			int randomSeed = creature.ID.RandomSeed;
			Wingnumber = 3 + (int)Mathf.Floor(creature.world.game.SeededRandom(randomSeed) * 8f);
			headHealth = new float[3];
			headSize = new float[3];
			for (int i = 0; i < 3; i++)
			{
				headHealth[i] = 3f;
				headSize[i] = creature.world.game.SeededRandom(randomSeed + i);
			}
		}
	}

	public InspectorAI AI;

	private ProjectionCircle projectionCircle;

	private LightSource myLight;

	public float lightpulse;

	public CoralNeuronSystem neuronSystem;

	public LightSource mySuperGlow;

	public Vector2 GoalPos;

	private Vector2 LastGoalPos;

	public Vector2 flyingPower;

	public List<IntVector2> pastPositions;

	public int stuckCounter;

	private Vector2 stuckPos;

	private int notFollowingPathToCurrentGoalCounter;

	public float squeezeFac;

	private bool squeeze;

	private Vector2 moveDirection;

	public Tentacle[] heads;

	private LightSource[] headlights;

	public int activeEye;

	public PhysicalObject[] headCuriosityFocus;

	public static int attentionDelayMax = 100;

	public float anger;

	public BodyChunk[] headGrabChunk;

	public BodyChunk[] headWantToGrabChunk;

	private int ownerIterator;

	public List<Vector2> DangerousThrowLocations;

	public float dying;

	public float lastDying;

	private List<PlacedObject> antiStrandingZones;

	public Vector2 controlAimDirection;

	public Color bodyColor
	{
		get
		{
			if (base.stun <= 0 && grabbedBy.Count == 0 && !GrabbedByDaddyCorruption)
			{
				return Color.Lerp(TrueColor, Color.red, anger * 0.78f);
			}
			if (Random.value > 0.7f)
			{
				Color black = Color.black;
				black.a = 0f;
				return black;
			}
			return Color.Lerp(TrueColor, Color.red, 0.86f);
		}
	}

	public Color TrueColor
	{
		get
		{
			if (room == null)
			{
				return Color.black;
			}
			Vector3 vector = Custom.RGB2HSL(OwneriteratorColor);
			HSLColor hSLColor = new HSLColor(vector.x, vector.y, vector.z);
			hSLColor.saturation = 0.9f;
			hSLColor.hue += room.world.game.SeededRandom(base.abstractCreature.ID.RandomSeed + 20) / 20f;
			return hSLColor.rgb;
		}
	}

	public override Vector2 VisionPoint
	{
		get
		{
			if (activeEye == -1)
			{
				return base.mainBodyChunk.pos;
			}
			return heads[activeEye].Tip.pos;
		}
	}

	public Color OwneriteratorColor
	{
		get
		{
			if (ownerIterator == 1)
			{
				return new Color(1f, 0.8f, 0.3f);
			}
			if (ownerIterator == 2)
			{
				return new Color(0f, 1f, 0f);
			}
			if (ownerIterator == 3)
			{
				return new Color(1f, 0.2f, 0f);
			}
			return new Color(38f / 85f, 46f / 51f, 0.76862746f);
		}
	}

	public Inspector(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[1];
		bodyChunkConnections = new BodyChunkConnection[0];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 2.6f, 2.15f);
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.3f;
		surfaceFriction = 0.87f;
		surfaceFriction = 0.4f;
		lightpulse = 0f;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.01f;
		ChangeCollisionLayer(1);
		ownerIterator = -1;
		squeeze = false;
		squeezeFac = 1f;
		GoalPos = base.mainBodyChunk.pos;
		LastGoalPos = base.mainBodyChunk.pos;
		heads = new Tentacle[headCount()];
		headlights = new LightSource[headCount()];
		headCuriosityFocus = new PhysicalObject[headCount()];
		headGrabChunk = new BodyChunk[headCount()];
		headWantToGrabChunk = new BodyChunk[headCount()];
		appendages = new List<Appendage>();
		DangerousThrowLocations = new List<Vector2>();
		antiStrandingZones = new List<PlacedObject>();
		for (int i = 0; i < heads.Length; i++)
		{
			heads[i] = new Tentacle(this, base.mainBodyChunk, 300f);
			heads[i].tProps = new Tentacle.TentacleProps(stiff: false, rope: true, shorten: true, 0.5f, 0f, 0.5f, 0.05f, 0.05f, 2.2f, 12f, 1f / 3f, 5f, 15, 60, 12, 20);
			heads[i].tChunks = new Tentacle.TentacleChunk[8];
			for (int j = 0; j < heads[i].tChunks.Length; j++)
			{
				heads[i].tChunks[j] = new Tentacle.TentacleChunk(heads[i], j, (float)(j + 1) / (float)heads[i].tChunks.Length, Mathf.Lerp(3f, 5f, (float)j / (float)(heads[i].tChunks.Length - 1)));
			}
			heads[i].stretchAndSqueeze = 0.1f;
			appendages.Add(new Appendage(this, i, heads[i].tChunks.Length));
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null || base.abstractCreature.InDen)
		{
			return;
		}
		if (squeeze || enteringShortCut.HasValue)
		{
			squeezeFac = Mathf.Min(1f, squeezeFac + 0.02f);
		}
		else
		{
			squeezeFac = Mathf.Max(0f, squeezeFac - 1f / 30f);
		}
		if (squeezeFac > 0.8f)
		{
			for (int i = 0; i < heads.Length; i++)
			{
				for (int j = 0; j < heads[i].tChunks.Length; j++)
				{
					heads[i].tChunks[j].pos = Vector2.Lerp(heads[i].tChunks[j].pos, base.mainBodyChunk.pos, Custom.LerpMap(squeezeFac, 0.8f, 1f, 0f, 0.5f));
				}
			}
		}
		myLight.HardSetPos(base.mainBodyChunk.pos);
		mySuperGlow.HardSetPos(base.mainBodyChunk.pos);
		lightpulse += 0.03f;
		float num = 6f + Mathf.Sin(AI.AttentionGrabberTimer * 4f) * 6f * AI.AttentionGrabberTimer / (float)attentionDelayMax;
		float num2 = Mathf.InverseLerp(1f, 0.8f, squeezeFac);
		myLight.HardSetRad(210f + num + 20f * Mathf.Sin(lightpulse) + 1900f * dying);
		myLight.HardSetAlpha((0.9f + Mathf.Sin(lightpulse) / 10f) * num2 + dying);
		mySuperGlow.HardSetRad(50f + num + 10f * Mathf.Sin(lightpulse) + 90f * dying);
		mySuperGlow.HardSetAlpha(num2 + dying);
		for (int k = 0; k < headCount(); k++)
		{
			headlights[k].HardSetRad(60f + 10f * Mathf.Sin(lightpulse + (float)k) - 1f * dying);
			headlights[k].HardSetAlpha(0.23f * num2 + dying);
			Vector2 lastPos = heads[k].tChunks[heads[k].tChunks.Length - 2].lastPos;
			Vector2 lastPos2 = heads[k].Tip.lastPos;
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(lastPos, lastPos2));
			vector *= 9f;
			headlights[k].HardSetPos(heads[k].Tip.pos + vector);
			heads[k].Update();
			heads[k].limp = !base.Consious;
		}
		lastDying = dying;
		if (base.dead)
		{
			squeezeFac = 0.2f;
			if (squeezeFac > 1f)
			{
				squeezeFac = 1f;
			}
			base.mainBodyChunk.vel += Custom.RNV() * Random.value * 4f * dying;
			base.mainBodyChunk.pos += Custom.RNV() * Random.value * 4f * dying;
			if (lastDying == 0f)
			{
				room.PlaySound(SoundID.Overseer_Death, base.mainBodyChunk.pos);
				if (room.ViewedByAnyCamera(base.mainBodyChunk.pos, 900f))
				{
					for (int l = 0; l < headCount(); l++)
					{
						int num3 = Random.Range(0, heads[l].tChunks.Length);
						room.AddObject(new ShockWave(heads[l].tChunks[num3].pos, Random.Range(40, 160), 0.05f, 7));
					}
				}
				if (base.graphicsModule != null)
				{
					for (int m = 0; m < headCount(); m++)
					{
						AbstractPhysicalObject abstractPhysicalObject = new OverseerCarcass.AbstractOverseerCarcass(room.world, null, base.abstractPhysicalObject.pos, room.game.GetNewID(), OwneriteratorColor, ownerIterator);
						(abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode = true;
						room.abstractRoom.AddEntity(abstractPhysicalObject);
						abstractPhysicalObject.pos = base.abstractCreature.pos;
						abstractPhysicalObject.RealizeInRoom();
						abstractPhysicalObject.realizedObject.firstChunk.HardSetPosition(heads[m].Tip.pos);
						abstractPhysicalObject.realizedObject.firstChunk.vel = heads[m].Tip.vel;
						(abstractPhysicalObject.realizedObject as OverseerCarcass).sparkling = 1f;
						(abstractPhysicalObject.realizedObject as OverseerCarcass).rotation = Custom.VecToDeg(getHeadDirection(m));
						(abstractPhysicalObject.realizedObject as OverseerCarcass).lastRotation = (abstractPhysicalObject.realizedObject as OverseerCarcass).rotation;
					}
				}
			}
			dying = Mathf.Min(1f, dying + 1f / 6f);
			if (dying >= 1f)
			{
				Die();
				Destroy();
			}
			return;
		}
		if (antiStrandingZones.Count > 0)
		{
			for (int n = 0; n < antiStrandingZones.Count; n++)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, antiStrandingZones[n].pos, 1000f))
				{
					float rad = (antiStrandingZones[n].data as PlacedObject.ResizableObjectData).Rad;
					Vector2 normalized = (antiStrandingZones[n].data as PlacedObject.ResizableObjectData).handlePos.normalized;
					if (Custom.DistLess(base.firstChunk.pos, antiStrandingZones[n].pos, rad))
					{
						base.bodyChunks[0].vel += normalized * 2.4f * Mathf.InverseLerp(rad, rad / 5f, Vector2.Distance(base.firstChunk.pos, antiStrandingZones[n].pos));
					}
				}
			}
		}
		float num4 = (1f + Mathf.Sin(AI.AttentionGrabberTimer / 2f) / 2f) * AI.AttentionGrabberTimer / ((float)attentionDelayMax * 2f);
		if (AI.behavior == InspectorAI.Behavior.InspectArea)
		{
			float b = Mathf.Max(0.15f, (0.25f + Mathf.Sin(AI.newIdlePosCounter) / 5f) * ((float)AI.newIdlePosCounter / 2500f));
			num4 = Mathf.Max(num4, b);
		}
		if (!base.Consious)
		{
			num4 = 0f;
		}
		myLight.color = bodyColor;
		mySuperGlow.color = Color.Lerp(bodyColor, Color.blue, num4);
		if (neuronSystem == null)
		{
			neuronSystem = new CoralNeuronSystem();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, new Vector2(Input.mousePosition.x, Input.mousePosition.y) + room.game.cameras[0].pos) * 5f;
			base.Stun(12);
		}
		if (base.Consious)
		{
			if (Random.value > 0.6f)
			{
				activeEye = Random.Range(0, headCount());
				bool flag = true;
				if (base.graphicsModule != null && (base.graphicsModule as InspectorGraphics).blinks[activeEye] > 0f)
				{
					flag = false;
				}
				if (HeadsCrippled(activeEye) || !flag)
				{
					activeEye = -1;
				}
			}
			Act();
		}
		else
		{
			for (int num5 = 0; num5 < headCount(); num5++)
			{
				headGrabChunk[num5] = null;
				headWantToGrabChunk[num5] = null;
			}
			activeEye = -1;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (ownerIterator == -1)
		{
			if (room.game.IsStorySession && room.world.region != null)
			{
				if (room.world.region.name == "SS")
				{
					ownerIterator = 0;
				}
				else if (room.world.region.name == "MS" || room.world.region.name == "DM")
				{
					ownerIterator = 1;
				}
			}
			else
			{
				ownerIterator = room.game.SeededRandomRange(base.abstractCreature.ID.RandomSeed, 0, 5);
			}
		}
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new InspectorGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public Vector2 CircleCenter(int index, float timeStacker)
	{
		return base.mainBodyChunk.pos;
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		antiStrandingZones.Clear();
		for (int i = 0; i < newRoom.roomSettings.placedObjects.Count; i++)
		{
			if (newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.NoLeviathanStrandingZone)
			{
				antiStrandingZones.Add(newRoom.roomSettings.placedObjects[i]);
			}
		}
		DangerousThrowLocations.Clear();
		if (newRoom.zapCoils != null && newRoom.zapCoils.Count > 0)
		{
			foreach (ZapCoil zapCoil in newRoom.zapCoils)
			{
				Vector2 item = new Vector2(Mathf.Lerp(zapCoil.rect.left, zapCoil.rect.right, 0.5f) * 20f, Mathf.Lerp(zapCoil.rect.bottom, zapCoil.rect.top, 0.5f) * 20f);
				DangerousThrowLocations.Add(item);
				Custom.Log("Zap coil added to hazards", item.ToString());
			}
		}
		if (newRoom.roomSettings.placedObjects != null && newRoom.roomSettings.placedObjects.Count > 0)
		{
			foreach (PlacedObject placedObject in newRoom.roomSettings.placedObjects)
			{
				if (placedObject.type == PlacedObject.Type.Corruption)
				{
					Vector2 pos = placedObject.pos;
					DangerousThrowLocations.Add(pos);
					Custom.Log("Corruption added to hazards", pos.ToString());
				}
			}
		}
		if (myLight != null)
		{
			myLight.RemoveFromRoom();
			myLight.Destroy();
		}
		if (mySuperGlow != null)
		{
			mySuperGlow.RemoveFromRoom();
			mySuperGlow.Destroy();
		}
		for (int j = 0; j < headCount(); j++)
		{
			if (headlights[j] != null)
			{
				headlights[j].RemoveFromRoom();
				headlights[j].Destroy();
			}
			headlights[j] = new LightSource(base.mainBodyChunk.pos, environmentalLight: false, Color.white, this);
			headlights[j].flat = true;
			room.AddObject(headlights[j]);
		}
		myLight = new LightSource(base.mainBodyChunk.pos, environmentalLight: false, bodyColor, this);
		room.AddObject(myLight);
		mySuperGlow = new LightSource(base.mainBodyChunk.pos, environmentalLight: false, bodyColor, this);
		mySuperGlow.flat = true;
		room.AddObject(mySuperGlow);
		UpdateCoralNeuronSystem();
		for (int k = 0; k < headCount(); k++)
		{
			heads[k].NewRoom(newRoom);
		}
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
		{
			room.AddObject(new ProjectedCircle(newRoom, this, 0, 300f));
		}
		pastPositions = new List<IntVector2>();
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is Creature)
		{
			AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
			AI.tracker.RepresentationForObject(otherObject, AddIfMissing: false);
			if (activeEye != -1)
			{
				headCuriosityFocus[activeEye] = otherObject;
			}
		}
	}

	private void UpdateCoralNeuronSystem()
	{
		if (neuronSystem != null && base.graphicsModule != null)
		{
			for (int i = 0; i < (base.graphicsModule as InspectorGraphics).mycelia.Length; i++)
			{
				neuronSystem.mycelia.Remove((base.graphicsModule as InspectorGraphics).mycelia[i]);
			}
		}
		neuronSystem = null;
		for (int num = room.updateList.Count - 1; num >= 0; num--)
		{
			if (room.updateList[num] is CoralNeuronSystem)
			{
				neuronSystem = room.updateList[num] as CoralNeuronSystem;
				break;
			}
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as InspectorGraphics).UpdateNeuronSystemForMycelia();
		}
	}

	public bool CanGrabCritter(AbstractCreature creature)
	{
		if (base.abstractCreature != creature && creature.realizedCreature != null && creature.creatureTemplate.TopAncestor().type != CreatureTemplate.Type.DaddyLongLegs && creature.creatureTemplate.TopAncestor().type != MoreSlugcatsEnums.CreatureTemplateType.Inspector)
		{
			if (!(creature.creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Overseer))
			{
				if (ModManager.MMF)
				{
					return MMF.cfgVanillaExploits.Value;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void Act()
	{
		AI.Update();
		if (base.safariControlled)
		{
			if (controlAimDirection == Vector2.zero)
			{
				controlAimDirection = Custom.RNV() * 80f;
			}
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.AnyDirectionalInput)
			{
				controlAimDirection = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 80f;
			}
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)
			{
				float num = float.MaxValue;
				float current = Custom.VecToDeg(controlAimDirection);
				if (anger < 0.5f)
				{
					Creature creature = null;
					for (int i = 0; i < base.abstractCreature.Room.creatures.Count; i++)
					{
						if (CanGrabCritter(base.abstractCreature.Room.creatures[i]))
						{
							float target = Custom.AimFromOneVectorToAnother(base.mainBodyChunk.pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
							float num2 = Custom.Dist(base.mainBodyChunk.pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
							if (Mathf.Abs(Mathf.DeltaAngle(current, target)) < 22.5f && num2 < num)
							{
								num = num2;
								creature = base.abstractCreature.Room.creatures[i].realizedCreature;
							}
						}
					}
					if (creature != null)
					{
						AI.OrderAHeadToGrabObject(creature);
					}
				}
				else if (room != null)
				{
					Weapon weapon = null;
					for (int j = 0; j < room.physicalObjects.Length; j++)
					{
						for (int k = 0; k < room.physicalObjects[j].Count; k++)
						{
							if (room.physicalObjects[j][k] is Weapon)
							{
								float target2 = Custom.AimFromOneVectorToAnother(base.mainBodyChunk.pos, room.physicalObjects[j][k].firstChunk.pos);
								float num3 = Custom.Dist(base.mainBodyChunk.pos, room.physicalObjects[j][k].firstChunk.pos);
								if (Mathf.Abs(Mathf.DeltaAngle(current, target2)) < 22.5f && num3 < num)
								{
									num = num3;
									weapon = room.physicalObjects[j][k] as Weapon;
								}
							}
						}
					}
					if (weapon != null)
					{
						AI.OrderAHeadToGrabObject(weapon);
					}
				}
			}
		}
		for (int l = 0; l < headCount(); l++)
		{
			if ((base.State as InspectorState).headHealth[l] <= 0f)
			{
				heads[l].limp = true;
				headWantToGrabChunk[l] = null;
				headGrabChunk[l] = null;
				for (int m = 0; m < heads[l].tChunks.Length; m++)
				{
					heads[l].tChunks[m].vel *= 0.9f;
					if (heads[l].tChunks[m].vel.y > -3f)
					{
						heads[l].tChunks[m].vel += new Vector2(0f, -0.2f * room.gravity);
					}
					_ = heads[l].tChunks[m];
				}
			}
			else if (headWantToGrabChunk[l] != null)
			{
				if (headWantToGrabChunk[l].owner.room != room)
				{
					headWantToGrabChunk[l] = null;
					heads[l].floatGrabDest = AI.HeadGoal(l);
					Custom.Log("Inspector stopped grab due to not the same room");
					continue;
				}
				if (headWantToGrabChunk[l].owner is SSOracleSwarmer && headWantToGrabChunk[l].owner.grabbedBy.Count == 0)
				{
					headWantToGrabChunk[l] = null;
					heads[l].floatGrabDest = AI.HeadGoal(l);
					Custom.Log("Inspector stopped neuron grab due to release");
					continue;
				}
				heads[l].floatGrabDest = headWantToGrabChunk[l].pos;
				if (Vector2.Distance(heads[l].Tip.pos, headWantToGrabChunk[l].pos) < 25f)
				{
					headGrabChunk[l] = headWantToGrabChunk[l];
					if (headGrabChunk[l].owner is Player)
					{
						room.PlaySound(SoundID.Vulture_Grab_Player, headGrabChunk[l].pos);
					}
					else if (headGrabChunk[l].owner is Weapon)
					{
						room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, headGrabChunk[l].pos);
						if (headGrabChunk[l].owner is Spear)
						{
							(headGrabChunk[l].owner as Spear).PulledOutOfStuckObject();
							(headGrabChunk[l].owner as Spear).PickedUp(this);
							(headGrabChunk[l].owner as Spear).ChangeMode(Weapon.Mode.Free);
						}
					}
					else
					{
						room.PlaySound(SoundID.Vulture_Grab_NPC, headGrabChunk[l].pos);
					}
					headWantToGrabChunk[l] = null;
					headGrabChunk[l].pos = heads[l].Tip.pos + getHeadDirection(l) * 5f;
					headGrabChunk[l].vel *= 0f;
					heads[l].floatGrabDest = AI.HeadGoal(l);
				}
				else if (HeadWeaponized(l) && Vector2.Distance(base.mainBodyChunk.pos, headWantToGrabChunk[l].pos) > 800f)
				{
					headWantToGrabChunk[l] = null;
				}
				else if (!HeadWeaponized(l) && Vector2.Distance(heads[l].Tip.pos, headWantToGrabChunk[l].pos) > 1300f)
				{
					headWantToGrabChunk[l] = null;
				}
				else
				{
					if (base.safariControlled && !(anger > 0.5f))
					{
						continue;
					}
					for (int n = 0; n < AI.itemTracker.ItemCount; n++)
					{
						ItemTracker.ItemRepresentation rep = AI.itemTracker.GetRep(n);
						PhysicalObject realizedObject = rep.representedItem.realizedObject;
						if (realizedObject != null && rep.VisualContact && Vector2.Distance((realizedObject as Weapon).firstChunk.pos, base.mainBodyChunk.pos) < 400f && Vector2.Distance((realizedObject as Weapon).firstChunk.pos, heads[l].Tip.pos) > 10f && !isOtherHeadsGoalChunk(l, (realizedObject as Weapon).firstChunk) && (realizedObject as Weapon).mode != Weapon.Mode.Thrown && Vector2.Distance(heads[l].Tip.pos, (realizedObject as Weapon).firstChunk.pos) < Vector2.Distance(heads[l].Tip.pos, headWantToGrabChunk[l].pos))
						{
							headWantToGrabChunk[l] = (realizedObject as Weapon).firstChunk;
						}
					}
				}
			}
			else if (headGrabChunk[l] != null && headGrabChunk[l].owner.room != room)
			{
				headGrabChunk[l] = null;
				heads[l].floatGrabDest = AI.HeadGoal(l);
			}
			else if (headGrabChunk[l] != null)
			{
				headGrabChunk[l].owner.AllGraspsLetGoOfThisObject(evenNonExlusive: true);
				headWantToGrabChunk[l] = null;
				headGrabChunk[l].pos = heads[l].Tip.pos + getHeadDirection(l) * 25f;
				headGrabChunk[l].vel *= 0f;
				if (!HeadWeaponized(l))
				{
					heads[l].floatGrabDest = Vector2.Lerp(heads[l].Tip.pos, base.mainBodyChunk.pos, 0.24f);
				}
				_ = headGrabChunk[l];
				if (!HeadWeaponized(l) && Random.value < 0.1f && Vector2.Distance(heads[l].Tip.pos, base.mainBodyChunk.pos) < 80f && !base.safariControlled)
				{
					BodyChunk[] array = headGrabChunk[l].owner.bodyChunks;
					for (int num4 = 0; num4 < array.Length; num4++)
					{
						array[num4].vel = Vector2.Lerp(headGrabChunk[l].vel * 0.001f, headGrabChunk[l].vel, anger);
					}
					headGrabChunk[l] = null;
				}
				if (base.safariControlled && !HeadWeaponized(l) && inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					BodyChunk[] array2 = headGrabChunk[l].owner.bodyChunks;
					if (inputWithDiagonals.Value.AnyDirectionalInput)
					{
						for (int num5 = 0; num5 < array2.Length; num5++)
						{
							array2[num5].vel = Vector2.Lerp(headGrabChunk[l].vel * 0.001f, headGrabChunk[l].vel, anger) + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 15f;
						}
						room.PlaySound(SoundID.Vulture_Peck, heads[l].Tip.pos);
					}
					else
					{
						for (int num6 = 0; num6 < array2.Length; num6++)
						{
							array2[num6].vel = Vector2.Lerp(headGrabChunk[l].vel * 0.001f, headGrabChunk[l].vel, anger);
						}
					}
					headGrabChunk[l] = null;
				}
				else
				{
					if (!HeadWeaponized(l))
					{
						continue;
					}
					if (headGrabChunk[l].owner is Spear)
					{
						(headGrabChunk[l].owner as Spear).setRotation = Custom.PerpendicularVector(getHeadDirection(l));
					}
					Creature creature2 = null;
					Vector2 vector = Vector2.zero;
					int num7 = 0;
					if (!base.safariControlled && AI.preyTracker.MostAttractivePrey != null)
					{
						creature2 = AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature;
						vector = AI.preyTracker.MostAttractivePrey.lastSeenCoord.Tile.ToVector2() * 20f;
						num7 = AI.preyTracker.MostAttractivePrey.lastSeenCoord.room;
					}
					if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw)
					{
						float num8 = float.MaxValue;
						float current2 = Custom.VecToDeg(controlAimDirection);
						Creature creature3 = null;
						for (int num9 = 0; num9 < base.abstractCreature.Room.creatures.Count; num9++)
						{
							if (base.abstractCreature != base.abstractCreature.Room.creatures[num9] && base.abstractCreature.Room.creatures[num9].realizedCreature != null)
							{
								float target3 = Custom.AimFromOneVectorToAnother(heads[l].Tip.pos, base.abstractCreature.Room.creatures[num9].realizedCreature.mainBodyChunk.pos);
								float num10 = Custom.Dist(heads[l].Tip.pos, base.abstractCreature.Room.creatures[num9].realizedCreature.mainBodyChunk.pos);
								if (Mathf.Abs(Mathf.DeltaAngle(current2, target3)) < 45f && num10 < num8)
								{
									num8 = num10;
									creature3 = base.abstractCreature.Room.creatures[num9].realizedCreature;
								}
							}
						}
						if (creature3 != null)
						{
							creature2 = creature3;
							vector = creature3.mainBodyChunk.pos;
							num7 = creature3.abstractCreature.Room.index;
						}
						else if (inputWithDiagonals.Value.AnyDirectionalInput)
						{
							creature2 = this;
							vector = heads[l].Tip.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f;
							num7 = base.abstractCreature.Room.index;
						}
						else
						{
							creature2 = this;
							vector = Custom.RNV() * 400f;
							num7 = base.abstractCreature.Room.index;
						}
					}
					if (creature2 != null)
					{
						Vector2 vector2 = vector;
						if (num7 == room.abstractRoom.index)
						{
							Vector2 pos = heads[l].Tip.pos;
							Vector2 vector3 = Custom.DirVec(headGrabChunk[l].pos, vector2);
							if (!heads[l].floatGrabDest.HasValue)
							{
								heads[l].Tip.vel += vector3 * 40f;
								if (!(Vector2.Distance(headGrabChunk[l].pos, base.mainBodyChunk.pos) > 80f) || !(Vector2.Distance(headGrabChunk[l].pos, vector2) < Vector2.Distance(vector2, base.mainBodyChunk.pos)))
								{
									continue;
								}
								if (heads[l].Tip.vel.magnitude > 25f)
								{
									IntVector2 a = IntVector2.FromVector2(vector3.normalized * 2f);
									a = IntVector2.ClampAtOne(a);
									if (a.x != 0 || a.y != 0)
									{
										Custom.Log($"Inspector throw weapon {headGrabChunk[l].owner}");
										Custom.Log("Dir", a.ToString());
										(headGrabChunk[l].owner as Weapon).Thrown(this, headGrabChunk[l].pos, headGrabChunk[l].pos - a.ToVector2() * 15f, a, 1f, eu: true);
										headGrabChunk[l] = null;
										headWantToGrabChunk[l] = null;
									}
								}
								else
								{
									heads[l].floatGrabDest = base.mainBodyChunk.pos;
								}
							}
							else if ((Vector2.Distance(headGrabChunk[l].pos, base.mainBodyChunk.pos) > 80f && room.RayTraceTilesForTerrain(IntVector2.FromVector2(pos / 20f).x, IntVector2.FromVector2(pos / 20f).y, IntVector2.FromVector2(headGrabChunk[l].pos / 20f).x, IntVector2.FromVector2(headGrabChunk[l].pos / 20f).y) && Vector2.Distance(headGrabChunk[l].pos, vector2) > Vector2.Distance(vector2, base.mainBodyChunk.pos)) || Vector2.Distance(headGrabChunk[l].pos, vector2) < 10f)
							{
								heads[l].floatGrabDest = null;
								heads[l].tChunks[heads[l].tChunks.Length - 1].vel += vector3 * 80f;
								heads[l].tChunks[heads[l].tChunks.Length - 2].vel += vector3 * 80f;
								heads[l].tChunks[heads[l].tChunks.Length - 3].vel += vector3 * 80f;
								heads[l].tChunks[heads[l].tChunks.Length - 4].vel += vector3 * 80f;
								heads[l].tChunks[heads[l].tChunks.Length - 5].vel += vector3 * 80f;
							}
							else
							{
								heads[l].floatGrabDest = base.mainBodyChunk.pos + vector3 * -80f;
							}
						}
						else
						{
							heads[l].floatGrabDest = base.mainBodyChunk.pos;
						}
					}
					else if (Random.value < 0.01f && !base.safariControlled)
					{
						headGrabChunk[l] = null;
					}
				}
			}
			else if (headCuriosityFocus[l] != null && !headCuriosityFocus[l].slatedForDeletetion)
			{
				if (headCuriosityFocus[l] is Creature && (headCuriosityFocus[l] as Creature).abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
				{
					heads[l].floatGrabDest = Vector2.Lerp(heads[l].Tip.pos, (headCuriosityFocus[l] as Inspector).heads[l].Tip.pos, 0.25f);
				}
				else
				{
					heads[l].floatGrabDest = Vector2.Lerp(base.mainBodyChunk.pos, headCuriosityFocus[l].firstChunk.pos, 0.8f);
				}
			}
			else
			{
				heads[l].floatGrabDest = AI.HeadGoal(l);
			}
		}
		LastGoalPos = GoalPos;
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(base.abstractCreature.pos, actuallyFollowingThisPath: true);
		if (base.safariControlled)
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && !inputWithDiagonals.Value.pckp)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, lastInputWithDiagonals.Value.y) * 80f), 2);
				}
			}
		}
		if (grabbedBy.Count > 0)
		{
			base.mainBodyChunk.vel += new Vector2(Random.Range(-1f, 2f) / 2f, Random.Range(-1f, 2f) / 2f);
		}
		else
		{
			GoalPos = movementConnection.DestTile.ToVector2() * 20f;
			Vector2 vector4 = Custom.DirVec(base.mainBodyChunk.pos, GoalPos);
			vector4 *= 0.3f;
			for (int num11 = 0; num11 < headCount(); num11++)
			{
				vector4 = Vector2.Lerp(vector4, Custom.DirVec(base.mainBodyChunk.pos, heads[num11].Tip.pos), Mathf.InverseLerp(100f, 10f, Vector2.Distance(heads[num11].Tip.pos, GoalPos)));
			}
			float t = Mathf.InverseLerp(2f, 16f, (base.State as InspectorState).Wingnumber);
			if (Vector2.Distance(base.mainBodyChunk.pos, GoalPos) > 15f)
			{
				flyingPower = Vector2.Lerp(flyingPower, vector4, t);
			}
			else if (flyingPower.magnitude > 1f)
			{
				flyingPower *= 0.8f;
			}
			else
			{
				flyingPower *= 0f;
				base.mainBodyChunk.vel = Vector2.MoveTowards(base.mainBodyChunk.vel, base.mainBodyChunk.vel.normalized, 0.24f);
			}
		}
		base.GoThroughFloors = movementConnection.DestTile.y < movementConnection.StartTile.y;
		if (shortcutDelay < 1)
		{
			squeeze = movementConnection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze;
			if (squeeze)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[0].pos, room.MiddleOfTile(movementConnection.DestTile)) * 0.6f;
			}
		}
		else
		{
			squeeze = false;
		}
		if (shortcutDelay < 1 && movementConnection.type == MovementConnection.MovementType.ShortCut)
		{
			squeeze = true;
			if (squeezeFac >= 1f)
			{
				enteringShortCut = movementConnection.StartTile;
				return;
			}
		}
		if (room.zapCoils != null && room.zapCoils.Count > 0)
		{
			foreach (ZapCoil zapCoil in room.zapCoils)
			{
				Vector2 pos2 = base.mainBodyChunk.pos + base.mainBodyChunk.vel;
				IntRect rect = zapCoil.rect;
				rect.left -= 2;
				rect.right += 2;
				rect.bottom -= 2;
				rect.top += 2;
				if (Custom.InsideRect(room.GetTilePosition(pos2), rect))
				{
					FloatRect floatRect = zapCoil.rect.ToFloatRect();
					Vector2 vector5 = Custom.DirVec(new Vector2(Mathf.Lerp(floatRect.left * 20f, floatRect.right * 20f, 0.5f), Mathf.Lerp(floatRect.bottom * 20f, floatRect.top * 20f, 0.5f)), base.mainBodyChunk.pos) * 0.5f;
					vector5 = new Vector2(Mathf.Lerp(vector5.x, 0f, 0.75f), Mathf.Lerp(vector5.y, 0f, 0.75f));
					flyingPower += vector5 * 1.5f;
				}
			}
		}
		Vector2 vector6 = default(Vector2);
		if (room.gravity > 0f)
		{
			vector6.y += room.gravity * 0.9f;
		}
		float num12 = 1f + Mathf.Clamp(anger, 0f, 2f) * 3f + AI.rainTracker.Utility();
		base.mainBodyChunk.vel += flyingPower * num12 / 30f + vector6;
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment <= 0)
		{
			return base.mainBodyChunk.pos;
		}
		return heads[appendage].tChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			heads[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
			heads[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
		}
		else
		{
			heads[pos.appendage.appIndex].connectedChunk.pos += momentum / heads[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
			heads[pos.appendage.appIndex].connectedChunk.vel += momentum / heads[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
		}
		heads[pos.appendage.appIndex].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
		heads[pos.appendage.appIndex].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
	}

	public static int headCount()
	{
		return 3;
	}

	public float Rad(float f)
	{
		if (f < 0.5f)
		{
			return Custom.LerpMap(f, 0f, 0.14f, 5.8f, 0.01f);
		}
		return Custom.LerpMap(f, 0.85f, 1f, 0.01f, 2.6f);
	}

	public Vector2 getHeadDirection(int index)
	{
		Vector2 lastPos = heads[index].tChunks[heads[index].tChunks.Length - 2].lastPos;
		Vector2 lastPos2 = heads[index].Tip.lastPos;
		return Custom.DirVec(lastPos, lastPos2);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (type == DamageType.Electric)
		{
			return;
		}
		if (source != null && source.owner != null && source.owner is Weapon && (source.owner as Weapon).thrownBy != null)
		{
			AI.preyTracker.AddPrey(AI.tracker.RepresentationForCreature((source.owner as Weapon).thrownBy.abstractCreature, addIfMissing: true));
			if (hitAppendage == null)
			{
				damage = ((!((double)Random.value < 0.001)) ? 0f : 100f);
			}
			else
			{
				(base.State as InspectorState).headHealth[hitAppendage.appendage.appIndex] -= damage;
				damage = 0.12f;
			}
		}
		else if (source != null && source.owner != null && source.owner is Creature)
		{
			directionAndMomentum = new Vector2(0f, 0f);
			AI.preyTracker.AddPrey(AI.tracker.RepresentationForCreature((source.owner as Creature).abstractCreature, addIfMissing: true));
			if (hitAppendage == null)
			{
				damage = ((!((double)Random.value < 0.001)) ? 0f : 100f);
			}
			else
			{
				(base.State as InspectorState).headHealth[hitAppendage.appendage.appIndex] -= damage;
				damage = 0.12f;
			}
		}
		AI.behavior = InspectorAI.Behavior.Idle;
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 80;
		Vector2 vel = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) + Custom.RNV();
			base.bodyChunks[i].lastPos = base.bodyChunks[i].pos;
			base.bodyChunks[i].vel = vel;
		}
		squeezeFac = 1f;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		for (int j = 0; j < heads.Length; j++)
		{
			headGrabChunk[j] = null;
			headWantToGrabChunk[j] = null;
			heads[j].Reset(heads[j].connectedChunk.pos);
		}
	}

	public bool HeadsCrippled(int index)
	{
		return heads[index].limp;
	}

	public bool AllHeadsCrippled()
	{
		int num = 0;
		while (num < headCount())
		{
			if (!HeadsCrippled(num))
			{
				return false;
			}
		}
		return true;
	}

	public override void Abstractize()
	{
		if (myLight != null)
		{
			myLight.RemoveFromRoom();
		}
		if (mySuperGlow != null)
		{
			mySuperGlow.RemoveFromRoom();
		}
		for (int i = 0; i < headCount(); i++)
		{
			if (headlights[i] != null)
			{
				headlights[i].RemoveFromRoom();
			}
		}
		base.Abstractize();
	}

	public bool HeadWeaponized(int index)
	{
		if ((base.State as InspectorState).headHealth[index] > 0f)
		{
			if (headWantToGrabChunk[index] != null && headWantToGrabChunk[index].owner is Weapon)
			{
				return true;
			}
			if (headGrabChunk[index] != null && headGrabChunk[index].owner is Weapon)
			{
				return true;
			}
		}
		return false;
	}

	public bool isOtherHeadsGoalChunk(int index, BodyChunk otherChunk)
	{
		bool result = false;
		for (int i = 0; i < headCount(); i++)
		{
			if (index != i && (headWantToGrabChunk[i] == otherChunk || headGrabChunk[i] == otherChunk))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int WeaponizedHeadCount()
	{
		int num = 0;
		for (int i = 0; i < headCount(); i++)
		{
			if (HeadWeaponized(i))
			{
				num++;
			}
		}
		return num;
	}

	public override Color ShortCutColor()
	{
		return TrueColor;
	}

	public override void Blind(int blnd)
	{
		base.Blind(blnd);
		for (int i = 0; i < headCount(); i++)
		{
			if (headWantToGrabChunk[i] != null)
			{
				headWantToGrabChunk[i] = null;
				heads[i].floatGrabDest = AI.HeadGoal(i);
			}
		}
	}

	public Room HostingCircleFromRoom()
	{
		return room;
	}

	public bool CanHostCircle()
	{
		if (!base.dead)
		{
			return !base.slatedForDeletetion;
		}
		return false;
	}
}
