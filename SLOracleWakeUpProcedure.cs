using System;
using RWCustom;
using UnityEngine;

public class SLOracleWakeUpProcedure : UpdatableAndDeletable
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase LookingForSwarmer = new Phase("LookingForSwarmer", register: true);

		public static readonly Phase WaitingForSwarmerRelease = new Phase("WaitingForSwarmerRelease", register: true);

		public static readonly Phase GoToRoom = new Phase("GoToRoom", register: true);

		public static readonly Phase GoToAboveOracle = new Phase("GoToAboveOracle", register: true);

		public static readonly Phase GoToOracle = new Phase("GoToOracle", register: true);

		public static readonly Phase Rumble = new Phase("Rumble", register: true);

		public static readonly Phase Booting = new Phase("Booting", register: true);

		public static readonly Phase SwarmersEnter = new Phase("SwarmersEnter", register: true);

		public static readonly Phase GetMark = new Phase("GetMark", register: true);

		public static readonly Phase MoonWakesUp = new Phase("MoonWakesUp", register: true);

		public static readonly Phase Done = new Phase("Done", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class BootLabel : CosmeticSprite
	{
		private int counter;

		private float fade;

		private float lastFade;

		private float flicker;

		public bool red;

		public bool kill;

		private bool PunctuationVisible
		{
			get
			{
				if (counter % 40 < 20)
				{
					return !red;
				}
				return false;
			}
		}

		public BootLabel(Room room, Vector2 pos)
		{
			base.room = room;
			base.pos = pos;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			counter++;
			lastFade = fade;
			flicker = Mathf.Max(0f, flicker - 1f / 14f);
			if (kill)
			{
				fade = Mathf.Max(0f, fade - 0.125f);
				if ((fade <= 0f) & (lastFade <= 0f))
				{
					Destroy();
				}
				return;
			}
			if (red)
			{
				fade = 0.7f * Mathf.Lerp(0.95f - 0.7f * flicker * UnityEngine.Random.value, 1f, UnityEngine.Random.value);
				if (UnityEngine.Random.value < 1f / 30f)
				{
					flicker = Mathf.Pow(UnityEngine.Random.value, 0.5f);
				}
				else
				{
					flicker = Mathf.Max(0.5f, flicker);
				}
				return;
			}
			if (counter % 40 == 0)
			{
				room.PlaySound(SoundID.SS_AI_Text_Blink, 0f, 1f, 1f);
			}
			fade = 0.5f * Mathf.Lerp(0.95f - 0.4f * flicker * UnityEngine.Random.value, 1f, UnityEngine.Random.value);
			if (UnityEngine.Random.value < 1f / 60f)
			{
				flicker = Mathf.Pow(UnityEngine.Random.value, 0.5f);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("bootLabel1");
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["LightSource"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].x = pos.x - camPos.x;
				sLeaser.sprites[i].y = pos.y - camPos.y;
			}
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(PunctuationVisible ? "bootLabel1" : "bootLabel2");
			sLeaser.sprites[0].alpha = 0.5f * fade;
			sLeaser.sprites[0].color = (red ? Color.red : new Color(0f, 76f / 85f, 1f));
			sLeaser.sprites[1].scaleX = (PunctuationVisible ? 450f : 430f) / 16f;
			sLeaser.sprites[1].scaleY = (PunctuationVisible ? 250f : 240f) / 16f;
			if (PunctuationVisible)
			{
				sLeaser.sprites[1].x -= 3f;
			}
			sLeaser.sprites[1].color = sLeaser.sprites[0].color;
			sLeaser.sprites[1].alpha = 0.5f * fade;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Foreground");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public Oracle SLOracle;

	public NSHSwarmer resqueSwarmer;

	public int inPhaseCounter;

	public int customCounter;

	public int totCounter;

	public Phase phase;

	private BootLabel bootLabel;

	public SLOracleWakeUpProcedure(Oracle SLOracle)
	{
		this.SLOracle = SLOracle;
		Custom.Log("wake up procedure");
		phase = Phase.LookingForSwarmer;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		inPhaseCounter++;
		totCounter++;
		if (resqueSwarmer != null && (int)phase > 1 && (int)phase < 4 && resqueSwarmer.grabbedBy.Count > 0)
		{
			Custom.Log("swarmer grabbed again");
			phase = Phase.WaitingForSwarmerRelease;
		}
		if (phase == Phase.LookingForSwarmer)
		{
			if (resqueSwarmer != null)
			{
				NextPhase();
				return;
			}
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is NSHSwarmer)
				{
					resqueSwarmer = room.updateList[i] as NSHSwarmer;
					break;
				}
			}
		}
		else if (phase == Phase.WaitingForSwarmerRelease)
		{
			if (resqueSwarmer.room != room)
			{
				return;
			}
			if (resqueSwarmer.grabbedBy.Count == 0 && resqueSwarmer.firstChunk.pos.y < 400f)
			{
				NextPhase();
			}
			else if (resqueSwarmer.grabbedBy.Count > 0 && resqueSwarmer.firstChunk.pos.x > 1120f && UnityEngine.Random.value < 1f / 30f)
			{
				for (int num = resqueSwarmer.grabbedBy.Count - 1; num >= 0; num--)
				{
					resqueSwarmer.grabbedBy[num].Release();
				}
				resqueSwarmer.firstChunk.vel.y = 7f;
				for (int j = 0; j < 7; j++)
				{
					room.AddObject(new Spark(resqueSwarmer.firstChunk.pos, Custom.RNV() * Mathf.Lerp(4f, 16f, UnityEngine.Random.value), resqueSwarmer.myColor, null, 9, 40));
				}
				NextPhase();
			}
		}
		else if (phase == Phase.GoToRoom)
		{
			resqueSwarmer.storyFly = true;
			Vector2 vector = new Vector2(1511f, 448f);
			vector += Custom.DegToVec((float)totCounter * 6.2f) * 100f;
			resqueSwarmer.storyFlyTarget = vector;
			if (Custom.DistLess(resqueSwarmer.firstChunk.pos, vector, 300f) && room.ViewedByAnyCamera(resqueSwarmer.firstChunk.pos, 0f))
			{
				customCounter++;
				if (customCounter > 90)
				{
					NextPhase();
				}
			}
		}
		else if (phase == Phase.GoToAboveOracle)
		{
			resqueSwarmer.storyFly = true;
			Vector2 vector = Vector2.Lerp(new Vector2(1511f, 448f), SLOracle.firstChunk.pos, Custom.LerpMap(inPhaseCounter, 0f, 80f, 0f, 0.8f, 2f));
			vector += Custom.DegToVec((float)totCounter * 6.2f) * Custom.LerpMap(inPhaseCounter, 0f, 80f, 100f, 0f);
			resqueSwarmer.storyFlyTarget = vector;
			if (inPhaseCounter > 80)
			{
				room.game.manager.CueAchievement(RainWorld.AchievementID.HunterPayload, 5f);
				NextPhase();
			}
		}
		else if (phase == Phase.GoToOracle)
		{
			if (room.world.rainCycle.TimeUntilRain < 4800)
			{
				room.world.rainCycle.pause = 4800;
			}
			resqueSwarmer.storyFly = true;
			resqueSwarmer.storyFlyTarget = SLOracle.firstChunk.pos;
			if (Custom.DistLess(resqueSwarmer.firstChunk.pos, SLOracle.firstChunk.pos, 30f))
			{
				if (room.game.IsStorySession)
				{
					room.game.GetStorySession.saveState.miscWorldSaveData.moonRevived = true;
				}
				resqueSwarmer.firstChunk.pos = SLOracle.firstChunk.pos;
				SLOracle.firstChunk.vel += new Vector2(0f, 2f);
				SLOracleSwarmer sLOracleSwarmer = new SLOracleSwarmer(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, null, room.GetWorldCoordinate(resqueSwarmer.firstChunk.pos), room.game.GetNewID()), room.world);
				sLOracleSwarmer.affectedByGravity = 0f;
				room.abstractRoom.entities.Add(sLOracleSwarmer.abstractPhysicalObject);
				sLOracleSwarmer.firstChunk.HardSetPosition(resqueSwarmer.firstChunk.pos);
				room.AddObject(sLOracleSwarmer);
				SLOracle.mySwarmers.Add(sLOracleSwarmer);
				if (ModManager.MMF)
				{
					(room.game.session as StoryGameSession).RemovePersistentTracker(resqueSwarmer.abstractPhysicalObject);
				}
				resqueSwarmer.Destroy();
				room.PlaySound(SoundID.Moon_Wake_Up_Green_Swarmer_Flash, resqueSwarmer.firstChunk.pos, 1f, 1f);
				room.AddObject(new ElectricDeath.SparkFlash(resqueSwarmer.firstChunk.pos, 300f));
				NextPhase();
				resqueSwarmer = null;
			}
		}
		else if (phase == Phase.Rumble)
		{
			room.ScreenMovement(SLOracle.firstChunk.pos, new Vector2(0f, 0f), Mathf.Min(Custom.LerpMap(inPhaseCounter, 140f, 420f, 0f, 1.5f, 1.2f), Custom.LerpMap(inPhaseCounter, 560f, 620f, 1.5f, 0f)));
			float num2 = Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(400f, 600f, inPhaseCounter) * (float)Math.PI));
			float num3 = Mathf.Sin(Mathf.InverseLerp(300f, 620f, inPhaseCounter) * (float)Math.PI);
			room.gravity = 1f - 0.8f * num2;
			if (num2 > 0f)
			{
				for (int k = 0; k < room.physicalObjects.Length; k++)
				{
					for (int l = 0; l < room.physicalObjects[k].Count; l++)
					{
						for (int m = 0; m < room.physicalObjects[k][l].bodyChunks.Length; m++)
						{
							room.physicalObjects[k][l].bodyChunks[m].vel.y += Mathf.Pow(num2, 2f) * 0.4f * UnityEngine.Random.value * Mathf.InverseLerp(500f, 270f, room.physicalObjects[k][l].bodyChunks[m].pos.y);
						}
					}
				}
				room.waterObject.GeneralUpsetSurface(Mathf.Pow(num2, 0.5f) * 3f);
				if (UnityEngine.Random.value < num2)
				{
					for (int n = 0; n < room.game.Players.Count; n++)
					{
						if (room.game.Players[n].realizedCreature != null && room.game.Players[n].realizedCreature.room == room && (room.game.Players[n].realizedCreature as Player).bodyChunks[1].ContactPoint.y < 0)
						{
							(room.game.Players[n].realizedCreature as Player).standing = false;
							(room.game.Players[n].realizedCreature as Player).Stun(12);
							(room.game.Players[n].realizedCreature as Player).bodyChunks[1].vel.y = 3f;
						}
					}
				}
			}
			if (num3 > 0f)
			{
				float num4 = Mathf.Lerp(400f, 30f, num3);
				if (!Custom.DistLess(SLOracle.bodyChunks[1].pos, new Vector2(1649f, 323f), num4))
				{
					Vector2 vector2 = Custom.DirVec(SLOracle.bodyChunks[1].pos, new Vector2(1649f, 323f)) * (Vector2.Distance(SLOracle.bodyChunks[1].pos, new Vector2(1649f, 323f)) - num4);
					SLOracle.bodyChunks[1].pos += vector2 * 0.1f;
					SLOracle.bodyChunks[1].vel += vector2 * 0.1f;
				}
			}
			if (inPhaseCounter == 300)
			{
				room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 1f);
			}
			if (inPhaseCounter == 560)
			{
				room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, 1f, 1f);
			}
			if ((SLOracle.oracleBehavior as SLOracleBehavior).fuses != null)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).fuses.power = Mathf.InverseLerp(150f, 400f, inPhaseCounter);
				(SLOracle.oracleBehavior as SLOracleBehavior).fuses.powerFlicker = (SLOracle.oracleBehavior as SLOracleBehavior).fuses.power;
			}
			if (inPhaseCounter > 620)
			{
				room.gravity = 1f;
				NextPhase();
			}
		}
		else if (phase == Phase.Booting)
		{
			if ((SLOracle.oracleBehavior as SLOracleBehavior).fuses != null)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).fuses.power = Mathf.InverseLerp(90f, 10f, inPhaseCounter);
				(SLOracle.oracleBehavior as SLOracleBehavior).fuses.powerFlicker = (SLOracle.oracleBehavior as SLOracleBehavior).fuses.power;
			}
			if (inPhaseCounter > 90)
			{
				if (bootLabel == null)
				{
					Futile.atlasManager.LoadAtlasFromTexture("bootLabel1", Resources.Load("Atlases/bootLabel1") as Texture2D, textureFromAsset: true);
					Futile.atlasManager.LoadAtlasFromTexture("bootLabel2", Resources.Load("Atlases/bootLabel2") as Texture2D, textureFromAsset: true);
					bootLabel = new BootLabel(room, new Vector2(1653f, 450f));
					room.AddObject(bootLabel);
					room.PlaySound(SoundID.SS_AI_Text_Blink, 0f, 1f, 1f);
				}
				if (inPhaseCounter == 182)
				{
					bootLabel.red = true;
				}
				else if (inPhaseCounter == 224)
				{
					bootLabel.kill = true;
				}
				else if (inPhaseCounter > 238)
				{
					room.ScreenMovement(SLOracle.firstChunk.pos, new Vector2(0f, 0f), 0.3f);
					NextPhase();
					bootLabel = null;
				}
			}
		}
		else if (phase == Phase.SwarmersEnter)
		{
			for (int num5 = 0; num5 < SLOracle.mySwarmers.Count; num5++)
			{
				if (UnityEngine.Random.value < 1f / 30f && UnityEngine.Random.value < (SLOracle.mySwarmers[num5] as SLOracleSwarmer).blackMode && UnityEngine.Random.value < Mathf.InverseLerp(230f, 70f, inPhaseCounter))
				{
					room.AddObject(new WaterDrip(SLOracle.mySwarmers[num5].firstChunk.pos, SLOracle.mySwarmers[num5].firstChunk.vel, waterColor: true));
				}
			}
			if (inPhaseCounter == 20)
			{
				SwarmerEnterRoom(new IntVector2(87, 2));
			}
			else if (inPhaseCounter == 32)
			{
				SwarmerEnterRoom(new IntVector2(88, 2));
			}
			else if (inPhaseCounter == 63)
			{
				SwarmerEnterRoom(new IntVector2(76, 32));
			}
			else if (inPhaseCounter == 76)
			{
				SwarmerEnterRoom(new IntVector2(71, 3));
			}
			if (inPhaseCounter <= 120)
			{
				return;
			}
			bool flag = false;
			for (int num6 = 0; num6 < SLOracle.mySwarmers.Count; num6++)
			{
				if ((SLOracle.mySwarmers[num6] as SLOracleSwarmer).blackMode == 1f && (UnityEngine.Random.value < 0.02f || inPhaseCounter > 300))
				{
					(SLOracle.mySwarmers[num6] as SLOracleSwarmer).blackMode -= 0.0011111111f;
				}
				if ((SLOracle.mySwarmers[num6] as SLOracleSwarmer).blackMode > 0f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				NextPhase();
			}
		}
		else if (phase == Phase.GetMark)
		{
			if (inPhaseCounter == 6)
			{
				PingSwarmer(0);
			}
			else if (inPhaseCounter == 47)
			{
				PingSwarmer(1);
			}
			else if (inPhaseCounter == 56)
			{
				PingSwarmer(2);
			}
			else if (inPhaseCounter == 71)
			{
				PingSwarmer(3);
			}
			else if (inPhaseCounter == 89)
			{
				PingSwarmer(4);
			}
			else if (inPhaseCounter > 170)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).State.neuronsLeft = SLOracle.mySwarmers.Count;
				(SLOracle.oracleBehavior as SLOracleBehavior).State.likesPlayer = 0.6f;
				(SLOracle.oracleBehavior as SLOracleBehavior).State.playerEncounters = 0;
				(SLOracle.oracleBehavior as SLOracleBehavior).State.playerEncountersWithMark = 0;
				(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).stillWakingUp = true;
				(SLOracle.oracleBehavior as SLOracleBehavior).holdKnees = false;
				NextPhase();
			}
		}
		else if (phase == Phase.MoonWakesUp)
		{
			SLOracle.health = Mathf.InverseLerp(80f, 200f, inPhaseCounter) * Mathf.InverseLerp(0f, 5f, (SLOracle.oracleBehavior as SLOracleBehavior).State.neuronsLeft);
			SLOracle.firstChunk.vel += Custom.RNV() * 2f * UnityEngine.Random.value * Mathf.Sin(Mathf.InverseLerp(0f, 220f, inPhaseCounter) * (float)Math.PI);
			SLOracle.bodyChunks[1].vel += Custom.RNV() * 2f * UnityEngine.Random.value * Mathf.Sin(Mathf.InverseLerp(0f, 220f, inPhaseCounter) * (float)Math.PI);
			if (inPhaseCounter > 280)
			{
				(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).stillWakingUp = false;
				NextPhase();
			}
		}
		else
		{
			if (!(phase == Phase.Done))
			{
				return;
			}
			if (room.game.manager.musicPlayer != null)
			{
				MusicEvent musicEvent = null;
				for (int num7 = 0; num7 < room.roomSettings.triggers.Count; num7++)
				{
					if (room.roomSettings.triggers[num7].tEvent is MusicEvent)
					{
						musicEvent = room.roomSettings.triggers[num7].tEvent as MusicEvent;
						break;
					}
				}
				if (musicEvent != null)
				{
					room.game.manager.musicPlayer.GameRequestsSong(musicEvent);
				}
			}
			Destroy();
		}
	}

	private void SwarmerEnterRoom(IntVector2 tilePos)
	{
		SLOracleSwarmer sLOracleSwarmer = new SLOracleSwarmer(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, null, room.GetWorldCoordinate(tilePos), room.game.GetNewID()), room.world);
		sLOracleSwarmer.affectedByGravity = 0f;
		room.abstractRoom.entities.Add(sLOracleSwarmer.abstractPhysicalObject);
		sLOracleSwarmer.firstChunk.HardSetPosition(room.MiddleOfTile(tilePos) + new Vector2(0f, -5f));
		room.AddObject(sLOracleSwarmer);
		SLOracle.mySwarmers.Add(sLOracleSwarmer);
		sLOracleSwarmer.firstChunk.vel.y = 3f;
		sLOracleSwarmer.direction = ((UnityEngine.Random.value < 0.5f) ? new Vector2(-1f, 0f) : new Vector2(1f, 0f));
		sLOracleSwarmer.lastDirection = sLOracleSwarmer.direction;
		sLOracleSwarmer.lazyDirection = sLOracleSwarmer.direction;
		sLOracleSwarmer.lastLazyDirection = sLOracleSwarmer.direction;
		sLOracleSwarmer.rotation = UnityEngine.Random.value * 360f;
		sLOracleSwarmer.blackMode = 1f;
	}

	private void PingSwarmer(int swrmrIndex)
	{
		if (swrmrIndex >= 0 && swrmrIndex < SLOracle.mySwarmers.Count)
		{
			for (int i = 0; i < 20; i++)
			{
				room.AddObject(new Spark(SLOracle.mySwarmers[swrmrIndex].firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
			}
			SLOracle.mySwarmers[swrmrIndex].firstChunk.vel.y -= 4f;
			room.PlaySound(SoundID.Moon_Wake_Up_Swarmer_Ping, 0f, 1f, 1f);
		}
	}

	private void NextPhase()
	{
		phase = new Phase(ExtEnum<Phase>.values.GetEntry(phase.Index + 1));
		if (phase == Phase.Rumble)
		{
			room.PlaySound(SoundID.Moon_Wake_Up_Rumble, 0f, 1f, 1f);
		}
		Custom.Log($"phase: {phase}");
		inPhaseCounter = 0;
		customCounter = 0;
	}
}
