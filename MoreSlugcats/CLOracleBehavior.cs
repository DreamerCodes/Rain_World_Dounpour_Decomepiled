using System;
using System.Collections.Generic;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CLOracleBehavior : OracleBehavior
{
	public class CLConversation : Conversation
	{
		public SSOracleRotBehavior owner;

		public CLConversation(SSOracleRotBehavior owner, ID id, DialogBox dialogBox)
			: base(owner, id, dialogBox)
		{
			this.owner = owner;
			AddEvents();
		}

		protected override void AddEvents()
		{
			if (id == MoreSlugcatsEnums.ConversationID.Pebbles_Pearl_RM)
			{
				LoadEventsFromFile(120);
			}
		}
	}

	public bool hasNoticedPlayer;

	private float crawlCounter;

	public bool holdKnees;

	public bool protest;

	public float protestCounter;

	public bool armsProtest;

	protected bool conversationAdded;

	public List<SoundID> painLines;

	public int dontHoldKnees;

	private Vector2 nextPos;

	public bool floatyMovement;

	private Vector2 currentGetTo;

	private double idleCounter;

	private Vector2 lastPos;

	private Vector2 lastPosHandle;

	private Vector2 nextPosHandle;

	public int dehabilitateTime;

	public bool initiated;

	public int timeOutOfSitZone;

	private HalcyonPearl halcyon;

	private bool seenHalcyonOnce;

	public int conversationCooldown;

	public Conversation currentConversation;

	private int helloDelay;

	private bool saidHello;

	private bool rainInterrupt;

	private bool deadTalk;

	public int noConversationTime;

	public float Crawl => Mathf.Cos(crawlCounter * (float)Math.PI * 2f);

	public float CrawlSpeed => 0.5f + 0.5f * Mathf.Sin(crawlCounter * (float)Math.PI * 2f);

	public override Vector2 OracleGetToPos => new Vector2(2565f, 145f);

	public override Vector2 GetToDir
	{
		get
		{
			if (InSitPosition)
			{
				return new Vector2(0f, 1f);
			}
			return Custom.DirVec(OracleGetToPos, oracle.firstChunk.pos);
		}
	}

	public override bool EyesClosed
	{
		get
		{
			if (oracle.health != 0f)
			{
				if (!hasNoticedPlayer)
				{
					return InSitPosition;
				}
				return false;
			}
			return true;
		}
	}

	public bool InSitPosition => oracle.room.GetTilePosition(oracle.firstChunk.pos).x == 128;

	public bool FocusedOnHalcyon
	{
		get
		{
			if (halcyon != null && oracle.Consious && !halcyon.Carried && InSitPosition && (dialogBox == null || dialogBox.messages.Count == 0) && currentConversation == null)
			{
				return Custom.DistLess(halcyon.firstChunk.pos, oracle.firstChunk.pos, 90f);
			}
			return false;
		}
	}

	public override DialogBox dialogBox
	{
		get
		{
			if (currentConversation != null)
			{
				return currentConversation.dialogBox;
			}
			if (oracle.room.game.cameras[0].hud.dialogBox == null)
			{
				oracle.room.game.cameras[0].hud.InitDialogBox();
			}
			return oracle.room.game.cameras[0].hud.dialogBox;
		}
	}

	public CLOracleBehavior(Oracle oracle)
		: base(oracle)
	{
		holdKnees = true;
		painLines = new List<SoundID>();
		painLines.Add(SoundID.SS_AI_Talk_1);
		painLines.Add(SoundID.SS_AI_Talk_4);
		oracle.health = (base.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles ? 0f : 1f);
		if (base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0 && UnityEngine.Random.value < 0.3f)
		{
			base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts--;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!initiated)
		{
			if (oracle.myScreen == null)
			{
				oracle.myScreen = new OracleProjectionScreen(oracle.room, this);
			}
			initiated = true;
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
		if (FocusedOnHalcyon)
		{
			halcyon.hoverPos = oracle.firstChunk.pos + new Vector2(-40f, 5f);
			lookPoint = halcyon.firstChunk.pos;
		}
		else if (halcyon != null)
		{
			halcyon.hoverPos = null;
			if (halcyon.room == null)
			{
				halcyon = null;
				Custom.Log("halcyon pearl room nulled");
			}
		}
		if (!oracle.Consious)
		{
			return;
		}
		if (halcyon == null)
		{
			for (int j = 0; j < oracle.room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < oracle.room.physicalObjects[j].Count; k++)
				{
					if (oracle.room.physicalObjects[j][k] is HalcyonPearl)
					{
						halcyon = oracle.room.physicalObjects[j][k] as HalcyonPearl;
						break;
					}
				}
				if (halcyon != null)
				{
					break;
				}
			}
		}
		if (halcyon != null && halcyon.room == oracle.room)
		{
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen = false;
		}
		else if (player != null && (halcyon == null || halcyon.room != oracle.room) && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen)
		{
			if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.Interrupt(Translate("...No, don't... take!"), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.Interrupt(Translate("...No..."), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.Interrupt(Translate("...Give... back..."), 60);
			}
			else
			{
				dialogBox.Interrupt(Translate("...All I... have..."), 60);
			}
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen = true;
		}
		if (!FocusedOnHalcyon && player != null && hasNoticedPlayer)
		{
			lookPoint = player.DangerPos;
		}
		if (player != null && player.mainBodyChunk.pos.x >= 1430f && player.mainBodyChunk.pos.x <= 1660f && oracle.firstChunk.pos.x > player.mainBodyChunk.pos.x)
		{
			timeOutOfSitZone = 0;
		}
		else
		{
			timeOutOfSitZone++;
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
			if (player != null && player.room == oracle.room && player.mainBodyChunk.pos.x > 2248f)
			{
				hasNoticedPlayer = true;
				oracle.firstChunk.vel += Custom.DegToVec(45f) * 3f;
				oracle.bodyChunks[1].vel += Custom.DegToVec(-90f) * 2f;
			}
		}
		else if (!saidHello)
		{
			if (helloDelay < 40)
			{
				InitateConversation();
				saidHello = true;
			}
			else
			{
				helloDelay++;
			}
		}
		else if (hasNoticedPlayer && !rainInterrupt && player.room == oracle.room && oracle.room.world.rainCycle.TimeUntilRain < 1600 && oracle.room.world.rainCycle.pause < 1 && saidHello && noConversationTime >= 80 && player != null && !player.dead)
		{
			InterruptRain();
			rainInterrupt = true;
			if (currentConversation != null)
			{
				currentConversation.Destroy();
			}
		}
		else if (hasNoticedPlayer && player != null && saidHello)
		{
			lookPoint = player.firstChunk.pos;
			if (player.dead)
			{
				TalkToDeadPlayer();
			}
		}
		if (currentConversation == null && (dialogBox == null || dialogBox.messages.Count == 0))
		{
			noConversationTime++;
		}
		else
		{
			noConversationTime = 0;
		}
		UpdateNormal(eu);
		oracle.arm.isActive = false;
	}

	public virtual void Pain()
	{
		if ((painLines.Count > 0 && UnityEngine.Random.value < 1f / 3f) || painLines.Count >= 2)
		{
			AirVoice(painLines[0]);
			painLines.RemoveAt(0);
		}
		if (oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts == 0)
		{
			dialogBox.Interrupt(Translate("..."), 5);
		}
		oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts++;
		dehabilitateTime = 900;
		oracle.stun = 0;
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

	private Vector2 ClampVectorInRoom(Vector2 v)
	{
		Vector2 result = v;
		result.x = Mathf.Clamp(result.x, oracle.arm.cornerPositions[0].x + 100f, oracle.arm.cornerPositions[1].x - 100f);
		result.y = Mathf.Clamp(result.y, oracle.arm.cornerPositions[2].y + 100f, oracle.arm.cornerPositions[1].y - 100f);
		return result;
	}

	public Vector2 ClampMediaPos(Vector2 mediaPos)
	{
		float x = mediaPos.x;
		float y = mediaPos.y;
		x = Math.Max(Math.Min(x, 1770f), 1480f);
		y = Math.Max(Math.Min(y, 570f), 500f);
		if (x == mediaPos.x && y == mediaPos.y)
		{
			return mediaPos;
		}
		return new Vector2(x, y);
	}

	public bool isCurrentlyCommunicating()
	{
		return voice != null;
	}

	public Vector2 RandomRoomPoint()
	{
		return new Vector2(1270f + UnityEngine.Random.value * 490f, 200f + UnityEngine.Random.value * 350f);
	}

	public void UpdateNormal(bool eu)
	{
		if (InSitPosition)
		{
			holdKnees = true;
			if (FocusedOnHalcyon)
			{
				holdKnees = false;
			}
		}
		else
		{
			holdKnees = false;
			BodyChunk firstChunk = oracle.firstChunk;
			firstChunk.vel.x = firstChunk.vel.x + ((oracle.firstChunk.pos.x >= OracleGetToPos.x) ? (-1f) : 1f) * 0.6f * CrawlSpeed;
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
	}

	private void InitateConversation()
	{
		dialogBox.NewMessage(Translate("..."), 200);
		if (oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen)
		{
			if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...Why... take all I... have..."), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...Give it... back..."), 60);
			}
			else
			{
				dialogBox.NewMessage(Translate("...Bring it back..."), 60);
			}
		}
		else if (oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0)
		{
			if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...Go away..."), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...Not forgotten pain..."), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...So little... left. Why hurt... me more..."), 60);
			}
			else
			{
				dialogBox.NewMessage(Translate("...Leave me... alone."), 60);
			}
		}
		else if (oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
		{
			dialogBox.NewMessage(Translate("...Little... green thing..."), 60);
			dialogBox.NewMessage(Translate("...Hello..."), 60);
			dialogBox.NewMessage(Translate("...Nothing here... Nothing... left..."), 60);
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
		}
		else if (oracle.room.world.rainCycle.TimeUntilRain < 1600)
		{
			rainInterrupt = true;
			if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...You should... find heat."), 60);
			}
			else if (UnityEngine.Random.value < 0.15f)
			{
				dialogBox.NewMessage(Translate("...Go find... shelter."), 60);
			}
			else
			{
				dialogBox.NewMessage(Translate("...Friend... please find... safety."), 60);
			}
		}
		else if (UnityEngine.Random.value < 0.15f)
		{
			dialogBox.NewMessage(Translate("...Why back?"), 60);
		}
		else if (UnityEngine.Random.value < 0.15f)
		{
			dialogBox.NewMessage(Translate("...It is... warmer... today."), 60);
		}
		else if (UnityEngine.Random.value < 0.15f)
		{
			dialogBox.NewMessage(Translate("...Little green friend. Hello."), 60);
		}
		else if (UnityEngine.Random.value < 0.15f)
		{
			dialogBox.NewMessage(Translate("...Nice to see..."), 60);
		}
		else
		{
			dialogBox.NewMessage(Translate("...Thank you... for... company."), 60);
		}
	}

	private void InterruptRain()
	{
		if (!oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen && oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts <= 0)
		{
			if (UnityEngine.Random.value < 0.3f)
			{
				dialogBox.Interrupt(Translate("...Cold coming..."), 5);
			}
			else if (UnityEngine.Random.value < 0.3f)
			{
				dialogBox.Interrupt(Translate("...Please find... Warmth."), 5);
			}
			else
			{
				dialogBox.Interrupt(Translate("...Find heat... Cold is danger."), 5);
			}
		}
	}

	public void TalkToDeadPlayer()
	{
		if (deadTalk || !oracle.room.ViewedByAnyCamera(oracle.firstChunk.pos, 0f))
		{
			return;
		}
		if (oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen || oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0)
		{
			dialogBox.Interrupt(Translate("..."), 60);
			return;
		}
		float value = UnityEngine.Random.value;
		if (value <= 0.33f)
		{
			dialogBox.Interrupt(Translate("...Why did... you stay..."), 60);
		}
		else if (value <= 0.67f)
		{
			dialogBox.Interrupt(Translate("...Too cold... Alone again."), 60);
		}
		else
		{
			dialogBox.Interrupt(Translate("..."), 60);
		}
		deadTalk = true;
	}

	public override void UnconciousUpdate()
	{
		base.UnconciousUpdate();
		if (halcyon != null)
		{
			halcyon.hoverPos = null;
			halcyon = null;
		}
		holdKnees = false;
	}
}
