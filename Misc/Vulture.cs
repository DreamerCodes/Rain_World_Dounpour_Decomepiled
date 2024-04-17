using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;

public class Vulture : AirBreatherCreature, IFlyingCreature, PhysicalObject.IHaveAppendages
{
	private class VultureThruster
	{
		public Vulture vulture;

		public BodyChunk smokeChunkA;

		public BodyChunk smokeChunkB;

		public float smokeChunkLerp;

		public float angle;

		public NewVultureSmoke smoke;

		private int thrust;

		public bool lastActive;

		public Vector2 ThrustVector => Custom.DegToVec(Custom.AimFromOneVectorToAnother(vulture.bodyChunks[1].pos, vulture.bodyChunks[0].pos) + angle);

		public bool Active
		{
			get
			{
				if (ThrustersControlled)
				{
					if (vulture.landingBrake <= 0)
					{
						return thrust > 0;
					}
					return true;
				}
				return false;
			}
		}

		public Vector2 ExhaustPos => Vector2.Lerp(smokeChunkA.pos, smokeChunkB.pos, smokeChunkLerp) + ThrustVector * 14f;

		public float Force
		{
			get
			{
				float num = Mathf.Min(1f, vulture.jetFuel * 2f);
				num *= Mathf.Min(1f, (float)thrust / 5f);
				num = Mathf.Pow(num, 0.4f);
				if (vulture.landingBrake > 0 && num < 0.5f)
				{
					if (!vulture.IsMiros)
					{
						return 0.5f;
					}
					return 2.7f;
				}
				return num;
			}
		}

		public bool ThrustersControlled
		{
			get
			{
				if (vulture.safariControlled)
				{
					if (vulture.inputWithDiagonals.HasValue)
					{
						if (!vulture.inputWithDiagonals.Value.AnyDirectionalInput)
						{
							return vulture.inputWithDiagonals.Value.jmp;
						}
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public void Activate(int frames)
		{
			if (ThrustersControlled && thrust < frames)
			{
				thrust = frames;
			}
		}

		public VultureThruster(Vulture vulture, int smokeChunkA, int smokeChunkB, float smokeChunkLerp, float angle)
		{
			this.vulture = vulture;
			this.smokeChunkA = vulture.bodyChunks[smokeChunkA];
			this.smokeChunkB = vulture.bodyChunks[smokeChunkB];
			this.smokeChunkLerp = smokeChunkLerp;
			this.angle = angle;
		}

		public void Update(bool eu)
		{
			if (thrust > 0)
			{
				thrust--;
			}
			if (Active)
			{
				for (int i = 0; i < 4; i++)
				{
					vulture.bodyChunks[i].vel -= ThrustVector * (vulture.IsKing ? 1.2f : 0.8f) * Force;
				}
				vulture.jetFuel -= 1f / (vulture.IsKing ? 60f : 40f);
				if (vulture.room.BeingViewed)
				{
					if (!vulture.room.PointSubmerged(ExhaustPos))
					{
						if (smoke == null)
						{
							StartSmoke();
						}
						smoke.MoveTo(ExhaustPos, eu);
						smoke.EmitSmoke(Vector2.Lerp(smokeChunkA.vel, smokeChunkB.vel, smokeChunkLerp) + ThrustVector * (vulture.IsKing ? 55f : 45f), Force);
					}
					else
					{
						vulture.room.AddObject(new Bubble(ExhaustPos, ThrustVector * 45f * Force, bottomBubble: false, fakeWaterBubble: false));
					}
				}
				if (!lastActive)
				{
					vulture.room.PlaySound(SoundID.Vulture_Jet_Start, smokeChunkA);
				}
			}
			else if (lastActive)
			{
				vulture.room.PlaySound(SoundID.Vulture_Jet_Stop, smokeChunkA);
			}
			if (smoke != null)
			{
				for (int j = 0; j < vulture.tentacles.Length; j++)
				{
					if (vulture.tentacles[j].mode == VultureTentacle.Mode.Fly)
					{
						int num = vulture.tentacles[j].tChunks.Length - 1;
						smoke.WindDrag(vulture.tentacles[j].tChunks[num].pos, Custom.DirVec(vulture.tentacles[j].connectedChunk.pos, vulture.tentacles[j].tChunks[num].pos) * 2f * Mathf.Sin((float)Math.PI * 2f * vulture.wingFlap) + vulture.tentacles[j].tChunks[num].vel * 0.4f, 120f);
						if (vulture.wingFlap > 0.2f && vulture.wingFlap < 0.4f)
						{
							smoke.WindPuff(vulture.tentacles[j].tChunks[num].pos, 4f, 240f);
						}
					}
				}
				if (smoke.slatedForDeletetion || vulture.room != smoke.room)
				{
					smoke = null;
				}
			}
			lastActive = Active;
		}

		public float Utility()
		{
			Vector2 vector = new Vector2(0f, 0f);
			for (int i = 0; i < vulture.bodyChunks.Length; i++)
			{
				vector += vulture.bodyChunks[i].vel;
			}
			vector /= (float)vulture.bodyChunks.Length;
			return Mathf.Max(0f, 0f - Vector2.Dot(vulture.moveDirection, vector.normalized)) * Mathf.Max(0f, Vector2.Dot(vulture.moveDirection, -ThrustVector));
		}

		private void StartSmoke()
		{
			smoke = new NewVultureSmoke(vulture.room, ExhaustPos, vulture);
			vulture.room.AddObject(smoke);
		}
	}

	public class VultureState : HealthState
	{
		public float[] wingHealth;

		public bool mask;

		public VultureState(AbstractCreature creature)
			: base(creature)
		{
			bool flag = ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
			wingHealth = new float[flag ? 4 : 2];
			for (int i = 0; i < wingHealth.Length; i++)
			{
				wingHealth[i] = 1f;
			}
			mask = !flag;
		}

		public override string ToString()
		{
			string text = HealthBaseSaveString() + (mask ? "" : "<cB>NOMASK");
			foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
			}
			return text;
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			for (int i = 0; i < s.Length; i++)
			{
				string text = Regex.Split(s[i], "<cC>")[0];
				if (text != null && text == "NOMASK")
				{
					mask = false;
				}
			}
			unrecognizedSaveStrings.Remove("NOMASK");
		}
	}

	public VultureAI AI;

	public VultureTentacle[] tentacles;

	public Tentacle neck;

	public IntVector2 mouseTilePos;

	public Vector2 moveDirection;

	public bool hangingInTentacle;

	public int cantFindNewGripCounter;

	private int releaseGrippingTentacle;

	public bool hoverStill;

	public bool lastHoverStill;

	public IntVector2 hoverPos;

	public int dontSwitchModesCounter;

	public int timeSinceLastTakeoff;

	public float wingFlapAmplitude;

	public float wingFlap;

	public int landingBrake;

	public Vector2 landingBrakePos;

	private VultureThruster[] thrusters;

	private float jf;

	public int stuck;

	public int stuckShake;

	public int stuckShakeDuration;

	private MovementConnection lastConnection;

	public float tuskCharge;

	public BodyChunk snapAt;

	public int snapFrames;

	private bool temporarilyAllowInForbiddenTiles;

	public ChunkSoundEmitter jetSound;

	public ChunkSoundEmitter tuskChargeSound;

	public KingTusks kingTusks;

	public float jawOpen;

	public float lastJawOpen;

	public float jawVel;

	private float keepJawOpenPos;

	private int jawSlamPause;

	private int jawKeepOpenPause;

	public int laserCounter;

	public LightSource LaserLight;

	public bool controlledJawSnap;

	public float drown;

	public Vector2 snapAtPos;

	public bool IsKing => base.Template.type == CreatureTemplate.Type.KingVulture;

	public bool IsMiros
	{
		get
		{
			if (ModManager.MSC)
			{
				return base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
			}
			return false;
		}
	}

	public float jetFuel
	{
		get
		{
			return jf;
		}
		set
		{
			jf = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool Snapping
	{
		get
		{
			if (snapFrames > 0)
			{
				return snapFrames <= 21;
			}
			return false;
		}
	}

	public bool ChargingSnap
	{
		get
		{
			if (snapFrames > 0)
			{
				return snapFrames > 21;
			}
			return false;
		}
	}

	public float TusksStuck
	{
		get
		{
			if (!((float)snapFrames > 0f) && base.grasps[0] == null)
			{
				return tuskCharge;
			}
			return 1f;
		}
	}

	public bool AirBorne
	{
		get
		{
			if (tentacles[0].mode == VultureTentacle.Mode.Fly)
			{
				return tentacles[1].mode == VultureTentacle.Mode.Fly;
			}
			return false;
		}
	}

	public override Vector2 VisionPoint => base.bodyChunks[4].pos;

	public override Vector2 DangerPos
	{
		get
		{
			if (IsKing)
			{
				if (kingTusks.tusks[0].mode == KingTusks.Tusk.Mode.ShootingOut || kingTusks.tusks[0].mode == KingTusks.Tusk.Mode.Charging)
				{
					return kingTusks.tusks[0].chunkPoints[0, 0];
				}
				if (kingTusks.tusks[1].mode == KingTusks.Tusk.Mode.ShootingOut || kingTusks.tusks[1].mode == KingTusks.Tusk.Mode.Charging)
				{
					return kingTusks.tusks[1].chunkPoints[0, 0];
				}
			}
			return base.mainBodyChunk.pos;
		}
	}

	public bool MostlyConsious
	{
		get
		{
			if (base.stun < 40)
			{
				return !base.dead;
			}
			return false;
		}
	}

	public Vulture(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[5];
		float num = (IsKing ? 1.4f : 1f);
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, IsMiros ? 1.8f : (1.2f * num));
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9.5f, IsMiros ? 1.8f : (1.2f * num));
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 9.5f, IsMiros ? 1.8f : (1.2f * num));
		base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 9.5f, IsMiros ? 1.8f : (1.2f * num));
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 6.5f, 0.3f * num);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].restrictInRoomRange = 2400f;
			base.bodyChunks[i].defaultRestrictInRoomRange = 2400f;
		}
		bodyChunkConnections = new BodyChunkConnection[(ModManager.MMF && !IsMiros) ? 8 : 7];
		float num2 = 40f;
		float num3 = 26f;
		float num4 = 10f;
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], num3, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], num2, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[1], Mathf.Sqrt(Mathf.Pow(num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[3], Mathf.Sqrt(Mathf.Pow(num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[0], Mathf.Sqrt(Mathf.Pow(num3 - num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[5] = new BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[0], Mathf.Sqrt(Mathf.Pow(num3 - num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[6] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[4], IsKing ? 70f : 60f, BodyChunkConnection.Type.Pull, 0.6f, 0f);
		if (ModManager.MMF && !IsMiros)
		{
			bodyChunkConnections[7] = new BodyChunkConnection(base.bodyChunks[4], base.bodyChunks[0], IsKing ? 75f : 65f, BodyChunkConnection.Type.Pull, 1f, -1f);
		}
		tentacles = new VultureTentacle[IsMiros ? 4 : 2];
		for (int j = 0; j < tentacles.Length; j++)
		{
			tentacles[j] = new VultureTentacle(this, base.bodyChunks[2 + j % 2], (IsKing ? 9f : 7f) * 20f, j);
		}
		neck = new Tentacle(this, base.bodyChunks[0], (IsKing ? 6f : 5f) * 20f);
		neck.tProps = new Tentacle.TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0f, 0.5f, 1.8f, 0.2f, 1.2f, 10f, 0.25f, 3f, 15, 20, 6, 0);
		neck.tChunks = new Tentacle.TentacleChunk[4];
		for (int k = 0; k < neck.tChunks.Length; k++)
		{
			neck.tChunks[k] = new Tentacle.TentacleChunk(neck, k, (float)(k + 1) / (float)neck.tChunks.Length, IsKing ? 6f : 5f);
		}
		neck.tChunks[neck.tChunks.Length - 1].rad = 7f;
		neck.stretchAndSqueeze = 0f;
		appendages = new List<Appendage>();
		appendages.Add(new Appendage(this, 0, neck.tChunks.Length + 2));
		for (int l = 0; l < tentacles.Length; l++)
		{
			appendages.Add(new Appendage(this, l + 1, tentacles[l].tChunks.Length + 1));
		}
		lastConnection = new MovementConnection(MovementConnection.MovementType.Standard, new WorldCoordinate(0, 0, 0, 0), new WorldCoordinate(0, 0, 0, 0), 0);
		thrusters = new VultureThruster[4];
		jetFuel = 1f;
		thrusters[0] = new VultureThruster(this, 2, 0, 0.5f, 15f);
		thrusters[1] = new VultureThruster(this, 3, 0, 0.5f, -15f);
		thrusters[2] = new VultureThruster(this, 2, 1, 0.2f, 100f);
		thrusters[3] = new VultureThruster(this, 3, 1, 0.2f, -100f);
		base.GoThroughFloors = true;
		wingFlapAmplitude = 1f;
		mouseTilePos = abstractCreature.pos.Tile;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.35f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.92f;
		if (IsKing)
		{
			kingTusks = new KingTusks(this);
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new VultureGraphics(this);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((i == 1) ? 10f : 5f) + Custom.DegToVec(UnityEngine.Random.value * 360f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 5f;
		}
		for (int j = 0; j < tentacles.Length; j++)
		{
			tentacles[j].Reset(tentacles[j].connectedChunk.pos);
		}
		neck.Reset(base.mainBodyChunk.pos);
		shortcutDelay = 80;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public void SpawnFlyingCreature(WorldCoordinate coord)
	{
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void NewRoom(Room room)
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].NewRoom(room);
		}
		neck.NewRoom(room);
		if (kingTusks != null)
		{
			kingTusks.NewRoom(room);
		}
		base.NewRoom(room);
	}

	public override void Update(bool eu)
	{
		CheckFlip();
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (temporarilyAllowInForbiddenTiles && room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.mainBodyChunk.pos), base.Template))
		{
			temporarilyAllowInForbiddenTiles = false;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel *= 0.9f;
			}
			Stun(12);
		}
		if (IsMiros && laserCounter > 0)
		{
			laserCounter--;
			if (laserCounter == 10 && !base.dead)
			{
				LaserExplosion();
				if (LaserLight != null)
				{
					LaserLight.Destroy();
				}
				laserCounter--;
			}
			if (base.dead || !MostlyConsious || base.graphicsModule == null || (base.graphicsModule as VultureGraphics).shadowMode)
			{
				if (LaserLight != null)
				{
					LaserLight.Destroy();
				}
				laserCounter = 0;
			}
			else if (LaserLight != null)
			{
				Vector2 pos = Head().pos;
				Vector2 vector = Custom.DirVec(neck.Tip.pos, pos);
				vector *= -1f;
				Vector2 corner = Custom.RectCollision(pos, pos - vector * 100000f, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, pos, corner);
				if (intVector.HasValue)
				{
					corner = Custom.RectCollision(corner, pos, room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
					LaserLight.HardSetPos(corner);
					LaserLight.HardSetRad(laserCounter);
					LaserLight.color = new Color((200f - (float)laserCounter) / 200f, (float)laserCounter / 200f, 0.1f);
					LaserLight.HardSetAlpha(1f);
				}
			}
		}
		hangingInTentacle = false;
		for (int j = 0; j < tentacles.Length; j++)
		{
			tentacles[j].Update();
		}
		if (hangingInTentacle)
		{
			cantFindNewGripCounter += 2;
			if (cantFindNewGripCounter > (IsMiros ? 200 : 400))
			{
				for (int k = 0; k < tentacles.Length; k++)
				{
					if (tentacles[k].hasAnyGrip)
					{
						tentacles[k].ReleaseGrip();
					}
				}
			}
		}
		else if (cantFindNewGripCounter > 0)
		{
			cantFindNewGripCounter--;
		}
		if (!enteringShortCut.HasValue)
		{
			UpdateNeck();
		}
		if (landingBrake > 0)
		{
			landingBrake--;
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				base.bodyChunks[l].vel *= 0.7f;
			}
			base.bodyChunks[1].vel += Vector2.ClampMagnitude(landingBrakePos - base.bodyChunks[1].pos, 40f) / 20f;
		}
		bool flag = true;
		float num = 0f;
		for (int m = 0; m < thrusters.Length; m++)
		{
			thrusters[m].Update(eu);
			if (thrusters[m].Active)
			{
				flag = false;
				if (thrusters[m].Force > num)
				{
					num = thrusters[m].Force;
				}
			}
		}
		if (flag)
		{
			jetFuel += 1f / 120f;
		}
		if (UnityEngine.Random.value * 0.75f > (base.State as HealthState).health)
		{
			Stun(10);
		}
		if (jetSound != null)
		{
			if (num == 0f)
			{
				jetSound.alive = false;
				jetSound = null;
			}
			else
			{
				jetSound.alive = true;
				jetSound.volume = Mathf.InverseLerp(0f, 0.1f, num);
				jetSound.pitch = Mathf.Lerp(0.4f, 2.2f, num);
			}
		}
		else if (num > 0f)
		{
			jetSound = room.PlaySound(SoundID.Vulture_Jet_LOOP, base.mainBodyChunk);
			jetSound.requireActiveUpkeep = true;
		}
		if (tuskChargeSound != null)
		{
			if (!ChargingSnap)
			{
				tuskChargeSound.alive = false;
				tuskChargeSound = null;
			}
			else
			{
				tuskChargeSound.alive = true;
			}
		}
		else if (ChargingSnap)
		{
			tuskChargeSound = room.PlaySound(SoundID.Vulture_Jaws_Carged_LOOP, base.mainBodyChunk);
			tuskChargeSound.requireActiveUpkeep = true;
		}
		if (room.game.devToolsActive && Input.GetKey("g"))
		{
			base.mainBodyChunk.vel.y += 20f;
		}
		if (base.grasps[0] != null)
		{
			Carry();
		}
		if (base.Consious)
		{
			Act(eu);
		}
		if (kingTusks != null)
		{
			kingTusks.Update();
		}
	}

	public void Act(bool eu)
	{
		AI.Update();
		if (IsMiros)
		{
			lastJawOpen = jawOpen;
			if (base.grasps[0] != null)
			{
				jawOpen = 0.15f;
			}
			else if (jawSlamPause > 0)
			{
				jawSlamPause--;
			}
			else
			{
				if (isLaserActive())
				{
					jawKeepOpenPause = 10;
					keepJawOpenPos = 1f;
				}
				if (jawVel == 0f)
				{
					jawVel = 0.15f;
				}
				if (base.abstractCreature.controlled && jawVel >= 0f && jawVel < 1f && !controlledJawSnap)
				{
					jawVel = 0f;
					jawOpen = 0f;
				}
				jawOpen += jawVel;
				if (jawKeepOpenPause > 0)
				{
					jawKeepOpenPause--;
					jawOpen = Mathf.Clamp(Mathf.Lerp(jawOpen, keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
				}
				else if (UnityEngine.Random.value < 1f / ((!base.Blinded) ? 40f : 15f) && !base.abstractCreature.controlled)
				{
					jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
					keepJawOpenPos = ((UnityEngine.Random.value >= 0.5f) ? 1f : 0f);
					jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
					jawOpen = Mathf.Clamp(jawOpen, 0f, 1f);
				}
				else if (jawOpen <= 0f)
				{
					jawOpen = 0f;
					if (jawVel < -0.4f)
					{
						JawSlamShut();
						controlledJawSnap = false;
					}
					jawVel = 0.15f;
					jawSlamPause = 5;
				}
				else if (jawOpen >= 1f)
				{
					jawOpen = 1f;
					jawVel = -0.5f;
				}
			}
		}
		if (!AirBorne)
		{
			float num = 100f;
			if (base.mainBodyChunk.pos.x < 0f - num || base.mainBodyChunk.pos.y < 0f - num || base.mainBodyChunk.pos.x > room.PixelWidth + num || base.mainBodyChunk.pos.y > room.PixelHeight + num)
			{
				for (int i = 0; i < tentacles.Length; i++)
				{
					tentacles[i].SwitchMode(VultureTentacle.Mode.Fly);
				}
			}
		}
		if (wingFlap < 0.5f)
		{
			wingFlap += 1f / 30f;
		}
		else
		{
			wingFlap += 0.02f;
		}
		if (wingFlap > 1f)
		{
			wingFlap -= 1f;
		}
		float num2 = 0f;
		for (int j = 0; j < tentacles.Length; j++)
		{
			num2 += tentacles[j].Support() * (IsMiros ? 0.75f : 0.5f);
		}
		num2 = Mathf.Pow(num2, 0.5f);
		num2 = Mathf.Max(num2, 0.1f);
		hoverStill = false;
		IntVector2 tilePosition = room.GetTilePosition(base.mainBodyChunk.pos);
		for (int k = 0; k < 5; k++)
		{
			if (room.aimap.TileAccessibleToCreature(tilePosition + Custom.fourDirectionsAndZero[k], base.Template))
			{
				tilePosition += Custom.fourDirectionsAndZero[k];
			}
		}
		if (room == null)
		{
			return;
		}
		MovementConnection movementConnection = (AI.pathFinder as VulturePather).FollowPath(room.GetWorldCoordinate(tilePosition), actuallyFollowingThisPath: true);
		VultureTentacle.Mode mode = VultureTentacle.Mode.Climb;
		if (base.safariControlled)
		{
			bool flag = false;
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type) || movementConnection.type == MovementConnection.MovementType.OutsideRoom || movementConnection.type == MovementConnection.MovementType.OffScreenMovement)
			{
				movementConnection = default(MovementConnection);
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				else
				{
					for (int l = 0; l < Custom.fourDirections.Length; l++)
					{
						if (room.GetTile(base.mainBodyChunk.pos + Custom.fourDirections[l].ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							type = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
							break;
						}
					}
				}
				flag = true;
			}
			if (inputWithDiagonals.HasValue)
			{
				if ((!IsMiros || isLaserActive()) && inputWithDiagonals.Value.thrw && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0))
				{
					Vector2 p = base.bodyChunks[4].pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f;
					base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, p) * 15f;
					neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, p) * num2;
				}
				else if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && flag)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
				if (inputWithDiagonals.Value.jmp)
				{
					mode = VultureTentacle.Mode.Fly;
					if (!lastInputWithDiagonals.Value.jmp)
					{
						bool flag2 = false;
						for (int m = 0; m < tentacles.Length; m++)
						{
							if (tentacles[m].mode == VultureTentacle.Mode.Climb)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							TakeOff();
						}
					}
					for (int n = 0; n < tentacles.Length; n++)
					{
						tentacles[n].SwitchMode(VultureTentacle.Mode.Fly);
					}
				}
				if (IsMiros && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
				{
					controlledJawSnap = true;
				}
				if (!IsMiros && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp && snapFrames == 0)
				{
					if (AI.focusCreature != null && AI.focusCreature.VisualContact)
					{
						Creature realizedCreature = AI.focusCreature.representedCreature.realizedCreature;
						if (realizedCreature.bodyChunks.Length != 0)
						{
							BodyChunk bodyChunk = realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)];
							Snap(bodyChunk);
						}
					}
					else if (inputWithDiagonals.Value.AnyDirectionalInput)
					{
						SnapTowards(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f);
					}
					else
					{
						SnapTowards(base.mainBodyChunk.pos + Custom.RNV() * 200f);
					}
				}
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					if (base.grasps[0] != null)
					{
						LoseAllGrasps();
					}
					else if (IsMiros && !isLaserActive())
					{
						FireLaser();
					}
				}
				if (flag)
				{
					if (inputWithDiagonals.Value.y < 0)
					{
						base.GoThroughFloors = true;
					}
					else
					{
						base.GoThroughFloors = false;
					}
				}
			}
		}
		if (movementConnection == default(MovementConnection) || Custom.ManhattanDistance(room.GetWorldCoordinate(base.mainBodyChunk.pos), AI.pathFinder.GetDestination) < 2)
		{
			hoverStill = true;
		}
		neck.retractFac = Mathf.Clamp(neck.retractFac + 1f / 30f, 0f, 0.6f);
		base.bodyChunks[4].vel *= 0.9f;
		for (int num3 = 0; num3 < 4; num3++)
		{
			if (AirBorne)
			{
				base.bodyChunks[num3].vel *= 0.98f;
				num2 = 0f;
				for (int num4 = 0; num4 < tentacles.Length; num4++)
				{
					num2 += ((tentacles[num4].stun >= 5) ? 0f : (1f / (float)tentacles.Length));
				}
			}
			else
			{
				base.bodyChunks[num3].vel *= Mathf.Lerp(0.98f, 0.9f, num2);
				if (num2 > 0.1f)
				{
					base.bodyChunks[num3].vel.y += Mathf.Lerp(1.2f, 0.5f, num2);
				}
			}
		}
		base.bodyChunks[1].vel.y += 1.9f * num2 * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);
		base.bodyChunks[0].vel.y -= 1.9f * num2 * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);
		if (!hoverStill && (movementConnection == default(MovementConnection) || (movementConnection.DestTile == lastConnection.DestTile && room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile))))
		{
			stuck++;
			if (stuck > 60)
			{
				stuckShake = stuckShakeDuration;
				stuckShakeDuration += 30;
			}
		}
		else
		{
			stuck = 0;
			if (stuckShakeDuration > 30)
			{
				stuckShakeDuration--;
			}
		}
		if (room == null)
		{
			return;
		}
		for (int num5 = 0; num5 < 5; num5++)
		{
			if (room.GetTile(base.abstractCreature.pos.Tile + Custom.fourDirectionsAndZero[num5]).wormGrass)
			{
				stuckShake = Math.Max(stuckShake, 40);
				base.mainBodyChunk.vel -= Custom.fourDirectionsAndZero[num5].ToVector2() * 2f + Custom.RNV() * 6f + new Vector2(0f, 6f);
			}
		}
		if (AI.stuckTracker.Utility() > 0.9f)
		{
			stuckShake = Math.Max(stuckShake, 5);
		}
		if (stuckShake > 0)
		{
			stuckShake--;
			StuckBehavior();
			return;
		}
		if (!hoverStill)
		{
			bool flag3 = true;
			for (int num6 = 0; num6 < tentacles.Length; num6++)
			{
				flag3 = flag3 && (tentacles[num6].hasAnyGrip || tentacles[num6].mode != VultureTentacle.Mode.Climb);
			}
			if (hangingInTentacle && flag3)
			{
				releaseGrippingTentacle++;
				if (releaseGrippingTentacle > 5 && CheckTentacleModeAnd(VultureTentacle.Mode.Climb))
				{
					tentacles[TentacleMaxReleaseInd()].ReleaseGrip();
				}
				else if (releaseGrippingTentacle > 50)
				{
					tentacles[TentacleMaxReleaseInd()].ReleaseGrip();
				}
			}
			else
			{
				releaseGrippingTentacle = 0;
			}
			bool flag4 = true;
			for (int num7 = 0; num7 < tentacles.Length; num7++)
			{
				flag4 = flag4 && tentacles[num7].WingSpace();
			}
			if (!base.safariControlled && IsMiros && isLaserActive() && CheckTentacleModeOr(VultureTentacle.Mode.Climb))
			{
				if (timeSinceLastTakeoff >= 40)
				{
					TakeOff();
				}
				else
				{
					for (int num8 = 0; num8 < tentacles.Length; num8++)
					{
						tentacles[num8].SwitchMode(VultureTentacle.Mode.Fly);
					}
				}
				dontSwitchModesCounter = 200;
			}
			timeSinceLastTakeoff++;
			if (dontSwitchModesCounter > 0)
			{
				dontSwitchModesCounter--;
			}
			else if (IsMiros)
			{
				if (!hoverStill && room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && base.mainBodyChunk.vel.y > 4f && moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(base.mainBodyChunk.pos), room.GetTilePosition(base.mainBodyChunk.pos + moveDirection * 400f)) && flag4)
				{
					TakeOff();
					dontSwitchModesCounter = 200;
				}
				else if (!hoverStill && room.aimap.getTerrainProximity(movementConnection.DestTile) > 4 && CheckTentacleModeOr(VultureTentacle.Mode.Climb) && (!base.safariControlled || mode == VultureTentacle.Mode.Fly))
				{
					for (int num9 = 0; num9 < tentacles.Length; num9++)
					{
						tentacles[num9].SwitchMode(VultureTentacle.Mode.Fly);
					}
					dontSwitchModesCounter = 200;
				}
				else if (room.aimap.getTerrainProximity(movementConnection.DestTile) <= (IsMiros ? 4 : 8) && room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && CheckTentacleModeAnd(VultureTentacle.Mode.Fly) && (!base.safariControlled || mode == VultureTentacle.Mode.Climb))
				{
					for (int num10 = 0; num10 < tentacles.Length; num10++)
					{
						tentacles[num10].SwitchMode(VultureTentacle.Mode.Climb);
					}
					AirBrake(30);
					dontSwitchModesCounter = 200;
				}
			}
			else if (room.aimap.getTerrainProximity(movementConnection.DestTile) <= (IsMiros ? 4 : 8) && room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && CheckTentacleModeAnd(VultureTentacle.Mode.Fly) && (!base.safariControlled || mode == VultureTentacle.Mode.Climb))
			{
				for (int num11 = 0; num11 < tentacles.Length; num11++)
				{
					tentacles[num11].SwitchMode(VultureTentacle.Mode.Climb);
				}
				AirBrake(30);
				dontSwitchModesCounter = 200;
			}
			else if (!hoverStill && room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && base.mainBodyChunk.vel.y > 4f && moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(base.mainBodyChunk.pos), room.GetTilePosition(base.mainBodyChunk.pos + moveDirection * 400f)) && flag4 && (!base.safariControlled || mode == VultureTentacle.Mode.Fly))
			{
				TakeOff();
				dontSwitchModesCounter = 200;
			}
		}
		bool flag5 = true;
		for (int num12 = 0; num12 < tentacles.Length; num12++)
		{
			flag5 = flag5 && !tentacles[num12].hasAnyGrip;
		}
		if (base.mainBodyChunk.vel.y < -10f && CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && flag5 && landingBrake < 1 && (!base.safariControlled || mode == VultureTentacle.Mode.Fly))
		{
			for (int num13 = 0; num13 < tentacles.Length; num13++)
			{
				tentacles[num13].SwitchMode(VultureTentacle.Mode.Fly);
			}
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as VultureGraphics).MakeColorWave(UnityEngine.Random.Range(10, 20));
			}
		}
		bool flag6 = true;
		for (int num14 = 0; num14 < tentacles.Length; num14++)
		{
			flag6 = flag6 && tentacles[num14].mode != VultureTentacle.Mode.Fly;
		}
		if (CheckTentacleModeOr(VultureTentacle.Mode.Fly))
		{
			wingFlapAmplitude = Mathf.Clamp(wingFlapAmplitude + 1f / 30f, 0f, 1f);
		}
		else if (flag6)
		{
			wingFlapAmplitude = 0f;
		}
		else
		{
			wingFlapAmplitude = Mathf.Clamp(wingFlapAmplitude + 0.0125f, 0f, 0.5f);
		}
		if (hoverStill)
		{
			if (!lastHoverStill || !Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(hoverPos), 60f))
			{
				hoverPos = room.GetTilePosition(base.mainBodyChunk.pos);
			}
			base.bodyChunks[1].vel.y += 0.1f * num2;
			for (int num15 = 0; num15 < 4; num15++)
			{
				base.bodyChunks[num15].vel *= Mathf.Lerp(1f, 0.9f, num2);
				base.bodyChunks[num15].vel += 0.6f * num2 * Vector2.ClampMagnitude(room.MiddleOfTile(hoverPos) - base.mainBodyChunk.pos, 10f) / 10f;
			}
		}
		else if (movementConnection != default(MovementConnection))
		{
			Vector2 a = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord));
			a = Vector2.Lerp(a, IntVector2.ClampAtOne(movementConnection.DestTile - movementConnection.StartTile).ToVector2(), 0.5f);
			if (AirBorne)
			{
				if (room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile) || a.y < 0f)
				{
					a.y *= 0.5f;
					if (a.y < 0f)
					{
						a.y = 0f;
					}
				}
				if (movementConnection.destinationCoord.y > movementConnection.startCoord.y || !movementConnection.destinationCoord.TileDefined)
				{
					base.bodyChunks[1].vel.y += 3.5f;
				}
				else if (movementConnection.destinationCoord.y < movementConnection.startCoord.y)
				{
					wingFlap -= 1f / 70f;
				}
			}
			else if (!room.IsPositionInsideBoundries(movementConnection.DestTile))
			{
				if (movementConnection.destinationCoord.y > room.TileHeight)
				{
					TakeOff();
				}
				else if (!base.safariControlled || mode == VultureTentacle.Mode.Fly)
				{
					for (int num16 = 0; num16 < tentacles.Length; num16++)
					{
						tentacles[num16].SwitchMode(VultureTentacle.Mode.Fly);
					}
				}
			}
			for (int num17 = 0; num17 < 4; num17++)
			{
				base.bodyChunks[num17].vel += a * (AirBorne ? 0.6f : (IsKing ? 1.9f : 1.2f)) * num2;
			}
			MovementConnection movementConnection2 = movementConnection;
			for (int num18 = 0; num18 < 3; num18++)
			{
				MovementConnection movementConnection3 = (AI.pathFinder as VulturePather).FollowPath(movementConnection2.destinationCoord, actuallyFollowingThisPath: false);
				movementConnection2 = movementConnection3;
			}
			if (movementConnection2 == movementConnection)
			{
				moveDirection = (moveDirection + a * 0.1f).normalized;
			}
			else
			{
				moveDirection = (moveDirection + Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection2.destinationCoord)) * 0.5f).normalized;
			}
		}
		bool flag7 = false;
		for (int num19 = 0; num19 < tentacles.Length; num19++)
		{
			flag7 = flag7 || !tentacles[num19].hasAnyGrip;
		}
		if (!hoverStill && !hangingInTentacle && flag7)
		{
			float num20 = 0f;
			int num21 = -1;
			for (int num22 = 0; num22 < thrusters.Length; num22++)
			{
				float num23 = thrusters[num22].Utility();
				if (num23 > num20)
				{
					num20 = num23;
					num21 = num22;
				}
			}
			num20 *= jetFuel;
			if (num20 > 0.05f)
			{
				thrusters[num21].Activate(10 + (int)(Mathf.InverseLerp(0.05f, 0.4f, num20) * 20f));
			}
		}
		if (snapFrames == 0)
		{
			if (AI.preyInTuskChargeRange || AirBorne)
			{
				tuskCharge = Mathf.Clamp(tuskCharge + 0.025f, 0f, 1f);
			}
			else
			{
				tuskCharge = Mathf.Clamp(tuskCharge - 1f / 90f, 0f, 1f);
			}
		}
		else
		{
			Vector2 pos = snapAtPos;
			if (snapAt != null)
			{
				pos = snapAt.pos;
			}
			if (Snapping)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, pos) * 1f;
			}
			else if (ChargingSnap)
			{
				base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[1].pos, pos) * 0.5f;
				if (!AirBorne)
				{
					for (int num24 = 0; num24 < 4; num24++)
					{
						base.bodyChunks[num24].vel *= Mathf.Lerp(1f, 0.2f, num2);
					}
				}
			}
			snapFrames--;
		}
		lastHoverStill = hoverStill;
		if (movementConnection != default(MovementConnection))
		{
			lastConnection = movementConnection;
		}
	}

	private void StuckBehavior()
	{
		bool flag = false;
		for (int i = 0; i < 4; i++)
		{
			if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0 || room.GetTile(base.bodyChunks[i].pos).wormGrass)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			for (int j = 2; j < 5; j++)
			{
				for (float num = 0f; num < 360f; num += 36f)
				{
					if (SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(base.mainBodyChunk.pos), room.GetTilePosition(base.mainBodyChunk.pos + Custom.DegToVec(num) * 20f * j)))
					{
						for (int k = 0; k < 4; k++)
						{
							base.bodyChunks[k].vel += Custom.DegToVec(num);
						}
						break;
					}
				}
			}
		}
		Vector2 vector = Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f;
		for (int l = 0; l < 4; l++)
		{
			base.bodyChunks[l].vel += vector;
		}
		bool flag2 = true;
		for (int m = 0; m < thrusters.Length; m++)
		{
			if (thrusters[m].Active)
			{
				flag2 = false;
				break;
			}
		}
		if (jetFuel > 0.2f && flag2 && thrusters.Length != 0)
		{
			thrusters[UnityEngine.Random.Range(0, thrusters.Length)].Activate(5 + UnityEngine.Random.Range(0, 10));
		}
	}

	public void AccessSkyGate(WorldCoordinate start, WorldCoordinate dest)
	{
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SkyExit, start, dest);
		if (!ModManager.CoopAvailable)
		{
			return;
		}
		Grasp[] array = base.grasps;
		foreach (Grasp grasp in array)
		{
			if (grasp != null && grasp.grabbed != null && grasp.grabbed is Player)
			{
				(grasp.grabbed as Player).PermaDie();
			}
		}
	}

	public void AirBrake(int frames)
	{
		landingBrake = frames;
		landingBrakePos = base.bodyChunks[1].pos;
		if (frames > 5)
		{
			room.PlaySound(SoundID.Vulture_Jets_Air_Brake, base.mainBodyChunk);
		}
	}

	private void TakeOff()
	{
		timeSinceLastTakeoff = 0;
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].SwitchMode(VultureTentacle.Mode.Fly);
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as VultureGraphics).MakeColorWave(UnityEngine.Random.Range(30, 60));
		}
		AirBrake(5);
		thrusters[0].Activate(30);
		thrusters[1].Activate(30);
	}

	private void WaterBehavior()
	{
	}

	private void CheckFlip()
	{
		if (Custom.DistanceToLine(base.bodyChunks[2].pos, base.bodyChunks[0].pos, base.bodyChunks[1].pos) < 0f)
		{
			Vector2 pos = base.bodyChunks[2].pos;
			Vector2 vel = base.bodyChunks[2].vel;
			Vector2 lastPos = base.bodyChunks[2].lastPos;
			base.bodyChunks[2].pos = base.bodyChunks[3].pos;
			base.bodyChunks[2].vel = base.bodyChunks[3].vel;
			base.bodyChunks[2].lastPos = base.bodyChunks[3].lastPos;
			base.bodyChunks[3].pos = pos;
			base.bodyChunks[3].vel = vel;
			base.bodyChunks[3].lastPos = lastPos;
		}
	}

	public void Snap(BodyChunk snapAt)
	{
		tuskCharge = 0f;
		this.snapAt = snapAt;
		snapFrames = 49;
		room.PlaySound(SoundID.Vulture_Peck, base.bodyChunks[4]);
	}

	private void UpdateNeck()
	{
		neck.Update();
		if (AI.stuckTracker.closeToGoalButNotSeeingItTracker.counter > AI.stuckTracker.closeToGoalButNotSeeingItTracker.counterMin)
		{
			List<IntVector2> path = null;
			float num = AI.stuckTracker.closeToGoalButNotSeeingItTracker.Stuck;
			neck.MoveGrabDest(room.MiddleOfTile(AI.pathFinder.GetDestination), ref path);
			base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 10f * num;
			base.bodyChunks[4].pos += Custom.DirVec(base.bodyChunks[4].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 10f * num;
			for (int i = 0; i < neck.tChunks.Length; i++)
			{
				neck.tChunks[i].vel += Custom.DirVec(neck.tChunks[i].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 5f * num;
			}
			if (num > 0.95f)
			{
				base.bodyChunks[4].collideWithTerrain = false;
				return;
			}
		}
		base.bodyChunks[4].collideWithTerrain = true;
		for (int j = 0; j < neck.tChunks.Length; j++)
		{
			neck.tChunks[j].vel *= 0.95f;
			neck.tChunks[j].vel.y -= (neck.limp ? 0.7f : 0.1f);
			neck.tChunks[j].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * ((j == 0) ? 1.2f : 0.8f);
			neck.tChunks[j].vel -= neck.connectedChunk.vel;
			neck.tChunks[j].vel *= (AirBorne ? 0.2f : 0.75f);
			neck.tChunks[j].vel += neck.connectedChunk.vel;
		}
		neck.limp = !base.Consious;
		float num2 = ((neck.backtrackFrom == -1) ? 0.5f : 0f);
		if (base.grasps[0] == null)
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			float num3 = Vector2.Distance(base.bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			base.bodyChunks[4].pos -= (6f - num3) * vector * (1f - num2);
			base.bodyChunks[4].vel -= (6f - num3) * vector * (1f - num2);
			neck.tChunks[neck.tChunks.Length - 1].pos += (6f - num3) * vector * num2;
			neck.tChunks[neck.tChunks.Length - 1].vel += (6f - num3) * vector * num2;
			base.bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (AirBorne ? 2f : 6f) * (1f - num2);
			base.bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 1].pos, base.bodyChunks[4].pos) * (AirBorne ? 2f : 6f) * (1f - num2);
			neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (AirBorne ? 1f : 3f) * num2;
			neck.tChunks[neck.tChunks.Length - 2].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (AirBorne ? 1f : 3f) * num2;
		}
		if (!base.Consious)
		{
			return;
		}
		Vector2 pos = snapAtPos;
		if (snapAt != null)
		{
			pos = snapAt.pos;
		}
		if (ChargingSnap)
		{
			base.bodyChunks[4].vel += (base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, pos) * 50f - base.bodyChunks[4].pos) / 6f;
			neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, pos) * 10f * num2;
			return;
		}
		if (Snapping)
		{
			base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, pos) * 15f;
			neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, pos) * num2;
			return;
		}
		Vector2 vector2 = ((AI.creatureLooker.lookCreature == null) ? room.MiddleOfTile(AI.pathFinder.GetDestination) : ((!AI.creatureLooker.lookCreature.VisualContact) ? room.MiddleOfTile(AI.creatureLooker.lookCreature.BestGuessForPosition()) : AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos));
		if (Custom.DistLess(vector2, base.mainBodyChunk.pos, 220f) && !room.VisualContact(vector2, base.bodyChunks[4].pos))
		{
			List<IntVector2> path2 = null;
			neck.MoveGrabDest(vector2, ref path2);
		}
		else if (neck.backtrackFrom == -1)
		{
			neck.floatGrabDest = null;
		}
		Vector2 vector3 = Custom.DirVec(base.bodyChunks[4].pos, vector2);
		if (base.grasps[0] == null)
		{
			neck.tChunks[neck.tChunks.Length - 1].vel += vector3 * num2;
			neck.tChunks[neck.tChunks.Length - 2].vel -= vector3 * 0.5f * num2;
			base.bodyChunks[4].vel += vector3 * 4f * (1f - num2);
		}
		else
		{
			neck.tChunks[neck.tChunks.Length - 1].vel += vector3 * 2f * num2;
			neck.tChunks[neck.tChunks.Length - 2].vel -= vector3 * 2f * num2;
			base.grasps[0].grabbedChunk.vel += vector3 / base.grasps[0].grabbedChunk.mass;
		}
		if (Custom.DistLess(base.bodyChunks[4].pos, vector2, 80f))
		{
			for (int k = 0; k < neck.tChunks.Length; k++)
			{
				neck.tChunks[k].vel -= vector3 * Mathf.InverseLerp(80f, 20f, Vector2.Distance(base.bodyChunks[4].pos, vector2)) * 8f * num2;
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (IsMiros || !Snapping || myChunk != 4 || base.grasps[0] != null)
		{
			return;
		}
		if (AI.OnlyHurtDontGrab(otherObject))
		{
			if (otherObject is Creature)
			{
				(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * 2f, otherObject.bodyChunks[otherChunk], null, DamageType.Bite, 1.1f, 30f);
			}
		}
		else
		{
			Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: true);
			AI.creatureLooker.LookAtNothing();
			if (otherObject is Creature)
			{
				(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * 2f, otherObject.bodyChunks[otherChunk], null, DamageType.Bite, 0.4f, 20f);
			}
		}
		room.PlaySound((otherObject is Player) ? SoundID.Vulture_Grab_Player : SoundID.Vulture_Grab_NPC, base.bodyChunks[4]);
		snapFrames = 0;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (room == null)
		{
			return;
		}
		if (!IsMiros)
		{
			AI.disencouraged += (damage / 2f + stunBonus / 460f) * (IsKing ? 0.3f : 1f) * ((room.game.IsStorySession && room.game.StoryCharacter == SlugcatStats.Name.Yellow) ? 1.5f : 1f);
		}
		if (hitChunk != null && hitChunk.index == 4)
		{
			stunBonus += 20f + 20f * damage;
			if (damage > 0.1f || base.stun > 10)
			{
				snapFrames = 0;
			}
			bool flag = directionAndMomentum.HasValue && source != null && !SpearStick(source.owner as Spear, damage, hitChunk, onAppendagePos, directionAndMomentum.Value.normalized);
			if (kingTusks != null && source != null && UnityEngine.Random.value < (base.dead ? 0.2f : 0.8f) && source.owner is Spear && directionAndMomentum.HasValue && kingTusks.HitBySpear(directionAndMomentum.Value))
			{
				if (directionAndMomentum.HasValue)
				{
					hitChunk.vel += directionAndMomentum.Value * 0.8f;
				}
				damage *= 0.1f;
			}
			else if (!flag)
			{
				if (!IsMiros && (base.State as VultureState).mask && (damage > 0.9f || (ModManager.MSC && source != null && source.owner is LillyPuck)) && (source == null || !(source.owner is Weapon) || (source.owner as Weapon).meleeHitChunk == null))
				{
					DropMask((directionAndMomentum.HasValue ? (directionAndMomentum.Value / 5f) : new Vector2(0f, 0f)) + Custom.RNV() * 7f * UnityEngine.Random.value);
				}
				damage *= 1.5f;
			}
			else
			{
				Vector2 pos = ((source != null) ? Vector2.Lerp(hitChunk.pos, source.pos, 0.5f) : hitChunk.pos);
				if (damage > 0.1f || stunBonus > 30f)
				{
					room.AddObject(new StationaryEffect(pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
					for (int i = 0; i < 3 + (int)Mathf.Min(damage * 3f, 9f); i++)
					{
						room.AddObject(new Spark(pos, Custom.RNV() * UnityEngine.Random.value * 12f, new Color(1f, 1f, 1f), null, 6, 16));
					}
				}
				if (directionAndMomentum.HasValue)
				{
					hitChunk.vel += directionAndMomentum.Value * 0.8f;
				}
				damage *= 0.1f;
			}
		}
		if (onAppendagePos != null)
		{
			if (onAppendagePos.appendage.appIndex == 0 && type != DamageType.Blunt)
			{
				damage *= 2f;
			}
			else if (onAppendagePos.appendage.appIndex > 0 && (!IsMiros || type != DamageType.Explosion))
			{
				tentacles[onAppendagePos.appendage.appIndex - 1].Damage(type, damage, stunBonus);
				damage /= 10f;
				stunBonus /= 20f;
			}
		}
		if (IsMiros && !base.dead && base.grasps[0] == null && type != DamageType.Explosion)
		{
			FireLaser();
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		if (IsMiros)
		{
			return true;
		}
		if (chunk != null && chunk.index == 4 && !base.dead && (base.State as VultureState).mask && Vector2.Dot(direction.normalized, Custom.DirVec(neck.tChunks[neck.tChunks.Length - 1].pos, chunk.pos)) < -0.88f)
		{
			return false;
		}
		return base.SpearStick(source, dmg, chunk, appPos, direction);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (!(speed > 1.5f && firstContact))
		{
			return;
		}
		float num = Mathf.InverseLerp(6f, 14f, speed);
		if (IsMiros)
		{
			if (num < 1f)
			{
				room.PlaySound(SoundID.Miros_Light_Terrain_Impact, base.mainBodyChunk, loop: false, 1f - num, 1f);
			}
			if (num > 0f)
			{
				room.PlaySound(SoundID.Miros_Heavy_Terrain_Impact, base.mainBodyChunk, loop: false, num, 1f);
			}
		}
		else
		{
			if (num < 1f)
			{
				room.PlaySound(SoundID.Vulture_Light_Terrain_Impact, base.mainBodyChunk, loop: false, 1f - num, 1f);
			}
			if (num > 0f)
			{
				room.PlaySound(SoundID.Vulture_Heavy_Terrain_Impact, base.mainBodyChunk, loop: false, num, 1f);
			}
		}
	}

	public void DropMask(Vector2 violenceDir)
	{
		if ((base.State as VultureState).mask)
		{
			(base.State as VultureState).mask = false;
			AbstractPhysicalObject abstractPhysicalObject = new VultureMask.AbstractVultureMask(room.world, null, base.abstractPhysicalObject.pos, room.game.GetNewID(), base.abstractCreature.ID.RandomSeed, IsKing);
			room.abstractRoom.AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.pos = base.abstractCreature.pos;
			abstractPhysicalObject.RealizeInRoom();
			abstractPhysicalObject.realizedObject.firstChunk.HardSetPosition(base.bodyChunks[4].pos);
			abstractPhysicalObject.realizedObject.firstChunk.vel = base.bodyChunks[4].vel + violenceDir;
			(abstractPhysicalObject.realizedObject as VultureMask).fallOffVultureMode = 1f;
			if (killTag != null)
			{
				SocialMemory.Relationship orInitiateRelationship = base.State.socialMemory.GetOrInitiateRelationship(killTag.ID);
				orInitiateRelationship.like = -1f;
				orInitiateRelationship.tempLike = -1f;
				orInitiateRelationship.know = 1f;
			}
		}
	}

	public override void Stun(int st)
	{
		snapFrames = 0;
		if (IsMiros)
		{
			LoseAllGrasps();
		}
		base.Stun(st);
	}

	public void Carry()
	{
		if (!base.Consious)
		{
			LoseAllGrasps();
			return;
		}
		BodyChunk grabbedChunk = base.grasps[0].grabbedChunk;
		float num = 1f;
		if (ModManager.MSC && grabbedChunk.owner is Player && (grabbedChunk.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			num = 10f;
		}
		if (UnityEngine.Random.value < 1f / 120f * num && (!(grabbedChunk.owner is Creature) || base.Template.CreatureRelationship((grabbedChunk.owner as Creature).Template).type != CreatureTemplate.Relationship.Type.Eats))
		{
			LoseAllGrasps();
			return;
		}
		float num2 = grabbedChunk.mass / (grabbedChunk.mass + base.bodyChunks[4].mass);
		float num3 = grabbedChunk.mass / (grabbedChunk.mass + base.bodyChunks[0].mass);
		if (neck.backtrackFrom != -1 || enteringShortCut.HasValue)
		{
			num2 = 0f;
			num3 = 0f;
		}
		if (!Custom.DistLess(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos, 20f))
		{
			Vector2 vector = Custom.DirVec(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			float num4 = Vector2.Distance(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
			grabbedChunk.pos -= (20f - num4) * vector * (1f - num2);
			grabbedChunk.vel -= (20f - num4) * vector * (1f - num2);
			neck.tChunks[neck.tChunks.Length - 1].pos += (20f - num4) * vector * num2;
			neck.tChunks[neck.tChunks.Length - 1].vel += (20f - num4) * vector * num2;
		}
		if (!enteringShortCut.HasValue)
		{
			base.bodyChunks[4].pos = Vector2.Lerp(neck.tChunks[neck.tChunks.Length - 1].pos, grabbedChunk.pos, 0.1f);
			base.bodyChunks[4].vel = neck.tChunks[neck.tChunks.Length - 1].vel;
		}
		float num5 = 70f;
		if (!Custom.DistLess(base.mainBodyChunk.pos, grabbedChunk.pos, num5))
		{
			Vector2 vector2 = Custom.DirVec(grabbedChunk.pos, base.bodyChunks[0].pos);
			float num6 = Vector2.Distance(grabbedChunk.pos, base.bodyChunks[0].pos);
			grabbedChunk.pos -= (num5 - num6) * vector2 * (1f - num3);
			grabbedChunk.vel -= (num5 - num6) * vector2 * (1f - num3);
			base.bodyChunks[0].pos += (num5 - num6) * vector2 * num3;
			base.bodyChunks[0].vel += (num5 - num6) * vector2 * num3;
		}
	}

	public override void Die()
	{
		surfaceFriction = 0.3f;
		base.Die();
	}

	public override Color ShortCutColor()
	{
		if (base.graphicsModule != null)
		{
			return HSLColor.Lerp((base.graphicsModule as VultureGraphics).ColorA, (base.graphicsModule as VultureGraphics).ColorB, 0.5f).rgb;
		}
		return base.ShortCutColor();
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		segment--;
		if (appendage == 0)
		{
			if (segment < 0)
			{
				return base.mainBodyChunk.pos;
			}
			if (segment >= neck.tChunks.Length)
			{
				return base.bodyChunks[4].pos;
			}
			return neck.tChunks[segment].pos;
		}
		if (segment < 0)
		{
			return tentacles[appendage - 1].connectedChunk.pos;
		}
		return tentacles[appendage - 1].tChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.appendage.appIndex == 0)
		{
			if (pos.prevSegment > 0)
			{
				neck.tChunks[pos.prevSegment - 1].pos += momentum / 0.1f * (1f - pos.distanceToNext);
				neck.tChunks[pos.prevSegment - 1].vel += momentum / 0.05f * (1f - pos.distanceToNext);
			}
			else
			{
				base.mainBodyChunk.pos += momentum / base.mainBodyChunk.mass * (1f - pos.distanceToNext);
				base.mainBodyChunk.vel += momentum / base.mainBodyChunk.mass * (1f - pos.distanceToNext);
			}
			if (pos.prevSegment < neck.tChunks.Length - 1)
			{
				neck.tChunks[pos.prevSegment].pos += momentum / 0.1f * pos.distanceToNext;
				neck.tChunks[pos.prevSegment].vel += momentum / 0.05f * pos.distanceToNext;
			}
			else
			{
				base.bodyChunks[4].pos += momentum / base.bodyChunks[4].mass * pos.distanceToNext;
				base.bodyChunks[4].vel += momentum / base.bodyChunks[4].mass * pos.distanceToNext;
			}
		}
		else
		{
			if (pos.prevSegment > 0)
			{
				tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
				tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
			}
			else
			{
				tentacles[pos.appendage.appIndex - 1].connectedChunk.pos += momentum / tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
				tentacles[pos.appendage.appIndex - 1].connectedChunk.vel += momentum / tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
			}
			tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
			tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
		}
	}

	public void SnapTowards(Vector2 pos)
	{
		snapAtPos = pos;
		Snap(null);
	}

	public bool CheckTentacleModeOr(VultureTentacle.Mode mode)
	{
		bool flag = false;
		for (int i = 0; i < tentacles.Length; i++)
		{
			flag = flag || tentacles[i].mode == mode;
		}
		return flag;
	}

	public bool CheckTentacleModeAnd(VultureTentacle.Mode mode)
	{
		bool flag = true;
		for (int i = 0; i < tentacles.Length; i++)
		{
			flag = flag && tentacles[i].mode == mode;
		}
		return flag;
	}

	public int TentacleMaxReleaseInd()
	{
		float num = -1f;
		int result = -1;
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (tentacles[i].ReleaseScore() > num || num == -1f)
			{
				num = tentacles[i].ReleaseScore();
				result = i;
			}
		}
		return result;
	}

	public BodyChunk Head()
	{
		return base.bodyChunks[4];
	}

	public void JawSlamShut()
	{
		Vector2 vector = Custom.DirVec(neck.Tip.pos, Head().pos);
		neck.Tip.vel -= vector * 10f;
		neck.Tip.pos += vector * 20f;
		Head().pos += vector * 20f;
		int num = 0;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (base.grasps[0] != null)
			{
				break;
			}
			Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
			if (room.abstractRoom.creatures[i] == base.abstractCreature || !AI.DoIWantToBiteCreature(room.abstractRoom.creatures[i]) || realizedCreature == null || realizedCreature.enteringShortCut.HasValue || realizedCreature.inShortcut)
			{
				continue;
			}
			for (int j = 0; j < realizedCreature.bodyChunks.Length; j++)
			{
				if (base.grasps[0] != null)
				{
					break;
				}
				if (!Custom.DistLess(Head().pos + vector * 20f, realizedCreature.bodyChunks[j].pos, 20f + realizedCreature.bodyChunks[j].rad) || !room.VisualContact(Head().pos, realizedCreature.bodyChunks[j].pos))
				{
					continue;
				}
				if (realizedCreature != null)
				{
					num = ((!(realizedCreature is Player)) ? 1 : 2);
					if (!AI.OnlyHurtDontGrab(realizedCreature))
					{
						Grab(realizedCreature, 0, j, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: true);
						AI.creatureLooker.LookAtNothing();
						jawOpen = 0.15f;
						jawVel = 0f;
						realizedCreature.Violence(Head(), Custom.DirVec(Head().pos, realizedCreature.bodyChunks[j].pos) * 4f, realizedCreature.bodyChunks[j], null, DamageType.Bite, 1.2f, 30f);
					}
					else
					{
						realizedCreature.Violence(Head(), Custom.DirVec(Head().pos, realizedCreature.bodyChunks[j].pos) * 4f, realizedCreature.bodyChunks[j], null, DamageType.Bite, 1.2f, 20f);
					}
				}
				break;
			}
			if (!(realizedCreature is DaddyLongLegs))
			{
				continue;
			}
			for (int k = 0; k < (realizedCreature as DaddyLongLegs).tentacles.Length; k++)
			{
				for (int l = 0; l < (realizedCreature as DaddyLongLegs).tentacles[k].tChunks.Length; l++)
				{
					if (Custom.DistLess(Head().pos + vector * 20f, (realizedCreature as DaddyLongLegs).tentacles[k].tChunks[l].pos, 20f))
					{
						(realizedCreature as DaddyLongLegs).tentacles[k].stun = UnityEngine.Random.Range(10, 70);
						for (int m = l; m < (realizedCreature as DaddyLongLegs).tentacles[k].tChunks.Length; m++)
						{
							(realizedCreature as DaddyLongLegs).tentacles[k].tChunks[m].vel += Custom.DirVec((realizedCreature as DaddyLongLegs).tentacles[k].tChunks[m].pos, (realizedCreature as DaddyLongLegs).tentacles[k].connectedChunk.pos) * Mathf.Lerp(10f, 50f, UnityEngine.Random.value);
						}
						break;
					}
				}
			}
		}
		switch (num)
		{
		case 0:
			room.PlaySound(SoundID.Miros_Beak_Snap_Miss, Head());
			break;
		case 1:
			room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Slugcat, Head());
			break;
		default:
			room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Other, Head());
			break;
		}
	}

	public bool isLaserActive()
	{
		if (!base.dead && laserCounter > 0)
		{
			return base.grasps[0] == null;
		}
		return false;
	}

	private void FireLaser()
	{
		if (laserCounter <= 0 && !base.dead && MostlyConsious && base.graphicsModule != null && !(base.graphicsModule as VultureGraphics).shadowMode)
		{
			laserCounter = 200;
			if (room != null)
			{
				LaserLight = new LightSource(new Vector2(-1000f, -1000f), environmentalLight: false, new Color(0.1f, 1f, 0.1f), this);
				room.AddObject(LaserLight);
				LaserLight.HardSetAlpha(1f);
				LaserLight.HardSetRad(200f);
			}
		}
	}

	public void LaserExplosion()
	{
		if (room == null)
		{
			return;
		}
		Vector2 pos = Head().pos;
		Vector2 vector = Custom.DirVec(neck.Tip.pos, pos);
		vector *= -1f;
		Vector2 corner = Custom.RectCollision(pos, pos - vector * 100000f, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
		IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, pos, corner);
		if (!intVector.HasValue)
		{
			return;
		}
		Color color = new Color(1f, 0.4f, 0.3f);
		corner = Custom.RectCollision(corner, pos, room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
		room.AddObject(new Explosion(room, this, corner, 7, 250f, 6.2f, 2f, 280f, 0.25f, this, 0.3f, 160f, 1f));
		room.AddObject(new Explosion.ExplosionLight(corner, 280f, 1f, 7, color));
		room.AddObject(new Explosion.ExplosionLight(corner, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new ShockWave(corner, 330f, 0.045f, 5));
		for (int i = 0; i < 25; i++)
		{
			Vector2 vector2 = Custom.RNV();
			if (room.GetTile(corner + vector2 * 20f).Solid)
			{
				if (!room.GetTile(corner - vector2 * 20f).Solid)
				{
					vector2 *= -1f;
				}
				else
				{
					vector2 = Custom.RNV();
				}
			}
			for (int j = 0; j < 3; j++)
			{
				room.AddObject(new Spark(corner + vector2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), vector2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(color, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
			}
			room.AddObject(new Explosion.FlashingSmoke(corner + vector2 * 40f * UnityEngine.Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), color, UnityEngine.Random.Range(3, 11)));
		}
		for (int k = 0; k < 6; k++)
		{
			room.AddObject(new ScavengerBomb.BombFragment(corner, Custom.DegToVec(((float)k + UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, UnityEngine.Random.value)));
		}
		room.ScreenMovement(corner, default(Vector2), 0.9f);
		for (int l = 0; l < abstractPhysicalObject.stuckObjects.Count; l++)
		{
			abstractPhysicalObject.stuckObjects[l].Deactivate();
		}
		room.PlaySound(SoundID.Bomb_Explode, corner);
		room.InGameNoise(new InGameNoise(corner, 9000f, this, 1f));
	}
}
