using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LizardTongue
{
	public class State : ExtEnum<State>
	{
		public static readonly State Hidden = new State("Hidden", register: true);

		public static readonly State LashingOut = new State("LashingOut", register: true);

		public static readonly State Attatched = new State("Attatched", register: true);

		public static readonly State AttachedInSmallObject = new State("AttachedInSmallObject", register: true);

		public static readonly State Retracting = new State("Retracting", register: true);

		public static readonly State StuckInTerrain = new State("StuckInTerrain", register: true);

		public State(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Lizard lizard;

	public Vector2 lastPos;

	public Vector2 pos;

	public Vector2 vel;

	public Vector2[] graphPos;

	public float range;

	private readonly float elasticRange;

	public float dist;

	private readonly float totR;

	public float reelIn;

	public float reelInSpeed;

	public float chunkDrag;

	public float terrainDrag;

	public float lashOutSpeed;

	public float dragElasticity;

	public float emptyElasticity;

	public float involuntaryReleaseChance;

	public float voluntaryReleaseChance;

	public float attachTerrainChance;

	public bool baseDragOnly;

	public bool attachesBackgroundWalls;

	public float pullAtChunkRatio;

	public float detatchMinDistanceTerrain;

	public float detatchMinDistanceCreature;

	public float totRExtraLimit;

	public BodyChunk attached;

	public int stuckCounter;

	private int delay;

	private bool biteInanimate;

	public State state;

	private float totRange => totR * reelIn;

	private float elRange => totR * elasticRange * reelIn * reelIn;

	public bool Out => state != State.Hidden;

	public bool Ready => delay < 1;

	public float ChunkDrag
	{
		get
		{
			if (baseDragOnly)
			{
				return chunkDrag * dist;
			}
			return Mathf.Max(chunkDrag * dist, ((dist < totRange) ? dragElasticity : 1f) * (dist - elRange) * Mathf.InverseLerp(elRange, totRange, dist));
		}
	}

	public float TerrainDrag
	{
		get
		{
			if (baseDragOnly)
			{
				return terrainDrag * dist;
			}
			return Mathf.Max(terrainDrag * dist, ((dist < totRange) ? dragElasticity : 1f) * (dist - elRange) * Mathf.InverseLerp(elRange, totRange, dist));
		}
	}

	public float Stretched => Mathf.InverseLerp(0f, elRange, dist);

	public float SuperStretched => Mathf.InverseLerp(elRange, totRange, dist);

	public float CombinedStretched => Mathf.InverseLerp(0f, totR, dist);

	public bool StuckToSomething
	{
		get
		{
			if (!(state == State.Attatched))
			{
				return state == State.StuckInTerrain;
			}
			return true;
		}
	}

	public LizardTongue(Lizard lizard)
	{
		this.lizard = lizard;
		state = State.Hidden;
		attachTerrainChance = 0.5f;
		pullAtChunkRatio = 1f;
		detatchMinDistanceTerrain = 20f;
		detatchMinDistanceCreature = 20f;
		totRExtraLimit = 40f;
		if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			range = 540f;
			elasticRange = 0.1f;
			lashOutSpeed = 30f;
			reelInSpeed = 0.0033333334f;
			chunkDrag = 0f;
			terrainDrag = 0f;
			dragElasticity = 0.05f;
			emptyElasticity = 0.01f;
			involuntaryReleaseChance = 0.000125f;
			voluntaryReleaseChance = 0.05f;
		}
		else if (lizard.Template.type == CreatureTemplate.Type.Salamander)
		{
			range = 140f;
			elasticRange = 0.55f;
			lashOutSpeed = 16f;
			reelInSpeed = 0.000625f;
			chunkDrag = 0.01f;
			terrainDrag = 0.01f;
			dragElasticity = 0.1f;
			emptyElasticity = 0.8f;
			involuntaryReleaseChance = 0.0025f;
			voluntaryReleaseChance = 0.0125f;
		}
		else if (lizard.Template.type == CreatureTemplate.Type.BlueLizard)
		{
			range = 190f;
			elasticRange = 0f;
			lashOutSpeed = 26f;
			reelInSpeed = 0f;
			chunkDrag = 0.04f;
			terrainDrag = 0.04f;
			dragElasticity = 0f;
			emptyElasticity = 0.07f;
			involuntaryReleaseChance = 0.0033333334f;
			voluntaryReleaseChance = 1f;
			baseDragOnly = true;
		}
		else if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			range = 140f;
			elasticRange = 0.55f;
			lashOutSpeed = 16f;
			reelInSpeed = 0.002f;
			chunkDrag = 0.01f;
			terrainDrag = 0.01f;
			dragElasticity = 0.1f;
			emptyElasticity = 0.8f;
			involuntaryReleaseChance = 0.005f;
			voluntaryReleaseChance = 0.02f;
		}
		else if (ModManager.MMF && MMF.cfgAlphaRedLizards.Value && lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			range = 340f;
			elasticRange = 0.1f;
			lashOutSpeed = 37f;
			reelInSpeed = 0.0043333336f;
			chunkDrag = 0f;
			terrainDrag = 0f;
			dragElasticity = 0.05f;
			emptyElasticity = 0.01f;
			involuntaryReleaseChance = 1f;
			voluntaryReleaseChance = 1f;
		}
		else if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
		{
			range = 280f;
			elasticRange = 0.1f;
			lashOutSpeed = 30f;
			reelInSpeed = 0.002f;
			chunkDrag = 0.1f;
			terrainDrag = 0.05f;
			dragElasticity = 0.02f;
			emptyElasticity = 0.003f;
			involuntaryReleaseChance = 0.0025f;
			voluntaryReleaseChance = 0.005f;
			baseDragOnly = true;
			attachesBackgroundWalls = true;
			attachTerrainChance = 1f;
			pullAtChunkRatio = 0.05f;
			detatchMinDistanceTerrain = 60f;
			totRExtraLimit = 80f;
		}
		totR = range * 1.1f;
		graphPos = new Vector2[2];
	}

	public void Update()
	{
		graphPos[1] = graphPos[0];
		if (!lizard.Consious && state != State.Hidden)
		{
			state = State.Retracting;
		}
		if (Out)
		{
			if (state != State.Retracting && !lizard.room.VisualContact(lizard.bodyChunks[0].pos, pos))
			{
				state = State.Retracting;
			}
			dist = Vector2.Distance(pos, lizard.mainBodyChunk.pos);
			lizard.bodyChunks[0].vel += Custom.DirVec(lizard.bodyChunks[0].pos, pos) * 4f;
			lizard.bodyChunks[1].vel -= Custom.DirVec(lizard.bodyChunks[0].pos, pos) * 4f;
			if (StuckToSomething && (reelIn < 0f || Vector2.Dot(Custom.DirVec(lizard.bodyChunks[0].pos, pos), Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos)) < -0.5f || dist > totR + totRExtraLimit))
			{
				Retract();
			}
			if (state == State.StuckInTerrain && dist < detatchMinDistanceTerrain)
			{
				Retract();
			}
			if (state == State.Attatched && dist < detatchMinDistanceCreature)
			{
				Retract();
			}
			if (StuckToSomething)
			{
				stuckCounter++;
				if (lizard.AI.DoIWantToHoldThisWithMyTongue(attached))
				{
					if (Random.value < involuntaryReleaseChance && dist > elasticRange)
					{
						Retract();
					}
				}
				else if (Random.value < voluntaryReleaseChance)
				{
					Retract();
				}
				if (lizard.Template.type == CreatureTemplate.Type.BlueLizard && ((lizard.JawOpen < 0.1f && Random.value < 0.125f) || Random.value < Mathf.InverseLerp(20f, 120f, stuckCounter)))
				{
					Retract();
				}
			}
			else
			{
				stuckCounter = 0;
			}
		}
		if (state == State.Hidden)
		{
			if (delay > 0)
			{
				delay--;
			}
			lastPos = lizard.mainBodyChunk.lastPos;
			pos = lizard.mainBodyChunk.pos;
			graphPos[0] = pos;
			vel *= 0f;
		}
		else if (state == State.LashingOut)
		{
			lastPos = pos;
			pos += vel;
			if (lizard.room.PointSubmerged(pos))
			{
				vel *= 0.9f;
			}
			else
			{
				vel.y -= 0.1f;
			}
			graphPos[0] = pos;
			if (!Custom.DistLess(pos, lizard.mainBodyChunk.pos, totRange))
			{
				vel += Custom.DirVec(pos, lizard.mainBodyChunk.pos) * (dist - totRange);
				pos += Custom.DirVec(pos, lizard.mainBodyChunk.pos) * (dist - totRange);
			}
			else if (!Custom.DistLess(pos, lizard.mainBodyChunk.pos, elRange))
			{
				vel += Custom.DirVec(pos, lizard.mainBodyChunk.pos) * (dist - elRange) * emptyElasticity;
			}
			Vector2 rhs = Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos);
			attached = SharedPhysics.TraceProjectileAgainstBodyChunks(null, lizard.room, lastPos, ref pos, 3f, -1, lizard, hitAppendages: false).chunk;
			if (attached != null)
			{
				if (attached.owner is Spear && ((attached.owner as Spear).mode == Weapon.Mode.StuckInWall || (attached.owner as Spear).mode == Weapon.Mode.StuckInCreature))
				{
					Custom.Log("Spear Detached by tongue");
					(attached.owner as Spear).resetHorizontalBeamState();
					(attached.owner as Spear).PulledOutOfStuckObject();
					(attached.owner as Spear).ChangeMode(Weapon.Mode.Free);
				}
				if (attached.owner.TotalMass < 0.2f)
				{
					state = State.AttachedInSmallObject;
					attached.pos = pos;
					for (int num = attached.owner.grabbedBy.Count - 1; num >= 0; num--)
					{
						biteInanimate = true;
						attached.owner.grabbedBy[num].grabber.mainBodyChunk.vel += Custom.DirVec(attached.owner.grabbedBy[num].grabber.mainBodyChunk.pos, lizard.mainBodyChunk.pos) * 1.8f / attached.owner.grabbedBy[num].grabber.mainBodyChunk.mass;
						attached.owner.grabbedBy[num].grabber.ReleaseGrasp(attached.owner.grabbedBy[num].graspUsed);
						Custom.Log("--- lizard tongue snatch");
					}
				}
				else
				{
					Impact();
					state = State.Attatched;
					attached.vel += vel * (0.15f / attached.mass);
				}
				lizard.room.PlaySound((attached.owner is Player) ? SoundID.Lizard_Tongue_Attatch_Player : SoundID.Lizard_Tongue_Attatch_NPC, pos);
			}
			else if (lizard.room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
			{
				Vector2 vector = lastPos;
				for (float num2 = 0f; num2 < 1f && lizard.room.GetTile(Vector2.Lerp(lastPos, pos, num2)).Terrain != Room.Tile.TerrainType.Solid; num2 += 0.05f)
				{
					vector = Vector2.Lerp(lastPos, pos, num2);
				}
				pos = vector;
				lizard.room.PlaySound(SoundID.Lizard_Tongue_Attatch_Terrain, vector);
				if (Random.value < attachTerrainChance)
				{
					state = State.StuckInTerrain;
				}
				else
				{
					state = State.Retracting;
				}
				Impact();
				vel *= 0f;
			}
			else if (Vector2.Dot(vel.normalized, rhs) < 0f)
			{
				Vector2 vector2 = lastPos;
				bool flag = false;
				if (attachesBackgroundWalls)
				{
					for (float num3 = 0f; num3 < 1f; num3 += 0.05f)
					{
						if (lizard.room.GetTile(Vector2.Lerp(lastPos, pos, num3)).wallbehind)
						{
							flag = true;
							break;
						}
						vector2 = Vector2.Lerp(lastPos, pos, num3);
					}
				}
				if (attachesBackgroundWalls && (lizard.room.GetTile(pos).wallbehind || flag))
				{
					pos = vector2;
					lizard.room.PlaySound(SoundID.Lizard_Tongue_Attatch_Terrain, vector2);
					if (Random.value < attachTerrainChance)
					{
						state = State.StuckInTerrain;
					}
					else
					{
						state = State.Retracting;
					}
					Impact();
					vel *= 0f;
				}
				else
				{
					state = State.Retracting;
				}
			}
			lizard.JawOpen = Mathf.Clamp(lizard.JawOpen + 0.4f, 0f, 1f);
		}
		else if (state == State.Attatched)
		{
			if (attached.owner.room != lizard.room)
			{
				Retract();
			}
			else
			{
				pos = attached.pos;
				vel = attached.vel;
				lastPos = attached.lastPos;
				graphPos[0] = pos + Custom.DirVec(pos, lizard.mainBodyChunk.pos) * Mathf.Max(attached.rad - 7.5f, 0f);
				reelIn -= reelInSpeed;
				if (Random.value < 0.5f && !Custom.DistLess(lizard.mainBodyChunk.pos, pos, Mathf.Lerp(elRange, totRange, 0.75f)))
				{
					reelIn = Mathf.Min(reelIn + 0.01f, 1f);
				}
				PullAtChunk();
			}
			float num4 = Mathf.Lerp(0.04f, 0.11f, Mathf.InverseLerp(range, 140f, 540f));
			if (attached != null && attached.owner is Player && Random.value < Mathf.Lerp(0f, (ModManager.MMF && MMF.cfgGraspWiggling.Value) ? num4 : 0.05f, (attached.owner as Player).GraspWiggle))
			{
				Retract();
			}
		}
		else if (state == State.Retracting || state == State.AttachedInSmallObject)
		{
			lastPos = pos;
			pos += vel;
			graphPos[0] = pos;
			Vector2 vector3 = lizard.mainBodyChunk.pos + Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos) * dist * 0.5f;
			vel = Vector2.Lerp(vel, Custom.DirVec(pos, vector3) * (Vector2.Distance(pos, vector3) + 5f) * 0.5f, 0.5f);
			if (state == State.AttachedInSmallObject)
			{
				if (attached == null)
				{
					state = State.Retracting;
				}
				else
				{
					if (attached.owner is Weapon)
					{
						(attached.owner as Weapon).setRotation = Custom.PerpendicularVector(lizard.mainBodyChunk.pos, pos);
					}
					attached.pos = pos;
					attached.vel = vel;
				}
			}
			if (!Custom.DistLess(pos, lizard.mainBodyChunk.pos, 10f))
			{
				return;
			}
			lizard.room.PlaySound(SoundID.Lizard_Tongue_Go_Back_In_Mouth, lizard.mainBodyChunk);
			state = State.Hidden;
			if (attached != null)
			{
				if (biteInanimate)
				{
					lizard.GrabInanimate(attached);
				}
				attached = null;
			}
		}
		else if (state == State.StuckInTerrain)
		{
			graphPos[0] = pos;
			vel *= 0f;
			Vector2 vector4 = Custom.DirVec(lizard.mainBodyChunk.pos, pos);
			lizard.mainBodyChunk.vel += vector4 * TerrainDrag;
			lizard.mainBodyChunk.pos += vector4 * TerrainDrag;
			reelIn -= reelInSpeed;
		}
	}

	public void LashOut(Vector2 target)
	{
		target.y += Vector2.Distance(lizard.mainBodyChunk.pos, target) * 0.05f;
		Vector2 lhs = Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos);
		Vector2 vector = Custom.DirVec(lizard.bodyChunks[0].pos, target);
		if (!(Vector2.Dot(lhs, vector) > 0.3f))
		{
			return;
		}
		float num = Mathf.Lerp(Mathf.InverseLerp(elRange * 0.5f, totR, Vector2.Distance(lizard.mainBodyChunk.pos, target)), 1f, 0.75f);
		vel = vector * lashOutSpeed * num;
		state = State.LashingOut;
		reelIn = 1f;
		delay = 20;
		biteInanimate = false;
		lizard.JawOpen = 1f;
		if (lizard.Template.type != CreatureTemplate.Type.BlueLizard)
		{
			lizard.bodyChunks[1].vel -= vector * lashOutSpeed;
		}
		else
		{
			lizard.bodyChunks[1].vel -= vector * 5f;
		}
		if (lizard.graphicsModule != null)
		{
			for (int i = 0; i < (lizard.graphicsModule as LizardGraphics).tongue.Length; i++)
			{
				Vector2 vector2 = vector * lashOutSpeed * num + Custom.DegToVec(Random.value * 360f) * Random.value * lashOutSpeed * num * 0.75f;
				lizard.room.AddObject(new WaterDrip(lizard.mainBodyChunk.pos, vector2 * 0.5f * Random.value, waterColor: false));
				vector2 = ((!(lizard.Template.type != CreatureTemplate.Type.BlueLizard)) ? lizard.bodyChunks[0].vel : ((Vector2)Vector3.Slerp(vector2, Custom.DirVec(lizard.bodyChunks[0].pos, (lizard.graphicsModule as LizardGraphics).head.pos) * lashOutSpeed * num, (float)i / (float)(lizard.graphicsModule as LizardGraphics).tongue.Length)));
				(lizard.graphicsModule as LizardGraphics).tongue[i].vel = vector2;
			}
			if ((lizard.graphicsModule as LizardGraphics).snowAccCosmetic != null)
			{
				(lizard.graphicsModule as LizardGraphics).snowAccCosmetic.ShatterDebris();
			}
		}
		lizard.room.PlaySound((lizard.Template.type == CreatureTemplate.Type.BlueLizard) ? SoundID.Blue_Lizard_Tongue_Shoot_Out : SoundID.White_Lizard_Tongue_Shoot_Out, lizard.mainBodyChunk);
	}

	public void Retract()
	{
		if (state == State.StuckInTerrain)
		{
			lizard.room.PlaySound(SoundID.Lizard_Tongue_Detatch_NPC_Or_Terrain, pos);
		}
		else if (attached != null)
		{
			lizard.room.PlaySound((attached.owner is Player) ? SoundID.Lizard_Tongue_Detatch_Player : SoundID.Lizard_Tongue_Detatch_NPC_Or_Terrain, pos);
		}
		state = State.Retracting;
		attached = null;
	}

	private void PullAtChunk()
	{
		Vector2 vector = Custom.DirVec(lizard.mainBodyChunk.pos, attached.pos);
		float num = lizard.mainBodyChunk.mass / (lizard.mainBodyChunk.mass + attached.mass);
		lizard.mainBodyChunk.vel += vector * ChunkDrag * (1f - num);
		lizard.mainBodyChunk.pos += vector * ChunkDrag * (1f - num);
		attached.vel -= vector * ChunkDrag * num * pullAtChunkRatio;
		attached.pos -= vector * ChunkDrag * num * pullAtChunkRatio;
	}

	private void Impact()
	{
		if (lizard.graphicsModule != null)
		{
			for (int i = 0; i < 3; i++)
			{
				lizard.room.AddObject(new WaterDrip(pos, (-vel + Custom.DegToVec(Random.value * 360f) * vel.magnitude * Random.value) * Random.value, waterColor: false));
			}
			GenericBodyPart[] tongue = (lizard.graphicsModule as LizardGraphics).tongue;
			foreach (GenericBodyPart obj in tongue)
			{
				obj.pos += Custom.DegToVec(Random.value * 360f) * Random.value * 0.5f * vel.magnitude;
				obj.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 0.5f * vel.magnitude;
			}
		}
	}
}
