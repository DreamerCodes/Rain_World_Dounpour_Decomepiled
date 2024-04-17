using UnityEngine;

namespace MoreSlugcats;

public class OraclePanicDisplay : UpdatableAndDeletable
{
	public class PanicIcon : CosmeticSprite
	{
		public int timer;

		public float circleScale;

		public PanicIcon(Vector2 position)
		{
			pos = position;
		}

		public override void Update(bool eu)
		{
			circleScale = Mathf.Lerp(circleScale, 1f, 0.1f);
			if (circleScale > 0.98f)
			{
				timer++;
			}
			if (timer == 160)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
				sLeaser.sprites[i].scale = 0f;
			}
			sLeaser.sprites[1].color = new Color(0.003921569f, 0f, 0f);
			sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
			sLeaser.sprites[2] = new FSprite("miscDangerSymbol");
			sLeaser.sprites[2].isVisible = false;
			sLeaser.sprites[2].color = new Color(0f, 0f, 0f);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			bool isVisible = true;
			if (timer > 130 && timer % 8 < 4)
			{
				isVisible = false;
			}
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].x = pos.x - camPos.x;
				sLeaser.sprites[i].y = pos.y - camPos.y;
				sLeaser.sprites[i].scale = circleScale * 4f * ((i == 0) ? 1f : 0.9f);
				sLeaser.sprites[i].isVisible = isVisible;
			}
			sLeaser.sprites[2].x = pos.x - camPos.x;
			sLeaser.sprites[2].y = pos.y - camPos.y;
			if (circleScale > 0.98f)
			{
				sLeaser.sprites[2].isVisible = isVisible;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public Oracle oracle;

	public int timer;

	private int[] timings;

	public bool gravOn;

	public OracleChatLabel chatLabel;

	public OraclePanicDisplay(Oracle oracle)
	{
		this.oracle = oracle;
		timings = new int[4] { 120, 200, 320, 520 };
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		timer++;
		if (timer == 1)
		{
			gravOn = true;
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, inherited: false));
			}
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, inherited: false));
			}
			if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
			{
				oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, inherited: false));
			}
			oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, 1f, 1f);
		}
		if (timer < timings[0])
		{
			float t = (float)timer / (float)timings[0];
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 1f, t);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.4f, t);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.3f, t);
		}
		if (timer == timings[0])
		{
			oracle.arm.isActive = false;
			oracle.setGravity(0.9f);
			oracle.stun = 9999;
			oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Moon_Panic_Attack, 0f, 0.85f, 1f);
			for (int i = 0; i < oracle.room.game.cameras.Length; i++)
			{
				if (oracle.room.game.cameras[i].room == oracle.room && !oracle.room.game.cameras[i].AboutToSwitchRoom)
				{
					oracle.room.game.cameras[i].ScreenMovement(null, Vector2.zero, 15f);
				}
			}
		}
		if (timer == (timings[1] + timings[2]) / 2)
		{
			oracle.arm.isActive = false;
			oracle.room.PlaySound((Random.value < 0.5f) ? SoundID.SL_AI_Pain_1 : SoundID.SL_AI_Pain_2, 0f, 0.5f, 1f);
			chatLabel = new OracleChatLabel(oracle.oracleBehavior);
			chatLabel.pos = new Vector2(485f, 360f);
			chatLabel.NewPhrase(99);
			oracle.setGravity(0.9f);
			oracle.stun = 9999;
			oracle.room.AddObject(chatLabel);
		}
		if (timer > timings[1] && timer < timings[2] && timer % 16 == 0)
		{
			oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 2.5f);
			for (int j = 0; j < 6; j++)
			{
				if (Random.value < 0.5f)
				{
					oracle.room.AddObject(new PanicIcon(new Vector2(Random.Range(230, 740), Random.Range(100, 620))));
				}
			}
		}
		if (timer >= timings[2] && timer <= timings[3])
		{
			oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 1f);
		}
		if (timer == timings[3])
		{
			chatLabel.Destroy();
			oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 1f);
			gravOn = false;
		}
		if (timer > timings[3])
		{
			float t2 = (float)(timer - timings[3]) / (float)timings[0];
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(1f, 0f, t2);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0.4f, 0f, t2);
			oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0.3f, 0f, t2);
		}
		if (timer == timings[3] + timings[0])
		{
			oracle.setGravity(0f);
			oracle.arm.isActive = true;
			oracle.stun = 0;
			Destroy();
		}
	}
}
