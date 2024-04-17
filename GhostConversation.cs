using HUD;
using MoreSlugcats;

public class GhostConversation : Conversation
{
	public Ghost ghost;

	public GhostConversation(ID id, Ghost ghost, DialogBox dialogBox)
		: base(ghost, id, dialogBox)
	{
		this.ghost = ghost;
		currentSaveFile = ghost.room.game.GetStorySession.saveStateNumber;
		AddEvents();
	}

	protected override void AddEvents()
	{
		if (id == ID.Ghost_CC)
		{
			LoadEventsFromFile(1);
		}
		else if (id == ID.Ghost_SI)
		{
			LoadEventsFromFile(2);
		}
		else if (id == ID.Ghost_LF)
		{
			LoadEventsFromFile(3);
		}
		else if (id == ID.Ghost_SH)
		{
			LoadEventsFromFile(4);
		}
		else if (id == ID.Ghost_UW)
		{
			LoadEventsFromFile(5);
		}
		else if (id == ID.Ghost_SB)
		{
			LoadEventsFromFile(6);
		}
		else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Ghost_LC)
		{
			if (ghost.room.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && ghost.room.world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				LoadEventsFromFile(165);
			}
			else
			{
				LoadEventsFromFile(107);
			}
		}
		else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Ghost_UG)
		{
			LoadEventsFromFile(108);
		}
		else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Ghost_SL)
		{
			LoadEventsFromFile(109);
		}
		else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Ghost_CL)
		{
			LoadEventsFromFile(111);
		}
		else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Ghost_MS)
		{
			LoadEventsFromFile(115);
		}
	}
}
