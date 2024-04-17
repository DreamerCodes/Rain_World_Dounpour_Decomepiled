using System;
using UnityEngine;

public class GhostHunch : UpdatableAndDeletable, IDrawable
{
	public GhostWorldPresence.GhostID ghostNumber;

	public int counter;

	public int goAt;

	public float prog;

	public float lastProg;

	public float speed;

	public float alpha;

	public bool go;

	public DisembodiedDynamicSoundLoop soundLoop;

	public GhostHunch(Room room, GhostWorldPresence.GhostID ghostNumber)
	{
		base.room = room;
		this.ghostNumber = ghostNumber;
		goAt = UnityEngine.Random.Range(60, 200);
		float value = UnityEngine.Random.value;
		speed = 1f / Mathf.Lerp(30f, 240f, Mathf.Lerp(value, UnityEngine.Random.value, UnityEngine.Random.value * 0.5f));
		alpha = Mathf.Lerp(1f, 0.5f, Mathf.Lerp(value, UnityEngine.Random.value, UnityEngine.Random.value * 0.5f));
		soundLoop = new DisembodiedDynamicSoundLoop(this);
		soundLoop.sound = SoundID.Ghost_Hunch_LOOP;
		soundLoop.Volume = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.BeingViewed)
		{
			if (go)
			{
				Destroy();
			}
			return;
		}
		soundLoop.Update();
		soundLoop.Volume = Mathf.Pow(Mathf.Sin(Mathf.Pow(prog, 2f) * (float)Math.PI), 0.4f);
		counter++;
		if (counter > goAt - 80 && room.game.rainWorld.processManager.musicPlayer != null)
		{
			room.game.rainWorld.processManager.musicPlayer.FadeOutAllNonGhostSongs(60f);
		}
		if (ghostNumber != null && !go && counter >= goAt)
		{
			if (room.game.session is StoryGameSession && (!(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostNumber) || (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostNumber] < 1))
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostNumber] = 1;
			}
			go = true;
		}
		else if (ghostNumber == null && counter >= goAt)
		{
			go = true;
		}
		lastProg = prog;
		if (go)
		{
			prog = Mathf.Min(1f, prog + speed);
			if (prog >= 1f && lastProg >= 1f)
			{
				Destroy();
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].scaleX = 87.5f;
		sLeaser.sprites[0].scaleY = 50f;
		sLeaser.sprites[0].x = rCam.game.rainWorld.screenSize.x / 2f;
		sLeaser.sprites[0].y = rCam.game.rainWorld.screenSize.y / 2f;
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LevelMelt2"];
		sLeaser.sprites[0].color = new Color(1f, 0f, 0f);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Sin(Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastProg, prog, timeStacker)), 2f) * (float)Math.PI);
		if (num == 0f)
		{
			sLeaser.sprites[0].isVisible = false;
			return;
		}
		sLeaser.sprites[0].isVisible = true;
		sLeaser.sprites[0].alpha = 0.8f * num * alpha;
		rCam.ghostMode = num * alpha;
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
