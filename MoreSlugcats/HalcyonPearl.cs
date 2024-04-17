using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class HalcyonPearl : DataPearl
{
	public Vector2? hoverPos;

	public float volumeMultiplier;

	public float beatScale;

	public bool Carried => grabbedBy.Count != 0;

	public HalcyonPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		collisionLayer = 0;
		volumeMultiplier = 0f;
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (Carried)
		{
			hoverPos = null;
		}
		if (hoverPos.HasValue)
		{
			base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
			base.firstChunk.vel += Vector2.ClampMagnitude(hoverPos.Value - base.firstChunk.pos, 100f) / 100f * 0.4f;
			base.gravity = 0f;
			MusicControl();
		}
		else
		{
			beatScale = 0f;
			base.gravity = 0.9f;
			MusicStop();
		}
	}

	public void MusicControl()
	{
		if (room.game.manager.musicPlayer == null)
		{
			return;
		}
		if (room.game.manager.musicPlayer.song == null || !(room.game.manager.musicPlayer.song is HalcyonSong))
		{
			if (room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				room.game.manager.musicPlayer.RequestHalcyonSong("NA_19 - Halcyon Memories");
			}
			else
			{
				room.game.manager.musicPlayer.RequestHalcyonSong("NA_19x - Halcyon Memories");
			}
			return;
		}
		float[] array = new float[1024];
		float num = 0f;
		room.game.manager.musicPlayer.song.subTracks[0].source.GetSpectrumData(array, 0, FFTWindow.Hamming);
		for (int i = 0; i < 1024; i++)
		{
			num += array[i];
		}
		beatScale = Mathf.Clamp(num / 0.25f, 0f, 1f);
		volumeMultiplier = Mathf.Lerp(volumeMultiplier, 0.6f, 0.1f);
		float num2 = 1f;
		float num3 = 99999f;
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
		{
			if (firstAlivePlayer.realizedCreature.room != null && (firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_B04" || firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_D08"))
			{
				num3 = ((!(firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_D08")) ? Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, new Vector2(256f, 1230f)) : Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, new Vector2(3649f, 227f)));
				num2 = 0.23f;
			}
			else if (firstAlivePlayer.realizedCreature.room == null || firstAlivePlayer.realizedCreature.room != room)
			{
				num3 = 99999f;
			}
			else
			{
				num3 = Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, base.firstChunk.pos);
				if (room.CameraViewingPoint(base.firstChunk.pos) != room.CameraViewingPoint((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos))
				{
					num2 = 0.5f;
				}
			}
		}
		float num4 = Mathf.Max(0f, 1f - num3 / 1700f) * volumeMultiplier * num2;
		if ((room.game.manager.musicPlayer.song as HalcyonSong).setVolume.HasValue)
		{
			(room.game.manager.musicPlayer.song as HalcyonSong).setVolume = Mathf.Max((room.game.manager.musicPlayer.song as HalcyonSong).setVolume.Value, num4);
		}
		else
		{
			(room.game.manager.musicPlayer.song as HalcyonSong).setVolume = num4;
		}
	}

	public void MusicStop()
	{
		if (room.game.manager.musicPlayer != null && room.game.manager.musicPlayer.song != null && room.game.manager.musicPlayer.song is HalcyonSong)
		{
			room.game.manager.musicPlayer.song.FadeOut(20f);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (firstContact && speed > 2f)
		{
			room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, base.firstChunk, loop: false, Custom.LerpMap(speed, 0f, 8f, 0.2f, 1f), 1f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4];
		sLeaser.sprites[0] = new FSprite("JetFishEyeA");
		sLeaser.sprites[1] = new FSprite("tinyStar");
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[3] = new FSprite("LizardBubble6");
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) - camPos;
		sLeaser.sprites[sLeaser.sprites.Length - 1].x = vector.x;
		sLeaser.sprites[sLeaser.sprites.Length - 1].y = vector.y;
		sLeaser.sprites[sLeaser.sprites.Length - 1].scale = beatScale * 0.75f;
		sLeaser.sprites[sLeaser.sprites.Length - 1].color = Color.red;
		sLeaser.sprites[sLeaser.sprites.Length - 1].alpha = 0.25f + beatScale * 0.65f;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
