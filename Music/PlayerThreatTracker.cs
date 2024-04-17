using MoreSlugcats;
using UnityEngine;

namespace Music;

public class PlayerThreatTracker
{
	private MusicPlayer musicPlayer;

	public ThreatDetermination threatDetermine;

	private int playerNumber;

	public float recommendedDroneVolume = 0.3f;

	public float currentThreat;

	public float currentMusicAgnosticThreat;

	private int room = -1;

	private int lastRoom = -1;

	private int lastLastRoom = -1;

	public int roomSwitches;

	public int roomSwitchDelay = 200;

	public string region;

	public float ghostMode;

	public PlayerThreatTracker(MusicPlayer musicPlayer, int playerNumber)
	{
		this.musicPlayer = musicPlayer;
		this.playerNumber = playerNumber;
		threatDetermine = new ThreatDetermination(playerNumber);
	}

	public void Update()
	{
		if (musicPlayer.manager.currentMainLoop == null || musicPlayer.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game)
		{
			recommendedDroneVolume = 0f;
			currentThreat = 0f;
			currentMusicAgnosticThreat = 0f;
			region = null;
		}
		else
		{
			if (playerNumber >= (musicPlayer.manager.currentMainLoop as RainWorldGame).Players.Count || !((musicPlayer.manager.currentMainLoop as RainWorldGame).Players[playerNumber].realizedCreature is Player { room: not null } player))
			{
				return;
			}
			if (player.room.game.GameOverModeActive || player.redsIllness != null)
			{
				recommendedDroneVolume = 0f;
				currentThreat = 0f;
				currentMusicAgnosticThreat = 0f;
				return;
			}
			recommendedDroneVolume = player.room.roomSettings.BkgDroneVolume;
			if (!player.room.world.rainCycle.MusicAllowed && player.room.roomSettings.DangerType != RoomRain.DangerType.None)
			{
				recommendedDroneVolume = 0f;
			}
			if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].ghostMode > (ModManager.MMF ? 0.1f : 0f))
			{
				if (player.room.world.worldGhost != null)
				{
					ghostMode = player.room.world.worldGhost.GhostMode(player.room.abstractRoom, player.abstractCreature.world.RoomToWorldPos(player.mainBodyChunk.pos, player.room.abstractRoom.index));
				}
				else
				{
					ghostMode = 1f;
				}
			}
			else
			{
				ghostMode = 0f;
			}
			if (ghostMode > 0f)
			{
				recommendedDroneVolume = 0f;
				musicPlayer.FadeOutAllNonGhostSongs(120f);
				if (player.room.world.worldGhost != null && (musicPlayer.song == null || !(musicPlayer.song is GhostSong)))
				{
					musicPlayer.RequestGhostSong(player.room.world.worldGhost.songName);
				}
			}
			if (!player.room.world.singleRoomWorld)
			{
				if (player.room.abstractRoom.index != room)
				{
					lastLastRoom = lastRoom;
					lastRoom = room;
					room = player.room.abstractRoom.index;
					if (room != lastLastRoom)
					{
						roomSwitches++;
						if (player.room.world.region.name != region)
						{
							region = player.room.world.region.name;
							musicPlayer.NewRegion(region);
						}
					}
				}
				if (roomSwitches > 0 && roomSwitchDelay > 0)
				{
					roomSwitchDelay--;
					if (roomSwitchDelay < 1)
					{
						if (musicPlayer.song != null)
						{
							musicPlayer.song.PlayerToNewRoom();
						}
						if (musicPlayer.nextSong != null)
						{
							musicPlayer.nextSong.PlayerToNewRoom();
						}
						roomSwitchDelay = Random.Range(80, 400);
						roomSwitches--;
					}
				}
			}
			else if ((musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta != null && !string.IsNullOrEmpty((musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic))
			{
				string threatMusic = (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic;
				if (region != threatMusic)
				{
					region = threatMusic;
					musicPlayer.NewRegion(region);
				}
			}
			threatDetermine.Update(musicPlayer.manager.currentMainLoop as RainWorldGame);
			if (musicPlayer.song != null)
			{
				threatDetermine.currentThreat = 0f;
			}
			currentThreat = threatDetermine.currentThreat;
			currentMusicAgnosticThreat = threatDetermine.currentMusicAgnosticThreat;
		}
	}
}
