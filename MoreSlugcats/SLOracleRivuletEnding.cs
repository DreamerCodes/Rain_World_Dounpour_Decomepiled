using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SLOracleRivuletEnding : UpdatableAndDeletable
{
	public Oracle SLOracle;

	public ProjectedImage displayImage;

	public int displayImageNumber;

	private bool firstTrigger;

	public SLOracleRivuletEnding(Oracle SLOracle)
	{
		RainWorld.lockGameTimer = true;
		this.SLOracle = SLOracle;
		SLOrcacleState sLOracleState = (this.SLOracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState;
		sLOracleState.playerEncountersWithMark += 5;
		sLOracleState.playerEncounters += 5;
		sLOracleState.likesPlayer = 1f;
		Custom.Log("Rivulet ending procedure");
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (SLOracle.oracleBehavior.player == null || SLOracle.oracleBehavior.player.room != SLOracle.room)
		{
			return;
		}
		if (!firstTrigger)
		{
			FirstTrigger();
			firstTrigger = true;
		}
		if (SLOracle.oracleBehavior.player.firstChunk.pos.y > 600f)
		{
			BodyChunk firstChunk = SLOracle.oracleBehavior.player.firstChunk;
			firstChunk.vel.y = firstChunk.vel.y - 1f;
		}
		if (SLOracle.oracleBehavior.player.firstChunk.pos.y < 170f)
		{
			SLOracle.oracleBehavior.player.standing = true;
		}
		if (SLOracle.oracleBehavior.player.firstChunk.pos.x < 1520f)
		{
			BodyChunk firstChunk2 = SLOracle.oracleBehavior.player.firstChunk;
			firstChunk2.vel.x = firstChunk2.vel.x + 1f;
		}
		if (SLOracle.oracleBehavior.player.firstChunk.pos.x > 1535f)
		{
			BodyChunk firstChunk3 = SLOracle.oracleBehavior.player.firstChunk;
			firstChunk3.vel.x = firstChunk3.vel.x - 1f;
		}
		if (displayImageNumber != 0)
		{
			if (displayImageNumber == 1)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).forcedShowMediaPos = new Vector2(1460f, 310f);
			}
			else if (displayImageNumber == 2)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).forcedShowMediaPos = new Vector2(1530f, 310f);
			}
			else if (displayImageNumber == 3)
			{
				(SLOracle.oracleBehavior as SLOracleBehavior).forcedShowMediaPos = new Vector2(1540f, 330f);
			}
			if (displayImage != null)
			{
				displayImage.pos = (SLOracle.oracleBehavior as SLOracleBehavior).showMediaPos;
				displayImage.setAlpha = 0.91f + Random.value * 0.06f;
			}
		}
	}

	public void FirstTrigger()
	{
		if (room.game.manager.musicPlayer != null)
		{
			MusicEvent musicEvent = null;
			for (int i = 0; i < room.roomSettings.triggers.Count; i++)
			{
				if (room.roomSettings.triggers[i].tEvent is MusicEvent)
				{
					musicEvent = room.roomSettings.triggers[i].tEvent as MusicEvent;
					break;
				}
			}
			if (musicEvent != null)
			{
				room.game.manager.musicPlayer.GameRequestsSong(musicEvent);
			}
		}
		(SLOracle.oracleBehavior as SLOracleBehavior).setMovementBehavior(SLOracleBehavior.MovementBehavior.KeepDistance);
		SLOracle.oracleBehavior.player.controller = new Player.NullController();
		(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).sayHelloDelay = 0;
		(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).forceFlightMode = true;
		if ((SLOracle.oracleBehavior as SLOracleBehaviorHasMark).currentConversation != null)
		{
			(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).currentConversation.Destroy();
			(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).currentConversation = null;
		}
		(SLOracle.oracleBehavior as SLOracleBehaviorHasMark).currentConversation = new SLOracleBehaviorHasMark.MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_RivuletEnding, SLOracle.oracleBehavior, SLOracleBehaviorHasMark.MiscItemType.NA);
	}
}
