using RWCustom;
using UnityEngine;

public class ActiveTriggerChecker : UpdatableAndDeletable
{
	private EventTrigger eventTrigger;

	public int counter = -1;

	public int wait;

	public ActiveTriggerChecker(EventTrigger eventTrigger)
	{
		this.eventTrigger = eventTrigger;
		wait = 10;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (counter >= 0)
		{
			counter++;
			if (counter >= eventTrigger.delay)
			{
				FireEvent();
				counter = -1;
			}
		}
		else if (eventTrigger.type == EventTrigger.TriggerType.Spot)
		{
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].Room == room.abstractRoom)
				{
					if (wait > 0)
					{
						wait--;
					}
					else if (TriggerConditions(i) && room.game.Players[i].realizedCreature != null && Custom.DistLess(room.game.Players[i].realizedCreature.mainBodyChunk.pos, (eventTrigger as SpotTrigger).pos, (eventTrigger as SpotTrigger).rad))
					{
						Positive();
					}
				}
			}
		}
		else
		{
			if (!(eventTrigger.type == EventTrigger.TriggerType.PreRegionBump) && !(eventTrigger.type == EventTrigger.TriggerType.RegionBump))
			{
				return;
			}
			for (int j = 0; j < room.game.Players.Count; j++)
			{
				if (room.game.Players[j].Room != room.abstractRoom)
				{
					continue;
				}
				for (int k = 0; k < room.game.cameras.Length; k++)
				{
					if (room.game.cameras[k].followAbstractCreature == room.game.Players[j] && TriggerConditions(j) && room.game.cameras[k].hud != null && room.game.cameras[k].hud.textPrompt != null && room.game.cameras[k].hud.textPrompt.subregionTracker != null)
					{
						if ((eventTrigger.type == EventTrigger.TriggerType.PreRegionBump && room.game.cameras[k].hud.textPrompt.subregionTracker.PreRegionBump) || (eventTrigger.type == EventTrigger.TriggerType.RegionBump && room.game.cameras[k].hud.textPrompt.subregionTracker.RegionBump))
						{
							Positive();
						}
						break;
					}
				}
			}
		}
	}

	private bool TriggerConditions(int player)
	{
		if ((eventTrigger.entrance < 0 || room.game.Players[player].pos.abstractNode == eventTrigger.entrance) && room.game.Players[player].realizedCreature != null && !room.game.Players[player].realizedCreature.inShortcut && (room.game.Players[player].realizedCreature as Player).Karma >= eventTrigger.karma)
		{
			return !room.game.GameOverModeActive;
		}
		return false;
	}

	private void Positive()
	{
		if (eventTrigger.delay < 1)
		{
			FireEvent();
		}
		else if (counter < 0)
		{
			counter = 0;
		}
	}

	private void FireEvent()
	{
		if (eventTrigger.tEvent == null)
		{
			return;
		}
		if (eventTrigger.fireChance == 1f || Random.value < eventTrigger.fireChance)
		{
			if (eventTrigger.tEvent.type == TriggeredEvent.EventType.MusicEvent)
			{
				if (room.game.manager.musicPlayer != null && (room.game.world.rainCycle.MusicAllowed || room.roomSettings.DangerType == RoomRain.DangerType.None) && (!room.game.IsStorySession || !room.game.GetStorySession.RedIsOutOfCycles))
				{
					room.game.manager.musicPlayer.GameRequestsSong(eventTrigger.tEvent as MusicEvent);
				}
			}
			else if (eventTrigger.tEvent.type == TriggeredEvent.EventType.StopMusicEvent)
			{
				if (room.game.manager.musicPlayer != null)
				{
					room.game.manager.musicPlayer.GameRequestsSongStop(eventTrigger.tEvent as StopMusicEvent);
				}
			}
			else if (eventTrigger.tEvent.type == TriggeredEvent.EventType.ShowProjectedImageEvent)
			{
				ImageTrigger.AttemptTriggerFire(room.game, room, this, eventTrigger.tEvent as ShowProjectedImageEvent);
			}
			else if (eventTrigger.tEvent.type == TriggeredEvent.EventType.PickUpObjectInstruction)
			{
				room.AddObject(new PickupObjectInstruction(room));
			}
			else if (eventTrigger.tEvent.type == TriggeredEvent.EventType.RoomSpecificTextMessage)
			{
				room.AddObject(new RoomSpecificTextMessage(room));
			}
			else if (eventTrigger.tEvent.type == TriggeredEvent.EventType.BringPlayerGuideToRoom)
			{
				BringGuideToPlayerEvent.BringGuide(room.world, -0.5f);
			}
		}
		if (!eventTrigger.multiUse)
		{
			Destroy();
		}
	}
}
