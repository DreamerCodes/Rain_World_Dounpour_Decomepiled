public class ScavengerTradeSpot : UpdatableAndDeletable
{
	public PlacedObject placedObj;

	public ScavengersWorldAI.Trader worldTradeSpot;

	public float Rad => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.magnitude;

	public ScavengerTradeSpot(Room room, PlacedObject placedObj)
	{
		base.room = room;
		this.placedObj = placedObj;
		if (room.world.scavengersWorldAI == null)
		{
			return;
		}
		for (int i = 0; i < room.world.scavengersWorldAI.traders.Count; i++)
		{
			if (room.world.scavengersWorldAI.traders[i].room == room.abstractRoom.index)
			{
				worldTradeSpot = room.world.scavengersWorldAI.traders[i];
				break;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (worldTradeSpot != null && !worldTradeSpot.transgressedByPlayer && worldTradeSpot.squad != null && worldTradeSpot.squad.Active && worldTradeSpot.squad.leader.pos.room == room.abstractRoom.index && worldTradeSpot.squad.leader.realizedCreature != null && worldTradeSpot.squad.leader.realizedCreature.room == room)
		{
			(worldTradeSpot.squad.leader.realizedCreature as Scavenger).AI.tradeSpot = this;
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ScavMerchantMessage && room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Scavenger Merchant"), 0, 120, darken: true, hideHud: true);
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ScavMerchantMessage = true;
			}
		}
	}
}
