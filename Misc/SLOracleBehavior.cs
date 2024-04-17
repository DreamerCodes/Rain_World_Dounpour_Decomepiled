using System;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SLOracleBehavior : OracleBehavior
{
	public class MovementBehavior : ExtEnum<MovementBehavior>
	{
		public static readonly MovementBehavior Idle = new MovementBehavior("Idle", register: true);

		public static readonly MovementBehavior Meditate = new MovementBehavior("Meditate", register: true);

		public static readonly MovementBehavior KeepDistance = new MovementBehavior("KeepDistance", register: true);

		public static readonly MovementBehavior Investigate = new MovementBehavior("Investigate", register: true);

		public static readonly MovementBehavior Talk = new MovementBehavior("Talk", register: true);

		public static readonly MovementBehavior ShowMedia = new MovementBehavior("ShowMedia", register: true);

		public static readonly MovementBehavior InvestigateSlugcat = new MovementBehavior("InvestigateSlugcat", register: true);

		public MovementBehavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public bool hasNoticedPlayer;

	private float crawlCounter;

	public bool holdKnees = true;

	public bool protest;

	public float protestCounter;

	public bool armsProtest;

	protected bool conversationAdded;

	public int convertSwarmerCounter;

	public PhysicalObject holdingObject;

	public List<SoundID> painLines;

	public List<EntityID> pickedUpItemsThisRealization;

	private SLOrcacleState DEBUGSTATE;

	public SSOracleSwarmer reelInSwarmer;

	public float swarmerReelIn;

	public bool stillWakingUp;

	public int dontHoldKnees;

	public SuperStructureFuses fuses;

	private bool initWakeUpProcedure;

	private bool initRivuletEnding;

	public MovementBehavior movementBehavior;

	public bool moonActive;

	private double investigateAngle;

	private double invstAngSpeed;

	private Vector2 nextPos;

	public bool floatyMovement;

	private Vector2 currentGetTo;

	private double idleCounter;

	public int keepDistTime;

	private Vector2 lastPos;

	private Vector2 lastPosHandle;

	private Vector2 nextPosHandle;

	private double pathProgression;

	public int meditateTick;

	public int consistentShowMediaPosCounter;

	public Vector2 showMediaPos;

	private Vector2 baseIdeal;

	public Vector2 idealShowMediaPos;

	public int dehabilitateTime;

	public bool initiated;

	public SingularityBomb dangerousSingularity;

	private bool wasScaredBySingularity;

	public int timeOutOfSitZone;

	public bool forceFlightMode;

	public SLOracleRivuletEnding rivEnding;

	public Vector2? forcedShowMediaPos;

	public ProjectedImage displayImage;

	public int displayImageTimer;

	public int showMediaPhaseTime;

	public float Crawl => Mathf.Cos(crawlCounter * (float)Math.PI * 2f);

	public float CrawlSpeed => 0.5f + 0.5f * Mathf.Sin(crawlCounter * (float)Math.PI * 2f);

	public SLOrcacleState State
	{
		get
		{
			if (oracle.room.game.session is StoryGameSession)
			{
				return (oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState;
			}
			if (DEBUGSTATE == null)
			{
				DEBUGSTATE = new SLOrcacleState(isDebugState: true, null);
			}
			return DEBUGSTATE;
		}
	}

	public override Vector2 OracleGetToPos
	{
		get
		{
			if (moonActive)
			{
				Vector2 v = currentGetTo;
				if (floatyMovement && Custom.DistLess(oracle.firstChunk.pos, nextPos, 50f))
				{
					v = nextPos;
				}
				return ClampVectorInRoom(v);
			}
			if (oracle.room.game.IsMoonActive())
			{
				return new Vector2(1585f, 160f);
			}
			return new Vector2(1585f, ModManager.MSC ? 200f : 168f);
		}
	}

	public override Vector2 GetToDir
	{
		get
		{
			if (!moonActive)
			{
				if (InSitPosition)
				{
					return new Vector2(0f, 1f);
				}
				return Custom.DirVec(oracle.firstChunk.pos, OracleGetToPos);
			}
			if (movementBehavior == MovementBehavior.Idle)
			{
				return Custom.DegToVec((float)investigateAngle);
			}
			if (movementBehavior != MovementBehavior.Investigate)
			{
				return new Vector2(0f, 1f);
			}
			return -Custom.DegToVec((float)investigateAngle);
		}
	}

	public override bool EyesClosed
	{
		get
		{
			if (!stillWakingUp && oracle.health != 0f && (hasNoticedPlayer || !InSitPosition || holdingObject != null) && !(movementBehavior == MovementBehavior.Meditate))
			{
				return !oracle.Consious;
			}
			return true;
		}
	}

	public bool InSitPosition
	{
		get
		{
			if (oracle.room.GetTilePosition(oracle.firstChunk.pos).x == 79)
			{
				return !moonActive;
			}
			return false;
		}
	}

	public bool WantsToSit
	{
		get
		{
			if (!forceFlightMode)
			{
				if (timeOutOfSitZone >= 40 && holdingObject == null && oracle.Consious && dehabilitateTime <= 0)
				{
					if (this is SLOracleBehaviorHasMark)
					{
						return (this as SLOracleBehaviorHasMark).DamagedMode;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public SLOracleBehavior(Oracle oracle)
		: base(oracle)
	{
		pickedUpItemsThisRealization = new List<EntityID>();
		painLines = new List<SoundID>();
		painLines.Add(SoundID.SL_AI_Pain_1);
		painLines.Add(SoundID.SL_AI_Pain_2);
		oracle.health = Mathf.InverseLerp(0f, 5f, State.neuronsLeft);
		for (int i = 0; i < oracle.room.updateList.Count; i++)
		{
			if (oracle.room.updateList[i] is SuperStructureFuses)
			{
				fuses = oracle.room.updateList[i] as SuperStructureFuses;
				fuses.power = 0f;
				fuses.powerFlicker = 0f;
				break;
			}
		}
	}

	private void InitCutsceneObjects()
	{
		if (!initWakeUpProcedure && player != null && oracle.room.game.IsStorySession && oracle.room.game.GetStorySession.saveStateNumber == SlugcatStats.Name.Red && State.neuronsLeft < 1)
		{
			oracle.room.AddObject(new SLOracleWakeUpProcedure(oracle));
			initWakeUpProcedure = true;
		}
		if (!initRivuletEnding && ModManager.MSC && player != null && oracle.room.game.IsStorySession && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && oracle.room.game.IsMoonActive() && !oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && State.neuronsLeft > 0 && (!ModManager.Expedition || !oracle.room.game.rainWorld.ExpeditionMode))
		{
			rivEnding = new SLOracleRivuletEnding(oracle);
			oracle.room.AddObject(rivEnding);
			initRivuletEnding = true;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		InitCutsceneObjects();
		if (ModManager.MSC)
		{
			if (!initiated)
			{
				if (oracle.myScreen == null)
				{
					oracle.myScreen = new OracleProjectionScreen(oracle.room, this);
				}
				initiated = true;
			}
			if (oracle.room.game.IsMoonActive())
			{
				oracle.room.gravity = 0.2f;
			}
		}
		if (stillWakingUp)
		{
			dontHoldKnees = Math.Max(dontHoldKnees, 620);
		}
		else
		{
			oracle.health = Mathf.InverseLerp(0f, 5f, State.neuronsLeft);
		}
		if (dontHoldKnees > 0)
		{
			dontHoldKnees--;
		}
		if (InSitPosition && oracle.arm != null)
		{
			for (int i = 0; i < oracle.arm.joints.Length; i++)
			{
				if (oracle.arm.joints[i].vel.magnitude > 0.05f)
				{
					oracle.arm.joints[i].vel *= 0.98f;
				}
			}
		}
		moonActive = oracle.room.game.IsMoonActive();
		if (!oracle.Consious)
		{
			forceFlightMode = false;
			if (ModManager.MSC)
			{
				oracle.SetLocalGravity(1f);
			}
			return;
		}
		if (ModManager.MSC)
		{
			if (!forceFlightMode)
			{
				if (oracle.room.game.IsMoonActive() && player != null && player.mainBodyChunk.pos.x >= 1430f && player.mainBodyChunk.pos.x <= 1560f && oracle.firstChunk.pos.x > player.mainBodyChunk.pos.x)
				{
					timeOutOfSitZone = 0;
				}
				else if (!oracle.room.game.IsMoonActive() && player != null && player.mainBodyChunk.pos.x >= 1430f && player.mainBodyChunk.pos.x <= 1660f && oracle.firstChunk.pos.x > player.mainBodyChunk.pos.x)
				{
					timeOutOfSitZone = 0;
				}
				else
				{
					timeOutOfSitZone++;
				}
				if (WantsToSit && player != null)
				{
					moonActive = false;
					setMovementBehavior(MovementBehavior.InvestigateSlugcat);
					invstAngSpeed = 0.2;
					lookPoint = ((player.mainBodyChunk.pos.x <= oracle.room.PixelWidth * 0.85f) ? new Vector2(player.mainBodyChunk.pos.x + 100f, player.mainBodyChunk.pos.y + 150f) : new Vector2(player.mainBodyChunk.pos.x - 100f, player.mainBodyChunk.pos.y + 150f));
					if (oracle.room.game.IsMoonActive())
					{
						oracle.SetLocalGravity(Mathf.Lerp(oracle.gravity, 0f, 0.04f));
					}
					else
					{
						oracle.SetLocalGravity(Mathf.Lerp(oracle.gravity, 0.9f, 0.04f));
					}
				}
			}
			if ((movementBehavior == MovementBehavior.ShowMedia || movementBehavior == MovementBehavior.KeepDistance) && oracle.room.game.IsMoonActive())
			{
				moonActive = true;
				oracle.SetLocalGravity(Mathf.Lerp(oracle.gravity, 1f, 0.2f));
			}
		}
		for (int j = 0; j < oracle.room.abstractRoom.entities.Count; j++)
		{
			if (oracle.room.abstractRoom.entities[j] is AbstractPhysicalObject && (oracle.room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject != null && Custom.DistLess((oracle.room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject.firstChunk.pos, oracle.firstChunk.pos, 500f) && (oracle.room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject.grabbedBy.Count == 0 && (oracle.room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject is OracleSwarmer)
			{
				OracleSwarmer oracleSwarmer = (oracle.room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject as OracleSwarmer;
				oracleSwarmer.affectedByGravity = Mathf.InverseLerp(300f, 500f, Vector2.Distance(oracleSwarmer.firstChunk.pos, oracle.firstChunk.pos));
				if (reelInSwarmer == null && oracleSwarmer is SSOracleSwarmer && holdingObject == null)
				{
					reelInSwarmer = oracleSwarmer as SSOracleSwarmer;
				}
			}
		}
		if (reelInSwarmer != null && holdingObject == null)
		{
			swarmerReelIn = Mathf.Min(1f, swarmerReelIn + 1f / 60f);
			reelInSwarmer.firstChunk.vel *= Custom.LerpMap(swarmerReelIn, 0.4f, 1f, 1f, 0.3f, 6f);
			reelInSwarmer.firstChunk.vel += Custom.DirVec(reelInSwarmer.firstChunk.pos, oracle.firstChunk.pos) * 3.2f * swarmerReelIn;
			if (Custom.DistLess(reelInSwarmer.firstChunk.pos, oracle.firstChunk.pos, 30f))
			{
				GrabObject(reelInSwarmer);
				reelInSwarmer = null;
			}
		}
		else
		{
			swarmerReelIn = 0f;
		}
		dehabilitateTime--;
		if (!hasNoticedPlayer)
		{
			if (safariCreature != null)
			{
				lookPoint = safariCreature.mainBodyChunk.pos;
			}
			else if (InSitPosition)
			{
				lookPoint = oracle.firstChunk.pos + new Vector2(-145f, -45f);
			}
			else
			{
				lookPoint = OracleGetToPos;
			}
			if (ModManager.CoopAvailable)
			{
				IEnumerable<Player> source = base.PlayersInRoom.Where((Player p) => p.mainBodyChunk.pos.x > 1160f);
				if (base.PlayersInRoom.Count > 0 && source.Count() > 0)
				{
					player = source.OrderBy((Player x) => x.mainBodyChunk.pos.x).ToList()[0];
				}
			}
			if (player != null && player.room == oracle.room && player.mainBodyChunk.pos.x > 1160f)
			{
				hasNoticedPlayer = true;
				oracle.firstChunk.vel += Custom.DegToVec(45f) * 3f;
				oracle.bodyChunks[1].vel += Custom.DegToVec(-90f) * 2f;
			}
		}
		else if (ModManager.MSC && player != null && player.room == oracle.room && player.mainBodyChunk.pos.x < 1125f && moonActive)
		{
			hasNoticedPlayer = false;
			idleCounter = 0.0;
		}
		if (ModManager.MSC && UnityEngine.Random.value < 1f / 30f)
		{
			idealShowMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
			showMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
			if (!forcedShowMediaPos.HasValue)
			{
				idealShowMediaPos = ClampMediaPos(idealShowMediaPos);
				showMediaPos = ClampMediaPos(showMediaPos);
			}
		}
		if (holdingObject != null)
		{
			if (!oracle.Consious || holdingObject.grabbedBy.Count > 0)
			{
				if (this is SLOracleBehaviorHasMark && holdingObject.grabbedBy.Count > 0)
				{
					(this as SLOracleBehaviorHasMark).PlayerInterruptByTakingItem();
				}
				holdingObject = null;
			}
			else
			{
				holdingObject.firstChunk.MoveFromOutsideMyUpdate(eu, oracle.firstChunk.pos + new Vector2(-18f, -7f));
				holdingObject.firstChunk.vel *= 0f;
				if (holdingObject is SSOracleSwarmer && (oracle.room.game.cameras[0].hud.dialogBox == null || oracle.room.game.cameras[0].hud.dialogBox.messages.Count < 1))
				{
					convertSwarmerCounter++;
					if (convertSwarmerCounter > 40)
					{
						Vector2 pos = holdingObject.firstChunk.pos;
						holdingObject.Destroy();
						holdingObject = null;
						SLOracleSwarmer sLOracleSwarmer = new SLOracleSwarmer(new AbstractPhysicalObject(oracle.room.world, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, null, oracle.room.GetWorldCoordinate(pos), oracle.room.game.GetNewID()), oracle.room.world);
						oracle.room.abstractRoom.entities.Add(sLOracleSwarmer.abstractPhysicalObject);
						sLOracleSwarmer.firstChunk.HardSetPosition(pos);
						oracle.room.AddObject(sLOracleSwarmer);
						ConvertingSSSwarmer();
					}
				}
			}
		}
		if (moonActive)
		{
			UpdateActive(eu);
			oracle.arm.isActive = true;
			return;
		}
		if (InSitPosition)
		{
			if (holdingObject == null && dontHoldKnees < 1 && UnityEngine.Random.value < 0.025f && (player == null || !Custom.DistLess(oracle.firstChunk.pos, player.DangerPos, 50f)) && !protest && oracle.health >= 1f)
			{
				holdKnees = true;
			}
		}
		else
		{
			oracle.firstChunk.vel.x += ((oracle.firstChunk.pos.x < OracleGetToPos.x) ? 1f : (-1f)) * 0.6f * CrawlSpeed;
			if (player != null && player.DangerPos.x < oracle.firstChunk.pos.x)
			{
				if (oracle.firstChunk.ContactPoint.x != 0)
				{
					oracle.firstChunk.vel.y = Mathf.Lerp(oracle.firstChunk.vel.y, 1.2f, 0.5f) + 1.2f;
				}
				if (oracle.bodyChunks[1].ContactPoint.x != 0)
				{
					oracle.firstChunk.vel.y = Mathf.Lerp(oracle.firstChunk.vel.y, 1.2f, 0.5f) + 1.2f;
				}
			}
			if (player != null && !Custom.DistLess(oracle.firstChunk.pos, player.DangerPos, 50f) && (oracle.bodyChunks[1].pos.y > 140f || player.DangerPos.x < oracle.firstChunk.pos.x || Mathf.Abs(oracle.firstChunk.pos.x - oracle.firstChunk.lastPos.x) > 2f))
			{
				crawlCounter += 0.04f;
			}
			holdKnees = false;
		}
		if (oracle.arm.joints[2].pos.y < 140f)
		{
			oracle.arm.joints[2].pos.y = 140f;
			oracle.arm.joints[2].vel.y = Mathf.Abs(oracle.arm.joints[1].vel.y) * 0.2f;
		}
		oracle.WeightedPush(0, 1, new Vector2(0f, 1f), 4f * Mathf.InverseLerp(60f, 20f, Mathf.Abs(OracleGetToPos.x - oracle.firstChunk.pos.x)));
		oracle.arm.isActive = false;
	}

	public virtual void Pain()
	{
		if (oracle.Consious)
		{
			if (ModManager.MSC)
			{
				AirVoice(painLines[UnityEngine.Random.Range(0, painLines.Count)]);
				dehabilitateTime = 900;
				oracle.stun = 0;
			}
			else if ((painLines.Count > 0 && UnityEngine.Random.value < 1f / 3f) || painLines.Count >= 2)
			{
				AirVoice(painLines[0]);
				painLines.RemoveAt(0);
			}
		}
	}

	public virtual void ConvertingSSSwarmer()
	{
		State.neuronsLeft++;
		Custom.Log("Converting an SS swarmer,", State.neuronsLeft.ToString());
		State.InfluenceLike(0.65f);
		if (oracle.room.game.session is StoryGameSession)
		{
			(oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.angryWithPlayer = false;
		}
	}

	public void AirVoice(SoundID line)
	{
		if (voice != null)
		{
			if (voice.currentSoundObject != null)
			{
				voice.currentSoundObject.Stop();
			}
			voice.Destroy();
		}
		voice = oracle.room.PlaySound(line, oracle.firstChunk);
		voice.requireActiveUpkeep = line != SoundID.SL_AI_Pain_1 && line != SoundID.SL_AI_Pain_2;
	}

	public virtual void GrabObject(PhysicalObject obj)
	{
		bool flag = true;
		int num = 0;
		while (flag && num < pickedUpItemsThisRealization.Count)
		{
			if (obj.abstractPhysicalObject.ID == pickedUpItemsThisRealization[num])
			{
				flag = false;
			}
			num++;
		}
		if (flag)
		{
			pickedUpItemsThisRealization.Add(obj.abstractPhysicalObject.ID);
		}
		if (obj.graphicsModule != null)
		{
			obj.graphicsModule.BringSpritesToFront();
		}
		if (obj is IDrawable)
		{
			for (int i = 0; i < oracle.abstractPhysicalObject.world.game.cameras.Length; i++)
			{
				oracle.abstractPhysicalObject.world.game.cameras[i].MoveObjectToContainer(obj as IDrawable, null);
			}
		}
		holdingObject = obj;
	}

	public override void UnconciousUpdate()
	{
		base.UnconciousUpdate();
		InitCutsceneObjects();
		if (oracle.room.game.IsStorySession && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			oracle.SetLocalGravity(1f);
			if (oracle.room.world.rainCycle.brokenAntiGrav.on)
			{
				oracle.room.world.rainCycle.brokenAntiGrav.counter = -1;
				oracle.room.world.rainCycle.brokenAntiGrav.to = 0f;
			}
			oracle.arm.isActive = false;
			moonActive = false;
		}
	}

	public bool SingularityProtest()
	{
		if (dangerousSingularity == null || dangerousSingularity.slatedForDeletetion || oracle.glowers <= 0)
		{
			if (wasScaredBySingularity && oracle.glowers > 0)
			{
				if (this is SLOracleBehaviorHasMark)
				{
					if (UnityEngine.Random.value < 0.24f)
					{
						dialogBox.Interrupt(Translate("Why would you do that!?"), 7);
					}
					else if (UnityEngine.Random.value < 0.24f)
					{
						dialogBox.Interrupt(Translate("WHY!?"), 7);
					}
					else if (UnityEngine.Random.value < 0.24f)
					{
						dialogBox.Interrupt(Translate("Why would you try doing that!?"), 7);
					}
					else if (UnityEngine.Random.value < 0.24f)
					{
						dialogBox.Interrupt(Translate("What came over you!?"), 7);
					}
					else
					{
						dialogBox.Interrupt(Translate("WHY!? Why would you do something so dangerous!?"), 7);
					}
				}
				holdKnees = true;
			}
			dangerousSingularity = null;
			wasScaredBySingularity = false;
			return false;
		}
		wasScaredBySingularity = true;
		protest = true;
		holdKnees = false;
		oracle.bodyChunks[0].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value;
		oracle.bodyChunks[1].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value * 2f;
		protestCounter += 1f / 22f;
		lookPoint = oracle.bodyChunks[0].pos + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * Mathf.Sin(protestCounter * (float)Math.PI * 2f) * 145f;
		if (UnityEngine.Random.value < 1f / 30f)
		{
			armsProtest = !armsProtest;
		}
		return true;
	}

	public void UpdateActive(bool eu)
	{
		if (!isCurrentlyCommunicating())
		{
			pathProgression = Mathf.Min(1f, (float)pathProgression + 1f / Mathf.Lerp(40f + (float)pathProgression * 80f, Vector2.Distance(lastPos, nextPos) / 5f, 0.5f));
		}
		holdKnees = false;
		currentGetTo = Custom.Bezier(lastPos, ClampVectorInRoom(lastPos + lastPosHandle), nextPos, ClampVectorInRoom(nextPos + nextPosHandle), (float)pathProgression);
		floatyMovement = false;
		investigateAngle += invstAngSpeed;
		if (pathProgression < 1.0 || consistentBasePosCounter <= 100 || oracle.arm.baseMoving)
		{
			allStillCounter = 0;
		}
		else
		{
			allStillCounter++;
		}
		if (movementBehavior != MovementBehavior.ShowMedia)
		{
			showMediaPhaseTime = 0;
		}
		if (!forceFlightMode && (WantsToSit || (this is SLOracleBehaviorHasMark && (this as SLOracleBehaviorHasMark).dialogBox.messages.Count != 0)))
		{
			displayImageTimer = 0;
			if (hasNoticedPlayer)
			{
				setMovementBehavior(MovementBehavior.InvestigateSlugcat);
			}
			else if (movementBehavior == MovementBehavior.InvestigateSlugcat)
			{
				setMovementBehavior(MovementBehavior.Idle);
			}
		}
		if (displayImage != null)
		{
			forcedShowMediaPos = new Vector2(1460f, 310f);
			displayImage.pos = showMediaPos;
			displayImage.setAlpha = 0.91f + UnityEngine.Random.value * 0.06f;
			displayImageTimer--;
			if (displayImageTimer <= 0)
			{
				displayImage.Destroy();
				displayImage = null;
			}
		}
		Move();
	}

	public Vector2 RandomRoomPoint()
	{
		return new Vector2(1270f + UnityEngine.Random.value * 490f, 200f + UnityEngine.Random.value * 350f);
	}

	public void resetSwarmerPositions()
	{
		for (int i = 0; i < oracle.mySwarmers.Count; i++)
		{
			Vector2 newPos = OracleGetToPos + new Vector2(0f, 100f) + Custom.RNV() * UnityEngine.Random.value * 50f;
			oracle.mySwarmers[i].firstChunk.HardSetPosition(newPos);
		}
	}

	public void setMovementBehavior(MovementBehavior behavior)
	{
		if (behavior != movementBehavior)
		{
			movementBehavior = behavior;
			if (movementBehavior == MovementBehavior.KeepDistance)
			{
				keepDistTime = 0;
			}
		}
	}

	public void SetNewDestination(Vector2 dst)
	{
		lastPos = currentGetTo;
		nextPos = dst;
		lastPosHandle = Custom.RNV() * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(lastPos, nextPos);
		nextPosHandle = -GetToDir * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(lastPos, nextPos);
		pathProgression = 0.0;
	}

	private double ShowMediaScore(Vector2 tryPos)
	{
		if (oracle.room.GetTile(tryPos).Solid || player == null)
		{
			return double.MaxValue;
		}
		float num = Mathf.Abs(Vector2.Distance(tryPos, player.DangerPos) - 250f);
		num -= Mathf.Min(oracle.room.aimap.getTerrainProximity(tryPos), 9f) * 30f;
		num -= Vector2.Distance(tryPos, nextPos) * 0.5f;
		for (int i = 0; i < oracle.arm.joints.Length; i++)
		{
			num -= Mathf.Min(Vector2.Distance(tryPos, oracle.arm.joints[i].pos), 100f) * 10f;
		}
		return num;
	}

	public bool isCurrentlyCommunicating()
	{
		return voice != null;
	}

	private double BasePosScore(Vector2 tryPos)
	{
		if (movementBehavior == MovementBehavior.Meditate || player == null)
		{
			return Vector2.Distance(tryPos, oracle.room.MiddleOfTile(77, 18));
		}
		if (movementBehavior == MovementBehavior.ShowMedia)
		{
			return 0.0 - (double)Vector2.Distance(player.DangerPos, tryPos);
		}
		return Mathf.Abs(Vector2.Distance(nextPos, tryPos) - 200f) + Custom.LerpMap(Vector2.Distance(player.DangerPos, tryPos), 40f, 300f, 800f, 0f);
	}

	public Vector2 ClampMediaPos(Vector2 mediaPos)
	{
		float x = mediaPos.x;
		float y = mediaPos.y;
		x = Mathf.Max(Mathf.Min(x, 1770f), 1480f);
		y = Mathf.Max(Mathf.Min(y, 570f), 500f);
		if (x == mediaPos.x && y == mediaPos.y)
		{
			return mediaPos;
		}
		return new Vector2(x, y);
	}

	private Vector2 ClampVectorInRoom(Vector2 v)
	{
		Vector2 result = v;
		result.x = Mathf.Clamp(result.x, oracle.arm.cornerPositions[0].x + 100f, oracle.arm.cornerPositions[1].x - 100f);
		result.y = Mathf.Clamp(result.y, oracle.arm.cornerPositions[2].y + 100f, oracle.arm.cornerPositions[1].y - 100f);
		return result;
	}

	private double CommunicatePosScore(Vector2 tryPos)
	{
		if (oracle.room.GetTile(tryPos).Solid || player == null)
		{
			return double.MaxValue;
		}
		float num = Mathf.Abs(Vector2.Distance(tryPos, player.DangerPos) - ((movementBehavior != MovementBehavior.Talk) ? 400f : 250f));
		num -= (float)Custom.IntClamp(oracle.room.aimap.getTerrainProximity(tryPos), 0, 8) * 10f;
		if (movementBehavior == MovementBehavior.ShowMedia)
		{
			num += (float)(Custom.IntClamp(oracle.room.aimap.getTerrainProximity(tryPos), 8, 16) - 8) * 10f;
		}
		return num;
	}

	private void Move()
	{
		if (movementBehavior == MovementBehavior.Idle)
		{
			invstAngSpeed = 0.2;
			idleCounter -= 1.0;
			if (idleCounter <= 0.0)
			{
				if (UnityEngine.Random.value >= 0.4f || forceFlightMode)
				{
					SetNewDestination(RandomRoomPoint());
					investigateAngle = (double)UnityEngine.Random.value * 360.0 - 180.0;
					idleCounter = (double)UnityEngine.Random.value * 400.0 + 150.0;
				}
				else
				{
					idleCounter = (double)UnityEngine.Random.value * 900.0 + 450.0;
					if (UnityEngine.Random.value >= 0.3f)
					{
						setMovementBehavior(MovementBehavior.Meditate);
					}
					else
					{
						setMovementBehavior(MovementBehavior.ShowMedia);
					}
				}
			}
		}
		else if (movementBehavior == MovementBehavior.Meditate)
		{
			if (nextPos != oracle.room.MiddleOfTile(77, 18))
			{
				SetNewDestination(oracle.room.MiddleOfTile(77, 18));
			}
			investigateAngle = 0.0;
			lookPoint = oracle.firstChunk.pos + new Vector2(0f, -40f);
			idleCounter -= 1.0;
			meditateTick++;
			for (int i = 0; i < oracle.mySwarmers.Count; i++)
			{
				float num = 20f;
				float f = (float)meditateTick * 0.035f;
				num = ((i % 2 != 0) ? (num * Mathf.Cos(f)) : (num * Mathf.Sin(f)));
				float num2 = (float)i * ((float)Math.PI * 2f / (float)oracle.mySwarmers.Count);
				num2 += (float)meditateTick * 0.0035f;
				num2 %= (float)Math.PI * 2f;
				float num3 = 90f + num;
				Vector2 vector = new Vector2(Mathf.Cos(num2) * num3 + oracle.firstChunk.pos.x, (0f - Mathf.Sin(num2)) * num3 + oracle.firstChunk.pos.y);
				Vector2 newPos = new Vector2(oracle.mySwarmers[i].firstChunk.pos.x + (vector.x - oracle.mySwarmers[i].firstChunk.pos.x) * 0.05f, oracle.mySwarmers[i].firstChunk.pos.y + (vector.y - oracle.mySwarmers[i].firstChunk.pos.y) * 0.05f);
				oracle.mySwarmers[i].firstChunk.HardSetPosition(newPos);
				oracle.mySwarmers[i].firstChunk.vel = Vector2.zero;
				if (oracle.mySwarmers[i].ping <= 0)
				{
					oracle.mySwarmers[i].rotation = 0f;
					oracle.mySwarmers[i].revolveSpeed = 0f;
					if (meditateTick > 120 && (double)UnityEngine.Random.value <= 0.0015)
					{
						oracle.mySwarmers[i].ping = 40;
						oracle.room.AddObject(new Explosion.ExplosionLight(oracle.mySwarmers[i].firstChunk.pos, 500f + UnityEngine.Random.value * 400f, 1f, 10, Color.cyan));
						oracle.room.AddObject(new ElectricDeath.SparkFlash(oracle.mySwarmers[i].firstChunk.pos, 0.75f + UnityEngine.Random.value));
						if (player != null && player.room == oracle.room && player.mainBodyChunk.pos.x > 1000f)
						{
							oracle.room.PlaySound(SoundID.HUD_Exit_Game, player.mainBodyChunk.pos, 1f, 2f + (float)i / (float)oracle.mySwarmers.Count * 2f);
						}
					}
				}
				else
				{
					oracle.mySwarmers[i].ping--;
				}
			}
			if (idleCounter <= 0.0 || protest)
			{
				SetNewDestination(RandomRoomPoint());
				investigateAngle = (double)UnityEngine.Random.value * 360.0 - 180.0;
				idleCounter = (double)UnityEngine.Random.value * 400.0 + 150.0;
				if (hasNoticedPlayer)
				{
					setMovementBehavior(MovementBehavior.InvestigateSlugcat);
				}
				else
				{
					setMovementBehavior(MovementBehavior.Idle);
				}
			}
		}
		else if (movementBehavior == MovementBehavior.KeepDistance)
		{
			if (player == null)
			{
				setMovementBehavior(MovementBehavior.Idle);
			}
			else
			{
				lookPoint = player.DangerPos;
				Vector2 vector2 = RandomRoomPoint();
				if (!oracle.room.GetTile(vector2).Solid && oracle.room.aimap.getTerrainProximity(vector2) > 2 && Vector2.Distance(vector2, player.DangerPos) > Vector2.Distance(nextPos, player.DangerPos) + 100f)
				{
					SetNewDestination(vector2);
				}
				keepDistTime++;
			}
		}
		else if (movementBehavior == MovementBehavior.Investigate)
		{
			if (player == null)
			{
				setMovementBehavior(MovementBehavior.Idle);
			}
			else
			{
				lookPoint = player.DangerPos;
				if (investigateAngle < -90.0 || investigateAngle > 90.0 || (float)oracle.room.aimap.getTerrainProximity(nextPos) < 2f)
				{
					investigateAngle = Mathf.Lerp(-70f, 70f, UnityEngine.Random.value);
					invstAngSpeed = Mathf.Lerp(0.1f, 0.3f, UnityEngine.Random.value) * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f));
				}
				Vector2 vector3 = player.DangerPos + Custom.DegToVec((float)investigateAngle) * 150f;
				if (!((float)oracle.room.aimap.getTerrainProximity(vector3) < 2f))
				{
					if (pathProgression > 0.899999976158142)
					{
						if (Custom.DistLess(oracle.firstChunk.pos, vector3, 30f))
						{
							floatyMovement = true;
						}
						else if (!Custom.DistLess(nextPos, vector3, 30f))
						{
							SetNewDestination(vector3);
						}
					}
					nextPos = vector3;
				}
			}
		}
		else if (movementBehavior == MovementBehavior.Talk)
		{
			if (player == null)
			{
				setMovementBehavior(MovementBehavior.Idle);
			}
			else
			{
				lookPoint = player.DangerPos;
				Vector2 vector4 = RandomRoomPoint();
				if (!(CommunicatePosScore(vector4) + 40.0 >= CommunicatePosScore(nextPos)) && !Custom.DistLess(vector4, nextPos, 30f))
				{
					SetNewDestination(vector4);
				}
			}
		}
		else if (movementBehavior == MovementBehavior.ShowMedia)
		{
			showMediaPhaseTime++;
			investigateAngle = 0.0;
			lookPoint = oracle.firstChunk.pos + new Vector2(0f, 40f);
			if (showMediaPhaseTime < 10 && displayImage != null)
			{
				displayImage.Destroy();
				displayImage = null;
			}
			if (showMediaPhaseTime == 1)
			{
				SetNewDestination(RandomRoomPoint());
			}
			if (showMediaPhaseTime == 10 && !forceFlightMode)
			{
				if (UnityEngine.Random.value <= 0.75f)
				{
					displayImage = oracle.myScreen.AddImage("AIimg1_RIVEND");
				}
				else
				{
					displayImage = oracle.myScreen.AddImage("AIimg1_RIVEND2");
				}
				displayImageTimer = UnityEngine.Random.Range(350, 450);
			}
			if (showMediaPhaseTime > 10)
			{
				if (showMediaPhaseTime % 120 == 0)
				{
					SetNewDestination(RandomRoomPoint());
					investigateAngle = (double)UnityEngine.Random.value * 360.0 - 180.0;
				}
				if (displayImage == null)
				{
					if (hasNoticedPlayer)
					{
						setMovementBehavior(MovementBehavior.InvestigateSlugcat);
					}
					else
					{
						setMovementBehavior(MovementBehavior.Idle);
					}
				}
			}
		}
		else if (movementBehavior == MovementBehavior.InvestigateSlugcat)
		{
			if (player == null)
			{
				setMovementBehavior(MovementBehavior.Idle);
			}
			else
			{
				invstAngSpeed = 0.2;
				Vector2 vector5 = (lookPoint = ((player.mainBodyChunk.pos.x <= oracle.room.PixelWidth * 0.85f) ? new Vector2(player.mainBodyChunk.pos.x + 100f, player.mainBodyChunk.pos.y + 150f) : new Vector2(player.mainBodyChunk.pos.x - 100f, player.mainBodyChunk.pos.y + 150f)));
				if (Custom.DistLess(nextPos, vector5, 100f))
				{
					floatyMovement = true;
					nextPos = vector5 - Custom.DegToVec((float)investigateAngle) * 50f;
				}
				else
				{
					SetNewDestination(vector5 - Custom.DegToVec((float)investigateAngle) * 50f);
				}
				idleCounter -= 1.0;
				if (idleCounter <= 0.0)
				{
					idleCounter = (double)UnityEngine.Random.value * 400.0 + 150.0;
					if (!forceFlightMode)
					{
						if (UnityEngine.Random.value >= 0.3f)
						{
							setMovementBehavior(MovementBehavior.Meditate);
						}
						else
						{
							setMovementBehavior(MovementBehavior.ShowMedia);
						}
					}
				}
			}
		}
		Vector2 tryPos = RandomRoomPoint();
		if (!forcedShowMediaPos.HasValue)
		{
			if (ShowMediaScore(tryPos) + 40.0 < ShowMediaScore(idealShowMediaPos))
			{
				idealShowMediaPos = tryPos;
				consistentShowMediaPosCounter = 0;
			}
			tryPos = idealShowMediaPos + Custom.RNV() * UnityEngine.Random.value * 40f;
			if (ShowMediaScore(tryPos) + 20.0 < ShowMediaScore(idealShowMediaPos))
			{
				idealShowMediaPos = tryPos;
				consistentShowMediaPosCounter = 0;
			}
		}
		else
		{
			idealShowMediaPos = forcedShowMediaPos.Value;
			if (Vector2.Distance(showMediaPos, forcedShowMediaPos.Value) < 10f)
			{
				consistentShowMediaPosCounter = 0;
			}
			else
			{
				consistentShowMediaPosCounter += 10;
			}
		}
		if (consistentShowMediaPosCounter > 300)
		{
			showMediaPos = Vector2.Lerp(showMediaPos, idealShowMediaPos, 0.1f);
			showMediaPos = Custom.MoveTowards(showMediaPos, idealShowMediaPos, 10f);
		}
		consistentShowMediaPosCounter += (int)Custom.LerpMap(Vector2.Distance(showMediaPos, idealShowMediaPos), 0f, 200f, 1f, 10f);
		consistentBasePosCounter++;
		if (!oracle.room.readyForAI)
		{
			baseIdeal = nextPos;
			return;
		}
		Vector2 vector6 = new Vector2(UnityEngine.Random.value * oracle.room.PixelWidth, UnityEngine.Random.value * oracle.room.PixelHeight);
		if (!oracle.room.GetTile(vector6).Solid && !(BasePosScore(vector6) + 40.0 >= BasePosScore(baseIdeal)))
		{
			baseIdeal = vector6;
			consistentBasePosCounter = 0;
		}
	}
}
