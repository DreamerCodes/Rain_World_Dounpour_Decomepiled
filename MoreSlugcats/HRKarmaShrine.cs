using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class HRKarmaShrine : UpdatableAndDeletable, IDrawable
{
	private Vector2 position;

	private bool addKarma;

	private RoomSettings.RoomEffect meltEffect;

	private float effectAdd;

	private float effectInitLevel;

	public float Radius;

	public float fader;

	private PlacedObject.ResizableObjectData data;

	public override void Update(bool eu)
	{
		base.Update(eu);
		Radius = data.handlePos.magnitude;
		float num = Radius * 1.1f;
		if (num < 60f)
		{
			num = 60f;
		}
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == room && firstAlivePlayer.realizedCreature != null && Custom.Dist(position, firstAlivePlayer.realizedCreature.firstChunk.pos) < num)
		{
			if (room.game.session is StoryGameSession && addKarma)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 9;
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
				room.game.cameras[0].hud.karmaMeter.reinforceAnimation = 1;
				addKarma = false;
				room.PlaySound(SoundID.SB_A14, 0f, 1f, 1f);
				for (int i = 0; i < 20; i++)
				{
					room.AddObject(new MeltLights.MeltLight(1f, room.RandomPos(), room, RainWorld.GoldRGB));
				}
				effectAdd = 3f;
			}
			(room.game.Players[0].realizedCreature as Player).karmaCharging = 20;
			if (Random.value < 0.2f)
			{
				VoidParticle obj = new VoidParticle(position, Custom.RNV() * Random.Range(0.3f, 1.5f), 30f);
				room.AddObject(obj);
			}
		}
		effectAdd = Mathf.Max(0f, effectAdd - 0.03666667f);
		if (meltEffect != null)
		{
			meltEffect.amount = Mathf.Lerp(effectInitLevel, 1f, Custom.SCurve(effectAdd, 0.6f));
		}
		fader += 0.02f;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite(KarmaMeter.KarmaSymbolSprite(small: true, new IntVector2(9, 9)));
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		Vector2 vector = position;
		sLeaser.sprites[0].scale = Radius / 20f;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color a = Color.Lerp(RainWorld.GoldRGB, Color.yellow, 0.25f + Mathf.Sin(fader) / 5f);
		a = Color.Lerp(a, palette.blackColor, 0.6f - Mathf.Sin(fader) / 5f);
		sLeaser.sprites[0].color = a;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node2 in containers)
			{
				newContatiner.AddChild(node2);
			}
		}
	}

	public HRKarmaShrine(Room room, Vector2 pos, PlacedObject.ResizableObjectData data)
	{
		if (room.game.IsStorySession)
		{
			addKarma = room.game.GetStorySession.saveState.deathPersistentSaveData.karma < 9;
		}
		else
		{
			addKarma = false;
		}
		position = pos;
		base.room = room;
		Radius = data.handlePos.magnitude;
		this.data = data;
		for (int i = 0; i < room.roomSettings.effects.Count; i++)
		{
			if (room.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.VoidMelt)
			{
				meltEffect = room.roomSettings.effects[i];
				effectInitLevel = meltEffect.amount;
				break;
			}
		}
	}

	public void EffectFor(float time)
	{
		effectAdd = time;
	}
}
