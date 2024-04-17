using Music;
using UnityEngine;

namespace CoralBrain;

public class SSMusicTrigger : UpdatableAndDeletable
{
	public RoomSettings.RoomEffect effect;

	public SSMusicTrigger(RoomSettings.RoomEffect effect)
	{
		this.effect = effect;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MMF)
		{
			if (room.game.cameras[0].room != null && room.game.cameras[0].room == room)
			{
				Trigger();
			}
			return;
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
			{
				Trigger();
				break;
			}
		}
	}

	private void Trigger()
	{
		if (room.game.manager.musicPlayer != null && !(room.gravity > 0f))
		{
			if (room.game.manager.musicPlayer.song == null || !(room.game.manager.musicPlayer.song is SSSong))
			{
				room.game.manager.musicPlayer.RequestSSSong();
			}
			else if ((room.game.manager.musicPlayer.song as SSSong).setVolume.HasValue)
			{
				(room.game.manager.musicPlayer.song as SSSong).setVolume = Mathf.Max((room.game.manager.musicPlayer.song as SSSong).setVolume.Value, effect.amount);
			}
			else
			{
				(room.game.manager.musicPlayer.song as SSSong).setVolume = effect.amount;
			}
		}
	}
}
